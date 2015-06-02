Imports System.Numerics
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Effects
Imports Windows.Storage
Imports Windows.UI

NotInheritable Class App
    Inherits Application

    Public Const CHEIGHT = 240
    Public Const CWIDTH = 320
    Public Property Pixels As Color()

    Public Event Loaded As Action
    Public Event Unloading As Action

    Public Shared Shadows ReadOnly Property Current As App
        Get
            Return CType(Application.Current, App)
        End Get
    End Property

    Protected Overrides Sub OnLaunched(e As Windows.ApplicationModel.Activation.LaunchActivatedEventArgs)
        Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)
        If rootFrame Is Nothing Then
            rootFrame = New Frame()
            Window.Current.Content = rootFrame
            Pixels = New Color(CHEIGHT * CWIDTH - 1) {}
            LoadAsync().FireAndForget()
        End If
        If rootFrame.Content Is Nothing Then rootFrame.Navigate(GetType(MainPageV2), e.Arguments)
        Window.Current.Activate()
    End Sub

    Async Function LoadAsync() As Task
        Dim fn = $"pixels_{CWIDTH}x{CHEIGHT}.dat"
        Dim file = Await TryGetFileAsync(ApplicationData.Current.LocalFolder, fn)
        If file Is Nothing Then file = Await TryGetFileAsync(Package.Current.InstalledLocation, fn)
        If file IsNot Nothing Then
            Using stream = Await file.OpenStreamForReadAsync()
                Dim buf = New Byte(Pixels.Length * 4 - 1) {}
                Await stream.ReadAsync(buf, 0, buf.Length)
                For i = 0 To Pixels.Length - 1
                    Pixels(i) = Color.FromArgb(buf(i * 4 + 0), buf(i * 4 + 1), buf(i * 4 + 2), buf(i * 4 + 3))
                Next
            End Using
        End If
        RaiseEvent Loaded()
    End Function

    Private Async Function TryGetFileAsync(folder As StorageFolder, fn As String) As Task(Of StorageFile)
        Try
            Return Await folder.GetFileAsync(fn)
        Catch ex As FileNotFoundException
            Return Nothing
        End Try
    End Function

    Private Async Sub OnSuspending(sender As Object, e As SuspendingEventArgs) Handles Me.Suspending
        Dim deferral = e.SuspendingOperation.GetDeferral()
        RaiseEvent Unloading()
        Dim file = Await ApplicationData.Current.LocalFolder.CreateFileAsync($"pixels_{CWIDTH}x{CHEIGHT}.dat", CreationCollisionOption.ReplaceExisting)
        Using stream = Await file.OpenStreamForWriteAsync()
            Dim buf = New Byte(Pixels.Length * 4 - 1) {}
            For i = 0 To Pixels.Length - 1
                buf(4 * i + 0) = Pixels(i).A : buf(4 * i + 1) = Pixels(i).R : buf(4 * i + 2) = Pixels(i).G : buf(4 * i + 3) = Pixels(i).B
            Next
            Await stream.WriteAsync(buf, 0, buf.Length)
        End Using
        deferral.Complete()
    End Sub
End Class





Public Module Utils
    Public Function IntPow(x As Integer, y As Integer) As Integer
        Dim r = 1
        For i = 0 To y - 1
            r *= x
        Next
        Return r
    End Function

    Public Sub Swap(Of T)(ByRef x As T, ByRef y As T)
        Dim temp = x : x = y : y = temp
    End Sub

    <Extension>
    Function Invert(m As Matrix3x2) As Matrix3x2
        Dim r As Matrix3x2
        Dim b = Matrix3x2.Invert(m, r)
        If Not b Then Throw New ArgumentException(NameOf(m))
        Return r
    End Function

    <Extension>
    Public Function AsVisibility(b As Boolean) As Visibility
        Return If(b, Visibility.Visible, Visibility.Collapsed)
    End Function

    <Extension>
    Async Sub FireAndForget(t As Task)
        Try
            Await t
        Catch ex As Exception
            Stop
        End Try
    End Sub
End Module


Public Class EffectWrapper(Of T)
    Private EndEffect As ICanvasImage
    Private FixSources As Action(Of T)

    Sub New(endEffect As ICanvasImage, fixSources As Action(Of T))
        Me.EndEffect = endEffect
        Me.FixSources = fixSources
    End Sub

    Function Update(value1 As T) As ICanvasImage
        FixSources(value1)
        Return EndEffect
    End Function

End Class


Public Class KernelTracker
    Public _Matrix As Single()
    Public _Touched As Boolean()
    '
    Public ConvolveMatrix As Single()
    Public ConvolveDivisor As Integer
    Public TransferTable As Single()

    Private Shared Function Rotate(m As Single(), o As Orientation) As Single()
        If m.Length <> 9 Then Throw New ArgumentOutOfRangeException("m", "Only works with 3x3 matrices")
        Select Case o
            Case Orientation.Landscape : Return {m(0), m(1), m(2), m(3), m(4), m(5), m(6), m(7), m(8)}
            Case Orientation.Portrait : Return {m(2), m(5), m(8), m(1), m(4), m(7), m(0), m(3), m(6)}
            Case Orientation.LandscapeFlipped : Return {m(8), m(7), m(6), m(5), m(4), m(3), m(2), m(1), m(0)}
            Case Orientation.PortraitFlipped : Return {m(6), m(3), m(0), m(7), m(4), m(1), m(8), m(5), m(2)}
            Case Else : Throw New ArgumentOutOfRangeException("o")
        End Select
    End Function

    Shared Function MakeEffect(src1 As KernelTracker, src2 As KernelTracker, o As Orientation) As EffectWrapper(Of ICanvasImage)
        Dim conv1 As New ConvolveMatrixEffect With {.KernelMatrix = Rotate(src1.ConvolveMatrix, o), .Divisor = src1.ConvolveDivisor, .PreserveAlpha = True}
        Dim tran1 As New DiscreteTransferEffect With {.Source = conv1, .RedTable = src1.TransferTable, .GreenTable = src1.TransferTable, .BlueTable = src1.TransferTable}
        Dim conv2 As New ConvolveMatrixEffect With {.KernelMatrix = Rotate(src2.ConvolveMatrix, o), .Divisor = src2.ConvolveDivisor, .PreserveAlpha = True}
        Dim tran2 As New DiscreteTransferEffect With {.Source = conv2, .RedTable = src2.TransferTable, .GreenTable = src2.TransferTable, .BlueTable = src2.TransferTable}

        Dim compo As New CompositeEffect With {.Mode = CanvasComposite.Add}
        compo.Sources.Add(tran1)
        compo.Sources.Add(tran2)

        Return New EffectWrapper(Of ICanvasImage)(compo, Sub(s) If True Then conv1.Source = s : conv2.Source = s)
    End Function

    Shared Function Generate(values As IEnumerable(Of Single), touches As IEnumerable(Of Kernel), lambda As Func(Of Func(Of Integer, Single), Single)) As KernelTracker
        touches = touches.Reverse() ' so that when authors write "most significant item first", it's reflected in the transfer table
        Dim values_Count = values.Count
        Dim k As New KernelTracker
        k._Touched = {False, False, False, False, False, False, False, False, False}
        k._Matrix = New Single(8) {}
        Dim getMatrixLambda = Function(pos As Integer)
                                  k._Touched(pos) = True
                                  Return k._Matrix(pos)
                              End Function
        '
        Dim results As New List(Of Single)
        For i = 0 To IntPow(values_Count, touches.Count) - 1
            Dim v = i
            For Each t In touches
                k._Matrix(t) = values(v Mod values_Count)
                v \= values_Count
            Next
            Dim r = lambda(getMatrixLambda)
            results.Add(r)
        Next
        '
        k.TransferTable = results.ToArray()
        k.ConvolveMatrix = New Single(8) {}
        k.ConvolveDivisor = 0
        Dim m = 1
        For Each t In touches
            k.ConvolveMatrix(t) = m
            k.ConvolveDivisor += m
            m *= values_Count
        Next
        '
        For i = Kernel._First To Kernel._Last
            If k._Touched(i) AndAlso Not touches.Contains(i) Then Throw New ArgumentException($"Failed to declare that this kernel touches {i}")
            If Not k._Touched(i) AndAlso touches.Contains(i) Then Throw New ArgumentException($"Declared that this kernel touches {i} but it doesn't")
        Next
        Return k
    End Function

End Class

Public Enum Kernel
    _First = 0
    UpLeft = 8
    Up = 7
    UpRight = 6
    Left = 5
    Center = 4
    Right = 3
    DownLeft = 2
    Down = 1
    DownRight = 0
    _Last = 8
End Enum

Public Enum Orientation
    _First = 0
    Landscape = 0
    Portrait = 1
    LandscapeFlipped = 2
    PortraitFlipped = 3
    _Last = 3
End Enum

Public Enum Phase
    _First = 0
    DownThenLeft = 0
    DownThenRight = 1
    LeftThenDown = 2
    RightThenDown = 3
    _Last = 3
End Enum

Public Class DisplayTransform
    Public Scale As Single
    Public Offset As Vector2
    Public Rotation As Matrix3x2
End Class

