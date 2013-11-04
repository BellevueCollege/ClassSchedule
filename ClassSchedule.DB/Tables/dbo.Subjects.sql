CREATE TABLE [dbo].[Subjects]
(
[SubjectID] [int] NOT NULL IDENTITY(1, 1),
[DepartmentID] [int] NULL,
[Title] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Intro] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Slug] [varchar] (63) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[LastUpdatedBy] [varchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastUpdated] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[Subjects] ADD CONSTRAINT [PK_Subjects] PRIMARY KEY CLUSTERED  ([SubjectID]) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Subjects] ADD CONSTRAINT [UQ_Subjects_Slug] UNIQUE NONCLUSTERED  ([Slug]) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Subjects] ADD CONSTRAINT [FK_Subjects_Departments] FOREIGN KEY ([DepartmentID]) REFERENCES [dbo].[Departments] ([DepartmentID])
GO
GRANT SELECT ON  [dbo].[Subjects] TO [WebApplicationUser]
GRANT INSERT ON  [dbo].[Subjects] TO [WebApplicationUser]
GRANT UPDATE ON  [dbo].[Subjects] TO [WebApplicationUser]
GO
