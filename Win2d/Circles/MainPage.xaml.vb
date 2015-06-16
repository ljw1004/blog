Imports System.Numerics
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.UI
Imports Microsoft.Graphics.Canvas.UI.Xaml
Imports Windows.UI

Public NotInheritable Class MainPage
    Inherits Page

    Dim ZOOM_RATE As Double = 0.8
    Dim ROTATE_RATE As Double = 0.5
    Dim COLOR_RATE As Double = 0.07

    Dim Angle0 As Double = 0 ' initial rotation of element0
    Dim Hue0 As Double = 0 ' HSV colour of element0
    Dim OffX0, OffY0 As Double ' Offset of element0
    Dim Zoom0 As Double = 0 ' 0-1 of how zoomed we are between element0 and 1
    Dim DAngle As Double = 0 ' how much each bit is rotated by

    Private Sub Update()
        Dim elapsedTime = 1 / 60.0F

        Zoom0 = Zoom0 + ZOOM_RATE * elapsedTime
        DAngle = (DAngle + ROTATE_RATE * elapsedTime) Mod 2 * Math.PI
        Angle0 = (Angle0 + ROTATE_RATE * elapsedTime) Mod 2 * Math.PI

        If Zoom0 >= 1 Then
            Zoom0 -= 1
            Angle0 = Angle0 + DAngle
            Hue0 = (Hue0 + COLOR_RATE) Mod 1
        End If

        ' Calculate offset of element0 so that the furthest thing in we see remains centered...
        OffX0 = 0 : OffY0 = 0
        Dim angle = Angle0, radius = 1 * Math.Pow(2, Zoom0)
        Do
            angle += DAngle : radius /= 2
            OffX0 += radius * Math.Sin(angle)
            OffY0 += radius * Math.Cos(angle)
        Loop Until radius * ActualHeight <= 0.1

        ' ... but give it a little slow rolling
        OffX0 += 0.13 * Math.Sin(DAngle)
        OffY0 += 0.13 * Math.Cos(DAngle)
    End Sub



    Sub Draw(sender As CanvasControl, e As CanvasDrawEventArgs) Handles canvas1.Draw
        Update()

        Dim w = ActualWidth, h = ActualHeight

        ' Figure out the largest enclosing circle that encompases the whole screen
        Dim cx = w / 2 - OffX0 * h, cy = h / 2 - OffY0 * h
        Dim radius = h * Math.Pow(2, Zoom0)
        Dim angle = Angle0
        Dim hue = Hue0
        Dim saturation = Math.Pow(0.8, 2 - Zoom0)
        Do
            If radius > w * 8 Then Exit Do
            If cx - radius < -w * 2 AndAlso cx + radius > w * 3 AndAlso cy - radius < -w * 2 AndAlso cy + radius > w * 3 Then Exit Do
            hue -= COLOR_RATE
            saturation = Math.Sqrt(saturation)
            cx -= Math.Sin(angle) * radius : cy -= Math.Cos(angle) * radius
            angle -= DAngle
            radius *= 2
        Loop

        DrawMyCircles(e.DrawingSession, hue, saturation, True, angle, cx, cy, radius)
        ' Takes 20-40ms on my Lumia635 budget phone, depending on the size of the circles

        canvas1.Invalidate()
    End Sub


    Sub DrawMyCircles(g As CanvasDrawingSession, hue As Double, saturation As Double, isFilled As Boolean, angle As Double, centerX As Double, centerY As Double, radius As Double)
        If radius < 0.5 Then Return
        If centerX + radius < 0 OrElse centerX - radius > ActualWidth Then Return
        If centerY + radius < 0 OrElse centerY - radius > ActualHeight Then Return
        '
        If isFilled Then
            Dim c = ColFromHSV(hue, 1 - saturation, 1 - saturation)
            g.FillEllipse(CSng(centerX), CSng(centerY), CSng(radius), CSng(radius), c)
        End If
        g.DrawEllipse(CSng(centerX), CSng(centerY), CSng(radius), CSng(radius), Color.FromArgb(80, 255, 255, 255))
        '
        angle += DAngle
        Dim dx = Math.Sin(angle), dy = Math.Cos(angle)
        Dim subf = saturation : If isFilled Then subf *= subf
        Dim subco = hue : If isFilled Then
            subco = subco + COLOR_RATE
        End If
        DrawMyCircles(g, subco, subf, isFilled, angle, centerX + dx * radius / 2, centerY + dy * radius / 2, radius / 2)
        DrawMyCircles(g, hue, saturation, False, angle, centerX - dx * radius / 2, centerY - dy * radius / 2, radius / 2)
    End Sub


    Function ColFromHSV(h As Double, s As Double, v As Double) As Color
        If s < 0 OrElse s > 1 Then Throw New ArgumentOutOfRangeException(NameOf(s))
        If v < 0 OrElse v > 1 Then Throw New ArgumentOutOfRangeException(NameOf(v))
        h = h Mod 1 : If h < 0 Then h = h + 1
        '
        Dim i = CInt(Math.Floor(h * 6)) ' which of the six segments is the colour in? 0 <= i < 6
        Dim f = 6 * h - i ' how far around that segment? 0 <= f < 6
        Dim p1 = v * (1 - s)
        Dim p2 = v * (1 - (s * f))
        Dim p3 = v * (1 - (s * (1 - f)))
        Dim rgb As Double()
        Select Case i
            Case 0 : rgb = {v, p3, p1}
            Case 1 : rgb = {p2, v, p1}
            Case 2 : rgb = {p1, v, p3}
            Case 3 : rgb = {p1, p2, v}
            Case 4 : rgb = {p3, p1, v}
            Case Else : rgb = {v, p1, p2}
        End Select
        Return Color.FromArgb(255, CByte(rgb(0) * 255), CByte(rgb(1) * 255), CByte(rgb(2) * 255))
    End Function

End Class

