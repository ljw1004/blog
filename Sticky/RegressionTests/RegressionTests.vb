Module RegressionTests
    Public Const debugstick = "President Bush (Lucian).stk" '"President Bush (Lucian).stk"
    Public Const debuginteractive = True
    Public Const debugrandom = True
    Public Const debugbmp = False
    Public Const debugilimb = -1
    Public Const debugishape = -1

    Sub Main()
        MainAsync().GetAwaiter().GetResult()
    End Sub

    Async Function MainAsync() As Task
        Dim RND As New Random()

        If debugbmp Then IO.Directory.CreateDirectory("bmp")

        Dim f As MyForm = Nothing
        Dim dummy = Task.Factory.StartNew(
            Sub()
                Windows.Forms.Application.EnableVisualStyles()
                f = New MyForm
                f.bmp = New System.Drawing.Bitmap(480, 480)
                f.g = Drawing.Graphics.FromImage(f.bmp)
                Windows.Forms.Application.Run(f)
            End Sub)
        While f Is Nothing OrElse Not f.Visible : Threading.Thread.Sleep(100) : End While

        Using sw As New IO.StreamWriter(If(String.IsNullOrEmpty(debugstick), "results.txt", "tmp.txt"))
            For Each stickfn In If(String.IsNullOrEmpty(debugstick), IO.Directory.EnumerateFiles("sticks"), {"sticks\" + debugstick})
                f.Bitmaps.Clear()
                Dim stick As New Stick
                f.stick = stick
                Using stream As New IO.FileStream(stickfn, IO.FileMode.Open)
                    Await stick.LoadAsync(IO.Path.GetFileNameWithoutExtension(stickfn), stream, f)
                End Using
                Dim freq = New Double(3, 5) {}
                For Each rr In If(debuginteractive, {0.3}, {0.3, 1.0, 0.0})
                    Dim rrf = Function() If(debugrandom, Math.Pow(RND.NextDouble(), 2), rr)
                    For hz = Frequency.HzA_0to320 To Frequency.HzF_1600to1960
                        freq(Channel.Left, hz) = rrf()
                        freq(Channel.Right, hz) = rrf()
                        freq(Channel.RightMinusLeft, hz) = (freq(Channel.Right, hz) - freq(Channel.Left, hz)) * 0.5 + 0.5
                    Next
                    freq(Channel.Karaoke, Frequency.Karaoke_Vocals) = rrf()
                    freq(Channel.Karaoke, Frequency.Karaoke_Music) = rrf()
                    stick.Update(freq, TimeSpan.FromMilliseconds(30))

                    For i = 0 To stick.Limbs.Count - 1
                        sw.WriteLine("{1}{0}{2:f1}{0}limb({3})pos{0}f={4:f2}{0}ex={5:f2}{0}ey={6:f2}", vbTab, IO.Path.GetFileName(stickfn), rr, i, stick.Limbs(i).CurrentPosFraction.f, stick.Limbs(i).CurrentEndPoint.x, stick.Limbs(i).CurrentEndPoint.y)
                    Next
                    For i = 0 To stick.Limbs.Count - 1
                        If stick.Limbs(i).LineColorController.Segments.Count = 1 Then Continue For
                        If stick.Limbs(i).CurrentLinePaint.Paint.Kind <> FillKind.RGB Then Continue For
                        sw.WriteLine("{1}{0}{2:f1}{0}limb({3})linepaint{0}f={4:f2}{0}lp={5}", vbTab, IO.Path.GetFileName(stickfn), rr, i, stick.Limbs(i).CurrentLinePaint.Frac.f, stick.Limbs(i).CurrentLinePaint.Paint)
                    Next
                    For i = 0 To stick.Shapes.Count - 1
                        Dim shape = stick.Shapes(i)
                        If shape Is Nothing Then Continue For
                        If shape.Fill.Segments.Count > 1 Then
                            sw.WriteLine("{1}{0}{2:f1}{0}shape({3})fillpaint{0}f={4:f2}{0}fp={5}", vbTab, IO.Path.GetFileName(stickfn), rr, i, shape.CurrentFillPaint.Frac.f, shape.CurrentFillPaint.Paint)
                        End If
                        If shape.Line.Segments.Count > 1 Then
                            sw.WriteLine("{1}{0}{2:f1}{0}shape({3})linepaint{0}f={4:f2}{0}lp={5}", vbTab, IO.Path.GetFileName(stickfn), rr, i, shape.CurrentLinePaint.Frac.f, shape.CurrentLinePaint.Paint)
                        End If
                    Next

                    f.Log = Sub(msg As String) sw.WriteLine("{1}{0}{2:f1}{0}render{0}{3}", vbTab, IO.Path.GetFileName(stickfn), rr, msg)
                    stick.Draw(0, 0, 480, 480, f)

                    SyncLock f.bmp
                        If debugbmp Then f.bmp.Save("bmp\" & IO.Path.GetFileNameWithoutExtension(stickfn) & "_" & rr.ToString("0.0") & "_vb.png", Drawing.Imaging.ImageFormat.Png)
                    End SyncLock
                    f.Invalidate()
                    If debuginteractive Then
                        Do
                            Windows.Forms.Application.DoEvents()
                            If Not f.Visible Then Exit Function
                        Loop
                    End If

                Next rr
            Next stickfn
        End Using
    End Function


End Module



Class MyForm
    Inherits Windows.Forms.Form
    Implements IStickRenderer

    Public Bitmaps As New Dictionary(Of String, Tuple(Of Drawing.Bitmap, Boolean))
    Public bmp As System.Drawing.Bitmap
    Public g As Drawing.Graphics
    Public Log As Action(Of String) = Sub(msg) Return
    Public stick As Stick
    Public currentilimb As Integer = debugilimb
    Public currentishape As Integer = debugishape

    Sub New()
        Dim sz = SizeFromClientSize(New Drawing.Size(480, 480))
        Width = sz.Width
        Height = sz.Height
        SetStyle(Windows.Forms.ControlStyles.UserPaint, True)
        SetStyle(Windows.Forms.ControlStyles.AllPaintingInWmPaint, True)
        SetStyle(Windows.Forms.ControlStyles.OptimizedDoubleBuffer, True)
    End Sub

    Function Col(x As StickRGB) As Drawing.Color
        Return Drawing.Color.FromArgb(x.r, x.g, x.b)
    End Function

    Function Pen(c As StickRGB, t As Double) As Drawing.Pen
        If Not c.IsVisible Then Return Nothing
        If t = 0 Then Return New Drawing.Pen(Col(c))
        Return New Drawing.Pen(Col(c), CSng(t)) With {.EndCap = Drawing.Drawing2D.LineCap.Round, .LineJoin = Drawing.Drawing2D.LineJoin.Round}
    End Function

    Function Brush(c As StickRGB) As Drawing.Brush
        If Not c.IsVisible Then Return Nothing
        Return New Drawing.SolidBrush(Col(c))
    End Function

    Sub AdjustColors(tag As Object, ByRef linethickness As Double, ByRef linecolor As StickRGB, ByRef fillcolor As StickRGB)
        If currentilimb = -1 AndAlso currentishape = -1 Then Return
        If CStr(tag).IndexOf("limb(" & currentilimb & ")") <> -1 Then
            linethickness = 14
            linecolor = StickRGB.FromArgb(0, 255, 255)
        Else
            linecolor = StickRGB.FromArgb(linecolor.r \ 4, linecolor.g \ 4, linecolor.b \ 4)
        End If
        If CStr(tag).IndexOf("shape(" & currentishape & ")") <> -1 Then
            fillcolor = StickRGB.FromArgb(0, 255, 255)
        Else
            fillcolor = StickRGB.FromArgb(fillcolor.r \ 4, fillcolor.g \ 4, fillcolor.b \ 4)
        End If
    End Sub

    Protected Overrides Sub OnClick(e2 As EventArgs)
        If Not debuginteractive Then Return
        If currentilimb = -1 AndAlso currentishape = -1 Then Return
        Dim e = CType(e2, Windows.Forms.MouseEventArgs)
        Dim d = If(e.Button = Windows.Forms.MouseButtons.Left, 1, -1)
        currentilimb = (currentilimb + d + stick.Limbs.Count) Mod stick.Limbs.Count
        currentishape = (currentishape + d + stick.Shapes.Count) Mod stick.Shapes.Count
        stick.Draw(0, 0, 480, 480, Me)
        Invalidate()
    End Sub

    Protected Overrides Sub OnPaint(e As System.Windows.Forms.PaintEventArgs)
        SyncLock bmp
            e.Graphics.DrawImageUnscaled(bmp, 0, 0)
            If debuginteractive Then
                Dim s = ""
                If currentilimb <> -1 Then s &= "limb(" & currentilimb & ") "
                If currentishape <> -1 Then s &= "shape(" & currentishape & ") "
                If s <> "" Then e.Graphics.DrawString(s, New Drawing.Font("Courier New", 12), Drawing.Brushes.Azure, 0, 0)
            End If
        End SyncLock
    End Sub

    Public Function LoadBitmapAsync(name As String, src As IO.Stream) As Task Implements IStickRenderer.LoadBitmapAsync
        Dim b = CType(Drawing.Image.FromStream(src), Drawing.Bitmap)
        Dim UseTopleftPixelAsTransparent = Not name.EndsWith("-NT")
        Bitmaps.Add(name, Tuple.Create(b, UseTopleftPixelAsTransparent))
        Return Task.FromResult(0)
    End Function

    Public Function GetBitmapSize(name As String) As Tuple(Of Integer, Integer) Implements IStickRenderer.GetBitmapSize
        Dim b = Bitmaps(name).Item1
        Return Tuple.Create(b.Width, b.Height)
    End Function

    Public Sub DrawBitmap(name As String, x As Integer, y As Integer, w As Integer, h As Integer, m11 As Double, m12 As Double, m21 As Double, m22 As Double, dx As Double, dy As Double, tag As Object) Implements IStickRenderer.DrawBitmap
        Log(String.Format("{1}{0}bitmap{0}""{2}""{0}x={3}{0}y={4}{0}w={5}{0}h={6}", vbTab, tag, name, x, y, w, h))

        SyncLock bmp
            Dim b = Bitmaps(name).Item1
            Dim destRect As New Drawing.Rectangle(0, 0, w, h)
            g.PixelOffsetMode = Drawing.Drawing2D.PixelOffsetMode.Half
            g.InterpolationMode = Drawing.Drawing2D.InterpolationMode.NearestNeighbor
            g.SmoothingMode = Drawing.Drawing2D.SmoothingMode.AntiAlias

            g.MultiplyTransform(New Drawing.Drawing2D.Matrix(CSng(m11), CSng(m12), CSng(m21), CSng(m22), CSng(x + dx), CSng(y + dy)))
            Dim transparency As New Drawing.Imaging.ImageAttributes
            If Bitmaps(name).Item2 Then
                If b.Palette.Entries.Count > 0 AndAlso b.Palette.Entries.Count <= 2 Then
                    Dim cm As New Drawing.Imaging.ColorMatrix
                    cm.Matrix00 = 10
                    cm.Matrix11 = 10
                    cm.Matrix22 = 10
                    cm.Matrix33 = 0.3
                    transparency.SetColorMatrix(cm)
                Else
                    Dim color = b.GetPixel(0, 0)
                    transparency.SetColorKey(color, color)
                End If
            End If
            g.DrawImage(b, destRect, 0, 0, b.Width, b.Height, Drawing.GraphicsUnit.Pixel, transparency)
            g.ResetTransform()
        End SyncLock
    End Sub

    Public Sub Line(x0 As Integer, y0 As Integer, x1 As Integer, y1 As Integer, thickness As Double, color As StickRGB, tag As Object) Implements IStickRenderer.Line
        Log(String.Format("{1}{0}line{0}x0={2}{0}y0={3}{0}{4}{0}{5}", vbTab, tag, x0, y0, x1, y1))
        AdjustColors(tag, thickness, color, Nothing)
        SyncLock bmp
            If x0 = x1 AndAlso y0 = y1 Then x1 += 1
            If color.IsVisible Then g.DrawLine(Pen(color, thickness), x0, y0, x1, y1)
        End SyncLock
    End Sub

    Public Sub Arc(x As Integer, y As Integer, w As Integer, h As Integer, startAngle As Double, sweepAngle As Double, thickness As Double, color As StickRGB, tag As Object) Implements IStickRenderer.Arc
        Log(String.Format("{1}{0}arc{0}lth={2:f2}{0}lc={3}{0}x={4}{0}y={5}{0}w={6}{0}h={7}{0}sta={8:f2}{0}swa={9:f2}", vbTab, tag, thickness, color, x, y, w, h, startAngle, sweepAngle))
        AdjustColors(tag, thickness, color, Nothing)
        SyncLock bmp
            Dim startAngleDeg = CSng(startAngle * 360 / 2 / Math.PI)
            Dim sweepAngleDeg = CSng(sweepAngle * 360 / 2 / Math.PI)
            If color.IsVisible Then
                g.DrawArc(Pen(color, thickness), x, y, w, h, startAngleDeg, sweepAngleDeg)
            End If
        End SyncLock
    End Sub

    Public Sub Rectangle(x As Integer, y As Integer, w As Integer, h As Integer, linethickness As Double, linecolor As StickRGB, fillcolor As StickRGB, tag As Object) Implements IStickRenderer.Rectangle
        Log(String.Format("{1}{0}rectangle{0}lth={2:f2}{0}lc={3}{0}fc={4}{0}x={5}{0}y={6}{0}w={7}{0}h={8}", vbTab, tag, linethickness, linecolor, fillcolor, x, y, w, h))
        AdjustColors(tag, linethickness, linecolor, fillcolor)
        SyncLock bmp
            If fillcolor.IsVisible Then g.FillRectangle(Brush(fillcolor), x, y, w, h)
            If linecolor.IsVisible Then g.DrawRectangle(Pen(linecolor, linethickness), x, y, w, h)
        End SyncLock
    End Sub

    Public Sub Ellipse(x As Integer, y As Integer, w As Integer, h As Integer, linethickness As Double, linecolor As StickRGB, fillcolor As StickRGB, tag As Object) Implements IStickRenderer.Ellipse
        Log(String.Format("{1}{0}ellipse{0}lth={2:f2}{0}lc={3}{0}fc={4}{0}x={5}{0}y={6}{0}w={7}{0}h={8}", vbTab, tag, linethickness, linecolor, fillcolor, x, y, w, h))
        AdjustColors(tag, linethickness, linecolor, fillcolor)
        SyncLock bmp
            If fillcolor.IsVisible Then g.FillEllipse(Brush(fillcolor), x, y, w, h)
            If linecolor.IsVisible Then g.DrawEllipse(Pen(linecolor, linethickness), x, y, w, h)
        End SyncLock
    End Sub

    Public Sub Polygon(pts As List(Of Tuple(Of Integer, Integer)), FillAlternate As Boolean, linethickness As Double, linecolor As StickRGB, fillcolor As StickRGB, tag As Object) Implements IStickRenderer.Polygon
        Dim spts = String.Join(":", From pt In pts Select String.Format("{0},{1}", pt.Item1, pt.Item2))
        Log(String.Format("{1}{0}polygon{0}lth={2:f2}{0}lc={3}{0}fc={4}{0}pts=({5})", vbTab, tag, linethickness, linecolor, fillcolor, spts))
        AdjustColors(tag, linethickness, linecolor, fillcolor)
        SyncLock bmp
            If fillcolor.IsVisible Then g.FillPolygon(Brush(fillcolor), (From p In pts Select New Drawing.Point(p.Item1, p.Item2)).ToArray(), If(FillAlternate, Drawing.Drawing2D.FillMode.Alternate, Drawing.Drawing2D.FillMode.Winding))
            If linecolor.IsVisible Then g.DrawPolygon(Pen(linecolor, linethickness), (From p In pts Select New Drawing.Point(p.Item1, p.Item2)).ToArray())
        End SyncLock
    End Sub

End Class
