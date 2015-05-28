Imports Windows.Storage
Imports Windows.UI

NotInheritable Class App
    Inherits Application

    Public Const CHEIGHT = 240
    Public Const CWIDTH = 320
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
        Dim fn = $"pixels_{CWIDTH}x{CHEIGHT}.dat"
        Dim file = Await TryGetFileAsync(ApplicationData.Current.LocalFolder, fn)
        If file Is Nothing Then file = Await TryGetFileAsync(Package.Current.InstalledLocation, fn)
        If file IsNot Nothing Then
            Using stream = Await file.OpenStreamForReadAsync()
                Dim red = Await stream.ReadAsync(Pixels, 0, Pixels.Length)
                If red <> Pixels.Length Then Stop
            End Using
        End If
        RaiseEvent Loaded()
    End Function

    Private Async Function TryGetFileAsync(folder As StorageFolder, fn As String) As Task(Of StorageFile)
        Try
            Return Await folder.GetFileAsync(fn)
        Catch ex As FileNotFoundException
            Return Nothing
        End Try
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

