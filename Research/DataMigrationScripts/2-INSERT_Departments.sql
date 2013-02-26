/*
INSERT INTO dbo.Departments
        ( Title ,
          URL ,
          DivisionID,
          LastUpdatedBy ,
          LastUpdated
        )
--*/

--delete from dbo.Departments

SELECT
		--d.[Abbreviation]
  --    ,d.[URL]
--      ,d.[Title]
      d.[AcademicProgram]
      ,d.[ProgramURL]
      ,(
		SELECT d2.DivisionID
		FROM dbo.Divisions d2
		WHERE d2.Title = MIN(d.Division) AND d2.URL = MIN(d.DivisionURL)
	) AS DivisionID
      --,d.[Division]
      --,d.[ContactName]
      --,d.[ContactPhone]
      --,d.[Intro]
      --,d.[DivisionURL]
      ,(
		SELECT TOP 1
			p.[LastUpdatedBy]
--			,p.lastupdated
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

--SELECT * FROM dbo.Departments
