Imports System.Threading.Tasks
Imports Windows.ApplicationModel
Imports Windows.ApplicationModel.Activation
Imports Windows.ApplicationModel.Store
Imports Windows.Storage
Imports Windows.UI.ViewManagement
Imports Windows.UI.Xaml
Imports Windows.UI.Xaml.Controls


Public Interface IPlatformAbstraction
    Function SetStatusbarVisibilityAsync(isVisible As Boolean) As Task
End Interface

Public Class App
    Public Shared Platform As IPlatformAbstraction

    ' TRANSIENT STATE
    Shared RND As New Random()
    Shared _HasSpeedBoost As Boolean? = Nothing
    Shared BeepFile As StorageFile
    Shared wavePlayer As New WavePlayer

    ' LOCAL STATE
    Public Shared ix, iy, rz As Integer
    Public Shared dx, dy, dr As Integer

    ' ROAMING STATE
    Public Shared useBeachBall As Boolean = False

    Shared Sub LoadState()
        Dim v = ApplicationData.Current.LocalSettings.Values("dx")
        dx = If(v Is Nothing, RND.Next(20), CInt(v))
        v = ApplicationData.Current.LocalSettings.Values("dy")
        dy = If(v Is Nothing, RND.Next(20), CInt(v))
        v = ApplicationData.Current.LocalSettings.Values("dr")
        dr = If(v Is Nothing, RND.Next(30), CInt(v))
        ix = CInt(ApplicationData.Current.LocalSettings.Values("ix"))
        iy = CInt(ApplicationData.Current.LocalSettings.Values("iy"))
        rz = CInt(ApplicationData.Current.LocalSettings.Values("rz"))
        '
        LoadRoamingState(ApplicationData.Current, Nothing)
    End Sub

    Shared Sub LoadRoamingState(d As ApplicationData, o As Object)
        useBeachBall = CBool(d.RoamingSettings.Values("useBeachBall"))
    End Sub

    Shared Sub SaveState()
        ApplicationData.Current.LocalSettings.Values("dx") = dx
        ApplicationData.Current.LocalSettings.Values("dy") = dy
        ApplicationData.Current.LocalSettings.Values("dr") = dr
        ApplicationData.Current.LocalSettings.Values("ix") = ix
        ApplicationData.Current.LocalSettings.Values("iy") = iy
        ApplicationData.Current.LocalSettings.Values("rz") = rz
        '
        ApplicationData.Current.RoamingSettings.Values("useBeachBall") = useBeachBall
    End Sub


    Shared ReadOnly Property HasSpeedBoost As Boolean
        Get
            If _HasSpeedBoost Is Nothing Then _HasSpeedBoost = CurrentApp.LicenseInformation.ProductLicenses("SpeedBoost").IsActive
            Return _HasSpeedBoost.Value
        End Get
    End Property

    Shared Async Function PurchaseSpeedBoostAsync() As Task
        If HasSpeedBoost() Then Return
        _HasSpeedBoost = Nothing
        Dim log = CStr(ApplicationData.Current.LocalSettings.Values("log"))
        If log IsNot Nothing Then
            ' previous run of this app tried to purchase, but didn't succeed...
            ApplicationData.Current.LocalSettings.Values.Remove("log")
            SendErrorReport(log)
            _HasSpeedBoost = True ' so the user can at least use the item
            Return
        End If

        Try
            log = "About to await RequestProductPurchaseAsync"
            ApplicationData.Current.LocalSettings.Values("log") = log
            Dim result = Await CurrentApp.RequestProductPurchaseAsync("SpeedBoost")
            log &= vbCrLf & String.Format("Finished await. Status={0}, OfferId={1}, TransactionId={2}",
                                          result.Status, result.OfferId, result.TransactionId)
            ApplicationData.Current.LocalSettings.Values("log") = log
        Catch ex As Exception
            log &= vbCrLf & "EXCEPTION! " & ex.Message & ex.StackTrace
            ApplicationData.Current.LocalSettings.Values("log") = log
            SendErrorReport(ex)
        End Try
    End Function

    Shared Sub SendErrorReport(ex As Exception)
        SendErrorReport(ex.Message & vbCrLf & "stack:" & vbCrLf & ex.StackTrace)
    End Sub

    Shared Async Sub SendErrorReport(msg As String)
        Dim md As New Windows.UI.Popups.MessageDialog("Oops. There's been an internal error", "Bug report")
        Dim r As Boolean? = Nothing
        md.Commands.Add(New Windows.UI.Popups.UICommand("Send bug report", Sub() r = True))
        md.Commands.Add(New Windows.UI.Popups.UICommand("Cancel", Sub() r = False))
        Await md.ShowAsync()
        If Not r.HasValue OrElse Not r.Value Then Return
        '
        Dim emailTo = "lu@wischik.com"
        Dim emailSubject = "App1 problem report"
        Dim emailBody = "I encountered a problem with App1..." & vbCrLf & vbCrLf & msg
        Dim url = "mailto:?to=" & emailTo & "&subject=" & emailSubject & "&body=" & Uri.EscapeDataString(emailBody)
        Await Windows.System.Launcher.LaunchUriAsync(New Uri(url))
    End Sub


    Shared Sub Tick(maxWidth As Double, maxHeight As Double)
        ix += dx : iy += dy : rz += dr
        If HasSpeedBoost() Then ix += dx : iy += dy : rz += dr
        Dim hasBounced = False
        If ix < 0 Then dx = Math.Abs(dx) : hasBounced = True
        If ix > maxWidth Then dx = -Math.Abs(dx) : hasBounced = True
        If iy < 0 Then dy = Math.Abs(dy) : hasBounced = True
        If iy > maxHeight Then dy = -Math.Abs(dy) : hasBounced = True
        If hasBounced Then
            dx += RND.Next(10) - 5 : dy += RND.Next(10) - 5 : dr = -dr + RND.Next(10) - 5
            If BeepFile IsNot Nothing Then wavePlayer.StartPlay(BeepFile)
        End If
    End Sub

    Public Shared Sub OnLaunched(e As LaunchActivatedEventArgs)
        AddHandler ApplicationData.Current.DataChanged, AddressOf LoadRoamingState
        StartInitializationAsync()

        '
        Dim rootFrame = TryCast(Window.Current.Content, Frame)
        If rootFrame Is Nothing Then
            rootFrame = New Frame()
            'If e.PreviousExecutionState = ApplicationExecutionState.Terminated Then
            LoadState()
            'End If
            Window.Current.Content = rootFrame
        End If
        If rootFrame.Content Is Nothing Then
            rootFrame.Navigate(GetType(App1_Common.AdaptiveMainPage), e.Arguments)
        End If
        Window.Current.Activate()
    End Sub

    Shared Async Sub StartInitializationAsync()
        Try
            Await Task.Run(Async Function()
                               Dim folder = Await Package.Current.InstalledLocation.GetFolderAsync("App1_Common\Assets")
                               BeepFile = Await folder.GetFileAsync("beep.wav")
                           End Function)
        Catch ex As Exception
            SendErrorReport(ex)
        End Try
    End Sub

    Public Shared Sub OnResuming()
        If wavePlayer Is Nothing Then wavePlayer = New WavePlayer()
    End Sub

    Public Shared Async Function OnSuspendingAsync() As Task
        RemoveHandler ApplicationData.Current.DataChanged, AddressOf LoadRoamingState
        SaveState()
        wavePlayer.Dispose() : wavePlayer = Nothing
        If False Then Await Task.Delay(0) ' to suppress the compiler warning
    End Function

End Class
