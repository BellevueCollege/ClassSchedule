CREATE TABLE [dbo].[Divisions]
(
[DivisionID] [int] NOT NULL IDENTITY(1, 1),
[Title] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[URL] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastUpdatedBy] [varchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastUpdated] [datetime] NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Divisions] ADD CONSTRAINT [PK_Divisions] PRIMARY KEY CLUSTERED  ([DivisionID]) ON [PRIMARY]
GO
GRANT SELECT ON  [dbo].[Divisions] TO [WebApplicationUser]
GRANT INSERT ON  [dbo].[Divisions] TO [WebApplicationUser]
GRANT UPDATE ON  [dbo].[Divisions] TO [WebApplicationUser]
GO
