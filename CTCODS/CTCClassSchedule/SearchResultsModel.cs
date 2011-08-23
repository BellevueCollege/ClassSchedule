using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CTCClassSchedule.Common;

namespace CTCClassSchedule
{
	public class SearchResultsModel
	{
		public IEnumerable<SectionWithSeats> Section { get; set; }

		public IEnumerable<SearchResultNoSection> SearchResultNoSection { get; set; }
	}
}