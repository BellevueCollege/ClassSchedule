/* Bellevue College's query for identifying
	- Instructors
	- Days
	- Times
	- Locations
*/

select distinct *
from (
	select
		c.ClassID
		,c.DayID
		,c.StartTime
		,c.EndTime
		,c.InstructorSID
		,c.InstructorName
		,c.Room
	from vw_Class c
	where not c.Room is null
	and not c.StartTime is null
	and not c.EndTime is null
	and not c.DayID is null
	and c.SectionStatusID1 not like '%Z%'
	and c.SectionStatusID1 not like '%X%'
	and right(c.ClassID, 4) = 'B014'
	union
	select
	--	i.InstructionID
		i.ClassID
		,i.DayID
		,i.StartTime
		,i.EndTime
		,i.InstructorSID
		,i.InstructorName
		,i.Room
	from vw_Instruction i
	where not i.Room is null
	and not i.StartTime is null
	and not i.EndTime is null
	and not i.DayID is null
	and right(i.ClassID, 4) = 'B014'
	--order by right(i.ClassID, 4) desc
) a
