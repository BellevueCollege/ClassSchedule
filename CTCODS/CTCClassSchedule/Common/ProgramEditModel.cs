using System.Collections.Generic;
using CTCClassSchedule.Common;
using CTCClassSchedule.Models;


namespace CTCClassSchedule
{
	public class ProgramEditModel
	{
		public SubjectInfoResult ItemToUpdate { get; set; }

		public IEnumerable<string> Subjects { get; set; }

		public IEnumerable<string> MergedSubjects{ get; set; }

	}
}