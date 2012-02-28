SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

/*where Right(c.ClassID, 4)='B122'*/
CREATE VIEW [dbo].[vw_ClassScheduleData]
AS
SELECT     c.ClassID, c.YearQuarterID, c.CourseID, CASE WHEN isNULL(u.LastUpdated, DateAdd(day, - 1, getdate())) > isNULL(sa.LastUpdated, DATEADD(day, - 2, getdate()))
                      THEN CASE WHEN c.ClassCapacity - c.StudentsEnrolled < 0 THEN 0 ELSE c.ClassCapacity - c.StudentsEnrolled END ELSE sa.SeatsAvailable END AS SeatsAvailable,
                      CASE WHEN isNULL(u.LastUpdated, DateAdd(day, - 1, getdate())) > isNULL(sa.LastUpdated, DATEADD(day, - 2, getdate()))
                      THEN u.LastUpdated ELSE sa.LastUpdated END AS LastUpdated, cf.Footnote AS CourseFootnote, sf.Footnote AS SectionFootnote, p.Abbreviation, p.ProgramURL,
                      p.Title, p.URL, p.Division, p.ContactName, p.ContactPhone, p.Intro, sf.CustomTitle, sf.CustomDescription
FROM         dbo.vw_Class AS c LEFT OUTER JOIN
                      dbo.vw_SeatAvailability AS sa ON c.ClassID = sa.ClassID LEFT OUTER JOIN
                      dbo.SectionFootnote AS sf ON c.ClassID = sf.ClassID LEFT OUTER JOIN
                      dbo.CourseFootnote AS cf ON c.CourseID = cf.CourseID LEFT OUTER JOIN
                      dbo.ProgramInformation AS p ON c.Department = p.Abbreviation LEFT OUTER JOIN
                          (SELECT     TOP (1) UpdateDate AS LastUpdated
                            FROM          ODS.dbo.vw_TableTransferLog
                            WHERE      (ODSTableName = 'Class')
                            ORDER BY LastUpdated DESC) AS u ON 1 = 1

GO
GRANT SELECT ON  [dbo].[vw_ClassScheduleData] TO [WebApplicationUser]
GO
EXEC sp_addextendedproperty N'MS_DiagramPane1', N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties =
   Begin PaneConfigurations =
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane =
      Begin Origin =
         Top = 0
         Left = 0
      End
      Begin Tables =
         Begin Table = "c"
            Begin Extent =
               Top = 6
               Left = 38
               Bottom = 125
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "sa"
            Begin Extent =
               Top = 6
               Left = 246
               Bottom = 110
               Right = 406
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "sf"
            Begin Extent =
               Top = 114
               Left = 246
               Bottom = 233
               Right = 424
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "cf"
            Begin Extent =
               Top = 126
               Left = 38
               Bottom = 245
               Right = 200
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "p"
            Begin Extent =
               Top = 234
               Left = 238
               Bottom = 353
               Right = 412
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "u"
            Begin Extent =
               Top = 6
               Left = 444
               Bottom = 80
               Right = 604
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane =
   End
   Begin DataPane =
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane =
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
     ', 'SCHEMA', N'dbo', 'VIEW', N'vw_ClassScheduleData', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_DiagramPane2', N'    Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', 'SCHEMA', N'dbo', 'VIEW', N'vw_ClassScheduleData', NULL, NULL
GO
DECLARE @xp int
SELECT @xp=2
EXEC sp_addextendedproperty N'MS_DiagramPaneCount', @xp, 'SCHEMA', N'dbo', 'VIEW', N'vw_ClassScheduleData', NULL, NULL
GO
