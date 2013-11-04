CREATE TABLE [dbo].[CourseSearch]
(
[CourseKey] [varchar] (14) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[YearQuarterID] [char] (4) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[CourseID] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[CourseInfo] [varchar] (4065) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY]
GO
GRANT SELECT ON  [dbo].[CourseSearch] TO [WebApplicationUser]
GO

ALTER TABLE [dbo].[CourseSearch] ADD CONSTRAINT [PK_CourseSearch] PRIMARY KEY CLUSTERED  ([CourseKey]) ON [PRIMARY]
GO
CREATE FULLTEXT INDEX ON [dbo].[CourseSearch] KEY INDEX [PK_CourseSearch] ON [IX_ClassSearch]
GO
ALTER FULLTEXT INDEX ON [dbo].[CourseSearch] ADD ([CourseInfo] LANGUAGE 1033)
GO
