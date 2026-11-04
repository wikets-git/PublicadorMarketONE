Imports System.ComponentModel
Imports System.Runtime.CompilerServices

''' <summary>
''' Publicador DTE MarketONE: programa automático. Al abrir espera unos segundos, inicia el sistema con el usuario SYS
''' y cada N minutos toma los documentos electrónicos pendientes de envío de MarketONE (DocumentosG, STATUS_DTE 0 o 3),
''' los timbra y los envía al SII con el módulo DTE de MarketONE, igual que Control DTE Emitidos.
''' </summary>
Public Class PublicadorDTE

    'Filtro fijo del Publicador: todos los locales y tipos electrónicos, del mes anterior al mes en curso (se recalcula en cada ciclo)
    Dim gDesde As Date
    Dim gHasta As Date

    ''' <summary>Segundos de espera antes de iniciar.</summary>
    Const SEGUNDOS_INICIO As Integer = 5

    Public Structure EstadosPublicadorDTE
        Public Const ErrorEnvioSII = "Error en Envío a SII"
        Public Const PendienteEnvio = "Pendiente de Envío"
        Public Const EmitidoTED = "Emitido con TED"
        Public Const AprobadoSII = "Aprobado por SII"
        Public Const AprobadoReparosSII = "Aprobado con Reparos por SII"
        Public Const RechazadoSII = "Rechazado por SII"
        Public Const NoElectronico = "No es Electrónico"
    End Structure

    ''' <summary>Se activa con Cancelar o al cerrar la ventana: el proceso se detiene en el documento siguiente.</summary>
    Public flagDetener As Boolean = False
    ''' <summary>Se está cerrando el programa.</summary>
    Private Cerrando As Boolean = False
    ''' <summary>Se confirmó el cierre (no se vuelve a preguntar).</summary>
    Private CierreConfirmado As Boolean = False
    ''' <summary>El envío está detenido (botón Detener) hasta presionar Ejecutar.</summary>
    Private EnvioDetenido As Boolean = False
    ''' <summary>True mientras corre un ciclo (consulta o envío).</summary>
    Private Procesando As Boolean = False
    ''' <summary>Se pidió salir durante un ciclo: la ventana se cierra cuando el ciclo termina de detenerse.</summary>
    Private SalirPendiente As Boolean = False
    ReadOnly G_MINUTOS As Integer = 60000
    Private WithEvents Reintento As New System.Windows.Forms.Timer()
    Private WithEvents Espera As New System.Windows.Forms.Timer()
    Private SegundosEspera As Integer

    ''' <summary>Título base de la ventana (sin empresa), para no repetir la empresa en cada reintento.</summary>
    Private TituloBase As String = ""

    Private Sub PublicadorDTE_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        TituloBase = $"Publicador DTE MarketONE  v.{VERSION}"
        'La configuración ya se leyó al abrir (control de instancia): la empresa se ve desde el inicio
        If CONFIGURACION Is Nothing Then Settings.GetSettings()
        ActualizarTitulo()
        sDocumentos.Load(New List(Of ResultadoPublicadorDTE))
        CalcularFechas()

        'Programa desatendido: los mensajes nunca abren ventanas, van a la barra de operación y al log
        AvisoDesatendido = Sub(m, esError) Operacion(m, esError)
        IniciarBandeja()
        EscucharOtraInstancia()
    End Sub

    ''' <summary>
    ''' Si se abre otra copia del Publicador para la misma base, esa copia se cierra y avisa por un evento de Windows;
    ''' aquí se recibe el aviso y se muestra esta ventana (aunque esté minimizada en la bandeja).
    ''' </summary>
    Private Sub EscucharOtraInstancia()
        Dim evento = My.MyApplication.EventoMostrar
        If evento Is Nothing Then Return
        Dim hilo As New Threading.Thread(Sub()
                                             Do
                                                 Try
                                                     evento.WaitOne()
                                                     If IsDisposed OrElse Disposing Then Exit Do
                                                     BeginInvoke(New MethodInvoker(AddressOf MostrarVentana))
                                                 Catch
                                                     Exit Do
                                                 End Try
                                             Loop
                                         End Sub) With {.IsBackground = True, .Name = "OtraInstancia"}
        hilo.Start()
    End Sub

    ''' <summary>Título: "Publicador DTE MarketONE v.x - Empresa" (la empresa se ve desde que se abre).</summary>
    Private Sub ActualizarTitulo()
        Dim titulo = TituloBase
        Dim empresa = If(CONFIGURACION?.Empresa, "").Trim
        If empresa <> "" Then titulo &= $" - {empresa}"
        Text = titulo
        WinDeco1.TituloVentana = titulo
    End Sub

    ''' <summary>Rango del Publicador: desde el primer día del mes anterior hasta el fin del mes en curso.</summary>
    Private Sub CalcularFechas()
        gDesde = FechaDesde(FechaFiltro(ModoFecha.Inicial, Now.Date.AddMonths(-1)))
        gHasta = FechaHasta(FechaFiltro(ModoFecha.Final, Now.Date))
    End Sub

    ''' <summary>Documentos por lote (0 = todos de una vez). Viene de RegistrosPorCiclo y se puede cambiar en pantalla.</summary>
    Private ReadOnly Property RegistrosPorLote As Integer
        Get
            Return Math.Max(0, CInt(Val(xRegistros.Text)))
        End Get
    End Property

    Private Sub xRegistros_KeyPress(sender As Object, e As KeyPressEventArgs) Handles xRegistros.KeyPress
        ValidarDigitos(e)
    End Sub

    Private Sub xRegistros_Leave(sender As Object, e As EventArgs) Handles xRegistros.Leave
        xRegistros.Text = RegistrosPorLote.ToString
    End Sub

    Private Sub xTiempo_KeyPress(sender As Object, e As KeyPressEventArgs) Handles xTiempo.KeyPress
        ValidarDigitos(e)
    End Sub

    ''' <summary>Al mostrarse la ventana cuenta 5, 4, 3, 2, 1 y luego arranca.</summary>
    Private Sub PublicadorDTE_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        SegundosEspera = SEGUNDOS_INICIO
        MostrarCuenta()
        Espera.Interval = 1000
        Espera.Enabled = True
    End Sub

    Private Sub Espera_Tick(sender As Object, e As EventArgs) Handles Espera.Tick
        SegundosEspera -= 1
        If SegundosEspera > 0 Then
            MostrarCuenta()
            Return
        End If
        Espera.Enabled = False
        xSegundos.Text = ""
        lStatus.Text = "Listo"
        Arrancar()
    End Sub

    ''' <summary>Cuenta regresiva de inicio en Restante, en Procesando y en la barra de estado.</summary>
    Private Sub MostrarCuenta()
        xSegundos.Text = SegundosEspera.ToString
        lStatus.Text = $"Inicio en {SegundosEspera}..."
        Operacion($"El proceso inicia en {SegundosEspera}...")
    End Sub

    ''' <summary>
    ''' Inicia el sistema (sin login, usuario SYS) y el ciclo automático.
    ''' Si falla (configuración, base caída, etc.) muestra el motivo y reintenta en 1 minuto.
    ''' </summary>
    Private Sub Arrancar()
        If Cerrando Then Return
        Cursor = Cursors.WaitCursor
        Operacion("Iniciando sistema...")
        Dim errorInicio = ""
        Try
            errorInicio = IniciarSistema()
        Catch ex As Exception
            errorInicio = ex.Message
        End Try
        Cursor = Cursors.Arrow

        If errorInicio <> "" Then
            ActivityLog.LogError(errorInicio, "Arrancar")
            Operacion($"No se pudo iniciar: {errorInicio} Se reintenta en 1 minuto.", True)
            CambiarAlerta(True, "No se pudo iniciar: " & Strings.Left(errorInicio, 120))
            GrabarStatus(True)
            Reintento.Interval = G_MINUTOS
            Reintento.Enabled = True
            Return
        End If
        Reintento.Enabled = False

        ActualizarTitulo()
        ActualizarIconoBandeja()
        xTiempo.Text = If(CONFIGURACION.MinutosEnvioAutomatico > 0, CONFIGURACION.MinutosEnvioAutomatico, 5).ToString
        xRegistros.Text = Math.Max(0, CONFIGURACION.RegistrosPorCiclo).ToString
        IngresarAuditoria(TituloBase, A_INGRESADO)
        IngresarAuditoria(TituloBase, "Enviar a SII", "Envío Automático Iniciar")

        Ejecutar()
    End Sub

    Private Sub Reintento_Tick(sender As Object, e As EventArgs) Handles Reintento.Tick
        Reintento.Enabled = False
        Arrancar()
    End Sub

    ''' <summary>
    ''' Ejecutar: si no hay un proceso en curso, procesa de inmediato. Si el sistema aún no inició (cuenta regresiva
    ''' o reintento por error), inicia ahora.
    ''' </summary>
    Private Sub bEjecutar_Click(sender As Object, e As EventArgs) Handles bEjecutar.Click
        If Procesando OrElse Cerrando Then
            Operacion("Ya hay un proceso en curso.")
            Return
        End If
        EnvioDetenido = False
        Salir.Enabled = True     'Reanudar: Detener vuelve a quedar disponible
        ActualizarIconoBandeja()
        If Not SistemaIniciado Then
            Espera.Enabled = False
            Reintento.Enabled = False
            xSegundos.Text = ""
            Arrancar()
            Return
        End If
        Ejecutar()
    End Sub

    ''' <summary>
    ''' Detener: detiene el envío. Si hay un proceso en curso, se detiene en el documento siguiente; si está esperando
    ''' el próximo ciclo, en la cuenta regresiva o en un reintento, se cancela. El envío queda detenido (sin ciclo
    ''' automático) hasta presionar Ejecutar. Con la ventana minimizada, la bandeja avisa que el envío está detenido.
    ''' </summary>
    Private Sub Salir_Click(sender As Object, e As EventArgs) Handles Salir.Click
        If Procesando Then
            flagDetener = True
            Salir.Enabled = False
            Operacion("Deteniendo el envío... termina el documento en curso.")
            Return
        End If
        If EnvioDetenido Then
            Operacion("El envío ya está detenido. Presione Ejecutar para reanudar.")
            Return
        End If
        Espera.Enabled = False
        Reintento.Enabled = False
        Detener()
        MarcarEnvioDetenido()
    End Sub

    ''' <summary>Deja el envío detenido hasta que se presione Ejecutar.</summary>
    Private Sub MarcarEnvioDetenido()
        EnvioDetenido = True
        Salir.Enabled = False    'Detener queda deshabilitado hasta presionar Ejecutar
        bEjecutar.Enabled = True
        xSegundos.Text = ""
        lStatus.Text = "Envío detenido"
        Operacion("Envío detenido. Presione Ejecutar para reanudar.", False)
        ActualizarIconoBandeja()
    End Sub

    Private Sub PublicadorDTE_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        'Con la X (o Salir del menú de la bandeja) se pide confirmación; no al apagar Windows ni al cierre automático
        If Not CierreConfirmado AndAlso e.CloseReason = CloseReason.UserClosing Then
            If Not Visible Then MostrarVentana()
            If Not Pregunta($"¿Desea cerrar el {NombrePublicador}?", NombrePublicador) Then
                e.Cancel = True
                Return
            End If
            CierreConfirmado = True
        End If

        Cerrando = True
        flagDetener = True
        Espera.Enabled = False
        Reintento.Enabled = False
        Timer.Enabled = False
        Segundos.Enabled = False
        If Procesando Then
            e.Cancel = True
            SalirPendiente = True
            Salir.Enabled = False
            bEjecutar.Enabled = False
            Operacion("Deteniendo el proceso... el programa se cerrará al terminar el documento en curso.")
            Return
        End If
        Detener()
    End Sub

    Private Sub PublicadorDTE_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        CerrarBandeja()
        If SistemaIniciado Then IngresarAuditoria("Login", "Salida del Sistema", P_SISTEMA, VERSION)
    End Sub

    ''' <summary>Consulta los pendientes de envío. Con maximo > 0, solo ese lote.</summary>
    Sub PrepararConsulta(maximo As Integer)
        If Not SistemaIniciado Then Return
        xCantDoc.Text = ""
        Consultar(maximo)
        xCantDoc.Text = xTabla.Rows.Count.ToString
    End Sub

    ''' <summary>
    ''' Carga en la grilla los documentos pendientes de envío (STATUS_DTE 0 o 3) de DocumentosG, con el modelo de
    ''' Control DTE Emitidos de MarketONE. Con maximo > 0, solo ese lote.
    ''' </summary>
    Sub Consultar(Optional maximo As Integer = 0)
        Operacion("Consultando Documentos...")

        lStatus.Text = "Cargando Documentos..."
        sDocumentos.Load(New List(Of ResultadoPublicadorDTE))
        Cursor = Cursors.WaitCursor
        xTabla.Visible = False

        Dim resultado As New List(Of ResultadoPublicadorDTE)
        Try
            Dim docs = ListarPendientes(gDesde, gHasta, maximo)
            tProgresoBarra.Minimum = 0
            tProgresoBarra.Maximum = Math.Max(1, docs.Count)
            tProgresoBarra.Value = 0
            tProgresoBarra.Visible = True

            Dim DC = New MarketONEDataContext(P_CONEXION)
            Dim locales = DC.Locales.ToList().ToDictionary(Function(x) x.Local)
            'Clientes de los documentos (si son muchos se leen todos, para no pasar el límite de parámetros de SQL Server)
            Dim idsClientes = docs.Select(Function(x) x.Cliente).Distinct().ToList()
            Dim qClientes = If(idsClientes.Count <= 2000, DC.Clientes.Where(Function(x) idsClientes.Contains(x.Cliente)), DC.Clientes)
            Dim clientes = qClientes.Select(Function(x) New With {x.Cliente, x.Nombre, x.Rut}).ToList() _
                                    .GroupBy(Function(x) x.Cliente).ToDictionary(Function(g) g.Key, Function(g) g.First())
            Dim descFPagos = DC.FPagos.ToList().GroupBy(Function(x) If(x.FPago, "").Trim()) _
                                      .ToDictionary(Function(g) g.Key, Function(g) If(g.First().DescFPago, "").Trim())
            Dim descTipos = DC.T_TipoDoc.ToList().GroupBy(Function(x) If(x.TipoDoc, "").Trim()) _
                                        .ToDictionary(Function(g) g.Key, Function(g) If(g.First().DescTipoDoc, "").Trim())

            Dim posicion = 0
            For Each doc In docs
                If flagDetener Then Exit For
                posicion += 1
                If posicion Mod 50 = 0 Then
                    xCantDoc.Text = $"{posicion}/{docs.Count}"
                    DoEvents()
                End If

                Dim r As New ResultadoPublicadorDTE With {.Seleccion = False, .Documento = doc}
                AsignarEstado(r, doc)

                Dim loc As Locales = Nothing
                locales.TryGetValue(doc.Local, loc)

                r.Local = doc.Local
                r.NombreLocal = If(loc?.NombreLocal, "").Trim
                r.POS = doc.POS
                r.TipoDoc = doc.TipoDoc.Trim
                Dim desc = ""
                r.TipoDocDescripcion = If(descTipos.TryGetValue(r.TipoDoc, desc), desc, DescConstId(Of TiposDoc)(r.TipoDoc))
                r.NumDoc = doc.Numero
                r.Fecha = doc.Fecha
                r.Usuario = If(doc.Usuario, "").Trim

                'Igual que Control DTE Emitidos: nombre y RUT del cliente (sin cliente, RUT 00.000.000-0)
                Dim cli = If(clientes.ContainsKey(doc.Cliente), clientes(doc.Cliente), Nothing)
                r.Cliente = If(cli?.Cliente, 0D)
                r.NombreCliente = If(cli?.Nombre, "").Trim
                r.RutCliente = If(cli?.Rut, "00.000.000-0").Trim

                Dim fpago = If(If(doc.FPago, "").Trim <> "", doc.FPago.Trim, FormasPago.Efectivo)
                r.FPago = fpago
                r.FPagoDescripcion = If(descFPagos.TryGetValue(fpago, desc), desc, "Efectivo")
                r.MontoDocumento = doc.Total

                If tProgresoBarra.Value < tProgresoBarra.Maximum Then tProgresoBarra.Value += 1
                resultado.Add(r)
            Next
        Catch ex As Exception
            ActivityLog.LogException(ex.ToString, "Consultar")
            Operacion($"ERROR al consultar: {ex.Message}", True)
        End Try

        tProgresoBarra.Visible = False
        sDocumentos.Load(resultado)
        If xTabla.Rows.Count > 0 Then xTabla.Rows(0).Selected = True
        xCantDoc.Text = resultado.Count.ToString
        MostrarResumen(resultado.Count, maximo)
        lStatus.Text = "Listo  "
        Cursor = Cursors.Arrow
        xTabla.Visible = True
    End Sub

    ''' <summary>Resumen del lote bajo la grilla.</summary>
    Private Sub MostrarResumen(cantidad As Integer, maximo As Integer)
        lResumen.ForeColor = Color.FromArgb(72, 95, 142)
        If cantidad = 0 Then
            lResumen.Text = "No hay documentos pendientes de envío"
        ElseIf maximo > 0 Then
            lResumen.Text = $"Documentos pendientes de envío en este lote: {cantidad:N0} (máximo {maximo:N0})"
        Else
            lResumen.Text = $"Documentos pendientes de envío: {cantidad:N0}"
        End If
        lResumen.Visible = True
    End Sub

    ''' <summary>Estado según STATUS_DTE de MarketONE (0 No enviado, 3 Timbrado) y el último mensaje de envío.</summary>
    Private Sub AsignarEstado(r As ResultadoPublicadorDTE, doc As DocumentoPendiente)
        r.ColorFuente = Color.Black
        Select Case doc.STATUS_DTE
            Case EstadosSII.Timbrado
                r.Estado = EstadosPublicadorDTE.EmitidoTED
                r.ColorFondo = ColoresEstadosDTE.EnvioCorrecto
            Case Else
                r.Estado = EstadosPublicadorDTE.PendienteEnvio
                r.ColorFondo = ColoresEstadosDTE.EnvioPendiente
        End Select
        'Último error de envío que dejó MarketONE en el documento
        Dim mensaje = If(doc.MensajeSincronizacion, "").Trim
        r.Observacion = If(mensaje <> "", $"Último intento: {mensaje}", "")
    End Sub

    ''' <summary>
    ''' Timbra y envía al SII los documentos seleccionados, con el módulo DTE de MarketONE (GenerarTED y EmitirDTE, que
    ''' a su vez usa EmitirBoleta o EmitirDocumentoExportacion según el tipo), igual que Control DTE Emitidos.
    ''' Los errores no abren ventanas: quedan en la observación del documento y en el log.
    ''' </summary>
    Private Sub EnviarDocumentos()
        Dim hay = False
        MensajeDTE = ""
        'Confirma la marca del check que se está editando, para que el envío la tome y luego se pueda desmarcar
        xTabla.EndEdit()
        sDocumentos.EndEdit()
        Operacion("Enviando Documentos...")
        Cursor = Cursors.WaitCursor
        DoEvents()

        Dim documentos = CType(sDocumentos.DataSource, List(Of ResultadoPublicadorDTE))
        Dim total = documentos.Count
        Dim i = 0
        Dim localActual As Decimal = -1

        For Each documento In documentos.ToList()
            If flagDetener Then Exit For
            i += 1
            xCantDoc.Text = $"{i}/{total}"
            If i Mod 5 = 0 Then DoEvents()
            If Not documento.Seleccion Then Continue For
            If documento.Documento Is Nothing Then Continue For

            Dim tipoDoc = documento.TipoDoc
            Dim numDoc = documento.NumDoc
            Dim wLocal = documento.Local

            Try
                'Se vuelve a leer el estado en la base: si otro proceso ya lo envió, no se procesa
                Dim docG = ObtenerDocumento(wLocal, documento.Documento.TipoDoc, numDoc)
                If docG Is Nothing Then
                    MarcarError(documento, "Documento no encontrado en la base de datos")
                    Continue For
                End If
                If Not EstadosPendientes.Contains(docG.STATUS_DTE) Then
                    Dim estado = CType(CInt(docG.STATUS_DTE), EstadosSII)
                    If estado = EstadosSII.Aprobado Then
                        documento.Estado = EstadosPublicadorDTE.AprobadoSII
                        documento.ColorFondo = ColoresEstadosDTE.Aprobado
                    End If
                    documento.Observacion = $"Ya no está pendiente de envío (STATUS_DTE = {estado})"
                    documento.Seleccion = UNCHECK
                    Continue For
                End If

                'Parámetros de facturación electrónica del local del documento (RUT emisor, ambiente, DTEBox)
                If localActual <> wLocal Then
                    If Not DTE.Parametros_DTE(wLocal) Then
                        MarcarError(documento, $"Error al leer los parámetros de facturación electrónica del local {wLocal}")
                        localActual = -1
                        Continue For
                    End If
                    localActual = wLocal
                End If

                If Not DTE.EsElectronico(tipoDoc) Then
                    documento.Estado = EstadosPublicadorDTE.NoElectronico
                    documento.Observacion = "No se puede enviar a DTE Documentos No electrónicos"
                    documento.Seleccion = UNCHECK
                    Continue For
                End If

                Dim motivo = ValidarParaEnvio(docG)
                If motivo = "" AndAlso If(docG.Moneda, "").Trim = "USD" AndAlso G_DOLAR <= 0 AndAlso
                   {TiposDoc.FacturaExportacion, TiposDoc.NotaCreditoExportacion, TiposDoc.NotaDebitoExportacion}.Contains(tipoDoc) Then
                    motivo = "No se pudo obtener el valor del dólar: el documento en USD queda para el ciclo siguiente."
                End If
                If motivo <> "" Then
                    MarcarError(documento, motivo)
                    Continue For
                End If

                hay = True
                Operacion($"Enviando {tipoDoc} {numDoc}")

                'Timbre (TED) del documento, como Control DTE Emitidos: deja el documento en STATUS_DTE = 3 (Timbrado)
                If docG.STATUS_DTE = EstadosSII.No_Enviado Then
                    Dim ted = DTE.GenerarTED(wLocal, tipoDoc, numDoc, documento.Fecha, documento.RutCliente,
                                             documento.NombreCliente, documento.MontoDocumento, "")
                    If Not ted.Estado Then
                        ActivityLog.Log($"{tipoDoc} {numDoc} Local {wLocal} Error al generar TED: {ted.Mensaje}", "GenerarTimbreDTE", "DTE")
                    End If
                End If

                'Envío al DTE
                MensajeDTE = ""
                If DTE.EmitirDTE(wLocal, tipoDoc, numDoc, False, False) Then
                    documento.Estado = EstadosPublicadorDTE.AprobadoSII
                    documento.Seleccion = UNCHECK
                    documento.Observacion = "Enviado Correctamente"
                    documento.ColorFondo = ColoresEstadosDTE.Aprobado
                Else
                    MarcarError(documento, If(MensajeDTE.Trim = "", "Error al Enviar Documento Electrónico.", MensajeDTE.Trim))
                End If
            Catch ex As Exception
                ActivityLog.LogException(ex.ToString, "EnviarDocumentos")
                MarcarError(documento, $"Error al enviar: {ex.Message}")
            End Try

            documento.ColorFuente = Color.Black
            sDocumentos.ResetBindings(False)
            xTabla.Refresh()
        Next

        DTE.Parametros_DTE(G_LOCALACTUAL)
        DoEvents()

        xCantDoc.Text = $"{total}/{total}"
        IngresarAuditoria(TituloBase, "Enviar a SII", "Envío Automático", i.ToString, i)

        Cursor = Cursors.Arrow
        Operacion(If(hay, "Envío Finalizado", "Sin documentos para enviar"))
        sDocumentos.ResetBindings(True)
    End Sub

    ''' <summary>Deja el documento con error: el motivo queda en la observación y en el log (nunca en una ventana).</summary>
    Private Sub MarcarError(documento As ResultadoPublicadorDTE, motivo As String)
        documento.Estado = EstadosPublicadorDTE.ErrorEnvioSII
        documento.Observacion = motivo
        documento.ColorFondo = ColoresEstadosDTE.ErrorEnvio
        documento.ColorFuente = Color.Black
        ActivityLog.Log($"{documento.TipoDoc} {documento.NumDoc} Local {documento.Local} {motivo}", "ErrorEnvioDTE", "DTE")
    End Sub

    Private Sub Timer_Tick(sender As Object, e As EventArgs) Handles Timer.Tick
        Ejecutar()
    End Sub

    ''' <summary>
    ''' Ciclo automático, sin filtros: todos los locales y tipos de documento electrónicos, del mes anterior al mes en curso.
    ''' Registros = 0: envía todo lo pendiente. Registros = N: lee hasta N pendientes y los envía; el resto queda para
    ''' los ciclos siguientes. Luego espera el intervalo.
    ''' </summary>
    Sub Ejecutar()
        If Not SistemaIniciado OrElse Cerrando OrElse Procesando Then Return
        Procesando = True
        flagDetener = False
        bEjecutar.Enabled = False
        Salir.Enabled = True
        Detener()
        DoEvents()

        CalcularFechas()
        ActualizarDolar()
        Dim maximo = RegistrosPorLote
        Dim conError = 0
        InicioCicloBandeja()
        Try
            If maximo > 0 Then
                lStatus.Text = $"Lote de hasta {maximo:N0} registros"
                Operacion($"Lote de hasta {maximo:N0} registros...")
            End If

            PrepararConsulta(maximo)
            If Not flagDetener Then
                For Each d In CType(sDocumentos.DataSource, List(Of ResultadoPublicadorDTE))
                    d.Seleccion = True
                Next
                EnviarDocumentos()
                conError = CType(sDocumentos.DataSource, List(Of ResultadoPublicadorDTE)) _
                               .Where(Function(d) d.Estado = EstadosPublicadorDTE.ErrorEnvioSII).Count()
            End If
        Catch ex As Exception
            Operacion($"ERROR al ejecutar: {ex.Message}", True)
        Finally
            Procesando = False
        End Try

        If SalirPendiente Then
            'Se cerró la ventana durante el ciclo: ahora que terminó, se cierra
            BeginInvoke(New MethodInvoker(AddressOf Close))
            Return
        End If

        Dim cancelado = flagDetener
        flagDetener = False
        If Not cancelado Then
            FinCicloBandeja(conError)
            'status.json para el programa que vigila el Publicador (Error si algún documento o el ciclo tuvo error)
            GrabarStatus(ErroresCiclo OrElse conError > 0)
        End If
        bEjecutar.Enabled = True
        Salir.Enabled = Not cancelado
        If cancelado Then
            'Detenido con el botón Detener: no sigue el ciclo automático hasta presionar Ejecutar
            MarcarEnvioDetenido()
        Else
            Iniciar()
        End If
    End Sub

    ''' <summary>
    ''' Valor del dólar para las Facturas de Exportación en USD (Control DTE Emitidos lo obtiene al abrir; aquí en cada
    ''' ciclo). Si no se puede obtener, se mantiene el último valor conocido.
    ''' </summary>
    Private Sub ActualizarDolar()
        Try
            Dim dolar = ClienteREST.ObtenerDolar()
            If dolar > 0 Then G_DOLAR = dolar
        Catch ex As Exception
            ActivityLog.LogException(ex.Message, "ObtenerDolar")
        End Try
    End Sub

    Sub Iniciar()
        If Val(xTiempo.Text) <= 0 Then xTiempo.Text = "5"
        Timer.Enabled = False
        Timer.Interval = G_MINUTOS * CInt(Val(xTiempo.Text))
        Timer.Enabled = True
        Segundos.Enabled = False
        Segundos.Interval = 1000
        Segundos.Enabled = True
        xSegundos.Text = (Val(xTiempo.Text) * 60).ToString
        Operacion("Esperando Ciclo...")
    End Sub

    Sub Detener()
        Timer.Enabled = False
        Segundos.Enabled = False
        Operacion("")
        xSegundos.Text = ""
    End Sub

    Private Sub Segundos_Tick(sender As Object, e As EventArgs) Handles Segundos.Tick
        Dim xSeg = CInt(Val(xSegundos.Text) - 1)
        xSegundos.Text = If(xSeg < 0, "", xSeg.ToString)
    End Sub

    Private Sub xTabla_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles xTabla.CellFormatting
        If e.RowIndex = -1 Then Exit Sub
        Dim documento = CType(xTabla.Rows(e.RowIndex).DataBoundItem, ResultadoPublicadorDTE)
        If documento Is Nothing Then Exit Sub
        xTabla.Rows(e.RowIndex).DefaultCellStyle.BackColor = documento.ColorFondo
        xTabla.Rows(e.RowIndex).DefaultCellStyle.ForeColor = documento.ColorFuente
    End Sub

    Public Sub Operacion(mensaje As String, Optional fallido As Boolean = False)
        xOperacion.Text = mensaje
        If fallido Then ErroresCiclo = True
        If fallido Then
            xOperacion.BackColor = Color.FromArgb(230, 195, 220)
            xOperacion.ForeColor = Color.FromArgb(143, 10, 60)
        Else
            xOperacion.BackColor = Color.FromArgb(195, 220, 230)
            xOperacion.ForeColor = Color.FromArgb(10, 87, 139)
        End If
        DoEvents()
    End Sub

#Region "Bandeja de notificaciones"
    'Ícono en la bandeja de Windows: al minimizar la ventana se oculta y queda solo el ícono.
    'Doble clic (o "Abrir" en el menú) la vuelve a mostrar. Si un ciclo termina con errores,
    'el ícono pasa a rojo, parpadea mientras la ventana está oculta y aparece un aviso.
    Private WithEvents Bandeja As New NotifyIcon()
    Private WithEvents Parpadeo As New System.Windows.Forms.Timer() With {.Interval = 600}
    Private MenuBandeja As New ContextMenuStrip()
    Private IconoNormal As Icon
    Private IconoAlertaRojo As Icon
    Private IconoDetenido As Icon
    ''' <summary>Repite el aviso de envío detenido cada 30 minutos mientras la ventana esté minimizada.</summary>
    Private WithEvents Recordatorio As New System.Windows.Forms.Timer() With {.Interval = 30 * 60000}
    Private ParpadeoEncendido As Boolean
    ''' <summary>True si el último ciclo terminó con errores (se limpia con un ciclo sin errores).</summary>
    Private EnAlerta As Boolean = False
    ''' <summary>Se marcó algún error durante el ciclo en curso (Operacion con fallido = True).</summary>
    Private ErroresCiclo As Boolean = False

    ''' <summary>"Publicador Centermarket" (o solo "Publicador" si aún no se carga la configuración).</summary>
    Private ReadOnly Property NombrePublicador As String
        Get
            Dim empresa = If(CONFIGURACION?.Empresa, "").Trim
            Return If(empresa = "", "Publicador", $"Publicador {empresa}")
        End Get
    End Property

    Private ReadOnly Property TEXTO_OK As String
        Get
            Return NombrePublicador
        End Get
    End Property

    Private ReadOnly Property TEXTO_DETENIDO As String
        Get
            Return $"{NombrePublicador}: Envío detenido"
        End Get
    End Property

    Private ReadOnly Property TEXTO_ALERTA As String
        Get
            Return $"{NombrePublicador}: Errores en envío de datos"
        End Get
    End Property

    ''' <summary>Windows admite hasta 63 caracteres en el texto del ícono (más lanza error).</summary>
    Private Shared Function TextoIcono(texto As String) As String
        Return If(texto.Length <= 63, texto, texto.Substring(0, 60) & "...")
    End Function

    Private Sub IniciarBandeja()
        IconoNormal = If(Icon, SystemIcons.Application)
        IconoAlertaRojo = CrearIconoAlerta(IconoNormal, Color.Red)
        IconoDetenido = CrearIconoAlerta(IconoNormal, Color.Orange)
        MenuBandeja.Items.Add("Abrir", Nothing, Sub() MostrarVentana())
        MenuBandeja.Items.Add("Ejecutar ahora", Nothing, Sub() bEjecutar.PerformClick())
        MenuBandeja.Items.Add(New ToolStripSeparator())
        MenuBandeja.Items.Add("Salir", Nothing, Sub()
                                                    MostrarVentana()
                                                    Close()
                                                End Sub)
        Bandeja.ContextMenuStrip = MenuBandeja
        Bandeja.Icon = IconoNormal
        Bandeja.Text = TextoIcono(TEXTO_OK)
        Bandeja.Visible = True
    End Sub

    ''' <summary>Ícono del programa con un círculo de color (rojo: errores; ámbar: envío detenido).</summary>
    Private Shared Function CrearIconoAlerta(baseIcono As Icon, color As Color) As Icon
        Dim tam = SystemInformation.SmallIconSize
        Using bmp As New Bitmap(tam.Width, tam.Height)
            Using g = Graphics.FromImage(bmp)
                g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                Using chico As New Icon(baseIcono, tam)
                    g.DrawIcon(chico, New Rectangle(0, 0, tam.Width, tam.Height))
                End Using
                Dim d = CInt(tam.Width * 0.6)
                Dim r As New Rectangle(tam.Width - d, tam.Height - d, d - 1, d - 1)
                Using b As New SolidBrush(color)
                    g.FillEllipse(b, r)
                End Using
                g.DrawEllipse(Pens.White, r)
            End Using
            Return Icon.FromHandle(bmp.GetHicon())
        End Using
    End Function

    ''' <summary>Al minimizar, la ventana se oculta y queda el ícono en la bandeja.</summary>
    Private Sub PublicadorDTE_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If WindowState = FormWindowState.Minimized AndAlso Bandeja.Visible Then
            Hide()
            ShowInTaskbar = False
            ActualizarIconoBandeja()
        End If
    End Sub

    Private Sub Bandeja_DoubleClick(sender As Object, e As EventArgs) Handles Bandeja.MouseDoubleClick, Bandeja.BalloonTipClicked
        MostrarVentana()
    End Sub

    ''' <summary>Muestra la ventana (desde la bandeja o al abrir el programa otra vez).</summary>
    Public Sub MostrarVentana()
        ShowInTaskbar = True
        Show()
        If WindowState = FormWindowState.Minimized Then WindowState = FormWindowState.Normal
        Activate()
        ActualizarIconoBandeja()
    End Sub

    ''' <summary>Al empezar un ciclo se limpia la marca de errores del ciclo.</summary>
    Private Sub InicioCicloBandeja()
        ErroresCiclo = False
    End Sub

    ''' <summary>
    ''' Al terminar un ciclo: si hubo errores pasa a alerta (y avisa la primera vez); si no, vuelve a normal.
    ''' </summary>
    Private Sub FinCicloBandeja(documentosConError As Integer)
        Dim hubo = ErroresCiclo OrElse documentosConError > 0
        Dim detalle = If(documentosConError > 0, $"{documentosConError} documento(s) con error en el último envío.",
                                                "Revise el mensaje en la ventana del Publicador.")
        CambiarAlerta(hubo, detalle)
    End Sub

    Private Sub CambiarAlerta(alerta As Boolean, Optional detalle As String = "")
        Dim nueva = alerta AndAlso Not EnAlerta
        EnAlerta = alerta
        ActualizarIconoBandeja()
        If nueva Then Bandeja.ShowBalloonTip(10000, NombrePublicador, $"{TEXTO_ALERTA}{vbCrLf}{detalle}", ToolTipIcon.Error)
    End Sub

    Private Sub ActualizarIconoBandeja()
        'Prioridad: errores (rojo, parpadea si la ventana está oculta) > envío detenido (ámbar) > normal
        Dim oculta = Not Visible
        Bandeja.Text = TextoIcono(If(EnAlerta, TEXTO_ALERTA, If(EnvioDetenido, TEXTO_DETENIDO, TEXTO_OK)))
        If EnAlerta AndAlso oculta Then
            Parpadeo.Enabled = True
        Else
            Parpadeo.Enabled = False
            Bandeja.Icon = If(EnAlerta, IconoAlertaRojo, If(EnvioDetenido, IconoDetenido, IconoNormal))
        End If

        'Aviso de envío detenido con la ventana minimizada: al quedar así y luego cada 30 minutos
        If EnvioDetenido AndAlso oculta Then
            If Not Recordatorio.Enabled Then
                AvisarEnvioDetenido()
                Recordatorio.Enabled = True
            End If
        Else
            Recordatorio.Enabled = False
        End If
    End Sub

    Private Sub AvisarEnvioDetenido()
        Bandeja.ShowBalloonTip(10000, NombrePublicador,
                               $"{TEXTO_DETENIDO}{vbCrLf}No se están enviando documentos. Abra el Publicador y presione Ejecutar.",
                               ToolTipIcon.Warning)
    End Sub

    Private Sub Recordatorio_Tick(sender As Object, e As EventArgs) Handles Recordatorio.Tick
        If EnvioDetenido AndAlso Not Visible Then AvisarEnvioDetenido() Else Recordatorio.Enabled = False
    End Sub

    Private Sub Parpadeo_Tick(sender As Object, e As EventArgs) Handles Parpadeo.Tick
        ParpadeoEncendido = Not ParpadeoEncendido
        Bandeja.Icon = If(ParpadeoEncendido, IconoAlertaRojo, IconoNormal)
    End Sub

    Private Sub CerrarBandeja()
        Parpadeo.Enabled = False
        Recordatorio.Enabled = False
        Bandeja.Visible = False
        Bandeja.Dispose()
    End Sub

    Private Sub xLogo_Click(sender As Object, e As EventArgs) Handles xLogo.Click
        'Using acerca As New AcercaDe()
        '    acerca.ShowDialog(Me)
        'End Using
    End Sub
#End Region
End Class

Public Class ResultadoPublicadorDTE
    Implements INotifyPropertyChanged
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Protected Sub OnPropertyChanged(<CallerMemberName> ByVal Optional propertyName As String = Nothing)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
    Private _Seleccion As Boolean
    Public Property Seleccion As Boolean
        Get
            Return _Seleccion
        End Get
        Set(value As Boolean)
            _Seleccion = value
            OnPropertyChanged()
        End Set
    End Property
    Public Property Estado As String
    Public Property TipoDoc As String
    Public Property TipoDocDescripcion As String
    Public Property NumDoc As Decimal
    Public Property Fecha As DateTime
    Public Property Usuario As String
    Public Property Local As Decimal
    Public Property POS As Decimal
    Public Property NombreLocal As String
    Public Property Cliente As Decimal
    Public Property RutCliente As String
    Public Property NombreCliente As String
    Public Property FPago As String
    Public Property FPagoDescripcion As String
    Public Property MontoDocumento As Decimal
    Public Property Observacion As String = ""
    Public Property ColorFondo As Color
    Public Property ColorFuente As Color
    <Browsable(False)>
    Public Property Documento As DocumentoPendiente
End Class
