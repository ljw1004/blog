Imports Windows.UI

NotInheritable Class App
    Inherits Application

    Protected Overrides Sub OnLaunched(e As LaunchActivatedEventArgs)
        ApplicationView.GetForCurrentView().SetPreferredMinSize(New Size(100, 100))
        ApplicationView.GetForCurrentView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow)
        If Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar") Then
            StatusBar.GetForCurrentView().ForegroundColor = Colors.Red
        End If



        Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)
        If rootFrame Is Nothing Then
            rootFrame = New Frame()
            ' If e.PreviousExecutionState = ApplicationExecutionState.Terminated Then TODO : Load state from previously suspended application
            Window.Current.Content = rootFrame
        End If
        If rootFrame.Content Is Nothing Then rootFrame.Navigate(GetType(MainPage), e.Arguments)
        ApplicationView.GetForCurrentView().SetPreferredMinSize(New Size(200, 100))
        Window.Current.Activate()
    End Sub

End Class
