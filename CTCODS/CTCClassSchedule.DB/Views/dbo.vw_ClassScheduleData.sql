SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO


CREATE VIEW [dbo].[vw_ClassScheduleData]
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
GRANT SELECT ON  [dbo].[vw_ClassScheduleData] TO [WebApplicationUser]
GO
