Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Xml
Imports System.Xml.Serialization
Imports DocumentoDTE.SiiDte
Imports DTEBoxCliente
Imports IDAutomation.Windows.Forms.PDF417Barcode
Imports PublicadorMarketONE.Funciones

Public Module DTE
    'Facturación Electrónica
    Public FE_IP As String
    Public FE_EsElectronica As Boolean
    Public FE_Ambiente As Double
    Public FE_Rut_Emisor As String
    Public FE_Razon_Social As String
    Public FE_Giro As String
    Public FE_Direccion As String
    Public FE_Comuna As String
    Public FE_Ciudad As String
    Public FE_Email As String
    Public FE_Ateco As Double
    Public FE_Llave As String
    Public FE_NResolucion As Double
    Public FE_FResolucion As Date
    Public FE_DTE As String
    Public FE_TipoDTE As Double
    Public FE_FV As Boolean
    Public FE_GD As Boolean
    Public FE_NC As Boolean
    Public FE_FE As Boolean
    Public FE_ND As Boolean
    Public FE_BV As Boolean
    Public FE_BE As Boolean
    Public FE_FC As Boolean
    Public FE_FX As Boolean
    Public FE_NX As Boolean
    Public FE_DX As Boolean
    Public FE_Envio_Automatico As Boolean

    Public Resultado_DTE_Codigo As String
    Public Resultado_DTE_Descripcion As String
    Public Resultado_DTE_Observaciones As String

    Public Structure TipoArchivo
        Public Const PDF As String = "pdf"
        Public Const XML As String = "xml"
        Public Const JPG As String = "jpg"
        Public Const TXT As String = "txt"
        Public Const XMLDist As String = "xmlDist"
    End Structure

    Public MensajeDTE As String

    Public Class RespuestaTED
        Public Property Estado As Boolean
        Public Property Mensaje As String
        Public Property Imagen As Byte()
        Public Property XML As String

        Public Sub New(estado As Boolean, mensaje As String)
            Me.Estado = estado
            Me.Mensaje = mensaje
            Imagen = Nothing
            XML = ""
        End Sub

        Public Sub New(estado As Boolean, mensaje As String, imagen As Byte(), xml As String)
            Me.Estado = estado
            Me.Mensaje = mensaje
            Me.Imagen = imagen
            Me.XML = xml
        End Sub
    End Class

    Public Function ObtenerFirma(local As Decimal, tipoDoc As String, numDoc As Double, rut As String, nombre As String, fecha As DateTime, monto As Integer, Optional Mensaje As Boolean = True) As (firma As Byte(), xml As String)
        Dim Imagen As Byte() = Nothing

        Dim tipoDocumento = TipoDocumentoDTE(tipoDoc)
        If tipoDocumento = 0 Then Return (Imagen, "")

        Dim CAF = CAFDocumento(local, tipoDoc, numDoc)

        If CAF Is Nothing Then
            Return (New Byte() {0}, "")
        End If

        'Datos del PDF417
        Dim pdfTipo = GDEGateway.Timbre.TipoPdf417.Grafico
        Dim pdfColumnas = 13
        Dim pdfNivelCorreccion = 5
        Dim pdfTruncado = False
        Dim pdfFormato = GDEGateway.Timbre.FormatoPdf417.Jpeg
        Dim directorioTemporal = Directorio_TED

        Dim tedGenerado As GDEGateway.Timbre.TED
        Dim primeraLinea = Funciones.PrimeraLinea(local, tipoDoc, numDoc)

        Try
            tedGenerado = GDEGateway.Timbre.Generador.GenerarTimbre(
            FE_Rut_Emisor,
            tipoDocumento,
            fecha,
            numDoc.ToInt(),
            FormatoRut(rut, ModoRut.SII),
            TextoDTE(nombre),
            monto,
            primeraLinea,
            CAF,
            pdfTipo,
            pdfColumnas,
            pdfNivelCorreccion,
            pdfTruncado,
            pdfFormato,
            directorioTemporal)

            Imagen = tedGenerado.CodigoComoBytes()
            Dim xml = tedGenerado.Valor

            Return (Imagen, xml)
        Catch ex As Exception
            If Mensaje Then
                If InStr(ex.Message.ToUpper, "DATOS INCORRECTOS") > 0 Or InStr(ex.Message.ToUpper, "BAD DATA") > 0 Then
                    MsgError("Error en CAF descargado desde SII.  Favor contactarse con Sopote Informático.")
                End If
            End If
            Return (Nothing, "")
        End Try
    End Function

    Public Function TipoDocumentoDTE(tipoDoc As String) As GDEGateway.Timbre.TipoDocumento
        'Datos del TED
        Dim tipoDocumento As GDEGateway.Timbre.TipoDocumento = Nothing
        Select Case tipoDoc
            Case TiposDoc.Factura
                tipoDocumento = GDEGateway.Timbre.TipoDocumento.FacturaElectronica
            Case TiposDoc.GuiaDespacho
                tipoDocumento = GDEGateway.Timbre.TipoDocumento.GuiaDespachoElectronica
            Case TiposDoc.NotaCredito
                tipoDocumento = GDEGateway.Timbre.TipoDocumento.NotaCreditoElectronica
            Case TiposDoc.NotaDebito
                tipoDocumento = GDEGateway.Timbre.TipoDocumento.NotaDebitoElectronica
            Case TiposDoc.Boleta
                tipoDocumento = GDEGateway.Timbre.TipoDocumento.BoletaElectronica
            Case TiposDoc.FacturaCompra
                tipoDocumento = GDEGateway.Timbre.TipoDocumento.FacturaCompraElectronica
            Case TiposDoc.FacturaExenta
                tipoDocumento = GDEGateway.Timbre.TipoDocumento.FacturaElectronicaNoAfectaOExenta
            Case TiposDoc.BoletaExenta
                tipoDocumento = GDEGateway.Timbre.TipoDocumento.BoletaExentaElectronica
            Case TiposDoc.FacturaExportacion
                tipoDocumento = GDEGateway.Timbre.TipoDocumento.FacturaExportacion110
            Case TiposDoc.NotaDebitoExportacion
                tipoDocumento = GDEGateway.Timbre.TipoDocumento.FacturaExportacion111
            Case TiposDoc.NotaCreditoExportacion
                tipoDocumento = GDEGateway.Timbre.TipoDocumento.FacturaExportacion112
        End Select
        Return tipoDocumento
    End Function

    Public Function CAFDocumento(local As Decimal, tipoDoc As String, numDoc As Double) As Byte()
        Dim CAF() As Byte = Nothing
        Dim DC As New MarketONEDataContext(P_CONEXION)
        Try
            Dim fol = DC.Folios.FirstOrDefault(Function(x) x.RUT = FormatoRut(FE_Rut_Emisor) And x.Documento = tipoDoc And x.Folio = numDoc)
            If fol IsNot Nothing Then

                Dim c = DC.CAF.FirstOrDefault(Function(x) x.Id = fol.IdCAF)
                If c IsNot Nothing Then
                    CAF = Encoding.UTF8.GetBytes(c.XML)
                    Return CAF
                Else
                    Dim txtXML = fol.CAF.ToString
                    If fol IsNot Nothing Then
                        CAF = Encoding.UTF8.GetBytes(txtXML)
                        Return CAF
                    End If
                End If
            Else
                Dim cor = DC.T_Correlativos.FirstOrDefault(Function(x) x.Local = Val(local) And x.TipoDoc = tipoDoc)
                If cor IsNot Nothing Then
                    Dim txtXML = cor.CAF
                    CAF = Encoding.UTF8.GetBytes(txtXML)
                    Return CAF
                End If
            End If
        Catch ex As Exception
            Return Nothing
        End Try
        Return CAF
    End Function

    Public Function GenerarTED(local As Decimal, tipoDoc As String, numDoc As Decimal, fecha As Date, rut As String, nombre As String, monto As Decimal, primeraLinea As String) As RespuestaTED
        Dim DC As New MarketONEDataContext(P_CONEXION)
        Dim Imagen As Byte() = Nothing
        Dim XML As String

        Try
            Dim tipoDocumento = TipoDocumentoDTE(tipoDoc)
            Dim CAF = CAFDocumento(local, tipoDoc, numDoc)

            Dim DocG = DC.DocumentosG.FirstOrDefault(Function(x) x.Local = local And x.TipoDoc = tipoDoc And x.Numero = numDoc)
            If DocG Is Nothing Then
                ActivityLog.Log($"{tipoDoc} {numDoc} Local {local} {G_USUARIOACTUAL} Documento no existe", "GenerarTimbreDTE", "DTE")
                Return New RespuestaTED(False, "Error al recuperar CAF. Documento no existe")
            End If

            'Datos del PDF417
            Dim pdfTipo = GDEGateway.Timbre.TipoPdf417.Grafico
            Dim pdfColumnas = 13
            Dim pdfNivelCorreccion = 5
            Dim pdfTruncado = False
            Dim pdfFormato = GDEGateway.Timbre.FormatoPdf417.Jpeg
            Dim directorioTemporal = Directorio_TED


            Dim tedGenerado As GDEGateway.Timbre.TED

            primeraLinea = Funciones.PrimeraLinea(local, tipoDoc, numDoc)
            tedGenerado = GDEGateway.Timbre.Generador.GenerarTimbre(
                FE_Rut_Emisor,
                tipoDocumento,
                fecha,
                numDoc.ToInt(),
                FormatoRut(rut),
                TextoDTE(nombre),
                monto.ToInt(),
                primeraLinea,
                CAF,
                pdfTipo,
                pdfColumnas,
                pdfNivelCorreccion,
                pdfTruncado,
                pdfFormato,
                directorioTemporal)

            Dim tedValor = tedGenerado.Valor
            Imagen = tedGenerado.CodigoComoBytes()

            'NO GRABAR TED: JPG y XML
            'Dim ArchivoJPG As String = "", ArchivoXML As String = ""

            'Dim intentos = 3
            'While intentos > 0
            '    Try
            '        ArchivoJPG = RutaDocumentoElectronico(TipoArchivo.JPG, local, tipoDoc, numDoc, DocG.Cliente, 3 - intentos)
            '        System.IO.File.WriteAllBytes(ArchivoJPG, Imagen)
            '        intentos = 0
            '    Catch ex As Exception
            '        Thread.Sleep(100)
            '        intentos -= 1
            '    End Try
            'End While

            'intentos = 3
            'While intentos > 0
            '    Try
            '        ArchivoXML = RutaDocumentoElectronico(TipoArchivo.XML, local, tipoDoc, numDoc, DocG.Cliente, 3 - intentos)
            '        System.IO.File.WriteAllText(ArchivoXML, tedValor)
            '        intentos = 0
            '    Catch ex As Exception
            '        Thread.Sleep(100)
            '        intentos -= 1
            '    End Try
            'End While


            XML = tedValor

            If DocG IsNot Nothing Then
                Dim anterior = DocG.STATUS_DTE
                If DocG.STATUS_DTE = EstadosSII.No_Enviado Or DocG.STATUS_DTE = EstadosSII.Timbrado Then
                    'Cambiar el estado de un documento si este solo 0se ha timbrado
                    DocG.TED = XML
                    DocG.Firma = New Byte() {} ' = Imagen     Se deja de guardar para que no ocupe espacio
                    DocG.STATUS_DTE = EstadosSII.Timbrado
                    DC.Log = System.Console.Out
                    DC.SubmitChanges()
                    DC.Refresh(Data.Linq.RefreshMode.OverwriteCurrentValues)
                    ActivityLog.Log($"{tipoDoc} {numDoc} Local {local} {G_USUARIOACTUAL} Respuesta: {MensajeDTE} Estado: {EstadosSII.Timbrado} Anterior: {CType(anterior, EstadosSII)}", "GenerarTimbreDTE", "DTE")
                End If
            End If

        Catch ex As Exception
            Return New RespuestaTED(False, ex.ToString)
            ActivityLog.Log($"{tipoDoc} {numDoc} Local {local} {G_USUARIOACTUAL} {ex.Message}", "GenerarTimbreDTE", "DTE")
        End Try

        Return New RespuestaTED(True, "", Imagen, XML)
    End Function

    Public Function LeerTED(archivo As String) As String
        Try
            Dim wArchivo As New StreamReader(archivo.Trim)
            Return wArchivo.ReadToEnd()
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Public Function RutaDocumentoElectronico(extension As String, local As Decimal, tipoDoc As String, numero As Decimal, cliente As Decimal, Optional incremento As Decimal = 0) As String
        Dim DC As New MarketONEDataContext(P_CONEXION)
        Dim fantasia As String = ""

        If cliente = 0 Then
            Dim DocG = DC.DocumentosG.FirstOrDefault(Function(x) x.Local = local And x.TipoDoc = tipoDoc And x.Numero = numero)
            If DocG IsNot Nothing Then
                cliente = DocG.Cliente
            End If
        End If

        If cliente > 0 Then
            Dim cli = DC.Clientes.FirstOrDefault(Function(x) x.Cliente = cliente)
            If cli IsNot Nothing Then
                fantasia = cli.Fantasia.Trim
            End If
        End If

        Dim ruta = $"{tipoDoc.Trim()} ({local.ToString.PadLeft(2, "0"c)}) {numero.ToString.PadLeft(8, "0"c)} {fantasia}".Trim()

        If incremento > 0 Then
            ruta &= $"_{incremento}"
        End If

        ruta &= "."

        Dim rutaArchivo = ""

        Select Case extension
            Case TipoArchivo.PDF
                rutaArchivo = Directorio_PDF & ruta & "pdf"
            Case TipoArchivo.XML
                rutaArchivo = Directorio_TED & ruta & "xml"
            Case TipoArchivo.JPG
                rutaArchivo = Directorio_TED & ruta & "jpg"
            Case TipoArchivo.TXT
                rutaArchivo = Directorio_TXT & ruta & "txt"
            Case TipoArchivo.XMLDist
                rutaArchivo = Directorio_PDF & ruta & "xml"
            Case Else
                rutaArchivo = P_DIRECTORIO & ruta
        End Select

        Directory.CreateDirectory(Path.GetDirectoryName(rutaArchivo))
        Return rutaArchivo

    End Function

    '  <DebuggerStepThrough>
    Public Function EmitirDTE(local As Decimal, tipoDoc As String, numDoc As Decimal, Optional generarXML As Boolean = False, Optional mensajes As Boolean = True) As Boolean
        Try

            Dim DocumentosRef As New List(Of DocRef)

            Dim DC As New MarketONEDataContext(P_CONEXION)

            Dim rutEmisor = FE_Rut_Emisor
            Dim razonSocial = FE_Razon_Social
            Dim giro = FE_Giro
            Dim direccion = FE_Direccion
            Dim comuna = FE_Comuna
            Dim ciudad = FE_Ciudad
            Dim email = FE_Email
            Dim ateco = FE_Ateco.ToInt()
            Dim ambiente = FE_Ambiente
            Dim URL = FE_DTE
            Dim clave = FE_Llave
            Dim fechaResolucion = FE_FResolucion
            Dim resolucion = FE_NResolucion.ToInt()

            'Documentos con tratamiento especial
            If tipoDoc = TiposDoc.Boleta Or tipoDoc = TiposDoc.BoletaExenta Then
                Dim Respuesta = EmitirBoleta(local, tipoDoc, numDoc)
                Return Respuesta
            End If
            If tipoDoc = TiposDoc.FacturaExportacion Or tipoDoc = TiposDoc.NotaCreditoExportacion Or tipoDoc = TiposDoc.NotaDebitoExportacion Then
                Dim Respuesta = EmitirDocumentoExportacion(local, tipoDoc, numDoc)
                Return Respuesta
            End If

            Dim dte As New DocumentoDTE.SiiDte.DTE()
            dte.DTE_Choice = New DocumentoDTE.SiiDte.DTE_Choice()

            dte.DTE_Choice.Documento = New DocumentoDTE.SiiDte.Documento()
            Dim doc As DocumentoDTE.SiiDte.Documento
            doc = dte.DTE_Choice.Documento

            doc.Encabezado = New DocumentoDTE.SiiDte.Encabezado()
            doc.Encabezado.IdDoc = New DocumentoDTE.SiiDte.IdDoc()

            Dim DocG = DC.DocumentosG.FirstOrDefault(Function(x) x.Local = local And x.TipoDoc = tipoDoc And x.Numero = numDoc)
            If DocG Is Nothing Then
                Return False
            End If

            If DocG.STATUS_DTE = EstadosSII.Aprobado Then
                Return True
            End If

            If DocG.Total = 0 Then
                Return False
            End If

            Dim ListaDocD = DC.DocumentosD.Where(Function(x) x.Local = local And x.TipoDoc = tipoDoc And x.Numero = numDoc).ToList
            If Not ListaDocD.Any Then
                Return False
            End If

            Dim Cli = DC.Clientes.FirstOrDefault(Function(x) x.Cliente = DocG.Cliente)
            If Cli Is Nothing Then
                Return False
            End If

            Dim Loc = DC.Locales.FirstOrDefault(Function(x) x.Local = local)

            If Loc IsNot Nothing Then
                rutEmisor = FormatoRut(Loc.RutLocal, ModoRut.SII)
                ambiente = Val(IIf(Loc.Produccion, DTEBoxCliente.Ambiente.Produccion, DTEBoxCliente.Ambiente.Homologacion))
                razonSocial = TextoDTE(Loc.RazonLocal.Truncate(100))
                giro = TextoDTE(Loc.GiroLocal.Truncate(80))
                direccion = TextoDTE(Loc.DirLocal.Truncate(60))
                comuna = TextoDTE(ObtenerComuna(Loc.Comuna))
                ciudad = TextoDTE(ObtenerComuna(Loc.Comuna))
                email = Loc.EMailLocal.ToString.Trim
                ateco = Loc.ATECO.ToInt()
                clave = Loc.Llave.ToString.Trim
                resolucion = Loc.NResolucion.ToInt()
                fechaResolucion = Loc.FResolucion
                URL = "http://" + Loc.IP_DTE.ToString.Trim + "/api/Core.svc/Core"
            End If

            'Documento/Referencia
            Dim wDocR = DC.DocumentosR.Where(Function(x) x.Local = local And x.Numero = numDoc And x.TipoDoc = tipoDoc).ToList
            For Each docr In wDocR
                DocumentosRef.Add(New DocRef With
                {
                    .Fecha = docr.FechaRef,
                    .NumDoc = Regex.Replace(docr.NumDocRef, "\D", ""),
                    .TipoDoc = docr.TipoDocRef,
                    .Observacion = docr.Observacion
                })
            Next
            If Not DocumentosRef.Any Then
                DocumentosRef.Add(New DocRef With
                {
                    .Fecha = New Date(2000, 1, 1),
                    .NumDoc = "",
                    .TipoDoc = "",
                    .Observacion = ""
                })
            End If

            'Documento/Encabezado/IdDoc
            doc.Encabezado.IdDoc.TipoDTE = CType(CodigoDTEEmision(tipoDoc), DocumentoDTE.SiiDte.Enumerations.DTEType)     'Codigo del tipo .n99
            doc.Encabezado.IdDoc.Folio = numDoc.ToInt
            doc.Encabezado.IdDoc.FchEmis = New LiquidTechnologies.Runtime.Net20.XmlDateTime(DocG.Fecha.Date)
            doc.Encabezado.IdDoc.FmaPago = CType(IIf(DocG.FPago.Trim.ToUpper = FormasPago.Efectivo, DocumentoDTE.SiiDte.Enumerations.IdDoc_FmaPago.n1, DocumentoDTE.SiiDte.Enumerations.IdDoc_FmaPago.n2), DocumentoDTE.SiiDte.Enumerations.IdDoc_FmaPago)

            If tipoDoc = TiposDoc.GuiaDespacho Then
                doc.Encabezado.IdDoc.TipoDespacho = DocumentoDTE.SiiDte.Enumerations.IdDoc_TipoDespacho.n1
                doc.Encabezado.IdDoc.IndTraslado = CodigoReferenciaGD(tipoDoc, DocG.Motivo.ToString.Trim)
                doc.Encabezado.IdDoc.IndServicio = DocumentoDTE.SiiDte.Enumerations.IdDoc_IndServicio.n1
                doc.Encabezado.IdDoc.FchVenc = New LiquidTechnologies.Runtime.Net20.XmlDateTime(DocG.Fecha.Date)
            End If

            If tipoDoc = TiposDoc.NotaCredito Or tipoDoc = TiposDoc.NotaDebito Then
                doc.Encabezado.IdDoc.FmaPago = DocumentoDTE.SiiDte.Enumerations.IdDoc_FmaPago.n1
                doc.Encabezado.IdDoc.FchVenc = New LiquidTechnologies.Runtime.Net20.XmlDateTime(DocumentosRef.First().Fecha)
            End If

            'Documento/Encabezado/Emisor
            doc.Encabezado.Emisor = New DocumentoDTE.SiiDte.Emisor()
            doc.Encabezado.Emisor.RUTEmisor = rutEmisor
            doc.Encabezado.Emisor.RznSoc = razonSocial
            doc.Encabezado.Emisor.GiroEmis = giro
            doc.Encabezado.Emisor.Acteco.Add(ateco)
            doc.Encabezado.Emisor.DirOrigen = direccion
            doc.Encabezado.Emisor.CmnaOrigen = comuna
            doc.Encabezado.Emisor.CiudadOrigen = ciudad
            doc.Encabezado.Emisor.CorreoEmisor = email

            'Documento/Encabezado/Receptor
            doc.Encabezado.Receptor = New DocumentoDTE.SiiDte.Receptor()
            doc.Encabezado.Receptor.RUTRecep = FormatoRut(DocG.Rut, ModoRut.SII)
            doc.Encabezado.Receptor.RznSocRecep = TextoDTE(Cli.Nombre.Truncate(100))
            doc.Encabezado.Receptor.CdgIntRecep = Cli.Cliente.ToText
            If Cli.Giro.Trim <> "" Then
                doc.Encabezado.Receptor.GiroRecep = TextoDTE(Cli.Giro.Truncate(40))
            Else
                doc.Encabezado.Receptor.GiroRecep = "PARTICULAR"
            End If
            If Cli.Contacto.Trim <> "" Then
                doc.Encabezado.Receptor.Contacto = TextoDTE(Cli.Contacto.Truncate(80))
            Else
                doc.Encabezado.Receptor.Contacto = TextoDTE(Cli.Nombre.Truncate(80))
            End If
            If Cli.Email.Trim <> "" Then
                doc.Encabezado.Receptor.CorreoRecep = TextoDTE(Cli.Email.Truncate(80)).ToString.Trim
            End If
            doc.Encabezado.Receptor.DirRecep = TextoDTE(Cli.Direccion.Truncate(70))
            doc.Encabezado.Receptor.CmnaRecep = TextoDTE(ObtenerComuna(Cli.Comuna).Truncate(20))
            doc.Encabezado.Receptor.CiudadRecep = TextoDTE(ObtenerComuna(Cli.Comuna).Truncate(20))
            doc.Encabezado.Receptor.CorreoRecep = Cli.Email.ToString.Trim


            'Documento/Descuentos Generales
            If DocG.Descuento > 0 Then
                Dim dscRcgGlobal As DocumentoDTE.SiiDte.DscRcgGlobal
                dscRcgGlobal = New DocumentoDTE.SiiDte.DscRcgGlobal()
                dscRcgGlobal.NroLinDR = 1
                dscRcgGlobal.TpoMov = DocumentoDTE.SiiDte.Enumerations.DscRcgGlobal_TpoMov.D
                dscRcgGlobal.TpoValor = DocumentoDTE.SiiDte.Enumerations.DineroPorcentajeType.Dollar  'Dollar = Descuento en Pesos,  Percent=Descuento por porcentaje
                dscRcgGlobal.ValorDR = DocG.Descuento.ToInt()
                doc.DscRcgGlobal.Add(dscRcgGlobal)
            End If

            'Documento/Encabezado/Totales
            doc.Encabezado.Totales = New DocumentoDTE.SiiDte.Totales()
            doc.Encabezado.Totales.MntTotal = DocG.Total.ToInt()

            'PROPINA (ES NO FACTURABLE <> A EXENTO)
            If DocG.Propina > 0 Then
                doc.Encabezado.Totales.MontoNF = DocG.Propina.ToInt()
            End If

            If tipoDoc = TiposDoc.FacturaExenta Or tipoDoc = TiposDoc.BoletaExenta Or
                DocumentosRef.First().TipoDoc = TiposDoc.FacturaExenta Or DocumentosRef.First().TipoDoc = TiposDoc.BoletaExenta Then
                doc.Encabezado.Totales.MntExe = DocG.Neto.ToInt()
            Else
                doc.Encabezado.Totales.MntNeto = DocG.Neto.ToInt()
                doc.Encabezado.Totales.TasaIVA = G_IVA
                doc.Encabezado.Totales.IVA = DocG.IVA.ToInt()
            End If



            ' NOTA: Los impuestos adicionales solo debe especificarse si es que los hay, valores en 0 no pueden ir.
            ' Documento/Encabezado/Totales/ImptoReten

            If DocG.MIN > 0 Then
                Dim FE_Impuestos As DocumentoDTE.SiiDte.ImptoReten
                FE_Impuestos = New DocumentoDTE.SiiDte.ImptoReten()
                FE_Impuestos.TipoImp = DocumentoDTE.SiiDte.Enumerations.ImpAdicDTEType.n27
                FE_Impuestos.MontoImp = Math.Round(DocG.MIN).ToInt()
                FE_Impuestos.TasaImp = ImpuestosPorcentaje.Minerales
                doc.Encabezado.Totales.ImptoReten.Add(FE_Impuestos)
            End If
            If DocG.BEB > 0 Then
                Dim FE_Impuestos As DocumentoDTE.SiiDte.ImptoReten
                FE_Impuestos = New DocumentoDTE.SiiDte.ImptoReten()
                FE_Impuestos.TipoImp = DocumentoDTE.SiiDte.Enumerations.ImpAdicDTEType.n271
                FE_Impuestos.MontoImp = Math.Round(DocG.BEB).ToInt()
                FE_Impuestos.TasaImp = ImpuestosPorcentaje.Bebidas
                doc.Encabezado.Totales.ImptoReten.Add(FE_Impuestos)
            End If
            If DocG.CER > 0 Then
                Dim FE_Impuestos As DocumentoDTE.SiiDte.ImptoReten
                FE_Impuestos = New DocumentoDTE.SiiDte.ImptoReten()
                FE_Impuestos.TipoImp = DocumentoDTE.SiiDte.Enumerations.ImpAdicDTEType.n26
                FE_Impuestos.MontoImp = Math.Round(DocG.CER).ToInt()
                FE_Impuestos.TasaImp = ImpuestosPorcentaje.Cervezas
                doc.Encabezado.Totales.ImptoReten.Add(FE_Impuestos)
            End If
            If DocG.VIN > 0 Then
                Dim FE_Impuestos As DocumentoDTE.SiiDte.ImptoReten
                FE_Impuestos = New DocumentoDTE.SiiDte.ImptoReten()
                FE_Impuestos.TipoImp = DocumentoDTE.SiiDte.Enumerations.ImpAdicDTEType.n25
                FE_Impuestos.MontoImp = Math.Round(DocG.VIN).ToInt()
                FE_Impuestos.TasaImp = ImpuestosPorcentaje.Vinos
                doc.Encabezado.Totales.ImptoReten.Add(FE_Impuestos)
            End If
            If DocG.LIC > 0 Then
                Dim FE_Impuestos As DocumentoDTE.SiiDte.ImptoReten
                FE_Impuestos = New DocumentoDTE.SiiDte.ImptoReten()
                FE_Impuestos.TipoImp = DocumentoDTE.SiiDte.Enumerations.ImpAdicDTEType.n24
                FE_Impuestos.MontoImp = Math.Round(DocG.LIC).ToInt()
                FE_Impuestos.TasaImp = ImpuestosPorcentaje.Licores
                doc.Encabezado.Totales.ImptoReten.Add(FE_Impuestos)
            End If
            If DocG.HAR > 0 Then
                Dim FE_Impuestos As DocumentoDTE.SiiDte.ImptoReten
                FE_Impuestos = New DocumentoDTE.SiiDte.ImptoReten()
                FE_Impuestos.TipoImp = DocumentoDTE.SiiDte.Enumerations.ImpAdicDTEType.n19
                FE_Impuestos.MontoImp = Math.Round(DocG.HAR).ToInt()
                FE_Impuestos.TasaImp = ImpuestosPorcentaje.Harinas
                doc.Encabezado.Totales.ImptoReten.Add(FE_Impuestos)
            End If
            If DocG.CAR > 0 Then
                Dim FE_Impuestos As DocumentoDTE.SiiDte.ImptoReten
                FE_Impuestos = New DocumentoDTE.SiiDte.ImptoReten()
                FE_Impuestos.TipoImp = DocumentoDTE.SiiDte.Enumerations.ImpAdicDTEType.n18
                FE_Impuestos.MontoImp = Math.Round(DocG.CAR).ToInt()
                FE_Impuestos.TasaImp = ImpuestosPorcentaje.Carnes
                doc.Encabezado.Totales.ImptoReten.Add(FE_Impuestos)
            End If

            'IEV
            If (DocG.IEF + DocG.IEV) > 0 Then
                Dim FE_Impuestos As DocumentoDTE.SiiDte.ImptoReten
                FE_Impuestos = New DocumentoDTE.SiiDte.ImptoReten()
                FE_Impuestos.TipoImp = DocumentoDTE.SiiDte.Enumerations.ImpAdicDTEType.n14
                FE_Impuestos.MontoImp = (DocG.IEF + DocG.IEV).ToDecimal.ToInt
                FE_Impuestos.TasaImp = 0
                doc.Encabezado.Totales.ImptoReten.Add(FE_Impuestos)
            End If

            'Documento/Detalle
            Dim Lineas = 1

            Dim primeraLinea = "ARTICULO"
            For Each DocD In ListaDocD
                If Lineas >= 100 Then
                    'GDExpress no soporta mas de 100 lineas en un documento
                    Exit For
                End If
                Dim det = New DocumentoDTE.SiiDte.Detalle()

                Dim unidad = "UN"
                Dim Art = DC.Articulos.FirstOrDefault(Function(x) x.Articulo = DocD.Articulo)
                If Art IsNot Nothing Then
                    unidad = Art.Unidad.Trim
                End If

                Dim glosa = DocD.Glosa.Trim
                If glosa = "" Then glosa = Art.Descripcion.Trim
                Dim articulo = glosa.Split(CChar(vbLf))(0).Trim.Truncate(80)
                If primeraLinea = "ARTICULO" Then
                    primeraLinea = TextoDTE(articulo).Trim
                    articulo = primeraLinea
                End If

                det.NroLinDet = Lineas
                If Lineas = 1 Then
                    det.NmbItem = primeraLinea
                Else
                    det.NmbItem = TextoDTE(articulo).Truncate(80).Trim
                End If
                det.MontoItem = Math.Round(DocD.Cantidad * DocD.Neto, 0).ToInt()
                det.DscItem = String.Join(" - ", glosa.Split(CChar(vbLf)).Select(Function(x) TextoDTE(x.Trim).Trim)).Truncate(1000)
                If DocD.Cantidad = 0 Then
                    det.QtyItem = 1 'PANCITOS
                Else
                    det.QtyItem = DocD.Cantidad
                End If
                det.UnmdItem = unidad
                If DocD.Neto > 0 Then
                    det.PrcItem = Math.Round(DocD.Neto, 5)
                End If

                'Si es NC o ND y el Documento afecto es FE, debe tener este item especial de exento
                If DocumentosRef.First().TipoDoc = TiposDoc.FacturaExenta Or DocumentosRef.First().TipoDoc = TiposDoc.BoletaExenta Then
                    det.IndExe = 0   'En el DTE les llega como un 1
                End If


                'Impuestos Adicionales de los articulos
                If DocD.MIN > 0 Then
                    det.CodImpAdic.Add(DocumentoDTE.SiiDte.Enumerations.ImpAdicDTEType.n27)
                End If
                If DocD.BEB > 0 Then
                    det.CodImpAdic.Add(DocumentoDTE.SiiDte.Enumerations.ImpAdicDTEType.n271)
                End If
                If DocD.VIN > 0 Then
                    det.CodImpAdic.Add(DocumentoDTE.SiiDte.Enumerations.ImpAdicDTEType.n25)
                End If
                If DocD.CER > 0 Then
                    det.CodImpAdic.Add(DocumentoDTE.SiiDte.Enumerations.ImpAdicDTEType.n26)
                End If
                If DocD.LIC > 0 Then
                    det.CodImpAdic.Add(DocumentoDTE.SiiDte.Enumerations.ImpAdicDTEType.n24)
                End If
                If DocD.HAR > 0 Then
                    det.CodImpAdic.Add(DocumentoDTE.SiiDte.Enumerations.ImpAdicDTEType.n19)
                End If
                If DocD.CAR > 0 Then
                    det.CodImpAdic.Add(DocumentoDTE.SiiDte.Enumerations.ImpAdicDTEType.n18)
                End If

                'IEV
                If Lineas = 1 And (DocG.IEF + DocG.IEV) > 0 Then
                    det.CodImpAdic.Add(DocumentoDTE.SiiDte.Enumerations.ImpAdicDTEType.n14)
                End If

                doc.Detalle.Add(det)
                Lineas += 1
            Next

            If DocG.Propina > 0 Then
                Dim propina As New DocumentoDTE.SiiDte.Detalle
                propina.NroLinDet = Lineas
                propina.NmbItem = "Propina"
                propina.DscItem = "Propina"
                propina.PrcItem = DocG.Propina
                propina.QtyItem = 1D
                propina.MontoItem = DocG.Propina.ToInt()
                propina.IndExe = DocumentoDTE.SiiDte.Enumerations.Detalle_IndExe.n3 '2 (Debería ser n2)
                'propina.IndExe = CType(2, DocumentoDTE.SiiDte.Enumerations.Detalle_IndExe)
                doc.Detalle.Add(propina)
            End If


            DocumentosRef = DocumentosRef.Where(Function(x) Not {TiposDoc.Ticket}.Contains(x.TipoDoc)).ToList

            'Transporte
            Dim datosTransporte = False
            If (DocG.TipoDoc = TiposDoc.GuiaDespacho) Or
                (DocG.TipoDoc = TiposDoc.Factura And DocG.DireccionOrigen <> "" And DocG.ComunaOrigen <> "") Then
                datosTransporte = True
            End If

            If datosTransporte Then
                doc.Encabezado.Transporte = New DocumentoDTE.SiiDte.Transporte()
                doc.Encabezado.Transporte.Patente = Mid(DocG.PatenteMovil, 1, 8)
                If DocG.RutTransporte = "" Or DocG.RutTransporte = "00.000.000-0" Then
                    'No enviar Rut
                Else
                    doc.Encabezado.Transporte.RUTTrans = FormatoRut(DocG.RutTransporte, ModoRut.SII)
                End If

                doc.Encabezado.Transporte.DirDest = TextoDTE(Mid(DocG.DireccionDestino, 1, 70))
                doc.Encabezado.Transporte.CmnaDest = TextoDTE(Mid(DocG.ComunaDestino, 1, 20))
                doc.Encabezado.Transporte.PatenteCarro = Mid(DocG.PatenteRemolque, 1, 8)
                If DocG.SalidaTraslado.Year > 2000 Then
                    doc.Encabezado.Transporte.FchSalida = New LiquidTechnologies.Runtime.Net20.XmlDateTime(DocG.SalidaTraslado)
                    Dim hraSalida As New LiquidTechnologies.Runtime.Net20.XmlDateTime(LiquidTechnologies.Runtime.Net20.XmlDateTime.DateType.time)
                    hraSalida.SetTime(DocG.SalidaTraslado)
                    doc.Encabezado.Transporte.HraSalida = hraSalida
                End If
                If DocG.LlegadaTraslado.Year > 2000 Then
                    doc.Encabezado.Transporte.FchLlegada = New LiquidTechnologies.Runtime.Net20.XmlDateTime(DocG.LlegadaTraslado)
                End If
                Dim hraLlegada As New LiquidTechnologies.Runtime.Net20.XmlDateTime(LiquidTechnologies.Runtime.Net20.XmlDateTime.DateType.time)
                If Mid(DocG.RutChofer, 1, 2) = "  " Or Mid(DocG.RutChofer, 1, 2) = "__" Then ' dos espacios, es que no se grabo el rut bien, puede venir vacio o "  .   .   - "
                    'No se declara chofer
                Else
                    doc.Encabezado.Transporte.Chofer = New DocumentoDTE.SiiDte.Chofer()
                    doc.Encabezado.Transporte.Chofer.RUTChofer = FormatoRut(If(DocG.RutChofer.Trim = "", "00.000.000-0", DocG.RutChofer), ModoRut.SII)
                    doc.Encabezado.Transporte.Chofer.NombreChofer = TextoDTE(Mid(DocG.NombreChofer, 1, 30))
                End If
            End If

            'Referencias
            If DocumentosRef.Any Then
                If DocumentosRef.First().TipoDoc <> "" Then
                    Dim Referencias As Integer = 1
                    For Each dr In DocumentosRef
                        Dim reference = New DocumentoDTE.SiiDte.Referencia()
                        reference.NroLinRef = Referencias
                        reference.TpoDocRef = CodigoDocSII(dr.TipoDoc).ToText
                        If Val(CodigoDocSII(dr.TipoDoc)) = 999 Then
                            reference.TpoDocRef = "HES"
                        End If
                        If Val(CodigoDocSII(dr.TipoDoc)) = 998 Then
                            reference.TpoDocRef = "HEM"
                        End If
                        reference.FolioRef = dr.NumDoc.Trim
                        reference.FchRef = New LiquidTechnologies.Runtime.Net20.XmlDateTime(dr.Fecha.Date)
                        If tipoDoc = TiposDoc.NotaCredito Or tipoDoc = TiposDoc.NotaDebito Then
                            reference.CodRef = CodigoReferenciaNC(tipoDoc, DocG.Motivo.ToString.Trim)
                            reference.RazonRef = TextoDTE(DescripcionMotivo(tipoDoc, DocG.Motivo.ToString.Trim))
                        End If
                        reference.RazonRef = dr.Observacion
                        doc.Referencia.Add(reference)
                        Referencias += 1
                    Next
                End If
            End If

            'ENVIO AL DTE
            MensajeDTE = ""
            Dim AmbienteDTE = CType(ambiente, Ambiente)
            Dim GenerarFolio = False   'Es para que el DTE genere correlativo automatico o no
            Dim apiURL = URL
            Dim apiAuth = clave
            Dim ServicioDTE = New DTEBoxCliente.Servicio(apiURL, apiAuth)
            Dim ResultadoDTE = ServicioDTE.EnviarDocumento(dte, AmbienteDTE, fechaResolucion.Date, resolucion, DTEBoxCliente.TipoPdf417.Fuente, GenerarFolio)

            Dim anterior = DocG.STATUS_DTE

            If (ResultadoDTE.ResultadoServicio.Estado = DTEBoxCliente.EstadoResultado.Ok) Then
                If DocG.STATUS_DTE <> EstadosSII.Aprobado Then
                    DocG.DTE = True
                    DocG.STATUS_DTE = EstadosSII.Aprobado
                    Try
                        If ResultadoDTE.TED IsNot Nothing Then
                            DocG.TED = ResultadoDTE.TED
                        Else
                            DocG.STATUS_DTE = EstadosSII.ErrorTimbre
                        End If
                        DC.SubmitChanges()
                    Catch ex As Exception
                    End Try
                End If
            Else
                MensajeDTE = ResultadoDTE.ResultadoServicio.Descripcion
                If InStr(MensajeDTE.ToUpper, "BAD DATA") > 0 Then
                    MensajeDTE = "Error en CAF descargado desde SII."
                End If

                If DocG.STATUS_DTE = EstadosSII.No_Enviado Then
                    MensajeDTE = "Documento rechazado por el DTE"
                    DocG.STATUS_DTE = EstadosSII.Rechazado
                End If
                If DocG.STATUS_DTE = EstadosSII.Existente Then
                    MensajeDTE = "Documento ya existe en SII"
                    DocG.STATUS_DTE = EstadosSII.Rechazado
                End If
                If DocG.STATUS_DTE = EstadosSII.Timbrado Then
                    If MensajeDTE = "Existe un documento con el timbre fiscal generado" Then
                        DocG.STATUS_DTE = EstadosSII.Rechazado
                    End If
                End If

                DocG.MensajeSincronizacion = MensajeDTE

                DC.SubmitChanges()
                Return False
            End If

            ActivityLog.Log($"{tipoDoc} {numDoc} Local {local} {G_USUARIOACTUAL} Respuesta: {MensajeDTE} Estado: {CType(DocG.STATUS_DTE, EstadosSII)} Anterior: {CType(anterior, EstadosSII)}", "RepuestaEnvioDTE", "DTE")

            'No Generar XML Automático.  Solo si se imprime El documento.
            'If generarXML Then
            '    System.Threading.Thread.Sleep(1000)
            '    Dim resultado As DTEBoxCliente.ResultadoRecuperarXml = ServicioDTE.RecuperarXml(AmbienteDTE, GrupoBusqueda.Emitidos, rutEmisor,
            '                                                                                    CType(CodigoDTEConsulta(tipoDoc), DocumentoDTE.SiiDte.Enumerations.DTEType),
            '                                                                                    numDoc.ToInt(), True)
            '    If (resultado.ResultadoServicio.Estado = DTEBoxCliente.EstadoResultado.Ok) Then
            '        Dim xmlValor = resultado.Datos
            '        Dim ArchivoXML = RutaDocumentoElectronico(TipoArchivo.XMLDist, local, tipoDoc, numDoc, DocG.Cliente)
            '        System.IO.File.WriteAllText(ArchivoXML, xmlValor)
            '    Else
            '        If mensajes Then
            '            MsgError("Error al obtener XML de distribución: " & resultado.ResultadoServicio.Descripcion)
            '        End If
            '    End If
            'End If

        Catch ex As Exception
            ActivityLog.LogException(ex.Message, "Emitir_DTE")
            MensajeDTE = ex.Message.ToString.Trim
            Return False
        End Try


        Return True
    End Function


    Public Function EmitirDocumentoExportacion(local As Decimal, tipoDoc As String, numDoc As Decimal, Optional GenerarXML As Boolean = False, Optional mensajes As Boolean = True) As Boolean
        Try
            Dim DocumentosRef As New List(Of DocRef)

            Dim DC As New MarketONEDataContext(P_CONEXION)

            Dim rutEmisor = FE_Rut_Emisor
            Dim razonSocial = FE_Razon_Social
            Dim giro = FE_Giro
            Dim direccion = FE_Direccion
            Dim comuna = FE_Comuna
            Dim ciudad = FE_Ciudad
            Dim email = FE_Email
            Dim ateco = FE_Ateco.ToInt()
            Dim ambiente = FE_Ambiente
            Dim URL = FE_DTE
            Dim clave = FE_Llave
            Dim fechaResolucion = FE_FResolucion
            Dim resolucion = FE_NResolucion

            If tipoDoc = TiposDoc.FacturaExportacion Or tipoDoc = TiposDoc.NotaCreditoExportacion Or tipoDoc = TiposDoc.NotaDebitoExportacion Then
                'Permitir documento
            Else
                Return False
            End If

            Dim dte As New DocumentoDTE.SiiDte.DTE()
            dte.DTE_Choice = New DocumentoDTE.SiiDte.DTE_Choice()

            dte.DTE_Choice.Exportaciones = New DocumentoDTE.SiiDte.Exportaciones()
            Dim Exp As DocumentoDTE.SiiDte.Exportaciones
            Exp = dte.DTE_Choice.Exportaciones

            Exp.Encabezado = New DocumentoDTE.SiiDte.EncabezadoB()
            Exp.Encabezado.IdDoc = New DocumentoDTE.SiiDte.IdDocB()

            Dim DocG = DC.DocumentosG.FirstOrDefault(Function(x) x.Local = local And x.TipoDoc = tipoDoc And x.Numero = numDoc)
            If DocG Is Nothing Then
                Return False
            End If

            If DocG.STATUS_DTE = EstadosSII.Aprobado Then
                Return True
            End If

            If DocG.Total = 0 Then
                Return False
            End If

            Dim ListaDocD = DC.DocumentosD.Where(Function(x) x.Local = local And x.TipoDoc = tipoDoc And x.Numero = numDoc).ToList
            If Not ListaDocD.Any Then
                Return False
            End If

            Dim Cli = DC.Clientes.FirstOrDefault(Function(x) x.Cliente = DocG.Cliente)
            If Cli Is Nothing Then
                Return False
            End If

            Dim Loc = DC.Locales.FirstOrDefault(Function(x) x.Local = local)
            If Loc IsNot Nothing Then
                rutEmisor = FormatoRut(Loc.RutLocal, ModoRut.SII)
                ambiente = Val(IIf(Loc.Produccion, DTEBoxCliente.Ambiente.Produccion, DTEBoxCliente.Ambiente.Homologacion))
                razonSocial = TextoDTE(Loc.RazonLocal.Truncate(100))
                giro = TextoDTE(Loc.GiroLocal.Truncate(80))
                direccion = TextoDTE(Loc.DirLocal.Truncate(60))
                comuna = TextoDTE(ObtenerComuna(Loc.Comuna))
                ciudad = TextoDTE(ObtenerComuna(Loc.Comuna))
                email = Loc.EMailLocal.ToString.Trim
                ateco = Loc.ATECO.ToInt()
                clave = Loc.Llave.ToString.Trim
                resolucion = Loc.NResolucion.ToInt()
                fechaResolucion = Loc.FResolucion
                URL = $"http://{Loc.IP_DTE.ToString.Trim}/api/Core.svc/Core"
            End If

            'Documento/Referencia
            Dim wDocR = DC.DocumentosR.Where(Function(x) x.Local = local And x.Numero = numDoc And x.TipoDoc = tipoDoc).ToList
            For Each docr In wDocR
                DocumentosRef.Add(New DocRef With
                {
                    .Fecha = docr.FechaRef,
                    .NumDoc = Regex.Replace(docr.NumDocRef, "\D", ""),
                    .TipoDoc = docr.TipoDocRef,
                    .CodRef = "1",
                    .Observacion = docr.Observacion
                })
            Next
            If Not DocumentosRef.Any Then
                DocumentosRef.Add(New DocRef With
                {
                    .Fecha = New Date(2000, 1, 1),
                    .NumDoc = "",
                    .TipoDoc = "",
                    .Observacion = ""
                })
            End If

            'Documento/Encabezado/IdDoc
            Dim wTipoDoc As DocumentoDTE.SiiDte.Enumerations.EXPType
            If tipoDoc = TiposDoc.FacturaExportacion Then
                wTipoDoc = DocumentoDTE.SiiDte.Enumerations.EXPType.n110
            End If
            If tipoDoc = TiposDoc.NotaCreditoExportacion Then
                wTipoDoc = DocumentoDTE.SiiDte.Enumerations.EXPType.n112
            End If
            If tipoDoc = TiposDoc.NotaDebitoExportacion Then
                wTipoDoc = DocumentoDTE.SiiDte.Enumerations.EXPType.n111
            End If
            Exp.Encabezado.IdDoc.TipoDTE = wTipoDoc
            Exp.Encabezado.IdDoc.Folio = numDoc.ToInt()
            Exp.Encabezado.IdDoc.FchEmis = New LiquidTechnologies.Runtime.Net20.XmlDateTime(DocG.Fecha.Date)
            Exp.Encabezado.IdDoc.FmaPago = CType(IIf(DocG.FPago.Trim.ToUpper = FormasPago.Efectivo, DocumentoDTE.SiiDte.Enumerations.IdDocB_FmaPago.n1, DocumentoDTE.SiiDte.Enumerations.IdDocB_FmaPago.n2), DocumentoDTE.SiiDte.Enumerations.IdDocB_FmaPago)

            If tipoDoc = TiposDoc.NotaCredito Or tipoDoc = TiposDoc.NotaDebito Then
                Exp.Encabezado.IdDoc.FmaPago = DocumentoDTE.SiiDte.Enumerations.IdDocB_FmaPago.n1
                Exp.Encabezado.IdDoc.FchVenc = New LiquidTechnologies.Runtime.Net20.XmlDateTime(DocumentosRef.First().Fecha)
            End If

            'Documento/Encabezado/Emisor
            Exp.Encabezado.Emisor = New DocumentoDTE.SiiDte.EmisorB()
            Exp.Encabezado.Emisor.RUTEmisor = rutEmisor
            Exp.Encabezado.Emisor.RznSoc = razonSocial
            Exp.Encabezado.Emisor.GiroEmis = giro
            Exp.Encabezado.Emisor.Acteco.Add(ateco)
            Exp.Encabezado.Emisor.DirOrigen = direccion
            Exp.Encabezado.Emisor.CmnaOrigen = comuna
            Exp.Encabezado.Emisor.CiudadOrigen = ciudad
            Exp.Encabezado.Emisor.CorreoEmisor = email

            'Documento/Encabezado/Receptor
            Exp.Encabezado.Receptor = New DocumentoDTE.SiiDte.ReceptorB()
            Exp.Encabezado.Receptor.RUTRecep = FormatoRut(DocG.Rut, ModoRut.SII)
            Exp.Encabezado.Receptor.RUTRecep = FormatoRut(P_RUTEXPORTACION, ModoRut.SII) 'RUT Cliente Extranjero fijo 55.555.555-5 Obligatorio
            Exp.Encabezado.Receptor.RznSocRecep = TextoDTE(Cli.Nombre.Truncate(100))
            Exp.Encabezado.Receptor.CdgIntRecep = TextoDTE(Cli.IdentificadorExtranjero.ToString)
            If Cli.Giro.Trim <> "" Then
                Exp.Encabezado.Receptor.GiroRecep = TextoDTE(Cli.Giro.Truncate(40))
            Else
                Exp.Encabezado.Receptor.GiroRecep = "PARTICULAR"
            End If
            If Cli.Contacto.Trim <> "" Then
                Exp.Encabezado.Receptor.Contacto = TextoDTE(Cli.Contacto.Truncate(80))
            Else
                Exp.Encabezado.Receptor.Contacto = TextoDTE(Cli.Nombre.Truncate(80))
            End If
            If Cli.Email.Trim <> "" Then
                Exp.Encabezado.Receptor.CorreoRecep = TextoDTE(Cli.Email.Truncate(80)).ToString.Trim
            End If
            Exp.Encabezado.Receptor.DirRecep = TextoDTE(Cli.Direccion.Truncate(70))
            Exp.Encabezado.Receptor.CmnaRecep = TextoDTE(Cli.ComunaExtranjera).Truncate(20)
            Exp.Encabezado.Receptor.CiudadRecep = TextoDTE(Cli.CiudadExtranjera).Truncate(20)

            'Documento/Descuentos Generales
            If DocG.Descuento > 0 Then
                Dim descuento = New DocumentoDTE.SiiDte.DscRcgGlobalA()
                descuento.NroLinDR = 1
                If DocG.Moneda.Trim = "USD" Then
                    descuento.TpoMov = DocumentoDTE.SiiDte.Enumerations.DscRcgGlobalA_TpoMov.D
                    descuento.TpoValor = DocumentoDTE.SiiDte.Enumerations.DscRcgGlobalA_TpoValor.Dollar
                    descuento.ValorDR = DocG.Descuento.ToDecimal(2)
                Else
                    descuento.TpoMov = DocumentoDTE.SiiDte.Enumerations.DscRcgGlobalA_TpoMov.D
                    descuento.TpoValor = DocumentoDTE.SiiDte.Enumerations.DscRcgGlobalA_TpoValor.Dollar
                    descuento.ValorDR = DocG.Descuento.ToInt()
                End If

                Exp.DscRcgGlobal.Add(descuento)
            End If

            If DocG.RecargoLogistica > 0 Then
                Dim recargo = New DocumentoDTE.SiiDte.DscRcgGlobalA
                recargo.NroLinDR = 1
                If DocG.Descuento > 0 Then
                    recargo.NroLinDR += 1
                End If
                recargo.TpoMov = DocumentoDTE.SiiDte.Enumerations.DscRcgGlobalA_TpoMov.R
                recargo.TpoValor = DocumentoDTE.SiiDte.Enumerations.DscRcgGlobalA_TpoValor.Dollar
                recargo.ValorDR = DocG.RecargoLogistica.ToInt()
                Exp.DscRcgGlobal.Add(recargo)
            End If

            'Documento/Encabezado/Totales
            Exp.Encabezado.Totales = New DocumentoDTE.SiiDte.TotalesB()
            If DocG.Moneda.Trim = "USD" Then
                Exp.Encabezado.Totales.TpoMoneda = DocumentoDTE.SiiDte.Enumerations.TipMonType.DOLARSpaceUSA
                Exp.Encabezado.Totales.MntTotal = DocG.Total.ToDecimal(2)
                Exp.Encabezado.Totales.MntExe = DocG.Total.ToDecimal(2)
            Else
                Exp.Encabezado.Totales.TpoMoneda = DocumentoDTE.SiiDte.Enumerations.TipMonType.PESOSpaceCL
                Exp.Encabezado.Totales.MntTotal = DocG.Total.ToInt()
                Exp.Encabezado.Totales.MntExe = DocG.Total.ToInt()
            End If

            'Exportaciones/Encabezado/OtraMoneda
            'NOTA IMPORTANTE: Al vender en peso chileno, la conversion es en peso chileno.  
            '                 Al vender en dolar, se hace la venta en dolar y en esta seccion se anota la conversion en peso chileno

            Exp.Encabezado.OtraMoneda = New DocumentoDTE.SiiDte.OtraMonedaA()
            'Exp.Encabezado.OtraMoneda.TpoMoneda = DocumentoDTE.SiiDte.Enumerations.TipMonType.DOLARSpaceUSA
            'Exp.Encabezado.OtraMoneda.MntTotOtrMnda = (DocG.Total / G_DOLAR).ToDecimal(2)
            'Exp.Encabezado.OtraMoneda.TpoCambio = G_DOLAR
            'Exp.Encabezado.OtraMoneda.MntExeOtrMnda = (DocG.Total / G_DOLAR).ToDecimal(2)

            If DocG.Moneda.Trim = "USD" Then
                Exp.Encabezado.OtraMoneda.TpoMoneda = DocumentoDTE.SiiDte.Enumerations.TipMonType.PESOSpaceCL
                Exp.Encabezado.OtraMoneda.MntTotOtrMnda = (DocG.Total * G_DOLAR).ToDecimal(2)
                Exp.Encabezado.OtraMoneda.TpoCambio = G_DOLAR
                Exp.Encabezado.OtraMoneda.MntExeOtrMnda = (DocG.Total * G_DOLAR).ToDecimal(2)
            Else
                Exp.Encabezado.OtraMoneda.TpoMoneda = DocumentoDTE.SiiDte.Enumerations.TipMonType.PESOSpaceCL
                Exp.Encabezado.OtraMoneda.MntTotOtrMnda = DocG.Total.ToInt()
                Exp.Encabezado.OtraMoneda.TpoCambio = 1
                Exp.Encabezado.OtraMoneda.MntExeOtrMnda = DocG.Total.ToInt()
            End If


            'Exportaciones/Encabezado/Transporte
            Exp.Encabezado.Transporte = New DocumentoDTE.SiiDte.TransporteA()
            Exp.Encabezado.Transporte.Aduana = New DocumentoDTE.SiiDte.AduanaA()
            Exp.Encabezado.Transporte.Aduana.CodModVenta = 1   'Tipo 1: Afirme (El precio se pacta antes del embarque y no varía)

            'Cuando hay datos de aduana, se deben llenar
            'Exp.Encabezado.Transporte.Aduana.CodModVenta = 12
            'Exp.Encabezado.Transporte.Aduana.CodClauVenta = 12
            'Exp.Encabezado.Transporte.Aduana.TotClauVenta = 12345
            'Exp.Encabezado.Transporte.Aduana.CodPaisRecep = 53
            ' NOTA: NO hay impuestos adicionales en los detalles del documento

            'Documento/Detalle
            Dim det As DocumentoDTE.SiiDte.DetalleB
            Dim Lineas = 1

            Dim primeroLinea = "ARTICULO"
            For Each DocD In ListaDocD
                If Lineas >= 100 Then
                    'GDExpress no soporta mas de 100 lineas en un documento
                    Exit For
                End If
                det = New DocumentoDTE.SiiDte.DetalleB()

                Dim unidad = "UN"

                Dim Art = DC.Articulos.FirstOrDefault(Function(x) x.Articulo = DocD.Articulo)

                If Art IsNot Nothing Then
                    unidad = Art.Unidad.Trim
                End If

                Dim glosa = DocD.Glosa.Trim
                If glosa = "" Then glosa = Art.Descripcion.Trim
                Dim articulo = glosa.Split(CChar(vbLf))(0).Trim.Truncate(1000)
                If primeroLinea = "ARTICULO" Then
                    primeroLinea = TextoDTE(articulo).Trim
                    articulo = primeroLinea
                End If

                det.NroLinDet = Lineas
                If Lineas = 1 Then
                    det.NmbItem = primeroLinea
                Else
                    det.NmbItem = TextoDTE(articulo).Truncate(80).Trim
                End If
                det.MontoItem = Math.Round(DocD.Cantidad * DocD.Neto, 0).ToInt
                det.DscItem = TextoDTE(articulo).Truncate(1000).Trim
                det.QtyItem = DocD.Cantidad
                det.UnmdItem = unidad
                det.IndExe = 0   'En el DTE les llega como un 1
                det.PrcItem = Math.Round(DocD.Neto, 5)

                'Nota: No hay Impuestos Adicionales en Factura de Exportacion

                Exp.Detalle.Add(det)
                Lineas += 1
            Next


            'Referencias
            If DocumentosRef.Any Then
                If DocumentosRef.First().TipoDoc <> "" Then
                    Dim Referencias As Integer = 1
                    For Each dr In DocumentosRef
                        Dim reference = New DocumentoDTE.SiiDte.ReferenciaB()
                        reference.NroLinRef = Referencias
                        reference.TpoDocRef = CodigoDocSII(dr.TipoDoc).ToText
                        If Val(CodigoDocSII(dr.TipoDoc)) = 999 Then
                            reference.TpoDocRef = "HES"
                        End If
                        If Val(CodigoDocSII(dr.TipoDoc)) = 998 Then
                            reference.TpoDocRef = "HEM"
                        End If
                        reference.FolioRef = dr.NumDoc
                        reference.FchRef = New LiquidTechnologies.Runtime.Net20.XmlDateTime(dr.Fecha.Date)
                        If tipoDoc = TiposDoc.NotaCredito Or tipoDoc = TiposDoc.NotaDebito Then
                            reference.CodRef = CodigoReferenciaNCEXP(tipoDoc, DocG.Motivo.ToString.Trim)
                            reference.RazonRef = TextoDTE(DescripcionMotivo(tipoDoc, DocG.Motivo.ToString.Trim))
                        Else
                            reference.CodRef = CodigoReferenciaNCEXP(tipoDoc, DocG.Motivo.ToString.Trim)
                        End If
                        reference.RazonRef = dr.Observacion
                        Exp.Referencia.Add(reference)
                        Referencias += 1
                    Next
                End If
            End If

            Dim anterior = DocG.STATUS_DTE

            'ENVIO AL DTE
            MensajeDTE = ""
            Dim AmbienteDTE = CType(ambiente, Ambiente)
            Dim GenerarFolio = False   'Es para que el DTE genere correlativo automatico o no
            Dim apiURL = URL
            Dim apiAuth = clave
            Dim ServicioDTE = New DTEBoxCliente.Servicio(apiURL, apiAuth)
            Dim ResultadoDTE = ServicioDTE.EnviarDocumento(dte, AmbienteDTE, fechaResolucion.Date, resolucion.ToInt(), DTEBoxCliente.TipoPdf417.Fuente, GenerarFolio)


            If (ResultadoDTE.ResultadoServicio.Estado = DTEBoxCliente.EstadoResultado.Ok) Then
                If DocG.STATUS_DTE <> EstadosSII.Aprobado Then
                    DocG.DTE = True
                    DocG.STATUS_DTE = EstadosSII.Aprobado
                    Try
                        If ResultadoDTE.TED IsNot Nothing Then
                            DocG.TED = ResultadoDTE.TED
                        Else
                            DocG.STATUS_DTE = EstadosSII.ErrorTimbre
                        End If
                        DC.SubmitChanges()
                    Catch ex As Exception
                    End Try
                End If
            Else
                MensajeDTE = ResultadoDTE.ResultadoServicio.Descripcion

                If DocG.STATUS_DTE = EstadosSII.No_Enviado Then
                    DocG.STATUS_DTE = EstadosSII.Rechazado
                End If
                If DocG.STATUS_DTE = EstadosSII.Existente Then
                    DocG.STATUS_DTE = EstadosSII.Aprobado
                End If
                If DocG.STATUS_DTE = EstadosSII.Timbrado Then
                    If MensajeDTE = "Existe un documento con el timbre fiscal generado" Then
                        DocG.STATUS_DTE = EstadosSII.Aprobado
                    End If
                End If

                DC.SubmitChanges()
                Return False
            End If

            ActivityLog.Log($"{tipoDoc} {numDoc} Local {local} {G_USUARIOACTUAL} Respuesta: {MensajeDTE} Estado: {CType(DocG.STATUS_DTE, EstadosSII)} Anterior: {CType(anterior, EstadosSII)}", "RepuestaEnvioDTE", "DTE")

            If GenerarXML Then
                System.Threading.Thread.Sleep(1000)
                Dim resultado As DTEBoxCliente.ResultadoRecuperarXml = ServicioDTE.RecuperarXml(AmbienteDTE, GrupoBusqueda.Emitidos, rutEmisor,
                                                                                                CType(CodigoDTEConsulta(tipoDoc), DocumentoDTE.SiiDte.Enumerations.DTEType),
                                                                                                numDoc.ToInt(), True)
                If (resultado.ResultadoServicio.Estado = DTEBoxCliente.EstadoResultado.Ok) Then
                    Dim xmlValor = resultado.Datos
                    Dim ArchivoXML = RutaDocumentoElectronico(TipoArchivo.XMLDist, local, tipoDoc, numDoc, DocG.Cliente)
                    System.IO.File.WriteAllText(ArchivoXML, xmlValor)
                Else
                    If mensajes Then
                        MsgError("Error al obtener XML de distribución: " & resultado.ResultadoServicio.Descripcion)
                    End If
                End If
            End If

        Catch ex As Exception
            ActivityLog.LogException(ex.Message, "Emitir_DTE")
            MensajeDTE = ex.Message.ToString.Trim
            Return False
        End Try


        Return True
    End Function

    Public Function ActualizarXML(local As Decimal, tipoDoc As String, numDoc As Decimal) As Boolean
        Dim DC As New MarketONEDataContext(P_CONEXION)

        Dim ambiente = FE_Ambiente
        Dim URL = FE_DTE
        Dim clave = FE_Llave

        'Sacar Parámetros del Local para Emitir el Documento Electrónico
        Dim Loc = DC.Locales.FirstOrDefault(Function(x) x.Local = local)
        If Loc IsNot Nothing Then
            ambiente = If(Loc.Produccion, DTEBoxCliente.Ambiente.Produccion, DTEBoxCliente.Ambiente.Homologacion)
            clave = Loc.Llave.Trim
            URL = $"http://{Loc.IP_DTE.ToString.Trim}/api/Core.svc/Core"
        End If

        'Recuperar Documento       
        Dim DocG = DC.DocumentosG.FirstOrDefault(Function(x) x.Local = local And x.TipoDoc = tipoDoc And x.Numero = numDoc)
        If DocG Is Nothing Then
            Return False
        End If

        Dim ListaDocD = DC.DocumentosD.Where(Function(x) x.Local = local And x.TipoDoc = tipoDoc And x.Numero = numDoc).ToList
        If Not ListaDocD.Any Then
            Return False
        End If

        Dim Cli = DC.Clientes.FirstOrDefault(Function(x) x.Cliente = DocG.Cliente)
        If Cli Is Nothing Then
            Return False
        End If

        Dim dte As DocumentoDTE.SiiDte.DTE
        Try
            dte = New DocumentoDTE.SiiDte.DTE()
            dte.DTE_Choice = New DocumentoDTE.SiiDte.DTE_Choice()

            'Documento
            dte.DTE_Choice.Documento = New DocumentoDTE.SiiDte.Documento()

            Dim ServicioDTE As DTEBoxCliente.Servicio
            ServicioDTE = New DTEBoxCliente.Servicio(URL, clave)

            'Generar XML de Distribución
            Dim resultado As DTEBoxCliente.ResultadoRecuperarXml = ServicioDTE.RecuperarXml(CType(ambiente, Ambiente), GrupoBusqueda.Emitidos, FE_Rut_Emisor,
                                                                                            CType(CodigoDTEConsulta(tipoDoc), DocumentoDTE.SiiDte.Enumerations.DTEType),
                                                                                            numDoc.ToInt, True)

            If (resultado.ResultadoServicio.Estado = DTEBoxCliente.EstadoResultado.Ok) Then
                Dim xmlValor As String
                xmlValor = resultado.Datos
                Dim ArchivoXML = RutaDocumentoElectronico(TipoArchivo.XMLDist, local, tipoDoc, numDoc, DocG.Cliente)
                Try
                    System.IO.File.WriteAllText(ArchivoXML, xmlValor)
                Catch ex As Exception
                End Try
            End If
        Catch ex As Exception
        End Try

        Return True
    End Function

    Public Function EmitirBoleta(local As Double, tipoDoc As String, numDoc As Double) As Boolean
        Dim rutEmisor = FE_Rut_Emisor
        Dim razonSocial = FE_Razon_Social
        Dim giro = FE_Giro
        Dim direccion = FE_Direccion
        Dim comuna = FE_Comuna
        Dim ciudad = FE_Ciudad
        Dim email = FE_Email
        Dim ateco = FE_Ateco.ToInt()
        Dim ambiente = FE_Ambiente
        Dim URL = FE_DTE
        Dim clave = FE_Llave
        Dim fechaResolucion = FE_FResolucion
        Dim resolucion = FE_NResolucion

        Dim DC As New MarketONEDataContext(P_CONEXION)

        If tipoDoc <> TiposDoc.Boleta And tipoDoc <> TiposDoc.BoletaExenta Then
            Return False
        End If

        Dim Boleta As DocumentoBoleta.SiiDte.BOLETADefType
        Boleta = New DocumentoBoleta.SiiDte.BOLETADefType()

        'Boleta/Documento
        Boleta.Documento = New DocumentoBoleta.SiiDte.Documento()
        Dim doc As DocumentoBoleta.SiiDte.Documento
        doc = Boleta.Documento

        'Boleta/Documento/Encabezado
        doc.Encabezado = New DocumentoBoleta.SiiDte.Encabezado()
        doc.Encabezado.IdDoc = New DocumentoBoleta.SiiDte.IdDoc()

        'Boleta/Recuperar Documento
        Dim DocG = DC.DocumentosG.FirstOrDefault(Function(x) x.Local = local And x.TipoDoc = tipoDoc And x.Numero = numDoc)
        If DocG Is Nothing Then
            Return False
        End If

        'Verificar si el documento ya esta timbrado para saltarselo
        If DocG.STATUS_DTE = EstadosSII.Aprobado Then
            Return True
        End If

        'No enviar documento con valor 0, estos debe revisarse el caso
        If DocG.Total = 0 Then
            Return False
        End If

        Dim DetalleDoc = DC.DocumentosD.Where(Function(x) x.Local = local And x.TipoDoc = tipoDoc And x.Numero = numDoc).ToList
        If Not DetalleDoc.Any Then
            Return False
        End If

        Dim Cli = DC.Clientes.FirstOrDefault(Function(x) x.Cliente = DocG.Cliente)
        If Cli Is Nothing Then
            Return False
        End If

        'Sacar Parámetros del Local para Emitir el Documento Electrónico        
        Dim Loc = DC.Locales.FirstOrDefault(Function(x) x.Local = local)
        If Loc IsNot Nothing Then
            rutEmisor = FormatoRut(Loc.RutLocal, ModoRut.SII)
            ambiente = Val(IIf(Loc.Produccion, DTEBoxCliente.Ambiente.Produccion, DTEBoxCliente.Ambiente.Homologacion))
            razonSocial = TextoDTE(Loc.RazonLocal.Truncate(100))
            giro = TextoDTE(Loc.GiroLocal.Truncate(80))
            direccion = TextoDTE(Loc.DirLocal.Truncate(60))
            comuna = TextoDTE(ObtenerComuna(Loc.Comuna))
            ciudad = TextoDTE(ObtenerComuna(Loc.Comuna))
            email = Loc.EMailLocal.ToString.Trim
            ateco = Loc.ATECO.ToInt()
            clave = Loc.Llave.ToString.Trim
            resolucion = Loc.NResolucion.ToInt()
            fechaResolucion = Loc.FResolucion
            URL = "http://" + Loc.IP_DTE.ToString.Trim + "/api/Core.svc/Core"
        End If

        'Documento/Encabezado/IdDoc
        If tipoDoc = TiposDoc.Boleta Then
            'BOLETA NORMAL
            doc.Encabezado.IdDoc.TipoDTE = DocumentoBoleta.SiiDte.Enumerations.DTEType.n39
        Else
            'BOLETA EXENTA
            doc.Encabezado.IdDoc.TipoDTE = DocumentoBoleta.SiiDte.Enumerations.DTEType.n41
        End If
        doc.Encabezado.IdDoc.Folio = numDoc.ToInt()
        doc.Encabezado.IdDoc.FchEmis = New LiquidTechnologies.Runtime.Net20.XmlDateTime(DocG.Fecha.Date)
        doc.Encabezado.IdDoc.IndServicio = DocumentoBoleta.SiiDte.Enumerations.IdDoc_IndServicio.n1

        'Boleta/Documento/Encabezado/Emisor
        doc.Encabezado.Emisor = New DocumentoBoleta.SiiDte.Emisor()
        doc.Encabezado.Emisor.RUTEmisor = FormatoRut(rutEmisor, ModoRut.SII)
        doc.Encabezado.Emisor.RznSocEmisor = razonSocial
        doc.Encabezado.Emisor.GiroEmisor = giro
        doc.Encabezado.Emisor.DirOrigen = direccion
        doc.Encabezado.Emisor.CmnaOrigen = comuna
        doc.Encabezado.Emisor.CiudadOrigen = ciudad

        'Boleta/Documento/Encabezado/Receptor
        doc.Encabezado.Receptor = New DocumentoBoleta.SiiDte.Receptor()
        doc.Encabezado.Receptor.RUTRecep = FormatoRut(Cli.Rut, ModoRut.SII)
        doc.Encabezado.Receptor.RznSocRecep = TextoDTE(Cli.Nombre.Truncate(100))
        doc.Encabezado.Receptor.CdgIntRecep = Cli.Cliente.ToText()
        doc.Encabezado.Receptor.Contacto = TextoDTE(Cli.Contacto.Truncate(80))
        doc.Encabezado.Receptor.DirRecep = TextoDTE(Cli.Direccion.Truncate(70))
        doc.Encabezado.Receptor.CmnaRecep = TextoDTE(ObtenerComuna(Cli.Comuna).Truncate(20))
        doc.Encabezado.Receptor.CiudadRecep = TextoDTE(ObtenerComuna(Cli.Ciudad).Truncate(20))

        'Boleta/Documento/Encabezado/Totales
        doc.Encabezado.Totales = New DocumentoBoleta.SiiDte.Totales()

        doc.Encabezado.Totales.MntTotal = DocG.Total.ToInt()
        If tipoDoc = TiposDoc.BoletaExenta Then
            doc.Encabezado.Totales.MntExe = DocG.Total.ToInt()
        End If


        'PROPINA (ES NO FACTURABLE <> A EXENTO)
        If DocG.Propina > 0 Then
            doc.Encabezado.Totales.MontoNF = DocG.Propina.ToInt()
        End If


        'Por Defecto las Boletas de Puntos de Venta y POS se graban con valores brutos.  Solo las boletas de Emisión de Documentos se graban con Netos
        Dim ValoresBrutos As Boolean = True


        'Documento/Detalle
        Dim det As DocumentoBoleta.SiiDte.Detalle

        Dim Lineas = 1
        Dim primeraLinea = "ARTICULO"
        For Each DocD In DetalleDoc

            det = New DocumentoBoleta.SiiDte.Detalle()

            Dim unidad = "UN"
            Dim Art = DC.Articulos.FirstOrDefault(Function(x) x.Articulo = DocD.Articulo)

            If Art IsNot Nothing Then
                unidad = Art.Unidad.Trim
            End If

            Dim articulo = ""

            If Mid(DocD.Glosa, 1, 80).Trim = "" Then
                articulo = If(Art IsNot Nothing, Art.Descripcion.Trim, "")
            Else
                articulo = DocD.Glosa.Truncate(80).Trim
            End If
            If primeraLinea = "ARTICULO" Then
                primeraLinea = TextoDTE(articulo).Truncate(80).Trim
                articulo = primeraLinea
            End If
            If articulo.Trim = "" Then
                'El producto vendido se eliminó, por lo que se envia una glosa a SII.
                articulo = "VENTA DE PRODUCTO"
                primeraLinea = "VENTA DE PRODUCTO"
            End If

            det.NroLinDet = Lineas

            If Lineas = 1 Then
                det.NmbItem = primeraLinea
            Else
                det.NmbItem = TextoDTE(articulo).Truncate(80).Trim
            End If

            'Si la boleta graba neto o bruto
            If tipoDoc = TiposDoc.BoletaExenta Then
                det.MontoItem = Math.Round((DocD.Cantidad * DocD.Neto), 0).ToInt()
            Else
                If ValoresBrutos Then
                    det.MontoItem = Math.Round(DocD.Cantidad * DocD.Neto * (1 + (G_IVA / 100)), 0).ToInt()
                Else
                    det.MontoItem = Math.Round((DocD.Cantidad * DocD.Neto), 0).ToInt()
                End If
            End If


            det.DscItem = articulo
            det.QtyItem = DocD.Cantidad
            det.UnmdItem = unidad
            If DocD.Neto > 0 Then
                If tipoDoc = TiposDoc.BoletaExenta Then
                    det.PrcItem = Math.Round(DocD.Neto, 0).ToInt()
                Else
                    If ValoresBrutos Then
                        If Math.Round(DocD.Neto.ToDecimal(5) * (1 + (G_IVA / 100)), 0).ToInt() > 0 Then
                            det.PrcItem = Math.Round(DocD.Neto.ToDecimal(5) * (1 + (G_IVA / 100)), 0).ToInt()
                        End If
                    Else
                        If Math.Round(DocD.Neto, 0).ToInt() > 0 Then
                            det.PrcItem = Math.Round(DocD.Neto, 0).ToInt()
                        End If
                    End If
                End If
            End If

            doc.Detalle.Add(det)
            Lineas += 1
        Next

        If DocG.Propina > 0 Then
            Dim Propina = New DocumentoBoleta.SiiDte.Detalle()
            Propina.NroLinDet = Lineas
            Propina.NmbItem = "Propina"
            Propina.DscItem = "Propina"
            Propina.PrcItem = DocG.Propina
            Propina.QtyItem = 1D
            Propina.MontoItem = DocG.Propina.ToInt()
            Propina.IndExe = DocumentoBoleta.SiiDte.Enumerations.Detalle_IndExe.n2  'Debe ser un 2, item no facturable             
            doc.Detalle.Add(Propina)
        End If

        'Llamada al Servicio
        MensajeDTE = ""

        Dim AmbienteDTE = CType(ambiente, Ambiente)
        Dim TipoPdf417 = DTEBoxCliente.TipoPdf417.Fuente

        Dim GenerarFolio = False    'Es para que el DTE genere correlativo automatico o no

        Dim apiURL = URL
        Dim apiAuth = clave

        Dim ServicioDTE = New DTEBoxCliente.Servicio(apiURL, apiAuth)
        Dim ResultadoDTE = ServicioDTE.EnviarDocumento(Boleta, AmbienteDTE, fechaResolucion, resolucion.ToInt(), TipoPdf417, GenerarFolio)

        Dim anterior = DocG.STATUS_DTE
        Dim respuesta As Boolean = True

        Try

            DC = New MarketONEDataContext(P_CONEXION)
            DocG = DC.DocumentosG.FirstOrDefault(Function(x) x.Local = local And x.TipoDoc = tipoDoc And x.Numero = numDoc)

            'Procesar Resultado del Servicio
            If (ResultadoDTE.ResultadoServicio.Estado = DTEBoxCliente.EstadoResultado.Ok) Then
                respuesta = True
                'Usar el data que viene en el resultado de la llamada                        
                'Si tipoPdf417 = DTEBoxCliente.TipoPdf417.Fuente viene el código,
                ' si es DTEBoxCliente.TipoPdf417.Grafico , Entonces arreglo de bytes codigificado en base64,
                ' usar método result.Pdf417ComoArregloBytes()                        

                If DocG.STATUS_DTE <> EstadosSII.Aprobado Then
                    DocG.STATUS_DTE = EstadosSII.Aprobado
                    DocG.TED = ResultadoDTE.TED 'XElement.Parse(ResultadoDTE.TED)
                End If

                DC.SubmitChanges()

            Else
                respuesta = False
                MensajeDTE = ResultadoDTE.ResultadoServicio.Descripcion
                'Grabar Cambio de Status
                If DocG.STATUS_DTE = EstadosSII.No_Enviado Then
                    'Grabar Cambio de Estado                
                End If
                If DocG.STATUS_DTE = EstadosSII.Existente Then
                    'Ya Existe, por lo tanto dejar Aprobado
                    DocG.STATUS_DTE = EstadosSII.Aprobado
                    respuesta = True
                End If
                If UCase(MensajeDTE).Trim = UCase("Existe un documento con el timbre fiscal generado").Trim Then
                    'Ya enviado
                    DocG.STATUS_DTE = EstadosSII.Aprobado
                    respuesta = True
                End If

                DC.SubmitChanges()
            End If

        Catch ex As Exception
            'Publicador: el error queda también en la observación del documento
            MensajeDTE = $"Error al actualizar estado de Documento: {tipoDoc} {numDoc} - {ex.Message}"
            MsgError($"Error al actualizar estado de Documento: {tipoDoc} {numDoc} - {DocG.STATUS_DTE}" & vbCrLf & ex.Message)
            respuesta = False
        End Try


        ActivityLog.Log($"{tipoDoc} {numDoc} Local {local} {G_USUARIOACTUAL} Respuesta: {MensajeDTE} Estado: {CType(DocG.STATUS_DTE, EstadosSII)} Anterior: {CType(anterior, EstadosSII)}", "RepuestaEnvioDTE", "DTE")

        Return respuesta
    End Function

    Public Function AnularDocumentoDTE(local As Double, tipoDoc As String, numDoc As Double, Optional mensaje As String = "") As Boolean
        Dim rutEmisor = FE_Rut_Emisor
        Dim ambiente = FE_Ambiente
        Dim URL = FE_DTE
        Dim clave = FE_Llave

        Dim DC As New MarketONEDataContext(P_CONEXION)

        'Boleta/Recuperar Documento
        Dim DocG = DC.DocumentosG.FirstOrDefault(Function(x) x.Local = local And x.TipoDoc = tipoDoc And x.Numero = numDoc)
        If DocG Is Nothing Then
            Return False
        End If

        'Verificar si el documento este aprobado
        If DocG.STATUS_DTE <> EstadosSII.Aprobado Then
            Return False
        End If

        'Sacar Parámetros del Local para Emitir el Documento Electrónico        
        Dim Loc = DC.Locales.FirstOrDefault(Function(x) x.Local = local)
        If Loc IsNot Nothing Then
            rutEmisor = FormatoRut(Loc.RutLocal, ModoRut.SII)
            ambiente = Val(IIf(Loc.Produccion, DTEBoxCliente.Ambiente.Produccion, DTEBoxCliente.Ambiente.Homologacion))
            clave = Loc.Llave.ToString.Trim
            URL = "http://" + Loc.IP_DTE.ToString.Trim + "/api/Core.svc/Core"
        End If

        MensajeDTE = If(mensaje = "", "Anulación Documento", mensaje)

        'Llamada al Servicio
        Dim AmbienteDTE = CType(ambiente, Ambiente)
        Dim apiURL = URL
        Dim apiAuth = clave
        Dim Respuesta As Boolean = True

        Dim ServicioDTE = New DTEBoxCliente.Servicio(apiURL, apiAuth)
        Try
            Dim ResultadoDTE = ServicioDTE.AnularDocumento(AmbienteDTE, rutEmisor, TipoDocumentoDTE(tipoDoc), CLng(numDoc.ToString), MensajeDTE)

            If (ResultadoDTE.Estado = DTEBoxCliente.EstadoResultado.Ok) Then
                Respuesta = True
            Else
                Respuesta = False
                MensajeDTE = ResultadoDTE.Descripcion
                MsgError("Error en la anulación documento: " & MensajeDTE)
            End If

        Catch ex As Exception
            MsgError($"Error Anular Documento: {tipoDoc} {numDoc} - " & vbCrLf & ex.Message)
            Respuesta = False
        End Try

        ActivityLog.Log($"{tipoDoc} {numDoc} Local {local} {G_USUARIOACTUAL} Respuesta: {MensajeDTE}", "RepuestaAnulacionDTE", "DTE")

        Return Respuesta
    End Function


    Public Function Consultar_DTE(tipoDoc As String, numero As Double, Optional rut As String = "") As String
        Dim RutDF As String

        Resultado_DTE_Codigo = "0"
        Resultado_DTE_Descripcion = ""
        Resultado_DTE_Observaciones = ""

        FE_TipoDTE = CodigoDTEConsulta(tipoDoc)

        Dim apiURL As String
        Dim apiAuth As String
        apiURL = FE_DTE
        apiAuth = FE_Llave

        If rut = "" Then
            RutDF = FE_Rut_Emisor
        Else
            RutDF = FormatoRut(rut, ModoRut.SII)
        End If

        Dim ServicioDTE = New Servicio(apiURL, apiAuth)

        Dim FE_Grupo = GrupoBusqueda.Emitidos

        Dim FE_Resultado = ServicioDTE.EstadoFiscal(CType(FE_Ambiente, Ambiente), FE_Grupo, RutDF, CType(FE_TipoDTE, TipoDocumento), numero.ToInt())

        Consultar_DTE = ""

        'Resultado de la operación, 0 exitosa, diferente de cero es error al cunsultar DTE (no es error del documento)
        If (FE_Resultado.ResultadoServicio.Estado = 0) Then
            Resultado_DTE_Codigo = FE_Resultado.Estado

            Select Case FE_Resultado.Estado.ToInt()
                Case 0
                    Resultado_DTE_Descripcion = "El documento fue autorizado"
                Case 1
                    Resultado_DTE_Descripcion = "La resolución del SII está pendiente"
                Case 2
                    Resultado_DTE_Descripcion = "El documento fue autorizado, pero con observaciones o reparos"
                Case 9
                    Resultado_DTE_Descripcion = "El documento fue rechazado"
                Case Else
                    Resultado_DTE_Descripcion = FE_Resultado.Estado
            End Select

            'Comentarios asociados al estado de autorización
            Resultado_DTE_Observaciones = FE_Resultado.Comentarios

        Else
            Resultado_DTE_Codigo = "99"
            If InStr(FE_Resultado.ResultadoServicio.Descripcion, "No existe un documento con tipo") > 0 Then
                Resultado_DTE_Descripcion = "Documento no encontrado en SII o no enviado a SII"
                Resultado_DTE_Observaciones = ""
            Else
                Resultado_DTE_Descripcion = FE_Resultado.ResultadoServicio.Descripcion.Trim
                Resultado_DTE_Observaciones = "ERROR"
            End If
        End If
        Return ""
    End Function

    Public Function ConsultarEstadoComercial(tipoDoc As String, numero As Double, Optional rut As String = "") As String
        Dim RutDF As String

        Resultado_DTE_Codigo = "0"
        Resultado_DTE_Descripcion = ""
        Resultado_DTE_Observaciones = ""

        FE_TipoDTE = CodigoDTEConsulta(tipoDoc)

        Dim apiURL As String
        Dim apiAuth As String
        apiURL = FE_DTE
        apiAuth = FE_Llave

        If rut = "" Then
            RutDF = FE_Rut_Emisor
        Else
            RutDF = FormatoRut(rut, ModoRut.SII)
        End If

        Dim ServicioDTE As DTEBoxCliente.Servicio
        ServicioDTE = New DTEBoxCliente.Servicio(apiURL, apiAuth)

        Dim EstadoDTE As DTEBoxCliente.Servicio
        EstadoDTE = New DTEBoxCliente.Servicio(apiURL, apiAuth)

        Dim FE_Grupo As DTEBoxCliente.GrupoBusqueda
        FE_Grupo = DTEBoxCliente.GrupoBusqueda.Emitidos

        Dim RetornoXML As Boolean = True

        Dim EstadoComercial As DTEBoxCliente.ResultadoConsultarEstadoComercial
        EstadoComercial = ServicioDTE.EstadoComercialDocumento(CType(FE_Ambiente, Ambiente), FE_Grupo, RutDF, CType(FE_TipoDTE, TipoDocumento), numero.ToInt(), RetornoXML)
        ConsultarEstadoComercial = ""

        'EstadoComercialDTE
        '0: Sin aceptación o rechazo comercial
        '1: Aceptado Ley 19983
        '2: Aceptado
        '3: Rechazado

        'Resultado de la operación, 0 exitosa, diferente de cero es error al cunsultar DTE (no es error del documento)
        If (EstadoComercial.ResultadoServicio.Estado = 0) Then
            Resultado_DTE_Codigo = EstadoComercial.CodigoEstadoComercial
            Resultado_DTE_Descripcion = EstadoComercial.DescripcionEstadoComercial
            Resultado_DTE_Observaciones = EstadoComercial.Observaciones
        Else
            Resultado_DTE_Codigo = "99"
            Resultado_DTE_Descripcion = EstadoComercial.ResultadoServicio.Descripcion
            Resultado_DTE_Observaciones = "ERROR"
        End If
        Return ""
    End Function

    Public Function ConsultarTracking(tipoDoc As String, numero As Double, Optional prut As String = "") As String
        Dim RutDF As String

        Resultado_DTE_Codigo = "0"
        Resultado_DTE_Descripcion = ""
        Resultado_DTE_Observaciones = ""

        FE_TipoDTE = CodigoDTEConsulta(tipoDoc)

        Dim apiURL As String
        Dim apiAuth As String
        apiURL = FE_DTE
        apiAuth = FE_Llave

        If prut = "" Then
            RutDF = FE_Rut_Emisor
        Else
            RutDF = FormatoRut(prut, ModoRut.SII)
        End If

        Dim ServicioDTE As DTEBoxCliente.Servicio
        ServicioDTE = New DTEBoxCliente.Servicio(apiURL, apiAuth)

        Dim FE_Grupo As DTEBoxCliente.GrupoBusqueda
        FE_Grupo = DTEBoxCliente.GrupoBusqueda.Emitidos

        Dim FE_Resultado As DTEBoxCliente.ResultadoSeguimiento
        FE_Resultado = ServicioDTE.ObtenerTracking(CType(FE_Ambiente, Ambiente), FE_Grupo, RutDF, CType(FE_TipoDTE, TipoDocumento), numero.ToInt())

        If FE_Resultado.Datos IsNot Nothing Then
            Dim datos = FormatoXML(FE_Resultado.Datos.Replace("<TrackId></TrackId>", ""))
            Dim trk As Tracking
            trk = DeserializarXML(datos)

            Dim Resultado = trk.EstadoTrack.Select(Function(x) $"{x.TimeStamp.Date.ToShortDateString()} {x.Estado.ToUpper()} {x.Comentario}") _
                                           .Aggregate(Function(x, y) x & vbCrLf & y)
            Return Resultado
        End If

        Return ""
    End Function

    Private Function DeserializarXML(ByVal texto As String) As Tracking
        Dim settings = New XmlReaderSettings With {
        .DtdProcessing = DtdProcessing.Parse,
        .MaxCharactersFromEntities = CLng(10000000.0),
        .XmlResolver = Nothing
        }
        Using reader = New StringReader(texto)
            Using xmlRdr = XmlReader.Create(reader, settings)
                Dim serializer = New XmlSerializer(GetType(Tracking), New XmlRootAttribute("Tracking"))
                Return CType(serializer.Deserialize(xmlRdr), Tracking)
            End Using
        End Using
    End Function

    Private Function FormatoXML(XMLString As String) As String
        Dim sw As New StringWriter()
        Dim xw As New XmlTextWriter(sw)
        xw.Formatting = Formatting.Indented
        xw.Indentation = 4
        Dim doc As New XmlDocument
        doc.LoadXml(XMLString)
        doc.Save(xw)
        Return sw.ToString()
    End Function

    <XmlRoot(ElementName:="EstadoTrack")>
    Public Class EstadoTrack
        <XmlElement(ElementName:="TimeStamp")>
        Public Property TimeStamp As DateTime
        <XmlElement(ElementName:="Codigo")>
        Public Property Codigo As String
        <XmlElement(ElementName:="Estado")>
        Public Property Estado As String
        <XmlElement(ElementName:="Comentario")>
        Public Property Comentario As String
    End Class

    <XmlRoot(ElementName:="Tracking")>
    Public Class Tracking
        <XmlElement(ElementName:="EstadoTrack")>
        Public Property EstadoTrack As List(Of EstadoTrack)
        <XmlAttribute(AttributeName:="id")>
        Public Property Id As String
        <XmlText>
        Public Property Text As String
    End Class



    Public Function Parametros_DTE(wLocal As Double) As Boolean
        Dim DC As New MarketONEDataContext(P_CONEXION)

        Parametros_DTE = False
        Try
            Dim Loc = DC.Locales.FirstOrDefault(Function(x) x.Local = wLocal)

            If Loc IsNot Nothing Then
                FE_IP = Loc.IP_DTE.ToString.Trim
                FE_EsElectronica = Loc.FElectronica
                FE_Rut_Emisor = FormatoRut(Loc.RutLocal, ModoRut.SII)
                FE_Ambiente = Val(IIf(Loc.Produccion, DTEBoxCliente.Ambiente.Produccion, DTEBoxCliente.Ambiente.Homologacion))
                FE_Razon_Social = TextoDTE(Mid(Loc.RazonLocal, 1, 100))
                FE_Giro = TextoDTE(Mid(Loc.GiroLocal, 1, 80))
                FE_Direccion = TextoDTE(Mid(Loc.DirLocal, 1, 60))
                FE_Comuna = TextoDTE(ObtenerComuna(Loc.Comuna))
                FE_Ciudad = TextoDTE(ObtenerComuna(Loc.Comuna))
                FE_Email = Loc.EMailLocal.ToString.Trim
                FE_Ateco = Val(Loc.ATECO)
                FE_Llave = Loc.Llave.ToString.Trim
                FE_NResolucion = Loc.NResolucion
                FE_FResolucion = Loc.FResolucion
                FE_DTE = "http://" + FE_IP + "/api/Core.svc/Core"
                FE_FV = Loc.FV_Elect
                FE_GD = Loc.GD_Elect
                FE_NC = Loc.NC_Elect
                FE_FE = Loc.FE_Elect
                FE_ND = Loc.ND_Elect
                FE_BV = Loc.BV_Elect
                FE_BE = Loc.BE_Elect
                FE_FC = Loc.FC_Elect
                FE_FX = Loc.FX_Elect
                FE_NX = Loc.NX_Elect
                FE_DX = Loc.DX_Elect
                FE_Envio_Automatico = Loc.Envio_Automatico
                Parametros_DTE = True
            Else
                FE_IP = "127.0.0.1"
                FE_EsElectronica = False
                FE_Ambiente = DTEBoxCliente.Ambiente.Homologacion
                FE_Rut_Emisor = FormatoRut(Loc.RutLocal, ModoRut.SII)
                FE_Razon_Social = ""
                FE_Giro = ""
                FE_Direccion = ""
                FE_Comuna = ""
                FE_Ciudad = ""
                FE_Email = ""
                FE_Ateco = 0
                FE_Llave = ""
                FE_NResolucion = 0
                FE_FResolucion = New DateTime(2000, 1, 1)
                FE_DTE = "http://" + FE_IP + "/api/Core.svc/Core"
                FE_FV = False
                FE_GD = False
                FE_NC = False
                FE_FE = False
                FE_ND = False
                FE_BV = False
                FE_BE = False
                FE_FC = False
                FE_FX = False
                FE_NX = False
                FE_DX = False
                FE_Envio_Automatico = False
            End If

            If FE_Giro = "" Or FE_Direccion = "" Or FE_Rut_Emisor = "" Or FE_Razon_Social = "" Then
                Informacion("Algunos parámetros de facturación electrónica no se encuentran completos. Notifique a soporte.")
            End If

            'Publicador: solo se crea la carpeta TED (la usa el generador del timbre). CAF, PDF y TXT no se usan en el
            'envío automático; si alguna vez se necesitan, RutaDocumentoElectronico crea la carpeta al usarla.
            Directorio_CAF = P_DIRECTORIO & "\CAF\"
            Directorio_TED = P_DIRECTORIO & "\TED\"
            If Not System.IO.Directory.Exists(Directorio_TED) Then
                System.IO.Directory.CreateDirectory(Directorio_TED)
            End If
            Directorio_PDF = P_DIRECTORIO & "\PDF\"
            Directorio_TXT = P_DIRECTORIO & "\TXT\"

        Catch ex As Exception
            Return False
        End Try

        Return True
    End Function

    Public Function EsElectronico(tipoDoc As String) As Boolean
        If FE_EsElectronica Then
            If FE_FV And tipoDoc = TiposDoc.Factura Then Return True
            If FE_GD And tipoDoc = TiposDoc.GuiaDespacho Then Return True
            If FE_NC And tipoDoc = TiposDoc.NotaCredito Then Return True
            If FE_FE And tipoDoc = TiposDoc.FacturaExenta Then Return True
            If FE_ND And tipoDoc = TiposDoc.NotaDebito Then Return True
            If FE_BV And tipoDoc = TiposDoc.Boleta Then Return True
            If FE_BE And tipoDoc = TiposDoc.BoletaExenta Then Return True
            If FE_FC And tipoDoc = TiposDoc.FacturaCompra Then Return True
            If FE_FX And tipoDoc = TiposDoc.FacturaExportacion Then Return True
            If FE_NX And tipoDoc = TiposDoc.NotaCreditoExportacion Then Return True
            If FE_DX And tipoDoc = TiposDoc.NotaDebitoExportacion Then Return True
        End If
        Return False
    End Function

    Public Function CodigoDTEConsulta(tipoDoc As String) As Integer
        If tipoDoc = TiposDoc.Factura Then
            Return TipoDocumento.TIPO_33
        End If
        If tipoDoc = TiposDoc.GuiaDespacho Then
            Return TipoDocumento.TIPO_52
        End If
        If tipoDoc = TiposDoc.NotaCredito Then
            Return TipoDocumento.TIPO_61
        End If
        If tipoDoc = TiposDoc.NotaDebito Then
            Return TipoDocumento.TIPO_56
        End If
        If tipoDoc = TiposDoc.FacturaExenta Then
            Return TipoDocumento.TIPO_34
        End If
        If tipoDoc = TiposDoc.Cotizacion Then
            Return TipoDocumento.TIPO_41
        End If
        If tipoDoc = TiposDoc.Boleta Then
            Return TipoDocumento.TIPO_39
        End If
        If tipoDoc = TiposDoc.FacturaCompra Then
            Return TipoDocumento.TIPO_46
        End If
        If tipoDoc = TiposDoc.FacturaExportacion Then
            Return TipoDocumento.TIPO_110
        End If
        If tipoDoc = TiposDoc.NotaCreditoExportacion Then
            Return TipoDocumento.TIPO_112
        End If
        If tipoDoc = TiposDoc.NotaDebitoExportacion Then
            Return TipoDocumento.TIPO_111
        End If
        Return 0
    End Function

    Public Function CodigoDTEEmision(tipoDoc As String) As Integer
        If tipoDoc = TiposDoc.Factura Then
            Return DocumentoDTE.SiiDte.Enumerations.DTEType.n33
        End If
        If tipoDoc = TiposDoc.GuiaDespacho Then
            Return DocumentoDTE.SiiDte.Enumerations.DTEType.n52
        End If
        If tipoDoc = TiposDoc.NotaCredito Then
            Return DocumentoDTE.SiiDte.Enumerations.DTEType.n61
        End If
        If tipoDoc = TiposDoc.NotaDebito Then
            Return DocumentoDTE.SiiDte.Enumerations.DTEType.n56
        End If
        If tipoDoc = TiposDoc.FacturaExenta Then
            Return DocumentoDTE.SiiDte.Enumerations.DTEType.n34
        End If
        If tipoDoc = TiposDoc.BoletaExenta Then
            Return 41
        End If
        If tipoDoc = TiposDoc.Boleta Then
            Return 39
        End If
        If tipoDoc = TiposDoc.FacturaCompra Then
            Return DocumentoDTE.SiiDte.Enumerations.DTEType.n46
        End If
        If tipoDoc = TiposDoc.FacturaExportacion Then
            Return 110
        End If
        If tipoDoc = TiposDoc.NotaCreditoExportacion Then
            Return 112
        End If
        If tipoDoc = TiposDoc.NotaDebitoExportacion Then
            Return 111
        End If
        Return 0
    End Function


    Public Function CodigoReferenciaNC(wTipoDoc As String, wMotivo As String) As DocumentoDTE.SiiDte.Enumerations.Referencia_CodRef
        wMotivo = wMotivo.Trim().ToUpper

        If wTipoDoc = TiposDoc.NotaCredito Or wTipoDoc = TiposDoc.NotaDebito Then
            If wMotivo = MotivosDocumento.Anulacion Then
                Return DocumentoDTE.SiiDte.Enumerations.Referencia_CodRef.n1
            End If
            If wMotivo = MotivosDocumento.CorreccionDatos Then
                Return DocumentoDTE.SiiDte.Enumerations.Referencia_CodRef.n2
            End If
            If wMotivo = MotivosDocumento.CorreccionValores Or wMotivo = "P" Then 'P no existe 
                Return DocumentoDTE.SiiDte.Enumerations.Referencia_CodRef.n3
            End If
        End If
        Return DocumentoDTE.SiiDte.Enumerations.Referencia_CodRef.n1

    End Function

    Public Function CodigoReferenciaNCEXP(tipoDoc As String, motivo As String) As DocumentoDTE.SiiDte.Enumerations.ReferenciaB_CodRef
        motivo = motivo.Trim().ToUpper

        If tipoDoc = TiposDoc.NotaCreditoExportacion Or tipoDoc = TiposDoc.NotaDebitoExportacion Then
            If motivo = MotivosDocumento.Anulacion Then
                Return DocumentoDTE.SiiDte.Enumerations.ReferenciaB_CodRef.n1
            End If
            If motivo = MotivosDocumento.CorreccionDatos Then
                Return DocumentoDTE.SiiDte.Enumerations.ReferenciaB_CodRef.n2
            End If
            If motivo = MotivosDocumento.CorreccionValores Or motivo = "P" Then 'P no existe 
                Return DocumentoDTE.SiiDte.Enumerations.ReferenciaB_CodRef.n3
            End If
        End If
        Return DocumentoDTE.SiiDte.Enumerations.ReferenciaB_CodRef.n1

    End Function

    Public Function CodigoReferenciaGD(tipoDoc As String, motivo As String) As DocumentoDTE.SiiDte.Enumerations.IdDoc_IndTraslado
        motivo = motivo.Trim().ToUpper

        If tipoDoc = TiposDoc.GuiaDespacho Then
            If motivo.ToUpper.Trim = MotivosGuiaDespacho.Devolución Then
                Return DocumentoDTE.SiiDte.Enumerations.IdDoc_IndTraslado.n6
            End If
            If motivo.ToUpper.Trim = MotivosGuiaDespacho.Transito Then
                Return DocumentoDTE.SiiDte.Enumerations.IdDoc_IndTraslado.n5
            End If
            If motivo.ToUpper.Trim = MotivosGuiaDespacho.Mermas Then
                Return DocumentoDTE.SiiDte.Enumerations.IdDoc_IndTraslado.n6
            End If
            If motivo.ToUpper.Trim = MotivosGuiaDespacho.Venta Then
                Return DocumentoDTE.SiiDte.Enumerations.IdDoc_IndTraslado.n2
            End If
            If motivo.ToUpper.Trim = MotivosGuiaDespacho.Traspaso Then
                Return DocumentoDTE.SiiDte.Enumerations.IdDoc_IndTraslado.n5
            End If
        End If
        Return DocumentoDTE.SiiDte.Enumerations.IdDoc_IndTraslado.n1
    End Function

    Public Function CodigoDocSII(tipoDoc As String) As Decimal
        If tipoDoc = "" Then Return 0
        Dim DC As New MarketONEDataContext(P_CONEXION)
        Dim td = DC.T_TipoDoc.FirstOrDefault(Function(x) x.TipoDoc = tipoDoc)
        If td Is Nothing Then Return 0
        Return td.Cod_SII
    End Function

    Public Function RemoverCaracter(caracter As String, asc As Integer) As String
        Dim resultado = caracter
        If caracter.Trim <> "" Then
            If Strings.Asc(caracter) = asc Then
                resultado = ""
            End If
        End If
        Return resultado
    End Function

    Public Function ObtenerComuna(comuna As String) As String
        If comuna = "" Then Return ""
        Dim DC As New MarketONEDataContext(P_CONEXION)
        Dim Com = DC.Comunas.FirstOrDefault(Function(x) x.Codigo = comuna)
        If Com Is Nothing Then Return comuna
        Return Com.NombreComuna
    End Function

    Public Function BuscarComuna(comuna As String) As String
        If comuna = "" Then Return ""
        Dim DC As New MarketONEDataContext(P_CONEXION)
        Dim Com = DC.Comunas.ToList().FirstOrDefault(Function(x) TextoDTE(x.NombreComuna) = TextoDTE(comuna))
        If Com Is Nothing Then Return "00000"
        Return Com.Codigo
    End Function

    Public Function DescripcionMotivo(tipoDoc As String, motivo As String) As String
        Dim DC As New MarketONEDataContext(P_CONEXION)
        Dim mot = DC.T_Motivos.FirstOrDefault(Function(x) x.Motivo = motivo And x.TipoDoc = tipoDoc)
        If mot Is Nothing Then Return ""
        Return mot.DescMotivo.Truncate(50)
    End Function

    Public Function LeerJPG(archivo As String) As Byte()
        Try

            If Not File.Exists(archivo) Then Return Nothing
            Dim arc As New FileStream(archivo, FileMode.Open, FileAccess.Read)
            Dim Lectura As New BinaryReader(arc)
            Dim Imagen(arc.Length.ToInt()) As Byte
            Lectura.Read(Imagen, 0, arc.Length.ToInt())
            arc.Close()
            Return Imagen
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function FE_Fecha(fecha As Date) As String
        'El formato que reconoce el DTE es YYYY-MM-DD
        Return fecha.FormatoSeparador("-", FormatoFecha.YYYYMMDD)
    End Function

    Public Function TextoDTE(wTexto As String, Optional largo As Integer = 0) As String
        Dim Car As String, i As Integer, resultado As String

        TextoDTE = wTexto.ToUpper.Trim
        TextoDTE = TextoDTE.Replace("Á", "A")
        TextoDTE = TextoDTE.Replace("Ä", "A")
        TextoDTE = TextoDTE.Replace("É", "E")
        TextoDTE = TextoDTE.Replace("Í", "I")
        TextoDTE = TextoDTE.Replace("Ó", "O")
        TextoDTE = TextoDTE.Replace("Ö", "O")
        TextoDTE = TextoDTE.Replace("Ú", "U")
        TextoDTE = TextoDTE.Replace("Ü", "U")
        TextoDTE = TextoDTE.Replace("Ñ", "N")
        TextoDTE = TextoDTE.Replace("²", "2")
        TextoDTE = TextoDTE.Replace("º", "")
        TextoDTE = TextoDTE.Replace("°", "")
        TextoDTE = TextoDTE.Replace("°", "")
        TextoDTE = TextoDTE.Replace("°", "")
        TextoDTE = TextoDTE.Replace("º", "")
        TextoDTE = TextoDTE.Replace("ª", "")
        TextoDTE = TextoDTE.Replace("ª", "")
        TextoDTE = TextoDTE.Replace("%", "")
        TextoDTE = TextoDTE.Replace("$", "")
        TextoDTE = TextoDTE.Replace("&", "")
        TextoDTE = TextoDTE.Replace("*", "")
        TextoDTE = TextoDTE.Replace("^", "")
        TextoDTE = TextoDTE.Replace("~", "")
        TextoDTE = TextoDTE.Replace("#", "")
        TextoDTE = TextoDTE.Replace(Chr(34), "")
        TextoDTE = TextoDTE.Replace(Chr(39), "")
        TextoDTE = TextoDTE.Replace(Chr(96), "")
        TextoDTE = TextoDTE.Replace(Chr(176), "")
        TextoDTE = TextoDTE.Replace(Chr(186), "")

        resultado = ""
        'Sacar los caracteres no imprimibles despues de la limpieza de letras
        For i = 1 To Len(TextoDTE)
            Car = Mid(TextoDTE, i, 1)
            If Asc(Car) < 32 Or Asc(Car) > 122 Then
                'Eliminar si no es Imprimible
                Car = ""
            End If

            Car = RemoverCaracter(Car, 34)   '"
            Car = RemoverCaracter(Car, 35)   '#
            Car = RemoverCaracter(Car, 39)   ''
            Car = RemoverCaracter(Car, 58)   ':
            Car = RemoverCaracter(Car, 59)   ';
            'Car' = RemoverCaracter(Car, 64)   '@
            Car = RemoverCaracter(Car, 94)   '^
            Car = RemoverCaracter(Car, 96)   '´
            Car = RemoverCaracter(Car, 60)   '<
            Car = RemoverCaracter(Car, 61)   '=
            Car = RemoverCaracter(Car, 62)   '>

            resultado &= Car
        Next i

        TextoDTE = resultado

        If TextoDTE.Trim = "" Then
            TextoDTE = ""
        End If

        If largo > 0 Then
            TextoDTE = TextoDTE.PadRight(largo).Truncate(largo)
        End If

    End Function

    Function GenerarXML(local As Decimal, tipoDoc As String, numDoc As Decimal) As Boolean
        Dim ambiente As DTEBoxCliente.Ambiente = DTEBoxCliente.Ambiente.Produccion
        Dim esParaDistribucion As Boolean = True

        Dim ServicioDTE = New DTEBoxCliente.Servicio(FE_DTE, FE_Llave)
        Dim ResultadoDTE As DTEBoxCliente.ResultadoRecuperarXml = ServicioDTE.RecuperarXml(ambiente, GrupoBusqueda.Emitidos, FE_Rut_Emisor, CodigoDTEConsulta(tipoDoc), Convert.ToInt64(numDoc), esParaDistribucion)

        'Procesar resultado
        GenerarXML = False
        If (ResultadoDTE.ResultadoServicio.Estado = DTEBoxCliente.EstadoResultado.Ok) Then
            Try
                Dim ArchivoXML = RutaDocumentoElectronico(TipoArchivo.XML, local, tipoDoc, numDoc, 0, 0)
                System.IO.File.WriteAllText(ArchivoXML, ResultadoDTE.Datos)
                GenerarXML = True
            Catch ex As Exception
                MsgError("Error al grabar XML: " & ex.Message)
            End Try
        Else
            MsgError("Error al Generar XML: " & ResultadoDTE.ResultadoServicio.Descripcion)
        End If
    End Function

    Function ValidarFechaCAF() As Boolean
        Return True
    End Function

    Public Class DocRef
        Public Property Fecha As DateTime
        Public Property TipoDoc As String
        Public Property NumDoc As String
        Public Property CodRef As String
        Public Property Observacion As String
    End Class

    Public Class ColoresEstadosDTE
        Public Shared ErrorEnvio As Color = Color.FromArgb(255, 180, 106)
        Public Shared Rechazado As Color = Color.FromArgb(255, 220, 69)
        Public Shared RechazadoTxt As Color = Color.FromArgb(180, 16, 61)
        Public Shared Conflicto As Color = Color.FromArgb(234, 118, 119)
        Public Shared OtrosRechazado As Color = Color.FromArgb(255, 74, 213)
        Public Shared RespuestaPendiente As Color = Color.FromArgb(48, 108, 242)
        Public Shared TimbreErroneo As Color = Color.FromArgb(255, 220, 69)
        Public Shared Aprobado As Color = Color.FromArgb(174, 255, 103)
        Public Shared EnvioCorrecto As Color = Color.FromArgb(255, 225, 184)
        Public Shared EnvioPendiente As Color = Color.FromArgb(223, 173, 255)
    End Class

    Public Function ObtenerBodegaPrincipal(local As Decimal) As Decimal
        Dim DC = New MarketONEDataContext(P_CONEXION)
        Dim bod = DC.Bodegas.Where(Function(x) x.Local = local AndAlso x.Principal) _
                            .OrderBy(Function(x) x.Bodega) _
                            .FirstOrDefault()
        If bod Is Nothing Then Return 0
        Return bod.Bodega
    End Function
End Module
