CREATE TABLE [dbo].[SectionFootnote]
(
[ClassID] [char] (8) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Footnote] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastUpdatedBy] [nchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastUpdated] [datetime] NULL,
[CustomTitle] [varchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[CustomDescription] [varchar] (500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
GRANT SELECT ON  [dbo].[SectionFootnote] TO [ClassSchedule_WebUser]
GRANT UPDATE ON  [dbo].[SectionFootnote] TO [ClassSchedule_WebUser]
GRANT SELECT ON  [dbo].[SectionFootnote] TO [WebApplicationUser]
GRANT INSERT ON  [dbo].[SectionFootnote] TO [WebApplicationUser]
GRANT UPDATE ON  [dbo].[SectionFootnote] TO [WebApplicationUser]
GO

ALTER TABLE [dbo].[SectionFootnote] ADD CONSTRAINT [PK_SectionFootnote] PRIMARY KEY CLUSTERED  ([ClassID]) ON [PRIMARY]
GO
