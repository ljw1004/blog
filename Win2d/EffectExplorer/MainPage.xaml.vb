Imports System.Numerics
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Effects
Imports Microsoft.Graphics.Canvas.UI.Xaml
Imports Windows.Graphics.DirectX
Imports Windows.UI

Public NotInheritable Class MainPage
    Inherits Page

    Dim surface1, surface2 As CanvasRenderTarget
    Dim pointerColorLeft As ColorF = ColorF.White
    Dim pointerColorRight As ColorF = ColorF.Black
    Dim updateInProgress As Boolean = False
    WithEvents App As App = App.Current

    Sub New()
        InitializeComponent()

        For Each tb In effects1.GetDescendentsOfType(Of TextBox)
            If tb Is ptr1 OrElse tb Is ptr2 OrElse tb Is ptr3 OrElse tb Is ptr4 Then Continue For
            AddHandler tb.TextChanged, Sub() RespondToUIChange()
            AddHandler tb.GotFocus, Sub() tb.SelectAll()
        Next
        Dim lambda = Sub(e As KeyRoutedEventArgs, c As TextBox)
                         If e.Key < Windows.System.VirtualKey.Number0 Then Return
                         If e.Key > Windows.System.VirtualKey.Number9 Then Return
                         c.Focus(FocusState.Keyboard)
                         e.Handled = True
                     End Sub
        AddHandler cmk0.KeyUp, Sub(s, e) lambda(e, cmk1)
        AddHandler cmk1.KeyUp, Sub(s, e) lambda(e, cmk2)
        AddHandler cmk2.KeyUp, Sub(s, e) lambda(e, cmk3)
        AddHandler cmk3.KeyUp, Sub(s, e) lambda(e, cmk4)
        AddHandler cmk4.KeyUp, Sub(s, e) lambda(e, cmk5)
        AddHandler cmk5.KeyUp, Sub(s, e) lambda(e, cmk6)
        AddHandler cmk6.KeyUp, Sub(s, e) lambda(e, cmk7)
        AddHandler cmk7.KeyUp, Sub(s, e) lambda(e, cmk8)
        For Each cb In effects1.GetDescendentsOfType(Of CheckBox)
            AddHandler cb.Checked, Sub() RespondToUIChange()
            AddHandler cb.Unchecked, Sub() RespondToUIChange()
        Next
    End Sub

    Sub App_Loaded() Handles App.Loaded
        If surface1 IsNot Nothing Then surface1.SetPixelColorFs(App.pixels)
        WriteModelToUI()
    End Sub

    Sub RespondToUIChange()
        If updateInProgress Then Return
        App.model.ConvolveMatrixEnabled = cmx.IsChecked.GetValueOrDefault
        App.model.ConvolveMatrixKernel = {cmk8.Sng, cmk7.Sng, cmk6.Sng, cmk5.Sng, cmk4.Sng, cmk3.Sng, cmk2.Sng, cmk1.Sng, cmk0.Sng}
        App.model.ConvolveMatrixDivisor = cmd.Int
        App.model.DiscreteTransferEnabled = dtx.IsChecked.GetValueOrDefault
        Dim t2ss = Function(tt As String)
                       Dim ttarr = tt.Replace(",", " ").Split({" "c}, StringSplitOptions.RemoveEmptyEntries)
                       Dim ss = ttarr.Select(Function(t)
                                                 Dim s = 0F
                                                 Single.TryParse(t, s)
                                                 Return s
                                             End Function)
                       Return ss.ToArray()
                   End Function
        App.model.DiscreteTransferTableRed = t2ss(dtr.Text)
        App.model.DiscreteTransferTableGreen = t2ss(dtg.Text)
        App.model.DiscreteTransferTableBlue = t2ss(dtb.Text)
        canvas1.Invalidate()
    End Sub

    Sub WriteModelToUI()
        updateInProgress = True
        cmx.IsChecked = App.model.ConvolveMatrixEnabled
        cmk0.Text = App.model.ConvolveMatrixKernel(8).ToString()
        cmk1.Text = App.model.ConvolveMatrixKernel(7).ToString()
        cmk2.Text = App.model.ConvolveMatrixKernel(6).ToString()
        cmk3.Text = App.model.ConvolveMatrixKernel(5).ToString()
        cmk4.Text = App.model.ConvolveMatrixKernel(4).ToString()
        cmk5.Text = App.model.ConvolveMatrixKernel(3).ToString()
        cmk6.Text = App.model.ConvolveMatrixKernel(2).ToString()
        cmk7.Text = App.model.ConvolveMatrixKernel(1).ToString()
        cmk8.Text = App.model.ConvolveMatrixKernel(0).ToString()
        cmd.Text = App.model.ConvolveMatrixDivisor.ToString()
        dtx.IsChecked = App.model.DiscreteTransferEnabled
        dtr.Text = String.Join(" ", App.model.DiscreteTransferTableRed.Select(Function(s) s.ToString()))
        dtg.Text = String.Join(" ", App.model.DiscreteTransferTableGreen.Select(Function(s) s.ToString()))
        dtb.Text = String.Join(" ", App.model.DiscreteTransferTableBlue.Select(Function(s) s.ToString()))
        updateInProgress = False
        canvas1.Invalidate()
    End Sub

    Sub Preset_Click(sender As Object, e As RoutedEventArgs) Handles pre_gameoflife.Click, pre_blur.Click, pre_fallingsand.Click
        If sender Is pre_gameoflife Then
            For i = 0 To App.pixels.Length - 1 : App.pixels(i) = ColorF.Black : Next
            App.pixels(1 * App.SIMULATION_SIZE + 2) = ColorF.White
            App.pixels(2 * App.SIMULATION_SIZE + 3) = ColorF.White
            App.pixels(3 * App.SIMULATION_SIZE + 1) = ColorF.White
            App.pixels(3 * App.SIMULATION_SIZE + 2) = ColorF.White
            App.pixels(3 * App.SIMULATION_SIZE + 3) = ColorF.White
            surface1.SetPixelColorFs(App.pixels)
            '
            App.model.ConvolveMatrixEnabled = True
            App.model.ConvolveMatrixKernel = {2, 2, 2, 2, 1, 2, 2, 2, 2}
            App.model.ConvolveMatrixDivisor = 17
            App.model.DiscreteTransferEnabled = True
            Dim table As Single() = {0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            App.model.DiscreteTransferTableRed = table
            App.model.DiscreteTransferTableGreen = table
            App.model.DiscreteTransferTableBlue = table
            WriteModelToUI()

        ElseIf sender Is pre_blur Then
            For i = 0 To App.pixels.Length - 1 : App.pixels(i) = ColorF.Black : Next
            App.pixels(2 * App.SIMULATION_SIZE + 3) = ColorF.White
            App.pixels(2 * App.SIMULATION_SIZE + 4) = ColorF.White
            App.pixels(3 * App.SIMULATION_SIZE + 3) = ColorF.Red
            App.pixels(3 * App.SIMULATION_SIZE + 4) = ColorF.White
            surface1.SetPixelColorFs(App.pixels)
            '
            App.model.ConvolveMatrixEnabled = True
            App.model.ConvolveMatrixKernel = {1, 2, 1, 2, 4, 2, 1, 2, 1}
            App.model.ConvolveMatrixDivisor = 16
            App.model.DiscreteTransferEnabled = False
            Dim table As Single() = {0, 1}
            App.model.DiscreteTransferTableRed = table
            App.model.DiscreteTransferTableGreen = table
            App.model.DiscreteTransferTableBlue = table
            WriteModelToUI()

        ElseIf sender Is pre_fallingsand Then
            Dim kt = KernelTracker.Generate({0, 0.5, 1},
                                        {Kernel.Center, Kernel.Up, Kernel.UpRight, Kernel.Right, Kernel.Down, Kernel.DownLeft},
                                        Function(k)
                                            ' Brick:
                                            If k(Kernel.Center) = 1 Then Return 1
                                            ' Blank space that might get filled with sand:
                                            If k(Kernel.Center) = 0 AndAlso k(Kernel.Up) = 0.5 Then Return 0.5
                                            If k(Kernel.Center) = 0 AndAlso k(Kernel.UpRight) = 0.5 AndAlso k(Kernel.Right) <> 0 Then Return 0.5
                                            If k(Kernel.Center) = 0 Then Return 0
                                            ' Sand that might empty out below:
                                            If k(Kernel.Center) = 0.5 AndAlso k(Kernel.Down) = 0 Then Return 0
                                            If k(Kernel.Center) = 0.5 AndAlso k(Kernel.DownLeft) = 0 Then Return 0
                                            If k(Kernel.Center) = 0.5 Then Return 0.5
                                            Throw New Exception("unreachable code")
                                        End Function)
            '
            For i = 0 To App.pixels.Length - 1 : App.pixels(i) = ColorF.Black : Next
            App.pixels(0 * App.SIMULATION_SIZE + 4) = ColorF.Gray
            App.pixels(2 * App.SIMULATION_SIZE + 4) = ColorF.White
            App.pixels(2 * App.SIMULATION_SIZE + 5) = ColorF.White
            App.pixels(4 * App.SIMULATION_SIZE + 3) = ColorF.White
            App.pixels(4 * App.SIMULATION_SIZE + 4) = ColorF.White
            surface1.SetPixelColorFs(App.pixels)
            '
            App.model.ConvolveMatrixEnabled = True
            App.model.ConvolveMatrixKernel = kt.ConvolveMatrix
            App.model.ConvolveMatrixDivisor = kt.ConvolveDivisor
            App.model.DiscreteTransferEnabled = True
            App.model.DiscreteTransferTableRed = kt.TransferTable
            App.model.DiscreteTransferTableGreen = kt.TransferTable
            App.model.DiscreteTransferTableBlue = kt.TransferTable
            WriteModelToUI()
        End If
    End Sub

    Private Sub Iterate_Click(sender As Object, e As RoutedEventArgs) Handles Iterate.Click
        App.pixels = surface2.GetPixelColorFs()
        For i = 0 To App.pixels.Length - 1
            App.pixels(i).A = 1
        Next
        surface1.SetPixelColorFs(App.pixels)
        canvas1.Invalidate()
    End Sub

    Sub Canvas_CreateResources(sender As CanvasControl, args As Object) Handles canvas1.CreateResources
        Const defaultDpi = 96.0F
        Try
            surface1 = New CanvasRenderTarget(canvas1, App.SIMULATION_SIZE, App.SIMULATION_SIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
            surface2 = New CanvasRenderTarget(canvas1, App.SIMULATION_SIZE, App.SIMULATION_SIZE, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
        Catch ex As Exception
            surface1 = New CanvasRenderTarget(canvas1, App.SIMULATION_SIZE, App.SIMULATION_SIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
            surface2 = New CanvasRenderTarget(canvas1, App.SIMULATION_SIZE, App.SIMULATION_SIZE, defaultDpi, DirectXPixelFormat.R16G16B16A16Float, CanvasAlphaMode.Ignore)
        End Try
        surface1.SetPixelColorFs(App.pixels)
    End Sub

    Sub Canvas1_Draw(sender As CanvasControl, args As CanvasDrawEventArgs) Handles canvas1.Draw
        Dim convolveEffect As New ConvolveMatrixEffect With {.KernelMatrix = App.model.ConvolveMatrixKernel, .Divisor = App.model.ConvolveMatrixDivisor, .BorderMode = EffectBorderMode.Soft, .PreserveAlpha = True}
        Dim transferEffect As New DiscreteTransferEffect With {.RedTable = App.model.DiscreteTransferTableRed, .GreenTable = App.model.DiscreteTransferTableGreen, .BlueTable = App.model.DiscreteTransferTableBlue}
        '
        Dim updateEffect As ICanvasImage = Nothing
        If App.model.ConvolveMatrixEnabled AndAlso App.model.DiscreteTransferEnabled Then
            convolveEffect.Source = surface1 : transferEffect.Source = convolveEffect : updateEffect = transferEffect
        ElseIf App.model.ConvolveMatrixEnabled AndAlso Not App.model.DiscreteTransferEnabled Then
            convolveEffect.Source = surface1 : updateEffect = convolveEffect
        ElseIf Not App.model.ConvolveMatrixEnabled AndAlso App.model.DiscreteTransferEnabled Then
            transferEffect.Source = surface1 : updateEffect = transferEffect
        ElseIf Not App.model.ConvolveMatrixEnabled AndAlso Not App.model.DiscreteTransferEnabled Then
            updateEffect = surface1
        End If
        '
        Try
            Using ds = surface2.CreateDrawingSession()
                ds.DrawImage(updateEffect)
            End Using
            status1.Text = ""
        Catch ex As Exception
            status1.Text = ex.Message
        End Try



        Dim sourceSizeDips As New Vector2(canvas1.ConvertPixelsToDips(App.SIMULATION_SIZE), canvas1.ConvertPixelsToDips(App.SIMULATION_SIZE))
        Dim canvasSizeDips As New Vector2(CSng(canvas1.ActualWidth), CSng(canvas1.ActualHeight))
        Dim scale = canvasSizeDips.X / sourceSizeDips.X

        Dim xform1 As New Matrix3x2(scale, 0, 0, scale, 0, 0)
        Dim dpi1 As New DpiCompensationEffect With {.Source = surface1, .SourceDpi = New Vector2(canvas1.Dpi)}
        Dim xformEffect1 As New Transform2DEffect With {.Source = dpi1, .TransformMatrix = xform1, .InterpolationMode = CanvasImageInterpolation.NearestNeighbor}
        args.DrawingSession.DrawImage(xformEffect1)

        Dim xform2 As New Matrix3x2(scale, 0, 0, scale, 0, canvas1.ConvertPixelsToDips(App.SIMULATION_SIZE + 1) * scale)
        Dim dpi2 As New DpiCompensationEffect With {.Source = surface2, .SourceDpi = New Vector2(canvas1.Dpi)}
        Dim xformEffect2 As New Transform2DEffect With {.Source = dpi2, .TransformMatrix = xform2, .InterpolationMode = CanvasImageInterpolation.NearestNeighbor}
        args.DrawingSession.DrawImage(xformEffect2)

        ' Draw gridlines
        Dim rule = Color.FromArgb(80, 128, 128, 128)
        Dim lowerY = canvasSizeDips.X * CSng(1 + 1 / App.SIMULATION_SIZE)
        For i = 0 To App.SIMULATION_SIZE
            Dim f = CSng(i / App.SIMULATION_SIZE * canvasSizeDips.X)
            args.DrawingSession.DrawLine(0, f, canvasSizeDips.X, f, rule)
            args.DrawingSession.DrawLine(f, 0, f, canvasSizeDips.X, rule)
            args.DrawingSession.DrawLine(0, f + lowerY, canvasSizeDips.X, f + lowerY, rule)
            args.DrawingSession.DrawLine(f, lowerY, f, lowerY + canvasSizeDips.X, rule)
        Next
    End Sub


    Sub Canvas_Pointer(sender As Object, e As PointerRoutedEventArgs) Handles canvas1.PointerPressed, canvas1.PointerMoved
        Dim sourceSizeDips As New Vector2(canvas1.ConvertPixelsToDips(App.SIMULATION_SIZE), canvas1.ConvertPixelsToDips(App.SIMULATION_SIZE))
        Dim canvasSizeDips As New Vector2(CSng(canvas1.ActualWidth), CSng(canvas1.ActualHeight))
        Dim scale = canvasSizeDips.X / sourceSizeDips.X
        Dim canvasPointDips = e.GetCurrentPoint(canvas1).Position.ToVector2() / canvas1.ConvertPixelsToDips(1)
        Dim sourcePointDips = canvasPointDips / scale
        Dim x = CInt(Math.Floor(sourcePointDips.X))
        Dim y = CInt(Math.Floor(sourcePointDips.Y))
        If e.Pointer.IsInContact AndAlso x >= 0 AndAlso y >= 0 AndAlso x < App.SIMULATION_SIZE AndAlso y < App.SIMULATION_SIZE Then
            Dim pointerColor = If(e.GetCurrentPoint(canvas1).Properties.IsRightButtonPressed, pointerColorRight, pointerColorLeft)
            App.pixels(y * App.SIMULATION_SIZE + x) = pointerColor
            surface1.SetPixelColorFs({pointerColor}, x, y, 1, 1)
            canvas1.Invalidate()
        End If

        Dim cf As ColorF? = Nothing
        If x >= 0 AndAlso y >= 0 AndAlso x < App.SIMULATION_SIZE AndAlso y < App.SIMULATION_SIZE Then
            cf = surface1.GetPixelColorFs(x, y, 1, 1).First
        ElseIf x >= 0 AndAlso y >= App.SIMULATION_SIZE + 1 AndAlso x < App.SIMULATION_SIZE AndAlso y < App.SIMULATION_SIZE + 1 + App.SIMULATION_SIZE Then
            cf = surface2.GetPixelColorFs(x, y - App.SIMULATION_SIZE - 1, 1, 1).First
        End If
        Dim c = CType(cf, Color?)
        ptr1.Text = If(c.HasValue, $"#{c?.R:X2}{c?.G:X2}{c?.B:X2}", "")
        ptr2.Text = If(cf.HasValue, $"r={cf?.R}", "")
        ptr3.Text = If(cf.HasValue, $"g={cf?.G}", "")
        ptr4.Text = If(cf.HasValue, $"b={cf?.B}", "")
    End Sub

    Private Sub Rectangle_PointerPressed(sender As Object, e As PointerRoutedEventArgs)
        Dim r = CType(sender, Shapes.Rectangle)
        Dim b = CType(r.Fill, SolidColorBrush)
        Dim c = b.Color
        ' "c" is an 8bit value, e.g. #808080
        ' In case the user is doing fancy stuff, we'll silently transform #80 (=0.5019) into 0.5
        Dim cf As ColorF = c
        If c.A = 128 Then cf.A = 0.5
        If c.R = 128 Then cf.R = 0.5
        If c.G = 128 Then cf.G = 0.5
        If c.B = 128 Then cf.B = 0.5
        If e.GetCurrentPoint(canvas1).Properties.IsRightButtonPressed Then
            pointerColorRight = cf
        Else
            pointerColorLeft = cf
        End If
    End Sub


End Class


Module Utils

    <Extension>
    Async Function ExceptionsToNull(Of T)(this As Task(Of T)) As Task(Of T)
        Try
            Return Await this
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    <Extension>
    Async Sub FireAndForget(t As Task)
        Try
            Await t
        Catch ex As Exception
            Debug.WriteLine("OOPS! AN UNEXPECTED EXCEPTION OCCURED")
            Debug.WriteLine(ex.Message)
            Stop
        End Try
    End Sub

    <Extension>
    Sub FireAndForget(t As IAsyncAction)
        FireAndForget(t.AsTask)
    End Sub

    <Extension>
    Function GetDescendentsOfType(Of T As DependencyObject)(start As DependencyObject) As IEnumerable(Of T)
        Return start.GetDescendants().OfType(Of T)
    End Function

    <Extension>
    Iterator Function GetDescendants(start As DependencyObject) As IEnumerable(Of DependencyObject)
        If start Is Nothing Then Return

        Dim q As New Queue(Of DependencyObject)
        Dim popup = TryCast(start, Popup)
        If popup IsNot Nothing Then
            If popup.Child IsNot Nothing Then
                q.Enqueue(popup.Child)
                Yield popup.Child
            End If
        Else
            Dim count = VisualTreeHelper.GetChildrenCount(start)
            For i = 0 To count - 1
                Dim child = VisualTreeHelper.GetChild(start, i)
                q.Enqueue(child)
                Yield child
            Next
        End If

        While q.Count > 0
            Dim parent = q.Dequeue()
            popup = TryCast(parent, Popup)
            If popup IsNot Nothing Then
                If popup.Child IsNot Nothing Then
                    q.Enqueue(popup.Child)
                    Yield popup.Child
                End If
            Else
                Dim count = VisualTreeHelper.GetChildrenCount(parent)
                For i = 0 To count - 1
                    Dim child = VisualTreeHelper.GetChild(parent, i)
                    Yield child
                    q.Enqueue(child)
                Next
            End If
        End While
    End Function

    <Extension>
    Function Sng(t As TextBox) As Single
        Dim s = 0F
        Single.TryParse(t.Text, s)
        Return s
    End Function

    <Extension>
    Function Int(t As TextBox) As Integer
        Dim i = 0
        Integer.TryParse(t.Text, i)
        Return i
    End Function

    Public Function IntPow(x As Integer, y As Integer) As Integer
        Dim r = 1
        For i = 0 To y - 1
            r *= x
        Next
        Return r
    End Function

End Module


Public Class KernelTracker
    Public _Matrix As Single()
    Public _Touched As Boolean()
    '
    Public ConvolveDivisor As Integer
    Public TransferTable As Single()
    Public ConvolveMatrix As Single()

    Shared Function Generate(values As IEnumerable(Of Single), touches As IEnumerable(Of Kernel), lambda As Func(Of Func(Of Integer, Single), Single)) As KernelTracker
        touches = touches.Reverse() ' so that when authors write "most significant item first", it's reflected in the transfer table
        Dim values_Count = values.Count
        Dim k As New KernelTracker
        k._Touched = {False, False, False, False, False, False, False, False, False}
        k._Matrix = New Single(8) {}
        Dim getMatrixLambda = Function(pos As Integer)
                                  k._Touched(pos) = True
                                  Return k._Matrix(pos)
                              End Function
        '
        Dim results As New List(Of Single)
        For i = 0 To IntPow(values_Count, touches.Count) - 1
            Dim v = i
            For Each t In touches
                k._Matrix(t) = values(v Mod values_Count)
                v \= values_Count
            Next
            Dim r = lambda(getMatrixLambda)
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
        If k.ConvolveDivisor > 20 Then Debug.WriteLine("WARNING! This kernel Is complicated. It likely won't work on DX9 devices such as Lumia635 which just don't guarantee the necessary GPU precision")
        For i = Kernel._First To Kernel._Last
            If k._Touched(i) AndAlso Not touches.Contains(i) Then Throw New ArgumentException($"Failed to declare that this kernel touches {i}")
            If Not k._Touched(i) AndAlso touches.Contains(i) Then Throw New ArgumentException($"Declared that this kernel touches {i} but it doesn't")
        Next
        Return k
    End Function

End Class

Public Enum Kernel
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
