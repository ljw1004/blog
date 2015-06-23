Imports System.Numerics
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Effects
Imports Microsoft.Graphics.Canvas.UI.Xaml
Imports Windows.System
Imports Windows.Storage
Imports Windows.Graphics.DirectX
Imports Windows.UI
Imports Windows.UI.Core


' Differences from V1:
'
' This code shows in greyscale  the number of iterations needed for convergence: instead of
' only calculating "d = a*a + b*b" at the end of the iteration, it calculates it every step
' of the way. At every iteration, every pixel in "d" that diverged (is greater than 4) adds
' a small bit of lightness. In this way a pixel that diverges quickly is very white, and one
' that diverges slowly is more grey, and one that diverges not at all is black.
'
' Incidentally, phones typically don't support "Double" floating point precision numbers (64bits)
' They don't even support "Single" precision (32bits)
' Instead they have "Half" precision floating points (16bits).
' This half-precision floating-point isn't supported by .NET, and isn't supported by Win2d,
' so we roll our own routine "GetHalfFloatBytes" which turns a double-precision floating point
' number into the correct bit pattern for half-precision, and we write raw bytes to the surface.
'
' And also neither phone nor desktop supports bitmaps with just a single floating point value
' per pixel. They only work with four values (R,G,B,A) per pixel. We will work around this
' by "multiplexing", having each R,G,B value be a different adjacent value of "x" for purposes
' of iteration, and then demultiplexing them at the end.


Public NotInheritable Class MainPageV2
    Inherits Page

    Dim CSIZE As Integer = 600 ' size in pixels of our mandlebrot calculation
    Dim CSIZEX As Integer = CSIZE \ 3
    Dim CITER As Integer = 50  ' how many iterations to do.
    Dim MaxCanvasSize As Double ' for tracking perf
    Dim Perf As New LinkedList(Of Double) ' for tracking perf

    Dim MTopLeft As New Vector2(-2, -2) ' top-left corner of Mandlebrot
    Dim MSize As New Vector2(4, 4)      ' size of mandlebrot
    ' and to track pinch-zoom and drag:
    Dim ManipulationStart_MSize, ManipulationStart_MTopLeft, ManipulationStart_MCenterOn As Vector2

    Dim IsUpToDate As Boolean ' If MSize/MTopLeft change, then this gets reset, to indicate a recalc is needed
    Dim UnitX, UnitY, X, Y, A, A_prime, B, B_prime, Accumulator, MaskR, MaskG, MaskB, DrawBuffer As CanvasRenderTarget
    Dim RenderR, RenderG, RenderB As CompositeEffect
    Dim DrawEffect As Transform2DEffect

    WithEvents App As App = CType(Application.Current, App)
    WithEvents NavigationManager As SystemNavigationManager = SystemNavigationManager.GetForCurrentView

    Async Sub RefinePerf() Handles App.Launched
        Dim p = If(Perf.Count = 0, 100, Aggregate ms In Perf Into Average)

        If p < 20 AndAlso CSIZE < CInt(MaxCanvasSize) Then
            CSIZE = CSIZE * 3 \ 2
        ElseIf p < 60 AndAlso CSIZE < CInt(MaxCanvasSize) Then
            CSIZE = CSIZE * 5 \ 4
        ElseIf p < 20 Then
            CITER = CITER * 2
        ElseIf p < 60 Then
            CITER = CITER * 5 \ 4
        ElseIf p > 200 AndAlso CSIZE > 300 Then
            CSIZE = CSIZE \ 2
        ElseIf p > 100 AndAlso CSIZE > 300 Then
            CSIZE = CSIZE * 4 \ 5
        ElseIf p > 200 Then
            CITER = CITER \ 2
        ElseIf p > 100 Then
            CITER = CITER * 4 \ 5
        End If
        CSIZE = 3 * (CSIZE \ 3) ' has to be a multiple of 3
        If CSIZE >= MaxCanvasSize - 10 Then CSIZE = CInt(MaxCanvasSize)
        If CSIZE < 300 Then CSIZE = 300
        If CITER < 10 Then CITER = 10
        CSIZEX = CSIZE \ 3
        Await Task.Delay(1000)
        label1.Text = $"{Me.CSIZE}x{Me.CSIZE} pixels, {Me.CITER} iterations"
    End Sub

    Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
        SystemNavigationManager.GetForCurrentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible        '
        '
        Dim perf0 = CType(ApplicationData.Current.LocalSettings.Values("Perf"), Double?)
        Dim canvasSize0 = CType(ApplicationData.Current.LocalSettings.Values("MaxCanvasSize"), Double?)
        Dim citer0 = CType(ApplicationData.Current.LocalSettings.Values("CITER"), Integer?)
        Dim csize0 = CType(ApplicationData.Current.LocalSettings.Values("CSIZE"), Integer?)
        '
        If perf0.HasValue Then Perf.AddLast(perf0.Value)
        If canvasSize0.HasValue Then MaxCanvasSize = canvasSize0.Value
        If citer0.HasValue Then CITER = citer0.Value
        If csize0.HasValue Then CSIZE = csize0.Value : CSIZEX = CSIZE \ 3
    End Sub

    Sub SavePerf() Handles App.Suspending
        ApplicationData.Current.LocalSettings.Values("Perf") = Aggregate p In Perf Into Average
        ApplicationData.Current.LocalSettings.Values("MaxCanvasSize") = MaxCanvasSize
        ApplicationData.Current.LocalSettings.Values("CITER") = CITER
        ApplicationData.Current.LocalSettings.Values("CSIZE") = CSIZE
    End Sub

    Sub Page_SizeChanged() Handles Me.SizeChanged
        Dim s = Math.Max(Me.ActualWidth, Me.ActualHeight)
        canvas1.Width = s
        canvas1.Height = s
        MaxCanvasSize = Math.Max(MaxCanvasSize, s)
    End Sub

    Sub Canvas_CreateResources(sender As CanvasControl, args As Object) Handles canvas1.CreateResources
        Const defaultDpi = 96.0F
        IsUpToDate = False

        DrawBuffer = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.B8G8R8A8UIntNormalized, CanvasAlphaMode.Ignore)
        MaskR = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.B8G8R8A8UIntNormalized, CanvasAlphaMode.Ignore)
        MaskG = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.B8G8R8A8UIntNormalized, CanvasAlphaMode.Ignore)
        MaskB = New CanvasRenderTarget(canvas1, CSIZE, CSIZE, defaultDpi, DirectXPixelFormat.B8G8R8A8UIntNormalized, CanvasAlphaMode.Ignore)
        Dim createSurfaces = Sub(pixelFormat As DirectXPixelFormat)
                                 UnitX = New CanvasRenderTarget(canvas1, CSIZEX, CSIZE, defaultDpi, pixelFormat, CanvasAlphaMode.Ignore)
                                 UnitY = New CanvasRenderTarget(canvas1, CSIZEX, CSIZE, defaultDpi, pixelFormat, CanvasAlphaMode.Ignore)
                                 X = New CanvasRenderTarget(canvas1, CSIZEX, CSIZE, defaultDpi, pixelFormat, CanvasAlphaMode.Ignore)
                                 Y = New CanvasRenderTarget(canvas1, CSIZEX, CSIZE, defaultDpi, pixelFormat, CanvasAlphaMode.Ignore)
                                 A = New CanvasRenderTarget(canvas1, CSIZEX, CSIZE, defaultDpi, pixelFormat, CanvasAlphaMode.Ignore)
                                 A_prime = New CanvasRenderTarget(canvas1, CSIZEX, CSIZE, defaultDpi, pixelFormat, CanvasAlphaMode.Ignore)
                                 B = New CanvasRenderTarget(canvas1, CSIZEX, CSIZE, defaultDpi, pixelFormat, CanvasAlphaMode.Ignore)
                                 B_prime = New CanvasRenderTarget(canvas1, CSIZEX, CSIZE, defaultDpi, pixelFormat, CanvasAlphaMode.Ignore)
                                 Accumulator = New CanvasRenderTarget(canvas1, CSIZEX, CSIZE, defaultDpi, DirectXPixelFormat.B8G8R8A8UIntNormalized, CanvasAlphaMode.Ignore)
                             End Sub
        Try
            createSurfaces(DirectXPixelFormat.R32G32B32A32Float)
        Catch ex As Exception
            createSurfaces(DirectXPixelFormat.R16G16B16A16Float)
        End Try


        ' Initialize the "unitX and unitY" surfaces...
        Dim bpp = If(UnitX.Description.Format = DirectXPixelFormat.R32G32B32A32Float, 4, 2)
        Dim row = New Byte(bpp * 4 * CSIZEX - 1) {}
        Dim one = GetFloatBytes(bpp, 1.0F)
        For i = 0 To CSIZEX - 1
            Dim r = GetFloatBytes(bpp, CSng((i * 3 + 0) / (CSIZE - 1)))
            Dim g = GetFloatBytes(bpp, CSng((i * 3 + 1) / (CSIZE - 1)))
            Dim b = GetFloatBytes(bpp, CSng((i * 3 + 2) / (CSIZE - 1)))
            Array.Copy(r, 0, row, i * bpp * 4 + bpp * 0, bpp)
            Array.Copy(g, 0, row, i * bpp * 4 + bpp * 1, bpp)
            Array.Copy(b, 0, row, i * bpp * 4 + bpp * 2, bpp)
            Array.Copy(one, 0, row, i * bpp * 4 + bpp * 3, bpp)
        Next
        For i = 0 To CSIZE - 1
            UnitX.SetPixelBytes(row, 0, i, CSIZEX, 1)
        Next
        '
        Dim column = New Byte(bpp * 4 * CSIZE - 1) {}
        For i = 0 To CSIZE - 1
            Dim f = GetFloatBytes(bpp, CSng(i / (CSIZE - 1)))
            Array.Copy(f, 0, column, i * bpp * 4 + bpp * 0, bpp)
            Array.Copy(f, 0, column, i * bpp * 4 + bpp * 1, bpp)
            Array.Copy(f, 0, column, i * bpp * 4 + bpp * 2, bpp)
            Array.Copy(one, 0, column, i * bpp * 4 + bpp * 3, bpp)
        Next
        For i = 0 To CSIZEX - 1
            UnitY.SetPixelBytes(column, i, 0, 1, CSIZE)
        Next




        ' This is how rendering gets done, from the accumulator into the Render buffer
        ' During the iteration, we "multiplexed" each pixel so that each R,G,B component was
        ' being used for adjacent values of X. So the first job is to split that out
        ' into three pixels rgb(R,G,B), rgb(R,G,B), rgb(R,G,B)
        Dim render1 As New Transform2DEffect With {.Source = Accumulator, .TransformMatrix = Matrix3x2.CreateScale(3, 1), .InterpolationMode = CanvasImageInterpolation.NearestNeighbor}

        ' Next, split that into three separate bitmaps:
        ' renderR1:  rgb(R,R,R) rgb(R,R,R) rgb(R,R,R)
        ' renderG1:  rgb(G,G,G) rgb(G,G,G) rgb(G,G,G)
        ' renderB1:  rgb(B,B,B) rgb(B,B,B) rgb(B,B,B)
        Dim renderR1 As New ColorMatrixEffect With {.Source = render1, .ColorMatrix = New Matrix5x4 With {.M11 = 1, .M12 = 1, .M22 = 0, .M13 = 1, .M33 = 0, .M44 = 0, .M54 = 1}}
        Dim renderG1 As New ColorMatrixEffect With {.Source = render1, .ColorMatrix = New Matrix5x4 With {.M11 = 0, .M21 = 1, .M22 = 1, .M23 = 1, .M33 = 0, .M44 = 0, .M54 = 1}}
        Dim renderB1 As New ColorMatrixEffect With {.Source = render1, .ColorMatrix = New Matrix5x4 With {.M11 = 0, .M31 = 1, .M22 = 0, .M32 = 1, .M33 = 1, .M44 = 0, .M54 = 1}}

        ' Next, we'll "punch out" the three bitmaps with a stripe mask, so they can be added together:
        ' renderR:   rgb(R,R,R)     0          0
        ' renderG:       0      rgb(G,G,G)     0
        ' renderB:       0          0       rgb(B,B,B)
        Dim rangeR = New Color(CSIZE - 1) {}
        Dim rangeG = New Color(CSIZE - 1) {}
        Dim rangeB = New Color(CSIZE - 1) {}
        For i = 0 To CSIZEX - 1
            rangeR(i * 3 + 0) = Color.FromArgb(255, 0, 0, 0)
            rangeR(i * 3 + 1) = Color.FromArgb(0, 0, 0, 0)
            rangeR(i * 3 + 2) = Color.FromArgb(0, 0, 0, 0)
            rangeG(i * 3 + 0) = Color.FromArgb(0, 0, 0, 0)
            rangeG(i * 3 + 1) = Color.FromArgb(255, 0, 0, 0)
            rangeG(i * 3 + 2) = Color.FromArgb(0, 0, 0, 0)
            rangeB(i * 3 + 0) = Color.FromArgb(0, 0, 0, 0)
            rangeB(i * 3 + 1) = Color.FromArgb(0, 0, 0, 0)
            rangeB(i * 3 + 2) = Color.FromArgb(255, 0, 0, 0)
        Next
        For i = 0 To CSIZE - 1
            MaskR.SetPixelColors(rangeR, 0, i, CSIZE, 1)
            MaskG.SetPixelColors(rangeG, 0, i, CSIZE, 1)
            MaskB.SetPixelColors(rangeB, 0, i, CSIZE, 1)
        Next
        '
        RenderR = New CompositeEffect With {.Mode = CanvasComposite.DestinationIn}
        RenderR.Sources.Add(renderR1) : RenderR.Sources.Add(MaskR)
        RenderG = New CompositeEffect With {.Mode = CanvasComposite.DestinationIn}
        RenderG.Sources.Add(renderG1) : RenderG.Sources.Add(MaskG)
        RenderB = New CompositeEffect With {.Mode = CanvasComposite.DestinationIn}
        RenderB.Sources.Add(renderB1) : RenderB.Sources.Add(MaskB)


        ' This is how drawing gets done. The actual transform matrix for the Transform2DEffect
        ' is calculated and supplied in the Draw method.
        Dim draw1 As New DpiCompensationEffect With {.Source = DrawBuffer, .SourceDpi = New Vector2(canvas1.Dpi)}
        Dim hsvR = New Single(255) {}
        Dim hsvG = New Single(255) {}
        Dim hsvB = New Single(255) {}
        For i = 0 To 255
            Dim f = i / 255
            Dim col = ColorFromHSV((0.5 + f) Mod 1, Math.Min(1, 1.5 - f), If(i = 0, 0, 1))
            hsvR(i) = CSng(col.R / 255)
            hsvG(i) = CSng(col.G / 255)
            hsvB(i) = CSng(col.B / 255)
        Next
        Dim draw2 As New DiscreteTransferEffect With {.Source = draw1, .RedTable = hsvR, .GreenTable = hsvG, .BlueTable = hsvB, .AlphaTable = {1}}
        DrawEffect = New Transform2DEffect With {.Source = draw2}
    End Sub

    Shared Function GetFloatBytes(bpp As Integer, value As Single) As Byte()
        If bpp <> 4 AndAlso bpp <> 2 Then Throw New ArgumentOutOfRangeException(NameOf(bpp))
        If bpp = 4 Then Return BitConverter.GetBytes(value)
        '
        If value < 0 Then Throw New ArgumentOutOfRangeException(NameOf(value), "negatives not implemented")
        Dim fbits = BitConverter.ToUInt32(BitConverter.GetBytes(CSng(value)), 0)
        Dim val = (fbits And &H7FFFFFFFUI) + &H1000
        If val >= &H47800000 Then Throw New ArgumentOutOfRangeException(NameOf(value), "NaN/Inf/overflow not implemented")
        If val >= &H38800000 Then Return BitConverter.GetBytes(CUShort((val - &H38000000) >> 13))
        If val < &H33000000 Then Return {0, 0}
        Throw New ArgumentOutOfRangeException(NameOf(value), "subnormals not implemented")
    End Function


    Sub Update()
        Dim sw = Stopwatch.StartNew

        ' Set up X and Y for the current zoom
        Using dsx = Me.X.CreateDrawingSession(), dsy = Me.Y.CreateDrawingSession()
            dsx.DrawImage(New LinearTransferEffect With {.Source = UnitX, .AlphaDisable = True, .RedOffset = MTopLeft.X, .RedSlope = MSize.X, .GreenOffset = MTopLeft.X, .GreenSlope = MSize.X, .BlueOffset = MTopLeft.X, .BlueSlope = MSize.X})
            dsy.DrawImage(New LinearTransferEffect With {.Source = UnitY, .AlphaDisable = True, .RedOffset = MTopLeft.Y, .RedSlope = MSize.Y, .GreenOffset = MTopLeft.Y, .GreenSlope = MSize.Y, .BlueOffset = MTopLeft.Y, .BlueSlope = MSize.Y})
        End Using

        ' Initialize the iteration and the accumulator
        Dim black As New ColorSourceEffect With {.Color = Colors.Black}
        Using dsa = A.CreateDrawingSession(), dsb = B.CreateDrawingSession(), dsacc = Accumulator.CreateDrawingSession()
            dsa.DrawImage(black)
            dsb.DrawImage(black)
            dsacc.DrawImage(black)
        End Using


        ' A' = A*A - B*B + X
        Dim A_squared As New ArithmeticCompositeEffect With {.Source1 = A, .Source2 = A}
        Dim minus_B_squared As New ArithmeticCompositeEffect With {.Source1 = B, .Source2 = B, .MultiplyAmount = -1}
        Dim A_prime As New CompositeEffect With {.Mode = CanvasComposite.Add}
        A_prime.Sources.Add(A_squared) : A_prime.Sources.Add(minus_B_squared) : A_prime.Sources.Add(X)

        ' B' = 2*A*B + Y
        Dim two_A_B As New ArithmeticCompositeEffect With {.Source1 = A, .Source2 = B, .MultiplyAmount = 2}
        Dim B_prime As New CompositeEffect With {.Mode = CanvasComposite.Add}
        B_prime.Sources.Add(two_A_B) : B_prime.Sources.Add(Y)

        ' D = A*A + B*B...
        '
        ' COMPLICATION: We want to clip "D" so that all values >= 4 count just as "diverged".
        ' But Win2d only offers clamping to the range 0..1, and it clamps NaN into "0" rather than "1".
        ' So instead we do "clamp(1 - 0.25*X*X - 0.25*Y*Y)", re-using the X*X and -Y*Y intermediates from earlier.
        ' This will give 0 for all diverged pixels, and 0..1 for all not-yet-diverged pixels.
        Dim one_minus_quarter_d As New ArithmeticCompositeEffect With {.Source1 = A_squared, .Source2 = minus_B_squared, .MultiplyAmount = 0, .Source1Amount = -0.25, .Source2Amount = 0.25, .Offset = 1, .ClampOutput = True}
        '
        ' COMPLICATION: The result of all those ArithmeticCompositeEffects has turned the alpha channel
        ' into something useless. This isn't a problem for the ArithmeticCompositeEffects used above since
        ' they treat it independently, but it is a problem for "D" the way we're going to use it.
        ' So we'll use a ColorMatrixEffect to reset the alpha channel to 1.0 everywhere:
        Dim one_minus_quarter_d_fixed_alpha As New ColorMatrixEffect With {.Source = one_minus_quarter_d, .AlphaMode = CanvasAlphaMode.Straight, .ColorMatrix = New Matrix5x4 With {.M11 = 1, .M22 = 1, .M33 = 1, .M44 = 0, .M54 = 1}}
        '
        ' COMPLICATION: for display purposes, we want to calculate the number of iterations it takes
        ' before "D" diverges. The way we'll do this is to steadily accumulate the following:
        ' if the above "clamp(1 - ...)" value is 0, i.e. if this pixel is either currently diverged
        ' or has diverged in the past, then add 1/50th greyscale to our accumulator. This way
        ' over 50 iterations we'll accumulate solid black for the things never diverge, and we'll color
        ' dark grey the things that eventually diverge after 30+ iterations, and we'll color light grey
        ' or white those things that diverge immediately.
        ' We can't quite do "if value is equal to 0", but we can use a DiscreteTransferFunction to do
        ' the close approximation "if value is <= 1/20"...
        Dim table As Single() = {CSng(1 / CITER), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
        Dim is_d_diverged As New DiscreteTransferEffect With {.Source = one_minus_quarter_d_fixed_alpha, .RedTable = table, .GreenTable = table, .BlueTable = table, .AlphaTable = {1}}


        For iter = 1 To CITER

            Using daprime = Me.A_prime.CreateDrawingSession(), dbprime = Me.B_prime.CreateDrawingSession(), dacc = Accumulator.CreateDrawingSession
                daprime.Blend = CanvasBlend.Copy : daprime.DrawImage(A_prime)
                dbprime.Blend = CanvasBlend.Copy : dbprime.DrawImage(B_prime)
                dacc.Blend = CanvasBlend.Add : dacc.DrawImage(is_d_diverged)
            End Using
            ' COMPLICATION: The CanvasBlend mode is "SourceOver", which interacts badly with the alpha
            ' values in A_prime and B_prime. So instead we use "Copy".


            ' Swap "a" and "a_prime" around, and likewise "b" and "b_prime"
            Dim ta = A : A = Me.A_prime : Me.A_prime = ta
            Dim tb = B : B = Me.B_prime : Me.B_prime = tb
            A_squared.Source1 = A
            A_squared.Source2 = A
            minus_B_squared.Source1 = B
            minus_B_squared.Source2 = B
            two_A_B.Source1 = A
            two_A_B.Source2 = B
        Next


        ' DrawBuffer is what the screen will use whenever it needs to repaint itself
        Using ds = DrawBuffer.CreateDrawingSession()
            ds.Blend = CanvasBlend.Copy
            ds.DrawImage(RenderR)
            ds.Blend = CanvasBlend.Add
            ds.DrawImage(RenderG)
            ds.DrawImage(RenderB)
        End Using

        Dim ms = sw.Elapsed.TotalMilliseconds
        Perf.AddLast(ms)
        If Perf.Count = 2 Then Perf.RemoveFirst() : Perf.AddLast(ms) ' to discount the first one
        If Perf.Count > 20 Then Perf.RemoveFirst()
        ms = Aggregate p In Perf Into Average
        label1.Text = $"{ms:0}ms, <{MTopLeft.X:0.000},{MTopLeft.Y:0.000}>+<{MSize.X:0.000},{MSize.Y:0.000}>"

    End Sub


    Sub Canvas_Draw(sender As CanvasControl, args As CanvasDrawEventArgs) Handles canvas1.Draw
        If Not IsUpToDate Then Update() : IsUpToDate = True

        Dim sourceSizeDips = New Vector2(canvas1.ConvertPixelsToDips(CSIZE))
        Dim canvasSizeDips = canvas1.Size.ToVector2
        DrawEffect.TransformMatrix = Matrix3x2.CreateScale(canvasSizeDips / sourceSizeDips)
        args.DrawingSession.DrawImage(DrawEffect)
    End Sub

    Sub Zoom(Zoom As Double, fCenter As Vector2)
        Dim MCenter = MTopLeft + MSize * fCenter
        MSize /= CSng(Zoom)
        MTopLeft = MCenter - fCenter * MSize
        IsUpToDate = False : canvas1.Invalidate()
    End Sub

    Sub Page_PointerPressed(sender As Object, e As TappedRoutedEventArgs) Handles Me.Tapped
        Zoom(2, e.GetPosition(canvas1).ToVector2 / canvas1.Size.ToVector2)
    End Sub

    Sub Page_RightTapped(sender As Object, e As RightTappedRoutedEventArgs) Handles Me.RightTapped
        Zoom(0.5, e.GetPosition(canvas1).ToVector2 / canvas1.Size.ToVector2)
    End Sub

    Sub Page_PointerWheelChanged(sender As Object, e As PointerRoutedEventArgs) Handles Me.PointerWheelChanged
        If Window.Current.CoreWindow.GetKeyState(VirtualKey.Control) = CoreVirtualKeyStates.None Then Return
        Dim pointer = e.GetCurrentPoint(canvas1)
        If pointer.Properties.IsHorizontalMouseWheel Then Return
        Dim f = pointer.Properties.MouseWheelDelta / 100 ' expressed as positive or negative
        If f < 0 Then f = -1 / f
        Zoom(f, pointer.Position.ToVector2 / canvas1.Size.ToVector2)
    End Sub

    Sub BackRequested(sender As Object, e As BackRequestedEventArgs) Handles NavigationManager.BackRequested
        ' If we're already zoomed in, then the back button just zooms us back out:
        If MSize.Length < 4 Then
            e.Handled = True
            Zoom(0.5, New Vector2(0.5))
            Return
        End If

        ' If we're in touch mode and already fully zoomed out, then back button exits the app by leaving the event unhandled:
        If UIViewSettings.GetForCurrentView.UserInteractionMode = UserInteractionMode.Touch Then
            e.Handled = False
            Return
        End If

        ' Otherwise, in non-touch mode, we'll reset the view to default:
        MTopLeft = New Vector2(-2, -2)
        MSize = New Vector2(4, 4)
        e.Handled = True
        IsUpToDate = False : canvas1.Invalidate()
    End Sub


    Sub Page_ManipulationStarted(sender As Object, e As ManipulationStartedRoutedEventArgs) Handles Me.ManipulationStarted
        ManipulationStart_MSize = MSize
        ManipulationStart_MTopLeft = MTopLeft
        ManipulationStart_MCenterOn = MTopLeft + MSize * TransformToVisual(canvas1).TransformPoint(e.Position).ToVector2() / canvas1.Size.ToVector2
    End Sub

    Sub Page_ManipulationDelta(sender As Object, e As ManipulationDeltaRoutedEventArgs) Handles Me.ManipulationDelta
        MSize /= e.Delta.Scale

        MTopLeft = ManipulationStart_MCenterOn +
            MSize / ManipulationStart_MSize * (ManipulationStart_MTopLeft - ManipulationStart_MCenterOn) -
            e.Cumulative.Translation.ToVector2() / canvas1.Size.ToVector2() * MSize

        IsUpToDate = False : canvas1.Invalidate()
    End Sub


    Function ColorFromHSV(h As Double, s As Double, v As Double) As Color
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

