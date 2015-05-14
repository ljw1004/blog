Imports System.Numerics
Imports System.Threading
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Effects
Imports Microsoft.Graphics.Canvas.UI.Xaml
Imports Windows.Graphics.DirectX
Imports Windows.Graphics.Effects
Imports Windows.UI

' In this toy program I calculate the Mandelbrot set on the GPU using D2D effects
' and single-precision floats
'
' The rules of mandelbrot set for each pixel (x0,y0) are:
' let x=0,y=0. Repeatedly apply the formula x' := x*x - y*y + x0 and y' := 2*x*y + y0
' until x*x + y*y > 4, or until we've done enough iterations.
'
' TODO: move the RecalculateAsync out of a standalone method, and into event-driven Draw
' capped at 60fps. That will let it update progressively and not have any bad D2D effects.
'
' TODO: figure out how to reconcile greyscale of coarse (=50 iters) and fine (=100 or more).
' It'd be great if we had histogram, but that's absent from win2d.
' Actually, maybe the "coarse rendering" would more appropriately be a zoomed-in version
' of the previous fine, rather than an entire coarse calculation.
'
' TODO: pinch-and-drag.
'

Public NotInheritable Class MainPage
    Inherits Page

    Const CSIZE = 200, CITER = 50
    Const FSIZE = 640, FITER = 100

    Dim MTop As Single = -1
    Dim MLeft As Single = -2.5
    Dim MWidth As Single = 3.5
    Dim MHeight As Single = 2

    WithEvents canvas1 As CanvasControl
    Dim unitXcoarse, unitYcoarse, rangeXcoarse, rangeYcoarse, iterX1coarse, iterX2coarse, iterY1coarse, iterY2coarse, render1coarse, render2coarse As CanvasRenderTarget
    Dim unitXfine, unitYfine, rangeXfine, rangeYfine, iterX1fine, iterX2fine, iterY1fine, iterY2fine, render1fine, render2fine As CanvasRenderTarget
    Dim drawEffect As EffectWrapper(Of CanvasRenderTarget, Vector2)
    Dim cancelFine As CancellationTokenSource
    Dim calculateFineTask As Task

    Sub New()
        InitializeComponent()
        canvas1 = New CanvasControl
        container1.Children.Add(canvas1)
    End Sub

    Sub Canvas_CreateResources(sender As CanvasControl, args As Object) Handles canvas1.CreateResources
        Const defaultDpi = 96.0F
        unitXcoarse = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        unitYcoarse = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        rangeXcoarse = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        rangeYcoarse = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        iterX1coarse = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        iterX2coarse = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        iterY1coarse = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        iterY2coarse = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        render1coarse = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.B8G8R8A8UIntNormalized, CanvasAlphaMode.Ignore)
        render2coarse = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.B8G8R8A8UIntNormalized, CanvasAlphaMode.Ignore)
        '
        unitXfine = New CanvasRenderTarget(canvas1, FSIZE, FSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        unitYfine = New CanvasRenderTarget(canvas1, FSIZE, FSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        rangeXfine = New CanvasRenderTarget(canvas1, FSIZE, FSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        rangeYfine = New CanvasRenderTarget(canvas1, FSIZE, FSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        iterX1fine = New CanvasRenderTarget(canvas1, FSIZE, FSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        iterX2fine = New CanvasRenderTarget(canvas1, FSIZE, FSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        iterY1fine = New CanvasRenderTarget(canvas1, FSIZE, FSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        iterY2fine = New CanvasRenderTarget(canvas1, FSIZE, FSIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        render1fine = New CanvasRenderTarget(canvas1, FSIZE, FSIZE, defaultDpi, DirectXPixelFormat.B8G8R8A8UIntNormalized, CanvasAlphaMode.Ignore)
        render2fine = New CanvasRenderTarget(canvas1, FSIZE, FSIZE, defaultDpi, DirectXPixelFormat.B8G8R8A8UIntNormalized, CanvasAlphaMode.Ignore)

        ' Initialize the "unitX and unitY" surfaces.
        ' I could have done it more elegantly with the "ColorF" structure, but this code avoids most allocations...
        Dim rangecoarse = New Byte(16 * CSIZE - 1) {}
        Dim rangefine = New Byte(16 * FSIZE - 1) {}
        Dim buf1 = BitConverter.GetBytes(1.0F)
        For i = 0 To CSIZE - 1
            Dim f = CSng(i / (CSIZE - 1))
            Dim buf = BitConverter.GetBytes(f)
            Array.Copy(buf, 0, rangecoarse, i * 16 + 0, 4)
            Array.Copy(buf, 0, rangecoarse, i * 16 + 4, 4)
            Array.Copy(buf, 0, rangecoarse, i * 16 + 8, 4)
            Array.Copy(buf1, 0, rangecoarse, i * 16 + 12, 4)
        Next
        For i = 0 To FSIZE - 1
            Dim f = CSng(i / (FSIZE - 1))
            Dim buf = BitConverter.GetBytes(f)
            Array.Copy(buf, 0, rangefine, i * 16 + 0, 4)
            Array.Copy(buf, 0, rangefine, i * 16 + 4, 4)
            Array.Copy(buf, 0, rangefine, i * 16 + 8, 4)
            Array.Copy(buf1, 0, rangefine, i * 16 + 12, 4)
        Next
        For i = 0 To CSIZE - 1
            unitXcoarse.SetPixelBytes(rangecoarse, 0, i, CSIZE, 1)
            unitYcoarse.SetPixelBytes(rangecoarse, i, 0, 1, CSIZE)
        Next
        For i = 0 To FSIZE - 1
            unitXfine.SetPixelBytes(rangefine, 0, i, FSIZE, 1)
            unitYfine.SetPixelBytes(rangefine, i, 0, 1, FSIZE)
        Next

        ' This is how drawing gets done, in the Canvas_Draw event
        Dim draw1 As New DpiCompensationEffect With {.SourceDpi = New Vector2(canvas1.Dpi)}
        Dim draw2 As New Transform2DEffect With {.Source = draw1, .InterpolationMode = CanvasImageInterpolation.NearestNeighbor}
        drawEffect = New EffectWrapper(Of CanvasRenderTarget, Vector2)(draw2, Sub(src, scale)
                                                                                  draw1.Source = src
                                                                                  draw2.TransformMatrix = Matrix3x2.CreateScale(scale)
                                                                              End Sub)

        RestartCalculate()
    End Sub

    Sub RestartCalculate()
        'CalculateInner(MLeft, MTop, MWidth, MHeight, True, Nothing)
        'Using ds = render1coarse.CreateDrawingSession()
        '                            ds.Blend = CanvasBlend.Copy
        '                            ds.DrawImage(render2coarse)
        '                        End Using
        '                        canvas1.Invalidate()
        CalculateInner(MLeft, MTop, MWidth, MHeight, False, Nothing)
        Using ds = render1fine.CreateDrawingSession()
            ds.Blend = CanvasBlend.Copy
            ds.DrawImage(render2fine)
        End Using
        canvas1.Invalidate()
    End Sub

    Sub CalculateInner(left As Single, top As Single, width As Single, height As Single, isFast As Boolean, cancel As CancellationToken)
        Dim sw = Stopwatch.StartNew()

        Dim unitX, unitY, rangeX, rangeY, iterX1, iterX2, iterY1, iterY2, render1, render2 As CanvasRenderTarget
        Dim niter As Integer
        If isFast Then
            unitX = unitXcoarse : unitY = unitYcoarse : rangeX = rangeXcoarse : rangeY = rangeYcoarse
            iterX1 = iterX1coarse : iterX2 = iterX2coarse : iterY1 = iterY1coarse : iterY2 = iterY2coarse
            render1 = render1coarse : render2 = render2coarse
            niter = CITER
        Else
            unitX = unitXfine : unitY = unitYfine : rangeX = rangeXfine : rangeY = rangeYfine
            iterX1 = iterX1fine : iterX2 = iterX2fine : iterY1 = iterY1fine : iterY2 = iterY2fine
            render1 = render1fine : render2 = render2fine
            niter = FITER
        End If

        ' Set up rangeX and rangeY
        Dim ex As New LinearTransferEffect With {.Source = unitX, .AlphaDisable = True, .RedOffset = left, .RedSlope = width, .GreenOffset = left, .GreenSlope = width, .BlueOffset = left, .BlueSlope = width}
        Dim ey As New LinearTransferEffect With {.Source = unitY, .AlphaDisable = True, .RedOffset = top, .RedSlope = height, .GreenOffset = top, .GreenSlope = height, .BlueOffset = top, .BlueSlope = height}
        Using dsx = rangeX.CreateDrawingSession(), dsy = rangeY.CreateDrawingSession()
            dsx.Blend = CanvasBlend.Copy
            dsy.Blend = CanvasBlend.Copy
            dsx.DrawImage(ex)
            dsy.DrawImage(ey)
        End Using

        ' Initialize the iteration and the accumulator
        Dim eblack As New ColorSourceEffect With {.Color = Colors.Black}
        Using dsx = iterX1.CreateDrawingSession(), dsy = iterY1.CreateDrawingSession(), dsa = render2.CreateDrawingSession()
            dsx.Blend = CanvasBlend.Copy
            dsy.Blend = CanvasBlend.Copy
            dsa.Blend = CanvasBlend.Copy
            dsx.DrawImage(eblack)
            dsy.DrawImage(eblack)
            dsa.DrawImage(eblack)
        End Using

        Dim oldx = iterX1, oldy = iterY1

        ' newx = x*x - y*y + x0
        Dim _x2 As New ArithmeticCompositeEffect With {.Source1 = oldx, .Source2 = oldx}
        Dim _y2neg As New ArithmeticCompositeEffect With {.Source1 = oldy, .Source2 = oldy, .MultiplyAmount = -1}
        Dim newx As New CompositeEffect With {.Mode = CanvasComposite.Add}
        newx.Sources.Add(_x2) : newx.Sources.Add(_y2neg) : newx.Sources.Add(rangeX)

        ' newy = 2xy + y0
        Dim _2xy As New ArithmeticCompositeEffect With {.Source1 = oldx, .Source2 = oldy, .MultiplyAmount = 2}
        Dim newy As New CompositeEffect With {.Mode = CanvasComposite.Add}
        newy.Sources.Add(_2xy) : newy.Sources.Add(rangeY)

        ' For display... the thing is that we wish to treat "NaN" and "x^2+y^2>4" as equivalent,
        ' since both diverge. Therefore we have to clamp. But clamping always turns NaN into 0.
        ' So we'll negate it, "clamp(1 - 0.25*x*x*y*y)", which will turn NaN into 0, and
        ' large x^2+y^2>4 into 0, and small xy into the range 0..1
        ' We need to correct alpha prior (via color-matix) prior to the final step,
        ' a discrete transfer effect to turn 0 into "1", and everything else into "0"
        Dim dinvert As New ArithmeticCompositeEffect With {.Source1 = _x2, .Source2 = _y2neg, .MultiplyAmount = 0, .Source1Amount = -0.25, .Source2Amount = 0.25, .Offset = 1, .ClampOutput = True}
        Dim dm As New Matrix5x4 With {.M11 = 1, .M22 = 1, .M33 = 1, .M44 = 0, .M54 = 1}
        Dim dalpha As New ColorMatrixEffect With {.Source = dinvert, .AlphaMode = CanvasAlphaMode.Straight, .ColorMatrix = dm}
        Dim dt As Single() = {CSng(1 / niter), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
        Dim d1and0 As New DiscreteTransferEffect With {.Source = dalpha, .RedTable = dt, .GreenTable = dt, .BlueTable = dt, .AlphaTable = {1}}

        For iter = 0 To niter - 1
            cancel.ThrowIfCancellationRequested()

            Using dx = iterX2.CreateDrawingSession(), dy = iterY2.CreateDrawingSession(), da = render2.CreateDrawingSession
                dx.Blend = CanvasBlend.Copy : dy.Blend = CanvasBlend.Copy : da.Blend = CanvasBlend.Add
                dx.DrawImage(newx)
                dy.DrawImage(newy)
                da.DrawImage(d1and0)
            End Using

            Swap(iterX1, iterX2)
            Swap(iterY1, iterY2)
            _x2.Source1 = iterX1
            _x2.Source2 = iterX1
            _y2neg.Source1 = iterY1
            _y2neg.Source2 = iterY1
            _2xy.Source1 = iterX1
            _2xy.Source2 = iterY1
            dt(0) *= 1
            d1and0.RedTable = dt
            d1and0.GreenTable = dt
            d1and0.BlueTable = dt
        Next

        label1.Text = $"{sw.Elapsed.TotalMilliseconds:0}ms, ({MLeft},{MTop})-({MLeft + MWidth},{MTop + MHeight})"
    End Sub

    Sub Page_SizeChanged() Handles Me.SizeChanged
        canvas1.Width = container1.ActualWidth
        canvas1.Height = container1.ActualHeight
    End Sub

    Sub Canvas1_Draw(sender As CanvasControl, args As CanvasDrawEventArgs) Handles canvas1.Draw
        'If calculateFineTask Is Nothing OrElse Not calculateFineTask.IsCompleted Then
        '    Dim sourceSizeDips As New Vector2(canvas1.ConvertPixelsToDips(CSIZE), canvas1.ConvertPixelsToDips(CSIZE))
        '    Dim canvasSizeDips As New Vector2(CSng(canvas1.ActualWidth), CSng(canvas1.ActualHeight))
        '    Dim scale = canvasSizeDips / sourceSizeDips
        '    args.DrawingSession.DrawImage(drawEffect.Update(render1coarse, scale))
        'Else
        Dim sourceSizeDips As New Vector2(canvas1.ConvertPixelsToDips(FSIZE), canvas1.ConvertPixelsToDips(FSIZE))
        Dim canvasSizeDips As New Vector2(CSng(canvas1.ActualWidth), CSng(canvas1.ActualHeight))
        Dim scale = canvasSizeDips / sourceSizeDips
        args.DrawingSession.DrawImage(drawEffect.Update(render1fine, scale))
        'End If
    End Sub


    Sub Canvas_Pointer(sender As Object, e As PointerRoutedEventArgs) Handles canvas1.PointerMoved
        Dim sourceSizeDips As New Vector2(canvas1.ConvertPixelsToDips(CSIZE), canvas1.ConvertPixelsToDips(CSIZE))
        Dim canvasSizeDips As New Vector2(CSng(canvas1.ActualWidth), CSng(canvas1.ActualHeight))
        Dim scale = canvasSizeDips / sourceSizeDips
        Dim canvasPointDips = e.GetCurrentPoint(canvas1).Position.ToVector2() / canvas1.ConvertPixelsToDips(1)
        Dim sourcePointDips = canvasPointDips / scale
        Dim x = CInt(Math.Floor(sourcePointDips.X))
        Dim y = CInt(Math.Floor(sourcePointDips.Y))
        If x < 0 OrElse y < 0 OrElse x >= CSIZE OrElse y >= CSIZE Then Return
        'Dim px = rangeX.GetPixelColorFs(x, y, 1, 1).First
        'Dim py = rangeY.GetPixelColorFs(x, y, 1, 1).First
        'Dim cx = iterX1.GetPixelColorFs(x, y, 1, 1).First
        'Dim cy = iterY1.GetPixelColorFs(x, y, 1, 1).First
        'Dim c = render1coarse.GetPixelColors(x, y, 1, 1).First
        'label1.Text = $"({px.R:0.0},{py.R:0.0}) -> R={c.R:0.0} A={c.A:0.0} ({elapsed.TotalMilliseconds:0}ms)"
        'label1.Text = $"R={c.R:0.0} A={c.A:0.0} ({elapsed.TotalMilliseconds:0}ms)"
    End Sub

    Sub Canvas_PointerPressed(sender As Object, e As PointerRoutedEventArgs) Handles canvas1.PointerPressed
        Dim pt = e.GetCurrentPoint(canvas1).Position
        Dim fx = pt.X / canvas1.ActualWidth
        Dim fy = pt.Y / canvas1.ActualHeight
        Dim cx = MLeft + fx * MWidth
        Dim cy = MTop + fy * MHeight
        Dim scale = If(e.GetCurrentPoint(canvas1).Properties.IsRightButtonPressed, 2, 0.5F)
        MWidth = MWidth * scale
        MHeight = MHeight * scale
        MLeft = CSng(cx - MWidth / 2)
        MTop = CSng(cy - MHeight / 2)
        RestartCalculate()
    End Sub


End Class


Public Structure ColorF
    Public R As Single
    Public G As Single
    Public B As Single
    Public A As Single
    Shared Function FromRGBA(R As Single, G As Single, B As Single, A As Single) As ColorF
        Return New ColorF With {.R = R, .G = G, .B = B, .A = A}
    End Function
    Function GetBytes() As Byte()
        Dim buf = New Byte(15) {}
        Array.Copy(BitConverter.GetBytes(R), 0, buf, 0, 4)
        Array.Copy(BitConverter.GetBytes(G), 0, buf, 4, 4)
        Array.Copy(BitConverter.GetBytes(B), 0, buf, 8, 4)
        Array.Copy(BitConverter.GetBytes(A), 0, buf, 12, 4)
        Return buf
    End Function
    Public Overrides Function ToString() As String
        Return $"R={R:0.0} G={G:0.0} B={B:0.0} A={A:0.0}"
    End Function
End Structure


Public Structure ColorB
    Public R As Byte
    Public G As Byte
    Public B As Byte
    Public A As Byte
    Shared Function FromRGBA(R As Byte, G As Byte, B As Byte, A As Byte) As ColorB
        Return New ColorB With {.R = R, .G = G, .B = B, .A = A}
    End Function
    Function GetBytes() As Byte()
        Return {B, G, R, A}
    End Function
    Public Overrides Function ToString() As String
        Return $"R={R:x2} G={G:x2} B={B:x2} A={A:x2}"
    End Function
End Structure

Public Structure ColorH
    Public R As Single
    Public G As Single
    Public B As Single
    Public A As Single
    Shared Function FromRGBA(R As Single, G As Single, B As Single, A As Single) As ColorF
        Return New ColorF With {.R = R, .G = G, .B = B, .A = A}
    End Function
    Function GetBytes() As Byte()
        Dim buf = New Byte(7) {}
        Array.Copy(ToHalfFloat(R), 0, buf, 0, 2)
        Array.Copy(ToHalfFloat(G), 0, buf, 2, 2)
        Array.Copy(ToHalfFloat(B), 0, buf, 4, 2)
        Array.Copy(ToHalfFloat(A), 0, buf, 6, 2)
        Return buf
    End Function
    Public Overrides Function ToString() As String
        Return $"R={R:0.0} G={G:0.0} B={B:0.0} A={A:0.0}"
    End Function

    Public Shared Function ToHalfFloat(value As Single) As Byte()
        If value < 0 Then Throw New ArgumentOutOfRangeException(NameOf(value), "negatives not implemented")
        Dim fbits = BitConverter.ToUInt32(BitConverter.GetBytes(CSng(value)), 0)
        Dim val = (fbits And &H7FFFFFFFUI) + &H1000
        If val >= &H47800000 Then Throw New ArgumentOutOfRangeException(NameOf(value), "NaN/Inf/overflow not implemented")
        If val >= &H38800000 Then Return BitConverter.GetBytes(CUShort((val - &H38000000) >> 13))
        If val < &H33000000 Then Return {0, 0}
        Throw New ArgumentOutOfRangeException(NameOf(value), "subnormals not implemented")
    End Function

End Structure


Public Module Utils
    Public Sub Swap(Of T)(ByRef x As T, ByRef y As T)
        Dim temp = x : x = y : y = temp
    End Sub

    <Extension>
    Function GetPixelColorFs(bmp As CanvasBitmap) As ColorF()
        Dim buf = bmp.GetPixelBytes()
        Dim c = New ColorF(buf.Length \ 16) {}
        For i = 0 To buf.Length - 1 Step 16
            c(i).R = BitConverter.ToSingle(buf, i * 16 + 0)
            c(i).G = BitConverter.ToSingle(buf, i * 16 + 4)
            c(i).B = BitConverter.ToSingle(buf, i * 16 + 8)
            c(i).A = BitConverter.ToSingle(buf, i * 16 + 12)
        Next
        Return c
    End Function

    <Extension>
    Function GetPixelColorFs(bmp As CanvasBitmap, left As Integer, top As Integer, width As Integer, height As Integer) As ColorF()
        Dim buf = bmp.GetPixelBytes(left, top, width, height)
        Dim c = New ColorF(buf.Length \ 16) {}
        For i = 0 To buf.Length - 1 Step 16
            c(i).R = BitConverter.ToSingle(buf, i * 16 + 0)
            c(i).G = BitConverter.ToSingle(buf, i * 16 + 4)
            c(i).B = BitConverter.ToSingle(buf, i * 16 + 8)
            c(i).A = BitConverter.ToSingle(buf, i * 16 + 12)
        Next
        Return c
    End Function

    <Extension>
    Sub SetPixelColorFs(bmp As CanvasBitmap, colors As IEnumerable(Of ColorF))
        Dim buf = From c In colors From b In c.GetBytes Select b
        bmp.SetPixelBytes(buf.ToArray)
    End Sub

    <Extension>
    Sub SetPixelColorFs(bmp As CanvasBitmap, colors As IEnumerable(Of ColorF), left As Integer, top As Integer, width As Integer, height As Integer)
        Dim buf = From c In colors From b In c.GetBytes Select b
        bmp.SetPixelBytes(buf.ToArray, left, top, width, height)
    End Sub

End Module


Class EffectWrapper(Of T, U)
    Private EndEffect As ICanvasImage
    Private FixSources As Action(Of T, U)

    Sub New(endEffect As ICanvasImage, fixSources As Action(Of T, U))
        Me.EndEffect = endEffect
        Me.FixSources = fixSources
    End Sub

    Function Update(val1 As T, val2 As U) As ICanvasImage
        FixSources(val1, val2)
        Return EndEffect
    End Function
End Class
