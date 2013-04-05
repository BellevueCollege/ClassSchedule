using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Ctc.Ods;
using Ctc.Ods.Data;
using Ctc.Ods.Types;
using CTCClassSchedule.Models;
using CtcApi.Web.Security;
using MvcMiniProfiler;
using System.Configuration;
using System.Text;

namespace CTCClassSchedule.Common
{
	public static class Helpers
	{
		/// <summary>
		/// Useful if a helper method works with a timespan and should default to
		/// either 12:00am or 23:59pm (start time/end time).
		/// </summary>
		private enum DefaultTimeResult
		{
			StartTime,
			EndTime
		};

		public static string getBodyClasses(HttpContextBase Context)
		{
			string classes = String.Empty;

			if (Context.User.Identity.IsAuthenticated)
			{
				string appSetting = ConfigurationManager.AppSettings["ApplicationDeveloper"];
				if (!String.IsNullOrWhiteSpace(appSetting))
				{
					string[] roles = appSetting.Split(',');
					classes += ActiveDirectoryRoleProvider.IsUserInRoles(Context.User, roles) ? "role-developer" : "";
				}

				appSetting = ConfigurationManager.AppSettings["ApplicationAdmin"];
				if (!String.IsNullOrWhiteSpace(appSetting))
				{
					string[] roles = appSetting.Split(',');
					classes += ActiveDirectoryRoleProvider.IsUserInRoles(Context.User, roles) ? " role-schedule-editors " : "";
				}

				appSetting = ConfigurationManager.AppSettings["ApplicationEditor"];
				if (!String.IsNullOrWhiteSpace(appSetting))
				{
					string[] roles = appSetting.Split(',');
					classes += ActiveDirectoryRoleProvider.IsUserInRoles(Context.User, roles) ? " role-admin " : "";
				}
			}
			else
			{
				classes = "not-logged-in";
			}

			return classes;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="Context"></param>
		/// <returns></returns>
		public static bool isAdmin(HttpContextBase Context)
		{
			bool isAdmin = false;
			if (Context.User.Identity.IsAuthenticated)
			{
				string appSetting = ConfigurationManager.AppSettings["ApplicationAdmin"];
				if (!String.IsNullOrWhiteSpace(appSetting))
				{
					string[] roles = appSetting.Split(',');
					isAdmin = ActiveDirectoryRoleProvider.IsUserInRoles(Context.User, roles) ? true : false;
				}
			}
			return isAdmin;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="Context"></param>
		/// <returns></returns>
		public static bool isEditor(HttpContextBase Context)
		{
			bool isEditor = false;
			if (Context.User.Identity.IsAuthenticated)
			{
				string appSetting = ConfigurationManager.AppSettings["ApplicationEditor"];
				if (!String.IsNullOrWhiteSpace(appSetting))
				{
					string[] roles = appSetting.Split(',');
					isEditor = ActiveDirectoryRoleProvider.IsUserInRoles(Context.User, roles) ? true : false;
				}
			}
			return isEditor;
		}

		// TODO: Jeremy, another optional/BCC specific way of getting data - find another way?
		public static String getProfileURL(string SID)
		{
			Encryption64 en = new Encryption64();

			string returnString = String.Empty;
			returnString = "http://bellevuecollege.edu/directory/PersonDetails.aspx?PersonID=" + en.Encrypt(SID,"!#$a54?5");
			return returnString;

		}

		// TODO: Jeremy, another optional/BCC specific way of getting data - find another way?
		public static string getBookstoreBooksLink(IList<SectionWithSeats> linkedSections)
		{
			StringBuilder resultURL = new StringBuilder("http://bellevue.verbacompare.com/comparison?id=");
			for (int i = 0; i < linkedSections.Count; i++)
			{
				SectionWithSeats sec = linkedSections[i];
				resultURL.AppendFormat("{0}{1}__{2}__{3}{4}__{5}", (i > 0? "%2C" : String.Empty),
																	getYRQValueForBookstoreURL(sec.Yrq),
																	sec.CourseSubject,
																	sec.CourseNumber,
																	(sec.IsCommonCourse ? "%26" : String.Empty),
																	sec.ID.ItemNumber);
			}

			return resultURL.ToString();
		}

		/// <summary>
		/// returns an IList<ISectionFacet> that contains all of the facet information
		/// passed into the app by the user clicking on the faceted search left pane
		/// facets accepted: flex, time, days, availability
		/// </summary>
		static public IList<ISectionFacet> addFacets(string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string f_telecourse, string avail, string latestart, string numcredits)
		{
			IList<ISectionFacet> facets = new List<ISectionFacet>();

			//add the class format facet options (online, hybrid, telecourse, on campus)
			ModalityFacet.Options modality = ModalityFacet.Options.All;	// default

			if (!String.IsNullOrWhiteSpace(f_online))
			{
				modality = (modality | ModalityFacet.Options.Online);
			}
			if (!String.IsNullOrWhiteSpace(f_hybrid))
			{
				modality = (modality | ModalityFacet.Options.Hybrid);
			}
			if (!String.IsNullOrWhiteSpace(f_telecourse))
			{
				modality = (modality | ModalityFacet.Options.Telecourse);
			}
			if (!String.IsNullOrWhiteSpace(f_oncampus))
			{
				modality = (modality | ModalityFacet.Options.OnCampus);
			}
			facets.Add(new ModalityFacet(modality));


			//determine integer values for start/end time hours and minutes
			int startHour, startMinute;
			int endHour, endMinute;
			getHourAndMinuteFromString(timestart, out startHour, out startMinute);
			getHourAndMinuteFromString(timeend, out endHour, out endMinute, DefaultTimeResult.EndTime);

			//add the time facet
			facets.Add(new TimeFacet(new TimeSpan(startHour, startMinute, 0), new TimeSpan(endHour, endMinute, 0)));

			//day of the week facets
			DaysFacet.Options days = DaysFacet.Options.All;	// default

			if (!String.IsNullOrWhiteSpace(day_su))
			{
				days = (days | DaysFacet.Options.Sunday);
			}
			if (!String.IsNullOrWhiteSpace(day_m))
			{
				days = (days | DaysFacet.Options.Monday);
			}
			if (!String.IsNullOrWhiteSpace(day_t))
			{
				days = (days | DaysFacet.Options.Tuesday);
			}
			if (!String.IsNullOrWhiteSpace(day_w))
			{
				days = (days | DaysFacet.Options.Wednesday);
			}
			if (!String.IsNullOrWhiteSpace(day_th))
			{
				days = (days | DaysFacet.Options.Thursday);
			}
			if (!String.IsNullOrWhiteSpace(day_f))
			{
				days = (days | DaysFacet.Options.Friday);
			}
			if (!String.IsNullOrWhiteSpace(day_s))
			{
				days = (days | DaysFacet.Options.Saturday);
			}
			facets.Add(new DaysFacet(days));


			if (!String.IsNullOrWhiteSpace(avail))
			{
				if (avail == "All")
				{
					facets.Add(new AvailabilityFacet(AvailabilityFacet.Options.All));
				}

				if (avail == "Open")
				{
					facets.Add(new AvailabilityFacet(AvailabilityFacet.Options.Open));
				}
			}

			if (!String.IsNullOrWhiteSpace(latestart))
			{
				if (latestart == "true")
				{
					facets.Add(new LateStartFacet());
				}
			}


			if (!String.IsNullOrWhiteSpace(numcredits))
			{
				if (numcredits != "Any")
				{
					int credits;
					try
					{
						credits = Convert.ToInt16(numcredits);
						facets.Add(new CreditsFacet(credits));
					}
					catch
					{
						throw new FormatException("Number of credits was not a valid integer");
					}
				}

			}




			return facets;
		}

		/// <summary>
		/// Returns a friendly time in sentance form given a datetime. This value
		/// is create by subtracting the input datetime from the current datetime.
		/// example: 6/8/2011 07:23:123 -> about 4 hours ago
		/// </summary>
		public static string getFriendlyTime(DateTime theDate)
		{
			const int SECOND = 1;
			const int MINUTE = 60 * SECOND;
			const int HOUR = 60 * MINUTE;
			const int DAY = 24 * HOUR;
			const int MONTH = 30 * DAY;

			var deltaTimeSpan = new TimeSpan(DateTime.Now.Ticks - theDate.Ticks);

			var delta = deltaTimeSpan.TotalSeconds;

			if (delta < 0)
			{
				return "not yet";
			}

			if (delta < 1 * MINUTE)
			{
				return deltaTimeSpan.Seconds == 1 ? "one second ago" : deltaTimeSpan.Seconds + " seconds ago";
			}
			if (delta < 2 * MINUTE)
			{
				return "a minute ago";
			}
			if (delta < 45 * MINUTE)
			{
				return deltaTimeSpan.Minutes + " minutes ago";
			}
			if (delta < 90 * MINUTE)
			{
				return "an hour ago";
			}
			if (delta < 24 * HOUR)
			{
				return deltaTimeSpan.Hours + " hours ago";
			}
			if (delta < 48 * HOUR)
			{
				return "yesterday";
			}
			if (delta < 30 * DAY)
			{
				return deltaTimeSpan.Days + " days ago";
			}
			if (delta < 12 * MONTH)
			{
				int months = Convert.ToInt32(Math.Floor((double)deltaTimeSpan.Days / 30));
				return months <= 1 ? "one month ago" : months + " months ago";
			}
			else
			{
				int years = Convert.ToInt32(Math.Floor((double)deltaTimeSpan.Days / 365));
				return years <= 1 ? "one year ago" : years + " years ago";
			}
		}

    /// <summary>
    /// Take a full name and converts it to a short format, with the last name and first initial of
    /// the first name (e.g. ANDREW CRASWELL -> CRASWELL A).
    /// </summary>
    /// <param name="fullName">The full name to be converted.</param>
    /// <returns>A string with the short name version of the full name. If fullName was blank or empty, a null is returned.</returns>
    public static string getShortNameFormat(string fullName)
    {
      string shortName = null;
      if (!String.IsNullOrWhiteSpace(fullName))
      {
        int nameStartIndex = fullName.IndexOf(" ") + 1;
        int nameEndIndex = fullName.Length - nameStartIndex;
        shortName = String.Concat(fullName.Substring(nameStartIndex, nameEndIndex), " ", fullName.Substring(0, 1));
      }

      return shortName;
    }

		/// <summary>
		///
		/// </summary>
		/// <param name="fieldID"></param>
		/// <param name="fieldTitle"></param>
		/// <param name="fieldValue"></param>
		/// <returns></returns>
		static public GeneralFacetInfo getFacetInfo(string fieldID, string fieldTitle, string fieldValue)
		{
			return new GeneralFacetInfo {
			  ID = fieldID,
			  Title = fieldTitle,
				Value = fieldValue,
			  Selected = Utility.SafeConvertToBool(fieldValue)
			};
		}

		static public IList<GeneralFacetInfo> ConstructModalityList(string f_oncampus, string f_online, string f_hybrid, string f_telecourse)
		{
			IList<GeneralFacetInfo> modality = new List<GeneralFacetInfo>(4);

			modality.Add(getFacetInfo("f_oncampus", "On Campus", f_oncampus));
			modality.Add(getFacetInfo("f_online", "Online", f_online));
			modality.Add(getFacetInfo("f_hybrid", "Hybrid", f_hybrid));
			modality.Add(getFacetInfo("f_telecourse", "Telecourse", f_telecourse));

			return modality;
		}

		static public IList<GeneralFacetInfo> ConstructDaysList(string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s)
		{
			IList<GeneralFacetInfo> modality = new List<GeneralFacetInfo>(7);

			modality.Add(getFacetInfo("day_su", "S", day_su));
			modality.Add(getFacetInfo("day_m", "M", day_m));
			modality.Add(getFacetInfo("day_t", "T", day_t));
			modality.Add(getFacetInfo("day_w", "W", day_w));
			modality.Add(getFacetInfo("day_th", "T", day_th));
			modality.Add(getFacetInfo("day_f", "F", day_f));
			modality.Add(getFacetInfo("day_s", "S", day_s));

			return modality;
		}

		/// <summary>
		/// Gets the <see cref="YearQuarter"/> for the current, and previous 3 quarters.
		/// This drives the dynamic YRQ navigation bar
		/// </summary>
		static public IList<YearQuarter> getYearQuarterListForMenus(OdsRepository repository)
		{
			IList<YearQuarter> currentFutureQuarters = repository.GetRegistrationQuarters(4);
			return currentFutureQuarters;
		}

		/// <summary>
		/// Gets the <see cref="YearQuarter"/> for the current, and future 2 quarters.
		/// </summary>
		static public IList<YearQuarter> getFutureQuarters(OdsRepository repository)
		{
			IList<YearQuarter> futureQuarters = repository.GetFutureQuarters(3);
			return futureQuarters;
		}

		/// <summary>
		/// Gets the current http get/post params and assigns them to an IDictionary<string, object>
		/// This is mainly used to set a Viewbag variable so these can be passed into action links in the views.
		/// </summary>
		/// <param name="httpRequest"></param>
		/// <param name="ignoreKeys">List of key values to ignore.</param>
		static public IDictionary<string, object> getLinkParams(HttpRequestBase httpRequest, params string[] ignoreKeys)
		{
			IDictionary<string, object> linkParams = new Dictionary<string, object>(httpRequest.QueryString.Count);
			foreach (string key in httpRequest.QueryString.AllKeys)
			{
				if (key != "X-Requested-With" && !ignoreKeys.Contains(key, StringComparer.OrdinalIgnoreCase))
				{
					if (linkParams.ContainsKey(key))
					{
						linkParams[key] = httpRequest.QueryString[key];
					}
					else
					{
						linkParams.Add(key, httpRequest.QueryString[key]);
					}
				}
			}
			foreach (string key in httpRequest.Form.AllKeys)
			{
				if (!ignoreKeys.Contains(key, StringComparer.OrdinalIgnoreCase))
				{
					if (linkParams.ContainsKey(key))
					{
						linkParams[key] = httpRequest.Form[key];
					}
					else
					{
						linkParams.Add(key, httpRequest.Form[key]);
					}
				}
			}
			return linkParams;
		}

		/// <summary>
		/// Common method to retrieve <see cref="SectionWithSeats"/> records
		/// </summary>
		/// <param name="currentYrq"></param>
		/// <param name="sections"></param>
		/// <param name="db"></param>
		/// <returns></returns>
		public static IList<SectionWithSeats> GetSectionsWithSeats(string currentYrq, IList<Section> sections, ClassScheduleDb db)
		{
			MiniProfiler profiler = MiniProfiler.Current;

			// ensure we're ALWAYS getting the latest data from the database (i.e. ignore any cached data)
			// Reference: http://forums.asp.net/post/2848021.aspx
			db.vw_Class.MergeOption = MergeOption.OverwriteChanges;

			IList<vw_Class> classes;
      using (profiler.Step("API::Get Class Schedule Specific Data()"))
      {
				classes = db.vw_Class.Where(c => c.YearQuarterID == currentYrq).ToList();
      }


			IList<SectionWithSeats> sectionsEnum;
      using (profiler.Step("Joining all data"))
      {
          sectionsEnum = (
			      from c in sections
                      join d in classes on c.ID.ToString() equals d.ClassID into t1
			      from d in t1.DefaultIfEmpty()
											join ss in db.SectionSeats on (d != null ? d.ClassID : "") equals ss.ClassID into t2
											from ss in t2.DefaultIfEmpty()
											join sm in db.SectionsMetas on (d != null ? d.ClassID : "") equals sm.ClassID into t3
											from sm in t3.DefaultIfEmpty()
                      // NOTE:  This logic assumes that data will only be saved in ClassScheduleDb after having come through
                      //        the filter of the CtcApi - which normalizes spacing of the CourseID field data.
                      join cm in db.CourseMetas on (d != null ? d.CourseID : "") equals cm.CourseID into t4
											from cm in t4.DefaultIfEmpty()
			      orderby c.Yrq.ID descending
			      select new SectionWithSeats {
								ParentObject = c,
								SeatsAvailable = ss != null ? ss.SeatsAvailable : Int32.MinValue,	// allows us to identify past quarters (with no availability info)
								LastUpdated = (d != null ? d.LastUpdated.GetValueOrDefault() : DateTime.MinValue).ToString("h:mm tt").ToLower(),
													SectionFootnotes = sm != null && !String.IsNullOrWhiteSpace(sm.Footnote) ? sm.Footnote : String.Empty,
													CourseFootnotes = cm != null && !String.IsNullOrWhiteSpace(cm.Footnote) ? cm.Footnote : String.Empty,
                          CustomTitle = sm != null && !String.IsNullOrWhiteSpace(sm.Title) ? sm.Title : c.CourseTitle,
													CustomDescription = sm != null && !String.IsNullOrWhiteSpace(sm.Description) ? sm.Description : String.Empty,
											}).OrderBy(s => s.CourseNumber).ThenBy(s => s.CourseTitle).ToList();

        // Flag sections that are cross-linked with a Course
        foreach (string sec in db.SectionCourseCrosslistings.Select(x => x.ClassID).Distinct())
        {
          if (sectionsEnum.Any(s => s.ID.ToString() == sec))
          {
            sectionsEnum.Single(s => s.ID.ToString() == sec).IsCrossListed = true;
          }
        }
      }

			return sectionsEnum;
		}

    public static string getFriendlyDayRange(string dayString)
    {
      StringBuilder friendlyName = new StringBuilder(dayString);
      IDictionary<string, string> daysDictionary = new Dictionary<string, string>
                                                     {
        {"Online","Online/"},
        {"DAILY","Daily/"},
        {"ARRANGED","Arranged/"},
        {"M","Monday/"},
        /* HACK: See .Replace() comment below */
        {"Th","Xhursday/"},
        {"W","Wednesday/"},
        {"T","Tuesday/"},
        {"F","Friday/"},
        {"Sa","Saturday/"},
        {"Su","Sunday/"}
      };

      foreach (string key in daysDictionary.Keys)
      {
        friendlyName.Replace(key, daysDictionary[key]);
      }
      // HACK: If we don't obscure the "T" in Thursday, it will be replaced by the Tuesday evaluation.
      friendlyName.Replace("X", "T");

      return friendlyName.ToString().TrimEnd('/');
    }

	  /// <summary>
	  /// Groups a list of Sections by course, into descriptive SectionBlocks
	  /// </summary>
	  /// <param name="sections">List of sections to group</param>
	  /// <param name="db"></param>
	  /// <returns>List of SectionBlock objects which describe the block of sections</returns>
	  public static IList<SectionsBlock> groupSectionsIntoBlocks(IList<SectionWithSeats> sections, ClassScheduleDb db)
    {
      MiniProfiler profiler = MiniProfiler.Current;

      IList<SectionsBlock> results = new List<SectionsBlock>();
      IList<SectionWithSeats> allLinkedSections = sections.Where(s => s.IsLinked).ToList();
      IList<SectionWithSeats> nonLinkedSections;

      // minimize trips to the database, since this collection should be relatively small
      IList<SectionCourseCrosslisting> crosslistings;
      using (profiler.Step("Retrieving Section/Course cross-listings"))
      {
        crosslistings = db.SectionCourseCrosslistings.ToList();
      }

      // sort by the markers indicators where we need to being a new block (w/ course title, etc.)
      /* TODO: Implement a more configurable sort method. */
      using (profiler.Step("Sorting sections in preparation for grouping and linking"))
      {
        /* COMMENT THIS LINE TO DEBUG
        IEnumerable<SectionWithSeats> d1 = sections.Where(s => s.ID.ItemNumber.StartsWith("410"));
        // END DEBUGGING */
        nonLinkedSections = sections.Where(s => !s.IsLinked)
                          .OrderBy(s => s.CourseNumber)
                          .ThenBy(s => allLinkedSections.Where(l => l.LinkedTo == s.ID.ItemNumber).Count())
                          .ThenByDescending(s => s.IsVariableCredits)
                          .ThenBy(s => s.Credits)
                          .ThenBy(s => s.IsTelecourse)
                          .ThenBy(s => s.IsOnline)
                          .ThenBy(s => s.IsHybrid)
                          .ThenBy(s => s.IsOnCampus)
                          .ThenBy(s => s.SectionCode).ToList();
      }

      // Group all the sections into course blocks and determine which sections linked
      using (profiler.Step("Grouping/linking by course"))
      {
        int processedCount = 0;
        while (processedCount < nonLinkedSections.Count)
        {
          SectionsBlock courseBlock = new SectionsBlock();
          courseBlock.LinkedSections = new List<SectionWithSeats>();

          IList<SectionWithSeats> remainingSections = nonLinkedSections.Skip(processedCount).ToList();
          SectionWithSeats firstSection = remainingSections.First();

          courseBlock.Sections = remainingSections.TakeWhile(s =>
                                                             s.CourseID == firstSection.CourseID &&
                                                             s.CourseTitle == firstSection.CourseTitle &&
                                                             s.Credits == firstSection.Credits &&
                                                             s.IsVariableCredits == firstSection.IsVariableCredits &&
                                                             allLinkedSections.Count(l => l.LinkedTo == s.ID.ItemNumber) == allLinkedSections.Count(l => l.LinkedTo == firstSection.ID.ItemNumber))
                                                             .ToList();

          // Flag whether or not this course block is crosslisted with other sections
          courseBlock.IsCrosslisted = crosslistings.Any(x => x.CourseID == firstSection.CourseID);

          // Find all links associated to each of the grouped sections
          foreach (SectionWithSeats sec in courseBlock.Sections)
          {
            SectionWithSeats sect = sec;  // Use copy of object to ensure cross-compiler compatibility
            List<SectionWithSeats> linkedSections = allLinkedSections.Where(s => sect != null && s.LinkedTo == sect.ID.ItemNumber).ToList();
            if (linkedSections.Count > 0)
            {
              courseBlock.LinkedSections.AddRange(linkedSections);
            }
          }

          // Get a list of common footnotes shared with every section in the block
          courseBlock.CommonFootnotes = ExtractCommonFootnotes(courseBlock.Sections);

          processedCount += courseBlock.Sections.Count();
          results.Add(courseBlock);
        }
      }

      return results;
    }

		/// <summary>
		/// Retrieves all active and future course descriptions from a <see cref="Course"/>
		/// </summary>
		/// <param name="course">The course to process</param>
		/// <param name="currentYrq">The current YRQ</param>
		/// <returns>An ordered list of CourseDescriptions sorted current first</returns>
		public static IList<CourseDescription> getActiveCourseDescriptions(Course course, YearQuarter currentYrq)
		{
			IList<CourseDescription> results = new List<CourseDescription>();
			if (course.Descriptions != null && course.Descriptions.Count() > 0)
			{
        results = course.Descriptions.Where(d => String.Compare(d.YearQuarterBegin.ID, currentYrq.ID) <= 0)
                                     .OrderBy(d => d.YearQuarterBegin.ID)
                                     .Take(1)
                                     .ToList();
			}

			return results;
		}

		public static string BuildCourseID(string CourseNumber, string CourseSubject, bool IsCommonCourse)
		{
			char[] CourseID = new char[8];

			//fill the char array with spaces
			for (int i = 0; i < CourseID.Length; i++)
			{
				CourseID[i] = ' ';
			}

			//assign the subject to the first 2/3/4/5 char array slots
			for (int i = 0; i < CourseSubject.Count(); i++)
			{
				CourseID[i] = CourseSubject[i];
			}

			//add in the ampersand if it's a common course
			if (IsCommonCourse)
			{
				CourseID[CourseSubject.Length] = '&';
			}

			//if the coursenumber is 3 chars or longer, assign it to the last 3 slots in the return array
			if (CourseNumber.Length >= 3)
			{
				for (int i = 0; i < 3; i++)
				{
					CourseID[i + 5] = CourseNumber[i];
				}
			}

			//if the coursenumber is one of the exception 4 alphanumeric courseids, such as NURS 101X (2503B124),
			//create a new 9 character return array, copy the 8 character array to it, and append the last character of
			//the CourseNum to the last slot in the return array.
			if (CourseNumber.Length > 3)
			{
				char[] TempCourseID = CourseID;
				CourseID = new char[9];

				for (int i = 0; i < TempCourseID.Count(); i++)
				{
					CourseID[i] = TempCourseID[i];
				}

				CourseID[8] = CourseNumber[CourseNumber.Length - 1];
			}



			return new string(CourseID);
		}

		public static string SubjectWithCommonCourseFlag(SectionWithSeats sec)
		{
			return SubjectWithCommonCourseFlag(sec.CourseSubject, sec.IsCommonCourse);
		}

		public static string SubjectWithCommonCourseFlag(Course course)
		{
			return SubjectWithCommonCourseFlag(course.Subject, course.IsCommonCourse);
		}

		private static string SubjectWithCommonCourseFlag(string subject, bool isCommonCourse)
		{
			return isCommonCourse ? subject.Trim() + "&" : subject.Trim();
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="sectionBlock"></param>
		/// <returns></returns>
		public static IList<string> ExtractCommonFootnotes(IList<SectionWithSeats> sectionBlock)
		{
			IEnumerable<string> rollupFootnotes = new string[]{};

			for (int i = 0; i < sectionBlock.Count; i++)
			{
				IEnumerable<string> f = i > 0 ? rollupFootnotes.ToArray() : sectionBlock[0].Footnotes;

				rollupFootnotes = f.Intersect(sectionBlock[i].Footnotes);
			}

			return rollupFootnotes.ToList();
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="sectionBlock"></param>
		/// <returns></returns>
		public static IList<string> ExtractCommonFootnotes(IEnumerable<SectionWithSeats> sectionBlock)
		{
			return ExtractCommonFootnotes(sectionBlock.ToList());
		}

		/// <summary>
		/// Applies keyword markup to all instances of the <paramref name="searchTerm"/> found in the provided <paramref name="text"/>
		/// </summary>
		/// <param name="searchTerm"></param>
		/// <param name="text"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		static public string FormatWithSearchTerm(string searchTerm, string text, params object[] args)
		{
			if (String.IsNullOrWhiteSpace(text) || String.IsNullOrWhiteSpace(searchTerm))
			{
				return text;
			}

			if (args != null && args.Length > 0)
			{
				text = String.Format(text, args);
			}
			return Regex.Replace(text, searchTerm, @"<em class='keyword'>$&</em>", RegexOptions.IgnoreCase);
		}

		/// <summary>
		/// Takes a string that represents a time (ie "6:45pm") and outputs the parsed hour and minute integers
		/// based on a 24 hour clock. If a time cannot be parsed, the output defaults to either 12am or 11:59pm
		/// depending on the time default parameter passed
		/// </summary>
		/// <param name="time">String representing a time value</param>
		/// <param name="hour">Reference to an integer storage for the parsed hour value</param>
		/// <param name="minute">Reference to an integer storage for the parsed minute value</param>
		/// <param name="defaultResultMode">Flag which determines the default result values if a time values were unable to be parsed</param>
		static private void getHourAndMinuteFromString(string time, out int hour, out int minute, DefaultTimeResult defaultResultMode = DefaultTimeResult.StartTime)
		{
			// In case the method is unable to convert a time, default to either a start/end time
			switch (defaultResultMode)
			{
				case DefaultTimeResult.EndTime:
					hour = 23;
					minute = 59;
					break;
				default:
					hour = 0;
					minute = 0;
					break;
			}

			// Determine integer values for time hours and minutes
			if (!String.IsNullOrWhiteSpace(time))
			{
				string timeTrimmed = time.Trim();
				bool period = (timeTrimmed.Length > 2 ? timeTrimmed.Substring(timeTrimmed.Length - 2) : timeTrimmed).Equals("PM", StringComparison.OrdinalIgnoreCase);

				// Adjust the conversion to integers if the user leaves off a leading 0
				// (possible by using tab instead of mouseoff on the time selector)
				if (time.IndexOf(':') == 2)
				{
					hour = Convert.ToInt16(time.Substring(0, 2)) + (period ? 12 : 0);
					if (time.IndexOf(':') != -1)
					{
						minute = Convert.ToInt16(time.Substring(3, 2));
					}
				}
				else
				{
					hour = Convert.ToInt16(time.Substring(0, 1)) + (period ? 12 : 0);
					if (time.IndexOf(':') != -1)
					{
						minute = Convert.ToInt16(time.Substring(2, 2));
					}
				}
			}
		}

		static private string getYRQValueForBookstoreURL(YearQuarter yrq)
		{
			string quarter = String.Empty;
			string year = String.Empty;

			if (yrq != null)
			{
				quarter = yrq.FriendlyName.ToUpper();
				year = quarter.Substring(quarter.Length - 2);
				if (quarter.Contains("FALL"))
				{
					quarter = "F";
				}
				else if (quarter.Contains("WINTER"))
				{
					quarter = "W";
				}
				else if (quarter.Contains("SPRING"))
				{
					quarter = "S";
				}
				else // Summer
				{
					quarter = "X";
				}
			}

			return String.Concat(quarter, year);
		}

	  public static IList<SectionWithSeats> ParseCommonHeadingLinkedSections(List<SectionWithSeats> linkedSections)
	  {
	    string prevCourseID = String.Empty;
	    string prevTitle = String.Empty;
	    decimal prevCredits = 0;
	    bool prevIsVariableCredits = false;

	    IList<SectionWithSeats> common = new List<SectionWithSeats>(linkedSections.Count);

	    foreach (SectionWithSeats section in linkedSections)
	    {
	      if (!(section.CourseID == prevCourseID && section.CourseTitle == prevTitle && section.Credits == prevCredits && section.IsVariableCredits == prevIsVariableCredits))
	      {
	        common.Add(section);
	      }

	      prevCourseID = section.CourseID;
	      prevTitle = section.CourseTitle;
	      prevCredits = section.Credits;
	      prevIsVariableCredits = section.IsVariableCredits;
	    }

	    return common;
	  }
	}
}
