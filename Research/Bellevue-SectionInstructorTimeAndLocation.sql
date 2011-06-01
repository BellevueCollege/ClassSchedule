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
		,d.Title
		,c.StartTime
		,c.EndTime
		,c.InstructorSID
		,c.InstructorName
		,c.Room
	from vw_Class c
	join vw_Day d on c.DayID = d.DayID
	where c.SectionStatusID1 not like '%Z%'
	and c.SectionStatusID1 not like '%X%'
	union
	select
	--	i.InstructionID
		i.ClassID
		,d.Title
		,i.StartTime
		,i.EndTime
		,i.InstructorSID
		,i.InstructorName
		,i.Room
	from vw_Instruction i
	join vw_Day d on i.DayID = d.DayID
	where not i.Room is null
	and not i.StartTime is null
	and not i.EndTime is null
	and not i.DayID is null
	--order by right(i.ClassID, 4) desc
) a
where RIGHT(ClassID, 4) = 'B014'
