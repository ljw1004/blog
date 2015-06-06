Option Strict On
Imports System.Threading.Tasks
Imports System.Runtime.InteropServices

Namespace Global

    Public Class FFTResults
        Const FREQ_0_320 = 0
        Const FREQ_320_480 = 1
        Const FREQ_480_960 = 2
        Const FREQ_960_1280 = 3
        Const FREQ_1280_1600 = 4
        Const FREQ_1600_1960 = 5

        Public left(5) As Double
        Public right(5) As Double
        Public vocals As Double
        Public music As Double
        Public rightminusleft(5) As Double ' not used except in return from LevelsMonitor.Update
    End Class



    Public Class FFT

        ' This code is an adaption+optimization of FFT code by Gerald T. Beauregard, which he placed under the MIT-license.
        ' The following license comes from his code.

        ' -------------------------------------------------------------------------
        ' *** THIS LICENSE GOVERNS THE SOURCE-CODE FOR "FFT_VB_FASTER" BUT NOTHING ELSE ***
        ' 
        ' FFT_VB_FASTER: Performs an in-place complex FFT.
        ' Based on original work released under the MIT License (c) 2010 Gerald T. Beauregard
        ' Derivative work also released under the MIT license (c) 2012 Lucian Wischik
        '
        ' Permission is hereby granted, free of charge, to any person obtaining a copy of the class FFT_VB_FASTER (the "Software"),
        ' to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish,
        ' distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
        ' subject to the following conditions:
        '
        ' The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
        '
        ' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
        ' OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
        ' LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR
        ' IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
        ' -----------------------------------------------------------------------------

        ' TODO: Use a "real-only FFT" of size 512, rather than this 1024-element FFT. That will make it run twice as fast. Here's how:
        ' http://www.katjaas.nl/realFFT/realFFT2.html
        ' http://processors.wiki.ti.com/index.php/Efficient_FFT_Computation_of_Real_Input

        Private Class FFTElement
            Public re As Single, im As Single
            Public [next] As FFTElement ' on the CLR, using a linked-list is faster than array-element-access!
        End Class

        ' For performance, we pre-compute two linked-list structures.
        ' Each time FFT is performed, it copies a given Short() array of PCM data into those structures,
        ' then does the computation, then returns the results
        ' For performance, we actually have only a single "results" object, and each time FFT is performed,
        ' it merely returns that re-used object.
        Private Const nElements = 1024, lognElements = 10
        Private Elements(nElements - 1) As FFTElement
        Private ElementsIndirection(nElements - 1) As FFTElement ' used for unpacking elements, since they come out of FFT in the wrong order

        Public Sub New()
            For k = nElements - 1 To 0 Step -1
                Dim [next] = If(k = nElements - 1, Nothing, Elements(k + 1))
                Elements(k) = New FFTElement With {.next = [next]}
            Next k
            For k = 0 To nElements - 1
                Dim x = k, bitreverse = 0
                For i = 0 To lognElements - 1
                    bitreverse <<= 1
                    bitreverse = bitreverse Or (x And 1)
                    x >>= 1
                Next
                ElementsIndirection(bitreverse) = Elements(k)
            Next
        End Sub

        Public Sub DoFFT(arr As Short(), out_results As FFTResults)
            CopyArrayIntoElements(arr, 0)
            DoFFTOnElements()
            CopyFFTdElementsIntoBins(out_results.left)
            '
            CopyArrayIntoElements(arr, 1)
            DoFFTOnElements()
            CopyFFTdElementsIntoBins(out_results.right)
            '
            CopyArrayIntoKaraoke(arr, out_results.music, out_results.vocals)
        End Sub

        Private Sub CopyArrayIntoElements(arr As Short(), offset As Integer)
            For i = 0 To nElements - 1
                Elements(i).re = arr(i * 2 + offset) : Elements(i).im = 0
            Next
        End Sub

        Private Sub CopyFFTdElementsIntoBins(bins As Double())
            For i = 0 To 5
                Dim fmin = 450 + i * 344, fmax = fmin + 344
                Dim jmin = nElements * fmin \ 44100, jmax = nElements * fmax \ 44100
                Dim tot = 0.0
                For j = jmin To jmax - 1
                    Dim e = ElementsIndirection(j)
                    tot += e.re * e.re + e.im * e.im
                Next
                Dim av = tot / (jmax - jmin)
                bins(i) = Math.Pow(av, 0.25)
            Next
        End Sub

        Private Sub CopyArrayIntoKaraoke(arr As Short(), ByRef power_music As Double, ByRef power_vocals As Double)
            Dim tvoc = 0.0F, tmus = 0.0F
            For i = 0 To nElements - 1
                Dim left As Single = arr(i * 2), right As Single = arr(i * 2 + 1)
                Dim voc = (left + right) / 2, mus = left - right
                tvoc += voc * voc : tmus += mus * mus
            Next
            power_music = Math.Pow(tmus / nElements, 0.4) : power_vocals = Math.Pow(tvoc / nElements, 0.4)
        End Sub

        Private Sub DoFFTOnElements()
            Dim numFlies = nElements >> 1, span = nElements >> 1, spacing = nElements, wIndexStep = 1
            For stage = 0 To lognElements - 1
                Dim wAngleInc = wIndexStep * -2.0 * Math.PI / nElements, wMulRe = CSng(Math.Cos(wAngleInc)), wMulIm = CSng(Math.Sin(wAngleInc))
                For start = 0 To nElements - 1 Step spacing
                    Dim xTop = Elements(start), xBot = Elements(start + span), wRe = 1.0F, wIm = 0.0F
                    For flyCount = 0 To numFlies - 1
                        Dim xTopRe = xTop.re, xTopIm = xTop.im, xBotRe = xBot.re, xBotIm = xBot.im
                        xTop.re = xTopRe + xBotRe
                        xTop.im = xTopIm + xBotIm
                        xBotRe = xTopRe - xBotRe
                        xBotIm = xTopIm - xBotIm
                        xBot.re = xBotRe * wRe - xBotIm * wIm
                        xBot.im = xBotRe * wIm + xBotIm * wRe
                        xTop = xTop.next
                        xBot = xBot.next
                        Dim tRe = wRe
                        wRe = wRe * wMulRe - wIm * wMulIm
                        wIm = tRe * wMulIm + wIm * wMulRe
                    Next
                Next
                numFlies >>= 1 : span >>= 1 : spacing >>= 1 : wIndexStep <<= 1
            Next
        End Sub

    End Class

End Namespace
