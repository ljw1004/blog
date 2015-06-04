Imports System.Runtime.CompilerServices

Module Module1
    Sub Main()
        f()
    End Sub

    Dim b As Boolean = False

    Sub f()
        If Windows.Foundation.Metadata.ApiInformation.IsTypePresent("a") Then
            Windows.Stuff.Dummy.dummy2()
        End If
        If b Then
            Windows.Stuff.Dummy.dummy2()
        End If
    End Sub
End Module

Class C
    Sub g()
        If Windows.Foundation.Metadata.ApiInformation.IsTypePresent("a") Then
            Windows.Stuff.Dummy.dummy2()
        End If
        Windows.Stuff.Dummy.dummy2()
    End Sub

End Class

Namespace Global.Windows.Stuff
    Public Class Dummy
        Public Sub dummy1()
        End Sub
        Public Shared Sub dummy2()
        End Sub
    End Class
End Namespace

Namespace Global.Windows.Foundation.Metadata
    Public Class ApiInformation
        Public Shared Function IsTypePresent(s As String) As Boolean
            Return True
        End Function
    End Class
End Namespace