IF NOT EXISTS (SELECT * FROM master.dbo.syslogins WHERE loginname = N'ClassSchedule_WebUser')
CREATE LOGIN [ClassSchedule_WebUser] WITH PASSWORD = 'p@ssw0rd'
GO
CREATE USER [ClassSchedule_WebUser] FOR LOGIN [ClassSchedule_WebUser]
GO
