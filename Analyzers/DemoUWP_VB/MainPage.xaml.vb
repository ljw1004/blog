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

    Private Async Sub button10586_Click(sender As Object, e As RoutedEventArgs) Handles button10586.Click
        Dim holo = Windows.Graphics.Holographic.HolographicSpace.CreateForCoreWindow(Windows.UI.Core.CoreWindow.GetForCurrentThread())
        Dim folder = Await Windows.Storage.DownloadsFolder.CreateFileForUserAsync(Nothing, "download.png")
        Windows.ApplicationModel.Store.Preview.StoreConfiguration.PurchasePromptingPolicy = Nothing
        Dim space = Await Windows.Perception.Spatial.SpatialAnchorManager.RequestStoreAsync()
        Dim gesture As New Windows.UI.Input.Spatial.SpatialGestureRecognizer(Windows.UI.Input.Spatial.SpatialGestureSettings.Tap)
        Dim telemetry = Windows.System.Profile.PlatformDiagnosticsAndUsageDataSettings.CanCollectDiagnostics(Windows.System.Profile.PlatformDataCollectionLevel.Enhanced)
        Dim hook = Windows.UI.Input.KeyboardDeliveryInterceptor.GetForCurrentView()
        Dim jumps = Windows.UI.StartScreen.JumpList.IsSupported()
        Dim size = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().DiagonalSizeInInches
    End Sub
End Class

