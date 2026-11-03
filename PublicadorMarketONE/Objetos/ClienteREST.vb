Imports System.IO
Imports System.Net
Imports Newtonsoft.Json

''' <summary>
''' Consulta del valor del dólar (CMF), copiada de MarketONE (Objetos\ClienteREST.vb y Objetos\Dolar.vb).
''' La usa el envío de Facturas de Exportación en dólares, igual que Control DTE Emitidos al abrir.
''' </summary>
Public Class ClienteREST

    Public Shared Function ObtenerDolar() As Decimal
        Try
            Dim URL = "https://api.cmfchile.cl/api-sbifv3/recursos_api/dolar?apikey=9345d5bff36b42f0b4b1c942b6de2a97bb72d1b8&formato=json"
            Dim respuesta = ConectarREST_GET(URL)
            If respuesta.Item1 = 200 Then
                Dim resultado = JsonConvert.DeserializeObject(Of ResultadoDolar)(respuesta.Item3)
                If resultado IsNot Nothing Then
                    Return resultado.Dolares.FirstOrDefault().Valor.Replace(".", "").Replace(".", "").ToDecimal(3)
                End If
            End If
            Return 0
        Catch ex As Exception
            Return 0
        End Try
    End Function

    Public Shared Function ConectarREST_GET(ByVal wURL As String) As Tuple(Of Integer, String, String)

        Dim wReq = WebRequest.Create(wURL)
        wReq.Method = "GET"
        wReq.ContentType = "application/json; charset=utf-8"
        wReq.ContentLength = 0
        Dim wStatus As HttpStatusCode = HttpStatusCode.Unused
        Dim wStatusDesc = ""
        Dim wResponse = ""

        Try
            Dim wResp = CType(wReq.GetResponse(), HttpWebResponse)
            wStatus = wResp.StatusCode
            wStatusDesc = wResp.StatusDescription
            Dim StreamReader = New StreamReader(wResp.GetResponseStream())
            wResponse = StreamReader.ReadToEnd()
        Catch wex As WebException

            If wex.Message.Contains("403") Then
                wStatus = HttpStatusCode.Forbidden
                wStatusDesc = wStatus.ToString()
            End If

        Catch ex As Exception
            wStatus = 0
            wStatusDesc = ex.Message
        End Try

        Return New Tuple(Of Integer, String, String)(Convert.ToInt32(wStatus), wStatusDesc, wResponse)

    End Function


End Class

Public Class ResultadoDolar
    Public Property Dolares As List(Of DetalleDolar)
End Class

Public Class DetalleDolar
    Public Property Valor As String
    Public Property Fecha As String
End Class
