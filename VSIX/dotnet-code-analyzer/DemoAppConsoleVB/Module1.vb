Imports System.ComponentModel
Imports System.Net.Http

Module Module1
    Sub Main()
        Dim s1 = "abc"
        Dim x1 = From c1 In s1



    End Sub
End Module

Class Test
    Async Function t1() As Task
    End Function

    Async Function t2() As Task
    End Function

    Async Function t3() As Task
        Dim http As New HttpClient
    End Function
End Class

Class C : Implements INotifyPropertyChanged

    Sub p()
        Dim x = New PropertyChangedEventArgs("x=new")
        Dim y As New PropertyChangedEventArgs("y as new")
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("sub"))
    End Sub

    Function f1() As Integer
        Dim x = New PropertyChangedEventArgs("x=new")
        Return 0
    End Function

    Private x1 As PropertyChangedEventArgs = New PropertyChangedEventArgs("field")
    Private Shared x2 As PropertyChangedEventArgs = New PropertyChangedEventArgs("field-shared")
    Property x3 As PropertyChangedEventArgs = New PropertyChangedEventArgs("autoprop")
    Shared Property x4 As PropertyChangedEventArgs = New PropertyChangedEventArgs("autoprop-shared")
    Property x5 As Integer
        Get
            Dim t1 = New PropertyChangedEventArgs("getter")
            Return 1
        End Get
        Set(value As Integer)
            Dim t2 = New PropertyChangedEventArgs("setter")
        End Set
    End Property
    Shared Property x6 As Integer
        Get
            Dim t1 = New PropertyChangedEventArgs("shared-getter")
            Return 1
        End Get
        Set(value As Integer)
            Dim t2 = New PropertyChangedEventArgs("shared-setter")
        End Set
    End Property

    Private Shared _test As New PropertyChangedEventArgs("already-shared-field")

    Shared Sub q()
        RaiseEvent PropertyChanged2(Nothing, New PropertyChangedEventArgs("shared-sub"))
    End Sub

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    Public Shared Event PropertyChanged2 As PropertyChangedEventHandler
End Class

Module M
    Sub p()
        RaiseEvent PropertyChanged(Nothing, New PropertyChangedEventArgs("module-sub"))
    End Sub
    Public Event PropertyChanged As PropertyChangedEventHandler
End Module

Namespace NS1
    Class B
        Sub g() : End Sub
    End Class
    Class C(Of T) : Inherits B
        Sub f(Of U)(arg As Integer)
            Console.WriteLine("NS1") ' Not implemented
            Console.WriteLine("T") ' Not implemented
            Console.WriteLine("U") ' Not implemented
            Console.WriteLine("C") ' Not implemented
            Console.WriteLine("New") ' Not implemented
            Console.WriteLine("g") ' Not implemented
            Console.WriteLine("f")
            Console.WriteLine("arg")
            Console.WriteLine("p")
        End Sub
        Property p As Integer
        Sub New() : End Sub
    End Class

    Structure S
        Sub f(arg As Integer)
            Console.WriteLine("S") ' Not implemented
            Console.WriteLine("f")
            Console.WriteLine("arg")
            Console.WriteLine("p")
        End Sub
        Public Property p As Integer
    End Structure

    Module M
        Sub f(arg As Integer)
            Console.WriteLine("M") ' Not implemented
            Console.WriteLine("f")
            Console.WriteLine("arg")
            Console.WriteLine("p")
        End Sub
        Public Property p As Integer
    End Module

End Namespace
