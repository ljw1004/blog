Imports System.Numerics
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Effects
Imports Microsoft.Graphics.Canvas.UI.Xaml
Imports Windows.UI

Public NotInheritable Class MainPage
    Inherits Page

    WithEvents canvas1 As CanvasControl
    Dim surface1, surface2 As CanvasRenderTarget
    Dim pointerColorLeft As Color = Colors.White
    Dim pointerColorRight As Color = Colors.Black

    Sub New()
        InitializeComponent()
        canvas1 = New CanvasControl
        content1.Child = canvas1
        WriteModelToUI()

        Dim prevtb As TextBox = Nothing
        For Each tb In effects1.GetDescendentsOfType(Of TextBox)
            AddHandler tb.TextChanged, Sub() ReadModelFromUI()
            AddHandler tb.GotFocus, Sub() tb.SelectAll()
            prevtb = tb
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
            AddHandler cb.Checked, Sub() ReadModelFromUI()
            AddHandler cb.Unchecked, Sub() ReadModelFromUI()
        Next
    End Sub

    Sub ReadModelFromUI()
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
    End Sub

    Sub Canvas_CreateResources(sender As CanvasControl, args As Object) Handles canvas1.CreateResources
        Const defaultDpi = 96.0F
        surface1 = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi)
        surface2 = New CanvasRenderTarget(canvas1, App.CWIDTH, App.CHEIGHT, defaultDpi)
        surface1.SetPixelColors(App.pixels, 0, 0, App.CWIDTH, App.CHEIGHT)
    End Sub

    Sub Canvas1_Draw(sender As CanvasControl, args As CanvasDrawEventArgs) Handles canvas1.Draw
        Dim convolveEffect As New ConvolveMatrixEffect With {.KernelMatrix = App.model.ConvolveMatrixKernel, .Divisor = App.model.ConvolveMatrixDivisor, .BorderMode = EffectBorderMode.Hard, .PreserveAlpha = True}
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



        Dim sourceSizeDips As New Vector2(canvas1.ConvertPixelsToDips(App.CWIDTH), canvas1.ConvertPixelsToDips(App.CHEIGHT))
        Dim canvasSizeDips As New Vector2(CSng(canvas1.ActualWidth), CSng(canvas1.ActualHeight))
        Dim scale = canvasSizeDips.X / sourceSizeDips.X

        Dim xform1 As New Matrix3x2(scale, 0, 0, scale, 0, 0)
        Dim dpi1 As New DpiCompensationEffect With {.Source = surface1, .SourceDpi = New Vector2(canvas1.Dpi)}
        Dim xformEffect1 As New Transform2DEffect With {.Source = dpi1, .TransformMatrix = xform1, .InterpolationMode = CanvasImageInterpolation.NearestNeighbor}
        args.DrawingSession.DrawImage(xformEffect1)

        Dim xform2 As New Matrix3x2(scale, 0, 0, scale, 0, (sourceSizeDips.Y + 1) * scale)
        Dim dpi2 As New DpiCompensationEffect With {.Source = surface2, .SourceDpi = New Vector2(canvas1.Dpi)}
        Dim xformEffect2 As New Transform2DEffect With {.Source = dpi2, .TransformMatrix = xform2, .InterpolationMode = CanvasImageInterpolation.NearestNeighbor}
        args.DrawingSession.DrawImage(xformEffect2)
    End Sub


    Sub Canvas_Pointer(sender As Object, e As PointerRoutedEventArgs) Handles canvas1.PointerPressed, canvas1.PointerMoved
        Dim sourceSizeDips As New Vector2(canvas1.ConvertPixelsToDips(App.CWIDTH), canvas1.ConvertPixelsToDips(App.CHEIGHT))
        Dim canvasSizeDips As New Vector2(CSng(canvas1.ActualWidth), CSng(canvas1.ActualHeight))
        Dim scale = canvasSizeDips.X / sourceSizeDips.X
        Dim canvasPointDips = e.GetCurrentPoint(canvas1).Position.ToVector2() / canvas1.ConvertPixelsToDips(1)
        Dim sourcePointDips = canvasPointDips / scale
        Dim x = CInt(Math.Floor(sourcePointDips.X))
        Dim y = CInt(Math.Floor(sourcePointDips.Y))
        If e.Pointer.IsInContact AndAlso x >= 0 AndAlso y >= 0 AndAlso x < App.CWIDTH AndAlso y < App.CHEIGHT Then
            Dim pointerColor = If(e.GetCurrentPoint(canvas1).Properties.IsRightButtonPressed, pointerColorRight, pointerColorLeft)
            App.pixels(y * App.CWIDTH + x) = pointerColor
            surface1.SetPixelColors({pointerColor}, x, y, 1, 1)
            canvas1.Invalidate()
        End If

        Dim c As Color? = Nothing
        If x >= 0 AndAlso y >= 0 AndAlso x < App.CWIDTH AndAlso y < App.CHEIGHT Then
            c = surface1.GetPixelColors(x, y, 1, 1).First
        ElseIf x >= 0 AndAlso y >= App.CHEIGHT + 1 AndAlso x < App.CWIDTH AndAlso y < App.CHEIGHT + 1 + App.CHEIGHT Then
            c = surface2.GetPixelColors(x, y - App.CHEIGHT - 1, 1, 1).First
        End If
        ptr1.Text = If(c.HasValue, $"#{c?.R:X2}{c?.G:X2}{c?.B:X2}", "")
        ptr2.Text = If(c.HasValue, $"rgb({c?.R}, {c?.G}, {c?.B})", "")
        ptr3.Text = If(c.HasValue, $"rgb({c?.R / 255:0.00}, {c?.G / 255:0.00}, {c?.B / 255:0.00})", "")

    End Sub

    Private Sub Rectangle_PointerPressed(sender As Object, e As PointerRoutedEventArgs)
        Dim r = CType(sender, Shapes.Rectangle)
        Dim b = CType(r.Fill, SolidColorBrush)
        If e.GetCurrentPoint(canvas1).Properties.IsRightButtonPressed Then
            pointerColorRight = b.Color
        Else
            pointerColorLeft = b.Color
        End If
    End Sub


End Class


Module Utils

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

End Module

