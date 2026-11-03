Imports System.ComponentModel
Imports System.Data.SqlClient
Imports System.Runtime.CompilerServices

Public Module Funciones

    Public Const P_SISTEMA As String = "Publicador MarketONE"
    Public Const VERSION As String = "1.01"

    Public Const CHECK As Boolean = True
    Public Const UNCHECK As Boolean = False
    Public Const FECHA_NULA As String = "01/01/2000"

    Public A_INGRESADO As String = "Ingresado"

#Region "Variables globales"
    Public CONFIGURACION As Settings
    Public G_SUPERVISOR As Boolean
    Public G_USUARIOACTUAL As String = ""
    Public G_NOMBREUSUARIOACTUAL As String = ""
    Public G_ACCESOUSUARIO As Decimal
    Public G_LOCALACTUAL As Decimal
    Public G_NOMBRELOCALACTUAL As String = ""
    Public G_IVA As Decimal = 19
    ''' <summary>ID de cliente de la empresa (Parametros.Clave), igual que G_IDCLIENTE de MarketONE.</summary>
    Public G_IDCLIENTE As String = ""
    Public G_POS As Decimal = 0
    Public G_DOLAR As Decimal = 0

    'De MarketONE (Funciones.vb): RUT de exportación y máximo para evitar notación científica
    Public Property P_RUTSINCLIENTE As String = "66.666.666-6"
    Public Property P_RUTEXPORTACION As String = "55.555.555-5"
    Public MaxDouble As Double = 999999999999999

    'Carpetas de documentos electrónicos (las crea DTE.Parametros_DTE dentro de P_DIRECTORIO)
    Public Directorio_CAF As String = ""
    Public Directorio_TED As String = ""
    Public Directorio_PDF As String = ""
    Public Directorio_TXT As String = ""

    Public C_BLANCO As Color = Color.White

    ''' <summary>Cadena de conexión a la base MarketONE.</summary>
    Public ReadOnly Property P_CONEXION As String
        Get
            Return CONFIGURACION.CadenaConexion
        End Get
    End Property

    ''' <summary>
    ''' Carpeta base donde se crean las subcarpetas TED, CAF, PDF y TXT. Por defecto es la carpeta del programa.
    ''' RutaArchivos de la configuración se usa solo si es válida: relativa (se combina con la carpeta del programa)
    ''' o absoluta en una unidad que exista en este equipo; si no, se usa la carpeta del programa.
    ''' </summary>
    Public ReadOnly Property P_DIRECTORIO As String
        Get
            Dim programa = My.Application.Info.DirectoryPath
            Dim ruta = If(CONFIGURACION?.RutaArchivos, "").Trim
            If ruta = "" Then Return programa
            Try
                If Not System.IO.Path.IsPathRooted(ruta) Then Return System.IO.Path.Combine(programa, ruta).TrimEnd("\"c)
                Dim raiz = System.IO.Path.GetPathRoot(ruta)
                If raiz <> "" AndAlso System.IO.Directory.Exists(raiz) Then Return ruta.TrimEnd("\"c)
            Catch
            End Try
            Return programa
        End Get
    End Property

#End Region

#Region "Extensiones"
    ''' <summary>Quita tildes y caracteres especiales (de MarketONE, Controles\TextBoxSugerencia.vb).</summary>
    <Extension()>
    Public Function RemoverTildes(wTexto As String) As String
        Return Validacion.Clean(wTexto)
    End Function

    <Extension()>
    Public Sub Load(Source As BindingSource, Lista As Object)
        Source.DataSource = Nothing
        Source.DataSource = Lista
    End Sub
#End Region

#Region "Conexión"
    ''' <summary>Prueba la conexión a la base de datos.</summary>
    Public Function ProbarConexion(ByRef mensaje As String) As Boolean
        Try
            Using wCon As New SqlConnection(P_CONEXION)
                wCon.Open()
            End Using
            Return True
        Catch ex As Exception
            mensaje = ex.Message
            Return False
        End Try
    End Function
#End Region

#Region "Mensajes"
    ''' <summary>
    ''' El Publicador trabaja solo (envío automático): los errores y avisos nunca abren ventanas (nadie las cerraría y
    ''' el ciclo quedaría detenido). Se graban en el log y se muestran en la barra de operación.
    ''' </summary>
    Public AvisoDesatendido As Action(Of String, Boolean)

    Private Sub Avisar(texto As String, esError As Boolean)
        Try
            AvisoDesatendido?.Invoke(texto.Replace(vbCrLf, " ").Replace(vbLf, " "), esError)
        Catch
        End Try
    End Sub

    ''' <summary>Error (MsgError de MarketONE): queda en el log (Logs\ErrorMsg) y en la barra de operación. No abre ventanas.</summary>
    Public Sub MsgError(wError As String, Optional wTitulo As String = "Error")
        Dim lineas = wError.Split(New String() {Environment.NewLine}, StringSplitOptions.None)
        If lineas.Count > 20 Then lineas = lineas.Take(20).ToArray()
        wError = String.Join(vbCrLf, lineas)
        If wError.Length > 1000 Then wError = wError.Substring(0, 1000)
        ActivityLog.LogError(wError, wTitulo)
        Avisar(wError, True)
    End Sub

    ''' <summary>Aviso (Mensaje de MarketONE): queda en el log y en la barra de operación. No abre ventanas.</summary>
    Public Sub Mensaje(ByVal wMensaje As String, Optional wTitulo As String = "Mensaje")
        ActivityLog.Log(wMensaje.Replace(vbCrLf, " "), "Mensajes")
        Avisar(wMensaje, False)
    End Sub

    ''' <summary>Información (Informacion de MarketONE): queda en el log y en la barra de operación. No abre ventanas.</summary>
    Public Sub Informacion(ByVal wMensaje As String, Optional wTitulo As String = "Información")
        Mensaje(wMensaje, wTitulo)
    End Sub

    ''' <summary>Única ventana del Publicador: la confirmación al cerrarlo, que pide el propio usuario.</summary>
    Public Function Pregunta(ByVal wMensaje As String, Optional wTitulo As String = "Pregunta") As Boolean
        Return Mensajes.Mostrar(wTitulo, wMensaje, Mensajes.Tipo.Confirmacion)
    End Function
#End Region

#Region "Auditoría (tabla Auditoria de MarketONE)"
    ''' <summary>
    ''' Graba un registro en Auditoria con la estructura de MarketONE (igual que IngresarAuditoria):
    ''' Lugar, Fecha, Hora, Local, Usuario, Proceso, Evento, Dato, Valor, Cantidad, POS, Cuenta.
    ''' </summary>
    Public Sub IngresarAuditoria(Optional proceso As String = "", Optional evento As String = "",
                                 Optional dato As String = "", Optional valor As String = "0",
                                 Optional cantidad As Decimal = 0D)
        Try
            Dim DC = New MarketONEDataContext(P_CONEXION)
            Dim aud = New Auditoria With {
                .Lugar = "PC",
                .Fecha = Now,
                .Hora = Format(Now, "HH:mm:ss"),
                .Local = G_LOCALACTUAL,
                .Usuario = If(G_USUARIOACTUAL <> "", G_USUARIOACTUAL, USUARIO_PUBLICADOR),
                .Proceso = Corta(proceso, 100),
                .Evento = Corta(evento, 100),
                .Dato = Corta(dato, 50),
                .Valor = Corta(valor, 50),
                .Cantidad = cantidad,
                .POS = G_POS,
                .Cuenta = Corta(Environment.UserName, 100)
            }
            DC.Auditoria.InsertOnSubmit(aud)
            DC.SubmitChanges()
        Catch ex As Exception
            ActivityLog.LogException(ex.Message, "Auditoria")
        End Try
    End Sub

    Private Function Corta(texto As String, largo As Integer) As String
        texto = If(texto, "").Trim
        If texto.Length > largo Then texto = texto.Substring(0, largo)
        Return texto
    End Function
#End Region

#Region "Fechas"
    Public Enum ModoFecha
        Inicial = 1
        Final = 2
    End Enum

    Public Function FechaFiltro(ByVal modo As ModoFecha, ByVal fecha As DateTime, Optional ByVal mesesAdicionales As Integer = 0) As DateTime
        Dim resultado As DateTime
        Select Case modo
            Case ModoFecha.Inicial
                resultado = FechaDesde(New DateTime(fecha.Year, fecha.Month, 1))
            Case ModoFecha.Final
                resultado = FechaHasta(New DateTime(fecha.Year, fecha.Month, 1).AddMonths(1).AddDays(-1))
            Case Else
                resultado = Now
        End Select
        If mesesAdicionales <> 0 Then resultado = resultado.AddMonths(mesesAdicionales)
        Return resultado
    End Function

    Function FechaHasta(ByVal wFecha As DateTime) As DateTime
        Return New DateTime(wFecha.Year, wFecha.Month, wFecha.Day, 23, 59, 59)
    End Function

    Function FechaDesde(ByVal wFecha As DateTime) As DateTime
        Return New DateTime(wFecha.Year, wFecha.Month, wFecha.Day, 0, 0, 0)
    End Function
#End Region

#Region "Varios"
    Public Sub DoEvents()
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Primera línea del detalle del documento, para el timbre (TED). Copiada de MarketONE (Funciones.PrimeraLinea).
    ''' </summary>
    Public Function PrimeraLinea(local As Double, tipoDoc As String, numero As Double) As String
        Dim DC As New MarketONEDataContext(P_CONEXION)
        Dim resultado As String = "ARTICULO"
        Dim det = DC.DocumentosD.FirstOrDefault(Function(x) x.Local = local And x.TipoDoc = tipoDoc And x.Numero = numero)

        If det IsNot Nothing Then
            If det.Glosa.Trim <> "" Then
                resultado = TextoDTE(det.Glosa)
            Else
                Dim art = DC.Articulos.FirstOrDefault(Function(x) x.Articulo = det.Articulo)
                If art IsNot Nothing Then
                    resultado = TextoDTE(art.Descripcion).Trim
                End If
            End If
        End If
        Return resultado
    End Function
#End Region

End Module
