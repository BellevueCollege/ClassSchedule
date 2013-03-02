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

--delete from dbo.SectionsMeta

SELECT
	c.ClassID
	,c.Footnote
	,c.CustomTitle
	,c.CustomDescription
	,c.LastUpdatedBy
	,c.LastUpdated
FROM [MSSQL-D01\TESTMSSQL2008].[ClassSchedule].[dbo].SectionFootnote c

--SELECT * FROM dbo.SectionsMeta
