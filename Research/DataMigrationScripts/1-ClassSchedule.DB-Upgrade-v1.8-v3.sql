/*-----------------------------------------------------------------------------------------------------------
  Sanity check that we're in a ClassSchedule database before starting.
 -----------------------------------------------------------------------------------------------------------*/
IF NOT EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[dbo].[ProgramInformation]') OR object_id = OBJECT_ID(N'[dbo].[SeatAvailability]'))
BEGIN
	RAISERROR('The current database does not appear to be a ClassSchedule database. Please change to a ClassSchedule database', 18, 0)
	-- do not execute the remaining script (NOTE: needs to be turned back OFF later)
	-- see http://stackoverflow.com/questions/659188/sql-server-stop-or-break-execution-of-a-sql-script
	SET NOEXEC ON
END


/*-----------------------------------------------------------------------------------------------------------
  Back up existing database
 -----------------------------------------------------------------------------------------------------------*/
-- adapted from http://www.sqlexamples.info/SQL/tsql_backup_database.htm
DECLARE @fileName varchar(90)
DECLARE @fileDate varchar(20)

SET @fileDate = CONVERT(VARCHAR(20), GETDATE(),112)
SET @fileName = 'F:\Backup\'+DB_NAME()+'-Upgrade-v1_8-v3-'+@fileDate+'.bak'

BACKUP DATABASE DB_NAME() TO DISK = @fileName
GO

/*-----------------------------------------------------------------------------------------------------------
  Remove extraneous objects
 -----------------------------------------------------------------------------------------------------------*/

-- The new table has a PK with the same name, so we need to get rid of this one to avoid a naming collision
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[SeatAvailability]') AND name = N'PK_SeatAvailibility')
ALTER TABLE [dbo].[SeatAvailability] DROP CONSTRAINT [PK_SeatAvailibility]
GO

-- No longer needed. Should have been dropped long ago.
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Temp_SectionFootnotesImport]') AND type in (N'U'))
DROP TABLE [dbo].[Temp_SectionFootnotesImport]
GO

-- The following views are no longer needed/used in the new database schema
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_ProgramInformation]'))
DROP VIEW [dbo].[vw_ProgramInformation]
GO

IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_SeatAvailability]'))
DROP VIEW [dbo].[vw_SeatAvailability]
GO

IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_SectionFootnote]'))
DROP VIEW [dbo].[vw_SectionFootnote]
GO


/*-----------------------------------------------------------------------------------------------------------
  RedGate-generated script for migrating remaining data

              *****************************************************************************
              ** WARNING                                                                 **
              ** ----------------------------------------------------------------------- **
              ** This script was generated as a difference between databases at          **
              ** Bellevue College. If you are another college, this portion of the       **
              ** upgrade script may not work properly.                                   **
              **                   !! IT MAY EVEN BREAK YOUR DATABASE !!                 **
              *****************************************************************************
 -----------------------------------------------------------------------------------------------------------*/
/*
Run this script on:

        MSSQL-D01\TestMSSQL2008.ClassSchedule (Rev. 1118)    -  This database will be modified

to synchronize it with:

        MSSQL-D01\DevMSSQL2008.ClassSchedule (Rev. 1118)

You are recommended to back up your database before running this script

Script created by SQL Compare version 10.3.1 from Red Gate Software Ltd at 3/5/2013 12:25:23 PM

*/
IF EXISTS (SELECT * FROM tempdb..sysobjects WHERE id=OBJECT_ID('tempdb..#tmpErrors')) DROP TABLE #tmpErrors
GO
CREATE TABLE #tmpErrors (Error int)
GO
BEGIN TRANSACTION
GO
SET NUMERIC_ROUNDABORT OFF
GO
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS ON
GO
IF EXISTS (SELECT * FROM tempdb..sysobjects WHERE id=OBJECT_ID('tempdb..#tmpErrors')) DROP TABLE #tmpErrors
GO
CREATE TABLE #tmpErrors (Error int)
GO
SET XACT_ABORT ON
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO
BEGIN TRANSACTION
GO
PRINT N'Altering [dbo].[vw_Class]'
GO


ALTER view [dbo].[vw_Class]
AS
SELECT c.[ClassID]
      ,c.[YearQuarterID]
      ,c.[ItemNumber]
      ,c.[CourseID]
      ,c.[Department]
      ,c.[CourseNumber]
-- NOTE: The capacity/enrollment logic only works for B013 (Spring 2011) and later.
-- Previous to that, associated data in the cluster table is NULL. - shawn.south@bellevuecollege.edu
      ,CASE
		WHEN NOT c.[ClusterItemNumber] IS NULL
			THEN l.ClusterCapacity
			ELSE c.[ClassCapacity]
		END AS ClassCapacity
      ,CASE
		WHEN NOT c.[ClusterItemNumber] IS NULL
			THEN l.ClusterEnrolled
			ELSE c.[StudentsEnrolled]
		END AS StudentsEnrolled
	  ,u.LastUpdated
/* COMMENT THIS LINE FOR DEBUGGING
	  ,c.ClusterItemNumber AS "(ClusterItemNumber)"
	  ,c.ClassCapacity AS "(ClassCapacity)"
	  ,c.StudentsEnrolled AS "(StudentsEnrolled)"
	  ,l.ClusterCapacity AS "(ClusterCapacity)"
	  ,l.ClusterEnrolled AS "(ClusterEnrolled)"
--*/
  FROM [ODS].[dbo].[vw_Class] c
	LEFT JOIN [ODS].[dbo].[vw_ClassCluster] l ON l.ClusterItemNumber = c.ClusterItemNumber AND l.YearQuarterID = c.YearQuarterID
/* COMMENT THIS LINE FOR DEBUGGING
  WHERE NOT c.ClusterItemNumber IS NULL
  AND c.YearQuarterID >= 'B012'
  ORDER BY c.YearQuarterID
--*/
	LEFT OUTER JOIN (
		SELECT TOP (1)
			UpdateDate AS LastUpdated
		FROM ODS.dbo.vw_TableTransferLog
		WHERE (ODSTableName = 'Class')
		ORDER BY LastUpdated DESC
	) AS u ON 1 = 1

GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating [dbo].[SectionsMeta]'
GO
CREATE TABLE [dbo].[SectionsMeta]
(
[ClassID] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Footnote] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Title] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Description] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastUpdatedBy] [varchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastUpdated] [datetime] NULL
)
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating primary key [PK_Sections] on [dbo].[SectionsMeta]'
GO
ALTER TABLE [dbo].[SectionsMeta] ADD CONSTRAINT [PK_Sections] PRIMARY KEY CLUSTERED  ([ClassID])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating [dbo].[SectionSeats]'
GO
CREATE TABLE [dbo].[SectionSeats]
(
[ClassID] [char] (8) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[SeatsAvailable] [int] NULL,
[LastUpdated] [datetime] NULL
)
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating primary key [PK_SeatAvailibility] on [dbo].[SectionSeats]'
GO
ALTER TABLE [dbo].[SectionSeats] ADD CONSTRAINT [PK_SeatAvailibility] PRIMARY KEY CLUSTERED  ([ClassID])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating [dbo].[CourseMeta]'
GO
CREATE TABLE [dbo].[CourseMeta]
(
[CourseID] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Footnote] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastUpdatedBy] [varchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastUpdated] [datetime] NULL
)
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating primary key [PK_Courses] on [dbo].[CourseMeta]'
GO
ALTER TABLE [dbo].[CourseMeta] ADD CONSTRAINT [PK_Courses] PRIMARY KEY CLUSTERED  ([CourseID])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Altering [dbo].[vw_ClassScheduleData]'
GO


ALTER VIEW [dbo].[vw_ClassScheduleData]
AS
SELECT
	c.ClassID
	,c.YearQuarterID
	,c.CourseID
	,CASE
		WHEN isNULL(c.LastUpdated, DateAdd(day, - 1, getdate())) > isNULL(s.LastUpdated, DATEADD(day, - 2, getdate()))
			THEN CASE
				WHEN c.ClassCapacity - c.StudentsEnrolled < 0
					THEN 0
				ELSE c.ClassCapacity - c.StudentsEnrolled
			END
		ELSE s.SeatsAvailable
	 END AS SeatsAvailable
	,CASE
		WHEN isNULL(c.LastUpdated, DateAdd(day, - 1, getdate())) > isNULL(s.LastUpdated, DATEADD(day, - 2, getdate()))
			THEN c.LastUpdated
		ELSE s.LastUpdated
	 END AS LastUpdated
	,cm.Footnote AS CourseFootnote
	,sm.Footnote AS SectionFootnote
	--,p.Abbreviation
	--,p.ProgramURL
	--,p.Title
	--,p.URL
	--,p.Division
	--,p.ContactName
	--,p.ContactPhone
	--,p.Intro
	,sm.Title
	,sm.Description
FROM dbo.vw_Class c
LEFT OUTER JOIN dbo.SectionSeats s ON c.ClassID = s.ClassID
LEFT OUTER JOIN dbo.SectionsMeta sm ON c.ClassID = sm.ClassID
LEFT OUTER JOIN dbo.CourseMeta cm ON c.CourseID = cm.CourseID
--LEFT OUTER JOIN dbo.ProgramInformation AS p ON c.Department = p.Abbreviation


GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating [dbo].[Divisions]'
GO
CREATE TABLE [dbo].[Divisions]
(
[DivisionID] [int] NOT NULL IDENTITY(1, 1),
[Title] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[URL] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastUpdatedBy] [varchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastUpdated] [datetime] NULL
)
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating primary key [PK_Divisions] on [dbo].[Divisions]'
GO
ALTER TABLE [dbo].[Divisions] ADD CONSTRAINT [PK_Divisions] PRIMARY KEY CLUSTERED  ([DivisionID])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating [dbo].[Departments]'
GO
CREATE TABLE [dbo].[Departments]
(
[DepartmentID] [int] NOT NULL IDENTITY(1, 1),
[DivisionID] [int] NULL,
[Title] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[URL] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ProgramChairSID] [varchar] (9) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastUpdatedBy] [varchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastUpdated] [datetime] NULL
)
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating primary key [PK_Departments] on [dbo].[Departments]'
GO
ALTER TABLE [dbo].[Departments] ADD CONSTRAINT [PK_Departments] PRIMARY KEY CLUSTERED  ([DepartmentID])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating [dbo].[Subjects]'
GO
CREATE TABLE [dbo].[Subjects]
(
[SubjectID] [int] NOT NULL IDENTITY(1, 1),
[DepartmentID] [int] NULL,
[Title] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Intro] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Slug] [varchar] (63) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[LastUpdatedBy] [varchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastUpdated] [datetime] NULL
)
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating primary key [PK_Subjects] on [dbo].[Subjects]'
GO
ALTER TABLE [dbo].[Subjects] ADD CONSTRAINT [PK_Subjects] PRIMARY KEY CLUSTERED  ([SubjectID])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating [dbo].[SubjectsCoursePrefixes]'
GO
CREATE TABLE [dbo].[SubjectsCoursePrefixes]
(
[SubjectID] [int] NOT NULL,
[CoursePrefixID] [varchar] (5) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
)
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating primary key [PK_SubjectsCoursePrefixes] on [dbo].[SubjectsCoursePrefixes]'
GO
ALTER TABLE [dbo].[SubjectsCoursePrefixes] ADD CONSTRAINT [PK_SubjectsCoursePrefixes] PRIMARY KEY CLUSTERED  ([SubjectID], [CoursePrefixID])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating [dbo].[SectionCourseCrosslistings]'
GO
CREATE TABLE [dbo].[SectionCourseCrosslistings]
(
[ClassID] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[CourseID] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
)
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating primary key [PK_CourseSections] on [dbo].[SectionCourseCrosslistings]'
GO
ALTER TABLE [dbo].[SectionCourseCrosslistings] ADD CONSTRAINT [PK_CourseSections] PRIMARY KEY CLUSTERED  ([ClassID], [CourseID])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Adding constraints to [dbo].[Subjects]'
GO
ALTER TABLE [dbo].[Subjects] ADD CONSTRAINT [UQ_Subjects_Slug] UNIQUE NONCLUSTERED  ([Slug])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Adding foreign keys to [dbo].[Subjects]'
GO
ALTER TABLE [dbo].[Subjects] ADD CONSTRAINT [FK_Subjects_Departments] FOREIGN KEY ([DepartmentID]) REFERENCES [dbo].[Departments] ([DepartmentID])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Adding foreign keys to [dbo].[Departments]'
GO
ALTER TABLE [dbo].[Departments] ADD CONSTRAINT [FK_Departments_Division] FOREIGN KEY ([DivisionID]) REFERENCES [dbo].[Divisions] ([DivisionID])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Adding foreign keys to [dbo].[SubjectsCoursePrefixes]'
GO
ALTER TABLE [dbo].[SubjectsCoursePrefixes] ADD CONSTRAINT [FK_SubjectsCoursePrefixes_Subjects] FOREIGN KEY ([SubjectID]) REFERENCES [dbo].[Subjects] ([SubjectID])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Altering permissions on [dbo].[ClassSearch]'
GO
GRANT SELECT ON  [dbo].[ClassSearch] TO [WebApplicationUser]
GO
PRINT N'Altering permissions on [dbo].[CourseMeta]'
GO
GRANT SELECT ON  [dbo].[CourseMeta] TO [WebApplicationUser]
GRANT INSERT ON  [dbo].[CourseMeta] TO [WebApplicationUser]
GRANT UPDATE ON  [dbo].[CourseMeta] TO [WebApplicationUser]
GO
PRINT N'Altering permissions on [dbo].[CourseSearch]'
GO
GRANT SELECT ON  [dbo].[CourseSearch] TO [WebApplicationUser]
GO
PRINT N'Altering permissions on [dbo].[Departments]'
GO
GRANT SELECT ON  [dbo].[Departments] TO [WebApplicationUser]
GRANT INSERT ON  [dbo].[Departments] TO [WebApplicationUser]
GRANT UPDATE ON  [dbo].[Departments] TO [WebApplicationUser]
GO
PRINT N'Altering permissions on [dbo].[Divisions]'
GO
GRANT SELECT ON  [dbo].[Divisions] TO [WebApplicationUser]
GRANT INSERT ON  [dbo].[Divisions] TO [WebApplicationUser]
GRANT UPDATE ON  [dbo].[Divisions] TO [WebApplicationUser]
GO
PRINT N'Altering permissions on [dbo].[SectionCourseCrosslistings]'
GO
GRANT SELECT ON  [dbo].[SectionCourseCrosslistings] TO [WebApplicationUser]
GRANT INSERT ON  [dbo].[SectionCourseCrosslistings] TO [WebApplicationUser]
GRANT UPDATE ON  [dbo].[SectionCourseCrosslistings] TO [WebApplicationUser]
GO
PRINT N'Altering permissions on [dbo].[SectionSeats]'
GO
GRANT SELECT ON  [dbo].[SectionSeats] TO [WebApplicationUser]
GRANT INSERT ON  [dbo].[SectionSeats] TO [WebApplicationUser]
GRANT UPDATE ON  [dbo].[SectionSeats] TO [WebApplicationUser]
GO
PRINT N'Altering permissions on [dbo].[SectionsMeta]'
GO
GRANT SELECT ON  [dbo].[SectionsMeta] TO [WebApplicationUser]
GRANT INSERT ON  [dbo].[SectionsMeta] TO [WebApplicationUser]
GRANT UPDATE ON  [dbo].[SectionsMeta] TO [WebApplicationUser]
GO
PRINT N'Altering permissions on [dbo].[Subjects]'
GO
GRANT SELECT ON  [dbo].[Subjects] TO [WebApplicationUser]
GRANT INSERT ON  [dbo].[Subjects] TO [WebApplicationUser]
GRANT UPDATE ON  [dbo].[Subjects] TO [WebApplicationUser]
GO
PRINT N'Altering permissions on [dbo].[SubjectsCoursePrefixes]'
GO
GRANT SELECT ON  [dbo].[SubjectsCoursePrefixes] TO [WebApplicationUser]
GRANT INSERT ON  [dbo].[SubjectsCoursePrefixes] TO [WebApplicationUser]
GRANT UPDATE ON  [dbo].[SubjectsCoursePrefixes] TO [WebApplicationUser]
GO
IF EXISTS (SELECT * FROM #tmpErrors) ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT>0 BEGIN
PRINT 'The database update succeeded'
COMMIT TRANSACTION
END
ELSE PRINT 'The database update failed'
GO
DROP TABLE #tmpErrors
GO
IF EXISTS (SELECT * FROM tempdb..sysobjects WHERE id=OBJECT_ID('tempdb..#tmpErrors')) IF EXISTS(SELECT * FROM #tmpErrors) ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT>0 BEGIN
PRINT 'The database update succeeded'
COMMIT TRANSACTION
END
ELSE PRINT 'The database update failed'
GO
IF EXISTS (SELECT * FROM tempdb..sysobjects WHERE id=OBJECT_ID('tempdb..#tmpErrors')) DROP TABLE #tmpErrors
GO
-- End Red Gate SQL Compare-generated script


/*-----------------------------------------------------------------------------------------------------------
  Migrate the data into the new database structure.
  NOTE: Red Gate's tools work best when data comparisons are 1:1 (table-table), so these migrations were
        written manually.
 -----------------------------------------------------------------------------------------------------------*/

/*-----------------------------------------------------------------------------
  Divisions
 -----------------------------------------------------------------------------*/
--/*
INSERT INTO dbo.Divisions
        ( Title ,
          URL ,
          LastUpdatedBy ,
          LastUpdated
        )
--*/
SELECT
      d.[Division]
      ,d.[DivisionURL]
      ,(
		SELECT TOP 1
			p.[LastUpdatedBy]
		FROM [ProgramInformation] p
		WHERE NOT p.lastupdated IS null
		and p.Division = d.Division AND p.DivisionURL = d.divisionurl
		ORDER BY p.lastupdated DESC
		) AS LastUpdatedBy
      ,MAX(d.[LastUpdated]) AS LastUpdated
  FROM [ProgramInformation] d
  GROUP BY d.Division, d.DivisionURL
GO

/*-----------------------------------------------------------------------------
  Departments
 -----------------------------------------------------------------------------*/
--/*
INSERT INTO dbo.Departments
        ( Title ,
          URL ,
          DivisionID,
          LastUpdatedBy ,
          LastUpdated
        )
--*/
SELECT
      d.[AcademicProgram]
      ,d.[ProgramURL]
      ,(
		SELECT d2.DivisionID
		FROM dbo.Divisions d2
		WHERE d2.Title = MIN(d.Division) AND d2.URL = MIN(d.DivisionURL)
	) AS DivisionID
      ,(
		SELECT TOP 1
			p.[LastUpdatedBy]
		FROM [ProgramInformation] p
		WHERE NOT p.lastupdated IS null
		and p.[AcademicProgram] = d.[AcademicProgram] AND p.ProgramURL = d.ProgramURL
		ORDER BY p.lastupdated DESC
		) AS LastUpdatedBy
      ,MAX(d.[LastUpdated]) AS LastUpdated
  FROM [ProgramInformation] d
  GROUP BY d.[AcademicProgram], d.ProgramURL
  ORDER BY d.[AcademicProgram]
GO

/*-----------------------------------------------------------------------------
  Subjects
 -----------------------------------------------------------------------------*/
--/*
INSERT INTO dbo.Subjects
        ( DepartmentID
					Title,
          Intro,
          Slug,
          LastUpdatedBy,
          LastUpdated
        )
--*/
SELECT
			(SELECT DepartmentID FROM Departments WHERE Title = MIN(d.AcademicProgram) AND URL = MIN(d.ProgramURL)) AS DepartmentID
      d.[Title]
      ,ISNULL(d.[Intro], '')
	    ,d.[URL]
      ,(
		SELECT TOP 1
			p.[LastUpdatedBy]
		FROM [ProgramInformation] p
		WHERE NOT p.lastupdated IS null
		and p.Title = d.Title AND ISNULL(p.[Intro], '') = ISNULL(d.[Intro], '')
		ORDER BY p.lastupdated DESC
		) AS LastUpdatedBy
      ,MAX(d.[LastUpdated]) AS LastUpdated
  FROM [ProgramInformation] d
  GROUP BY d.Title, ISNULL(d.Intro, ''), d.URL
  ORDER BY d.Title
GO

/*-----------------------------------------------------------------------------
  SubjectsCoursePrefixes
 -----------------------------------------------------------------------------*/
--/*
INSERT INTO dbo.SubjectsCoursePrefixes
        ( SubjectID, CoursePrefixID )
--*/
SELECT DISTINCT
	s.SubjectID
	,p.Abbreviation
FROM dbo.Subjects s
JOIN [ProgramInformation] p	ON p.URL = s.Slug
GO

/*-----------------------------------------------------------------------------
  SectionsMeta
 -----------------------------------------------------------------------------*/
--/*
INSERT INTO dbo.SectionsMeta
        ( ClassID ,
          Footnote ,
          Title ,
          Description ,
          LastUpdatedBy ,
          LastUpdated
        )
--*/
SELECT
	c.ClassID
	,c.Footnote
	,c.CustomTitle
	,c.CustomDescription
	,c.LastUpdatedBy
	,c.LastUpdated
FROM SectionFootnote c
GO

/*-----------------------------------------------------------------------------
  SectionSeats
 -----------------------------------------------------------------------------*/
--/*
INSERT INTO dbo.SectionSeats
        ( ClassID ,
          SeatsAvailable ,
          LastUpdated
        )
--*/
SELECT
	c.ClassID
	,c.SeatsAvailable
	,c.LastUpdated
FROM SeatAvailability c
GO

/*-----------------------------------------------------------------------------
  CourseMeta
 -----------------------------------------------------------------------------*/
--/*
INSERT INTO dbo.CourseMeta
        ( CourseID ,
          Footnote ,
          LastUpdatedBy ,
          LastUpdated
        )
--*/
SELECT
	c.CourseID
	,c.Footnote
	,c.LastUpdatedBy
	,c.LastUpdated
FROM CourseFootnote c
GO


/*-----------------------------------------------------------------------------------------------------------
  Clean-up, etc.
 -----------------------------------------------------------------------------------------------------------*/

-- re-enable execution of script
-- see http://stackoverflow.com/questions/659188/sql-server-stop-or-break-execution-of-a-sql-script
SET NOEXEC OFF
