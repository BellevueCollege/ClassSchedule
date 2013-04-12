/*-----------------------------------------------------------------------------------------------------------
  Post-database upgrade clean-up script.

              *****************************************************************************
              ** WARNING                                                                 **
              ** ----------------------------------------------------------------------- **
              ** ONLY run this script AFTER the following steps have been completed:     **
              **                                                                         **
              **  1)  The database upgrade script has been successfully run.             **
              **                                                                         **
              **  2)  A "sanity test" has been completed to ensure the upgrade was       **
              **      successul.                                                         **
              **                                                                         **
              *****************************************************************************

  After this script has been run, you should perform ANOTHER sanity test to make sure removal of these
  tables hasn't broken anything unexpectedly.

 -----------------------------------------------------------------------------------------------------------*/
-- Sanity check that we're in an upgraded ClassSchedule database before starting.
IF NOT EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[dbo].[SectionsMeta]') OR object_id = OBJECT_ID(N'[dbo].[Divisions]'))
BEGIN
	RAISERROR('Unable to find new tables. Please ensure you are in the ClassSchedule database and that the upgrade script has completed successfully.', 18, 0)
	-- do not execute the remaining script (NOTE: needs to be turned back OFF later)
	-- see http://stackoverflow.com/questions/659188/sql-server-stop-or-break-execution-of-a-sql-script
	SET NOEXEC ON
END


/*-----------------------------------------------------------------------------------------------------------
  Remove tables that are obsolete now that the database schema has been upgraded.
 -----------------------------------------------------------------------------------------------------------*/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CourseFootnote]') AND type in (N'U'))
DROP TABLE [dbo].[CourseFootnote]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProgramInformation]') AND type in (N'U'))
DROP TABLE [dbo].[ProgramInformation]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SeatAvailability]') AND type in (N'U'))
DROP TABLE [dbo].[SeatAvailability]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SectionFootnote]') AND type in (N'U'))
DROP TABLE [dbo].[SectionFootnote]
GO

/*-----------------------------------------------------------------------------------------------------------
  Remove obsolete views.
 -----------------------------------------------------------------------------------------------------------*/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[vw_ClassScheduleData]') AND type in (N'V'))
DROP VIEW [dbo].[vw_ClassScheduleData]
GO


/*-----------------------------------------------------------------------------------------------------------
  Clean-up, etc.
 -----------------------------------------------------------------------------------------------------------*/

-- re-enable execution of script
-- see http://stackoverflow.com/questions/659188/sql-server-stop-or-break-execution-of-a-sql-script
SET NOEXEC OFF
