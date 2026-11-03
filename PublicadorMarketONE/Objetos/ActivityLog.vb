Imports System.IO

''' <summary>Registro de actividad y errores en archivos de texto (carpeta Logs junto al programa).</summary>
Module ActivityLog

    Public ReadOnly Property BASE_LOG_PATH As String
        Get
            Return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")
        End Get
    End Property

    Sub LogException(ByVal excepcion As String, ByVal procedimiento As String)
        Dim registro = $"{FechaLog(DateTime.Now)} {procedimiento} {G_USUARIOACTUAL}{vbCrLf} {excepcion}{vbCrLf}"
        SaveContent(Path.Combine(BASE_LOG_PATH, $"LogEx_{FechaArchivoLog()}.txt"), registro)
    End Sub

    Sub LogError(msgerror As String, procedimiento As String)
        Dim registro = $"{FechaLog(DateTime.Now)} {procedimiento} {G_USUARIOACTUAL} {Environment.UserName}{vbCrLf} {msgerror}{vbCrLf}"
        SaveContent(Path.Combine(BASE_LOG_PATH, "ErrorMsg", $"Log_{FechaArchivoLog()}.txt"), registro)
    End Sub

    Sub Log(registro As String, procedimiento As String, Optional Directorio As String = "")
        registro = FechaLog(DateTime.Now) & vbTab & G_USUARIOACTUAL & vbTab & registro
        Dim carpeta = If(Directorio <> "", Path.Combine(BASE_LOG_PATH, Directorio), BASE_LOG_PATH)
        SaveContent(Path.Combine(carpeta, $"Log_{procedimiento}_{FechaArchivoLog()}.txt"), registro)
    End Sub

    Sub SaveContent(ByVal Destino As String, ByVal registro As String, ByVal Optional sobreescribir As Boolean = False)
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(Destino))
            If sobreescribir Then
                File.WriteAllText(Destino, registro & vbCrLf)
            Else
                File.AppendAllText(Destino, registro & vbCrLf)
            End If
        Catch
            'El registro de actividad nunca debe detener el sistema.
        End Try
    End Sub

    Function FechaLog(ByVal wFecha As DateTime) As String
        Return wFecha.ToString("yyyy-MM-dd HH:mm:ss")
    End Function

    Public Function FechaArchivoLog() As String
        Return DateTime.Now.ToString("yyyyMMdd")
    End Function
End Module
