IF OBJECT_ID('namespace') IS NOT NULL DROP FUNCTION namespace
IF OBJECT_ID('type') IS NOT NULL DROP FUNCTION type
IF OBJECT_ID('file') IS NOT NULL DROP FUNCTION [file]
GO
CREATE FUNCTION namespace() RETURNS NVARCHAR(MAX) AS BEGIN RETURN 'FlurryWin8SDK' END
GO
CREATE FUNCTION type() RETURNS NVARCHAR(MAX) AS BEGIN RETURN 'WriteableBitmapExtensions' END
GO
CREATE FUNCTION [file]() RETURNS NVARCHAR(MAX) AS BEGIN RETURN 'NodaTime.dll' END
GO






-- What types are in the namespace? and which apps use them?
IF OBJECT_ID('Types_for_Namespace') IS NOT NULL DROP VIEW Types_for_Namespace
GO
CREATE VIEW Types_for_Namespace AS
SELECT TOP(5000) N.Name Namespace, T.Name Type, A.DisplayName, A.PublisherDisplayName
FROM Types T
INNER JOIN Namespaces N ON T.NamespaceKey = N.NamespaceKey
INNER JOIN XAppTypes AT ON T.TypeKey = AT.TypeKey
INNER JOIN TopNetAppxs A ON A.Appkey = AT.AppKey
AND N.Name LIKE dbo.namespace()
ORDER BY T.Name
GO

-- Which files are used by apps that declare this particular namespace/type?
IF OBJECT_ID('Files_for_NamespaceType') IS NOT NULL DROP VIEW Files_for_NamespaceType
GO
CREATE VIEW Files_for_NamespaceType AS
SELECT TOP(5000) N.Name Namespace, T.Name Type,
   F.Name CandidateFile,
   COUNT(DISTINCT A.AppKey) AppCount,
   (SELECT COUNT(DISTINCT AT.AppKey)
    FROM Types T
    INNER JOIN Namespaces N ON T.NamespaceKey = N.NamespaceKey
    INNER JOIN XAppTypes AT ON AT.TypeKey = T.TypeKey
    INNER JOIN TopNetAppxs A ON A.AppKey = AT.AppKey
    WHERE N.Name LIKE dbo.namespace()) OutOfApps
FROM Types T
INNER JOIN Namespaces N ON T.NamespaceKey = N.NamespaceKey
INNER JOIN XAppTypes AT ON AT.TypeKey = T.TypeKey
INNER JOIN TopNetAppxs A ON A.AppKey = AT.AppKey
INNER JOIN XAppFiles AF ON AF.AppKey = A.AppKey
INNER JOIN Files F ON F.FileKey = AF.FileKey
WHERE N.Name LIKE dbo.namespace() AND T.Name LIKE dbo.type()
GROUP BY N.Name, T.Name, F.Name
ORDER BY AppCount DESC
GO
--
IF OBJECT_ID('Files_for_Namespace') IS NOT NULL DROP VIEW Files_for_Namespace
GO
CREATE VIEW Files_for_Namespace AS
SELECT TOP(5000) N.Name Namespace,
   F.Name CandidateFile,
   COUNT(DISTINCT A.AppKey) AppCount,
   (SELECT COUNT(DISTINCT AT.AppKey)
    FROM Types T
    INNER JOIN Namespaces N ON T.NamespaceKey = N.NamespaceKey
    INNER JOIN XAppTypes AT ON AT.TypeKey = T.TypeKey
    INNER JOIN TopNetAppxs A ON A.AppKey = AT.AppKey
    WHERE N.Name LIKE dbo.namespace()) OutOfApps
FROM Types T
INNER JOIN Namespaces N ON T.NamespaceKey = N.NamespaceKey
INNER JOIN XAppTypes AT ON AT.TypeKey = T.TypeKey
INNER JOIN TopNetAppxs A ON A.AppKey = AT.AppKey
INNER JOIN XAppFiles AF ON AF.AppKey = A.AppKey
INNER JOIN Files F ON F.FileKey = AF.FileKey
WHERE N.Name LIKE dbo.namespace() 
GROUP BY N.Name, F.Name
ORDER BY AppCount DESC
GO


-- Of the apps that declare this type, which ones have / don't have a particular file?
IF OBJECT_ID('Apps_for_NamespaceTypeFile') IS NOT NULL DROP VIEW Apps_for_NamespaceTypeFile
GO
CREATE VIEW Apps_for_NamespaceTypeFile AS
SELECT DISTINCT TOP(5000)
		        N.Name Namespace, T.Name Type,
				A.DisplayName UsedInApp,
				A.RatingCount,
                FORMAT(A.RatingCount / (SELECT RatingsSum FROM ratingsSum),'p') PercentRating, A.PublisherDisplayName Publisher,
				(SELECT COUNT(DISTINCT F.Name) FROM Files F INNER JOIN XAppFiles AF ON AF.FileKey=F.FileKey WHERE A.AppKey=AF.AppKey AND F.Name LIKE dbo.[file]()) DoesAppHaveThisDll,
				dbo.[file]() DllName
FROM TopNetAppxs A
INNER JOIN XAppTypes AT ON A.AppKey = AT.AppKey
INNER JOIN Types T ON AT.TypeKey = T.TypeKey
INNER JOIN Namespaces N ON T.NamespaceKey = N.NamespaceKey
WHERE N.Name LIKE dbo.namespace() AND T.Name LIKE dbo.type()
ORDER BY DoesAppHaveThisDll
GO

-- Which apps use this file?
IF OBJECT_ID('Apps_for_File') IS NOT NULL DROP VIEW Apps_for_File
GO
CREATE VIEW Apps_for_File AS
SELECT TOP(5000) A.Name,
      1 AppCount,
	  A.RatingCount,
	  FORMAT(SUM(A.RatingCount) / (SELECT RatingsSum FROM ratingsSum), 'p') RatingsPercent,
	  dbo.[file]() UsesThisDll
FROM TopNetAppxs A 
INNER JOIN XAppFiles AF ON A.AppKey = AF.AppKey
INNER JOIN Files F ON AF.FileKey = F.FileKey
WHERE F.Name LIKE dbo.[file]()
GROUP BY A.Name, A.RatingCount WITH ROLLUP
ORDER BY A.RatingCount DESC
GO

-- What are the target platforms?
IF OBJECT_ID('TopPlatforms') IS NOT NULL DROP VIEW TopPlatforms
GO
CREATE VIEW TopPlatforms AS
SELECT TOP(1000) A.TargetPlatform,
       COUNT(*) AppCount,
	   FORMAT(COUNT(*) * 1.0 / (SELECT SUM(Q.AppCount) FROM (SELECT A.TargetPlatform, COUNT(*) AppCount, SUM(A.RatingCount) SumAppRatings FROM Apps A GROUP BY A.TargetPlatform) Q),'p') AppPercent,
	   SUM(A.RatingCount) RatingsSum,
	   FORMAT(SUM(A.RatingCount) * 1.0 / (SELECT SUM(Q.SumAppRatings) FROM (SELECT A.TargetPlatform, COUNT(*) AppCount, SUM(A.RatingCount) SumAppRatings FROM Apps A GROUP BY A.TargetPlatform) Q), 'p') PercentRatings
FROM Apps A
GROUP BY A.TargetPlatform
ORDER BY RatingsSum DESC
GO

-- What are the namespaces?
IF OBJECT_ID('TopNamespaces') IS NOT NULL DROP VIEW TopNamespaces
GO
CREATE VIEW TopNamespaces AS
SELECT TOP(3000) Q.Name Namespace,
       SUM(Q.Count) AppCount,
	   FORMAT(SUM(Q.Count) / (SELECT AppCount FROM appCount),'p') PercentApps,
	   SUM(Q.RatingCount) SumRatings,
	   FORMAT(SUM(Q.RatingCount) / (SELECT RatingsSum FROM ratingsSum),'p') PercentRating
FROM (SELECT DISTINCT N.Name, A.AppKey, 1 Count, A.RatingCount
      FROM TopNetAppxs A
      INNER JOIN XAppTypes AT ON A.AppKey = AT.AppKey
      INNER JOIN Types T ON AT.TypeKey = T.TypeKey
      INNER JOIN Namespaces N ON T.NamespaceKey = N.NamespaceKey) Q
GROUP BY Q.Name
ORDER BY SumRatings DESC
GO

-- Of the apps that declare this namespace, how important are they?
IF OBJECT_ID('Top200NetAppxs') IS NOT NULL DROP VIEW Top200NetAppxs
GO
CREATE VIEW Top200NetAppxs AS
SELECT TOP(200) *
FROM Apps WHERE TargetPlatform LIKE '%appx' AND AuthoringLanguage = '.NET'
ORDER BY RatingCount DESC
GO
--
IF OBJECT_ID('ratings200Sum') IS NOT NULL DROP VIEW ratings200Sum
GO
CREATE VIEW ratings200Sum AS SELECT SUM(RatingCount)*1.0 Ratings200Sum FROM Top200NetAppxs
GO
--
IF OBJECT_ID('Apps200_for_Namespace') IS NOT NULL DROP VIEW Apps200_for_Namespace
GO
CREATE VIEW Apps200_for_Namespace AS
SELECT DISTINCT TOP(5000)
		        N.Name Namespace,
				A.DisplayName UsedInApp,
				A.RatingCount,
                FORMAT(A.RatingCount / (SELECT Ratings200Sum FROM ratings200Sum),'p') PercentRating, A.PublisherDisplayName Publisher
FROM Top200NetAppxs A
INNER JOIN XAppTypes AT ON A.AppKey = AT.AppKey
INNER JOIN Types T ON AT.TypeKey = T.TypeKey
INNER JOIN Namespaces N ON T.NamespaceKey = N.NamespaceKey
WHERE N.Name LIKE dbo.namespace() 
ORDER BY A.RatingCount DESC
GO
--
IF OBJECT_ID('Summary_Apps200_for_Namespace') IS NOT NULL DROP VIEW Summary_Apps200_for_Namespace
GO
CREATE VIEW Summary_Apps200_for_Namespace AS
SELECT dbo.namespace() Namespace, COUNT(*) AppsOf200, 200.0 * SUM(A.RatingCount) / (SELECT Ratings200Sum FROM ratings200Sum) AdjustedAppsOf200
FROM Apps200_for_Namespace A
GO
--
IF OBJECT_ID('Apps200_for_File') IS NOT NULL DROP VIEW Apps200_for_File
GO
CREATE VIEW Apps200_for_File AS
SELECT DISTINCT TOP(5000)
		        F.Name [File],
				A.DisplayName UsedInApp,
				A.RatingCount,
                FORMAT(A.RatingCount / (SELECT Ratings200Sum FROM ratings200Sum),'p') PercentRating, A.PublisherDisplayName Publisher
FROM Top200NetAppxs A
INNER JOIN XAppFiles AF ON A.AppKey = AF.AppKey
INNER JOIN Files F ON AF.FileKey = F.FileKey
WHERE F.Name LIKE dbo.[file]() 
ORDER BY A.RatingCount DESC
GO
--
IF OBJECT_ID('Summary_Apps200_for_File') IS NOT NULL DROP VIEW Summary_Apps200_for_File
GO
CREATE VIEW Summary_Apps200_for_File AS
SELECT dbo.[file]() [File], COUNT(*) AppsOf200, 200.0 * SUM(A.RatingCount) / (SELECT Ratings200Sum FROM ratings200Sum) AdjustedAppsOf200
FROM Apps200_for_File A
GO


SELECT A.DisplayName, A.TargetPlatform
FROM Apps A
INNER JOIN XAppFiles AF ON A.AppKey = AF.AppKey
INNER JOIN Files F ON AF.FileKey = F.FileKey
WHERE F.Name LIKE '%NodaTime%'
