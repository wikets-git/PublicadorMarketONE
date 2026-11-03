#If Not VERIFICACION Then
Imports System.Windows.Forms.Integration
Imports LibMsg
#End If

''' <summary>
''' Ventanas de mensaje. Usa la librería Msg.dll de MarketONE (WPF).
''' La constante VERIFICACION solo se usa para compilar fuera de Windows y muestra MessageBox.
''' </summary>
Public Module Mensajes

    Public Enum Tipo
        [Error]
        Advertencia
        Ok
        Info
        Confirmacion
        AdvertenciaConfirmacion
    End Enum

    Public Function Mostrar(titulo As String, texto As String, tipoMsg As Tipo) As Boolean
#If Not VERIFICACION Then
        Dim wMsg = New Msg()
        Dim t As TipoMensaje
        Select Case tipoMsg
            Case Tipo.Error : t = TipoMensaje.Error
            Case Tipo.Advertencia : t = TipoMensaje.Advertencia
            Case Tipo.Ok : t = TipoMensaje.Ok
            Case Tipo.Info : t = TipoMensaje.Info
            Case Tipo.Confirmacion : t = TipoMensaje.Confirmacion
            Case Else : t = TipoMensaje.AdvertenciaConfirmacion
        End Select
        wMsg.Mostrar(titulo, texto, t, Ubicacion.Centrado)
        ElementHost.EnableModelessKeyboardInterop(wMsg)
        wMsg.ShowDialog()
        Return wMsg.DialogResult.HasValue AndAlso wMsg.DialogResult.Value
#Else
        Dim icono = MessageBoxIcon.Information
        Dim botones = MessageBoxButtons.OK
        Select Case tipoMsg
            Case Tipo.Error : icono = MessageBoxIcon.Error
            Case Tipo.Advertencia : icono = MessageBoxIcon.Warning
            Case Tipo.Confirmacion : icono = MessageBoxIcon.Question : botones = MessageBoxButtons.YesNo
            Case Tipo.AdvertenciaConfirmacion : icono = MessageBoxIcon.Warning : botones = MessageBoxButtons.YesNo
        End Select
        Dim r = MessageBox.Show(texto, titulo, botones, icono)
        Return r = DialogResult.OK OrElse r = DialogResult.Yes
#End If
    End Function
End Module
