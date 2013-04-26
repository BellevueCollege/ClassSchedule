IF NOT EXISTS (SELECT * FROM master.dbo.syslogins WHERE loginname = N'CAMPUS\w-in-ws-canvassyncqa')
CREATE LOGIN [CAMPUS\w-in-ws-canvassyncqa] FROM WINDOWS
GO
CREATE USER [CAMPUS\w-in-ws-canvassyncqa] FOR LOGIN [CAMPUS\w-in-ws-canvassyncqa]
GO
