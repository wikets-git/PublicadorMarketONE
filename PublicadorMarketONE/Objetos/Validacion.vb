Imports System.ComponentModel
Imports System.ComponentModel.DataAnnotations
Imports System.Data.SqlClient
Imports System.Globalization
Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading

Module Validacion
    Sub ValidateObj(ByVal obj As Object)
        'Version  Nueva
        If obj Is Nothing Then Return

        Dim properties As PropertyInfo() = obj.[GetType]().GetProperties()

        For Each propertyInfo In properties

            If Not propertyInfo.CanWrite Then Continue For

            ' Manejo de cadenas
            If propertyInfo.PropertyType = GetType(String) Then
                Dim att As StringLengthAttribute = Nothing
                Dim attArr = propertyInfo.GetCustomAttributes(GetType(StringLengthAttribute), False)

                If attArr IsNot Nothing AndAlso attArr.Length > 0 Then
                    att = If(CType(attArr(0), StringLengthAttribute), Nothing)
                End If

                Dim lenght = Convert.ToInt32(If(att?.MaximumLength, 0))

                If lenght > 0 Then
                    propertyInfo.SetValue(obj, If(propertyInfo.GetValue(obj), "").ToString().Trim().Truncate(lenght))
                Else
                    propertyInfo.SetValue(obj, If(propertyInfo.GetValue(obj), "").ToString().Trim())
                End If

                Continue For
            End If

            ' Manejo de valores nulos
            If Nullable.GetUnderlyingType(propertyInfo.PropertyType) IsNot Nothing Then
                Dim baseType = propertyInfo.PropertyType.GenericTypeArguments(0)

                If baseType = GetType(Decimal) OrElse baseType = GetType(Single) OrElse baseType = GetType(Double) Then
                    If propertyInfo.GetValue(obj) Is Nothing Then
                        propertyInfo.SetValue(obj, Activator.CreateInstance(baseType))
                    End If
                    Continue For
                End If

                If baseType = GetType(Integer) Then
                    If propertyInfo.GetValue(obj) Is Nothing Then
                        propertyInfo.SetValue(obj, 0)
                    End If
                    Continue For
                End If

                If baseType = GetType(DateTime) Then
                    If propertyInfo.GetValue(obj) Is Nothing Then
                        propertyInfo.SetValue(obj, New DateTime(2000, 1, 1))
                    End If
                    Continue For
                End If

                If baseType = GetType(Char) Then
                    If propertyInfo.GetValue(obj) Is Nothing Then
                        propertyInfo.SetValue(obj, " "c)
                    End If
                    Continue For
                End If

                If baseType = GetType(Boolean) Then
                    If propertyInfo.GetValue(obj) Is Nothing Then
                        propertyInfo.SetValue(obj, False)
                    End If
                    Continue For
                End If

            Else
                ' Tipos no nulos
                If propertyInfo.PropertyType = GetType(DateTime) Then
                    If propertyInfo.GetValue(obj).ToString().ToDateTime() < New DateTime(1901, 1, 1) Then
                        propertyInfo.SetValue(obj, New DateTime(2000, 1, 1))
                    End If
                    Continue For
                End If

                If propertyInfo.PropertyType = GetType(Char) AndAlso propertyInfo.GetValue(obj).ToString() = "" Then
                    propertyInfo.SetValue(obj, " "c)
                    Continue For
                End If

                If propertyInfo.PropertyType = GetType(Boolean) Then
                    If propertyInfo.GetValue(obj) Is Nothing OrElse Not CBool(propertyInfo.GetValue(obj)) Then
                        propertyInfo.SetValue(obj, False)
                        Continue For
                    End If
                End If
            End If
        Next

    End Sub

    <Extension()>
    Function Truncate(ByVal texto As String, ByVal largoMaximo As Integer) As String
        If String.IsNullOrEmpty(texto) Then texto = ""
        texto = If(texto.Length <= largoMaximo, texto, texto.Substring(0, largoMaximo))
        Return texto.Trim()
    End Function

    <Extension()>
    Function Ellipsis(ByVal texto As String, ByVal largoMaximo As Integer) As String
        If String.IsNullOrEmpty(texto) Then texto = ""
        texto = If(texto.Length <= largoMaximo, texto,
                texto.Substring(0, largoMaximo) & "...")
        Return texto.Trim()
    End Function

    <Extension()>
    Function ToText(ByVal num As Double) As String
        Return num.ToString("G29")
    End Function

    <Extension()>
    Function ToTextReceta(ByVal num As Double, posiciones As Integer) As String
        Dim dec = Convert.ToDecimal(num)
        Dim text = dec.ToText()
        text = Regex.Replace(text, "[^0-9.,-]", "").Trim()
        If text <> "" AndAlso Not text = "0" Then
            text = String.Format($"{{0:f{posiciones}}}", num)
        End If
        Return text
    End Function

    <Extension()>
    Function ToText(ByVal num As Decimal) As String
        Return num.ToString("G29")
    End Function

    <Extension()>
    Function ToText(ByVal num As Long) As String
        Return num.ToString("G29")
    End Function

    <Extension()>
    Function ToTextDecimal(ByVal num As Decimal) As String
        Dim val = num.ToString()
        val.Replace(".", ",")
        If val.Contains(",") Then
            Dim partes = val.Split(","c)
            If partes(1).All(Function(x) x = "0") Then
                val = partes(0).ToDecimal().ToString("##,#0")
            Else
                Dim dec = Regex.Replace(partes(1), "(\d*?)0+\b", "$1")
                val = $"{partes(0).ToDecimal().ToString("##,#0")},{dec}"
            End If
        Else
            val = num.ToString("##,#0")
        End If
        Return val
    End Function

    <Extension()>
    Function ToText(ByVal num As Integer) As String
        Return num.ToString("G29")
    End Function
    <Extension()>
    Function ToMoney(ByVal num As Integer) As String
        Return num.ToString("##,#0")
    End Function

    <Extension()>
    Function ToMoney(ByVal num As Decimal) As String
        Return num.ToString("##,#0")
    End Function
    <Extension()>
    Function ToMoneyDecimal(ByVal num As Decimal) As String
        Return ToTextDecimal(num)
    End Function
    <Extension()>
    Function ToMoney(num As Double) As String
        Return num.ToString("##,#0")
    End Function

    <Extension()>
    Function ToDateTime(ByVal text As String) As DateTime
        Dim result = New DateTime(2000, 1, 1)
        DateTime.TryParse(text, result)
        Return result
    End Function

    <Extension()>
    Function AppendText(ByRef text As String, newText As String) As String
        text &= vbCrLf & newText
        Return text
    End Function

    <Extension()>
    Public Function ToCapitalCase(input As String) As String
        If String.IsNullOrWhiteSpace(input) Then
            Return String.Empty
        End If

        Dim textInfo As TextInfo = CultureInfo.CurrentCulture.TextInfo
        Return textInfo.ToTitleCase(input.ToLower())
    End Function

    <Extension>
    Public Function ToDouble(ByRef Texto As TextBox) As Double
        Dim wTexto = Texto.Text
        Return If(IsNumeric(wTexto), wTexto.ToDecimal, 0)
    End Function

    <Extension>
    Public Function Trim(ByRef tb As TextBox) As String
        tb.Text = tb.Text.Trim()
        Return tb.Text
    End Function

    <Extension>
    Public Function Trim(ByRef tb As MaskedTextBox) As String
        tb.Text = tb.Text.Trim()
        Return tb.Text
    End Function
    <Extension>
    Public Function Trim(ByRef tb As RutMaskedTextBox) As String
        tb.Text = tb.Text.Trim()
        Return tb.Text
    End Function

    <Extension>
    Public Function Trim(ByRef cb As ComboBox) As String
        Return cb.Text.Trim()
    End Function

    <Extension>
    Public Function ToDouble(ByRef Texto As String) As Double
        Return If(IsNumeric(Texto), Convert.ToDouble(Texto.ToDecimal), 0)
    End Function
    <Extension>
    <DebuggerStepThrough()>
    Public Function ToDecimal(ByRef Texto As TextBox, Optional precision As Integer = 0) As Decimal
        Return Texto.Text.ToDecimal(precision)
    End Function

    <Extension>
    Public Function ToInt(num As Decimal) As Integer
        Return Convert.ToInt32(num)
    End Function
    <Extension>
    Public Function ToInt(num As Long) As Integer
        Return Convert.ToInt32(num)
    End Function
    <Extension()>
    Public Function ToInt(ByVal num As String) As Integer
        Return Convert.ToInt32(num.ToDecimal())
    End Function
    <Extension>
    Public Function ToChar(text As String) As Char
        Return Convert.ToChar(text)
    End Function

    <Extension()>
    Public Function ToDecimal(ByVal texto As String, ByVal Optional precision As Integer = 0, ByVal Optional truncar As Boolean = False) As Decimal
        Dim result = ""

        result = New String(texto.Where(Function(x) Char.IsDigit(x) OrElse New Char() {","c, "."c}.Contains(x)).ToArray())

        result = $"{If(texto.Length > 0, (If(texto(0) = "-", "-", "")), "")}{result}"

        If result.Contains(",") AndAlso result.Contains(".") Then
            result = result.Replace(".", "").Replace(",", ".")
        ElseIf result.Contains(",") Then
            result = result.Replace(",", ".")
        ElseIf result.Contains(".") Then
            result = result.Replace(".", "")
        End If

        If result.Where(Function(x) x = ","c).Count > 1 Then
            Return 0D
        End If
        If result.Where(Function(x) x = "."c).Count > 1 Then
            Return 0D
        End If

        'Validación
        If truncar Then
            Return If(result = "", 0D, Truncate(Convert.ToDecimal(result, New CultureInfo("en-us", False)), precision))
        Else
            result = TruncateDecimalString(result)
            Return If(result = "", 0D, Math.Round(Convert.ToDecimal(result, New CultureInfo("en-us", False)), precision, MidpointRounding.AwayFromZero))
        End If
    End Function

    Public Function TruncateDecimalString(value As String, Optional maxDecimals As Integer = 12) As String
        If String.IsNullOrWhiteSpace(value) Then
            Return value
        End If

        Dim parts = value.Split("."c)

        If parts.Length = 1 Then
            Return parts(0)
        End If

        Dim integerPart = parts(0)
        Dim decimalPart = parts(1)
        If decimalPart.Length > maxDecimals Then
            decimalPart = decimalPart.Substring(0, maxDecimals)
        End If
        Return $"{integerPart}.{decimalPart}"
    End Function


    <Extension()>
    <DebuggerStepThrough()>
    Public Function ToDecimal(ByVal num As Decimal, ByVal Optional precision As Integer = 0, ByVal Optional truncate As Boolean = False) As Decimal
        Return num.ToString("G29").ToDecimal(precision, truncate)
    End Function

    <Extension()>
    <DebuggerStepThrough()>
    Public Function ToDecimal(ByVal num As Double, ByVal Optional precision As Integer = 0, ByVal Optional truncate As Boolean = False) As Decimal
        Return num.ToString("G29").ToDecimal(precision, truncate)
    End Function

    <Extension()>
    Public Function ToInt(ByVal num As Double) As Integer
        Return CInt(num)
    End Function

    Public Function Truncate(ByVal valor As Decimal, ByVal precision As Integer) As Decimal
        Dim multiplo As Decimal = Convert.ToDecimal(Math.Pow(10, precision))
        Dim resultado As Decimal = Math.Truncate(multiplo * valor)
        Return (resultado / multiplo)
    End Function
    <Extension>
    Public Function ToInteger(ByRef Texto As TextBox) As Integer
        Return Convert.ToInt32(Texto.ToDecimal())
    End Function
    <Extension>
    Public Function ToInteger(ByRef Texto As String) As Decimal
        Return If(IsNumeric(Texto), Convert.ToDecimal(Texto), 0)
    End Function


    <Extension>
    <DebuggerStepThrough()>
    Public Sub ClearSelection(ByRef cb As ComboBox)
        If cb.Items.Count = 0 Then Return
        If cb.Items(0).ToString = "" Then
            cb.SelectedIndex = 0
        Else
            cb.SelectedIndex = -1
        End If
    End Sub

    <Extension>
    <DebuggerStepThrough()>
    Public Function IsSelected(ByRef cb As ComboBox) As Boolean
        If cb.Items.Count = 0 Then Return False
        If cb.SelectedIndex = -1 Then Return False
        If cb.Text = "" Then Return False
        Return True
    End Function

    <Extension>
    <DebuggerStepThrough()>
    Public Function Value(ByRef cb As ComboBox) As String
        If cb.SelectedValue Is Nothing Then Return ""
        If cb.SelectedValue.ToString() = "-1" Then Return ""
        Return cb.SelectedValue.ToString()
    End Function


    <Extension>
    <DebuggerStepThrough()>
    Public Function DecimalValue(ByRef cb As ComboBox) As Decimal
        If cb.SelectedValue Is Nothing Then Return 0
        If cb.SelectedValue.ToString() = "-1" Then Return 0
        Return cb.SelectedValue.ToString.ToDecimal()
    End Function

    <Extension>
    <DebuggerStepThrough()>
    Public Sub First(ByRef cb As ComboBox)
        If cb.Items.Count = 0 Then
            cb.SelectedIndex = -1
        Else
            cb.SelectedIndex = 0
        End If
    End Sub

    <Extension>
    Public Sub FirstItem(ByRef cb As ComboBox)
        If cb.Items.Count = 0 Then
            cb.SelectedIndex = -1
        Else
            If cb.Items.Count > 1 Then
                If cb.Items(0).ToString.Trim() = "" Then
                    cb.SelectedIndex = 1
                End If
            Else
                cb.SelectedIndex = 0
            End If

        End If
    End Sub


    Public Enum NumericFormat
        Normal
        Money
    End Enum

    <Extension>
    Public Function ToText(ByRef Numero As TextBox, Optional Precision As Integer = 0) As String
        Dim Dec = If(IsNumeric(Numero.Text), Convert.ToDecimal(Numero.Text), 0)
        Return Dec.ToText(NumericFormat.Normal, Precision)
    End Function

    <Extension>
    Public Function ToText(ByRef Numero As Double, Optional Precision As Integer = 0) As String
        Dim Dec = Convert.ToDecimal(Numero)
        Return Dec.ToText(NumericFormat.Normal, Precision)
    End Function



    <Extension>
    Public Function ToText(ByRef Numero As Decimal, Formato As NumericFormat, Optional Precision As Integer = 0) As String
        Dim wPartes() As String
        Dim wDecimal As String = ""
        Dim wEntero As String = ""
        Dim wFormato As String = ""
        Dim wCondicion As String = ""
        Numero = Math.Round(Numero, Precision, MidpointRounding.AwayFromZero)

        If InStr(Numero.ToString, ".") > 0 Then
            wPartes = Numero.ToString.Split(CType(".", Char()))
            wDecimal = wPartes(1).ToString
            wEntero = wPartes(0).ToString
        Else
            wEntero = Numero.ToString
        End If

        wFormato = FormatNumber(wEntero, Precision,,, TriState.True).Replace(",", ".").ToString

        Return If(wDecimal = "", wFormato, wFormato & "," & wDecimal)
    End Function

    Public Sub ValidarDecimal(ByRef e As KeyPressEventArgs)
        If e.KeyChar = Chr(Asc(".")) Then e.KeyChar = Chr(Asc(","))
        ValidarDigitosNegativos(e, True)
    End Sub

    Public Sub ValidarDigitosNegativos(ByRef e As KeyPressEventArgs, Optional ByRef decimales As Boolean = False)
        If Char.IsDigit(e.KeyChar) Then
            e.Handled = False
        ElseIf Char.IsControl(e.KeyChar) Then
            e.Handled = False
        ElseIf e.KeyChar = "-"c Then
            e.Handled = False
        Else
            If e.KeyChar = ","c And decimales Then
                e.Handled = False
                Exit Sub
            End If
            e.Handled = True
        End If
    End Sub

    Public Sub ValidarDigitos(ByRef e As KeyPressEventArgs, Optional ByRef decimales As Boolean = False)
        If Char.IsDigit(e.KeyChar) Then
            e.Handled = False
        ElseIf Char.IsControl(e.KeyChar) Then
            e.Handled = False
        Else
            If e.KeyChar = ","c And decimales Then
                e.Handled = False
                Exit Sub
            End If
            e.Handled = True
        End If
    End Sub

    Public Sub ValidarDigitos(ByRef sender As Object, Optional ByVal posiciones As Integer = 3)
        If sender Is Nothing Then Exit Sub
        Dim tb As TextBox = DirectCast(sender, TextBox)
        tb.Text = Regex.Replace(tb.Text, "[^0-9.,-]", "")
        tb.Text.Trim()
        If tb.Text <> "" AndAlso Not tb.Text = "0" Then
            Dim wValor = tb.Text.ToDecimal(posiciones)
            tb.Text = IIf(wValor = 0, "", wValor).ToString
        End If
    End Sub

    Public Sub ValidarDigitosDecimal(ByRef e As KeyPressEventArgs)

        If Char.IsDigit(e.KeyChar) Then
            e.Handled = False
        ElseIf e.KeyChar = CChar(",") Then
            e.Handled = False
        ElseIf e.KeyChar = CChar(".") Then
            e.KeyChar = CChar(",")
            e.Handled = False
        ElseIf Char.IsControl(e.KeyChar) Then
            e.Handled = False
        Else
            e.Handled = True
        End If

    End Sub

    Public Sub ValidarIngresoDigitosDecimal(ByRef e As KeyPressEventArgs, sender As System.Windows.Forms.Control)

        If Char.IsDigit(e.KeyChar) Then
            e.Handled = False
        ElseIf e.KeyChar = CChar(",") Then
            If sender.Text.Contains(",") Then
                e.Handled = True
            Else
                e.Handled = False
            End If
        ElseIf e.KeyChar = CChar(".") Then
            If sender.Text.Contains(",") Then
                e.Handled = True
            Else
                e.KeyChar = CChar(",")
                e.Handled = False
            End If
        ElseIf Char.IsControl(e.KeyChar) Then
            e.Handled = False
        Else
            e.Handled = True
        End If

    End Sub

    Public Sub ValidarDigitosNetoDoc(ByRef e As KeyPressEventArgs)
        If Char.IsDigit(e.KeyChar) Then
            e.Handled = False
        ElseIf e.KeyChar = CChar(",") Then
            e.Handled = False
        ElseIf e.KeyChar = CChar(".") Then
            e.KeyChar = CChar(",")
            e.Handled = False
        ElseIf e.KeyChar = CChar("*") Then
            e.Handled = False
        ElseIf Char.IsControl(e.KeyChar) Then
            e.Handled = False
        Else
            e.Handled = True
        End If

    End Sub

    <Extension()>
    Public Function Validar(ByRef wValor As Decimal, Optional ByVal wPosicionesDecimales As Integer = 3) As Double
        Try
            wValor = Decimal.Round(wValor, wPosicionesDecimales, MidpointRounding.AwayFromZero)
            If wValor > MaxDouble Then wValor = MaxDouble.ToDecimal
        Catch ex As OverflowException
            wValor = MaxDouble.ToDecimal
        End Try
        Return wValor
    End Function



    Public Sub ValidarDigitosDecimal(ByRef sender As Object, Optional ByVal posiciones As Integer = 3)
        If sender Is Nothing Then Exit Sub
        Dim tb As TextBox = DirectCast(sender, TextBox)
        tb.Text = Regex.Replace(tb.Text, "[^0-9.,-]", "")
        If tb.Text <> "" AndAlso Not tb.Text = "0" Then
            tb.Text = String.Format($"{{0:f{posiciones}}}", tb.Text.ToDecimal(posiciones))
        End If
    End Sub

    Public Function VerificarRut(Rut As String) As Boolean
        Rut = Rut.Replace(",", "").Replace(".", "").Replace(" ", "").Trim()

        If Rut = "" OrElse Not Rut.Contains("-") OrElse Rut.Length < 9 OrElse Rut.Length > 10 Then
            Return False
        End If

        Dim arr As Char() = Rut.Substring(0, Rut.IndexOf("-"c)).ToCharArray()
        Array.Reverse(arr)
        Dim val = New String(arr)
        Dim dig = Rut.Substring(Rut.IndexOf("-"c) + 1, 1).ToUpper()
        Dim mul = 2
        Dim total = 0D

        For i = 0 To val.Length - 1
            total += Convert.ToInt32(val.Substring(i, 1)) * mul
            mul += 1
            If mul = 8 Then
                mul = 2
            End If
        Next

        Dim result = (11 - (total Mod 11)).ToString()
        If result = "10" Then result = "K"
        If result = "11" Then result = "0"

        Return (dig = result)
    End Function

    Public Enum ModoRut
        Normal = 1
        SII = 2
    End Enum


    Public Function FormatoRut(ByVal Rut As String, Optional modo As ModoRut = ModoRut.Normal) As String
        Rut = Rut.Replace(" ", "").Replace("-", "").Replace(".", "")

        If Rut = "" OrElse Rut.Length < 8 Then
            Return ""
        End If

        If Rut.Length > 9 Then
            Return ""
        End If

        Dim n As Decimal = Nothing

        If Not Decimal.TryParse(Rut.Substring(0, Rut.Length - 1), n) Then
            Return ""
        End If

        Rut = Convert.ToDecimal(Rut.Substring(0, Rut.Length - 1)).ToString("#,##0").Replace(",", ".") & "-" & Rut.Substring(Rut.Length - 1)

        If Rut.Length = 9 Then
            Rut = "00." & Rut
        End If

        If Rut.Length < 12 Then
            Rut = Rut.PadLeft(12, "0"c)
        End If

        If modo = ModoRut.SII Then
            Rut = Rut.Replace(".", "")
            If (Rut.Substring(0, 1) = "0") Then
                Rut = Rut.Substring(1, Rut.Length - 1)
            End If
        End If

        Return Rut.ToUpper()
    End Function


    <Extension()>
    Public Function Clean(texto As String, Optional punctuation As Boolean = True) As String
        Dim bytes As Byte() = Encoding.GetEncoding("ISO-8859-8").GetBytes(texto)
        texto = Encoding.UTF8.GetString(bytes)
        texto = Regex.Replace(texto, "[^\u0000-\u007F]+", String.Empty)
        If Not punctuation Then
            texto = texto.Replace(",", ".").Replace(".", "").Replace(";", "").Replace(":", "")
        End If
        Return texto.Trim()
    End Function

    <Extension()>
    Public Function TrimAll(value As String) As String
        If String.IsNullOrEmpty(value) Then
            Return value
        End If

        value = value.Trim()
        value = value.Replace(vbCrLf, "")
        value = value.Replace(vbCr, "")
        value = value.Replace(vbLf, "")

        Dim regex As New System.Text.RegularExpressions.Regex("  +")
        value = regex.Replace(value, " ")

        Return value
    End Function

    <Extension()>
    Public Function RemoveExtraSpaces(text As String) As String
        Dim regex = New Regex("[ ]{2,}", RegexOptions.None)
        Return regex.Replace(text, " ").Replace("'", "").Trim()
    End Function

    Public Function GetGeneratedSql(ByVal cmd As SqlCommand) As String
        Dim result As String = cmd.CommandText.ToString()

        For Each p As SqlParameter In cmd.Parameters
            Dim isQuted As String = If((TypeOf p.Value Is String Or TypeOf p.Value Is DateTime), "'", "")
            If TypeOf p.Value Is DateTime Then
                Dim dt = CType(p.Value, DateTime)
                result = result.Replace(p.ParameterName.ToString() & ",", isQuted & dt.FormatoSeparador("-", FormatoFecha.YYYYMMDDHHMMSS, ":", "T") & isQuted & ",")
                result = result.Replace(p.ParameterName.ToString() & ")", isQuted & dt.FormatoSeparador("-", FormatoFecha.YYYYMMDDHHMMSS, ":", "T") & isQuted & ")")
            Else
                result = result.Replace(p.ParameterName.ToString() & ",", isQuted & p.Value.ToString() & isQuted & ",")
                result = result.Replace(p.ParameterName.ToString() & ")", isQuted & p.Value.ToString() & isQuted & ")")
            End If
        Next

        result = result.Replace(" AS [t0]", "").Replace("[t0].", "").Replace("[", "").Replace("]", "").Replace(vbCrLf, " ")
        'Clipboard.SetText(result)
        Return result
    End Function


    Public Function FirstCharToUpper(ByVal input As String) As String
        If input = "" Then Return ""
        input.Replace("  ", " ")
        input = input.ToLower().Trim()
        Dim words = input.Split(" "c)
        Dim outputList = New List(Of String)()

        For Each word In words
            If word.Trim() = "" Then Continue For

            Select Case word
                Case Nothing
                    Throw New ArgumentNullException(NameOf(word))
                Case ""
                    Throw New ArgumentException($"{NameOf(word)} cannot be empty", NameOf(word))
                Case Else
                    outputList.Add(word.First().ToString().ToUpper() & word.Substring(1))
            End Select
        Next

        Return outputList.Aggregate(Function(x, y) $"{x} {y}")
    End Function

    <Extension()>
    Function StartOfWeek(ByVal dt As DateTime, ByVal start As DayOfWeek) As DateTime
        Dim diff As Integer = (7 + (dt.DayOfWeek - start)) Mod 7
        Return dt.AddDays(-1 * diff).Date
    End Function

    <Extension()>
    Public Function GetAttribute(Of T As Attribute)(ByVal member As MemberInfo, ByVal isRequired As Boolean) As T
        Dim attribute = member.GetCustomAttributes(GetType(T), False).SingleOrDefault()

        If attribute Is Nothing AndAlso isRequired Then
            Throw New ArgumentException(String.Format(CultureInfo.InvariantCulture, "The {0} attribute must be defined on member {1}", GetType(T).Name, member.Name))
        End If

        Return CType(attribute, T)
    End Function

    Public Function GetPropertyInformation(ByVal propertyExpression As Expression) As MemberInfo
        Debug.Assert(propertyExpression IsNot Nothing, "propertyExpression != null")
        Dim memberExpr As MemberExpression = TryCast(propertyExpression, MemberExpression)

        If memberExpr Is Nothing Then
            Dim unaryExpr As UnaryExpression = TryCast(propertyExpression, UnaryExpression)

            If unaryExpr IsNot Nothing AndAlso unaryExpr.NodeType = ExpressionType.Convert Then
                memberExpr = TryCast(unaryExpr.Operand, MemberExpression)
            End If
        End If

        If memberExpr IsNot Nothing AndAlso memberExpr.Member.MemberType = MemberTypes.[Property] Then
            Return memberExpr.Member
        End If

        Return Nothing
    End Function

    Public Function IdConstDesc(Of T)(desc As String) As String
        Dim fieldInfo = GetType(T).GetFields().ToList()
        For Each item In fieldInfo
            Dim attr As DescriptionAttribute = CType(System.Attribute.GetCustomAttribute(item,
                                    GetType(DescriptionAttribute)), DescriptionAttribute)
            If attr.Description = desc Then
                Return item.GetRawConstantValue.ToString()
            End If
        Next
        Return ""
    End Function

    Public Function DescConstId(Of T)(id As String) As String
        Dim fieldInfo = GetType(T).GetFields().ToList()
        For Each item In fieldInfo

            Dim attr As DescriptionAttribute = CType(System.Attribute.GetCustomAttribute(item,
                                    GetType(DescriptionAttribute)), DescriptionAttribute)
            If item.GetRawConstantValue.ToString() = id Then
                Return attr.Description
            End If
        Next
        Return ""
    End Function

    Public Function GetConstValues(Of T)() As List(Of String)
        Return GetType(T).
        GetFields(BindingFlags.Public Or BindingFlags.Static Or BindingFlags.FlattenHierarchy).
        Where(Function(f) f.IsLiteral AndAlso Not f.IsInitOnly AndAlso f.FieldType Is GetType(String)).
        Select(Function(f) CStr(f.GetRawConstantValue())).
        ToList()
    End Function

    Public Function GetPropertyDescription(Of T)(fieldName As String) As String
        Dim result As String
        Dim fi As PropertyInfo = GetType(T).GetProperty(fieldName.ToString())
        If fi IsNot Nothing Then
            Try
                Dim descriptionAttrs As Object() = fi.GetCustomAttributes(GetType(DescriptionAttribute), False)
                Dim description As DescriptionAttribute = CType(descriptionAttrs(0), DescriptionAttribute)
                result = description.Description
            Catch
                result = Nothing
            End Try
        Else
            result = Nothing
        End If
        Return result
    End Function


    Public Function GetPropertyDisplayName(Of T)(ByVal fieldName As String) As String
        Dim result As String
        Dim fi As PropertyInfo = GetType(T).GetProperty(fieldName.ToString())
        If fi IsNot Nothing Then
            Try
                Dim descriptionAttrs As Object() = fi.GetCustomAttributes(GetType(DisplayNameAttribute), False)
                Dim description As DisplayNameAttribute = CType(descriptionAttrs(0), DisplayNameAttribute)
                result = description.DisplayName
            Catch
                result = Nothing
            End Try
        Else
            result = Nothing
        End If
        Return result
    End Function


    Public Function RutVacio(wRut As String) As Boolean
        Return wRut.Replace(".", "").Replace("-", "").Replace(" ", "") = ""
    End Function

    <Extension()>
    Public Function ToClipboard(ByRef xTabla As DataGridView) As Boolean
        Try
            If xTabla.Rows.Count = 0 Then Return False
            xTabla.SuspendLayout()
            xTabla.RowHeadersVisible = False
            If xTabla.SelectedRows.Count = 0 Then xTabla.SelectAll()
            Clipboard.Clear()
            Clipboard.SetDataObject(xTabla.GetClipboardContent())
            xTabla.ClearSelection()
        Catch ex As Exception
            MsgBox(ex.Message)
        Finally
            xTabla.RowHeadersVisible = True
            xTabla.ResumeLayout()
        End Try
        Return True
    End Function

    <Extension()>
    Public Function CloneObject(Of T As Class)(ByVal obj As T) As T
        If obj Is Nothing Then Return Nothing
        Dim inst As System.Reflection.MethodInfo = obj.[GetType]().GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance Or System.Reflection.BindingFlags.NonPublic)

        If inst IsNot Nothing Then
            Return CType(inst.Invoke(obj, Nothing), T)
        Else
            Return Nothing
        End If
    End Function

    <Extension()>
    Public Function GetDescription(ByVal GenericEnum As [Enum]) As String
        Dim genericEnumType As Type = GenericEnum.[GetType]()
        Dim memberInfo As MemberInfo() = genericEnumType.GetMember(GenericEnum.ToString())

        If (memberInfo IsNot Nothing AndAlso memberInfo.Length > 0) Then
            Dim _Attribs = memberInfo(0).GetCustomAttributes(GetType(System.ComponentModel.DescriptionAttribute), False)

            If (_Attribs IsNot Nothing AndAlso _Attribs.Count() > 0) Then
                Return (CType(_Attribs.ElementAt(0), System.ComponentModel.DescriptionAttribute)).Description
            End If
        End If

        Return GenericEnum.ToString()
    End Function

    <Extension()>
    Public Function IndexOf(Of TSource)(ByVal source As IEnumerable(Of TSource), ByVal predicate As Func(Of TSource, Boolean)) As Integer
        Dim index = 0

        For Each item In source

            If predicate.Invoke(item) Then
                Return index
            End If

            index += 1
        Next

        Return -1
    End Function

    Public Function ObtenerValorBoolean(texto As String) As Boolean
        If texto <> "" Then
            If texto.ToLower.RemoverTildes = "si" Then
                texto = "1"
            End If
            If texto.ToLower.RemoverTildes = "true" Then
                texto = "1"
            End If
            If texto.ToLower.RemoverTildes = "no" Then
                texto = "0"
            End If
            If texto.ToLower.RemoverTildes = "false" Then
                texto = "0"
            End If
            If IsNumeric(texto) Then
                If texto.ToDecimal > 1 Then
                    texto = "0"
                End If
            End If
        Else
            texto = "0"
        End If
        Return texto.ToDecimal = 1
    End Function

End Module

Module DateTimeExtensions
    Public gc As GregorianCalendar = New GregorianCalendar()

    <Extension()>
    Function GetWeekOfMonth(ByVal time As DateTime) As Integer
        Dim first As DateTime = New DateTime(time.Year, time.Month, 1)
        Return time.GetWeekOfYear() - first.GetWeekOfYear() + 1
    End Function

    <Extension()>
    Function GetLetterWeekOfMonth(ByVal time As DateTime) As String
        Select Case (time.GetWeekOfMonth())
            Case 1
                Return "A"
            Case 2
                Return "B"
            Case 3
                Return "C"
            Case 4
                Return "D"
            Case 5
                Return "E"
            Case Else
                Return ""
        End Select
    End Function

    <Extension()>
    Private Function GetWeekOfYear(ByVal time As DateTime) As Integer
        Return gc.GetWeekOfYear(time, CalendarWeekRule.FirstDay, DayOfWeek.Monday)
    End Function


    ''' <summary>
    ''' Sets the value to 01-01-2000
    ''' </summary>
    ''' <param name="Fecha"></param>
    <Extension()>
    Public Sub Clear(ByRef Fecha As DateTime)
        Fecha = New DateTime(2000, 1, 1)
    End Sub
    ''' <summary>
    ''' Checks if the value is 01-01-2000
    ''' </summary>
    ''' <param name="Fecha"></param>
    <Extension()>
    Public Function IsClear(Fecha As DateTime) As Boolean
        Return (Fecha = New DateTime(2000, 1, 1))
    End Function
    <Extension()>
    Public Function FechaHora(ByVal Fecha As DateTime) As DateTime
        Return New DateTime(Fecha.Year, Fecha.Month, Fecha.Day, Fecha.Hour, Fecha.Minute, 0, 0)
    End Function



    Public Enum FormatoFecha
        DDMMYYYY
        YYYYMMDD
        MMDDYYYY
        DDMMYYYYHHMM
        YYYYMMDDHHMM
        YYYYMMDDHHMMSS
        YYYYMMDDHHMMSSMMM
        HHMM
        HHMMSS
    End Enum

    <Extension()>
    Public Function FormatoSeparador(ByVal Fecha As DateTime, ByVal separador As String, ByVal Optional Formato As FormatoFecha = FormatoFecha.DDMMYYYY, ByVal Optional separadorHora As String = ":", ByVal Optional espacioFechaHora As String = " ") As String
        Select Case Formato
            Case FormatoFecha.HHMM
                Return Fecha.Hour.ToString().PadLeft(2, "0"c) & separadorHora + Fecha.Minute.ToString().PadLeft(2, "0"c)
            Case FormatoFecha.HHMMSS
                Return Fecha.Hour.ToString().PadLeft(2, "0"c) & separadorHora + Fecha.Minute.ToString().PadLeft(2, "0"c) & separadorHora + Fecha.Second.ToString().PadLeft(2, "0"c)
            Case FormatoFecha.DDMMYYYY
                Return Fecha.Day.ToString().PadLeft(2, "0"c) & separador + Fecha.Month.ToString().PadLeft(2, "0"c) & separador + Fecha.Year.ToString()
            Case FormatoFecha.YYYYMMDD
                Return Fecha.Year.ToString() & separador + Fecha.Month.ToString().PadLeft(2, "0"c) & separador + Fecha.Day.ToString().PadLeft(2, "0"c)
            Case FormatoFecha.MMDDYYYY
                Return Fecha.Month.ToString().PadLeft(2, "0"c) & separador + Fecha.Day.ToString().PadLeft(2, "0"c) & separador + Fecha.Year.ToString()
            Case FormatoFecha.YYYYMMDDHHMMSS
                Return Fecha.Year.ToString() & separador +
                    Fecha.Month.ToString().PadLeft(2, "0"c) &
                    separador &
                    Fecha.Day.ToString().PadLeft(2, "0"c) &
                    espacioFechaHora &
                    Fecha.Hour.ToString().PadLeft(2, "0"c) &
                    separadorHora &
                    Fecha.Minute.ToString().PadLeft(2, "0"c) &
                    separadorHora &
                    Fecha.Second.ToString().PadLeft(2, "0"c)
            Case FormatoFecha.YYYYMMDDHHMMSSMMM
                Thread.Sleep(1)
                Return Fecha.Year.ToString() & separador &
                    Fecha.Month.ToString().PadLeft(2, "0"c) &
                    separador &
                    Fecha.Day.ToString().PadLeft(2, "0"c) &
                    espacioFechaHora & Fecha.Hour.ToString().PadLeft(2, "0"c) &
                    separadorHora & Fecha.Minute.ToString().PadLeft(2, "0"c) &
                    separadorHora & Fecha.Second.ToString().PadLeft(2, "0"c) &
                    Fecha.Millisecond.ToString.PadLeft(3, "0"c)
            Case FormatoFecha.YYYYMMDDHHMM
                Return Fecha.Year.ToString() & separador &
                    Fecha.Month.ToString().PadLeft(2, "0"c) &
                    separador &
                    Fecha.Day.ToString().PadLeft(2, "0"c) &
                    espacioFechaHora &
                    Fecha.Hour.ToString().PadLeft(2, "0"c) &
                    separadorHora &
                    Fecha.Minute.ToString().PadLeft(2, "0"c)
            Case FormatoFecha.DDMMYYYYHHMM
                Return Fecha.Day.ToString().PadLeft(2, "0"c) &
                    separador &
                    Fecha.Month.ToString().PadLeft(2, "0"c) &
                    separador & Fecha.Year.ToString() &
                    espacioFechaHora &
                    Fecha.Hour.ToString().PadLeft(2, "0"c) &
                    separadorHora &
                    Fecha.Minute.ToString().PadLeft(2, "0"c)
            Case Else
                Return Fecha.Day.ToString().PadLeft(2, "0"c) &
                    separador &
                    Fecha.Month.ToString().PadLeft(2, "0"c) &
                    separador &
                    Fecha.Year.ToString()
        End Select
    End Function



End Module
Public Module PredicateBuilder
    Public Function [True](Of T)() As Expression(Of Func(Of T, Boolean))
        Return Function(f) True
    End Function

    Public Function [False](Of T)() As Expression(Of Func(Of T, Boolean))
        Return Function(f) False
    End Function

    <Extension()>
    Public Function [Or](Of T)(ByVal expr1 As Expression(Of Func(Of T, Boolean)), ByVal expr2 As Expression(Of Func(Of T, Boolean))) As Expression(Of Func(Of T, Boolean))
        Dim invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast(Of Expression)())
        Return Expression.Lambda(Of Func(Of T, Boolean))(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters)
    End Function

    <Extension()>
    Public Function [And](Of T)(ByVal expr1 As Expression(Of Func(Of T, Boolean)), ByVal expr2 As Expression(Of Func(Of T, Boolean))) As Expression(Of Func(Of T, Boolean))
        Dim invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast(Of Expression)())
        Return Expression.Lambda(Of Func(Of T, Boolean))(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters)
    End Function
End Module


