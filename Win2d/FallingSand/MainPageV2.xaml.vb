Imports System.Numerics
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Effects
Imports Microsoft.Graphics.Canvas.UI.Xaml
Imports Windows.UI


Public NotInheritable Class MainPageV2
    Inherits Page

    WithEvents App As App = App.Current
    WithEvents canvas1 As CanvasControl

    Dim Surface, SurfaceId, SurfaceAddFromUp, SurfaceAddFromDiag, SurfaceRemToDown, SurfaceRemToDiag As CanvasRenderTarget
    Dim StepEffect, StepAddFromUp, StepAddFromUpRight, StepAddFromUpLeft, StepRemToDown, StepRemToDownLeft, StepRemToDownRight As ICanvasImage
    Dim DrawEffect As Transform2DEffect

    Dim Pen As Color = Colors.Gray

    Sub New()
        InitializeComponent()
        canvas1 = New CanvasControl
        container1.Children.Insert(0, canvas1)
    End Sub


    Sub App_Loaded() Handles App.Loaded
        If Surface Is Nothing Then Return
        Surface.SetPixelColors(App.Pixels)
    End Sub

    Sub App_Unloading() Handles App.Unloading
        If Surface Is Nothing Then Return
        App.Pixels = Surface.GetPixelColors()
    End Sub


    Sub Canvas_CreateResources(sender As CanvasControl, args As Object) Handles canvas1.CreateResources
        Const defaultDpi = 96.0F
        Surface = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi)
        SurfaceId = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi)
        SurfaceAddFromUp = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi)
        SurfaceAddFromDiag = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi)
        SurfaceRemToDown = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi)
        SurfaceRemToDiag = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi)
        App_Loaded()


        Dim kernelAddFromUp = KernelTracker.Generate({0, 0.5, 1}, {Kernel.Center, Kernel.Up},
                                            Function(k)
                                                If k(Kernel.Center) = 0 AndAlso k(Kernel.Up) = 0.5 Then Return 0.5
                                                Return 0
                                            End Function)

        Dim kernelAddFromUpRight = KernelTracker.Generate({0, 0.5, 1}, {Kernel.Center, Kernel.UpRight, Kernel.Right},
                                            Function(k)
                                                If k(Kernel.Center) = 0 AndAlso k(Kernel.UpRight) = 0.5 AndAlso k(Kernel.Right) <> 0 Then Return 0.5
                                                Return 0
                                            End Function)

        Dim kernelAddFromUpLeft = KernelTracker.Generate({0, 0.5, 1}, {Kernel.Center, Kernel.UpLeft, Kernel.Left},
                                            Function(k)
                                                If k(Kernel.Center) = 0 AndAlso k(Kernel.UpLeft) = 0.5 AndAlso k(Kernel.Left) <> 0 Then Return 0.5
                                                Return 0
                                            End Function)

        Dim kernelRemToDown = KernelTracker.Generate({0, 0.5, 1}, {Kernel.Center, Kernel.Down},
                                        Function(k)
                                            If k(Kernel.Center) = 0.5 AndAlso k(Kernel.Down) = 0 Then Return 0.5
                                            Return 0
                                        End Function)

        Dim kernelRemToDownLeft = KernelTracker.Generate({0, 0.5, 1}, {Kernel.Center, Kernel.DownLeft, Kernel.Left},
                                        Function(k)
                                            If k(Kernel.Center) = 0.5 AndAlso k(Kernel.DownLeft) = 0 AndAlso k(Kernel.Left) <> 0.5 Then Return 0.5
                                            Return 0
                                        End Function)

        Dim kernelRemToDownRight = KernelTracker.Generate({0, 0.5, 1}, {Kernel.Center, Kernel.DownRight, Kernel.Right},
                                        Function(k)
                                            If k(Kernel.Center) = 0.5 AndAlso k(Kernel.DownRight) = 0 AndAlso k(Kernel.Right) <> 0.5 Then Return 0.5
                                            Return 0
                                        End Function)

        Dim convolveAddFromUp As New ConvolveMatrixEffect With {.Source = Surface, .KernelMatrix = kernelAddFromUp.ConvolveMatrix, .Divisor = kernelAddFromUp.ConvolveDivisor, .PreserveAlpha = True}
        Dim convolveAddFromUpRight As New ConvolveMatrixEffect With {.Source = Surface, .KernelMatrix = kernelAddFromUpRight.ConvolveMatrix, .Divisor = kernelAddFromUpRight.ConvolveDivisor, .PreserveAlpha = True}
        Dim convolveAddFromUpLeft As New ConvolveMatrixEffect With {.Source = Surface, .KernelMatrix = kernelAddFromUpLeft.ConvolveMatrix, .Divisor = kernelAddFromUpRight.ConvolveDivisor, .PreserveAlpha = True}
        Dim convolveRemToDown As New ConvolveMatrixEffect With {.Source = Surface, .KernelMatrix = kernelRemToDown.ConvolveMatrix, .Divisor = kernelRemToDown.ConvolveDivisor, .PreserveAlpha = True}
        Dim convolveRemToDownLeft As New ConvolveMatrixEffect With {.Source = Surface, .KernelMatrix = kernelRemToDownLeft.ConvolveMatrix, .Divisor = kernelRemToDownLeft.ConvolveDivisor, .PreserveAlpha = True}
        Dim convolveRemToDownRight As New ConvolveMatrixEffect With {.Source = Surface, .KernelMatrix = kernelRemToDownRight.ConvolveMatrix, .Divisor = kernelRemToDownLeft.ConvolveDivisor, .PreserveAlpha = True}
        StepAddFromUp = New DiscreteTransferEffect With {.Source = convolveAddFromUp, .RedTable = kernelAddFromUp.TransferTable, .GreenTable = kernelAddFromUp.TransferTable, .BlueTable = kernelAddFromUp.TransferTable}
        StepAddFromUpRight = New DiscreteTransferEffect With {.Source = convolveAddFromUpRight, .RedTable = kernelAddFromUpRight.TransferTable, .GreenTable = kernelAddFromUpRight.TransferTable, .BlueTable = kernelAddFromUpRight.TransferTable}
        StepAddFromUpLeft = New DiscreteTransferEffect With {.Source = convolveAddFromUpLeft, .RedTable = kernelAddFromUpLeft.TransferTable, .GreenTable = kernelAddFromUpLeft.TransferTable, .BlueTable = kernelAddFromUpLeft.TransferTable}
        StepRemToDown = New DiscreteTransferEffect With {.Source = convolveRemToDown, .RedTable = kernelRemToDown.TransferTable, .GreenTable = kernelRemToDown.TransferTable, .BlueTable = kernelRemToDown.TransferTable}
        StepRemToDownLeft = New DiscreteTransferEffect With {.Source = convolveRemToDownLeft, .RedTable = kernelRemToDownLeft.TransferTable, .GreenTable = kernelRemToDownLeft.TransferTable, .BlueTable = kernelRemToDownLeft.TransferTable}
        StepRemToDownRight = New DiscreteTransferEffect With {.Source = convolveRemToDownRight, .RedTable = kernelRemToDownRight.TransferTable, .GreenTable = kernelRemToDownRight.TransferTable, .BlueTable = kernelRemToDownRight.TransferTable}

        Dim stepAdd As New ArithmeticCompositeEffect With {.Source1 = SurfaceAddFromUp, .Source2 = SurfaceAddFromDiag, .Source1Amount = 2, .Source2Amount = 2, .MultiplyAmount = 0, .ClampOutput = True}
        Dim stepRem As New ArithmeticCompositeEffect With {.Source1 = SurfaceRemToDown, .Source2 = SurfaceRemToDiag, .Source1Amount = 2, .Source2Amount = 2, .MultiplyAmount = 0, .ClampOutput = True}
        Dim stepAcc1 As New ArithmeticCompositeEffect With {.Source1 = SurfaceId, .Source2 = stepRem, .Source1Amount = 1, .Source2Amount = -0.5, .MultiplyAmount = 0}
        Dim stepAcc2 As New ArithmeticCompositeEffect With {.Source1 = stepAcc1, .Source2 = stepAdd, .Source1Amount = 1, .Source2Amount = 0.5, .MultiplyAmount = 0}
        StepEffect = New DiscreteTransferEffect With {.Source = stepAcc2, .RedTable = {0, 0.5, 1}, .GreenTable = {0, 0.5, 1}, .BlueTable = {0, 0.5, 1}, .AlphaTable = {1}}


        Dim drawEffect0 As New DpiCompensationEffect With {.Source = Surface, .SourceDpi = New Vector2(canvas1.Dpi)}
        DrawEffect = New Transform2DEffect With {.Source = drawEffect0, .InterpolationMode = CanvasImageInterpolation.NearestNeighbor}
    End Sub

    Sub UpdateSimulation()
        Static Dim stepAddFromDiag As ICanvasImage = Nothing
        Static Dim stepRemToDiag As ICanvasImage = Nothing
        stepAddFromDiag = If(stepAddFromDiag IsNot StepAddFromUpRight, StepAddFromUpRight, StepAddFromUpLeft)
        stepRemToDiag = If(stepRemToDiag IsNot StepRemToDownLeft, StepRemToDownLeft, StepRemToDownRight)

        Using dsId = SurfaceId.CreateDrawingSession,
              dsAddFromUp = SurfaceAddFromUp.CreateDrawingSession(),
              dsAddFromDiag = SurfaceAddFromDiag.CreateDrawingSession(),
              dsRemToDown = SurfaceRemToDown.CreateDrawingSession(),
              dsRemToDiag = SurfaceRemToDiag.CreateDrawingSession()
            dsId.DrawImage(Surface)
            dsAddFromUp.DrawImage(StepAddFromUp)
            dsAddFromDiag.DrawImage(stepAddFromDiag)
            dsRemToDown.DrawImage(StepRemToDown)
            dsRemToDiag.DrawImage(stepRemToDiag)
        End Using

        Using ds = Surface.CreateDrawingSession()
            ds.DrawImage(StepEffect)
        End Using
    End Sub

    Sub Canvas1_Draw(sender As CanvasControl, args As CanvasDrawEventArgs) Handles canvas1.Draw
        Static Dim nextTime As DateTime
        If DateTime.Now > nextTime Then
            UpdateSimulation()
            nextTime = DateTime.Now + TimeSpan.FromSeconds(1 / 60)
        End If

        Dim sourceSizeDips As New Vector2(canvas1.ConvertPixelsToDips(App.CWIDTH), canvas1.ConvertPixelsToDips(App.CHEIGHT))
        Dim canvasSizeDips As New Vector2(CSng(canvas1.ActualWidth), CSng(canvas1.ActualHeight))
        DrawEffect.TransformMatrix = Matrix3x2.CreateScale(canvasSizeDips / sourceSizeDips)
        args.DrawingSession.DrawImage(DrawEffect)

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

        Using ds = Surface.CreateDrawingSession
            ds.Antialiasing = CanvasAntialiasing.Aliased
            ds.FillRectangle(cx - size \ 2, cy - size \ 2, size, size, Pen)
        End Using

        canvas1.Invalidate()
    End Sub

End Class
