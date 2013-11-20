using System.Collections.Generic;
using System.Data.Objects;
using System.Web.Mvc;
using CTCClassSchedule.Common;
using CTCClassSchedule.Models;


namespace CTCClassSchedule
{
	public class ProgramEditModel
	{
		public Subject Subject { get; set; }
		public Department Department { get; set; }
		public Division Division { get; set; }
		public IEnumerable<string> AllCoursePrefixes { get; set; }
    public IList<Department> AllDepartments { get; set; }
    public IList<Division> AllDivisions { get; set; }
	}
}