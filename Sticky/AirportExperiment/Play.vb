Option Strict On
' IAudioClient sample, (c) 2012 Lucian Wischik
' ------------------------------------------------
' In Windows 8 store apps, the waveOut family of functions are no longer available.
' The closest alternative is IAudioClient. In this sample we construct PCM buffers by
' writing to a memory array, and send them to the IAudioClient.
' IAudioClient has a simple sample here: http://msdn.microsoft.com/en-us/library/windows/desktop/dd316756(v=vs.85).aspx
' For Win8 app store, you obtain IAudioClient slightly differently as in this sample: http://code.msdn.microsoft.com/windowsapps/Windows-Audio-Session-22dcab6b

Module Play

    'Sub Main()
    '    Dim p As New Progress(Of String)(Sub(s) Console.WriteLine(s))
    '    TestFromConsoleAsync(p).Wait()
    'End Sub

    'Async Function TestFromConsoleAsync(progress As IProgress(Of String)) As Task
    '    Dim enumerator As New MMDeviceEnumerator()
    '    Dim pEnum = CType(enumerator, IMMDeviceEnumerator)
    '    Dim pDevices As IMMDeviceCollection = Nothing : pEnum.EnumAudioEndpoints(EDataFlow.eRender, DeviceStateFlags.DEVICE_STATE_ACTIVE, pDevices)
    '    ' We might enumerate the devices, or just pick the default...
    '    ' Dim pcDevices = 0 : pDevices.GetCount(pcDevices)
    '    ' Dim pDevice As IMMDevice = Nothing : pDevices.Item(pcDevices - 1, pDevice)
    '    Dim pDevice As IMMDevice = Nothing : pEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eConsole, pDevice)
    '    ' Some optional code to get properties about this device
    '    Dim pProps As IPropertyStore = Nothing : pDevice.OpenPropertyStore(StgmMode.STGM_READ, pProps)
    '    Dim varName As PROPVARIANT = Nothing : pProps.GetValue(PKEY_Device_FriendlyName, varName) : progress.Report(CStr(varName.Value)) : PropVariantClear(varName)
    '    Runtime.InteropServices.Marshal.ReleaseComObject(pProps) : pProps = Nothing

    '    Dim deviceId As String = Nothing : pDevice.GetId(deviceId)
    '    Dim endpointId = "\\?\SWD#MMDEVAPI#" & deviceId & "#{" & DEVINTERFACE_AUDIO_RENDER.ToString() & "}" ' The format of this id is undocumented
    '    Await PlaySoundOnAudioClientAsync(endpointId, progress)

    '    Runtime.InteropServices.Marshal.ReleaseComObject(pDevice) : pDevice = Nothing
    '    Runtime.InteropServices.Marshal.ReleaseComObject(pDevices) : pDevices = Nothing
    '    Runtime.InteropServices.Marshal.ReleaseComObject(pEnum) : pEnum = Nothing
    '    Runtime.InteropServices.Marshal.ReleaseComObject(enumerator) : enumerator = Nothing
    'End Function

    Async Function TestFromUWPAsync(progress As IProgress(Of String)) As Task
        Dim endpointId = Windows.Media.Devices.MediaDevice.GetDefaultAudioRenderId(Windows.Media.Devices.AudioDeviceRole.Default)
        ' We can also enumerate devices as follows:
        Dim audioSelector = Windows.Media.Devices.MediaDevice.GetAudioRenderSelector()
        Dim devices = Await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(audioSelector, {PKEY_AudioEndpoint_Supports_EventDriven_Mode.ToString()})
        For Each device In devices
            progress.Report(If(endpointId = device.Id, "* ", "") & device.Name)
        Next

        Await PlaySoundOnAudioClientAsync(endpointId, progress)
    End Function


    Async Function PlaySoundOnAudioClientAsync(endpointId As String, progress As IProgress(Of String)) As Task
        Dim icbh As New ActivateAudioInterfaceCompletionHandler
        Dim activationOperation As IActivateAudioInterfaceAsyncOperation = Nothing : ActivateAudioInterfaceAsync(endpointId, IID_IAudioClient, Nothing, icbh, activationOperation)
        Await icbh
        Dim hr As Integer = 0, unk As Object = Nothing : activationOperation.GetActivateResult(hr, unk) : Runtime.InteropServices.Marshal.ThrowExceptionForHR(hr, New IntPtr(-1))
        Dim pAudioClient = CType(unk, IAudioClient)
        Runtime.InteropServices.Marshal.ReleaseComObject(activationOperation) : activationOperation = Nothing

        Dim wfx As New WAVEFORMATEX With {.wFormatTag = 1, .nChannels = 2, .nSamplesPerSec = 44100, .wBitsPerSample = 16, .nBlockAlign = 4, .nAvgBytesPerSec = 44100 * 4, .cbSize = 0}
        ' Optional alternative ways we could pick wave-format... (note that IsFormatSupported will return pwfx=Nothing if wfx was satisfactory)
        Dim pwfx_default As IntPtr = Nothing : pAudioClient.GetMixFormat(pwfx_default) ' we could ask it for its preferred format
        Dim pwfx As IntPtr = Nothing : pAudioClient.IsFormatSupported(AUDCLNT_SHAREMODE.AUDCLNT_SHAREMODE_SHARED, wfx, pwfx)
        If pwfx_default <> Nothing Then Runtime.InteropServices.Marshal.FreeCoTaskMem(pwfx_default) : pwfx_default = Nothing
        If pwfx <> Nothing Then Runtime.InteropServices.Marshal.FreeCoTaskMem(pwfx)
        '
        pAudioClient.Initialize(AUDCLNT_SHAREMODE.AUDCLNT_SHAREMODE_SHARED, AUDCLNT_FLAGS.AUDCLNT_STREAMFLAGS_EVENTCALLBACK Or AUDCLNT_FLAGS.AUDCLNT_STREAMFLAGS_NOPERSIST, 10000000, 0, wfx, Nothing)
        Dim hEvent = CreateEventEx(Nothing, Nothing, 0, EventAccess.EVENT_ALL_ACCESS)
        pAudioClient.SetEventHandle(hEvent)
        Dim bufferFrameCount As Integer = 0 : pAudioClient.GetBufferSize(bufferFrameCount)
        Dim pRenderClient As IAudioRenderClient = Nothing : pAudioClient.GetService(IID_IAudioRenderClient, pRenderClient)
        Dim buf = New Short(bufferFrameCount * 2) {}

        Dim MaryHadALittleLamb = {247, 220, 196, 220, 247, 247, 247, 220, 220, 220, 247, 294, 294}
        Dim phase = 0.0, imusic = 0

        pAudioClient.Start()
        Dim cts As New Threading.CancellationTokenSource
        While Await WaitForSingleObjectAsync(hEvent, cts.Token)
            Dim numFramesPadding = 0 : pAudioClient.GetCurrentPadding(numFramesPadding)
            Dim numFramesAvailable = bufferFrameCount - numFramesPadding
            If numFramesAvailable = 0 Then Continue While
            Dim pData As IntPtr = Nothing : pRenderClient.GetBuffer(numFramesAvailable, pData)
            For b = 0 To numFramesAvailable - 1
                imusic += 1
                Dim freq As Double = If((imusic \ 4096) Mod 8 = 0 OrElse (imusic \ 4096) \ 8 >= MaryHadALittleLamb.Length, 0, MaryHadALittleLamb(imusic \ 4096 \ 8))
                If (imusic Mod 4096) = 0 Then progress.Report(freq & "Hz")
                phase += freq * 2.0 * 3.1415 / 44100.0
                Dim amp = CShort(Math.Sin(phase) * 0.5 * Short.MaxValue - 1)
                buf(b * 2 + 0) = amp
                buf(b * 2 + 1) = amp
            Next
            If imusic \ 4096 \ 8 >= MaryHadALittleLamb.Length Then cts.Cancel()
            Runtime.InteropServices.Marshal.Copy(buf, 0, pData, numFramesAvailable * 2)
            pRenderClient.ReleaseBuffer(numFramesAvailable, 0)
        End While
        ' and wait until the buffer plays out to the end
        While True
            Dim numFramesPadding = 0 : pAudioClient.GetCurrentPadding(numFramesPadding)
            If numFramesPadding = 0 Then Exit While
            Await Task.Delay(20)
        End While
        pAudioClient.Stop()

        Runtime.InteropServices.Marshal.ReleaseComObject(pRenderClient) : pRenderClient = Nothing
        Runtime.InteropServices.Marshal.ReleaseComObject(pAudioClient) : pAudioClient = Nothing
        CloseHandle(hEvent) : hEvent = Nothing
    End Function

    Class ActivateAudioInterfaceCompletionHandler
        Implements IActivateAudioInterfaceCompletionHandler, IAgileObject

        Public Sub ActivateCompleted(activateOperation As IActivateAudioInterfaceAsyncOperation) Implements IActivateAudioInterfaceCompletionHandler.ActivateCompleted
            tcs.SetResult(Nothing)
        End Sub

        Private tcs As New TaskCompletionSource(Of Object)
        Public Function GetAwaiter() As Runtime.CompilerServices.TaskAwaiter
            Return CType(tcs.Task, Task).GetAwaiter()
        End Function
    End Class

    Function WaitForSingleObjectAsync(hEvent As IntPtr, Optional cancel As Threading.CancellationToken = Nothing) As Task(Of Boolean)
        Return Task.Run(
            Function()
                While True
                    If WaitForSingleObjectEx(hEvent, 10, True) = 0 Then Return True
                    If cancel.IsCancellationRequested Then Exit While
                End While
                Return False
            End Function)
    End Function

    <Runtime.InteropServices.DllImport("mfplat.dll", ExactSpelling:=True, PreserveSig:=False)>
    Sub MFStartup(Version As Integer, Optional dwFlags As Integer = 0)
    End Sub
    <Runtime.InteropServices.DllImport("mfplat.dll", ExactSpelling:=True, PreserveSig:=False)>
    Public Sub MFShutdown()
    End Sub
    <Runtime.InteropServices.DllImport("Mmdevapi.dll", ExactSpelling:=True, PreserveSig:=False)>
    Public Sub ActivateAudioInterfaceAsync(<Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPWStr)> deviceInterfacePath As String, <Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> riid As Guid, activationParams As IntPtr, completionHandler As IActivateAudioInterfaceCompletionHandler, ByRef activationOperation As IActivateAudioInterfaceAsyncOperation)
    End Sub
    <Runtime.InteropServices.DllImport("ole32.dll", ExactSpelling:=True, PreserveSig:=False)>
    Public Sub PropVariantClear(ByRef pvar As PROPVARIANT)
    End Sub
    <Runtime.InteropServices.DllImport("kernel32.dll", CharSet:=Runtime.InteropServices.CharSet.Unicode, ExactSpelling:=False, PreserveSig:=True, SetLastError:=True)>
    Public Function CreateEventEx(lpEventAttributes As IntPtr, lpName As IntPtr, dwFlags As Integer, dwDesiredAccess As EventAccess) As IntPtr
    End Function
    <Runtime.InteropServices.DllImport("kernel32.dll", ExactSpelling:=True, PreserveSig:=True, SetLastError:=True)>
    Public Function CloseHandle(hObject As IntPtr) As Boolean
    End Function
    <Runtime.InteropServices.DllImport("kernel32", ExactSpelling:=True, PreserveSig:=True, SetLastError:=True)>
    Function WaitForSingleObjectEx(hEvent As IntPtr, milliseconds As Integer, bAlertable As Boolean) As Integer
    End Function

    Const MF_SDK_VERSION As Integer = &H2
    Const MF_API_VERSION As Integer = &H70
    Const MF_VERSION As Integer = (MF_SDK_VERSION << 16) Or MF_API_VERSION
    ReadOnly PKEY_Device_FriendlyName As New PROPERTYKEY With {.fmtid = New Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), .pid = 14}
    ReadOnly PKEY_Device_DeviceDesc As New PROPERTYKEY With {.fmtid = New Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), .pid = 2}
    ReadOnly PKEY_AudioEndpoint_Supports_EventDriven_Mode As New PROPERTYKEY With {.fmtid = New Guid("1da5d803-d492-4edd-8c23-e0c0ffee7f0e"), .pid = 7}
    ReadOnly DEVINTERFACE_AUDIO_RENDER As New Guid("E6327CAD-DCEC-4949-AE8A-991E976A79D2")
    ReadOnly DEVINTERFACE_AUDIO_CAPTURE As New Guid("2EEF81BE-33FA-4800-9670-1CD474972C3F")
    ReadOnly IID_IAudioClient As New Guid("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2")
    ReadOnly IID_IAudioRenderClient As New Guid("F294ACFC-3146-4483-A7BF-ADDCA7C260E2")

    <Runtime.InteropServices.ComImport, Runtime.InteropServices.Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")> Class MMDeviceEnumerator : End Class

    <Runtime.InteropServices.ComImport, Runtime.InteropServices.InterfaceType(Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown), Runtime.InteropServices.Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")>
    Public Interface IMMDeviceEnumerator
        Sub EnumAudioEndpoints(dataflow As EDataFlow, dwStateMask As DeviceStateFlags, ByRef ppDevices As IMMDeviceCollection)
        Sub GetDefaultAudioEndpoint(dataflow As EDataFlow, role As ERole, ByRef ppDevice As IMMDevice)
        Sub GetDevice(<Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPWStr)> pwstrId As String, ByRef ppDevice As IntPtr)
        Sub RegisterEndpointNotificationCallback(pClient As IntPtr)
        Sub UnregisterEndpointNotificationCallback(pClient As IntPtr)
    End Interface

    <Runtime.InteropServices.ComImport, Runtime.InteropServices.InterfaceType(Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown), Runtime.InteropServices.Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")>
    Public Interface IMMDeviceCollection
        Sub GetCount(ByRef pcDevices As Integer)
        Sub Item(nDevice As Integer, ByRef ppDevice As IMMDevice)
    End Interface

    <Runtime.InteropServices.ComImport, Runtime.InteropServices.InterfaceType(Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown), Runtime.InteropServices.Guid("D666063F-1587-4E43-81F1-B948E807363F")>
    Public Interface IMMDevice
        Sub Activate(<Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> iid As Guid, dwClsCtx As Integer, pActivationParams As IntPtr, ByRef ppInterface As IAudioClient)
        Sub OpenPropertyStore(stgmAccess As Integer, ByRef ppProperties As IPropertyStore)
        Sub GetId(<Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPWStr)> ByRef ppstrId As String)
        Sub GetState(ByRef pdwState As Integer)
    End Interface

    <Runtime.InteropServices.ComImport, Runtime.InteropServices.InterfaceType(Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown), Runtime.InteropServices.Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99")>
    Public Interface IPropertyStore
        'virtual HRESULT STDMETHODCALLTYPE GetCount(/*[out]*/ __RPC__out DWORD *cProps)
        Sub GetCount(ByRef cProps As Integer)
        'virtual HRESULT STDMETHODCALLTYPE GetAt(/*Runtime.InteropServices.In*/ DWORD iProp, /*[out]*/ __RPC__out PROPERTYKEY *pkey)
        Sub GetAt(iProp As Integer, ByRef pkey As IntPtr)
        'virtual HRESULT STDMETHODCALLTYPE GetValue(/*Runtime.InteropServices.In*/ __RPC__in REFPROPERTYKEY key, /*[out]*/ __RPC__out PROPVARIANT *pv)
        Sub GetValue(ByRef key As PROPERTYKEY, ByRef pv As PROPVARIANT)
        'virtual HRESULT STDMETHODCALLTYPE SetValue(/*Runtime.InteropServices.In*/ __RPC__in REFPROPERTYKEY key, /*Runtime.InteropServices.In*/ __RPC__in REFPROPVARIANT propvar)
        Sub SetValue(ByRef key As PROPERTYKEY, ByRef propvar As IntPtr)
        'virtual HRESULT STDMETHODCALLTYPE Commit()
        Sub Commit()
    End Interface

    <Runtime.InteropServices.ComImport, Runtime.InteropServices.InterfaceType(Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown), Runtime.InteropServices.Guid("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2")>
    Public Interface IAudioClient
        'virtual HRESULT STDMETHODCALLTYPE Initialize(/*[in]*/ _In_  AUDCLNT_SHAREMODE ShareMode,  /*[in]*/ _In_  DWORD StreamFlags, /*[in]*/ _In_  REFERENCE_TIME hnsBufferDuration, /*[in]*/ _In_  REFERENCE_TIME hnsPeriodicity, /*[in]*/ _In_  const WAVEFORMATEX *pFormat, /*[in]*/ _In_opt_  LPCGUID AudioSessionGuid) = 0;
        Sub Initialize(ShareMode As AUDCLNT_SHAREMODE, StreamFlags As AUDCLNT_FLAGS, hnsBufferDuration As Long, hnsPeriodicity As Long, ByRef pFormat As WAVEFORMATEX, AudioSessionGuid As IntPtr)
        'virtual HRESULT STDMETHODCALLTYPE GetBufferSize(/*[out]*/ _Out_  UINT32 *pNumBufferFrames) = 0;
        Sub GetBufferSize(ByRef pNumBufferFrames As Integer)
        'virtual HRESULT STDMETHODCALLTYPE GetStreamLatency(/*[out]*/ _Out_  REFERENCE_TIME *phnsLatency) = 0;
        Sub GetStreamLatency(ByRef phnsLatency As Long)
        'virtual HRESULT STDMETHODCALLTYPE GetCurrentPadding(/*[out]*/ _Out_  UINT32 *pNumPaddingFrames) = 0;
        Sub GetCurrentPadding(ByRef pNumPaddingFrames As Integer)
        'virtual HRESULT STDMETHODCALLTYPE IsFormatSupported(/*[in]*/ _In_  AUDCLNT_SHAREMODE ShareMode, /*[in]*/ _In_  const WAVEFORMATEX *pFormat, /*[unique][out]*/ _Out_opt_  WAVEFORMATEX **ppClosestMatch) = 0;
        Sub IsFormatSupported(ShareMode As AUDCLNT_SHAREMODE, ByRef pFormat As WAVEFORMATEX, ByRef ppClosestMatch As IntPtr)
        'virtual HRESULT STDMETHODCALLTYPE GetMixFormat(/*[out]*/ _Out_  WAVEFORMATEX **ppDeviceFormat) = 0;
        Sub GetMixFormat(ByRef ppDeviceFormat As IntPtr)
        'virtual HRESULT STDMETHODCALLTYPE GetDevicePeriod(/*[out]*/ _Out_opt_  REFERENCE_TIME *phnsDefaultDevicePeriod, /*[out]*/ _Out_opt_  REFERENCE_TIME *phnsMinimumDevicePeriod) = 0;
        Sub GetDevicePeriod(ByRef phnsDefaultDevicePeriod As Long, ByRef phnsMinimumDevicePeriod As Long)
        'virtual HRESULT STDMETHODCALLTYPE Start( void) = 0;
        Sub Start()
        'virtual HRESULT STDMETHODCALLTYPE Stop( void) = 0;
        Sub [Stop]()
        'virtual HRESULT STDMETHODCALLTYPE Reset( void) = 0;
        Sub Reset()
        'virtual HRESULT STDMETHODCALLTYPE SetEventHandle(/*[in]*/ HANDLE eventHandle) = 0;
        Sub SetEventHandle(eventHandle As IntPtr)
        'virtual HRESULT STDMETHODCALLTYPE GetService(/*[in]*/ _In_  REFIID riid, /*[iid_is][out]*/ _Out_  void **ppv) = 0;
        Sub GetService(<Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPStruct)> riid As Guid, ByRef ppv As IAudioRenderClient)
    End Interface

    <Runtime.InteropServices.ComImport, Runtime.InteropServices.InterfaceType(Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown), Runtime.InteropServices.Guid("F294ACFC-3146-4483-A7BF-ADDCA7C260E2")>
    Public Interface IAudioRenderClient
        'virtual HRESULT STDMETHODCALLTYPE GetBuffer(/*[in]*/ _In_  UINT32 NumFramesRequested, /*[out]*/ _Outptr_result_buffer_(_Inexpressible_("NumFramesRequested * pFormat->nBlockAlign"))  BYTE **ppData) = 0;
        Sub GetBuffer(NumFramesRequested As Integer, ByRef ppData As IntPtr)
        'virtual HRESULT STDMETHODCALLTYPE ReleaseBuffer(/*[in]*/ _In_  UINT32 NumFramesWritten, /*[in]*/ _In_  DWORD dwFlags) = 0;
        Sub ReleaseBuffer(NumFramesWritten As Integer, dwFlags As Integer)
    End Interface

    <Runtime.InteropServices.ComImport, Runtime.InteropServices.InterfaceType(Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown), Runtime.InteropServices.Guid("41D949AB-9862-444A-80F6-C261334DA5EB")>
    Public Interface IActivateAudioInterfaceCompletionHandler
        'virtual HRESULT STDMETHODCALLTYPE ActivateCompleted(/*[in]*/ _In_  IActivateAudioInterfaceAsyncOperation *activateOperation) = 0;
        Sub ActivateCompleted(activateOperation As IActivateAudioInterfaceAsyncOperation)
    End Interface

    <Runtime.InteropServices.ComImport, Runtime.InteropServices.InterfaceType(Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown), Runtime.InteropServices.Guid("72A22D78-CDE4-431D-B8CC-843A71199B6D")>
    Public Interface IActivateAudioInterfaceAsyncOperation
        'virtual HRESULT STDMETHODCALLTYPE GetActivateResult(/*[out]*/ _Out_  HRESULT *activateResult, /*[out]*/ _Outptr_result_maybenull_  IUnknown **activatedInterface) = 0;
        Sub GetActivateResult(ByRef activateResult As Integer, <Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.IUnknown)> ByRef activateInterface As Object)
    End Interface

    <Runtime.InteropServices.ComImport, Runtime.InteropServices.InterfaceType(Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown), Runtime.InteropServices.Guid("94ea2b94-e9cc-49e0-c0ff-ee64ca8f5b90")>
    Public Interface IAgileObject
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

    <Runtime.InteropServices.StructLayout(Runtime.InteropServices.LayoutKind.Sequential, Pack:=1)> Public Structure PROPVARIANT
        Dim vt As UShort
        Dim wReserved1 As UShort
        Dim wReserved2 As UShort
        Dim wReserved3 As UShort
        Dim p As IntPtr
        Dim p2 As Integer
        ReadOnly Property Value As Object
            Get
                Select Case vt
                    Case 31 : Return Runtime.InteropServices.Marshal.PtrToStringUni(p) ' VT_LPWSTR
                    Case Else
                        Throw New NotImplementedException
                End Select
            End Get
        End Property
    End Structure

    <Runtime.InteropServices.StructLayout(Runtime.InteropServices.LayoutKind.Sequential, Pack:=1)> Public Structure PROPERTYKEY
        Dim fmtid As Guid
        Dim pid As Integer
        Public Overrides Function ToString() As String
            Return "{" & fmtid.ToString() & "} " & pid.ToString()
        End Function
    End Structure

    Enum EDataFlow
        eRender = 0
        eCapture = 1
        eAll = 2
        EDataFlow_enum_count = 3
    End Enum

    Enum ERole
        eConsole = 0
        eMultimedia = 1
        eCommunications = 2
        ERole_enum_count = 3
    End Enum

    Enum StgmMode
        STGM_READ = 0
        STGM_WRITE = 1
        STGM_READWRITE = 2
    End Enum

    Enum AUDCLNT_SHAREMODE
        AUDCLNT_SHAREMODE_SHARED = 0
        AUDCLNT_SHAREMODE_EXCLUSIVE = 1
    End Enum

    <Flags> Enum DeviceStateFlags
        DEVICE_STATE_ACTIVE = 1
        DEVICE_STATE_DISABLED = 2
        DEVICE_STATE_NOTPRESENT = 4
        DEVICE_STATE_UNPLUGGED = 8
        DEVICE_STATEMASK_ALL = 15
    End Enum

    <Flags> Enum AUDCLNT_FLAGS
        AUDCLNT_STREAMFLAGS_CROSSPROCESS = &H10000
        AUDCLNT_STREAMFLAGS_LOOPBACK = &H20000
        AUDCLNT_STREAMFLAGS_EVENTCALLBACK = &H40000
        AUDCLNT_STREAMFLAGS_NOPERSIST = &H80000
        AUDCLNT_STREAMFLAGS_RATEADJUST = &H100000
        AUDCLNT_SESSIONFLAGS_EXPIREWHENUNOWNED = &H10000000
        AUDCLNT_SESSIONFLAGS_DISPLAY_HIDE = &H20000000
        AUDCLNT_SESSIONFLAGS_DISPLAY_HIDEWHENEXPIRED = &H40000000
    End Enum

    <Flags> Enum EventAccess
        STANDARD_RIGHTS_REQUIRED = &HF0000
        SYNCHRONIZE = &H100000
        EVENT_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED Or SYNCHRONIZE Or &H3
    End Enum

End Module

