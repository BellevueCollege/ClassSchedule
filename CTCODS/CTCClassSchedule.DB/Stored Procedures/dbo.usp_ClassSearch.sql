
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO


CREATE procedure [dbo].[usp_ClassSearch]
(
	@SearchWord nvarchar(100)
	,@YearQuarterID char(4)
)

AS
/*
DECLARE @SearchWord nvarchar(100), @YearQuarterID char(4)
SELECT @SearchWord = 'deved', @YearQuarterID = 'B124'
--*/

declare @SearchString nvarchar(100) --preserve original string typed in by the user
set @SearchString = @SearchWord

set @SearchWord = RTRIM(@SearchWord)
SET @SearchWord = N'("' + REPLACE(@SearchWord, ' ', '*" AND "') + '*")'


/*
select @SearchWord

select
*
from
ClassSearch
where ClassID='0640B121'
where CONTAINS((SearchGroup1, SearchGroup2, SearchGroup3), @SearchWord)
--*/

select
ClassID
,0 as SearchGroup
,0 as SearchRank
into #Results
from ClassSearch
where 1<>1

insert #Results
select FT_TBL.ClassID, 1, KEY_TBL.RANK
from ClassSearch AS FT_TBL
join CONTAINSTABLE(ClassSearch, SearchGroup1,@SearchWord) AS KEY_TBL
            ON FT_TBL.ClassID = KEY_TBL.[KEY]
where RIGHT(ClassID, 4) = @YearQuarterID
ORDER BY KEY_TBL.RANK DESC;

insert into #Results
select FT_TBL.ClassID, 2, KEY_TBL.RANK
from ClassSearch AS FT_TBL
join CONTAINSTABLE(ClassSearch, SearchGroup2,@SearchWord) AS KEY_TBL
            ON FT_TBL.ClassID = KEY_TBL.[KEY]
where
RIGHT(ClassID, 4) = @YearQuarterID
and FT_TBL.ClassID not in (select ClassID from #Results)
ORDER BY KEY_TBL.RANK DESC;

insert into #Results
select FT_TBL.ClassID, 3, KEY_TBL.RANK
from ClassSearch AS FT_TBL
join CONTAINSTABLE(ClassSearch, SearchGroup3,@SearchWord) AS KEY_TBL
            ON FT_TBL.ClassID = KEY_TBL.[KEY]
where
RIGHT(ClassID, 4) = @YearQuarterID
and FT_TBL.ClassID not in (select ClassID from #Results)
ORDER BY KEY_TBL.RANK DESC;

insert into #Results
select FT_TBL.ClassID, 4, KEY_TBL.RANK
from ClassSearch AS FT_TBL
join CONTAINSTABLE(ClassSearch, SearchGroup4,@SearchWord) AS KEY_TBL
            ON FT_TBL.ClassID = KEY_TBL.[KEY]
where
RIGHT(ClassID, 4) = @YearQuarterID
and FT_TBL.ClassID not in (select ClassID from #Results)
ORDER BY KEY_TBL.RANK DESC;


insert into #Results
SELECT FT_TBL.ClassID, 0, 0
FROM ClassSearch AS FT_TBL
WHERE RIGHT(ClassID, 4) = @YearQuarterID
AND ItemYrqLink IN (SELECT LEFT(ClassID, 4) FROM #Results)
and FT_TBL.ClassID not in (select ClassID from #Results)



insert SearchLog (SearchString,Group1Results,Group2Results,Group3Results)
select
@SearchString as SearchString
,(select COUNT(*) from #Results where SearchGroup=1) as Group1Results
,(select COUNT(*) from #Results where SearchGroup=2) as Group2Results
,(select COUNT(*) from #Results where SearchGroup=3) as Group3Results


select
--c.*
r.ClassID
,r.SearchGroup
,r.SearchRank
from #Results r
--left outer join vw_ClassSearch c on r.ClassID = c.ClassID
--order by r.SearchGroup, r.SearchRank DESC

drop table #Results

GO


GRANT EXECUTE ON  [dbo].[usp_ClassSearch] TO [WebApplicationUser]
GO
