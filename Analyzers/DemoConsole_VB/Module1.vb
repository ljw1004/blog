Imports System.Runtime.CompilerServices

Module Module1
    Sub Main()
        f()
    End Sub

    Event e As Action

    Dim b As Boolean = False

    Property q As Integer = Windows.Stuff.Dummy.dummy2()

    Property p As Integer
        Get
            Windows.Stuff.Dummy.dummy2()
            Return 1
        End Get
        Set(value As Integer)
            Windows.Stuff.Dummy.dummy2()
        End Set
    End Property

    Sub f(x As Integer)
    End Sub

    Function f() As Integer
        If Windows.Foundation.Metadata.ApiInformation.IsTypePresent("a") Then
            Windows.Stuff.Dummy.dummy2()
        End If
        If b Then
            Windows.Stuff.Dummy.dummy2()
        End If
        Dim x As New C
        Return 1
    End Function

    Public Property c1 As C
        Get
            Return New C
        End Get
        Set(value As C)
        End Set
    End Property

End Module

Class C
    Function g() As Integer
        If Windows.Foundation.Metadata.ApiInformation.IsTypePresent("a") Then
            Windows.Stuff.Dummy.dummy2()
        End If
        Windows.Stuff.Dummy.dummy2()
        Return 1
    End Function

    Sub New()
        Windows.Stuff.Dummy.dummy2()
    End Sub
End Class

Namespace Global.Windows.Stuff
    Public Class Dummy
        Sub New()
        End Sub
        Public Sub dummy1()
        End Sub
        Public Shared Function dummy2() As Integer
            Return 1
        End Function
    End Class
End Namespace

Namespace Global.Windows.Foundation.Metadata
    Public Class ApiInformation
        Public Shared Function IsTypePresent(s As String) As Boolean
            Return True
        End Function
    End Class
End Namespace
