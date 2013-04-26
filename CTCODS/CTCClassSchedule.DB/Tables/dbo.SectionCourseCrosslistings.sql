CREATE TABLE [dbo].[SectionCourseCrosslistings]
(
[ClassID] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[CourseID] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[SectionCourseCrosslistings] ADD CONSTRAINT [PK_CourseSections] PRIMARY KEY CLUSTERED  ([ClassID], [CourseID]) ON [PRIMARY]
GO
GRANT SELECT ON  [dbo].[SectionCourseCrosslistings] TO [WebApplicationUser]
GRANT INSERT ON  [dbo].[SectionCourseCrosslistings] TO [WebApplicationUser]
GRANT DELETE ON  [dbo].[SectionCourseCrosslistings] TO [WebApplicationUser]
GRANT UPDATE ON  [dbo].[SectionCourseCrosslistings] TO [WebApplicationUser]
GO
