using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Ctc.Ods;
using Ctc.Ods.Config;
using Ctc.Ods.Data;
using Ctc.Ods.Types;
using CTCClassSchedule.Common;
using CTCClassSchedule.Models;
using CTCClassSchedule.Properties;
using MvcMiniProfiler;

namespace CTCClassSchedule.Controllers
{
	public class ClassesController : Controller
	{
		#region controller member vars
		readonly private MiniProfiler _profiler = MiniProfiler.Current;
		private readonly ApiSettings _apiSettings = ConfigurationManager.GetSection(ApiSettings.SectionName) as ApiSettings;

		private ClassScheduleDevEntities _scheduledb = new ClassScheduleDevEntities();
		private ClassScheduleDevProgramEntities _programdb = new ClassScheduleDevProgramEntities();
		#endregion

		public ClassesController()
		{
			Debug.Print("==> instatiating ClassesController()...");
		}

		#region controller actions

		/// <summary>
		/// GET: /Classes/
		/// </summary>
		///
		[ActionOutputCache("IndexCacheTime")] // Caches for 1 day
		public ActionResult Index()
		{
			using (OdsRepository respository = new OdsRepository(HttpContext))
			{
				ViewBag.QuarterNavMenu = Helpers.getYearQuarterListForMenus(respository);
			}
			return View();
		}


		/// <summary>
		/// GET: /Classes/All
		/// </summary>
		///
		[ActionOutputCache("AllClassesCacheTime")] // Caches for 6 hours
		public ActionResult AllClasses(string letter)
		{

			setViewBagVars("", "", letter);
			ViewBag.WhichClasses = (string.IsNullOrWhiteSpace(letter) ? " (All)" : " (" + letter.ToUpper() + ")");
			ViewBag.AlphabetArray = new bool[26];
			ViewBag.AlphabetCharacter = 0;

			ViewBag.LinkParams = getLinkParams();

			using (OdsRepository respository = new OdsRepository(HttpContext))
			{
				ViewBag.QuarterNavMenu = Helpers.getYearQuarterListForMenus(respository);
				IList<CoursePrefix> courses;
				using (_profiler.Step("ODSAPI::GetCourseSubjects()"))
				{
					courses = respository.GetCourseSubjects();
				}
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
		///

		[ActionOutputCache("SubjectCacheTime")]
		public ActionResult Subject(string Subject, string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string f_telecourse, string avail)
		{
			ViewBag.Subject = Subject;
			setViewBagVars("", avail, "");
			ViewBag.timestart = timestart;
			ViewBag.timeend = timeend;
			ViewBag.day_su = day_su;
			ViewBag.day_m = day_m;
			ViewBag.day_t = day_t;
			ViewBag.day_w = day_w;
			ViewBag.day_th = day_th;
			ViewBag.day_f = day_f;
			ViewBag.day_s = day_s;

			IList<ModalityFacetInfo> modality = new List<ModalityFacetInfo>(4);
			modality.Add(Helpers.getModalityInfo("f_oncampus", "On Campus", f_oncampus) );
			modality.Add(Helpers.getModalityInfo("f_online", "Online", f_online));
			modality.Add(Helpers.getModalityInfo("f_hybrid", "Hybrid", f_hybrid));
			modality.Add(Helpers.getModalityInfo("f_telecourse", "Telecourse", f_telecourse));
			ViewBag.Modality = modality;

			ViewBag.avail = avail;

			ViewBag.LinkParams = getLinkParams();

			IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s, f_oncampus, f_online, f_hybrid, f_telecourse, avail);
			facets.Add(new RegistrationQuartersFacet(Settings.Default.QuartersToDisplay));

			setProgramInfo(Subject);

			using (OdsRepository respository = new OdsRepository(HttpContext))
			{
				ViewBag.QuarterNavMenu = Helpers.getYearQuarterListForMenus(respository);

				if (Subject != null)
				{
					IEnumerable<Course> coursesEnum = respository.GetCourses(Subject, facets).Distinct();
					ViewBag.ItemCount = coursesEnum.Count();

					return View(coursesEnum.OrderBy(c => c.Subject).ThenBy(c => c.Number));
				}
				else
				{
					IEnumerable<Course> coursesEnum = respository.GetCourses(facets).Distinct();
					ViewBag.ItemCount = coursesEnum.Count();

					return View(coursesEnum);
				}
			}
		}



		/// <summary>
		/// GET: /Classes/{FriendlyYRQ}/
		/// </summary>

		[ActionOutputCache("YearQuarterCacheTime")]
		public ActionResult YearQuarter(String YearQuarter, string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string f_telecourse, string avail, string letter)
		{
			ViewBag.WhichClasses = (letter == null || letter == "" ? " (All)" : " (" + letter.ToUpper() + ")");
			setViewBagVars(YearQuarter, avail, letter);

			ViewBag.timestart = timestart;
			ViewBag.timeend = timeend;
			ViewBag.day_su = day_su;
			ViewBag.day_m = day_m;
			ViewBag.day_t = day_t;
			ViewBag.day_w = day_w;
			ViewBag.day_th = day_th;
			ViewBag.day_f = day_f;
			ViewBag.day_s = day_s;

			IList<ModalityFacetInfo> modality = new List<ModalityFacetInfo>(4);
			modality.Add(Helpers.getModalityInfo("f_oncampus", "On Campus", f_oncampus) );
			modality.Add(Helpers.getModalityInfo("f_online", "Online", f_online));
			modality.Add(Helpers.getModalityInfo("f_hybrid", "Hybrid", f_hybrid));
			modality.Add(Helpers.getModalityInfo("f_telecourse", "Telecourse", f_telecourse));
			ViewBag.Modality = modality;

			ViewBag.avail = avail;

			ViewBag.LinkParams = getLinkParams();

			YearQuarter yrq = Ctc.Ods.Types.YearQuarter.FromFriendlyName(YearQuarter);

			ViewBag.Subject = "All";
			ViewBag.AlphabetCharacter = 0;
			ViewBag.YRQ = yrq.ToString();
			ViewBag.FriendlyYRQ = yrq.FriendlyName;

			IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s, f_oncampus, f_online, f_hybrid, f_telecourse, avail);

			using (OdsRepository respository = new OdsRepository(HttpContext))
			{
				ViewBag.QuarterNavMenu = Helpers.getYearQuarterListForMenus(respository);

				IList<CoursePrefix> courses;
				IList<ProgramInformation> progInfo;
				//IEnumerable<CoursePrefix> coursesLocalEnum;
				using (_profiler.Step("ODSAPI::GetCourseSubjects()"))
				{
					courses = respository.GetCourseSubjects(yrq, facets);
				}



				progInfo = (from s in _programdb.ProgramInformation
											select s).ToList();

				IEnumerable<ScheduleCoursePrefix> coursesLocalEnum = (from p in progInfo
														where courses.Select(c => c.Subject).Contains(p.Abbreviation.TrimEnd('&'))
														select new ScheduleCoursePrefix
													{
														Subject = p.URL,
														Title = p.Title
													});
				coursesLocalEnum = coursesLocalEnum.ToList().Distinct();



				IEnumerable<String> alphabet;
				ViewBag.AlphabetArray = new bool[26];

				alphabet = from c in courses
										orderby c.Title
										select c.Title.Substring(0, 1);

				alphabet = alphabet.Distinct();
				ViewBag.Alphabet = alphabet;

				if (letter != null)
				{
					IEnumerable<ScheduleCoursePrefix> coursesEnum;
					coursesEnum = from c in coursesLocalEnum
												where c.Title.StartsWith(letter, StringComparison.OrdinalIgnoreCase)
												select c;

					coursesEnum = coursesEnum.Distinct();

					ViewBag.ItemCount = coursesEnum.Count();

					return View(coursesEnum);
				}
				else
				{
					ViewBag.ItemCount = coursesLocalEnum.Count();

					return View(coursesLocalEnum);
				}
			}
		}



		/// <summary>
		/// GET: /Classes/{FriendlyYRQ}/{Subject}/
		/// </summary>

		[ActionOutputCache("YearQuarterSubjectCacheTime")] // Caches for 30 minutes
		public ActionResult YearQuarterSubject(String YearQuarter, string Subject, string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string f_telecourse, string avail)
		{
			setViewBagVars(YearQuarter, avail, "");

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

			ViewBag.Subject = Subject;

			setProgramInfo(Subject);

			IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s, f_oncampus, f_online, f_hybrid, f_telecourse, avail);

			using (OdsRepository respository = new OdsRepository(HttpContext))
			{
				ViewBag.QuarterNavMenu = Helpers.getYearQuarterListForMenus(respository);
				YearQuarter YRQ = Ctc.Ods.Types.YearQuarter.FromFriendlyName(YearQuarter);

				// TODO: Add query string info (e.g. facets) to the routeValues dictionary so we can pass it all as one chunk.
				IDictionary<string, object> routeValues = new Dictionary<string, object>(3);
				routeValues.Add("YearQuarterID", YearQuarter);
				ViewBag.RouteValues = routeValues;

				var seatsAvailableLocal = (from s in _scheduledb.vw_SeatAvailability
																	 where s.ClassID.Substring(4) == YRQ.ID
																	 select s);

				IList<Section> sections;
				using (_profiler.Step("ODSAPI::GetSections()"))
				{
					sections = respository.GetSections(getPrefix(Subject), YRQ, facets);
				}
				IEnumerable<SectionWithSeats> sectionsEnum;
				sectionsEnum = (
											from c in sections
											join d in seatsAvailableLocal on c.ID.ToString() equals d.ClassID

											select new SectionWithSeats
											{
													ParentObject = c,
													SeatsAvailable = d.SeatsAvailable,
													LastUpdated = Helpers.getFriendlyTime(d.LastUpdated.GetValueOrDefault()),
											}
				               );

				ViewBag.Modality = Helpers.ConstructModalityList(sectionsEnum, f_oncampus, f_online, f_hybrid, f_telecourse);

				return View(sectionsEnum);
			}
		}

		/// <summary>
		/// GET: /Classes/All/{Subject}/{ClassNum}
		/// </summary>
		///
		[ActionOutputCache("ClassDetailsCacheTime")]
		public ActionResult ClassDetails(string YearQuarterID, string Subject, string ClassNum)
		{
			ICourseID courseID = CourseID.FromString(Subject, ClassNum);

			setProgramInfo(Subject);

			using (OdsRepository respository = new OdsRepository(HttpContext))
			{
				IList<YearQuarter> yrqRange = Helpers.getYearQuarterListForMenus(respository);
				ViewBag.QuarterNavMenu = yrqRange;

				string currentYrq = yrqRange[0].ID;
				var seatsAvailableLocal = from s in _scheduledb.vw_SeatAvailability
																	where s.ClassID.Substring(4) == currentYrq
					                        select s;

				// TODO: move this declaration somewhere it can more easily be re-used
				IList<ISectionFacet> facets = new List<ISectionFacet> {new RegistrationQuartersFacet(Settings.Default.QuartersToDisplay)};

				IList<Section> sections;
				using (_profiler.Step("ODSAPI::GetSections()"))
				{
					sections = respository.GetSections(courseID, facetOptions: facets);
				}

				Section courseInfo = sections.First();
				ViewBag.CourseInfo = courseInfo;
				using (_profiler.Step("Retrieving course outcomes"))
				{
					ViewBag.CourseOutcome = getCourseOutcome(courseInfo.IsCommonCourse ? string.Concat(Subject, _apiSettings.RegexPatterns.CommonCourseChar) : Subject, ClassNum);
				}
				IEnumerable<SectionWithSeats> sectionsEnum = (
									from c in sections
									join d in seatsAvailableLocal on c.ID.ToString() equals d.ClassID into cd
									from d in cd.DefaultIfEmpty()	// include all sections, even if don't have an associated seatsAvailable
									orderby c.Yrq.ID descending
									select new SectionWithSeats
										{
												ParentObject = c,
												SeatsAvailable = d == null ? int.MinValue : d.SeatsAvailable,	// allows us to identify past quarters (with no availability info)
												LastUpdated = d == null ? string.Empty : Helpers.getFriendlyTime(d.LastUpdated.GetValueOrDefault()),
										}
				                           );

				return View(sectionsEnum);
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="courseIdPlusYRQ"></param>
		/// <returns></returns>
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
		/// Gets the course outcome information by scraping the Bellevue College
		/// course outcomes website
		/// </summary>
		private static dynamic getCourseOutcome(string Subject, string ClassNum)
		{
			const string SETTING_KEY = "IncludeCourseOutcomes";
			bool includeCourseOutcomes = false;
			string returnString = "";
			string url = "";

			if (ConfigurationManager.AppSettings.AllKeys.Contains(SETTING_KEY))
			{
				bool.TryParse(ConfigurationManager.AppSettings[SETTING_KEY], out includeCourseOutcomes);
			}

			if (includeCourseOutcomes)
			{
				if (Subject.Contains("&"))
				{
					url = "http://bellevuecollege.edu/courseoutcomes/?CourseID=" + Subject.Replace("&", "") + "^" + ClassNum;
				}
				else
				{
					url = "http://bellevuecollege.edu/courseoutcomes/?CourseID=" + Subject + "%20" + ClassNum;
				}

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


				//do some error checking. If there is no outcome found, the following String will be returned that we have to clean up:
				// Error: We didn't find any outcomes for this course.
				if (sb.ToString().Contains("Error: We"))
				{
					returnString = "";
				}
				else
				{
					returnString = sb.ToString();
				}

				// return course outcome page source
				return returnString;
			}

			return string.Empty;
		}

		/// <summary>
		/// Sets all of the common ViewBag variables
		/// </summary>
		private void setViewBagVars(string YearQuarter, string avail, string letter)
		{
			ViewBag.ErrorMsg = "";

			if (YearQuarter != "")
			{
				ViewBag.QuarterViewing = Ctc.Ods.Types.YearQuarter.ToYearQuarterID(YearQuarter);
			}
			ViewBag.YearQuarter = YearQuarter ?? "all";

			ViewBag.YearQuarter_a_to_z = "/" + YearQuarter;
			ViewBag.letter = letter;
			ViewBag.avail = avail ?? "all";
			ViewBag.activeClass = " class=active";
			ViewBag.currentUrl = Request.Url.AbsolutePath;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="Subject"></param>
		private void setProgramInfo(string Subject)
		{
			using (_profiler.Step("Retrieving course program information"))
			{
				const string DEFAULT_TITLE = "";
				const string DEFAULT_URL = "";

				var specificProgramInfo = from s in _programdb.ProgramInformation
				                          where s.Abbreviation == Subject
				                          select s;

				if (specificProgramInfo.Count() > 0)
				{
					var program = specificProgramInfo.Take(1).Single();

					ViewBag.ProgramTitle = program.Title ?? DEFAULT_TITLE;

					string url = program.ProgramURL ?? DEFAULT_URL;

					//if the url is a fully qualified url (e.g. http://continuinged.bellevuecollege.edu/about)
					//just return it, otherwise prepend iwth the current school url.
					if (!Regex.IsMatch(url, @"^https?://"))
					{
						url =  ConfigurationManager.AppSettings["currentSchoolUrl"].UriCombine(url);
					}
					ViewBag.ProgramUrl = url;
				}
				else
				{
					ViewBag.ProgramTitle = DEFAULT_TITLE;
					ViewBag.ProgramUrl = DEFAULT_URL;
				}
			}
		}

		private List<string> getPrefix(string URLprefix)
		{
			List<string> prefixList = new List<string>();

			prefixList = (	from s in _programdb.ProgramInformation
											where s.URL == URLprefix
											select s.Abbreviation).ToList();


			return prefixList;
		}

		#endregion
	}
}
