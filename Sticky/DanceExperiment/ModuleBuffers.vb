Option Strict On

Imports System.Threading.Tasks.Dataflow
Imports System.Threading


Public Module BufferManager
    Private Const maxDecodedAndWaitingToPlayMs As Integer = 500 ' anything more from the decoder will block when it tries to hand to player
    Private Const maxPlayedAndWaitingForHistoryMs As Integer = 1000
    Private Const bufferDurationMs As Integer = 44100 \ 4096
    Private toPlayBuffers As New BufferBlock(Of AudioBuffer)(New DataflowBlockOptions With {.BoundedCapacity = maxDecodedAndWaitingToPlayMs \ bufferDurationMs})
    Private historicalBuffers As New SortedSet(Of AudioBuffer)(New SequenceNumberComparer)
    Private historicalBuffersLock As New SemaphoreSlim(1)
    Private historicalRequests As New LinearRegressionMonitor()
    Private availableArrays As New BufferBlock(Of Short())
    Private availableBuffers As New BufferBlock(Of AudioBuffer)
    Private fft1 As New FFT()

    Private Class SequenceNumberComparer : Implements IComparer(Of AudioBuffer)
        Public Function Compare(x As AudioBuffer, y As AudioBuffer) As Integer Implements IComparer(Of AudioBuffer).Compare
            Return x.SequenceNumber - y.SequenceNumber
        End Function
    End Class

    Public Sub DiscardAudioBuffer(buf As AudioBuffer)
        If buf.IsDisposed Then Return
        buf.IsDisposed = True
        AudioBufferDetachArray(buf)
        availableBuffers.Post(buf)
    End Sub

    Private Sub AudioBufferDetachArray(buf As AudioBuffer)
        If buf.Arr Is Nothing Then Return
        Dim t = buf.Arr : buf.Arr = Nothing
        availableArrays.Post(t)
    End Sub


    Public Function GetFresh(SequenceNumber As Integer, Optional cancel As CancellationToken = Nothing) As AudioBuffer
        Dim buf As AudioBuffer = Nothing
        Dim arr As Short() = Nothing
        Try
            If Not availableBuffers.TryReceive(buf) Then buf = New AudioBuffer()
            If Not availableArrays.TryReceive(arr) Then arr = New Short(1024 * 2 - 1) {}
            buf.Arr = arr
            buf.SequenceNumber = SequenceNumber
            buf.IsDisposed = False
            Return buf
        Catch ex As OperationCanceledException
            If buf IsNot Nothing Then DiscardAudioBuffer(buf) : buf = Nothing
            Throw
        End Try
    End Function

    Public Sub DoFFT(buf As AudioBuffer)
        SyncLock fft1
            fft1.DoFFT(buf.Arr, buf.RawFourierData)
        End SyncLock
    End Sub

    Public Function HandToPlayerAsync(buf As AudioBuffer, Optional cancel As CancellationToken = Nothing) As task
        Return toPlayBuffers.SendAsync(buf, cancel)
    End Function

    Public Function GetForPlayerAsync(Optional cancel As CancellationToken = Nothing) As Task(Of AudioBuffer)
        Return toPlayBuffers.ReceiveAsync(cancel)
    End Function

    Public Async Function HandToHistoryAsync(buf As AudioBuffer, Optional cancel As CancellationToken = Nothing) As task
        AudioBufferDetachArray(buf)
        Await historicalBuffersLock.WaitAsync(cancel)
        If historicalBuffers.Count < 100 Then historicalBuffers.Add(buf) Else DiscardAudioBuffer(buf)
        historicalBuffersLock.Release()
    End Function

    Public Async Function ResetAsync(Optional cancel As CancellationToken = Nothing) As task
        Dim rbuf As AudioBuffer = Nothing
        While toPlayBuffers.TryReceive(rbuf)
            DiscardAudioBuffer(rbuf)
        End While
        ' note: can't use TryReceiveAll - https://connect.microsoft.com/VisualStudio/feedback/details/785185/receive-doesnt-work-after-tryreceiveall-in-tpl-dataflow
        '
        Await historicalBuffersLock.WaitAsync(cancel)
        Dim bufsToRemove = historicalBuffers.ToArray()
        historicalBuffers.Clear()
        historicalBuffersLock.Release()
        For Each buf In bufsToRemove
            DiscardAudioBuffer(buf)
        Next
    End Function

    Public Async Function GetNearestHistoryAsync(SequenceNumber As Double, Optional cancel As CancellationToken = Nothing) As Task(Of AudioBuffer)
        historicalRequests.Report(SequenceNumber)
        Await historicalBuffersLock.WaitAsync(cancel)
        '
        Dim previousBuf As AudioBuffer = Nothing
        Dim nextBuf As AudioBuffer = Nothing
        Dim returnBuf As AudioBuffer = Nothing
        Dim canForgetThreshhold = historicalRequests.Estimate(DateTimeOffset.Now - TimeSpan.FromMilliseconds(500))
        Dim bufsToRemove = New List(Of AudioBuffer)()
        For Each buf In historicalBuffers
            If canForgetThreshhold.HasValue AndAlso buf.SequenceNumber < canForgetThreshhold.Value Then bufsToRemove.Add(buf)
            If buf.SequenceNumber <= SequenceNumber Then previousBuf = buf
            If buf.SequenceNumber >= SequenceNumber Then nextBuf = buf : Exit For
        Next
        For Each buf In bufsToRemove : historicalBuffers.Remove(buf) : Next
        historicalBuffersLock.Release()
        For Each buf In bufsToRemove : DiscardAudioBuffer(buf) : Next
        '
        If nextBuf Is Nothing Then
            returnBuf = previousBuf
        ElseIf previousBuf Is Nothing Then
            returnBuf = nextBuf
        Else
            Dim dprev = Math.Abs(SequenceNumber - previousBuf.SequenceNumber), dnext = Math.Abs(SequenceNumber - nextBuf.SequenceNumber)
            returnBuf = If(dprev < dnext, previousBuf, nextBuf)
        End If
        Return returnBuf
    End Function


End Module

