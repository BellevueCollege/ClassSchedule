IF NOT EXISTS (SELECT * FROM master.dbo.syslogins WHERE loginname = N'CAMPUS\tiis-schedulebetaqa')
CREATE LOGIN [CAMPUS\tiis-schedulebetaqa] FROM WINDOWS
GO
CREATE USER [CAMPUS\tiis-schedulebetaqa] FOR LOGIN [CAMPUS\tiis-schedulebetaqa]
GO
