''' <summary>
''' Ventana "Acerca de": datos del sistema y de propiedad intelectual de Wikets Software Ltda.
''' Se abre con doble clic en el logo de Wikets del menú principal. Se arma por código (sin diseñador).
''' </summary>
Public Class AcercaDe
    Inherits Form

    Private Const EMPRESA_DESARROLLO As String = "Wikets Software Ltda."
    Private Const GIRO_DESARROLLO As String = "Desarrollo de Sistemas Informáticos"
    Private Const RUT_DESARROLLO As String = "76.235.899-9"
    Private Const CONTACTO As String = "Pedro L. González G."
    Private Const TELEFONO As String = "+569 98842333"
    Private Const CORREO As String = "contacto@wikets.cl"
    Private Const WEB As String = "www.wikets.cl"

    Private ReadOnly AzulTitulo As Color = Color.FromArgb(29, 63, 120)
    Private ReadOnly GrisTexto As Color = Color.FromArgb(64, 64, 64)

    Public Sub New()
        Text = "Acerca de " & P_SISTEMA
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        BackColor = Color.White
        Font = New Font("Arial", 9.75!)
        ClientSize = New Size(460, 470)
        Try
            Icon = PublicadorDTE.Icon
        Catch
        End Try

        Dim y = 16

        Dim logo As New PictureBox With {
            .Image = My.Resources.Resources.Wikets,
            .SizeMode = PictureBoxSizeMode.Zoom,
            .Location = New Point(130, y),
            .Size = New Size(200, 96)}
        Controls.Add(logo)
        y += 106

        y = Titulo($"{P_SISTEMA}  v.{VERSION}", y, 13.0!)
        If If(CONFIGURACION?.Empresa, "").Trim <> "" Then
            y = Linea($"Licenciado a: {CONFIGURACION.Empresa.Trim}", y)
        End If
        y = Separador(y + 6)

        y = Titulo(EMPRESA_DESARROLLO, y, 11.25!)
        y = Linea(GIRO_DESARROLLO, y)
        y = Linea($"RUT: {RUT_DESARROLLO}", y)
        y += 8
        y = Linea($"Contacto: {CONTACTO}", y)
        y = Linea($"Teléfono: {TELEFONO}", y)
        y = Enlace("Correo: ", CORREO, "mailto:" & CORREO, y)
        y = Enlace("Página web: ", WEB, "https://" & WEB, y)
        y = Separador(y + 6)

        Dim aviso As New Label With {
            .Text = $"© {Math.Max(2026, Now.Year)} {EMPRESA_DESARROLLO} Todos los derechos reservados." & vbCrLf &
                    "Este programa y su código fuente son propiedad intelectual de " & EMPRESA_DESARROLLO &
                    ", protegidos por la Ley N° 17.336 sobre Propiedad Intelectual. Se prohíbe su copia, " &
                    "distribución o modificación sin autorización escrita.",
            .ForeColor = GrisTexto,
            .Font = New Font("Arial", 8.25!),
            .Location = New Point(20, y),
            .Size = New Size(420, 64),
            .TextAlign = ContentAlignment.TopCenter}
        Controls.Add(aviso)
        y += 70

        Dim bAceptar As New Button With {
            .Text = "Aceptar",
            .DialogResult = DialogResult.OK,
            .Size = New Size(110, 32),
            .Location = New Point((ClientSize.Width - 110) \ 2, y),
            .FlatStyle = FlatStyle.Flat,
            .BackColor = AzulTitulo,
            .ForeColor = Color.White,
            .Font = New Font("Arial", 9.75!, FontStyle.Bold)}
        Controls.Add(bAceptar)
        AcceptButton = bAceptar
        CancelButton = bAceptar
        ClientSize = New Size(460, y + 48)
    End Sub

    Private Function Titulo(texto As String, y As Integer, tamaño As Single) As Integer
        Dim l As New Label With {
            .Text = texto, .ForeColor = AzulTitulo, .Font = New Font("Arial", tamaño, FontStyle.Bold),
            .AutoSize = False, .TextAlign = ContentAlignment.MiddleCenter,
            .Location = New Point(10, y), .Size = New Size(440, CInt(tamaño * 2.2))}
        Controls.Add(l)
        Return y + l.Height + 2
    End Function

    Private Function Linea(texto As String, y As Integer) As Integer
        Dim l As New Label With {
            .Text = texto, .ForeColor = GrisTexto, .AutoSize = False, .TextAlign = ContentAlignment.MiddleCenter,
            .Location = New Point(10, y), .Size = New Size(440, 22)}
        Controls.Add(l)
        Return y + 22
    End Function

    Private Function Enlace(etiqueta As String, texto As String, url As String, y As Integer) As Integer
        'Etiqueta (Label) y enlace (LinkLabel) en controles separados: el LinkLabel con LinkArea parcial
        'dibuja el enlace desplazado y corta la última letra ("contacto@wikets.c").
        Dim lEtiqueta As New Label With {
            .Text = etiqueta.TrimEnd(), .ForeColor = GrisTexto, .AutoSize = True, .Padding = New Padding(0, 3, 0, 3)}
        Dim lEnlace As New LinkLabel With {
            .Text = texto, .LinkColor = AzulTitulo, .ActiveLinkColor = AzulTitulo, .VisitedLinkColor = AzulTitulo,
            .AutoSize = True, .Padding = New Padding(0, 3, 4, 3)}
        AddHandler lEnlace.LinkClicked, Sub(s, e)
                                            Try
                                                Process.Start(url)
                                            Catch ex As Exception
                                                MsgError($"No se pudo abrir {texto}: {ex.Message}")
                                            End Try
                                        End Sub
        Controls.Add(lEtiqueta)
        Controls.Add(lEnlace)
        Dim separacion = 4
        Dim total = lEtiqueta.PreferredSize.Width + separacion + lEnlace.PreferredSize.Width
        Dim x = Math.Max(0, (ClientSize.Width - total) \ 2)
        lEtiqueta.Location = New Point(x, y)
        lEnlace.Location = New Point(x + lEtiqueta.PreferredSize.Width + separacion, y)
        Return y + 22
    End Function

    Private Function Separador(y As Integer) As Integer
        Controls.Add(New Label With {
            .BorderStyle = BorderStyle.Fixed3D, .AutoSize = False,
            .Location = New Point(40, y), .Size = New Size(380, 2)})
        Return y + 12
    End Function

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        '
        'AcercaDe
        '
        Me.ClientSize = New System.Drawing.Size(284, 261)
        Me.Name = "AcercaDe"
        Me.ResumeLayout(False)

    End Sub

    Private Sub AcercaDe_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class
