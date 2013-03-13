using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ctc.Ods.Types;

namespace CTCClassSchedule.Models
{
	public class SubjectViewModel
	{
    public YearQuarter CurrentQuarter { get; set; }
    public IEnumerable<Course> Courses { get; set; }
    public IList<YearQuarter> NavigationQuarters { get; set; }
		public string Slug { get; set; }
		public string SubjectTitle { get; set; }
		public string SubjectIntro { get; set; }
		public string DepartmentTitle { get; set; }
		public string DepartmentURL { get; set; }
	}
}