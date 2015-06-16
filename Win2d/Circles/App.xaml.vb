NotInheritable Class App
    Inherits Application

    Protected Overrides Sub OnLaunched(e As LaunchActivatedEventArgs)
        Dim rootFrame = TryCast(Window.Current.Content, Frame)
        If rootFrame Is Nothing Then rootFrame = New Frame() : Window.Current.Content = rootFrame
        If rootFrame.Content Is Nothing Then rootFrame.Navigate(GetType(MainPage), e.Arguments)
        Window.Current.Activate()
    End Sub

End Class
