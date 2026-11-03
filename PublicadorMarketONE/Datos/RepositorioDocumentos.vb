''' <summary>Datos del documento que necesita el Publicador (sin las columnas pesadas de DocumentosG, como Firma o TED).</summary>
Public Class DocumentoPendiente
    Public Property Local As Decimal
    Public Property TipoDoc As String = ""
    Public Property Numero As Decimal
    Public Property Fecha As Date
    Public Property Cliente As Decimal
    Public Property Rut As String = ""
    Public Property Total As Decimal
    Public Property FPago As String = ""
    Public Property POS As Decimal
    Public Property Usuario As String = ""
    Public Property STATUS_DTE As Decimal
    Public Property MensajeSincronizacion As String = ""

    Public ReadOnly Property Clave As String
        Get
            Return $"{Local}|{TipoDoc}|{Numero}"
        End Get
    End Property
End Class

''' <summary>
''' Lectura de los documentos pendientes de envío al SII, con el modelo de datos de MarketONE (MarketONEDataContext),
''' igual que el módulo Control DTE Emitidos de MarketONE: tabla DocumentosG, estado en STATUS_DTE.
''' </summary>
Public Module RepositorioDocumentos

    ''' <summary>
    ''' Documentos electrónicos que envía el Publicador, en el mismo orden de carga y envío de Control DTE Emitidos.
    ''' Solo se toman los que además estén en TipoDoc con Electronica = 1 y Cod_SII > 0.
    ''' </summary>
    Public ReadOnly TiposDocElectronicos As String() = {
        TiposDoc.Factura, TiposDoc.Boleta, TiposDoc.BoletaExenta, TiposDoc.FacturaExenta,
        TiposDoc.NotaCredito, TiposDoc.NotaDebito, TiposDoc.GuiaDespacho,
        TiposDoc.FacturaExportacion, TiposDoc.NotaCreditoExportacion, TiposDoc.NotaDebitoExportacion}

    ''' <summary>
    ''' STATUS_DTE que el Publicador considera pendientes de envío:
    ''' 0 (No enviado) y 3 (Timbrado: tiene TED pero aún no se aprueba su envío, por ejemplo si falló la conexión).
    ''' Los rechazados, aprobados, anulados y demás estados no se envían automáticamente.
    ''' </summary>
    Public ReadOnly EstadosPendientes As Decimal() = {CDec(EstadosSII.No_Enviado), CDec(EstadosSII.Timbrado)}


    ''' <summary>Indica si el local emite electrónicamente el tipo de documento (misma regla de DTE.EsElectronico).</summary>
    Public Function EsElectronico(loc As Locales, tipoDoc As String) As Boolean
        If loc Is Nothing OrElse Not loc.FElectronica Then Return False
        Select Case tipoDoc
            Case TiposDoc.Factura : Return loc.FV_Elect
            Case TiposDoc.GuiaDespacho : Return loc.GD_Elect
            Case TiposDoc.NotaCredito : Return loc.NC_Elect
            Case TiposDoc.FacturaExenta : Return loc.FE_Elect
            Case TiposDoc.NotaDebito : Return loc.ND_Elect
            Case TiposDoc.Boleta : Return loc.BV_Elect
            Case TiposDoc.BoletaExenta : Return loc.BE_Elect
            Case TiposDoc.FacturaCompra : Return loc.FC_Elect
            Case TiposDoc.FacturaExportacion : Return loc.FX_Elect
            Case TiposDoc.NotaCreditoExportacion : Return loc.NX_Elect
            Case TiposDoc.NotaDebitoExportacion : Return loc.DX_Elect
        End Select
        Return False
    End Function

    ''' <summary>
    ''' Lista los documentos pendientes de envío (STATUS_DTE 0 o 3) entre las fechas, de todos los locales con
    ''' facturación electrónica y de todos los tipos electrónicos que cada local emite.
    ''' Van primero los nuevos (STATUS_DTE = 0) y al final los que ya se intentaron (Timbrado), para que los que fallan
    ''' siempre no ocupen todo el lote. Con maximo > 0 se devuelven a lo más esa cantidad.
    ''' </summary>
    Public Function ListarPendientes(desde As Date, hasta As Date, Optional maximo As Integer = 0) As List(Of DocumentoPendiente)
        Dim DC = New MarketONEDataContext(P_CONEXION)

        'Tipos electrónicos habilitados en la tabla TipoDoc
        Dim tiposBase = DC.T_TipoDoc.Where(Function(x) x.Electronica AndAlso x.Cod_SII > 0) _
                                    .Select(Function(x) x.TipoDoc).ToList() _
                                    .Select(Function(x) x.Trim()).ToList()
        Dim tipos = TiposDocElectronicos.Where(Function(t) tiposBase.Contains(t)).ToList()

        Dim locales = DC.Locales.Where(Function(x) x.FElectronica).ToList()
        Dim estados = EstadosPendientes

        Dim resultado As New List(Of DocumentoPendiente)
        For Each tipo In tipos
            Dim t = tipo
            Dim localesTipo = locales.Where(Function(l) EsElectronico(l, t)).Select(Function(l) l.Local).ToList()
            If Not localesTipo.Any Then Continue For

            Dim q = DC.DocumentosG.Where(Function(x) x.TipoDoc = t AndAlso
                                                     x.Fecha >= desde AndAlso x.Fecha <= hasta AndAlso
                                                     localesTipo.Contains(x.Local) AndAlso
                                                     estados.Contains(x.STATUS_DTE)) _
                                  .OrderBy(Function(x) x.STATUS_DTE).ThenBy(Function(x) x.Fecha).ThenBy(Function(x) x.Numero) _
                                  .Select(Function(x) New DocumentoPendiente With {
                                      .Local = x.Local, .TipoDoc = x.TipoDoc, .Numero = x.Numero, .Fecha = x.Fecha,
                                      .Cliente = x.Cliente, .Rut = x.Rut, .Total = x.Total, .FPago = x.FPago,
                                      .POS = x.POS, .Usuario = x.Usuario, .STATUS_DTE = x.STATUS_DTE,
                                      .MensajeSincronizacion = x.MensajeSincronizacion})
            If maximo > 0 Then q = q.Take(maximo)
            resultado.AddRange(q.ToList())
        Next

        'Orden final: nuevos primero, después los ya intentados; dentro de cada grupo, el orden de los tipos y la fecha
        Dim ordenTipo = Function(t As String) Array.IndexOf(TiposDocElectronicos, t.Trim())
        Dim ordenados = resultado.OrderBy(Function(d) If(d.STATUS_DTE = EstadosSII.No_Enviado, 0, 1)) _
                                 .ThenBy(Function(d) ordenTipo(d.TipoDoc)) _
                                 .ThenBy(Function(d) d.Fecha).ThenBy(Function(d) d.Numero)
        If maximo > 0 Then Return ordenados.Take(maximo).ToList()
        Return ordenados.ToList()
    End Function

    ''' <summary>Vuelve a leer el documento desde la base (Nothing si ya no existe).</summary>
    Public Function ObtenerDocumento(local As Decimal, tipoDoc As String, numero As Decimal) As DocumentosG
        Dim DC = New MarketONEDataContext(P_CONEXION)
        Return DC.DocumentosG.FirstOrDefault(Function(x) x.Local = local AndAlso x.TipoDoc = tipoDoc AndAlso x.Numero = numero)
    End Function

    ''' <summary>
    ''' Revisa lo que EmitirDTE/EmitirBoleta de MarketONE exigen y rechazan sin mensaje (total 0, sin detalle, sin
    ''' cliente), para dejar el motivo en la observación. Devuelve "" si el documento se puede enviar.
    ''' </summary>
    Public Function ValidarParaEnvio(doc As DocumentosG) As String
        If doc.Total = 0 Then Return "Documento con total 0: no se envía, debe revisarse."
        Dim DC = New MarketONEDataContext(P_CONEXION)
        If Not DC.DocumentosD.Any(Function(x) x.Local = doc.Local AndAlso x.TipoDoc = doc.TipoDoc AndAlso x.Numero = doc.Numero) Then
            Return "Documento no tiene detalle."
        End If
        If Not DC.Clientes.Any(Function(x) x.Cliente = doc.Cliente) Then
            Return $"Cliente {doc.Cliente} del documento no existe."
        End If
        Return ""
    End Function

End Module
