Imports System.Numerics
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.UI
Imports Microsoft.Graphics.Canvas.UI.Xaml
Imports Windows.UI

Public NotInheritable Class MainPage
    Inherits Page

    Dim bmp(2) As CanvasBitmap
    Dim pos(2) As Vector2
    Dim vel(2) As Vector2
    Dim rot(2) As Single

    Dim SPEED As Single = 10

    Sub CreateResources(sender As CanvasControl, e As CanvasCreateResourcesEventArgs) Handles canvas1.CreateResources
        Dim loadTask =
            Async Function()
                For i = 1 To 2
                    bmp(i) = Await CanvasBitmap.LoadAsync(canvas1, $"ball{i}.png")
                    pos(i) = New Vector2(Rnd(ActualWidth), Rnd(ActualHeight))
                    vel(i) = New Vector2(Rnd(-SPEED, SPEED), Rnd(-SPEED, SPEED))
                Next
            End Function

        e.TrackAsyncAction(loadTask().AsAsyncAction)
    End Sub


    Sub Update()
        Dim elapsedTime = 1 / 60.0F

        For i = 1 To 2
            If pos(i).X > ActualWidth Then vel(i).X = -Rnd(SPEED)
            If pos(i).X < 0 Then vel(i).X = Rnd(SPEED)
            If pos(i).Y > ActualHeight Then vel(i).Y = -Rnd(SPEED)
            If pos(i).Y < 0 Then vel(i).Y = Rnd(SPEED)

            pos(i) += vel(i)
            rot(i) += 0.1F
        Next

    End Sub

    Sub Draw(sender As CanvasControl, e As CanvasDrawEventArgs) Handles canvas1.Draw
        Update()

        e.DrawingSession.DrawLine(pos(1), pos(2), Colors.Azure)

        For i = 1 To 2
            Dim center = bmp(i).Size.ToVector2() * 0.5F
            Dim xform = Matrix3x2.CreateRotation(rot(i), center) *
                        Matrix3x2.CreateScale(0.2, center) *
                        Matrix3x2.CreateTranslation(pos(i) - center)
            e.DrawingSession.DrawImage(bmp(i), 0, 0, bmp(i).Bounds, 1, CanvasImageInterpolation.Linear, New Matrix4x4(xform))
        Next

        canvas1.Invalidate()
    End Sub


    Function Rnd(max As Double) As Single
        Return rnd(0, max)
    End Function

    Function Rnd(min As Double, max As Double) As Single
        Static Dim r As New Random
        Return CSng(min + (max - min) * r.NextDouble())
    End Function

End Class
