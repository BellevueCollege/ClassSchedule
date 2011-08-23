using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CTCClassSchedule.Common
{
	public class SearchResult
	{

		public string ClassID { get; set; }
		public int? SearchGroup { get; set; }
		public int? SearchRank { get; set; }
	}
}