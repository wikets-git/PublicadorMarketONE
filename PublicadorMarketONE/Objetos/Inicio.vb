''' <summary>
''' Inicio del Publicador sin pantalla de acceso: carga la configuración, prueba la conexión, lee Parametros
''' (IVA y Clave, igual que MarketONE) y deja como usuario conectado a "SYS".
''' </summary>
Public Module Inicio

    Public Const USUARIO_PUBLICADOR As String = "SYS"

    ''' <summary>Indica si el sistema ya quedó listo (configuración, base y usuario).</summary>
    Public SistemaIniciado As Boolean = False

    ''' <summary>Deja el sistema listo para enviar. Devuelve "" si todo está bien o el motivo del error.</summary>
    Public Function IniciarSistema() As String
        SistemaIniciado = False

        Dim resultado = Settings.GetSettings()
        If resultado <> "" Then Return $"Configuración: {resultado}"

        Dim mensaje = ""
        If Not ProbarConexion(mensaje) Then
            Return $"No se pudo conectar con la base de datos {CONFIGURACION.Servidor} : {CONFIGURACION.Base}. {mensaje}"
        End If

        Dim DC = New MarketONEDataContext(P_CONEXION)

        'Parámetros del sistema (MarketONE: ObtenerParametros)
        Dim par = DC.Parametros.FirstOrDefault()
        If par Is Nothing Then Return "Error al leer la tabla Parametros."
        If par.Iva > 0 Then G_IVA = CDec(par.Iva)
        G_IDCLIENTE = If(par.Clave, "").Trim
        G_POS = 0

        'Local de trabajo: el configurado (LocalPublicador) o el primero con facturación electrónica
        Dim locales = DC.Locales.OrderBy(Function(x) x.Local).ToList()
        If Not locales.Any Then Return "No hay locales en la tabla Locales."
        Dim loc = locales.FirstOrDefault(Function(x) x.Local = CONFIGURACION.LocalPublicador)
        If loc Is Nothing Then loc = locales.FirstOrDefault(Function(x) x.FElectronica)
        If loc Is Nothing Then loc = locales.First

        G_USUARIOACTUAL = USUARIO_PUBLICADOR
        G_NOMBREUSUARIOACTUAL = "Publicador"
        G_ACCESOUSUARIO = 0
        G_SUPERVISOR = False
        G_LOCALACTUAL = loc.Local
        G_NOMBRELOCALACTUAL = loc.NombreLocal.Trim

        If Not DTE.Parametros_DTE(G_LOCALACTUAL) Then Return $"Error al leer los parámetros de facturación electrónica del local {G_LOCALACTUAL}."
        IngresarAuditoria("Login", "Ingreso al Sistema", P_SISTEMA, VERSION)

        SistemaIniciado = True
        Return ""
    End Function
End Module
