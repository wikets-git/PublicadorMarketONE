Imports System.Security.Cryptography
Imports System.Threading

Namespace My
    ''' <summary>Eventos de la aplicación.</summary>
    Partial Friend Class MyApplication

        ''' <summary>Mientras el programa corre mantiene tomado este mutex (uno por base de datos).</summary>
        Private Shared MutexInstancia As Mutex
        ''' <summary>Evento con que otra copia le pide a la que corre que muestre su ventana.</summary>
        Friend Shared EventoMostrar As EventWaitHandle

        ''' <summary>
        ''' Una sola instancia por base de datos (Servidor + Base de la configuración) en todo el equipo, aunque se abra
        ''' desde otra carpeta o con otro usuario de Windows. Así pueden convivir publicadores de distintas empresas
        ''' (cada uno en su carpeta, con su configuración), pero nunca dos enviando la misma base.
        ''' Si ya hay uno corriendo para esa base, se muestra su ventana y este se cierra.
        ''' </summary>
        Private Sub MyApplication_Startup(sender As Object, e As ApplicationServices.StartupEventArgs) Handles Me.Startup
            Dim nombre = "Global\PublicadorMarketONE_" & ClaveInstancia()
            Dim nueva As Boolean
            Try
                MutexInstancia = New Mutex(True, nombre, nueva)
            Catch ex As UnauthorizedAccessException
                nueva = False   'lo tomó otro usuario de Windows
            End Try

            If Not nueva Then
                Try
                    EventWaitHandle.OpenExisting(nombre & "_Mostrar").Set()
                Catch
                End Try
                e.Cancel = True
                Return
            End If

            Try
                EventoMostrar = New EventWaitHandle(False, EventResetMode.AutoReset, nombre & "_Mostrar")
            Catch
                EventoMostrar = Nothing
            End Try
        End Sub

        Private Sub MyApplication_Shutdown(sender As Object, e As EventArgs) Handles Me.Shutdown
            Try
                MutexInstancia?.ReleaseMutex()
                MutexInstancia?.Dispose()
            Catch
            End Try
        End Sub

        ''' <summary>
        ''' Identifica la base de datos de esta copia (Servidor|Base del archivo de configuración).
        ''' Si aún no hay configuración válida, se usa la carpeta del programa.
        ''' </summary>
        Private Shared Function ClaveInstancia() As String
            Dim texto As String
            If Global.PublicadorMarketONE.Settings.GetSettings() = "" AndAlso CONFIGURACION IsNot Nothing Then
                texto = $"{CONFIGURACION.Servidor.Trim}|{CONFIGURACION.Base.Trim}".ToUpperInvariant()
            Else
                texto = System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd("\"c).ToUpperInvariant()
            End If
            Using sha = SHA1.Create()
                Return BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(texto))).Replace("-", "")
            End Using
        End Function

    End Class
End Namespace
