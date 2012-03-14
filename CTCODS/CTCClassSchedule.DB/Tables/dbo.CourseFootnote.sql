CREATE TABLE [dbo].[CourseFootnote]
(
[CourseID] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Footnote] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastUpdatedBy] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastUpdated] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
GRANT SELECT ON  [dbo].[CourseFootnote] TO [WebApplicationUser]
GRANT INSERT ON  [dbo].[CourseFootnote] TO [WebApplicationUser]
GRANT UPDATE ON  [dbo].[CourseFootnote] TO [WebApplicationUser]
GO

ALTER TABLE [dbo].[CourseFootnote] ADD CONSTRAINT [PK_CourseFootnote] PRIMARY KEY CLUSTERED  ([CourseID]) ON [PRIMARY]
GO
