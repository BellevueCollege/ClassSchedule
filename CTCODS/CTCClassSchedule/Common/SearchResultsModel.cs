using System.Collections.Generic;
using CTCClassSchedule.Common;

namespace CTCClassSchedule
{
	public class SearchResultsModel
	{
		public IEnumerable<SectionWithSeats> Section { get; set; }

		public IEnumerable<SearchResultNoSection> SearchResultNoSection { get; set; }

		public IEnumerable<string> Subjects { get; set; }

	}
}