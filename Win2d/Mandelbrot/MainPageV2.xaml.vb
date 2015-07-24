Imports System.Numerics
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Effects
Imports Microsoft.Graphics.Canvas.UI.Xaml
Imports Windows.System
Imports Windows.Storage
Imports Windows.Graphics.DirectX
Imports Windows.UI
Imports Windows.UI.Core
Imports Windows.UI.Text


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

    Dim CSIZE As Integer = 600  ' size in pixels of our mandlebrot calculation
    Dim CITER As Integer = 50   ' how many iterations to do.
    Dim MaxCanvasSize As Double ' for tracking perf


    Dim MTopLeft As New Vector2(-2, -2) ' top-left corner of Mandlebrot
    Dim MSize As New Vector2(4, 4)      ' size of mandlebrot
    ' and to track pinch-zoom and drag:
    Dim ManipulationStart_MSize, ManipulationStart_MTopLeft, ManipulationStart_MCenterOn As Vector2
    Dim InvalidationTime As New Stopwatch ' Started upon a move/zoom, and used to track performance

    ' These are the surfaces and effects, created in Canvas_CreateResources, used in Update() and Canvas_Draw()
    Dim UnitX, UnitY, X, Y, A, A_prime, B, B_prime, Accumulator, MaskR, MaskG, MaskB, DrawBuffer As CanvasRenderTarget
    Dim e_RangeX, e_RangeY As LinearTransferEffect
    Dim e_Black As ColorSourceEffect
    Dim e_A_prime, e_B_prime As CompositeEffect
    Dim e_A_squared, e_minus_B_squared, e_two_A_B As ArithmeticCompositeEffect
    Dim e_is_d_diverged As DiscreteTransferEffect
    Dim e_renderR, e_renderG, e_renderB As CompositeEffect
    Dim e_Draw As Transform2DEffect

    WithEvents App As App = CType(Application.Current, App)
    WithEvents NavigationManager As SystemNavigationManager = SystemNavigationManager.GetForCurrentView



    Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
        SystemNavigationManager.GetForCurrentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible        '
        LoadPerf()
    End Sub


    Sub Page_SizeChanged() Handles Me.SizeChanged
        Dim s = Math.Max(Me.ActualWidth, Me.ActualHeight)
        canvas1.Width = s
        canvas1.Height = s
        MaxCanvasSize = Math.Max(MaxCanvasSize, s)
    End Sub


    Sub Canvas_CreateResources() Handles canvas1.CreateResources
        Const defaultDpi = 96.0F
        InvalidationTime.Restart()

        DrawBuffer?.Dispose()
        MaskR?.Dispose() : MaskG?.Dispose() : MaskB?.Dispose()
        UnitX?.Dispose() : UnitY?.Dispose()
        X?.Dispose() : Y?.Dispose() : A?.Dispose() : B?.Dispose()
        A_prime?.Dispose() : B_prime?.Dispose() : Accumulator?.Dispose()

        If CSIZE Mod 3 <> 0 Then Throw New ArgumentException("CSIZE")
        Dim CSIZEX = CSIZE \ 3

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


        ' Effects to set up X and Y for the current zoom (these are populated in the Update method)
        e_RangeX = New LinearTransferEffect With {.Source = UnitX, .AlphaDisable = True}
        e_RangeY = New LinearTransferEffect With {.Source = UnitY, .AlphaDisable = True}

        ' Effects to initialize the iteration and the accumulator
        e_Black = New ColorSourceEffect With {.Color = Colors.Black}


        ' Effects to calculate A' = A*A - B*B + X
        e_A_squared = New ArithmeticCompositeEffect With {.Source1 = A, .Source2 = A}
        e_minus_B_squared = New ArithmeticCompositeEffect With {.Source1 = B, .Source2 = B, .MultiplyAmount = -1}
        e_A_prime = New CompositeEffect With {.Mode = CanvasComposite.Add}
        e_A_prime.Sources.Add(e_A_squared) : e_A_prime.Sources.Add(e_minus_B_squared) : e_A_prime.Sources.Add(X)


        ' Effects to calculate B' = 2*A*B + Y
        e_two_A_B = New ArithmeticCompositeEffect With {.Source1 = A, .Source2 = B, .MultiplyAmount = 2}
        e_B_prime = New CompositeEffect With {.Mode = CanvasComposite.Add}
        e_B_prime.Sources.Add(e_two_A_B) : e_B_prime.Sources.Add(Y)


        ' Effects to calculate D = A*A + B*B...
        '
        ' COMPLICATION: We want to clip "D" so that all values >= 4 count just as "diverged".
        ' But Win2d only offers clamping to the range 0..1, and it clamps NaN into "0" rather than "1".
        ' So instead we do "clamp(1 - 0.25*X*X - 0.25*Y*Y)", re-using the X*X and -Y*Y intermediates from earlier.
        ' This will give 0 for all diverged pixels, and 0..1 for all not-yet-diverged pixels.
        Dim e_one_minus_quarter_d As New ArithmeticCompositeEffect With {.Source1 = e_A_squared, .Source2 = e_minus_B_squared, .MultiplyAmount = 0, .Source1Amount = -0.25, .Source2Amount = 0.25, .Offset = 1, .ClampOutput = True}
        '
        ' COMPLICATION: The result of all those ArithmeticCompositeEffects has turned the alpha channel
        ' into something useless. This isn't a problem for the ArithmeticCompositeEffects used above since
        ' they treat it independently, but it is a problem for "D" the way we're going to use it.
        ' So we'll use a ColorMatrixEffect to reset the alpha channel to 1.0 everywhere:
        Dim e_one_minus_quarter_d_fixed_alpha As New ColorMatrixEffect With {.Source = e_one_minus_quarter_d, .AlphaMode = CanvasAlphaMode.Straight, .ColorMatrix = New Matrix5x4 With {.M11 = 1, .M22 = 1, .M33 = 1, .M44 = 0, .M54 = 1}}
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
        Dim e_graytable As Single() = {CSng(1 / CITER), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
        e_is_d_diverged = New DiscreteTransferEffect With {.Source = e_one_minus_quarter_d_fixed_alpha, .RedTable = e_graytable, .GreenTable = e_graytable, .BlueTable = e_graytable, .AlphaTable = {1}}




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
        e_renderR = New CompositeEffect With {.Mode = CanvasComposite.DestinationIn}
        e_renderR.Sources.Add(renderR1) : e_renderR.Sources.Add(MaskR)
        e_renderG = New CompositeEffect With {.Mode = CanvasComposite.DestinationIn}
        e_renderG.Sources.Add(renderG1) : e_renderG.Sources.Add(MaskG)
        e_renderB = New CompositeEffect With {.Mode = CanvasComposite.DestinationIn}
        e_renderB.Sources.Add(renderB1) : e_renderB.Sources.Add(MaskB)


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
        e_Draw = New Transform2DEffect With {.Source = draw2}
    End Sub



    Sub Update()
        ' Set up X and Y for the current zoom
        With e_RangeX
            .RedOffset = MTopLeft.X : .RedSlope = MSize.X
            .GreenOffset = MTopLeft.X : .GreenSlope = MSize.X
            .BlueOffset = MTopLeft.X : .BlueSlope = MSize.X
        End With
        With e_RangeY
            .RedOffset = MTopLeft.Y : .RedSlope = MSize.Y
            .GreenOffset = MTopLeft.Y : .GreenSlope = MSize.Y
            .BlueOffset = MTopLeft.Y : .BlueSlope = MSize.Y
        End With
        Using dsx = X.CreateDrawingSession(), dsy = Y.CreateDrawingSession()
            dsx.DrawImage(e_RangeX)
            dsy.DrawImage(e_RangeY)
        End Using

        ' Initialize the iteration and the accumulator
        Using dsa = A.CreateDrawingSession(), dsb = B.CreateDrawingSession(), dsacc = Accumulator.CreateDrawingSession()
            dsa.DrawImage(e_Black)
            dsb.DrawImage(e_Black)
            dsacc.DrawImage(e_Black)
        End Using


        ' Do the iteration
        For iter = 1 To CITER

            Using daprime = A_prime.CreateDrawingSession(), dbprime = B_prime.CreateDrawingSession(), dacc = Accumulator.CreateDrawingSession
                daprime.Blend = CanvasBlend.Copy : daprime.DrawImage(e_A_prime)
                dbprime.Blend = CanvasBlend.Copy : dbprime.DrawImage(e_B_prime)
                dacc.Blend = CanvasBlend.Add : dacc.DrawImage(e_is_d_diverged)
            End Using
            ' COMPLICATION: The CanvasBlend mode is "SourceOver", which interacts badly with the alpha
            ' values in A_prime and B_prime. So instead we use "Copy".


            ' Swap "a" and "a_prime" around, and likewise "b" and "b_prime"
            Swap(A, A_prime)
            Swap(B, B_prime)
            ' And rewire all the effects that depend on "a" and "b"
            e_A_squared.Source1 = A
            e_A_squared.Source2 = A
            e_minus_B_squared.Source1 = B
            e_minus_B_squared.Source2 = B
            e_two_A_B.Source1 = A
            e_two_A_B.Source2 = B
        Next


        ' DrawBuffer is what the screen will use whenever it needs to repaint itself
        Using ds = DrawBuffer.CreateDrawingSession()
            ds.Blend = CanvasBlend.Copy
            ds.DrawImage(e_renderR)
            ds.Blend = CanvasBlend.Add
            ds.DrawImage(e_renderG)
            ds.DrawImage(e_renderB)
        End Using

    End Sub


    Sub Canvas_Draw(sender As CanvasControl, args As CanvasDrawEventArgs) Handles canvas1.Draw
        Static Dim FrameCount As Integer = 0
        If InvalidationTime.IsRunning Then Update() : FrameCount += 1

        Dim sourceSizeDips = New Vector2(canvas1.ConvertPixelsToDips(CSIZE))
        Dim canvasSizeDips = canvas1.Size.ToVector2
        e_Draw.TransformMatrix = Matrix3x2.CreateScale(canvasSizeDips / sourceSizeDips)
        args.DrawingSession.DrawImage(e_Draw)

        ' Perf display and tuning...
        If Not InvalidationTime.IsRunning Then Return
        InvalidationTime.Stop()
        Dim elapsed = InvalidationTime.Elapsed.TotalMilliseconds
        Static Dim PerfCounts As New LinkedList(Of Double)
        PerfCounts.AddLast(elapsed)
        If PerfCounts.Count > 40 Then PerfCounts.RemoveFirst()
        Dim perf = If(PerfCounts.Count <= 7, elapsed, Aggregate p In PerfCounts.Skip(7) Into Average)
        label1.Text = $"<{MTopLeft.X:0.000},{MTopLeft.Y:0.000}>+<{MSize.X:0.000},{MSize.Y:0.000}>"
        label2.Text = $"{Me.CSIZE}x{Me.CSIZE} pixels, {Me.CITER} iterations, {perf:0}ms"
        If FrameCount Mod 10 = 0 AndAlso PerfCounts.Count >= 10 Then
            Dim perfChanged = RefinePerf(perf)
            If perfChanged Then PerfCounts.Clear()
        End If
    End Sub


    Sub Zoom(Zoom As Double, fCenter As Vector2)
        Dim MCenter = MTopLeft + MSize * fCenter
        MSize /= CSng(Zoom)
        MTopLeft = MCenter - fCenter * MSize
        InvalidationTime.Restart() : canvas1.Invalidate()
    End Sub


    Sub Page_LeftTapped(sender As Object, e As TappedRoutedEventArgs) Handles Me.Tapped
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
        InvalidationTime.Restart()
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

        InvalidationTime.Restart() : canvas1.Invalidate()
    End Sub


    Sub SavePerf() Handles App.Suspending
        ApplicationData.Current.LocalSettings.Values("MaxCanvasSize") = MaxCanvasSize
        ApplicationData.Current.LocalSettings.Values("CITER") = CITER
        ApplicationData.Current.LocalSettings.Values("CSIZE") = CSIZE
    End Sub

    Sub LoadPerf()
        Dim canvasSize0 = CType(ApplicationData.Current.LocalSettings.Values("MaxCanvasSize"), Double?)
        Dim citer0 = CType(ApplicationData.Current.LocalSettings.Values("CITER"), Integer?)
        Dim csize0 = CType(ApplicationData.Current.LocalSettings.Values("CSIZE"), Integer?)
        '
        If canvasSize0.HasValue Then MaxCanvasSize = canvasSize0.Value
        If citer0.HasValue Then CITER = citer0.Value
        If csize0.HasValue Then CSIZE = csize0.Value
    End Sub

    Function RefinePerf(perf As Double) As Boolean
        Static Dim isRefining As Boolean = False
        If isRefining Then Return False Else isRefining = True
        '
        Dim XGOOD As Integer = 15 ' in milliseconds
        Dim GOOD As Integer = 30
        Dim BAD As Integer = 60
        Dim XBAD As Integer = 100

        Dim newSize = CSIZE, newIter = CITER
        Select Case True
            Case perf < XGOOD AndAlso newSize < CInt(MaxCanvasSize / 4) : newSize = newSize * 3 \ 2
            Case perf < XGOOD : newIter = newIter * 3 \ 2
            Case perf < GOOD AndAlso newIter < 100 : newIter = newIter * 5 \ 4
            Case perf < GOOD AndAlso newSize < CInt(MaxCanvasSize / 4) : newSize = newSize * 5 \ 4
            Case perf < GOOD AndAlso newIter < 120 : newIter = newIter * 5 \ 4
            Case perf < GOOD AndAlso newSize < CInt(MaxCanvasSize) : newSize = CInt(MaxCanvasSize)
            Case perf > XBAD AndAlso newSize > 500 : newSize = newSize * 2 \ 3
            Case perf > XBAD AndAlso newIter > 150 : newIter = newIter * 2 \ 3
            Case perf > XBAD AndAlso newSize > 300 : newSize = newSize * 2 \ 3
            Case perf > BAD AndAlso newIter > 50 : newIter = newIter * 4 \ 5
            Case perf > BAD AndAlso newSize > 300 : newSize = newSize * 4 \ 5
            Case perf > BAD AndAlso newIter > 20 : newIter = newIter * 4 \ 5
            Case Else : Return False
        End Select

        If newSize >= MaxCanvasSize - 10 Then newSize = CInt(MaxCanvasSize) + 3
        newSize = 3 * (newSize \ 3) ' has to be a multiple of 3
        If newSize < 300 Then newSize = 300
        If newIter < 20 Then newIter = 20
        If newSize = CSIZE AndAlso newIter = CITER Then Return False

        ' RefinePerf is called from within Draw. We can't reset the drawing surfaces at that time.
        ' So we'll postpone it slightly, to a time when it's not inside a Draw/CreateResources callback.
        ' That's what the Async Sub coupled with Task.Delay achieves.
        Call Async Sub()
                 Await Task.Delay(10)
                 If newSize <> CSIZE Then CSIZE = newSize : Canvas_CreateResources()
                 If newIter <> CITER Then
                     CITER = newIter
                     Dim e_graytable As Single() = {CSng(1 / CITER), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
                     e_is_d_diverged.RedTable = e_graytable
                     e_is_d_diverged.GreenTable = e_graytable
                     e_is_d_diverged.BlueTable = e_graytable
                 End If
                 canvas1.Invalidate()
                 isRefining = False
             End Sub
        Return True
    End Function


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

    Shared Sub Swap(Of T)(ByRef x As T, ByRef y As T)
        Dim temp = x
        x = y
        y = temp
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

End Class

