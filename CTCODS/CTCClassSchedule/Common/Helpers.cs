using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Ctc.Ods;
using Ctc.Ods.Data;
using Ctc.Ods.Types;
using CTCClassSchedule.Models;
using MvcMiniProfiler;
using Ctc.Web.Security;
using System.Configuration;

namespace CTCClassSchedule.Common
{
	public static class Helpers
	{
		public static MvcHtmlString IncludePageURL(this HtmlHelper htmlHelper, string url)
		{

			return MvcHtmlString.Create(new WebClient().DownloadString(url));

		}

		public static string getBodyClasses(HttpContextBase Context)
		{
			string classes = "";

			if (Context.User.Identity.IsAuthenticated)
			{

				classes += ActiveDirectoryRoleProvider.IsUserInRoles(Context.User, "Developers") ? "role-developer" : "";
				classes += ActiveDirectoryRoleProvider.IsUserInRoles(Context.User, "InApp-Classes-Schedule-Editor") ? " role-schedule-editors " : "";
				classes += ActiveDirectoryRoleProvider.IsUserInRoles(Context.User, "InApp-Classes-Admin") ? " role-admin " : "";
			}
			else
			{
				classes = "not-logged-in";
			}

			return classes;
		}


		public static bool isAdmin(HttpContextBase Context)
		{
			bool isAdmin = false;
			if (Context.User.Identity.IsAuthenticated)
			{
				isAdmin = ActiveDirectoryRoleProvider.IsUserInRoles(Context.User, ConfigurationManager.AppSettings["ApplicationDeveloper"]) ? true : false;
				if (isAdmin == false)
				{
					isAdmin = ActiveDirectoryRoleProvider.IsUserInRoles(Context.User, ConfigurationManager.AppSettings["ApplicationAdmin"]) ? true : false;
				}

			}
			return isAdmin;
		}

		public static bool isEditor(HttpContextBase Context)
		{
			bool isEditor = false;
			if (Context.User.Identity.IsAuthenticated)
			{

				isEditor = ActiveDirectoryRoleProvider.IsUserInRoles(Context.User, ConfigurationManager.AppSettings["ApplicationDeveloper"]) ? true : false;
				if (isEditor == false)
				{
					isEditor = ActiveDirectoryRoleProvider.IsUserInRoles(Context.User, ConfigurationManager.AppSettings["ApplicationAdmin"]) ? true : false;
				}
				if (isEditor == false)
				{
					isEditor = ActiveDirectoryRoleProvider.IsUserInRoles(Context.User, ConfigurationManager.AppSettings["ApplicationEditor"]) ? true : false;
				}


			}
			return isEditor;
		}


		// TODO: Jeremy, another optional/BCC specific way of getting data - find another way?
		public static String getProfileURL(string SID)
		{
			Encryption64 en = new Encryption64();

			string returnString = "";
			returnString = "http://bellevuecollege.edu/directory/PersonDetails.aspx?PersonID=" + en.Encrypt(SID,"!#$a54?5");
			return returnString;

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

			int startHour = 0;
			int startMinute = 0;
			int endHour = 23;
			int endMinute = 59;

			//determine integer values for start/end time hours and minutes
			if (!String.IsNullOrWhiteSpace(timestart))
			{
				startHour = Convert.ToInt16(timestart.Substring(0, 2));
				startMinute = Convert.ToInt16(timestart.Substring(3, 2));
			}
			if (!String.IsNullOrWhiteSpace(timeend))
			{
				endHour = Convert.ToInt16(timeend.Substring(0, 2));
				endMinute = Convert.ToInt16(timeend.Substring(3, 2));
			}

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
						throw new System.FormatException("Number of credits was not a valid integer");
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
		/// Returns true/false if the value passed does not meet valid year requirements
		/// </summary>
		public static bool IsBadlyFormedYear(string year)
		{
			short yearNumber;

			if (Int16.TryParse(year, out yearNumber))
			{
				return (yearNumber < 1975 || yearNumber > 2030);
			}

			return true;
		}

		/// <summary>
		/// Gets the current year given some input params. Helper method for getFriendlyDateFromYRQ
		/// </summary>
		public static string getYearHelper(string quarter, string year1, string year2, string decade, bool isLastTwoQuarters)
		{
			string first2OfYear = "";
			string last2OfYear = "";
			string ThirdOfYear = "";

			int intYear1 = Convert.ToInt16(year1);
			int intYear2 = Convert.ToInt16(year2);

			switch (decade)
			{
				case "7":
					first2OfYear = "19";
					break;
				case "8":
					first2OfYear = "19";
					break;
				case "9":
					first2OfYear = isLastTwoQuarters == true ? "20" : "19";
					break;
				case "A":
					first2OfYear = "20";
					break;
				case "B":
					first2OfYear = "20";
					break;
				case "C":
					first2OfYear = "20";
					break;
				case "D":
					first2OfYear = "20";
					break;
				default:
					break;
			}

			switch (quarter)
			{
				case "1":
					last2OfYear = getDecadeIntegerFromString(decade) + intYear1.ToString();
					break;
				case "2":
					last2OfYear = getDecadeIntegerFromString(decade) + intYear1.ToString();
					break;
				case "3":
					ThirdOfYear = isLastTwoQuarters == true ? getDecadeIntegerFromString(getNextDecade(decade)) : getDecadeIntegerFromString(decade);
					last2OfYear = ThirdOfYear + intYear2.ToString();
					break;
				case "4":
					ThirdOfYear = isLastTwoQuarters == true ? getDecadeIntegerFromString(getNextDecade(decade)) : getDecadeIntegerFromString(decade);
					last2OfYear = ThirdOfYear + intYear2.ToString();
					break;
				default:

					break;

			}

			return first2OfYear + last2OfYear;

		}

		/// <summary>
		/// Gets the friendly decade value from the HP decade (A = 2000's, B = 2010's). Helper method for getYearHelper
		/// </summary>
		static public string getDecadeIntegerFromString(string decade)
		{
			switch (decade)
			{
				case "7":
					return "7";
				case "8":
					return "8";
				case "9":
					return "9";
				case "A":
					return "0";
				case "B":
					return "1";
				case "C":
					return "2";
				case "D":
					return "3";
			}
			return "";
		}

		/// <summary>
		/// Gets the next decade in HP format (8, 9, A, B)
		/// </summary>
		static public string getNextDecade(string decade)
		{
			switch (decade)
			{
				case "7":
					return "8";
				case "8":
					return "9";
				case "9":
					return "A";
				case "A":
					return "B";
				case "B":
					return "C";
				case "C":
					return "D";
				case "D":
					return "E";
			}
			return "";

		}

		/// <summary>
		///
		/// </summary>
		/// <param name="fieldID"></param>
		/// <param name="fieldTitle"></param>
		/// <param name="fieldValue"></param>
		/// <returns></returns>
		static public ModalityFacetInfo getModalityInfo(string fieldID, string fieldTitle, string fieldValue)
		{
			return new ModalityFacetInfo
				{
						ID = fieldID,
						Title = fieldTitle,
						Selected = Utility.SafeConvertToBool(fieldValue)
				};
		}

		static public IList<ModalityFacetInfo> ConstructModalityList(IEnumerable<SectionWithSeats> sections, string f_oncampus, string f_online, string f_hybrid, string f_telecourse)
		{
			IList<ModalityFacetInfo> modality = new List<ModalityFacetInfo>(4);

/*
 * Disabled contextual showing/hiding of facets because this functionality needs much more
 * discussion, planning and design. - 9/21/2011, shawn.south@bellevuecollege.edu
 *
 * See also: Bug 154
 *
			if (sections.Where(s => s.IsOnCampus).Count() > 0)
			{
				modality.Add(getModalityInfo("f_oncampus", "On Campus", f_oncampus) );
			}
			if (sections.Where(s => s.IsOnline).Count() > 0)
			{
				modality.Add(getModalityInfo("f_online", "Online", f_online));
			}
			if (sections.Where(s => s.IsHybrid).Count() > 0)
			{
				modality.Add(getModalityInfo("f_hybrid", "Hybrid", f_hybrid));
			}
			if (sections.Where(s => s.IsTelecourse).Count() > 0)
			{
				modality.Add(getModalityInfo("f_telecourse", "Telecourse", f_telecourse));
			}
*/
			modality.Add(getModalityInfo("f_oncampus", "On Campus", f_oncampus) );
			modality.Add(getModalityInfo("f_online", "Online", f_online));
			modality.Add(getModalityInfo("f_hybrid", "Hybrid", f_hybrid));
			modality.Add(getModalityInfo("f_telecourse", "Telecourse", f_telecourse));

			return modality;
		}

		/// <summary>
		/// Gets the <see cref="YearQuarter"/> for the current, and previous 3 quarters.
		/// This drives the dynamic YRQ navigation bar
		/// </summary>
		static public IList<YearQuarter> getYearQuarterListForMenus(OdsRepository respository)
		{
			IList<YearQuarter> currentFutureQuarters = respository.GetRegistrationQuarters(4);
			return currentFutureQuarters;
		}

		/// <summary>
		/// Gets the current http get/post params and assigns them to an IDictionary<string, object>
		/// This is mainly used to set a Viewbag variable so these can be passed into action links in the views.
		/// </summary>
		/// <param name="httpRequest"></param>
		static public IDictionary<string, object> getLinkParams(HttpRequestBase httpRequest)
		{
			IDictionary<string, object> linkParams = new Dictionary<string, object>(httpRequest.QueryString.Count);
			foreach (string key in httpRequest.QueryString.AllKeys)
			{
				if (key != "X-Requested-With")
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
				if (linkParams.ContainsKey(key))
				{
					linkParams[key] = httpRequest.Form[key];
				}
				else
				{
					linkParams.Add(key, httpRequest.Form[key]);
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
		public static IList<SectionWithSeats> getSectionsWithSeats(string currentYrq, IList<Section> sections, ClassScheduleDb db)
		{
			MiniProfiler profiler = MiniProfiler.Current;
            IList<SectionWithSeats> sectionsEnum;

			// ensure we're ALWAYS getting the latest data from the database
			// Reference: http://forums.asp.net/post/2848021.aspx
			db.vw_ClassScheduleData.MergeOption = MergeOption.OverwriteChanges;

            string yrqID = YearQuarter.FromString(currentYrq).ID;

			IList<vw_ClassScheduleData> classScheduleData;
            using (profiler.Step("API::Get Class Schedule Specific Data()"))
            {
            classScheduleData = (from c in db.vw_ClassScheduleData

			                    where c.YearQuarterID == yrqID // && c.ClassID == "0917B123"
			                    select c
			                ).ToList();
            }

            using (profiler.Step("Joining all data"))
            {
                sectionsEnum = (
			            from c in sections
			            join d in classScheduleData on c.ID.ToString() equals d.ClassID into cd
			            from d in cd.DefaultIfEmpty()
			            orderby c.Yrq.ID descending
			            select new SectionWithSeats
						{
						    ParentObject = c,
						    SeatsAvailable = d != null ? d.SeatsAvailable : int.MinValue,	// allows us to identify past quarters (with no availability info)
						    LastUpdated = (d != null ? d.LastUpdated.GetValueOrDefault() : DateTime.MinValue).ToString("h:mm tt").ToLower(),
						    SectionFootnotes = d != null ? d.SectionFootnote ?? string.Empty : string.Empty,
						    CourseFootnotes = d != null ? d.CourseFootnote ?? string.Empty : string.Empty
						}).ToList();
            }

			return sectionsEnum;
		}

		public static string getFriendlyDayRange(string dayString, Dictionary<string, string> dict)
		{
			string friendlyName;
			if (dict.ContainsKey(dayString))
			{
				friendlyName = dict[dayString];
			}
			else
			{
				friendlyName = dayString;
			}
			return friendlyName;
		}

		public static Dictionary<string, string> getDayDictionary(){
			Dictionary<string, string> dict = new Dictionary<string, string>();

			dict.Add("Online", "Online");
			dict.Add("M", "Monday");
			dict.Add("T", "Tuesday");
			dict.Add("W", "Wednesday");
			dict.Add("Th", "Thursday");
			dict.Add("F", "Friday");
			dict.Add("Sa", "Saturday");
			dict.Add("DAILY", "Daily");
			dict.Add("MWF", "Monday/Wednesday/Friday");
			dict.Add("TTh", "Tuesday/Thursday");
			dict.Add("MWThF", "Monday/Wednesday/Thursday/Friday");
			dict.Add("MTWTh", "Monday/Tuesday/Wednesday/Thursday");
			dict.Add("MW", "Monday/Wednesday");
			dict.Add("WF", "Wednesday/Friday");
			dict.Add("TF", "Tuesday/Friday");
			dict.Add("MF", "Monday/Friday");
			dict.Add("ThF", "Thursday/Friday");
			dict.Add("MWTh", "Monday/Wednesday/Thursday");
			dict.Add("MTWF", "Monday/Tuesday/Wednesday/Friday");
			dict.Add("MTTh", "Monday/Tuesday/Thursday");
			dict.Add("WTh", "Wednesday/Thursday");
			dict.Add("MTF", "Monday/Tuesday/Friday");
			dict.Add("MT", "Monday/Tuesday");
			dict.Add("TWThF", "Tuesday/Wednesday/Thursday/Friday");
			dict.Add("ARRANGED", "Arranged");
			dict.Add("Su", "Sunday");
			dict.Add("TThF", "Tuesday/Thursday/Friday");
			dict.Add("TW", "Tuesday/Wednesday");
			dict.Add("MTh", "Monday/Thursday");
			dict.Add("MTW", "Monday/Tuesday/Wednesday");
			dict.Add("MThF", "Monday/Thursday/Friday");
			dict.Add("TWTh", "Tuesday/Wednesday/Thursday");
			dict.Add("TWF", "Tuesday/Wednesday/Friday");
			dict.Add("WThF", "Wednesday/Thursday/Friday");
			dict.Add("MTThF", "Monday/Tuesday/Thursday/Friday");
			dict.Add("TSa", "Tuesday/Saturday");
			dict.Add("ThSa", "Thursday/Saturday");
			dict.Add("FSa", "Friday/Saturday");
			dict.Add("ThSu", "Thursday/Sunday");
			dict.Add("MSu", "Monday/Sunday");
			dict.Add("WSa", "Wednesday/Saturday");
			dict.Add("MWSa", "Monday/Wednesday/Saturday");
			dict.Add("TThSa", "Tuesday/Thursday/Saturday");
			dict.Add("WThSa", "Wednesday/Thursday/Saturday");
			dict.Add("SaSu", "Saturday/Sunday");
			dict.Add("WSu", "Wednesday/Sunday");
			dict.Add("TWThSa", "Tuesday/Wednesday/Thursday/Saturday");
			dict.Add("MTWThFSaSu", "Monday/Tuesday/Wednesday/Thursday/Friday/Saturday/Sunday");
			dict.Add("WFSa", "Wednesday/Friday/Saturday");
			dict.Add("MTWThSa", "Monday/Tuesday/Wednesday/Thursday/Saturday");
			dict.Add("MTSaSu", "Monday/Tuesday/Saturday/Sunday");
			dict.Add("FSaSu", "Friday/Saturday/Sunday");
			dict.Add("MFSa", "Monday/Friday/Saturday");
			dict.Add("ThFSa", "Thursday/Friday/Saturday");
			dict.Add("MTWThFSa", "Monday/Tuesday/Wednesday/Thursday/Friday/Saturday");
			dict.Add("MSa", "Monday/Saturday");
			dict.Add("TSu", "Tuesday/Sunday");
			dict.Add("TWFSa", "Tuesday/Wednesday/Friday/Saturday");
			dict.Add("TFSu", "Tuesday/Friday/Sunday");
			dict.Add("TThSu", "Tuesday/Thursday/Sunday");
			dict.Add("TThSaSu", "Tuesday/Thursday/Saturday/Sunday");
			dict.Add("MWFSa", "Monday/Wednesday/Friday/Saturday");
			dict.Add("TWThSaSu", "Tuesday/Wednesday/Thursday/Saturday/Sunday");
			dict.Add("TWFSaSu", "Tuesday/Wednesday/Friday/Saturday/Sunday");
			dict.Add("MTSu", "Monday/Tuesday/Sunday");
			dict.Add("TFSa", "Tuesday/Friday/Saturday");
			dict.Add("TWThFSa", "Tuesday/Wednesday/Thursday/Friday/Saturday");
			dict.Add("TFSaSu", "Tuesday/Friday/Saturday/Sunday");
			dict.Add("TWSa", "Tuesday/Wednesday/Saturday");
			dict.Add("ThSaSu", "Thursday/Saturday/Sunday");
			dict.Add("MTWSa", "Monday/Tuesday/Wednesday/Saturday");
			dict.Add("MWSaSu", "Monday/Wednesday/Saturday/Sunday");
			dict.Add("FSu", "Friday/Sunday");
			dict.Add("WThFSa", "Wednesday/Thursday/Friday/Saturday");
			dict.Add("TWThSu", "Tuesday/Wednesday/Thursday/Sunday");
			dict.Add("TWThFSu", "Tuesday/Wednesday/Thursday/Friday/Sunday");
			dict.Add("MWThSa", "Monday/Wednesday/Thursday/Saturday");
			dict.Add("ThFSaSu", "Thursday/Friday/Saturday/Sunday");
			dict.Add("ThFSu", "Thursday/Friday/Sunday");
			dict.Add("MTWThFSu", "Monday/Tuesday/Wednesday/Thursday/Friday/Sunday");
			dict.Add("TSaSu", "Tuesday/Saturday/Sunday");
			dict.Add("WFSu", "Wednesday/Friday/Sunday");
			dict.Add("MTThSa", "Monday/Tuesday/Thursday/Saturday");
			dict.Add("MTWThSaSu", "Monday/Tuesday/Wednesday/Thursday/Saturday/Sunday");
			dict.Add("MTSa", "Monday/Tuesday/Saturday");
			dict.Add("WFSaSu", "Wednesday/Friday/Saturday/Sunday");
			dict.Add("MSaSu", "Monday/Saturday/Sunday");
			dict.Add("MThSa", "Monday/Thursday/Saturday");
			dict.Add("MFSaSu", "Monday/Friday/Saturday/Sunday");
			dict.Add("TWSu", "Tuesday/Wednesday/Sunday");
			dict.Add("TThFSa", "Tuesday/Thursday/Friday/Saturday");
			dict.Add("MTWFSa", "Monday/Tuesday/Wednesday/Friday/Saturday");
			dict.Add("MTWThSu", "Monday/Tuesday/Wednesday/Thursday/Sunday");
			dict.Add("TThFSaSu", "Tuesday/Thursday/Friday/Saturday/Sunday");
			dict.Add("MTWSu", "Monday/Tuesday/Wednesday/Sunday");
			dict.Add("MWSu", "Monday/Wednesday/Sunday");
			dict.Add("MTFSaSu", "Monday/Tuesday/Friday/Saturday/Sunday");
			dict.Add("MTFSa", "Monday/Tuesday/Friday/Saturday");
			dict.Add("MWThFSa", "Monday/Wednesday/Thursday/Friday/Saturday");

			return dict;
		}
	}
}
