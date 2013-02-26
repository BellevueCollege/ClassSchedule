/*
INSERT INTO dbo.Divisions
        ( Title ,
          URL ,
          LastUpdatedBy ,
          LastUpdated
        )
--*/

--delete from dbo.Divisions

SELECT
		--[Abbreviation]
  --    ,[URL]
  --    ,[ProgramURL]
  --    ,[Title]
      d.[Division]
      --,[ContactName]
      --,[ContactPhone]
      --,[Intro]
      --,[AcademicProgram]
      ,d.[DivisionURL]
      ,(
		SELECT TOP 1
			p.[LastUpdatedBy]
--			,p.lastupdated
		FROM [MSSQL-D01\TESTMSSQL2008].[ClassSchedule].[dbo].[ProgramInformation] p
		WHERE NOT p.lastupdated IS null
		and p.Division = d.Division AND p.DivisionURL = d.divisionurl
		ORDER BY p.lastupdated DESC
		) AS LastUpdatedBy
      ,MAX(d.[LastUpdated]) AS LastUpdated
  FROM [MSSQL-D01\TESTMSSQL2008].[ClassSchedule].[dbo].[ProgramInformation] d
  GROUP BY d.Division, d.DivisionURL
GO

SELECT * FROM dbo.Divisions
