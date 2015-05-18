Imports System.Numerics
Imports System.Threading
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Effects
Imports Microsoft.Graphics.Canvas.UI.Xaml
Imports Windows.Foundation.Metadata
Imports Windows.Graphics.DirectX
Imports Windows.Graphics.Effects
Imports Windows.Phone.UI.Input
Imports Windows.UI

' In this toy program I calculate the Mandelbrot set on the GPU using D2D effects
' and single-precision floats
'
' The rules of mandelbrot set for each pixel (x0,y0) are:
' let x=0,y=0. Repeatedly apply the formula x' := x*x - y*y + x0 and y' := 2*x*y + y0
' until x*x + y*y > 4, or until we've done enough iterations.
'

Public NotInheritable Class MainPage
    Inherits Page

    Dim CSIZE As Integer = 300
    Dim CITER As Integer = 50

    Dim MTopLeft As New Vector2(-2, -2)
    Dim MSize As New Vector2(4, 4)

    WithEvents canvas1 As CanvasControl
    Dim unitX, unitY, rangeX, rangeY, iterX1, iterX2, iterY1, iterY2, render1, render2 As CanvasRenderTarget
    Dim drawEffect As Transform2DEffect

    Sub New()
        InitializeComponent()
        canvas1 = New CanvasControl
        container1.Children.Add(canvas1)
        If ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons") Then
            AddHandler HardwareButtons.BackPressed, AddressOf Canvas_BackPressed
        End If
    End Sub

    Sub Canvas_CreateResources(sender As CanvasControl, args As Object) Handles canvas1.CreateResources
        Const defaultDpi = 96.0F

        unitX = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        unitY = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        rangeX = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        rangeY = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        iterX1 = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        iterX2 = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        iterY1 = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        iterY2 = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        render1 = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.B8G8R8A8UIntNormalized, CanvasAlphaMode.Ignore)
        render2 = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.B8G8R8A8UIntNormalized, CanvasAlphaMode.Ignore)

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
            unitX.SetPixelBytes(range, 0, i, CSIZE, 1)
            unitY.SetPixelBytes(range, i, 0, 1, CSIZE)
        Next

        ' This is how drawing gets done, in the Canvas_Draw event
        Dim draw1 As New DpiCompensationEffect With {.Source = render1, .SourceDpi = New Vector2(canvas1.Dpi)}
        drawEffect = New Transform2DEffect With {.Source = draw1, .InterpolationMode = CanvasImageInterpolation.NearestNeighbor}
        Calculate()
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

    Sub Calculate()
        Dim sw = Stopwatch.StartNew

        ' Set up rangeX and rangeY
        Dim ex As New LinearTransferEffect With {.Source = unitX, .AlphaDisable = True, .RedOffset = MTopLeft.X, .RedSlope = MSize.X, .GreenOffset = MTopLeft.X, .GreenSlope = MSize.X, .BlueOffset = MTopLeft.X, .BlueSlope = MSize.X}
        Dim ey As New LinearTransferEffect With {.Source = unitY, .AlphaDisable = True, .RedOffset = MTopLeft.Y, .RedSlope = MSize.Y, .GreenOffset = MTopLeft.Y, .GreenSlope = MSize.Y, .BlueOffset = MTopLeft.Y, .BlueSlope = MSize.Y}
        Using dsx = rangeX.CreateDrawingSession(), dsy = rangeY.CreateDrawingSession()
            dsx.DrawImage(ex)
            dsy.DrawImage(ey)
        End Using

        ' Initialize the iteration and the accumulator
        Dim eblack As New ColorSourceEffect With {.Color = Colors.Black}
        Using dsx = iterX1.CreateDrawingSession(), dsy = iterY1.CreateDrawingSession(), dsa = render2.CreateDrawingSession()
            dsx.DrawImage(eblack)
            dsy.DrawImage(eblack)
            dsa.DrawImage(eblack)
        End Using

        Dim oldx = iterX1, oldy = iterY1

        ' newx = x*x - y*y + x0
        Dim xx As New ArithmeticCompositeEffect With {.Source1 = oldx, .Source2 = oldx}
        Dim yyneg As New ArithmeticCompositeEffect With {.Source1 = oldy, .Source2 = oldy, .MultiplyAmount = -1}
        Dim newx As New CompositeEffect With {.Mode = CanvasComposite.Add}
        newx.Sources.Add(xx) : newx.Sources.Add(yyneg) : newx.Sources.Add(rangeX)

        ' newy = 2xy + y0
        Dim xy2 As New ArithmeticCompositeEffect With {.Source1 = oldx, .Source2 = oldy, .MultiplyAmount = 2}
        Dim newy As New CompositeEffect With {.Mode = CanvasComposite.Add}
        newy.Sources.Add(xy2) : newy.Sources.Add(rangeY)

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

        For iter = 0 To CITER - 1

            Using dx = iterX2.CreateDrawingSession(), dy = iterY2.CreateDrawingSession(), da = render2.CreateDrawingSession
                dx.Blend = CanvasBlend.Copy : dy.Blend = CanvasBlend.Copy : da.Blend = CanvasBlend.Add
                dx.DrawImage(newx)
                dy.DrawImage(newy)
                da.DrawImage(d1and0)
            End Using

            Dim tx = iterX1 : iterX1 = iterX2 : iterX2 = tx
            Dim ty = iterY1 : iterY1 = iterY2 : iterY2 = ty
            xx.Source1 = iterX1
            xx.Source2 = iterX1
            yyneg.Source1 = iterY1
            yyneg.Source2 = iterY1
            xy2.Source1 = iterX1
            xy2.Source2 = iterY1
        Next

        Using ds = render1.CreateDrawingSession()
            ds.DrawImage(render2)
        End Using
        canvas1.Invalidate()

        label1.Text = $"{sw.Elapsed.TotalMilliseconds:0}ms, {MTopLeft}+{MSize}"

    End Sub

    Sub Page_SizeChanged() Handles Me.SizeChanged
        canvas1.Width = container1.ActualWidth
        canvas1.Height = container1.ActualHeight
    End Sub

    Sub Canvas1_Draw(sender As CanvasControl, args As CanvasDrawEventArgs) Handles canvas1.Draw
        Dim sourceSizeDips As New Vector2(canvas1.ConvertPixelsToDips(CSIZE), canvas1.ConvertPixelsToDips(CSIZE))
        Dim canvasSizeDips As New Vector2(CSng(canvas1.ActualWidth), CSng(canvas1.ActualHeight))
        drawEffect.TransformMatrix = Matrix3x2.CreateScale(canvasSizeDips / sourceSizeDips)
        args.DrawingSession.DrawImage(drawEffect)
    End Sub

    Sub Canvas_PointerPressed(sender As Object, e As PointerRoutedEventArgs) Handles canvas1.PointerPressed
        Dim frac = e.GetCurrentPoint(canvas1).Position.ToVector2 / canvas1.Size.ToVector2
        Dim MCenter = MTopLeft + frac * MSize
        MSize *= If(e.GetCurrentPoint(canvas1).Properties.IsRightButtonPressed, 2, 0.5F)
        MTopLeft = MCenter - MSize / 2
        Calculate()
    End Sub

    Sub Canvas_BackPressed(sender As Object, e As BackPressedEventArgs)
        Dim MCenter = MTopLeft + 0.5 * MSize
        MSize *= 2
        MTopLeft = MCenter - MSize / 2
        Calculate()
        If MSize.Length <= 6 Then e.Handled = True
    End Sub

End Class

