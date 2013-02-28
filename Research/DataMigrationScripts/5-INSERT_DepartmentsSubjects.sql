

/* ============================================================================
	This script still needs work. It's not picking up some of the records in
	the	original table, and I didn't have time to determine whether or not
	that is the correct behavior. - shawn.south@bellevuecollege.edu
 ============================================================================ */



/*
INSERT INTO dbo.DepartmentsSubjects
        ( DepartmentID, SubjectID )
--*/

-- delete from dbo.Subjects

SELECT
	--p.Abbreviation,
	--p.URL,
	--p.Title,
	d.DepartmentID
	--,d.Title
	--,p.AcademicProgram
	,x.SubjectID
	--,d.URL
	--,p.ProgramURL
	--,p.lastupdated
FROM dbo.Departments d
JOIN [MSSQL-D01\TESTMSSQL2008].[ClassSchedule].[dbo].[ProgramInformation] p
	ON p.AcademicProgram = d.Title AND p.ProgramURL = d.URL -- AND d.LastUpdated = p.LastUpdated
JOIN dbo.SubjectsCoursePrefixes x ON x.CoursePrefixID = p.Abbreviation
ORDER BY p.Abbreviation, p.URL