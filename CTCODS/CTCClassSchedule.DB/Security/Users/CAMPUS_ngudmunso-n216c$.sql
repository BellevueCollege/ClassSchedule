IF NOT EXISTS (SELECT * FROM master.dbo.syslogins WHERE loginname = N'CAMPUS\ngudmunso-n216c$')
CREATE LOGIN [CAMPUS\ngudmunso-n216c$] FROM WINDOWS
GO
CREATE USER [CAMPUS\ngudmunso-n216c$] FOR LOGIN [CAMPUS\ngudmunso-n216c$]
GO
