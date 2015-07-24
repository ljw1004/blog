Imports Windows.UI

NotInheritable Class App
    Inherits Application

    Protected Overrides Sub OnLaunched(e As LaunchActivatedEventArgs)
        ApplicationView.GetForCurrentView().SetPreferredMinSize(New Size(150, 250))
        ApplicationView.GetForCurrentView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow)
        If Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar") Then
            StatusBar.GetForCurrentView().ForegroundColor = Colors.Black
        End If

        Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)
        If rootFrame Is Nothing Then rootFrame = New Frame() : Window.Current.Content = rootFrame
        If rootFrame.Content Is Nothing Then rootFrame.Navigate(GetType(MainPageCPU), e.Arguments)
        Window.Current.Activate()
    End Sub

End Class
