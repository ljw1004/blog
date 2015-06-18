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
' Conventionally, if you had a 480x480 pixel screen, you'd do the outer loop
' 480x480 = 230 thousand times, and the inner loop 11 million times.
' That's a lot of loops!
'
'
' Instead let's do it on the graphics card. Imagine we ask
' the graphics card to do the inner loop simultaneously on every single
' pixel on the screen at once -- that way the entire Mandelbrot Set will
' be finished in just 50 iterations! ...
'
'   ||   start with a 480x480 matrix, where wach element at location (px, py) is
'   ||   itself a pair of numbers <x,y> initialized to <0,0>.
'   ||
'   ||   repeat this loop 50 times:
'   ||       simultaneously for every pixel (px, py) in the matrix,
'   ||       <x,y> := < x*x - y*y + px,  2*x*y + py >
'   ||
'   ||   at the end, color every pixel according to whether <x,y> > 2.
'
'
' Under the hood, the graphics card can't quite do every pixel simultaneously.
' A modern graphics card has about 2000 cores, so it will split the 480x480
' matrix into 2000 tiles of 100 pixels Each. So really the work takes
' 50 * 100 = 5000 iterations total. Still, that's plenty fast enough!
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
'
'
' Incidentally, phones typically don't support "Double" floating point precision numbers (64bits)
' They don't even support "Single" precision (32bits)
' Instead they have "Half" precision floating points (16bits).


Public NotInheritable Class MainPage
    Inherits Page

    Dim CSIZE As Integer = 300 ' size in pixels of our mandlebrot calculation
    Dim CITER As Integer = 50  ' how many iterations to do.

    Dim MTopLeft As New Vector2(-2, -2) ' top-left corner of Mandlebrot
    Dim MSize As New Vector2(4, 4)      ' size of mandlebrot

    Dim IsUpToDate As Boolean ' If MSize/MTopLeft change, then this gets reset, to indicate a recalc is needed
    Dim UnitX, UnitY, RangeX, RangeY, IterX, IterX2, IterY, IterY2, Accumulator, DrawBuffer As CanvasRenderTarget
    Dim DrawEffect As Transform2DEffect

    WithEvents NavigationManager As SystemNavigationManager = SystemNavigationManager.GetForCurrentView ' for back button


    Sub Canvas_CreateResources(sender As CanvasControl, args As Object) Handles canvas1.CreateResources
        Const defaultDpi = 96.0F

        UnitX = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        UnitY = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        RangeX = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        RangeY = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        IterX = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        IterX2 = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        IterY = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        IterY2 = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        Accumulator = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.B8G8R8A8UIntNormalized, CanvasAlphaMode.Ignore)
        DrawBuffer = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.B8G8R8A8UIntNormalized, CanvasAlphaMode.Ignore)

        ' Initialize the "unitX and unitY" surfaces.
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

        ' Set up rangeX and rangeY
        Dim ex As New LinearTransferEffect With {.Source = UnitX, .AlphaDisable = True, .RedOffset = MTopLeft.X, .RedSlope = MSize.X, .GreenOffset = MTopLeft.X, .GreenSlope = MSize.X, .BlueOffset = MTopLeft.X, .BlueSlope = MSize.X}
        Dim ey As New LinearTransferEffect With {.Source = UnitY, .AlphaDisable = True, .RedOffset = MTopLeft.Y, .RedSlope = MSize.Y, .GreenOffset = MTopLeft.Y, .GreenSlope = MSize.Y, .BlueOffset = MTopLeft.Y, .BlueSlope = MSize.Y}
        Using dsx = RangeX.CreateDrawingSession(), dsy = RangeY.CreateDrawingSession()
            dsx.DrawImage(ex)
            dsy.DrawImage(ey)
        End Using

        ' Initialize the iteration and the accumulator
        Dim eblack As New ColorSourceEffect With {.Color = Colors.Black}
        Using dsx = IterX.CreateDrawingSession(), dsy = IterY.CreateDrawingSession(), dsa = Accumulator.CreateDrawingSession()
            dsx.DrawImage(eblack)
            dsy.DrawImage(eblack)
            dsa.DrawImage(eblack)
        End Using

        Dim oldx = IterX, oldy = IterY

        ' newx = x*x - y*y + x0
        Dim xx As New ArithmeticCompositeEffect With {.Source1 = oldx, .Source2 = oldx}
        Dim yyneg As New ArithmeticCompositeEffect With {.Source1 = oldy, .Source2 = oldy, .MultiplyAmount = -1}
        Dim newx As New CompositeEffect With {.Mode = CanvasComposite.Add}
        newx.Sources.Add(xx) : newx.Sources.Add(yyneg) : newx.Sources.Add(RangeX)

        ' newy = 2xy + y0
        Dim xy2 As New ArithmeticCompositeEffect With {.Source1 = oldx, .Source2 = oldy, .MultiplyAmount = 2}
        Dim newy As New CompositeEffect With {.Mode = CanvasComposite.Add}
        newy.Sources.Add(xy2) : newy.Sources.Add(RangeY)

        ' For display... the thing is that we wish to treat "NaN" and "x^2+y^2>4" as equivalent,
        ' since both diverge. Therefore we have to clamp. But clamping always turns NaN into 0.
        ' So we'll negate it, "clamp(1 - 0.25*x*x*y*y)", which will turn NaN into 0, and
        ' large x^2+y^2>4 into 0, and small xy into the range 0..1
        ' We need to correct alpha prior (via color-matix) prior to the final step,
        ' a discrete transfer effect to turn 0 into "1", and everything else into "0"
        Dim dinvert As New ArithmeticCompositeEffect With {.Source1 = xx, .Source2 = yyneg, .MultiplyAmount = 0, .Source1Amount = -0.25, .Source2Amount = 0.25, .Offset = 1, .ClampOutput = True}
        Dim dm As New Matrix5x4 With {.M11 = 1, .M22 = 1, .M33 = 1, .M44 = 0, .M54 = 1}
        Dim dalpha As New ColorMatrixEffect With {.Source = dinvert, .AlphaMode = CanvasAlphaMode.Straight, .ColorMatrix = dm}
        Dim dt As Single() = {CSng(1 / CITER), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
        Dim d1and0 As New DiscreteTransferEffect With {.Source = dalpha, .RedTable = dt, .GreenTable = dt, .BlueTable = dt, .AlphaTable = {1}}

        For iter = 1 To CITER

            Using dx = IterX2.CreateDrawingSession(), dy = IterY2.CreateDrawingSession(), da = Accumulator.CreateDrawingSession
                dx.Blend = CanvasBlend.Copy : dy.Blend = CanvasBlend.Copy : da.Blend = CanvasBlend.Add
                dx.DrawImage(newx)
                dy.DrawImage(newy)
                da.DrawImage(d1and0)
            End Using

            ' Swap the buffers around
            Dim tx = IterX : IterX = IterX2 : IterX2 = tx
            Dim ty = IterY : IterY = IterY2 : IterY2 = ty
            xx.Source1 = IterX
            xx.Source2 = IterX
            yyneg.Source1 = IterY
            yyneg.Source2 = IterY
            xy2.Source1 = IterX
            xy2.Source2 = IterY
        Next

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
    End Sub

    Sub BackRequested(sender As Object, e As BackRequestedEventArgs) Handles NavigationManager.BackRequested
        Dim MCenter = MTopLeft + MSize / 2
        MSize *= 2
        MTopLeft = MCenter - MSize / 2
        If MSize.Length > 6 Then Return
        e.Handled = True
        IsUpToDate = False : canvas1.Invalidate()
    End Sub

End Class

