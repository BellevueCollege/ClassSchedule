/*
This file is part of CtcClassSchedule.

CtcClassSchedule is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

CtcClassSchedule is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with CtcClassSchedule.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Data.Objects;
using CtcApi.Extensions;
// ReSharper disable RedundantUsingDirective
using System.Diagnostics;
// ReSharper restore RedundantUsingDirective
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;
using CTCClassSchedule.Properties;
using Common.Logging;
using Ctc.Ods.Data;
using Ctc.Ods.Types;
using CTCClassSchedule.Models;
using CtcApi.Web.Security;
using StackExchange.Profiling;
using System.Configuration;
using System.Text;
using Ctc.Ods.Config;
using Encoder = Microsoft.Security.Application.Encoder;

namespace CTCClassSchedule.Common
{
	public static class Helpers
	{
	  private static readonly ILog _log = LogManager.GetLogger(typeof(Helpers));

	  /// <summary>
    /// 
    /// </summary>
	  public static string GlobalsPath
	  {
	    get
	    {
	      string globalsPath = ConfigurationManager.AppSettings["Globals_LocalPath"] ?? @"\";
	      return globalsPath;
	    }
	  }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Context"></param>
    /// <returns></returns>
	  public static string GetBodyClasses(HttpContextBase Context)
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
		public static bool IsAdmin(HttpContextBase Context)
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
		public static bool IsEditor(HttpContextBase Context)
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
		public static String GetProfileUrl(string SID)
		{
			Encryption64 en = new Encryption64();

			string returnString = String.Empty;
      // TODO: store encryption key in .config settings
			returnString = "http://bellevuecollege.edu/directory/PersonDetails.aspx?PersonID=" + en.Encrypt(SID,"!#$a54?5");
			return returnString;

		}

		// TODO: Jeremy, another optional/BCC specific way of getting data - find another way?
		public static string GetBookstoreBooksLink(IList<SectionWithSeats> linkedSections)
		{
			StringBuilder resultURL = new StringBuilder("http://bellevue.verbacompare.com/comparison?id=");
			for (int i = 0; i < linkedSections.Count; i++)
			{
				SectionWithSeats sec = linkedSections[i];
        // TODO: can we make the bookstore link template configurable in case it needs to change?
				resultURL.AppendFormat("{0}{1}__{2}__{3}{4}__{5}", (i > 0? "%2C" : String.Empty),
																	GetYrqValueForBookstoreUrl(sec.Yrq),
																	sec.CourseSubject,
																	sec.CourseNumber,
                                  // TODO: get CommonCourseChar from .config instead of hard-coding
																	(sec.IsCommonCourse ? "%26" : String.Empty),
																	sec.ID.ItemNumber);
			}

			return resultURL.ToString();
		}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="courseId"></param>
    /// <returns></returns>
	  public static CourseID GetCourseIdFromString(string courseId)
    {
      int ix = courseId.IndexOfAny(new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' });
      string subject = courseId.Substring(0, ix).TrimEnd();
      string courseNumber = courseId.Substring(ix, courseId.Length - ix).TrimStart();

      // Get the common course char
      ApiSettings _apiSettings = ConfigurationManager.GetSection(ApiSettings.SectionName) as ApiSettings;
      string commonCourseChar = _apiSettings.RegexPatterns.CommonCourseChar;

      return new CourseID(subject.Replace(commonCourseChar, String.Empty), courseNumber, subject.Contains(commonCourseChar));
    }

	  /// <summary>
    /// Take a full name and converts it to a short format, with the last name and first initial of
    /// the first name (e.g. ANDREW CRASWELL -> CRASWELL A).
    /// </summary>
    /// <param name="fullName">The full name to be converted.</param>
    /// <returns>A string with the short name version of the full name. If fullName was blank or empty, a null is returned.</returns>
    public static string GetShortNameFormat(string fullName)
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
		/// Gets the <see cref="YearQuarter"/> for the current, and previous 3 quarters.
		/// This drives the dynamic YRQ navigation bar
		/// </summary>
		static public IList<YearQuarter> GetYearQuarterListForMenus(OdsRepository repository)
		{
			IList<YearQuarter> currentFutureQuarters = repository.GetRegistrationQuarters(10);
			return currentFutureQuarters;
		}

		/// <summary>
		/// Gets the <see cref="YearQuarter"/> for the current, and future 2 quarters.
		/// </summary>
		static public IList<YearQuarter> GetFutureQuarters(OdsRepository repository)
		{
			IList<YearQuarter> futureQuarters = repository.GetFutureQuarters(3);
			return futureQuarters;
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
                  //orderby c.Credits, c.Offered.First().StartTime, c.Yrq.ID //9/15/2014 johanna.aqui, added credit and start time to sort order.  Order applied at different times and has different effects depending on the controller (Search, Classes, Scheduler)
                  orderby c.Credits , (c.Offered != null && c.Offered.FirstOrDefault() != null && c.Offered.FirstOrDefault().StartTime != null ? c.Offered.FirstOrDefault().StartTime : DateTime.MinValue), c.Yrq.ID descending   // 7/20/17 Updated by Nicole S to consider situation where instruction(offered) data doesn't exists. This shouldn't happen if data is correct but it did throw an error with bad data.
                  select new SectionWithSeats {
								ParentObject = c,
								SeatsAvailable = ss != null ? ss.SeatsAvailable : Int32.MinValue,	// allows us to identify past quarters (with no availability info)
                          SeatsLastUpdated = (ss != null ? ss.LastUpdated.GetValueOrDefault() : DateTime.MinValue).ToString(Settings.Default.SeatUpdatedDateTimeFormat).ToLower(),
								LastUpdated = (d != null ? d.LastUpdated.GetValueOrDefault() : DateTime.MinValue).ToString("h:mm tt").ToLower(),
													SectionFootnotes = sm != null && !String.IsNullOrWhiteSpace(sm.Footnote) ? sm.Footnote : String.Empty,
													CourseFootnotes = cm != null && !String.IsNullOrWhiteSpace(cm.Footnote) ? cm.Footnote : String.Empty,
                          CourseTitle = sm != null && !String.IsNullOrWhiteSpace(sm.Title) ? sm.Title : c.CourseTitle,
													CustomDescription = sm != null && !String.IsNullOrWhiteSpace(sm.Description) ? sm.Description : String.Empty,
                }).OrderBy(s => s.CourseNumber).ThenBy(s => s.CourseTitle).ToList();

          string _availValue = "";
          if (HttpContext.Current.Request.QueryString["avail"] != null)
          {
              _availValue = HttpContext.Current.Request.QueryString["avail"].ToString();
          }
          if (_availValue == "Open")
          {
              sectionsEnum = (from open in sectionsEnum
                             where open.SeatsAvailable != 0
                             select open).ToList();
          }



#if DEBUG
      /* COMMENT THIS LINE TO DEBUG 
      if (sectionsEnum.Any(s => s.CourseSubject.StartsWith("ENGL") && s.CourseNumber == "101" && s.IsCommonCourse))
      {
        IEnumerable<SectionWithSeats> zSecs = sectionsEnum.Where(s => s.CourseSubject.StartsWith("ENGL") && s.CourseNumber == "101" && s.IsCommonCourse);
        foreach (SectionWithSeats zSec in zSecs)
        {
            string s1 = zSec.ID.ToString();
            Debug.Print("\n{0} - {1} {2}\t{3}\t(Crosslinks: {4})\n", zSec.ID, zSec.CourseID, zSec.IsCommonCourse ? "(&)" : string.Empty, zSec.CourseTitle,
                                                                 db.SectionCourseCrosslistings.Select(x => x.ClassID).Distinct().Count(x => x == s1));
        }
      }
      else
      {
        Debug.Print("\nENGL 101 - NOT FOUND AMONG SECTIONS.\n");
      }

      foreach (SectionWithSeats ss in sectionsEnum)
      {
          Debug.Print("\n{0} - {1} {2} - {3}, iscommoncourse: {4}\n", ss.CourseID, ss.CourseSubject, ss.CourseNumber, ss.CourseTitle, ss.IsCommonCourse);
      }
      // END DEBUGGING */
#endif

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

	  public static string GetFriendlyDayRange(string dayString)
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
      // HACK: If we don't obscure the "T" in Thursday, it will be replaced by the Tuesday evaluation, resulting in "Tuesdayhursday".
      friendlyName.Replace("X", "T");

      return friendlyName.ToString().TrimEnd('/');
    }

	  /// <summary>
	  /// Groups a list of Sections by course, into descriptive SectionBlocks
	  /// </summary>
	  /// <param name="sections">List of sections to group</param>
	  /// <param name="db"></param>
	  /// <returns>List of SectionBlock objects which describe the block of sections</returns>
	  public static IList<SectionsBlock> GroupSectionsIntoBlocks(IList<SectionWithSeats> sections, ClassScheduleDb db)
    {
#if DEBUG
  /* COMMENT THIS LINE TO DEBUG 
      if (sections.Any(s => s.CourseSubject.StartsWith("ENGL") && s.CourseNumber == "092"))
      {
        SectionWithSeats zengl266 = sections.Where(s => s.CourseSubject.StartsWith("ENGL") && s.CourseNumber == "092").First();
        Debug.Print("\n{0} - {1} {2}\t[{4}]\t(Linked to: {3})\n", zengl266.ID, zengl266.CourseID, zengl266.CourseTitle, zengl266.LinkedTo, zengl266.IsLinked ? "LINKED" : string.Empty);
      }
      else
      {
        Debug.Print("\nENGL 092 - NOT FOUND AMONG SECTIONS.\n");
      }
      // END DEBUGGING */
#endif
	    MiniProfiler profiler = MiniProfiler.Current;

      IList<SectionsBlock> results = new List<SectionsBlock>();
      //we need to captuer linked classes and exclude from the export:
      // Linked section defintion: An item number linking a class to another class in the next quarter. Automatic registration into the linked 
      //class occurs in batch registration for students enrolled in the class containing the ITM-YRQ-LINK.
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
#if DEBUG
  /* COMMENT THIS LINE TO DEBUG
        IEnumerable<SectionWithSeats> d1 = sections.Where(s => s.ID.ItemNumber.StartsWith("410"));
        // END DEBUGGING */
#endif
        nonLinkedSections = sections.Where(s => !s.IsLinked)
                          .OrderBy(s => s.CourseNumber)
                          .ThenBy(s => allLinkedSections.Where(l => l.LinkedTo == s.ID.ItemNumber).Count())
                          .ThenBy(s => s.CourseTitle)
                          .ThenByDescending(s => s.IsVariableCredits)     // johanna.aqui 9/11/2014 moved IsVariableCredits & Credits above starttime to keep correct section group
                          .ThenBy(s => s.Credits)
                          .ThenBy(s => s.IsOnline)
                          //.ThenBy(s => s.Offered.First().StartTime)
                          .ThenBy(s => (s.Offered != null && s.Offered.FirstOrDefault() != null && s.Offered.FirstOrDefault().StartTime != null ? s.Offered.First().StartTime : DateTime.MinValue)) // 7/20/17 Updated by Nicole S to consider situation where instruction(offered) data doesn't exists. This shouldn't happen if data is correct but it did throw an error with bad data.
                          .ThenBy(s => s.IsTelecourse)
                          .ThenBy(s => s.IsHybrid)
                          .ThenBy(s => s.IsOnCampus)
                          .ThenBy(s => s.SectionCode).ToList();

        //nonLinkedSections = sections.Where(s => !s.IsLinked)
        //                  .OrderBy(s => s.CourseNumber)
        //                  .ThenBy(s => allLinkedSections.Where(l => l.LinkedTo == s.ID.ItemNumber).Count())
        //                  .ThenBy(s => s.CourseTitle)
        //                  .ThenByDescending(s => s.IsVariableCredits)
        //                  .ThenBy(s => s.Credits)
        //                  .ThenBy(s => s.IsTelecourse)
        //                  .ThenBy(s => s.IsOnline)
        //                  .ThenBy(s => s.IsHybrid)
        //                  .ThenBy(s => s.IsOnCampus)
        //                  .ThenBy(s => s.SectionCode).ToList();
      }

#if DEBUG
  /* COMMENT THIS LINE TO DEBUG *
	    foreach (SectionWithSeats zs in nonLinkedSections.Where(s => s.CourseSubject.StartsWith("MATH") && s.CourseNumber == "097"))
	    {
	      Debug.Print("{0} - {1} {2}\t{3}\t(Linked sections: {4})", zs.ID, zs.CourseID, zs.CourseTitle, zs.IsLinked ? " [LINKED] " : string.Empty,
	                  allLinkedSections.Count(l => l.LinkedTo == zs.ID.ItemNumber));
	    }
      if (!allLinkedSections.Any(s => s.CourseSubject.StartsWith("ENGL") && s.CourseNumber == "266"))
	    {
	      Debug.Print("\nENGL 266 - NOT FOUND AMONG LINKED SECTIONS.");
	    }
	    else
	    {
        SectionWithSeats zengl266 = allLinkedSections.Where(s => s.CourseSubject.StartsWith("MATH") && s.CourseNumber == "097").First();
        Debug.Print("\n{0} - {1} {2}\t(Linked to: {3})", zengl266.ID, zengl266.CourseID, zengl266.CourseTitle, zengl266.LinkedTo);
      }

      if (!allLinkedSections.Any(s => s.CourseSubject.StartsWith("HD") && s.CourseNumber == "120"))
	    {
	      Debug.Print("\nHD 120 - NOT FOUND AMONG LINKED SECTIONS.");
	    }
	    else
	    {
        SectionWithSeats zengl246 = allLinkedSections.Where(s => s.CourseSubject.StartsWith("ENGL") && s.CourseNumber == "246").First();
        Debug.Print("\n{0} - {1} {2}\t(Linked to: {3})", zengl246.ID, zengl246.CourseID, zengl246.CourseTitle, zengl246.LinkedTo);
      }
      // END DEBUGGING */
#endif

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
          // TODO: Replace BuildCourseID() with this logic - and pull in CommonCourceChar from .config
          string blockCourseID = String.Format("{0}{1} {2}", firstSection.CourseSubject, firstSection.IsCommonCourse ? "&" : String.Empty, firstSection.CourseNumber);

          if (allLinkedSections.Any(l => l.LinkedTo == firstSection.ID.ItemNumber))
          {
            // If a Section has other Sections linked to it, then it should be displayed in its own block
            courseBlock.Sections = new List<SectionWithSeats> {firstSection};
          }
          else
          {
            courseBlock.Sections = remainingSections.TakeWhile(s =>
                                                               s.CourseID == firstSection.CourseID &&
                                                               s.CourseTitle == firstSection.CourseTitle &&
                                                               s.Credits == firstSection.Credits &&
                                                               s.IsVariableCredits == firstSection.IsVariableCredits &&
                                                               allLinkedSections.All(l => l.LinkedTo != s.ID.ItemNumber)
                                                               ).ToList();
          }

#if DEBUG
          /* COMMENT THIS LINE TO DEBUG
          // use the following variable as a conditional for breakpoints
          bool foo = courseBlock.Sections.Any(s => s.CourseSubject == "ACCT");
          // END DEBUGGING */

          /* COMMENT THIS LINE TO DEBUG
          Debug.Print("\nProcessing block: {0} - {1} {2}\t{3}\t(Crosslinks: {4})", firstSection.ID, firstSection.CourseID,
                                                                                     firstSection.IsCommonCourse ? "(&)" : string.Empty,
                                                                                     firstSection.CourseTitle,
                                                                                     crosslistings.Count(x => x.CourseID == blockCourseID));
          // END DEBUGGING */
#endif
          // Flag whether or not this course block is crosslisted with other sections offered in the same quarter
          courseBlock.IsCrosslisted = crosslistings.Any(x => x.CourseID == blockCourseID && x.ClassID.EndsWith(firstSection.Yrq.ID));

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

    // TODO: Remove BuildCourseID()
    // It should not be necessary, and undoes much of the work the API performs to make code less reliant on HP-specific formatting

    /// <summary>
    ///
    /// </summary>
    /// <param name="CourseNumber"></param>
    /// <param name="CourseSubject"></param>
    /// <param name="IsCommonCourse"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sec"></param>
    /// <returns></returns>
		public static string SubjectWithCommonCourseFlag(SectionWithSeats sec)
		{
			return SubjectWithCommonCourseFlag(sec.CourseSubject, sec.IsCommonCourse);
		}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="course"></param>
    /// <returns></returns>
		public static string SubjectWithCommonCourseFlag(Course course)
		{
			return SubjectWithCommonCourseFlag(course.Subject, course.IsCommonCourse);
		}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="course"></param>
    /// <returns></returns>
    public static string SubjectWithCommonCourseFlag(CourseID course)
    {
      return SubjectWithCommonCourseFlag(course.Subject, course.IsCommonCourse);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="isCommonCourse"></param>
    /// <returns></returns>
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

            return Regex.Replace(text, Regex.Escape(searchTerm), @"<em class='keyword'>$&</em>", RegexOptions.IgnoreCase);
		}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="yrq"></param>
    /// <returns></returns>
	  static private string GetYrqValueForBookstoreUrl(YearQuarter yrq)
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="linkedSections"></param>
    /// <returns></returns>
	  public static IList<SectionWithSeats> ParseCommonHeadingLinkedSections(List<SectionWithSeats> linkedSections)
	  {
	    string prevCourseID = String.Empty;
	    string prevTitle = String.Empty;
	    decimal prevCredits = 0;
	    bool prevIsVariableCredits = false;

	    IList<SectionWithSeats> common = new List<SectionWithSeats>(linkedSections.Count);

	    foreach (SectionWithSeats section in linkedSections)
	    {
	      if (!(section.CourseID == prevCourseID && ((Section)section).CourseTitle == prevTitle && section.Credits == prevCredits && section.IsVariableCredits == prevIsVariableCredits))
	      {
	        common.Add(section);
	      }

	      prevCourseID = section.CourseID;
	      prevTitle = ((Section)section).CourseTitle;
	      prevCredits = section.Credits;
	      prevIsVariableCredits = section.IsVariableCredits;
	    }

	    return common;
	  }

    // TODO: Move StripHtml() into CtcApi
    ///  <summary>
	  /// 
	  ///  </summary>
	  ///  <param name="withHtml"></param>
	  /// <param name="whitelistPattern"></param>
	  /// <returns></returns>
	  public static string StripHtml(string withHtml, string whitelistPattern = null)
	  {
	    string stripped;
	    if (String.IsNullOrWhiteSpace(whitelistPattern))
	    {
	      whitelistPattern = ConfigurationManager.AppSettings["CMSHtmlParsingAllowedElements"];
	    }
	    try
	    {
	      string pattern = @"</?(?(?=" + whitelistPattern +
	                       @")notag|[a-zA-Z0-9]+)(?:\s[a-zA-Z0-9\-]+=?(?:(["",']?).*?\1?)?)*\s*/?>";
	      stripped = Regex.Replace(withHtml, pattern, String.Empty);
	    }
	    catch (Exception ex)
	    {
	      stripped = Encoder.HtmlEncode(withHtml);
	      _log.Warn(m => m("Unable to remove HTML from string '{0}'\nReturning HTML-encoded string instead.\n{1}", withHtml, ex));
	    }
	    return stripped;
	  }

        /// <summary>
        /// Gets the course outcome information by scraping the Bellevue College
        /// course outcomes website
        /// </summary>
        public static dynamic GetCourseOutcome(ICourseID courseId)
        {
            string fullCourseID = BuildCourseID(courseId.Number, courseId.Subject.TrimEnd(), courseId.IsCommonCourse);
            string subject = courseId.Subject;
            if (courseId.IsCommonCourse) subject += "&";

            string courseOutcomes;
            try
            {
                Service1Client client = new Service1Client();
                string rawCourseOutcomes = client.GetCourseOutcome(subject, courseId.Number);

                if (rawCourseOutcomes.IndexOf("<li>", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    courseOutcomes = StripHtml(rawCourseOutcomes, "ul|UL|li|LI");
                }
                else
                {
                    courseOutcomes = StripHtml(rawCourseOutcomes, "");
                    string[] outcomeArray = courseOutcomes.Split('\n');

                    StringBuilder outcomes = new StringBuilder("<ul>");
                    foreach (string outcome in outcomeArray)
                    {
                        outcomes.AppendFormat("<li>{0}</li>", outcome);
                    }
                    outcomes.Append("</ul>");

                    courseOutcomes = outcomes.ToString();
                }
            }
            catch (Exception ex)
            {
                // TODO: log exception details
                courseOutcomes = "Error: Cannot find outcomes for this course or cannot connect to the course outcomes webservice.";
                _log.Warn(m => m("Unable to retrieve course outcomes for '{0}'", fullCourseID), ex);
                _log.Error(ex.InnerException);
                _log.Warn(ex.Message);
            }

            return courseOutcomes;
        }

        /// <summary>
        /// Allows user to enter "current" in URL in place of quarter
        /// </summary>
        /// <param name="quarter"></param>
        /// <param name="currentQuarter"></param>
        /// <param name="routeData"></param>
        /// <returns></returns>
        public static YearQuarter DetermineRegistrationQuarter(string quarter, YearQuarter currentQuarter, RouteData routeData)
	  {
	    YearQuarter yrq;
	    if (String.IsNullOrWhiteSpace(quarter))
	    {
	      yrq = null;
	    }
	    else
	    {
	      if (quarter.ToUpper() == "CURRENT")
	      {
	        yrq = currentQuarter;
	        routeData.Values["YearQuarter"] = yrq.FriendlyName.Replace(" ", string.Empty);
	      }
	      else
	      {
	        yrq = YearQuarter.FromFriendlyName(quarter);
	      }
	    }
	    return yrq;
	  }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="resource">The path and file to load over HTTP</param>
    /// <returns>A full URL to the 'globals' resource</returns>
	  public static string GlobalsHttp(string resource)
	  {
	    string globalsRoot = string.Empty;
	    try
	    {
	      globalsRoot = ConfigurationManager.AppSettings["Globals_UrlRoot"];
	    }
	    catch (System.Configuration.ConfigurationException ex)
	    {
	      _log.Error(m => m("Unable to retrieve 'Globals_UrlRoot' from appSettings: {0}", ex));
	    }

	    if (string.IsNullOrWhiteSpace(globalsRoot))
	    {
	      _log.Warn(m => m("'Globals_UrlRoot' was not found in appSettings, or was empty. Will likely result in an HTTP resource error."));
	      return resource;
	    }
	    string fullUrl = globalsRoot.CombineUrl(resource);
	    return fullUrl;
	  }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeString"></param>
    /// <returns></returns>
	  public static bool IsValidTimeString(string timeString)
    {
      // an empty value is "valid" - defaults will be used
      if (string.IsNullOrWhiteSpace(timeString)) return true;

	    string pattern = ConfigurationManager.AppSettings["Regex_Time"];

	    if (string.IsNullOrWhiteSpace(pattern))
	    {
	      pattern = @"^(?:0?[1-9]:[0-5]|1(?:[012]):[0-5])\d(?:\s?[ap]m)"; // default pattern - for 12/hour times
	    }

	    return Regex.IsMatch(timeString, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
	  }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pathAndFilename"></param>
    /// <returns></returns>
	  public static string GlobalsFile(string pathAndFilename)
	  {
	    return GlobalsPath.CombinePath(pathAndFilename);
	  }
	}
}
