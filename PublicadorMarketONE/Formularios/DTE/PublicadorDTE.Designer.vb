<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PublicadorDTE
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Requerido por el Diseñador de Windows Forms
    Private components As System.ComponentModel.IContainer

    'NOTA: el Diseñador de Windows Forms necesita el siguiente procedimiento
    'Se puede modificar usando el Diseñador de Windows Forms.  
    'No lo modifique con el editor de código.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(PublicadorDTE))
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle4 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.xRegistros = New System.Windows.Forms.TextBox()
        Me.lLote = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.lResumen = New System.Windows.Forms.Label()
        Me.sEstado = New System.Windows.Forms.StatusStrip()
        Me.lStatus = New System.Windows.Forms.ToolStripStatusLabel()
        Me.tProgresoBarra = New System.Windows.Forms.ToolStripProgressBar()
        Me.lRegistros = New System.Windows.Forms.ToolStripStatusLabel()
        Me.gInformacion = New System.Windows.Forms.GroupBox()
        Me.bEjecutar = New System.Windows.Forms.Button()
        Me.Salir = New System.Windows.Forms.Button()
        Me.gStatus = New System.Windows.Forms.GroupBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.xCantDoc = New System.Windows.Forms.TextBox()
        Me.Label15 = New System.Windows.Forms.Label()
        Me.Label14 = New System.Windows.Forms.Label()
        Me.xSegundos = New System.Windows.Forms.TextBox()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.xTiempo = New System.Windows.Forms.TextBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.xOperacion = New System.Windows.Forms.TextBox()
        Me.Timer = New System.Windows.Forms.Timer(Me.components)
        Me.Segundos = New System.Windows.Forms.Timer(Me.components)
        Me.xTabla = New System.Windows.Forms.DataGridView()
        Me.TipoDocDescripcion = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.POS = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.WinDeco1 = New WinDeco.WinDeco()
        Me.xLogo = New System.Windows.Forms.PictureBox()
        Me.SeleccionDataGridViewCheckBoxColumn = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.EstadoDataGridViewTextBoxColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.NumDocDataGridViewTextBoxColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.FechaDataGridViewTextBoxColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.UsuarioDataGridViewTextBoxColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.NombreLocalDataGridViewTextBoxColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ClienteDataGridViewTextBoxColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.RutClienteDataGridViewTextBoxColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.NombreClienteDataGridViewTextBoxColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.FPagoDescripcionDataGridViewTextBoxColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.MontoDocumentoDataGridViewTextBoxColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ObservacionDataGridViewTextBoxColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.sDocumentos = New System.Windows.Forms.BindingSource(Me.components)
        Me.sEstado.SuspendLayout()
        Me.gInformacion.SuspendLayout()
        Me.gStatus.SuspendLayout()
        CType(Me.xTabla, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.xLogo, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.sDocumentos, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'xRegistros
        '
        Me.xRegistros.BackColor = System.Drawing.Color.FromArgb(CType(CType(149, Byte), Integer), CType(CType(230, Byte), Integer), CType(CType(168, Byte), Integer))
        Me.xRegistros.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.xRegistros.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xRegistros.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.xRegistros.Location = New System.Drawing.Point(267, 18)
        Me.xRegistros.MaxLength = 5
        Me.xRegistros.Name = "xRegistros"
        Me.xRegistros.Size = New System.Drawing.Size(75, 21)
        Me.xRegistros.TabIndex = 147
        Me.xRegistros.Text = "0"
        Me.xRegistros.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'lLote
        '
        Me.lLote.AutoSize = True
        Me.lLote.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lLote.ForeColor = System.Drawing.Color.FromArgb(CType(CType(10, Byte), Integer), CType(CType(87, Byte), Integer), CType(CType(139, Byte), Integer))
        Me.lLote.Location = New System.Drawing.Point(188, 22)
        Me.lLote.Name = "lLote"
        Me.lLote.Size = New System.Drawing.Size(55, 13)
        Me.lLote.TabIndex = 146
        Me.lLote.Text = "Registros"
        '
        'Label7
        '
        Me.Label7.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label7.BackColor = System.Drawing.Color.FromArgb(CType(CType(10, Byte), Integer), CType(CType(87, Byte), Integer), CType(CType(139, Byte), Integer))
        Me.Label7.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold)
        Me.Label7.ForeColor = System.Drawing.Color.White
        Me.Label7.Location = New System.Drawing.Point(1, 2)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(964, 16)
        Me.Label7.TabIndex = 138
        Me.Label7.Text = " Información"
        Me.Label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lResumen
        '
        Me.lResumen.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lResumen.BackColor = System.Drawing.Color.Transparent
        Me.lResumen.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lResumen.ForeColor = System.Drawing.Color.FromArgb(CType(CType(10, Byte), Integer), CType(CType(87, Byte), Integer), CType(CType(139, Byte), Integer))
        Me.lResumen.Location = New System.Drawing.Point(148, 597)
        Me.lResumen.Name = "lResumen"
        Me.lResumen.Size = New System.Drawing.Size(943, 22)
        Me.lResumen.TabIndex = 200
        Me.lResumen.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'sEstado
        '
        Me.sEstado.BackColor = System.Drawing.Color.FromArgb(CType(CType(32, Byte), Integer), CType(CType(44, Byte), Integer), CType(CType(67, Byte), Integer))
        Me.sEstado.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.lStatus, Me.tProgresoBarra, Me.lRegistros})
        Me.sEstado.Location = New System.Drawing.Point(0, 628)
        Me.sEstado.Name = "sEstado"
        Me.sEstado.Size = New System.Drawing.Size(1240, 22)
        Me.sEstado.SizingGrip = False
        Me.sEstado.TabIndex = 139
        Me.sEstado.Text = "StatusStrip1"
        '
        'lStatus
        '
        Me.lStatus.Font = New System.Drawing.Font("Arial", 8.25!, System.Drawing.FontStyle.Bold)
        Me.lStatus.ForeColor = System.Drawing.Color.White
        Me.lStatus.Name = "lStatus"
        Me.lStatus.Size = New System.Drawing.Size(53, 17)
        Me.lStatus.Text = "Proceso"
        '
        'tProgresoBarra
        '
        Me.tProgresoBarra.ForeColor = System.Drawing.Color.White
        Me.tProgresoBarra.Name = "tProgresoBarra"
        Me.tProgresoBarra.Size = New System.Drawing.Size(1050, 16)
        Me.tProgresoBarra.Value = 10
        Me.tProgresoBarra.Visible = False
        '
        'lRegistros
        '
        Me.lRegistros.Name = "lRegistros"
        Me.lRegistros.Size = New System.Drawing.Size(0, 17)
        '
        'gInformacion
        '
        Me.gInformacion.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.gInformacion.BackColor = System.Drawing.Color.Transparent
        Me.gInformacion.Controls.Add(Me.bEjecutar)
        Me.gInformacion.Controls.Add(Me.Salir)
        Me.gInformacion.Controls.Add(Me.gStatus)
        Me.gInformacion.Controls.Add(Me.Label7)
        Me.gInformacion.Controls.Add(Me.xOperacion)
        Me.gInformacion.Location = New System.Drawing.Point(263, 485)
        Me.gInformacion.Name = "gInformacion"
        Me.gInformacion.Size = New System.Drawing.Size(964, 94)
        Me.gInformacion.TabIndex = 118
        Me.gInformacion.TabStop = False
        '
        'bEjecutar
        '
        Me.bEjecutar.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.bEjecutar.BackColor = System.Drawing.Color.White
        Me.bEjecutar.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.bEjecutar.Font = New System.Drawing.Font("Arial", 9.0!)
        Me.bEjecutar.Image = Global.PublicadorMarketONE.My.Resources.Resources.rotate24
        Me.bEjecutar.Location = New System.Drawing.Point(758, 24)
        Me.bEjecutar.Name = "bEjecutar"
        Me.bEjecutar.Size = New System.Drawing.Size(100, 64)
        Me.bEjecutar.TabIndex = 242
        Me.bEjecutar.Text = "Ejecutar"
        Me.bEjecutar.TextAlign = System.Drawing.ContentAlignment.BottomCenter
        Me.bEjecutar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText
        Me.bEjecutar.UseVisualStyleBackColor = False
        '
        'Salir
        '
        Me.Salir.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Salir.BackColor = System.Drawing.Color.White
        Me.Salir.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.Salir.Font = New System.Drawing.Font("Arial", 9.0!)
        Me.Salir.Image = Global.PublicadorMarketONE.My.Resources.Resources.remove32
        Me.Salir.Location = New System.Drawing.Point(864, 24)
        Me.Salir.Name = "Salir"
        Me.Salir.Size = New System.Drawing.Size(100, 64)
        Me.Salir.TabIndex = 241
        Me.Salir.Text = "Detener"
        Me.Salir.TextAlign = System.Drawing.ContentAlignment.BottomCenter
        Me.Salir.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText
        Me.Salir.UseVisualStyleBackColor = False
        '
        'gStatus
        '
        Me.gStatus.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.gStatus.BackColor = System.Drawing.Color.Transparent
        Me.gStatus.Controls.Add(Me.Label1)
        Me.gStatus.Controls.Add(Me.xRegistros)
        Me.gStatus.Controls.Add(Me.lLote)
        Me.gStatus.Controls.Add(Me.xCantDoc)
        Me.gStatus.Controls.Add(Me.Label15)
        Me.gStatus.Controls.Add(Me.Label14)
        Me.gStatus.Controls.Add(Me.xSegundos)
        Me.gStatus.Controls.Add(Me.Label13)
        Me.gStatus.Controls.Add(Me.xTiempo)
        Me.gStatus.Controls.Add(Me.Label10)
        Me.gStatus.Controls.Add(Me.Label9)
        Me.gStatus.Location = New System.Drawing.Point(392, 20)
        Me.gStatus.Name = "gStatus"
        Me.gStatus.Size = New System.Drawing.Size(350, 70)
        Me.gStatus.TabIndex = 154
        Me.gStatus.TabStop = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(10, Byte), Integer), CType(CType(87, Byte), Integer), CType(CType(139, Byte), Integer))
        Me.Label1.Location = New System.Drawing.Point(188, 47)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(73, 13)
        Me.Label1.TabIndex = 148
        Me.Label1.Text = "Documentos"
        '
        'xCantDoc
        '
        Me.xCantDoc.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(99, Byte), Integer))
        Me.xCantDoc.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.xCantDoc.Enabled = False
        Me.xCantDoc.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold)
        Me.xCantDoc.Location = New System.Drawing.Point(267, 44)
        Me.xCantDoc.Name = "xCantDoc"
        Me.xCantDoc.Size = New System.Drawing.Size(75, 21)
        Me.xCantDoc.TabIndex = 16
        Me.xCantDoc.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label15
        '
        Me.Label15.AutoSize = True
        Me.Label15.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label15.ForeColor = System.Drawing.Color.FromArgb(CType(CType(10, Byte), Integer), CType(CType(87, Byte), Integer), CType(CType(139, Byte), Integer))
        Me.Label15.Location = New System.Drawing.Point(96, 44)
        Me.Label15.Name = "Label15"
        Me.Label15.Size = New System.Drawing.Size(58, 13)
        Me.Label15.TabIndex = 146
        Me.Label15.Text = "segundos"
        '
        'Label14
        '
        Me.Label14.AutoSize = True
        Me.Label14.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label14.ForeColor = System.Drawing.Color.FromArgb(CType(CType(10, Byte), Integer), CType(CType(87, Byte), Integer), CType(CType(139, Byte), Integer))
        Me.Label14.Location = New System.Drawing.Point(96, 22)
        Me.Label14.Name = "Label14"
        Me.Label14.Size = New System.Drawing.Size(50, 13)
        Me.Label14.TabIndex = 145
        Me.Label14.Text = "minutos"
        '
        'xSegundos
        '
        Me.xSegundos.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(99, Byte), Integer))
        Me.xSegundos.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.xSegundos.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold)
        Me.xSegundos.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.xSegundos.Location = New System.Drawing.Point(69, 41)
        Me.xSegundos.Name = "xSegundos"
        Me.xSegundos.ReadOnly = True
        Me.xSegundos.Size = New System.Drawing.Size(27, 21)
        Me.xSegundos.TabIndex = 144
        Me.xSegundos.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label13.ForeColor = System.Drawing.Color.FromArgb(CType(CType(10, Byte), Integer), CType(CType(87, Byte), Integer), CType(CType(139, Byte), Integer))
        Me.Label13.Location = New System.Drawing.Point(5, 44)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(52, 13)
        Me.Label13.TabIndex = 143
        Me.Label13.Text = "Restante"
        '
        'xTiempo
        '
        Me.xTiempo.BackColor = System.Drawing.Color.FromArgb(CType(CType(149, Byte), Integer), CType(CType(230, Byte), Integer), CType(CType(168, Byte), Integer))
        Me.xTiempo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.xTiempo.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xTiempo.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.xTiempo.Location = New System.Drawing.Point(69, 18)
        Me.xTiempo.MaxLength = 2
        Me.xTiempo.Name = "xTiempo"
        Me.xTiempo.Size = New System.Drawing.Size(27, 21)
        Me.xTiempo.TabIndex = 140
        Me.xTiempo.Text = "5"
        Me.xTiempo.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label10.ForeColor = System.Drawing.Color.FromArgb(CType(CType(10, Byte), Integer), CType(CType(87, Byte), Integer), CType(CType(139, Byte), Integer))
        Me.Label10.Location = New System.Drawing.Point(6, 22)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(53, 13)
        Me.Label10.TabIndex = 139
        Me.Label10.Text = "Intervalo"
        '
        'Label9
        '
        Me.Label9.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label9.BackColor = System.Drawing.Color.FromArgb(CType(CType(10, Byte), Integer), CType(CType(87, Byte), Integer), CType(CType(139, Byte), Integer))
        Me.Label9.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold)
        Me.Label9.ForeColor = System.Drawing.Color.White
        Me.Label9.Location = New System.Drawing.Point(-3, 0)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(353, 16)
        Me.Label9.TabIndex = 138
        Me.Label9.Text = " Parámetros"
        Me.Label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'xOperacion
        '
        Me.xOperacion.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.xOperacion.BackColor = System.Drawing.Color.FromArgb(CType(CType(195, Byte), Integer), CType(CType(220, Byte), Integer), CType(CType(230, Byte), Integer))
        Me.xOperacion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.xOperacion.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xOperacion.ForeColor = System.Drawing.Color.FromArgb(CType(CType(10, Byte), Integer), CType(CType(87, Byte), Integer), CType(CType(139, Byte), Integer))
        Me.xOperacion.Location = New System.Drawing.Point(6, 23)
        Me.xOperacion.Multiline = True
        Me.xOperacion.Name = "xOperacion"
        Me.xOperacion.ReadOnly = True
        Me.xOperacion.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.xOperacion.Size = New System.Drawing.Size(366, 67)
        Me.xOperacion.TabIndex = 142
        '
        'Timer
        '
        Me.Timer.Interval = 30000
        '
        'Segundos
        '
        '
        'xTabla
        '
        Me.xTabla.AllowUserToAddRows = False
        Me.xTabla.AllowUserToDeleteRows = False
        Me.xTabla.AllowUserToResizeRows = False
        Me.xTabla.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.xTabla.AutoGenerateColumns = False
        Me.xTabla.BackgroundColor = System.Drawing.Color.SlateGray
        Me.xTabla.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.xTabla.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.SeleccionDataGridViewCheckBoxColumn, Me.EstadoDataGridViewTextBoxColumn, Me.TipoDocDescripcion, Me.NumDocDataGridViewTextBoxColumn, Me.FechaDataGridViewTextBoxColumn, Me.UsuarioDataGridViewTextBoxColumn, Me.NombreLocalDataGridViewTextBoxColumn, Me.POS, Me.ClienteDataGridViewTextBoxColumn, Me.RutClienteDataGridViewTextBoxColumn, Me.NombreClienteDataGridViewTextBoxColumn, Me.FPagoDescripcionDataGridViewTextBoxColumn, Me.MontoDocumentoDataGridViewTextBoxColumn, Me.ObservacionDataGridViewTextBoxColumn})
        Me.xTabla.DataSource = Me.sDocumentos
        Me.xTabla.Location = New System.Drawing.Point(12, 60)
        Me.xTabla.Name = "xTabla"
        Me.xTabla.RowHeadersVisible = False
        Me.xTabla.Size = New System.Drawing.Size(1215, 419)
        Me.xTabla.TabIndex = 340
        '
        'TipoDocDescripcion
        '
        Me.TipoDocDescripcion.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.TipoDocDescripcion.DataPropertyName = "TipoDocDescripcion"
        Me.TipoDocDescripcion.HeaderText = "Documento"
        Me.TipoDocDescripcion.Name = "TipoDocDescripcion"
        Me.TipoDocDescripcion.ReadOnly = True
        Me.TipoDocDescripcion.Width = 87
        '
        'POS
        '
        Me.POS.DataPropertyName = "POS"
        DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle2.Format = "N0"
        DataGridViewCellStyle2.NullValue = Nothing
        Me.POS.DefaultCellStyle = DataGridViewCellStyle2
        Me.POS.HeaderText = "POS"
        Me.POS.Name = "POS"
        Me.POS.ReadOnly = True
        Me.POS.Width = 40
        '
        'DataGridViewTextBoxColumn1
        '
        Me.DataGridViewTextBoxColumn1.DataPropertyName = "TipoDocDescripcion"
        Me.DataGridViewTextBoxColumn1.HeaderText = "Documento"
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        Me.DataGridViewTextBoxColumn1.ReadOnly = True
        Me.DataGridViewTextBoxColumn1.Width = 120
        '
        'WinDeco1
        '
        Me.WinDeco1.AlturaFooter = 65
        Me.WinDeco1.AlturaTitulo = 50
        Me.WinDeco1.BackColor = System.Drawing.Color.FromArgb(CType(CType(10, Byte), Integer), CType(CType(87, Byte), Integer), CType(CType(139, Byte), Integer))
        Me.WinDeco1.BordeVentana = 2
        Me.WinDeco1.ColorBordeVentana = System.Drawing.Color.FromArgb(CType(CType(10, Byte), Integer), CType(CType(87, Byte), Integer), CType(CType(139, Byte), Integer))
        Me.WinDeco1.ColorPanelBanner = System.Drawing.Color.FromArgb(CType(CType(10, Byte), Integer), CType(CType(87, Byte), Integer), CType(CType(139, Byte), Integer))
        Me.WinDeco1.Dock = System.Windows.Forms.DockStyle.Top
        Me.WinDeco1.FondoAplicacionA = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.WinDeco1.FondoAplicacionB = System.Drawing.Color.FromArgb(CType(CType(220, Byte), Integer), CType(CType(230, Byte), Integer), CType(CType(235, Byte), Integer))
        Me.WinDeco1.FondoColorA = System.Drawing.Color.FromArgb(CType(CType(29, Byte), Integer), CType(CType(73, Byte), Integer), CType(CType(140, Byte), Integer))
        Me.WinDeco1.FondoColorB = System.Drawing.Color.FromArgb(CType(CType(29, Byte), Integer), CType(CType(73, Byte), Integer), CType(CType(140, Byte), Integer))
        Me.WinDeco1.FooterColorA = System.Drawing.Color.FromArgb(CType(CType(29, Byte), Integer), CType(CType(73, Byte), Integer), CType(CType(140, Byte), Integer))
        Me.WinDeco1.FooterColorB = System.Drawing.Color.FromArgb(CType(CType(32, Byte), Integer), CType(CType(44, Byte), Integer), CType(CType(67, Byte), Integer))
        Me.WinDeco1.FooterSolido = System.Drawing.Color.FromArgb(CType(CType(32, Byte), Integer), CType(CType(44, Byte), Integer), CType(CType(67, Byte), Integer))
        Me.WinDeco1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(32, Byte), Integer), CType(CType(44, Byte), Integer), CType(CType(67, Byte), Integer))
        Me.WinDeco1.ImagenBanner = Global.PublicadorMarketONE.My.Resources.Resources.banner3
        Me.WinDeco1.Location = New System.Drawing.Point(0, 0)
        Me.WinDeco1.Margin = New System.Windows.Forms.Padding(2)
        Me.WinDeco1.MuestraBordeExterior = -1
        Me.WinDeco1.Name = "WinDeco1"
        Me.WinDeco1.SiempreVisible = False
        Me.WinDeco1.Size = New System.Drawing.Size(1240, 50)
        Me.WinDeco1.TabIndex = 147
        Me.WinDeco1.TituloVentana = "Publicador DTE"
        Me.WinDeco1.UbicacionTitulo = New System.Drawing.Point(4, 19)
        Me.WinDeco1.VerCerrar = True
        Me.WinDeco1.VerLogo = True
        Me.WinDeco1.VerMaximizar = True
        Me.WinDeco1.VerMinimizar = True
        Me.WinDeco1.VerTitulo = True
        '
        'xLogo
        '
        Me.xLogo.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.xLogo.BackColor = System.Drawing.Color.Transparent
        Me.xLogo.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.xLogo.Image = CType(resources.GetObject("xLogo.Image"), System.Drawing.Image)
        Me.xLogo.Location = New System.Drawing.Point(12, 485)
        Me.xLogo.Name = "xLogo"
        Me.xLogo.Size = New System.Drawing.Size(179, 94)
        Me.xLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.xLogo.TabIndex = 342
        Me.xLogo.TabStop = False
        '
        'SeleccionDataGridViewCheckBoxColumn
        '
        Me.SeleccionDataGridViewCheckBoxColumn.DataPropertyName = "Seleccion"
        Me.SeleccionDataGridViewCheckBoxColumn.HeaderText = ""
        Me.SeleccionDataGridViewCheckBoxColumn.Name = "SeleccionDataGridViewCheckBoxColumn"
        Me.SeleccionDataGridViewCheckBoxColumn.Width = 40
        '
        'EstadoDataGridViewTextBoxColumn
        '
        Me.EstadoDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.EstadoDataGridViewTextBoxColumn.DataPropertyName = "Estado"
        Me.EstadoDataGridViewTextBoxColumn.HeaderText = "Estado"
        Me.EstadoDataGridViewTextBoxColumn.Name = "EstadoDataGridViewTextBoxColumn"
        Me.EstadoDataGridViewTextBoxColumn.ReadOnly = True
        Me.EstadoDataGridViewTextBoxColumn.Width = 65
        '
        'NumDocDataGridViewTextBoxColumn
        '
        Me.NumDocDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.NumDocDataGridViewTextBoxColumn.DataPropertyName = "NumDoc"
        Me.NumDocDataGridViewTextBoxColumn.HeaderText = "Número"
        Me.NumDocDataGridViewTextBoxColumn.Name = "NumDocDataGridViewTextBoxColumn"
        Me.NumDocDataGridViewTextBoxColumn.ReadOnly = True
        Me.NumDocDataGridViewTextBoxColumn.Width = 69
        '
        'FechaDataGridViewTextBoxColumn
        '
        Me.FechaDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.FechaDataGridViewTextBoxColumn.DataPropertyName = "Fecha"
        DataGridViewCellStyle1.Format = "g"
        DataGridViewCellStyle1.NullValue = Nothing
        Me.FechaDataGridViewTextBoxColumn.DefaultCellStyle = DataGridViewCellStyle1
        Me.FechaDataGridViewTextBoxColumn.HeaderText = "Fecha"
        Me.FechaDataGridViewTextBoxColumn.Name = "FechaDataGridViewTextBoxColumn"
        Me.FechaDataGridViewTextBoxColumn.ReadOnly = True
        Me.FechaDataGridViewTextBoxColumn.Width = 62
        '
        'UsuarioDataGridViewTextBoxColumn
        '
        Me.UsuarioDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.UsuarioDataGridViewTextBoxColumn.DataPropertyName = "Usuario"
        Me.UsuarioDataGridViewTextBoxColumn.HeaderText = "Usuario"
        Me.UsuarioDataGridViewTextBoxColumn.Name = "UsuarioDataGridViewTextBoxColumn"
        Me.UsuarioDataGridViewTextBoxColumn.ReadOnly = True
        Me.UsuarioDataGridViewTextBoxColumn.Width = 68
        '
        'NombreLocalDataGridViewTextBoxColumn
        '
        Me.NombreLocalDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.NombreLocalDataGridViewTextBoxColumn.DataPropertyName = "NombreLocal"
        Me.NombreLocalDataGridViewTextBoxColumn.HeaderText = "Local"
        Me.NombreLocalDataGridViewTextBoxColumn.Name = "NombreLocalDataGridViewTextBoxColumn"
        Me.NombreLocalDataGridViewTextBoxColumn.ReadOnly = True
        Me.NombreLocalDataGridViewTextBoxColumn.Width = 58
        '
        'ClienteDataGridViewTextBoxColumn
        '
        Me.ClienteDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.ClienteDataGridViewTextBoxColumn.DataPropertyName = "Cliente"
        Me.ClienteDataGridViewTextBoxColumn.HeaderText = "Cliente"
        Me.ClienteDataGridViewTextBoxColumn.Name = "ClienteDataGridViewTextBoxColumn"
        Me.ClienteDataGridViewTextBoxColumn.ReadOnly = True
        Me.ClienteDataGridViewTextBoxColumn.Width = 64
        '
        'RutClienteDataGridViewTextBoxColumn
        '
        Me.RutClienteDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.RutClienteDataGridViewTextBoxColumn.DataPropertyName = "RutCliente"
        Me.RutClienteDataGridViewTextBoxColumn.HeaderText = "Rut"
        Me.RutClienteDataGridViewTextBoxColumn.Name = "RutClienteDataGridViewTextBoxColumn"
        Me.RutClienteDataGridViewTextBoxColumn.ReadOnly = True
        Me.RutClienteDataGridViewTextBoxColumn.Width = 49
        '
        'NombreClienteDataGridViewTextBoxColumn
        '
        Me.NombreClienteDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.NombreClienteDataGridViewTextBoxColumn.DataPropertyName = "NombreCliente"
        Me.NombreClienteDataGridViewTextBoxColumn.HeaderText = "Nombre"
        Me.NombreClienteDataGridViewTextBoxColumn.Name = "NombreClienteDataGridViewTextBoxColumn"
        Me.NombreClienteDataGridViewTextBoxColumn.ReadOnly = True
        Me.NombreClienteDataGridViewTextBoxColumn.Width = 69
        '
        'FPagoDescripcionDataGridViewTextBoxColumn
        '
        Me.FPagoDescripcionDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.FPagoDescripcionDataGridViewTextBoxColumn.DataPropertyName = "FPagoDescripcion"
        Me.FPagoDescripcionDataGridViewTextBoxColumn.HeaderText = "F.Pago"
        Me.FPagoDescripcionDataGridViewTextBoxColumn.Name = "FPagoDescripcionDataGridViewTextBoxColumn"
        Me.FPagoDescripcionDataGridViewTextBoxColumn.ReadOnly = True
        Me.FPagoDescripcionDataGridViewTextBoxColumn.Width = 66
        '
        'MontoDocumentoDataGridViewTextBoxColumn
        '
        Me.MontoDocumentoDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.MontoDocumentoDataGridViewTextBoxColumn.DataPropertyName = "MontoDocumento"
        DataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        DataGridViewCellStyle3.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle3.Format = "N0"
        DataGridViewCellStyle3.NullValue = Nothing
        Me.MontoDocumentoDataGridViewTextBoxColumn.DefaultCellStyle = DataGridViewCellStyle3
        Me.MontoDocumentoDataGridViewTextBoxColumn.HeaderText = "Monto"
        Me.MontoDocumentoDataGridViewTextBoxColumn.Name = "MontoDocumentoDataGridViewTextBoxColumn"
        Me.MontoDocumentoDataGridViewTextBoxColumn.ReadOnly = True
        Me.MontoDocumentoDataGridViewTextBoxColumn.Width = 62
        '
        'ObservacionDataGridViewTextBoxColumn
        '
        Me.ObservacionDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.ObservacionDataGridViewTextBoxColumn.DataPropertyName = "Observacion"
        DataGridViewCellStyle4.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ObservacionDataGridViewTextBoxColumn.DefaultCellStyle = DataGridViewCellStyle4
        Me.ObservacionDataGridViewTextBoxColumn.HeaderText = "Observación"
        Me.ObservacionDataGridViewTextBoxColumn.MinimumWidth = 800
        Me.ObservacionDataGridViewTextBoxColumn.Name = "ObservacionDataGridViewTextBoxColumn"
        Me.ObservacionDataGridViewTextBoxColumn.ReadOnly = True
        Me.ObservacionDataGridViewTextBoxColumn.Width = 800
        '
        'sDocumentos
        '
        Me.sDocumentos.DataSource = GetType(PublicadorMarketONE.ResultadoPublicadorDTE)
        '
        'PublicadorDTE
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1240, 650)
        Me.Controls.Add(Me.xLogo)
        Me.Controls.Add(Me.lResumen)
        Me.Controls.Add(Me.xTabla)
        Me.Controls.Add(Me.WinDeco1)
        Me.Controls.Add(Me.sEstado)
        Me.Controls.Add(Me.gInformacion)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximumSize = New System.Drawing.Size(1240, 768)
        Me.Name = "PublicadorDTE"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Publicador DTE"
        Me.sEstado.ResumeLayout(False)
        Me.sEstado.PerformLayout()
        Me.gInformacion.ResumeLayout(False)
        Me.gInformacion.PerformLayout()
        Me.gStatus.ResumeLayout(False)
        Me.gStatus.PerformLayout()
        CType(Me.xTabla, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.xLogo, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.sDocumentos, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Label7 As Label
    Friend WithEvents lResumen As Label
    Friend WithEvents sEstado As StatusStrip
    Friend WithEvents lStatus As ToolStripStatusLabel
    Friend WithEvents tProgresoBarra As ToolStripProgressBar
    Friend WithEvents lRegistros As ToolStripStatusLabel
    Friend WithEvents WinDeco1 As WinDeco.WinDeco
    Friend WithEvents gInformacion As GroupBox
    Friend WithEvents xCantDoc As TextBox
    Friend WithEvents Timer As Timer
    Friend WithEvents gStatus As GroupBox
    Friend WithEvents Label9 As Label
    Friend WithEvents xOperacion As TextBox
    Friend WithEvents xTiempo As TextBox
    Friend WithEvents xRegistros As TextBox
    Friend WithEvents lLote As Label
    Friend WithEvents Label10 As Label
    Friend WithEvents Segundos As Timer
    Friend WithEvents Label14 As Label
    Friend WithEvents xSegundos As TextBox
    Friend WithEvents Label13 As Label
    Friend WithEvents Label15 As Label
    Friend WithEvents xTabla As DataGridView
    Friend WithEvents sDocumentos As BindingSource
    Friend WithEvents DataGridViewTextBoxColumn1 As DataGridViewTextBoxColumn
    Friend WithEvents Salir As Button
    Friend WithEvents bEjecutar As Button
    Friend WithEvents SeleccionDataGridViewCheckBoxColumn As DataGridViewCheckBoxColumn
    Friend WithEvents EstadoDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents TipoDocDescripcion As DataGridViewTextBoxColumn
    Friend WithEvents NumDocDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents FechaDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents UsuarioDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents NombreLocalDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents POS As DataGridViewTextBoxColumn
    Friend WithEvents ClienteDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents RutClienteDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents NombreClienteDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents FPagoDescripcionDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents MontoDocumentoDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents ObservacionDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents Label1 As Label
    Friend WithEvents xLogo As PictureBox
End Class
