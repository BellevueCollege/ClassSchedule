SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE procedure [dbo].[usp_PopulateClassSearchTables]
AS

truncate table CourseSearch
truncate table ClassSearch

insert CourseSearch (
CourseKey
,YearQuarterID
,CourseID
,CourseInfo
)
select
CourseID+YearQuarterID
,YearQuarterID
,CourseID
,CourseID + ' ' + isNULL(CourseTitle2, '') + ' ' + isNULL(CourseDescription, '') + ' ' + isNULL(CourseDescription2, '')
from vw_CourseSearch


insert ClassSearch
(
ClassID
,SearchGroup1
,SearchGroup2
,SearchGroup3
)
select
ClassID
,isNULL(RTrim(Left(CourseID, 5)), '') + ' ' + ISNULL(Replace(CourseID, Left(CourseID,5), ''), '') + ' ' +  -- Remove extra spaces from CourseID, i.e. convert "ART  101" to "ART 101"
isNULL(RTrim(Left(CourseID, 5)), '') + ISNULL(Replace(CourseID, Left(CourseID,5), ''), '') + ' ' + --a version of CourseID without spaces
isNULL(ItemNumber, '') + ' ' + isNULL(CourseTitle, '') + ' ' + isNULL(CourseSubject, '') as SearchGroup1
,ISNULL(InstructorName, '') + ' ' + ISNULL(CourseDescription, '') as SearchGroup2
,ISNULL(Left(Footnotes, 264) , '') as SearchGroup3
from vw_ClassSearch
--where YearQuarterID >='B011'

update ClassSearch
set SearchGroup4=SearchGroup1+ ' ' + SearchGroup2 + ' ' + SearchGroup3
from ClassSearch s
where ClassID = s.ClassID


GO
