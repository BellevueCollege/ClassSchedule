using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ctc.Ods.Types;

namespace CTCClassSchedule.Models
{
	public class QuarterNavigationModel
	{
		public YearQuarter CurrentQuarter { get; set; }
		public IList<YearQuarter> NavigationQuarters { get; set; }
	}
}