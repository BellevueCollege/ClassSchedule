IF NOT EXISTS (SELECT * FROM master.dbo.syslogins WHERE loginname = N'CAMPUS\andrew.craswell')
CREATE LOGIN [CAMPUS\andrew.craswell] FROM WINDOWS
GO
CREATE USER [CAMPUS\andrew.craswell] FOR LOGIN [CAMPUS\andrew.craswell]
GO
