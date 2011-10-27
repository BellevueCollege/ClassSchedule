﻿using System;
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
		private ClassScheduleFootnoteEntities _footnotedb = new ClassScheduleFootnoteEntities();
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

			ViewBag.LinkParams = Helpers.getLinkParams(Request);

			using (OdsRepository respository = new OdsRepository(HttpContext))
			{
				ViewBag.QuarterNavMenu = Helpers.getYearQuarterListForMenus(respository);
				IList<CoursePrefix> courses;
				IList<ProgramInformation> progInfo;
				using (_profiler.Step("ODSAPI::GetCourseSubjects()"))
				{
					courses = respository.GetCourseSubjects();
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
				coursesLocalEnum = coursesLocalEnum.ToList().Distinct().OrderBy(p => p.Title);

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

					return View(coursesEnum);
				}
				else
				{
					return View(coursesLocalEnum);
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

			ViewBag.LinkParams = Helpers.getLinkParams(Request);

			IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s, f_oncampus, f_online, f_hybrid, f_telecourse, avail);
			facets.Add(new RegistrationQuartersFacet(Settings.Default.QuartersToDisplay));

			setProgramInfo(Subject);

			using (OdsRepository respository = new OdsRepository(HttpContext))
			{
				ViewBag.QuarterNavMenu = Helpers.getYearQuarterListForMenus(respository);

				if (Subject != null)
				{
					IEnumerable<Course> coursesEnum = respository.GetCourses(getPrefix(Subject), facets).Distinct();
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

			ViewBag.LinkParams = Helpers.getLinkParams(Request);

			YearQuarter yrq = Ctc.Ods.Types.YearQuarter.FromFriendlyName(YearQuarter);
			ViewBag.YearQuarter = yrq;

			ViewBag.Subject = "All";
			ViewBag.AlphabetCharacter = 0;

			IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s, f_oncampus, f_online, f_hybrid, f_telecourse, avail);

			using (OdsRepository respository = new OdsRepository(HttpContext))
			{
				ViewBag.QuarterNavMenu = Helpers.getYearQuarterListForMenus(respository);

				IList<CoursePrefix> courses;
				//IEnumerable<CoursePrefix> coursesLocalEnum;
				using (_profiler.Step("ODSAPI::GetCourseSubjects()"))
				{
					courses = respository.GetCourseSubjects(yrq, facets);
				}

				var progInfo = (from s in _programdb.ProgramInformation
												select new {s.Abbreviation, s.URL, s.Title}).ToList();

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

			ViewBag.LinkParams = Helpers.getLinkParams(Request);
			ViewBag.Subject = Subject;

			setProgramInfo(Subject);

			IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s, f_oncampus, f_online, f_hybrid, f_telecourse, avail);

			using (OdsRepository respository = new OdsRepository(HttpContext))
			{
				IList<YearQuarter> yrqRange = Helpers.getYearQuarterListForMenus(respository);
				ViewBag.QuarterNavMenu = yrqRange;
				YearQuarter YRQ = Ctc.Ods.Types.YearQuarter.FromFriendlyName(YearQuarter);

				// TODO: Add query string info (e.g. facets) to the routeValues dictionary so we can pass it all as one chunk.
				IDictionary<string, object> routeValues = new Dictionary<string, object>(3);
				routeValues.Add("YearQuarterID", YearQuarter);
				ViewBag.RouteValues = routeValues;

				IList<Section> sections;
				using (_profiler.Step("ODSAPI::GetSections()"))
				{
					sections = respository.GetSections(getPrefix(Subject), YRQ, facets);
				}

				IEnumerable<SectionWithSeats> sectionsEnum;
				using (_profiler.Step("Getting app-specific Section records from the DB"))
				{
					sectionsEnum = Helpers.getSectionsWithSeats(yrqRange[0].ID, sections);
				}

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
			ViewBag.Subject = Subject;
			ViewBag.ClassNum = ClassNum;

			using (OdsRepository respository = new OdsRepository(HttpContext))
			{
				IList<YearQuarter> yrqRange = Helpers.getYearQuarterListForMenus(respository);
				ViewBag.QuarterNavMenu = yrqRange;

				// TODO: move this declaration somewhere it can more easily be re-used
				IList<ISectionFacet> facets = new List<ISectionFacet> {new RegistrationQuartersFacet(Settings.Default.QuartersToDisplay)};

				IList<Section> sections;
				using (_profiler.Step("ODSAPI::GetSections()"))
				{
					sections = respository.GetSections(courseID, facetOptions: facets);
				}

				Section courseInfo = sections.First();
				using (_profiler.Step("Retrieving course outcomes"))
				{
					ViewBag.CourseOutcome = getCourseOutcome(courseInfo.IsCommonCourse ? string.Concat(Subject, _apiSettings.RegexPatterns.CommonCourseChar) : Subject, ClassNum);
				}
				ViewBag.CourseInfo = courseInfo;

				IEnumerable<SectionWithSeats> sectionsEnum;
				using (_profiler.Step("Getting app-specific Section records from the DB"))
				{
					sectionsEnum = Helpers.getSectionsWithSeats(yrqRange[0].ID, sections);
				}

				// Use the real abbreviation as the lookup since we're not longer doing the translation workaround at this level.
				setProgramInfo(sectionsEnum.First().IsCommonCourse ? string.Concat(Subject, _apiSettings.RegexPatterns.CommonCourseChar) : Subject, true);

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

			ViewBag.YearQuarter = string.IsNullOrWhiteSpace(YearQuarter) ? null : Ctc.Ods.Types.YearQuarter.FromFriendlyName(YearQuarter);

			ViewBag.letter = letter;
			ViewBag.avail = avail ?? "all";
			ViewBag.activeClass = " class=active";
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="Subject"></param>
		/// <param name="useRealAbbreviation"></param>
		private void setProgramInfo(string Subject, bool useRealAbbreviation = false)
		{
			using (_profiler.Step("Retrieving course program information"))
			{
				const string DEFAULT_TITLE = "";
				const string DEFAULT_URL = "";

				IQueryable<ProgramInformation> specificProgramInfo;
				if (useRealAbbreviation)
				{
					specificProgramInfo = from s in _programdb.ProgramInformation
																where s.Abbreviation == Subject
																select s;
				}
				else
				{
					specificProgramInfo = from s in _programdb.ProgramInformation
					                      where s.URL == Subject
					                      select s;

				}

				if (specificProgramInfo.Count() > 0)
				{
					ProgramInformation program = specificProgramInfo.Take(1).Single();

					ViewBag.ProgramTitle = program.Title ?? DEFAULT_TITLE;

					string url = program.ProgramURL ?? DEFAULT_URL;

					//if the url is a fully qualified url (e.g. http://continuinged.bellevuecollege.edu/about)
					//or empty just return it, otherwise prepend iwth the current school url.
					if (!string.IsNullOrWhiteSpace(url) && !Regex.IsMatch(url, @"^https?://"))
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

		/// <summary>
		///
		/// </summary>
		/// <param name="URLprefix"></param>
		/// <returns></returns>
		private List<string> getPrefix(string URLprefix)
		{
			List<string> prefixList = (from s in _programdb.ProgramInformation
																 where s.URL == URLprefix
																 select s.Abbreviation).ToList();

			return prefixList;
		}
		#endregion
	}
}
