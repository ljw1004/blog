Imports Newtonsoft.Json
Imports Windows.ApplicationModel.Store
Imports Windows.Storage
Imports Windows.Storage.Search


NotInheritable Class App
    Inherits Application

    ' Persisted data
    Private _HasGuidanceSystem As Boolean? ' (persisted via store)
    Private _IsTrial As Boolean? ' (persisted via store)
    Public HighScores As Scores ' (persisted in RoamingSettings)
    Private _HighScoresFromOthers As Scores ' (also persisted)
    Private _LocalGuid As Guid ' (persist this)
    Public Dat As GameData ' (persisted in local filesystem)

    ' Transient static data, initialized once
    Dim files As New List(Of Tuple(Of StorageFile, Size)) ' this grows over time, updated by background thread, and is SyncLocked on itself
    Private wavePlayer As New WavePlayer
    Dim stockBackgroundsTask As Task(Of IReadOnlyList(Of StorageFile))
    Dim stockForegroundsTask As Task(Of IReadOnlyList(Of StorageFile))
    Dim PaddleBeep, BrickBeep, GuidedBeep, WinBeep, LoseBeep As StorageFile
    Dim RND As New System.Random

    ' Dynamic data for the level currently being played
    Event DatChanged()
    Public isStartingLevel As Boolean = False

    Public Overloads Shared ReadOnly Property Current As App
        Get
            Return CType(Application.Current, App)
        End Get
    End Property


    Protected Overrides Sub OnLaunched(e As LaunchActivatedEventArgs)
        Try
#If WINDOWS_UAP Then
            ApplicationView.GetForCurrentView().SetPreferredMinSize(New Size(300, 500))
#End If
            AddHandler ApplicationData.Current.DataChanged, AddressOf LoadScores
            If ApplicationData.Current.LocalSettings.Values.ContainsKey("LocalGuid") Then
                _LocalGuid = CType(ApplicationData.Current.LocalSettings.Values("LocalGuid"), Guid)
            Else
                _LocalGuid = Guid.NewGuid
                ApplicationData.Current.LocalSettings.Values("LocalGuid") = _LocalGuid
            End If

            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow)
            LoadScores(Nothing, Nothing)
            DoStaticInitializationAsync().FireAndForget()

            Dim rootFrame = CType(Window.Current.Content, Frame)
            If rootFrame Is Nothing Then
                rootFrame = New Frame()
                LoadLocalGameStateAsync().FireAndForget()
                Window.Current.Content = rootFrame
            End If
            If rootFrame.Content Is Nothing Then rootFrame.Navigate(GetType(MainPage), e.Arguments)
            Window.Current.Activate()
        Catch ex As Exception
            ReportErrorAsync("OnLaunched", ex).FireAndForget()
        End Try
    End Sub

    Protected Sub OnResuming(sender As Object, e As Object) Handles Me.Resuming
        If wavePlayer Is Nothing Then wavePlayer = New WavePlayer
    End Sub

    Private Async Sub OnSuspending(sender As Object, e As SuspendingEventArgs) Handles Me.Suspending
        Dim deferral As SuspendingDeferral = e.SuspendingOperation.GetDeferral()
        Try
            SaveScores()
            Await SaveLocalGameStateAsync().Log("SaveLocalGameStateAsync")
            wavePlayer.Dispose() : wavePlayer = Nothing
        Catch ex As Exception
            ReportErrorAsync("OnSuspendingAsync", ex).FireAndForget
        Finally
            deferral.Complete()
        End Try
    End Sub


    Async Function DoStaticInitializationAsync() As Task
        Dim filesTask = Task.Run(AddressOf FileScannerAsync)
        stockBackgroundsTask = Task.Run(Async Function()
                                            Dim folder = Await Package.Current.InstalledLocation.GetFolderAsync("Assets\StockBackgrounds").Log("GetFolderAsync", "StockBackgrounds")
                                            Return Await folder.GetFilesAsync().Log("GetFilesAsync", "StockBackgrounds")
                                        End Function)
        stockForegroundsTask = Task.Run(Async Function()
                                            Dim folder = Await Package.Current.InstalledLocation.GetFolderAsync("Assets\StockForegrounds").Log("GetFolderAsync", "StockForegrounds")
                                            Return Await folder.GetFilesAsync().Log("GetFilesAsync", "StockForegrounds")
                                        End Function)
        Dim beepTask = Task.Run(Async Function()
                                    Dim folder = Await Package.Current.InstalledLocation.GetFolderAsync("Assets").Log("GetFolderAsync", "Assets")
                                    Dim paddleBeepTask = folder.GetFileAsync("beep-paddle.wav")
                                    Dim brickBeepTask = folder.GetFileAsync("beep-brick.wav")
                                    Dim guidedBeepTask = folder.GetFileAsync("beep-guided.wav")
                                    Dim winBeepTask = folder.GetFileAsync("win.wav")
                                    Dim loseBeepTask = folder.GetFileAsync("lose.wav")
                                    PaddleBeep = Await paddleBeepTask.Log("GetFileAsync", "beep-paddle.wav")
                                    BrickBeep = Await brickBeepTask.Log("GetFileAsync", "beep-brick.wav")
                                    GuidedBeep = Await guidedBeepTask.Log("GetFileAsync", "beep-guided.wav")
                                    WinBeep = Await winBeepTask.Log("GetFileAsync", "win.wav")
                                    LoseBeep = Await loseBeepTask.Log("GetFileASync", "lose.wav")
                                End Function)
        Await Task.WhenAll(filesTask, stockBackgroundsTask, stockForegroundsTask, beepTask).Log("Task.WhenAll", "files/backgrounds/foregrounds/beepTask")
    End Function


    Async Function FileScannerAsync() As Task
        ' CommonFileQuery.OrderBySearchRank: works on Windows8.1, but Phone8.1 throws exception on all but DefaultQuery
        Dim queue As New Queue(Of StorageFolder)
        Dim folder = KnownFolders.PicturesLibrary

        While True ' outer loop over folders and subfolders
            Dim subfolders = Await folder.GetFoldersAsync().Log("GetFoldersAsync", folder.Path)
            For Each ff In subfolders : queue.Enqueue(ff) : Next
            Dim index = 0UI
            While True ' inner loop over files within folder
                Dim batch = Await folder.GetFilesAsync(CommonFileQuery.DefaultQuery, index, 10).Log("GetFilesAsync", index)
                If batch.Count = 0 Then Exit While
                index += CUInt(batch.Count)
                Dim prevcount = files.Count
                '
                Dim props = Await Task.WhenAll(batch.Select(Function(file) file.Properties.GetImagePropertiesAsync().AsTask())).Log("Task.WhenAll", "getImagePropertiesAsyncTasks")
                Dim toAdd As New List(Of Tuple(Of StorageFile, Size))
                For i = 0 To batch.Count - 1
                    If batch(i).Path.Contains("Screenshots") Then Continue For
                    Dim w = CInt(props(i).Width), h = CInt(props(i).Height)
                    If w < 300 OrElse h < 300 Then Continue For
                    If w > 5000 OrElse h > 5000 Then Continue For
                    toAdd.Add(Tuple.Create(batch(i), New Size(w, h)))
                Next
                '
                SyncLock files
                    files.AddRange(toAdd)
                End SyncLock
                '
                ' Every 100 files or so, pause for 10 seconds
                Dim newcount = files.Count
                If prevcount \ 100 < newcount \ 100 Then Await Task.Delay(10000).Log("Task.Delay", 10000)

            End While
            If queue.Count = 0 Then Exit While
            folder = queue.Dequeue()
        End While
    End Function

    Public Function HasGuidanceSystem() As Boolean
#If WINDOWS_UAP Then
        Return True
#End If
        If _HasGuidanceSystem IsNot Nothing Then Return _HasGuidanceSystem.Value
        _HasGuidanceSystem = CurrentApp.LicenseInformation.ProductLicenses("GuidanceSystem").IsActive
        Return _HasGuidanceSystem.Value
    End Function

    Public Async Function PurchaseGuidanceSystemAsync() As Task
        If HasGuidanceSystem() Then Return
        _HasGuidanceSystem = Nothing
        Dim log = CStr(ApplicationData.Current.LocalSettings.Values("PurchaseGuidanceSystemLog"))
        If log IsNot Nothing Then
            ApplicationData.Current.LocalSettings.Values.Remove("PurchaseGuidanceSystemLog")
            ReportErrorAsync("PurchaseGuidanceSystemAsync", log).FireAndForget()
            _HasGuidanceSystem = True
            Return
        End If

        Try
            log = "About to call RequestProductPurchaseAsync(""GuidanceSystem"")"
            ApplicationData.Current.LocalSettings.Values("PurchaseGuidanceSystemLog") = log
            Dim task = CurrentApp.RequestProductPurchaseAsync("GuidanceSystem")
            log &= vbCrLf & "Got back IAsyncOperation with ex = " & If(task.ErrorCode Is Nothing, "null", task.ErrorCode.Message) & vbCrLf & "About to await it..."
            ApplicationData.Current.LocalSettings.Values("PurchaseGuidanceSystemLog") = log
            Dim result = Await task
            log &= vbCrLf & String.Format("Finished await. Status={0}, OfferID={1}, TransactionId={2}", result.Status, result.OfferId, result.TransactionId)
            ApplicationData.Current.LocalSettings.Values("PurchaseGuidanceSystemLog") = log
        Catch ex As Exception
            log &= vbCrLf & "EXCEPTION! ex.Message = " & ex.Message & vbCrLf & "Stack = " & ex.StackTrace & vbCrLf & "About to call StartErrorReport"
            ApplicationData.Current.LocalSettings.Values("PurchaseGuidanceSystemLog") = log
            ReportErrorAsync("PurchaseGuidanceSystemAsync", ex).FireAndForget
            log &= vbCrLf & "Finished Call To StartErrorReport"
            ApplicationData.Current.LocalSettings.Values("PurchaseGuidanceSystemLog") = log
        End Try
    End Function

    Public Function IsTrial() As Boolean
#If WINDOWS_UAP Then
        Return False
#End If
        If _IsTrial IsNot Nothing Then Return _IsTrial.Value
        _IsTrial = CurrentApp.LicenseInformation.IsActive AndAlso CurrentApp.LicenseInformation.IsTrial
        Return _IsTrial.Value
    End Function

    Public Async Function PurchaseAppAsync() As Task
        If Not IsTrial() Then Return
        _IsTrial = Nothing

        Dim log = CStr(ApplicationData.Current.LocalSettings.Values("PurchaseAppLog"))
        If log IsNot Nothing Then
            ApplicationData.Current.LocalSettings.Values.Remove("PurchaseAppLog")
            ReportErrorAsync("PurchaseAppAsync", log).FireAndForget()
            _IsTrial = False
            Return
        End If

        Try
            log = "About To Call RequestAppPurchaseAsync(False). IsActive=" & CurrentApp.LicenseInformation.IsActive & ", IsTrial=" & CurrentApp.LicenseInformation.IsTrial
            ApplicationData.Current.LocalSettings.Values("PurchaseAppLog") = log
            Dim task = CurrentApp.RequestAppPurchaseAsync(False)
            log &= vbCrLf & "Got back IAsyncOperation With ex = " & If(task.ErrorCode Is Nothing, "null", task.ErrorCode.Message) & vbCrLf & "About To await it..."
            ApplicationData.Current.LocalSettings.Values("PurchaseAppLog") = log
            Dim result = Await task
            log &= vbCrLf & "Finished await. Result = """ & result & """"
            ApplicationData.Current.LocalSettings.Values("PurchaseAppLog") = log
        Catch ex As Exception
            log &= vbCrLf & "EXCEPTION! ex.Message = " & ex.Message & vbCrLf & "Stack = " & ex.StackTrace & vbCrLf & "About To Call StartErrorReport"
            ApplicationData.Current.LocalSettings.Values("PurchaseAppLog") = log
            ReportErrorAsync("PurchaseAppAsync", ex).FireAndForget()
            log &= vbCrLf & "Finished Call To StartErrorReport"
            ApplicationData.Current.LocalSettings.Values("PurchaseAppLog") = log
        End Try
    End Function

    Sub SaveScores()
        Dim localDelta = HighScores - _HighScoresFromOthers
        Dim s = JsonConvert.SerializeObject(localDelta)
        ApplicationData.Current.RoamingSettings.Values(_LocalGuid.ToString()) = s
    End Sub

    Sub LoadScores(data As ApplicationData, o As Object)
        Dim others As New Scores, local As New Scores
        For Each kv In ApplicationData.Current.RoamingSettings.Values.ToList()
            Try
                Dim s = JsonConvert.DeserializeObject(Of Scores)(CType(kv.Value, String))
                If kv.Key = _LocalGuid.ToString() Then local = s Else others += s
            Catch ex As Exception
                ApplicationData.Current.RoamingSettings.Values.Remove(kv.Key)
            End Try
        Next
        '
        If HighScores IsNot Nothing Then local = HighScores - _HighScoresFromOthers
        _HighScoresFromOthers = others
        HighScores = others + local
    End Sub


    Async Function LoadLocalGameStateAsync() As Task
        If isStartingLevel Then Throw New InvalidOperationException("Starting level")

        Try
            isStartingLevel = True
            Dat = Nothing
            RaiseEvent DatChanged()

            Try
                Dim file = Await ApplicationData.Current.LocalFolder.GetFileAsync("save.json").Log("GetFileAsync", "save.json")
                Using stream = Await file.OpenStreamForReadAsync().Log("file.OpenStreamForReadAsync"),
                    reader As New StreamReader(stream)
                    Dim s = Await reader.ReadToEndAsync()
                    Dat = JsonConvert.DeserializeObject(Of GameData)(s)
                    GameLogic.InitializeCanvasForGameData(Dat)
                End Using
            Catch ex As FileNotFoundException
            End Try

        Finally
            isStartingLevel = False
            RaiseEvent DatChanged()
            If Dat Is Nothing Then NewLevelAsync().FireAndForget()
        End Try
    End Function

    Async Function SaveLocalGameStateAsync() As Task
        Dim file = Await ApplicationData.Current.LocalFolder.CreateFileAsync("save.json", CreationCollisionOption.ReplaceExisting).Log("CreateFileAsync", "save.json:CollisionOption.ReplaceExisting")
        Using stream = Await file.OpenStreamForWriteAsync().Log("file.OpenStreamForWriteAsync"),
                                writer As New StreamWriter(stream)
            Dim s = JsonConvert.SerializeObject(Dat)
            Await writer.WriteAsync(s).Log("writer.WriteAsync", s.Length)
        End Using
    End Function

    Async Function NewLevelAsync() As Task
        If isStartingLevel Then Return
        isStartingLevel = True
        SaveScores()
        RaiseEvent DatChanged()
        '
        Do
            Dim background As StorageFile = Nothing, foreground As StorageFile = Nothing
            Dim portrait = True
            If files.Count > 0 Then
                Dim ttfile = files(RND.Next(files.Count))
                background = ttfile.Item1 : foreground = ttfile.Item1
                portrait = ttfile.Item2.Width < ttfile.Item2.Height
            End If
            If foreground Is Nothing OrElse portrait Then
                Dim stockForegrounds = Await stockForegroundsTask.Log("stockForegroundsTask")
                foreground = stockForegrounds(RND.Next(stockForegrounds.Count))
            End If
            If background Is Nothing OrElse Not portrait Then
                Dim stockBackgrounds = Await stockBackgroundsTask.Log("stockBackgroundsTask")
                background = stockBackgrounds(RND.Next(stockBackgrounds.Count))
            End If
            Dat = Await GameLogic.InitializeGameDataAsync(background, foreground).Log("GameLogic.InitializeGameDataAsync", background.Path & " " & foreground.Path)
        Loop Until Dat IsNot Nothing
        isStartingLevel = False
        RaiseEvent DatChanged()
    End Function

    Sub Tick(tinterval As TimeSpan)
        If Dat Is Nothing Then Return
        If isStartingLevel Then Return
        Dim wantBrickBeep = False, wantPaddleBeep = False, wantGuidedBeep = False
        Dim bricksDestroyed = 0UL, ballsLost = 0UL
        '
        GameLogic.UpdateBalls(Dat, tinterval, wantPaddleBeep, wantBrickBeep, wantGuidedBeep, ballsLost, bricksDestroyed)
        '
        If wantBrickBeep Then wavePlayer.StartPlay(BrickBeep)
        If wantPaddleBeep Then wavePlayer.StartPlay(PaddleBeep)
        If wantGuidedBeep Then wavePlayer.StartPlay(GuidedBeep)
        HighScores.BallsLost += ballsLost
        HighScores.BricksDestroyed += bricksDestroyed

        Dim wantNewLevel = False
        If Dat.BrickCount = 0 Then
            wavePlayer.StartPlay(WinBeep) : wantNewLevel = True : HighScores.GamesWon += 1UL
        ElseIf Dat.Balls.Count = 0 Then
            wavePlayer.StartPlay(LoseBeep) : wantNewLevel = True : HighScores.GamesLost += 1UL
        End If
        If wantNewLevel Then NewLevelAsync().FireAndForget()
    End Sub

    Function ReportErrorAsync(location As String, ex As Exception) As Task
        Dim msg = ex.Message & vbCrLf & vbCrLf & "Stack: " & vbCrLf & ex.StackTraceEx & vbCrLf & "OriginalStack: " & vbCrLf & ex.StackTrace.ToString()
        Return ReportErrorAsync(location, msg)
    End Function

    Async Function ReportErrorAsync(location As String, msg As String) As Task
        Stop
        Dim md As New Windows.UI.Popups.MessageDialog("Oops. There's been an internal error.", "Bug report")
        Dim r As Boolean? = Nothing
        md.Commands.Add(New Windows.UI.Popups.UICommand("Send bug report", Sub() r = True))
        md.Commands.Add(New Windows.UI.Popups.UICommand("Cancel", Sub() r = False))
        Await md.ShowAsync().Log("MessageDialog.ShowAsync")
        If Not r.HasValue OrElse Not r.Value Then Return
        '
        Dim emailTo = "lu@wischik.com"
        Dim emailSubject = "My Pictures Breakout problem report"
        Dim emailBody = "I encountered a problem with My Pictures Breakout..." & vbCrLf & vbCrLf & If(location, "") & " error:" & vbCrLf & msg
        Dim url = "mailto:?to=" & emailTo & "&subject=" & emailSubject & "&body=" & Uri.EscapeDataString(emailBody)
        Await Windows.System.Launcher.LaunchUriAsync(New Uri(url))
    End Function

End Class

Module Helpers
    <Extension>
    Async Sub FireAndForget(t As Task, Optional loc As String = Nothing)
        Try
            Await t
        Catch ex As Exception
#Disable Warning BC42358
            App.Current.ReportErrorAsync(loc, ex)
#Enable Warning BC42358
        End Try
    End Sub

    <Extension>
    Sub FireAndForget(t As IAsyncAction, Optional loc As String = Nothing)
        FireAndForget(t.AsTask, loc)
    End Sub

End Module
