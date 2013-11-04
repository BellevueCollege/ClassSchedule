CREATE TABLE [dbo].[SectionsMeta]
(
[ClassID] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Footnote] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Title] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Description] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastUpdatedBy] [varchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastUpdated] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[SectionsMeta] ADD CONSTRAINT [PK_Sections] PRIMARY KEY CLUSTERED  ([ClassID]) ON [PRIMARY]
GO
GRANT SELECT ON  [dbo].[SectionsMeta] TO [WebApplicationUser]
GRANT INSERT ON  [dbo].[SectionsMeta] TO [WebApplicationUser]
GRANT UPDATE ON  [dbo].[SectionsMeta] TO [WebApplicationUser]
GO
