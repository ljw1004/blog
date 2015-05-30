Imports System.Numerics
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Effects
Imports Microsoft.Graphics.Canvas.UI.Xaml
Imports Windows.Graphics.DirectX
Imports Windows.UI


Public NotInheritable Class MainPageMinimal
    Inherits Page

    WithEvents App As App = App.Current
    WithEvents canvas1 As CanvasControl

    Dim surface1, surface2 As CanvasRenderTarget
    Dim surface3 As CanvasRenderTarget
    Dim DownLeftEffect1, DownLeftEffect2 As ConvolveMatrixEffect
    Dim DownLeftEffect3, DownLeftEffect4 As DiscreteTransferEffect
    Dim DownLeftEffect5 As CompositeEffect
    'Dim StepEffect(1) As EffectWrapper(Of ICanvasImage)
    Dim DrawEffect1 As DpiCompensationEffect
    Dim DrawEffect2 As Transform2DEffect

    Dim PenMode As Integer = 0

    Sub New()
        InitializeComponent()
        canvas1 = New CanvasControl
        container1.Children.Insert(0, canvas1)
        PenMode = 2
    End Sub


    Sub App_Loaded() Handles App.Loaded
        If surface1 Is Nothing Then Return
        'Dim c = New ColorF(App.Pixels.Length - 1) {}
        'For i = 0 To App.Pixels.Length - 1
        '    c(i) = If(App.Pixels(i) = 1, Colors.White, If(App.Pixels(i) = 2, Colors.Gray, Colors.Black))
        'Next
        'surface1.SetPixelColorFs(c, 0, 0, App.CWIDTH, App.CHEIGHT)

        Dim c = Enumerable.Repeat(ColorF.Black, App.Pixels.Length).ToArray()
        c(App.CWIDTH + 1) = ColorF.Gray
        surface1.SetPixelColorFs(c, 0, 0, App.CWIDTH, App.CHEIGHT)

    End Sub

    Sub App_Unloading() Handles App.Unloading
        If surface1 Is Nothing Then Return
        Return
        Dim c = surface1.GetPixelColors()
        If c.Length <> App.Pixels.Length Then Stop
        For i = 0 To Math.Min(c.Length, App.Pixels.Length) - 1
            App.Pixels(i) = If(c(i) = Colors.White, CByte(1), If(c(i) = Colors.Gray, CByte(2), CByte(0)))
        Next
    End Sub


    Sub Canvas_CreateResources(sender As CanvasControl, args As Object) Handles canvas1.CreateResources
        Const defaultDpi = 96.0F
        surface1 = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Premultiplied)
        surface2 = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Premultiplied)
        surface3 = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Premultiplied)
        App_Loaded()

        DrawEffect1 = New DpiCompensationEffect With {.SourceDpi = New Vector2(canvas1.Dpi)}
        DrawEffect2 = New Transform2DEffect With {.Source = DrawEffect1, .InterpolationMode = CanvasImageInterpolation.NearestNeighbor}

        Dim ingressDL = KernelTracker.Generate({0, 0.5, 1},
                                               {Kernel.Center, Kernel.Up, Kernel.UpRight, Kernel.Right},
                                            Function(k)
                                                If k(Kernel.Center) = 1 Then Return 0
                                                If k(Kernel.Center) = 0.5 Then Return 0
                                                If k(Kernel.Center) = 0 AndAlso k(Kernel.Up) = 0.5 Then Return 0.5
                                                If k(Kernel.Center) = 0 AndAlso k(Kernel.UpRight) = 0.5 AndAlso k(Kernel.Right) <> 0 Then Return 0.5
                                                Return 0
                                            End Function)

        Dim egressDL = KernelTracker.Generate({0, 0.5, 1},
                                        {Kernel.Center, Kernel.Down, Kernel.DownLeft, Kernel.Left},
                                        Function(k)
                                            If k(Kernel.Center) = 1 Then Return 1
                                            If k(Kernel.Center) = 0 Then Return 0
                                            If k(Kernel.Center) = 0.5 AndAlso k(Kernel.Down) = 0 Then Return 0
                                            If k(Kernel.Center) = 0.5 AndAlso k(Kernel.DownLeft) = 0 AndAlso k(Kernel.Left) <> 0.5 Then Return 0
                                            Return 0.5
                                        End Function)
        'StepEffect(0) = KernelTracker.MakeEffect(ingressDL, egressDL, Orientation.Landscape)
        DownLeftEffect1 = New ConvolveMatrixEffect With {.KernelMatrix = ingressDL.ConvolveMatrix, .Divisor = ingressDL.ConvolveDivisor, .PreserveAlpha = True}
        DownLeftEffect2 = New ConvolveMatrixEffect With {.KernelMatrix = egressDL.ConvolveMatrix, .Divisor = egressDL.ConvolveDivisor, .PreserveAlpha = True}
        DownLeftEffect3 = New DiscreteTransferEffect With {.Source = DownLeftEffect1, .RedTable = ingressDL.TransferTable, .GreenTable = ingressDL.TransferTable, .BlueTable = ingressDL.TransferTable}
        DownLeftEffect4 = New DiscreteTransferEffect With {.Source = DownLeftEffect2, .RedTable = egressDL.TransferTable, .GreenTable = egressDL.TransferTable, .BlueTable = egressDL.TransferTable}
        DownLeftEffect5 = New CompositeEffect With {.Mode = CanvasComposite.Add}
        DownLeftEffect5.Sources.Add(DownLeftEffect3)
        DownLeftEffect5.Sources.Add(DownLeftEffect4)



        Dim ingressDR = KernelTracker.Generate({0, 0.5, 1},
                                        {Kernel.Center, Kernel.Up, Kernel.UpLeft, Kernel.Left},
                                        Function(k)
                                            If k(Kernel.Center) = 1 Then Return 0
                                            If k(Kernel.Center) = 0.5 Then Return 0
                                            If k(Kernel.Center) = 0 AndAlso k(Kernel.Up) = 0.5 Then Return 0.5
                                            If k(Kernel.Center) = 0 AndAlso k(Kernel.UpLeft) = 0.5 AndAlso k(Kernel.Left) <> 0 Then Return 0.5
                                            Return 0
                                        End Function)
        Dim egressDR = KernelTracker.Generate({0, 0.5, 1},
                                        {Kernel.Center, Kernel.Down, Kernel.DownRight, Kernel.Right},
                                        Function(k)
                                            If k(Kernel.Center) = 1 Then Return 1
                                            If k(Kernel.Center) = 0 Then Return 0
                                            If k(Kernel.Center) = 0.5 AndAlso k(Kernel.Down) = 0 Then Return 0
                                            If k(Kernel.Center) = 0.5 AndAlso k(Kernel.DownRight) = 0 AndAlso k(Kernel.Right) <> 0.5 Then Return 0
                                            Return 0.5
                                        End Function)
        'StepEffect(1) = KernelTracker.MakeEffect(ingressDR, egressDR, Orientation.Landscape)
    End Sub

    Dim needsStep As Boolean = False
    Sub Step_Click(sender As Object, e As RoutedEventArgs) Handles btnStep.Click
        needsStep = True
    End Sub

    Sub Canvas1_Draw(sender As CanvasControl, args As CanvasDrawEventArgs) Handles canvas1.Draw
        Static Dim nextTime As DateTime
        'If DateTime.Now > nextTime Then
        If needsStep Then
            UpdateSimulation()
            nextTime = DateTime.Now + TimeSpan.FromSeconds(2)
            needsStep = False
        End If

        DrawEffect1.Source = surface1
        Dim sourceSizeDips As New Vector2(canvas1.ConvertPixelsToDips(App.CWIDTH), canvas1.ConvertPixelsToDips(App.CHEIGHT))
        Dim canvasSizeDips As New Vector2(CSng(canvas1.ActualWidth), CSng(canvas1.ActualHeight))
        DrawEffect2.TransformMatrix = Matrix3x2.CreateScale(canvasSizeDips / sourceSizeDips)
        args.DrawingSession.DrawImage(DrawEffect2)

        canvas1.Invalidate()
    End Sub


    Sub UpdateSimulation()
        DownLeftEffect1.Source = surface1
        DownLeftEffect2.Source = surface1

        Using ds = surface2.CreateDrawingSession()
            ds.DrawImage(DownLeftEffect5)
        End Using
        'Win2dSimulator.Simulate(DownLeftEffect5).Verify(surface2)

        Debug.WriteLine($"Source{vbCrLf}{Win2dSimulator.Simulate(surface1)}")
        Using ds = surface3.CreateDrawingSession()
            ds.Blend = CanvasBlend.Copy
            ds.Antialiasing = CanvasAntialiasing.Aliased
            ds.DrawImage(DownLeftEffect1)
        End Using
        Win2dSimulator.Simulate(DownLeftEffect1).Verify(surface3)

        Swap(surface1, surface2)
    End Sub


    Sub Pen_Clicked(sender As Object, e As RoutedEventArgs) Handles btnBrick.Click, btnSand.Click, btnNothing.Click
        PenMode = If(sender Is btnBrick, 1, If(sender Is btnSand, 2, 0))
    End Sub


    Sub Canvas_Pointer(sender As Object, e As PointerRoutedEventArgs) Handles canvas1.PointerPressed, canvas1.PointerMoved
        If Not e.Pointer.IsInContact Then Return

        Dim frac = e.GetCurrentPoint(canvas1).Position.ToVector2 / canvas1.Size.ToVector2
        Dim cx = CInt(frac.X * App.CWIDTH)
        Dim cy = CInt(frac.Y * App.CHEIGHT)
        Dim radius = Math.Max(App.CWIDTH \ 16, 1)

        Dim c = If(PenMode = 1, Colors.White, If(PenMode = 2, Colors.Gray, Colors.Black))
        Using ds = surface1.CreateDrawingSession
            ds.Antialiasing = CanvasAntialiasing.Aliased
            ds.FillCircle(cx, cy, radius, c)
        End Using

        '' We can't write pixels outside the bounds of the surface
        'Dim left = cx - radius \ 2, right = left + radius - 1, top = cy - radius \ 2, bottom = top + radius - 1
        'If left < 0 Then left = 0
        'If right >= App.CWIDTH Then right = App.CWIDTH - 1
        'If top < 0 Then top = 0
        'If bottom >= App.CHEIGHT Then bottom = App.CHEIGHT - 1
        'Dim width = right - left + 1, height = bottom - top + 1
        'If width <= 0 OrElse height <= 0 Then Return


        '' Let's generate and cache a Color() array that's suitable for a singel call to surface1.SetPixelColors
        '' (because if we did many of them, then the interop cost is too high)
        'Static Dim cols As New Dictionary(Of String, Color())
        'Dim key = $"{width},{height},{PenMode}"
        'If Not cols.ContainsKey(key) Then
        '    Dim c = If(PenMode = 1, Colors.White, If(PenMode = 2, Colors.Gray, Colors.Black))
        '    cols(key) = Enumerable.Range(0, width * height).Select(Function(i) c).ToArray()
        'End If
        'surface1.SetPixelColors(cols(key), left, top, width, height)

        canvas1.Invalidate()
    End Sub



End Class


Module Testing
    Sub TestCompositeAdd(canvas1 As CanvasControl)
        Dim defaultDpi = 96.0F
        Debug.WriteLine("amode1,alpha1,rgb1,amode2,alpha2,rgb2,amodeR,alphaR,rgbR")
        For Each am1 In {CanvasAlphaMode.Ignore, CanvasAlphaMode.Premultiplied}
            For Each am2 In {CanvasAlphaMode.Ignore, CanvasAlphaMode.Premultiplied}
                For Each am3 In {CanvasAlphaMode.Ignore, CanvasAlphaMode.Premultiplied}
                    Dim test1 As New CanvasRenderTarget(canvas1, 6, 6, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, am1)
                    Dim test2 As New CanvasRenderTarget(canvas1, 6, 6, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, am2)
                    Dim test3 As New CanvasRenderTarget(canvas1, 6, 6, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, am3)
                    Dim teste As New CompositeEffect With {.Mode = CanvasComposite.Add}
                    teste.Sources.Add(test1) : teste.Sources.Add(test2)
                    For Each a1 In {0F, 0.5F, 1.0F}
                        For Each v1 In {0F, 0.5F, 1.0F}
                            For Each a2 In {0F, 0.5F, 1.0F}
                                For Each v2 In {0F, 0.5F, 1.0F}
                                    Dim c1 = ColorF.FromArgb(a1, v1, v1, v1)
                                    Dim c2 = ColorF.FromArgb(a2, v2, v2, v2)
                                    test1.SetPixelColorFs({c1}, 0, 0, 1, 1)
                                    test2.SetPixelColorFs({c2}, 0, 0, 1, 1)
                                    Using ds = test3.CreateDrawingSession()
                                        ds.Blend = CanvasBlend.Copy
                                        ds.DrawImage(teste)
                                    End Using
                                    Dim c3 = test3.GetPixelColorFs(0, 0, 1, 1)(0)
                                    Debug.WriteLine($"{am1},{c1.A},{c1.R},{am2},{c2.A},{c2.R},{am3},{c3.A},{c3.R}")
                                Next v2
                            Next a2
                        Next v1
                    Next a1
                Next am3
            Next am2
        Next am1
        Stop
    End Sub
End Module