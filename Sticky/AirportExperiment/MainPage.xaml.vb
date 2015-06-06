Public NotInheritable Class MainPage
    Inherits Page

    Private Sub BtnExit_Click(sender As Object, e As RoutedEventArgs) Handles BtnExit.Click
        Application.Current.Exit()
    End Sub

    Private Async Sub BtnConvert_Click(sender As Object, e As RoutedEventArgs) Handles BtnConvert.Click
        Dim fn = Await ConvertWav.TestFromUWPAsync()
        TextBox1.Text = fn
    End Sub

    Private Async Sub BtnPlay_Click(sender As Object, e As RoutedEventArgs) Handles BtnPlay.Click
        TextBox1.Text = ""
        Dim p As New Progress(Of String)(Sub(s) TextBox1.Text &= s & vbCrLf)
        Await Play.TestFromUWPAsync(p)
    End Sub

    Private Async Sub BtnDetect_Click(sender As Object, e As RoutedEventArgs) Handles BtnDetect.Click
        TextBox1.Text = ""
        Dim p As New Progress(Of String)(Sub(s) TextBox1.Text &= s & vbCrLf)
        Await Mdns.TestMulticastFromUWPAsync(p)
        Await Mdns.TestUnicastFromUWPAsync(p)
    End Sub

    Private Async Sub BtnStream_Click(sender As Object, e As RoutedEventArgs) Handles BtnStream.Click
        TextBox1.Text = ""
        Dim p As New Progress(Of String)(Sub(s) TextBox1.Text &= s & vbCrLf)
        Await Raop.TestFromUWPAsync(p)
    End Sub

End Class
