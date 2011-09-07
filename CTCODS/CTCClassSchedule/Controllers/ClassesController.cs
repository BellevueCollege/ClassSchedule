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
using CTCClassSchedule.Common;
using CTCClassSchedule.Models;
using CTCClassSchedule.Properties;

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

			ViewBag.Title = "All " +  @Subject + " classes";
			IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s, f_oncampus, f_online, f_hybrid, f_telecourse, avail);

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

			YearQuarter yrq = Ctc.Ods.Types.YearQuarter.FromString(getYRQFromFriendlyDate(YearQuarter));

			ViewBag.Subject = "All";
			ViewBag.AlphabetCharacter = 0;
			ViewBag.YRQ = yrq.ToString();
			ViewBag.FriendlyYRQ = yrq.FriendlyName;
			ViewBag.Title = ViewBag.Subject + "Classes for " + @ViewBag.Yearquarter;
			IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s, f_oncampus, f_online, f_hybrid, f_telecourse, avail);

			using (OdsRepository respository = new OdsRepository())
			{
				getCurrentFutureYRQs(respository);

				IList<CoursePrefix> courses = respository.GetCourseSubjects(yrq, facets);

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
					ViewBag.ItemCount = courses.Count();

					return View(courses);
				}
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
			IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s, f_oncampus, f_online, f_hybrid, f_telecourse, avail);

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
													LastUpdated = Helpers.getFriendlyTime(d.LastUpdated.GetValueOrDefault()),
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

			ICourseID courseID = CourseID.FromString(Subject, ClassNum);
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
					// TODO: if we joined the seat availablility lookup w/ the sections, would it return faster?
					var seatsAvailableLocal = (from s in _scheduledb.vw_SeatAvailability
					                           select s);

					// TODO: move this declaration somewhere it can more easily be re-used
					IList<ISectionFacet> facets = new List<ISectionFacet> {new RegistrationQuartersFacet(Settings.Default.QuartersToDisplay)};

					IList<Section> sections = respository.GetSections(courseID, facetOptions: facets);

					IEnumerable<SectionWithSeats> sectionsEnum;
					sectionsEnum = (
								from c in sections
								join d in seatsAvailableLocal on c.ID.ToString() equals d.ClassID
								where c.CourseSubject == Subject.ToUpper()
								select new SectionWithSeats
									{
																				ParentObject = c,
											SeatsAvailable = d.SeatsAvailable,
											LastUpdated = Helpers.getFriendlyTime(d.LastUpdated.GetValueOrDefault()),
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
				friendlyTime = Helpers.getFriendlyTime(seat.LastUpdated.GetValueOrDefault());
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

			ViewBag.QuarterOneFriendly = currentFutureQuarters[0].FriendlyName;
			ViewBag.QuarterTwoFriendly = currentFutureQuarters[1].FriendlyName;
			ViewBag.QuarterThreeFriendly = currentFutureQuarters[2].FriendlyName;

			ViewBag.QuarterOneURL = ViewBag.QuarterOneFriendly.Replace(" ", "");
			ViewBag.QuarterTwoURL = ViewBag.QuarterTwoFriendly.Replace(" ", "");
			ViewBag.QuarterThreeURL = ViewBag.QuarterThreeFriendly.Replace(" ", "");

		}

		// TODO: Jeremy, make optional AND configurable in web.config
		/// <summary>
		/// Gets the course outcome information by scraping the Bellevue College
		/// course outcomes website
		/// </summary>
		private static dynamic getCourseOutcome(string Subject, string ClassNum)
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

			if (Helpers.IsInteger(year))
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

		private string getProgramUrl(string Subject)
		{
			string ProgramURL = "";
			var specificProgramInfo = from s in _programdb.ProgramInformation
																where s.Abbreviation == Subject
																select s;

			// TODO: should this be a loop? Only the last item is being used.
			foreach (ProgramInformation program in specificProgramInfo)
			{
				ProgramURL = program.Url;
			}


			//if the url is a fully qualified url (e.g. http://continuinged.bellevuecollege.edu/about)
			//just return it, otherwise prepend iwth the current school url.
			// TODO: should this be .StartsWith()?
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
