' This code is an adaption of the original at
' https://github.com/Microsoft/Win2D/blob/master/tests/ExampleGallery/Shared/GameOfLife.xaml.cs
' which is (c) Microsoft and licensed under the Apache License

Imports System.Numerics
Imports System.Threading
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Effects
Imports Microsoft.Graphics.Canvas.UI.Xaml
Imports Windows.UI

Public NotInheritable Class MainPage
    Inherits Page

    Const simulationW = 640
    Const simulationH = 480

    Dim UseGPU As Boolean = True

    ' For traditional CPU-based calculation: it's done by swapping back and forth two arrays to do the
    ' cellular-automaton calculation, then copying them onto a Win2D surface that can be scaled
    ' and drawn on screen
    Dim cols1, cols2 As Color()
    Dim neighbourOffsets As Integer() = {-simulationW - 1, -simulationW, -simulationW + 1, -1, 1, simulationW - 1, simulationW, simulationW + 1}

    ' For GPU-acclerated calculation: it's done by swapping back and forth two Win2D surfaces
    ' and the cellular-automaton calculations are done by Win2d "image effects".
    Dim surface1, surface2 As CanvasRenderTarget
    Dim countNeighboursEffect As ConvolveMatrixEffect
    Dim liveOrDieEffect As DiscreteTransferEffect
    Dim invertEffect As LinearTransferEffect
    Dim transformEffect As Transform2DEffect

    ' Drawing onto the screen is done with a CanvasControl, which you can draw a Win2d surface onto
    WithEvents canvas1 As CanvasControl
    Dim DontUpdateUntilFrameIsDrawn As New SemaphoreSlim(0) ' so we don't run too fast
    Dim times As New LinkedList(Of TimeSpan) ' for the frame-counter

    ' Responding to mouse input:
    Dim lastPointerX, lastPointerY As Integer


    Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
        If canvas1 IsNot Nothing Then Return
        canvas1 = New CanvasControl
        grid1.Children.Insert(0, canvas1)
        StartSimulationAsync()
    End Sub

    Protected Overrides Sub OnNavigatedFrom(e As NavigationEventArgs)
        canvas1.RemoveFromVisualTree()
        canvas1 = Nothing
    End Sub

    Sub Button1_Click() Handles button1.Click
        UseGPU = Not UseGPU
        button1.Content = If(UseGPU, "GPU", "CPU")
        times.Clear()
    End Sub

    Async Sub StartSimulationAsync()
        ' TODO: remove the timer and the count, to make the code clean
        ' (after I've tested it on my Lumia 635 to get perf numbers there for CPU vs GPU)
        Dim count = 0
        Dim timer As Stopwatch = Stopwatch.StartNew()

        While canvas1 IsNot Nothing

            ' We can't do anything if the canvas resources aren't set up...
            If surface1 Is Nothing Then Await Task.Delay(60) : Continue While

            count += 1
            If UseGPU Then UpdateSimulationGPU() Else Await UpdateSimulationCPUAsync()
            canvas1.Invalidate() ' tells the canvas it needs to fire another Draw event

            times.AddLast(timer.Elapsed)
            If times.Count > 100 Then times.RemoveFirst()
            If times.Count > 2 Then
                Dim fps = times.Count / (times.Last.Value - times.First.Value).TotalSeconds
                label1.Text = $"{fps:0}fps"
            End If

            ' On a SurfacePro, 640x480 pixel buffer
            ' the Win2D update-logic can do about 3000 updates per second (0.3ms)
            ' but the manual update-logic one can only manage 15fps (70ms)
            ' Regardless, once it's been updated, it needs to be redrawn on the Win2D XAML canas.
            ' Redrawing is capped at 60fps, so I'll the update-loop to do no more than that
            If count > 0 Then count = 0 : Await DontUpdateUntilFrameIsDrawn.WaitAsync()
        End While
    End Sub

    Sub Canvas_CreateResources(sender As CanvasControl, args As Object) Handles canvas1.CreateResources
        InitializeGPUtables()
        InitializeCPUtables()
    End Sub

    Sub Canvas_Draw(sender As CanvasControl, args As CanvasDrawEventArgs) Handles canvas1.Draw
        invertEffect.Source = surface1
        transformEffect.TransformMatrix = GetDisplayTransform(canvas1.Size, canvas1, simulationW, simulationH)
        args.DrawingSession.DrawImage(transformEffect)

        DontUpdateUntilFrameIsDrawn.Release()
    End Sub


    Sub UpdateSimulationGPU()
        ' Use the current surface as input.
        countNeighboursEffect.Source = surface1

        ' Perform a simulation-update step using an effect that implements the rules of the cellular automaton.
        Using ds = surface2.CreateDrawingSession()
            ds.DrawImage(liveOrDieEffect)
        End Using

        ' Swap the current and next surfaces.
        Dim t = surface1 : surface1 = surface2 : surface2 = t
    End Sub


    Async Function UpdateSimulationCPUAsync() As Task
        ' Update logic is CPU-intensive, so we do it on a background thread:
        '  - for each cell, count how many of its 8 neighbors are alive
        '  - if less than two, the cell dies from loneliness
        '  - if exactly two, the cell keeps its current state
        '  - if exactly three, the cell become alive
        '  - if more than three, the cell dies from overcrowding
        Await Task.Run(Sub()
                           For y = 0 To simulationH - 1
                               For x = 0 To simulationW - 1
                                   Dim i = y * simulationW + x
                                   If y = 0 OrElse y = simulationH - 1 OrElse x = 0 OrElse x = simulationW - 1 Then cols2(i) = Colors.Black : Continue For
                                   Dim numNeighbours = 0
                                   For Each ni In neighbourOffsets
                                       numNeighbours += If(cols1(i + ni).R > 0, 1, 0)
                                   Next
                                   Dim col As Color
                                   Select Case numNeighbours
                                       Case < 2 : col = Colors.Black
                                       Case = 2 : col = cols1(i)
                                       Case = 3 : col = Colors.White
                                       Case > 3 : col = Colors.Black
                                   End Select
                                   cols2(i) = col
                               Next
                           Next
                       End Sub)

        Dim t = cols1 : cols1 = cols2 : cols2 = t
        surface1.SetPixelColors(cols1)
    End Function


    Sub InitializeCPUtables()
        cols1 = New Color(simulationW * simulationH - 1) {}
        cols2 = New Color(simulationW * simulationH - 1) {}

        Dim random As New Random()
        For i = 0 To cols1.Length - 1
            cols1(i) = If(random.NextDouble() < 0.25, Colors.White, Colors.Black)
            cols2(i) = Colors.Black
        Next

        neighbourOffsets = {-simulationW - 1, -simulationW, -simulationW + 1, -1, 1, simulationW - 1, simulationW, simulationW + 1}
    End Sub


    Sub InitializeGPUtables()
        Const defaultDpi = 96.0F
        surface1 = New CanvasRenderTarget(canvas1, simulationW, simulationH, defaultDpi)
        surface2 = New CanvasRenderTarget(canvas1, simulationW, simulationH, defaultDpi)

        Dim random As New Random()
        Dim cols = New Color(simulationW * simulationH - 1) {}
        For i = 0 To cols.Length - 1
            cols(i) = If(random.NextDouble() < 0.25, Colors.White, Colors.Black)
        Next
        surface1.SetPixelColors(cols)


        ' The Game of Life Is a cellular automaton with very simple rules.
        ' Each cell (pixel) can be either alive (white) Or dead (black).
        ' The state Is updated by:
        '
        '  - for each cell, count how many of its 8 neighbors are alive
        '  - if less than two, the cell dies from loneliness
        '  - if exactly two, the cell keeps its current state
        '  - if exactly three, the cell become alive
        '  - if more than three, the cell dies from overcrowding

        ' Step 1: use a convolve matrix to count how many neighbors are alive. This filter
        ' also includes the state of the current cell, but with a lower weighting. The result
        ' is an arithmetic encoding where (value / 2) indicates how many neighbors are alive,
        ' and (value % 2) Is the state of the cell itself. This is divided by 18 to make it
        ' fit within 0-1 color range.

        countNeighboursEffect = New ConvolveMatrixEffect With {
            .KernelMatrix = {2.0F, 2.0F, 2.0F,
                             2.0F, 1.0F, 2.0F,
                             2.0F, 2.0F, 2.0F},
            .Divisor = 18,
            .BorderMode = EffectBorderMode.Hard
            }

        ' Step 2: use a color transfer table to map the different states produced by the
        ' convolve matrix to whether the cell should live or die. Each pair of entries in
        ' this table corresponds to a certain number of live neighbors. The first of the
        ' pair is the result if the current cell is dead, or the second if it is alive.

        Dim transferTable =
            {
                0F, 0F,    ' 0 live neighbors -> dead cell
                0F, 0F,    ' 1 live neighbors -> dead cell
                0F, 1.0F,  ' 2 live neighbors -> cell keeps its current state
                1.0F, 1.0F,' 3 live neighbors -> live cell
                0F, 0F,    ' 4 live neighbors -> dead cell
                0F, 0F,    ' 5 live neighbors -> dead cell
                0F, 0F,    ' 6 live neighbors -> dead cell
                0F, 0F,    ' 7 live neighbors -> dead cell
                0F, 0F     ' 8 live neighbors -> dead cell
            }


        liveOrDieEffect = New DiscreteTransferEffect With {
            .Source = countNeighboursEffect,
            .RedTable = transferTable,
            .GreenTable = transferTable,
            .BlueTable = transferTable
        }

        ' Step 3: the algorithm is implemented in terms of white = live,
        ' black = dead, but we invert these colors before displaying the
        ' result, just 'cause I think it looks better that way.

        invertEffect = New LinearTransferEffect With
            {
                .RedSlope = -1,
                .RedOffset = 1,
                .GreenSlope = -1,
                .GreenOffset = 1,
                .BlueSlope = -1,
                .BlueOffset = 1
            }

        ' Step 4: insert our own DPI compensation effect to stop the system trying to
        ' automatically convert DPI for us. The Game of Life simulation always works
        ' in pixels (96 DPI) regardless of display DPI. Normally, the system would
        ' handle this mismatch automatically and scale the image up as needed to fit
        ' higher DPI displays. We don't want that behavior here, because it would use
        ' a linear filter while we want nearest neighbor. So we insert a no-op DPI
        ' converter of our own. This overrides the default adjustment by telling the
        ' system the source image is already the same DPI as the destination canvas
        ' (even though it really isn't). We'll handle any necessary scaling later
        ' ourselves, using Transform2DEffect to control the interpolation mode.

        Dim dpiCompensationEffect As New DpiCompensationEffect With
            {
                .Source = invertEffect,
                .SourceDpi = New Vector2(canvas1.Dpi)
            }

        ' Step 5: a transform matrix scales up the simulation rendertarget and moves
        ' it to the right part of the screen. This uses nearest neighbor filtering
        ' to avoid unwanted blurring of the cell shapes.

        transformEffect = New Transform2DEffect With
            {
                .Source = dpiCompensationEffect,
                .InterpolationMode = CanvasImageInterpolation.NearestNeighbor
            }
    End Sub


    ' Toggles the color of cells when they are clicked/drgged on
    Sub ProcessPointerInput(sender As Object, e As PointerRoutedEventArgs) Handles grid1.PointerPressed, grid1.PointerMoved, grid1.PointerReleased
        If Not e.Pointer.IsInContact Then lastPointerX = 0 : lastPointerY = 0 : Return

        ' Invert the display transform, to convert pointer positions into simulation rendertarget space.
        Dim transform As Matrix3x2
        Matrix3x2.Invert(GetDisplayTransform(canvas1.Size, canvas1, simulationW, simulationH), transform)

        Dim point = e.GetCurrentPoint(canvas1)
        Dim pos = Vector2.Transform(point.Position.ToVector2(), transform)
        Dim x = canvas1.ConvertDipsToPixels(pos.X)
        Dim y = canvas1.ConvertDipsToPixels(pos.Y)

        ' If the point is within the bounds of the rendertarget, and not the same as the last point...
        If x < 0 OrElse y < 0 OrElse x >= simulationW OrElse y >= simulationH Then Return
        If x = lastPointerX OrElse y = lastPointerY Then Return

        ' Read the current color.
        Dim cellColorGPU = surface1.GetPixelColors(x, y, 1, 1).First
        Dim cellColorCPU = cols1(y * simulationW + x)

        ' Toggle the value.
        cellColorGPU = If(cellColorGPU.R > 0, Colors.Black, Colors.White)
        cellColorCPU = If(cellColorCPU.R > 0, Colors.Black, Colors.White)

        ' Set the new color.
        surface1.SetPixelColors({cellColorGPU}, x, y, 1, 1)
        cols1(y * simulationW + x) = cellColorCPU

        lastPointerX = x
        lastPointerY = y
    End Sub


    Public Function GetDisplayTransform(controlSize As Size, canvas As ICanvasResourceCreatorWithDpi, designWidth As Integer, designHeight As Integer) As Matrix3x2
        Dim sourceSize As New Vector2(canvas1.ConvertPixelsToDips(designWidth), canvas1.ConvertPixelsToDips(designHeight))
        Dim outputSize = controlSize.ToVector2

        ' Scale the display to fill the control.
        Dim scale = outputSize / sourceSize
        Dim offset = Vector2.Zero

        ' Letterbox Or pillarbox to preserve aspect ratio.
        If scale.X > scale.Y Then
            scale.X = scale.Y
            offset.X = (outputSize.X - sourceSize.X * scale.X) / 2
        Else
            scale.Y = scale.X
            offset.Y = (outputSize.Y - sourceSize.Y * scale.Y) / 2
        End If

        ' TODO #4479 once .NET Native x64 codegen bug is fixed, change this back to:
        'Return Matrix3x2.CreateScale(scale) * Matrix3x2.CreateTranslation(offset)
        Return New Matrix3x2(scale.X, 0, 0, scale.Y, offset.X, offset.Y)
    End Function



End Class
