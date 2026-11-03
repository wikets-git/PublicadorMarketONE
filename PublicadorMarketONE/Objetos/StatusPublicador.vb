Imports System.IO
Imports System.Text

''' <summary>
''' Archivo status.json junto al ejecutable, que lee otro programa para avisar por correo si el Publicador tiene
''' errores o dejó de publicar. Se reescribe al terminar cada ciclo (automático o con el botón Ejecutar):
''' { "Empresa": "...", "Publicacion": "aaaa-mm-dd hh:mm:ss", "Status": "Normal" | "Error" }
''' </summary>
Public Module StatusPublicador

    Public Const STATUS_NORMAL As String = "Normal"
    Public Const STATUS_ERROR As String = "Error"

    Public ReadOnly Property RutaStatus As String
        Get
            Return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "status.json")
        End Get
    End Property

    ''' <summary>
    ''' Graba la fecha y hora de la publicación y su estado. Se escribe primero un archivo temporal y luego se
    ''' reemplaza status.json, para que el otro programa nunca lea un archivo a medio escribir.
    ''' </summary>
    Public Sub GrabarStatus(conError As Boolean, Optional fecha As Date = Nothing)
        If fecha = Nothing Then fecha = Now
        'Empresa de la configuración, con las comillas y caracteres especiales escapados para JSON
        Dim empresa = Newtonsoft.Json.JsonConvert.ToString(If(CONFIGURACION?.Empresa, "").Trim)
        Dim json = "{" & vbCrLf &
                   $"  ""Empresa"": {empresa}," & vbCrLf &
                   $"  ""Publicacion"": ""{fecha:yyyy-MM-dd HH:mm:ss}""," & vbCrLf &
                   $"  ""Status"": ""{If(conError, STATUS_ERROR, STATUS_NORMAL)}""" & vbCrLf &
                   "}" & vbCrLf
        Dim destino = RutaStatus
        Dim temporal = destino & ".tmp"
        Try
            File.WriteAllText(temporal, json, New UTF8Encoding(False))
            If File.Exists(destino) Then
                File.Replace(temporal, destino, Nothing)
            Else
                File.Move(temporal, destino)
            End If
        Catch ex As Exception
            'Si el otro programa lo tiene abierto en ese momento, se intenta escribir directo
            Try
                File.WriteAllText(destino, json, New UTF8Encoding(False))
                If File.Exists(temporal) Then File.Delete(temporal)
            Catch ex2 As Exception
                ActivityLog.LogException($"No se pudo grabar {destino}: {ex2.Message}", "GrabarStatus")
            End Try
        End Try
    End Sub
End Module
