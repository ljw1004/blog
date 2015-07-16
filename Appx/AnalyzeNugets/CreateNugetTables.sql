IF NOT EXISTS(SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[Nugets]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
CREATE TABLE  [dbo].[Nugets] (
    [ID] NVARCHAR(256) NOT NULL PRIMARY KEY,
    [Version] NVARCHAR(32),
	[Title] NVARCHAR(256),
	[Authors] NVARCHAR(256),
    [Path] NVARCHAR(256),
    [DownloadCount] INT)

IF NOT EXISTS(SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[References]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
CREATE TABLE  [dbo].[References] (
    [ID] NVARCHAR(256) NOT NULL,
    [File] NVARCHAR(256) NOT NULL,
	CONSTRAINT [PK_R] PRIMARY KEY (ID,[File]),
	CONSTRAINT [FK_R_ID] FOREIGN KEY ([ID]) REFERENCES [Nugets]([ID]) )

IF NOT EXISTS(SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[FrameworkAssemblies]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
CREATE TABLE  [dbo].[FrameworkAssemblies] (
    [ID] NVARCHAR(256) NOT NULL,
    [AssemblyName] NVARCHAR(256) NOT NULL,
	[TargetFramework] NVARCHAR(256)
	CONSTRAINT [PK_FA] PRIMARY KEY (ID,AssemblyName,TargetFramework),
	CONSTRAINT [FK_FA_ID] FOREIGN KEY ([ID]) REFERENCES [Nugets]([ID]) )

IF NOT EXISTS(SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[Dependencies]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
CREATE TABLE  [dbo].[Dependencies] (
    [ID] NVARCHAR(256),
    [TargetFramework] NVARCHAR(256),
	[DependencyID] NVARCHAR(256),
	CONSTRAINT [PK_D] PRIMARY KEY (ID,TargetFramework,DependencyID),
	CONSTRAINT [FK_D_ID] FOREIGN KEY ([ID]) REFERENCES [Nugets]([ID]),
	CONSTRAINT [FK_D_D] FOREIGN KEY ([DependencyID]) REFERENCES [Nugets]([ID]) )

IF NOT EXISTS(SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[Files]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
CREATE TABLE  [dbo].[Files] (
    [ID] NVARCHAR(256) NOT NULL,
    [Directory] NVARCHAR(256) NOT NULL,
	[Name] NVARCHAR(256) NOT NULL,
	[Extension] NVARCHAR(256) NOT NULL,
	CONSTRAINT [PK_F] PRIMARY KEY (ID,Directory,Name),
	CONSTRAINT [FK_F_ID] FOREIGN KEY ([ID]) REFERENCES [Nugets]([ID]) )
