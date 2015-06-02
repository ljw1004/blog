Imports System.Numerics
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Effects
Imports Microsoft.Graphics.Canvas.UI.Xaml
Imports Windows.Graphics.DirectX
Imports Windows.UI


Public NotInheritable Class MainPageV1
    Inherits Page

    WithEvents App As App = App.Current
    WithEvents canvas1 As CanvasControl

    Dim Surface1, Surface2 As CanvasRenderTarget
    Dim StepEffectDL1, StepEffectDL2, StepEffectDR1, StepEffectDR2 As ConvolveMatrixEffect
    Dim StepEffectDL, StepEffectDR As ArithmeticCompositeEffect
    Dim DrawEffect1 As DpiCompensationEffect
    Dim DrawEffect2 As Transform2DEffect

    Dim Pen As Color = Colors.Gray

    Sub New()
        InitializeComponent()
        canvas1 = New CanvasControl
        container1.Children.Insert(0, canvas1)
    End Sub


    Sub App_Loaded() Handles App.Loaded
        If Surface1 Is Nothing Then Return
        Dim cols = App.Pixels.Select(Function(c) CType(Colors.Black, ColorF)).ToArray()
        Surface1.SetPixelColorFs(cols, 0, 0, App.CWIDTH, App.CHEIGHT)
        'Dim buf = New Byte(App.Pixels.Length * 16 - 1) {}
        'For i = 0 To App.Pixels.Length - 1
        '    Array.Copy(BitConverter.GetBytes(App.Pixels(i).R / 255.0F), 0, buf, i * 16 + 0, 4)
        '    Array.Copy(BitConverter.GetBytes(App.Pixels(i).G / 255.0F), 0, buf, i * 16 + 4, 4)
        '    Array.Copy(BitConverter.GetBytes(App.Pixels(i).B / 255.0F), 0, buf, i * 16 + 8, 4)
        '    Array.Copy(BitConverter.GetBytes(App.Pixels(i).A / 255.0F), 0, buf, i * 16 + 12, 4)
        'Next
        'Surface1.SetPixelBytes(buf)
    End Sub

    Sub App_Unloading() Handles App.Unloading
        If Surface1 Is Nothing Then Return
        Dim cols = Surface1.GetPixelColorFs(0, 0, App.CWIDTH, App.CHEIGHT)
        App.Pixels = cols.Select(Function(c) CType(c, Color)).ToArray()
        'Dim bb = Function(s As Single)
        '             s = Math.Max(0, Math.Min(1, s))
        '             Return CByte(s * 255)
        '         End Function

        'Dim buf = Surface1.GetPixelBytes()
        'For i = 0 To App.Pixels.Length - 1
        '    Dim r = BitConverter.ToSingle(buf, i * 16 + 0)
        '    Dim g = BitConverter.ToSingle(buf, i * 16 + 4)
        '    Dim b = BitConverter.ToSingle(buf, i * 16 + 8)
        '    Dim a = BitConverter.ToSingle(buf, i * 16 + 12)
        '    App.Pixels(i) = Color.FromArgb(bb(a), bb(r), bb(g), bb(b))
        'Next
    End Sub


    Sub Canvas_CreateResources(sender As CanvasControl, args As Object) Handles canvas1.CreateResources
        Const defaultDpi = 96.0F
        Surface1 = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        Surface2 = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        App_Loaded()


        Dim kernelAddDL = KernelTracker.Generate({0, 0.5, 1},
                                              {Kernel.Center, Kernel.Up, Kernel.UpRight, Kernel.Right},
                                              Function(k)
                                                  ' Brick:
                                                  If k(Kernel.Center) = 1 Then Return 1
                                                  ' Blank space that might get filled with sand:
                                                  If k(Kernel.Center) = 0 AndAlso k(Kernel.Up) = 0.5 Then Return 0.5
                                                  If k(Kernel.Center) = 0 AndAlso k(Kernel.UpRight) = 0.5 AndAlso k(Kernel.Right) <> 0 Then Return 0.5
                                                  If k(Kernel.Center) = 0 Then Return 0
                                                  Return 0
                                              End Function)

        Dim kernelRemDL = KernelTracker.Generate({0, 0.5, 1},
                                                 {Kernel.Center, Kernel.Down, Kernel.DownLeft, Kernel.Left},
                                                 Function(k)
                                                     ' Sand that might empty out below:
                                                     If k(Kernel.Center) = 0.5 AndAlso k(Kernel.Down) = 0 Then Return 0
                                                     If k(Kernel.Center) = 0.5 AndAlso k(Kernel.DownLeft) = 0 AndAlso k(Kernel.Left) <> 0.5 Then Return 0
                                                     If k(Kernel.Center) = 0.5 Then Return 0.5
                                                     Return 0
                                                 End Function)


        Dim kernelAddDR = KernelTracker.Generate({0, 0.5, 1},
                                              {Kernel.Center, Kernel.Up, Kernel.UpLeft, Kernel.Left},
                                              Function(k)
                                                  ' Brick:
                                                  If k(Kernel.Center) = 1 Then Return 1
                                                  ' Blank space that might get filled with sand:
                                                  If k(Kernel.Center) = 0 AndAlso k(Kernel.Up) = 0.5 Then Return 0.5
                                                  If k(Kernel.Center) = 0 AndAlso k(Kernel.UpLeft) = 0.5 AndAlso k(Kernel.Left) <> 0 Then Return 0.5
                                                  If k(Kernel.Center) = 0 Then Return 0
                                                  Return 0
                                              End Function)

        Dim kernelRemDR = KernelTracker.Generate({0, 0.5, 1},
                                                 {Kernel.Center, Kernel.Down, Kernel.DownRight, Kernel.Right},
                                                 Function(k)
                                                     ' Sand that might empty out below:
                                                     If k(Kernel.Center) = 0.5 AndAlso k(Kernel.Down) = 0 Then Return 0
                                                     If k(Kernel.Center) = 0.5 AndAlso k(Kernel.DownRight) = 0 AndAlso k(Kernel.Right) <> 0.5 Then Return 0
                                                     If k(Kernel.Center) = 0.5 Then Return 0.5
                                                     Return 0
                                                 End Function)

        StepEffectDL1 = New ConvolveMatrixEffect With {.KernelMatrix = kernelAddDL.ConvolveMatrix, .Divisor = kernelAddDL.ConvolveDivisor, .PreserveAlpha = True}
        StepEffectDL2 = New ConvolveMatrixEffect With {.KernelMatrix = kernelRemDL.ConvolveMatrix, .Divisor = kernelRemDL.ConvolveDivisor, .PreserveAlpha = True}
        Dim StepEffectDL3 = New DiscreteTransferEffect With {.Source = StepEffectDL1, .RedTable = kernelAddDL.TransferTable, .GreenTable = kernelAddDL.TransferTable, .BlueTable = kernelAddDL.TransferTable}
        Dim StepEffectDL4 = New DiscreteTransferEffect With {.Source = StepEffectDL2, .RedTable = kernelRemDL.TransferTable, .GreenTable = kernelRemDL.TransferTable, .BlueTable = kernelRemDL.TransferTable}
        StepEffectDL = New ArithmeticCompositeEffect With {.Source1 = StepEffectDL3, .Source1Amount = 1, .Source2 = StepEffectDL4, .Source2Amount = 1, .MultiplyAmount = 0, .ClampOutput = True}

        StepEffectDR1 = New ConvolveMatrixEffect With {.KernelMatrix = kernelAddDR.ConvolveMatrix, .Divisor = kernelAddDR.ConvolveDivisor, .PreserveAlpha = True}
        StepEffectDR2 = New ConvolveMatrixEffect With {.KernelMatrix = kernelRemDR.ConvolveMatrix, .Divisor = kernelRemDR.ConvolveDivisor, .PreserveAlpha = True}
        Dim StepEffectDR3 = New DiscreteTransferEffect With {.Source = StepEffectDR1, .RedTable = kernelAddDR.TransferTable, .GreenTable = kernelAddDR.TransferTable, .BlueTable = kernelAddDR.TransferTable}
        Dim StepEffectDR4 = New DiscreteTransferEffect With {.Source = StepEffectDR2, .RedTable = kernelRemDR.TransferTable, .GreenTable = kernelRemDR.TransferTable, .BlueTable = kernelRemDR.TransferTable}
        StepEffectDR = New ArithmeticCompositeEffect With {.Source1 = StepEffectDR3, .Source1Amount = 1, .Source2 = StepEffectDR4, .Source2Amount = 1, .MultiplyAmount = 0, .ClampOutput = True}


        DrawEffect1 = New DpiCompensationEffect With {.SourceDpi = New Vector2(canvas1.Dpi)}
        DrawEffect2 = New Transform2DEffect With {.Source = DrawEffect1, .InterpolationMode = CanvasImageInterpolation.NearestNeighbor}
    End Sub

    Sub UpdateSimulation()
        StepEffectDL1.Source = Surface1
        StepEffectDL2.Source = Surface1
        StepEffectDR1.Source = Surface1
        StepEffectDR2.Source = Surface1

        Static Dim StepEffect As ICanvasImage = Nothing
        StepEffect = If(StepEffect IsNot StepEffectDL, StepEffectDL, StepEffectDR)

        Using ds = Surface2.CreateDrawingSession()
            ds.DrawImage(StepEffect)
        End Using

        Swap(Surface1, Surface2)
    End Sub


    Sub Canvas1_Draw(sender As CanvasControl, args As CanvasDrawEventArgs) Handles canvas1.Draw
        Static Dim nextTime As DateTime
        If DateTime.Now > nextTime Then
            UpdateSimulation()
            nextTime = DateTime.Now + TimeSpan.FromSeconds(1 / 60)
        End If

        Dim sourceSizeDips As New Vector2(canvas1.ConvertPixelsToDips(App.CWIDTH), canvas1.ConvertPixelsToDips(App.CHEIGHT))
        Dim canvasSizeDips As New Vector2(CSng(canvas1.ActualWidth), CSng(canvas1.ActualHeight))
        DrawEffect2.TransformMatrix = Matrix3x2.CreateScale(canvasSizeDips / sourceSizeDips)
        DrawEffect1.Source = Surface1
        args.DrawingSession.DrawImage(DrawEffect2)

        canvas1.Invalidate()
    End Sub




    Sub Pen_Clicked(sender As Object, e As RoutedEventArgs) Handles btnBrick.Click, btnSand.Click, btnNothing.Click
        If sender Is btnBrick Then Pen = Colors.White
        If sender Is btnSand Then Pen = Colors.Gray
        If sender Is btnNothing Then Pen = Colors.Black
    End Sub


    Sub Canvas_Pointer(sender As Object, e As PointerRoutedEventArgs) Handles canvas1.PointerPressed, canvas1.PointerMoved
        If Not e.Pointer.IsInContact Then Return

        Dim frac = e.GetCurrentPoint(canvas1).Position.ToVector2 / canvas1.Size.ToVector2
        Dim cx = CInt(Math.Floor(frac.X * App.CWIDTH))
        Dim cy = CInt(Math.Floor(frac.Y * App.CHEIGHT))
        Dim size = Math.Max(App.CWIDTH \ 32, 1)

        Using ds = Surface1.CreateDrawingSession
            ds.Antialiasing = CanvasAntialiasing.Aliased
            ds.FillRectangle(cx - size \ 2, cy - size \ 2, size, size, Pen)
        End Using

        canvas1.Invalidate()
    End Sub



End Class


