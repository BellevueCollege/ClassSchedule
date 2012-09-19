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
using Ctc.Web.Security;

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
			if (HttpContext.User.Identity.IsAuthenticated == true)
			{

				// Configurables
				int subjectSeparatedLineBreaks = 4;

				// Use this to convert strings to file byte arrays
				ASCIIEncoding encoding = new ASCIIEncoding();
				StringBuilder fileText = new StringBuilder();

				// Determine whether to use a specific Year Quarter, or the current Year Quarter
				YearQuarter yrq;
				IList<vw_ProgramInformation> programs = new List<vw_ProgramInformation>();
				if (String.IsNullOrEmpty(YearQuarterID))
				{
					using (OdsRepository _db = new OdsRepository())
					{
							yrq = _db.CurrentYearQuarter;
					}
				}
				else
				{
					yrq = Ctc.Ods.Types.YearQuarter.FromString(YearQuarterID);
				}


				// Get a dictionary of sorted collections of sections, collated by division
				IDictionary<vw_ProgramInformation, List<SectionWithSeats>> divisions = new Dictionary<vw_ProgramInformation, List<SectionWithSeats>>();
				divisions = getSectionsByDivision(yrq);

				// Create all file data
				string hyrbidFootnote = AutomatedFootnotesConfig.Footnotes("hybrid").Text;
				foreach (vw_ProgramInformation program in divisions.Keys)
				{
					for (int i = 0; i < subjectSeparatedLineBreaks; i++)
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
							fileText.AppendLine(String.Concat("<CLSN>", line.Trim()));
						}
						line = AutomatedFootnotesConfig.getAutomatedFootnotesText(section);

						// Only display the last hyrbid footnote in a grouping of hybrid courses
						if (line.Contains(hyrbidFootnote) && section != divisions[program].Where(s => s.IsHybrid && section.CourseNumber == s.CourseNumber && section.Credits == s.Credits && section.CourseTitle == s.CourseTitle).LastOrDefault())
						{
							line = line.Replace(hyrbidFootnote, string.Empty);
						}

						if (!String.IsNullOrWhiteSpace(line))
						{
							fileText.AppendLine(String.Concat("<CLSY>", line.Trim()));
						}
					}
				}

				// Write the file data as an HTTP response
				string fileName = String.Concat("CourseData-", yrq.ID, "-", DateTime.Now.ToShortDateString(), ".rtf");
				fileText.Remove(0, subjectSeparatedLineBreaks * 2); // Remove the first set of line breaks
				byte[] fileData = encoding.GetBytes(fileText.ToString());
				HttpResponseBase response = ControllerContext.HttpContext.Response;
				string contentDisposition = String.Concat("attachment; filename=", fileName);
				response.AddHeader("Content-Disposition", contentDisposition);
				response.ContentType = "application/force-download";
				response.BinaryWrite(fileData);
			}
		}

		/// <summary>
		/// GET: /Classes/All
		/// </summary>
		///
		[HttpGet]
		[OutputCache(CacheProfile = "AllClassesCacheTime")] // Caches for 6 hours
		public ActionResult AllClasses(string letter, string format)
		{
			using (OdsRepository repository = new OdsRepository(HttpContext))
			{
				IList<CoursePrefix> courses;
				using (_profiler.Step("ODSAPI::GetCourseSubjects()"))
				{
					courses = repository.GetCourseSubjects();
				}

				// TODO: Refactor the following code into its own method
				// after reconciling the noted differences between AllClasses() and YearQuarter() - 4/27/2012, shawn.south@bellevuecollege.edu
				using (ClassScheduleDb db = new ClassScheduleDb())
				{
					IList<vw_ProgramInformation> progInfo = (from s in db.vw_ProgramInformation
					                                         select s).ToList();

					IList<ScheduleCoursePrefix> coursesJoin = (from p in progInfo
					                                           // TODO: Can we use the AbbreviationTrimmed field here instead of calling TrimEnd()?
					                                           // see YearQuarter() - 4/27/2012, shawn.south@bellevuecollege.edu
					                                           where courses.Select(c => c.Subject).Contains(p.Abbreviation.TrimEnd('&'))
					                                           orderby p.Title
					                                           select new ScheduleCoursePrefix
												{
														Subject = p.URL,
														Title = p.Title
												}).Distinct().ToList();

					IList<char> alphabet = coursesJoin.Select(c => c.Title.First()).Distinct().ToList();
					ViewBag.Alphabet = alphabet;

					IEnumerable<ScheduleCoursePrefix> coursesEnum;
					if (letter != null)
					{
						coursesEnum = from c in coursesJoin
						              where c.Title.StartsWith(letter, StringComparison.OrdinalIgnoreCase)
						              select c;
					}
					else
					{
						coursesEnum = coursesJoin;
					}

					if (format == "json")
					{
						// NOTE: AllowGet exposes the potential for JSON Hijacking (see http://haacked.com/archive/2009/06/25/json-hijacking.aspx)
						// but is not an issue here because we are receiving and returning public (e.g. non-sensitive) data
						return Json(coursesEnum, JsonRequestBehavior.AllowGet);
					}

					// set up all the ancillary data we'll need to display the View
					SetCommonViewBagVars(repository, "", letter);
					ViewBag.QuarterNavMenu = Helpers.getYearQuarterListForMenus(repository);
					ViewBag.WhichClasses = (string.IsNullOrWhiteSpace(letter) ? "All" : letter.ToUpper());
					ViewBag.LinkParams = Helpers.getLinkParams(Request);

					return View(coursesEnum);
				}
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="Subject"></param>
		/// <param name="format"></param>
		/// <returns></returns>
		[HttpGet]
		[OutputCache(CacheProfile = "SubjectCacheTime")]
		public ActionResult Subject(string Subject, string format)
		{
			using (OdsRepository repository = new OdsRepository(HttpContext))
			{
				IEnumerable<Course> coursesEnum = (Subject != null ? repository.GetCourses(RealSubjectPrefixes(Subject)) : repository.GetCourses()).Distinct();

				if (format == "json")
				{
					// NOTE: AllowGet exposes the potential for JSON Hijacking (see http://haacked.com/archive/2009/06/25/json-hijacking.aspx)
					// but is not an issue here because we are receiving and returning public (e.g. non-sensitive) data
					return Json(coursesEnum.OrderBy(c => c.Subject).ThenBy(c => c.Number), JsonRequestBehavior.AllowGet);
				}

				// Display the web page
				ViewBag.Subject = Subject;

				ViewBag.ItemCount = coursesEnum.Count();
				ViewBag.LinkParams = Helpers.getLinkParams(Request);



				IList<YearQuarter> currentQuarter = repository.GetRegistrationQuarters(1);

				ViewBag.CurrentQuarter = currentQuarter[0];

				SetProgramInfoVars(Subject);

				SetCommonViewBagVars(repository, "", "");
				ViewBag.QuarterNavMenu = Helpers.getYearQuarterListForMenus(repository);

				return View(coursesEnum.OrderBy(c => c.Subject).ThenBy(c => c.Number));
			}
		}



		/// <summary>
		/// GET: /Classes/{FriendlyYRQ}/
		/// </summary>
		[OutputCache(CacheProfile = "YearQuarterCacheTime")]
		public ActionResult YearQuarter(String YearQuarter, string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string f_telecourse, string avail, string letter, string latestart, string numcredits, string format)
		{
			YearQuarter yrq	= string.IsNullOrWhiteSpace(YearQuarter) ? null : Ctc.Ods.Types.YearQuarter.FromFriendlyName(YearQuarter);
			IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s, f_oncampus, f_online, f_hybrid, f_telecourse, avail, latestart, numcredits);

			using (OdsRepository repository = new OdsRepository(HttpContext))
			{
				IList<CoursePrefix> courses;

				using (_profiler.Step("ODSAPI::GetCourseSubjects()"))
				{
					courses = repository.GetCourseSubjects(yrq, facets);
				}

				// TODO: Refactor the following code into its own method
				// after reconciling the noted differences between AllClasses() and YearQuarter() - 4/27/2012, shawn.south@bellevuecollege.edu
				using (ClassScheduleDb db = new ClassScheduleDb())
				{
					IList<vw_ProgramInformation> progInfo = (from s in db.vw_ProgramInformation
// The following line was causing problems at Penninsula College. Since I already felt this
// line was suspect, I've gone ahead and commented it out. (Note that we're not doing this
// filter in AllClasses(). - 8/29/2012, shawn.south@bellevuecollege.edu
//					                                         where s.Abbreviation == s.URL
					                                         select s).ToList();

					IList<ScheduleCoursePrefix> coursesJoin = (from p in progInfo
					                                           where courses.Select(c => c.Subject).Contains(p.AbbreviationTrimmed)
					                                           orderby p.Title
					                                           select new ScheduleCoursePrefix
												{
														Subject = p.URL,
														Title = p.Title,
												}).Distinct().ToList();

					IList<char> alphabet = coursesJoin.Select(c => c.Title.First()).Distinct().ToList();
					ViewBag.Alphabet = alphabet;

					IEnumerable<ScheduleCoursePrefix> coursesEnum;
					if (letter != null)
					{
						coursesEnum = (from c in coursesJoin
						               where c.Title.StartsWith(letter, StringComparison.OrdinalIgnoreCase)
						               select c).Distinct();
					}
					else
					{
						coursesEnum = coursesJoin;
					}

					if (format == "json")
					{
						// NOTE: AllowGet exposes the potential for JSON Hijacking (see http://haacked.com/archive/2009/06/25/json-hijacking.aspx)
						// but is not an issue here because we are receiving and returning public (e.g. non-sensitive) data
						return Json(coursesEnum, JsonRequestBehavior.AllowGet);
					}

					// set up all the ancillary data we'll need to display the View
					SetCommonViewBagVars(repository, avail, letter);
					ViewBag.QuarterNavMenu = Helpers.getYearQuarterListForMenus(repository);

					ViewBag.WhichClasses = (string.IsNullOrWhiteSpace(letter) ? "All" : letter.ToUpper());

					ViewBag.timestart = timestart;
					ViewBag.timeend = timeend;
					ViewBag.latestart = latestart;
					ViewBag.avail = avail;
					ViewBag.Subject = "All";
					ViewBag.YearQuarter = yrq;

					ViewBag.Modality = Helpers.ConstructModalityList(f_oncampus, f_online, f_hybrid, f_telecourse);
					ViewBag.Days = Helpers.ConstructDaysList(day_su, day_m, day_t, day_w, day_th, day_f, day_s);

					ViewBag.LinkParams = Helpers.getLinkParams(Request);

					ViewBag.ItemCount = coursesEnum.Count();
					return View(coursesEnum);
				}
			}
		}




		/// <summary>
		/// GET: /Classes/{FriendlyYRQ}/{Subject}/
		/// </summary>
		[OutputCache(CacheProfile = "YearQuarterSubjectCacheTime")] // Caches for 30 minutes
		public ActionResult YearQuarterSubject(String YearQuarter, string Subject, string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string f_telecourse, string avail, string latestart, string numcredits, string format)
		{
			YearQuarter yrq = Ctc.Ods.Types.YearQuarter.FromFriendlyName(YearQuarter);
			IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s, f_oncampus, f_online, f_hybrid, f_telecourse, avail, latestart, numcredits);

			using (OdsRepository repository = new OdsRepository(HttpContext))
			{
				IList<Section> sections;
				using (_profiler.Step("ODSAPI::GetSections()"))
				{
					sections = repository.GetSections(RealSubjectPrefixes(Subject), yrq, facets);
				}

				using (ClassScheduleDb db = new ClassScheduleDb())
				{
					IList<SectionWithSeats> sectionsEnum;
					using (_profiler.Step("Getting app-specific Section records from DB"))
					{
						sectionsEnum = Helpers.GetSectionsWithSeats(yrq.ID, sections, db);
					}

					if (format == "json")
					{
						// NOTE: AllowGet exposes the potential for JSON Hijacking (see http://haacked.com/archive/2009/06/25/json-hijacking.aspx)
						// but is not an issue here because we are receiving and returning public (e.g. non-sensitive) data
						return Json(sectionsEnum, JsonRequestBehavior.AllowGet);
					}

					// set up all the ancillary data we'll need to display the View
					ViewBag.timestart = timestart;
					ViewBag.timeend = timeend;
					ViewBag.avail = avail;
					ViewBag.latestart = latestart;

					ViewBag.LinkParams = Helpers.getLinkParams(Request);
					ViewBag.Subject = Subject;

					//add the dictionary that converts MWF -> Monday/Wednesday/Friday for section display.
					TempData["DayDictionary"] = Helpers.getDayDictionary();

					SetCommonViewBagVars(repository, avail, "");
					SetProgramInfoVars(Subject, db);

					ViewBag.YearQuarter = yrq;
					IList<YearQuarter> yrqRange = Helpers.getYearQuarterListForMenus(repository);
					ViewBag.QuarterNavMenu = yrqRange;
					ViewBag.CurrentRegistrationQuarter = yrqRange[0];

					ViewBag.Modality = Helpers.ConstructModalityList(f_oncampus, f_online, f_hybrid, f_telecourse);
					ViewBag.Days = Helpers.ConstructDaysList(day_su, day_m, day_t, day_w, day_th, day_f, day_s);

					// TODO: Add query string info (e.g. facets) to the routeValues dictionary so we can pass it all as one chunk.
					IDictionary<string, object> routeValues = new Dictionary<string, object>(3);
					routeValues.Add("YearQuarterID", YearQuarter);
					ViewBag.RouteValues = routeValues;

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
			// YearQuarter.FromString(YearQuarterID);


			using (OdsRepository repository = new OdsRepository(HttpContext))
			{
				ViewBag.CurrentYearQuarter = repository.CurrentYearQuarter;
				IList<YearQuarter> yrqRange = Helpers.getYearQuarterListForMenus(repository);
				ViewBag.QuarterNavMenu = yrqRange;

				IList<Course> courses;
				using (_profiler.Step("ODSAPI::GetSections()"))
				{
					courses = repository.GetCourses(courseID);
				}

				if (courses.Count > 0)
				{
					ICourseID realCourseID = CourseID.FromString(courses.First().CourseID);
					realCourseID.IsCommonCourse = courses.First().IsCommonCourse;

					using (_profiler.Step("Getting Section counts (per YRQ)"))
					{
						// Identify which, if any, of the current range of quarters has Sections for this Course
						IList<YearQuarter> quartersOffered = new List<YearQuarter>(4);
						foreach (YearQuarter quarter in yrqRange)
						{
							// TODO: for better performance, overload method to accept more than one YRQ
							int sectionCount = repository.SectionCountForCourse(realCourseID, quarter);
							if (sectionCount > 0)
							{
								quartersOffered.Add(quarter);
							}
						}
						ViewBag.QuartersOffered = quartersOffered;
					}

					string realSubject = realCourseID.IsCommonCourse ? string.Concat(realCourseID.Subject, _apiSettings.RegexPatterns.CommonCourseChar) : realCourseID.Subject;

					using (_profiler.Step("Retrieving course outcomes"))
					{
						ViewBag.CourseOutcome = getCourseOutcome(realSubject, realCourseID.Number);
					}

					SetProgramInfoVars(realSubject, null, true);

					ViewBag.CMSFootnote = getCMSFootnote(Subject, ClassNum, courseID);

				}
				return View(courses);
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
		/// Takes a Subject, ClassNum and CourseID and finds the CMS Footnote for the course.
		/// </summary>
		/// <param name="Subject">The course Subject</param>
		/// <param name="ClassNum">The CourseNumber</param>
		/// /// <param name="courseID">The courseID for the course.</param>
		private string getCMSFootnote(string Subject, string CourseNum, ICourseID courseID)
		{
			using (ClassScheduleDb db = new ClassScheduleDb())
			{
				using (_profiler.Step("Getting app-specific Section records from DB"))
				{
					CourseFootnote item = null;
					char[] trimChar = { '&' };

					string FullCourseID = Helpers.BuildCourseID(CourseNum, Subject.TrimEnd(trimChar), courseID.IsCommonCourse);
					try
					{
						item = db.CourseFootnotes.Single(s => s.CourseID.Trim().ToUpper() == FullCourseID);

						if (item != null)
						{
							return item.Footnote;
						}


					}
					catch (InvalidOperationException e)
					{
						Trace.Write("Course has no CMS footnote: " + e);
					}




				}
			}
			return null;
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
			string arrangedStr = "Arranged";
			string hybridCodeStr = "[h]";
			string instructorDefaultNameStr = "staff";

			// Build a string that represents the DAY and TIME of the course
			string dayTimeStr = String.Concat("\t", arrangedStr.ToLower()); // Default if no date or time is available is "arranged"
			if (item.StartTime != null && item.EndTime != null) // If there is a time available
			{
				string startTimeStr = item.StartTime.Value.ToString("h:mmt").ToLower();
				string endTimeStr = item.EndTime.Value.ToString("h:mmt").ToLower();
				dayTimeStr = String.Concat(item.Days.Equals(arrangedStr) ? arrangedStr.ToLower() : item.Days, "\t", startTimeStr, "-", endTimeStr);
			}
			else if (section.IsOnline) // Online class
			{
				dayTimeStr = String.Concat("\t", onlineDaysStr);
			}

			// Get the tag code that describes the offered item. The tag is determined by the primary offered item
			string tagStr = getSectionExportTag(section, item);


			// Set or override variable values for tags that have special conditions
			string roomStr = String.Concat("\t", item.Room ?? onlineRoomStr);
			switch (tagStr)
			{
				case "<CLSA>":
					roomStr = String.IsNullOrEmpty(item.Room) ? string.Empty : String.Concat("\t", item.Room);
					break;
				case "<CLSD>":
					roomStr = String.Concat("\t", item.Room);
					break;
			}


			// Construct the finalized tag itself, based on whether or not the current item is the primary
			string line;
			if (item.IsPrimary)
			{
				string instructorName = getNameShortFormat(item.InstructorName) ?? instructorDefaultNameStr;
				string sectionCodeStr = String.Concat((section.IsHybrid ? String.Concat(hybridCodeStr, " ") : string.Empty), section.SectionCode);

				line = String.Concat(tagStr, section.ID.ItemNumber, "\t", sectionCodeStr, "\t", instructorName, "\t", dayTimeStr, roomStr);
			}
			else // Not primary
			{
				line = String.Concat(tagStr, "\t\talso meets\t", dayTimeStr, roomStr);
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
		/// Takes an offered item and determines whether the course qualifies as an evening course.
		/// </summary>
		/// <param name="course">The OfferedItem to evaluate.</param>
		/// <returns>True if evening course, otherwise false.</returns>
		private static bool isEveneingCourse(OfferedItem course)
		{
			bool isEvening = false;

			TimeSpan eveningCourse = new TimeSpan(17, 30, 0); // Hard coded evening course definition; TODO: Move to app settings
			if (course.StartTime.HasValue && course.EndTime.HasValue)
			{
				if (new TimeSpan(course.StartTime.Value.Hour, course.StartTime.Value.Minute, course.StartTime.Value.Second) >= eveningCourse)
				{
					isEvening = false;
				}
			}

			return isEvening;
		}

		/// <summary>
		/// Takes a section and returns the coded tag. This method is only used during a Class Schedule export.
		/// </summary>
		/// <param name="section">The Section being evaluated.</param>
		/// <param name="item">The particular OfferedItem within the given Section being evaluated.</param>
		/// <returns>A string containing the tag code that represents the given Section.</returns>
		private static string getSectionExportTag(Section section, OfferedItem item)
		{
			string tag = "<CLS5>";
			string distanceEdMinStr = "7000";
			string distanceEdMaxStr = "ZZZZ";
			string arrangedStr = "Arranged";

			OfferedItem primary = section.Offered.Where(o => o.IsPrimary).FirstOrDefault();
			if ((primary.Days.Contains("Sa") || primary.Days.Contains("Su")) &&
			    !(primary.Days.Contains("M") || primary.Days.Contains("T") || primary.Days.Contains("W") || primary.Days.Contains("Th"))) // Weekend course
			{
				tag = "<CLS7>";
			}
			else if (isEveneingCourse(primary)) // Evening course
			{
				tag = "<CLS6>";
			}
			else if (section.ID.ItemNumber.CompareTo(distanceEdMinStr) >= 0 && section.ID.ItemNumber.CompareTo(distanceEdMaxStr) <= 0) // Distance Ed
			{
				tag = "<CLSD>";
			}
			else if (item.Days == arrangedStr && !section.IsOnline) // Arranged course
			{
				tag = "<CLSA>";
			}

			return tag;
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
				allSectionsWithSeats = Helpers.GetSectionsWithSeats(yrq.ID, allSections, _csDb);
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
			string CourseOutcome = string.Empty;
			try
			{
				Service1Client client = new Service1Client();
				ICourseID courseID = CourseID.FromString(Subject, ClassNum);
				string realCourseID = Helpers.BuildCourseID(ClassNum, Subject, courseID.IsCommonCourse);
				CourseOutcome = client.GetCourseOutcome(realCourseID);
			}
			catch (Exception ex)
			{
				CourseOutcome = "Error: Cannot find course outcome for this course or cannot connect to the course outcomes webservice.";
			}
			return CourseOutcome;
		}

		/// <summary>
		/// Sets all of the common ViewBag variables
		/// </summary>
		private void SetCommonViewBagVars(OdsRepository repository, string avail, string letter)
		{
			ViewBag.ErrorMsg = "";
			ViewBag.CurrentYearQuarter = repository.CurrentYearQuarter;

			ViewBag.letter = letter;
			ViewBag.avail = string.IsNullOrWhiteSpace(avail) ? "all" : avail;
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
