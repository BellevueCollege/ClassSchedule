CREATE TABLE [dbo].[SubjectsCoursePrefixes]
(
[CoursePrefixID] [varchar] (5) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[SubjectID] [int] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[SubjectsCoursePrefixes] ADD CONSTRAINT [PK_SubjectsCoursePrefixes_1] PRIMARY KEY CLUSTERED  ([CoursePrefixID]) ON [PRIMARY]
GO
ALTER TABLE [dbo].[SubjectsCoursePrefixes] ADD CONSTRAINT [FK_SubjectsCoursePrefixes_Subjects] FOREIGN KEY ([SubjectID]) REFERENCES [dbo].[Subjects] ([SubjectID])
GO
GRANT SELECT ON  [dbo].[SubjectsCoursePrefixes] TO [WebApplicationUser]
GRANT INSERT ON  [dbo].[SubjectsCoursePrefixes] TO [WebApplicationUser]
GRANT DELETE ON  [dbo].[SubjectsCoursePrefixes] TO [WebApplicationUser]
GRANT UPDATE ON  [dbo].[SubjectsCoursePrefixes] TO [WebApplicationUser]
GO
