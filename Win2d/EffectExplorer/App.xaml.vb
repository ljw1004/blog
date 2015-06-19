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

    Public Event Loaded As Action

    Public Shared ReadOnly SIMULATION_SIZE As Integer = 8
    Public Shared Property model As EffectsModel
    Public Shared Property pixels As ColorF()

    Protected Overrides Sub OnLaunched(e As LaunchActivatedEventArgs)
        ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow)
        If Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar") Then
            StatusBar.GetForCurrentView().HideAsync().FireAndForget()
        End If


        Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)
        If rootFrame Is Nothing Then
            rootFrame = New Frame()
            '
            Dim loadTask = Async Function()
                               model = Await LoadJsonAsync(Of EffectsModel)("model.json").ExceptionsToNull
                               If model Is Nothing Then model = New EffectsModel
                               pixels = Await LoadJsonAsync(Of ColorF())("pixels.json").ExceptionsToNull
                               If pixels Is Nothing Then pixels = Enumerable.Repeat(ColorF.Black, SIMULATION_SIZE * SIMULATION_SIZE).ToArray()
                               For i = 0 To pixels.Length - 1
                                   pixels(i).A = 1
                               Next
                               RaiseEvent Loaded()
                           End Function()
            loadTask.FireAndForget()
            '
            Window.Current.Content = rootFrame
        End If
        If rootFrame.Content Is Nothing Then rootFrame.Navigate(GetType(MainPage), e.Arguments)
        Window.Current.Activate()
    End Sub

    Private Async Sub OnSuspending(sender As Object, e As SuspendingEventArgs) Handles Me.Suspending
        Dim deferral As SuspendingDeferral = e.SuspendingOperation.GetDeferral()
        Await SaveJsonAsync("model.json", model)
        Await SaveJsonAsync("pixels.json", pixels)
        deferral.Complete()
    End Sub

    Async Function SaveJsonAsync(name As String, o As Object) As Task
        Dim file = Await ApplicationData.Current.LocalFolder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting)
        Dim s = JsonConvert.SerializeObject(o)
        Using stream = Await file.OpenStreamForWriteAsync(), writer As New StreamWriter(stream)
            Await writer.WriteAsync(s)
        End Using
    End Function

    Async Function LoadJsonAsync(Of T)(name As String) As Task(Of T)
        Dim file = Await ApplicationData.Current.LocalFolder.GetFileAsync(name)
        If file Is Nothing Then Return Nothing
        Using stream = Await file.OpenStreamForReadAsync(), reader As New StreamReader(stream)
            Dim s = Await reader.ReadToEndAsync()
            Return JsonConvert.DeserializeObject(Of T)(s)
        End Using
    End Function

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
