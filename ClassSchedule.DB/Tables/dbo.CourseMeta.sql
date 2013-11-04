CREATE TABLE [dbo].[CourseMeta]
(
[CourseID] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Footnote] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastUpdatedBy] [varchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastUpdated] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[CourseMeta] ADD CONSTRAINT [PK_Courses] PRIMARY KEY CLUSTERED  ([CourseID]) ON [PRIMARY]
GO
GRANT SELECT ON  [dbo].[CourseMeta] TO [WebApplicationUser]
GRANT INSERT ON  [dbo].[CourseMeta] TO [WebApplicationUser]
GRANT UPDATE ON  [dbo].[CourseMeta] TO [WebApplicationUser]
GO
