Imports App1_Common

NotInheritable Class App
    Inherits Application
    Implements App1_Common.IPlatformAbstraction

    Protected Overrides Sub OnLaunched(e As LaunchActivatedEventArgs)
        ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow)
        App1_Common.App.Platform = Me
        App1_Common.App.OnLaunched(e)
    End Sub

    Sub OnResuming(sender As Object, e As Object) Handles Me.Resuming
        App1_Common.App.OnResuming()
    End Sub

    Private Async Sub OnSuspending(sender As Object, e As SuspendingEventArgs) Handles Me.Suspending
        Dim deferral As SuspendingDeferral = e.SuspendingOperation.GetDeferral()
        Await App1_Common.App.OnSuspendingAsync()
        deferral.Complete()
    End Sub

    Public Async Function SetStatusbarVisibilityAsync(isVisible As Boolean) As Task Implements IPlatformAbstraction.SetStatusbarVisibilityAsync
        If isVisible Then
            Await StatusBar.GetForCurrentView().ShowAsync()
        Else
            Await StatusBar.GetForCurrentView().HideAsync()
        End If
    End Function
End Class
