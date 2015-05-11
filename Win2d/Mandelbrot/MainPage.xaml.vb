Imports System.Numerics
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Effects
Imports Microsoft.Graphics.Canvas.UI.Xaml
Imports Windows.Graphics.DirectX
Imports Windows.UI

' In this experiment I attempted to calculate the Mandelbrot set on the GPU
' using R32G32B32A32Float surfaces (i.e. single precision floats).
' The experiment failed because the combing effects (CompositeEffect, ArithmeticCompositeEffect)
' don't seem to treat negatives right: for instance, -3.0 + 0.5 yields -3.5 rather than -2.5
'
' The rules of mandelbrot set for each pixel (x0,y0) are:
' let x=0,y=0. Repeatedly apply the formula x' := x*x - y*y + x0 and y' := 2*x*y + y0
' until x*x + y*y > 4, or until we've done enough iterations.
' Color the pixel according to the number of iterations.
'
' My plan was to initialize several surfaces:
'   surfaceScaleX: every pixel (x0,y0) has value RGB(x0,x0,x0) 
'   surfaceScaleY: every pixel (x0,y0) has value RGB(y0,y0,y0)
'   surfaceX, surfaceY: every pixel is initialized to black
' Then on each iteration apply this formula:
'   surfaceX' = surfaceX1*surfaceX1 - surfaceY1*surfaceY1 + surfaceScaleX
'   surfaceY' = 2*surfaceX1*surfaceY1 + surfaceScaleY
' The multiplies can be done with ArithmeticCompositeEffect, the additions by CompositeEffect.
' Or at least I thought they could, but the additions don't seem to work.

Public NotInheritable Class MainPage
    Inherits Page

    Const CHEIGHT = 400
    Const CWIDTH = 400

    WithEvents canvas1 As CanvasControl
    Dim unitX, unitY, scaleX, scaleY, surfaceX1, surfaceX2, surfaceY1, surfaceY2 As CanvasRenderTarget

    Sub New()
        InitializeComponent()
        canvas1 = New CanvasControl With {.Width = 300, .Height = 300}
        container1.Children.Add(canvas1)
    End Sub


    Sub Canvas_CreateResources(sender As CanvasControl, args As Object) Handles canvas1.CreateResources
        Const defaultDpi = 96.0F
        unitX = New CanvasRenderTarget(canvas1, CWIDTH, CHEIGHT, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        unitY = New CanvasRenderTarget(canvas1, CWIDTH, CHEIGHT, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        scaleX = New CanvasRenderTarget(canvas1, CWIDTH, CHEIGHT, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        scaleY = New CanvasRenderTarget(canvas1, CWIDTH, CHEIGHT, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        surfaceX1 = New CanvasRenderTarget(canvas1, CWIDTH, CHEIGHT, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        surfaceX2 = New CanvasRenderTarget(canvas1, CWIDTH, CHEIGHT, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        surfaceY1 = New CanvasRenderTarget(canvas1, CWIDTH, CHEIGHT, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        surfaceY2 = New CanvasRenderTarget(canvas1, CWIDTH, CHEIGHT, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)

        Dim greyx = New Byte(16 * CWIDTH - 1) {}
        Dim blackx = New Byte(16 * CWIDTH - 1) {}
        For x = 0 To CWIDTH - 1
            Dim f = CSng(x / (CWIDTH - 1))
            EncodeR32G32B32A32Float(f, f, f, 1.0, greyx, x * 16)
            EncodeR32G32B32A32Float(0, 0, 0, 1.0, blackx, x * 16)
        Next
        For y = 0 To CHEIGHT - 1
            unitX.SetPixelBytes(greyx, 0, y, CWIDTH, 1)
            surfaceX1.SetPixelBytes(blackx, 0, y, CWIDTH, 1)
        Next

        Dim colsy = New Byte(16 * CHEIGHT - 1) {}
        Dim blacky = New Byte(16 * CHEIGHT - 1) {}
        For y = 0 To CHEIGHT - 1
            Dim f = CSng(y / (CHEIGHT - 1))
            EncodeR32G32B32A32Float(f, f, f, 1.0, colsy, y * 16)
            EncodeR32G32B32A32Float(0, 0, 0, 1.0, blacky, y * 16)
        Next
        For x = 0 To CWIDTH - 1
            unitY.SetPixelBytes(colsy, x, 0, 1, CHEIGHT)
            surfaceY1.SetPixelBytes(blacky, x, 0, 1, CHEIGHT)
        Next

        Dim xmin = -2.0F, xmax = -0.5F, ymin = -0.4F, ymax = 0.4F
        Dim xscale = xmax - xmin, yscale = ymax - ymin
        Dim ex As New LinearTransferEffect With {.Source = unitX, .AlphaDisable = True, .RedOffset = xmin, .RedSlope = xscale, .GreenOffset = xmin, .GreenSlope = xscale, .BlueOffset = xmin, .BlueSlope = xscale}
        Dim ey As New LinearTransferEffect With {.Source = unitY, .AlphaDisable = True, .RedOffset = ymin, .RedSlope = yscale, .GreenOffset = ymin, .GreenSlope = yscale, .BlueOffset = ymin, .BlueSlope = yscale}
        Using dsx = scaleX.CreateDrawingSession(), dsy = scaleY.CreateDrawingSession()
            dsx.Blend = CanvasBlend.Copy
            dsy.Blend = CanvasBlend.Copy
            dsx.DrawImage(ex)
            dsy.DrawImage(ey)
        End Using


    End Sub

    Sub DoStep() Handles button1.Click
        Dim probeX = 20, probeY = 220
        Dim probeXval = scaleX.GetGreyscale(probeX, probeY), probeYval = scaleY.GetGreyscale(probeX, probeY)
        Dim probeX0 = surfaceX1.GetGreyscale(probeX, probeY), probeY0 = surfaceY1.GetGreyscale(probeX, probeY)

        Dim e1 As New ArithmeticCompositeEffect With {.Source1 = surfaceX1, .Source2 = surfaceX1}
        Dim e2 As New ArithmeticCompositeEffect With {.Source1 = surfaceY1, .Source2 = surfaceY1, .MultiplyAmount = -1}
        Dim e3 As New CompositeEffect With {.Mode = CanvasComposite.Add}
        e3.Sources.Add(e1) : e3.Sources.Add(e2) : e3.Sources.Add(scaleX)
        Dim probeX1 = probeX0 * probeX0 - probeY0 * probeY0 + probeXval

        Dim e4 As New ArithmeticCompositeEffect With {.Source1 = surfaceX1, .Source2 = surfaceY1, .MultiplyAmount = 2}
        Dim e5 As New CompositeEffect With {.Mode = CanvasComposite.Add}
        e5.Sources.Add(e4) : e5.Sources.Add(scaleY)
        Dim probeY1 = probeX0 * probeY0 * 2 + probeYval

        Using dx = surfaceX2.CreateDrawingSession(), dy = surfaceY2.CreateDrawingSession()
            dx.Blend = CanvasBlend.Copy
            dy.Blend = CanvasBlend.Copy
            dx.DrawImage(e3)
            dy.DrawImage(e5)
        End Using
        Dim probeX2 = surfaceX2.GetGreyscale(probeX, probeY), probeY2 = surfaceY2.GetGreyscale(probeX, probeY)

        Swap(surfaceX1, surfaceX2)
        Swap(surfaceY1, surfaceY2)
        canvas1.Invalidate()
    End Sub


    Sub Canvas1_Draw(sender As CanvasControl, args As CanvasDrawEventArgs) Handles canvas1.Draw
        Dim sourceSizeDips As New Vector2(canvas1.ConvertPixelsToDips(CWIDTH), canvas1.ConvertPixelsToDips(CHEIGHT))
        Dim canvasSizeDips As New Vector2(CSng(canvas1.ActualWidth), CSng(canvas1.ActualHeight))
        Dim scale = canvasSizeDips / sourceSizeDips

        Dim effect0x As New ArithmeticCompositeEffect With {.Source1 = surfaceX1, .Source2 = surfaceX1}
        Dim effect0y As New ArithmeticCompositeEffect With {.Source1 = surfaceY1, .Source2 = surfaceY1}
        Dim effect0 As New CompositeEffect With {.Mode = CanvasComposite.Add}
        effect0.Sources.Add(effect0x)
        effect0.Sources.Add(effect0y)
        Dim effect1 As New DpiCompensationEffect With {.Source = effect0, .SourceDpi = New Vector2(canvas1.Dpi)}
        Dim effect2 As New Transform2DEffect With {.Source = effect1, .TransformMatrix = Matrix3x2.CreateScale(scale), .InterpolationMode = CanvasImageInterpolation.NearestNeighbor}
        args.DrawingSession.DrawImage(effect2)
    End Sub


    Sub Canvas_Pointer(sender As Object, e As PointerRoutedEventArgs) Handles canvas1.PointerPressed
        Dim sourceSizeDips As New Vector2(canvas1.ConvertPixelsToDips(CWIDTH), canvas1.ConvertPixelsToDips(CHEIGHT))
        Dim canvasSizeDips As New Vector2(CSng(canvas1.ActualWidth), CSng(canvas1.ActualHeight))
        Dim scale = canvasSizeDips.X / sourceSizeDips.X
        Dim canvasPointDips = e.GetCurrentPoint(canvas1).Position.ToVector2() / canvas1.ConvertPixelsToDips(1)
        Dim sourcePointDips = canvasPointDips / scale
        Dim x = CInt(Math.Floor(sourcePointDips.X))
        Dim y = CInt(Math.Floor(sourcePointDips.Y))
        If e.Pointer.IsInContact AndAlso x >= 0 AndAlso y >= 0 AndAlso x < CWIDTH AndAlso y < CHEIGHT Then
            canvas1.Invalidate()
        End If
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


Public Module Utils
    Public Sub Swap(Of T)(ByRef x As T, ByRef y As T)
        Dim temp = x : x = y : y = temp
    End Sub

    <Extension>
    Function ToColorF(buf As Byte()) As ColorF()
        Dim c = New ColorF(buf.Length \ 16) {}
        For i = 0 To buf.Length - 1 Step 16
            c(i).R = BitConverter.ToSingle(buf, i * 16 + 0)
            c(i).G = BitConverter.ToSingle(buf, i * 16 + 4)
            c(i).B = BitConverter.ToSingle(buf, i * 16 + 8)
            c(i).A = BitConverter.ToSingle(buf, i * 16 + 12)
        Next
        Return c
    End Function

    Sub EncodeR32G32B32A32Float(R As Single, G As Single, B As Single, A As Single, buf As Byte(), offset As Integer)
        Dim fr = BitConverter.GetBytes(R), fg = BitConverter.GetBytes(G), fb = BitConverter.GetBytes(B), fa = BitConverter.GetBytes(A)
        Array.Copy(fr, 0, buf, offset + 0, 4)
        Array.Copy(fg, 0, buf, offset + 4, 4)
        Array.Copy(fb, 0, buf, offset + 8, 4)
        Array.Copy(fa, 0, buf, offset + 12, 4)
    End Sub

    'Sub EncodeR16G16B16A16Float(R As Single, G As Single, B As Single, A As Single, buf As Byte(), offset As Integer)
    '    Dim hr = ToHalfFloat(R), hg = ToHalfFloat(G), hb = ToHalfFloat(B), ha = ToHalfFloat(A)
    '    Array.Copy(hr, 0, buf, offset + 0, 2)
    '    Array.Copy(hg, 0, buf, offset + 2, 2)
    '    Array.Copy(hb, 0, buf, offset + 4, 2)
    '    Array.Copy(ha, 0, buf, offset + 6, 2)
    'End Sub

    'Function ToHalfFloat(value As Single) As Byte()
    '    Dim fbits = BitConverter.ToUInt32(BitConverter.GetBytes(CSng(value)), 0)
    '    Dim val = (fbits And &H7FFFFFFFUI) + &H1000
    '    If value < 0 Then
    '        Throw New ArgumentOutOfRangeException(NameOf(value), "negatives not implemented")
    '    ElseIf val >= &H47800000 Then
    '        Throw New ArgumentOutOfRangeException(NameOf(value), "NaN/Inf/overflow not implemented")
    '    ElseIf val >= &H38800000 Then
    '        Return BitConverter.GetBytes(CUShort((val - &H38000000) >> 13))
    '    ElseIf val < &H33000000 Then
    '        Return {0, 0}
    '    Else
    '        Throw New ArgumentOutOfRangeException(NameOf(value), "subnormals not implemented")
    '    End If
    'End Function

    <Extension>
    Function GetGreyscale(c As CanvasRenderTarget, x As Integer, y As Integer) As Single
        If c.Format <> DirectXPixelFormat.R32G32B32A32Float Then Throw New ArgumentException("wrong pixel format")
        Dim buf = c.GetPixelBytes(x, y, 1, 1)
        Dim R = BitConverter.ToSingle(buf, 0)
        Dim G = BitConverter.ToSingle(buf, 4)
        Dim B = BitConverter.ToSingle(buf, 8)
        Dim A = BitConverter.ToSingle(buf, 12)
        Return R
    End Function
End Module
