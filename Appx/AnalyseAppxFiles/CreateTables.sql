IF NOT EXISTS(SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[Apps]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
CREATE TABLE  [dbo].[Apps] (
    [AppKey] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(128) NOT NULL,
    [Publisher] NVARCHAR(128) NOT NULL,
    [ProcessorArchitecture] NVARCHAR(20) NOT NULL,
    [Version] NVARCHAR(20) NOT NULL,
    [TargetPlatform] NVARCHAR(20) NOT NULL,
    [Path] NVARCHAR(200),
    [DisplayName] NVARCHAR(200) NOT NULL,
    [PublisherDisplayName] NVARCHAR(200) NOT NULL,
    [AuthoringLanguage] NVARCHAR(5) NOT NULL,
    [IsTop] BIT,
    [Rating] INT,
    [RatingCount] INT,
    [MediaType] NVARCHAR(5),
    [Category] NVARCHAR(60))


IF NOT EXISTS(SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[Files]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
CREATE TABLE  [dbo].[Files] (
    [FileKey] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(128) NOT NULL,
	[Size] INT NOT NULL,
	[Title] NVARCHAR(256),
	[TargetFramework] NVARCHAR(256),
	[TargetFrameworkDisplayName] NVARCHAR(256),
	[Flags] NVARCHAR(64))


IF NOT EXISTS(SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[Namespaces]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
CREATE TABLE  [dbo].[Namespaces] (
    [NamespaceKey] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(128) NOT NULL)


IF NOT EXISTS(SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[Types]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
CREATE TABLE  [dbo].[Types] (
    [TypeKey] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [NamespaceKey] INT NOT NULL,
    [Name] NVARCHAR(MAX) NOT NULL,
    CONSTRAINT [FK_T_N] FOREIGN KEY ([NamespaceKey]) REFERENCES [Namespaces]([NamespaceKey]) )


IF NOT EXISTS(SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[References]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
CREATE TABLE  [dbo].[References] (
    [ReferenceKey] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(MAX) NOT NULL,
	[Version] NVARCHAR(32))


IF NOT EXISTS(SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[XAppFiles]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
CREATE TABLE  [dbo].[XAppFiles] (
    [AppKey] INT NOT NULL,
	[FileKey] INT NOT NULL,
	CONSTRAINT [FK_AF_A] FOREIGN KEY ([AppKey]) REFERENCES [Apps]([AppKey]),
	CONSTRAINT [FK_AF_F] FOREIGN KEY ([FileKey]) REFERENCES [Files]([FileKey]),
	CONSTRAINT [PK_AF] PRIMARY KEY (AppKey,FileKey) )


IF NOT EXISTS(SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[XFileTypes]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
CREATE TABLE  [dbo].[XFileTypes] (
    [FileKey] INT NOT NULL,
	[TypeKey] INT NOT NULL,
	CONSTRAINT [FK_FT_F] FOREIGN KEY ([FileKey]) REFERENCES [Files]([FileKey]),
	CONSTRAINT [FK_FT_T] FOREIGN KEY ([TypeKey]) REFERENCES [Types]([TypeKey]),
	CONSTRAINT [PK_FT] PRIMARY KEY (FileKey,TypeKey) )


IF NOT EXISTS(SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[XFileReferences]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
CREATE TABLE  [dbo].[XFileReferences] (
    [FileKey] INT NOT NULL,
	[ReferenceKey] INT NOT NULL,
	CONSTRAINT [FK_FR_F] FOREIGN KEY ([FileKey]) REFERENCES [Files]([FileKey]),
	CONSTRAINT [FK_FR_R] FOREIGN KEY ([ReferenceKey]) REFERENCES [References]([ReferenceKey]),
	CONSTRAINT [PK_FR] PRIMARY KEY (FileKey,ReferenceKey) )



IF OBJECT_ID('AppsTypes') IS NOT NULL DROP VIEW AppsTypes
GO
CREATE VIEW AppsTypes AS
SELECT A.*, F.Name Filename, N.Name Namespace, T.Name Type
FROM Apps A
INNER JOIN XAppFiles AF ON A.AppKey = AF.AppKey
INNER JOIN Files F ON AF.FileKey = F.FileKey
INNER JOIN XFileTypes FT ON F.FileKey = FT.FileKey
INNER JOIN Types T ON FT.TypeKey = T.TypeKey
INNER JOIN Namespaces N ON T.NamespaceKey = N.NamespaceKey
GO

IF OBJECT_ID('AppsReferences') IS NOT NULL DROP VIEW AppsReferences
GO
CREATE VIEW AppsReferences AS
SELECT A.*, F.Name Filename, F.TargetFramework, F.TargetFrameworkDisplayName, F.Flags, R.Name Reference, R.Version RefVersion
FROM Apps A
INNER JOIN XAppFiles AF ON A.AppKey = AF.AppKey
INNER JOIN Files F ON AF.FileKey = F.FileKey
INNER JOIN XFileReferences FR ON F.FileKey = FR.FileKey
INNER JOIN [References] R ON FR.ReferenceKey = R.ReferenceKey
GO
