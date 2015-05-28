Imports System.Numerics
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Effects
Imports Microsoft.Graphics.Canvas.UI.Xaml
Imports Windows.UI


Public NotInheritable Class MainPageMinimal
    Inherits Page

    WithEvents App As App = App.Current
    WithEvents canvas1 As CanvasControl

    Dim surface1, surface2 As CanvasRenderTarget
    Dim StepEffect(1) As EffectWrapper(Of ICanvasImage)
    Dim DrawEffect As Transform2DEffect

    Dim PenMode As Integer = 0

    Sub New()
        InitializeComponent()
        canvas1 = New CanvasControl
        container1.Children.Insert(0, canvas1)
        PenMode = 2
    End Sub


    Sub App_Loaded() Handles App.Loaded
        If surface1 Is Nothing Then Return
        Dim c = New Color(App.Pixels.Length - 1) {}
        For i = 0 To App.Pixels.Length - 1
            c(i) = If(App.Pixels(i) = 1, Colors.White, If(App.Pixels(i) = 2, Colors.Gray, Colors.Black))
        Next
        surface1.SetPixelColors(c, 0, 0, App.CWIDTH, App.CHEIGHT)
    End Sub

    Sub App_Unloading() Handles App.Unloading
        If surface1 Is Nothing Then Return
        Dim c = surface1.GetPixelColors()
        If c.Length <> App.Pixels.Length Then Stop
        For i = 0 To Math.Min(c.Length, App.Pixels.Length) - 1
            App.Pixels(i) = If(c(i) = Colors.White, CByte(1), If(c(i) = Colors.Gray, CByte(2), CByte(0)))
        Next
    End Sub


    Sub Canvas_CreateResources(sender As CanvasControl, args As Object) Handles canvas1.CreateResources
        Const defaultDpi = 96.0F
        surface1 = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi)
        surface2 = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi)
        App_Loaded()

        DrawEffect = New Transform2DEffect

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
        StepEffect(0) = KernelTracker.MakeEffect(ingressDL, egressDL, Orientation.Landscape)


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
        StepEffect(1) = KernelTracker.MakeEffect(ingressDR, egressDR, Orientation.Landscape)
    End Sub


    Sub Canvas1_Draw(sender As CanvasControl, args As CanvasDrawEventArgs) Handles canvas1.Draw
        UpdateSimulation()

        DrawEffect.Source = surface1
        Dim sourceSizeDips As New Vector2(App.CWIDTH, App.CHEIGHT)
        Dim canvasSizeDips As New Vector2(CSng(canvas1.ActualWidth), CSng(canvas1.ActualHeight))
        DrawEffect.TransformMatrix = Matrix3x2.CreateScale(canvasSizeDips / sourceSizeDips)
        args.DrawingSession.DrawImage(DrawEffect)

        canvas1.Invalidate()
    End Sub


    Sub UpdateSimulation()
        Static Dim i As Integer = 0
        i = 1 - i
        Using ds = surface2.CreateDrawingSession()
            ds.DrawImage(StepEffect(i).Update(surface1))
        End Using
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
        Dim radius = 10

        ' We can't write pixels outside the bounds of the surface
        Dim left = cx - radius \ 2, right = left + radius - 1, top = cy - radius \ 2, bottom = top + radius - 1
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
            Dim c = If(PenMode = 1, Colors.White, If(PenMode = 2, Colors.Gray, Colors.Black))
            cols(key) = Enumerable.Range(0, width * height).Select(Function(i) c).ToArray()
        End If
        surface1.SetPixelColors(cols(key), left, top, width, height)

        canvas1.Invalidate()
    End Sub



End Class


