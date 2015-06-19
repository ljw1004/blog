Imports System.Numerics
Imports System.Threading
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Effects
Imports Microsoft.Graphics.Canvas.UI.Xaml
Imports Windows.UI



Public NotInheritable Class MainPageCPU
    Inherits Page

    Const SIMULATION_SIZE = 16

    ' We use double-buffering, drawing onto Buffer1 on one frame, then Buffer2 the next, and so on.
    ' For rendering, we copy the pixels onto a Win2d surface that can be scaled and drawn onscreen.
    Dim Buffer1 As Color() = New Color(SIMULATION_SIZE * SIMULATION_SIZE - 1) {}
    Dim Buffer2 As Color() = New Color(SIMULATION_SIZE * SIMULATION_SIZE - 1) {}
    Dim Surface As CanvasRenderTarget
    Dim DisplayEffect1 As DpiCompensationEffect
    Dim DisplayEffect2 As Transform2DEffect

    ' We'll run an animation loop for the duration that this page is open, and cancel it once done
    Dim Cancel As CancellationTokenSource


    Protected Overrides Async Sub OnNavigatedTo(e As NavigationEventArgs)
        Dim random As New Random()
        For i = 0 To Buffer1.Length - 1
            Buffer1(i) = If(random.NextDouble() < 0.5, Colors.White, Colors.Black)
            Buffer2(i) = Colors.Black
        Next

        Cancel = New CancellationTokenSource
        While Not Cancel.IsCancellationRequested
            ' Update rate of 12hz (not counting the time it takes to do the simulation)
            Await Task.Delay(TimeSpan.FromSeconds(1 / 12))

            ' Update logic is CPU-intensive, so we do it on a background thread.
            ' (We con't call Surface.SetPixelColors until the surface has been initialized.)
            Await Task.Run(AddressOf Update)
            If Surface IsNot Nothing Then Surface.SetPixelColors(Buffer1) : canvas1.Invalidate()
        End While

    End Sub

    Protected Overrides Sub OnNavigatedFrom(e As NavigationEventArgs)
        Cancel.Cancel()
    End Sub

    Private Sub Page_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Me.SizeChanged
        Dim newsizeDips = Math.Min(ActualWidth, ActualHeight) * 0.95
        canvas1.Width = newsizeDips
        canvas1.Height = newsizeDips
    End Sub


    Sub Canvas_CreateResources(sender As CanvasControl, args As Object) Handles canvas1.CreateResources
        Const defaultDpi = 96.0F
        Surface = New CanvasRenderTarget(canvas1, SIMULATION_SIZE, SIMULATION_SIZE, defaultDpi)

        ' For display purposes, Win2d normally does DPI-aware scaling. But we're going
        ' to suppress that by falsely claiming that our source bitmaps have the same
        ' DPI as the display. Then we'll do our own scaling with NearestNeighbor interpolation.
        ' (The scaling matrix is computed dynamically in the Draw method, to cope with resizing.)
        DisplayEffect1 = New DpiCompensationEffect With {.Source = Surface, .SourceDpi = New Vector2(canvas1.Dpi)}
        DisplayEffect2 = New Transform2DEffect With {.Source = DisplayEffect1, .InterpolationMode = CanvasImageInterpolation.NearestNeighbor}
    End Sub


    Sub Update()
        '  - for each cell, count how many of its 8 neighbors are alive
        '  - if less than two, the cell dies from loneliness
        '  - if exactly two, the cell keeps its current state
        '  - if exactly three, the cell become alive
        '  - if more than three, the cell dies from overcrowding

        Static Dim neighbourOffsets As Integer() = {-SIMULATION_SIZE - 1, -SIMULATION_SIZE, -SIMULATION_SIZE + 1, -1, 1, SIMULATION_SIZE - 1, SIMULATION_SIZE, SIMULATION_SIZE + 1}

        For y = 0 To SIMULATION_SIZE - 1
            For x = 0 To SIMULATION_SIZE - 1

                Dim i = y * SIMULATION_SIZE + x
                If y = 0 OrElse y = SIMULATION_SIZE - 1 OrElse x = 0 OrElse x = SIMULATION_SIZE - 1 Then Buffer2(i) = Colors.White : Continue For
                Dim liveNeighborCount = 0
                For Each ni In neighbourOffsets
                    liveNeighborCount += If(Buffer1(i + ni).R = 0, 1, 0)
                Next

                Dim nextColor As Color
                Select Case liveNeighborCount
                    Case < 2 : nextColor = Colors.White
                    Case = 2 : nextColor = Buffer1(i)
                    Case = 3 : nextColor = Colors.Black
                    Case > 3 : nextColor = Colors.White
                End Select
                Buffer2(i) = nextColor
            Next
        Next

        ' Swap the two buffers
        Dim t = Buffer1 : Buffer1 = Buffer2 : Buffer2 = t
    End Sub




    Sub Canvas_Draw(sender As CanvasControl, args As CanvasDrawEventArgs) Handles canvas1.Draw
        Dim scale = canvas1.Width / canvas1.ConvertPixelsToDips(SIMULATION_SIZE)
        DisplayEffect2.TransformMatrix = Matrix3x2.CreateScale(CSng(scale))
        args.DrawingSession.DrawImage(DisplayEffect2)

        If SIMULATION_SIZE > 20 Then Return
        Dim w = CSng(canvas1.ActualWidth), h = CSng(canvas1.ActualHeight)
        Dim rule = Color.FromArgb(255, 202, 235, 253)
        For i = 0 To SIMULATION_SIZE
            Dim f = CSng(i / SIMULATION_SIZE)
            args.DrawingSession.DrawLine(0, f * h, w, f * h, rule)
            args.DrawingSession.DrawLine(f * w, 0, f * w, h, rule)
        Next
    End Sub



    Sub ProcessPointerInput(sender As Object, e As PointerRoutedEventArgs) Handles canvas1.PointerPressed, canvas1.PointerMoved
        If Not e.Pointer.IsInContact Then Return

        Dim fractionalPosition = e.GetCurrentPoint(canvas1).Position.ToVector2 / canvas1.Size.ToVector2
        Dim x = CInt(fractionalPosition.X * SIMULATION_SIZE)
        Dim y = CInt(fractionalPosition.Y * SIMULATION_SIZE)
        If x < 0 OrElse y < 0 OrElse x >= SIMULATION_SIZE OrElse y >= SIMULATION_SIZE Then Return

        Buffer1(y * SIMULATION_SIZE + x) = Colors.Black
    End Sub


    Sub Button1_Click() Handles button1.Click
        Frame.BackStack.Clear()
        Frame.Navigate(GetType(MainPageGPU))
    End Sub


End Class
