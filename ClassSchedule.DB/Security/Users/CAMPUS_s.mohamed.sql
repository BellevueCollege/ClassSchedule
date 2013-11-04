IF NOT EXISTS (SELECT * FROM master.dbo.syslogins WHERE loginname = N'CAMPUS\s.mohamed')
CREATE LOGIN [CAMPUS\s.mohamed] FROM WINDOWS
GO
CREATE USER [CAMPUS\s.mohamed] FOR LOGIN [CAMPUS\s.mohamed]
GO
