using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using Ctc.Ods;
using Ctc.Ods.Data;
using Ctc.Ods.Types;
using CTCClassSchedule.Models;



namespace CTCClassSchedule.Controllers
{
	public class ClassesController : Controller
	{
		private ClassScheduleDevEntities _scheduledb = new ClassScheduleDevEntities();
		private ClassScheduleDevProgramEntities _programdb = new ClassScheduleDevProgramEntities();

		#region controller actions

		/// <summary>
		/// GET: /Classes/
		/// </summary>
		public ActionResult Index()
		{
			using (OdsRepository respository = new OdsRepository())
			{
				getCurrentFutureYRQs(respository);
			}
			return View();
		}


		/// <summary>
		/// GET: /Classes/All
		/// </summary>
		public ActionResult AllClasses(string letter)
		{

			setViewBagVars("", "", "", "", "", letter);
			ViewBag.WhichClasses = (letter == null || letter == "" ? " (All)" : " (" + letter.ToUpper() + ")");
			ViewBag.AlphabetArray = new bool[26];
			ViewBag.AlphabetCharacter = 0;
			ViewBag.Title = "All classes";

			using (OdsRepository respository = new OdsRepository())
			{
				getCurrentFutureYRQs(respository);
				IList<CoursePrefix> courses = respository.GetCourseSubjects();
				IEnumerable<String> alphabet;

				ViewBag.AlphabetArray = new bool[26];


				alphabet = from c in courses
									 orderby c.Title
									 select c.Title.Substring(0, 1);

				alphabet = alphabet.Distinct();
				ViewBag.Alphabet = alphabet;


				if (letter != null)
				{
					IEnumerable<CoursePrefix> coursesEnum;
					coursesEnum = from c in courses
												where c.Title.StartsWith(letter, StringComparison.OrdinalIgnoreCase)
												select c;

					return View(coursesEnum);
				}
				else
				{
					View(courses);
				}
			}
			return View();
		}

		/// <summary>
		/// GET: /Classes/All/{Subject}/
		/// </summary>
		public ActionResult Subject(string Subject, string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string f_telecourse, string avail)
		{
			ViewBag.Subject = Subject;
			setViewBagVars("", "", "", "", avail, "");
			ViewBag.Title = "All " +  @Subject + " classes";
			IList<ISectionFacet> facets = addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s, f_oncampus, f_online, f_hybrid, f_telecourse, avail);

			ViewBag.ProgramUrl = getProgramUrl(Subject);



			using (OdsRepository respository = new OdsRepository())
			{
				getCurrentFutureYRQs(respository);

				IList<Course> courses = respository.GetCourses(facets);

				if (Subject != null)
				{
					IEnumerable<Course> coursesEnum;
					coursesEnum = (from c in courses
												where c.Subject == Subject.ToUpper()
												select c).OrderBy(c => c.Subject).ThenBy(c => c.Number);
					ViewBag.ItemCount = coursesEnum.Count();

					return View(coursesEnum);
				}
				else
				{
					View();
				}
			}
			return View();
		}



		/// <summary>
		/// GET: /Classes/{FriendlyYRQ}/
		/// </summary>

		public ActionResult YearQuarter(String YearQuarter, string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string f_telecourse, string avail, string letter)
		{
			ViewBag.WhichClasses = (letter == null || letter == "" ? " (All)" : " (" + letter.ToUpper() + ")");
			setViewBagVars(YearQuarter, "", "", "", avail, letter);
			ViewBag.AlphabetArray = new bool[26];
			ViewBag.Subject = "All";
			ViewBag.AlphabetCharacter = 0;
			ViewBag.YRQ = Ctc.Ods.Types.YearQuarter.FromString(getYRQFromFriendlyDate(YearQuarter)).ToString();
			ViewBag.FriendlyYRQ = getFriendlyDateFromYRQ(Ctc.Ods.Types.YearQuarter.FromString(getYRQFromFriendlyDate(YearQuarter)));
			ViewBag.Title = ViewBag.Subject + "Classes for " + @ViewBag.Yearquarter;
			IList<ISectionFacet> facets = addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s, f_oncampus, f_online, f_hybrid, f_telecourse, avail);

			using (OdsRepository respository = new OdsRepository())
			{
				getCurrentFutureYRQs(respository);
				YearQuarter YRQ = Ctc.Ods.Types.YearQuarter.FromString(getYRQFromFriendlyDate(YearQuarter));

				IList<CoursePrefix> courses = respository.GetCourseSubjects(YRQ, facetOptions: facets);
				ViewBag.ItemCount = courses.Count();

				IEnumerable<String> alphabet;
				ViewBag.AlphabetArray = new bool[26];

				alphabet = from c in courses
									 orderby c.Title
									 select c.Title.Substring(0, 1);

				alphabet = alphabet.Distinct();
				ViewBag.Alphabet = alphabet;

				if (letter != null)
				{
					IEnumerable<CoursePrefix> coursesEnum;
					coursesEnum = from c in courses
												where c.Title.StartsWith(letter, StringComparison.OrdinalIgnoreCase)
												select c;

					coursesEnum = coursesEnum.Distinct();
					ViewBag.ItemCount = coursesEnum.Count();

					return View(coursesEnum);
				}
				else
				{
					View(courses);
				}
				return View();
			}

		}

		/// <summary>
		/// GET: /Classes/{FriendlyYRQ}/{Subject}/
		/// </summary>
		public ActionResult YearQuarterSubject(String YearQuarter, string Subject, string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string f_telecourse, string avail)
		{
			setViewBagVars(YearQuarter, "", "", "", avail, "");

			ViewBag.timestart = timestart;
			ViewBag.timeend = timeend;
			ViewBag.day_su = day_su;
			ViewBag.day_m = day_m;
			ViewBag.day_t = day_t;
			ViewBag.day_w = day_w;
			ViewBag.day_th = day_th;
			ViewBag.day_f = day_f;
			ViewBag.day_s = day_s;
			ViewBag.f_oncampus = f_oncampus;
			ViewBag.f_online = f_online;
			ViewBag.f_hybrid = f_hybrid;
			ViewBag.f_telecourse = f_telecourse;
			ViewBag.avail = avail;



			ViewBag.displayedCourseNum = 0;
			ViewBag.seatAvailbilityDisplayed = false;
			ViewBag.Subject = Subject;
			ViewBag.Title = @ViewBag.Yearquarter + " " + @Subject + " classes";
			IList<ISectionFacet> facets = addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s, f_oncampus, f_online, f_hybrid, f_telecourse, avail);



			using (OdsRepository respository = new OdsRepository())
			{
				getCurrentFutureYRQs(respository);
				YearQuarter YRQ = Ctc.Ods.Types.YearQuarter.FromString(getYRQFromFriendlyDate(YearQuarter));

				var seatsAvailableLocal = (from s in _scheduledb.vw_SeatAvailability
																	 select s);

				//IList<Section> sections = respository.GetSections(Subject, YRQ);
				IList<Section> sections = respository.GetSections(Subject, YRQ, facets);
				ViewBag.ItemCount = sections.Count();




				IEnumerable<SectionWithSeats> sectionsEnum;
				sectionsEnum = (
											from c in sections
											join d in seatsAvailableLocal on c.ID.ToString() equals d.ClassID
											where c.CourseSubject == Subject.ToUpper()
											&& c.Yrq.ToString() == YRQ.ToString()
											select new SectionWithSeats
											{
													ParentObject = c,
													SeatsAvailable = d.SeatsAvailable,
													LastUpdated = this.getFriendlyTime(Convert.ToDateTime(d.LastUpdated)),
											}
				               );

				ViewBag.ItemCount = sectionsEnum.Count();
				return View(sectionsEnum);
			}
		}



		/// <summary>
		/// GET: /Classes/{FriendlyYRQ}/{Subject}/{ClassNum}
		/// </summary>
		//public ActionResult YRQClassDetails(string YearQuarterID, string Subject, string ClassNum)
		//{

		//  string courseID = string.Concat(Subject, string.Concat(" ", ClassNum));
		//  ViewBag.displayedCourseNum = 0;

		//  using (OdsRepository respository = new OdsRepository())
		//  {
		//    getCurrentFutureYRQs(respository);

		//    if (courseID != null)
		//    {
		//      IList<Section> sections = respository.GetSections(CourseID.FromString(courseID),  Ctc.Ods.Types.YearQuarter.FromString(YearQuarterID));
		//      ViewBag.ItemCount = sections.Count();
		//      return View(sections);
		//    }
		//    else
		//    {
		//      return View();
		//    }
		//  }
		//}

		/// <summary>
		/// GET: /Classes/All/{Subject}/{ClassNum}
		/// </summary>
		public ActionResult ClassDetails(string YearQuarterID, string Subject, string ClassNum)
		{
			string courseID = string.Concat(Subject, string.Concat(" ", ClassNum));
			ViewBag.titleDisplayed = false;
			ViewBag.seatAvailbilityDisplayed = false;
			ViewBag.CourseOutcome = getCourseOutcome(Subject, ClassNum);
			ViewBag.Title = @Subject + " " + ClassNum + " sections";
			ViewBag.ProgramUrl = getProgramUrl(Subject);


			using (OdsRepository respository = new OdsRepository())
			{
				getCurrentFutureYRQs(respository);

				if (courseID != null)
				{
					IList<CourseDescription> courseDescriptions = respository.GetCourseDescription(CourseID.FromString(courseID));

					foreach(CourseDescription desc in courseDescriptions) {
						ViewBag.CourseDescription = desc.Description;

					}

					var seatsAvailableLocal = (from s in _scheduledb.vw_SeatAvailability

					                           select s);

					// TODO: move this declaration somewhere it can more easily be re-used
					IList<ISectionFacet> facets = new List<ISectionFacet> {new RegistrationQuartersFacet(-4)};	// Current & previous 3 quarters

					IList<Section> sections = respository.GetSections(CourseID.FromString(courseID), facetOptions: facets);

					IEnumerable<SectionWithSeats> sectionsEnum;
					sectionsEnum = (
								from c in sections
								join d in seatsAvailableLocal on c.ID.ToString() equals d.ClassID
								where c.CourseSubject == Subject.ToUpper()
								select new SectionWithSeats
									{
																				ParentObject = c,
											SeatsAvailable = d.SeatsAvailable,
											LastUpdated = this.getFriendlyTime(Convert.ToDateTime(d.LastUpdated)),
																				Footnotes = c.Footnotes,
									}

					               );





					return View(sectionsEnum);
				}
				else
				{
					return View();
				}
			}
		}


		[HttpPost]
		public ActionResult getSeats(string courseIdPlusYRQ)
		{
			int? seats = null;
			string friendlyTime = "";

			string classID = courseIdPlusYRQ.Substring(0, 4);
			string yrq = courseIdPlusYRQ.Substring(4, 4);


			CourseHPQuery query = new CourseHPQuery();
			int HPseats = query.findOpenSeats(classID, yrq);
			string HPseatsAvailable = seats.ToString();




			var seatsAvailableLocal =	from s in _scheduledb.SeatAvailabilities
							where s.ClassID == courseIdPlusYRQ
							select s;
			int rows = seatsAvailableLocal.Count();

			if (rows == 0)
			{
				//insert the value
				SeatAvailability newseat = new SeatAvailability();
				newseat.ClassID = courseIdPlusYRQ;
				newseat.SeatsAvailable = HPseats;
				newseat.LastUpdated = DateTime.Now;

				_scheduledb.SeatAvailabilities.AddObject(newseat);


			}
			else
			{
				//update the value
				foreach (SeatAvailability seat in seatsAvailableLocal)
				{
					seat.SeatsAvailable = HPseats;
					seat.LastUpdated = DateTime.Now;
				}

			}

			_scheduledb.SaveChanges();

			var seatsAvailable = from s in _scheduledb.vw_SeatAvailability
			                     where s.ClassID == courseIdPlusYRQ
			                     select s;

			foreach (var seat in seatsAvailable)
			{
				seats = seat.SeatsAvailable;

				DateTime lastUpdated = Convert.ToDateTime(seat.LastUpdated);
				friendlyTime = getFriendlyTime(lastUpdated);
			}


			var jsonReturnValue = seats.ToString() + "|" + friendlyTime;
			return Json(jsonReturnValue);
		}


		#endregion




		#region helper methods




		/// <summary>
		/// Gets the current <see cref="YearQuarter"/> and assigns ViewBag variables
		/// for the current, +1, +2 quarters. This drives the dynamic YRQ navigation bar
		/// </summary>
		private void getCurrentFutureYRQs(OdsRepository respository)
		{
			IList<YearQuarter> currentFutureQuarters;
			currentFutureQuarters = respository.GetRegistrationQuarters(3);
			ViewBag.QuarterOne = currentFutureQuarters[0];
			ViewBag.QuarterTwo = currentFutureQuarters[1];
			ViewBag.QuarterThree = currentFutureQuarters[2];

			ViewBag.QuarterOneFriendly = getFriendlyDateFromYRQ(currentFutureQuarters[0]);
			ViewBag.QuarterTwoFriendly = getFriendlyDateFromYRQ(currentFutureQuarters[1]);
			ViewBag.QuarterThreeFriendly = getFriendlyDateFromYRQ(currentFutureQuarters[2]);

			ViewBag.QuarterOneURL = ViewBag.QuarterOneFriendly.Replace(" ", "");
			ViewBag.QuarterTwoURL = ViewBag.QuarterTwoFriendly.Replace(" ", "");
			ViewBag.QuarterThreeURL = ViewBag.QuarterThreeFriendly.Replace(" ", "");

		}

		// TODO: Jeremy, make optional AND configurable in web.config
		/// <summary>
		/// Gets the course outcome information by scraping the Bellevue College
		/// course outcomes website
		/// </summary>
		private dynamic getCourseOutcome(string Subject, string ClassNum)
		{
			string url = "http://bellevuecollege.edu/courseoutcomes/?CourseID=" + Subject + "%20" + ClassNum;
			StringBuilder sb = new StringBuilder();

			byte[] buffer = new byte[8000];
			HttpWebRequest request = (HttpWebRequest)
			                         WebRequest.Create(url);

			// execute the request
			HttpWebResponse response = (HttpWebResponse)
			                           request.GetResponse();

			// we will read data via the response stream
			Stream resStream = response.GetResponseStream();

			string tempString = null;
			int count = 0;

			do
			{
				// fill the buffer with data
				count = resStream.Read(buffer, 0, buffer.Length);

				// make sure we read some data
				if (count != 0)
				{
					// translate from bytes to ASCII text
					tempString = Encoding.ASCII.GetString(buffer, 0, count);

					// continue building the string
					sb.Append(tempString);
				}
			}
			while (count > 0); // any more data to read?

			// return course outcome page source
			return sb.ToString();



		}

		/// <summary>
		/// Sets all of the common ViewBag variables
		/// </summary>
		private void setViewBagVars(string YearQuarter, string flex, string time, string days, string avail, string letter)
		{
			if (ConfigurationManager.AppSettings != null)
			{
				ViewBag.currentAppSubdirectory = ConfigurationManager.AppSettings["currentAppSubdirectory"];
			}
			ViewBag.ErrorMsg = "";

			ViewBag.YearQuarter = YearQuarter;
			if (YearQuarter != "")
			{
				ViewBag.YearQuarterHP = getYRQFromFriendlyDate(YearQuarter);

			}

			ViewBag.YearQuarter_a_to_z = "/" + YearQuarter;
			ViewBag.letter = letter;
			ViewBag.YearQuarter = YearQuarter ?? "all";
			ViewBag.flex = flex ?? "all";
			ViewBag.time = time ?? "all";
			ViewBag.days = days ?? "all";
			ViewBag.avail = avail ?? "all";


			ViewBag.activeClass = " class=active";
			ViewBag.currentUrl = Request.Url.AbsolutePath;


			//create the GET string for links
			ViewBag.queryStringNoFlex = "&time=" + ViewBag.time + "&days=" + ViewBag.days + "&avail=" + ViewBag.avail + "&letter=" + letter;
			ViewBag.queryStringNoTimes = "&flex=" + ViewBag.flex + "&days=" + ViewBag.days + "&avail=" + ViewBag.avail + "&letter=" + letter;
			ViewBag.queryStringNoDays = "&flex=" + ViewBag.flex + "&time=" + ViewBag.time + "&avail=" + ViewBag.avail + "&letter=" + letter;
			ViewBag.queryStringNoAvail = "&flex=" + ViewBag.flex + "&time=" + ViewBag.time + "&days=" + ViewBag.days + "&letter=" + letter;
			ViewBag.queryStringNoLetter = "&flex=" + ViewBag.flex + "&time=" + ViewBag.time + "&days=" + ViewBag.days + "&avail=" + avail;
			ViewBag.queryStringAll = "&flex=" + ViewBag.flex + "&time=" + ViewBag.time + "&days=" + ViewBag.days + "&avail=" + ViewBag.avail + "&letter=" + letter;


			ViewBag.ActiveFlexAll = "";
			ViewBag.ActiveFlexOnline = "";
			ViewBag.ActiveFlexHybrid = "";
			ViewBag.ActiveFlexTelecourse = "";
			ViewBag.ActiveFlexReducedTrip = "";
			ViewBag.ActiveFlexAll = "";



			ViewBag.QuarterURL = YearQuarter;

		}

		/// <summary>
		/// Converts a friendly YRQ (Fall2011) into a <see cref="YearQuarter"/>
		/// </summary>
		private string getYRQFromFriendlyDate(string friendlyDate)
		{
			//Example: Winter 2008 = A783

			//summer: xxx1
			//fall:   xxx2
			//winter: xxx3
			//spring:	xxx4

			//Academic year 2006-2007: x67x
			//Academic year 2011-2012: x12x

			string year = friendlyDate.Substring(friendlyDate.Length - 4);  //2011
			string quarterFriendly = friendlyDate.Substring(0, friendlyDate.Length - 4); //Spring
			string decade = "";
			int yearBaseTen = 0;
			int yearBaseTenPlusOne = 0;
			string YearQuarter1 = "";
			string YearQuarter23 = "";
			string quarter = "";
			bool isLastTwoQuarters = false;
			bool badlyFormedQuarter = false;
			bool badlyFormedYear = false;

			if (IsInteger(year))
			{
				if (Convert.ToInt16(year) < 1975 || Convert.ToInt16(year) > 2030)
				{
					badlyFormedYear = true;
				}
				else
				{
					yearBaseTen = Convert.ToInt16(year.Substring(3, 1));
					decade = year.Substring(0, 3); //201
				}
			}

			//determine the quarter in string form "1", "2", etc... essentially the xxx2 character
			switch (quarterFriendly.ToLower())
			{
				case "summer":
					quarter = "1";
					break;
				case "fall":
					quarter = "2";
					break;
				case "winter":
					quarter = "3";
					break;
				case "spring":
					quarter = "4";
					break;
				default:
					badlyFormedQuarter = true;
					break;

			}

			if (!badlyFormedQuarter && !badlyFormedYear)
			{




				//is the year an overlapping year? e.g. spring 2000 = 9904 but summer 2000 = A011
				if (yearBaseTen == 0 && (quarter == "3" || quarter == "4"))
				{
					isLastTwoQuarters = true;
				}

				//find out which decade it is in, to determine Axxx (first character in string)
				switch (decade)
				{
					case "197":
						YearQuarter1 = isLastTwoQuarters == true ? "6" : "7";
						break;
					case "198":
						YearQuarter1 = isLastTwoQuarters == true ? "7" : "8";
						break;
					case "199":
						YearQuarter1 = isLastTwoQuarters == true ? "8" : "9";
						break;
					case "200":
						YearQuarter1 = isLastTwoQuarters == true ? "9" : "A";
						break;
					case "201":
						YearQuarter1 = isLastTwoQuarters == true ? "A" : "B";
						break;
					case "202":
						YearQuarter1 = isLastTwoQuarters == true ? "B" : "C";
						break;


				}

				//figure out what the x23x portion of the YRQ is
				if (quarter == "1" || quarter == "2")
				{
					if (yearBaseTen + 1 > 9)
					{
						yearBaseTenPlusOne = yearBaseTen + 1 - 10;
					}
					else
					{
						yearBaseTenPlusOne = yearBaseTen + 1;
					}
					YearQuarter23 = Convert.ToString(yearBaseTen) + Convert.ToString((yearBaseTenPlusOne));

				}
				else if (quarter == "3" || quarter == "4")
				{
					int tempYearBaseTen;
					if (yearBaseTen == 0)
					{
						tempYearBaseTen = 10;
					}
					else
					{
						tempYearBaseTen = yearBaseTen;
					}

					YearQuarter23 = Convert.ToString(tempYearBaseTen - 1) + Convert.ToString((yearBaseTen));
				}

				return YearQuarter1 + YearQuarter23 + quarter;

			}

			if (badlyFormedYear == true)
			{
				ViewBag.ErrorMsg = ViewBag.ErrorMsg + "<li>Badly formed year, please enter a new year in the URL in the format 'Quarter2011'</li>";
			}

			if (badlyFormedQuarter == true)
			{
				ViewBag.ErrorMsg = ViewBag.ErrorMsg + "<li>Badly formed quarter, please enter a new Quarter in the URL in the format 'Fall20XX'</li>";
			}

			return "Z999";



		}

		/// <summary>
		/// Converts a <see cref="YearQuarter"/> into a friendly YRQ (Fall2011)
		/// </summary>
		private String getFriendlyDateFromYRQ(YearQuarter YRQ)
		{
			//Example: Winter 2008 = A783

			//summer: xxx1
			//fall:   xxx2
			//winter: xxx3
			//spring:	xxx4

			//Academic year 2006-2007: x67x
			//Academic year 2011-2012: x12x

			string stringYRQ = YRQ.ID.ToString();

			string year1 = stringYRQ.Substring(stringYRQ.Length - 3, 1); //x6xx = 2006
			string year2 = stringYRQ.Substring(stringYRQ.Length - 2, 1); //xx7x = 2007
			string quarter = stringYRQ.Substring(stringYRQ.Length - 1, 1); //Spring
			string decade = stringYRQ.Substring(stringYRQ.Length - 4, 1); //Axxx = 2000's
			string strQuarter = "";
			bool isLastTwoQuarters = false;
			bool badlyFormedQuarter = false;
			bool badlyFormedYear = false;

			string year = "";

			//is the year an overlapping year? e.g. spring 2000 = 9904 but summer 2000 = A011
			if (year2 == "0" && (quarter == "3" || quarter == "4"))
			{
				isLastTwoQuarters = true;
			}

			//determine the quarter in string form "1", "2", etc... essentially the xxx2 character
			switch (quarter)
			{
				case "1":
					strQuarter = "Summer";
					year = getYearHelper(quarter, year1, year2, decade, isLastTwoQuarters);
					break;
				case "2":
					strQuarter = "Fall";
					year = getYearHelper(quarter, year1, year2, decade, isLastTwoQuarters);
					break;
				case "3":
					strQuarter = "Winter";
					year = getYearHelper(quarter, year1, year2, decade, isLastTwoQuarters);
					break;
				case "4":
					strQuarter = "Spring";
					year = getYearHelper(quarter, year1, year2, decade, isLastTwoQuarters);
					break;
				default:
					badlyFormedQuarter = true;
					break;

			}
			if (IsInteger(year))
			{
				if (Convert.ToInt16(year) < 1975 || Convert.ToInt16(year) > 2030)
				{
					badlyFormedYear = true;
				}
			}

			string returnFriendly = "You have entered a badly formed quarter/year.";

			if (!badlyFormedQuarter || !badlyFormedYear)
			{
				returnFriendly = strQuarter + " " + year;
			}

			return returnFriendly;




		}

		/// <summary>
		/// Gets the current year given some input params. Helper method for getFriendlyDateFromYRQ
		/// </summary>
		public string getYearHelper(string quarter, string year1, string year2, string decade, bool isLastTwoQuarters)
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
		private string getDecadeIntegerFromString(string decade)
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
		private string getNextDecade(string decade)
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
		/// Returns true/false if the value passed is an integer
		/// </summary>
		public static bool IsInteger(string value)
		{
			try
			{
				Convert.ToInt32(value);
				return true;
			}
			catch
			{
				return false;
			}

		}

		/// <summary>
		/// Returns a friendly time in sentance form given a datetime. This value
		/// is create by subtracting the input datetime from the current datetime.
		/// example: 6/8/2011 07:23:123 -> about 4 hours ago
		/// </summary>
		public string getFriendlyTime(DateTime theDate)
		{

			if (theDate == null)
			{
				return "last updated time unavailable";
			}
			else
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

		}

		/// <summary>
		/// returns an IList<ISectionFacet> that contains all of the facet information
		/// passed into the app by the user clicking on the faceted search left pane
		/// facets accepted: flex, time, days, availability
		/// </summary>
		private IList<ISectionFacet> addFacets(string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string f_telecourse, string avail)
		{
			IList<ISectionFacet> facets = new List<ISectionFacet>();

			//add the class format facet options (online, hybrid, telecourse, on campus)
			if (!string.IsNullOrWhiteSpace(f_online))
			{
				facets.Add(new ModalityFacet(ModalityFacet.Options.Online));
			}
			if (!string.IsNullOrWhiteSpace(f_hybrid))
			{
				facets.Add(new ModalityFacet(ModalityFacet.Options.Hybrid));
			}
			if (!string.IsNullOrWhiteSpace(f_telecourse))
			{
				facets.Add(new ModalityFacet(ModalityFacet.Options.Telecourse));
			}
			if (!string.IsNullOrWhiteSpace(f_oncampus))
			{
				facets.Add(new ModalityFacet(ModalityFacet.Options.OnCampus));
			}



			//if only the starting time has a value, add that facet as the min and 23:59 as the max
			if (!string.IsNullOrWhiteSpace(timestart) && string.IsNullOrWhiteSpace(timeend))
			{
				facets.Add(new TimeFacet(new TimeSpan(0, 0, 0), new TimeSpan(23, 59, 0)));
			}
			//if only the ending time has a value, add that facet as the max and 00:00 as the min
			else if (string.IsNullOrWhiteSpace(timestart) && !string.IsNullOrWhiteSpace(timeend))
			{
				facets.Add(new TimeFacet(new TimeSpan(0, 0, 0), new TimeSpan(11, 59, 0)));
			}
			//if both times have a value, add those times as a facet's min and max
			else if (!string.IsNullOrWhiteSpace(timestart) && !string.IsNullOrWhiteSpace(timeend))
			{
				facets.Add(new TimeFacet(new TimeSpan(0, 0, 0), new TimeSpan(11, 59, 0)));
			}


			//day of the week facets
			if (!string.IsNullOrWhiteSpace(day_su))
			{
				facets.Add(new DaysFacet(DaysFacet.Options.Sunday));
			}
			if (!string.IsNullOrWhiteSpace(day_m))
			{
				facets.Add(new DaysFacet(DaysFacet.Options.Monday));
			}
			if (!string.IsNullOrWhiteSpace(day_t))
			{
				facets.Add(new DaysFacet(DaysFacet.Options.Tuesday));
			}
			if (!string.IsNullOrWhiteSpace(day_w))
			{
				facets.Add(new DaysFacet(DaysFacet.Options.Wednesday));
			}
			if (!string.IsNullOrWhiteSpace(day_th))
			{
				facets.Add(new DaysFacet(DaysFacet.Options.Thursday));
			}
			if (!string.IsNullOrWhiteSpace(day_f))
			{
				facets.Add(new DaysFacet(DaysFacet.Options.Friday));
			}
			if (!string.IsNullOrWhiteSpace(day_s))
			{
				facets.Add(new DaysFacet(DaysFacet.Options.Saturday));
			}


			if (!string.IsNullOrWhiteSpace(avail))
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

		private string getProgramUrl(string Subject)
		{
			string ProgramURL = "";
			var specificProgramInfo = from s in _programdb.ProgramInformation
																where s.Abbreviation == Subject
																select s;

			foreach (ProgramInformation program in specificProgramInfo)
			{
				ProgramURL = program.Url;
			}


			//if the url is a fully qualified url (e.g. http://continuinged.bellevuecollege.edu/about)
			//just return it, otherwise prepend iwth the current school url.
			if (ProgramURL.Contains("http://"))
			{
				return ProgramURL;
			}
			else
			{
				ProgramURL = ConfigurationManager.AppSettings["currentSchoolUrl"] + ConfigurationManager.AppSettings["currentAppSubdirectory"] + ProgramURL;

			}

			return ProgramURL;
		}



		#endregion
	}
}
