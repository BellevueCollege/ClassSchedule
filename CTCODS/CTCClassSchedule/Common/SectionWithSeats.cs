using Ctc.Ods.Types;

namespace CTCClassSchedule
{
	public class SectionWithSeats : Section
	{
		public int? SeatsAvailable { get; set; }
		public string LastUpdated { get; set; }
		public string LastUpdatedBy { get; set; }
		public string CourseFootnotes { get; set; }
		public string SectionFootnotes { get; set; }
		public string CustomTitle { get; set; }
		public string CustomDescription { get; set; }

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
				CourseDescriptions = value.CourseDescriptions;
				Credits = value.Credits;
				Offered = value.Offered;
				SectionCode = value.SectionCode;
				WaitlistCount = value.WaitlistCount;
				StartDate = value.StartDate;
				EndDate = value.EndDate;
				Yrq = value.Yrq;
				IsCommonCourse = value.IsCommonCourse;
				IsOnline = value.IsOnline;
				IsOnCampus = value.IsOnCampus;
				IsHybrid = value.IsHybrid;
				IsLateStart = value.IsLateStart;
				IsContinuousEnrollment = value.IsContinuousEnrollment;
				IsVariableCredits = IsVariableCredits;
				IsTelecourse = value.IsTelecourse;
				Footnotes = value.Footnotes;
                IsDifferentEndDate = value.IsDifferentEndDate;
				IsLinked = value.IsLinked;
				LinkedTo = value.LinkedTo;
			}
		}
	}
}
