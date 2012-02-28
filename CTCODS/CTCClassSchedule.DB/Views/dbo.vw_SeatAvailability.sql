SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO





CREATE view [dbo].[vw_SeatAvailability]
AS

select
s.ClassID
/*
,CASE
	WHEN isNULL(c.LastUpdated, DateAdd(day, -1, getdate()))>isNULL(s.LastUpdated, DATEADD(day, -2, getdate())) THEN c.SeatsAvailable
	ELSE s.SeatsAvailable
END as SeatsAvailable
*/
,s.SeatsAvailable
/*
,CASE
	WHEN isNULL(c.LastUpdated, DateAdd(day, -1, getdate()))>isNULL(s.LastUpdated, DATEADD(day, -2, getdate())) THEN c.LastUpdated
	ELSE s.LastUpdated
END as LastUpdated
*/
,s.LastUpdated
from SeatAvailability s
--right outer join vw_Class c on c.ClassID = s.ClassID

--where Right(c.ClassID, 4) = 'B122'





GO
GRANT SELECT ON  [dbo].[vw_SeatAvailability] TO [WebApplicationUser]
GO
