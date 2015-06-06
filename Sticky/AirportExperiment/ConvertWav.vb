' MFSourceReader sample, (c) 2012 Lucian Wischik
' ------------------------------------------------
' MediaFoundation is an audio subsystem introduced in Vista, and allowed in Win8 store apps.
' It is an evolution of the old DirectShow. In this sample we stream data from a url into
' raw PCM, and then write that PCM data into a WAV file.
' It is largely based on Microsoft's C++ MFSourceReader sample http://msdn.microsoft.com/en-us/library/windows/desktop/dd757929(v=vs.85).aspx
' and borrowed a few pinvoke prototypes from Tamir Khason http://www.codeproject.com/Articles/239378/Video-encoder-and-metadata-reading-by-using-Window

Module ConvertWav

    Async Function TestFromUWPAsync() As Task(Of String)
        Dim file = Await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("output.wav", Windows.Storage.CreationCollisionOption.ReplaceExisting)
        Using dest = Await file.OpenStreamForWriteAsync()
            Await ConvertToWavAsync("ms-appx:///music/Bilbo.mp3", dest)
        End Using
        Return file.Path
    End Function

    Async Function ConvertToWavAsync(source As String, dest As IO.Stream) As Task
        MFStartup(MF_VERSION)

        ' IMFSourceReader is the component that does all the work
        Dim pReader As IMFSourceReader = Nothing : MFCreateSourceReaderFromURL(source, Nothing, pReader)

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

        ' It can tell us the authoritative wave format that it picked
        Dim pAudioType As IMFMediaType = Nothing : pReader.GetCurrentMediaType(MF_SOURCE_READER_FIRST_AUDIO_STREAM, pAudioType)
        Dim cbBlockSize = 0 : pAudioType.GetUINT32(MF_MT_AUDIO_BLOCK_ALIGNMENT, cbBlockSize)
        Dim cbBytesPerSecond = 0 : pAudioType.GetUINT32(MF_MT_AUDIO_AVG_BYTES_PER_SECOND, cbBytesPerSecond)
        Dim pWav As IntPtr, cbFormat As Integer : MFCreateWaveFormatExFromMFMediaType(pAudioType, pWav, cbFormat)
        Dim wfx = New Byte(cbFormat - 1) {} : Runtime.InteropServices.Marshal.Copy(pWav, wfx, 0, cbFormat)
        pReader.SetStreamSelection(MF_SOURCE_READER_FIRST_AUDIO_STREAM, True)
        Runtime.InteropServices.Marshal.ReleaseComObject(pAudioType) : pAudioType = Nothing

        ' We'll write up to 50 seconds of audio into a WAV file. WAV files have pretty straightforward headers
        Dim cbMaxAudioData = (cbBytesPerSecond * 50 \ cbBlockSize) * cbBlockSize
        Dim header As Byte() = {CByte(AscW("R"c)), CByte(AscW("I"c)), CByte(AscW("F"c)), CByte(AscW("F"c)), 0, 0, 0, 0,
                      CByte(AscW("W"c)), CByte(AscW("A"c)), CByte(AscW("V"c)), CByte(AscW("E"c)),
                      CByte(AscW("f"c)), CByte(AscW("m"c)), CByte(AscW("t"c)), CByte(AscW(" "c)),
                      CByte(cbFormat And 255), CByte((cbFormat >> 8) And 255), CByte((cbFormat >> 16) And 255), CByte((cbFormat >> 24) And 255)}
        Dim dataHeader As Byte() = {CByte(AscW("d"c)), CByte(AscW("a"c)), CByte(AscW("t"c)), CByte(AscW("a"c)), 0, 0, 0, 0}
        Await dest.WriteAsync(header, 0, header.Length)
        Await dest.WriteAsync(wfx, 0, wfx.Length)
        Await dest.WriteAsync(dataHeader, 0, dataHeader.Length)
        Runtime.InteropServices.Marshal.FreeCoTaskMem(pWav)
        Dim cbHeader = header.Length + cbFormat + dataHeader.Length

        ' Now a loop to get buffer after buffer from the MFSourceReader, and write it to disk:
        Dim cbAudioData = 0
        Do
            Dim pSample As IMFSample = Nothing, dwFlags As Integer : pReader.ReadSample(MF_SOURCE_READER_FIRST_AUDIO_STREAM, 0, 0, dwFlags, 0, pSample)
            If dwFlags <> 0 Then pSample = Nothing : Exit Do
            Dim pBuffer As IMFMediaBuffer = Nothing : pSample.ConvertToContiguousBuffer(pBuffer)
            Dim pAudioData As IntPtr, cbBuffer As Integer : pBuffer.Lock(pAudioData, Nothing, cbBuffer)
            Dim buf = New Byte(cbBuffer - 1) {} : Runtime.InteropServices.Marshal.Copy(pAudioData, buf, 0, cbBuffer)
            Await dest.WriteAsync(buf, 0, cbBuffer) : cbAudioData += cbBuffer
            pBuffer.Unlock()
            Runtime.InteropServices.Marshal.ReleaseComObject(pBuffer) : pBuffer = Nothing
            Runtime.InteropServices.Marshal.ReleaseComObject(pSample) : pSample = Nothing
            If cbAudioData >= cbMaxAudioData Then Exit Do
        Loop

        ' Some fields in the WAV file header need to be patched up, now that we know the correct sizes
        Dim cbRiffFileSize = cbHeader + cbAudioData - 8
        dest.Seek(4, IO.SeekOrigin.Begin) : Await dest.WriteAsync(BitConverter.GetBytes(cbRiffFileSize), 0, 4)
        dest.Seek(cbHeader - 4, IO.SeekOrigin.Begin) : Await dest.WriteAsync(BitConverter.GetBytes(cbAudioData), 0, 4)

        Runtime.InteropServices.Marshal.ReleaseComObject(pReader) : pReader = Nothing
        MFShutdown()
    End Function


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

    Const MF_SOURCE_READER_ALL_STREAMS As Integer = &HFFFFFFFE
    Const MF_SOURCE_READER_FIRST_AUDIO_STREAM As Integer = &HFFFFFFFD
    Const MF_SDK_VERSION As Integer = &H2
    Const MF_API_VERSION As Integer = &H70
    Const MF_VERSION As Integer = (MF_SDK_VERSION << 16) Or MF_API_VERSION
    ReadOnly MF_MT_MAJOR_TYPE As New Guid("48eba18e-f8c9-4687-bf11-0a74c9f96a8f")
    ReadOnly MF_MT_SUBTYPE As New Guid("f7e34c9a-42e8-4714-b74b-cb29d72c35e5")
    ReadOnly MF_MT_AUDIO_BLOCK_ALIGNMENT As New Guid("322de230-9eeb-43bd-ab7a-ff412251541d")
    ReadOnly MF_MT_AUDIO_AVG_BYTES_PER_SECOND As New Guid("1aab75c8-cfef-451c-ab95-ac034b8e1731")
    ReadOnly MF_MT_AUDIO_NUM_CHANNELS As New Guid("37e48bf5-645e-4c5b-89de-ada9e29b696a")
    ReadOnly MF_MT_AUDIO_SAMPLES_PER_SECOND As New Guid("5faeeae7-0290-4c31-9e8a-c534f68d9dba")
    ReadOnly MF_MT_AUDIO_BITS_PER_SAMPLE As New Guid("f2deb57f-40fa-4764-aa33-ed4f2d1ff669")
    ReadOnly MFMediaType_Audio As New Guid("73647561-0000-0010-8000-00AA00389B71")
    ReadOnly MFAudioFormat_PCM As New Guid("00000001-0000-0010-8000-00AA00389B71")

    <Runtime.InteropServices.ComImport, Runtime.InteropServices.InterfaceType(Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown), Runtime.InteropServices.Guid("70ae66f2-c809-4e4f-8915-bdcb406b7993")>
    Public Interface IMFSourceReader
        Sub GetStreamSelection(<Runtime.InteropServices.In> dwStreamIndex As Integer, <Runtime.InteropServices.Out> ByRef pSelected As Boolean)
        Sub SetStreamSelection(<Runtime.InteropServices.In> dwStreamIndex As Integer, <Runtime.InteropServices.In> pSelected As Boolean)
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
        Sub GetRepresentation(<Runtime.InteropServices.In> guidRepresentation As Guid, ByRef ppvRepresentation As IntPtr)
        Sub FreeRepresentation(<Runtime.InteropServices.In> guidRepresentation As Guid, <Runtime.InteropServices.In> pvRepresentation As IntPtr)
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

    <Runtime.InteropServices.StructLayout(Runtime.InteropServices.LayoutKind.Sequential, Pack:=1)> Structure WAVEFORMATEX
        Dim wFormatTag As Short
        Dim nChannels As Short
        Dim nSamplesPerSec As Integer
        Dim nAvgBytesPerSec As Integer
        Dim nBlockAlign As Short
        Dim wBitsPerSample As Short
        Dim cbSize As Short
    End Structure

    Function FCC(s As String) As Integer
        Return BitConverter.ToInt32({CByte(AscW(s(0))), CByte(AscW(s(1))), CByte(AscW(s(2))), CByte(AscW(s(3)))}, 0)
    End Function

End Module


