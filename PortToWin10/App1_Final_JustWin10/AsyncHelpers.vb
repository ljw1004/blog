Imports System.Runtime.CompilerServices
Imports System.Threading.Tasks
Imports Windows.Foundation

Module AsyncHelpers
    <Extension> Async Sub FireAndForget(task As Task)
        Try
            Await task
        Catch ex As Exception
            App.SendErrorReport(ex)
        End Try
    End Sub

    <Extension> Async Sub FireAndForget(task As IAsyncAction)
        Try
            Await task
        Catch ex As Exception
            App.SendErrorReport(ex)
        End Try
    End Sub

    <Extension> Async Sub FireAndForget(Of T)(task As IAsyncOperation(Of T))
        Try
            Await task
        Catch ex As Exception
            App.SendErrorReport(ex)
        End Try
    End Sub
End Module
