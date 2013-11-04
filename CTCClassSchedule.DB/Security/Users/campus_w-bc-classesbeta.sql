IF NOT EXISTS (SELECT * FROM master.dbo.syslogins WHERE loginname = N'campus\w-bc-classesbeta')
CREATE LOGIN [campus\w-bc-classesbeta] FROM WINDOWS
GO
CREATE USER [campus\w-bc-classesbeta] FOR LOGIN [campus\w-bc-classesbeta]
GO
