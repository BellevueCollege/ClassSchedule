using Ctc.Ods.Types;

namespace CTCClassSchedule.Models
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
		public new string CourseTitle { get; set; }

    public new ClassScheduleSectionID ID {get;set;}

    /// <summary>
    /// Identifies whether or not a cross-listing relationship exists in <see cref="ClassScheduleDb.SectionCourseCrosslistings"/>
    /// </summary>
    /// <remarks>
    /// This property exists in this model so that we don't have to make additional database calls
    /// to determine IF a relationship exists - once we know that one does, then we can query
    /// the database to find out what they are.
    /// </remarks>
    public bool IsCrossListed{get;set;}

		/// <summary>
		/// Provides a means to set protected values of the parent object.
		/// </summary>
		/// <remarks>
		/// <para>
		///		The members of <see cref="Section"/> are read-only to external code, which is problematic
		///		for LINQ queries that need to assign values. So this Property takes the parent <see cref="Section"/>
		///		object and assigns these values because it <b><i>is</i></b> within scope of the parent
		///		<see cref="Section"/>.
		/// </para>
		/// <note type="important">
		///		Whenever a new property is added to the base <see cref="Section"/> class, an assignment
		///		should be added within this property's setter.
		/// </note>
		/// </remarks>
		/// <seealso cref="Section"/>
		public Section ParentObject
		{
			set
			{
				CourseID = value.CourseID;
        // ClassScheduleSectionID has additional Attributes that we need.
				ID = new ClassScheduleSectionID(value.ID);
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
				IsDifferentStartDate = value.IsDifferentStartDate;
				IsContinuousEnrollment = value.IsContinuousEnrollment;
				LastRegistrationDate = value.LastRegistrationDate;
				IsVariableCredits = value.IsVariableCredits;
				IsTelecourse = value.IsTelecourse;
				Footnotes = value.Footnotes;
        IsDifferentEndDate = value.IsDifferentEndDate;
				IsLinked = value.IsLinked;
				LinkedTo = value.LinkedTo;
			}
		}
  }
}
