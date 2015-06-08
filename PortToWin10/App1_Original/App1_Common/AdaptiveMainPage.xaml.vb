Imports System.Threading.Tasks
Imports Windows.UI.Xaml.Controls
Imports Windows.UI.Xaml.Input
Imports Windows.UI.Xaml.Media.Imaging
Imports Windows.UI.Xaml.Navigation
Imports Windows.UI.Xaml

Public NotInheritable Class AdaptiveMainPage
    Inherits Page

    Dim isOpen As Boolean = False

    Protected Overrides Async Sub OnNavigatedTo(e As NavigationEventArgs)
        MyBase.OnNavigatedTo(e)
        UpdateVisibility()
        App.Platform.SetStatusbarVisibilityAsync(False).FireAndForget()
        isOpen = True
        Dim img = ""
        While isOpen
            App.Tick(border1.Width - image1.Width, border1.Height - image1.Height)
            Canvas.SetLeft(image1, App.ix)
            Canvas.SetTop(image1, App.iy)
            rotation1.RotationZ = App.rz
            Dim new_img = If(App.useBeachBall, "ms-appx:///App1_Common/Assets/beach-ball.png",
                                               "ms-appx:///App1_Common/Assets/twelve-ball.png")
            If img <> new_img Then img = new_img : image1.Source = New BitmapImage(New Uri(img, UriKind.Absolute))

            Await Task.Delay(30)
        End While
    End Sub

    Protected Overrides Sub OnNavigatingFrom(e As NavigatingCancelEventArgs)
        MyBase.OnNavigatingFrom(e)
        isOpen = False
    End Sub

    Protected Overrides Sub OnPointerPressed(e As PointerRoutedEventArgs)
        MyBase.OnPointerPressed(e)
        App.useBeachBall = Not App.useBeachBall
    End Sub

    Private Sub Page_SizeChanged(sender As Object, e As Windows.UI.Xaml.SizeChangedEventArgs)
        Dim w = e.NewSize.Width, h = e.NewSize.Height
        If w > h Then w = h Else h = w
        ' picks the largest square that will fit inside the shape of the current window
        border1.Width = w : border1.Height = h
        image1.Width = w / 8 : image1.Height = h / 8
    End Sub

    Private Async Sub purchase1_Click(sender As Object, e As RoutedEventArgs) Handles purchase1.Click
        Await App.PurchaseSpeedBoostAsync()
        UpdateVisibility()
    End Sub

    Sub UpdateVisibility()
        purchase1.Visibility = If(App.HasSpeedBoost(), Windows.UI.Xaml.Visibility.Collapsed, Windows.UI.Xaml.Visibility.Visible)
    End Sub
End Class
