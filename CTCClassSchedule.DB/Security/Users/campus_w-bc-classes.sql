IF NOT EXISTS (SELECT * FROM master.dbo.syslogins WHERE loginname = N'CAMPUS\w-bc-classes')
CREATE LOGIN [CAMPUS\w-bc-classes] FROM WINDOWS
GO
CREATE USER [campus\w-bc-classes] FOR LOGIN [CAMPUS\w-bc-classes]
GO
