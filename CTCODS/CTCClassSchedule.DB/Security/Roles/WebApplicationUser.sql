CREATE ROLE [WebApplicationUser]
AUTHORIZATION [dbo]
EXEC sp_addrolemember N'WebApplicationUser', N'CAMPUS\N216-E027543$'

EXEC sp_addrolemember N'WebApplicationUser', N'CAMPUS\SSOUTHDT-N216K$'

EXEC sp_addrolemember N'WebApplicationUser', N'campus\w-bc-classes'

EXEC sp_addrolemember N'WebApplicationUser', N'campus\w-bc-classesbeta'

EXEC sp_addrolemember N'WebApplicationUser', N'CAMPUS\w-in-ws-canvassync'

EXEC sp_addrolemember N'WebApplicationUser', N'CAMPUS\w-in-ws-canvassyncqa'

EXEC sp_addrolemember N'WebApplicationUser', N'CAMPUS\maricel.medina'

EXEC sp_addrolemember N'WebApplicationUser', N'CAMPUS\N216J-E027168$'





GO

EXEC sp_addrolemember N'WebApplicationUser', N'campus\julloa'
GO

EXEC sp_addrolemember N'WebApplicationUser', N'ClassSchedule_WebUser'
GO
