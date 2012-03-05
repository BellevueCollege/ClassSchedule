
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

CREATE view [dbo].[vw_ProgramInformation]
AS
SELECT Replace([Abbreviation], '&', '') as AbbreviationTrimmed
	,[Abbreviation]
      ,[URL]
      ,[ProgramURL]
      ,[Title]
      ,[Division]
      ,[ContactName]
      ,[ContactPhone]
      ,[Intro]
      ,AcademicProgram
      ,DivisionURL
      ,LastUpdated
      ,LastUpdatedBy
  FROM [ProgramInformation]

GO

GRANT SELECT ON  [dbo].[vw_ProgramInformation] TO [WebApplicationUser]
GO
