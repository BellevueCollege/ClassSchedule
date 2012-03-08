CREATE TABLE [dbo].[SearchLog]
(
[SearchString] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Group1Results] [int] NULL,
[Group2Results] [int] NULL,
[Group3Results] [int] NULL,
[DateStamp] [datetime] NULL CONSTRAINT [DF_SearchLog_DateStamp] DEFAULT (getdate())
) ON [PRIMARY]
GO
