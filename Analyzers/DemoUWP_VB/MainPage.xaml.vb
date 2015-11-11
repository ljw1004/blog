Imports Windows.Media.Devices
Imports Windows.System.UserProfile

Public NotInheritable Class MainPage
    Inherits Page

    Async Sub MobileDesktop_Click(sender As Object, e As RoutedEventArgs) Handles buttonMobileDesktop.Click
        label1.Text = $"Name: {Await UserInformation.GetDisplayNameAsync()}"

        Dim imageFile = UserInformation.GetAccountPicture(AccountPictureKind.LargeImage)
        If imageFile IsNot Nothing Then
            Using stream = Await imageFile.OpenReadAsync()
                Dim bi As New BitmapImage
                Await bi.SetSourceAsync(stream)
                image1.Source = bi
            End Using
        End If
    End Sub

    Async Sub Mobile_Click(sender As Object, e As RoutedEventArgs) Handles buttonMobile.Click
        Await StatusBar.GetForCurrentView().HideAsync()
    End Sub

    Sub Desktop_Click(sender As Object, e As RoutedEventArgs) Handles buttonDesktop.Click
        label1.Text = $"Ringer: {CallControl.GetDefault()?.HasRinger}"
    End Sub

End Class

