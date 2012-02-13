using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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
using System.Globalization;

namespace CTCClassSchedule.Controllers
{
	public partial class ClassesController : Controller
	{
		#region controller member vars
		readonly private MiniProfiler _profiler = MiniProfiler.Current;
		private readonly ApiSettings _apiSettings = ConfigurationManager.GetSection(ApiSettings.SectionName) as ApiSettings;
		#endregion

		public ClassesController()
		{
			ViewBag.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		}

		#region controller actions

		/// <summary>
		/// GET: /Classes/
		/// </summary>
		///
		[OutputCache(CacheProfile = "IndexCacheTime")] // Caches for 1 day
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
		[OutputCache(CacheProfile = "AllClassesCacheTime")] // Caches for 6 hours
		public ActionResult AllClasses(string letter)
		{
			setViewBagVars("", "", letter);
			ViewBag.WhichClasses = (string.IsNullOrWhiteSpace(letter) ? " (All)" : " (" + letter.ToUpper() + ")");

			ViewBag.LinkParams = Helpers.getLinkParams(Request);

			using (OdsRepository respository = new OdsRepository(HttpContext))
			{
				ViewBag.QuarterNavMenu = Helpers.getYearQuarterListForMenus(respository);
				IList<CoursePrefix> courses;
				IList<vw_ProgramInformation> progInfo;
				using (_profiler.Step("ODSAPI::GetCourseSubjects()"))
				{
					courses = respository.GetCourseSubjects();
				}

				using (ClassScheduleDb db = new ClassScheduleDb())
				{
					progInfo = (from s in db.vw_ProgramInformation
					            select s).ToList();

					IList<ScheduleCoursePrefix> coursesLocalEnum = (from p in progInfo
					                                                where courses.Select(c => c.Subject).Contains(p.Abbreviation.TrimEnd('&'))
					                                                orderby p.Title
					                                                select new ScheduleCoursePrefix
												{
														Subject = p.URL,
														Title = p.Title
												}).Distinct().ToList();

					IList<char> alphabet = coursesLocalEnum.Select(c => c.Title.First()).Distinct().ToList();
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
		}

		/// <summary>
		/// GET: /Classes/All/{Subject}/
		/// </summary>
		///

		[OutputCache(CacheProfile = "SubjectCacheTime")]
		public ActionResult Subject(string Subject, string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string f_telecourse, string avail, string latestart, string numcredits)
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

			IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s, f_oncampus, f_online, f_hybrid, f_telecourse, avail, latestart, numcredits);
			facets.Add(new RegistrationQuartersFacet(Settings.Default.QuartersToDisplay));

			setProgramInfo(Subject);

			using (OdsRepository respository = new OdsRepository(HttpContext))
			{
				ViewBag.QuarterNavMenu = Helpers.getYearQuarterListForMenus(respository);

				IEnumerable<Course> coursesEnum;
				if (Subject != null)
				{
					coursesEnum = respository.GetCourses(getPrefix(Subject), facets).Distinct();
					ViewBag.ItemCount = coursesEnum.Count();

					return View(coursesEnum.OrderBy(c => c.Subject).ThenBy(c => c.Number));
				}
				else
				{
					coursesEnum = respository.GetCourses(facets).Distinct();
					ViewBag.ItemCount = coursesEnum.Count();

					return View(coursesEnum.OrderBy(c => c.Subject).ThenBy(c => c.Number));
				}
			}
		}



		/// <summary>
		/// GET: /Classes/{FriendlyYRQ}/
		/// </summary>

		[OutputCache(CacheProfile = "YearQuarterCacheTime")]
		public ActionResult YearQuarter(String YearQuarter, string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string f_telecourse, string avail, string letter, string latestart, string numcredits)
		{
			ViewBag.WhichClasses = (string.IsNullOrWhiteSpace(letter) ? " (All)" : " (" + letter.ToUpper() + ")");
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
			ViewBag.latestart = latestart;
			ViewBag.numcredits = numcredits;

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

			IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s, f_oncampus, f_online, f_hybrid, f_telecourse, avail, latestart, numcredits);

			using (OdsRepository respository = new OdsRepository(HttpContext))
			{
				ViewBag.QuarterNavMenu = Helpers.getYearQuarterListForMenus(respository);

				IList<CoursePrefix> courses;

				using (_profiler.Step("ODSAPI::GetCourseSubjects()"))
				{
					courses = respository.GetCourseSubjects(yrq, facets);

				}

				using (ClassScheduleDb db = new ClassScheduleDb())
				{
					// gets a distinct list of URL's from the program information table. this couldn't be combined with coursesLocalEnum
					// because the distinct doesn't work due to merged classes having potentially different names


					IEnumerable<string> progInfo = (from s in db.vw_ProgramInformation
																					select s.URL).Distinct();

					var innerQuery = from c in courses
													 select c.Subject;

					//grab the details for the ScheduleCoursePrefix item for each program from the same table, starting with a distinct list of URL's.
					IList<ScheduleCoursePrefix> coursesLocalEnum = (from v in db.vw_ProgramInformation
																													where progInfo.Contains(v.Abbreviation)
														&& innerQuery.Contains(v.Abbreviation)
																													orderby v.Title ascending
					                                                select new ScheduleCoursePrefix
					                                                {
					                                                    Title = v.Title,
																															URL = v.URL,
																															Subject = v.Abbreviation

					                                                }).Distinct().ToList();

					//this code is definitely not how we want to do things. This is because the Accounting people won't
					//rename their ACCT& course prefix...talk to Juan for full details
					AddAccounting(ref coursesLocalEnum);

					IList<char> alphabet = coursesLocalEnum.Select(c => c.Title.First()).Distinct().ToList();
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
		}




		/// <summary>
		/// GET: /Classes/{FriendlyYRQ}/{Subject}/
		/// </summary>

		[OutputCache(CacheProfile = "YearQuarterSubjectCacheTime")] // Caches for 30 minutes
		public ActionResult YearQuarterSubject(String YearQuarter, string Subject, string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string f_telecourse, string avail, string latestart, string numcredits)
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
			ViewBag.numcredits = numcredits;
			ViewBag.latestart = latestart;

			ViewBag.LinkParams = Helpers.getLinkParams(Request);
			ViewBag.Subject = Subject;

			//add the dictionary that converts MWF -> Monday/Wednesday/Friday for section display.
			TempData["DayDictionary"] = Helpers.getDayDictionary();

			IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s, f_oncampus, f_online, f_hybrid, f_telecourse, avail, latestart, numcredits);

			using (OdsRepository respository = new OdsRepository(HttpContext))
			{
				IList<YearQuarter> yrqRange = Helpers.getYearQuarterListForMenus(respository);
				ViewBag.QuarterNavMenu = yrqRange;
				YearQuarter YRQ = Ctc.Ods.Types.YearQuarter.FromFriendlyName(YearQuarter);

				// TODO: Add query string info (e.g. facets) to the routeValues dictionary so we can pass it all as one chunk.
				IDictionary<string, object> routeValues = new Dictionary<string, object>(3);
				routeValues.Add("YearQuarterID", YearQuarter);
				routeValues.Add("YRQ", YRQ);
				ViewBag.RouteValues = routeValues;



				IList<Section> sections;
				using (_profiler.Step("ODSAPI::GetSections()"))
				{
					sections = respository.GetSections(getPrefix(Subject), YRQ, facets);
				}

				using (ClassScheduleDb db = new ClassScheduleDb())
				{
					setProgramInfo(Subject, db);

					IList<SectionWithSeats> sectionsEnum;
					using (_profiler.Step("Getting app-specific Section records from DB"))
					{
						sectionsEnum = Helpers.getSectionsWithSeats(yrqRange[0].ID, sections, db);
					}

					ViewBag.Modality = Helpers.ConstructModalityList(sectionsEnum, f_oncampus, f_online, f_hybrid, f_telecourse);

					return View(sectionsEnum);
				}
			}
		}

		/// <summary>
		/// GET: /Classes/All/{Subject}/{ClassNum}
		/// </summary>
		///
		[OutputCache(CacheProfile = "ClassDetailsCacheTime")]
		public ActionResult ClassDetails(string YearQuarterID, string Subject, string ClassNum)
		{
			ICourseID courseID = CourseID.FromString(Subject, ClassNum);
			ViewBag.Subject = Subject;
			ViewBag.ClassNum = ClassNum;

			//add the dictionary that converts MWF -> Monday/Wednesday/Friday for section display.
			TempData["DayDictionary"] = Helpers.getDayDictionary();

			using (OdsRepository repository = new OdsRepository(HttpContext))
			{
				IList<YearQuarter> yrqRange = Helpers.getYearQuarterListForMenus(repository);
				ViewBag.ActiveQuarter = repository.CurrentYearQuarter.FriendlyName;
				ViewBag.QuarterNavMenu = yrqRange;

				// TODO: move this declaration somewhere it can more easily be re-used
				IList<ISectionFacet> facets = new List<ISectionFacet> {new RegistrationQuartersFacet(Settings.Default.QuartersToDisplay)};

				IList<Section> sections;
				using (_profiler.Step("ODSAPI::GetSections()"))
				{
					sections = repository.GetSections(courseID, facetOptions: facets);
				}

				ICourse courseInfo;
				if (sections != null && sections.Count > 0)
				{
					using (_profiler.Step("ODSAPI::GetCourses() - course information"))
					{
						string cidString = sections.First().CourseID;
						ICourseID cid = CourseID.FromString(cidString);
						IList<Course> foo = repository.GetCourses(cid);
						courseInfo = foo.First();
					}
				}
				else
				{
					courseInfo = null;
				}
				ViewBag.CourseInfo = courseInfo;

				if (courseInfo != null)
				{
					using (_profiler.Step("Retrieving course outcomes"))
					{
						ViewBag.CourseOutcome = getCourseOutcome(courseInfo.IsCommonCourse ? string.Concat(Subject, _apiSettings.RegexPatterns.CommonCourseChar) : Subject, ClassNum);
					}
				}
				else
				{
					ViewBag.CourseOutcome = null;
				}

				IEnumerable<SectionWithSeats> sectionsEnum;
				using (ClassScheduleDb db = new ClassScheduleDb())
				{
					using (_profiler.Step("Getting app-specific Section records from the DB"))
					{
						sectionsEnum = Helpers.getSectionsWithSeats(yrqRange[0].ID, sections, db); //this isn't looking at the ProgramInformation table's URL values when getting classes, resulting in CJ showing up but CJ& not showing up
					}

					if (sectionsEnum != null && sectionsEnum.Count() > 0)
					{
						// Use the real abbreviation as the lookup since we're not longer doing the translation workaround at this level.
						setProgramInfo(sectionsEnum.First().IsCommonCourse ? string.Concat(Subject, _apiSettings.RegexPatterns.CommonCourseChar) : Subject, db, true);
					}
					return View(sectionsEnum);
				}
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

			using (ClassScheduleDb db = new ClassScheduleDb())
			{
				var seatsAvailableLocal =	from s in db.SeatAvailabilities
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

					db.SeatAvailabilities.AddObject(newseat);
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

				db.SaveChanges();

				var seatsAvailable = from s in db.vw_SeatAvailability
				                     where s.ClassID == courseIdPlusYRQ
				                     select s;

				foreach (var seat in seatsAvailable)
				{
					seats = seat.SeatsAvailable;
					friendlyTime = seat.LastUpdated.GetValueOrDefault().ToString("h:mm tt").ToLower();
				}
			}


			var jsonReturnValue = seats.ToString() + "|" + friendlyTime;
			return Json(jsonReturnValue);
		}


		#endregion




		#region helper methods



		public static void AddAccounting(ref IList<ScheduleCoursePrefix> coursesLocalEnum)
		{
			ScheduleCoursePrefix acctp = new ScheduleCoursePrefix
			{
				Title = "Accounting-Transfer",
				URL = "ACCT&",
				Subject = "ACCT&"
			};

			ScheduleCoursePrefix acct = coursesLocalEnum.Single(c => c.URL == "ACCT");

			int pos = coursesLocalEnum.IndexOf(acct);
			if (pos != -1)
			{
				coursesLocalEnum.Add(acctp);
				coursesLocalEnum.Insert(pos + 1, acctp);
			}
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
					returnString = stripTitle(returnString);
				}


				// return course outcome page source
				return returnString;
			}

			return string.Empty;
		}

		private static string stripTitle(string returnString)
		{
			string temp = returnString;
			string search1 = "<title>";
			string search2 = "</title>";
			int location1 = returnString.IndexOf(search1);
			int location2 = returnString.IndexOf(search2);

			if (location1 > 0 && location2 > 0) {
				int range = location2 - location1 + search2.Length;
				temp = temp.Remove(location1, range);
			}


			return temp;
		}

		/// <summary>
		/// Sets all of the common ViewBag variables
		/// </summary>
		private void setViewBagVars(string YearQuarter, string avail, string letter)
		{
			ViewBag.ErrorMsg = "";
			var debug = HttpContext.Request;
			ViewBag.YearQuarter = string.IsNullOrWhiteSpace(YearQuarter) ? null : Ctc.Ods.Types.YearQuarter.FromFriendlyName(YearQuarter);

			ViewBag.letter = letter;
			ViewBag.avail = avail ?? "all";
			ViewBag.activeClass = " class=active";
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="Subject"></param>
		/// <param name="db"></param>
		/// <param name="useRealAbbreviation"></param>
		private void setProgramInfo(string Subject, ClassScheduleDb db = null, bool useRealAbbreviation = false)
		{
			using (_profiler.Step("Retrieving course program information"))
			{
				const string DEFAULT_TITLE = "";
				const string DEFAULT_URL = "";
				const string DEFAULT_INTRO = "";
				bool disposeDb = false;

				if (db == null)
				{
					db = new ClassScheduleDb();
					disposeDb = true;
				}

				try
				{
					IQueryable<vw_ProgramInformation> specificProgramInfo;
					if (useRealAbbreviation)
					{
						specificProgramInfo = from s in db.vw_ProgramInformation
						                      where s.Abbreviation == Subject
						                      select s;
					}
					else
					{
						specificProgramInfo = from s in db.vw_ProgramInformation
						                      where s.URL == Subject
						                      select s;

					}

					if (specificProgramInfo.Count() > 0)
					{
						vw_ProgramInformation program = specificProgramInfo.Take(1).Single();

						ViewBag.ProgramTitle = program.Title ?? DEFAULT_TITLE;
						ViewBag.SubjectIntro = program.Intro ?? DEFAULT_INTRO;

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
				finally
				{
					// clean up the database connection if we had to create one
					if (disposeDb && db != null)
					{
						db.Dispose();
					}
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
			using (ClassScheduleDb db = new ClassScheduleDb())
			{
				List<string> prefixList = (from s in db.vw_ProgramInformation
				                           where s.URL == URLprefix
				                           select s.Abbreviation).ToList();

				return prefixList;
			}
		}
		#endregion
	}
}
