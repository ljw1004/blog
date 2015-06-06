Imports System.Threading
Imports Windows.Storage

Public NotInheritable Class MainPage
    Inherits Page

    Dim sequenceNumberEstimator As New LinearRegressionMonitor()
    Dim levelsMonitor As New LevelsMonitor()
    Dim flushRequest As New StrongBox(Of Boolean)(False)
    Dim canvases As New HashSet(Of Canvas)
    Dim currentPlayTask As Task = Nothing
    Dim currentPlayCts As CancellationTokenSource = Nothing
    Dim currentPlayIndex As Integer = -1

    Sub New()
        InitializeComponent()
        StartBufferConsumers()
        Dim sticks As New Dictionary(Of String, String) From {
                        {"ms-appx:///sticks/Dancing Stick Guy (Justin).stk", "ms-appx:///music/Bilbo.mp3"},
                        {"ms-appx:///sticks/They're All Fruity! (Lucian).stk", "ms-appx:///music/EyeOfTheTiger.mp3"},
                        {"ms-appx:///sticks/Singing Dog (TJW).stk", "ms-appx:///music/Churchill.mp3"}
                    }
        '"ms-appx:///sticks/President Bush (Lucian).stk",
        '"ms-appx:///sticks/Hypnotised by a Space Moth (Gaekwad2).stk",
        '"ms-appx:///sticks/Quantum Physics Illustrated (Gaekwad2).stk"
        flip1.ItemsSource = sticks

    End Sub

    Async Sub StartBufferConsumers()
        ' Music will be produced by someone and placed into the BufferManager.
        ' There are two consumers of those buffers: the WASAPI Player (which sends
        ' buffers to the sound-card), and the Visualizer (which draws on screen)

        Dim period = TimeSpan.FromMilliseconds(20) ' every 20ms we update the screen
        Dim latency = TimeSpan.FromMilliseconds(2)  ' we'll draw 2ms ahead of what's currently playing (to allow for latency)
        '
        Dim levels As New FFTResults()
        Dim freqs = New Double(5, 5) {}
        Dim prevTime = DateTime.Now

        PlayerThreadRunAsync(Nothing, sequenceNumberEstimator, flushRequest).FireAndForget()

        While True
            Dim now = DateTime.Now, dt = prevTime + period - now : prevTime = now
            If dt.TotalMilliseconds > 0 Then Await Task.Delay(dt)
            Dim seqNum = sequenceNumberEstimator.Estimate(DateTimeOffset.Now + latency)
            If Not seqNum.HasValue Then Continue While
            Dim buf = Await BufferManager.GetNearestHistoryAsync(seqNum.Value)
            If buf Is Nothing Then Continue While

            levelsMonitor.Decay(dt)
            levelsMonitor.Update(buf.RawFourierData, levels)
            '
            For i = 0 To 5
                freqs(0, i) = levels.left(i)
                freqs(1, i) = levels.right(i)
                freqs(2, i) = levels.rightminusleft(i)
            Next
            freqs(3, 0) = levels.vocals
            freqs(3, 1) = levels.music
            '
            For Each canvas1 In canvases
                Dim tl = canvas1.TransformToVisual(flip1).TransformPoint(New Point(0, 0))
                If tl.X >= flip1.ActualWidth - 20 OrElse tl.X + canvas1.ActualWidth < 20 Then Continue For
                If tl.Y >= flip1.ActualHeight - 20 OrElse tl.Y + canvas1.ActualHeight < 20 Then Continue For
                Dim ci = TryCast(canvas1.Tag, Tuple(Of Stick, Win8StickRenderer)) : If ci Is Nothing Then Continue For
                ci.Item1.Update(freqs, dt)
                ci.Item1.Draw(0, 0, CInt(canvas1.ActualWidth), CInt(canvas1.ActualHeight), ci.Item2)
            Next
        End While
    End Sub


    Private Sub flip1_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If flip1.SelectedIndex = -1 OrElse flip1.SelectedIndex = currentPlayIndex Then Return
        currentPlayIndex = flip1.SelectedIndex
        Dim prevTask = currentPlayTask
        Dim prevCts = currentPlayCts
        Dim kv = CType(flip1.SelectedItem, KeyValuePair(Of String, String))
        '
        currentPlayCts = New CancellationTokenSource() : Dim snapshotCts = currentPlayCts
        currentPlayTask =
            Async Function()
                If prevCts IsNot Nothing Then prevCts.Cancel()
                If prevTask IsNot Nothing Then Await prevTask.NoThrowCancellation()

                Await BufferManager.ResetAsync()
                sequenceNumberEstimator.Reset()
                levelsMonitor.Reset()
                flushRequest.Value = True

                Dim file = Await StorageFile.GetFileFromApplicationUriAsync(New Uri(kv.Value))
                Await DecoderThreadRunAsync(New Uri("file:" & file.Path), snapshotCts.Token)
            End Function()


    End Sub

    Private Async Sub Canvas_DataContextChanged(sender As FrameworkElement, args As DataContextChangedEventArgs)
        Dim canvas1 = TryCast(sender, Canvas) : If canvas1 Is Nothing Then Return
        canvases.Remove(canvas1)
        Dim etag = New Object()
        canvas1.Tag = etag
        Dim dataContext = canvas1.DataContext
        If dataContext Is Nothing Then Return
        Dim kv = CType(dataContext, KeyValuePair(Of String, String))
        '
        Dim stick As New Stick
        Dim renderer As New Win8StickRenderer(canvas1)
        Dim file = Await StorageFile.GetFileFromApplicationUriAsync(New Uri(kv.Key))
        Using stream = Await file.OpenStreamForReadAsync()
            Await stick.LoadAsync(System.IO.Path.GetFileNameWithoutExtension(kv.Key), stream, renderer)
        End Using
        ' The following test is a way of avoiding races. It means that if datacontext
        ' was set twice on one canvas in quick succession, only the latest will win.
        If canvas1.Tag IsNot etag Then Return
        canvas1.Tag = Tuple.Create(stick, renderer)
        canvases.Add(canvas1)
    End Sub

    Private Sub Canvas_SizeChanged(sender As Object, e As SizeChangedEventArgs)
        Clip = New RectangleGeometry With {.Rect = New Rect(0, 0, ActualWidth, ActualHeight)}
    End Sub
End Class
