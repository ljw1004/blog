Imports System.Data.SqlClient
Imports System.IO
Imports System.IO.Compression
Imports <xmlns:manifest="http://schemas.microsoft.com/appx/2010/manifest">
Imports <xmlns:build="http://schemas.microsoft.com/developer/appx/2012/build">
Imports <xmlns:manifest10="http://schemas.microsoft.com/appx/manifest/foundation/windows10">

Module Module1

    ' AnalyzeAppxFiles
    ' ==================
    ' So you have a bunch of appx files for all the projects you've submitted to the store,
    ' but you've begun to lose track of them? What they all are, what they do?
    ' Don't know which of them use a particular library that might need updating?
    ' This code reads through them and extracts out filenames, types and names,
    ' and stores them in a database that it creates.
    ' Hopefully this will help you get your numerous appx files under control.

    Dim Db As SqlConnection = InitDb("C:\Users\lwischik\Desktop\AppxDatabase.mdf")
    Dim AppxPaths As String() = Nothing ' Search all apps installed on this machine. Or, you can provide a list of paths of appx files
    Dim AppxMax As Integer? = Nothing   ' Process every appx. Or, you can limit it

    ' Example: to find all of my appxs that use Callisto (i.e. declare the type Callisto.Callisto_XamlTypeInfo.Getter)
    ' and get a list of all the assembly-names that this appx contains:
    Dim example1 As String = "
Select Case A.DisplayName, F.Name
FROM Appxs A, Files F, Types T, XAppxTypes AT, XAppxFiles AF
WHERE T.Name = 'Callisto.Callisto_XamlTypeInfo.Getter'
AND AT.AppxKey = A.AppxKey AND AT.TypeKey = T.TypeKey
AND AF.AppxKey = A.AppxKey AND AF.FileKey = F.FileKey
"

    ' Example: to find all of my appxs that use sqlite-net (i.e. declare the type SQLite.NotNullConstraintViolationException)
    ' as source code rather than the sqlite.net-pcl package (i.e. don't contain the file SQLite.Net.dll):
    Dim example2 As String = "
SELECT A.DisplayName
FROM Appxs A, Types T, XAppxTypes AT
WHERE T.Name = 'SQLite.NotNullConstraintViolationException'
AND (SELECT FileKey FROM Files WHERE Name='SQLite.Net.dll') NOT IN (SELECT FileKey FROM XAppxFiles WHERE AppxKey=A.AppxKey)
AND AT.AppxKey = A.AppxKey And AT.TypeKey = T.TypeKey
"

    ' Example: count how many appxs have been surveyed and entered into this database
    Dim example3 As String = "
SELECT COUNT(*) FROM Appxs
"

    Sub Main()
        Console.WriteLine("Scanning...")
        Dim appxFns As String()
        If AppxPaths Is Nothing Then
            Try
                appxFns = (From d In Directory.EnumerateDirectories("C:\Program Files\WindowsApps")
                           Let m = $"{d}\AppxManifest.xml"
                           Where File.Exists(m)
                           Select m).ToArray()
            Catch ex As UnauthorizedAccessException
                Console.Error.WriteLine("Please re-run as administrator, to be able to read the 'C:\Program Files\WindowsApps' directory")
                Return
            End Try
        Else
            appxFns = (From path In AppxPaths
                       From file In Directory.EnumerateFiles(path, "*.appx", SearchOption.AllDirectories)
                       Select file).ToArray
        End If
        Console.WriteLine($"Found {appxFns.Count} appxs")

        Dim EffectiveAppxMax = If(AppxMax.HasValue, Math.Min(AppxMax.Value, appxFns.Length), appxFns.Length)
        Dim RND As New Random
        Dim closeActions As New List(Of Action)

        Dim sw As New Stopwatch
        Dim cumulativeCount = 0
        Dim cumulativeTime As New TimeSpan

        Dim pickAppxFns As Func(Of IEnumerable(Of String))
        If EffectiveAppxMax > appxFns.Length \ 4 Then
            pickAppxFns = Iterator Function()
                              Dim prob = EffectiveAppxMax / appxFns.Length
                              For i = 0 To appxFns.Length - 1
                                  If RND.NextDouble() <= prob Then Yield appxFns(i)
                              Next
                          End Function
        Else
            pickAppxFns = Iterator Function()
                              For i = 0 To EffectiveAppxMax - 1
                                  Yield appxFns(RND.Next(appxFns.Length))
                              Next
                          End Function
        End If

        Dim count = 0
        For Each appxFn In pickAppxFns()
            count += 1

            sw.Restart()
            Dim countThisForTimekeeping = False
            Try
                countThisForTimekeeping = Appx_EnterIntoDb(appxFn)
            Catch ex As Exception
                Console.Error.WriteLine(($"Error in appx {Path.GetFileName(appxFn)} - {ex.Message}"))
            End Try

            If countThisForTimekeeping Then
                cumulativeTime += sw.Elapsed
                cumulativeCount += 1
            End If
            Dim remaining = ""
            If cumulativeCount > 0 Then
                Dim remainingTime = New TimeSpan(CLng(cumulativeTime.Ticks / cumulativeCount * (EffectiveAppxMax - count)))
                remaining = $"; {remainingTime:%h\hmm\mss\s} remaining"
            End If

            Console.WriteLine(($"Appx {count} of {EffectiveAppxMax} ({count / EffectiveAppxMax:P2}, {sw.Elapsed:%s\.ff\s}, {cumulativeTime:%h\hmm\mss\s} elapsed{remaining})..."))

        Next

        Db.Close()
    End Sub


    Function Appx_EnterIntoDb(appxFn As String) As Boolean
        Dim appxName As String
        Dim isWindowsStore = False
        Dim isPhoneStore = False
        '
        Dim GetAppxManifestFile As Func(Of String) = Nothing
        Dim EnumerateAssemblies As Func(Of IEnumerable(Of Tuple(Of String, String))) = Nothing
        Dim closeActions As New List(Of Action)
        '
        If Path.GetFileName(appxFn).ToLower() = "appxmanifest.xml" Then
            Dim dir = Path.GetDirectoryName(appxFn)
            appxName = Path.GetFileName(dir)
            isWindowsStore = True
            GetAppxManifestFile = Function() appxFn
            EnumerateAssemblies = Iterator Function()
                                      For Each fn In Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories)
                                          Dim ext = Path.GetExtension(fn).ToLower()
                                          If ext <> ".exe" AndAlso ext <> ".dll" AndAlso ext <> ".winmd" Then Continue For
                                          Dim tfn = Path.GetTempFileName()
                                          File.Copy(fn, tfn, True)
                                          closeActions.Add(Sub() File.Delete(tfn))
                                          Yield Tuple.Create(tfn, Path.GetFileName(fn))
                                      Next
                                  End Function
        Else
            appxName = Path.GetFileNameWithoutExtension(appxFn)
            isWindowsStore = (appxFn Like "*\windowsstore\*")
            isPhoneStore = (appxFn Like "*\phone\*")
            Dim zip = ZipFile.OpenRead(appxFn)
            closeActions.Add(Sub() zip.Dispose())
            GetAppxManifestFile = Function()
                                      Dim zipEntry = zip.GetEntry("AppxManifest.xml")
                                      Dim tfn = Path.GetTempFileName()
                                      zipEntry.ExtractToFile(tfn, True)
                                      closeActions.Add(Sub() File.Delete(tfn))
                                      Return tfn
                                  End Function
            EnumerateAssemblies = Iterator Function()
                                      For Each zipEntry In zip.Entries
                                          Dim ext = Path.GetExtension(zipEntry.FullName).ToLower()
                                          If ext <> ".exe" AndAlso ext <> ".dll" AndAlso ext <> ".winmd" Then Continue For
                                          Dim assemblyName = Path.GetFileName(zipEntry.Name).Replace("%20", " ")
                                          Dim tfn = Path.GetTempFileName()
                                          zipEntry.ExtractToFile(tfn, True)
                                          Yield Tuple.Create(tfn, assemblyName)
                                          closeActions.Add(Sub() File.Delete(tfn))
                                      Next
                                  End Function
        End If




        Try
            Dim appxKey = -1
            Using cmd As New SqlCommand($"SELECT TOP(1) AppxKey FROM Appxs WHERE AppxFileName=@appxFileName", Db)
                cmd.Parameters.AddWithValue("appxFileName", appxName)
                Dim r = cmd.ExecuteScalar()
                If r IsNot Nothing Then appxKey = CInt(r)
            End Using

            If appxKey <> -1 Then Return False

            If appxKey = -1 Then
                Dim manifestFn = GetAppxManifestFile()
                Dim xml = XDocument.Parse(File.ReadAllText(manifestFn))
                Dim identity = xml.<manifest:Package>.<manifest:Identity>.@Name
                Dim publisher = xml.<manifest:Package>.<manifest:Identity>.@Publisher
                Dim processorArchitecture = xml.<manifest:Package>.<manifest:Identity>.@ProcessorArchitecture
                Dim displayName = xml.<manifest:Package>.<manifest:Properties>.<manifest:DisplayName>.Value
                Dim publisherDisplayName = xml.<manifest:Package>.<manifest:Properties>.<manifest:PublisherDisplayName>.Value
                Dim app = xml.<manifest:Package>.<manifest:Applications>.<manifest:Application>.FirstOrDefault
                If app Is Nothing Then Return True
                Dim language = "?"
                If app.@StartPage IsNot Nothing Then
                    language = "JS"
                Else
                    Dim buildItems = xml.<manifest:Package>.<build:Metadata>.<build:Item>
                    Dim clBuildItem = (From item In buildItems
                                       Where item.@Name = "cl.exe").FirstOrDefault
                    language = If(clBuildItem Is Nothing, ".NET", "C++")
                End If
                If identity Is Nothing Then Return False ' probably was a win10 appxmanifest; we don't handle those yet
                If processorArchitecture Is Nothing Then processorArchitecture = "neutral"
                Using cmd As New SqlCommand($"INSERT INTO Appxs(AppxFileName,[Identity],Publisher,DisplayName,PublisherDisplayName,ProcessorArchitecture,IsWindowsStore,IsPhoneStore,Language)
                                          OUTPUT INSERTED.AppxKey
                                          VALUES(@appxFileName,@identity,@publisher,@displayName,@publisherDisplayName,@processorArchitecture,@isWindowsStore,@isPhoneStore,@language)", Db)
                    cmd.Parameters.AddWithValue("appxFileName", appxName)
                    cmd.Parameters.AddWithValue("identity", identity)
                    cmd.Parameters.AddWithValue("publisher", publisher)
                    cmd.Parameters.AddWithValue("displayName", displayName)
                    cmd.Parameters.AddWithValue("publisherDisplayName", publisherDisplayName)
                    cmd.Parameters.AddWithValue("processorArchitecture", processorArchitecture)
                    cmd.Parameters.AddWithValue("isWindowsStore", isWindowsStore)
                    cmd.Parameters.AddWithValue("isPhoneStore", isPhoneStore)
                    cmd.Parameters.AddWithValue("language", language)
                    appxKey = CInt(cmd.ExecuteScalar())
                End Using
            End If


            For Each assemblyTuple In EnumerateAssemblies()
                Dim assemblyFn = assemblyTuple.Item1
                Dim assemblyName = assemblyTuple.Item2

                Dim fileKey = -1
                Dim isNewFile = False
                Using cmd As New SqlCommand($"SELECT TOP(1) FileKey FROM Files WHERE Name=@assemblyName", Db)
                    cmd.Parameters.AddWithValue("assemblyName", assemblyName)
                    Dim r = cmd.ExecuteScalar()
                    If r IsNot Nothing Then
                        fileKey = CInt(r)
                    Else
                        cmd.CommandText = "INSERT INTO Files(Name) OUTPUT INSERTED.FileKey VALUES(@assemblyName)"
                        fileKey = CInt(cmd.ExecuteScalar())
                        isNewFile = True
                    End If
                End Using

                Using cmd As New SqlCommand("SELECT TOP(1) * from XAppxFiles WHERE AppxKey=@appxKey AND FileKey=@fileKey", Db)
                    cmd.Parameters.AddWithValue("appxKey", appxKey)
                    cmd.Parameters.AddWithValue("fileKey", fileKey)
                    Dim r = cmd.ExecuteScalar()
                    If r Is Nothing Then
                        cmd.CommandText = "INSERT INTO XAppxFiles(AppxKey,FileKey) VALUES(@appxKey,@fileKey)"
                        cmd.ExecuteNonQuery()
                    End If
                End Using

                If Not isNewFile AndAlso Not assemblyName.ToLower.EndsWith(".exe") Then Continue For
                Using stream As New FileStream(assemblyFn, FileMode.Open),
                    reader As New Reflection.PortableExecutable.PEReader(stream)
                    If Not reader.HasMetadata Then Continue For
                    Dim assembly = MetadataReaderDLL.CreateFromPEReader(reader)
                    Dim count = 0
                    For Each typeDefHandle In assembly.TypeDefinitions
                        count += 1
                        Dim typeName = ""
                        Try
                            Dim typeDef = assembly.GetTypeDefinition(typeDefHandle)
                            Dim declaringType = typeDef.GetDeclaringType()
                            If Not declaringType.IsNil Then Continue For ' skip nested types
                            typeName = assembly.GetString(typeDef.Name)
                            If Not typeDef.Namespace.IsNil Then typeName = assembly.GetString(typeDef.Namespace) & "." & typeName
                        Catch ex As Exception
                            Continue For
                        End Try
                        If typeName.StartsWith("<") Then Continue For

                        Dim typeKey = -1
                        Using cmd As New SqlCommand($"SELECT TOP(1) TypeKey FROM Types WHERE Name=@typeName", Db)
                            cmd.Parameters.AddWithValue("typeName", typeName)
                            Dim r = cmd.ExecuteScalar()
                            If r IsNot Nothing Then
                                typeKey = CInt(r)
                            Else
                                cmd.CommandText = "INSERT INTO Types(Name) OUTPUT INSERTED.TypeKey VALUES(@typeName)"
                                typeKey = CInt(cmd.ExecuteScalar())
                            End If
                        End Using

                        Using cmd As New SqlCommand("Select TOP(1) * from XAppxTypes WHERE AppxKey=@appxKey AND TypeKey=@typeKey", Db)
                            cmd.Parameters.AddWithValue("appxKey", appxKey)
                            cmd.Parameters.AddWithValue("typeKey", typeKey)
                            Dim r = cmd.ExecuteScalar()
                            If r Is Nothing Then
                                cmd.CommandText = "INSERT INTO XAppxTypes(AppxKey,TypeKey) VALUES(@appxKey,@typeKey)"
                                cmd.ExecuteNonQuery()
                            End If
                        End Using


                    Next
                End Using
            Next
        Finally
            For Each act In closeActions
                act()
            Next
        End Try

        Return True
    End Function

    Function InitDb(DbPath As String) As SqlConnection
        Dim dbName = Path.GetFileNameWithoutExtension(DbPath)
        If Not File.Exists(DbPath) Then
            Dim conn0 = "Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;"
            Using db0 As New SqlConnection(conn0)
                db0.Open()
                Using cmd = db0.CreateCommand()
                    cmd.CommandText = $"CREATE DATABASE {dbName} ON PRIMARY (NAME={dbName}, FILENAME='{DbPath}')"
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
            cmd.CommandText = $"IF NOT EXISTS(SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[Appxs]')
                                AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
                                CREATE TABLE  [dbo].[Appxs] (
                                    [AppxKey] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                    [AppxFileName] NVARCHAR(128) NOT NULL,
                                    [Identity] NVARCHAR(128) NOT NULL,
                                    [Publisher] NVARCHAR(128) NOT NULL,
                                    [DisplayName] NVARCHAR(200) NOT NULL,
                                    [PublisherDisplayName] NVARCHAR(200) NOT NULL,
                                    [ProcessorArchitecture] NVARCHAR(20) NOT NULL,
                                    [IsWindowsStore] BIT NOT NULL,
                                    [IsPhoneStore] BIT NOT NULL,
                                    [Language] NVARCHAR(5) NOT NULL,
                                    [IsPopular] BIT,
                                    [DownloadCount] INT
                                )"
            cmd.ExecuteNonQuery()

            cmd.CommandText = $"IF NOT EXISTS(SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[Files]')
                                AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
                                CREATE TABLE  [dbo].[Files] (
                                    [FileKey] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                    [Name] NVARCHAR(128) NOT NULL)"
            cmd.ExecuteNonQuery()

            cmd.CommandText = $"IF NOT EXISTS(SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[Types]')
                                AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
                                CREATE TABLE  [dbo].[Types] (
                                    [TypeKey] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                    [Name] NVARCHAR(MAX) NOT NULL)"
            cmd.ExecuteNonQuery()

            cmd.CommandText = $"IF NOT EXISTS(SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[XAppxFiles]')
                                AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
                                CREATE TABLE  [dbo].[XAppxFiles] (
                                	[AppxKey] INT NOT NULL,
	                                [FileKey] INT NOT NULL,
	                                CONSTRAINT [FK_AF_A] FOREIGN KEY ([AppxKey]) REFERENCES [Appxs]([AppxKey]),
	                                CONSTRAINT [FK_AF_F] FOREIGN KEY ([FileKey]) REFERENCES [Files]([FileKey]),
	                                CONSTRAINT [PK_AF] PRIMARY KEY (AppxKey,FileKey) )"
            cmd.ExecuteNonQuery()

            cmd.CommandText = $"IF NOT EXISTS(SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[XAppxTypes]')
                                AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
                                CREATE TABLE  [dbo].[XAppxTypes] (
                                	[AppxKey] INT NOT NULL,
	                                [TypeKey] INT NOT NULL,
	                                CONSTRAINT [FK_AT_A] FOREIGN KEY ([AppxKey]) REFERENCES [Appxs]([AppxKey]),
	                                CONSTRAINT [FK_AT_F] FOREIGN KEY ([TypeKey]) REFERENCES [Types]([TypeKey]),
	                                CONSTRAINT [PK_AT] PRIMARY KEY (AppxKey,TypeKey) )"
            cmd.ExecuteNonQuery()
        End Using

        Return db
    End Function


End Module
