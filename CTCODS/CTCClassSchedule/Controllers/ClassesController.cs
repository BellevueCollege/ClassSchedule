using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
using System.Web;
using System.Text;

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
			using (OdsRepository repository = new OdsRepository(HttpContext))
			{
				ViewBag.CurrentYearQuarter = repository.CurrentYearQuarter;
				ViewBag.QuarterNavMenu = Helpers.getYearQuarterListForMenus(repository);
				ViewBag.RegistrationQuarters = Helpers.getFutureQuarters(repository);
			}
			return View();
		}

		/// <summary>
		/// GET: /Classes/Export/{YearQuarterID}
		/// </summary>
		/// <returns>An Adobe InDesign formatted text file with all course data. File is
		/// returned as an HTTP response.</returns>
		public void Export(String YearQuarterID)
		{
			// Configurables
			int subjectSeparatoreLineBreaks = 4;

			// Use this to convert strings to file byte arrays
			System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
			StringBuilder fileText = new StringBuilder();

			// Determine whether to use a specific Year Quarter, or the current Year Quarter
			YearQuarter yrq;
			IList<vw_ProgramInformation> programs = new List<vw_ProgramInformation>();
			using (OdsRepository _db = new OdsRepository())
			{
				if (String.IsNullOrEmpty(YearQuarterID))
				{
					yrq = _db.CurrentYearQuarter;
				}
				else
				{
					yrq = Ctc.Ods.Types.YearQuarter.FromString(YearQuarterID);
				}
			}

			// Get a dictionary of sorted collections of sections, collated by division
			IDictionary<vw_ProgramInformation, List<SectionWithSeats>> divisions = new Dictionary<vw_ProgramInformation, List<SectionWithSeats>>();
			divisions = getSectionsByDivision(yrq);

			// Create all file data
			foreach (vw_ProgramInformation program in divisions.Keys)
			{
				for (int i = 0; i < subjectSeparatoreLineBreaks; i++)
				{
					fileText.AppendLine();
				}

				fileText.AppendLine(String.Concat("<CLS1>", program.Title));
				fileText.AppendLine(String.Concat("<CLS9>", program.Division));
				if (!String.IsNullOrEmpty(program.Intro))
				{
					fileText.AppendLine(String.Concat("<CLSP>", program.Intro));
				}

				string line = string.Empty;
				SectionWithSeats previousSection = new SectionWithSeats();
				foreach (SectionWithSeats section in divisions[program])
				{
					// Build all section information such as title and footnotes
					if (section.CourseNumber != previousSection.CourseNumber ||
						  section.Credits != previousSection.Credits ||
							section.CourseTitle != previousSection.CourseTitle)
					{
						buildSectionExportText(fileText, section);
					}
					previousSection = section;

					// Compile list of all offered instances of the section
					foreach (OfferedItem item in section.Offered.OrderBy(o => o.SequenceOrder))
					{
						buildOfferedItemsExportText(fileText, section, item);
					}

					// Section and course footnotes
					line = section.SectionFootnotes;
					if (!String.IsNullOrWhiteSpace(line))
					{
						fileText.AppendLine(String.Concat("<CLSX>", line.Trim()));
					}
					line = AutomatedFootnotesConfig.getAutomatedFootnotesText(section);
					if (!String.IsNullOrWhiteSpace(line))
					{
						fileText.AppendLine(String.Concat("<CLSY>", line.Trim()));
					}
				}
			}

			// Write the file data as an HTTP response
			string fileName = String.Concat("CourseData-", yrq.ID, "-", DateTime.Now.ToShortDateString(), ".txt");
			fileText.Remove(0, subjectSeparatoreLineBreaks * 2); // Remove the first set of line breaks
			byte[] fileData = encoding.GetBytes(fileText.ToString());
			HttpResponseBase response = ControllerContext.HttpContext.Response;
			string contentDisposition = String.Concat("attachment; filename=", fileName);
			response.AddHeader("Content-Disposition", contentDisposition);
			response.ContentType = "application/force-download";
			response.BinaryWrite(fileData);
		}

		/// <summary>
		/// GET: /Classes/All
		/// </summary>
		///
		[OutputCache(CacheProfile = "AllClassesCacheTime")] // Caches for 6 hours
		public ActionResult AllClasses(string letter)
		{
			ViewBag.WhichClasses = (string.IsNullOrWhiteSpace(letter) ? "All" : letter.ToUpper());

			ViewBag.LinkParams = Helpers.getLinkParams(Request);

			using (OdsRepository repository = new OdsRepository(HttpContext))
			{
				SetCommonViewBagVars("", "", letter, repository);
				ViewBag.QuarterNavMenu = Helpers.getYearQuarterListForMenus(repository);
				IList<CoursePrefix> courses;
				using (_profiler.Step("ODSAPI::GetCourseSubjects()"))
				{
					courses = repository.GetCourseSubjects();
				}

				using (ClassScheduleDb db = new ClassScheduleDb())
				{
					IList<vw_ProgramInformation> progInfo = (from s in db.vw_ProgramInformation
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
		///
		/// </summary>
		/// <param name="Subject"></param>
		/// <param name="timestart"></param>
		/// <param name="timeend"></param>
		/// <param name="day_su"></param>
		/// <param name="day_m"></param>
		/// <param name="day_t"></param>
		/// <param name="day_w"></param>
		/// <param name="day_th"></param>
		/// <param name="day_f"></param>
		/// <param name="day_s"></param>
		/// <param name="f_oncampus"></param>
		/// <param name="f_online"></param>
		/// <param name="f_hybrid"></param>
		/// <param name="f_telecourse"></param>
		/// <param name="avail"></param>
		/// <param name="latestart"></param>
		/// <param name="numcredits"></param>
		/// <param name="format"></param>
		/// <returns></returns>
		[HttpGet]
		[OutputCache(CacheProfile = "SubjectCacheTime")]
		public ActionResult Subject(string Subject, string timestart, string timeend,
																string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s,
																string f_oncampus, string f_online, string f_hybrid, string f_telecourse,
																string avail, string latestart, string numcredits, string format)
		{
			using (OdsRepository repository = new OdsRepository(HttpContext))
			{
				IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s, f_oncampus, f_online, f_hybrid, f_telecourse, avail, latestart, numcredits);
				facets.Add(new RegistrationQuartersFacet(Settings.Default.QuartersToDisplay));

				IEnumerable<Course> coursesEnum;
				if (Subject != null)
				{
					coursesEnum = repository.GetCourses(RealSubjectPrefixes(Subject), facets).Distinct();
				}
				else
				{
					coursesEnum = repository.GetCourses(facets).Distinct();
				}

				if (format == "json")
				{
					// NOTE: AllowGet exposes the potential for JSON Hijacking (see http://haacked.com/archive/2009/06/25/json-hijacking.aspx)
					// but is not an issue here because we are receiving and returning public (e.g. non-sensitive) data
					return Json(coursesEnum.OrderBy(c => c.Subject).ThenBy(c => c.Number), JsonRequestBehavior.AllowGet);
				}

				// Display the web page
				ViewBag.Subject = Subject;
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

				ViewBag.ItemCount = coursesEnum.Count();
				ViewBag.LinkParams = Helpers.getLinkParams(Request);

				SetProgramInfoVars(Subject);

				IList<ModalityFacetInfo> modality = new List<ModalityFacetInfo>(4);
				modality.Add(Helpers.getModalityInfo("f_oncampus", "On Campus", f_oncampus) );
				modality.Add(Helpers.getModalityInfo("f_online", "Online", f_online));
				modality.Add(Helpers.getModalityInfo("f_hybrid", "Hybrid", f_hybrid));
				modality.Add(Helpers.getModalityInfo("f_telecourse", "Telecourse", f_telecourse));
				ViewBag.Modality = modality;

				SetCommonViewBagVars("", avail, "", repository);
				ViewBag.QuarterNavMenu = Helpers.getYearQuarterListForMenus(repository);

				return View(coursesEnum.OrderBy(c => c.Subject).ThenBy(c => c.Number));
			}
		}



		/// <summary>
		/// GET: /Classes/{FriendlyYRQ}/
		/// </summary>
		[OutputCache(CacheProfile = "YearQuarterCacheTime")]
		public ActionResult YearQuarter(String YearQuarter, string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string f_telecourse, string avail, string letter, string latestart, string numcredits)
		{
			ViewBag.WhichClasses = (string.IsNullOrWhiteSpace(letter) ? "All" : letter.ToUpper());

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

			//YearQuarter yrq = Ctc.Ods.Types.YearQuarter.FromFriendlyName(YearQuarter);
			//ViewBag.YearQuarter = yrq;

			ViewBag.Subject = "All";

			IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s, f_oncampus, f_online, f_hybrid, f_telecourse, avail, latestart, numcredits);

			using (OdsRepository repository = new OdsRepository(HttpContext))
			{
				SetCommonViewBagVars(YearQuarter, avail, letter, repository);
				ViewBag.QuarterNavMenu = Helpers.getYearQuarterListForMenus(repository);

				IList<CoursePrefix> courses;

				using (_profiler.Step("ODSAPI::GetCourseSubjects()"))
				{
					courses = repository.GetCourseSubjects(ViewBag.YearQuarter as YearQuarter, facets);

				}

				using (ClassScheduleDb db = new ClassScheduleDb())
				{
					IList<vw_ProgramInformation> progInfo = (from s in db.vw_ProgramInformation
																									 where s.Abbreviation == s.URL
					                                         select s).ToList();

					IList<ScheduleCoursePrefix> coursesLocalEnum = (from p in progInfo
					                                                where courses.Select(c => c.Subject).Contains(p.AbbreviationTrimmed)
					                                                orderby p.Title
					                                                select new ScheduleCoursePrefix
												{
														Subject = p.URL,
														Title = p.Title,
												}).Distinct().ToList();

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

			using (OdsRepository repository = new OdsRepository(HttpContext))
			{
				SetCommonViewBagVars(YearQuarter, avail, "", repository);
				IList<YearQuarter> yrqRange = Helpers.getYearQuarterListForMenus(repository);
				ViewBag.QuarterNavMenu = yrqRange;
				ViewBag.CurrentRegistrationQuarter = yrqRange[0];
				YearQuarter YRQ = Ctc.Ods.Types.YearQuarter.FromFriendlyName(YearQuarter);

				// TODO: Add query string info (e.g. facets) to the routeValues dictionary so we can pass it all as one chunk.
				IDictionary<string, object> routeValues = new Dictionary<string, object>(3);
				routeValues.Add("YearQuarterID", YearQuarter);
				ViewBag.RouteValues = routeValues;



				IList<Section> sections;
				using (_profiler.Step("ODSAPI::GetSections()"))
				{
					sections = repository.GetSections(RealSubjectPrefixes(Subject), YRQ, facets);
				}

				using (ClassScheduleDb db = new ClassScheduleDb())
				{
					SetProgramInfoVars(Subject, db);

					IList<SectionWithSeats> sectionsEnum;
					using (_profiler.Step("Getting app-specific Section records from DB"))
					{
						sectionsEnum = Helpers.getSectionsWithSeats(YRQ.ID, sections, db);
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
				ViewBag.CurrentYearQuarter = repository.CurrentYearQuarter;
				IList<YearQuarter> yrqRange = Helpers.getYearQuarterListForMenus(repository);
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
						IList<Course> coursesTemp = repository.GetCourses(cid);

						// NOTE: The view handles situations where foo is null or empty
						courseInfo = coursesTemp != null && coursesTemp.Count > 0 ? coursesTemp.First() : null;
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
						SetProgramInfoVars(sectionsEnum.First().IsCommonCourse ? string.Concat(Subject, _apiSettings.RegexPatterns.CommonCourseChar) : Subject, db, true);
					}
					return View(sectionsEnum);
				}
			}
		}

		/// <summary>
		/// Retrieves and updates Seats Available data for the specified <see cref="Section"/>
		/// </summary>
		/// <param name="classID"></param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult getSeats(string classID)
		{
			int? seats = null;
			string friendlyTime = "";

			string itemNumber = classID.Substring(0, 4);
			string yrq = classID.Substring(4, 4);

			CourseHPQuery query = new CourseHPQuery();
			int hpSeats = query.FindOpenSeats(itemNumber, yrq);

			using (ClassScheduleDb db = new ClassScheduleDb())
			{
				//if the HP query didn't fail, save the changes. Otherwise, leave the SeatAvailability table alone.
				//This way, the correct number of seats can be pulled by the app and displayed instead of updating the
				//table with a 0 for seats available.
				if (hpSeats >= 0)
				{
					IQueryable<SeatAvailability> seatsAvailableLocal = from s in db.SeatAvailabilities
					                                                   where s.ClassID == classID
					                                                   select s;
					int rows = seatsAvailableLocal.Count();

					if (rows > 0)
					{
						// TODO: Should only be updating one record
						//update the value
						foreach (SeatAvailability seat in seatsAvailableLocal)
						{
							seat.SeatsAvailable = hpSeats;
							seat.LastUpdated = DateTime.Now;
						}
					}
					else
					{
						//insert the value
						SeatAvailability newseat = new SeatAvailability();
						newseat.ClassID = classID;
						newseat.SeatsAvailable = hpSeats;
						newseat.LastUpdated = DateTime.Now;

						db.SeatAvailabilities.AddObject(newseat);
					}

					db.SaveChanges();
				}

				// retrieve updated seats data
				IQueryable<vw_SeatAvailability> seatsAvailable = from s in db.vw_SeatAvailability
																												 where s.ClassID == classID
																												 select s;

				vw_SeatAvailability newSeat = seatsAvailable.First();

				seats = newSeat.SeatsAvailable;
				friendlyTime = newSeat.LastUpdated.GetValueOrDefault().ToString("h:mm tt").ToLower();
			}

			string jsonReturnValue =  string.Format("{0}|{1}", seats, friendlyTime);
			return Json(jsonReturnValue);
		}


		#endregion


		#region helper methods
		/// <summary>
		/// Takes a section and StringBuilder and appends the section title, and HP footnotes
		/// in an Adobe InDesign format so that the data can be added to a file. The file is useful
		/// when printing the paper version of the class schedule.
		/// </summary>
		/// <param name="text">The StringBuilder that the data should be added to.</param>
		/// <param name="section">The SectionWithSeats object whose data should be recorded.</param>
		private static void buildSectionExportText(StringBuilder text, SectionWithSeats section)
		{
			string creditsText;
			string line;

			creditsText = section.Credits.ToString();
			if (section.Credits == Math.Floor(section.Credits)) { creditsText = creditsText.Remove(creditsText.IndexOf('.')); }
			creditsText = String.Concat(section.IsVariableCredits ? "V 1-" : string.Empty, creditsText, " CR");

			// Section title
			text.AppendLine();
			line = String.Concat("<CLS2>", section.CourseSubject, section.IsCommonCourse ? "&" : string.Empty, " ", section.CourseNumber, "\t", section.CourseTitle, " [-] ", creditsText);
			text.AppendLine(line);


			// Add Course and HP footnotes
			if (section.Footnotes.Count() > 0)
			{
				text.AppendLine(String.Concat("<CLS3>", section.CourseFootnotes, String.IsNullOrEmpty(section.CourseFootnotes) ? string.Empty : " ", string.Join(" ", section.Footnotes)));
			}

		}

		/// <summary>
		/// Takes section, offered item, and StringBuilder and appends data related to the offered item
		/// in an Adobe InDesign format so that the data can be added to a file. The file is useful
		/// when printing the paper version of the class schedule.
		/// </summary>
		/// <param name="text">The StringBuilder that the data should be added to.</param>
		/// <param name="section">The SectionWithSeats object that the OfferedItem belongs to.</param>
		/// <param name="item">The OfferedItem object whose data should be recorded.</param>
		private static void buildOfferedItemsExportText(StringBuilder text, SectionWithSeats section, OfferedItem item)
		{
			// Configurables
			string onlineDaysStr = "[online]";
			string onlineRoomStr = "D110";
			string distanceEdMinStr = "7000";
			string distanceEdMaxStr = "ZZZZ";
			string arrangedStr = "Arranged";
			string hybridCodeStr = "[h]";
			string instructorDefaultNameStr = "staff";
			TimeSpan eveningCourse = new TimeSpan(17, 30, 0);

			string daysStr;
			string roomStr;
			string line = string.Empty;
			string tagStr = string.Empty;
			string startTimeStr = string.Empty;
			string endTimeStr = string.Empty;
			string hybridStr = string.Empty;
			string instructorName = string.Empty;
			DateTime startTime;

			// Build the line that describes the offered instance of the course
			tagStr = "<CLS5>";
			hybridStr = section.IsHybrid ? String.Concat(hybridCodeStr, " ") : string.Empty;
			if (item.IsPrimary || !item.IsPrimary && !String.IsNullOrEmpty(item.InstructorName))
			{
				instructorName = getNameShortFormat(item.InstructorName) ?? instructorDefaultNameStr;
			}
			if (item.StartTime != null && item.EndTime != null) // Handle normal daily and evening courses
			{
				startTimeStr = item.StartTime.Value.ToString("h:mmt").ToLower();
				endTimeStr = item.EndTime.Value.ToString("h:mmt").ToLower();
				startTime = item.StartTime.Value;
				if (new TimeSpan(startTime.Hour, startTime.Minute, startTime.Second) >= eveningCourse) { tagStr = "<CLS6>"; }

				if (item.IsPrimary)
				{
					line = String.Concat(tagStr, section.ID.ItemNumber, "\t", hybridStr, section.SectionCode, "\t", instructorName, "\t", item.Days, "\t", startTimeStr, "-", endTimeStr, "\t", item.Room);
				}
				else
				{
					line = String.Concat(tagStr, "\t\t\t", instructorName, "\t", item.Days, "\t", startTimeStr, "-", endTimeStr, "\t", item.Room);
				}
			}
			else if (section.IsOnline) // Handle online courses
			{
				if (item.IsPrimary)
				{
					line = String.Concat(tagStr, section.ID.ItemNumber, "\t", hybridStr, section.SectionCode, "\t", instructorName, "\t", onlineDaysStr, "\t", item.Room ?? onlineRoomStr);
				}
				else
				{
					line = String.Concat(tagStr, "\t\t\t", instructorName, "\t", onlineDaysStr, "\t", item.Room ?? onlineRoomStr);
				}
			}
			else // Handle Distance Ed or arranged courses
			{
				daysStr = item.Days;
				roomStr = String.Concat("\t", item.Room);
				if (section.ID.ItemNumber.CompareTo(distanceEdMinStr) >= 0 && section.ID.ItemNumber.CompareTo(distanceEdMaxStr) <= 0)
				{
					tagStr = "<CLSD>";
				}
				else if (item.Days == arrangedStr)
				{
					tagStr = "<CLSA>";
					daysStr = String.Concat("\t", arrangedStr.ToLower());
					if (String.IsNullOrEmpty(item.Room))
					{
						roomStr = string.Empty;
					}
				}

				if (item.IsPrimary)
				{
					line = String.Concat(tagStr, section.ID.ItemNumber, "\t", hybridStr, section.SectionCode, "\t", instructorName, "\t", daysStr, roomStr);
				}
				else
				{
					line = String.Concat(tagStr, "\t\t\t", instructorName, "\t", daysStr, roomStr);
				}
			}

			// Append the line to the file
			text.AppendLine(line);
		}

		/// <summary>
		/// Take a full name and converts it to a short format, with the last name and first initial of
		/// the first name (e.g. ANDREW CRASWELL -> CRASWELL A).
		/// </summary>
		/// <param name="fullName">The full name to be converted.</param>
		/// <returns>A string with the short name version of the full name. If fullName was blank or empty, a null is returned.</returns>
		private static string getNameShortFormat(string fullName)
		{
			string shortName = null;
			if (!String.IsNullOrEmpty(fullName))
			{
				int nameStartIndex = fullName.IndexOf(" ") + 1;
				int nameEndIndex = fullName.Length - nameStartIndex;
				shortName = String.Concat(fullName.Substring(nameStartIndex, nameEndIndex), " ", fullName.Substring(0, 1));
			}

			return shortName;
		}

		/// <summary>
		/// Gets all the sections in a given quarter and sorts them into a dictionary by division.
		/// </summary>
		/// <param name="yrq">The quarter to fetch sections for.</param>
		/// <returns>A dictionary where they key is the division, and the values are the corresponding lists of Sections.</returns>
		private IDictionary<vw_ProgramInformation, List<SectionWithSeats>> getSectionsByDivision(YearQuarter yrq)
		{
			IList<Section> allSections;
			IList<CoursePrefix> subjects = new List<CoursePrefix>();
			IDictionary<vw_ProgramInformation, List<SectionWithSeats>> results = new Dictionary<vw_ProgramInformation, List<SectionWithSeats>>();
			using (OdsRepository _db = new OdsRepository())
			{
				// Get subject and program information
				subjects = _db.GetCourseSubjects(yrq).Distinct().ToList();

				// Get all sections for the given quarter
				allSections = _db.GetSections(yrq);
			}

			IList<vw_ProgramInformation> programs = new List<vw_ProgramInformation>();
			IList<SectionWithSeats> allSectionsWithSeats;
			using (ClassScheduleDb _csDb = new ClassScheduleDb())
			{
				// Get a sorted list of all programs
				programs = _csDb.vw_ProgramInformation.OrderBy(p => p.URL).ToList();

				// Convert all Sections to SectionsWithSeats so we get footnote data
				allSectionsWithSeats = Helpers.getSectionsWithSeats(yrq.ID, allSections, _csDb);
			}


			// Get and sort all sections
			IList<SectionWithSeats> tempSections;
			vw_ProgramInformation currentProgram = new vw_ProgramInformation();
			vw_ProgramInformation lastProgram = new vw_ProgramInformation();
			string commonCourseChar = _apiSettings.RegexPatterns.CommonCourseChar;
			foreach (vw_ProgramInformation program in programs)
			{
				if (lastProgram.URL != program.URL) // New division
				{
					// Sort the sections in the last division
					if (results.ContainsKey(currentProgram))
					{
						if (results[currentProgram].Count > 0)
						{
							results[currentProgram] = results[currentProgram].OrderBy(s => s.CourseNumber)
																															 .ThenByDescending(s => s.IsOnCampus)
																															 .ThenByDescending(s => s.IsHybrid)
																															 .ThenByDescending(s => s.IsOnline)
																															 .ThenByDescending(s => s.IsTelecourse)
																															 .ThenBy(s => s.SectionCode).ToList();
						}
						else
						{
							results.Remove(lastProgram);
						}
					}
					lastProgram = program;

					// Create a new division
					if (subjects.Where(s => s.Subject == program.AbbreviationTrimmed).Count() > 0)
					{
						results.Add(program, new List<SectionWithSeats>());
						currentProgram = program;
					}
					else // If the program has no sections this quarter
					{
						continue;
					}
				}

				// Collate sections into the current division
				bool isCommonCourse = program.Abbreviation.Contains(commonCourseChar);
				tempSections = allSectionsWithSeats.Where(s => s.CourseSubject == program.AbbreviationTrimmed && s.IsCommonCourse == isCommonCourse).ToList();
				results[currentProgram].AddRange(tempSections);
			}

			return results.OrderBy(p => p.Key.Division).ToDictionary(p => p.Key, p => p.Value);
		}

		/// <summary>
		/// Gets the course outcome information by scraping the Bellevue College
		/// course outcomes website
		/// </summary>
		private static dynamic getCourseOutcome(string Subject, string ClassNum)
		{
			string CourseOutcome = "";
			try
			{
				Service1Client client = new Service1Client();

				ICourseID courseID = CourseID.FromString(Subject, ClassNum);

				CourseOutcome = client.GetCourseOutcome(courseID.ToString());
			}
			catch
			{
				CourseOutcome = "Cannot find course outcome for this course or cannot connect to the course outcomes webservice.";
			}
			return CourseOutcome;
		}

		/// <summary>
		/// Sets all of the common ViewBag variables
		/// </summary>
		private void SetCommonViewBagVars(string YearQuarter, string avail, string letter, OdsRepository repository)
		{
			ViewBag.ErrorMsg = "";
			ViewBag.YearQuarter = string.IsNullOrWhiteSpace(YearQuarter) ? null : Ctc.Ods.Types.YearQuarter.FromFriendlyName(YearQuarter);
			ViewBag.CurrentYearQuarter = repository.CurrentYearQuarter;

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
		private void SetProgramInfoVars(string Subject, ClassScheduleDb db = null, bool useRealAbbreviation = false)
		{
			using (_profiler.Step("Retrieving course program information"))
			{
				const string DEFAULT_TITLE = "";
				const string DEFAULT_URL = "";
				const string DEFAULT_INTRO = "";
				const string DEFAULT_ACADEMICPROGRAM = "";
				const string DEFAULT_DIVISIONURL = "";
				const string DEFAULT_DIVISIONTITLE = "";

				/*
				 *
				 *
				 * 				using (ClassScheduleDb db = new ClassScheduleDb()){
					var DepartmentInfo = (from d in db.vw_ProgramInformation
																where d.Abbreviation == Subject
																select d).Take(1).ToList();


					foreach (vw_ProgramInformation temp in DepartmentInfo)
					{
						string DivisionTitle
							string DivisionUrl

					}
				}
				 *
				 *
				 */
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
						                      where s.Abbreviation == Subject
						                      select s;

					}

					if (specificProgramInfo.Count() > 0)
					{
						vw_ProgramInformation program = specificProgramInfo.Take(1).Single();

						ViewBag.ProgramTitle = program.Title ?? DEFAULT_TITLE;
						ViewBag.SubjectIntro = program.Intro ?? DEFAULT_INTRO;
						ViewBag.AcademicProgram = program.AcademicProgram ?? DEFAULT_ACADEMICPROGRAM;
						ViewBag.DivisionURL = program.DivisionURL ?? DEFAULT_DIVISIONURL;
						ViewBag.DivisionTitle = program.Division ?? DEFAULT_DIVISIONTITLE;



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
		/// Gets a list of course prefixes for the specified URL subject
		/// </summary>
		/// <param name="URLprefix"></param>
		/// <returns></returns>
		/// <remarks>
		/// The <i>Subject</i> portion of a route URL can reference more than one,
		/// or a completely different, <see cref="CoursePrefix"/>. This method looks
		/// up these mappings in the application database for the appropriate
		/// list of actual prefix abbreviations.
		/// </remarks>
		private List<string> RealSubjectPrefixes(string URLprefix)
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
