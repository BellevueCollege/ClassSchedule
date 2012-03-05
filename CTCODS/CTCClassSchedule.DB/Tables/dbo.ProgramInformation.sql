CREATE TABLE [dbo].[ProgramInformation]
(
[Abbreviation] [varchar] (5) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[URL] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ProgramURL] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Title] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Division] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ContactName] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ContactPhone] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Intro] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[AcademicProgram] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[DivisionURL] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastUpdated] [datetime] NULL,
[LastUpdatedBy] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
GRANT SELECT ON  [dbo].[ProgramInformation] TO [WebApplicationUser]
GRANT INSERT ON  [dbo].[ProgramInformation] TO [WebApplicationUser]
GRANT UPDATE ON  [dbo].[ProgramInformation] TO [WebApplicationUser]
GO

ALTER TABLE [dbo].[ProgramInformation] ADD CONSTRAINT [PK_Sheet1$] PRIMARY KEY CLUSTERED  ([Abbreviation]) ON [PRIMARY]
GO
