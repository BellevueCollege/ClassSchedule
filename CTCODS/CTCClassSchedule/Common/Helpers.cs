using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using Ctc.Ods;

namespace CTCClassSchedule.Common
{
	public static class Helpers
	{
		public static MvcHtmlString IncludePageURL(this HtmlHelper htmlHelper, string url)
		{
			return MvcHtmlString.Create(new WebClient().DownloadString(url));

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
		static public IList<ISectionFacet> addFacets(string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string f_telecourse, string avail)
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
	}
}