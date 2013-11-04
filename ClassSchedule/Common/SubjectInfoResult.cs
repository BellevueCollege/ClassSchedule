using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CTCClassSchedule.Models;

namespace CTCClassSchedule.Common
{
	public class SubjectInfoResult
	{
		public Subject Subject { get; set; }
		public Department Department { get; set; }
		public Division Division { get; set; }
		public IList<SubjectsCoursePrefix> CoursePrefixes { get; set; }
	}
}