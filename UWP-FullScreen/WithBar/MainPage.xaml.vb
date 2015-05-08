Imports Windows.Foundation.Metadata

Public NotInheritable Class MainPage
    Inherits Page

    ' I am writing a casual game. I need to decide how to handle
    '   - Windowed mode (available only on Desktop)
    '   - Tablet mode (available on Desktop; is default on Mobile)
    '   - FullScreen mode (available on Desktop and Mobile)
    '
    ' Design: my app isn't very immersive. It's a quick time-waster game...
    '  - On Mobile and on Desktop Tablet Mode, to keep things simple,
    '    I'll just accept Tablet mode as a natural decent UI. No further
    '    user worries.
    '  - On Desktop Desktop mode I will offer the user a button
    '    within my game to go full-screen, or they can press Alt+Enter.
    '
    '
    ' What is the difference between Tablet and FullScreen mode? The two are not the same...
    '  - Tablet mode is a system-wide setting to make apps feel better on touch-first devices
    '    like phones, tablets and 2-in-1s. Tablet mode means that apps are full-screen without
    '    title bar, but the soft navigation controls (phone) and taskbar (desktop) remain.
    '    All apps benefit from tablet mode automatically without any work on their part, even
    '    win32 apps.
    '  - FullScreen mode requires work on the app to support. Full screen mode is when
    '    even the soft navigation controls (phone) or taskbar (desktop) are hidden and the
    '    app fills the entire screen. For Win8.1 apps, the system automatically provides
    '    an "enter full-screen" button just to the left of the minimize button.
    '    For UWP apps the app must decide where to put the button. Some might put the button
    '    in a custom-drawn title bar, if switching is frequent. Games might put the button
    '    in a Settings page. Video/Photo players might put a button in the content itself
    '    so you can maximize the content. Powerpoint might have a taskbar button to enter
    '    full-screen presentation mode. The app can also specify in code whether it
    '    prefers its first ever launch to be full-screen or not. Subsequent launches will
    '    automatically use the app's previous state.
    '
    '
    ' How will users get out of my game?
    '  - Mobile: they can press the hardware Home or Back buttons. If their phone only
    '    has soft Home/Back buttons, well, my app isn't full-screen so they can use that too.
    '  - Desktop Tablet Mode: the taskbar will be there. They can also mouse to the top,
    '    or swipe from the top or sides. They can also press Esc.
    '  - Desktop Full Screen Mode: they can mouse to the top, or swipe from the top or sides,
    '    or press Esc.
    '  - Desktop Windowed Mode: they can use the Minimize/Maximimize/Close buttons, or the
    '    taskbar.
    '
    '  * KNOWN OS BUG: TryEnterFullScreen doesnt yet work on Mobile. It is supposed to.
    '  * KNOWN OS BUG: If app does TryEnterFullScreenMode in desktop mode, then user
    '    switches to Tablet mode, it should get a SizeChanged event but doesn't.
    '  * KNOWN OS BUG: If app does TryEnterFullScreenMode in desktop mode, then user
    '    switches to Tablet mode, then app does ExitFullScreenMode, then the app
    '    sometimes gets turned into an overlapped window, and sometimes gets turned
    '    into a proper Tablet mode window but with a permanently-on title bar that wrongly
    '    has a minimize button.
    '  * KNOWN OS BUG 2162940: as of Win10 build 10074, when device orientation changes
    '    and you're in tablet mode, apps fail to resize to the new orientation. This is
    '    supposed to happen automatically.
    '  * KNOWN OS BUG 2635736: as of Win10 build 10074, if you enter full-screen then leave it
    '    correctly resizes back to the size you were before; but if you enter a second time
    '    and leave again a second time then it fails to resize back to what you were before.
    '    It's supposed to.
    '  * KNOWN OS BUG 2635736: as of Win10 build 10074, sometimes when you launch the app it launches
    '    at the wrong size. It's supposed to remember what size it was before and launch at
    '    that size.
    '  * KNOWN OS BUG 2280327: as of Win10 build 10074, on mobile and desktop,
    '    ApplicationView.Orientation often gives the wrong answers. You can't trust it.

    Sub New()
        InitializeComponent()

        ' On Mobile, hide the status bar
        If ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar") Then
            StatusBar.GetForCurrentView.HideAsync().FireAndForget()
        End If

        ' In App.OnLaunched, ApplicationView.PreferredLaunchWindowingMode
        ' is left at its default value of "Auto". That will automatically
        ' launch my app the same size and same full-screenedness as it was when it
        ' last exited.
        f()
    End Sub

    Async Sub f()
        While True
            UpdateStatus()
            Await Task.Delay(1000)
        End While
    End Sub

    Sub UpdateStatus() Handles Me.SizeChanged
        Dim isFullScreen = ApplicationView.GetForCurrentView.IsFullScreenMode
        Dim isTabletMode = (UIViewSettings.GetForCurrentView().UserInteractionMode = UserInteractionMode.Touch)
        Dim isLandscape = (Window.Current.Bounds.Width >= Window.Current.Bounds.Height)

        'button1.Visibility = If(isTabletMode, Visibility.Collapsed, Visibility.Visible)
        button1.Content = If(isFullScreen, "ExitFullScreenMode", "TryEnterFullScreenMode")
        label1.Text = $"IsFullScreen: {isFullScreen}"
        label2.Text = $"IsTabletMode: {isTabletMode}"
        label3.Text = $"WindowOrientation: {If(isLandscape, "Landscape", "Portrait")}"
        label4.Text = $"Window.Bounds: ({Window.Current.Bounds.Width:0.0}, {Window.Current.Bounds.Height:0.0})"
        label5.Text = $"CurrentRotation: {DisplayInformation.GetForCurrentView.CurrentOrientation}"
        label6.Text = $"NativeRotation: {DisplayInformation.GetForCurrentView.NativeOrientation}"
    End Sub

    Sub FullScreenButton() Handles button1.Click
        Dim isFullScreen = ApplicationView.GetForCurrentView.IsFullScreenMode

        If isFullScreen Then
            ApplicationView.GetForCurrentView.ExitFullScreenMode()
        Else
            ApplicationView.GetForCurrentView.TryEnterFullScreenMode()
        End If
    End Sub
End Class


Module Utils
    <Extension>
    Async Sub FireAndForget(t As Task)
        Dim ex As Exception = Nothing
        Try
            Await t
            Return
        Catch ex1 As Exception
            ex = ex1
        End Try
        Stop
        Dim msg As New Windows.UI.Popups.MessageDialog(ex.Message, "Catastrophic error. This was never supposed to fail.")
        Await msg.ShowAsync()
    End Sub

    <Extension>
    Sub FireAndForget(t As IAsyncAction)
        t.AsTask.FireAndForget()
    End Sub

    Declare Function MessageBeep Lib "user32.dll" (type As Integer) As Boolean

End Module