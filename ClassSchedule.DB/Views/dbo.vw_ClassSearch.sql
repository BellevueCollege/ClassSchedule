
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO









CREATE view [dbo].[vw_ClassSearch]

AS


select
c.ClassID
,c.YearQuarterID
,c.ItemNumber
,c.CourseID
,ISNULL(c1.CourseTitle2, c.CourseTitle) as CourseTitle
,p.Title as CourseSubject
,f.FootnoteText + f1.FootnoteText as Footnotes
,CASE
	WHEN e.AliasName is NULL then e.FirstName + ' ' + e.LastName
	ELSE e.FirstName + ' (' + e.AliasName + ') ' + e.LastName
END as InstructorName
,(select top 1 [Description] from ODS.dbo.vw_CourseDescription where CourseID=c.CourseID order by EffectiveYearQuarterBegin DESC) as CourseDescription
,(select top 1 [Description] from ODS.dbo.vw_CourseDescription2 where CourseID=c.CourseID order by EffectiveYearQuarterBegin DESC) as CourseDescription2
,c.ItemYRQLink
from ODS.dbo.vw_Class c
left outer join ODS.dbo.vw_CoursePrefix p on c.Department = p.CoursePrefixID
left outer join ODS.dbo.vw_Course c1 on c.CourseID=c1.CourseID and c1.EffectiveYearQuarterEnd='Z999'
left outer join ODS.dbo.vw_Footnote f on c.FootnoteID1 = f.FootnoteID
left outer join ODS.dbo.vw_Footnote f1 on c.FootnoteID2 = f1.FootnoteID
left outer join ODS.dbo.vw_Employee e on c.InstructorSID=e.[SID]
where YearQuarterID>='B013'






GO

GRANT SELECT ON  [dbo].[vw_ClassSearch] TO [WebApplicationUser]
GO
