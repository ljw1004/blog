Imports Windows.Storage
Imports Windows.UI

NotInheritable Class App
    Inherits Application

    Public Const CHEIGHT = 480
    Public Const CWIDTH = 640
    Public Property Pixels As Byte()

    Public Event Loaded As Action
    Public Event Unloading As Action

    Public Shared Shadows ReadOnly Property Current As App
        Get
            Return CType(Application.Current, App)
        End Get
    End Property

    Protected Overrides Sub OnLaunched(e As Windows.ApplicationModel.Activation.LaunchActivatedEventArgs)
        Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)
        If rootFrame Is Nothing Then
            rootFrame = New Frame()
            Window.Current.Content = rootFrame
            pixels = New Byte(CHEIGHT * CWIDTH - 1) {}
            LoadAsync().FireAndForget()
        End If
        If rootFrame.Content Is Nothing Then rootFrame.Navigate(GetType(MainPage), e.Arguments)
        Window.Current.Activate()
    End Sub

    Async Function LoadAsync() As Task
        Try
            Dim file = Await ApplicationData.Current.LocalFolder.GetFileAsync($"pixels_{CWIDTH}x{CHEIGHT}.dat")
            Using stream = Await file.OpenStreamForReadAsync()
                Dim red = Await stream.ReadAsync(pixels, 0, pixels.Length)
                If red <> pixels.Length Then Stop
            End Using
        Catch ex As FileNotFoundException
        End Try
        RaiseEvent Loaded()
    End Function

    Private Async Sub OnSuspending(sender As Object, e As SuspendingEventArgs) Handles Me.Suspending
        Dim deferral = e.SuspendingOperation.GetDeferral()
        RaiseEvent Unloading()
        Dim file = Await ApplicationData.Current.LocalFolder.CreateFileAsync($"pixels_{CWIDTH}x{CHEIGHT}.dat", CreationCollisionOption.ReplaceExisting)
        Using stream = Await file.OpenStreamForWriteAsync()
            Await stream.WriteAsync(pixels, 0, pixels.Count)
        End Using
        deferral.Complete()
    End Sub


End Class

