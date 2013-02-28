/*
INSERT INTO dbo.SubjectsCoursePrefixes
        ( SubjectID, CoursePrefixID )
--*/

-- delete from dbo.Subjects

SELECT DISTINCT
	s.SubjectID
--	,s.Slug
	,p.Abbreviation
FROM dbo.Subjects s
JOIN [MSSQL-D01\TESTMSSQL2008].[ClassSchedule].[dbo].[ProgramInformation] p
	ON p.URL = s.Slug
