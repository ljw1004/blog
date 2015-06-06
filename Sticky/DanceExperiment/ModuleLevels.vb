Option Strict On

Namespace Global
    Public Class AudioBuffer
        Public id As Integer = getid()
        Public SequenceNumber As Integer
        Public Arr As Short()
        Public RawFourierData As New FFTResults
        Public IsDisposed As Boolean

        Private Shared Function getid() As Integer
            Static Dim count As Integer = 0
            count += 1
            Return count
        End Function
    End Class


    Public Class LinearRegressionMonitor
        Implements IProgress(Of Double)

        Public Property IgnoreReportsWithinMs As Integer = 20
        Public Property ForgetDataBeforeMs As Integer = 5000

        Private StartTime As DateTimeOffset = DateTimeOffset.Now
        Private MostRecentX As Double
        Private Reports As New SortedDictionary(Of Double, Double)

        Public Sub Report(value As Double) Implements IProgress(Of Double).Report
            Dim x = (DateTimeOffset.Now - StartTime).TotalMilliseconds
            Dim y = value
            '
            If MostRecentX + IgnoreReportsWithinMs > x Then Return Else MostRecentX = x ' discard progress reports that come more than once every 20ms
            '
            SyncLock Reports
                Reports.Add(x, y)
                For Each olderThanFiveSeconds In (From x2 In Reports.Keys Where x2 + ForgetDataBeforeMs < x).ToArray()
                    Reports.Remove(olderThanFiveSeconds)
                Next
            End SyncLock
        End Sub

        Public ReadOnly Property Current As Double?
            Get
                Return Estimate(DateTimeOffset.Now)
            End Get
        End Property

        Public Function Estimate(time As DateTimeOffset) As Double?
            SyncLock Reports
                If Reports.Count = 0 Then Return Nothing
                ' linear regression, thanks to John Wakefield http://dynamicnotions.blogspot.com/2009/05/linear-regression-in-c.html
                Dim xtot = 0.0, ytot = 0.0 : For Each kv In Reports : xtot += kv.Key : ytot += kv.Value : Next
                Dim xav = xtot / Reports.Count, yav = ytot / Reports.Count
                Dim v1 = 0.0, v2 = 0.0
                For Each kv In Reports
                    Dim dx = kv.Key - xav
                    v1 += dx * (kv.Value - yav)
                    v2 += dx * dx
                Next
                Dim m = v1 / v2
                Dim c = yav - m * xav
                Dim x = (time - StartTime).TotalMilliseconds
                Dim y = m * x + c
                If Double.IsNaN(y) OrElse Double.IsInfinity(y) Then Return Nothing
                Return y
            End SyncLock
        End Function

        Public Sub Reset()
            SyncLock Reports
                Reports.Clear()
            End SyncLock
        End Sub

    End Class


    Public Class LevelsMonitor
        Public Min As New FFTResults
        Public Max As New FFTResults
        Public PeakMin As New FFTResults
        Public PeakMax As New FFTResults
        Private nextUpdateResetPeaks As Boolean = True

        Public Sub New()
            Reset()
        End Sub

        Public Sub Update(raw As FFTResults, output As FFTResults)
            For i = 0 To 5
                output.left(i) = Frac(raw.left(i), Min.left(i), Max.left(i))
                output.right(i) = Frac(raw.right(i), Min.right(i), Max.right(i))
                ' and the left/right balance
                Dim cmin = Math.Min(Min.left(i), Min.right(i)), cmax = Math.Max(Max.left(i), Max.right(i))
                Dim fl = Frac(raw.left(i), cmin, cmax), fr = Frac(raw.right(i), cmin, cmax)
                Dim f = (1.0 + fr - fl) * 0.5 : f = If(f < 0, 0, If(f > 1, 1, f))
                output.rightminusleft(i) = f
            Next
            output.vocals = Frac(raw.vocals, Min.vocals, Max.vocals)
            output.music = Frac(raw.music, Min.music, Max.music)

            ' and for our internal purposes, update the recent peaks
            If nextUpdateResetPeaks Then
                CopyInto(raw, PeakMin)
                CopyInto(raw, PeakMax)
                nextUpdateResetPeaks = False
            Else
                CopyMinInto(raw, PeakMin)
                CopyMaxInto(raw, PeakMax)
            End If
        End Sub

        Public Sub Decay(dt As TimeSpan)
            Dim cmul = dt.TotalMilliseconds / 50

            ' decay the maximums down
            Dim fm = Math.Pow(0.999, cmul), fk = Math.Pow(0.997, cmul)
            For i = 0 To 5 : Max.left(i) *= fm : Max.right(i) *= fm : Next
            Max.music *= fk : Max.vocals *= fk

            ' decay the minimums up
            fm = Math.Pow(1.005, cmul) : fk = fm
            For i = 0 To 5
                Min.left(i) *= fm : Min.left(i) = Math.Max(Min.left(i), 100)
                Min.right(i) *= fm : Min.right(i) = Math.Max(Min.right(i), 100)
            Next
            Min.music *= fk : Min.music = Math.Max(Min.music, 50)
            Min.vocals *= fk : Min.vocals = Math.Max(Min.vocals, 50)

            ' Grow Max if there were any larger PeakMax
            If Not nextUpdateResetPeaks Then
                For i = 0 To 5
                    Max.left(i) = Math.Max(Max.left(i), PeakMax.left(i))
                    Max.right(i) = Math.Max(Max.right(i), PeakMax.right(i))
                Next
                Max.vocals = Math.Max(Max.vocals, PeakMax.vocals)
                Max.music = Math.Max(Max.music, PeakMax.music)
            End If

            ' Shrink Min if there were any smaller PeakMin
            If Not nextUpdateResetPeaks Then
                fm = Math.Pow(0.9, cmul)
                For i = 0 To 5
                    If PeakMin.left(i) < Min.left(i) Then Min.left(i) *= fm
                    If PeakMin.right(i) < Min.right(i) Then Min.right(i) *= fm
                Next
                Min.vocals = Math.Min(Min.vocals, PeakMin.vocals)
                Min.music = Math.Min(Min.music, PeakMin.music)
            End If

            ' Sanity check
            For i = 0 To 5
                Min.left(i) = Math.Min(Min.left(i), Max.left(i))
                Max.left(i) = Math.Max(Min.left(i), Max.left(i))
                Min.right(i) = Math.Min(Min.right(i), Max.right(i))
                Max.right(i) = Math.Max(Min.right(i), Max.right(i))
            Next
            Min.vocals = Math.Min(Min.vocals, Max.vocals)
            Max.vocals = Math.Max(Min.vocals, Max.vocals)
            Min.music = Math.Min(Min.music, Max.music)
            Max.music = Math.Max(Min.music, Max.music)

            ' Make sure that the "peaks" arrays get updated next time
            nextUpdateResetPeaks = True
        End Sub

        Public Sub Reset()
            nextUpdateResetPeaks = True
            Min.vocals = 100 : Max.vocals = 1500
            Min.music = 100 : Max.music = 1500
            For i = 0 To 5
                Min.left(i) = 210 - 10 * i : Min.right(i) = Min.left(i)
                Max.left(i) = 1600 - 100 * i : Max.right(i) = Max.left(i)
            Next
        End Sub

        Private Function Frac(raw As Double, cmin As Double, cmax As Double) As Double
            Dim f = (raw - cmin) / (cmax - cmin + 1)
            f = If(f < 0, 0, If(f > 1, 1, f))
            Return f
        End Function

        Private Sub CopyInto(src As FFTResults, dst As FFTResults)
            For i = 0 To 5
                dst.left(i) = src.left(i)
                dst.right(i) = src.right(i)
            Next
            dst.music = src.music
            dst.vocals = src.vocals
        End Sub

        Private Sub CopyMinInto(src As FFTResults, dst As FFTResults)
            For i = 0 To 5
                dst.left(i) = Math.Min(src.left(i), dst.left(i))
                dst.right(i) = Math.Min(src.right(i), dst.right(i))
            Next
            dst.music = Math.Min(src.music, dst.music)
            dst.vocals = Math.Min(src.vocals, dst.vocals)
        End Sub

        Private Sub CopyMaxInto(src As FFTResults, dst As FFTResults)
            For i = 0 To 5
                dst.left(i) = Math.Max(src.left(i), dst.left(i))
                dst.right(i) = Math.Max(src.right(i), dst.right(i))
            Next
            dst.music = Math.Max(src.music, dst.music)
            dst.vocals = Math.Max(src.vocals, dst.vocals)
        End Sub


    End Class

    Public Module AudioExtensions
        <Runtime.CompilerServices.Extension>
        Public Async Sub FireAndForget(this As Threading.Tasks.Task)
            Try
                Await this
            Catch ex As Exception
                Call New Windows.UI.Popups.MessageDialog("Error: " & ex.Message).ShowAsync()
            End Try
        End Sub

        <Runtime.CompilerServices.Extension>
        Public Async Function NoThrowCancellation(this As Threading.Tasks.Task) As Threading.Tasks.Task
            Try
                Await this
            Catch ex As OperationCanceledException
            End Try
        End Function

    End Module

End Namespace