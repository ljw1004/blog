Option Strict On
Imports Windows.UI.Xaml.Controls
Imports System.Threading.Tasks
Imports Windows.UI.Xaml.Media
Imports System.IO.WindowsRuntimeStreamExtensions
Imports System.IO.WindowsRuntimeStorageExtensions
Imports Windows.UI.Xaml.Media.Imaging
Imports System.IO
Imports Windows.UI.Xaml

Namespace Global

    Public Class Win8StickRenderer
        Implements IStickRenderer
        Dim Canvas As Windows.UI.Xaml.Controls.Canvas
        Dim Elements As New Dictionary(Of Object, Windows.UI.Xaml.UIElement)
        Dim Bitmaps As New Dictionary(Of String, BitmapSource)

        Function Element(Of T As {Windows.UI.Xaml.UIElement, New})(tag As Object) As T
            Dim e As Windows.UI.Xaml.UIElement = Nothing
            If Elements.TryGetValue(tag, e) Then
                Dim re = TryCast(e, T)
                If Not re Is Nothing Then Return re
                Canvas.Children.Remove(e)
                Elements.Remove(tag)
            End If
            Dim r As New T
            Elements.Add(tag, r)
            Canvas.Children.Add(r)
            Return r
        End Function

        Sub New(Canvas As Canvas)
            Me.Canvas = Canvas
        End Sub


        Public Async Function LoadBitmapAsync(name As String, src As Stream) As Task Implements IStickRenderer.LoadBitmapAsync
            ' First, we need to convert from the src-stream (which contains JPEG bits) into an IMemoryStream to load it into win8
            Using srcms As New Windows.Storage.Streams.InMemoryRandomAccessStream
                Using srcs = srcms.AsStreamForWrite() ' nb. disposing "srcs" will also dispose srcms, so we don't do that yet
                    Dim format = ""
                    Dim buf = New Byte(16384) {}
                    Do
                        Dim red = Await src.ReadAsync(buf, 0, buf.Length)
                        If red = 0 Then Exit Do
                        Await srcs.WriteAsync(buf, 0, red)
                        If format <> "" Then Continue Do
                        ' First time around, assume that we read enough bytes for the header, and check file format
                        If red > 50 AndAlso buf(0) = 66 AndAlso buf(1) = 77 Then
                            format = "BMP"
                            Dim BitmapInfoHeaderSize = BitConverter.ToInt32(buf, 14)
                            Dim PaletteCount = BitConverter.ToInt32(buf, 46)
                            If BitmapInfoHeaderSize >= 40 AndAlso PaletteCount = 1 Then format = "BMP_monochrome"
                        ElseIf red > 8 AndAlso buf(0) = &H89 AndAlso buf(1) = &H50 AndAlso buf(2) = &H4E AndAlso buf(3) = &H47 Then
                            format = "PNG"
                        ElseIf red > 18 AndAlso buf(0) = &HFF AndAlso (buf(1) = &HE0 OrElse buf(1) = &HD8) Then
                            format = "JPEG"
                        Else
                            format = "???"
                        End If
                    Loop
                    Await srcs.FlushAsync()

                    ' Next, load an image from the src-stream
                    srcms.Seek(0)
                    Dim decoder = Await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(srcms)
                    Dim frame = Await decoder.GetFrameAsync(0)
                    Dim pixelprovider = Await frame.GetPixelDataAsync(
                                        Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8,
                                        Windows.Graphics.Imaging.BitmapAlphaMode.Straight,
                                        New Windows.Graphics.Imaging.BitmapTransform,
                                        Windows.Graphics.Imaging.ExifOrientationMode.RespectExifOrientation,
                                        Windows.Graphics.Imaging.ColorManagementMode.DoNotColorManage)

                    ' Get at the raw pixel bits
                    Dim pixels As Byte() = pixelprovider.DetachPixelData() ' premultiplied (i.e. if we want alpha=0 then we must set rgb=0 as well

                    ' Do color-keyed alpha if necessary
                    If Not name.EndsWith("-NT") AndAlso (format = "BMP" OrElse format = "JPEG") Then
                        Dim key As Integer() = {pixels(0), pixels(1), pixels(2), pixels(3)}
                        For i = 0 To pixels.Length - 1 Step 4
                            Dim diff = Math.Abs(pixels(i + 0) - key(0)) + Math.Abs(pixels(i + 1) - key(1)) + Math.Abs(pixels(i + 2) - key(2))
                            If diff <= 3 Then ' chroma-keying is a little fuzzy
                                pixels(i + 0) = 0 : pixels(i + 1) = 0 : pixels(i + 2) = 0
                                pixels(i + 3) = 0
                            End If
                        Next
                    ElseIf Not name.EndsWith("-NT") AndAlso format = "BMP_monochrome" Then
                        ' An idiosyncracit back-compat alpha format relating to an old GDI way of doing alpha
                        For i = 0 To pixels.Length - 1 Step 4
                            pixels(i + 3) = 30
                        Next
                    End If

                    ' Create a XAML-bitmap out of that Graphics-bitmap
                    Dim bmp As New Windows.UI.Xaml.Media.Imaging.WriteableBitmap(CInt(decoder.OrientedPixelWidth), CInt(decoder.OrientedPixelHeight))
                    Using writer = Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions.AsStream(bmp.PixelBuffer)
                        writer.Write(pixels, 0, pixels.Length - 1)
                    End Using
                    Bitmaps.Add(name, bmp)

                End Using
            End Using
        End Function

        Public Function GetBitmapSize(name As String) As Tuple(Of Integer, Integer) Implements IStickRenderer.GetBitmapSize
            Dim bi = Bitmaps(name)
            Return Tuple.Create(bi.PixelWidth, bi.PixelHeight)
        End Function

        Public Sub DrawBitmap(name As String, x As Integer, y As Integer, w As Integer, h As Integer, m11 As Double, m12 As Double, m21 As Double, m22 As Double, dx As Double, dy As Double, tag As Object) Implements IStickRenderer.DrawBitmap
            Dim r = Element(Of Windows.UI.Xaml.Controls.Image)(tag)
            r.Visibility = Visibility.Visible
            Dim rr As New Windows.Foundation.Rect
            '
            r.Source = Bitmaps(name)
            r.Width = w
            r.Height = h
            Windows.UI.Xaml.Controls.Canvas.SetLeft(r, x)
            Windows.UI.Xaml.Controls.Canvas.SetTop(r, y)

            Dim transform = TryCast(r.RenderTransform, Windows.UI.Xaml.Media.MatrixTransform)
            If transform Is Nothing Then transform = New Windows.UI.Xaml.Media.MatrixTransform
            transform.Matrix = New Matrix(m11, m12, m21, m22, dx, dy)
            r.RenderTransform = transform
        End Sub

        Public Sub Rectangle(x As Integer, y As Integer, w As Integer, h As Integer, linethickness As Double, linecolor As StickRGB, fillcolor As StickRGB, tag As Object) Implements IStickRenderer.Rectangle
            Dim r = Element(Of Windows.UI.Xaml.Shapes.Rectangle)(tag)
            If Not linecolor.IsVisible AndAlso Not fillcolor.IsVisible Then r.Visibility = Visibility.Collapsed : Return
            r.Visibility = Visibility.Visible
            '
            r.Width = w
            r.Height = h
            Windows.UI.Xaml.Controls.Canvas.SetLeft(r, x)
            Windows.UI.Xaml.Controls.Canvas.SetTop(r, y)

            If linecolor.IsVisible Then
                Dim lc As New Windows.UI.Color With {.A = 255, .R = CByte(linecolor.r), .G = CByte(linecolor.g), .B = CByte(linecolor.b)}
                Dim lb As New Windows.UI.Xaml.Media.SolidColorBrush(lc)
                r.Stroke = lb
                r.StrokeThickness = linethickness
            Else
                r.Stroke = Nothing
            End If

            If fillcolor.IsVisible Then
                Dim fc As New Windows.UI.Color With {.A = 255, .R = CByte(fillcolor.r), .G = CByte(fillcolor.g), .B = CByte(fillcolor.b)}
                Dim fb As New Windows.UI.Xaml.Media.SolidColorBrush(fc)
                r.Fill = fb
            Else
                r.Fill = Nothing
            End If
        End Sub

        Public Sub Arc(x As Integer, y As Integer, w As Integer, h As Integer, startAngle As Double, sweepAngle As Double, thickness As Double, linecolor As StickRGB, tag As Object) Implements IStickRenderer.Arc
            Dim r = Element(Of Windows.UI.Xaml.Shapes.Path)(tag)
            If Not linecolor.IsVisible Then r.Visibility = Visibility.Collapsed : Return
            r.Visibility = Visibility.Visible

            Dim cx = x + w / 2, cy = y + h / 2
            Dim radius = w / 2, scaley = h / w
            If sweepAngle >= Math.PI * 2 Then sweepAngle = Math.PI * 1.9999

            Dim geometry = TryCast(r.Data, Windows.UI.Xaml.Media.PathGeometry)
            If geometry Is Nothing Then geometry = New Windows.UI.Xaml.Media.PathGeometry : r.Data = geometry

            Dim figure = geometry.Figures.FirstOrDefault
            If figure Is Nothing Then figure = New Windows.UI.Xaml.Media.PathFigure : geometry.Figures.Add(figure)
            figure.StartPoint = New Windows.Foundation.Point(cx + radius * Math.Cos(startAngle), cy + radius * Math.Sin(startAngle) * scaley)

            Dim segment = TryCast(figure.Segments.FirstOrDefault, Windows.UI.Xaml.Media.ArcSegment)
            If segment Is Nothing Then segment = New Windows.UI.Xaml.Media.ArcSegment : figure.Segments.Add(segment)
            segment.IsLargeArc = (sweepAngle >= Math.PI)
            segment.Point = New Windows.Foundation.Point(cx + radius * Math.Cos(startAngle + sweepAngle), cy + radius * Math.Sin(startAngle + sweepAngle) * scaley)
            segment.SweepDirection = SweepDirection.Clockwise
            segment.Size = New Windows.Foundation.Size(w / 2, h / 2)

            If linecolor.IsVisible Then
                Dim lc As New Windows.UI.Color With {.A = 255, .R = CByte(linecolor.r), .G = CByte(linecolor.g), .B = CByte(linecolor.b)}
                Dim lb As New Windows.UI.Xaml.Media.SolidColorBrush(lc)
                r.Stroke = lb
                r.StrokeThickness = thickness
            Else
                r.Stroke = Nothing
            End If
        End Sub

        Public Sub Ellipse(x As Integer, y As Integer, w As Integer, h As Integer, linethickness As Double, linecolor As StickRGB, fillcolor As StickRGB, tag As Object) Implements IStickRenderer.Ellipse
            Dim r = Element(Of Windows.UI.Xaml.Shapes.Ellipse)(tag)
            If Not linecolor.IsVisible AndAlso Not fillcolor.IsVisible Then r.Visibility = Visibility.Collapsed : Return
            r.Visibility = Visibility.Visible
            '
            r.Width = w
            r.Height = h
            Windows.UI.Xaml.Controls.Canvas.SetLeft(r, x)
            Windows.UI.Xaml.Controls.Canvas.SetTop(r, y)

            If linecolor.IsVisible Then
                Dim lc As New Windows.UI.Color With {.A = 255, .R = CByte(linecolor.r), .G = CByte(linecolor.g), .B = CByte(linecolor.b)}
                Dim lb As New Windows.UI.Xaml.Media.SolidColorBrush(lc)
                r.Stroke = lb
                r.StrokeThickness = linethickness
            Else
                r.Stroke = Nothing
            End If

            If fillcolor.IsVisible Then
                Dim fc As New Windows.UI.Color With {.A = 255, .R = CByte(fillcolor.r), .G = CByte(fillcolor.g), .B = CByte(fillcolor.b)}
                Dim fb As New Windows.UI.Xaml.Media.SolidColorBrush(fc)
                r.Fill = fb
            Else
                r.Fill = Nothing
            End If
        End Sub

        Public Sub Line(x0 As Integer, y0 As Integer, x1 As Integer, y1 As Integer, thickness As Double, linecolor As StickRGB, tag As Object) Implements IStickRenderer.Line
            Dim r = Element(Of Windows.UI.Xaml.Shapes.Line)(tag)
            If Not linecolor.IsVisible Then r.Visibility = Visibility.Collapsed : Return
            r.Visibility = Visibility.Visible
            '
            r.X1 = x0
            r.Y1 = y0
            r.X2 = x1
            r.Y2 = y1
            If linecolor.IsVisible Then
                Dim lc As New Windows.UI.Color With {.A = 255, .R = CByte(linecolor.r), .G = CByte(linecolor.g), .B = CByte(linecolor.b)}
                Dim lb As New Windows.UI.Xaml.Media.SolidColorBrush(lc)
                r.Stroke = lb
                r.StrokeThickness = thickness
            Else
                r.Stroke = Nothing
            End If
        End Sub

        Public Sub Polygon(pts As List(Of Tuple(Of Integer, Integer)), FillAlternate As Boolean, linethickness As Double, linecolor As StickRGB, fillcolor As StickRGB, tag As Object) Implements IStickRenderer.Polygon
            Dim r = Element(Of Windows.UI.Xaml.Shapes.Path)(tag)
            If Not linecolor.IsVisible AndAlso Not fillcolor.IsVisible Then r.Visibility = Visibility.Collapsed : Return
            r.Visibility = Visibility.Visible

            Dim geometry = TryCast(r.Data, Windows.UI.Xaml.Media.PathGeometry)
            If geometry Is Nothing Then geometry = New Windows.UI.Xaml.Media.PathGeometry : r.Data = geometry

            Dim figure = geometry.Figures.FirstOrDefault
            If figure Is Nothing Then figure = New Windows.UI.Xaml.Media.PathFigure : geometry.Figures.Add(figure)
            figure.StartPoint = New Windows.Foundation.Point(pts(0).Item1, pts(0).Item2)

            For i = 1 To pts.Count - 1
                Dim segment As Windows.UI.Xaml.Media.LineSegment
                If figure.Segments.Count >= i Then segment = CType(figure.Segments(i - 1), Windows.UI.Xaml.Media.LineSegment) Else segment = New Windows.UI.Xaml.Media.LineSegment : figure.Segments.Add(segment)
                segment.Point = New Windows.Foundation.Point(pts(i).Item1, pts(i).Item2)
            Next
            While figure.Segments.Count > pts.Count - 1
                figure.Segments.RemoveAt(figure.Segments.Count - 1)
            End While

            If linecolor.IsVisible Then
                Dim lc As New Windows.UI.Color With {.A = 255, .R = CByte(linecolor.r), .G = CByte(linecolor.g), .B = CByte(linecolor.b)}
                Dim lb As New Windows.UI.Xaml.Media.SolidColorBrush(lc)
                r.Stroke = lb
                r.StrokeThickness = linethickness
            Else
                r.Stroke = Nothing
            End If

            If fillcolor.IsVisible Then
                Dim fc As New Windows.UI.Color With {.A = 255, .R = CByte(fillcolor.r), .G = CByte(fillcolor.g), .B = CByte(fillcolor.b)}
                Dim fb As New Windows.UI.Xaml.Media.SolidColorBrush(fc)
                r.Fill = fb
            Else
                r.Fill = Nothing
            End If
        End Sub

    End Class


End Namespace
