/*
INSERT INTO dbo.SectionSeats
        ( ClassID ,
          SeatsAvailable ,
          LastUpdated
        )
--*/

--delete from dbo.SectionSeats

SELECT
	--c.ClassID
	--,COUNT(c.LastUpdated)
	c.ClassID
	,c.SeatsAvailable
	,c.LastUpdated
FROM [MSSQL-D01\TESTMSSQL2008].[ClassSchedule].[dbo].SeatAvailability c
--GROUP BY c.ClassID
--ORDER BY COUNT(c.ClassID) desc

--SELECT * FROM dbo.SectionSeats
