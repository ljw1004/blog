Imports Windows.Devices.Input

Public NotInheritable Class MainPage
    Inherits Page

    Dim Pointers As New Dictionary(Of UInteger, Point)
    Dim CurrentReleaseStarted As DateTime?
    Dim CurrentPlayStarted As DateTime?
    Dim TickWatch As New Stopwatch
    Dim isPageOpen As Boolean = False

    WithEvents App As App = App.Current
    Public ReadOnly Property Dat As GameData
        Get
            Return App.Dat
        End Get
    End Property

    Protected Overrides Async Sub OnNavigatedTo(e As NavigationEventArgs)
        Try
            isPageOpen = True
            UpdateDashboard()
            TickWatch.Start()
            While isPageOpen
                Try
                    Dim t = Task.Delay(15)
                    If CurrentPlayStarted IsNot Nothing Then App.Tick(TickWatch.Elapsed) : TickWatch.Reset() : TickWatch.Start()
                    If CurrentPlayStarted IsNot Nothing AndAlso CurrentReleaseStarted IsNot Nothing AndAlso (DateTime.Now - CurrentReleaseStarted.Value).TotalMilliseconds > 50 Then StopPlay()
                    Await t.Log("Task.Delay", 15)
                Catch ex As Exception
                    App.ReportErrorAsync("Loop", ex).FireAndForget
                End Try
            End While
        Catch ex As Exception
            App.ReportErrorAsync("OnNavigatedTo", ex).FireAndForget()
        End Try
    End Sub

    Protected Overrides Sub OnNavigatedFrom(e As NavigationEventArgs)
        Try
            isPageOpen = False
        Catch ex As Exception
            App.ReportErrorAsync("OnNavigatedFrom", ex).FireAndForget()
        End Try
    End Sub

    Sub OnDatChanged() Handles App.DatChanged
        Try
            playArea1.Content = Nothing
            If Dat IsNot Nothing Then playArea1.Content = Dat.Canvas
            OnResize(Nothing, Nothing)
            UpdateDashboard()
        Catch ex As Exception
            App.ReportErrorAsync("OnDatChanged", ex).FireAndForget()
        End Try
    End Sub

    Sub OnResize(sender As Object, e As SizeChangedEventArgs) Handles Me.SizeChanged
        Try
            Dim w = CInt(ActualWidth), h = CInt(ActualHeight)
            If h > w * 5 \ 3 Then h = w * 5 \ 3 Else w = h * 3 \ 5
            If Dat IsNot Nothing Then GameLogic.Resize(Dat, w, h)
            grid1.Width = w : grid1.Height = h
            paused2.Width = w
        Catch ex As Exception
            App.ReportErrorAsync("OnResize", ex).FireAndForget()
        End Try
    End Sub

    Sub StartPlay()
        If CurrentPlayStarted Is Nothing Then CurrentPlayStarted = DateTime.Now
        CurrentReleaseStarted = Nothing
        UpdateDashboard()
        TickWatch.Start()
    End Sub

    Sub StopPlay()
        TickWatch.Stop()
        If CurrentPlayStarted IsNot Nothing Then App.HighScores.TimePlayed += DateTime.Now - CurrentPlayStarted.Value
        CurrentPlayStarted = Nothing
        UpdateDashboard()
    End Sub

    Protected Overrides Sub OnPointerPressed(e As PointerRoutedEventArgs)
        Try
            e.Handled = True
            CurrentReleaseStarted = Nothing
            Pointers(e.Pointer.PointerId) = PointerToPoint(e)
            If CurrentPlayStarted Is Nothing Then StartPlay()
        Catch ex As Exception
            App.ReportErrorAsync("OnPointerPressed", ex).FireAndForget()
        End Try
    End Sub

    Function PointerToPoint(e As PointerRoutedEventArgs) As Point
        Dim pt = e.GetCurrentPoint(grid1).Position
        Dim finger = 40.0
        Dim info = DisplayInformation.GetForCurrentView()
        If info.RawDpiX > 0 Then finger = 0.3 * info.RawDpiX / info.RawPixelsPerViewPixel
        pt.Y -= finger
        Return pt
    End Function

    Protected Overrides Sub OnPointerMoved(e As PointerRoutedEventArgs)
        Try
            e.Handled = True
            If Pointers.ContainsKey(e.Pointer.PointerId) Then Pointers(e.Pointer.PointerId) = PointerToPoint(e)
            If Dat IsNot Nothing Then GameLogic.HandlePointers(Dat, Pointers)
        Catch ex As Exception
            App.ReportErrorAsync("OnPointerMoved", ex).FireAndForget()
        End Try
    End Sub

    Protected Overrides Sub OnPointerReleased(e As PointerRoutedEventArgs)
        Try
            e.Handled = True
            If Pointers.ContainsKey(e.Pointer.PointerId) Then Pointers.Remove(e.Pointer.PointerId)
            If Pointers.Count = 0 Then CurrentReleaseStarted = DateTime.Now
            If Dat IsNot Nothing Then GameLogic.HandlePointers(Dat, Pointers)
        Catch ex As Exception
            App.ReportErrorAsync("OnPointerReleased", ex).FireAndForget()
        End Try
    End Sub

    Sub UpdateDashboard()
        buyGuidanceSystem.Visibility = If(App.HasGuidanceSystem, Visibility.Collapsed, Visibility.Visible)
        buyApp.Visibility = If(App.IsTrial, Visibility.Visible, Visibility.Collapsed)
        '
        Dim bricks = If(App.HighScores.BricksDestroyed < 10000, CStr(App.HighScores.BricksDestroyed), App.HighScores.BricksDestroyed.ToString("#,##0").Replace(",", " "))
        scoreBricks.Text = bricks & " bricks"
        scoreGames.Text = CStr(App.HighScores.GamesWon) & " games"
        Dim hours = App.HighScores.TimePlayed.Days * 24 + App.HighScores.TimePlayed.Hours
        Dim time = If(hours > 0, hours & "h", "") & App.HighScores.TimePlayed.Minutes & "m" & App.HighScores.TimePlayed.Seconds & "s"
        scoreTime.Text = time & " played"
        '
        paused1.Visibility = If(CurrentPlayStarted Is Nothing, Visibility.Visible, Visibility.Collapsed)
        paused2.Visibility = paused1.Visibility
        If paused1.Visibility = Visibility.Visible Then
            StatusBar.GetForCurrentView.HideAsync().FireAndForget()
        Else
            StatusBar.GetForCurrentView.ShowAsync().FireAndForget()
        End If
        loading1.Visibility = If(Dat Is Nothing AndAlso App.isStartingLevel, Visibility.Visible, Visibility.Collapsed)
    End Sub

    Private Async Sub buyGuidanceSystem_Click(sender As Object, e As RoutedEventArgs) Handles buyGuidanceSystem.Click
        Try
            Await App.PurchaseGuidanceSystemAsync().Log("PurchaseGuidanceSystemAsync")
            UpdateDashboard()
        Catch ex As Exception
            App.ReportErrorAsync("buyGuidanceSystem_Click", ex).FireAndForget()
        End Try
    End Sub

    Private Async Sub buyApp_Click(sender As Object, e As RoutedEventArgs) Handles buyApp.Click
        Try
            Await App.PurchaseAppAsync().Log("PurchaseAppAsync")
            UpdateDashboard()
        Catch ex As Exception
            App.ReportErrorAsync("buyApp_Click", ex).FireAndForget()
        End Try
    End Sub

    Private Sub paused1_PointerMoved(sender As Object, e As PointerRoutedEventArgs) Handles paused1.PointerMoved
        e.Handled = True
    End Sub
End Class
