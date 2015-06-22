Imports System.Numerics
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Effects
Imports Microsoft.Graphics.Canvas.UI.Xaml
Imports Windows.Graphics.DirectX
Imports Windows.UI
Imports Windows.UI.Core


' In this toy program I calculate the Mandelbrot Set on the GPU using win2d effects.
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
'
' Incidentally, phones typically don't support "Double" floating point precision numbers (64bits)
' They don't even support "Single" precision (32bits)
' Instead they have "Half" precision floating points (16bits).
' And also they don't support bitmaps with only a single value per pixel.
' They only work with four values (R,G,B,A) per pixel. So we're basically having to do
' four times as much work as we'd like to do.


Public NotInheritable Class MainPage
    Inherits Page

    Dim CSIZE As Integer = 300 ' size in pixels of our mandlebrot calculation
    Dim CITER As Integer = 50  ' how many iterations to do.

    Dim MTopLeft As New Vector2(-2, -2) ' top-left corner of Mandlebrot
    Dim MSize As New Vector2(4, 4)      ' size of mandlebrot

    Dim IsUpToDate As Boolean ' If MSize/MTopLeft change, then this gets reset, to indicate a recalc is needed
    Dim UnitX, UnitY, X, Y, A, A_prime, B, B_prime, Accumulator, DrawBuffer As CanvasRenderTarget
    Dim DrawEffect As Transform2DEffect

    WithEvents NavigationManager As SystemNavigationManager = SystemNavigationManager.GetForCurrentView ' for back button


    Sub Canvas_CreateResources(sender As CanvasControl, args As Object) Handles canvas1.CreateResources
        Const defaultDpi = 96.0F
        IsUpToDate = False

        UnitX = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        UnitY = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        X = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        Y = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        A = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        A_prime = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        B = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        B_prime = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        Accumulator = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.B8G8R8A8UIntNormalized, CanvasAlphaMode.Ignore)
        DrawBuffer = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.B8G8R8A8UIntNormalized, CanvasAlphaMode.Ignore)

        ' Initialize the "unitX and unitY" surfaces...
        '
        ' COMPLICATION: the only floating-point pixel format supported on low-end devices like Phone is
        ' "half-precision floating point" R16G16B16A16Float.
        ' This half-precision floating-point isn't supported by .NET, and isn't supported by Win2d,
        ' so we roll our own routine "GetHalfFloatBytes" which turns a double-precision floating point
        ' number into the correct bit pattern for half-precision, and we write raw bytes to the surface.
        Dim range = New Byte(8 * CSIZE - 1) {}
        Dim buf1 = GetHalfFloatBytes(1.0F)
        For i = 0 To CSIZE - 1
            Dim f = CSng(i / (CSIZE - 1))
            Dim buf = GetHalfFloatBytes(f)
            Array.Copy(buf, 0, range, i * 8 + 0, 2)
            Array.Copy(buf, 0, range, i * 8 + 2, 2)
            Array.Copy(buf, 0, range, i * 8 + 4, 2)
            Array.Copy(buf1, 0, range, i * 8 + 6, 2)
        Next
        For i = 0 To CSIZE - 1
            UnitX.SetPixelBytes(range, 0, i, CSIZE, 1)
            UnitY.SetPixelBytes(range, i, 0, 1, CSIZE)
        Next

        ' This is how drawing gets done, in the Canvas_Draw event
        Dim draw1 As New DpiCompensationEffect With {.Source = DrawBuffer, .SourceDpi = New Vector2(canvas1.Dpi)}
        DrawEffect = New Transform2DEffect With {.Source = draw1, .InterpolationMode = CanvasImageInterpolation.NearestNeighbor}
    End Sub

    Shared Function GetHalfFloatBytes(value As Single) As Byte()
        If value < 0 Then Throw New ArgumentOutOfRangeException(NameOf(value), "negatives not implemented")
        Dim fbits = BitConverter.ToUInt32(BitConverter.GetBytes(CSng(value)), 0)
        Dim val = (fbits And &H7FFFFFFFUI) + &H1000
        If val >= &H47800000 Then Throw New ArgumentOutOfRangeException(NameOf(value), "NaN/Inf/overflow not implemented")
        If val >= &H38800000 Then Return BitConverter.GetBytes(CUShort((val - &H38000000) >> 13))
        If val < &H33000000 Then Return {0, 0}
        Throw New ArgumentOutOfRangeException(NameOf(value), "subnormals not implemented")
    End Function


    Sub Update()
        Dim sw = Stopwatch.StartNew

        ' Set up X and Y for the current zoom
        Using dsx = Me.X.CreateDrawingSession(), dsy = Me.Y.CreateDrawingSession()
            dsx.DrawImage(New LinearTransferEffect With {.Source = UnitX, .AlphaDisable = True, .RedOffset = MTopLeft.X, .RedSlope = MSize.X, .GreenOffset = MTopLeft.X, .GreenSlope = MSize.X, .BlueOffset = MTopLeft.X, .BlueSlope = MSize.X})
            dsy.DrawImage(New LinearTransferEffect With {.Source = UnitY, .AlphaDisable = True, .RedOffset = MTopLeft.Y, .RedSlope = MSize.Y, .GreenOffset = MTopLeft.Y, .GreenSlope = MSize.Y, .BlueOffset = MTopLeft.Y, .BlueSlope = MSize.Y})
        End Using

        ' Initialize the iteration and the accumulator
        Dim black As New ColorSourceEffect With {.Color = Colors.Black}
        Using dsa = A.CreateDrawingSession(), dsb = B.CreateDrawingSession(), dsacc = Accumulator.CreateDrawingSession()
            dsa.DrawImage(black)
            dsb.DrawImage(black)
            dsacc.DrawImage(black)
        End Using


        ' A' = A*A - B*B + X
        Dim A_squared As New ArithmeticCompositeEffect With {.Source1 = A, .Source2 = A}
        Dim minus_B_squared As New ArithmeticCompositeEffect With {.Source1 = B, .Source2 = B, .MultiplyAmount = -1}
        Dim A_prime As New CompositeEffect With {.Mode = CanvasComposite.Add}
        A_prime.Sources.Add(A_squared) : A_prime.Sources.Add(minus_B_squared) : A_prime.Sources.Add(X)

        ' B' = 2*A*B + Y
        Dim two_A_B As New ArithmeticCompositeEffect With {.Source1 = A, .Source2 = B, .MultiplyAmount = 2}
        Dim B_prime As New CompositeEffect With {.Mode = CanvasComposite.Add}
        B_prime.Sources.Add(two_A_B) : B_prime.Sources.Add(Y)

        ' D = A*A + B*B...
        '
        ' COMPLICATION: We want to clip "D" so that all values >= 4 count just as "diverged".
        ' But Win2d only offers clamping to the range 0..1, and it clamps NaN into "0" rather than "1".
        ' So instead we do "clamp(1 - 0.25*X*X - 0.25*Y*Y)", re-using the X*X and -Y*Y intermediates from earlier.
        ' This will give 0 for all diverged pixels, and 0..1 for all not-yet-diverged pixels.
        Dim one_minus_quarter_d As New ArithmeticCompositeEffect With {.Source1 = A_squared, .Source2 = minus_B_squared, .MultiplyAmount = 0, .Source1Amount = -0.25, .Source2Amount = 0.25, .Offset = 1, .ClampOutput = True}
        '
        ' COMPLICATION: The result of all those ArithmeticCompositeEffects has turned the alpha channel
        ' into something useless. This isn't a problem for the ArithmeticCompositeEffects used above since
        ' they treat it independently, but it is a problem for "D" the way we're going to use it.
        ' So we'll use a ColorMatrixEffect to reset the alpha channel to 1.0 everywhere:
        Dim one_minus_quarter_d_fixed_alpha As New ColorMatrixEffect With {.Source = one_minus_quarter_d, .AlphaMode = CanvasAlphaMode.Straight, .ColorMatrix = New Matrix5x4 With {.M11 = 1, .M22 = 1, .M33 = 1, .M44 = 0, .M54 = 1}}
        '
        ' COMPLICATION: for display purposes, we want to calculate the number of iterations it takes
        ' before "D" diverges. The way we'll do this is to steadily accumulate the following:
        ' if the above "clamp(1 - ...)" value is 0, i.e. if this pixel is either currently diverged
        ' or has diverged in the past, then add 1/50th greyscale to our accumulator. This way
        ' over 50 iterations we'll accumulate solid black for the things never diverge, and we'll color
        ' dark grey the things that eventually diverge after 30+ iterations, and we'll color light grey
        ' or white those things that diverge immediately.
        ' We can't quite do "if value is equal to 0", but we can use a DiscreteTransferFunction to do
        ' the close approximation "if value is <= 1/20"...
        Dim table As Single() = {CSng(1 / CITER), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
        Dim is_d_diverged As New DiscreteTransferEffect With {.Source = one_minus_quarter_d_fixed_alpha, .RedTable = table, .GreenTable = table, .BlueTable = table, .AlphaTable = {1}}


        For iter = 1 To CITER

            Using daprime = Me.A_prime.CreateDrawingSession(), dbprime = Me.B_prime.CreateDrawingSession(), dacc = Accumulator.CreateDrawingSession
                daprime.Blend = CanvasBlend.Copy : daprime.DrawImage(A_prime)
                dbprime.Blend = CanvasBlend.Copy : dbprime.DrawImage(B_prime)
                dacc.Blend = CanvasBlend.Add : dacc.DrawImage(is_d_diverged)
            End Using
            ' COMPLICATION: The CanvasBlend mode is "SourceOver", which interacts badly with the alpha
            ' values in A_prime and B_prime. So instead we use "Copy".


            ' Swap "a" and "a_prime" around, and likewise "b" and "b_prime"
            Dim ta = A : A = Me.A_prime : Me.A_prime = ta
            Dim tb = B : B = Me.B_prime : Me.B_prime = tb
            A_squared.Source1 = A
            A_squared.Source2 = A
            minus_B_squared.Source1 = B
            minus_B_squared.Source2 = B
            two_A_B.Source1 = A
            two_A_B.Source2 = B
        Next

        ' DrawBuffer is what the screen will use whenever it needs to repaint itself
        Using ds = DrawBuffer.CreateDrawingSession()
            ds.DrawImage(Accumulator)
        End Using

        label1.Text = $"{sw.Elapsed.TotalMilliseconds:0}ms, {MTopLeft}+{MSize}"
    End Sub


    Sub Canvas_Draw(sender As CanvasControl, args As CanvasDrawEventArgs) Handles canvas1.Draw
        If Not IsUpToDate Then Update() : IsUpToDate = True

        Dim sourceSizeDips = New Vector2(canvas1.ConvertPixelsToDips(CSIZE))
        Dim canvasSizeDips = canvas1.Size.ToVector2
        DrawEffect.TransformMatrix = Matrix3x2.CreateScale(canvasSizeDips / sourceSizeDips)
        args.DrawingSession.DrawImage(DrawEffect)
    End Sub

    Sub Page_PointerPressed(sender As Object, e As PointerRoutedEventArgs) Handles Me.PointerPressed
        Dim frac = e.GetCurrentPoint(canvas1).Position.ToVector2 / canvas1.Size.ToVector2
        Dim MCenter = MTopLeft + frac * MSize
        MSize *= If(e.GetCurrentPoint(canvas1).Properties.IsRightButtonPressed, 2, 0.5F)
        MTopLeft = MCenter - MSize / 2
        IsUpToDate = False : canvas1.Invalidate()
        ShowBackButton()
    End Sub

    Sub BackRequested(sender As Object, e As BackRequestedEventArgs) Handles NavigationManager.BackRequested
        If MSize.Length > 4 Then Return
        Dim MCenter = MTopLeft + MSize / 2
        MSize *= 2
        MTopLeft = MCenter - MSize / 2
        e.Handled = True
        IsUpToDate = False : canvas1.Invalidate()
        ShowBackButton()
    End Sub

    Sub ShowBackButton()
        NavigationManager.AppViewBackButtonVisibility = If(MSize.Length > 4, AppViewBackButtonVisibility.Collapsed, AppViewBackButtonVisibility.Visible)
    End Sub

End Class

