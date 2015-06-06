Option Strict On
Imports System.Threading.Tasks
Imports System.Runtime.InteropServices

Namespace Global

    Public Module Interop

        Public Function WaitForSingleObjectAsync(hEvent As IntPtr, Optional cancel As Threading.CancellationToken = Nothing) As Task
            cancel.ThrowIfCancellationRequested()
            Return Task.Run(Sub()
                                'Dim hCancelEvent = CreateEventEx(Nothing, Nothing, 0, EventAccess.EVENT_ALL_ACCESS)
                                'Dim lock As New Object
                                'cancel.Register(Public Sub()
                                '                    SyncLock lock
                                '                        If hCancelEvent <> Nothing Then SetEvent(hCancelEvent)
                                '                    End SyncLock
                                '                End Sub)
                                'Dim r = WaitForMultipleObjectsEx(2, {hEvent, hCancelEvent}, False, &HFFFFFFFF, True)
                                'SyncLock lock
                                '    CloseHandle(hEvent) : hCancelEvent = Nothing
                                'End SyncLock
                                Dim r = WaitForSingleObjectEx(hEvent, &HFFFFFFFF, True)
                                If r = 1 Then cancel.ThrowIfCancellationRequested()
                                If r <> 0 Then Throw New Exception("Unexpected event")
                            End Sub, cancel)
        End Function

        Public Class ActivateAudioInterfaceCompletionHandler
            Implements IActivateAudioInterfaceCompletionHandler, IAgileObject

            Private InitializeAction As Action(Of IAudioClient2)
            Private tcs As New TaskCompletionSource(Of IAudioClient2)

            Public Sub New(InitializeAction As Action(Of IAudioClient2))
                Me.InitializeAction = InitializeAction
            End Sub

            Private Sub ActivateCompleted(activateOperation As IActivateAudioInterfaceAsyncOperation) _
                       Implements IActivateAudioInterfaceCompletionHandler.ActivateCompleted
                ' First get the activation results, and see if anything bad happened then
                Dim hr As Integer = 0, unk As Object = Nothing : activateOperation.GetActivateResult(hr, unk)
                If hr <> 0 Then
                    tcs.TrySetException(Marshal.GetExceptionForHR(hr, New IntPtr(-1)))
                    Return
                End If

                Dim pAudioClient = CType(unk, IAudioClient2)

                ' Next try to call the client's (synchronous, blocking) initialization method.
                Try
                    InitializeAction(pAudioClient)
                    tcs.SetResult(pAudioClient)
                Catch ex As Exception
                    tcs.TrySetException(ex)
                End Try
            End Sub

            Public Function GetAwaiter() As Runtime.CompilerServices.TaskAwaiter(Of IAudioClient2)
                Return tcs.Task.GetAwaiter()
            End Function
        End Class

        <DllImport("mfplat.dll", ExactSpelling:=True, PreserveSig:=False)>
        Public Sub MFStartup(Version As Integer, Optional dwFlags As Integer = 0)
        End Sub

        <DllImport("mfplat.dll", ExactSpelling:=True, PreserveSig:=False)>
        Public Sub MFShutdown()
        End Sub

        <DllImport("mfplat.dll", ExactSpelling:=True, PreserveSig:=False)>
        Public Sub MFCreateMediaType(<Out> ByRef ppMFType As IMFMediaType)
        End Sub

        <DllImport("mfplat.dll", ExactSpelling:=True, PreserveSig:=False)>
        Public Sub MFCreateWaveFormatExFromMFMediaType(pMFType As IMFMediaType, _
           ByRef ppWF As IntPtr, <Out> ByRef pcbSize As Integer, Optional Flags As Integer = 0)
        End Sub

        <DllImport("mfreadwrite.dll", ExactSpelling:=True, PreserveSig:=False)>
        Public Sub MFCreateSourceReaderFromURL(<MarshalAs( _
       UnmanagedType.LPWStr)> pwszURL As String, _
       pAttributes As IntPtr, <Out> ByRef ppSourceReader As IMFSourceReader)
        End Sub

        <DllImport("mfreadwrite.dll", ExactSpelling:=True, PreserveSig:=False)>
        Public Sub MFCreateSourceReaderFromByteStream(pByteStream As IMFByteStream, _
       pAttributes As IntPtr, <Out> ByRef ppSourceReader As IMFSourceReader)
        End Sub

        <DllImport("mfplat.dll", ExactSpelling:=True, PreserveSig:=False)>
        Public Sub MFCreateMFByteStreamOnStreamEx(<MarshalAs( _
       UnmanagedType.IUnknown)> punkStream As Object, _
       <Out> ByRef ppByteStream As IMFByteStream)
        End Sub

        <DllImport("Mmdevapi.dll", ExactSpelling:=True, PreserveSig:=False)>
        Public Sub ActivateAudioInterfaceAsync(<MarshalAs( _
           UnmanagedType.LPWStr)> deviceInterfacePath As String, _
           <MarshalAs(UnmanagedType.LPStruct)> _
           riid As Guid, activationParams As IntPtr, completionHandler As  _
           IActivateAudioInterfaceCompletionHandler, _
           ByRef activationOperation As IActivateAudioInterfaceAsyncOperation)
        End Sub

        <DllImport("ole32.dll", _
                    ExactSpelling:=True, PreserveSig:=False)>
        Public Sub PropVariantClear(ByRef pvar As PROPVARIANT)
        End Sub

        <DllImport("kernel32.dll", CharSet:=CharSet.Unicode, ExactSpelling:=False, PreserveSig:=True, SetLastError:=True)>
        Public Function CreateEventEx(lpEventAttributes As IntPtr, lpName As IntPtr, _
           dwFlags As Integer, dwDesiredAccess As EventAccess) As IntPtr
        End Function

        <DllImport("kernel32.dll", ExactSpelling:=True, PreserveSig:=True, SetLastError:=True)>
        Public Function CloseHandle(hObject As IntPtr) As Boolean
        End Function

        <DllImport("kernel32", ExactSpelling:=True, PreserveSig:=True, SetLastError:=True)>
        Public Function WaitForSingleObjectEx(hEvent As IntPtr, milliseconds As Integer, bAlertable As Boolean) As Integer
        End Function

        <DllImport("kernel32", ExactSpelling:=True, PreserveSig:=True, SetLastError:=True)>
        Public Function WaitForMultipleObjectsEx(nCount As Integer, pHandles As IntPtr(), bWaitAll As Boolean, dwMilliseconds As Integer, bAlerable As Boolean) As Integer
        End Function

        <DllImport("kernel32", ExactSpelling:=True, PreserveSig:=True)>
        Public Function GetCurrentThread() As IntPtr
        End Function

        <DllImport("kernel32.dll", ExactSpelling:=True, PreserveSig:=True)>
        Public Function SetEvent(hEvent As IntPtr) As Boolean
        End Function

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
        Public ReadOnly PKEY_Device_FriendlyName As New PROPERTYKEY With {.fmtid = New Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), .pid = 14}
        Public ReadOnly PKEY_Device_DeviceDesc As New PROPERTYKEY With {.fmtid = New Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), .pid = 2}
        Public ReadOnly PKEY_AudioEndpoint_Supports_EventDriven_Mode As New PROPERTYKEY With {.fmtid = New Guid("1da5d803-d492-4edd-8c23-e0c0ffee7f0e"), .pid = 7}
        Public ReadOnly IID_IAudioClient As New Guid("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2")
        Public ReadOnly IID_IAudioClient2 As New Guid("726778CD-F60A-4eda-82DE-E47610CD78AA")
        Public ReadOnly IID_IAudioRenderClient As New Guid("F294ACFC-3146-4483-A7BF-ADDCA7C260E2")
        Public ReadOnly IID_IAudioCaptureClient As New Guid("C8ADBD64-E71E-48a0-A4DE-185C395CD317")

        <ComImport, InterfaceType( _
           ComInterfaceType.InterfaceIsIUnknown), _
           Guid("70ae66f2-c809-4e4f-8915-bdcb406b7993")>
        Public Interface IMFSourceReader
            Sub GetStreamSelection(<[In]> _
                dwStreamIndex As Integer, <Out> ByRef pSelected As Boolean)
            Sub SetStreamSelection(<[In]> _
                dwStreamIndex As Integer, <[In]> pSelected As Boolean)
            Sub GetNativeMediaType(<[In]> dwStreamIndex As Integer, _
                <[In]> dwMediaTypeIndex As Integer, _
                <Out> ByRef ppMediaType As IntPtr)
            Sub GetCurrentMediaType(<[In]> dwStreamIndex As Integer, _
                <Out> ByRef ppMediaType As IMFMediaType)
            Sub SetCurrentMediaType(<[In]> dwStreamIndex _
                As Integer, pdwReserved As IntPtr, <[In]> pMediaType As IMFMediaType)
            Sub SetCurrentPosition(<[In], _
                MarshalAs(UnmanagedType.LPStruct)> _
                guidTimeFormat As Guid, <[In]> varPosition As Object)
            Sub ReadSample(<[In]> dwStreamIndex As Integer, _
                <[In]> dwControlFlags As Integer, _
                <Out> ByRef pdwActualStreamIndex As Integer, _
                <Out> ByRef pdwStreamFlags As Integer, _
                <Out> ByRef pllTimestamp As UInt64, _
                <Out> ByRef ppSample As IMFSample)
            Sub Flush(<[In]> dwStreamIndex As Integer)
            Sub GetServiceForStream(<[In]> dwStreamIndex As Integer, _
                <[In], MarshalAs( _
                UnmanagedType.LPStruct)> guidService As Guid, _
                <[In], MarshalAs( _
                UnmanagedType.LPStruct)> riid As Guid, _
                <Out> ByRef ppvObject As IntPtr)
            Sub GetPresentationAttribute(<[In]> _
                dwStreamIndex As Integer, <[In], _
                MarshalAs(UnmanagedType.LPStruct)> _
                guidAttribute As Guid, <Out> pvarAttribute As IntPtr)
        End Interface


        <ComImport, InterfaceType( _
           ComInterfaceType.InterfaceIsIUnknown), _
           Guid("2CD2D921-C447-44A7-A13C-4ADABFC247E3")>
        Public Interface IMFAttributes
            Sub GetItem(<[In], MarshalAs( _
                UnmanagedType.LPStruct)> guidKey As Guid, pValue As IntPtr)
            Sub GetItemType(<[In], _
                MarshalAs(UnmanagedType.LPStruct)> _
                guidKey As Guid, ByRef pType As Integer)
            Sub CompareItem(<[In], MarshalAs( _
                UnmanagedType.LPStruct)> guidKey As Guid, _
                Value As IntPtr, <MarshalAs( _
                UnmanagedType.Bool)> ByRef pbResult As Boolean)
            Sub Compare(<MarshalAs( _
                UnmanagedType.Interface)> _
                pTheirs As IMFAttributes, MatchType As Integer, _
                <MarshalAs(UnmanagedType.Bool)> ByRef pbResult As Boolean)
            Sub GetUINT32(<[In], _
                MarshalAs(UnmanagedType.LPStruct)> _
                guidKey As Guid, ByRef punValue As Integer)
            Sub GetUINT64(<[In], MarshalAs( _
                UnmanagedType.LPStruct)> guidKey As Guid, ByRef punValue As Long)
            Sub GetDouble(<[In], MarshalAs( _
                UnmanagedType.LPStruct)> guidKey As Guid, ByRef pfValue As Double)
            Sub GetGUID(<[In], MarshalAs( _
                UnmanagedType.LPStruct)> guidKey As Guid, ByRef pguidValue As Guid)
            Sub GetStringLength(<[In], _
                MarshalAs(UnmanagedType.LPStruct)> _
                guidKey As Guid, ByRef pcchLength As Integer)
            Sub GetString(<[In], MarshalAs( _
                UnmanagedType.LPStruct)> guidKey As Guid, _
                <Out, MarshalAs( _
                UnmanagedType.LPWStr)> pwszValue As Text.StringBuilder, _
                cchBufSize As Integer, ByRef pcchLength As Integer)
            Sub GetAllocatedString(<[In], _
                MarshalAs(UnmanagedType.LPStruct)> _
                guidKey As Guid, <MarshalAs( _
                UnmanagedType.LPWStr)> ByRef ppwszValue As String, ByRef pcchLength As Integer)
            Sub GetBlobSize(<[In], MarshalAs( _
                UnmanagedType.LPStruct)> guidKey As Guid, ByRef pcbBlobSize As Integer)
            Sub GetBlob(<[In], MarshalAs( _
                UnmanagedType.LPStruct)> guidKey As Guid, _
                <Out, MarshalAs( _
                UnmanagedType.LPArray)> pBuf As Byte(), _
                cbBufSize As Integer, ByRef pcbBlobSize As Integer)
            Sub GetAllocatedBlob(<[In], MarshalAs( _
                UnmanagedType.LPStruct)> guidKey As Guid, ByRef ip As IntPtr, ByRef pcbSize As Integer)
            Sub GetUnknown(<[In], MarshalAs( _
                UnmanagedType.LPStruct)> guidKey As Guid, _
                <[In], MarshalAs( _
                UnmanagedType.LPStruct)> riid As Guid, _
                <MarshalAs(UnmanagedType.IUnknown)> ByRef ppv As Object)
            Sub SetItem(<[In], MarshalAs( _
                UnmanagedType.LPStruct)> guidKey As Guid, Value As IntPtr)
            Sub DeleteItem(<[In], _
                MarshalAs( _
                UnmanagedType.LPStruct)> guidKey As Guid)
            Sub DeleteAllItems()
            Sub SetUINT32(<[In], MarshalAs( _
                UnmanagedType.LPStruct)> guidKey As Guid, unValue As Integer)
            Sub SetUINT64(<[In], MarshalAs( _
                UnmanagedType.LPStruct)> guidKey As Guid, unValue As Long)
            Sub SetDouble(<[In], MarshalAs( _
                UnmanagedType.LPStruct)> guidKey As Guid, fValue As Double)
            Sub SetGUID(<[In], MarshalAs( _
                UnmanagedType.LPStruct)> guidKey As Guid, _
                <[In], MarshalAs( _
                UnmanagedType.LPStruct)> guidValue As Guid)
            Sub SetString(<[In], MarshalAs( _
                UnmanagedType.LPStruct)> guidKey As Guid, _
                <[In], MarshalAs( _
                UnmanagedType.LPWStr)> wszValue As String)
            Sub SetBlob(<[In], MarshalAs( _
                UnmanagedType.LPStruct)> guidKey As Guid, _
                <[In], MarshalAs( _
                UnmanagedType.LPArray, SizeParamIndex:=2)> pBuf As Byte(), cbBufSize As Integer)
            Sub SetUnknown(<MarshalAs(UnmanagedType.LPStruct)> _
                guidKey As Guid, <[In], _
                MarshalAs(UnmanagedType.IUnknown)> pUnknown As Object)
            Sub LockStore()
            Sub UnlockStore()
            Sub GetCount(ByRef pcItems As Integer)
            Sub GetItemByIndex(unIndex As Integer, ByRef pguidKey As Guid, pValue As IntPtr)
            Sub CopyAllItems(<[In], MarshalAs( _
                UnmanagedType.Interface)> pDest As IMFAttributes)
        End Interface


        <ComImport, InterfaceType( _
            ComInterfaceType.InterfaceIsIUnknown), _
            Guid("44AE0FA8-EA31-4109-8D2E-4CAE4997C555")>
        Public Interface IMFMediaType
            Inherits IMFAttributes
            Overloads Sub GetItem(<[In], _
                      MarshalAs( _
                      UnmanagedType.LPStruct)> _
                      guidKey As Guid, pValue As IntPtr)
            Overloads Sub GetItemType(<[In], _
                      MarshalAs(UnmanagedType.LPStruct)> _
                      guidKey As Guid, ByRef pType As Integer)
            Overloads Sub CompareItem(<[In], _
                      MarshalAs(UnmanagedType.LPStruct)> _
                      guidKey As Guid, Value As IntPtr, <MarshalAs( _
                      UnmanagedType.Bool)> ByRef pbResult As Boolean)
            Overloads Sub Compare(<MarshalAs( _
                      UnmanagedType.Interface)> _
                      pTheirs As IMFAttributes, MatchType As Integer, _
                      <MarshalAs( _
                      UnmanagedType.Bool)> ByRef pbResult As Boolean)
            Overloads Sub GetUINT32(<[In], _
                      MarshalAs(UnmanagedType.LPStruct)> _
                      guidKey As Guid, ByRef punValue As Integer)
            Overloads Sub GetUINT64(<[In], MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, ByRef punValue As Long)
            Overloads Sub GetDouble(<[In], MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, ByRef pfValue As Double)
            Overloads Sub GetGUID(<[In], MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, ByRef pguidValue As Guid)
            Overloads Sub GetStringLength(<[In], MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, ByRef pcchLength As Integer)
            Overloads Sub GetString(<[In], MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, _
                      <Out, MarshalAs( _
                      UnmanagedType.LPWStr)> pwszValue _
                      As Text.StringBuilder, cchBufSize As Integer, ByRef pcchLength As Integer)
            Overloads Sub GetAllocatedString(<[In], _
                      MarshalAs(UnmanagedType.LPStruct)> _
                      guidKey As Guid, <MarshalAs( _
                      UnmanagedType.LPWStr)> _
                      ByRef ppwszValue As String, ByRef pcchLength As Integer)
            Overloads Sub GetBlobSize(<[In], _
                      MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, ByRef pcbBlobSize As Integer)
            Overloads Sub GetBlob(<[In], _
                      MarshalAs(UnmanagedType.LPStruct)> _
                      guidKey As Guid, <Out, MarshalAs( _
                      UnmanagedType.LPArray)> pBuf As Byte(), _
                      cbBufSize As Integer, ByRef pcbBlobSize As Integer)
            Overloads Sub GetAllocatedBlob(<[In], _
                      MarshalAs( _
                      UnmanagedType.LPStruct)> _
                      guidKey As Guid, ByRef ip As IntPtr, ByRef pcbSize As Integer)
            Overloads Sub GetUnknown(<[In], _
                      MarshalAs(UnmanagedType.LPStruct)> _
                      guidKey As Guid, <[In], MarshalAs( _
                      UnmanagedType.LPStruct)> riid As Guid, _
                      <MarshalAs( _
                      UnmanagedType.IUnknown)> ByRef ppv As Object)
            Overloads Sub SetItem(<[In], MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, Value As IntPtr)
            Overloads Sub DeleteItem(<[In], _
                      MarshalAs(UnmanagedType.LPStruct)> guidKey As Guid)
            Overloads Sub DeleteAllItems()
            Overloads Sub SetUINT32(<[In], MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, unValue As Integer)
            Overloads Sub SetUINT64(<[In], MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, unValue As Long)
            Overloads Sub SetDouble(<[In], MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, fValue As Double)
            Overloads Sub SetGUID(<[In], MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, _
                      <[In], MarshalAs( _
                      UnmanagedType.LPStruct)> guidValue As Guid)
            Overloads Sub SetString(<[In], MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, _
                      <[In], MarshalAs( _
                      UnmanagedType.LPWStr)> wszValue As String)
            Overloads Sub SetBlob(<[In], MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, _
                      <[In], MarshalAs( _
                      UnmanagedType.LPArray, SizeParamIndex:=2)> pBuf As Byte(), cbBufSize As Integer)
            Overloads Sub SetUnknown(<MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, _
                      <[In], MarshalAs( _
                      UnmanagedType.IUnknown)> pUnknown As Object)
            Overloads Sub LockStore()
            Overloads Sub UnlockStore()
            Overloads Sub GetCount(ByRef pcItems As Integer)
            Overloads Sub GetItemByIndex(unIndex As Integer, ByRef pguidKey As Guid, pValue As IntPtr)
            Overloads Sub CopyAllItems(<[In], _
                      MarshalAs( _
                      UnmanagedType.Interface)> pDest As IMFAttributes)
            '
            Sub GetMajorType(ByRef pguidMajorType As Guid)
            Sub IsCompressedFormat(<MarshalAs( _
                      UnmanagedType.Bool)> ByRef pfCompressed As Boolean)
            <PreserveSig> Function IsEqual(<[In], _
                  MarshalAs(UnmanagedType.Interface)> _
                  pIMediaType As IMFMediaType, ByRef pdwFlags As Integer) As Integer
            Sub GetRepresentation(<[In]> guidRepresentation As Guid, _
                      ByRef ppvRepresentation As IntPtr)
            Sub FreeRepresentation(<[In]> guidRepresentation As Guid, <[In]> pvRepresentation As IntPtr)
        End Interface


        <ComImport, InterfaceType( _
                      ComInterfaceType.InterfaceIsIUnknown), _
                      Guid("c40a00f2-b93a-4d80-ae8c-5a1c634f58e4")>
        Public Interface IMFSample
            Inherits IMFAttributes
            Overloads Sub GetItem(<[In], _
                      MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, pValue As IntPtr)
            Overloads Sub GetItemType(<[In], _
                      MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, ByRef pType As Integer)
            Overloads Sub CompareItem(<[In], _
                      MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, _
                      Value As IntPtr, <MarshalAs( _
                      UnmanagedType.Bool)> ByRef pbResult As Boolean)
            Overloads Sub Compare(<MarshalAs( _
                      UnmanagedType.Interface)> _
                      pTheirs As IMFAttributes, MatchType As Integer, _
                      <MarshalAs( _
                      UnmanagedType.Bool)> ByRef pbResult As Boolean)
            Overloads Sub GetUINT32(<[In], _
                      MarshalAs(UnmanagedType.LPStruct)> _
                      guidKey As Guid, ByRef punValue As Integer)
            Overloads Sub GetUINT64(<[In], _
                      MarshalAs(UnmanagedType.LPStruct)> _
                      guidKey As Guid, ByRef punValue As Long)
            Overloads Sub GetDouble(<[In], _
                      MarshalAs(UnmanagedType.LPStruct)> _
                      guidKey As Guid, ByRef pfValue As Double)
            Overloads Sub GetGUID(<[In], MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, ByRef pguidValue As Guid)
            Overloads Sub GetStringLength(<[In], _
                      MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, ByRef pcchLength As Integer)
            Overloads Sub GetString(<[In], _
                      MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, _
                      <Out, MarshalAs( _
                      UnmanagedType.LPWStr)> _
                      pwszValue As Text.StringBuilder, cchBufSize As Integer, ByRef pcchLength As Integer)
            Overloads Sub GetAllocatedString(<[In], _
                      MarshalAs(UnmanagedType.LPStruct)> _
                      guidKey As Guid, <MarshalAs( _
                      UnmanagedType.LPWStr)> _
                      ByRef ppwszValue As String, ByRef pcchLength As Integer)
            Overloads Sub GetBlobSize(<[In], _
                      MarshalAs(UnmanagedType.LPStruct)> _
                      guidKey As Guid, ByRef pcbBlobSize As Integer)
            Overloads Sub GetBlob(<[In], MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, _
                      <Out, MarshalAs( _
                      UnmanagedType.LPArray)> pBuf As Byte(), _
                      cbBufSize As Integer, ByRef pcbBlobSize As Integer)
            Overloads Sub GetAllocatedBlob(<[In], _
                      MarshalAs(UnmanagedType.LPStruct)> _
                      guidKey As Guid, ByRef ip As IntPtr, ByRef pcbSize As Integer)
            Overloads Sub GetUnknown(<[In], MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, _
                      <[In], MarshalAs( _
                      UnmanagedType.LPStruct)> riid As Guid, _
                      <MarshalAs(UnmanagedType.IUnknown)> ByRef ppv As Object)
            Overloads Sub SetItem(<[In], _
                      MarshalAs(UnmanagedType.LPStruct)> _
                      guidKey As Guid, Value As IntPtr)
            Overloads Sub DeleteItem(<[In], _
                      MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid)
            Overloads Sub DeleteAllItems()
            Overloads Sub SetUINT32(<[In], _
                      MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, unValue As Integer)
            Overloads Sub SetUINT64(<[In], _
                      MarshalAs( _
                      UnmanagedType.LPStruct)> _
                      guidKey As Guid, unValue As Long)
            Overloads Sub SetDouble(<[In], _
                      MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, fValue As Double)
            Overloads Sub SetGUID(<[In], _
                      MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, _
                      <[In], MarshalAs( _
                      UnmanagedType.LPStruct)> guidValue As Guid)
            Overloads Sub SetString(<[In], MarshalAs( _
                      UnmanagedType.LPStruct)> _
                      guidKey As Guid, <[In], _
                      MarshalAs(UnmanagedType.LPWStr)> _
                      wszValue As String)
            Overloads Sub SetBlob(<[In], MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, _
                      <[In], MarshalAs( _
                      UnmanagedType.LPArray, SizeParamIndex:=2)> pBuf As Byte(), cbBufSize As Integer)
            Overloads Sub SetUnknown(<MarshalAs( _
                      UnmanagedType.LPStruct)> guidKey As Guid, _
                      <[In], MarshalAs( _
                      UnmanagedType.IUnknown)> pUnknown As Object)
            Overloads Sub LockStore()
            Overloads Sub UnlockStore()
            Overloads Sub GetCount(ByRef pcItems As Integer)
            Overloads Sub GetItemByIndex(unIndex As Integer, ByRef pguidKey As Guid, pValue As IntPtr)
            Overloads Sub CopyAllItems(<[In], _
                      MarshalAs( _
                      UnmanagedType.Interface)> pDest As IMFAttributes)
            '
            Sub GetSampleFlags(ByRef pdwSampleFlags As Integer)
            Sub SetSampleFlags(dwSampleFlags As Integer)
            Sub GetSampleTime(ByRef phnsSampletime As Long)
            Sub SetSampleTime(hnsSampleTime As Long)
            Sub GetSampleDuration(ByRef phnsSampleDuration As Long)
            Sub SetSampleDuration(hnsSampleDuration As Long)
            Sub GetBufferCount(ByRef pdwBufferCount As Integer)
            Sub GetBufferByIndex(dwIndex As Integer, ByRef ppBuffer As IMFMediaBuffer)
            Sub ConvertToContiguousBuffer(<Out> ByRef ppBuffer As IMFMediaBuffer)
            Sub AddBuffer(pBuffer As IMFMediaBuffer)
            Sub RemoveBuferByindex(dwIndex As Integer)
            Sub RemoveAllBuffers()
            Sub GetTotalLength(ByRef pcbTotalLength As Integer)
            Sub CopyToByffer(pBuffer As IMFMediaBuffer)
        End Interface


        <ComImport, InterfaceType( _
                      ComInterfaceType.InterfaceIsIUnknown), _
                      Guid("045FA593-8799-42b8-BC8D-8968C6453507")>
        Public Interface IMFMediaBuffer
            Sub Lock(<Out> ByRef ppbBuffer As IntPtr, <Out> ByRef pcbMaxLength As Integer, <Out> ByRef pcbCurrentLength As Integer)
            Sub Unlock()
            Sub GetCurrentLength(ByRef pcbCurrentLength As Integer)
            Sub SetCurrentLength(cbCurrentLength As Integer)
            Sub GetMaxLength(ByRef pcbMaxLength As Integer)
        End Interface

        <ComImport, InterfaceType( _
                      ComInterfaceType.InterfaceIsIUnknown), _
                      Guid("ad4c1b00-4bf7-422f-9175-756693d9130d")>
        Public Interface IMFByteStream
            Sub GetCapabilities(ByRef pdwCapabiities As Integer)
            Sub GetLength(ByRef pqwLength As Long)
            Sub SetLength(qwLength As Long)
            Sub GetCurrentPosition(ByRef pqwPosition As Long)
            Sub SetCurrentPosition(qwPosition As Long)
            Sub IsEndOfStream(<MarshalAs( _
                  UnmanagedType.Bool)> ByRef pfEndOfStream As Boolean)
            Sub Read(pb As IntPtr, cb As Integer, ByRef pcbRead As Integer)
            Sub BeginRead(pb As IntPtr, cb As Integer, pCallback As IntPtr, punkState As IntPtr)
            Sub EndRead(pResult As IntPtr, ByRef pcbRead As Integer)
            Sub Write(pb As IntPtr, cb As Integer, ByRef pcbWritten As Integer)
            Sub BeginWrite(pb As IntPtr, cb As Integer, pCallback As IntPtr, punkState As IntPtr)
            Sub EndWrite(pResult As IntPtr, ByRef pcbWritten As Integer)
            Sub Seek(SeekOrigin As Integer, llSeekOffset As Long, dwSeekFlags As Integer, ByRef pqwCurrentPosition As Long)
            Sub Flush()
            Sub Close()
        End Interface



        <ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")>
        Public Class MMDeviceEnumerator
        End Class


        <ComImport, InterfaceType( _
           ComInterfaceType.InterfaceIsIUnknown), _
           Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")>
        Public Interface IMMDeviceEnumerator
            Sub EnumAudioEndpoints(dataflow As EDataFlow, dwStateMask _
                As DeviceStateFlags, ByRef ppDevices As IMMDeviceCollection)
            Sub GetDefaultAudioEndpoint(dataflow As EDataFlow, role As ERole, ByRef ppDevice As IMMDevice)
            Sub GetDevice(<MarshalAs( _
                UnmanagedType.LPWStr)> pwstrId As String, ByRef ppDevice As IntPtr)
            Sub RegisterEndpointNotificationCallback(pClient As IntPtr)
            Sub UnregisterEndpointNotificationCallback(pClient As IntPtr)
        End Interface


        <ComImport, InterfaceType( _
           ComInterfaceType.InterfaceIsIUnknown), _
           Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")>
        Public Interface IMMDeviceCollection
            Sub GetCount(ByRef pcDevices As Integer)
            Sub Item(nDevice As Integer, ByRef ppDevice As IMMDevice)
        End Interface


        <ComImport, InterfaceType( _
           ComInterfaceType.InterfaceIsIUnknown), _
           Guid("D666063F-1587-4E43-81F1-B948E807363F")>
        Public Interface IMMDevice
            Sub Activate(<MarshalAs( _
                UnmanagedType.LPStruct)> iid As Guid, _
                dwClsCtx As CLSCTX, pActivationParams As IntPtr, ByRef ppInterface As IAudioClient2)
            Sub OpenPropertyStore(stgmAccess As Integer, ByRef ppProperties As IPropertyStore)
            Sub GetId(<MarshalAs( _
                UnmanagedType.LPWStr)> ByRef ppstrId As String)
            Sub GetState(ByRef pdwState As Integer)
        End Interface


        <ComImport, InterfaceType( _
              ComInterfaceType.InterfaceIsIUnknown), _
              Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99")>
        Public Interface IPropertyStore
            'virtual HRESULT STDMETHODCALLTYPE GetCount(/*[out]*/ __RPC__out DWORD *cProps)
            Sub GetCount(ByRef cProps As Integer)
            'virtual HRESULT STDMETHODCALLTYPE GetAt(/*In*/ 
            '   DWORD iProp, /*[out]*/ __RPC__out PROPERTYKEY *pkey)
            Sub GetAt(iProp As Integer, ByRef pkey As IntPtr)
            'virtual HRESULT STDMETHODCALLTYPE GetValue(/*In*/
            '    __RPC__in REFPROPERTYKEY key, /*[out]*/ __RPC__out PROPVARIANT *pv)
            Sub GetValue(ByRef key As PROPERTYKEY, ByRef pv As PROPVARIANT)
            'virtual HRESULT STDMETHODCALLTYPE SetValue(/*In*/ 
            '  __RPC__in REFPROPERTYKEY key, /*In*/ __RPC__in REFPROPVARIANT propvar)
            Sub SetValue(ByRef key As PROPERTYKEY, ByRef propvar As IntPtr)
            'virtual HRESULT STDMETHODCALLTYPE Commit()
            Sub Commit()
        End Interface


        <ComImport, InterfaceType( _
           ComInterfaceType.InterfaceIsIUnknown), _
           Guid("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2")>
        Public Interface IAudioClient
            Sub Initialize(ShareMode As AUDCLNT_SHAREMODE, StreamFlags As AUDCLNT_FLAGS, _
                hnsBufferDuration As Long, hnsPeriodicity As Long, ByRef _
                pFormat As WAVEFORMATEX, AudioSessionGuid As IntPtr)
            'virtual HRESULT STDMETHODCALLTYPE GetBufferSize(/*[out]*/ _Out_  UINT32 *pNumBufferFrames) = 0;
            Sub GetBufferSize(ByRef pNumBufferFrames As Integer)
            'virtual HRESULT STDMETHODCALLTYPE GetStreamLatency(/*[out]*/ _Out_  REFERENCE_TIME *phnsLatency) = 0;
            Sub GetStreamLatency(ByRef phnsLatency As Long)
            'virtual HRESULT STDMETHODCALLTYPE GetCurrentPadding(/*[out]*/ _Out_  UINT32 *pNumPaddingFrames) = 0;
            Sub GetCurrentPadding(ByRef pNumPaddingFrames As Integer)
            'virtual HRESULT STDMETHODCALLTYPE IsFormatSupported(/*[in]*/ _In_  
            '   AUDCLNT_SHAREMODE ShareMode, /*[in]*/ _In_  const WAVEFORMATEX *pFormat, 
            '   /*[unique][out]*/ _Out_opt_  WAVEFORMATEX **ppClosestMatch) = 0;
            Sub IsFormatSupported(ShareMode As AUDCLNT_SHAREMODE, ByRef pFormat _
                                  As WAVEFORMATEX, ByRef ppClosestMatch As IntPtr)
            'virtual HRESULT STDMETHODCALLTYPE GetMixFormat(/*[out]*/ _Out_  WAVEFORMATEX **ppDeviceFormat) = 0;
            Sub GetMixFormat(ByRef ppDeviceFormat As IntPtr)
            'virtual HRESULT STDMETHODCALLTYPE GetDevicePeriod(/*[out]*/ _Out_opt_  
            '    REFERENCE_TIME *phnsDefaultDevicePeriod, /*[out]*/ 
            '    _Out_opt_  REFERENCE_TIME *phnsMinimumDevicePeriod) = 0;
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
            Sub GetService(<MarshalAs( _
                UnmanagedType.LPStruct)> riid As Guid, _
                <MarshalAs(UnmanagedType.IUnknown)> ByRef ppv As Object)
        End Interface


        <ComImport, InterfaceType( _
                         ComInterfaceType.InterfaceIsIUnknown), _
                         Guid("726778CD-F60A-4eda-82DE-E47610CD78AA")>
        Public Interface IAudioClient2
            '!!!Sub Initialize(ShareMode As AUDCLNT_SHAREMODE, StreamFlags As AUDCLNT_FLAGS, _
            '    hnsBufferDuration As Long, hnsPeriodicity As Long, _
            '    ByRef pFormat As WAVEFORMATEX, AudioSessionGuid As IntPtr)
            Sub Initialize(ShareMode As AUDCLNT_SHAREMODE, StreamFlags As AUDCLNT_FLAGS, _
                hnsBufferDuration As Long, hnsPeriodicity As Long,
                pFormat As IntPtr, AudioSessionGuid As IntPtr)
            Sub GetBufferSize(<Out> ByRef pNumBufferFrames As Integer)
            Sub GetStreamLatency(<Out> ByRef phnsLatency As Long)
            Sub GetCurrentPadding(<Out> ByRef pNumPaddingFrames As Integer)
            Sub IsFormatSupported(ShareMode As AUDCLNT_SHAREMODE, _
                ByRef pFormat As WAVEFORMATEX, ByRef ppClosestMatch As IntPtr)
            Sub GetMixFormat(ByRef ppDeviceFormat As IntPtr)
            Sub GetDevicePeriod(<Out> ByRef phnsDefaultDevicePeriod As Long, <Out> ByRef phnsMinimumDevicePeriod As Long)
            Sub Start()
            Sub [Stop]()
            Sub Reset()
            Sub SetEventHandle(eventHandle As IntPtr)
            Sub GetService(<MarshalAs( _
                UnmanagedType.LPStruct)> riid As Guid, _
                <Out, MarshalAs(UnmanagedType.IUnknown)> ByRef ppv As Object)
            'virtual HRESULT STDMETHODCALLTYPE IsOffloadCapable(/*[in]*/ _In_  
            '   AUDIO_STREAM_CATEGORY Category, /*[in]*/ _Out_  BOOL *pbOffloadCapable) = 0;
            Sub IsOffloadCapable(Category As Integer, ByRef pbOffloadCapable As Boolean)
            'virtual HRESULT STDMETHODCALLTYPE SetClientProperties(/*[in]*/ _In_  
            '  const AudioClientProperties *pProperties) = 0;
            Sub SetClientProperties(pProperties As IntPtr)
            'virtual HRESULT STDMETHODCALLTYPE GetBufferSizeLimits(/*[in]*/ _In_  
            '   const WAVEFORMATEX *pFormat, /*[in]*/ _In_  BOOL bEventDriven, /*[in]*/ 
            '  _Out_  REFERENCE_TIME *phnsMinBufferDuration, /*[in]*/ _Out_  
            '  REFERENCE_TIME *phnsMaxBufferDuration) = 0;
            Sub GetBufferSizeLimits(pFormat As IntPtr, bEventDriven As Boolean, _
                     phnsMinBufferDuration As IntPtr, phnsMaxBufferDuration As IntPtr)
        End Interface

        <ComImport, InterfaceType( _
            ComInterfaceType.InterfaceIsIUnknown), _
            Guid("F294ACFC-3146-4483-A7BF-ADDCA7C260E2")>
        Public Interface IAudioRenderClient
            'virtual HRESULT STDMETHODCALLTYPE GetBuffer(/*[in]*/ _In_  UINT32 NumFramesRequested,
            '   /*[out]*/ _Outptr_result_buffer_( _Inexpressible_(
            '  "NumFramesRequested * pFormat->nBlockAlign"))  BYTE **ppData) = 0;
            Sub GetBuffer(NumFramesRequested As Integer, <Out> ByRef ppData As IntPtr)
            'virtual HRESULT STDMETHODCALLTYPE ReleaseBuffer(/*[in]*/ _In_  
            '   UINT32 NumFramesWritten, /*[in]*/ _In_  DWORD dwFlags) = 0;
            Sub ReleaseBuffer(NumFramesWritten As Integer, dwFlags As Integer)
        End Interface

        <ComImport, InterfaceType( _
            ComInterfaceType.InterfaceIsIUnknown), _
            Guid("C8ADBD64-E71E-48a0-A4DE-185C395CD317")>
        Public Interface IAudioCaptureClient
            'virtual HRESULT STDMETHODCALLTYPE GetBuffer(/*[out]*/ _Outptr_result_buffer_(
            '   _Inexpressible_("*pNumFramesToRead * pFormat->nBlockAlign"))  
            '   BYTE **ppData, /*[out]*/ _Out_  UINT32 *pNumFramesToRead, /*[out]*/_Out_ 
            '   DWORD *pdwFlags, /*[out]*/_Out_opt_  UINT64 *pu64DevicePosition, 
            '   /*[out]*/_Out_opt_  UINT64 *pu64QPCPosition) = 0;
            Sub GetBuffer(ByRef ppData As IntPtr, ByRef pNumFramesToRead As Integer, _
                   ByRef pdwFlags As Integer, pu64DevicePosition As IntPtr, pu64QPCPosition As IntPtr)
            'virtual HRESULT STDMETHODCALLTYPE ReleaseBuffer(/*[in]*/ _In_  UINT32 NumFramesRead) = 0;
            Sub ReleaseBuffer(NumFramesRead As Integer)
            'virtual HRESULT STDMETHODCALLTYPE GetNextPacketSize(
            '       /*[out]*/ _Out_  UINT32 *pNumFramesInNextPacket) = 0;
            Sub GetNextPacketSize(ByRef pNumFramesInNextPacket As Integer)
        End Interface

        <ComImport, InterfaceType( _
          ComInterfaceType.InterfaceIsIUnknown), _
          Guid("41D949AB-9862-444A-80F6-C261334DA5EB")>
        Public Interface IActivateAudioInterfaceCompletionHandler
            'virtual HRESULT STDMETHODCALLTYPE ActivateCompleted(/*[in]*/ _In_  
            '   IActivateAudioInterfaceAsyncOperation *activateOperation) = 0;
            Sub ActivateCompleted(activateOperation As IActivateAudioInterfaceAsyncOperation)
        End Interface

        <ComImport, InterfaceType( _
           ComInterfaceType.InterfaceIsIUnknown), _
           Guid("72A22D78-CDE4-431D-B8CC-843A71199B6D")>
        Public Interface IActivateAudioInterfaceAsyncOperation
            'virtual HRESULT STDMETHODCALLTYPE GetActivateResult(/*[out]*/ _Out_  
            '  HRESULT *activateResult, /*[out]*/ _Outptr_result_maybenull_  IUnknown **activatedInterface) = 0;
            Sub GetActivateResult(ByRef activateResult As Integer, _
                <MarshalAs(UnmanagedType.IUnknown)> _
                ByRef activateInterface As Object)
        End Interface

        <ComImport, InterfaceType( _
           ComInterfaceType.InterfaceIsIUnknown), _
           Guid("94ea2b94-e9cc-49e0-c0ff-ee64ca8f5b90")>
        Public Interface IAgileObject
        End Interface


        <StructLayout(LayoutKind.Sequential, Pack:=1)>
        Public Structure WAVEFORMATEX
            Dim wFormatTag As Short
            Dim nChannels As Short
            Dim nSamplesPerSec As Integer
            Dim nAvgBytesPerSec As Integer
            Dim nBlockAlign As Short
            Dim wBitsPerSample As Short
            Dim cbSize As Short
        End Structure


        <StructLayout(LayoutKind.Sequential, Pack:=1)>
        Public Structure PROPVARIANT
            Dim vt As UShort
            Dim wReserved1 As UShort
            Dim wReserved2 As UShort
            Dim wReserved3 As UShort
            Dim p As IntPtr
            Dim p2 As Integer
            ReadOnly Property Value As Object
                Get
                    Select Case vt
                        Case 31 : Return Marshal.PtrToStringUni(p) ' VT_LPWSTR
                        Case Else
                            Throw New NotImplementedException
                    End Select
                End Get
            End Property
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=1)>
        Public Structure PROPERTYKEY
            Dim fmtid As Guid
            Dim pid As Integer
            Public Overrides Function ToString() As String
                Return "{" & fmtid.ToString() & "} " & pid.ToString()
            End Function
        End Structure

        Public Enum EDataFlow
            eRender = 0
            eCapture = 1
            eAll = 2
            EDataFlow_enum_count = 3
        End Enum

        Public Enum ERole
            eConsole = 0
            eMultimedia = 1
            eCommunications = 2
            ERole_enum_count = 3
        End Enum


        Public Enum StgmMode
            STGM_READ = 0
            STGM_WRITE = 1
            STGM_READWRITE = 2
        End Enum

        Public Enum AUDCLNT_SHAREMODE
            AUDCLNT_SHAREMODE_SHARED = 0
            AUDCLNT_SHAREMODE_EXCLUSIVE = 1
        End Enum

        <Flags> Public Enum DeviceStateFlags
            DEVICE_STATE_ACTIVE = 1
            DEVICE_STATE_DISABLED = 2
            DEVICE_STATE_NOTPRESENT = 4
            DEVICE_STATE_UNPLUGGED = 8
            DEVICE_STATEMASK_ALL = 15
        End Enum

        <Flags> Public Enum AUDCLNT_FLAGS
            AUDCLNT_STREAMFLAGS_CROSSPROCESS = &H10000
            AUDCLNT_STREAMFLAGS_LOOPBACK = &H20000
            AUDCLNT_STREAMFLAGS_EVENTCALLBACK = &H40000
            AUDCLNT_STREAMFLAGS_NOPERSIST = &H80000
            AUDCLNT_STREAMFLAGS_RATEADJUST = &H100000
            AUDCLNT_SESSIONFLAGS_EXPIREWHENUNOWNED = &H10000000
            AUDCLNT_SESSIONFLAGS_DISPLAY_HIDE = &H20000000
            AUDCLNT_SESSIONFLAGS_DISPLAY_HIDEWHENEXPIRED = &H40000000
        End Enum

        <Flags> Public Enum EventAccess
            STANDARD_RIGHTS_REQUIRED = &HF0000
            SYNCHRONIZE = &H100000
            EVENT_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED Or SYNCHRONIZE Or &H3
        End Enum

        <Flags> Public Enum CLSCTX
            CLSCTX_INPROC_SERVER = 1
            CLSCTX_INPROC_HANDLER = 2
            CLSCTX_LOCAL_SERVER = 4
            CLSCTX_REMOTE_SERVER = 16
            CLSCTX_ALL = CLSCTX_INPROC_SERVER Or CLSCTX_INPROC_HANDLER Or _
                         CLSCTX_LOCAL_SERVER Or CLSCTX_REMOTE_SERVER
        End Enum

    End Module

End Namespace
