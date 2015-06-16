Imports System.Numerics
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.UI
Imports Microsoft.Graphics.Canvas.UI.Xaml

Public NotInheritable Class MainPage
    Inherits Page

    Dim bitmap As CanvasBitmap
    Dim particles As New LinkedList(Of Particle)

    Class Particle
        Public Position, Velocity, Acceleration As Vector2
        Public Scale, Rotation, RotationSpeed As Single
        Public Lifetime, TimeSinceStart As Single
    End Class

    Sub CreateResources(sender As CanvasControl, e As CanvasCreateResourcesEventArgs) Handles canvas1.CreateResources
        Dim loadTask = Async Function()
                           bitmap = Await CanvasBitmap.LoadAsync(canvas1, "explosion.png")
                       End Function

        e.TrackAsyncAction(loadTask().AsAsyncAction)
    End Sub

    Sub UpdateParticles()
        Dim elapsedTime = 1 / 60.0F

        Dim node = particles.First
        While node IsNot Nothing
            Dim particle = node.Value
            Dim nextNode = node.Next
            particle.Velocity += particle.Acceleration * elapsedTime
            particle.Position += particle.Velocity * elapsedTime
            particle.Rotation += particle.RotationSpeed * elapsedTime
            particle.TimeSinceStart += elapsedTime
            If (particle.TimeSinceStart >= particle.Lifetime) Then particles.Remove(node)
            node = nextNode
        End While
    End Sub

    Sub CreateNewParticles()
        Static Dim RND As New Random()
        Dim position = New Vector2(CSng(ActualWidth * RND.NextDouble()), CSng(ActualHeight * RND.NextDouble()))
        For i = 0 To RND.Next(20, 25)
            Dim particle As New Particle()
            Dim angle = RND.NextDouble() * Math.PI * 2
            Dim speed = CSng(30 + 270 * RND.NextDouble())
            particle.Velocity = New Vector2(CSng(speed * Math.Cos(angle)), CSng(speed * Math.Sin(angle)))
            particle.Lifetime = CSng(0.5 + 0.5 * RND.NextDouble())
            particle.Acceleration = -particle.Velocity / particle.Lifetime
            particle.Scale = CSng(0.3 + 0.7 * RND.NextDouble())
            particle.RotationSpeed = CSng(-Math.PI * 0.25 + Math.PI * 0.5 * RND.NextDouble())
            particle.Rotation = CSng(2 * Math.PI * RND.NextDouble())
            particle.Position = position
            particle.TimeSinceStart = 0.0F
            particles.AddLast(particle)
        Next
    End Sub

    Sub Draw(sender As CanvasControl, e As CanvasDrawEventArgs) Handles canvas1.Draw
        UpdateParticles()

        Static Dim count As Integer = 0
        count += 1
        If count Mod 20 = 0 Then CreateNewParticles() : count = 0

        e.DrawingSession.Blend = CanvasBlend.Add
        For Each particle In particles
            Dim normalizedLifetime = particle.TimeSinceStart / particle.Lifetime
            Dim alpha = 4 * normalizedLifetime * (1 - normalizedLifetime)
            Dim scale = particle.Scale * (0.75F + 0.25F * normalizedLifetime)
            Dim bitmapCenter = bitmap.Size.ToVector2() * 0.5F
            Dim Transform = Matrix3x2.CreateRotation(particle.Rotation, bitmapCenter) *
                            Matrix3x2.CreateScale(scale, bitmapCenter) *
                            Matrix3x2.CreateTranslation(particle.Position - bitmapCenter)
            e.DrawingSession.DrawImage(bitmap, 0, 0, bitmap.Bounds, alpha, CanvasImageInterpolation.Linear, New Matrix4x4(Transform))
        Next
        ' This drawing takes about 10ms on my budget Lumia635 phone.

        canvas1.Invalidate() ' causes a new Draw to be executed next screen refresh
    End Sub

End Class

