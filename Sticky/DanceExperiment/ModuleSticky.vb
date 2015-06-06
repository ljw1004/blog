Option Strict On

Imports System.Threading.Tasks

Namespace Global

    Public Class Stick
        Public Name As String
        Public Version As Decimal
        Public Category As String
        Public Copyright As String
        Friend Shapes As New List(Of Shape)
        Friend Limbs As New List(Of Limb)

        Public Sub New()
        End Sub

        ' The following cached info is created upon load, as a synthesis of what's come before
        Friend ComputedPaintControllers As New List(Of Tuple(Of PaintController, CurrentPaintData))
        Friend ComputedFractionControllers As New List(Of Tuple(Of FractionController, CurrentFractionData))

        ' The following information is updated each frame
        Public CurrentAnchorX, CurrentAnchorY As Double

        Public Function LoadAsync(name As String, stream As IO.Stream, renderer As IStickRenderer) As Task
            Return Loader.LoadStickAsync(Me, name, stream, renderer)
        End Function

        ''' <summary>
        ''' Updates stick positions according to frequency. Period is used for cumulative limbs.
        ''' </summary>
        ''' <param name="freqs">freq[left=0|right=1|rightminusleft=2, hz0to320=0|hz320to480=1|hz480to960=2|hz960to1280=3|hz1280to1600=4|hz1600to1900=5]
        ''' or freq[karaoke=3, vocals=0|music=1]</param>
        ''' <param name="period"></param>
        ''' <remarks></remarks>
        Public Sub Update(freqs As Double(,), period As TimeSpan)
            Updater.Update(Me, freqs, period)
        End Sub

        Public Sub Draw(left As Integer, top As Integer, width As Integer, height As Integer, r As IStickRenderer)
            Drawer.Draw(Me, left, top, width, height, r)
        End Sub
    End Class

    Public Interface IStickSource
        ReadOnly Property Name As String
        ReadOnly Property SingleFile As IO.Stream
        ReadOnly Property Container As IDictionary(Of String, IO.Stream)
    End Interface

    Public Interface IStickRenderer
        Function LoadBitmapAsync(name As String, src As IO.Stream) As Task
        Function GetBitmapSize(name As String) As Tuple(Of Integer, Integer)
        Sub DrawBitmap(name As String, x As Integer, y As Integer, w As Integer, h As Integer, m11 As Double, m12 As Double, m21 As Double, m22 As Double, dx As Double, dy As Double, tag As Object)
        Sub Line(x0 As Integer, y0 As Integer, x1 As Integer, y1 As Integer, thickness As Double, linecolor As StickRGB, tag As Object)
        Sub Arc(x As Integer, y As Integer, w As Integer, h As Integer, startAngle As Double, sweepAngle As Double, thickness As Double, linecolor As StickRGB, tag As Object)
        Sub Rectangle(x As Integer, y As Integer, w As Integer, h As Integer, linethickness As Double, linecolor As StickRGB, fillcolor As StickRGB, tag As Object)
        Sub Ellipse(x As Integer, y As Integer, w As Integer, h As Integer, linethickness As Double, linecolor As StickRGB, fillcolor As StickRGB, tag As Object)
        Sub Polygon(pts As List(Of Tuple(Of Integer, Integer)), FillAlternate As Boolean, linethickness As Double, linecolor As StickRGB, fillcolor As StickRGB, tag As Object)
    End Interface

    Public Structure StickRGB
        Dim r, g, b As Integer
        '
        Shared Function FromArgb(r As Integer, g As Integer, b As Integer, Optional a As Integer = 255) As StickRGB
            If a <> 255 Then Throw New NotImplementedException("haven't implemented alpha yet")
            Return New StickRGB With {.r = r, .g = g, .b = b}
        End Function
        Shared ReadOnly Invisible As New StickRGB With {.r = -1, .g = -1, .b = -1}
        ReadOnly Property IsVisible As Boolean
            Get
                Return r <> -1 AndAlso g <> -1 AndAlso b <> -1
            End Get
        End Property
        Public Overrides Function ToString() As String
            Return If(IsVisible, String.Format("RGB({0},{1},{2})", r, g, b), "Invisible")
        End Function
    End Structure


    Friend Class Limb
        Public Tag As String
        Public Children As New List(Of Limb)
        Public Anchor As LimbAnchor
        Public Thickness As Double ' multiplier of the standard thickness. -1 means "1pixel"
        Public LineColorController As New PaintController(Paint.None, "fill???")
        Public PosController As New FractionController("limb?pos")
        '
        Public Kind As LimbKind
        Public MaxLength, AngleStart As Double
        Public IsAngleStartRelativeToParent As Boolean
        Public RotatingAngleMaxSweep As Double
        Public StretchingAngle As Double
        Public StretchingMinLength As Double
        '
        ' Drawing a limb:
        ' Line: StartPoint=parent.EndPoint, length=MaxLength, direction=AngleStart + f*RotatingAngleMaxSweep
        ' Arc: Origin=parent.EndPoint, radius=MaxLength, StartAngle=AngleStart, Sweep=f*RotatingAngleMaxSweep
        ' StretchLine: StartPoint=parent.EndPoint, length=f * MaxLength-to-MinLength, direction=RotatingAngleStart+StretchingAngle
        ' StretchCircle: Origin=parent.Endpoint, radius=f * Maxlength-to-MinLength
        '
        ' Calculating the EndPoint of a limb:
        ' For Line/Arc/StretchLine, it's clear.
        ' For StretchCircle, it goes out in direction RotatingAngleStart+StretchingAngle


        ' The following are recomputed each sample
        Public CurrentLinePaint As New CurrentPaintData ' from the fill controller
        Public CurrentPosFraction As New CurrentFractionData ' from the fraction-controller
        Public CurrentStartPoint, CurrentEndPoint As BodyPoint
        Public CurrentAngMin, CurrentAng, CurrentAngMax As Double
    End Class


    Friend Class Shape
        Public Tag As String
        Public Line As PaintController
        Public Fill As PaintController
        Public Thickness As Double
        Public IsFillAlternate As Boolean ' vs winding
        Public Points As New List(Of Joint)
        ' A shape's outline is generally made of straight-line segments between its specified joints.
        ' But if a segment goes from ArcLimb.Start->ArcLimb.End, then that segment becomes a curved segment rather than a straight-line.
        ' (similarly, in the reverse direction ArcLimb.End->ArcLimb.Start, and similarly with CircleLimbs)

        Public CurrentLinePaint As New CurrentPaintData
        Public CurrentFillPaint As New CurrentPaintData
    End Class

    Friend Class PaintController
        Public Segments As New List(Of Tuple(Of Double, Paint, Boolean))
        Public FractionController As FractionController

        Public Sub New(ConstantFill As Paint, tag As String)
            Segments.Add(Tuple.Create(1.0, ConstantFill, False))
            FractionController = New FractionController(0.0, tag)
        End Sub
        Public Sub New(copy As PaintController, tag As String)
            For Each s In copy.Segments : Segments.Add(s) : Next
            FractionController = New FractionController(copy.FractionController, tag)
        End Sub

    End Class

    Friend Class FractionController
        Public Tag As String
        Public Channel As Channel
        Public Frequency As Frequency
        Public IsNegative As Boolean ' For non-cumulatives, says whether to use 1-f or just the normal f
        Public IsCumulative As Boolean ' Means that each tick we merely increase f, rather than assigning it (doesn't apply to Channels.Fixed)
        Public CumulativeRate As Double ' For cumulative things, is a multiplier on calculated increase. Negative means absolute increase
        Public CumulativeIsReflecting As Boolean ' For cumulative things, says whether to bounce back or wrap around
        Public FixedValue As Double ' For Channel = Channels.Fixed, this is the value to use
        ' The fraction (0..1) is computed and stored in some associated CurrentFractionData.
        ' In the case of cumulatives, CurrentFractionData also contains the current direction we're moving in.

        Public Sub New(tag As String)
            Me.Tag = tag
        End Sub
        Public Sub New(f As Double, tag As String)
            Channel = Channel.Fixed
            Frequency = 0
            IsCumulative = False
            IsNegative = False
            FixedValue = f
            Me.Tag = tag
        End Sub
        Public Sub New(copy As FractionController, tag As String)
            Channel = copy.Channel : Frequency = copy.Frequency : IsNegative = copy.IsNegative : FixedValue = copy.FixedValue
            IsCumulative = copy.IsCumulative : CumulativeRate = copy.CumulativeRate
            Me.Tag = tag
        End Sub
    End Class

    Friend Class Paint
        Public Kind As FillKind
        Public color As StickRGB ' for Kind=RGB
        Public bmp As String ' for Kind=Bitmap
        '
        Public Shared Function RGBA(r As Integer, g As Integer, b As Integer) As Paint
            If r > 255 OrElse g > 255 OrElse b > 255 OrElse r < 0 OrElse g < 0 OrElse b < 0 Then Throw New ArgumentException("invalid rgb")
            Return New Paint With {.Kind = FillKind.RGB, .color = StickRGB.FromArgb(r, g, b)}
        End Function
        Public Shared Function Bitmap(b As String) As Paint
            Return New Paint With {.Kind = FillKind.Bitmap, .bmp = b}
        End Function
        Public Shared ReadOnly None As Paint = New Paint With {.Kind = FillKind.None}
        Public Overrides Function ToString() As String
            If Kind = FillKind.None Then Return "NoPaint"
            If Kind = FillKind.Bitmap Then Return "BMP(""" & bmp & """)"
            If Kind = FillKind.RGB Then Return color.ToString()
            Debug.Assert(False, "invalid fill kind")
            Return "Fill???"
        End Function
    End Class


    Friend Structure BodyPoint
        ReadOnly x, y As Double
        '
        Sub New(x As Double, y As Double)
            Me.x = x : Me.y = y
        End Sub
        Function Offset(dx As Double, dy As Double) As BodyPoint
            Return New BodyPoint(Me.x + dx, Me.y + dy)
        End Function
        Public Shared Operator +(a As BodyPoint, b As BodyPoint) As BodyPoint
            Return New BodyPoint(a.x + b.x, a.y + b.y)
        End Operator
        Public Shared Operator -(a As BodyPoint, b As BodyPoint) As BodyPoint
            Return New BodyPoint(a.x - b.x, a.y - b.y)
        End Operator
        Public ReadOnly Property Length As Double
            Get
                Return Math.Sqrt(x * x + y * y)
            End Get
        End Property
        Public Overrides Function ToString() As String
            Return String.Format("({0:f2},{1:f2})", x, y)
        End Function
    End Structure

    Friend Structure Joint
        Public Limb As Limb ' Normally the joint just refers to the endpoint of this limb
        Public UseStartOfArc As Boolean ' but for arcs, this indicates that we point to the start of the arc curve (rather than its endpoint)
    End Structure

    Friend Enum LimbKind
        RotatingLine = 0
        RotatingArc = 1
        StretchingLine = 2
        StretchingCircle = 3
    End Enum

    Friend Enum LimbAnchor
        None = 0
        Top = 1
        Right = 2
        Bottom = 3
        Left = 4
    End Enum

    Friend Class CurrentFractionData
        Public f As Double
        Public dir As Integer = 1 ' +1 or -1, used for cumulatives
    End Class

    Friend Class CurrentPaintData
        Public Paint As Paint
        Public Frac As New CurrentFractionData
    End Class

    Friend Enum FillKind
        None
        RGB
        Bitmap
    End Enum

    Public Enum Channel
        Left = 0
        Right = 1
        RightMinusLeft = 2
        Karaoke = 3 ' only uses two frequency-bands, 0 and 1
        Fixed = 4 ' only uses one frequency-band, 0
    End Enum

    Public Enum Frequency
        HzA_0to320 = 0
        HzB_320to480 = 1
        HzC_480to960 = 2
        HzD_960to1280 = 3
        HzE_1280to1600 = 4
        HzF_1600to1960 = 5
        Karaoke_Vocals = 0 ' sum of the two waveforms is the vocal track (since the vocals have a single source and are in phase, but the music isn't so much)
        Karaoke_Music = 1 ' difference of the two waveforms is the music track (since this will likely remove the vocals)
    End Enum


    Friend Class Updater
        Public Shared Sub Update(stick As Stick, freqs As Double(,), period As TimeSpan)
            UpdateFractions(stick, freqs, period)
            UpdatePaints(stick)
            UpdateLimbs(stick)
            UpdateAnchor(stick)
        End Sub

        Private Shared Sub UpdateFractions(stick As Stick, freqs As Double(,), period As TimeSpan)
            Dim cmul = CDbl(period.TotalMilliseconds)
            For Each t In stick.ComputedFractionControllers
                Dim fc = t.Item1, dat = t.Item2
                '
                Dim f As Double
                If fc.Channel = Channel.Left OrElse fc.Channel = Channel.Right Then : f = freqs(fc.Channel, fc.Frequency)
                ElseIf fc.Channel = Channel.RightMinusLeft Then : f = freqs(fc.Channel, fc.Frequency) : If fc.IsCumulative Then f -= 0.5
                ElseIf fc.Channel = Channel.Karaoke Then : f = freqs(fc.Channel, fc.Frequency)
                ElseIf fc.Channel = Channel.Fixed Then : f = fc.FixedValue
                Else : f = 0 : End If
                '
                If fc.Channel <> Channel.Fixed Then
                    If fc.IsCumulative Then
                        Dim dir = If(fc.IsNegative, -dat.dir, dat.dir)
                        Dim df = If(fc.CumulativeRate >= 0, fc.CumulativeRate * cmul * f, -fc.CumulativeRate * cmul)
                        f = dat.f + dir * 0.01 * df
                        If fc.CumulativeIsReflecting Then
                            Do
                                If f > 1 Then : f = 2 - f : dat.dir *= -1
                                ElseIf f < 0 Then : f = -f : dat.dir *= -1
                                Else : Exit Do : End If
                            Loop
                        Else
                            While f < 0 : f += 1 : End While
                            While f > 1 : f -= 1 : End While
                        End If
                    Else
                        f = If(fc.IsNegative, 1.0 - f, f)
                    End If
                End If
                dat.f = f
            Next
        End Sub

        Private Shared Sub UpdatePaints(stick As Stick)
            For Each t In stick.ComputedPaintControllers
                Dim fc = t.Item1, dat = t.Item2
                Dim f = dat.Frac.f

                If fc.Segments.Count = 1 Then dat.Paint = fc.Segments(0).Item2 : Continue For

                Dim iSegment = 0, fSegment = 1.0
                Dim tot = 0.0 : For iSegment = 0 To fc.Segments.Count - 1
                    Dim segfrac = fc.Segments(iSegment).Item1
                    If f >= tot + segfrac Then tot += segfrac : Continue For
                    Dim segmin = tot, segmax = If(iSegment = fc.Segments.Count - 1, 1.0, tot + segfrac)
                    fSegment = (f - segmin) / (segmax - segmin)
                    Exit For
                Next
                If iSegment = fc.Segments.Count Then iSegment = fc.Segments.Count - 1 : fSegment = 1.0

                Dim IsInterpolated = fc.Segments(iSegment).Item3
                Dim CurrentFill = fc.Segments(iSegment).Item2
                dat.Paint = CurrentFill
                Dim NextFill = fc.Segments((iSegment + 1) Mod fc.Segments.Count).Item2
                If IsInterpolated AndAlso CurrentFill.Kind = FillKind.RGB AndAlso NextFill.Kind = FillKind.RGB Then
                    Dim r1 = CDbl(CurrentFill.color.r), g1 = CDbl(CurrentFill.color.g), b1 = CDbl(CurrentFill.color.b)
                    Dim r2 = CDbl(NextFill.color.r), g2 = CDbl(NextFill.color.g), b2 = CDbl(NextFill.color.b)
                    Dim r = r1 * (1 - fSegment) + r2 * fSegment, g = g1 * (1 - fSegment) + g2 * fSegment, b = b1 * (1 - fSegment) + b2 * fSegment
                    dat.Paint = Paint.RGBA(CInt(r), CInt(g), CInt(b))
                End If
            Next
        End Sub

        Private Shared Sub UpdateLimbs(stick As Stick)
            UpdateCurrentLimbRec(stick.Limbs(0), Nothing)
        End Sub

        Private Shared Sub UpdateCurrentLimbRec(limb As Limb, parent As Limb)
            Dim f = limb.CurrentPosFraction.f
            Dim len As Double
            limb.CurrentAngMin = limb.AngleStart + If(limb.IsAngleStartRelativeToParent, parent.CurrentAng, 0)
            limb.CurrentAngMax = limb.CurrentAngMin + limb.RotatingAngleMaxSweep

            If limb.Kind = LimbKind.StretchingLine OrElse limb.Kind = LimbKind.StretchingCircle Then
                limb.CurrentAng = limb.CurrentAngMin + limb.StretchingAngle
                len = limb.StretchingMinLength * f + limb.MaxLength * (1 - f)
            ElseIf limb.Kind = LimbKind.RotatingLine OrElse limb.Kind = LimbKind.RotatingArc Then
                limb.CurrentAng = limb.CurrentAngMin + limb.RotatingAngleMaxSweep * f
                len = limb.MaxLength
            End If

            limb.CurrentStartPoint = If(parent Is Nothing, New BodyPoint(0, 0), parent.CurrentEndPoint)
            limb.CurrentEndPoint = limb.CurrentStartPoint.Offset(len * Math.Cos(limb.CurrentAng), len * Math.Sin(limb.CurrentAng))

            For Each child In limb.Children
                UpdateCurrentLimbRec(child, limb)
            Next
        End Sub

        Shared Sub UpdateAnchor(stick As Stick)
            Dim anch = {5.0, -5.0, -5.0, 5.0}, ab = {False, False, False, False}
            ' anch(1..4) says where north, east, south, west may be anchored, i.e. if anch[east] is set,
            ' then the stick must be offset such that anch[east] gets plotted at the right hand side.
            For Each limb In stick.Limbs
                Dim ilimb = stick.Limbs.IndexOf(limb)
                If limb.Anchor = LimbAnchor.Top Then : ab(0) = True : anch(0) = Math.Min(anch(0), limb.CurrentEndPoint.y)
                ElseIf limb.Anchor = LimbAnchor.Right Then : ab(1) = True : anch(1) = Math.Max(anch(1), limb.CurrentEndPoint.x)
                ElseIf limb.Anchor = LimbAnchor.Bottom Then : ab(2) = True : anch(2) = Math.Max(anch(2), limb.CurrentEndPoint.y)
                ElseIf limb.Anchor = LimbAnchor.Left Then : ab(3) = True : anch(3) = Math.Min(anch(3), limb.CurrentEndPoint.x) : End If
            Next
            stick.CurrentAnchorX = 0 : stick.CurrentAnchorY = 0
            If ab(0) Then stick.CurrentAnchorY = -anch(0) - 5
            If ab(1) Then stick.CurrentAnchorX = -anch(1) + 5
            If ab(2) Then stick.CurrentAnchorY = -anch(2) + 5
            If ab(3) Then stick.CurrentAnchorX = -anch(3) - 5
        End Sub
    End Class


    Friend Class Loader
        Private stick As Stick
        Private d As Dictionary(Of String, String)
        Private fillcontrollers As New Dictionary(Of String, PaintController)

        Public Shared Async Function LoadStickAsync(stick As Stick, name As String, stream As IO.Stream, renderer As IStickRenderer) As Task
            Dim loader As New Loader With {.stick = stick}
            stick.Name = name
            Dim zip As IO.Compression.ZipArchive = Nothing
            Try
                zip = New System.IO.Compression.ZipArchive(stream, IO.Compression.ZipArchiveMode.Read, True)
                Using zstream = zip.GetEntry("mainstick.txt").Open()
                    loader.LoadMainStick(stick, zstream)
                End Using
                For Each image In (From entry In zip.Entries Where {".bmp", ".jpg", ".jpeg", ".png", ".gif"}.Contains(IO.Path.GetExtension(entry.Name).ToLower()))
                    Dim iname = IO.Path.GetFileNameWithoutExtension(image.Name)
                    Using zstream = image.Open()
                        Await renderer.LoadBitmapAsync(iname, zstream)
                    End Using
                Next
            Catch ex As IO.InvalidDataException
                loader.LoadMainStick(stick, stream)
            Finally
                If Not zip Is Nothing Then zip.Dispose()
            End Try
        End Function


        Private Sub LoadMainStick(stick As Stick, stream As IO.Stream)
            d = MakeDictionary(stream)
            '
            stick.Version = If(d.ContainsKey("version"), CDec(d("version")), 3.4D)
            stick.Category = If(d.ContainsKey("category"), d("category"), Nothing)
            stick.Copyright = If(d.ContainsKey("copyright"), d("copyright"), Nothing)
            '
            LoadFillControllers()
            LoadLimbs()
            LoadShapes()
            ComputeControllers()
        End Sub


        Private Sub LoadFillControllers()
            Dim neffects = If(d.ContainsKey("neffects"), CInt(d("neffects")), 0)
            For ifc = 0 To neffects - 1
                Dim sfc = d("effect(" & ifc & ")")
                Dim eqpos = sfc.IndexOf("="c) : If eqpos = -1 Then Throw New FormatException("Bad effect syntax " & sfc)
                Dim name = sfc.Substring(0, eqpos).Trim()
                sfc = sfc.Substring(eqpos + 1).Trim()
                Dim fc As New PaintController(Paint.None, "effect" & ifc.ToString()) : fc.Segments.Clear()
                fillcontrollers.Add(name, fc)
                Dim dfc = MakeDictionary(sfc, True)
                fc.FractionController.Channel = CType(CInt(dfc("0:freq")(0)), Channel)
                fc.FractionController.Frequency = CType(CInt(dfc("0:freq")(1)), Frequency)
                fc.FractionController.IsNegative = 0 <> CInt(dfc("0:freq")(2))
                fc.FractionController.IsCumulative = 0 <> CInt(dfc("0:freq")(3))
                fc.FractionController.CumulativeRate = CDbl(dfc("0:freq")(4))
                fc.FractionController.CumulativeIsReflecting = 0 <> CInt(dfc("0:freq")(5))
                '
                Dim count = 0
                Do
                    count += 1
                    If dfc.ContainsKey(count & ":rgb") Then
                        Dim val = dfc(count & ":rgb")
                        Dim frac = CDbl(val(0))
                        Dim interpolated = 0 <> CInt(val(1))
                        ' COMPAT
                        interpolated = True
                        Dim col = Paint.RGBA(CInt(val(2)), CInt(val(3)), CInt(val(4)))
                        fc.Segments.Add(Tuple.Create(frac, col, interpolated))
                    ElseIf dfc.ContainsKey(count & ":bmp") Then
                        Dim val = dfc(count & ":bmp")
                        Dim frac = CDbl(val(0))
                        Dim col = Paint.Bitmap(ungarble(val(1)))
                        fc.Segments.Add(Tuple.Create(frac, col, False))
                    ElseIf dfc.ContainsKey(count & ":none") Then
                        Dim val = dfc(count & ":none")
                        Dim frac = CDbl(val(0))
                        Dim col = Paint.None
                        fc.Segments.Add(Tuple.Create(frac, col, False))
                    Else
                        Exit Do
                    End If
                Loop
            Next
        End Sub


        Private Sub LoadLimbs()
            Dim nlimbs = CInt(d("nlimbs"))
            For ilimb = 0 To nlimbs - 1 : stick.Limbs.Add(New Limb) : Next
            ' COMPAT: some files omit most details about limb(0), and assume a default
            stick.Limbs(0).AngleStart = 0 : stick.Limbs(0).RotatingAngleMaxSweep = 2 * Math.PI
            stick.Limbs(0).Kind = LimbKind.RotatingArc : stick.Limbs(0).LineColorController = New PaintController(Paint.RGBA(0, 0, 0), "limb(0)line")
            stick.Limbs(0).IsAngleStartRelativeToParent = False : stick.Limbs(0).CurrentPosFraction.f = 0 : stick.Limbs(0).PosController = New FractionController(0, "limb(0)pos")
            '
            For ilimb = 0 To nlimbs - 1
                Dim limb = stick.Limbs(ilimb)
                limb.Tag = "limb(" & ilimb.ToString() & ")"
                limb.PosController.Tag = limb.Tag & "pos"
                Dim tagLine = limb.Tag & "line"
                Dim slimb = d(limb.Tag)
                Dim dlimb = MakeDictionary(slimb)
                '
                Dim root = CInt(dlimb("root")(0))
                '
                If dlimb.ContainsKey("line") Then : limb.Kind = LimbKind.RotatingLine
                ElseIf dlimb.ContainsKey("arc") Then : limb.Kind = LimbKind.RotatingArc
                ElseIf dlimb.ContainsKey("spring") Then : limb.Kind = LimbKind.StretchingLine
                ElseIf dlimb.ContainsKey("circle") Then : limb.Kind = LimbKind.StretchingCircle
                Else : Throw New FormatException("unknown limb kind " & slimb) : End If
                If dlimb.ContainsKey("negative") Then limb.PosController.IsNegative = True
                Dim angkeyword = ""
                If dlimb.ContainsKey("angabs") Then : angkeyword = "angabs" : limb.IsAngleStartRelativeToParent = False
                ElseIf dlimb.ContainsKey("ang") Then : angkeyword = "ang" : limb.IsAngleStartRelativeToParent = True
                Else : Throw New FormatException("unknown angle " & slimb) : End If
                limb.AngleStart = CDbl(dlimb(angkeyword)(0))
                limb.RotatingAngleMaxSweep = CDbl(dlimb(angkeyword)(1))
                limb.StretchingAngle = If(dlimb(angkeyword).Length < 3, 0.0, CDbl(dlimb(angkeyword)(2)))
                limb.MaxLength = CDbl(dlimb("length")(0))
                If dlimb.ContainsKey("lmin") Then limb.StretchingMinLength = CDbl(dlimb("lmin")(0)) Else limb.StretchingMinLength = limb.MaxLength * 0.9
                limb.PosController.Channel = CType(CInt(dlimb("freq")(0)), Channel)
                limb.PosController.Frequency = CType(CInt(dlimb("freq")(1)), Frequency)
                ' COMPAT:
                If limb.PosController.Frequency = -1 Then limb.PosController.Frequency = 0
                ' COMPAT bug:
                If stick.Version < 3.7 AndAlso limb.PosController.Channel = Channel.Karaoke Then limb.PosController.Channel = Channel.Fixed : limb.PosController.Frequency = 0
                ' COMPAT:
                If root = 0 AndAlso
                    ((limb.Kind = LimbKind.StretchingCircle OrElse limb.Kind = LimbKind.StretchingLine) OrElse
                     (Not (limb.PosController.Channel = Channel.Fixed AndAlso limb.PosController.Frequency = 0) AndAlso limb.Kind <> LimbKind.RotatingLine AndAlso limb.Kind <> LimbKind.RotatingLine)) Then
                    limb.IsAngleStartRelativeToParent = False
                End If
                '
                If dlimb.ContainsKey("cum") Then limb.PosController.IsCumulative = (CInt(dlimb("cum")(0)) <> 0) Else limb.PosController.IsCumulative = False
                If dlimb.ContainsKey("cum") Then limb.PosController.CumulativeRate = CDbl(dlimb("cum")(1))
                If dlimb.ContainsKey("cum") AndAlso dlimb("cum").Length >= 3 Then
                    Dim crate = CInt(dlimb("cum")(2))
                    limb.PosController.CumulativeIsReflecting = (crate <> 0)
                    limb.CurrentPosFraction.dir = If(crate = -1, -1, 1)
                Else : limb.PosController.CumulativeIsReflecting = False : End If
                ' COMPAT: background color wasn't stored in the file back then
                If ilimb = 0 AndAlso stick.Version < 3.44 Then : limb.LineColorController = New PaintController(Paint.RGBA(0, 0, 0), "limb0linecol")
                    ' COMPAT: ilimb=0 means we ignore invisible flag
                ElseIf dlimb.ContainsKey("invisible") AndAlso ilimb <> 0 Then : limb.LineColorController = New PaintController(Paint.None, tagLine)
                ElseIf dlimb.ContainsKey("col") Then : limb.LineColorController = New PaintController(LoadRGB(dlimb("col")(0), dlimb("col")(1), dlimb("col")(2)), tagLine)
                Else : limb.LineColorController = New PaintController(Paint.RGBA(255, 255, 255), tagLine) : End If
                If dlimb.ContainsKey("thickness") Then limb.Thickness = CDbl(dlimb("thickness")(0)) Else limb.Thickness = 1.0
                If dlimb.ContainsKey("anchor") Then limb.Anchor = CType(CInt(dlimb("anchor")(0)), LimbAnchor) Else limb.Anchor = LimbAnchor.None
                Dim frac = If(dlimb.ContainsKey("frac"), CDbl(dlimb("frac")(0)), 0.5)
                If limb.PosController.Channel = Channel.Fixed Then limb.PosController.FixedValue = frac Else limb.CurrentPosFraction.f = frac
                ' COMPAT: fixed channels were weird
                If limb.PosController.Channel = Channel.Fixed AndAlso limb.PosController.Frequency = 0 Then
                    If limb.Kind = LimbKind.RotatingLine OrElse limb.Kind = LimbKind.RotatingArc Then
                        If stick.Version < 3.46 Then limb.PosController.FixedValue -= limb.AngleStart
                        '
                        If limb.RotatingAngleMaxSweep < 0.01 Then : limb.RotatingAngleMaxSweep = limb.PosController.FixedValue : limb.PosController.FixedValue = 1
                        Else : limb.PosController.FixedValue /= limb.RotatingAngleMaxSweep : End If
                    ElseIf limb.Kind = LimbKind.StretchingLine OrElse limb.Kind = LimbKind.StretchingCircle Then
                        limb.PosController.FixedValue = 0
                    End If
                Else
                    limb.PosController.Frequency = 0
                End If
                '
                ' STRICT:
                If limb.CurrentPosFraction.f < -100 OrElse limb.CurrentPosFraction.f > 100 Then limb.CurrentPosFraction.f = 0.2
                If (limb.Kind = LimbKind.StretchingLine OrElse limb.Kind = LimbKind.StretchingCircle) AndAlso (limb.StretchingAngle < -100 OrElse limb.StretchingAngle > 100) Then limb.StretchingAngle = 0.3
                '            
                If dlimb.ContainsKey("unicol") Then
                    Dim unival = String.Join(",", dlimb("unicol"))
                    ' COMPAT
                    If ilimb = 0 Then unival = unival.Replace("type(ctNone", "type(ctRGB")
                    '
                    limb.LineColorController = LoadFillController(unival, limb.CurrentLinePaint.Frac.f, fillcontrollers, tagLine)
                End If
                '
                If ilimb <> root Then stick.Limbs(root).Children.Add(limb)
            Next
            ' COMPAT:
            stick.Limbs(0).Thickness = 0
            stick.Limbs(0).Anchor = LimbAnchor.None
            If stick.Limbs(0).MaxLength = 0 Then
                If stick.Limbs(0).PosController.Channel = Channel.Fixed Then stick.Limbs(0).PosController.FixedValue = 0 Else stick.Limbs(0).CurrentPosFraction.f = 0
            End If
            '
            Dim roots = (From r In d("root").Split(","c) Select CInt(r.Trim())).ToArray()
            If roots.Length <> 1 OrElse roots.First <> 0 Then Throw New FormatException("need sole root to be #0")
        End Sub

        Private Sub LoadShapes()
            Dim nshapes = If(d.ContainsKey("nshapes"), CInt(d("nshapes")), 0)
            ' COMPAT:
            If nshapes = 0 Then stick.Shapes.Add(Nothing) : Return
            '
            For ishape = 0 To nshapes - 1
                Dim shape As New Shape
                shape.Tag = "shape(" & ishape.ToString() & ")"
                Dim sshape = d(shape.Tag)
                Dim dshape = MakeDictionary(sshape)
                If dshape.ContainsKey("limbs") Then stick.Shapes.Add(Nothing) : Continue For
                stick.Shapes.Add(shape)
                Dim tagFill = shape.Tag & "fill"
                Dim tagLine = shape.Tag & "line"
                '
                If dshape.ContainsKey("bitmap") AndAlso dshape.ContainsKey("brush") Then : shape.Fill = New PaintController(Paint.Bitmap(ungarble(dshape("bitmap")(0))), tagFill)
                ElseIf dshape.ContainsKey("brush") Then : shape.Fill = New PaintController(LoadRGB(dshape("brush")(0), dshape("brush")(1), dshape("brush")(2)), tagFill)
                Else : shape.Fill = New PaintController(Paint.None, tagFill) : End If
                shape.IsFillAlternate = dshape.ContainsKey("alternate")
                ' COMPAT:
                If stick.Version < 3.48 Then shape.IsFillAlternate = True
                ' COMPAT: now the "brush" directive controls visibility of bitmaps; previously all bitmaps were visible.
                If stick.Version < 3.625 AndAlso dshape.ContainsKey("bitmap") Then shape.Fill = New PaintController(Paint.Bitmap(ungarble(dshape("bitmap")(0))), tagFill)
                '
                Dim brushval = If(dshape.ContainsKey("unibrush"), dshape("unibrush"), If(dshape.ContainsKey("unibrushc"), dshape("unibrushc"), Nothing))
                If Not brushval Is Nothing Then shape.Fill = LoadFillController(String.Join(",", brushval), shape.CurrentFillPaint.Frac.f, fillcontrollers, tagFill)
                '
                If dshape.ContainsKey("line") Then : shape.Thickness = CDbl(dshape("line")(0)) : shape.Line = New PaintController(LoadRGB(dshape("line")(1), dshape("line")(2), dshape("line")(3)), tagLine)
                Else : shape.Line = New PaintController(Paint.None, tagLine) : End If
                Dim lineval = If(dshape.ContainsKey("uniline"), dshape("uniline"), If(dshape.ContainsKey("unilinec"), dshape("unilinec"), Nothing))
                If Not lineval Is Nothing Then shape.Line = LoadFillController(String.Join(",", lineval), shape.CurrentLinePaint.Frac.f, fillcontrollers, tagLine)
                '
                Dim arcroots = If(dshape.ContainsKey("arcroots"), (From i In dshape("arcroots") Select CInt(i)).ToArray(), {})
                For Each p In dshape("points")
                    shape.Points.Add(New Joint With {.UseStartOfArc = arcroots.Contains(shape.Points.Count), .Limb = stick.Limbs(CInt(p))})
                Next
                ' COMPAT:
                If shape.Points.Count = 1 Then shape.Points.Add(shape.Points(0))
            Next
        End Sub

        Private Function LoadRGB(r As String, g As String, b As String) As Paint
            Dim ri = CInt(r), gi = CInt(g), bi = CInt(b)
            If ri = -1 AndAlso gi = -1 AndAlso bi = -1 Then ri = 255 : gi = 255 : bi = 255
            ri = ri Mod 256 : gi = gi Mod 256 : bi = bi Mod 256
            Return Paint.RGBA(ri, gi, bi)
        End Function

        Private Function LoadFillController(s As String, ByRef f As Double, fillcontrollers As Dictionary(Of String, PaintController), tag As String) As PaintController
            Dim dfill = MakeDictionary(s)
            If Not dfill.ContainsKey("type") Then Throw New FormatException("missing fill-controller type " & s)
            Dim t = dfill("type")(0)
            If t = "ctNone" Then Return New PaintController(Paint.None, tag)
            If t = "ctRGB" Then Return New PaintController(Paint.RGBA(CInt(dfill("rgb")(0)), CInt(dfill("rgb")(1)), CInt(dfill("rgb")(2))), tag)
            If t = "ctBitmap" Then Return New PaintController(Paint.Bitmap(ungarble(dfill("cbmp")(0))), tag)
            If t = "ctEffect" Then f = CDbl(dfill("effect")(0)) : Return New PaintController(fillcontrollers(ungarble(dfill("effect")(1))), tag)
            Throw New FormatException("Unrecognized type " & t)
        End Function


        Private Sub ComputeControllers()
            For Each limb In stick.Limbs
                stick.ComputedPaintControllers.Add(Tuple.Create(limb.LineColorController, limb.CurrentLinePaint))
            Next
            For Each shape In stick.Shapes.Where(Function(s) Not s Is Nothing)
                stick.ComputedPaintControllers.Add(Tuple.Create(shape.Fill, shape.CurrentFillPaint))
                stick.ComputedPaintControllers.Add(Tuple.Create(shape.Line, shape.CurrentLinePaint))
            Next

            For Each limb In stick.Limbs
                stick.ComputedFractionControllers.Add(Tuple.Create(limb.PosController, limb.CurrentPosFraction))
            Next
            For Each controller In stick.ComputedPaintControllers
                stick.ComputedFractionControllers.Add(Tuple.Create(controller.Item1.FractionController, controller.Item2.Frac))
            Next
        End Sub

        Private Shared Function MakeDictionary(slimb As String, Optional NumberItems As Boolean = False) As Dictionary(Of String, String())
            slimb = slimb.Replace(")(", ",") & " "
            Dim dlimb As New Dictionary(Of String, String())
            Dim count = 0
            Do
                Dim prefix = If(NumberItems, count.ToString() & ":", "") : count += 1
                Dim spacei = slimb.IndexOf(" "c), lpareni = slimb.IndexOf("("c)
                If lpareni = -1 OrElse lpareni > spacei Then
                    Dim key = slimb.Substring(0, spacei)
                    slimb = slimb.Substring(spacei + 1).TrimStart()
                    dlimb.Add(prefix & key.Trim(), Nothing)
                Else
                    Dim key = slimb.Substring(0, lpareni)
                    Dim rpareni = -1 : Dim ParenCount = 0 : For i = lpareni To slimb.Length - 1
                        If slimb(i) = "("c Then ParenCount += 1
                        If slimb(i) = ")"c Then ParenCount -= 1 : If ParenCount = 0 Then rpareni = i : Exit For
                    Next
                    If rpareni = -1 Then Throw New FormatException("Missing rparen " & slimb)
                    Dim values = (From v In slimb.Substring(lpareni + 1, rpareni - lpareni - 1).Split(","c) Select v.Trim()).ToArray()
                    slimb = slimb.Substring(rpareni + 1).TrimStart()
                    dlimb.Add(prefix & key.Trim(), values)
                End If
            Loop Until String.IsNullOrWhiteSpace(slimb)
            Return dlimb
        End Function


        Private Shared Function MakeDictionary(this As IO.Stream) As Dictionary(Of String, String)
            Dim d As New Dictionary(Of String, String)
            Using reader As New IO.StreamReader(this)
                Do
                    Dim line = reader.ReadLine() : If line Is Nothing Then Return d
                    Dim eq = line.IndexOf("="c) : If eq = -1 Then Continue Do
                    d.Add(line.Substring(0, eq).Trim(), line.Substring(eq + 1).Trim())
                Loop
            End Using
        End Function


        Private Shared Function ungarble(s As String) As String
            Dim sb As New Text.StringBuilder
            For i = 0 To s.Length - 1 Step 2
                sb.Append(s(i))
            Next
            Return sb.ToString()
        End Function

    End Class


    Friend Class Drawer
        Dim stick As Stick
        Dim r As IStickRenderer
        Dim scale As Double, soffx As Integer, soffy As Integer

        Private Structure ScreenPoint
            Dim x, y As Integer
        End Structure

        Public Shared Sub Draw(stick As Stick, left As Integer, top As Integer, width As Integer, height As Integer, r As IStickRenderer)
            Dim d As New Drawer
            d.r = r
            d.stick = stick
            ' The body has dimensions -5 to +5, and we add anchx/anchy to the body's coordinates.
            ' This 10x10 square must be mapped onto the largest square inside rc.
            ' We do this as screenx = (bodyx+anchx+5)*scale+soffset
            If width > height Then : d.soffx = left + (width - height) \ 2 : d.soffy = top : d.scale = height / 10.0
            Else : d.soffx = left : d.soffy = top + (height - width) \ 2 : d.scale = width / 10.0 : End If

            Debug.Assert(stick.Limbs(0).CurrentLinePaint.Paint.Kind = FillKind.RGB, "expected RGB for limb(0) color, which is used as background")
            Dim clrBackground = stick.Limbs(0).CurrentLinePaint.Paint.color
            r.Rectangle(left, top, width, height, 0, StickRGB.Invisible, clrBackground, "background")
            For Each shape In stick.Shapes
                If shape Is Nothing Then
                    For Each limb In stick.Limbs.Skip(1)
                        d.DrawLimb(limb)
                    Next
                Else
                    d.DrawShape(shape)
                End If
            Next
        End Sub

        Private Function g2b2(bp As BodyPoint) As ScreenPoint
            Dim xx = CInt((bp.x + stick.CurrentAnchorX + 5) * scale + 0.5) + soffx
            Dim yy = CInt((bp.y + stick.CurrentAnchorY + 5) * scale + 0.5) + soffy
            Return New ScreenPoint With {.x = xx, .y = yy}
        End Function

        Sub DrawLimb(limb As Limb)
            Dim linecolor = If(limb.CurrentLinePaint.Paint.Kind = FillKind.RGB, limb.CurrentLinePaint.Paint.color, StickRGB.Invisible)
            Dim thickness = If(limb.Thickness = -1, 0, limb.Thickness * scale * 0.03)

            Dim pt0 = g2b2(limb.CurrentStartPoint), lx0 = pt0.x, ly0 = pt0.y
            Dim pt1 = g2b2(limb.CurrentEndPoint), lx1 = pt1.x, ly1 = pt1.y

            If limb.Kind = LimbKind.RotatingLine OrElse limb.Kind = LimbKind.StretchingLine Then
                r.Line(lx0, ly0, lx1, ly1, thickness, linecolor, limb.Tag)
            ElseIf limb.Kind = LimbKind.RotatingArc Then
                Dim radius = limb.MaxLength * scale
                Dim angA = If(limb.RotatingAngleMaxSweep >= 0, limb.CurrentAngMin, limb.CurrentAng), angB = If(limb.RotatingAngleMaxSweep >= 0, limb.CurrentAng, limb.CurrentAngMin)
                Dim xA = lx0 + CInt(radius * Math.Cos(angA)), yA = ly0 + CInt(radius * Math.Sin(angA))
                Dim xB = lx0 + CInt(radius * Math.Cos(angB)), yB = ly0 + CInt(radius * Math.Sin(angB))
                Dim ddAB = (xB - xA) * (xB - xA) + (yB - yA) * (yB - yA)
                Dim sweepAngle = angB - angA
                If (ddAB < 5 * 5 AndAlso angB - angA < Math.PI / 2) OrElse radius <= 2 Then
                    r.Line(xA, yA, xB, yB, thickness, linecolor, limb.Tag) ' draw a straight line for very short things
                Else
                    r.Arc(CInt(lx0 - radius), CInt(ly0 - radius), CInt(radius * 2), CInt(radius * 2), angA, sweepAngle, thickness, linecolor, limb.Tag)
                End If
            ElseIf limb.Kind = LimbKind.StretchingCircle Then
                Dim dxy = limb.CurrentEndPoint - limb.CurrentStartPoint, dd = dxy.Length, d = CInt(scale * dd)
                r.Ellipse(lx0 - d, ly0 - d, d * 2, d * 2, thickness, linecolor, StickRGB.Invisible, limb.Tag)
            End If
        End Sub

        Sub DrawShape(shape As Shape)
            Dim thickness = If(shape.Thickness = -1, 0, shape.Thickness * scale * 0.03)
            Dim line_color = If(shape.CurrentLinePaint.Paint.Kind = FillKind.RGB, shape.CurrentLinePaint.Paint.color, StickRGB.Invisible)
            Dim brush_color = If(shape.CurrentFillPaint.Paint.Kind = FillKind.RGB, shape.CurrentFillPaint.Paint.color, StickRGB.Invisible)
            Dim ishape = stick.Shapes.IndexOf(shape)

            Dim IsSimpleCircle = shape.Points.Count = 2 AndAlso shape.Points(0).Limb Is shape.Points(1).Limb AndAlso shape.Points(0).UseStartOfArc <> shape.Points(1).UseStartOfArc AndAlso shape.Points(0).Limb.Kind = LimbKind.StretchingCircle
            If IsSimpleCircle AndAlso shape.CurrentFillPaint.Paint.Kind = FillKind.Bitmap Then
                Dim limb = shape.Points(0).Limb
                Dim name = shape.CurrentFillPaint.Paint.bmp
                Dim dxy = limb.CurrentEndPoint - limb.CurrentStartPoint, rad = dxy.Length
                Dim pt0 = g2b2(limb.CurrentStartPoint.Offset(-rad, -rad)), pt1 = g2b2(limb.CurrentStartPoint.Offset(rad, rad))
                ' adjust the rect so the bitmap is kept proportional
                Dim sz = r.GetBitmapSize(name)
                Dim f = sz.Item1 / sz.Item2
                If f > 1 Then
                    Dim tcy = (pt0.y + pt1.y) \ 2, h = CInt((pt1.y - pt0.y) / f)
                    pt0.y = tcy - h \ 2 : pt1.y = tcy + h \ 2
                Else
                    Dim tcx = (pt0.x + pt1.x) \ 2, w = CInt((pt1.x - pt0.x) * f)
                    pt0.x = tcx - w \ 2 : pt1.x = tcx + w \ 2
                End If
                Dim ang = limb.CurrentAng, c = Math.Cos(ang), s = Math.Sin(ang), cx = (pt1.x - pt0.x) / 2, cy = (pt1.y - pt0.y) / 2
                r.DrawBitmap(name, pt0.x, pt0.y, pt1.x - pt0.x, pt1.y - pt0.y, c, s, -s, c, cx + cy * s - cx * c, cy - cy * c - cx * s, shape.Tag)
                Return
            End If

            If IsSimpleCircle Then
                Dim limb = shape.Points(0).Limb
                Dim dxy = limb.CurrentEndPoint - limb.CurrentStartPoint, rad = dxy.Length
                Dim pt0 = g2b2(limb.CurrentStartPoint.Offset(-rad, -rad))
                Dim pt1 = g2b2(limb.CurrentStartPoint.Offset(rad, rad))

                r.Ellipse(pt0.x, pt0.y, pt1.x - pt0.x, pt1.y - pt0.y, thickness, line_color, brush_color, shape.Tag)
                Return
            End If

            ' not a simple circle, so we do it the long way...,
            ' first, build the pt[] array.
            Dim ept As New List(Of Tuple(Of Integer, Integer))
            For pi = 0 To shape.Points.Count - 1
                Dim j0 = shape.Points(pi), j1 = shape.Points((pi + 1) Mod shape.Points.Count)
                Dim limb0 = j0.Limb
                If j0.Limb Is j1.Limb AndAlso j0.UseStartOfArc <> j1.UseStartOfArc AndAlso (limb0.Kind = LimbKind.RotatingArc OrElse limb0.Kind = LimbKind.StretchingCircle) Then
                    Dim arange = Math.Abs(j0.Limb.CurrentAng - j0.Limb.CurrentAngMin)
                    If j0.Limb.Kind = LimbKind.StretchingCircle Then arange = 1.96 * Math.PI
                    Dim perim = scale * arange * j0.Limb.MaxLength
                    Dim dnumdivs = 1 + arange * 12 / Math.PI
                    Dim mul = 1 + Math.Log(perim / 50) / Math.Log(2) : If mul < 1 Then mul = 1
                    dnumdivs *= mul : Dim numdivs = CInt(dnumdivs)
                    If numdivs > 100 Then numdivs = 100
                    For fi = 0 To numdivs - 1
                        Dim f = CDbl(fi) / CDbl(numdivs)
                        If Not j0.UseStartOfArc Then f = 1.0 - f
                        Dim bp = GetPosAlongArc(j0.Limb, f), sp = g2b2(bp)
                        ept.Add(Tuple.Create(sp.x, sp.y))
                    Next
                Else
                    Dim bp = GetPosOfJoint(j0), sp = g2b2(bp)
                    ept.Add(Tuple.Create(sp.x, sp.y))
                End If
            Next
            If shape.Points.Count > 2 Then ept.Add(ept(0)) ' close it if necessary
            r.Polygon(ept, shape.IsFillAlternate, thickness, line_color, brush_color, shape.Tag)
        End Sub

        Shared Function GetPosOfJoint(joint As Joint) As BodyPoint
            If joint.UseStartOfArc AndAlso (joint.Limb.Kind = LimbKind.RotatingArc OrElse joint.Limb.Kind = LimbKind.StretchingCircle) Then
                Return GetPosAlongArc(joint.Limb, 0.0)
            Else
                Return joint.Limb.CurrentEndPoint
            End If
        End Function

        Shared Function GetPosAlongArc(limb As Limb, f As Double) As BodyPoint
            If limb.Kind = LimbKind.RotatingArc Then
                Dim d = limb.MaxLength, fi = 1.0 - f
                Dim angA = If(limb.RotatingAngleMaxSweep < 0, limb.CurrentAngMin, limb.CurrentAng)
                Dim angB = If(limb.RotatingAngleMaxSweep < 0, limb.CurrentAng, limb.CurrentAngMin)
                Dim ang = If(limb.RotatingAngleMaxSweep < 0, angA * fi + angB * f, angA * f + angB * fi)
                Return limb.CurrentStartPoint.Offset(d * Math.Cos(ang), d * Math.Sin(ang))
            ElseIf limb.Kind = LimbKind.StretchingCircle Then
                Dim dxy = limb.CurrentEndPoint - limb.CurrentStartPoint, d = dxy.Length
                Dim angB = limb.CurrentAng, angA = angB - 1.96 * Math.PI, fi = 1.0 - f, ang = angA * fi + angB * f
                Return limb.CurrentStartPoint.Offset(d * Math.Cos(ang), d * Math.Sin(ang))
            Else
                Throw New ArgumentException("GetPosAlongArc should only be called on an arc/circle")
            End If
        End Function

    End Class

End Namespace


' LOGIC FOR RESPONDING TO FREQUENCY OVER TIME:
'  cmul=0;
'bool adjust=false;
'  cmul = ((double)(nowtime-prevtime))/10.0; ' diff as a multiple of 10ms, the standard interval
'    ' The mmax-ratings will fade over time
'    if (GotSoundYet)
'    { if (cmul>10) ' we fade no more frequently than ten times a second.
'      { for (int p=0; p<6; p++)
'        { mmax[p]*=0.999; kmmax[p]*=0.997;
'          if (mmin[p]<22) mmin[p]=22; mmin[p]*=1.002;
'          if (kmmin[p]<12) kmmin[p]=12; kmmin[p]*=1.002;
'        }
'        adjust=true;
'        prevtime=nowtime;
'      }
'    }
'
' the frequency[] array is 1024 elements big, first element 20Hz, last one 22050Hz. Approx Freq(i) = 20 + i*21.5
' So our bands are, in hertz
' 0:  450 -  772
' 1:  794 - 1117
' 2: 1134 - 1461
' 3: 1480 - 1827
' 4: 1827 - 2171
' 5: 2171 - 2515
' Or, expressed in terms of which elements of the frequence[] array,
' 0: freq[20...35], 
' 1: freq[36...51]
' 2: freq[52...67]
' 3: freq[68...83]
' 4: freq[84...99]
' 5: freq[100...115]
'  int boff=20, bwidth=16;
'  for (int p=0; p<6; p++) 
'  { l[p]=0; r[p]=0; int i;
'    for (i=0; i<bwidth; i++)
'    { l[p] += pLevels->frequency[0][boff+p*bwidth+i];
'      r[p] += pLevels->frequency[1][boff+p*bwidth+i];
'    }
'    if (l[p]>mmax[p]) mmax[p]=l[p];
'    if (r[p]>mmax[p]) mmax[p]=r[p];
'    if (adjust)
'    { if (l[p]<mmin[p] || r[p]<mmin[p]) mmin[p]*=0.9;
'    }
'  }
'  '
'  ' the waveform is also 1024 elements big, stored as unsigned chars.
'  ' 0.vocals=diff, 1.music=average
'  int kvoc=0, kmus=0;
'  for (int i=0; i<1024; i++)
'  { int voc = (pLevels->waveform[0][i]+pLevels->waveform[1][i])/2-128;
'    int mus = (int)(pLevels->waveform[0][i])-(int)(pLevels->waveform[1][i]);
'    kvoc += voc*voc; kmus += mus*mus;
'  }
'  k[0]=(int)sqrt(kvoc); k[1]=2*(int)sqrt(kmus);
'  if (k[0]<kmmin[0]) kmmin[0]=k[0];  if (k[0]>kmmax[0]) kmmax[0]=k[0];
'  if (k[1]<kmmin[1]) kmmin[1]=k[1];  if (k[1]>kmmax[1]) kmmax[1]=k[1];
'for (int p=0; p<6; p++)
'{ l[p] = (l[p]-mmin[p]) / (mmax[p]-mmin[p]+1); if (l[p]<0) l[p]=0; l[p]*=l[p];
'  r[p] = (r[p]-mmin[p]) / (mmax[p]-mmin[p]+1); if (r[p]<0) r[p]=0; r[p]*=r[p];
'}
'for (int p=0; p<2; p++)
'{ k[p] = (k[p]-kmmin[p]) / (kmmax[p]-kmmin[p]+1); if (k[p]<0) k[p]=0; k[p]*=k[p];
'}
'for (int p=0; p<6; p++)
'{ if (l[p]>0.01 || r[p]>0.01) {if (!GotSoundYet) prevtime=nowtime; GotSoundYet=true;}
'}

