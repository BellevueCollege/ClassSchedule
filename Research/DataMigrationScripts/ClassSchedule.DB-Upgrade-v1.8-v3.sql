/*-----------------------------------------------------------------------------------------------------------
  Back up existing database
 -----------------------------------------------------------------------------------------------------------*/

-- TODO


/*-----------------------------------------------------------------------------------------------------------
  Remove extraneous objects
 -----------------------------------------------------------------------------------------------------------*/

-- TODO


/*-----------------------------------------------------------------------------------------------------------
  Migrate data into tables manually
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


/*-----------------------------------------------------------------------------------------------------------
  RedGate-generated script for migrating remaining data
 -----------------------------------------------------------------------------------------------------------*/
