Imports System.Numerics
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Effects
Imports Microsoft.Graphics.Canvas.UI.Xaml
Imports Windows.UI


Public NotInheritable Class MainPageV3
    Inherits Page

    WithEvents App As App = App.Current
    WithEvents canvas1 As CanvasControl

    Dim surface1, surface2, surfaceR As CanvasRenderTarget
    Dim StepEffect(Orientation._Last, Phase._Last) As EffectWrapper(Of ICanvasImage)
    Dim RenderEffect, DrawEffect As EffectWrapper(Of ICanvasImage)
    Dim _displayTransform As DisplayTransform

    Dim WindowedOrientation As Orientation
    Dim _penMode As Integer = 0

    Sub New()
        InitializeComponent()
        canvas1 = New CanvasControl
        container1.Children.Insert(0, canvas1)
        PenMode = 2
    End Sub


    Sub App_Loaded() Handles App.Loaded
        If surface1 Is Nothing Then Return
        ' TODO
        'Dim c = New Color(App.Pixels.Length - 1) {}
        'For i = 0 To App.Pixels.Length - 1
        '    c(i) = If(App.Pixels(i) = 1, Colors.White, If(App.Pixels(i) = 2, Colors.Gray, Colors.Black))
        'Next
        'surface1.SetPixelColors(c, 0, 0, App.CWIDTH, App.CHEIGHT)
    End Sub

    Sub App_Unloading() Handles App.Unloading
        If surface1 Is Nothing Then Return
        Dim c = surface1.GetPixelColors()
        If c.Length <> App.Pixels.Length Then Stop
        For i = 0 To Math.Min(c.Length, App.Pixels.Length) - 1
            ' TODO
            'App.Pixels(i) = If(c(i) = Colors.White, CByte(1), If(c(i) = Colors.Gray, CByte(2), CByte(0)))
        Next
    End Sub


    Sub Canvas_CreateResources(sender As CanvasControl, args As Object) Handles canvas1.CreateResources
        Const defaultDpi = 96.0F
        surface1 = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi)
        surface2 = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi)
        surfaceR = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi)
        App_Loaded()

        Dim drawEffect1 As New DiscreteTransferEffect With {.RedTable = {0, 1, 1}, .GreenTable = {0, 0.8, 1}, .BlueTable = {0.1, 0.1, 1}}
        Dim drawEffect2 As New DpiCompensationEffect With {.Source = drawEffect1, .SourceDpi = New Vector2(canvas1.Dpi)}
        Dim drawEffect3 As New Transform2DEffect With {.Source = drawEffect2}
        DrawEffect = New EffectWrapper(Of ICanvasImage)(drawEffect3,
                                                          Sub(source)
                                                              drawEffect1.Source = source
                                                              Dim s = DisplayTransform.Scale
                                                              Dim r = DisplayTransform.Rotation
                                                              Dim o = DisplayTransform.Offset
                                                              drawEffect3.TransformMatrix = New Matrix3x2(s * r.M11, s * r.M12, s * r.M21, s * r.M22, o.X + s * canvas1.ConvertPixelsToDips(CInt(r.M31)), o.Y + s * canvas1.ConvertPixelsToDips(CInt(r.M32)))
                                                          End Sub)


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


        Dim ingressLD = KernelTracker.Generate({0, 0.5, 1},
                                        {Kernel.Center, Kernel.Up, Kernel.UpRight, Kernel.Right},
                                        Function(k)
                                            If k(Kernel.Center) = 1 Then Return 0
                                            If k(Kernel.Center) = 0.5 Then Return 0
                                            If k(Kernel.Center) = 0 AndAlso k(Kernel.UpRight) <> 0.5 AndAlso k(Kernel.Up) = 0.5 Then Return 0.5
                                            If k(Kernel.Center) = 0 AndAlso k(Kernel.UpRight) = 0.5 AndAlso k(Kernel.Right) <> 0 Then Return 0.5
                                            Return 0
                                        End Function)
        Dim egressLD = KernelTracker.Generate({0, 0.5, 1},
                                        {Kernel.Center, Kernel.Down, Kernel.DownLeft, Kernel.Right},
                                        Function(k)
                                            If k(Kernel.Center) = 1 Then Return 1
                                            If k(Kernel.Center) = 0 Then Return 0
                                            If k(Kernel.Center) = 0.5 AndAlso k(Kernel.Down) = 0 AndAlso k(Kernel.Right) <> 0.5 Then Return 0
                                            If k(Kernel.Center) = 0.5 AndAlso k(Kernel.Down) <> 0 AndAlso k(Kernel.DownLeft) = 0 Then Return 0
                                            Return 0.5
                                        End Function)


        Dim ingressRD = KernelTracker.Generate({0, 0.5, 1},
                                        {Kernel.Center, Kernel.Up, Kernel.UpLeft, Kernel.Left},
                                        Function(k)
                                            If k(Kernel.Center) = 1 Then Return 0
                                            If k(Kernel.Center) = 0.5 Then Return 0
                                            If k(Kernel.Center) = 0 AndAlso k(Kernel.UpLeft) <> 0.5 AndAlso k(Kernel.Up) = 0.5 Then Return 0.5
                                            If k(Kernel.Center) = 0 AndAlso k(Kernel.UpLeft) = 0.5 AndAlso k(Kernel.Left) <> 0 Then Return 0.5
                                            Return 0
                                        End Function)
        Dim egressRD = KernelTracker.Generate({0, 0.5, 1},
                                        {Kernel.Center, Kernel.Down, Kernel.DownRight, Kernel.Left},
                                        Function(k)
                                            If k(Kernel.Center) = 1 Then Return 1
                                            If k(Kernel.Center) = 0 Then Return 0
                                            If k(Kernel.Center) = 0.5 AndAlso k(Kernel.Down) = 0 AndAlso k(Kernel.Left) <> 0.5 Then Return 0
                                            If k(Kernel.Center) = 0.5 AndAlso k(Kernel.Down) <> 0 AndAlso k(Kernel.DownRight) = 0 Then Return 0
                                            Return 0.5
                                        End Function)

        For Each o In {Orientation.Landscape, Orientation.Portrait, Orientation.LandscapeFlipped, Orientation.PortraitFlipped}
            StepEffect(o, Phase.DownThenLeft) = KernelTracker.MakeEffect(ingressDL, egressDL, o)
            StepEffect(o, Phase.DownThenRight) = KernelTracker.MakeEffect(ingressDR, egressDR, o)
            StepEffect(o, Phase.LeftThenDown) = KernelTracker.MakeEffect(ingressLD, egressLD, o)
            StepEffect(o, Phase.RightThenDown) = KernelTracker.MakeEffect(ingressRD, egressRD, o)
        Next


        ' This render effect looks at the pixels "left" and "up", and draws in sand if any of them have sand.
        ' This is because our sand algorithm tends to leave one-pixel gaps
        Dim renderTable As Single() = {0, 0.5, 0, 0.5, 0.5, 0.5, 0, 0.5, 0,
            0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
        Dim renderEffect1 As New ConvolveMatrixEffect With {.Divisor = 13, .PreserveAlpha = True, .KernelMatrix = {0, 0, 0, 3, 9, 0, 0, 1, 0}}
        Dim renderEffect2 As New DiscreteTransferEffect With {.Source = renderEffect1, .RedTable = renderTable, .GreenTable = renderTable, .BlueTable = renderTable}
        RenderEffect = New EffectWrapper(Of ICanvasImage)(renderEffect2, Sub(src) renderEffect1.Source = src)
    End Sub


    Sub Page_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Me.SizeChanged
        _displayTransform = Nothing

        Dim isFullScreen = ApplicationView.GetForCurrentView.IsFullScreenMode
        Dim isTablet = (UIViewSettings.GetForCurrentView().UserInteractionMode = UserInteractionMode.Touch)

        btnRotateLeft.Visibility = (Not isTablet).AsVisibility
        btnRotateRight.Visibility = (Not isTablet).AsVisibility
        btnFullScreen.Visibility = (Not isTablet).AsVisibility

        If isTablet AndAlso isFullScreen Then ApplicationView.GetForCurrentView().ExitFullScreenMode()
        canvas1.Invalidate()
    End Sub


    Sub Canvas1_Draw(sender As CanvasControl, args As CanvasDrawEventArgs) Handles canvas1.Draw
        ' The Canvas.Draw will be fired once per screen refresh
        ' (which is how frequently we want to update our simulation)
        UpdateSimulation()

        args.DrawingSession.DrawImage(DrawEffect.Update(surfaceR))
        sender.Invalidate()
    End Sub


    Sub UpdateSimulation()
        Static Dim i As Integer = Phase._First
        Static Dim rnd As New System.Random
        i = rnd.Next() Mod (Phase._Last + 1)
        Using ds = surface2.CreateDrawingSession()
            ds.DrawImage(StepEffect(LogicalOrientation, i).Update(surface1))
            i = (i + 1) Mod (Phase._Last + 1)
        End Using
        Swap(surface1, surface2)
        Using ds = surfaceR.CreateDrawingSession()
            ds.DrawImage(RenderEffect.Update(surface1))
        End Using

        canvas1.Invalidate()
    End Sub




    Property PenMode As Integer
        Get
            Return _penMode
        End Get
        Set(value As Integer)
            Static Dim penUnselectedBrush As Brush = CType(btnBrick.Content, Border).BorderBrush
            Static Dim penSelectedBrush As Brush = CType(btnSand.Content, Border).BorderBrush

            _penMode = value
            CType(btnNothing.Content, Border).BorderBrush = If(_penMode = 0, penSelectedBrush, penUnselectedBrush)
            CType(btnBrick.Content, Border).BorderBrush = If(_penMode = 1, penSelectedBrush, penUnselectedBrush)
            CType(btnSand.Content, Border).BorderBrush = If(_penMode = 2, penSelectedBrush, penUnselectedBrush)
        End Set
    End Property


    Sub Pen_Clicked(sender As Object, e As RoutedEventArgs) Handles btnBrick.Click, btnSand.Click, btnNothing.Click
        PenMode = If(sender Is btnBrick, 1, If(sender Is btnSand, 2, 0))
    End Sub



    ReadOnly Property LogicalOrientation As Orientation
        Get
            Dim isTablet = (UIViewSettings.GetForCurrentView().UserInteractionMode = UserInteractionMode.Touch)
            If Not isTablet Then Return WindowedOrientation
            Dim r = DisplayInformation.GetForCurrentView.CurrentOrientation
            If r = DisplayOrientations.Landscape Then Return Orientation.Landscape
            If r = DisplayOrientations.Portrait Then Return Orientation.Portrait
            If r = DisplayOrientations.LandscapeFlipped Then Return Orientation.LandscapeFlipped
            If r = DisplayOrientations.PortraitFlipped Then Return Orientation.PortraitFlipped
            Return Orientation.Landscape
        End Get
    End Property


    Sub Rotate_Clicked(sender As Object, e As RoutedEventArgs) Handles btnRotateLeft.Click, btnRotateRight.Click
        Dim i As Integer = WindowedOrientation
        i = (i + If(sender Is btnRotateLeft, -1, 1) + Orientation._Last + 1) Mod (Orientation._Last + 1)
        WindowedOrientation = CType(i, Orientation)
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
        Dim b = CInt(radius * 2)

        ' We can't write pixels outside the bounds of the surface
        Dim left = cx - b \ 2, right = left + b - 1, top = cy - b \ 2, bottom = top + b - 1
        If left < 0 Then left = 0
        If right >= App.CWIDTH Then right = App.CWIDTH - 1
        If top < 0 Then top = 0
        If bottom >= App.CHEIGHT Then bottom = App.CHEIGHT - 1
        Dim width = right - left + 1, height = bottom - top + 1
        If width <= 0 OrElse height <= 0 Then Return

        ' Let's generate and cache a Color() array that's suitable for a singel call to surface1.SetPixelColors
        ' (because if we did many of them, then the interop cost is too high)
        Static Dim cols As New Dictionary(Of String, Color())
        Dim key = $"{width},{height},{PenMode}"
        If Not cols.ContainsKey(key) Then
            Dim arr = New Color(width * height - 1) {}
            For y = 0 To height - 1
                Dim c = If(PenMode = 1, Colors.White, If(PenMode = 2 AndAlso (y + cy) Mod 2 = 0, Colors.Gray, Colors.Black))
                For x = 0 To width - 1
                    arr(y * width + x) = c
                Next
            Next
            cols(key) = arr
        End If
        surface1.SetPixelColors(cols(key), left, top, width, height)

        canvas1.Invalidate()
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


