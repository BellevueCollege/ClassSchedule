SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

CREATE view [dbo].[vw_CourseSearch]
AS

select
c.CourseID
,y.YearQuarterID
,c.CourseTitle
,c.CourseTitle2
,c.Credits
--,c.EffectiveYearQuarterBegin
--,c.EffectiveYearQuarterEnd
,(select top 1 [Description] from ODS.dbo.vw_CourseDescription where CourseID=c.CourseID order by EffectiveYearQuarterBegin DESC) as CourseDescription
,(select top 1 [Description] from ODS.dbo.vw_CourseDescription2 where CourseID=c.CourseID order by EffectiveYearQuarterBegin DESC) as CourseDescription2
from ODS.dbo.vw_Course c
left outer join ODS.dbo.vw_YearQuarter y on y.YearQuarterID between c.EffectiveYearQuarterBegin and c.EffectiveYearQuarterEnd
left outer join ODS.dbo.vw_Class cl on c.CourseID = cl.CourseID and cl.YearQuarterID = y.YearQuarterID
where
cl.ClassID is NULL
and y.YearQuarterID <> 'Z999'
and y.YearQuarterID >= 'B013'
--and c.CourseID='ART  154'



GO
GRANT SELECT ON  [dbo].[vw_CourseSearch] TO [WebApplicationUser]
GO
