Imports System.Data.SqlClient
Imports System.IO
Imports System.IO.Compression
Imports System.Runtime.CompilerServices


Module Module1

    ' AnalyzeNugets
    ' ==================
    ' Curious about the nugets installed on your machine?
    ' This code reads through the NuGetV3 package cache in c:\users\<name>\.nuget\packages
    ' and extracts out some of the metadata.
    ' It stores the results in a database...

    Dim Db As SqlConnection = InitDb($"{My.Computer.FileSystem.SpecialDirectories.Desktop}\NugetDatabase2.mdf")
    Dim NugetPath As String = Nothing ' Search all nugets in user cache. Or you can search a given directory


    Sub Main()
        NugetPath = If(NugetPath, Environment.ExpandEnvironmentVariables("%USERPROFILE%\.nuget\packages"))

        Dim downloadCounts As New Dictionary(Of String, Integer)
        Dim catalogPath = $"{NugetPath}\_nugetCatalog.xml"
        If File.Exists(catalogPath) Then
            Console.WriteLine("Reading catalog...")
            Dim catalog = XDocument.Parse(File.ReadAllText(catalogPath))
            For Each package In catalog.<packages>.<package>
                Dim count = 0 : downloadCounts.TryGetValue(package.@id, count)
                count += CInt(package.@downloadCount)
                downloadCounts(package.@id) = count
            Next
        End If

        Console.WriteLine("Scanning...")
        Dim nugets = Directory.EnumerateFiles(NugetPath, "*.nupkg", SearchOption.AllDirectories).ToArray
        Console.WriteLine($"Found {nugets.Length} nugets")

        Dim sw = Stopwatch.StartNew()
        For i = 1 To nugets.Length
            Dim nuget = nugets(i - 1)
            Dim thisStartTime = sw.Elapsed

            Try
                Nuget_EnterIntoDb(nuget, downloadCounts)
            Catch ex As Exception
                Console.Error.WriteLine(($"Error In nuget {Path.GetFileName(nuget)} - {ex.Message}"))
            End Try

            Console.WriteLine(($"Nuget {i} of {nugets.Length} " &
                              $"took {(sw.Elapsed - thisStartTime).ToConciseString} " &
                              $"({i / nugets.Length:0%} in {sw.Elapsed.ToConciseString} elapsed; " &
                              $"{TimeSpan.FromSeconds(sw.Elapsed.TotalSeconds / i * (nugets.Length - i)).ToConciseString} remaining)..."))
        Next

        Db.Close()
    End Sub


    Sub Nuget_EnterIntoDb(path As String, downloadCounts As Dictionary(Of String, Integer))

        Dim isDir = File.GetAttributes(path).HasFlag(FileAttributes.Directory)
        Dim zip = If(isDir, Nothing, ZipFile.OpenRead(path))
        Dim files = If(isDir, Directory.GetFiles(path, "*", SearchOption.AllDirectories),
                        zip.Entries.Select(Function(ze) ze.FullName)).ToList
        If Not files.Any(Function(f)
                             Dim ext = IO.Path.GetExtension(f).ToLower
                             Return ext = ".cs" OrElse ext = ".vb" OrElse ext = ".dll" OrElse ext = ".winmd"
                         End Function) Then Return
        If files.Any(Function(f) IO.Path.GetExtension(f).ToLower = ".js") Then Return

        Dim nuspecFn = files.Single(Function(f) IO.Path.GetDirectoryName(f) = "" AndAlso IO.Path.GetExtension(f).ToLower = ".nuspec")
        Dim nuspecText = ""
        If isDir Then
            nuspecText = File.ReadAllText($"{path}\{nuspecFn}")
        Else
            Dim ze = zip.GetEntry(nuspecFn)
            Using reader As New StreamReader(ze.Open())
                nuspecText = reader.ReadToEnd()
            End Using
        End If
        Dim nuspec = XDocument.Parse(nuspecText)
        nuspec.StripNamespaces()


        Dim metadata = nuspec.<package>.<metadata>
        Dim id = metadata.<id>.Value
        Dim version = metadata.<version>.Value
        Dim title = metadata.<title>.Value
        Dim authors = metadata.<authors>.Value
        Dim i = 0
        Dim downloadCount = If(downloadCounts?.TryGetValue(id, i), i, CType(Nothing, Integer?))


        Using cmd = Sql($"MERGE Nugets WITH (HOLDLOCK) AS n
                          USING (SELECT {id} AS ID, {version} AS Version, {title} AS Title, {authors} AS Authors, {path} AS Path, {downloadCount} AS DownloadCount) AS new_row ON n.ID = new_row.ID
                          WHEN MATCHED THEN UPDATE SET n.Version=new_row.Version, n.Title=new_row.Title, n.Authors=new_row.Authors, n.Path=new_row.Path, n.DownloadCount=new_row.DownloadCount
                          WHEN NOT MATCHED THEN INSERT (ID,Version,Title,Authors,Path,DownloadCount) VALUES (new_row.ID,new_row.Version,new_row.Title,new_row.Authors,new_row.Path,new_row.DownloadCount);", Db)
            cmd.ExecuteNonQuery()
        End Using

        For Each reference In metadata.<references>.<reference>
            Dim file = reference.@file
            Using cmd = Sql($"MERGE [References] WITH (HOLDLOCK) AS r
                          USING (SELECT {id} AS ID, {file} AS [File]) AS new_row ON r.ID=new_row.ID AND r.[File]=new_row.[File]
                          WHEN NOT MATCHED THEN INSERT (ID,[File]) VALUES (new_row.ID,new_row.[File]);", Db)
                cmd.ExecuteNonQuery()
            End Using
        Next


        For Each frameworkAssembly In metadata.<frameworkAssemblies>.<frameworkAssembly>
            Dim assemblyName = frameworkAssembly.@assemblyName
            Dim targetFramework = If(frameworkAssembly.@targetFramework, "")
            Using cmd = Sql($"MERGE FrameworkAssemblies WITH (HOLDLOCK) AS f
                          USING (SELECT {id} AS ID, {assemblyName} AS AssemblyName, {targetFramework} AS TargetFramework) AS new_row ON f.ID=new_row.ID AND f.AssemblyName=new_row.AssemblyName AND f.TargetFramework=new_row.TargetFramework
                          WHEN NOT MATCHED THEN INSERT (ID,AssemblyName,TargetFramework) VALUES (new_row.ID,new_row.AssemblyName,new_row.TargetFramework);", Db)
                cmd.ExecuteNonQuery()
            End Using
        Next


        Dim grouplessDependencies = From dependency In metadata.<dependencies>.<dependency>
                                    Let targetFramework As String = Nothing
                                    Select Tuple.Create(targetFramework, dependency.@id)
        Dim groupedDependencies = From group In metadata.<dependencies>.<group>
                                  Let targetFramework = group.@targetFramework
                                  From dependency In group.<dependency>
                                  Select Tuple.Create(targetFramework, dependency.@id)
        For Each dependencyTuple In grouplessDependencies.Concat(groupedDependencies)
            Dim targetFramework = If(dependencyTuple.Item1, "")
            Dim dependencyId = dependencyTuple.Item2

            Using cmd = Sql($"MERGE Nugets WITH (HOLDLOCK) AS n
                          USING (SELECT {dependencyId} AS ID) AS new_row ON n.ID = new_row.ID
                          WHEN NOT MATCHED THEN INSERT (ID) VALUES (new_row.ID);", Db)
                cmd.ExecuteNonQuery()
            End Using

            Using cmd = Sql($"MERGE Dependencies WITH (HOLDLOCK) AS d
                          USING (SELECT {id} AS ID, {targetFramework} AS TargetFramework, {dependencyId} AS DependencyID) AS new_row ON d.ID=new_row.ID AND d.TargetFramework=new_row.TargetFramework AND d.DependencyID=new_row.DependencyID
                          WHEN NOT MATCHED THEN INSERT (ID,TargetFramework,DependencyID) VALUES (new_row.ID,new_row.TargetFramework,new_row.DependencyID);", Db)
                cmd.ExecuteNonQuery()
            End Using
        Next


        For Each file In files
            Dim fn = file.UnescapePercentUtf8
            Dim directory = IO.Path.GetDirectoryName(fn)
            Dim name = IO.Path.GetFileName(fn)
            Dim extension = IO.Path.GetExtension(fn).ToLower
            Using cmd = Sql($"MERGE Files WITH (HOLDLOCK) AS f
                          USING (SELECT {id} AS ID, {directory} AS Directory, {name} AS Name, {extension} as Extension) AS new_row ON f.ID=new_row.ID AND f.Directory=new_row.Directory AND f.Name=new_row.Name
                          WHEN NOT MATCHED THEN INSERT (ID,Directory,Name,Extension) VALUES (new_row.ID,new_row.Directory,new_row.Name,new_row.Extension);", Db)
                cmd.ExecuteNonQuery()
            End Using
        Next
    End Sub




    Function InitDb(DbPath As String) As SqlConnection
        Console.WriteLine("Connecting to DB...")
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
            For Each batch In File.ReadAllText("CreateNugetTables.sql").Split({"GO"}, StringSplitOptions.RemoveEmptyEntries)
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

    <Extension>
    Sub StripNamespaces(xml As XDocument)
        For Each e In xml.Descendants
            e.Name = e.Name.LocalName
            e.ReplaceAttributes(From a In e.Attributes
                                Where Not a.IsNamespaceDeclaration
                                Select New XAttribute(a.Name.LocalName, a.Value))
        Next
    End Sub

End Module
