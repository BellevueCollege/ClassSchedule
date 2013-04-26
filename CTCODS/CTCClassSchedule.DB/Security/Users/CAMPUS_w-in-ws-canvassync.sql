IF NOT EXISTS (SELECT * FROM master.dbo.syslogins WHERE loginname = N'CAMPUS\w-in-ws-canvassync')
CREATE LOGIN [CAMPUS\w-in-ws-canvassync] FROM WINDOWS
GO
CREATE USER [CAMPUS\w-in-ws-canvassync] FOR LOGIN [CAMPUS\w-in-ws-canvassync]
GO
