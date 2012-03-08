
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
		WHEN isNULL(u.LastUpdated, DateAdd(day, - 1, getdate())) > isNULL(sa.LastUpdated, DATEADD(day, - 2, getdate()))
			THEN CASE
				WHEN c.ClassCapacity - c.StudentsEnrolled < 0
					THEN 0
				ELSE c.ClassCapacity - c.StudentsEnrolled
			END
		ELSE sa.SeatsAvailable
	END AS SeatsAvailable
	,CASE
		WHEN isNULL(u.LastUpdated, DateAdd(day, - 1, getdate())) > isNULL(sa.LastUpdated, DATEADD(day, - 2, getdate()))
			THEN u.LastUpdated
		ELSE sa.LastUpdated
	END AS LastUpdated
	,cf.Footnote AS CourseFootnote
	,sf.Footnote AS SectionFootnote
	,p.Abbreviation
	,p.ProgramURL
	,p.Title
	,p.URL
	,p.Division
	,p.ContactName
	,p.ContactPhone
	,p.Intro
	,sf.CustomTitle
	,sf.CustomDescription
FROM dbo.vw_Class AS c
LEFT OUTER JOIN dbo.vw_SeatAvailability AS sa ON c.ClassID = sa.ClassID
LEFT OUTER JOIN dbo.SectionFootnote AS sf ON c.ClassID = sf.ClassID
LEFT OUTER JOIN dbo.CourseFootnote AS cf ON c.CourseID = cf.CourseID
LEFT OUTER JOIN dbo.ProgramInformation AS p ON c.Department = p.Abbreviation
LEFT OUTER JOIN (
	SELECT TOP (1)
		UpdateDate AS LastUpdated
	FROM ODS.dbo.vw_TableTransferLog
	WHERE (ODSTableName = 'Class')
	ORDER BY LastUpdated DESC
) AS u ON 1 = 1

GO

GRANT SELECT ON  [dbo].[vw_ClassScheduleData] TO [WebApplicationUser]
GO
