/*
INSERT INTO dbo.CourseMeta
        ( CourseID ,
          Footnote ,
          LastUpdatedBy ,
          LastUpdated
        )
--*/

--delete from dbo.CourseMeta

SELECT
	c.CourseID
	,c.Footnote
	,c.LastUpdatedBy
	,c.LastUpdated
FROM [MSSQL-D01\TESTMSSQL2008].[ClassSchedule].[dbo].CourseFootnote c

--SELECT * FROM dbo.CourseMeta
