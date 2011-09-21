using System;
using System.Collections;
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
			ViewBag.WhichClasses = (string.IsNullOrWhiteSpace(letter) ? " (All)" : " (" + letter.ToUpper() + ")");
			ViewBag.AlphabetArray = new bool[26];
			ViewBag.AlphabetCharacter = 0;
			ViewBag.Title = "All classes";

			ViewBag.LinkParams = getLinkParams();

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
					return View(courses);
				}
			}
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

			IList<KeyValuePair<string,KeyValuePair<string,bool>>> modality = new List<KeyValuePair<string, KeyValuePair<string,bool>>>(4);
			modality.Add(Helpers.GetModalityInfo("f_oncampus", "On Campus", f_oncampus) );
			modality.Add(Helpers.GetModalityInfo("f_online", "Online", f_online));
			modality.Add(Helpers.GetModalityInfo("f_hybrid", "Hybrid", f_hybrid));
			modality.Add(Helpers.GetModalityInfo("f_telecourse", "Telecourse", f_telecourse));
			ViewBag.Modality = modality;

			ViewBag.avail = avail;

			ViewBag.LinkParams = getLinkParams();


			ViewBag.Title = "All " +  @Subject + " classes";
			IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s, f_oncampus, f_online, f_hybrid, f_telecourse, avail);

			ViewBag.ProgramUrl = getProgramUrl(Subject);

			using (OdsRepository respository = new OdsRepository())
			{
				getCurrentFutureYRQs(respository);

				// TODO: GetCourses() can take a Subject parameter - returning a smaller dataset which doesn't need to be further filtered.
				IList<Course> courses = respository.GetCourses(facets);

				if (Subject != null)
				{
					IEnumerable<Course> coursesEnum;
					coursesEnum = (from c in courses
												where c.Subject == Subject.ToUpper()
												select c).OrderBy(c => c.Subject).ThenBy(c => c.Number).Distinct();
					ViewBag.ItemCount = coursesEnum.Count();

					return View(coursesEnum);
				}
				else
				{
					return View();
				}
			}
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

			IList<KeyValuePair<string,KeyValuePair<string,bool>>> modality = new List<KeyValuePair<string, KeyValuePair<string,bool>>>(4);
			modality.Add(Helpers.GetModalityInfo("f_oncampus", "On Campus", f_oncampus) );
			modality.Add(Helpers.GetModalityInfo("f_online", "Online", f_online));
			modality.Add(Helpers.GetModalityInfo("f_hybrid", "Hybrid", f_hybrid));
			modality.Add(Helpers.GetModalityInfo("f_telecourse", "Telecourse", f_telecourse));
			ViewBag.Modality = modality;

			ViewBag.avail = avail;


			ViewBag.LinkParams = getLinkParams();




			YearQuarter yrq = Ctc.Ods.Types.YearQuarter.FromFriendlyName(YearQuarter);

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

			ViewBag.avail = avail;

			ViewBag.LinkParams = getLinkParams();


			ViewBag.displayedCourseNum = 0;
			ViewBag.seatAvailbilityDisplayed = false;
			ViewBag.Subject = Subject;

			ViewBag.ProgramUrl = getProgramUrl(Subject);

			IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s, f_oncampus, f_online, f_hybrid, f_telecourse, avail);

			using (OdsRepository respository = new OdsRepository())
			{
				getCurrentFutureYRQs(respository);
				YearQuarter YRQ = Ctc.Ods.Types.YearQuarter.FromFriendlyName(YearQuarter);

				var seatsAvailableLocal = (from s in _scheduledb.vw_SeatAvailability
																	 select s);

				IList<Section> sections = respository.GetSections(Subject, YRQ, facets);

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

				ViewBag.Modality = Helpers.ConstructModalityList(sectionsEnum, f_oncampus, f_online, f_hybrid, f_telecourse);

				ViewBag.ItemCount = sectionsEnum.Count();
				return View(sectionsEnum);
			}
		}

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

			ViewBag.LinkParams = getLinkParams();

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
														orderby c.Yrq.ID descending
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
		/// Gets the current http get/post params and assigns them to an IDictionary<string, object>
		/// This is mainly used to set a Viewbag variable so these can be passed into action links in the views.
		/// </summary>
		private IDictionary<string, object> getLinkParams()
		{
			IDictionary<string, object> linkParams = new Dictionary<string, object>(Request.QueryString.Count);
			foreach (string key in Request.QueryString.AllKeys)
			{
				if (key != "X-Requested-With")
				{
					linkParams.Add(key, Request.QueryString[key]);
				}
			}
			return linkParams;
		}

		/// <summary>
		/// Gets the current <see cref="YearQuarter"/> and assigns ViewBag variables
		/// for the current, +1, +2 quarters. This drives the dynamic YRQ navigation bar
		/// </summary>
		private void getCurrentFutureYRQs(OdsRepository respository)
		{
			IList<YearQuarter> currentFutureQuarters;
			currentFutureQuarters = respository.GetRegistrationQuarters(4);
			ViewBag.QuarterOne = currentFutureQuarters[0];
			ViewBag.QuarterTwo = currentFutureQuarters[1];
			ViewBag.QuarterThree = currentFutureQuarters[2];
			ViewBag.QuarterFour = currentFutureQuarters[3];

			ViewBag.QuarterOneFriendly = currentFutureQuarters[0].FriendlyName;
			ViewBag.QuarterTwoFriendly = currentFutureQuarters[1].FriendlyName;
			ViewBag.QuarterThreeFriendly = currentFutureQuarters[2].FriendlyName;
			ViewBag.QuarterFourFriendly = currentFutureQuarters[3].FriendlyName;

			ViewBag.QuarterOneURL = ViewBag.QuarterOneFriendly.Replace(" ", "");
			ViewBag.QuarterTwoURL = ViewBag.QuarterTwoFriendly.Replace(" ", "");
			ViewBag.QuarterThreeURL = ViewBag.QuarterThreeFriendly.Replace(" ", "");
			ViewBag.QuarterFourURL = ViewBag.QuarterFourFriendly.Replace(" ", "");

		}

		// TODO: Jeremy, make optional AND configurable in web.config
		/// <summary>
		/// Gets the course outcome information by scraping the Bellevue College
		/// course outcomes website
		/// </summary>
		private static dynamic getCourseOutcome(string Subject, string ClassNum)
		{
			const string SETTING_KEY = "IncludeCourseOutcomes";
			bool includeCourseOutcomes = false;

			if (ConfigurationManager.AppSettings.AllKeys.Contains(SETTING_KEY))
			{
				bool.TryParse(ConfigurationManager.AppSettings[SETTING_KEY], out includeCourseOutcomes);
			}

			if (includeCourseOutcomes)
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

			return string.Empty;
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
				ViewBag.YearQuarterHP = Ctc.Ods.Types.YearQuarter.ToYearQuarterID(YearQuarter);

			}

			ViewBag.YearQuarter_a_to_z = "/" + YearQuarter;
			ViewBag.letter = letter;
			ViewBag.YearQuarter = YearQuarter ?? "all";
			ViewBag.avail = avail ?? "all";
			ViewBag.activeClass = " class=active";
			ViewBag.currentUrl = Request.Url.AbsolutePath;
			ViewBag.QuarterURL = YearQuarter;

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
