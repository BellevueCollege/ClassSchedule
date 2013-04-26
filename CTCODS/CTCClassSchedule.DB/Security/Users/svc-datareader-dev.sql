IF NOT EXISTS (SELECT * FROM master.dbo.syslogins WHERE loginname = N'svc-datareader-dev')
CREATE LOGIN [svc-datareader-dev] WITH PASSWORD = 'p@ssw0rd'
GO
CREATE USER [svc-datareader-dev] FOR LOGIN [svc-datareader-dev]
GO
