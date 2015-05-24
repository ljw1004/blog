Imports System.Numerics
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Effects
Imports Microsoft.Graphics.Canvas.UI.Xaml
Imports Windows.UI


Public NotInheritable Class MainPage
    Inherits Page

    WithEvents App As App = App.Current
    WithEvents canvas1 As CanvasControl
    Dim surface1, surface2 As CanvasRenderTarget
    Dim StepEffect(Kernel._Last, 1) As EffectWrapper
    Dim renderEffect1 As New DiscreteTransferEffect With {.RedTable = {0, 1, 1}, .GreenTable = {0, 0.8, 1}, .BlueTable = {0.4, 0.1, 1}}
    Dim renderEffect2 As New DpiCompensationEffect With {.Source = renderEffect1}
    Dim renderEffect3 As New Transform2DEffect With {.Source = renderEffect2, .InterpolationMode = CanvasImageInterpolation.NearestNeighbor}

    Dim timer As Stopwatch = Stopwatch.StartNew()
    Dim lastTime As TimeSpan

    Dim _penMode As Integer = 0
    Dim penHoverBrush As New SolidColorBrush(Colors.DarkGray)
    Dim penSelectedBrush As New SolidColorBrush(Colors.Blue)
    Dim penUnselectedBrush As New SolidColorBrush(Color.FromArgb(255, 70, 70, 70))


    Sub New()
        InitializeComponent()
        canvas1 = New CanvasControl
        container1.Children.Insert(0, canvas1)
        PenMode = 2
    End Sub


    Property PenMode As Integer
        Get
            Return _penMode
        End Get
        Set(value As Integer)
            _penMode = value
            CType(btnNothing.Content, Border).BorderBrush = If(_penMode = 0, penSelectedBrush, penUnselectedBrush)
            CType(btnBrick.Content, Border).BorderBrush = If(_penMode = 1, penSelectedBrush, penUnselectedBrush)
            CType(btnSand.Content, Border).BorderBrush = If(_penMode = 2, penSelectedBrush, penUnselectedBrush)
        End Set
    End Property

    Sub Pen_Clicked(sender As Object, e As RoutedEventArgs) Handles btnBrick.Click, btnSand.Click, btnNothing.Click
        PenMode = If(sender Is btnBrick, 1, If(sender Is btnSand, 2, 0))
    End Sub

    Sub Canvas_CreateResources(sender As CanvasControl, args As Object) Handles canvas1.CreateResources
        Const defaultDpi = 96.0F
        surface1 = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi)
        surface2 = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi)
        renderEffect2.SourceDpi = New Vector2(canvas1.Dpi)
        App_Loaded()

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
        StepEffect(Kernel.Down, 0) = KernelTracker.MakeEffect(ingressDL, egressDL)


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
        StepEffect(Kernel.Down, 1) = KernelTracker.MakeEffect(ingressDR, egressDR)


    End Sub

    Sub App_Loaded() Handles App.Loaded
        If surface1 Is Nothing Then Return
        Dim c = New Color(App.pixels.Length - 1) {}
        For i = 0 To App.pixels.Length - 1
            c(i) = If(App.pixels(i) = 1, Colors.White, If(App.pixels(i) = 2, Colors.Gray, Colors.Black))
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


    Sub DoStep()
        Static Dim i As Integer = 0
        Using ds = surface2.CreateDrawingSession()
            ds.DrawImage(StepEffect(Kernel.Down, i).FromSource(surface1))
            i = 1 - i
        End Using
        Swap(surface1, surface2)

        canvas1.Invalidate()
    End Sub

    Sub Page_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Me.SizeChanged
        Dim isFullScreen = ApplicationView.GetForCurrentView.IsFullScreenMode
        Dim isTablet = (UIViewSettings.GetForCurrentView().UserInteractionMode = UserInteractionMode.Touch)

        Dim xform0 = GetDisplayTransform()
        renderEffect3.TransformMatrix = New Matrix3x2(xform0.Scale, 0, 0, xform0.Scale, xform0.Offset.X, xform0.Offset.Y)
        ' Should be Matrix3x2.CreateScale(scale) * Matrix3x2.CreateTranslation(offset) but there's currently a bug in x64 .NET Native

        btnRotateLeft.Visibility = (Not isTablet).AsVisibility
        btnRotateRight.Visibility = (Not isTablet).AsVisibility
        btnFullScreen.Visibility = (Not isTablet).AsVisibility
    End Sub


    Sub Canvas1_Draw(sender As CanvasControl, args As CanvasDrawEventArgs) Handles canvas1.Draw
        Dim currentTime = timer.Elapsed
        Dim elapsed = (currentTime - lastTime).TotalSeconds
        If elapsed > 1 / 60 Then
            DoStep() : DoStep() : DoStep()
            lastTime = currentTime
        End If

        renderEffect1.Source = surface1
        args.DrawingSession.DrawImage(renderEffect3)
        sender.Invalidate()
    End Sub


    Sub Canvas_Pointer(sender As Object, e As PointerRoutedEventArgs) Handles canvas1.PointerPressed, canvas1.PointerMoved
        If Not e.Pointer.IsInContact Then Return

        Dim xform = GetDisplayTransform()
        Dim canvasPointDips = e.GetCurrentPoint(canvas1).Position.ToVector2() - xform.Offset
        canvasPointDips /= canvas1.ConvertPixelsToDips(1)
        Dim sourcePointDips = canvasPointDips / xform.Scale
        Dim cx = CInt(Math.Floor(sourcePointDips.X))
        Dim cy = CInt(Math.Floor(sourcePointDips.Y))
        Dim radius = CType(btnBrick.Content, Border).ActualWidth / 2 / xform.Scale

        Dim b = CInt(radius * 2)
        Static Dim cols As Color()
        If cols Is Nothing OrElse cols.Length <> b * b - 1 Then
            cols = New Color(b * b - 1) {}
            Dim c = If(PenMode = 1, Colors.White, If(PenMode = 2, Colors.Gray, Colors.Black))
            For i = 0 To cols.Length - 1
                cols(i) = c
            Next
        End If
        surface1.SetPixelColors(cols, cx - b \ 2, cy - b \ 2, b, b)
        'For y = CInt(cy - radius) To CInt(cy + radius)
        '    For x = CInt(cx - radius) To CInt(cx + radius)
        '        If x < 0 OrElse x >= App.CWIDTH OrElse y < 0 OrElse y >= App.CHEIGHT Then Continue For
        '        If (y - cy) * (y - cy) + (x - cx) * (x - cx) > radius * radius Then Continue For
        '        Dim c = If(PenMode = 1, Colors.White, If(PenMode = 2, Colors.Gray, Colors.Black))
        '        surface1.SetPixelColors({c}, x, y, 1, 1)
        '    Next
        'Next

    End Sub

    Structure DisplayTransform
        Public Scale As Single
        Public Offset As Vector2
    End Structure

    Function GetDisplayTransform() As DisplayTransform
        Dim bitmapSizeDips As New Vector2(canvas1.ConvertPixelsToDips(App.CWIDTH), canvas1.ConvertPixelsToDips(App.CHEIGHT))
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
        Return New DisplayTransform With {.Scale = scale.X, .Offset = offset}
    End Function

End Class


Public Module Utils
    Public Function IntPow(x As Integer, y As Integer) As Integer
        Dim r = 1
        For i = 0 To y - 1
            r *= x
        Next
        Return r
    End Function

    Public Sub Swap(Of T)(ByRef x As T, ByRef y As T)
        Dim temp = x : x = y : y = temp
    End Sub

    <Extension>
    Public Function AsVisibility(b As Boolean) As Visibility
        Return If(b, Visibility.Visible, Visibility.Collapsed)
    End Function

    <Extension>
    Public Function Fmt(v As Vector2) As String
        Return $"<{v.X:0.0},{v.Y:0.0}>"
    End Function

    <Extension>
    Public Function Fmt(m As Matrix3x2) As String
        Return $"<{m.M11:0.0},{m.M12:0.0} / {m.M21:0.0},{m.M22:0.0} + {m.M31:0.0},{m.M32:0.0}"
    End Function

    <Extension>
    Async Sub FireAndForget(t As Task)
        Try
            Await t
        Catch ex As Exception
            Stop
        End Try
    End Sub

End Module

Class EffectWrapper
    Private EndEffect As ICanvasImage
    Private FixSources As Action(Of ICanvasImage)

    Sub New(endEffect As ICanvasImage, fixSources As Action(Of ICanvasImage))
        Me.EndEffect = endEffect
        Me.FixSources = fixSources
    End Sub

    Function FromSource(source As ICanvasImage) As ICanvasImage
        FixSources(source)
        Return EndEffect
    End Function
End Class

Class KernelTracker
    Private _Matrix As Single()
    Private _Touched As Boolean()
    '
    Public ConvolveMatrix As Single()
    Public ConvolveDivisor As Integer
    Public TransferTable As Single()

    Shared Function MakeEffect(src1 As KernelTracker, src2 As KernelTracker) As EffectWrapper
        Dim conv1 As New ConvolveMatrixEffect With {.KernelMatrix = src1.ConvolveMatrix, .Divisor = src1.ConvolveDivisor, .PreserveAlpha = True}
        Dim tran1 As New DiscreteTransferEffect With {.Source = conv1, .RedTable = src1.TransferTable, .GreenTable = src1.TransferTable, .BlueTable = src1.TransferTable}
        Dim conv2 As New ConvolveMatrixEffect With {.KernelMatrix = src2.ConvolveMatrix, .Divisor = src2.ConvolveDivisor, .PreserveAlpha = True}
        Dim tran2 As New DiscreteTransferEffect With {.Source = conv2, .RedTable = src2.TransferTable, .GreenTable = src2.TransferTable, .BlueTable = src2.TransferTable}

        Dim compo As New CompositeEffect With {.Mode = CanvasComposite.Add}
        compo.Sources.Add(tran1)
        compo.Sources.Add(tran2)

        Return New EffectWrapper(compo, Sub(s) If True Then conv1.Source = s : conv2.Source = s)
    End Function

    Shared Function Generate(values As IEnumerable(Of Single), touches As IEnumerable(Of Kernel), lambda As Func(Of KernelTracker, Single)) As KernelTracker
        touches = touches.Reverse() ' so that when authors write "most significant item first", it's reflected in the transfer table
        Dim values_Count = values.Count
        Dim k As New KernelTracker
        k._Touched = {False, False, False, False, False, False, False, False, False}
        k._Matrix = New Single(8) {}
        '
        Dim results As New List(Of Single)
        For i = 0 To IntPow(values_Count, touches.Count) - 1
            Dim v = i
            For Each t In touches
                k._Matrix(t) = values(v Mod values_Count)
                v \= values_Count
            Next
            Dim r = lambda(k)
            results.Add(r)
        Next
        '
        k.TransferTable = results.ToArray()
        k.ConvolveMatrix = New Single(8) {}
        k.ConvolveDivisor = 0
        Dim m = 1
        For Each t In touches
            k.ConvolveMatrix(t) = m
            k.ConvolveDivisor += m
            m *= values_Count
        Next
        '
        For i = Kernel._First To Kernel._Last
            If k._Touched(i) AndAlso Not touches.Contains(i) Then Throw New ArgumentException($"Failed to declare that this kernel touches {i}")
            If Not k._Touched(i) AndAlso touches.Contains(i) Then Throw New ArgumentException($"Declared that this kernel touches {i} but it doesn't")
        Next
        '
        Return k
    End Function

    Default Public ReadOnly Property Matrix(i As Kernel) As Single
        Get
            _Touched(i) = True
            Return _Matrix(i)
        End Get
    End Property
End Class

Enum Kernel
    _First = 0
    UpLeft = 8
    Up = 7
    UpRight = 6
    Left = 5
    Center = 4
    Right = 3
    DownLeft = 2
    Down = 1
    DownRight = 0
    _Last = 8
End Enum
