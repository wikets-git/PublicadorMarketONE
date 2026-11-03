Imports System.ComponentModel

''' <summary>
''' Constantes de negocio de MarketONE (copiadas de MarketONE\Funciones.vb, sin cambios) que usa el módulo DTE.
''' Si cambian en MarketONE, deben actualizarse aquí.
''' </summary>
Public Module ConstantesMarketONE

    Public Structure Ambientes
        Public Const Homologacion As String = "Homologación"
        Public Const Produccion As String = "Producción"
    End Structure

    Public Structure TiposDoc
        <Description("Movimiento")>
        Public Const Movimiento As String = "MV"
        <Description("Boleta")>
        Public Const Boleta As String = "BV"
        <Description("Factura")>
        Public Const Factura As String = "FV"
        <Description("Guía de Despacho")>
        Public Const GuiaDespacho As String = "GD"
        <Description("Guía de Despacho de Compra")>
        Public Const GuiaDespachoCompra As String = "GC"
        <Description("Nota de Crédito")>
        Public Const NotaCredito As String = "NC"
        <Description("Nota de Débito")>
        Public Const NotaDebito As String = "ND"
        <Description("Factura Exenta")>
        Public Const FacturaExenta As String = "FE"
        <Description("Boleta Exenta")>
        Public Const BoletaExenta As String = "BE"
        <Description("Nota de Crédito de Compra")>
        Public Const NotaCreditoCompra As String = "CC"
        <Description("Nota de Débito de Compra")>
        Public Const NotaDebitoCompra As String = "DC"
        <Description("Factura Exenta de Compra")>
        Public Const FacturaExentaCompra As String = "EC"
        <Description("Boleta Exenta de Compra")>
        Public Const BoletaExentaCompra As String = "EB"
        <Description("Factura de Compra")>
        Public Const FacturaCompra As String = "FC"
        <Description("Boleta de Compra")>
        Public Const BoletaCompra As String = "BC"
        <Description("Cotización")>
        Public Const Cotizacion As String = "CO"
        <Description("Orden de Trabajo")>
        Public Const OrdenTrabajo As String = "OT"
        <Description("Ticket")>
        Public Const Ticket As String = "TK"
        <Description("Sin Documento")>
        Public Const SinDocumento As String = "SD"
        <Description("Receta")>
        Public Const Receta As String = "RO"
        <Description("Crédito")>
        Public Const Credito As String = "CR"
        <Description("Orden de Compra")>
        Public Const OrdenCompra As String = "OC"
        <Description("Nota de Venta")>
        Public Const NotaVenta As String = "NV"
        <Description("Factura de Exportación ")>
        Public Const FacturaExportacion As String = "FX"
        <Description("Nota de Crédito Exportación")>
        Public Const NotaCreditoExportacion As String = "NX"
        <Description("Nota de Débito Exportación")>
        Public Const NotaDebitoExportacion As String = "DX"
        <Description("Hoja de Servicio")>
        Public Const HojaServicio As String = "HS"
        <Description("Hoja de Entrada de Materiales")>
        Public Const HojaEntradaMat As String = "HM"
        <Description("Inventario")>
        Public Const Inventario As String = "IV"
        <Description("Solicitud de Pedido")>
        Public Const SolicitudPedido As String = "SP"
        <Description("Solicitud de Compra")>
        Public Const SolicitudCompra As String = "SC"
        <Description("Nota de Venta Exenta")>
        Public Const NotaVentaExenta As String = "NE"
        <Description("Comprobante de Pago")>
        Public Const ComprobantePago As String = "CP"
    End Structure

    Public Structure FormasPago

        <Description("Efectivo")>
        Public Const Efectivo As String = "E"
        <Description("Depósito")>
        Public Const Deposito As String = "D"
        <Description("Transferencia")>
        Public Const Transferencia As String = "B"
        <Description("Cheque")>
        Public Const Cheque As String = "C"
        <Description("Tarjeta Débito")>
        Public Const TarjetaDebito As String = "R"
        <Description("Tarjeta Crédito")>
        Public Const TarjetaCredito As String = "T"
        <Description("Varias")>
        Public Const VariasFormas As String = "V"
        <Description("Crédito")>
        Public Const Credito As String = "X"
        <Description("Online")>
        Public Const Online = "O"
        <Description("WebPay")>
        Public Const WebPay As String = "W"
        <Description("MercadoPago")>
        Public Const MercadoPago As String = "M"
        <Description("Cheque 30 días")>
        Public Const Cheque30Dias As String = "3"
        <Description("Cheque 30, 60, 90 días")>
        Public Const Cheque306090Dias As String = "6"
        <Description("Nota de Crédito")>
        Public Const NotaCredito As String = "N"
        <Description("Pago Proveedor")>
        Public Const Proveedor As String = "P"
        <Description("Aporte Convenio")>
        Public Const AporteConvenio As String = "AC"
        <Description("Orden de Compra")>
        Public Const OrdeCompra = "OC"
        <Description("Junaeb")>
        Public Const Junaeb As String = "J"
        <Description("Sodexo")>
        Public Const Sodexo As String = "S"
        <Description("Propina")>
        Public Const Propina As String = "PR"
        <Description("Garantía")>
        Public Const Garantia As String = "G"
        <Description("Vale")>
        Public Const Vale As String = "L"
        <Description("Gift Card")>
        Public Const GiftCard As String = "K"
        <Description("Transporte")>
        Public Const Transporte As String = "Q"
        <Description("Cupón")>
        Public Const Cupon As String = "CU"
        <Description("Tarjeta Alimentacion")>
        Public Const TarjetaAlimentacion As String = "TA"


    End Structure

    Public Structure MotivosDocumento
        <Description("Corrección de Valores")>
        Public Const CorreccionValores = "O"
        <Description("Corrección de Datos")>
        Public Const CorreccionDatos = "D"
        <Description("Anulación")>
        Public Const Anulacion = "A"
        <Description("Tránsito")>
        Public Const Transito = "S"
        <Description("Sin Motivo")>
        Public Const SinMotivo = "X"
        <Description("Abono Crédito")>
        Public Const AbonoCredito = "C"
        <Description("Licitación")>
        Public Const Licitacion = "L"
    End Structure

    Public Structure MotivosGuiaDespacho
        <Description("Devolución")>
        Public Const Devolución = "O"
        <Description("Tránsito")>
        Public Const Transito = "S"
        <Description("Venta")>
        Public Const Venta = "V"
        <Description("Traspaso")>
        Public Const Traspaso = "T"
        <Description("Mermas")>
        Public Const Mermas = "M"
        <Description("Exposición")>
        Public Const Exposición = "E"
        <Description("Sin Motivo")>
        Public Const SinMotivo = ""
    End Structure

    Public Enum EstadosSII
        Null = -1
        No_Enviado = 0
        Aprobado = 1
        Rechazado = 2
        Timbrado = 3
        Existente = 4
        ErrorTimbre = 5
        CAFVencido = 6
        ReHacer = 7
        EnvioAnulado = 8
        Control = 9
    End Enum

    Public Enum EstadoComercialDTE
        Sin_Aceptacion = 0
        AceptadoLey19983 = 1
        Aceptado = 2
        Rechazado = 3
    End Enum

    Public Structure ImpuestosPorcentaje
        Public Const Exento As Decimal = 0
        Public Const Harinas As Decimal = 12
        Public Const Carnes As Decimal = 5
        Public Const Minerales As Decimal = 10
        Public Const Bebidas As Decimal = 18
        Public Const Vinos As Decimal = 20.5D
        Public Const Cervezas As Decimal = 20.5D
        Public Const Licores As Decimal = 31.5D
        'FEPP = a) 63% para los contribuyentes cuyos ingresos anuales hayan sido iguales o inferiores a 18.600 UTM. b) 39% para los contribuyentes cuyos ingresos anuales hayan sido superiores a 18.600 y no excedan de 42.500 UTM. c) 29,65% para los contribuyentes cuyos ingresos anuales sean superiores a 42.500 UTM.
        Public Const FEPP As Decimal = 63
    End Structure

End Module
