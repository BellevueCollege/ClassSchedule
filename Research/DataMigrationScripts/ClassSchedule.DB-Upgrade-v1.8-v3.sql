

-- TODO: Sanity check that we're in a ClassSchedule database before starting.



/*-----------------------------------------------------------------------------------------------------------
  Back up existing database
 -----------------------------------------------------------------------------------------------------------*/

-- TODO


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



/*-----------------------------------------------------------------------------------------------------------
  Perform data migrations that are too complex to set up easily in RedGate SQL Compare
 -----------------------------------------------------------------------------------------------------------*/

/*-----------------------------------------------------------------------------
  Divisions
 -----------------------------------------------------------------------------*/
/*
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
		FROM [MSSQL-D01\TESTMSSQL2008].[ClassSchedule].[dbo].[ProgramInformation] p
		WHERE NOT p.lastupdated IS null
		and p.Division = d.Division AND p.DivisionURL = d.divisionurl
		ORDER BY p.lastupdated DESC
		) AS LastUpdatedBy
      ,MAX(d.[LastUpdated]) AS LastUpdated
  FROM [MSSQL-D01\TESTMSSQL2008].[ClassSchedule].[dbo].[ProgramInformation] d
  GROUP BY d.Division, d.DivisionURL
GO

/*-----------------------------------------------------------------------------
  Departments
 -----------------------------------------------------------------------------*/
/*
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
		FROM [MSSQL-D01\TESTMSSQL2008].[ClassSchedule].[dbo].[ProgramInformation] p
		WHERE NOT p.lastupdated IS null
		and p.[AcademicProgram] = d.[AcademicProgram] AND p.ProgramURL = d.ProgramURL
		ORDER BY p.lastupdated DESC
		) AS LastUpdatedBy
      ,MAX(d.[LastUpdated]) AS LastUpdated
  FROM [MSSQL-D01\TESTMSSQL2008].[ClassSchedule].[dbo].[ProgramInformation] d
  GROUP BY d.[AcademicProgram], d.ProgramURL
  ORDER BY d.[AcademicProgram]
GO

/*-----------------------------------------------------------------------------
  Subjects
 -----------------------------------------------------------------------------*/
/*
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
		FROM [MSSQL-D01\TESTMSSQL2008].[ClassSchedule].[dbo].[ProgramInformation] p
		WHERE NOT p.lastupdated IS null
		and p.Title = d.Title AND ISNULL(p.[Intro], '') = ISNULL(d.[Intro], '')
		ORDER BY p.lastupdated DESC
		) AS LastUpdatedBy
      ,MAX(d.[LastUpdated]) AS LastUpdated
  FROM [MSSQL-D01\TESTMSSQL2008].[ClassSchedule].[dbo].[ProgramInformation] d
  GROUP BY d.Title, ISNULL(d.Intro, ''), d.URL
  ORDER BY d.Title
GO

SELECT * FROM dbo.Subjects

/*-----------------------------------------------------------------------------
  SubjectsCoursePrefixes
 -----------------------------------------------------------------------------*/
/*
INSERT INTO dbo.SubjectsCoursePrefixes
        ( SubjectID, CoursePrefixID )
--*/
SELECT DISTINCT
	s.SubjectID
	,p.Abbreviation
FROM dbo.Subjects s
JOIN [MSSQL-D01\TESTMSSQL2008].[ClassSchedule].[dbo].[ProgramInformation] p
	ON p.URL = s.Slug

/*-----------------------------------------------------------------------------
  SectionsMeta
 -----------------------------------------------------------------------------*/
/*
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
FROM [MSSQL-D01\TESTMSSQL2008].[ClassSchedule].[dbo].SectionFootnote c

/*-----------------------------------------------------------------------------
  SectionSeats
 -----------------------------------------------------------------------------*/
/*
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
FROM [MSSQL-D01\TESTMSSQL2008].[ClassSchedule].[dbo].SeatAvailability c

/*-----------------------------------------------------------------------------
  CourseMeta
 -----------------------------------------------------------------------------*/
/*
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
FROM [MSSQL-D01\TESTMSSQL2008].[ClassSchedule].[dbo].CourseFootnote c

