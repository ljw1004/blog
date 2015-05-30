Imports System.Numerics
Imports System.Text
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Effects
Imports Windows.Graphics.DirectX
Imports Windows.Graphics.Effects
Imports Windows.Storage
Imports Windows.UI

NotInheritable Class App
    Inherits Application

    Public Const CHEIGHT = 6
    Public Const CWIDTH = 6
    Public Property Pixels As Byte()

    Public Event Loaded As Action
    Public Event Unloading As Action

    Public Shared Shadows ReadOnly Property Current As App
        Get
            Return CType(Application.Current, App)
        End Get
    End Property

    Protected Overrides Sub OnLaunched(e As Windows.ApplicationModel.Activation.LaunchActivatedEventArgs)
        Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)
        If rootFrame Is Nothing Then
            rootFrame = New Frame()
            Window.Current.Content = rootFrame
            Pixels = New Byte(CHEIGHT * CWIDTH - 1) {}
            LoadAsync().FireAndForget()
        End If
        If rootFrame.Content Is Nothing Then rootFrame.Navigate(GetType(MainPageMinimal), e.Arguments)
        Window.Current.Activate()
    End Sub

    Async Function LoadAsync() As Task
        Dim fn = $"pixels_{CWIDTH}x{CHEIGHT}.dat"
        Dim file = Await TryGetFileAsync(ApplicationData.Current.LocalFolder, fn)
        If file Is Nothing Then file = Await TryGetFileAsync(Package.Current.InstalledLocation, fn)
        If file IsNot Nothing Then
            Using stream = Await file.OpenStreamForReadAsync()
                Dim red = Await stream.ReadAsync(Pixels, 0, Pixels.Length)
                If red <> Pixels.Length Then Stop
            End Using
        End If
        RaiseEvent Loaded()
    End Function

    Private Async Function TryGetFileAsync(folder As StorageFolder, fn As String) As Task(Of StorageFile)
        Try
            Return Await folder.GetFileAsync(fn)
        Catch ex As FileNotFoundException
            Return Nothing
        End Try
    End Function

    Private Async Sub OnSuspending(sender As Object, e As SuspendingEventArgs) Handles Me.Suspending
        Dim deferral = e.SuspendingOperation.GetDeferral()
        RaiseEvent Unloading()
        Dim file = Await ApplicationData.Current.LocalFolder.CreateFileAsync($"pixels_{CWIDTH}x{CHEIGHT}.dat", CreationCollisionOption.ReplaceExisting)
        Using stream = Await file.OpenStreamForWriteAsync()
            Await stream.WriteAsync(Pixels, 0, Pixels.Count)
        End Using
        deferral.Complete()
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

    <Extension>
    Function Invert(m As Matrix3x2) As Matrix3x2
        Dim r As Matrix3x2
        Dim b = Matrix3x2.Invert(m, r)
        If Not b Then Throw New ArgumentException(NameOf(m))
        Return r
    End Function

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

    Function BitConverter_ToHalf(buf As Byte(), offset As Integer) As Single
        Dim hbits = CUInt(buf(offset)) Or (CUInt(buf(offset + 1)) << 8)
        Dim sign = hbits And &H8000
        Dim mant = hbits And &H03FF
        Dim exp = hbits And &H7C00
        If exp = &H7C00 Then
            exp = &H3FC00 ' NaN/Inf
        ElseIf exp <> 0 Then ' normal
            exp += &H1C000
        ElseIf exp = 0 AndAlso mant <> 0 Then ' subnormal
            exp = &H1C400
            Do
                mant = mant << 1
                exp -= &H400
            Loop While (mant And &H400) = 0
        Else
            ' +/- 0
        End If
        '
        Dim hbits2 = sign << 16 Or exp << 13 Or mant << 13
        Return BitConverter.ToSingle(BitConverter.GetBytes(hbits2), 0)
    End Function

    Function BitConverter_GetHalfBytes(value As Single) As Byte()
        If value < 0 Then Throw New ArgumentOutOfRangeException(NameOf(value), "negatives not implemented")
        Dim fbits = BitConverter.ToUInt32(BitConverter.GetBytes(CSng(value)), 0)
        Dim val = (fbits And &H7FFFFFFFUI) + &H1000
        If val >= &H47800000 Then Throw New ArgumentOutOfRangeException(NameOf(value), "NaN/Inf/overflow not implemented")
        If val >= &H38800000 Then Return BitConverter.GetBytes(CUShort((val - &H38000000) >> 13))
        If val < &H33000000 Then Return {0, 0}
        Throw New ArgumentOutOfRangeException(NameOf(value), "subnormals not implemented")
    End Function

    <Extension>
    Sub SetPixelColorFs(bmp As CanvasRenderTarget, cols As ColorF(), left As Integer, top As Integer, width As Integer, height As Integer)
        If bmp.Description.Format = DirectXPixelFormat.B8G8R8A8UIntNormalized Then
            Dim buf = cols.Select(Function(c) CType(c, Color)).ToArray
            bmp.SetPixelColors(buf, left, top, width, height)
        ElseIf bmp.Description.Format = DirectXPixelFormat.R32G32B32A32Float Then
            Dim buf = New Byte(cols.Length * 16 - 1) {}
            For i = 0 To cols.Length - 1
                Array.Copy(BitConverter.GetBytes(cols(i).R), 0, buf, i * 16 + 0, 4)
                Array.Copy(BitConverter.GetBytes(cols(i).G), 0, buf, i * 16 + 4, 4)
                Array.Copy(BitConverter.GetBytes(cols(i).B), 0, buf, i * 16 + 8, 4)
                Array.Copy(BitConverter.GetBytes(cols(i).A), 0, buf, i * 16 + 12, 4)
            Next
            bmp.SetPixelBytes(buf, left, top, width, height)
        ElseIf bmp.Description.Format = DirectXPixelFormat.R16G16B16A16Float Then
            Dim buf = New Byte(cols.Length * 8 - 1) {}
            For i = 0 To cols.Length - 1
                Array.Copy(BitConverter_GetHalfBytes(cols(i).R), 0, buf, i * 8 + 0, 2)
                Array.Copy(BitConverter_GetHalfBytes(cols(i).G), 0, buf, i * 8 + 2, 2)
                Array.Copy(BitConverter_GetHalfBytes(cols(i).B), 0, buf, i * 8 + 4, 2)
                Array.Copy(BitConverter_GetHalfBytes(cols(i).A), 0, buf, i * 8 + 6, 2)
            Next
            bmp.SetPixelBytes(buf, left, top, width, height)
        Else
            Throw New ArgumentException(NameOf(bmp.Description.Format))
        End If


    End Sub

    <Extension>
    Function GetPixelColorFs(bmp As CanvasRenderTarget, left As Integer, top As Integer, width As Integer, height As Integer) As ColorF()
        Dim c = New ColorF(width * height - 1) {}
        If bmp.Description.Format = DirectXPixelFormat.B8G8R8A8UIntNormalized Then
            Dim buf = bmp.GetPixelColors()
            For i = 0 To c.Length - 1
                c(i) = buf(i)
            Next
        ElseIf bmp.Description.Format = DirectXPixelFormat.R32G32B32A32Float Then
            Dim buf = bmp.GetPixelBytes()
            For i = 0 To c.Length - 1
                c(i).R = BitConverter.ToSingle(buf, i * 16 + 0)
                c(i).G = BitConverter.ToSingle(buf, i * 16 + 4)
                c(i).B = BitConverter.ToSingle(buf, i * 16 + 8)
                c(i).A = BitConverter.ToSingle(buf, i * 16 + 12)
            Next
        ElseIf bmp.Description.Format = DirectXPixelFormat.R16G16B16A16Float Then
            Dim buf = bmp.GetPixelBytes()
            For i = 0 To c.Length - 1
                c(i).R = BitConverter_ToHalf(buf, i * 8 + 0)
                c(i).G = BitConverter_ToHalf(buf, i * 8 + 2)
                c(i).B = BitConverter_ToHalf(buf, i * 8 + 4)
                c(i).A = BitConverter_ToHalf(buf, i * 8 + 6)
            Next
        Else
            Throw New ArgumentException(NameOf(bmp.Description.Format))
        End If
        Return c
    End Function
End Module


Public Class EffectWrapper(Of T)
    Private EndEffect As ICanvasImage
    Private FixSources As Action(Of T)

    Sub New(endEffect As ICanvasImage, fixSources As Action(Of T))
        Me.EndEffect = endEffect
        Me.FixSources = fixSources
    End Sub

    Function Update(value1 As T) As ICanvasImage
        FixSources(value1)
        Return EndEffect
    End Function

End Class


Public Class KernelTracker
    Public _Matrix As Single()
    Public _Touched As Boolean()
    '
    Public ConvolveMatrix As Single()
    Public ConvolveDivisor As Integer
    Public TransferTable As Single()

    Private Shared Function Rotate(m As Single(), o As Orientation) As Single()
        If m.Length <> 9 Then Throw New ArgumentOutOfRangeException("m", "Only works with 3x3 matrices")
        Select Case o
            Case Orientation.Landscape : Return {m(0), m(1), m(2), m(3), m(4), m(5), m(6), m(7), m(8)}
            Case Orientation.Portrait : Return {m(2), m(5), m(8), m(1), m(4), m(7), m(0), m(3), m(6)}
            Case Orientation.LandscapeFlipped : Return {m(8), m(7), m(6), m(5), m(4), m(3), m(2), m(1), m(0)}
            Case Orientation.PortraitFlipped : Return {m(6), m(3), m(0), m(7), m(4), m(1), m(8), m(5), m(2)}
            Case Else : Throw New ArgumentOutOfRangeException("o")
        End Select
    End Function

    Shared Function MakeEffect(src1 As KernelTracker, src2 As KernelTracker, o As Orientation) As EffectWrapper(Of ICanvasImage)
        Dim conv1 As New ConvolveMatrixEffect With {.KernelMatrix = Rotate(src1.ConvolveMatrix, o), .Divisor = src1.ConvolveDivisor, .PreserveAlpha = True}
        Dim tran1 As New DiscreteTransferEffect With {.Source = conv1, .RedTable = src1.TransferTable, .GreenTable = src1.TransferTable, .BlueTable = src1.TransferTable}
        Dim conv2 As New ConvolveMatrixEffect With {.KernelMatrix = Rotate(src2.ConvolveMatrix, o), .Divisor = src2.ConvolveDivisor, .PreserveAlpha = True}
        Dim tran2 As New DiscreteTransferEffect With {.Source = conv2, .RedTable = src2.TransferTable, .GreenTable = src2.TransferTable, .BlueTable = src2.TransferTable}

        Dim compo As New CompositeEffect With {.Mode = CanvasComposite.Add}
        compo.Sources.Add(tran1)
        compo.Sources.Add(tran2)

        Return New EffectWrapper(Of ICanvasImage)(compo, Sub(s) If True Then conv1.Source = s : conv2.Source = s)
    End Function

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

Public Enum Orientation
    _First = 0
    Landscape = 0
    Portrait = 1
    LandscapeFlipped = 2
    PortraitFlipped = 3
    _Last = 3
End Enum

Public Enum Phase
    _First = 0
    DownThenLeft = 0
    DownThenRight = 1
    LeftThenDown = 2
    RightThenDown = 3
    _Last = 3
End Enum

Public Class DisplayTransform
    Public Scale As Single
    Public Offset As Vector2
    Public Rotation As Matrix3x2
End Class

Public Structure ColorF
    Public R As Single
    Public G As Single
    Public B As Single
    Public A As Single
    Public Shared ReadOnly White As ColorF = New ColorF With {.R = 1, .G = 1, .B = 1, .A = 1}
    Public Shared ReadOnly Gray As ColorF = New ColorF With {.R = 0.5, .G = 0.5, .B = 0.5, .A = 0.5}
    Public Shared ReadOnly Black As ColorF = New ColorF With {.R = 0, .G = 0, .B = 0, .A = 1}

    Public Shared Function FromArgb(a As Double, r As Double, g As Double, b As Double) As ColorF
        Return New ColorF With {.A = CSng(a), .R = CSng(r), .G = CSng(g), .B = CSng(b)}
    End Function
    Public Shared Widening Operator CType(x As Color) As ColorF
        Return ColorF.FromArgb(x.A / 255, x.R / 255, x.G / 255, x.B / 255)
    End Operator
    Private Shared Function bb(s As Single) As Byte
        If s < 0 Then Return 0
        If s > 1 Then Return 255
        Return CByte(s * 255)
    End Function
    Public Shared Narrowing Operator CType(x As ColorF) As Color
        Return Color.FromArgb(bb(x.A), bb(x.R), bb(x.G), bb(x.B))
    End Operator
    Public Shared Operator *(s As Double, x As ColorF) As ColorF
        Return ColorF.FromArgb(s * x.A, s * x.R, s * x.G, s * x.B)
    End Operator
    Public Shared Operator *(x As ColorF, s As Double) As ColorF
        Return ColorF.FromArgb(s * x.A, s * x.R, s * x.G, s * x.B)
    End Operator
    Public Shared Operator /(x As ColorF, s As Double) As ColorF
        Return ColorF.FromArgb(x.A / s, x.R / s, x.G / s, x.B / s)
    End Operator
    Public Shared Operator +(x As ColorF, y As ColorF) As ColorF
        Return ColorF.FromArgb(x.A + y.A, x.R + y.R, x.G + y.G, x.B + y.B)
    End Operator
End Structure

Public Class Win2dSimulator
    Public ReadOnly cols As ColorF()
    Public ReadOnly w, h As Integer

    Sub New(w As Integer, h As Integer)
        Me.cols = New ColorF(w * h - 1) {}
        Me.w = w
        Me.h = h
    End Sub

    Sub New(w As Integer, h As Integer, cols As ColorF())
        If cols.Length <> w * h Then Throw New ArgumentException("cols.Length")
        Me.cols = cols
        Me.w = w
        Me.h = h
    End Sub

    Function ToColors() As Color()
        Dim cols = New Color(Me.cols.Length - 1) {}
        For i = 0 To cols.Length - 1
            cols(i) = CType(Me.cols(i), Color)
        Next
        Return cols
    End Function

    Public Sub Verify(reference As CanvasRenderTarget)
        If w <> reference.Bounds.Width OrElse h <> reference.Bounds.Height Then Throw New Exception("mismatched sizes")
        Dim simulated = cols
        Dim win2d = reference.GetPixelColorFs(0, 0, w, h)
        Dim hasErrors = False
        Dim epsilon = 0.01F
        For i = 0 To win2d.Length - 1
            If Math.Abs(simulated(i).R - win2d(i).R) > epsilon Then hasErrors = True : Exit For
            If Math.Abs(simulated(i).G - win2d(i).G) > epsilon Then hasErrors = True : Exit For
            If Math.Abs(simulated(i).B - win2d(i).B) > epsilon Then hasErrors = True : Exit For
            If Math.Abs(simulated(i).A - win2d(i).A) > epsilon Then hasErrors = True : Exit For
        Next
        Debug.WriteLine($"Expected:{vbCrLf}{Me}")
        Debug.WriteLine($"Actual:{vbCrLf}{New Win2dSimulator(w, h, win2d)}")
        If hasErrors Then Stop
    End Sub

    Public Overrides Function ToString() As String
        Dim sb As New StringBuilder
        For y = 0 To h - 1
            For x = 0 To w - 1
                Dim c = cols(y * w + x)
                'sb.Append($"#{c.A:0.00}_{c.R:0.00}_{c.G:0.00}_{c.B:0.00} ")
                sb.Append($"#[{c.A:0.00}]{c.R:0.0000} ")
            Next
            sb.AppendLine()
        Next
        Return sb.ToString()
    End Function

    Public Shared Function Simulate(e As IGraphicsEffectSource) As Win2dSimulator
        If TypeOf e Is ConvolveMatrixEffect Then Return SimulateConvolve(CType(e, ConvolveMatrixEffect))
        If TypeOf e Is DiscreteTransferEffect Then Return SimulateDiscrete(CType(e, DiscreteTransferEffect))
        If TypeOf e Is CompositeEffect Then Return SimulateComposite(CType(e, CompositeEffect))
        If TypeOf e Is CanvasRenderTarget Then Return SimulateCanvas(CType(e, CanvasRenderTarget))
        Throw New ArgumentException(e.GetType().Name)
    End Function

    Private Shared Function SimulateCanvas(e As CanvasRenderTarget) As Win2dSimulator
        Dim w = CInt(e.SizeInPixels.Width), h = CInt(e.SizeInPixels.Height)
        Return New Win2dSimulator(w, h, e.GetPixelColorFs(0, 0, w, h))
    End Function

    Private Shared Function SimulateConvolve(e As ConvolveMatrixEffect) As Win2dSimulator
        If Not e.PreserveAlpha Then Throw New NotImplementedException(NameOf(e.PreserveAlpha))
        If e.BorderMode <> EffectBorderMode.Soft Then Throw New NotImplementedException(NameOf(e.BorderMode))
        If e.KernelHeight <> 3 OrElse e.KernelWidth <> 3 Then Throw New NotImplementedException(NameOf(e.KernelWidth) & "," & NameOf(e.KernelHeight))
        If e.InterpolationMode <> CanvasImageInterpolation.Linear Then Throw New NotImplementedException(NameOf(e.InterpolationMode))
        If e.KernelOffset.X <> 0 OrElse e.KernelOffset.Y <> 0 Then Throw New NotImplementedException(NameOf(e.KernelOffset))
        If e.KernelScale.X <> 1 OrElse e.KernelScale.Y <> 1 Then Throw New NotImplementedException(NameOf(e.KernelScale))

        Dim src = Simulate(e.Source)

        Dim c = Function(x As Integer, y As Integer) As ColorF
                    If x < 0 OrElse x >= src.w OrElse y < 0 OrElse y >= src.h Then Return ColorF.FromArgb(0, 0, 0, 0)
                    Return src.cols(y * src.w + x)
                End Function

        Dim s As New Win2dSimulator(src.w, src.h)
        For y = 0 To src.h - 1
            For x = 0 To src.w - 1
                Dim v As ColorF = ColorF.FromArgb(0, 0, 0, 0)
                v += e.KernelMatrix(Kernel.UpLeft) * c(x - 1, y - 1)
                v += e.KernelMatrix(Kernel.Up) * c(x, y - 1)
                v += e.KernelMatrix(Kernel.UpRight) * c(x + 1, y - 1)
                v += e.KernelMatrix(Kernel.Left) * c(x - 1, y)
                v += e.KernelMatrix(Kernel.Center) * c(x, y)
                v += e.KernelMatrix(Kernel.Right) * c(x + 1, y)
                v += e.KernelMatrix(Kernel.DownLeft) * c(x - 1, y + 1)
                v += e.KernelMatrix(Kernel.Down) * c(x, y + 1)
                v += e.KernelMatrix(Kernel.DownRight) * c(x + 1, y + 1)
                v /= e.Divisor
                'v *= c(x, y).A
                v.A = c(x, y).A
                If e.ClampOutput Then v = ColorF.FromArgb(Math.Max(1, v.A), Math.Max(1, v.R), Math.Max(1, v.G), Math.Max(1, v.B))
                s.cols(y * src.w + x) = v
            Next
        Next
        Return s
    End Function

    Public Shared Function Simulate(lambda As Func(Of Func(Of Integer, Single), Single), src As Win2dSimulator) As Win2dSimulator
        Dim kc = New ColorF(8) {}
        Dim ks = New Single(8) {}

        Dim c = Function(x As Integer, y As Integer) As ColorF
                    If x < 0 OrElse x >= src.w OrElse y < 0 OrElse y >= src.h Then Return ColorF.FromArgb(1, 0, 0, 0)
                    Return src.cols(y * src.w + x)
                End Function

        Dim m = Function(channel As Func(Of ColorF, Single)) As Single
                    For i = 0 To 8 : ks(i) = channel(kc(i)) : Next
                    Return lambda(Function(j) ks(j))
                End Function

        Dim s As New Win2dSimulator(src.w, src.h)
        For y = 0 To src.h - 1
            For x = 0 To src.w - 1
                kc(Kernel.UpLeft) = c(x - 1, y - 1)
                kc(Kernel.Up) = c(x, y - 1)
                kc(Kernel.UpRight) = c(x + 1, y - 1)
                kc(Kernel.Left) = c(x - 1, y)
                kc(Kernel.Center) = c(x, y)
                kc(Kernel.Right) = c(x + 1, y)
                kc(Kernel.DownLeft) = c(x - 1, y + 1)
                kc(Kernel.Down) = c(x, y + 1)
                kc(Kernel.DownRight) = c(x + 1, y + 1)

                Dim v = ColorF.FromArgb(m(Function(col) col.A), m(Function(col) col.R), m(Function(col) col.G), m(Function(col) col.B))
                s.cols(y * src.w + x) = v
            Next
        Next
        Return s
    End Function


    Private Shared Function SimulateDiscrete(e As DiscreteTransferEffect) As Win2dSimulator
        If e.AlphaDisable Then Throw New NotImplementedException(NameOf(e.AlphaDisable))
        If e.RedDisable Then Throw New NotImplementedException(NameOf(e.RedDisable))
        If e.GreenDisable Then Throw New NotImplementedException(NameOf(e.GreenDisable))
        If e.BlueDisable Then Throw New NotImplementedException(NameOf(e.BlueDisable))

        Dim src = Simulate(e.Source)

        Dim map = Function(val As Single, t As Single()) As Single
                      Dim i = CInt(Math.Floor(val * t.Length))
                      If i = t.Length Then i = t.Length - 1
                      Return t(i)
                  End Function

        Dim s As New Win2dSimulator(src.w, src.h)
        For i = 0 To src.cols.Length - 1
            Dim v = src.cols(i)
            v = ColorF.FromArgb(map(v.A, e.AlphaTable), map(v.R, e.RedTable), map(v.G, e.GreenTable), map(v.B, e.BlueTable))
            If e.ClampOutput Then v = ColorF.FromArgb(Math.Max(1, v.A), Math.Max(1, v.R), Math.Max(1, v.G), Math.Max(1, v.B))
            s.cols(i) = v
        Next
        Return s
    End Function

    Private Shared Function SimulateComposite(e As CompositeEffect) As Win2dSimulator
        If e.Mode <> CanvasComposite.Add Then Throw New NotImplementedException(NameOf(e.Mode))

        ' result = (s1.A+s1.A, s1.R+s2.R, s1.G+s2.G, s1.B+s2.B)
        ' This is true regardless of AlphaBlendingMode = Ignore|Premultiplied, on either of the sources or the target
        ' It will happily produce A,R,G,B outside the 0..1 range.

        Dim srcs = e.Sources.Select(Function(esrc) Simulate(esrc)).ToArray()
        If srcs.Count = 0 Then Throw New ArgumentException(NameOf(e.Sources))
        Dim w = srcs(0).w, h = srcs(0).h
        For Each src In srcs
            If src.w <> w OrElse src.h <> h Then Throw New ArgumentException(NameOf(w) & "," & NameOf(h))
        Next

        Dim s As New Win2dSimulator(w, h)
        For i = 0 To s.cols.Length - 1
            Dim v = ColorF.FromArgb(0, 0, 0, 0)
            For Each src In srcs
                v += src.cols(i)
            Next
            s.cols(i) = v
        Next
        Return s
    End Function


End Class


