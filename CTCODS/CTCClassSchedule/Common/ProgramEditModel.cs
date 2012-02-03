using System.Collections.Generic;
using CTCClassSchedule.Common;
using CTCClassSchedule.Models;


namespace CTCClassSchedule
{
	public class ProgramEditModel
	{
		public ProgramInformation itemToUpdate { get; set; }

		public IEnumerable<string> MergeSubjectChoices { get; set; }

		public IEnumerable<string> MergedClasses{ get; set; }

	}
}