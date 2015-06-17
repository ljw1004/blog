Imports System.Numerics
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Effects
Imports Microsoft.Graphics.Canvas.UI.Xaml
Imports Windows.UI

' This code is an adaption of the original at
' https://github.com/Microsoft/Win2D/blob/master/tests/ExampleGallery/Shared/GameOfLife.xaml.cs
' which is (c) Microsoft and licensed under the Apache License



Public NotInheritable Class MainPageGPU
    Inherits Page

    Const SIMULATION_SIZE = 480

    ' The cellular-automaton calculations are done by a pair of Win2d effects
    ' We use double-buffering, drawing onto Surface1 on one frame, then Surface2 the next, and so on
    Dim Surface1, Surface2 As CanvasRenderTarget
    Dim UpdateEffect1 As ConvolveMatrixEffect
    Dim UpdateEffect2 As DiscreteTransferEffect
    Dim DisplayEffect1 As DpiCompensationEffect
    Dim DisplayEffect2 As Transform2DEffect



    Private Sub Page_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Me.SizeChanged
        Dim newsizeDips = Math.Min(ActualWidth, ActualHeight) * 0.95
        canvas1.Width = newsizeDips
        canvas1.Height = newsizeDips
    End Sub


    Sub Canvas_CreateResources(sender As CanvasControl, args As Object) Handles canvas1.CreateResources
        Const defaultDpi = 96.0F
        Surface1 = New CanvasRenderTarget(canvas1, SIMULATION_SIZE, SIMULATION_SIZE, defaultDpi)
        Surface2 = New CanvasRenderTarget(canvas1, SIMULATION_SIZE, SIMULATION_SIZE, defaultDpi)

        ' Initialize the playing field to "random"
        Dim random As New Random()
        Dim cols = New Color(SIMULATION_SIZE * SIMULATION_SIZE - 1) {}
        For i = 0 To cols.Length - 1
            cols(i) = If(random.NextDouble() < 0.5, Colors.White, Colors.Black)
        Next
        Surface1.SetPixelColors(cols)


        ' The Game of Life is a cellular automaton. Each cell can be either alive (black/0) or dead (white=1).
        ' The cell's state is updated based on its current state, and how many neighbors it has alive...
        ' We use a convolve matrix to count how many neighbors are alive, and count whether
        ' the cell itself is alive. We encode both counts into a single floating point number
        ' in the range 0..1 with the formula "(Self + 2*NeighborCount) / 17.0"
        '
        Dim kernel As Single() = {2, 2, 2,
                                  2, 1, 2,
                                  2, 2, 2}

        ' Next, we'll split that range 0..1 into 17 discrete buckets, and use a lookup table
        ' to tell us the next state of the cell based on which bucket it's in. This table
        ' embodies the rules of this particular cellular automaton.
        '
        Dim table As Single() =
            {
                1, 1,    ' 8 live neighbors -> dead cell
                1, 1,    ' 7 live neighbors -> dead cell
                1, 1,    ' 6 live neighbors -> dead cell
                1, 1,    ' 5 live neighbors -> dead cell
                1, 1,    ' 4 live neighbors -> dead cell
                0, 0,    ' 3 live neighbors -> live cell
                0, 1,    ' 2 live neighbors -> cell keeps its current state
                1, 1,    ' 1 live neighbors -> dead cell
                1, 1     ' 0 live neighbors -> dead cell
            }


        ' These two effects are how Win2d accomplishes the kernel and the buckets.
        ' For the matrix, how should we count up neighbors at the edges? We can use either
        ' BorderMode.Soft which treats them as black (alive), or Hard which uses a mirror.
        UpdateEffect1 = New ConvolveMatrixEffect With {.Source = Surface1, .KernelMatrix = kernel, .Divisor = 17, .BorderMode = EffectBorderMode.Hard}
        UpdateEffect2 = New DiscreteTransferEffect With {.Source = UpdateEffect1, .RedTable = table, .GreenTable = table, .BlueTable = table}


        ' For display purposes, Win2d normally does DPI-aware scaling. But we're going
        ' to suppress that by falsely claiming that our source bitmaps have the same
        ' DPI as the display. Then we'll do our own scaling with NearestNeighbor interpolation.
        ' (The scaling matrix is computed dynamically in the Draw method, to cope with resizing.)
        DisplayEffect1 = New DpiCompensationEffect With {.Source = Surface1, .SourceDpi = New Vector2(canvas1.Dpi)}
        DisplayEffect2 = New Transform2DEffect With {.Source = DisplayEffect1, .InterpolationMode = CanvasImageInterpolation.NearestNeighbor}
    End Sub


    Sub Update()
        Using ds = Surface2.CreateDrawingSession()
            ds.DrawImage(UpdateEffect2)
        End Using

        ' Swap the current and next surfaces.
        Dim t = Surface1 : Surface1 = Surface2 : Surface2 = t
        DisplayEffect1.Source = Surface1
        UpdateEffect1.Source = Surface1
    End Sub


    Sub Canvas_Draw(sender As CanvasControl, args As CanvasDrawEventArgs) Handles canvas1.Draw
        Static Dim count As Integer = 0
        count += 1 : If count > 5 Then Update() : count = 5 ' update once every 5 refreshes, at 12Hz

        Dim scale = canvas1.Width / canvas1.ConvertPixelsToDips(SIMULATION_SIZE)
        DisplayEffect2.TransformMatrix = Matrix3x2.CreateScale(CSng(scale))
        args.DrawingSession.DrawImage(DisplayEffect2)

        canvas1.Invalidate() ' causes the Draw event to be fired at 60Hz
    End Sub



    Sub ProcessPointerInput(sender As Object, e As PointerRoutedEventArgs) Handles canvas1.PointerPressed, canvas1.PointerMoved
        If Not e.Pointer.IsInContact Then Return

        Dim fractionalPosition = e.GetCurrentPoint(canvas1).Position.ToVector2 / canvas1.Size.ToVector2
        Dim x = CInt(fractionalPosition.X * SIMULATION_SIZE)
        Dim y = CInt(fractionalPosition.Y * SIMULATION_SIZE)
        If x < 0 OrElse y < 0 OrElse x >= SIMULATION_SIZE OrElse y >= SIMULATION_SIZE Then Return

        Surface1.SetPixelColors({Colors.Black}, x, y, 1, 1)
    End Sub


    Sub Button1_Click() Handles button1.Click
        Frame.BackStack.Clear()
        Frame.Navigate(GetType(MainPageCPU))
    End Sub


End Class
