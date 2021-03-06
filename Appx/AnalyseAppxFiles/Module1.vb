﻿Imports System.Data.SqlClient
Imports System.IO
Imports System.IO.Compression
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports <xmlns:manifest8="http://schemas.microsoft.com/appx/2010/manifest">
Imports <xmlns:manifest10="http://schemas.microsoft.com/appx/manifest/foundation/windows10">
Imports <xmlns:build="http://schemas.microsoft.com/developer/appx/2012/build">
Imports <xmlns:mp="http://schemas.microsoft.com/appx/2014/phone/manifest">
Imports <xmlns:deployment7="http://schemas.microsoft.com/windowsphone/2009/deployment">
Imports <xmlns:deployment8="http://schemas.microsoft.com/windowsphone/2012/deployment">
Imports <xmlns:deployment81="http://schemas.microsoft.com/windowsphone/2014/deployment">


Module Module1

    ' AnalyzeAppxFiles
    ' ==================
    ' Curious about the apps installed on your machine?
    ' Have a bunch of appx/xap files which you've submitted to the store but you've lost track of what's in them?
    ' This code reads through such apps and extracts out app metadata, the assemblies it contains, and the types in them.
    ' It stores the results in a database...

    Dim Db As SqlConnection = InitDb($"{My.Computer.FileSystem.SpecialDirectories.Desktop}\AppDatabase.mdf")
    Dim AppPaths As String() = Nothing ' Search all apps installed on this machine. Or, you can provide a list of paths of appx files


    Sub Main()
        Console.WriteLine("Scanning...")
        Dim apps As AppInvestigator()

        ' We might be given a CSV that contains the directories/files to search, or we scan ourselves...
        If AppPaths IsNot Nothing AndAlso AppPaths.Count = 1 AndAlso Path.GetExtension(AppPaths(0)).ToLower = ".csv" Then
            Using reader As New StreamReader(AppPaths(0), Text.Encoding.UTF8), csv As New CsvHelper.CsvReader(reader)
                apps = csv.GetRecords(Of AppInvestigator).ToArray()
            End Using
        Else
            Dim paths = If(AppPaths, {"C:\Program Files\WindowsApps"})
            Dim appFiles = From path In paths
                           From file In Directory.EnumerateFiles(path, "*.*x*", SearchOption.AllDirectories)
                           Let ext = IO.Path.GetExtension(file).ToLowerInvariant()
                           Where ext = ".appx" OrElse ext = ".xap"
                           Select New AppInvestigator With {.Path = file}
            Dim appDirs = From path In paths
                          From dir In Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories)
                          Where File.Exists($"{dir}\AppxManifest.xml") OrElse File.Exists($"{dir}\WMAppManifest.xml")
                          Select New AppInvestigator With {.Path = dir}

            Try
                apps = appFiles.Concat(appDirs).ToArray()
            Catch ex As UnauthorizedAccessException
                Console.Error.WriteLine("Please re-run as administrator, to be able to read the 'C:\Program Files\WindowsApps' directory")
                Return
            End Try
        End If

        Console.WriteLine($"Found {apps.Length} apps")
        Dim sw = Stopwatch.StartNew()

        For i = 1 To apps.Length
            Dim app = apps(i - 1)
            Dim thisAppStartTime = sw.Elapsed

            Try
                App_EnterIntoDb(app)
            Catch ex As Exception
                Console.Error.WriteLine(($"Error In appx {Path.GetFileName(app.Path)} - {ex.Message}"))
            Finally
                app.Dispose()
            End Try

            Console.WriteLine(($"App {i} of {apps.Length} " &
                              $"took {(sw.Elapsed - thisAppStartTime).ToConciseString} " &
                              $"({i / apps.Length:0%} in {sw.Elapsed.ToConciseString} elapsed; " &
                              $"{TimeSpan.FromSeconds(sw.Elapsed.TotalSeconds / i * (apps.Length - i)).ToConciseString} remaining)..."))
        Next

        Db.Close()
    End Sub


    Sub App_EnterIntoDb(app As AppInvestigator)
        Dim appKey = -1
        Dim isNewApp = False

        Dim path = app.Path
        Using cmd = Sql($"SELECT TOP(1) AppKey FROM Apps WHERE Path={path}", Db)
            Dim r = cmd.ExecuteScalar()
            If r IsNot Nothing Then appKey = CInt(r)
        End Using
        If appKey = -1 Then
            Dim ai = app.GetAppInfo()
            If ai Is Nothing Then Return
            Using cmd = Sql($"INSERT INTO Apps(Name,Publisher,ProcessorArchitecture,Version,TargetPlatform,Path,DisplayName,PublisherDisplayName,AuthoringLanguage,IsTop,Rating,RatingCount,MediaType,Category)
                                          OUTPUT INSERTED.AppKey
                                          VALUES({ai.Name},{ai.Publisher},{ai.ProcessorArchitecture},{ai.Version},{ai.TargetPlatform},{ai.Path},{ai.DisplayName},{ai.PublisherDisplayName},{ai.AuthoringLanguage},{app.IsTop},{CType(app.Rating * 10, Integer?)},{app.RatingCount},{app.MediaType},{app.Category})", Db)
                appKey = CInt(cmd.ExecuteScalar())
                isNewApp = True
            End Using
        End If

        If Not isNewApp Then Return ' If the app already exists in the DB, we won't bother getting its metadata or files or types...

        For Each assemblyTuple In app.Assemblies
            Dim assemblyFn = assemblyTuple.Item1
            Dim assemblyName = assemblyTuple.Item2
            Dim assemblySize = New FileInfo(assemblyFn).Length
            Dim assemblyTitle = "", assemblyTargetFramework = "", assemblyTargetFrameworkDisplayName = "", assemblyFlags = ""

            Dim fileKey = -1
            Dim isNewFile = False
            Dim stream As FileStream = Nothing
            Dim pereader As Reflection.PortableExecutable.PEReader = Nothing
            Try

                Using cmd = Sql($"SELECT TOP(1) FileKey FROM Files WHERE Name={assemblyName} AND Size={assemblySize}", Db)
                    cmd.Parameters.AddWithValue("assemblyName", assemblyName)
                    cmd.Parameters.AddWithValue("assemblySize", assemblySize)
                    Dim r = cmd.ExecuteScalar()
                    If r IsNot Nothing Then
                        fileKey = CInt(r)
                    Else
                        stream = New FileStream(assemblyFn, FileMode.Open)
                        pereader = New Reflection.PortableExecutable.PEReader(stream)
                        TryGetAssemblyTargets(pereader, assemblyTitle, assemblyTargetFramework, assemblyTargetFrameworkDisplayName, assemblyFlags)
                        Using cmd2 = Sql($"INSERT INTO Files(Name,Size,Title,TargetFramework,TargetFrameworkDisplayName,Flags)
                                           OUTPUT INSERTED.FileKey
                                         VALUES({assemblyName},{assemblySize},{assemblyTitle},{assemblyTargetFramework},{assemblyTargetFrameworkDisplayName},{assemblyFlags})", Db)
                            fileKey = CInt(cmd2.ExecuteScalar())
                            isNewFile = True
                        End Using
                    End If
                End Using

                Using cmd As New SqlCommand("SELECT TOP(1) * from XAppFiles WHERE AppKey=@appKey AND FileKey=@fileKey", Db)
                    cmd.Parameters.AddWithValue("appKey", appKey)
                    cmd.Parameters.AddWithValue("fileKey", fileKey)
                    Dim r = cmd.ExecuteScalar()
                    If r Is Nothing Then
                        cmd.CommandText = "INSERT INTO XAppFiles(AppKey,FileKey) VALUES(@appKey,@fileKey)"
                        cmd.ExecuteNonQuery()
                    End If
                End Using

                If Not isNewFile Then Return ' If the file already exists in the DB, short circuit...

                stream = If(stream, New FileStream(assemblyFn, FileMode.Open))
                pereader = If(pereader, New Reflection.PortableExecutable.PEReader(stream))
                If Not pereader.HasMetadata Then Continue For
                Dim assembly = Reflection.Metadata.PEReaderExtensions.GetMetadataReader(pereader)

                For Each referenceHandle In assembly.AssemblyReferences
                    Dim reference = assembly.GetAssemblyReference(referenceHandle)
                    Dim referenceNameHandle = reference.Name
                    Dim referenceName = assembly.GetString(referenceNameHandle)
                    Dim referenceVersion = reference.Version.ToString

                    Dim referenceKey = -1
                    Using cmd As New SqlCommand($"SELECT TOP(1) ReferenceKey FROM [References] WHERE Name=@referenceName AND Version=@referenceVersion", Db)
                        cmd.Parameters.AddWithValue("referenceName", referenceName)
                        cmd.Parameters.AddWithValue("referenceVersion", referenceVersion)
                        Dim r = cmd.ExecuteScalar()
                        If r IsNot Nothing Then
                            referenceKey = CInt(r)
                        Else
                            cmd.CommandText = "INSERT INTO [References](Name,Version) OUTPUT INSERTED.ReferenceKey VALUES(@referenceName,@referenceVersion)"
                            referenceKey = CInt(cmd.ExecuteScalar())
                        End If
                    End Using

                    Using cmd As New SqlCommand("SELECT TOP(1) * from XFileReferences WHERE FileKey=@fileKey AND ReferenceKey=@referenceKey", Db)
                        cmd.Parameters.AddWithValue("fileKey", fileKey)
                        cmd.Parameters.AddWithValue("referenceKey", referenceKey)
                        Dim r = cmd.ExecuteScalar()
                        If r Is Nothing Then
                            cmd.CommandText = "INSERT INTO XFileReferences(FileKey,ReferenceKey) VALUES(@fileKey,@referenceKey)"
                            cmd.ExecuteNonQuery()
                        End If
                    End Using
                Next


                Dim count = 0

                For Each typeDefHandle In assembly.TypeDefinitions
                    count += 1
                    Dim typeName = ""
                    Dim namespaceName = ""
                    Try
                        Dim typeDef = assembly.GetTypeDefinition(typeDefHandle)
                        Dim declaringType = typeDef.GetDeclaringType()
                        If Not declaringType.IsNil Then Continue For ' skip nested types
                        typeName = assembly.GetString(typeDef.Name)
                        If Not typeDef.Namespace.IsNil Then namespaceName = assembly.GetString(typeDef.Namespace)
                    Catch ex As Exception
                        Continue For
                    End Try
                    If typeName.StartsWith("<") OrElse namespaceName.StartsWith("<") Then Continue For

                    Dim namespaceKey = -1
                    Using cmd As New SqlCommand($"SELECT TOP(1) NamespaceKey FROM Namespaces WHERE Name=@namespaceName", Db)
                        cmd.Parameters.AddWithValue("namespaceName", namespaceName)
                        Dim r = cmd.ExecuteScalar()
                        If r IsNot Nothing Then
                            namespaceKey = CInt(r)
                        Else
                            cmd.CommandText = "INSERT INTO Namespaces(Name) OUTPUT INSERTED.NamespaceKey VALUES(@namespaceName)"
                            namespaceKey = CInt(cmd.ExecuteScalar())
                        End If
                    End Using


                    Dim typeKey = -1
                    Using cmd As New SqlCommand($"SELECT TOP(1) TypeKey FROM Types WHERE NamespaceKey=@namespaceKey AND Name=@typeName", Db)
                        cmd.Parameters.AddWithValue("NamespaceKey", namespaceKey)
                        cmd.Parameters.AddWithValue("typeName", typeName)
                        Dim r = cmd.ExecuteScalar()
                        If r IsNot Nothing Then
                            typeKey = CInt(r)
                        Else
                            cmd.CommandText = "INSERT INTO Types(NamespaceKey,Name) OUTPUT INSERTED.TypeKey VALUES(@namespaceKey,@typeName)"
                            typeKey = CInt(cmd.ExecuteScalar())
                        End If
                    End Using


                    Using cmd As New SqlCommand("Select TOP(1) * from XFileTypes WHERE FileKey=@fileKey AND TypeKey=@typeKey", Db)
                        cmd.Parameters.AddWithValue("fileKey", fileKey)
                        cmd.Parameters.AddWithValue("typeKey", typeKey)
                        Dim r = cmd.ExecuteScalar()
                        If r Is Nothing Then
                            cmd.CommandText = "INSERT INTO XFileTypes(FileKey,TypeKey) VALUES(@fileKey,@typeKey)"
                            cmd.ExecuteNonQuery()
                        End If
                    End Using
                Next typeDefHandle
            Finally
                pereader?.Dispose() : pereader = Nothing
                stream?.Dispose() : stream = Nothing
            End Try
        Next assemblyTuple
    End Sub




    Class AppInfo
        Public Name As String
        Public Publisher As String
        Public ProcessorArchitecture As String
        Public Version As String
        Public TargetPlatform As String
        '
        Public Path As String
        Public DisplayName As String
        Public PublisherDisplayName As String
        Public AuthoringLanguage As String
    End Class

    Class AppInvestigator : Implements IDisposable
        Public Property Rating As Double?
        Public Property RatingCount As Integer?
        Public Property MediaType As String
        Public Property Category As String
        Public Property IsTop As Boolean?
        Public Property Path As String

        Private _zip As ZipArchive
        Private _tfns As New Dictionary(Of String, String)
        Private _isAppx As Boolean?
        Private _ai As AppInfo

        Public Sub Dispose() Implements IDisposable.Dispose
            _zip?.Dispose() : _zip = Nothing
            For Each tfn In _tfns
                File.Delete(tfn.Value)
            Next
            _tfns.Clear()
            _isAppx = Nothing
            _ai = Nothing
        End Sub

        Private ReadOnly Property IsDir As Boolean
            Get
                Dim attr = File.GetAttributes(Path)
                Return attr.HasFlag(FileAttributes.Directory)
            End Get
        End Property

        Private ReadOnly Property Zip As ZipArchive
            Get
                If _zip IsNot Nothing Then Return _zip
                If IsDir Then Throw New Exception("Not a zipfile")
                _zip = ZipFile.OpenRead(Path)
                Return _zip
            End Get
        End Property

        Private ReadOnly Property IsAppx As Boolean
            Get
                If _isAppx.HasValue Then Return _isAppx.Value
                If IsDir Then
                    Dim f1 = GetFile("AppxManifest.xml")
                    If f1 IsNot Nothing Then _isAppx = True : Return _isAppx.Value
                    Dim f2 = GetFile("WMAppManifest.xml")
                    If f2 IsNot Nothing Then _isAppx = False : Return _isAppx.Value
                    Throw New Exception("This directory isn't appx or xap")
                Else
                    Dim ext = IO.Path.GetExtension(Path).ToLowerInvariant()
                    If ext = ".appx" Then _isAppx = True : Return _isAppx.Value
                    If ext = ".xap" Then _isAppx = False : Return _isAppx.Value
                    Throw New Exception("This file isn't appx or xap")
                End If
            End Get
        End Property

        Public Function GetAppInfo() As AppInfo
            If _ai IsNot Nothing Then Return _ai
            _ai = New AppInfo()
            _ai.Path = Path
            Dim xml = XDocument.Parse(File.ReadAllText(GetFile(If(IsAppx, "AppxManifest.xml", "WMAppManifest.xml"))))
            Dim _is10appx = IsAppx AndAlso xml.<manifest10:Package>.<manifest10:Identity>.FirstOrDefault IsNot Nothing
            Dim _isXap = Not IsAppx
            Dim _is8appx = IsAppx AndAlso Not _is10appx

            If _isXap Then
                Dim app = xml.<deployment8:Deployment>.<App>.FirstOrDefault, ver = "8"
                If app Is Nothing Then app = xml.<deployment7:Deployment>.<App>.FirstOrDefault : ver = "7"
                If app Is Nothing Then app = xml.<deployment81:Deployment>.<App>.FirstOrDefault : ver = "81"
                If app Is Nothing Then Return Nothing
                _ai.Name = app.@ProductID.TrimStart({"{"c}).TrimEnd({"}"c})
                _ai.Publisher = If(ver = "7", app.@Publisher, app.@PublisherID.TrimStart({"{"c}).TrimEnd({"}"c}))
                _ai.ProcessorArchitecture = "neutral"
                _ai.Version = app.@Version
                _ai.DisplayName = app.@Title
                _ai.PublisherDisplayName = app.@Publisher
                _ai.AuthoringLanguage = ".NET"
                _ai.TargetPlatform = $"Phone{ver}.{app.@RuntimeType}"

            ElseIf _is8appx Then
                Dim identity = xml.<manifest8:Package>.<manifest8:Identity>.Single
                Dim properties = xml.<manifest8:Package>.<manifest8:Properties>.Single
                Dim app = xml.<manifest8:Package>.<manifest8:Applications>.<manifest8:Application>.FirstOrDefault
                Dim phoneIdentity = xml.<manifest8:Package>.<mp:PhoneIdentity>.FirstOrDefault
                If app Is Nothing Then Return Nothing
                '
                _ai.Name = identity.@Name
                _ai.Publisher = identity.@Publisher
                _ai.Version = identity.@Version
                _ai.ProcessorArchitecture = If(identity.@ProcessorArchitecture, "neutral")
                _ai.DisplayName = properties.<manifest8:DisplayName>.Value
                _ai.PublisherDisplayName = properties.<manifest8:PublisherDisplayName>.Value
                '
                If app.@StartPage IsNot Nothing Then
                    _ai.AuthoringLanguage = "JS"
                Else
                    Dim clBuildItems = From item In xml.<manifest8:Package>.<build:Metadata>.<build:Item>
                                       Where item.@Name = "cl.exe"

                    _ai.AuthoringLanguage = If(clBuildItems.Any(), "C++", ".NET")
                End If
                '
                _ai.TargetPlatform = If(phoneIdentity Is Nothing, "Win8*.Appx", "Phone81.Appx")

            Else ' UWP APPX
                Dim identity = xml.<manifest10:Package>.<manifest10:Identity>.Single
                Dim properties = xml.<manifest10:Package>.<manifest10:Properties>.Single
                Dim app = xml.<manifest10:Package>.<manifest10:Applications>.<manifest10:Application>.FirstOrDefault
                Dim phoneIdentity = xml.<manifest10:Package>.<mp:PhoneIdentity>.FirstOrDefault
                Dim dependencies = xml.<manifest10:Package>.<manifest10:Dependencies>.FirstOrDefault
                If app Is Nothing Then Return Nothing
                '
                _ai.Name = identity.@Name
                _ai.Publisher = identity.@Publisher
                _ai.Version = identity.@Version
                _ai.ProcessorArchitecture = identity.@ProcessorArchitecture
                _ai.DisplayName = properties.<manifest10:DisplayName>.Value
                _ai.PublisherDisplayName = properties.<manifest10:PublisherDisplayName>.Value
                '
                If app.@StartPage IsNot Nothing Then
                    _ai.AuthoringLanguage = "JS"
                Else
                    Dim clBuildItems = From item In xml.<manifest8:Package>.<build:Metadata>.<build:Item>
                                       Where item.@Name = "cl.exe"
                    _ai.AuthoringLanguage = If(clBuildItems.Any(), "C++", ".NET")
                End If
                '
                _ai.TargetPlatform = dependencies.<manifest10:TargetDeviceFamily>.FirstOrDefault?.@Name.Replace("Windows.", "Win10.")
                If _ai.TargetPlatform Is Nothing Then _ai.TargetPlatform = dependencies.<manifest10:TargetPlatform>.FirstOrDefault?.@Name.Replace("Windows.", "Win10.")
                If _ai.TargetPlatform Is Nothing Then _ai.TargetPlatform = "Win10.Universal"
            End If

            If _isXap AndAlso _ai.DisplayName.StartsWith("@") Then
                Dim rr = _ai.DisplayName.Substring(1).Split({","c})
                Dim dllName = rr(0), dllIndex = CInt(rr(1))
                Dim dllFile = GetFile(dllName)
                If dllFile IsNot Nothing Then
                    Dim tmp = ""
                    If TryGetStringTableValue(dllFile, dllIndex, tmp) Then _ai.DisplayName = tmp
                End If
            ElseIf IsAppx AndAlso _ai.DisplayName.StartsWith("ms-resource:") Then
                Dim priFile = GetFile("resources.pri")
                If priFile IsNot Nothing Then
                    Dim priXml = DumpPri(priFile).GetAwaiter().GetResult()
                    Dim tmp = ""
                    If TryGetResourceValue(_ai.DisplayName, priXml, tmp) Then _ai.DisplayName = tmp
                End If
            End If
            Return _ai
        End Function

        Private Function GetFile(fn As String) As String
            If _tfns.ContainsKey(fn) Then Return _tfns(fn)
            Dim ext = IO.Path.GetExtension(fn)
            '
            If IsDir Then
                If Not File.Exists($"{Path}\{fn}") Then Return Nothing
                Dim tfn = $"{IO.Path.GetTempPath}{Guid.NewGuid}{ext}"
                File.Copy($"{Path}\{fn}", tfn, True)
                _tfns(fn) = tfn
                Return tfn
            Else
                Dim ze = Zip.GetEntry(fn)
                If ze Is Nothing Then Return Nothing
                Dim tfn = $"{IO.Path.GetTempPath}{Guid.NewGuid}{ext}"
                ze.ExtractToFile(tfn, True)
                _tfns(fn) = tfn
                Return tfn
            End If
        End Function

        Public Iterator Function Assemblies() As IEnumerable(Of Tuple(Of String, String))
            If IsDir Then
                For Each fn In Directory.GetFiles(Path, "*.*", SearchOption.AllDirectories)
                    Dim ext = IO.Path.GetExtension(fn).ToLower()
                    If ext <> ".exe" AndAlso ext <> ".dll" AndAlso ext <> ".winmd" Then Continue For
                    If Not _tfns.ContainsKey(fn) Then
                        Dim tfn = IO.Path.GetTempFileName()
                        File.Copy(fn, tfn, True)
                        _tfns(fn) = tfn
                    End If
                    Dim assemblyName = IO.Path.GetFileName(fn)
                    Yield Tuple.Create(_tfns(fn), assemblyName)
                Next
            Else
                For Each ze In Zip.Entries
                    Dim ext = IO.Path.GetExtension(ze.FullName).ToLower()
                    If ext <> ".exe" AndAlso ext <> ".dll" AndAlso ext <> ".winmd" Then Continue For
                    If Not _tfns.ContainsKey(ze.FullName) Then
                        Dim tfn = IO.Path.GetTempFileName
                        ze.ExtractToFile(tfn, True)
                        _tfns(ze.FullName) = tfn
                    End If
                    Dim assemblyName = IO.Path.GetFileName(ze.FullName).UnescapePercentUtf8()
                    If assemblyName.Contains("\") OrElse assemblyName.Contains("/") Then Stop
                    Yield Tuple.Create(_tfns(ze.FullName), assemblyName)
                Next
            End If
        End Function
    End Class




    Function InitDb(DbPath As String) As SqlConnection
        Dim dbName = Path.GetFileNameWithoutExtension(DbPath)
        If Not File.Exists(DbPath) Then
            Dim conn0 = "Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;"
            Using db0 As New SqlConnection(conn0)
                db0.Open()
                Using cmd = db0.CreateCommand()
                    cmd.CommandText = $"CREATE DATABASE {dbName} ON PRIMARY (NAME={dbName}, FILENAME ='{DbPath}')"
                    cmd.ExecuteNonQuery()
                    cmd.CommandText = $"EXEC sp_detach_db '{dbName}', 'true'"
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End If

        Dim conn = $"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={DbPath};Integrated Security=True"
        Dim db As New SqlConnection(conn)
        db.Open()

        Using cmd = db.CreateCommand()
            For Each batch In File.ReadAllText("CreateTables.sql").Split({"GO"}, StringSplitOptions.RemoveEmptyEntries)
                cmd.CommandText = batch
                cmd.ExecuteNonQuery()
            Next
        End Using

        Return db
    End Function

    Function Sql(s As FormattableString, connection As SqlConnection) As SqlCommand
        Dim argNames = Enumerable.Range(0, s.ArgumentCount).Select(Function(i) CObj($"@arg{i}")).ToArray()
        Dim cmd As New SqlCommand()
        cmd.Connection = connection
        cmd.CommandText = String.Format(s.Format, argNames).Replace(vbCrLf, " ").Replace("    ", " ").Replace("  ", " ")
        For i = 0 To s.ArgumentCount - 1
            cmd.Parameters.AddWithValue($"@arg{i}", If(s.GetArgument(i), DBNull.Value))
        Next
        Return cmd
    End Function

    <Extension>
    Function ToConciseString(t As TimeSpan) As String
        If t.TotalSeconds < 1 Then Return $"{t.TotalMilliseconds:0}ms"
        If t.TotalSeconds < 10 Then Return $"{t.TotalSeconds:0.0}s"
        If t.TotalMinutes <= 1 Then Return $"{t.TotalSeconds:0}s"
        If t.TotalMinutes < 60 Then
            Dim m = Math.Floor(t.TotalMinutes)
            Dim s = (t.TotalMinutes - m) * 60
            Return $"{m:0}m{s:0}s"
        End If
        If t.TotalHours < 24 Then
            Dim h = Math.Floor(t.TotalHours)
            Dim m = (t.TotalHours - h) * 60
            Return $"{h:0}h{m:0}m"
        Else
            Dim d = Math.Floor(t.TotalDays)
            Dim h0 = (t.TotalDays - d) * 24
            Dim h = Math.Floor(h0)
            Dim m = (h0 - h) * 60
            Return $"{d:0}d{h:0}h{m:0}m"
        End If
    End Function

    <Extension>
    Function UnescapePercentUtf8(s As String) As String
        If Not s.Contains("%") Then Return s
        Dim buf As New List(Of Byte)
        For i = 0 To s.Length - 1
            If s(i) = "%" Then
                If i + 2 >= s.Length Then Throw New ArgumentException(NameOf(s))
                buf.Add(CByte(Integer.Parse(s(i + 1) & s(i + 2), Globalization.NumberStyles.HexNumber)))
                i += 2
            Else
                buf.Add(CByte(AscW(s(i))))
            End If
        Next
        Return Text.Encoding.UTF8.GetString(buf.ToArray)
    End Function


    Async Function DumpPri(src As String) As Task(Of XDocument)
        Dim tfn = $"{Path.GetTempPath}{Guid.NewGuid}.pri.xml"
        Dim p As New Process
        Try
            Dim cmd = "makepri.exe"
            Dim args = $"dump /if ""{src}"" /of ""{tfn}"""
            p.StartInfo.FileName = cmd
            p.StartInfo.Arguments = args
            p.StartInfo.UseShellExecute = False
            p.StartInfo.RedirectStandardInput = False
            p.StartInfo.RedirectStandardOutput = True
            p.StartInfo.RedirectStandardError = True
            p.Start()

            Dim outputTask = p.StandardOutput.ReadToEndAsync
            Dim errTask = p.StandardError.ReadToEndAsync
            Dim output = (Await outputTask).Replace(vbNullChar, "")
            Dim err = Await errTask
            Await Task.Run(Sub() p.WaitForExit(5000))
            If Not p.HasExited Then
                p.Kill()
                Await Task.Run(Sub() p.WaitForExit(2000))
                Return Nothing
            End If
            If output.Contains("MakePri: error") OrElse err.Contains("MakePri: error") Then Throw New Exception("MakePri error")

            Using stream As New IO.StreamReader(tfn)
                Dim txt = Await stream.ReadToEndAsync()
                Return XDocument.Parse(txt)
            End Using
        Finally
            p.Dispose()
            File.Delete(tfn)
        End Try
    End Function


    Function TryGetResourceValue(id As String, xml As XDocument, ByRef result As String) As Boolean
        ' ms-resource://ResourceMap/ResourceSubtree/.../NamedResource
        ' ms-resource:///ResourceSubtree/.../NamedResource -- implicitly use the ResourceMap of the current package
        ' ms-resource:/ResourceSubtree/.../NamedResource -- again, implicitly use the ResourceMap of the current package
        ' ms-resource:NamedResource -- implicitly use ResourceMap of current package, and look in ResourceSubtree named "Resources"
        If Not id.StartsWith("ms-resource:") Then Throw New ArgumentException(NameOf(id))
        Dim key = id.Substring(12)
        If key.Length = 0 Then Throw New ArgumentException(NameOf(id))
        If key(0) <> "/"c Then key = "///Resources/" & key
        If key(0) = "/"c AndAlso key(1) <> "/"c Then key = "//" & key
        If Not key.StartsWith("//") Then Throw New Exception("oops, we should at least start with // by now")
        Dim keys = New LinkedList(Of String)(key.Substring(2).Split({"/"c}))

        ' ResourceMap (we won't bother checking name)
        Dim x = xml.<PriInfo>.<ResourceMap>
        keys.RemoveFirst()

        ' ResourceSubtrees
        While keys.Count > 1
            Dim k = keys.First.Value.ToLowerInvariant()
            x = x.<ResourceMapSubtree>.Where(Function(r) r.@name ?.ToLowerInvariant() = k)
            keys.RemoveFirst()
        End While

        ' NamedResource
        If keys.Count = 0 Then Throw New ArgumentException(NameOf(id))
        Dim x1 = x.<NamedResource>.Where(Function(r) r.@name = keys.First.Value).ToArray()
        If x1.Count = 0 Then x1 = x.<NamedResource>.Where(Function(r) r.@name.ToLowerInvariant() = keys.First.Value.ToLowerInvariant()).ToArray()
        If x1.Count = 0 Then Return False
        x = x1

        ' Candidates
        Dim values As New Dictionary(Of String, String)
        For Each c In x.<Candidate>
            Dim lang = If(c.@qualifiers ?.ToLowerInvariant.Replace("language-", ""), "en-us")
            Dim val = c.<Value>.Value
            If Not values.ContainsKey(lang) Then values.Add(lang, val)
        Next
        If values.TryGetValue("en-us", result) Then Return True
        Dim enKey = values.Keys.Where(Function(k) k.StartsWith("en")).FirstOrDefault
        If enKey IsNot Nothing AndAlso values.TryGetValue(enKey, result) Then Return True
        If values.Count > 0 Then result = values.Values(0) : Return True
        Return False
    End Function

    Function TryGetStringTableValue(fn As String, id As Integer, ByRef result As String) As Boolean
        Dim hInstance = LoadLibrary(fn) : If hInstance = IntPtr.Zero Then Stop : Return False
        Dim sb As New Text.StringBuilder(1024)
        Dim i = LoadString(hInstance, CUInt(-id), sb, sb.Capacity + 1)
        If i = 0 Then Return False
        FreeLibrary(hInstance)
        result = sb.ToString()
        Return True
    End Function


    Sub Main_NameToPath()
        ' If the input file is a .csv where the "Path" column actually contains PackageFamilyName
        ' instead of Path, this routine looks up in the database for the Path that matches
        ' this PFN, and writes it
        Dim srcFn = AppPaths(0)
        Dim dstFn = $"{Path.GetDirectoryName(srcFn)}\{Path.GetFileNameWithoutExtension(srcFn)}_out{Path.GetExtension(srcFn)}"
        Dim apps As AppInvestigator()

        Console.WriteLine($"Reading {Path.GetFileName(srcFn)}")
        Using srcStream As New StreamReader(srcFn, Text.Encoding.UTF8),
            srcCsv As New CsvHelper.CsvReader(srcStream)
            apps = srcCsv.GetRecords(Of AppInvestigator).ToArray()
        End Using

        Console.WriteLine("Processing...")
        For i = 0 To apps.Length - 1
            Dim app = apps(i)
            Dim name = app.Path
            Using cmd = Sql($"SELECT TOP(1) Path FROM Apps WHERE Name={name}", Db)
                Dim r = cmd.ExecuteScalar()
                Dim path = If(r Is Nothing OrElse r Is DBNull.Value, Nothing, CStr(r))
                app.Path = path
            End Using
            If (i Mod 100) = 99 Then Console.WriteLine($"... {i + 1}/{apps.Length}")
        Next

        Console.WriteLine($"Writing {Path.GetFileName(dstFn)}")
        Using dstStream As New StreamWriter(dstFn, False, Text.Encoding.UTF8),
            dstCsv As New CsvHelper.CsvWriter(dstStream)
            dstCsv.WriteRecords(apps)
        End Using
    End Sub

    Function TryGetAssemblyTargets(pereader As Reflection.PortableExecutable.PEReader, ByRef title As String, ByRef targetFramework As String, ByRef targetFrameworkDisplayName As String, ByRef flags As String) As Boolean
        title = Nothing : targetFramework = Nothing : targetFrameworkDisplayName = Nothing : flags = Nothing
        If Not pereader.HasMetadata Then Return False
        Dim assembly = Reflection.Metadata.PEReaderExtensions.GetMetadataReader(pereader)

        For Each mrAttrHandle In assembly.CustomAttributes
            flags = $"{pereader.PEHeaders.CorHeader?.Flags}"
            Dim mrAttr = assembly.GetCustomAttribute(mrAttrHandle)
            Dim entityHandle = mrAttr.Constructor
            If entityHandle.Kind <> Reflection.Metadata.HandleKind.MemberReference Then Continue For
            Dim ctor = assembly.GetMemberReference(CType(entityHandle, Reflection.Metadata.MemberReferenceHandle))
            Dim ctorParentHandle = ctor.Parent
            If ctorParentHandle.Kind <> Reflection.Metadata.HandleKind.TypeReference Then Continue For
            Dim parent = assembly.GetTypeReference(CType(ctorParentHandle, Reflection.Metadata.TypeReferenceHandle))
            Dim parentName = $"{assembly.GetString(parent.Namespace)}.{assembly.GetString(parent.Name)}"
            If parentName = "System.Reflection.AssemblyTitleAttribute" Then
                Dim reader = assembly.GetBlobReader(mrAttr.Value)
                If reader.ReadUInt16() <> 1 Then Stop : Continue For ' oops! prolog
                Dim len = reader.ReadByte()
                title = If(len = 255, Nothing, reader.ReadUTF8(len))
            ElseIf parentName = "System.Runtime.Versioning.TargetFrameworkAttribute" Then
                Dim reader = assembly.GetBlobReader(mrAttr.Value)
                If reader.ReadUInt16() <> 1 Then Stop : Continue For ' oops! prolog
                Dim len = reader.ReadByte()
                targetFramework = If(len = 255, "", reader.ReadUTF8(len))
                Dim numNamed = reader.ReadUInt16()
                For i = 1 To numNamed
                    Dim propKind = reader.ReadByte()
                    Dim propType = reader.ReadByte()
                    If propType = &H1D OrElse propType = &H55 Then Stop : Throw New NotImplementedException("unimplemented types for named arguments")
                    len = reader.ReadByte()
                    Dim propName = If(len = 255, Nothing, reader.ReadUTF8(len))
                    len = reader.ReadByte()
                    Dim propValue = If(len = 255, Nothing, reader.ReadUTF8(len))
                    If propName = "FrameworkDisplayName" Then targetFrameworkDisplayName = propValue
                Next
            End If
        Next
        Return True
    End Function


    Declare Unicode Function LoadLibrary Lib "kernel32" Alias "LoadLibraryW" (fn As String) As IntPtr
    Declare Unicode Function LoadString Lib "user32" Alias "LoadStringW" (hInstance As IntPtr, uID As UInt32, buf As Text.StringBuilder, nBufMax As Integer) As Integer
    Declare Unicode Function FreeLibrary Lib "kernel32" (hInstance As IntPtr) As IntPtr
End Module
