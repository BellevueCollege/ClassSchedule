CREATE TABLE [dbo].[ClassSearch]
(
[ClassID] [char] (8) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[SearchGroup1] [varchar] (200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[SearchGroup2] [varchar] (4065) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[SearchGroup3] [varchar] (265) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[SearchGroup4] [varchar] (4411) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ClassSearch] ADD CONSTRAINT [PK_ClassSearch] PRIMARY KEY CLUSTERED  ([ClassID]) ON [PRIMARY]
GO
CREATE FULLTEXT INDEX ON [dbo].[ClassSearch] KEY INDEX [PK_ClassSearch] ON [IX_ClassSearch]
GO
ALTER FULLTEXT INDEX ON [dbo].[ClassSearch] ADD ([SearchGroup1] LANGUAGE 1033)
GO
ALTER FULLTEXT INDEX ON [dbo].[ClassSearch] ADD ([SearchGroup2] LANGUAGE 1033)
GO
ALTER FULLTEXT INDEX ON [dbo].[ClassSearch] ADD ([SearchGroup3] LANGUAGE 1033)
GO
ALTER FULLTEXT INDEX ON [dbo].[ClassSearch] ADD ([SearchGroup4] LANGUAGE 1033)
GO
