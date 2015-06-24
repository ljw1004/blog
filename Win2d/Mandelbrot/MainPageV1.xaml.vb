Imports System.Numerics
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Effects
Imports Microsoft.Graphics.Canvas.UI.Xaml
Imports Windows.Graphics.DirectX
Imports Windows.UI
Imports Windows.UI.Core


' In this toy program I calculate the Mandelbrot Set on the GPU using win2d effects.
' Use left+right click to zoom in and out.
'
' The basic algorithm Of the Mandelbrot Set is like this:
' 
'   ||   for each pixel(px, py) on the screen, do
'   ||       x := 0, y:=0
'   ||       repeatedly do this
'   ||           x := x*x - y*y + px,   y:=2 * x * y + py
'   ||       until x*x+y*y > 4, or until 50 iterations
'   ||
'   ||   color the pixel (px,py) according to the number of iterations done
'
'
' Conventionally, if you had a 500x500 pixel screen, you'd do the outer loop
' 500x500 = 250 thousand times, and the inner loop 12.5 million times.
' That's a lot of loops!
'
'
' Instead let's do it on the graphics card. Imagine we ask
' the graphics card to do the inner loop simultaneously on every single
' pixel on the screen at once -- that way the entire Mandelbrot Set will
' be finished in just 50 iterations! ...
'
'   ||   start with a 500x500 matrix, where each element at location (x, y) is
'   ||   itself a pair of numbers (a,b) initialized to (0,0).
'   ||
'   ||   repeat this loop 50 times:
'   ||       simultaneously for every pixel (px, py) in the matrix,
'   ||       (a,b)' := ( a*a - b*b + x,  2*a*b + y )
'   ||
'   ||   at the end, color every pixel according to whether x*x+y*y > 4.
'
'
' Under the hood, the graphics card can't quite do every pixel simultaneously.
' A modern graphics card has about 2000 cores, so it will split the 500x500
' matrix into 2000 tiles of 120 pixels Each. So really the work takes
' 50 * 120 = 6000 iterations total. Still, that's plenty fast enough!
'
'
' Ideally we'd code that fast algorithm directly using GPGPU, like these
' Mandelbrot implementations
'    CUDA - http://docs.nvidia.com/cuda/cuda-samples/index.html#mandelbrot
'    OpenCL - https://forum.beyond3d.com/threads/opencl-mandelbrot-generator.47593/
'    Matlab - http://www.mathworks.com/help/distcomp/examples/illustrating-three-approaches-to-gpu-computing-the-mandelbrot-set.html
'    C++AMP - http://blogs.microsoft.co.il/pavely/2014/03/23/mandelbrot-set-with-c-amp/
' I estimate that would give a 20-fold performance increase over what we have here.
' But C++AMP And DirectCompute and the like aren't supported on most Phone devices,
' and they aren't exposed in Win2d, so we'll make do with what we have:
'
' ArithmeticCompositeEffect - given two bitmaps A and B, gives a third bitmap x*A + y*B + z*A*B
' CompositeEffect - given a number of bitmaps P,Q,R, gives a fourth bitmap P+Q+R
'


Public NotInheritable Class MainPageV1
    Inherits Page

    Dim CSIZE As Integer = 500 ' size in pixels of our mandlebrot calculation
    Dim CITER As Integer = 50  ' how many iterations to do.

    Dim MTopLeft As New Vector2(-2, -2) ' top-left corner of Mandlebrot
    Dim MSize As New Vector2(4, 4)      ' size of mandlebrot

    Dim IsUpToDate As Boolean ' If MSize/MTopLeft change, then this gets reset, to indicate a recalc is needed
    Dim UnitX, UnitY, X, Y, A, A_prime, B, B_prime, DrawBuffer As CanvasRenderTarget
    Dim e_RangeX, e_RangeY, e_quarter_d As LinearTransferEffect
    Dim e_A_prime, e_B_prime As CompositeEffect
    Dim e_Black As ColorSourceEffect
    Dim e_A_squared, e_minus_B_squared, e_two_A_B As ArithmeticCompositeEffect
    Dim e_Draw As Transform2DEffect

    WithEvents NavigationManager As SystemNavigationManager = SystemNavigationManager.GetForCurrentView ' for back button


    Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
        NavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible
    End Sub

    Sub Canvas_CreateResources(sender As CanvasControl, args As Object) Handles canvas1.CreateResources
        Const defaultDpi = 96.0F
        IsUpToDate = False

        UnitX = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        UnitY = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        X = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        Y = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        A = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        A_prime = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        B = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        B_prime = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        DrawBuffer = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.B8G8R8A8UIntNormalized, CanvasAlphaMode.Ignore)

        ' Initialize the "unitX and unitY" surfaces...
        Dim range = New Byte(16 * CSIZE - 1) {}
        Dim buf1 = BitConverter.GetBytes(CSng(1))
        For i = 0 To CSIZE - 1
            Dim buf = BitConverter.GetBytes(CSng(i / (CSIZE - 1)))
            Array.Copy(buf, 0, range, i * 16 + 0, 4)
            Array.Copy(buf, 0, range, i * 16 + 4, 4)
            Array.Copy(buf, 0, range, i * 16 + 8, 4)
            Array.Copy(buf1, 0, range, i * 16 + 12, 4)
        Next
        For i = 0 To CSIZE - 1
            UnitX.SetPixelBytes(range, 0, i, CSIZE, 1)
            UnitY.SetPixelBytes(range, i, 0, 1, CSIZE)
        Next


        ' Effects to set up X and Y for the current zoom (these are populated in the Update method)
        e_RangeX = New LinearTransferEffect With {.Source = UnitX, .AlphaDisable = True}
        e_RangeY = New LinearTransferEffect With {.Source = UnitY, .AlphaDisable = True}

        ' Effects to initialize the iteration and the accumulator
        e_Black = New ColorSourceEffect With {.Color = Colors.Black}

        ' Effects to calculate A' = A*A - B*B + X
        e_A_squared = New ArithmeticCompositeEffect With {.Source1 = A, .Source2 = A}
        e_minus_B_squared = New ArithmeticCompositeEffect With {.Source1 = B, .Source2 = B, .MultiplyAmount = -1}
        e_A_prime = New CompositeEffect With {.Mode = CanvasComposite.Add}
        e_A_prime.Sources.Add(e_A_squared) : e_A_prime.Sources.Add(e_minus_B_squared) : e_A_prime.Sources.Add(X)

        ' Effects to calculate B' = 2*A*B + Y
        e_two_A_B = New ArithmeticCompositeEffect With {.Source1 = A, .Source2 = B, .MultiplyAmount = 2}
        e_B_prime = New CompositeEffect With {.Mode = CanvasComposite.Add}
        e_B_prime.Sources.Add(e_two_A_B) : e_B_prime.Sources.Add(Y)


        ' Effect to calculate D = A*A + B*B...
        '
        ' COMPLICATION: We want to clip "D" so that all values >= 4 count just as "diverged".
        ' But Win2d only offers clamping to the range 0..1, and it clamps NaN into "0" rather than "1".
        ' So instead we do "clamp(1 - 0.25*X*X - 0.25*Y*Y)", re-using the X*X and -Y*Y intermediates from earlier.
        ' This will give 0 for all diverged pixels, and 0..1 for all not-yet-diverged pixels.
        Dim e_one_minus_quarter_d As New ArithmeticCompositeEffect With {.Source1 = e_A_squared, .Source2 = e_minus_B_squared, .MultiplyAmount = 0, .Source1Amount = -0.25, .Source2Amount = 0.25, .Offset = 1, .ClampOutput = True}
        '
        ' COMPLICATION: The result of all those ArithmeticCompositeEffects has turned the alpha channel
        ' into something useless. This isn't a problem for the ArithmeticCompositeEffects used above since
        ' they treat it independently, but it is a problem for "D" the way we're going to use it.
        ' So we'll use a ColorMatrixEffect to reset the alpha channel to 1.0 everywhere:
        Dim e_one_minus_quarter_d_fixed_alpha As New ColorMatrixEffect With {.Source = e_one_minus_quarter_d, .AlphaMode = CanvasAlphaMode.Straight, .ColorMatrix = New Matrix5x4 With {.M11 = 1, .M22 = 1, .M33 = 1, .M44 = 0, .M54 = 1}}
        '
        e_quarter_d = New LinearTransferEffect With {.Source = e_one_minus_quarter_d_fixed_alpha, .RedOffset = 1, .RedSlope = -1, .GreenOffset = 1, .GreenSlope = -1, .BlueOffset = 1, .BlueSlope = -1}



        ' Effects for how drawing to the screen is done. (The actual transform matrix for the Transform2DEffect
        ' is calculated and supplied in the Draw method)
        Dim draw1 As New DpiCompensationEffect With {.Source = DrawBuffer, .SourceDpi = New Vector2(canvas1.Dpi)}
        e_Draw = New Transform2DEffect With {.Source = draw1}
    End Sub


    Sub Update()
        Dim sw = Stopwatch.StartNew

        ' Set up X and Y for the current zoom
        With e_RangeX
            .RedOffset = MTopLeft.X : .RedSlope = MSize.X
            .GreenOffset = MTopLeft.X : .GreenSlope = MSize.X
            .BlueOffset = MTopLeft.X : .BlueSlope = MSize.X
        End With
        With e_RangeY
            .RedOffset = MTopLeft.Y : .RedSlope = MSize.Y
            .GreenOffset = MTopLeft.Y : .GreenSlope = MSize.Y
            .BlueOffset = MTopLeft.Y : .BlueSlope = MSize.Y
        End With
        Using dsx = X.CreateDrawingSession(), dsy = Y.CreateDrawingSession()
            dsx.DrawImage(e_RangeX)
            dsy.DrawImage(e_RangeY)
        End Using

        ' Initialize the iteration and the accumulator
        Using dsa = A.CreateDrawingSession(), dsb = B.CreateDrawingSession()
            dsa.DrawImage(e_Black)
            dsb.DrawImage(e_Black)
        End Using


        ' Do the iteration
        For iter = 1 To CITER

            Using daprime = A_prime.CreateDrawingSession(), dbprime = B_prime.CreateDrawingSession()
                daprime.Blend = CanvasBlend.Copy : daprime.DrawImage(e_A_prime)
                dbprime.Blend = CanvasBlend.Copy : dbprime.DrawImage(e_B_prime)
            End Using
            ' COMPLICATION: The CanvasBlend mode is "SourceOver", which interacts badly with the alpha
            ' values in A_prime and B_prime. So instead we use "Copy".


            ' Swap "a" and "a_prime" around, and likewise "b" and "b_prime"
            Swap(A, A_prime)
            Swap(B, B_prime)
            ' And rewire all the effects that depend on "a" and "b"
            e_A_squared.Source1 = A
            e_A_squared.Source2 = A
            e_minus_B_squared.Source1 = B
            e_minus_B_squared.Source2 = B
            e_two_A_B.Source1 = A
            e_two_A_B.Source2 = B
        Next


        ' DrawBuffer is what the screen will use whenever it needs to repaint itself
        Using ds = DrawBuffer.CreateDrawingSession()
            ds.DrawImage(e_quarter_d)
        End Using

        label1.Text = $"{sw.Elapsed.TotalMilliseconds:0}ms, {MTopLeft}+{MSize}"
    End Sub


    Sub Canvas_Draw(sender As CanvasControl, args As CanvasDrawEventArgs) Handles canvas1.Draw
        If Not IsUpToDate Then Update() : IsUpToDate = True

        Dim sourceSizeDips = New Vector2(canvas1.ConvertPixelsToDips(CSIZE))
        Dim canvasSizeDips = canvas1.Size.ToVector2
        e_Draw.TransformMatrix = Matrix3x2.CreateScale(canvasSizeDips / sourceSizeDips)
        args.DrawingSession.DrawImage(e_Draw)
    End Sub

    Sub Zoom(Zoom As Double, fCenter As Vector2)
        Dim MCenter = MTopLeft + MSize * fCenter
        MSize /= CSng(Zoom)
        MTopLeft = MCenter - fCenter * MSize
        IsUpToDate = False : canvas1.Invalidate()
    End Sub

    Sub Page_PointerPressed(sender As Object, e As PointerRoutedEventArgs) Handles Me.PointerPressed
        Dim center = e.GetCurrentPoint(canvas1).Position.ToVector2 / canvas1.Size.ToVector2
        Dim zoom_factor = If(e.GetCurrentPoint(canvas1).Properties.IsRightButtonPressed, 0.5, 2)
        Zoom(zoom_factor, center)
    End Sub

    Sub BackRequested(sender As Object, e As BackRequestedEventArgs) Handles NavigationManager.BackRequested
        If MSize.Length > 4 Then Return ' leaving the event unhandled: will exit the app
        e.Handled = True
        Zoom(0.5, New Vector2(0.5))
    End Sub


    Shared Sub Swap(Of T)(ByRef x As T, ByRef y As T)
        Dim temp = x
        x = y
        y = temp
    End Sub

End Class

