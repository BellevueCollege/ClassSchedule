SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE procedure [dbo].[usp_CourseSearch]
(
	@SearchWord nvarchar(100)
	,@YearQuarterID char(4)
)

AS
/* DEBUG
DECLARE @SearchWord nvarchar(50), @YearQuarterID char(4)
set @SearchWord = 'race'
set @YearQuarterID = 'B122'
--*/


declare @SearchString nvarchar(100) --preserve original string typed in by the user
set @SearchString = @SearchWord

set @SearchWord = RTRIM(@SearchWord)
SET @SearchWord = N'("' + REPLACE(@SearchWord, ' ', '*" AND "') + '*")'
--select @SearchWord


select
CourseID
,0 as SearchRank
into #Results
from CourseSearch
where 1<>1

insert #Results
select FT_TBL.CourseID, KEY_TBL.RANK
from CourseSearch AS FT_TBL
join CONTAINSTABLE(CourseSearch, CourseInfo,@SearchWord) AS KEY_TBL
            ON FT_TBL.CourseKey = KEY_TBL.[KEY]
where YearQuarterID = @YearQuarterID
ORDER BY KEY_TBL.RANK DESC;



select
--c.*
r.CourseID
,r.SearchRank
,ISNULL(c.CourseTitle2, c.CourseTitle) as CourseTitle
,c.Credits
,ISNULL(c.CourseDescription, c.CourseDescription2) as CourseDescription
from #Results r
left outer join vw_CourseSearch c on r.CourseID = c.CourseID and c.YearQuarterID=@YearQuarterID
order by r.SearchRank DESC, r.CourseID

drop table #Results
GO
GRANT EXECUTE ON  [dbo].[usp_CourseSearch] TO [WebApplicationUser]
GO
