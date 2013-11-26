using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Ctc.Ods;
using Ctc.Ods.Config;
using Ctc.Ods.Data;
using Ctc.Ods.Types;
using CTCClassSchedule.Common;
using CTCClassSchedule.Models;
using CtcApi.Extensions;
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
			using (OdsRepository repository = new OdsRepository())
			{
				ViewBag.CurrentYearQuarter = repository.CurrentYearQuarter;
				ViewBag.QuarterNavMenu = Helpers.GetYearQuarterListForMenus(repository);
				ViewBag.RegistrationQuarters = Helpers.GetFutureQuarters(repository);
			}
			return View();
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

#if DEBUG
          Debug.Print("======= Subject list =======");
				  foreach (Subject subject in subjects)
				  {
				    Debug.Print("{0} ({1})", subject.Title, subject.CoursePrefixes.Select(p => p.CoursePrefixID).ToArray().Mash(", "));
				  }
          Debug.Print("===== END Subject list =====");
#endif
          // Construct the model
          AllClassesModel model = new AllClassesModel
          {
            CurrentQuarter = repository.CurrentYearQuarter,
            NavigationQuarters = Helpers.GetYearQuarterListForMenus(repository),
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
				IList<string> prefixes = subject.CoursePrefixes.Select(p => p.CoursePrefixID).ToList();
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
																	 CurrentQuarter = repository.CurrentYearQuarter,
																	 NavigationQuarters = Helpers.GetYearQuarterListForMenus(repository)
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
		public ActionResult YearQuarter(String YearQuarter, string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string avail, string letter, string latestart, string numcredits, string format)
		{
		  _log.Trace(m => m("Calling: [.../classes/{0}...] From (referrer): [{1}]", YearQuarter, Request.UrlReferrer));

      // TODO: come up with a better way to maintain various State flags
      ViewBag.Modality = Helpers.ConstructModalityList(f_oncampus, f_online, f_hybrid);
      ViewBag.Days = Helpers.ConstructDaysList(day_su, day_m, day_t, day_w, day_th, day_f, day_s);
      ViewBag.LinkParams = Helpers.getLinkParams(Request);
      ViewBag.timestart = timestart;
      ViewBag.timeend = timeend;
      ViewBag.latestart = latestart;
      ViewBag.avail = avail;
      IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s, f_oncampus, f_online, f_hybrid, avail, latestart, numcredits);

      YearQuarterModel model = new YearQuarterModel
      {
        ViewingSubjects = new List<Subject>(),
        SubjectLetters = new List<char>(),
      };

      try
      {
        YearQuarter yrq = string.IsNullOrWhiteSpace(YearQuarter)
                            ? null
                            : Ctc.Ods.Types.YearQuarter.FromFriendlyName(YearQuarter);
        model.ViewingQuarter = yrq;

        using (OdsRepository repository = new OdsRepository(HttpContext))
        {
          model.NavigationQuarters = Helpers.GetYearQuarterListForMenus(repository);

          // set up all the ancillary data we'll need to display the View
          SetCommonViewBagVars(repository, avail, letter);

          // TODO: Refactor the following code to use ApiController.GetSubjectList()
          // after reconciling the noted differences between AllClasses() and YearQuarter() - 4/27/2012, shawn.south@bellevuecollege.edu
          using (ClassScheduleDb db = new ClassScheduleDb())
          {
            // Compile a list of active subjects
            char[] commonCourseChar = _apiSettings.RegexPatterns.CommonCourseChar.ToCharArray();
            IEnumerable<string> activePrefixes = repository.GetCourseSubjects(yrq, facets).Select(p => p.Subject);

            IList<Subject> subjects = new List<Subject>();
            // NOTE: Unable to reduce the following loop to a LINQ statement because it complains about not being able to use TrimEnd(char[]) w/ LINQ-to-Entities
            // (Although it appears to be doing so just fine in the if statement below). - shawn.south@bellevuecollege.edu
            foreach (Subject sub in db.Subjects)
            {
              // TODO: whether the CoursePrefix has active courses or not, any Prefix with a '&' will be included
              //			 because GetCourseSubjects() does not include the common course char.
              if (
                sub.CoursePrefixes.Select(sp => sp.CoursePrefixID)
                   .Any(sp => activePrefixes.Contains(sp.TrimEnd(commonCourseChar))))
              {
                subjects.Add(sub);
              }
            }

            model.SubjectLetters = subjects.Select(s => s.Title.First()).Distinct().ToList();
            model.ViewingSubjects = (letter != null
                                       ? subjects.Where(
                                         s => s.Title.StartsWith(letter, StringComparison.OrdinalIgnoreCase)).Distinct()
                                       : subjects
                                    ).ToList();

            if (format == "json")
            {
              // NOTE: AllowGet exposes the potential for JSON Hijacking (see http://haacked.com/archive/2009/06/25/json-hijacking.aspx)
              // but is not an issue here because we are receiving and returning public (e.g. non-sensitive) data
              return Json(model, JsonRequestBehavior.AllowGet);
            }

            return View(model);
          }
        }
      }
      catch (ArgumentOutOfRangeException ex)
      {
        if (ex.Message.ToUpper().Contains("MUST BE A VALID QUARTER TITLE"))
        {
          throw new HttpException(404, string.Format("'{0}' is not a recognized Quarter.", YearQuarter), ex);
        }
        _log.Error(m => m("An unhandled ArgumentOutOfRangeException ocurred, returning an empty Model to the YearQuarter view."), ex);
      }
      catch (Exception ex)
		  {
		    _log.Error(m => m("An unhandled exception occurred, returning an empty Model to the YearQuarter view."), ex);
		  }

      // empty model
		  return View(model);
		}


		/// <summary>
		/// GET: /Classes/{FriendlyYRQ}/{Subject}/
		/// </summary>
		[OutputCache(CacheProfile = "YearQuarterSubjectCacheTime")] // Caches for 30 minutes
		public ActionResult YearQuarterSubject(String YearQuarter, string Subject, string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string avail, string latestart, string numcredits, string format)
		{
			YearQuarter yrq = Ctc.Ods.Types.YearQuarter.FromFriendlyName(YearQuarter);
			IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s, f_oncampus, f_online, f_hybrid, avail, latestart, numcredits);

			using (OdsRepository repository = new OdsRepository(HttpContext))
			{
        // Get the courses to display on the View
				IList<Section> sections;
				using (_profiler.Step("ODSAPI::GetSections()"))
				{
					IList<string> prefixes = SubjectInfo.GetSubjectPrefixes(Subject);
					sections = repository.GetSections(prefixes, yrq, facets);
				}

#if DEBUG
          /* COMMENT THIS LINE FOR DEBUGGING/TROUBLESHOOTING
        Debug.Print("==> sections");
        string zItemNum = "1230", zYrq = "B234";  // ENGL 266
        if (sections.Any(s => s.ID.ItemNumber == zItemNum && s.ID.YearQuarter == zYrq))
        {
          Section foo = sections.Where(s => s.ID.ItemNumber == zItemNum && s.ID.YearQuarter == zYrq).First();
          Debug.Print("\n{0} - {1} {2}\t(Linked to: {3})", foo.ID, foo.CourseID, foo.CourseTitle, foo.LinkedTo);
        }
        else
        {
          Debug.Print("ClassID '{0}{1}' not found.", zItemNum, zYrq);
        }
        // */
#endif

        IList<SectionsBlock> courseBlocks = new List<SectionsBlock>();
				using (ClassScheduleDb db = new ClassScheduleDb())
				{
					IList<SectionWithSeats> sectionsEnum;
					using (_profiler.Step("Getting app-specific Section records from DB"))
					{
						sectionsEnum = Helpers.GetSectionsWithSeats(yrq.ID, sections, db);
					}

#if DEBUG
          /* COMMENT THIS LINE FOR DEBUGGING/TROUBLESHOOTING
				  Debug.Print("==> sectionsEnum");
//				  string zItemNum, zYrq;
				  zItemNum = "1230"; zYrq = "B234"; // ENGL 266
          if (sectionsEnum.Any(s => s.ID.ItemNumber == zItemNum && s.ID.YearQuarter == zYrq))
				  {
				    SectionWithSeats foo = sectionsEnum.Where(s => s.ID.ItemNumber == zItemNum && s.ID.YearQuarter == zYrq).First();
				    Debug.Print("\n{0} - {1} {2}\t(Linked to: {3})", foo.ID, foo.CourseID, foo.CourseTitle, foo.LinkedTo);
				  }
          else
          {
            Debug.Print("ClassID '{0}{1}' not found.", zItemNum, zYrq);
          }
          // */

          /* COMMENT THIS LINE FOR DEBUGGING/TROUBLESHOOTING
				  IEnumerable<SectionWithSeats> s1 = sectionsEnum.Where(s => s.CourseSubject == "ENGL" && s.CourseNumber == "239");
          // */
#endif
					courseBlocks = Helpers.GroupSectionsIntoBlocks(sectionsEnum, db);
        }

        // Construct the model
        SubjectInfoResult subject = SubjectInfo.GetSubjectInfoFromPrefix(Subject);
        IList<YearQuarter> yrqRange = Helpers.GetYearQuarterListForMenus(repository);

			  YearQuarterSubjectModel model = new YearQuarterSubjectModel
			                                    {
			                                      Courses = courseBlocks,
			                                      CurrentQuarter = repository.CurrentYearQuarter,
			                                      CurrentRegistrationQuarter = yrqRange[0],
			                                      NavigationQuarters = yrqRange,
			                                      ViewingQuarter = yrq,
                                            // if we were unable to determine a Slug, use the Subject (e.g. Prefix) that brought us here
			                                      Slug = subject != null ? subject.Subject.Slug : Subject,
			                                      SubjectTitle = subject != null ? subject.Subject.Title : string.Empty,
			                                      SubjectIntro = subject != null ? subject.Subject.Intro : string.Empty,
			                                      DepartmentTitle = subject != null ? subject.Department.Title : string.Empty,
			                                      DepartmentURL = subject != null ? subject.Department.URL : string.Empty,
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
        ViewBag.Modality = Helpers.ConstructModalityList(f_oncampus, f_online, f_hybrid);
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


        IList<YearQuarter> navigationQuarters = Helpers.GetYearQuarterListForMenus(repository);
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
          LearningOutcomes = learningOutcomes,
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
          model.DivisionTitle = programInfo.Division.Title;
          model.DivisionURL = programInfo.Division.URL;
        }
			}

      return View(model);
		}

		#endregion


		#region helper methods
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
	}
}
