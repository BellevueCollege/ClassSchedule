IF NOT EXISTS (SELECT * FROM master.dbo.syslogins WHERE loginname = N'CAMPUS\julloa')
CREATE LOGIN [CAMPUS\julloa] FROM WINDOWS
GO
CREATE USER [campus\julloa] FOR LOGIN [CAMPUS\julloa]
GO
