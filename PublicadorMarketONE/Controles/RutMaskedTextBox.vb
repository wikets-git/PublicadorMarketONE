Imports System.Globalization

Public Class RutMaskedTextBox
    Inherits MaskedTextBox

    Public Sub New()
        MyBase.Culture = New CultureInfo("en-US", False)
        MyBase.Mask = "##\.###\.###\-A"
    End Sub

    Public Shadows Property Mask As String
        Get
            Return MyBase.Mask
        End Get
        Set(value As String)
            MyBase.Culture = New CultureInfo("en-US", False)
            MyBase.Mask = "##\.###\.###\-A"
        End Set
    End Property

    Public Shadows Property Culture As CultureInfo
        Get
            Return MyBase.Culture
        End Get
        Set(value As CultureInfo)
            MyBase.Culture = New CultureInfo("en-US", False)
        End Set
    End Property

End Class
