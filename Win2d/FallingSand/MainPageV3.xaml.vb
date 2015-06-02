Imports System.Numerics
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Effects
Imports Microsoft.Graphics.Canvas.UI.Xaml
Imports Windows.UI


Public NotInheritable Class MainPageV3
    Inherits Page

    WithEvents App As App = App.Current
    WithEvents canvas1 As CanvasControl

    Dim SurfaceR, Surface, SurfaceId, SurfaceAddFromUp, SurfaceAddFromDiag, SurfaceRemToDown, SurfaceRemToDiag As CanvasRenderTarget
    Dim StepEffect, StepAddFromUp(Orientation._Last, Phase._Last), StepAddFromDiag(Orientation._Last, Phase._Last), StepRemToDown(Orientation._Last, Phase._Last), StepRemToDiag(Orientation._Last, Phase._Last) As ICanvasImage
    Dim RenderEffect As ICanvasImage
    Dim DrawEffect As Transform2DEffect
    Dim _displayTransform As DisplayTransform

    Dim _WindowedOrientation As Orientation
    Dim _pen As Color

    Sub New()
        InitializeComponent()
        canvas1 = New CanvasControl
        container1.Children.Insert(0, canvas1)
        Pen = Colors.Gray
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
        SurfaceR = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi)
        Surface = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi)
        SurfaceId = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi)
        SurfaceAddFromUp = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi)
        SurfaceAddFromDiag = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi)
        SurfaceRemToDown = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi)
        SurfaceRemToDiag = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi)
        App_Loaded()

        Dim kernelAddFromUp(Phase._Last), kernelAddFromDiag(Phase._Last), kernelRemToDown(Phase._Last), kernelRemToDiag(Phase._Last) As KernelTracker


        kernelAddFromUp(Phase.DownThenLeft) = KernelTracker.Generate({0, 0.5, 1}, {Kernel.Center, Kernel.Up},
                                            Function(k)
                                                If k(Kernel.Center) = 0 AndAlso k(Kernel.Up) = 0.5 Then Return 0.5
                                                Return 0
                                            End Function)

        kernelAddFromUp(Phase.DownThenRight) = kernelAddFromUp(Phase.DownThenLeft)

        kernelAddFromUp(Phase.LeftThenDown) = KernelTracker.Generate({0, 0.5, 1}, {Kernel.Center, Kernel.Up, Kernel.UpRight},
                                                Function(k)
                                                    If k(Kernel.Center) = 0 AndAlso k(Kernel.UpRight) <> 0.5 AndAlso k(Kernel.Up) = 0.5 Then Return 0.5
                                                    Return 0
                                                End Function)

        kernelAddFromUp(Phase.RightThenDown) = KernelTracker.Generate({0, 0.5, 1}, {Kernel.Center, Kernel.Up, Kernel.UpLeft},
                                                    Function(k)
                                                        If k(Kernel.Center) = 0 AndAlso k(Kernel.UpLeft) <> 0.5 AndAlso k(Kernel.Up) = 0.5 Then Return 0.5
                                                        Return 0
                                                    End Function)

        kernelAddFromDiag(Phase.DownThenLeft) = KernelTracker.Generate({0, 0.5, 1}, {Kernel.Center, Kernel.UpRight, Kernel.Right},
                                            Function(k)
                                                If k(Kernel.Center) = 0 AndAlso k(Kernel.UpRight) = 0.5 AndAlso k(Kernel.Right) <> 0 Then Return 0.5
                                                Return 0
                                            End Function)

        kernelAddFromDiag(Phase.DownThenRight) = KernelTracker.Generate({0, 0.5, 1}, {Kernel.Center, Kernel.UpLeft, Kernel.Left},
                                            Function(k)
                                                If k(Kernel.Center) = 0 AndAlso k(Kernel.UpLeft) = 0.5 AndAlso k(Kernel.Left) <> 0 Then Return 0.5
                                                Return 0
                                            End Function)

        kernelAddFromDiag(Phase.LeftThenDown) = KernelTracker.Generate({0, 0.5, 1}, {Kernel.Center, Kernel.UpRight, Kernel.Right},
                                        Function(k)
                                            If k(Kernel.Center) = 0 AndAlso k(Kernel.UpRight) = 0.5 AndAlso k(Kernel.Right) <> 0 Then Return 0.5
                                            Return 0
                                        End Function)

        kernelAddFromDiag(Phase.RightThenDown) = KernelTracker.Generate({0, 0.5, 1}, {Kernel.Center, Kernel.UpLeft, Kernel.Left},
                                        Function(k)
                                            If k(Kernel.Center) = 0 AndAlso k(Kernel.UpLeft) = 0.5 AndAlso k(Kernel.Left) <> 0 Then Return 0.5
                                            Return 0
                                        End Function)


        kernelRemToDown(Phase.DownThenLeft) = KernelTracker.Generate({0, 0.5, 1}, {Kernel.Center, Kernel.Down},
                                        Function(k)
                                            If k(Kernel.Center) = 0.5 AndAlso k(Kernel.Down) = 0 Then Return 0.5
                                            Return 0
                                        End Function)

        kernelRemToDown(Phase.DownThenRight) = kernelRemToDown(Phase.DownThenLeft)

        kernelRemToDown(Phase.LeftThenDown) = KernelTracker.Generate({0, 0.5, 1}, {Kernel.Center, Kernel.Down, Kernel.Right},
                                        Function(k)
                                            If k(Kernel.Center) = 0.5 AndAlso k(Kernel.Down) = 0 AndAlso k(Kernel.Right) <> 0.5 Then Return 0.5
                                            Return 0
                                        End Function)

        kernelRemToDown(Phase.RightThenDown) = KernelTracker.Generate({0, 0.5, 1}, {Kernel.Center, Kernel.Down, Kernel.Left},
                                        Function(k)
                                            If k(Kernel.Center) = 0.5 AndAlso k(Kernel.Down) = 0 AndAlso k(Kernel.Left) <> 0.5 Then Return 0.5
                                            Return 0
                                        End Function)

        kernelRemToDiag(Phase.DownThenLeft) = KernelTracker.Generate({0, 0.5, 1}, {Kernel.Center, Kernel.DownLeft, Kernel.Left},
                                        Function(k)
                                            If k(Kernel.Center) = 0.5 AndAlso k(Kernel.DownLeft) = 0 AndAlso k(Kernel.Left) <> 0.5 Then Return 0.5
                                            Return 0
                                        End Function)

        kernelRemToDiag(Phase.DownThenRight) = KernelTracker.Generate({0, 0.5, 1}, {Kernel.Center, Kernel.DownRight, Kernel.Right},
                                        Function(k)
                                            If k(Kernel.Center) = 0.5 AndAlso k(Kernel.DownRight) = 0 AndAlso k(Kernel.Right) <> 0.5 Then Return 0.5
                                            Return 0
                                        End Function)

        kernelRemToDiag(Phase.LeftThenDown) = KernelTracker.Generate({0, 0.5, 1}, {Kernel.Center, Kernel.Down, Kernel.DownLeft},
                                        Function(k)
                                            If k(Kernel.Center) = 0.5 AndAlso k(Kernel.Down) <> 0 AndAlso k(Kernel.DownLeft) = 0 Then Return 0.5
                                            Return 0
                                        End Function)

        kernelRemToDiag(Phase.RightThenDown) = KernelTracker.Generate({0, 0.5, 1}, {Kernel.Center, Kernel.Down, Kernel.DownRight},
                                        Function(k)
                                            If k(Kernel.Center) = 0.5 AndAlso k(Kernel.Down) <> 0 AndAlso k(Kernel.DownRight) = 0 Then Return 0.5
                                            Return 0
                                        End Function)

        For p = Phase._First To Phase._Last
            For o = Orientation._First To Orientation._Last
                Dim convolveAddFromUp_p As New ConvolveMatrixEffect With {.Source = Surface, .KernelMatrix = kernelAddFromUp(p).ConvolveMatrixRotated(o), .Divisor = kernelAddFromUp(p).ConvolveDivisor, .PreserveAlpha = True}
                Dim convolveAddFromDiag_p As New ConvolveMatrixEffect With {.Source = Surface, .KernelMatrix = kernelAddFromDiag(p).ConvolveMatrixRotated(o), .Divisor = kernelAddFromDiag(p).ConvolveDivisor, .PreserveAlpha = True}
                Dim convolveRemToDown_p As New ConvolveMatrixEffect With {.Source = Surface, .KernelMatrix = kernelRemToDown(p).ConvolveMatrixRotated(o), .Divisor = kernelRemToDown(p).ConvolveDivisor, .PreserveAlpha = True}
                Dim convolveRemToDiag_p As New ConvolveMatrixEffect With {.Source = Surface, .KernelMatrix = kernelRemToDiag(p).ConvolveMatrixRotated(o), .Divisor = kernelRemToDiag(p).ConvolveDivisor, .PreserveAlpha = True}
                StepAddFromUp(o, p) = New DiscreteTransferEffect With {.Source = convolveAddFromUp_p, .RedTable = kernelAddFromUp(p).TransferTable, .GreenTable = kernelAddFromUp(p).TransferTable, .BlueTable = kernelAddFromUp(p).TransferTable, .AlphaTable = {1}}
                StepAddFromDiag(o, p) = New DiscreteTransferEffect With {.Source = convolveAddFromDiag_p, .RedTable = kernelAddFromDiag(p).TransferTable, .GreenTable = kernelAddFromDiag(p).TransferTable, .BlueTable = kernelAddFromDiag(p).TransferTable, .AlphaTable = {1}}
                StepRemToDown(o, p) = New DiscreteTransferEffect With {.Source = convolveRemToDown_p, .RedTable = kernelRemToDown(p).TransferTable, .GreenTable = kernelRemToDown(p).TransferTable, .BlueTable = kernelRemToDown(p).TransferTable, .AlphaTable = {1}}
                StepRemToDiag(o, p) = New DiscreteTransferEffect With {.Source = convolveRemToDiag_p, .RedTable = kernelRemToDiag(p).TransferTable, .GreenTable = kernelRemToDiag(p).TransferTable, .BlueTable = kernelRemToDiag(p).TransferTable, .AlphaTable = {1}}
            Next
        Next


        Dim stepAdd As New ArithmeticCompositeEffect With {.Source1 = SurfaceAddFromUp, .Source2 = SurfaceAddFromDiag, .Source1Amount = 2, .Source2Amount = 2, .MultiplyAmount = 0, .ClampOutput = True}
        Dim stepRem As New ArithmeticCompositeEffect With {.Source1 = SurfaceRemToDown, .Source2 = SurfaceRemToDiag, .Source1Amount = 2, .Source2Amount = 2, .MultiplyAmount = 0, .ClampOutput = True}
        Dim stepAcc1 As New ArithmeticCompositeEffect With {.Source1 = SurfaceId, .Source2 = stepRem, .Source1Amount = 1, .Source2Amount = -0.5, .MultiplyAmount = 0}
        Dim stepAcc2 As New ArithmeticCompositeEffect With {.Source1 = stepAcc1, .Source2 = stepAdd, .Source1Amount = 1, .Source2Amount = 0.5, .MultiplyAmount = 0}
        StepEffect = New DiscreteTransferEffect With {.Source = stepAcc2, .RedTable = {0, 0.5, 1}, .GreenTable = {0, 0.5, 1}, .BlueTable = {0, 0.5, 1}, .AlphaTable = {1}}


        ' This render effect looks at the pixels "left" and "up", and draws in sand if any of them have sand.
        ' This is because our sand algorithm tends to leave one-pixel gaps
        Dim renderTable As Single() = {
            0, 0.5, 0, 0.5, 0.5, 0.5, 0, 0.5, 0,
            0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
        Dim renderEffect1 As New ConvolveMatrixEffect With {.Source = Surface, .Divisor = 13, .PreserveAlpha = True, .KernelMatrix = {0, 0, 0, 3, 9, 0, 0, 1, 0}}
        RenderEffect = New DiscreteTransferEffect With {.Source = renderEffect1, .RedTable = renderTable, .GreenTable = renderTable, .BlueTable = renderTable}


        Dim drawEffect1 As New DiscreteTransferEffect With {.Source = SurfaceR, .RedTable = {0, 1, 1}, .GreenTable = {0, 0.8, 1}, .BlueTable = {0.1, 0.1, 1}}
        Dim drawEffect2 As New DpiCompensationEffect With {.Source = drawEffect1, .SourceDpi = New Vector2(canvas1.Dpi)}
        DrawEffect = New Transform2DEffect With {.Source = drawEffect2}
    End Sub


    Sub UpdateSimulation()
        Static Dim rnd As New Random
        Static Dim p As Phase
        p = CType(rnd.Next() Mod (Phase._Last + 1), Phase)
        Dim o = LogicalOrientation

        Using dsId = SurfaceId.CreateDrawingSession,
              dsAddFromUp = SurfaceAddFromUp.CreateDrawingSession(),
              dsAddFromDiag = SurfaceAddFromDiag.CreateDrawingSession(),
              dsRemToDown = SurfaceRemToDown.CreateDrawingSession(),
              dsRemToDiag = SurfaceRemToDiag.CreateDrawingSession()

            dsId.DrawImage(Surface)
            dsAddFromUp.DrawImage(StepAddFromUp(o, p))
            dsAddFromDiag.DrawImage(StepAddFromDiag(o, p))
            dsRemToDown.DrawImage(StepRemToDown(o, p))
            dsRemToDiag.DrawImage(StepRemToDiag(o, p))
        End Using

        Using ds = Surface.CreateDrawingSession()
            ds.DrawImage(StepEffect)
        End Using

        Using dsR = SurfaceR.CreateDrawingSession()
            dsR.DrawImage(RenderEffect)
        End Using


    End Sub


    Sub Canvas1_Draw(sender As CanvasControl, args As CanvasDrawEventArgs) Handles canvas1.Draw
        ' The Canvas.Draw will be fired once per screen refresh 60hz
        ' I'd like ~200hz updates, so I'll update the simulation three times per frame
        UpdateSimulation() : UpdateSimulation() : UpdateSimulation()

        Dim s = DisplayTransform.Scale, r = DisplayTransform.Rotation, o = DisplayTransform.Offset
        DrawEffect.TransformMatrix = New Matrix3x2(s * r.M11, s * r.M12, s * r.M21, s * r.M22, o.X + s * canvas1.ConvertPixelsToDips(CInt(r.M31)), o.Y + s * canvas1.ConvertPixelsToDips(CInt(r.M32)))
        args.DrawingSession.DrawImage(DrawEffect)

        sender.Invalidate()
    End Sub

    Sub Page_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Me.SizeChanged
        _displayTransform = Nothing

        Dim isFullScreen = ApplicationView.GetForCurrentView.IsFullScreenMode
        Dim isTablet = (UIViewSettings.GetForCurrentView().UserInteractionMode = UserInteractionMode.Touch)

        btnRotateLeft.Visibility = (Not isTablet).AsVisibility
        btnRotateRight.Visibility = (Not isTablet).AsVisibility
        btnFullScreen.Visibility = (Not isTablet).AsVisibility

        If isTablet AndAlso isFullScreen Then ApplicationView.GetForCurrentView().ExitFullScreenMode()
    End Sub





    Property Pen As Color
        Get
            Return _pen
        End Get
        Set(value As Color)
            Static Dim penUnselectedBrush As Brush = CType(btnBrick.Content, Border).BorderBrush
            Static Dim penSelectedBrush As Brush = CType(btnSand.Content, Border).BorderBrush

            _pen = value
            CType(btnNothing.Content, Border).BorderBrush = If(_pen = Colors.Black, penSelectedBrush, penUnselectedBrush)
            CType(btnBrick.Content, Border).BorderBrush = If(_pen = Colors.White, penSelectedBrush, penUnselectedBrush)
            CType(btnSand.Content, Border).BorderBrush = If(_pen = Colors.Gray, penSelectedBrush, penUnselectedBrush)
        End Set
    End Property


    Sub Pen_Clicked(sender As Object, e As RoutedEventArgs) Handles btnBrick.Click, btnSand.Click, btnNothing.Click
        If sender Is btnBrick Then Pen = Colors.White
        If sender Is btnSand Then Pen = Colors.Gray
        If sender Is btnNothing Then Pen = Colors.Black
    End Sub



    ReadOnly Property LogicalOrientation As Orientation
        Get
            Dim isTablet = (UIViewSettings.GetForCurrentView().UserInteractionMode = UserInteractionMode.Touch)
            If Not isTablet Then Return _WindowedOrientation
            Dim r = DisplayInformation.GetForCurrentView.CurrentOrientation
            If r = DisplayOrientations.Landscape Then Return Orientation.Landscape
            If r = DisplayOrientations.Portrait Then Return Orientation.Portrait
            If r = DisplayOrientations.LandscapeFlipped Then Return Orientation.LandscapeFlipped
            If r = DisplayOrientations.PortraitFlipped Then Return Orientation.PortraitFlipped
            Return Orientation.Landscape
        End Get
    End Property


    Sub Rotate_Clicked(sender As Object, e As RoutedEventArgs) Handles btnRotateLeft.Click, btnRotateRight.Click
        Dim i As Integer = _WindowedOrientation
        i = (i + If(sender Is btnRotateLeft, -1, 1) + Orientation._Last + 1) Mod (Orientation._Last + 1)
        _WindowedOrientation = CType(i, Orientation)
        _displayTransform = Nothing
        canvas1.Invalidate()
    End Sub


    Sub FullScreen_Clicked(sender As Object, e As RoutedEventArgs) Handles btnFullScreen.Click
        Dim isFullScreen = ApplicationView.GetForCurrentView.IsFullScreenMode

        If isFullScreen Then
            ApplicationView.GetForCurrentView.ExitFullScreenMode()
        Else
            ApplicationView.GetForCurrentView.TryEnterFullScreenMode()
        End If
    End Sub

    Sub Canvas_Pointer(sender As Object, e As PointerRoutedEventArgs) Handles canvas1.PointerPressed, canvas1.PointerMoved
        If Not e.Pointer.IsInContact Then Return

        Dim canvasPointDips = e.GetCurrentPoint(canvas1).Position.ToVector2() - DisplayTransform.Offset
        canvasPointDips /= canvas1.ConvertPixelsToDips(1)
        Dim sourcePointPixels = Vector2.Transform(canvasPointDips / DisplayTransform.Scale, DisplayTransform.Rotation.Invert)
        Dim cx = CInt(Math.Floor(sourcePointPixels.X))
        Dim cy = CInt(Math.Floor(sourcePointPixels.Y))

        Dim radius = CType(btnBrick.Content, Border).ActualWidth / 2 / DisplayTransform.Scale
        Dim size = Math.Max(1, CInt(radius * 2))
        Dim left = cx - size \ 2, top = cy - size \ 2, width = size, height = size
        Using ds = Surface.CreateDrawingSession
            ds.Antialiasing = CanvasAntialiasing.Aliased
            If Pen <> Colors.Gray Then
                ds.FillRectangle(left, top, width, height, Pen)
            Else
                For y = top To top + height
                    Dim c = If(y Mod 2 = 0, Pen, Colors.Black)
                    ds.FillRectangle(left, y, width, 1, c)
                Next
            End If
        End Using

    End Sub



    ReadOnly Property DisplayTransform As DisplayTransform
        Get
            If _displayTransform IsNot Nothing Then Return _displayTransform

            Dim isOnSide = (LogicalOrientation = Orientation.Portrait OrElse LogicalOrientation = Orientation.PortraitFlipped)
            Dim bitmapSizeDips As New Vector2(canvas1.ConvertPixelsToDips(App.CWIDTH), canvas1.ConvertPixelsToDips(App.CHEIGHT))
            If isOnSide Then Swap(bitmapSizeDips.X, bitmapSizeDips.Y)
            Dim canvasSizeDips As New Vector2(CSng(canvas1.ActualWidth), CSng(canvas1.ActualHeight))
            Dim scale = canvasSizeDips / bitmapSizeDips
            Dim offset = Vector2.Zero

            If scale.X > scale.Y Then
                scale.X = scale.Y
                offset.X = (canvasSizeDips.X - bitmapSizeDips.X * scale.X) / 2
            Else
                scale.Y = scale.X
                offset.Y = (canvasSizeDips.Y - bitmapSizeDips.Y * scale.Y) / 2
            End If

            Dim r As Matrix3x2
            If LogicalOrientation = Orientation.Landscape Then r = New Matrix3x2(1, 0, 0, 1, 0, 0) ' identity
            If LogicalOrientation = Orientation.PortraitFlipped Then r = New Matrix3x2(0, -1, 1, 0, 0, App.CWIDTH) ' 90deg counter-clockwise
            If LogicalOrientation = Orientation.LandscapeFlipped Then r = New Matrix3x2(-1, 0, 0, -1, App.CWIDTH, App.CHEIGHT) ' 180deg
            If LogicalOrientation = Orientation.Portrait Then r = New Matrix3x2(0, 1, -1, 0, App.CHEIGHT, 0) ' 270deg counter-clickwise, i.e. 90deg clockwise

            _displayTransform = New DisplayTransform With {.Scale = scale.X, .Offset = offset, .Rotation = r}
            Return _displayTransform
        End Get
    End Property

End Class


