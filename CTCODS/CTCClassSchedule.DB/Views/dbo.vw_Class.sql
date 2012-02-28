SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO




CREATE view [dbo].[vw_Class]
AS
SELECT [ClassID]
      ,[YearQuarterID]
      ,[ItemNumber]
      ,[CourseID]
      ,[Department]
      ,[CourseNumber]
      ,[ClassCapacity]
      ,[StudentsEnrolled]
      /*
	,CASE
		WHEN ClassCapacity - StudentsEnrolled < 0 then 0
		ELSE ClassCapacity - StudentsEnrolled
	END as SeatsAvailable
	*/
	  --,Cast('11/1/2011 11:00:000 AM' as datetime) as LastUpdated --u.[UpdateDate] as LastUpdated
  FROM [ODS].[dbo].[vw_Class] c
  /*
	left outer join
		(
		select top 1 UpdateDate
		from ODS.dbo.vw_TableTransferLog
		where ODSTableName='Class'
		order by UpdateDate DESC) u on 1=1
		*/




GO
GRANT SELECT ON  [dbo].[vw_Class] TO [WebApplicationUser]
GO
