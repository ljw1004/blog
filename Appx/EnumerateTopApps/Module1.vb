Imports System.IO.Compression
Imports System.IO
Imports Newtonsoft.Json.Linq
Imports System.Runtime.CompilerServices
Imports <xmlns="http://www.w3.org/1999/xhtml">

' EnumerateTopApps
' ==================
'
' I couldn't find good data on the top Windows Store / Phone Store apps, nor a good way to fetch that data.
' So this app is a workaround...
'
' To gather Store data: launch Fiddler (http://www.telerik.com/download/fiddler), launch the Store app,
' click on "App Top Charts" and browse through everything, and then "Game Top Charts".
' This way Fiddler has captured HTTP traffic with JSON payloads concerning all of the top apps.
' Within Fiddler, File>Export>AllSessions>Raw, and zip up the results.
' 
' To gather Phone data: again launch Fiddler, but this time navigate in your browser to http://www.windowsphone.com/en-us/store
' Again scroll through all of the top apps and games, and again export all Fiddler sessions as "raw"
' and zip up the results. This time Fiddler has captured HTTP traffic with HTML payloads concerning
' all of the top apps.
'
' Once you have those zipped-up captures, that's where this app comes in. It enumerates all files in
' the zip, finds all the ones that have product-listings, and extracts the data.
' For example output, see "Results.xlsx". (I've tidied it up just a little).


Module Module1

    Sub Main()
        ExtractTopStoreAppsFromFiddlerDump("FiddlerDump-Store-2015.07.05.zip")
        ExtractTopPhoneAppsFromFiddlerDump("FiddlerDump-Phone-2015.07.05.zip")
    End Sub


    Sub ExtractTopPhoneAppsFromFiddlerDump(dumpfn As String)
        Dim fn = GetUniqueFileName($"{My.Computer.FileSystem.SpecialDirectories.Desktop}\TopPhoneApps.csv")

        Using zip = ZipFile.OpenRead(dumpfn),
                writer As New StreamWriter(fn, False, Text.Encoding.UTF8),
                csv As New CsvHelper.CsvWriter(writer)

            Dim products = From content In zip.FileContents
                           Let xml = TidyAsync(content).GetAwaiter().GetResult()
                           Let x = XDocument.Parse(xml)
                           From table In x...<table>.Where(Function(t0) t0.@class = "appList")
                           From td In table...<td>
                           Let p = PhoneProduct.FromTd(td)
                           Where p IsNot Nothing
                           Select p
                           Group By p.AppxName Into Group
                           Select Group.First

            csv.WriteRecords(products)
        End Using
        Process.Start(fn)
    End Sub

    Class PhoneProduct
        Public Property DisplayName As String
        Public Property AppxName As String  ' 92507698-67a2-e011-986b-78e7d1fa76f8
        Public Property MediaType As String ' Apps or Games
        Public Property Rating As Decimal
        Public Property RatingCount As Integer

        Public Shared Function FromTd(td As XElement) As PhoneProduct
            Static Dim RatingCodes As New Dictionary(Of String, Decimal) From {
                {"zeroPtZero", 0D}, {"zeroPtFive", 0.5D},
                {"onePtZero", 1D}, {"onePtFive", 1.5D},
                {"twoPtZero", 2D}, {"twoPtFive", 2.5D},
                {"threePtZero", 3D}, {"threePtFive", 3.5D},
                {"fourPtZero", 4D}, {"fourPtFive", 4.5D},
                {"fivePtZero", 5D}
                }
            Dim anchor = td.<a>.LastOrDefault
            If anchor Is Nothing Then Return Nothing
            Dim dataos = anchor.@<data-os>
            Dim dataov = anchor.@<data-ov>
            Dim dataov1 = dataov.Split(" "c)
            Dim dataov2 = dataov1(0).Split(";"c)
            Dim ratings = td...<div>
            Dim rating0 = ratings.Where(Function(r) r.@class.StartsWith("ratingSmall")).First.@class.Replace("ratingSmall ", "")
            Dim ratingCount0 = ratings.Where(Function(r) r.@class.StartsWith("ratingCount")).Value.Replace("Ratings: ", "")
            Return New PhoneProduct With {
                .MediaType = dataov2(0),
                .AppxName = dataov2(1),
                .DisplayName = anchor.Value.Trim(),
                .Rating = RatingCodes(rating0),
                .RatingCount = If(ratingCount0 = "No reviews", 0, CInt(ratingCount0))
                }
        End Function
    End Class


    Sub ExtractTopStoreAppsFromFiddlerDump(dumpfn As String)
        Dim fn = GetUniqueFileName($"{My.Computer.FileSystem.SpecialDirectories.Desktop}\TopStoreApps.csv")

        Using zip = ZipFile.OpenRead(dumpfn),
                writer As New StreamWriter(fn, False, Text.Encoding.UTF8),
                csv As New CsvHelper.CsvWriter(writer)

            Dim products = From content In zip.FileContents()
                           Where Not String.IsNullOrWhiteSpace(content)
                           Let json = JToken.Parse(content)
                           From productList In FindProductLists(json)
                           From product In productList.OfType(Of JObject)
                           Let p = product.ToObject(Of StoreProduct)
                           Where Not String.IsNullOrEmpty(p.PackageFamilyName)
                           Where Not p.RecommendationReason Like "*you have*"
                           Select p
                           Group By p.PackageFamilyName Into Group
                           Select Group.First

            csv.WriteRecords(products)
        End Using
        Process.Start(fn)
    End Sub

    Class StoreProduct
        Public Property PackageFamilyName As String
        Public Property AverageRating As Decimal
        Public Property RatingCount As Integer
        Public Property Title As String
        Public Property ProductType As String
        Public Property MediaType As String
        Public Property Categories As List(Of String) ' can be read by Json, but not written by CsvHelper
        Public ReadOnly Property MainCategory As String ' this one is written by CsvHelper
            Get
                Return Categories?.FirstOrDefault
            End Get
        End Property
        Public Property ProductId As String
        Public Property Description As String
        Public Property PublisherName As String
        Public Property PublisherId As String
        Public Property RecommendationReason As String
    End Class

    <Extension>
    Iterator Function FileContents(zip As ZipArchive) As IEnumerable(Of String)
        For Each zipEntry In zip.Entries
            Using stream = zipEntry.Open(), reader As New StreamReader(stream)
                Yield reader.ReadToEnd()
            End Using
        Next
    End Function


    ''' <summary>
    ''' Make sure to return a filename like 'fn' that is writeable, appending a suffix if necessary
    ''' </summary>
    Function GetUniqueFileName(fn As String) As String
        Dim dir = Path.GetDirectoryName(fn)
        Dim name = Path.GetFileNameWithoutExtension(fn)
        Dim ext = Path.GetExtension(fn)
        Dim i = 0
        Do
            Dim suffix = If(i = 0, "", CStr(i))
            Dim tfn = $"{dir}\{name}{suffix}{ext}"
            If Not File.Exists(tfn) Then Return tfn
            Try
                File.Delete(tfn) : Return tfn
            Catch ex As Exception
            End Try
            i += 1
        Loop
    End Function

    ''' <summary>
    ''' Recursively walks the JSON and returns all "Products" arrays in it
    ''' </summary>
    Iterator Function FindProductLists(json As JToken) As IEnumerable(Of JArray)
        Dim q As New LinkedList(Of JToken)({json})

        While q.Any
            Dim j = q.First.Value : q.RemoveFirst()
            If j.Type = JTokenType.Object Then
                For Each p In CType(j, JObject)
                    Dim key = p.Key, value = p.Value
                    If key = "Products" AndAlso value.Type = JTokenType.Array Then Yield CType(value, JArray) : Continue For
                    q.AddLast(value)
                Next
            ElseIf j.Type = JTokenType.Array Then
                For Each i In CType(j, JArray)
                    q.AddLast(i)
                Next
            End If
        End While
    End Function

    ''' <summary>
    ''' Given an html string, turns it into valid xhtml - suitable for XDocument.Parse()
    ''' </summary>
    Async Function TidyAsync(html As String) As Task(Of String)
        Using tidy As New System.Diagnostics.Process
            Dim cmd = "tidy.exe"
            Dim args = "-asxml -numeric -quiet --doctype omit --new-blocklevel-tags section --force-output true"
            ' MONO: XElement.Load throws an exception if DOCTYPE is present. CLR: doesn't. Hence we omit the DOCTYPE.
            tidy.StartInfo.FileName = cmd
            tidy.StartInfo.Arguments = args
            tidy.StartInfo.UseShellExecute = False
            tidy.StartInfo.RedirectStandardInput = True
            tidy.StartInfo.RedirectStandardOutput = True
            tidy.StartInfo.RedirectStandardError = True
            tidy.Start()
            '
            Dim outputTask = tidy.StandardOutput.ReadToEndAsync
            Dim errorTask = tidy.StandardError.ReadToEndAsync()
            Await tidy.StandardInput.WriteAsync(html)
            tidy.StandardInput.Close()
            Dim op = Await outputTask
            Dim err = Await errorTask

            Await Task.Run(Sub() tidy.WaitForExit(5000))
            If Not tidy.HasExited Then
                tidy.Kill()
                Await Task.Run(Sub() tidy.WaitForExit(2000))
                Return Nothing
            End If

            ' We had already asked ("-numeric") for tidy to escape non-ascii characters. But
            ' nonetheless, XElement.Load will throw an exception if there are any, and we really
            ' don't want that, so we'll do belt-and-braces here:
            Dim op2 As New Text.StringBuilder(op.Length)
            For i = 0 To op.Length - 1
                Dim c = AscW(op(i))
                If (c >= 32 AndAlso c < 127) OrElse c = 13 OrElse c = 10 OrElse c = 9 Then
                    op2.Append(op(i))
                End If
            Next
            Return op2.ToString()
        End Using
    End Function


End Module
