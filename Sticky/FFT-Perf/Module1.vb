Module Module1

    Dim form As MyForm = Nothing

    <MTAThread>
    Sub Main()
        RegressionTest()
        Console.WriteLine("press any key")
        'Console.ReadKey()
        'PerfTest()
        VisualizationTestAsync().GetAwaiter().GetResult()
    End Sub

    Sub RegressionTest()
        Dim arr = SetupShorts(1024)
        '
        Dim fft_orig As New FFT_VB()
        Dim buf_orig As New AudioBuffer() With {.arr = arr}
        fft_orig.RunAll(buf_orig)
        '
        Dim fft As New FFT_VB()
        Dim buf As New AudioBuffer() With {.arr = arr}
        fft.RunAll(buf)
        '
        CompareDoubles("vocals", buf_orig.power_vocals, buf.power_vocals)
        CompareDoubles("music", buf_orig.power_music, buf.power_music)
        For i = 0 To 5
            CompareDoubles("left(" & i.ToString() & ")", buf_orig.power_left(i), buf.power_left(i))
            CompareDoubles("right(" & i.ToString() & ")", buf_orig.power_right(i), buf.power_right(i))
        Next
    End Sub

    Sub CompareDoubles(s As String, orig As Double, [new] As Double)
        Dim x0 = Math.Round(orig, 2), x1 = Math.Round([new], 2)
        If x0 <> x1 Then Console.WriteLine("*** {0}: orig={1}, new={2}", s, x0, x1)
    End Sub

    Sub PerfTest()
        Dim arr = SetupShorts(1024)
        Dim fft As New FFT_VB()
        Dim buf As New AudioBuffer() With {.arr = arr}
        Dim s As New Stopwatch
        s.Start()
        For iteration = 0 To 10000
            fft.RunAll(buf)
        Next
        s.Stop()
        ' we have 44100KHz sound, and buffers of size 1024 samples
        Dim ms_per_fft = s.ElapsedMilliseconds / 10000
        Dim ffts_per_second = 44100.0 / 1024
        Dim fft_percent = ms_per_fft * ffts_per_second / 10
        Console.WriteLine("FFT:{0}ms, i.e. {1:.0}% cpu", ms_per_fft, fft_percent)
    End Sub

    Async Function VisualizationTestAsync() As Task
        Dim dummy = Task.Factory.StartNew(
            Sub()
                System.Windows.Forms.Application.EnableVisualStyles()
                form = New MyForm
                form.bmp = New System.Drawing.Bitmap(512, 512)
                form.g = Drawing.Graphics.FromImage(form.bmp)
                System.Windows.Forms.Application.Run(form)
            End Sub)
        While form Is Nothing OrElse Not form.Visible : Threading.Thread.Sleep(100) : End While

        MFStartup(MF_VERSION)

        Dim pReader As IMFSourceReader = Nothing : MFCreateSourceReaderFromURL("Toccata.mp3", Nothing, pReader)

        Dim font = New System.Drawing.Font("Times New Roman", 12)
        Dim pts As Drawing.Point() = Nothing

        Dim t = Task.Run(Sub() ConvertToPcm(pReader))
        'Dim t = Task.Run(Sub() GenerateIncreasingSin())

        Dim buf As AudioBuffer = Nothing
        Dim buforig As AudioBuffer = Nothing

        Dim fft As FFT_VB = Nothing
        Dim fftorig As FFT_VB = Nothing
        Dim nSamples = 0
        While form.Visible
            Dim delay = Task.Delay(1000 * nSamples \ 44100)
            Dim bytebuf As Byte() = Nothing
            If Not queue.TryDequeue(bytebuf) Then Await Task.Delay(10) : Continue While
            If bytebuf Is Nothing Then Exit While
            nSamples = bytebuf.Length \ 4
            If buf Is Nothing Then
                buf = New AudioBuffer
                buf.arr = New Short(nSamples * 2 - 1) {}
                buforig = New AudioBuffer
                buforig.arr = buf.arr
                fft = New FFT_VB()
                fftorig = New FFT_VB()
                pts = New Drawing.Point(nSamples - 1) {}
            End If
            Buffer.BlockCopy(bytebuf, 0, buf.arr, 0, bytebuf.Length)
            '
            fft.RunAll(buf)
            fftorig.RunAll(buforig)
            '
            SyncLock form.bmp
                form.g.FillRectangle(Drawing.Brushes.Black, 0, 0, 512, 512)
                For i = 0 To nSamples - 1
                    pts(i).X = i * 512 \ nSamples
                    pts(i).Y = buf.arr(i * 2) \ 256 + 256
                Next
                form.g.DrawLines(Drawing.Pens.DarkGreen, pts)
                For i = 0 To nSamples - 1
                    pts(i).Y = buf.arr(i * 2 + 1) \ 256 + 256
                Next
                form.g.DrawLines(Drawing.Pens.DarkRed, pts)
                '
                For i = 0 To 5
                    Dim xmin = i * 85 + 2, xw2 = 40
                    Dim ytop = 256, yhl = CInt(buf.power_left(i) * 0.1), yhr = CInt(buf.power_right(i) * 0.1)
                    form.g.FillRectangle(Drawing.Brushes.Yellow, xmin, ytop, xw2, yhl)
                    form.g.FillRectangle(Drawing.Brushes.Yellow, xmin + xw2, ytop, xw2, yhr)
                    '
                    yhl = CInt(buforig.power_left(i) * 0.1) : yhr = CInt(buforig.power_right(i) * 0.1)
                    form.g.DrawRectangle(Drawing.Pens.Azure, xmin, ytop, xw2, yhl)
                    form.g.DrawRectangle(Drawing.Pens.Azure, xmin + xw2, ytop, xw2, yhr)
                Next i

                Dim zvoc = CInt(buf.power_vocals * 0.02), zmus = CInt(buf.power_music * 0.02)
                form.g.FillRectangle(Drawing.Brushes.AliceBlue, 256 - zvoc, 450, zvoc, 50)
                form.g.FillRectangle(Drawing.Brushes.Aquamarine, 256, 450, zmus, 50)
                zvoc = CInt(buforig.power_vocals * 0.02) : zmus = CInt(buforig.power_music * 0.02)
                form.g.FillRectangle(Drawing.Brushes.AliceBlue, 256 - zvoc, 400, zvoc, 50)
                form.g.FillRectangle(Drawing.Brushes.Aquamarine, 256, 400, zmus, 50)

                form.Invalidate()
            End SyncLock

            Await delay
        End While
        Await t

        Runtime.InteropServices.Marshal.ReleaseComObject(pReader) : pReader = Nothing

        MFShutdown()
    End Function

    Function SetupShorts(len As Integer) As Short()
        Dim RND As New Random()
        Dim arr = New Short(len * 2 - 1) {}
        For i = 0 To arr.Length - 1
            arr(i) = CShort(RND.Next(Short.MinValue, Short.MaxValue))
        Next
        Return arr
    End Function

    Dim queue As New Concurrent.ConcurrentQueue(Of Byte())

    Sub GenerateIncreasingSin()
        Dim arr = New Short(1024 * 2 - 1) {}
        For freq = 450 To 2514
            Dim radians_per_sec = freq * 2 * Math.PI
            Dim samples_per_sec = 44100
            Dim radians_per_sample = radians_per_sec / samples_per_sec
            Dim phase = 0.0
            For i = 0 To (arr.Length \ 2) - 1
                Dim amp = Math.Sin(phase)
                Dim amps = CShort(amp * 0.95 * Short.MaxValue)
                arr(i * 2) = amps
                arr(i * 2 + 1) = amps
                phase += radians_per_sample
            Next
            Dim buf = New Byte(1024 * 4 - 1) {}
            Buffer.BlockCopy(arr, 0, buf, 0, 1024 * 4)
            queue.Enqueue(buf)
        Next
        queue.Enqueue(Nothing)
    End Sub

    Sub ConvertToPcm(pReader As IMFSourceReader)
        ' We'll ask it for PCM data, 44100Hz, 2 channels, 16 bits per sample
        pReader.SetStreamSelection(MF_SOURCE_READER_ALL_STREAMS, False)
        pReader.SetStreamSelection(MF_SOURCE_READER_FIRST_AUDIO_STREAM, True)
        Dim pRequestedType As IMFMediaType = Nothing : MFCreateMediaType(pRequestedType)
        pRequestedType.SetGUID(MF_MT_MAJOR_TYPE, MFMediaType_Audio)
        pRequestedType.SetGUID(MF_MT_SUBTYPE, MFAudioFormat_PCM)
        pRequestedType.SetUINT32(MF_MT_AUDIO_NUM_CHANNELS, 2)
        pRequestedType.SetUINT32(MF_MT_AUDIO_BITS_PER_SAMPLE, 16)
        pRequestedType.SetUINT32(MF_MT_AUDIO_SAMPLES_PER_SECOND, 44100)
        pReader.SetCurrentMediaType(MF_SOURCE_READER_FIRST_AUDIO_STREAM, Nothing, pRequestedType)
        Runtime.InteropServices.Marshal.ReleaseComObject(pRequestedType) : pRequestedType = Nothing

        Dim arr = New Byte(1024 * 4 - 1) {}, cbArr = 0

        ' Now a loop to get buffer after buffer from the MFSourceReader, and write it to disk:
        Do
            Dim pSample As IMFSample = Nothing, dwFlags As Integer : pReader.ReadSample(MF_SOURCE_READER_FIRST_AUDIO_STREAM, 0, 0, dwFlags, 0, pSample)
            If dwFlags <> 0 Then pSample = Nothing : Exit Do
            Dim pBuffer As IMFMediaBuffer = Nothing : pSample.ConvertToContiguousBuffer(pBuffer)
            Dim pAudioData As IntPtr, cbBuffer As Integer : pBuffer.Lock(pAudioData, Nothing, cbBuffer)
            Dim iData = 0
            While cbBuffer > 0
                Dim cbCopy = Math.Min(arr.Length - cbArr, cbBuffer)
                Runtime.InteropServices.Marshal.Copy(IntPtr.Add(pAudioData, iData), arr, cbArr, cbCopy)
                iData += cbCopy : cbArr += cbCopy : cbBuffer -= cbCopy
                If cbArr = arr.Length Then
                    queue.Enqueue(arr) : arr = New Byte(arr.Length - 1) {} : cbArr = 0
                End If
            End While
            pBuffer.Unlock()
            Runtime.InteropServices.Marshal.ReleaseComObject(pBuffer) : pBuffer = Nothing
            Runtime.InteropServices.Marshal.ReleaseComObject(pSample) : pSample = Nothing
        Loop

        queue.Enqueue(Nothing)
    End Sub
End Module


Module Interop
    <Runtime.InteropServices.DllImport("mfplat.dll", ExactSpelling:=True, PreserveSig:=False)>
    Sub MFStartup(Version As Integer, Optional dwFlags As Integer = 0)
    End Sub
    <Runtime.InteropServices.DllImport("mfplat.dll", ExactSpelling:=True, PreserveSig:=False)>
    Public Sub MFShutdown()
    End Sub
    <Runtime.InteropServices.DllImport("mfplat.dll", ExactSpelling:=True, PreserveSig:=False)>
    Public Sub MFCreateMediaType(ByRef ppMFType As IMFMediaType)
    End Sub
    <Runtime.InteropServices.DllImport("mfplat.dll", ExactSpelling:=True, PreserveSig:=False)>
    Public Sub MFCreateWaveFormatExFromMFMediaType(pMFType As IMFMediaType, ByRef ppWF As IntPtr, ByRef pcbSize As Integer, Optional Flags As Integer = 0)
    End Sub
    <Runtime.InteropServices.DllImport("mfreadwrite.dll", ExactSpelling:=True, PreserveSig:=False)>
    Public Sub MFCreateSourceReaderFromURL(<Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPWStr)> pwszURL As String, pAttributes As IntPtr, ByRef ppSourceReader As IMFSourceReader)
    End Sub
    <Runtime.InteropServices.DllImport("mfreadwrite.dll", ExactSpelling:=True, PreserveSig:=False)>
    Public Sub MFCreateSourceReaderFromByteStream(pByteStream As IMFByteStream, pAttributes As IntPtr, ByRef ppSourceReader As IMFSourceReader)
    End Sub
    <Runtime.InteropServices.DllImport("mfplat.dll", ExactSpelling:=True, PreserveSig:=False)>
    Public Sub MFCreateMFByteStreamOnStreamEx(<Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.IUnknown)> punkStream As Object, ByRef ppByteStream As IMFByteStream)
    End Sub


    Public Const MF_SOURCE_READER_ALL_STREAMS As Integer = &HFFFFFFFE
    Public Const MF_SOURCE_READER_FIRST_AUDIO_STREAM As Integer = &HFFFFFFFD
    Public Const MF_SDK_VERSION As Integer = &H2
    Public Const MF_API_VERSION As Integer = &H70
    Public Const MF_VERSION As Integer = (MF_SDK_VERSION << 16) Or MF_API_VERSION
    Public ReadOnly MF_MT_MAJOR_TYPE As New Guid("48eba18e-f8c9-4687-bf11-0a74c9f96a8f")
    Public ReadOnly MF_MT_SUBTYPE As New Guid("f7e34c9a-42e8-4714-b74b-cb29d72c35e5")
    Public ReadOnly MF_MT_AUDIO_BLOCK_ALIGNMENT As New Guid("322de230-9eeb-43bd-ab7a-ff412251541d")
    Public ReadOnly MF_MT_AUDIO_AVG_BYTES_PER_SECOND As New Guid("1aab75c8-cfef-451c-ab95-ac034b8e1731")
    Public ReadOnly MF_MT_AUDIO_NUM_CHANNELS As New Guid("37e48bf5-645e-4c5b-89de-ada9e29b696a")
    Public ReadOnly MF_MT_AUDIO_SAMPLES_PER_SECOND As New Guid("5faeeae7-0290-4c31-9e8a-c534f68d9dba")
    Public ReadOnly MF_MT_AUDIO_BITS_PER_SAMPLE As New Guid("f2deb57f-40fa-4764-aa33-ed4f2d1ff669")
    Public ReadOnly MFMediaType_Audio As New Guid("73647561-0000-0010-8000-00AA00389B71")
    Public ReadOnly MFAudioFormat_PCM As New Guid("00000001-0000-0010-8000-00AA00389B71")

    <Runtime.InteropServices.ComImport, Runtime.InteropServices.InterfaceType(Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown), Runtime.InteropServices.Guid("70ae66f2-c809-4e4f-8915-bdcb406b7993")>
    Public Interface IMFSourceReader
        Sub GetStreamSelection(<Runtime.InteropServices.In> dwStreamIndex As Integer, <Runtime.InteropServices.Out, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.Bool)> ByRef pSelected As Boolean)
        Sub SetStreamSelection(<Runtime.InteropServices.In> dwStreamIndex As Integer, <Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.Bool)> pSelected As Boolean)
        Sub GetNativeMediaType(<Runtime.InteropServices.In> dwStreamIndex As Integer, <Runtime.InteropServices.In> dwMediaTypeIndex As Integer, <Runtime.InteropServices.Out> ByRef ppMediaType As IntPtr)
        Sub GetCurrentMediaType(<Runtime.InteropServices.In> dwStreamIndex As Integer, <Runtime.InteropServices.Out> ByRef ppMediaType As IMFMediaType)
        Sub SetCurrentMediaType(<Runtime.InteropServices.In> dwStreamIndex As Integer, pdwReserved As IntPtr, <Runtime.InteropServices.In> pMediaType As IMFMediaType)
        Sub SetCurrentPosition(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidTimeFormat As Guid, <Runtime.InteropServices.In> varPosition As Object)
        Sub ReadSample(<Runtime.InteropServices.In> dwStreamIndex As Integer, <Runtime.InteropServices.In> dwControlFlags As Integer, <Runtime.InteropServices.Out> ByRef pdwActualStreamIndex As Integer, <Runtime.InteropServices.Out> ByRef pdwStreamFlags As Integer, <Runtime.InteropServices.Out> ByRef pllTimestamp As UInt64, <Runtime.InteropServices.Out> ByRef ppSample As IMFSample)
        Sub Flush(<Runtime.InteropServices.In> dwStreamIndex As Integer)
        Sub GetServiceForStream(<Runtime.InteropServices.In> dwStreamIndex As Integer, <Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidService As Guid, <Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> riid As Guid, <Runtime.InteropServices.Out> ByRef ppvObject As IntPtr)
        Sub GetPresentationAttribute(<Runtime.InteropServices.In> dwStreamIndex As Integer, <Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidAttribute As Guid, <Runtime.InteropServices.Out> pvarAttribute As IntPtr)
    End Interface

    <Runtime.InteropServices.ComImport, Runtime.InteropServices.InterfaceType(Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown), Runtime.InteropServices.Guid("2CD2D921-C447-44A7-A13C-4ADABFC247E3")>
    Public Interface IMFAttributes
        Sub GetItem(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, pValue As IntPtr)
        Sub GetItemType(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef pType As Integer)
        Sub CompareItem(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, Value As IntPtr, <Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.Bool)> ByRef pbResult As Boolean)
        Sub Compare(<Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.Interface)> pTheirs As IMFAttributes, MatchType As Integer, <Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.Bool)> ByRef pbResult As Boolean)
        Sub GetUINT32(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef punValue As Integer)
        Sub GetUINT64(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef punValue As Long)
        Sub GetDouble(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef pfValue As Double)
        Sub GetGUID(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef pguidValue As Guid)
        Sub GetStringLength(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef pcchLength As Integer)
        Sub GetString(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.Out, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPWStr)> pwszValue As Text.StringBuilder, cchBufSize As Integer, ByRef pcchLength As Integer)
        Sub GetAllocatedString(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPWStr)> ByRef ppwszValue As String, ByRef pcchLength As Integer)
        Sub GetBlobSize(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef pcbBlobSize As Integer)
        Sub GetBlob(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.Out, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPArray)> pBuf As Byte(), cbBufSize As Integer, ByRef pcbBlobSize As Integer)
        Sub GetAllocatedBlob(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef ip As IntPtr, ByRef pcbSize As Integer)
        Sub GetUnknown(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> riid As Guid, <Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.IUnknown)> ByRef ppv As Object)
        Sub SetItem(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, Value As IntPtr)
        Sub DeleteItem(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid)
        Sub DeleteAllItems()
        Sub SetUINT32(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, unValue As Integer)
        Sub SetUINT64(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, unValue As Long)
        Sub SetDouble(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, fValue As Double)
        Sub SetGUID(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidValue As Guid)
        Sub SetString(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPWStr)> wszValue As String)
        Sub SetBlob(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPArray, SizeParamIndex:=2)> pBuf As Byte(), cbBufSize As Integer)
        Sub SetUnknown(<Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.IUnknown)> pUnknown As Object)
        Sub LockStore()
        Sub UnlockStore()
        Sub GetCount(ByRef pcItems As Integer)
        Sub GetItemByIndex(unIndex As Integer, ByRef pguidKey As Guid, pValue As IntPtr)
        Sub CopyAllItems(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.Interface)> pDest As IMFAttributes)
    End Interface

    <Runtime.InteropServices.ComImport, Runtime.InteropServices.InterfaceType(Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown), Runtime.InteropServices.Guid("44AE0FA8-EA31-4109-8D2E-4CAE4997C555")>
    Public Interface IMFMediaType
        Inherits IMFAttributes
        Overloads Sub GetItem(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, pValue As IntPtr)
        Overloads Sub GetItemType(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef pType As Integer)
        Overloads Sub CompareItem(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, Value As IntPtr, <Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.Bool)> ByRef pbResult As Boolean)
        Overloads Sub Compare(<Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.Interface)> pTheirs As IMFAttributes, MatchType As Integer, <Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.Bool)> ByRef pbResult As Boolean)
        Overloads Sub GetUINT32(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef punValue As Integer)
        Overloads Sub GetUINT64(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef punValue As Long)
        Overloads Sub GetDouble(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef pfValue As Double)
        Overloads Sub GetGUID(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef pguidValue As Guid)
        Overloads Sub GetStringLength(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef pcchLength As Integer)
        Overloads Sub GetString(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.Out, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPWStr)> pwszValue As Text.StringBuilder, cchBufSize As Integer, ByRef pcchLength As Integer)
        Overloads Sub GetAllocatedString(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPWStr)> ByRef ppwszValue As String, ByRef pcchLength As Integer)
        Overloads Sub GetBlobSize(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef pcbBlobSize As Integer)
        Overloads Sub GetBlob(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.Out, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPArray)> pBuf As Byte(), cbBufSize As Integer, ByRef pcbBlobSize As Integer)
        Overloads Sub GetAllocatedBlob(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef ip As IntPtr, ByRef pcbSize As Integer)
        Overloads Sub GetUnknown(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> riid As Guid, <Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.IUnknown)> ByRef ppv As Object)
        Overloads Sub SetItem(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, Value As IntPtr)
        Overloads Sub DeleteItem(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid)
        Overloads Sub DeleteAllItems()
        Overloads Sub SetUINT32(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, unValue As Integer)
        Overloads Sub SetUINT64(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, unValue As Long)
        Overloads Sub SetDouble(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, fValue As Double)
        Overloads Sub SetGUID(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidValue As Guid)
        Overloads Sub SetString(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPWStr)> wszValue As String)
        Overloads Sub SetBlob(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPArray, SizeParamIndex:=2)> pBuf As Byte(), cbBufSize As Integer)
        Overloads Sub SetUnknown(<Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.IUnknown)> pUnknown As Object)
        Overloads Sub LockStore()
        Overloads Sub UnlockStore()
        Overloads Sub GetCount(ByRef pcItems As Integer)
        Overloads Sub GetItemByIndex(unIndex As Integer, ByRef pguidKey As Guid, pValue As IntPtr)
        Overloads Sub CopyAllItems(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.Interface)> pDest As IMFAttributes)
        '
        Sub GetMajorType(ByRef pguidMajorType As Guid)
        Sub IsCompressedFormat(<Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.Bool)> ByRef pfCompressed As Boolean)
        <Runtime.InteropServices.PreserveSig> Function IsEqual(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.Interface)> pIMediaType As IMFMediaType, ByRef pdwFlags As Integer) As Integer
        Sub GetRepresentation(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.Struct)> guidRepresentation As Guid, ByRef ppvRepresentation As IntPtr)
        Sub FreeRepresentation(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.Struct)> guidRepresentation As Guid, <Runtime.InteropServices.In> pvRepresentation As IntPtr)
    End Interface

    <Runtime.InteropServices.ComImport, Runtime.InteropServices.InterfaceType(Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown), Runtime.InteropServices.Guid("c40a00f2-b93a-4d80-ae8c-5a1c634f58e4")>
    Public Interface IMFSample
        Inherits IMFAttributes
        Overloads Sub GetItem(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, pValue As IntPtr)
        Overloads Sub GetItemType(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef pType As Integer)
        Overloads Sub CompareItem(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, Value As IntPtr, <Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.Bool)> ByRef pbResult As Boolean)
        Overloads Sub Compare(<Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.Interface)> pTheirs As IMFAttributes, MatchType As Integer, <Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.Bool)> ByRef pbResult As Boolean)
        Overloads Sub GetUINT32(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef punValue As Integer)
        Overloads Sub GetUINT64(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef punValue As Long)
        Overloads Sub GetDouble(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef pfValue As Double)
        Overloads Sub GetGUID(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef pguidValue As Guid)
        Overloads Sub GetStringLength(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef pcchLength As Integer)
        Overloads Sub GetString(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.Out, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPWStr)> pwszValue As Text.StringBuilder, cchBufSize As Integer, ByRef pcchLength As Integer)
        Overloads Sub GetAllocatedString(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPWStr)> ByRef ppwszValue As String, ByRef pcchLength As Integer)
        Overloads Sub GetBlobSize(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef pcbBlobSize As Integer)
        Overloads Sub GetBlob(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.Out, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPArray)> pBuf As Byte(), cbBufSize As Integer, ByRef pcbBlobSize As Integer)
        Overloads Sub GetAllocatedBlob(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, ByRef ip As IntPtr, ByRef pcbSize As Integer)
        Overloads Sub GetUnknown(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> riid As Guid, <Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.IUnknown)> ByRef ppv As Object)
        Overloads Sub SetItem(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, Value As IntPtr)
        Overloads Sub DeleteItem(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid)
        Overloads Sub DeleteAllItems()
        Overloads Sub SetUINT32(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, unValue As Integer)
        Overloads Sub SetUINT64(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, unValue As Long)
        Overloads Sub SetDouble(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, fValue As Double)
        Overloads Sub SetGUID(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidValue As Guid)
        Overloads Sub SetString(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPWStr)> wszValue As String)
        Overloads Sub SetBlob(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPArray, SizeParamIndex:=2)> pBuf As Byte(), cbBufSize As Integer)
        Overloads Sub SetUnknown(<Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> guidKey As Guid, <Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.IUnknown)> pUnknown As Object)
        Overloads Sub LockStore()
        Overloads Sub UnlockStore()
        Overloads Sub GetCount(ByRef pcItems As Integer)
        Overloads Sub GetItemByIndex(unIndex As Integer, ByRef pguidKey As Guid, pValue As IntPtr)
        Overloads Sub CopyAllItems(<Runtime.InteropServices.In, Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.Interface)> pDest As IMFAttributes)
        '
        Sub GetSampleFlags(ByRef pdwSampleFlags As Integer)
        Sub SetSampleFlags(dwSampleFlags As Integer)
        Sub GetSampleTime(ByRef phnsSampletime As Long)
        Sub SetSampleTime(hnsSampleTime As Long)
        Sub GetSampleDuration(ByRef phnsSampleDuration As Long)
        Sub SetSampleDuration(hnsSampleDuration As Long)
        Sub GetBufferCount(ByRef pdwBufferCount As Integer)
        Sub GetBufferByIndex(dwIndex As Integer, ByRef ppBuffer As IMFMediaBuffer)
        Sub ConvertToContiguousBuffer(ByRef ppBuffer As IMFMediaBuffer)
        Sub AddBuffer(pBuffer As IMFMediaBuffer)
        Sub RemoveBuferByindex(dwIndex As Integer)
        Sub RemoveAllBuffers()
        Sub GetTotalLength(ByRef pcbTotalLength As Integer)
        Sub CopyToByffer(pBuffer As IMFMediaBuffer)
    End Interface

    <Runtime.InteropServices.ComImport, Runtime.InteropServices.InterfaceType(Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown), Runtime.InteropServices.Guid("045FA593-8799-42b8-BC8D-8968C6453507")>
    Public Interface IMFMediaBuffer
        Sub Lock(ByRef ppbBuffer As IntPtr, ByRef pcbMaxLength As Integer, ByRef pcbCurrentLength As Integer)
        Sub Unlock()
        Sub GetCurrentLength(ByRef pcbCurrentLength As Integer)
        Sub SetCurrentLength(cbCurrentLength As Integer)
        Sub GetMaxLength(ByRef pcbMaxLength As Integer)
    End Interface

    <Runtime.InteropServices.ComImport, Runtime.InteropServices.InterfaceType(Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown), Runtime.InteropServices.Guid("ad4c1b00-4bf7-422f-9175-756693d9130d")>
    Public Interface IMFByteStream
        'virtual HRESULT STDMETHODCALLTYPE GetCapabilities(/*[out]*/ __RPC__out DWORD *pdwCapabilities) = 0;
        Sub GetCapabilities(ByRef pdwCapabiities As Integer)
        'virtual HRESULT STDMETHODCALLTYPE GetLength(/*[out]*/ __RPC__out QWORD *pqwLength) = 0;
        Sub GetLength(ByRef pqwLength As Long)
        'virtual HRESULT STDMETHODCALLTYPE SetLength(/*[in]*/ QWORD qwLength) = 0;
        Sub SetLength(qwLength As Long)
        'virtual HRESULT STDMETHODCALLTYPE GetCurrentPosition(/*[out]*/ __RPC__out QWORD *pqwPosition) = 0;
        Sub GetCurrentPosition(ByRef pqwPosition As Long)
        'virtual HRESULT STDMETHODCALLTYPE SetCurrentPosition(/*[in]*/ QWORD qwPosition) = 0;
        Sub SetCurrentPosition(qwPosition As Long)
        'virtual HRESULT STDMETHODCALLTYPE IsEndOfStream(/*[out]*/ __RPC__out BOOL *pfEndOfStream) = 0;
        Sub IsEndOfStream(<Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.Bool)> ByRef pfEndOfStream As Boolean)
        'virtual HRESULT STDMETHODCALLTYPE Read(/*[size_is][out]*/ __RPC__out_ecount_full(cb) BYTE *pb, /*[in]*/ ULONG cb, /*[out]*/ __RPC__out ULONG *pcbRead) = 0;
        Sub Read(pb As IntPtr, cb As Integer, ByRef pcbRead As Integer)
        'virtual /*[local]*/ HRESULT STDMETHODCALLTYPE BeginRead(/*[out]*/ _Out_writes_bytes_(cb)  BYTE *pb, /*[in]*/ ULONG cb, /*[in]*/ IMFAsyncCallback *pCallback, /*[in]*/ IUnknown *punkState) = 0;
        Sub BeginRead(pb As IntPtr, cb As Integer, pCallback As IntPtr, punkState As IntPtr)
        'virtual /*[local]*/ HRESULT STDMETHODCALLTYPE EndRead(/*[in]*/ IMFAsyncResult *pResult, /*[out]*/ _Out_  ULONG *pcbRead) = 0;
        Sub EndRead(pResult As IntPtr, ByRef pcbRead As Integer)
        'virtual HRESULT STDMETHODCALLTYPE Write(/*[size_is][in]*/ __RPC__in_ecount_full(cb) const BYTE *pb, /*[in]*/ ULONG cb, /*[out]*/ __RPC__out ULONG *pcbWritten) = 0;
        Sub Write(pb As IntPtr, cb As Integer, ByRef pcbWritten As Integer)
        'virtual /*[local]*/ HRESULT STDMETHODCALLTYPE BeginWrite(/*[in]*/ _In_reads_bytes_(cb)  const BYTE *pb, /*[in]*/ ULONG cb, /*[in]*/ IMFAsyncCallback *pCallback, /*[in]*/ IUnknown *punkState) = 0;
        Sub BeginWrite(pb As IntPtr, cb As Integer, pCallback As IntPtr, punkState As IntPtr)
        'virtual /*[local]*/ HRESULT STDMETHODCALLTYPE EndWrite(/*[in]*/ IMFAsyncResult *pResult, /*[out]*/ _Out_  ULONG *pcbWritten) = 0;
        Sub EndWrite(pResult As IntPtr, ByRef pcbWritten As Integer)
        'virtual HRESULT STDMETHODCALLTYPE Seek(/*[in]*/ MFBYTESTREAM_SEEK_ORIGIN SeekOrigin, /*[in]*/ LONGLONG llSeekOffset, /*[in]*/ DWORD dwSeekFlags, /*[out]*/ __RPC__out QWORD *pqwCurrentPosition) = 0;
        Sub Seek(SeekOrigin As Integer, llSeekOffset As Long, dwSeekFlags As Integer, ByRef pqwCurrentPosition As Long)
        'virtual HRESULT STDMETHODCALLTYPE Flush( void) = 0;
        Sub Flush()
        'virtual HRESULT STDMETHODCALLTYPE Close( void) = 0;
        Sub Close()
    End Interface

End Module

Class MyForm
    Inherits System.Windows.Forms.Form

    Public bmp As System.Drawing.Bitmap
    Public g As Drawing.Graphics

    Sub New()
        Dim sz = SizeFromClientSize(New Drawing.Size(512, 512))
        Width = sz.Width
        Height = sz.Height
        SetStyle(System.Windows.Forms.ControlStyles.UserPaint, True)
        SetStyle(System.Windows.Forms.ControlStyles.AllPaintingInWmPaint, True)
        SetStyle(System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer, True)
    End Sub

    Protected Overrides Sub OnPaint(e As System.Windows.Forms.PaintEventArgs)
        SyncLock bmp
            e.Graphics.DrawImageUnscaled(bmp, 0, 0)
        End SyncLock
    End Sub

End Class





Class AudioBuffer
    Public arr As Short()
    Public power_left(5) As Double
    Public power_right(5) As Double
    Public power_vocals As Double
    Public power_music As Double
End Class


Class FFT_VB
    ' This code is an adaption+optimization of FFT code by Gerald T. Beauregard, which he placed under the MIT-license.
    ' The following license comes from his code.

    ' -------------------------------------------------------------------------
    ' *** THIS LICENSE GOVERNS THE SOURCE-CODE FOR "FFT_VB_FASTER" BUT NOTHING ELSE ***
    ' 
    ' FFT_VB_FASTER: Performs an in-place complex FFT.
    ' Based on original work released under the MIT License (c) 2010 Gerald T. Beauregard
    ' Derivative work also released under the MIT license (c) 2012 Lucian Wischik
    '
    ' Permission is hereby granted, free of charge, to any person obtaining a copy of the class FFT_VB_FASTER (the "Software"),
    ' to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish,
    ' distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
    ' subject to the following conditions:
    '
    ' The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
    '
    ' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
    ' OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
    ' LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR
    ' IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    ' -----------------------------------------------------------------------------

    ' TODO: Use a "real-only FFT" of size 512, rather than this 1024-element FFT. That will make it run twice as fast. Here's how:
    ' http://www.katjaas.nl/realFFT/realFFT2.html
    ' http://processors.wiki.ti.com/index.php/Efficient_FFT_Computation_of_Real_Input

    Class FFTElement
        Public re As Single, im As Single
        Public [next] As FFTElement ' on the CLR, using a linked-list is faster than array-element-access!
    End Class

    Private Const nElements = 1024, lognElements = 10
    Private Elements(nElements - 1) As FFTElement
    Private ElementsIndirection(nElements - 1) As FFTElement ' used for unpacking elements, since they come out of FFT in the wrong order

    Sub New()
        For k = nElements - 1 To 0 Step -1
            Dim [next] = If(k = nElements - 1, Nothing, Elements(k + 1))
            Elements(k) = New FFTElement With {.next = [next]}
        Next k
        For k = 0 To nElements - 1
            Dim x = k, bitreverse = 0
            For i = 0 To lognElements - 1
                bitreverse <<= 1
                bitreverse = bitreverse Or (x And 1)
                x >>= 1
            Next
            ElementsIndirection(bitreverse) = Elements(k)
        Next
    End Sub

    Sub RunAll(buf As AudioBuffer)
        CopyIntoComplex(buf, 0)
        DoFFT()
        GetBins(buf.power_left)
        '
        CopyIntoComplex(buf, 1)
        DoFFT()
        GetBins(buf.power_right)
        '
        ComputeKaraoke(buf)
    End Sub

    Sub CopyIntoComplex(buf As AudioBuffer, offset As Integer)
        For i = 0 To nElements - 1
            Elements(i).re = buf.arr(i * 2 + offset) : Elements(i).im = 0
        Next
    End Sub

    Sub GetBins(bins As Double())
        For i = 0 To 5
            Dim fmin = 450 + i * 344, fmax = fmin + 344
            Dim jmin = nElements * fmin \ 44100, jmax = nElements * fmax \ 44100
            Dim tot = 0.0
            For j = jmin To jmax - 1
                Dim e = ElementsIndirection(j)
                tot += e.re * e.re + e.im * e.im
            Next
            Dim av = tot / (jmax - jmin)
            bins(i) = Math.Pow(av, 0.25)
        Next
    End Sub

    Sub ComputeKaraoke(buf As AudioBuffer)
        Dim tvoc = 0.0F, tmus = 0.0F
        For i = 0 To nElements - 1
            Dim left As Single = buf.arr(i * 2), right As Single = buf.arr(i * 2 + 1)
            Dim voc = (left + right) / 2, mus = left - right
            tvoc += voc * voc : tmus += mus * mus
        Next
        buf.power_vocals = Math.Sqrt(tvoc / nElements) : buf.power_music = Math.Sqrt(tmus / nElements)
    End Sub

    Private Sub DoFFT()
        Dim numFlies = nElements >> 1, span = nElements >> 1, spacing = nElements, wIndexStep = 1
        For stage = 0 To lognElements - 1
            Dim wAngleInc = wIndexStep * -2.0 * Math.PI / nElements, wMulRe = CSng(Math.Cos(wAngleInc)), wMulIm = CSng(Math.Sin(wAngleInc))
            For start = 0 To nElements - 1 Step spacing
                Dim xTop = Elements(start), xBot = Elements(start + span), wRe = 1.0F, wIm = 0.0F
                For flyCount = 0 To numFlies - 1
                    Dim xTopRe = xTop.re, xTopIm = xTop.im, xBotRe = xBot.re, xBotIm = xBot.im
                    xTop.re = xTopRe + xBotRe
                    xTop.im = xTopIm + xBotIm
                    xBotRe = xTopRe - xBotRe
                    xBotIm = xTopIm - xBotIm
                    xBot.re = xBotRe * wRe - xBotIm * wIm
                    xBot.im = xBotRe * wIm + xBotIm * wRe
                    xTop = xTop.next
                    xBot = xBot.next
                    Dim tRe = wRe
                    wRe = wRe * wMulRe - wIm * wMulIm
                    wIm = tRe * wMulIm + wIm * wMulRe
                Next
            Next
            numFlies >>= 1 : span >>= 1 : spacing >>= 1 : wIndexStep <<= 1
        Next
    End Sub

End Class


