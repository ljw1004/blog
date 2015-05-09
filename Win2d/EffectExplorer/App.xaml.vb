Imports Newtonsoft.Json
Imports Windows.Storage
Imports Windows.UI

NotInheritable Class App
    Inherits Application

    Public Shared Shadows ReadOnly Property Current As App
        Get
            Return CType(Application.Current, App)
        End Get
    End Property

    Public Shared ReadOnly CWIDTH As Integer = 8
    Public Shared ReadOnly CHEIGHT As Integer = 8
    Public Shared Property model As EffectsModel
    Public Shared Property pixels As Color()

    Protected Overrides Sub OnLaunched(e As LaunchActivatedEventArgs)
        Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)
        If rootFrame Is Nothing Then
            rootFrame = New Frame()
            '
            Dim json = CStr(ApplicationData.Current.LocalSettings.Values("model"))
            If json IsNot Nothing Then model = JsonConvert.DeserializeObject(Of EffectsModel)(json)
            If model Is Nothing Then model = New EffectsModel
            json = CStr(ApplicationData.Current.LocalSettings.Values("pixels"))
            If json IsNot Nothing Then pixels = JsonConvert.DeserializeObject(Of Color())(json)
            If pixels Is Nothing Then pixels = Enumerable.Repeat(Colors.Black, CWIDTH * CHEIGHT).ToArray()
            '
            Window.Current.Content = rootFrame
        End If
        If rootFrame.Content Is Nothing Then rootFrame.Navigate(GetType(MainPage), e.Arguments)
        Window.Current.Activate()
    End Sub

    Private Sub OnSuspending(sender As Object, e As SuspendingEventArgs) Handles Me.Suspending
        Dim deferral As SuspendingDeferral = e.SuspendingOperation.GetDeferral()
        ApplicationData.Current.LocalSettings.Values("model") = JsonConvert.SerializeObject(model)
        ApplicationData.Current.LocalSettings.Values("pixels") = JsonConvert.SerializeObject(pixels)
        deferral.Complete()
    End Sub

End Class

Public Class EffectsModel
    Public ConvolveMatrixEnabled As Boolean = True
    Public ConvolveMatrixKernel As Single() = {0, 1, 0, 0, 2, 0, 0, 0, 0}
    Public ConvolveMatrixDivisor As Integer = 3
    Public DiscreteTransferEnabled As Boolean = False
    Public DiscreteTransferTableRed As Single() = {0, 0.1, 0.9, 1.0}
    Public DiscreteTransferTableGreen As Single() = {0, 0.1, 0.9, 1.0}
    Public DiscreteTransferTableBlue As Single() = {0, 0.1, 0.9, 1.0}
End Class
