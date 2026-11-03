Imports System.IO
Imports Newtonsoft.Json

''' <summary>
''' Configuración del sistema, en el archivo PublicadorMarketONE_Settings.json junto al ejecutable (formato simple del
''' Publicador Supermercado: conexión directa a la base, sin ServerConfig ni ServidoresEmpresas de MarketONE).
''' </summary>
Public Class Settings
    Private Const NombreArchivo As String = "PublicadorMarketONE_Settings"

    ''' <summary>Nombre de la empresa (se muestra en el acceso y en los títulos).</summary>
    Public Property Empresa As String = ""
    ''' <summary>Servidor SQL Server (ej.: 192.168.1.10,1433).</summary>
    Public Property Servidor As String = ""
    ''' <summary>Base de datos de MarketONE de la empresa (ej.: MarketONE_Empresa).</summary>
    Public Property Base As String = ""
    Public Property UsuarioBD As String = ""
    Public Property ClaveBD As String = ""
    ''' <summary>Segundos de espera para conectar.</summary>
    Public Property TiempoConexion As Integer = 5
    ''' <summary>Carpeta base donde se crean las subcarpetas TED, CAF, PDF y TXT. Vacío = carpeta del programa.</summary>
    Public Property RutaArchivos As String = ""
    ''' <summary>Minutos entre ciclos del envío automático en Publicador DTE.</summary>
    Public Property MinutosEnvioAutomatico As Integer = 5
    ''' <summary>Documentos por lote en el envío automático. 0 = todos de una vez.</summary>
    Public Property RegistrosPorCiclo As Integer = 0
    ''' <summary>Local con que trabaja el Publicador (usuario SYS). 0 = el primer local con facturación electrónica.</summary>
    Public Property LocalPublicador As Decimal = 0
    <JsonIgnore>
    Public ReadOnly Property CadenaConexion As String
        Get
            Dim cs = $"Data Source={Servidor};Initial Catalog={Base};Connect Timeout={If(TiempoConexion > 0, TiempoConexion, 5)};"
            If UsuarioBD.Trim = "" Then
                cs &= "Integrated Security=True;"
            Else
                cs &= $"User ID={UsuarioBD};Password={ClaveBD};"
            End If
            Return cs
        End Get
    End Property

    Public Function Check() As String
        If Servidor.Trim = "" Then Return "Falta el servidor de base de datos (Servidor)."
        If Base.Trim = "" Then Return "Falta el nombre de la base de datos (Base)."
        Return ""
    End Function

    Public Shared ReadOnly Property RutaArchivoConfiguracion As String
        Get
            Return Path.Combine(Path.GetDirectoryName(Reflection.Assembly.GetEntryAssembly().Location), $"{NombreArchivo}.json")
        End Get
    End Property

    ''' <summary>Carga la configuración. Devuelve "" si está correcta o el mensaje de error.</summary>
    Public Shared Function GetSettings() As String
        Try
            If Not File.Exists(RutaArchivoConfiguracion) Then
                CONFIGURACION = New Settings()
                SaveConfig(CONFIGURACION)
                Return $"No se ha configurado el sistema.{vbCrLf}Se creó el archivo:{vbCrLf}{RutaArchivoConfiguracion}"
            End If

            CONFIGURACION = JsonConvert.DeserializeObject(Of Settings)(File.ReadAllText(RutaArchivoConfiguracion),
                New JsonSerializerSettings With {.ObjectCreationHandling = ObjectCreationHandling.Replace})
            If CONFIGURACION Is Nothing Then Throw New Exception("El archivo de configuración está vacío.")

            Return CONFIGURACION.Check()
        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    Public Shared Function SaveConfig(obj As Settings) As String
        Try
            File.WriteAllText(RutaArchivoConfiguracion, JsonConvert.SerializeObject(obj, Formatting.Indented))
            Return RutaArchivoConfiguracion
        Catch
            Return ""
        End Try
    End Function
End Class
