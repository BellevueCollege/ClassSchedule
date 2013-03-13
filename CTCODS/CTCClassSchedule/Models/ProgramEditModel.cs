using System.Collections.Generic;
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
	}
}