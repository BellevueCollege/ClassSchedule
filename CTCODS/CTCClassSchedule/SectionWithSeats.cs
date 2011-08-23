using Ctc.Ods.Types;
using System.Collections.Generic;

namespace CTCClassSchedule
{
	public class SectionWithSeats : Section
	{
		public int? SeatsAvailable { get; set; }
		public string LastUpdated { get; set; }
		public IEnumerable<string> Footnotes { get; set; }

		/// <summary>
		/// Provides a means to set protected values of the parent object.
		/// </summary>
		public Section ParentObject
		{
			set
			{
				CourseID = value.CourseID;
				ID = value.ID;
				CourseNumber = value.CourseNumber;
				CourseTitle = value.CourseTitle;
				CourseSubject = value.CourseSubject;
				Credits = value.Credits;
				Offered = value.Offered;
				SectionCode = value.SectionCode;
				WaitlistCount = value.WaitlistCount;
				Yrq = value.Yrq;
				IsOnline = value.IsOnline;
			}
		}
	}
}