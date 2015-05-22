' The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class MainPage
    Inherits Page

    Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
        Dim d As New ObservableCollection(Of Patient)
        d.Add(New Patient With {.Name = "Dawn", .Health = "Healthy", .Thumbnail = "http://tinyurl.com/mnelmpf"})
        listview1.DataContext = d
    End Sub

End Class

Public Class Patient
    Public Property Name As String
    Public Property Health As String
    Public Property Thumbnail As String
End Class

Public Class SamplePatientData : Inherits ObservableCollection(Of Patient)
    Public Sub New()
        Add(New Patient With {.Name = "Arthur", .Health = "Healthy", .Thumbnail = "http://tinyurl.com/ldde88y"})
        Add(New Patient With {.Name = "Betty", .Health = "Healthy", .Thumbnail = "http://tinyurl.com/k95djeu"})
        Add(New Patient With {.Name = "Charles", .Health = "Healthy", .Thumbnail = "http://tinyurl.com/mqdj5n2"})
    End Sub
End Class
