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

-- delete from dbo.Subjects

SELECT
			(SELECT DepartmentID FROM Departments WHERE Title = MIN(d.AcademicProgram) AND URL = MIN(d.ProgramURL)) AS DepartmentID
	 --d.[Abbreviation]
      d.[Title]
      --,d.[ProgramURL]
      --,d.[Division]
      --,d.[ContactName]
      --,d.[ContactPhone]
      ,ISNULL(d.[Intro], '')
	    ,d.[URL]
      --,d.[AcademicProgram]
      --,d.[DivisionURL]
      ,(
		SELECT TOP 1
			p.[LastUpdatedBy]
--			,p.lastupdated
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

