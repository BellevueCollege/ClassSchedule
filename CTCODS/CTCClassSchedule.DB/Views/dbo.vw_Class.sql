
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

CREATE view [dbo].[vw_Class]
AS
SELECT c.[ClassID]
      ,c.[YearQuarterID]
      ,c.[ItemNumber]
      ,c.[CourseID]
      ,c.[Department]
      ,c.[CourseNumber]
-- NOTE: The capacity/enrollment logic only works for B013 (Spring 2011) and later.
-- Previous to that, associated data in the cluster table is NULL. - shawn.south@bellevuecollege.edu
      ,CASE
		WHEN NOT c.[ClusterItemNumber] IS NULL
			THEN l.ClusterCapacity
			ELSE c.[ClassCapacity]
		END AS ClassCapacity
      ,CASE
		WHEN NOT c.[ClusterItemNumber] IS NULL
			THEN l.ClusterEnrolled
			ELSE c.[StudentsEnrolled]
		END AS StudentsEnrolled
/* COMMENT THIS LINE FOR DEBUGGING
	  ,c.ClusterItemNumber AS "(ClusterItemNumber)"
	  ,c.ClassCapacity AS "(ClassCapacity)"
	  ,c.StudentsEnrolled AS "(StudentsEnrolled)"
	  ,l.ClusterCapacity AS "(ClusterCapacity)"
	  ,l.ClusterEnrolled AS "(ClusterEnrolled)"
--*/
  FROM [ODS].[dbo].[vw_Class] c
  LEFT JOIN [ODS].[dbo].[vw_ClassCluster] l ON l.ClusterItemNumber = c.ClusterItemNumber AND l.YearQuarterID = c.YearQuarterID
/* COMMENT THIS LINE FOR DEBUGGING
  WHERE NOT c.ClusterItemNumber IS NULL
  AND c.YearQuarterID >= 'B012'
  ORDER BY c.YearQuarterID
--*/
GO

GRANT SELECT ON  [dbo].[vw_Class] TO [WebApplicationUser]
GO
