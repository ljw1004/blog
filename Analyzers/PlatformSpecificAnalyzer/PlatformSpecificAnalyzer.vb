' FOR EASIER F5 DEBUGGING OF THIS ANALYZER:
' Set "PlatformSpecificAnalyzer" as your startup project. Then under MyProject > Debug, set
' StartAction: external program
'     C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe
' Command-line arguments: 
'     "C:\Users\lwischik\Source\Repos\blog\Analyzers\DemoUWP_CS\DemoUWP_CS.sln" /RootSuffix Analyzer
'     "C:\Users\lwischik\Source\Repos\blog\Analyzers\DemoUWP_VB\DemoUWP_VB.sln" /RootSuffix Analyzer
' Note: I want to migrate this analyzer over to a PCL. But PCLs don't support the debug tab: https://github.com/dotnet/roslyn/issues/4542

Imports System.Collections.Immutable
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.CodeAnalysis

Public Enum PlatformKind
    Unchecked ' .NET and Win8.1
    Uwp ' the core UWP platform
    ExtensionSDK ' Desktop, Mobile, IOT, Xbox extension SDK
    User ' from when the user put a *Specific attribute on something
End Enum

Public Structure Platform
    Public Kind As PlatformKind
    Public Version As String ' For UWP, this is version 10240 or 10586. For User, the fully qualified name of the attribute in use

    Public Sub New(kind As PlatformKind, Optional version As String = Nothing)
        Me.Kind = kind
        Me.Version = version
        Select Case kind
            Case PlatformKind.Unchecked : If version IsNot Nothing Then Throw New ArgumentException("No version expected")
            Case PlatformKind.Uwp : If version <> "10240" AndAlso version <> "10586" Then Throw New ArgumentException("Only known SDKs are 10240 and 10586")
            Case PlatformKind.ExtensionSDK : If version IsNot Nothing Then Throw New ArgumentException("Don't specify versions for extension SDKs")
            Case PlatformKind.User : If Not version?.EndsWith("Specific") Then Throw New ArgumentException("User specific should end in Specific")
        End Select
    End Sub

    Public Shared Function OfSymbol(symbol As ISymbol) As Platform
        ' This function tells which version/platform the symbol is from.
        ' This function is hard-coded with knowledge up to SDK 10586.
        ' I could have made it a general-purpose function which looks up the SDK
        ' files on disk. But I think it's more elegant to hard-code it into the analyzer,
        ' so as to reduce disk-access while the analyzer runs.

        If symbol Is Nothing Then Return New Platform(PlatformKind.Unchecked)
        If symbol.ContainingNamespace?.ToDisplayString.StartsWith("Windows.") Then
            Dim assembly = symbol.ContainingAssembly.Name, version = symbol.ContainingAssembly.Identity.Version.Major

            ' Any call to ApiInformation.* is allowed without warning
            If symbol.ContainingType?.Name = "ApiInformation" Then Return New Platform(PlatformKind.Uwp, "10240")

            ' I don't want to give warning when analyzing code in an 8.1 or PCL project.
            ' In those two targets, every Windows type is found in Windows.winmd, so that's how we'll suppress it:
            If assembly = "Windows" Then Return New Platform(PlatformKind.Unchecked)

            ' Some WinRT types like Windows.UI.Color get projected to come from this assembly:
            If assembly = "System.Runtime.WindowsRuntime" Then Return New Platform(PlatformKind.Uwp, "10240")

            ' Some things are emphatically part of UWP.10240
            If assembly = "Windows.Foundation.FoundationContract" OrElse
                (assembly = "Windows.Foundation.UniversalApiContract" AndAlso version = 1) OrElse
                assembly = "Windows.Networking.Connectivity.WwanContract" Then Return New Platform(PlatformKind.Uwp, "10240")

            ' Some things were in platform-specific in 10240, but moved to UWP in 10586
            ' Should we report them as "platform-specific"? Or should we report them as "version-specific"?
            ' I'll report them as version-specific, because I think that will be a nicer message.
            If assembly = "Windows.ApplicationModel.Calls.CallsVoipContract" OrElse
               assembly = "Windows.Graphics.Printing3D.Printing3DContract" OrElse
               assembly = "Windows.Devices.Printers.PrintersContract" Then Return New Platform(PlatformKind.Uwp, "10586")

            ' Some things in UWP have been added between 10240 and 10586
            If assembly = "Windows.Foundation.UniversalApiContract" Then
                Dim d = GetUniversalApiAdditions()
                Dim isType = (symbol.Kind = SymbolKind.NamedType)
                Dim typeName = If(isType, symbol.ToDisplayString, symbol.ContainingType.ToDisplayString)
                Dim newMembers As List(Of String) = Nothing
                Dim in10586 = d.TryGetValue(typeName, newMembers)
                If Not in10586 Then Return New Platform(PlatformKind.Uwp, "10240") ' the type was in 10240
                If newMembers Is Nothing Then Return New Platform(PlatformKind.Uwp, "10586") ' the entire type was new in 10586
                If isType Then Return New Platform(PlatformKind.Uwp, "10240") ' the type was in 10240, even though members are new in 10586
                Dim memberName = symbol.Name
                If newMembers.Contains(memberName) Then Return New Platform(PlatformKind.Uwp, "10586") ' this member was new in 10586
                Return New Platform(PlatformKind.Uwp, "10240") ' this member existed in 10240
            End If

            ' All other Windows.* types come from platform-specific extensions
            Return New Platform(PlatformKind.ExtensionSDK)

        Else
            Dim attr = GetPlatformSpecificAttribute(symbol)
            If attr IsNot Nothing Then Return New Platform(PlatformKind.User, attr)
            Return New Platform(PlatformKind.Unchecked)
        End If
    End Function

End Structure


Class HowToGuard
    Public TypeToCheck As String
    Public MemberToCheck As String
    Public KindOfCheck As String = "IsTypePresent"
    Public AttributeToIntroduce As String = "System.Runtime.CompilerServices.PlatformSpecific"
    Public AttributeFriendlyName As String = "PlatformSpecific"

    Shared Function Symbol(target As ISymbol) As HowToGuard
        Dim plat = Platform.OfSymbol(target)
        '
        If plat.Kind = PlatformKind.Unchecked Then
            Throw New InvalidOperationException("oops! don't know why I was asked to check something that's fine")
        ElseIf plat.Kind = PlatformKind.User Then
            Dim lastDot = plat.Version.LastIndexOf("."c)
            Dim attrName = If(lastDot = -1, plat.Version, plat.Version.Substring(lastDot + 1))
            Return New HowToGuard With {.AttributeToIntroduce = plat.Version, .AttributeFriendlyName = attrName, .TypeToCheck = "??"}
        ElseIf plat.Kind = PlatformKind.ExtensionSDK Then
            Return New HowToGuard With {.TypeToCheck = If(target.Kind = SymbolKind.NamedType, target.ToDisplayString, target.ContainingType.ToDisplayString)}
        ElseIf plat.Kind = PlatformKind.Uwp AndAlso target.Kind = SymbolKind.NamedType Then
            Return New HowToGuard With {.TypeToCheck = target.ToDisplayString}
        ElseIf plat.Kind = PlatformKind.Uwp AndAlso target.Kind <> SymbolKind.NamedType Then
            Dim g As New HowToGuard With {.TypeToCheck = target.ContainingType.ToDisplayString}
            Dim d = GetUniversalApiAdditions(), newMembers As List(Of String) = Nothing
            If Not d.TryGetValue(g.TypeToCheck, newMembers) Then Throw New InvalidOperationException("oops! expected this UWP version API to be in the dictionary of new things")
            If newMembers IsNot Nothing Then
                g.MemberToCheck = target.Name
                If target.Kind = SymbolKind.Field Then g.KindOfCheck = "IsEnumNamedValuePresent" ' the only fields in WinRT are enum fields
                If target.Kind = SymbolKind.Event Then g.KindOfCheck = "IsEventPresent"
                If target.Kind = SymbolKind.Property Then g.KindOfCheck = "IsPropertyPresent" ' TODO: if SDK starts introducing additional accessors on properties, we'll have to change this
                If target.Kind = SymbolKind.Method Then g.KindOfCheck = "IsMethodPresent"
            End If
            Return g
        Else
            Throw New InvalidOperationException("oops! impossible platform kind")
        End If
    End Function

End Class


Module PlatformSpecificAnalyzer
    Public RulePlatform As New DiagnosticDescriptor("UWP001", "Platform-specific", "Platform-specific code", "Safety", DiagnosticSeverity.Warning, True)
    Public RuleVersion As New DiagnosticDescriptor("UWP002", "Version-specific", "Version-specific code", "Safety", DiagnosticSeverity.Warning, True)

    Function GetTargetPlatformMinVersion(additionalFiles As ImmutableArray(Of AdditionalText)) As Integer
        ' When PlatformSpecificAnalyzer is build as a NuGet package, the package includes
        ' a.targets File with the following lines. The effect is to add a fake file,
        ' which doesn't show up in SolnExplorer and which doesn't even exist, but whose
        ' FILENAME encodes the TargetPlatformMinVersion. That way, when the user modifies
        ' TargetPlatformMinVersion from within the ProjectProperties, msbuild re-evaluates
        ' the AdditionalFiles, and Roslyn re-runs its analyzers and can pick it up.
        ' Thanks Jason Malinowski for the hint on how to do this. He instructed me to
        ' write in the comments "this is a terrible hack and no one should ever copy it".
        '      <AdditionalFileItemNames>PlatformSpecificAnalyzerInfo</AdditionalFileItemNames>
        '      <ItemGroup>
        '        <PlatformSpecificAnalyzerInfo Include = "tpmv_$(TargetPlatformMinVersion).tpmv"><Visible>False</Visible></PlatformSpecificAnalyzerInfo>
        '      </ItemGroup>

        ' I'm caching the value because, heck, it seems weird to recompute it every time.
        Static Dim cacheKey As ImmutableArray(Of AdditionalText) = Nothing
        Static Dim cacheValue As Integer = 10240 ' if we don't find that terrible hack, assume 10240
        If additionalFiles = cacheKey Then Return cacheValue Else cacheKey = additionalFiles
        Dim tpmv = additionalFiles.FirstOrDefault(Function(af) af.Path.EndsWith(".tpmv"))?.Path
        If tpmv Is Nothing Then
            cacheValue = 10240
        Else
            tpmv = Path.GetFileNameWithoutExtension(tpmv).Replace("tpmv_10.0.", "").Replace(".0", "")
            Dim i As Integer : cacheValue = If(Integer.TryParse(tpmv, i), i, 10240)
        End If
        Return cacheValue
    End Function

    Function GetPlatformSpecificAttribute(symbol As ISymbol) As String
        If symbol Is Nothing Then Return Nothing
        For Each attr In symbol.GetAttributes
            If attr.AttributeClass.Name.EndsWith("SpecificAttribute") Then Return attr.AttributeClass.ToDisplayString.Replace("Attribute", "")
        Next
        Return Nothing
    End Function

    Function HasPlatformSpecificAttribute(symbol As ISymbol) As Boolean
        Return (GetPlatformSpecificAttribute(symbol) IsNot Nothing)
    End Function


    Function GetUniversalApiAdditions() As Dictionary(Of String, List(Of String))
        ' I don't yet know what the new Windows SDK will be like, nor what new types it will add.
        ' As a placeholder, to test my code, I'm going to pretend that these two APIs from the existing SDK are actually new...
        Static Dim _d As New Dictionary(Of String, List(Of String)) From {
    {"Windows.ApplicationModel.Activation.ActivationKind", "DevicePairing", "Print3DWorkflow"},
    {"Windows.ApplicationModel.Activation.DevicePairingActivatedEventArgs"},
    {"Windows.ApplicationModel.Activation.IDevicePairingActivatedEventArgs"},
    {"Windows.ApplicationModel.Appointments.AppointmentCalendarSyncStatus", "ManualAccountRemovalRequired"},
    {"Windows.ApplicationModel.Background.SensorDataThresholdTrigger"},
    {"Windows.ApplicationModel.Chat.ChatConversation", "CanModifyParticipants"},
    {"Windows.ApplicationModel.Contacts.ContactCardOptions", "ServerSearchContactListIds"},
    {"Windows.ApplicationModel.Contacts.ContactListSyncStatus", "ManualAccountRemovalRequired"},
    {"Windows.ApplicationModel.DataTransfer.DataPackageView", "SetAcceptedFormatId"},
    {"Windows.ApplicationModel.DataTransfer.DragDrop.Core.CoreDragInfo", "AllowedOperations"},
    {"Windows.ApplicationModel.DataTransfer.DragDrop.Core.CoreDragOperation", "AllowedOperations"},
    {"Windows.ApplicationModel.DataTransfer.OperationCompletedEventArgs", "AcceptedFormatId"},
    {"Windows.ApplicationModel.Email.EmailCertificateValidationStatus"},
    {"Windows.ApplicationModel.Email.EmailMailbox", "ResolveRecipientsAsync", "TryCreateFolderAsync", "TryDeleteFolderAsync", "TryEmptyFolderAsync", "ValidateCertificatesAsync"},
    {"Windows.ApplicationModel.Email.EmailMailboxCapabilities", "CanCreateFolder", "CanDeleteFolder", "CanEmptyFolder", "CanMoveFolder", "CanResolveRecipients", "CanValidateCertificates"},
    {"Windows.ApplicationModel.Email.EmailMailboxCreateFolderResult"},
    {"Windows.ApplicationModel.Email.EmailMailboxCreateFolderStatus"},
    {"Windows.ApplicationModel.Email.EmailMailboxDeleteFolderStatus"},
    {"Windows.ApplicationModel.Email.EmailMailboxEmptyFolderStatus"},
    {"Windows.ApplicationModel.Email.EmailMailboxPolicies", "MustEncryptSmimeMessages", "MustSignSmimeMessages"},
    {"Windows.ApplicationModel.Email.EmailMailboxSyncStatus", "ManualAccountRemovalRequired"},
    {"Windows.ApplicationModel.Email.EmailMeetingInfo", "IsReportedOutOfDateByServer"},
    {"Windows.ApplicationModel.Email.EmailMessage", "SmimeData", "SmimeKind"},
    {"Windows.ApplicationModel.Email.EmailMessageSmimeKind"},
    {"Windows.ApplicationModel.Email.EmailRecipientResolutionResult"},
    {"Windows.ApplicationModel.Email.EmailRecipientResolutionStatus"},
    {"Windows.ApplicationModel.Store.CurrentApp", "GetCustomerCollectionsIdAsync", "GetCustomerPurchaseIdAsync"},
    {"Windows.ApplicationModel.Store.ListingInformation", "CurrencyCode", "FormattedBasePrice", "IsOnSale", "SaleEndDate"},
    {"Windows.ApplicationModel.Store.Preview.InstallControl.AppInstallItem", "Cancel", "Pause", "Restart"},
    {"Windows.ApplicationModel.Store.Preview.InstallControl.AppInstallManager", "Cancel", "GetIsAppAllowedToInstallAsync", "Pause", "Restart", "SearchForAllUpdatesAsync", "SearchForUpdatesAsync", "StartAppInstallAsync", "UpdateAppByPackageFamilyNameAsync"},
    {"Windows.ApplicationModel.Store.Preview.StoreConfiguration", "PurchasePromptingPolicy"},
    {"Windows.ApplicationModel.Store.ProductListing", "CurrencyCode", "FormattedBasePrice", "IsOnSale", "SaleEndDate"},
    {"Windows.ApplicationModel.UserDataAccounts.UserDataAccount", "EnterpriseId", "IsProtectedUnderLock"},
    {"Windows.Devices.Bluetooth.BluetoothAddressType"},
    {"Windows.Devices.Bluetooth.BluetoothDevice", "DeviceInformation", "GetDeviceSelectorFromBluetoothAddress", "GetDeviceSelectorFromClassOfDevice", "GetDeviceSelectorFromConnectionStatus", "GetDeviceSelectorFromDeviceName", "GetDeviceSelectorFromPairingState"},
    {"Windows.Devices.Bluetooth.BluetoothError", "DisabledByUser"},
    {"Windows.Devices.Bluetooth.BluetoothLEAppearance"},
    {"Windows.Devices.Bluetooth.BluetoothLEAppearanceCategories"},
    {"Windows.Devices.Bluetooth.BluetoothLEAppearanceSubcategories"},
    {"Windows.Devices.Bluetooth.BluetoothLEDevice", "Appearance", "BluetoothAddressType", "DeviceInformation", "FromBluetoothAddressAsync", "GetDeviceSelectorFromAppearance", "GetDeviceSelectorFromBluetoothAddress", "GetDeviceSelectorFromConnectionStatus", "GetDeviceSelectorFromDeviceName", "GetDeviceSelectorFromPairingState"},
    {"Windows.Devices.Bluetooth.Rfcomm.RfcommServiceProvider", "StartAdvertising"},
    {"Windows.Devices.Enumeration.DeviceInformationCustomPairing"},
    {"Windows.Devices.Enumeration.DeviceInformationPairing", "Custom", "PairAsync", "ProtectionLevel", "TryRegisterForAllInboundPairingRequests", "UnpairAsync"},
    {"Windows.Devices.Enumeration.DevicePairingKinds"},
    {"Windows.Devices.Enumeration.DevicePairingRequestedEventArgs"},
    {"Windows.Devices.Enumeration.DevicePairingResultStatus", "Failed", "OperationAlreadyInProgress", "PairingCanceled", "RejectedByHandler", "RemoteDeviceHasAssociation", "RequiredHandlerNotRegistered"},
    {"Windows.Devices.Enumeration.DeviceUnpairingResult"},
    {"Windows.Devices.Enumeration.DeviceUnpairingResultStatus"},
    {"Windows.Devices.Enumeration.IDevicePairingSettings"},
    {"Windows.Devices.Perception.KnownPerceptionFrameSourceProperties", "DeviceId"},
    {"Windows.Devices.Perception.PerceptionColorFrameSource", "DeviceId"},
    {"Windows.Devices.Perception.PerceptionDepthFrameSource", "DeviceId"},
    {"Windows.Devices.Perception.PerceptionInfraredFrameSource", "DeviceId"},
    {"Windows.Devices.Printers.Extensions.Print3DWorkflow"},
    {"Windows.Devices.Printers.Extensions.Print3DWorkflowDetail"},
    {"Windows.Devices.Printers.Extensions.Print3DWorkflowPrintRequestedEventArgs"},
    {"Windows.Devices.Printers.Extensions.Print3DWorkflowStatus"},
    {"Windows.Devices.Sensors.ISensorDataThreshold"},
    {"Windows.Devices.Sensors.Pedometer", "GetCurrentReadings", "GetReadingsFromTriggerDetails"},
    {"Windows.Devices.Sensors.PedometerDataThreshold"},
    {"Windows.Devices.Sensors.ProximitySensor", "GetReadingsFromTriggerDetails"},
    {"Windows.Devices.Sensors.ProximitySensorDataThreshold"},
    {"Windows.Devices.Sensors.SensorDataThresholdTriggerDetails"},
    {"Windows.Devices.Sensors.SensorType"},
    {"Windows.Devices.WiFiDirect.WiFiDirectAdvertisement", "SupportedConfigurationMethods"},
    {"Windows.Devices.WiFiDirect.WiFiDirectConfigurationMethod"},
    {"Windows.Devices.WiFiDirect.WiFiDirectConnectionParameters", "GetDevicePairingKinds", "PreferenceOrderedConfigurationMethods", "PreferredPairingProcedure"},
    {"Windows.Devices.WiFiDirect.WiFiDirectPairingProcedure"},
    {"Windows.Gaming.UI.GameBar"},
    {"Windows.Graphics.DirectX.DirectXAlphaMode"},
    {"Windows.Graphics.Display.DisplayInformation", "DiagonalSizeInInches"},
    {"Windows.Graphics.Holographic.HolographicAdapterId"},
    {"Windows.Graphics.Holographic.HolographicCamera"},
    {"Windows.Graphics.Holographic.HolographicCameraPose"},
    {"Windows.Graphics.Holographic.HolographicCameraRenderingParameters"},
    {"Windows.Graphics.Holographic.HolographicFrame"},
    {"Windows.Graphics.Holographic.HolographicFramePrediction"},
    {"Windows.Graphics.Holographic.HolographicFramePresentResult"},
    {"Windows.Graphics.Holographic.HolographicFramePresentWaitBehavior"},
    {"Windows.Graphics.Holographic.HolographicSpace"},
    {"Windows.Graphics.Holographic.HolographicSpaceCameraAddedEventArgs"},
    {"Windows.Graphics.Holographic.HolographicSpaceCameraRemovedEventArgs"},
    {"Windows.Graphics.Holographic.HolographicStereoTransform"},
    {"Windows.Management.Deployment.PackageInstallState", "Paused"},
    {"Windows.Media.Capture.AppCapture"},
    {"Windows.Media.Core.MediaBinder"},
    {"Windows.Media.Core.MediaBindingEventArgs"},
    {"Windows.Media.Core.MediaSource", "CreateFromMediaBinder", "Reset", "State", "StateChanged"},
    {"Windows.Media.Core.MediaSourceState"},
    {"Windows.Media.Core.MediaSourceStateChangedEventArgs"},
    {"Windows.Media.DialProtocol.DialDevice", "FriendlyName", "Thumbnail"},
    {"Windows.Media.Playback.MediaPlaybackItem", "FindFromMediaSource"},
    {"Windows.Media.Playback.MediaPlaybackList", "MaxPrefetchTime", "SetShuffledItems", "ShuffledItems", "StartingItem"},
    {"Windows.Media.Playback.MediaPlayer", "AddAudioEffect", "RemoveAllEffects"},
    {"Windows.Media.Protection.ProtectionCapabilities"},
    {"Windows.Media.Protection.ProtectionCapabilityResult"},
    {"Windows.Media.Streaming.Adaptive.AdaptiveMediaSource", "AdvancedSettings"},
    {"Windows.Media.Streaming.Adaptive.AdaptiveMediaSourceAdvancedSettings"},
    {"Windows.Perception.People.HeadPose"},
    {"Windows.Perception.PerceptionTimestamp"},
    {"Windows.Perception.PerceptionTimestampHelper"},
    {"Windows.Perception.Spatial.SpatialAnchor"},
    {"Windows.Perception.Spatial.SpatialAnchorManager"},
    {"Windows.Perception.Spatial.SpatialAnchorRawCoordinateSystemAdjustedEventArgs"},
    {"Windows.Perception.Spatial.SpatialAnchorStore"},
    {"Windows.Perception.Spatial.SpatialAnchorTransferManager"},
    {"Windows.Perception.Spatial.SpatialBoundingBox"},
    {"Windows.Perception.Spatial.SpatialBoundingFrustum"},
    {"Windows.Perception.Spatial.SpatialBoundingOrientedBox"},
    {"Windows.Perception.Spatial.SpatialBoundingSphere"},
    {"Windows.Perception.Spatial.SpatialBoundingVolume"},
    {"Windows.Perception.Spatial.SpatialCoordinateSystem"},
    {"Windows.Perception.Spatial.SpatialLocatability"},
    {"Windows.Perception.Spatial.SpatialLocation"},
    {"Windows.Perception.Spatial.SpatialLocator"},
    {"Windows.Perception.Spatial.SpatialLocatorAttachedFrameOfReference"},
    {"Windows.Perception.Spatial.SpatialLocatorPositionalTrackingDeactivatingEventArgs"},
    {"Windows.Perception.Spatial.SpatialPerceptionAccessStatus"},
    {"Windows.Perception.Spatial.SpatialStationaryFrameOfReference"},
    {"Windows.Perception.Spatial.Surfaces.SpatialSurfaceInfo"},
    {"Windows.Perception.Spatial.Surfaces.SpatialSurfaceMesh"},
    {"Windows.Perception.Spatial.Surfaces.SpatialSurfaceMeshBuffer"},
    {"Windows.Perception.Spatial.Surfaces.SpatialSurfaceMeshOptions"},
    {"Windows.Perception.Spatial.Surfaces.SpatialSurfaceObserver"},
    {"Windows.Security.Authentication.Web.Core.WebTokenRequest", "AppProperties"},
    {"Windows.Security.Authentication.Web.Provider.WebAccountManager", "PullCookiesAsync"},
    {"Windows.Security.Credentials.KeyCredential", "RetrievePublicKey"},
    {"Windows.Services.Maps.MapService", "DataAttributions"},
    {"Windows.Storage.DownloadsFolder", "CreateFileForUserAsync", "CreateFolderForUserAsync"},
    {"Windows.Storage.KnownFolderId"},
    {"Windows.Storage.KnownFolders", "GetFolderForUserAsync"},
    {"Windows.Storage.StorageLibrary", "GetLibraryForUserAsync"},
    {"Windows.System.MemoryManager", "TrySetAppMemoryUsageLimit"},
    {"Windows.System.Profile.PlatformDataCollectionLevel"},
    {"Windows.System.Profile.PlatformDiagnosticsAndUsageDataSettings"},
    {"Windows.UI.Composition.ColorKeyFrameAnimation"},
    {"Windows.UI.Composition.CompositionAnimation", "SetColorParameter", "SetQuaternionParameter"},
    {"Windows.UI.Composition.CompositionBackfaceVisibility"},
    {"Windows.UI.Composition.CompositionBatchCompletedEventArgs"},
    {"Windows.UI.Composition.CompositionBatchTypes"},
    {"Windows.UI.Composition.CompositionBitmapInterpolationMode"},
    {"Windows.UI.Composition.CompositionBorderMode"},
    {"Windows.UI.Composition.CompositionBrush"},
    {"Windows.UI.Composition.CompositionColorBrush"},
    {"Windows.UI.Composition.CompositionColorSpace"},
    {"Windows.UI.Composition.CompositionCommitBatch"},
    {"Windows.UI.Composition.CompositionCompositeMode"},
    {"Windows.UI.Composition.CompositionDrawingSurface"},
    {"Windows.UI.Composition.CompositionEffectBrush"},
    {"Windows.UI.Composition.CompositionEffectFactory", "CreateBrush", "ExtendedError", "LoadStatus"},
    {"Windows.UI.Composition.CompositionEffectFactoryLoadStatus", "Pending"},
    {"Windows.UI.Composition.CompositionGraphicsDevice", "CreateDrawingSurface", "RenderingDeviceReplaced"},
    {"Windows.UI.Composition.CompositionObject", "StartAnimation", "StopAnimation"},
    {"Windows.UI.Composition.CompositionPropertySet", "InsertColor", "InsertQuaternion", "TryGetColor", "TryGetQuaternion"},
    {"Windows.UI.Composition.CompositionScopedBatch"},
    {"Windows.UI.Composition.CompositionSurfaceBrush"},
    {"Windows.UI.Composition.Compositor", "CreateColorBrush", "CreateColorKeyFrameAnimation", "CreateQuaternionKeyFrameAnimation", "CreateScopedBatch", "CreateSpriteVisual", "CreateSurfaceBrush", "GetCommitBatch"},
    {"Windows.UI.Composition.QuaternionKeyFrameAnimation"},
    {"Windows.UI.Composition.RenderingDeviceReplacedEventArgs"},
    {"Windows.UI.Composition.SpriteVisual"},
    {"Windows.UI.Composition.Visual", "AnchorPoint", "BackfaceVisibility", "BorderMode", "CompositeMode", "IsVisible", "RotationAngleInDegrees"},
    {"Windows.UI.Core.CoreWindow", "PointerRoutedAway", "PointerRoutedReleased", "PointerRoutedTo"},
    {"Windows.UI.Core.ICorePointerRedirector"},
    {"Windows.UI.Input.KeyboardDeliveryInterceptor"},
    {"Windows.UI.Input.Spatial.SpatialGestureRecognizer"},
    {"Windows.UI.Input.Spatial.SpatialGestureSettings"},
    {"Windows.UI.Input.Spatial.SpatialHoldCanceledEventArgs"},
    {"Windows.UI.Input.Spatial.SpatialHoldCompletedEventArgs"},
    {"Windows.UI.Input.Spatial.SpatialHoldStartedEventArgs"},
    {"Windows.UI.Input.Spatial.SpatialInteraction"},
    {"Windows.UI.Input.Spatial.SpatialInteractionDetectedEventArgs"},
    {"Windows.UI.Input.Spatial.SpatialInteractionManager"},
    {"Windows.UI.Input.Spatial.SpatialInteractionSource"},
    {"Windows.UI.Input.Spatial.SpatialInteractionSourceEventArgs"},
    {"Windows.UI.Input.Spatial.SpatialInteractionSourceKind"},
    {"Windows.UI.Input.Spatial.SpatialInteractionSourceLocation"},
    {"Windows.UI.Input.Spatial.SpatialInteractionSourceProperties"},
    {"Windows.UI.Input.Spatial.SpatialInteractionSourceState"},
    {"Windows.UI.Input.Spatial.SpatialManipulationCanceledEventArgs"},
    {"Windows.UI.Input.Spatial.SpatialManipulationCompletedEventArgs"},
    {"Windows.UI.Input.Spatial.SpatialManipulationDelta"},
    {"Windows.UI.Input.Spatial.SpatialManipulationStartedEventArgs"},
    {"Windows.UI.Input.Spatial.SpatialManipulationUpdatedEventArgs"},
    {"Windows.UI.Input.Spatial.SpatialNavigationCanceledEventArgs"},
    {"Windows.UI.Input.Spatial.SpatialNavigationCompletedEventArgs"},
    {"Windows.UI.Input.Spatial.SpatialNavigationStartedEventArgs"},
    {"Windows.UI.Input.Spatial.SpatialNavigationUpdatedEventArgs"},
    {"Windows.UI.Input.Spatial.SpatialPointerPose"},
    {"Windows.UI.Input.Spatial.SpatialRecognitionEndedEventArgs"},
    {"Windows.UI.Input.Spatial.SpatialRecognitionStartedEventArgs"},
    {"Windows.UI.Input.Spatial.SpatialTappedEventArgs"},
    {"Windows.UI.StartScreen.JumpList"},
    {"Windows.UI.StartScreen.JumpListItem"},
    {"Windows.UI.StartScreen.JumpListItemKind"},
    {"Windows.UI.StartScreen.JumpListSystemGroupKind"},
    {"Windows.UI.Text.Core.CoreTextEditContext", "NotifyFocusLeaveCompleted"},
    {"Windows.UI.ViewManagement.ApplicationViewTransferContext"},
    {"Windows.UI.WebUI.WebUIDevicePairingActivatedEventArgs"},
    {"Windows.UI.Xaml.Automation.AutomationElementIdentifiers", "LandmarkTypeProperty", "LocalizedLandmarkTypeProperty"},
    {"Windows.UI.Xaml.Automation.AutomationProperties", "GetLandmarkType", "GetLocalizedLandmarkType", "LandmarkTypeProperty", "LocalizedLandmarkTypeProperty", "SetLandmarkType", "SetLocalizedLandmarkType"},
    {"Windows.UI.Xaml.Automation.Peers.AutomationLandmarkType"},
    {"Windows.UI.Xaml.Automation.Peers.AutomationPeer", "GetLandmarkType", "GetLandmarkTypeCore", "GetLocalizedLandmarkType", "GetLocalizedLandmarkTypeCore"},
    {"Windows.UI.Xaml.Controls.Maps.MapActualCameraChangedEventArgs", "ChangeReason"},
    {"Windows.UI.Xaml.Controls.Maps.MapActualCameraChangingEventArgs", "ChangeReason"},
    {"Windows.UI.Xaml.Controls.Maps.MapCameraChangeReason"},
    {"Windows.UI.Xaml.Controls.Maps.MapControl", "MapRightTapped"},
    {"Windows.UI.Xaml.Controls.Maps.MapLoadingStatus", "DataUnavailable"},
    {"Windows.UI.Xaml.Controls.Maps.MapPolygon", "Paths"},
    {"Windows.UI.Xaml.Controls.Maps.MapRightTappedEventArgs"},
    {"Windows.UI.Xaml.Controls.Maps.MapTargetCameraChangedEventArgs", "ChangeReason"},
    {"Windows.UI.Xaml.Controls.MenuFlyoutPresenter", "TemplateSettings"},
    {"Windows.UI.Xaml.Controls.Primitives.ComboBoxTemplateSettings", "DropDownContentMinWidth"},
    {"Windows.UI.Xaml.Controls.Primitives.CommandBarTemplateSettings", "OverflowContentMaxWidth"},
    {"Windows.UI.Xaml.Controls.Primitives.MenuFlyoutPresenterTemplateSettings"},
    {"Windows.UI.Xaml.Controls.RichEditBox", "ClipboardCopyFormat", "ClipboardCopyFormatProperty", "GetLinguisticAlternativesAsync"},
    {"Windows.UI.Xaml.Controls.RichEditClipboardFormat"},
    {"Windows.UI.Xaml.Controls.TextBox", "GetLinguisticAlternativesAsync"},
    {"Windows.UI.Xaml.Controls.WebViewPermissionType", "PointerLock"},
    {"Windows.UI.Xaml.Hosting.ElementCompositionPreview", "GetElementChildVisual", "GetElementVisual", "GetScrollViewerManipulationPropertySet", "SetElementChildVisual"},
    {"Windows.UI.Xaml.Media.FontFamily", "XamlAutoFontFamily"},
    {"Windows.UI.Xaml.Media.PartialMediaFailureDetectedEventArgs", "ExtendedError"},
    {"Windows.Web.Http.Filters.HttpBaseProtocolFilter", "CookieUsageBehavior"},
    {"Windows.Web.Http.Filters.HttpCookieUsageBehavior"}
                }
        Return _d
    End Function


    <Extension>
    Sub Add(d As Dictionary(Of String, List(Of String)), type As String, ParamArray members As String())
        If members.Length = 0 Then d.Add(type, Nothing) Else d.Add(type, members.ToList())
    End Sub

End Module

