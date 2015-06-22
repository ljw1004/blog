Imports System.Text
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Effects
Imports Microsoft.Graphics.Canvas.UI.Xaml
Imports Windows.Graphics.DirectX
Imports Windows.Graphics.Effects
Imports Windows.UI

Public Class Win2dSimulator : Implements IFormattable

    Public ReadOnly cols As ColorF()
    Public ReadOnly w, h As Integer
    Public ReadOnly ProvenanceCount As Tuple(Of Integer, Integer) ' Float, Byte

    Sub New(w As Integer, h As Integer, prov As Tuple(Of Integer, Integer))
        Me.cols = New ColorF(w * h - 1) {}
        Me.w = w
        Me.h = h
        Me.ProvenanceCount = prov
    End Sub

    Sub New(w As Integer, h As Integer, prov As Tuple(Of Integer, Integer), cols As ColorF())
        If cols.Length <> w * h Then Throw New ArgumentException("cols.Length")
        Me.cols = cols
        Me.w = w
        Me.h = h
        Me.ProvenanceCount = prov
    End Sub

    Public Sub Verify(reference As CanvasRenderTarget)
        If w <> reference.Bounds.Width OrElse h <> reference.Bounds.Height Then Throw New Exception("mismatched sizes")
        Dim expected = Me
        Dim actual = Simulate(reference)
        Dim hasErrors = False
        Dim epsilon = 0.05F
        For i = 0 To actual.cols.Length - 1
            If Math.Abs(expected.cols(i).R - actual.cols(i).R) > epsilon Then hasErrors = True : Exit For
            If Math.Abs(expected.cols(i).G - actual.cols(i).G) > epsilon Then hasErrors = True : Exit For
            If Math.Abs(expected.cols(i).B - actual.cols(i).B) > epsilon Then hasErrors = True : Exit For
            'If Math.Abs(expected.cols(i).A - actual.cols(i).A) > epsilon Then hasErrors = True : Exit For
        Next
        If Not hasErrors Then Return
        Debug.WriteLine($"Expected:{vbCrLf}{expected}")
        Debug.WriteLine($"Actual:{vbCrLf}{actual}")
        Stop
    End Sub

    Public Overloads Function ToString(format As String, formatProvider As IFormatProvider) As String Implements IFormattable.ToString
        If format Is Nothing OrElse format = "a" OrElse format = "" Then
            Dim floatingProvenanceCount = ProvenanceCount.Item1, byteProvenanceCount = ProvenanceCount.Item2

            Dim rgb_are_equal = True
            For i = 0 To cols.Length - 1
                Dim c = cols(i)
                If c.R <> c.G OrElse c.R <> c.B Then rgb_are_equal = False : Exit For
            Next

            If floatingProvenanceCount > 0 AndAlso rgb_are_equal Then
                format = "r"
            ElseIf floatingProvenanceCount > 0 Then
                format = "f"
            Else
                format = "c"
            End If
        End If

        If format <> "c" AndAlso format <> "f" AndAlso format <> "r" Then Throw New ArgumentException(NameOf(format))

        Dim sb As New StringBuilder
        For y = 0 To h - 1
            For x = 0 To w - 1
                Dim c = cols(y * w + x)
                If format = "r" Then sb.Append($"#[{c.A:0.00}]{c.R:0.0000} ")
                If format = "f" Then sb.Append($"#{c.A:0.00}_{c.R:0.00}_{c.G:0.00}_{c.B:0.00} ")
                If format = "c" Then sb.Append($"{CType(c, Color)} ")
            Next
            sb.AppendLine()
        Next
        Return sb.ToString()
    End Function

    Public Overrides Function ToString() As String
        Return ToString("", Nothing)
    End Function

    Public Shared Function Simulate(e As IGraphicsEffectSource) As Win2dSimulator
        If TypeOf e Is ConvolveMatrixEffect Then Return SimulateConvolve(CType(e, ConvolveMatrixEffect))
        If TypeOf e Is DiscreteTransferEffect Then Return SimulateDiscrete(CType(e, DiscreteTransferEffect))
        If TypeOf e Is CompositeEffect Then Return SimulateComposite(CType(e, CompositeEffect))
        If TypeOf e Is CanvasRenderTarget Then Return SimulateCanvas(CType(e, CanvasRenderTarget))
        If TypeOf e Is ArithmeticCompositeEffect Then Return SimulateArithmetic(CType(e, ArithmeticCompositeEffect))
        Throw New ArgumentException(e.GetType().Name)
    End Function

    Private Shared Function SimulateCanvas(e As CanvasRenderTarget) As Win2dSimulator
        Dim w = CInt(e.SizeInPixels.Width), h = CInt(e.SizeInPixels.Height)
        Dim p = If(e.Description.Format = DirectXPixelFormat.B8G8R8A8UIntNormalized, Tuple.Create(0, 1), Tuple.Create(1, 0))
        Return New Win2dSimulator(w, h, p, e.GetPixelColorFs(0, 0, w, h))
    End Function

    Private Shared Function SimulateArithmetic(e As ArithmeticCompositeEffect) As Win2dSimulator
        Dim src1 = Simulate(e.Source1), src2 = Simulate(e.Source2)
        Dim p = Tuple.Create(src1.ProvenanceCount.Item1 + src2.ProvenanceCount.Item1, src1.ProvenanceCount.Item2 + src2.ProvenanceCount.Item2)
        Dim w = src1.w, h = src1.h
        If src2.w <> w OrElse src2.h <> h Then Throw New ArgumentException(NameOf(w) & "," & NameOf(h))
        Dim s As New Win2dSimulator(w, h, p)
        For i = 0 To s.cols.Length - 1
            Dim r = e.Source1Amount * src1.cols(i).R + e.Source2Amount * src2.cols(i).R + e.MultiplyAmount * src1.cols(i).R * src2.cols(i).R + e.Offset
            Dim g = e.Source1Amount * src1.cols(i).G + e.Source2Amount * src2.cols(i).G + e.MultiplyAmount * src1.cols(i).G * src2.cols(i).G + e.Offset
            Dim b = e.Source1Amount * src1.cols(i).B + e.Source2Amount * src2.cols(i).B + e.MultiplyAmount * src1.cols(i).B * src2.cols(i).B + e.Offset
            Dim a = e.Source1Amount * src1.cols(i).A + e.Source2Amount * src2.cols(i).A + e.MultiplyAmount * src1.cols(i).A * src2.cols(i).A + e.Offset
            Dim v = ColorF.FromArgb(a, r, g, b)
            If e.ClampOutput Then v = v.Clamp()
            s.cols(i) = v
        Next
        Return s
    End Function

    Private Shared Function SimulateConvolve(e As ConvolveMatrixEffect) As Win2dSimulator
        ' Consider three pixels <argb> | <argb1> | <argb3>
        ' with a 5x1 convolution matrix [0,0,0,X,Y]
        ' What is the new value of the leftmost pixel? Notionally it's X<argb1> + Y<argb2>.
        ' But it's a bit more complicated than that...
        '
        ' (1) "AlphaMode" of the source and target surface is ignored completely
        ' (2) If PreserveAlpha=False, then it's just a piecewise composition:
        '       a' = Xa1 + Ya2
        '       r' = Xr1 + Yr2
        '       g' = Xg1 + Yg2
        '       b' = Xb1 + Yb2
        ' (3) If PreserveAlpha=True then it's done differently:
        '       a' = a
        '       r' = X(a'/a1)r1 + Y(a'/a2)r2   except if a1=0 or a2=0 then omit that component
        '       g' = X(a'/a1)g1 + Y(a'/a2)g2   except if a1=0 or a2=0 then omit that component
        '       b' = X(a'/a1)b1 + Y(a'/a2)b2   except if a1=0 or a2=0 then omit that component
        ' 


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

        Dim f = Function(c0 As ColorF, k As Single, ck As ColorF) As ColorF
                    If e.PreserveAlpha Then
                        If ck.A = 0 Then Return ColorF.FromArgb(c0.A, 0, 0, 0)
                        Return ColorF.FromArgb(c0.A, k * ck.R * c0.A / ck.A, k * ck.G * c0.A / ck.A, k * ck.B * c0.A / ck.A)
                    Else
                        Return ColorF.FromArgb(k * ck.A, k * ck.R, k * ck.G, k * ck.B)
                    End If
                End Function

        Dim s As New Win2dSimulator(src.w, src.h, src.ProvenanceCount)
        For y = 0 To src.h - 1
            For x = 0 To src.w - 1
                Dim c0 = c(x, y)
                Dim v As ColorF = ColorF.FromArgb(0, 0, 0, 0)
                v += f(c0, e.KernelMatrix(Kernel.UpLeft), c(x - 1, y - 1))
                v += f(c0, e.KernelMatrix(Kernel.Up), c(x, y - 1))
                v += f(c0, e.KernelMatrix(Kernel.UpRight), c(x + 1, y - 1))
                v += f(c0, e.KernelMatrix(Kernel.Left), c(x - 1, y))
                v += f(c0, e.KernelMatrix(Kernel.Center), c(x, y))
                v += f(c0, e.KernelMatrix(Kernel.Right), c(x + 1, y))
                v += f(c0, e.KernelMatrix(Kernel.DownLeft), c(x - 1, y + 1))
                v += f(c0, e.KernelMatrix(Kernel.Down), c(x, y + 1))
                v += f(c0, e.KernelMatrix(Kernel.DownRight), c(x + 1, y + 1))
                v /= e.Divisor
                If e.ClampOutput Then v = v.Clamp()
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

        Dim s As New Win2dSimulator(src.w, src.h, src.ProvenanceCount)
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
                      If i >= t.Length Then i = t.Length - 1
                      If i < 0 Then i = 0
                      Return t(i)
                  End Function

        Dim s As New Win2dSimulator(src.w, src.h, src.ProvenanceCount)
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
        ' 

        Dim srcs = e.Sources.Select(Function(esrc) Simulate(esrc)).ToArray()
        If srcs.Count = 0 Then Throw New ArgumentException(NameOf(e.Sources))
        Dim w = srcs(0).w, h = srcs(0).h, p = Tuple.Create(0, 0)
        For Each src In srcs
            If src.w <> w OrElse src.h <> h Then Throw New ArgumentException(NameOf(w) & "," & NameOf(h))
            p = Tuple.Create(p.Item1 + src.ProvenanceCount.Item1, p.Item2 + src.ProvenanceCount.Item2)
        Next

        Dim s As New Win2dSimulator(w, h, p)
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


Public Structure ColorF
    Public R As Single
    Public G As Single
    Public B As Single
    Public A As Single
    Public Shared ReadOnly White As ColorF = ColorF.FromArgb(1, 1, 1, 1)
    Public Shared ReadOnly Gray As ColorF = ColorF.FromArgb(1, 0.5, 0.5, 0.5)
    Public Shared ReadOnly Black As ColorF = ColorF.FromArgb(1, 0, 0, 0)
    Public Shared ReadOnly Red As ColorF = ColorF.FromArgb(1, 1, 0, 0)

    Public Shared Function FromArgb(a As Double, r As Double, g As Double, b As Double) As ColorF
        Return New ColorF With {.A = CSng(a), .R = CSng(r), .G = CSng(g), .B = CSng(b)}
    End Function
    Public Shared Widening Operator CType(x As Color) As ColorF
        Return ColorF.FromArgb(x.A / 255, x.R / 255, x.G / 255, x.B / 255)
    End Operator
    Public Function Clamp() As ColorF
        Return ColorF.FromArgb(Math.Max(0, Math.Min(1, A)), Math.Max(0, Math.Min(1, R)), Math.Max(0, Math.Min(1, G)), Math.Max(0, Math.Min(1, B)))
    End Function
    Public Overrides Function ToString() As String
        Return $"<{A:0.00},{R:0.00},{G:0.00},{B:0.00}>"
    End Function
    Public Shared Operator =(x As ColorF, y As ColorF) As Boolean
        Return x.A = y.A AndAlso x.R = y.R AndAlso x.G = y.G AndAlso x.B = y.B
    End Operator
    Public Shared Operator <>(x As ColorF, y As ColorF) As Boolean
        Return x.A <> y.A OrElse x.R = y.R OrElse x.G = y.G OrElse x.B = y.B
    End Operator
    Public Overrides Function Equals(obj As Object) As Boolean
        If obj Is Nothing Then Return False
        Dim you = CType(obj, ColorF)
        Return Me = you
    End Function
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




Module Win2dSimulator_Utils

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
    Sub SetPixelColorFs(bmp As CanvasRenderTarget, cols As ColorF())
        SetPixelColorFs(bmp, cols, 0, 0, bmp.Description.Width, bmp.Description.Height)
    End Sub

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
    Function GetPixelColorFs(bmp As CanvasRenderTarget) As ColorF()
        Return GetPixelColorFs(bmp, 0, 0, bmp.Description.Width, bmp.Description.Height)
    End Function

    <Extension>
    Function GetPixelColorFs(bmp As CanvasRenderTarget, left As Integer, top As Integer, width As Integer, height As Integer) As ColorF()
        Dim c = New ColorF(width * height - 1) {}
        If bmp.Description.Format = DirectXPixelFormat.B8G8R8A8UIntNormalized Then
            Dim buf = bmp.GetPixelColors(left, top, width, height)
            For i = 0 To c.Length - 1
                c(i) = buf(i)
            Next
        ElseIf bmp.Description.Format = DirectXPixelFormat.R32G32B32A32Float Then
            Dim buf = bmp.GetPixelBytes(left, top, width, height)
            For i = 0 To c.Length - 1
                c(i).R = BitConverter.ToSingle(buf, i * 16 + 0)
                c(i).G = BitConverter.ToSingle(buf, i * 16 + 4)
                c(i).B = BitConverter.ToSingle(buf, i * 16 + 8)
                c(i).A = BitConverter.ToSingle(buf, i * 16 + 12)
            Next
        ElseIf bmp.Description.Format = DirectXPixelFormat.R16G16B16A16Float Then
            Dim buf = bmp.GetPixelBytes(left, top, width, height)
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


Module Testing

    Sub TestConvolve(canvas1 As CanvasControl)
        Debug.WriteLine($"srcAMode,X.A,X.R,Y.A,Y.R,preserveAlpha,dstAMode,X.A,X.R,4X+Y.A,4X+Y.R,X+4Y.A,X+4Y.R,Y.A,Y.R")
        Dim defaultDpi = 96.0F
        For Each am1 In {CanvasAlphaMode.Ignore, CanvasAlphaMode.Premultiplied}
            For Each am2 In {CanvasAlphaMode.Ignore, CanvasAlphaMode.Premultiplied}
                For Each preserveAlpha In {True, False}
                    Dim s1 As New CanvasRenderTarget(canvas1, 6, 6, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, am1)
                    Dim s2 As New CanvasRenderTarget(canvas1, 6, 6, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, am2)
                    Dim e As New ConvolveMatrixEffect With {.Source = s1, .KernelMatrix = {0, 0, 0, 1, 4, 1, 0, 0, 0}, .PreserveAlpha = preserveAlpha}
                    For Each a1 In {0, 0.5, 1}
                        For Each c1 In {0, 0.5, 1}
                            For Each a2 In {0, 0.1, 1}
                                For Each c2 In {0, 0.5, 1}
                                    Dim colX = ColorF.FromArgb(a1, c1, c1, c1)
                                    Dim colY = ColorF.FromArgb(a2, c2, c2, c2)
                                    SetPixelColorFs(s1, {colX, colY}, 1, 1, 2, 1)
                                    Using ds = s2.CreateDrawingSession
                                        ds.Blend = CanvasBlend.Copy
                                        ds.DrawImage(e)
                                    End Using
                                    Dim cols = s2.GetPixelColorFs(0, 1, 4, 1)
                                    Debug.WriteLine($"{am1},{colX.A},{colX.R},{colY.A},{colY.R},{preserveAlpha},{am2},{cols(0).A},{cols(0).R},{cols(1).A},{cols(1).R},{cols(2).A},{cols(2).R},{cols(3).A},{cols(3).R}")
                                Next c2
                            Next a2
                        Next c1
                    Next a1
                Next preserveAlpha
            Next am2
        Next am1
        Stop
    End Sub

    Sub TestComposite(canvas1 As CanvasControl)
        Dim defaultDpi = 96.0F
        Dim csv = "DA,D,SA,S"
        For ccmode = CanvasComposite.SourceOver To CanvasComposite.MaskInvert
            csv &= $",OA.{ccmode},O.{ccmode}"
        Next
        Debug.WriteLine(csv)
        For Each am1 In {CanvasAlphaMode.Ignore, CanvasAlphaMode.Premultiplied}
            For Each am2 In {CanvasAlphaMode.Ignore, CanvasAlphaMode.Premultiplied}
                For Each am3 In {CanvasAlphaMode.Ignore, CanvasAlphaMode.Premultiplied}
                    If am1 <> CanvasAlphaMode.Ignore OrElse am2 <> CanvasAlphaMode.Ignore OrElse am3 <> CanvasAlphaMode.Ignore Then Continue For
                    Dim surfaceD As New CanvasRenderTarget(canvas1, 6, 6, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, am1)
                    Dim surfaceS As New CanvasRenderTarget(canvas1, 6, 6, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, am2)
                    Dim surfaceO As New CanvasRenderTarget(canvas1, 6, 6, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, am3)
                    Dim e As New CompositeEffect
                    e.Sources.Add(surfaceD) : e.Sources.Add(surfaceS)
                    For Each DA In {0F, 0.5F, 1.0F}
                        For Each D In {0F, 0.5F, 1.0F}
                            For Each SA In {0F, 0.3F, 1.0F}
                                For Each S In {0F, 0.5F, 5.0F}
                                    csv = $"{DA},{D},{SA},{S}"
                                    For ccmode = CanvasComposite.SourceOver To CanvasComposite.MaskInvert
                                        e.Mode = ccmode
                                        surfaceD.SetPixelColorFs({ColorF.FromArgb(DA, D, D, D)}, 0, 0, 1, 1)
                                        surfaceS.SetPixelColorFs({ColorF.FromArgb(SA, S, S, S)}, 0, 0, 1, 1)
                                        Using ds = surfaceO.CreateDrawingSession()
                                            ds.Blend = CanvasBlend.Copy
                                            ds.DrawImage(e)
                                        End Using
                                        Dim c = surfaceO.GetPixelColorFs(0, 0, 1, 1)(0), O = c.R, OA = c.A
                                        csv &= $",{OA},{O}"
                                    Next ccmode
                                    Debug.WriteLine(csv)
                                Next S
                            Next SA
                        Next D
                    Next DA
                Next am3
            Next am2
        Next am1
        Stop
    End Sub

    Sub TestCompositeAdd2(canvas1 As CanvasControl)
        ' What this test does:
        '    It does Composite.Add(surface1, DiscreteTransfer(surface1,{0,-0.5,1}))
        '    It does it first on a floating point surface with an input pixel set to Gray,
        '    then does the same on an 8bit byte surface with input pixel set to Gray.
        ' What I expect:
        '    It should give similar answers in both cases, Black.
        ' What I get:
        '    R32G32B32A32Float: a=2.0 r=0.0 g=0.0 b=0.0
        '    B8G8R8A8UIntNormalized: #FF808080

        Dim defaultDpi = 96.0F
        With Nothing
            Dim surface1 As New CanvasRenderTarget(canvas1, 6, 6, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
            Dim surface2 As New CanvasRenderTarget(canvas1, 6, 6, defaultDpi, DirectXPixelFormat.R32G32B32A32Float, CanvasAlphaMode.Ignore)
            Dim e1 As New DiscreteTransferEffect With {.Source = surface1, .RedTable = {0, -0.5, 1}, .GreenTable = {0, -0.5, 1}, .BlueTable = {0, -0.5, 1}}
            Dim e2 As New CompositeEffect With {.Mode = CanvasComposite.Add}
            e2.Sources.Add(surface1) : e2.Sources.Add(e1)
            Dim r = 0.5F, g = 0.5F, b = 0.5F, a = 1.0F
            Dim buf = New Byte(16 - 1) {}
            Array.Copy(BitConverter.GetBytes(r), 0, buf, 0, 4)
            Array.Copy(BitConverter.GetBytes(g), 0, buf, 4, 4)
            Array.Copy(BitConverter.GetBytes(b), 0, buf, 8, 4)
            Array.Copy(BitConverter.GetBytes(a), 0, buf, 12, 4)
            surface1.SetPixelBytes(buf, 1, 1, 1, 1)
            Using ds = surface2.CreateDrawingSession
                ds.Blend = CanvasBlend.Copy
                ds.DrawImage(e2)
            End Using
            buf = surface2.GetPixelBytes(1, 1, 1, 1)
            r = BitConverter.ToSingle(buf, 0)
            g = BitConverter.ToSingle(buf, 4)
            b = BitConverter.ToSingle(buf, 8)
            a = BitConverter.ToSingle(buf, 12)
            Debug.WriteLine($"{surface1.Description.Format}: a={a:0.0} r={r:0.0} g={g:0.0} b={b:0.0}")
        End With

        With Nothing
            Dim surface1 As New CanvasRenderTarget(canvas1, 6, 6, defaultDpi, DirectXPixelFormat.B8G8R8A8UIntNormalized, CanvasAlphaMode.Ignore)
            Dim surface2 As New CanvasRenderTarget(canvas1, 6, 6, defaultDpi, DirectXPixelFormat.B8G8R8A8UIntNormalized, CanvasAlphaMode.Ignore)
            Dim e1 As New DiscreteTransferEffect With {.Source = surface1, .RedTable = {0, -0.5, 1}, .GreenTable = {0, -0.5, 1}, .BlueTable = {0, -0.5, 1}}
            Dim e2 As New CompositeEffect With {.Mode = CanvasComposite.Add}
            e2.Sources.Add(surface1)
            e2.Sources.Add(e1)
            surface1.SetPixelColorFs({Colors.Gray}, 1, 1, 1, 1)
            Using ds = surface2.CreateDrawingSession
                ds.Blend = CanvasBlend.Copy
                ds.DrawImage(e2)
            End Using
            Dim c = surface2.GetPixelColors(1, 1, 1, 1)(0)
            Debug.WriteLine($"{surface1.Description.Format}: {c}")
        End With

        Stop
    End Sub
End Module
