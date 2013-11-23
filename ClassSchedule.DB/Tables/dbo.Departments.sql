CREATE TABLE [dbo].[Departments]
(
[DepartmentID] [int] NOT NULL IDENTITY(1, 1),
[DivisionID] [int] NULL,
[Title] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[URL] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ProgramChairSID] [varchar] (9) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastUpdatedBy] [varchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastUpdated] [datetime] NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Departments] ADD CONSTRAINT [PK_Departments] PRIMARY KEY CLUSTERED  ([DepartmentID]) ON [PRIMARY]
GO

GRANT SELECT ON  [dbo].[Departments] TO [WebApplicationUser]
GRANT INSERT ON  [dbo].[Departments] TO [WebApplicationUser]
GRANT UPDATE ON  [dbo].[Departments] TO [WebApplicationUser]
GO
