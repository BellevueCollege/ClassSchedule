IF NOT EXISTS (SELECT * FROM master.dbo.syslogins WHERE loginname = N'CAMPUS\maricel.medina')
CREATE LOGIN [CAMPUS\maricel.medina] FROM WINDOWS
GO
CREATE USER [CAMPUS\maricel.medina] FOR LOGIN [CAMPUS\maricel.medina]
GO
