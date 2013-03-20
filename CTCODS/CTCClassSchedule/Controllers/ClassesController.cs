using System;
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
using MvcMiniProfiler;
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
		[OutputCache(CacheProfile = "AllClassesCacheTime")] // Caches for 6 hours
		public ActionResult Export(String YearQuarterID)
		{
			if (HttpContext.User.Identity.IsAuthenticated == true)
			{
				throw new UnauthorizedAccessException("Class Schedule Exports are currently disabled pending updates.");
				/*
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
				byte[] fileData = encoding.GetBytes(fileText.ToString());
				string fileName = String.Concat("CourseData-", yrq.ID, "-", DateTime.Now.ToShortDateString(), ".rtf");
				return File(fileData, "application/force-download", fileName);
				*/
			}
			else
			{
				throw new UnauthorizedAccessException("You do not have sufficient privileges to export course data.");
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
				// TODO: Refactor the following code into its own method
				// after reconciling the noted differences between AllClasses() and YearQuarter() - 4/27/2012, shawn.south@bellevuecollege.edu
				using (ClassScheduleDb db = new ClassScheduleDb())
				{
          IList<Subject> subjects = db.Subjects.ToList();
          IList<char> subjectLetters = subjects.Select(s => s.Title.First()).Distinct().ToList();
          if (letter != null)
					{
            subjects = subjects.Where(s => s.Title.StartsWith(letter, StringComparison.OrdinalIgnoreCase)).ToList();
					}

					if (format == "json")
					{
						// NOTE: AllowGet exposes the potential for JSON Hijacking (see http://haacked.com/archive/2009/06/25/json-hijacking.aspx)
						// but is not an issue here because we are receiving and returning public (e.g. non-sensitive) data
            return Json(subjects, JsonRequestBehavior.AllowGet);
					}

          // Construct the model
          AllClassesModel model = new AllClassesModel
          {
            CurrentQuarter = repository.CurrentYearQuarter,
            NavigationQuarters = Helpers.getYearQuarterListForMenus(repository),
            Subjects = subjects,
            LettersList = subjectLetters,
            ViewingLetter = String.IsNullOrEmpty(letter) ? (char?)null : letter.First()
          };

					// set up all the ancillary data we'll need to display the View
					SetCommonViewBagVars(repository, string.Empty, letter);
					ViewBag.LinkParams = Helpers.getLinkParams(Request);

          return View(model);
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
				SubjectInfoResult subject = SubjectInfo.GetSubjectInfo(Subject);
				IList<string> prefixes = subject.SubjectCoursePrefixes.Select(p => p.CoursePrefixID).ToList();
				IEnumerable<Course> coursesEnum = (prefixes.Count > 0 ? repository.GetCourses(prefixes) : repository.GetCourses())
																						.Distinct()
																						.OrderBy(c => c.Subject)
																						.ThenBy(c => c.Number);

				SubjectViewModel model = new SubjectViewModel
																 {
																	 Courses = coursesEnum,
																	 Slug = subject.Subject.Slug,
																	 SubjectTitle = subject.Subject.Title,
																	 SubjectIntro = subject.Subject.Intro,
																	 DepartmentTitle = subject.Department.Title,
																	 DepartmentURL = subject.Department.URL,
																	 CurrentQuarter = repository.GetRegistrationQuarters(1)[0],
																	 NavigationQuarters = Helpers.getYearQuarterListForMenus(repository)
																 };

        if (format == "json")
				{
					// NOTE: AllowGet exposes the potential for JSON Hijacking (see http://haacked.com/archive/2009/06/25/json-hijacking.aspx)
					// but is not an issue here because we are receiving and returning public (e.g. non-sensitive) data
					return Json(model, JsonRequestBehavior.AllowGet);
				}

				ViewBag.LinkParams = Helpers.getLinkParams(Request);
				SetCommonViewBagVars(repository, string.Empty, string.Empty);

				return View(model);
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
				// TODO: Refactor the following code into its own method
				// after reconciling the noted differences between AllClasses() and YearQuarter() - 4/27/2012, shawn.south@bellevuecollege.edu
				using (ClassScheduleDb db = new ClassScheduleDb())
				{
					// Compile a list of active subjects
					string commonCourseChar = _apiSettings.RegexPatterns.CommonCourseChar;
					IEnumerable<string> activePrefixes = repository.GetCourseSubjects(yrq).Select(p => p.Subject);
					IList<Subject> subjects = new List<Subject>();
					foreach (Subject sub in db.Subjects)
					{
						// TODO: whether the CoursePrefix has active courses or not, any Prefix with a '&' will be included
						//			 because GetCourseSubjects() does not include the common course char.
						if (sub.SubjectsCoursePrefixes.Select(sp => sp.CoursePrefixID).Any(sp => activePrefixes.Contains(sp.TrimEnd(commonCourseChar.ToCharArray()))))
						{
							subjects.Add(sub);
						}
					}

          ViewBag.alphabet = subjects.Select(s => s.Title.First()).Distinct().ToList();


					IEnumerable<Subject> subjectsEnum;
					if (letter != null)
					{
					    subjectsEnum = subjects.Where(s => s.Title.StartsWith(letter, StringComparison.OrdinalIgnoreCase)).Distinct();
					}
					else
					{
					    subjectsEnum = subjects;
					}
					subjectsEnum.OrderBy(s => s.Title);


          if (format == "json")
					{
						// NOTE: AllowGet exposes the potential for JSON Hijacking (see http://haacked.com/archive/2009/06/25/json-hijacking.aspx)
						// but is not an issue here because we are receiving and returning public (e.g. non-sensitive) data
						return Json(subjectsEnum, JsonRequestBehavior.AllowGet);
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

					return View(subjectsEnum);
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
        // Get the courses to display on the View
				IList<Section> sections;
				using (_profiler.Step("ODSAPI::GetSections()"))
				{
					IList<string> prefixes = SubjectInfo.GetSubjectPrefixes(Subject).Select(p => p.CoursePrefixID).ToList();
					sections = repository.GetSections(prefixes, yrq, facets);
				}

        IList<SectionsBlock> courseBlocks = new List<SectionsBlock>();
				using (ClassScheduleDb db = new ClassScheduleDb())
				{
					IList<SectionWithSeats> sectionsEnum;
					using (_profiler.Step("Getting app-specific Section records from DB"))
					{
						sectionsEnum = Helpers.GetSectionsWithSeats(yrq.ID, sections, db);
					}

					courseBlocks = groupSectionsIntoBlocks(sectionsEnum, db);
        }


        // Construct the model
        SubjectInfoResult subject = SubjectInfo.GetSubjectInfo(Subject);
        IList<YearQuarter> yrqRange = Helpers.getYearQuarterListForMenus(repository);
        YearQuarterSubjectModel model = new YearQuarterSubjectModel
        {
          Courses = courseBlocks,
          CurrentQuarter = repository.CurrentYearQuarter,
          CurrentRegistrationQuarter = yrqRange[0],
          NavigationQuarters = yrqRange,
          ViewingQuarter = yrq,
          Slug = subject.Subject.Slug,
          SubjectTitle = subject.Subject.Title,
          SubjectIntro = subject.Subject.Intro,
          DepartmentTitle = subject.Department.Title,
          DepartmentURL = subject.Department.URL,
        };


        if (format == "json")
				{
					// NOTE: AllowGet exposes the potential for JSON Hijacking (see http://haacked.com/archive/2009/06/25/json-hijacking.aspx)
					// but is not an issue here because we are receiving and returning public (e.g. non-sensitive) data
					return Json(model, JsonRequestBehavior.AllowGet);
				}


        // set up all the ancillary data we'll need to display the View
				ViewBag.timestart = timestart;
				ViewBag.timeend = timeend;
				ViewBag.avail = avail;
				ViewBag.latestart = latestart;
        ViewBag.Modality = Helpers.ConstructModalityList(f_oncampus, f_online, f_hybrid, f_telecourse);
        ViewBag.Days = Helpers.ConstructDaysList(day_su, day_m, day_t, day_w, day_th, day_f, day_s);
        ViewBag.LinkParams = Helpers.getLinkParams(Request);
        SetCommonViewBagVars(repository, avail, string.Empty);

        // TODO: Add query string info (e.g. facets) to the routeValues dictionary so we can pass it all as one chunk.
				IDictionary<string, object> routeValues = new Dictionary<string, object>(3);
				routeValues.Add("YearQuarterID", YearQuarter);
				ViewBag.RouteValues = routeValues;

				return View(model);
			}
		}


		/// <summary>
		/// GET: /Classes/All/{Subject}/{ClassNum}
		/// </summary>
		///
		[OutputCache(CacheProfile = "ClassDetailsCacheTime")]
		public ActionResult ClassDetails(string Prefix, string ClassNum)
		{
      ICourseID courseID = CourseID.FromString(Prefix, ClassNum);
      ClassDetailsModel model = new ClassDetailsModel();

			using (OdsRepository repository = new OdsRepository(HttpContext))
			{
				IList<Course> courses;
				using (_profiler.Step("ODSAPI::GetCourses()"))
				{
					courses = repository.GetCourses(courseID);
				}


        IList<YearQuarter> navigationQuarters = Helpers.getYearQuarterListForMenus(repository);
        IList<YearQuarter> quartersOffered = new List<YearQuarter>();
        string learningOutcomes = string.Empty;
				if (courses.Count > 0)
				{
					using (_profiler.Step("Getting Course counts (per YRQ)"))
					{
						// Identify which, if any, of the current range of quarters has Sections for this Course
						foreach (YearQuarter quarter in navigationQuarters)
						{
							// TODO: for better performance, overload method to accept more than one YRQ
							if (repository.SectionCountForCourse(courseID, quarter) > 0)
							{
								quartersOffered.Add(quarter);
							}
						}
					}

          // Get the course learning outcomes
          using (_profiler.Step("Retrieving course outcomes"))
					{
            learningOutcomes = getCourseOutcome(courseID);
					}
				}

        // Create the model
        model = new ClassDetailsModel
        {
          Courses = courses,
          CurrentQuarter = repository.CurrentYearQuarter,
          NavigationQuarters = navigationQuarters,
          QuartersOffered = quartersOffered,
          CMSFootnote = getCMSFootnote(courseID),
          LearningOutcomes = learningOutcomes
        };

        Subject subject = SubjectInfo.GetSubjectFromPrefix(Prefix);
        if (subject != null)
        {
          SubjectInfoResult programInfo = SubjectInfo.GetSubjectInfo(subject.Slug);
          model.Slug = programInfo.Subject.Slug;
          model.SubjectTitle = programInfo.Subject.Title;
          model.SubjectIntro = programInfo.Subject.Intro;
          model.DepartmentTitle = programInfo.Department.Title;
          model.DepartmentURL = programInfo.Department.URL;
        }
			}

      return View(model);
		}

		#endregion


		#region helper methods
		/// <summary>
		/// Groups a list of Sections by course, into descriptive SectionBlocks
		/// </summary>
		/// <param name="sections">List of sections to group</param>
		/// <returns>List of SectionBlock objects which describe the block of sections</returns>
		private IList<SectionsBlock> groupSectionsIntoBlocks(IList<SectionWithSeats> sections, ClassScheduleDb db)
		{
			IList<SectionsBlock> results = new List<SectionsBlock>();
			IList<SectionWithSeats> allLinkedSections = sections.Where(s => s.IsLinked).ToList();
			IList<SectionWithSeats> nonLinkedSections;

			// sort by the markers indicators where we need to being a new block (w/ course title, etc.)
			/* TODO: Implement a more configurable sort method. */
			using (_profiler.Step("Sorting sections in preparation for grouping and linking"))
			{
				/* COMMENT THIS LINE TO DEBUG
				IEnumerable<SectionWithSeats> d1 = sections.Where(s => s.ID.ItemNumber.StartsWith("410"));
				// END DEBUGGING */
				nonLinkedSections = sections.Where(s => !s.IsLinked)
													.OrderBy(s => s.CourseNumber)
													.ThenBy(s => allLinkedSections.Where(l => l.LinkedTo == s.ID.ItemNumber).Count())
													.ThenByDescending(s => s.IsVariableCredits)
													.ThenBy(s => s.Credits)
													.ThenBy(s => s.IsTelecourse)
													.ThenBy(s => s.IsOnline)
													.ThenBy(s => s.IsHybrid)
													.ThenBy(s => s.IsOnCampus)
													.ThenBy(s => s.SectionCode).ToList();
			}

			// Group all the sections into course blocks and determine which sections linked
			using (_profiler.Step("Grouping/linking by course"))
			{
				int processedCount = 0;
				while (processedCount < nonLinkedSections.Count)
				{
					SectionsBlock courseBlock = new SectionsBlock();
					courseBlock.LinkedSections = new List<SectionWithSeats>();

					IList<SectionWithSeats> remainingSections = nonLinkedSections.Skip(processedCount).ToList();
					SectionWithSeats firstSection = remainingSections.First();

				  courseBlock.Sections = remainingSections.TakeWhile(s =>
				                                                     s.CourseID == firstSection.CourseID &&
				                                                     s.CourseTitle == firstSection.CourseTitle &&
				                                                     s.Credits == firstSection.Credits &&
				                                                     s.IsVariableCredits == firstSection.IsVariableCredits &&
				                                                     allLinkedSections.Count(l => l.LinkedTo == s.ID.ItemNumber) == allLinkedSections.Count(l => l.LinkedTo == firstSection.ID.ItemNumber))
				                                          .ToList();

/* IMPLEMENT IN FUTURE RELEASE
          // Get list of Section/Course cross-listings, if any
				  if (courseBlock.Sections.Any(s => s.IsCrossListed))
				  {
            // NOTE:  This logic assumes that data will only be saved in ClassScheduleDb after having come through
            //        the filter of the CtcApi - which normalizes spacing of the ClassID/SectionID field data.
            courseBlock.CrossListings = db.SectionCourseCrosslistings.Where(x => courseBlock.Sections.Select(s => s.ID.ToString()).Contains(x.ClassID)).ToList();
				  }
*/

					// Find all links associated to each of the grouped sections
					foreach (SectionWithSeats sec in courseBlock.Sections)
					{
					  SectionWithSeats sect = sec;  // Use copy of object to ensure cross-compiler compatibility
					  List<SectionWithSeats> linkedSections = allLinkedSections.Where(s => sect != null && s.LinkedTo == sect.ID.ItemNumber).ToList();
						if (linkedSections.Count > 0)
						{
							courseBlock.LinkedSections.AddRange(linkedSections);
						}
					}

					// Get a list of common footnotes shared with every section in the block
					courseBlock.CommonFootnotes = Helpers.ExtractCommonFootnotes(courseBlock.Sections);

					processedCount += courseBlock.Sections.Count();
					results.Add(courseBlock);
				}
			}

			return results;
		}

		/// <summary>
		/// Takes a Subject, ClassNum and CourseID and finds the CMS Footnote for the course.
		/// </summary>
		/// <param name="Subject">The course Subject</param>
		/// <param name="ClassNum">The CourseNumber</param>
		/// /// <param name="courseID">The courseID for the course.</param>
		private string getCMSFootnote(ICourseID courseId)
		{
			using (ClassScheduleDb db = new ClassScheduleDb())
			{
				using (_profiler.Step("Getting app-specific Section records from DB"))
				{
					CourseMeta item = null;
					char trimChar = _apiSettings.RegexPatterns.CommonCourseChar.ToCharArray()[0];

          string FullCourseID = Helpers.BuildCourseID(courseId.Number, courseId.Subject.TrimEnd(), courseId.IsCommonCourse);
          item = db.CourseMetas.Where(s => s.CourseID.Trim().Equals(FullCourseID, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

					if (item != null)
					{
						return item.Footnote;
					}

				}
			}
			return null;
		}

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
		/*
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
		*/
		/// <summary>
		/// Gets the course outcome information by scraping the Bellevue College
		/// course outcomes website
		/// </summary>
		private static dynamic getCourseOutcome(ICourseID courseId)
		{
			string CourseOutcome = string.Empty;
			try
			{
				Service1Client client = new Service1Client();
        string FullCourseID = Helpers.BuildCourseID(courseId.Number, courseId.Subject.TrimEnd(), courseId.IsCommonCourse);
        CourseOutcome = client.GetCourseOutcome(FullCourseID);
			}
			catch (Exception)
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
			ViewBag.ErrorMsg = string.Empty;
			ViewBag.CurrentYearQuarter = repository.CurrentYearQuarter;

			ViewBag.letter = letter;
			ViewBag.avail = string.IsNullOrWhiteSpace(avail) ? "all" : avail;
		}
		#endregion

		public static IList<SectionWithSeats> ParseCommonHeadingLinkedSections(List<SectionWithSeats> linkedSections)
		{
			string prevCourseID = string.Empty;
			string prevTitle = string.Empty;
			decimal prevCredits = 0;
			bool prevIsVariableCredits = false;

			IList<SectionWithSeats> common = new List<SectionWithSeats>(linkedSections.Count);

			foreach (SectionWithSeats section in linkedSections)
			{
				if (!(section.CourseID == prevCourseID && section.CourseTitle == prevTitle && section.Credits == prevCredits && section.IsVariableCredits == prevIsVariableCredits))
				{
					common.Add(section);
				}

				prevCourseID = section.CourseID;
				prevTitle = section.CourseTitle;
				prevCredits = section.Credits;
				prevIsVariableCredits = section.IsVariableCredits;
			}

			return common;
		}
	}
}
