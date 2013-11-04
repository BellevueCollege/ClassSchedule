
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

/*
Procedure will perform either udpate or an insert of data depending on
whether a particular section information already exists in the table
*/
CREATE procedure [dbo].[usp_UPSERT_SeatsAvailable]
(
	@ClassID char(8)
	,@SeatsAvailable int
)
AS

if (select COUNT(*) from dbo.SectionSeats where ClassID=@ClassID)>0
	update dbo.SectionSeats set
	SeatsAvailable=@SeatsAvailable
	,LastUpdated=GETDATE()
	where ClassID=@ClassID
else
	insert SectionSeats
	select @ClassID, @SeatsAvailable, GETDATE()
GO

GRANT EXECUTE ON  [dbo].[usp_UPSERT_SeatsAvailable] TO [WebApplicationUser]
GO
