Imports System.Numerics
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Effects
Imports Microsoft.Graphics.Canvas.UI.Xaml
Imports Windows.UI

'DisplayInformation.NativeOrientation : Surface = Landscape, Phone=Portrait

'DispayInformation.Orientation:
'Surface can switch between Landscape, LandscapeFlipped, Portrait, PortraitFlipped
'Phone omits PortraitFlipped

'My bitmap Is always 640x480

'The NativeOrientation says what the "ground state" Is.
'...


Public NotInheritable Class MainPage
    Inherits Page

    Const CHEIGHT = 8
    Const CWIDTH = 8

    WithEvents canvas1 As CanvasControl
    Dim surfaceTruth1, surfaceTruth2 As CanvasRenderTarget
    Dim surfaceRender As CanvasRenderTarget
    Dim UseDL As Boolean = True

    Sub New()
        InitializeComponent()
        canvas1 = New CanvasControl With {.Width = 300, .Height = 300}
        container1.Children.Add(canvas1)
    End Sub


    Sub Canvas_CreateResources(sender As CanvasControl, args As Object) Handles canvas1.CreateResources
        Const defaultDpi = 96.0F
        surfaceTruth1 = New CanvasRenderTarget(canvas1, CWIDTH, CHEIGHT, defaultDpi)
        surfaceTruth2 = New CanvasRenderTarget(canvas1, CWIDTH, CHEIGHT, defaultDpi)
        surfaceRender = New CanvasRenderTarget(canvas1, CWIDTH, CHEIGHT, defaultDpi)
        surfaceTruth1.SetPixelColors(Enumerable.Repeat(Colors.Black, CWIDTH * CHEIGHT).ToArray, 0, 0, CWIDTH, CHEIGHT)

        surfaceTruth1.SetPixelColors({Colors.Red, Colors.Green, Colors.Blue, Colors.White}, 0, 0, 4, 1)
        surfaceTruth1.SetPixelColors({Colors.White, Colors.LightGray, Colors.DarkGray, Colors.DarkSlateBlue}, 0, 1, 4, 1)
        surfaceTruth1.SetPixelColors({Colors.White}, 2, 2, 1, 1)
        surfaceTruth1.SetPixelColors({Colors.Gray}, 5, 2, 1, 1)

        UpdateRender()
    End Sub

    Sub UpdateRender()
        Using ds = surfaceRender.CreateDrawingSession()
            ds.DrawImage(surfaceTruth1)
        End Using
    End Sub

    Sub DoStep() Handles buttonDown.Click

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
        Dim effectDL = KernelTracker.MakeEffect(ingressDL, egressDL)


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
        Dim effectDR = KernelTracker.MakeEffect(ingressDR, egressDR)


        Using ds = surfaceTruth2.CreateDrawingSession()
            ds.DrawImage(If(UseDL, effectDL, effectDR).FromSource(surfaceTruth1))
            UseDL = Not UseDL
        End Using
        Swap(surfaceTruth1, surfaceTruth2)
        UpdateRender()

        canvas1.Invalidate()
    End Sub


    Sub Canvas1_Draw(sender As CanvasControl, args As CanvasDrawEventArgs) Handles canvas1.Draw
        Dim sourceSizeDips As New Vector2(canvas1.ConvertPixelsToDips(CWIDTH), canvas1.ConvertPixelsToDips(CHEIGHT))
        Dim canvasSizeDips As New Vector2(CSng(canvas1.ActualWidth), CSng(canvas1.ActualHeight))
        Dim scale = canvasSizeDips / sourceSizeDips

        Dim effect1 As New DiscreteTransferEffect With {.Source = surfaceTruth1, .RedTable = {0, 0, 1}, .GreenTable = {0, 0.5, 1}, .BlueTable = {0, 0.8, 1}}
        Dim effect2 As New DpiCompensationEffect With {.Source = effect1, .SourceDpi = New Vector2(canvas1.Dpi)}
        Dim effect3 As New Transform2DEffect With {.Source = effect2, .TransformMatrix = Matrix3x2.CreateScale(scale), .InterpolationMode = CanvasImageInterpolation.NearestNeighbor}
        args.DrawingSession.DrawImage(effect3)
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
            Dim c = surfaceTruth1.GetPixelColors(x, y, 1, 1).First
            If e.GetCurrentPoint(canvas1).Properties.IsRightButtonPressed Then
                c = Colors.Black
            ElseIf c = Colors.Gray Then
                c = Colors.White
            Else
                c = Colors.Gray
            End If
            surfaceTruth1.SetPixelColors({c}, x, y, 1, 1)
            UpdateRender()
            canvas1.Invalidate()
        End If
    End Sub

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
