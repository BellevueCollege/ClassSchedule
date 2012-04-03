IF NOT EXISTS (SELECT * FROM master.dbo.syslogins WHERE loginname = N'CAMPUS\elasater')
CREATE LOGIN [CAMPUS\elasater] FROM WINDOWS
GO
CREATE USER [CAMPUS\elasater] FOR LOGIN [CAMPUS\elasater]
GO
