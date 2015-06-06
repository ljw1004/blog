Option Strict On

Imports System.Threading
Imports System.Runtime.InteropServices
Imports Windows.Media.Devices

Module Audio

    Public Function DecoderThreadRunAsync(location As Uri, Optional cancel As CancellationToken = Nothing) As task
        ' note: the decoder seems to crash if run on the UI thread...
        Return Task.Run(Function() DecoderAsync(location, cancel))
        Return Nothing ' !!!
    End Function

    Private Async Function DecoderAsync(location As Uri, cancel As CancellationToken) As task
        Dim pReader As Interop.IMFSourceReader = Nothing
        If location.Scheme <> "file" Then
            Interop.MFCreateSourceReaderFromURL(location.ToString(), Nothing, pReader)
        Else
            Dim src_file = Await Windows.Storage.StorageFile.GetFileFromPathAsync(location.LocalPath)
            Using src_stream = Await src_file.OpenAsync(Windows.Storage.FileAccessMode.Read).AsTask()
                Dim src_bytestream As Interop.IMFByteStream = Nothing : Interop.MFCreateMFByteStreamOnStreamEx(src_stream, src_bytestream)
                Interop.MFCreateSourceReaderFromByteStream(src_bytestream, Nothing, pReader)
                Marshal.ReleaseComObject(src_bytestream)
            End Using
        End If

        ' We'll ask it for PCM data, 44100Hz, 2 channels, 16 bits per sample
        pReader.SetStreamSelection(Interop.MF_SOURCE_READER_ALL_STREAMS, False)
        pReader.SetStreamSelection(Interop.MF_SOURCE_READER_FIRST_AUDIO_STREAM, True)
        Dim pRequestedType As Interop.IMFMediaType = Nothing : Interop.MFCreateMediaType(pRequestedType)
        pRequestedType.SetGUID(Interop.MF_MT_MAJOR_TYPE, Interop.MFMediaType_Audio)
        pRequestedType.SetGUID(Interop.MF_MT_SUBTYPE, Interop.MFAudioFormat_PCM)
        pRequestedType.SetUINT32(Interop.MF_MT_AUDIO_NUM_CHANNELS, 2)
        pRequestedType.SetUINT32(Interop.MF_MT_AUDIO_BITS_PER_SAMPLE, 16)
        pRequestedType.SetUINT32(Interop.MF_MT_AUDIO_SAMPLES_PER_SECOND, 44100)
        pReader.SetCurrentMediaType(Interop.MF_SOURCE_READER_FIRST_AUDIO_STREAM, Nothing, pRequestedType)
        Marshal.ReleaseComObject(pRequestedType) : pRequestedType = Nothing

        Dim SequenceNumber = 0, cbArr = 0, dummy = 0, dummyLong = 0UL
        Dim buf = BufferManager.GetFresh(SequenceNumber, cancel)
        While True
            ' This call may block. If it succeeds, then all access of the buffer must remain in the same thread
            Dim pSample As Interop.IMFSample = Nothing, dwFlags = 0 : pReader.ReadSample(Interop.MF_SOURCE_READER_FIRST_AUDIO_STREAM, 0, dummy, dwFlags, dummyLong, pSample)
            If dwFlags <> 0 OrElse cancel.IsCancellationRequested Then
                If pSample IsNot Nothing Then Marshal.ReleaseComObject(pSample) : pSample = Nothing
                Exit While
            End If
            '
            Dim pBuffer As Interop.IMFMediaBuffer = Nothing : pSample.ConvertToContiguousBuffer(pBuffer)
            Marshal.ReleaseComObject(pSample) : pSample = Nothing

            '
            Dim pAudioData As IntPtr, cbBuffer As Integer : pBuffer.Lock(pAudioData, dummy, cbBuffer)
            Dim iBuffer = 0
            While cbBuffer > 0
                Dim cbWriteArr = Math.Min(buf.Arr.Length * 2 - cbArr, cbBuffer)
                Marshal.Copy(IntPtr.Add(pAudioData, iBuffer), buf.Arr, cbArr \ 2, cbWriteArr \ 2)
                cbArr += cbWriteArr : iBuffer += cbWriteArr : cbBuffer -= cbWriteArr
                If cbArr < buf.Arr.Length * 2 Then Continue While
                ' TODO: support cancellation here
                BufferManager.DoFFT(buf)
                BufferManager.HandToPlayerAsync(buf).GetAwaiter().GetResult() ' it's now someone else's responsibility to dispose this buffer
                SequenceNumber += 1
                buf = BufferManager.GetFresh(SequenceNumber)
                cbArr = 0
            End While
            pBuffer.Unlock()
            Marshal.ReleaseComObject(pBuffer) : pBuffer = Nothing
        End While

        BufferManager.DiscardAudioBuffer(buf) : buf = Nothing
        Marshal.ReleaseComObject(pReader) : pReader = Nothing
    End Function


    Public Function PlayerThreadRunAsync(cancel As CancellationToken, progress As IProgress(Of Double), flushRequest As StrongBox(Of Boolean)) As Task
        ' the WASAPI player code seems to crash if run on the UI thread...
        Return Task.Run(Function() PlayerAsync(cancel, progress, flushRequest))
    End Function

    Private Async Function PlayerAsync(cancel As CancellationToken, progress As IProgress(Of Double), flushRequest As StrongBox(Of Boolean)) As task
        Dim defaultDeviceId = MediaDevice.GetDefaultAudioRenderId(Windows.Media.Devices.AudioDeviceRole.Default)

        Dim icbh = New Interop.ActivateAudioInterfaceCompletionHandler(
                   Sub(pAudioClient2)
                       ' this initialization code is packaged into a delegate because it must
                       ' be run synchronously on a specified provided thread
                       Dim wfx As New Interop.WAVEFORMATEX With {.wFormatTag = 1, .nChannels = 2, .nSamplesPerSec = 44100, .wBitsPerSample = 16, .nBlockAlign = 4, .nAvgBytesPerSec = 44100 * 4, .cbSize = 0}
                       Dim pwfx_suggested As IntPtr = Nothing : pAudioClient2.IsFormatSupported(Interop.AUDCLNT_SHAREMODE.AUDCLNT_SHAREMODE_SHARED, wfx, pwfx_suggested)
                       If pwfx_suggested = Nothing Then
                           pwfx_suggested = Marshal.AllocCoTaskMem(18)
                           Marshal.StructureToPtr(Of Interop.WAVEFORMATEX)(wfx, pwfx_suggested, False)
                       Else
                           Dim wfx_suggested = Marshal.PtrToStructure(Of Interop.WAVEFORMATEX)(pwfx_suggested)
                       End If
                       pAudioClient2.Initialize(Interop.AUDCLNT_SHAREMODE.AUDCLNT_SHAREMODE_SHARED, Interop.AUDCLNT_FLAGS.AUDCLNT_STREAMFLAGS_NOPERSIST, 1000000, 0, pwfx_suggested, Nothing)
                       Marshal.FreeCoTaskMem(pwfx_suggested)
                   End Sub)

        Dim activationOperation As Interop.IActivateAudioInterfaceAsyncOperation = Nothing
        Interop.ActivateAudioInterfaceAsync(defaultDeviceId, Interop.IID_IAudioClient2, Nothing, icbh, activationOperation)
        Dim pAudioClient = Await icbh

        Dim bufferFrameCount = 0 : pAudioClient.GetBufferSize(bufferFrameCount)
        Dim ppv As Object = Nothing : pAudioClient.GetService(Interop.IID_IAudioRenderClient, ppv)
        Dim pRenderClient = CType(ppv, Interop.IAudioRenderClient)
        pAudioClient.Start()

        Dim buf = Await BufferManager.GetForPlayerAsync(), cbArr = 0
        While True
            If cancel.IsCancellationRequested Then Exit While
            Await Task.Delay(5, cancel)
            If flushRequest.Value Then
                flushRequest.Value = False
                pAudioClient.Stop()
                pAudioClient.Reset()
                pAudioClient.Start()
            End If

            Dim numFramesPadding = 0 : pAudioClient.GetCurrentPadding(numFramesPadding)
            Dim numFramesAvailable = bufferFrameCount - numFramesPadding
            If numFramesAvailable < 1024 Then Continue While

            Dim seqNowPlaying = buf.SequenceNumber + (cbArr \ buf.Arr.Length \ 2) - (numFramesPadding \ buf.Arr.Length \ 2)
            progress.Report(seqNowPlaying)

            Dim pData As IntPtr = Nothing : pRenderClient.GetBuffer(numFramesAvailable, pData)
            Dim ibData = 0, cbData = numFramesAvailable * 4
            While cbData > 0
                Dim cbCopy = Math.Min(cbData, buf.Arr.Length * 2 - cbArr)
                Marshal.Copy(buf.Arr, cbArr \ 2, IntPtr.Add(pData, ibData), cbCopy \ 2)
                ibData += cbCopy : cbData -= cbCopy : cbArr += cbCopy
                If cbArr < buf.Arr.Length * 2 Then Continue While
                Try
                    ' TODO: support cancellation here
                    BufferManager.HandToHistoryAsync(buf).GetAwaiter().GetResult()
                    buf = BufferManager.GetForPlayerAsync().GetAwaiter().GetResult() : cbArr = 0
                Catch ex As OperationCanceledException
                    Exit While
                End Try
            End While

            pRenderClient.ReleaseBuffer(numFramesAvailable, 0)
        End While

        pAudioClient.Stop()
        BufferManager.DiscardAudioBuffer(buf) : buf = Nothing
        Marshal.ReleaseComObject(pRenderClient) : pRenderClient = Nothing
        Marshal.ReleaseComObject(pAudioClient) : pAudioClient = Nothing
        Marshal.ReleaseComObject(activationOperation) : activationOperation = Nothing
    End Function

End Module
