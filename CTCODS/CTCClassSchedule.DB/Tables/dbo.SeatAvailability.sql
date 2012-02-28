CREATE TABLE [dbo].[SeatAvailability]
(
[ClassID] [char] (8) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[SeatsAvailable] [int] NULL,
[LastUpdated] [datetime] NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[SeatAvailability] ADD CONSTRAINT [PK_SeatAvailibility] PRIMARY KEY CLUSTERED  ([ClassID]) ON [PRIMARY]
GO
GRANT SELECT ON  [dbo].[SeatAvailability] TO [WebApplicationUser]
GRANT INSERT ON  [dbo].[SeatAvailability] TO [WebApplicationUser]
GRANT UPDATE ON  [dbo].[SeatAvailability] TO [WebApplicationUser]
GO
