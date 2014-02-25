/*
This file is part of CtcClassSchedule.

CtcClassSchedule is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

CtcClassSchedule is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with CtcClassSchedule.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Ctc.Ods;
using Ctc.Ods.Config;
using Ctc.Ods.Data;
using Ctc.Ods.Types;
using CTCClassSchedule.Common;
using CTCClassSchedule.Models;
using CtcApi.Extensions;
using Microsoft.Security.Application;
using MvcMiniProfiler;

namespace CTCClassSchedule.Controllers
{
// ReSharper disable RedundantExtendsListEntry
	public partial class ClassesController : Controller
// ReSharper restore RedundantExtendsListEntry
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
          // force CoursePrefixes to load up front for Subjects (otherwise we get an error trying to load after db has been disposed)
          db.ContextOptions.LazyLoadingEnabled = false;
          IList<Subject> subjects = db.Subjects.Include("CoursePrefixes").ToList();

          IList<char> subjectLetters = subjects.Select(s => s.Title.First()).Distinct().ToList();
          if (letter != null)
					{
            subjects = subjects.Where(s => s.Title.StartsWith(letter, StringComparison.OrdinalIgnoreCase)).ToList();
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
            Subjects = subjects.Select(s => new SubjectModel
                                              {
                                                Title = s.Title,
                                                Slug = s.Slug,
                                                CoursePrefixes = s.CoursePrefixes.Select(p => p.CoursePrefixID).ToList()
                                              }).ToList(),
            LettersList = subjectLetters,
            ViewingLetter = String.IsNullOrEmpty(letter) ? (char?)null : letter.First()
          };


          if (format == "json")
          {
            // NOTE: AllowGet exposes the potential for JSON Hijacking (see http://haacked.com/archive/2009/06/25/json-hijacking.aspx)
            // but is not an issue here because we are receiving and returning public (e.g. non-sensitive) data
            return Json(model, JsonRequestBehavior.AllowGet);
          }
          
          // set up all the ancillary data we'll need to display the View
					SetCommonViewBagVars(repository, letter);
				  FacetHelper facetHelper = new FacetHelper(Request);
				  ViewBag.LinkParams = facetHelper.LinkParameters;

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

        // TODO: Utilize SubjectModel in SubjectViewModel
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

			  FacetHelper facetHelper = new FacetHelper(Request);
			  ViewBag.LinkParams = facetHelper.LinkParameters;
				SetCommonViewBagVars(repository, string.Empty);

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

      FacetHelper facetHelper = new FacetHelper(Request);
		  facetHelper.SetModalities(f_oncampus, f_online, f_hybrid);
		  facetHelper.SetDays(day_su, day_m, day_t, day_w, day_th, day_f, day_s);
		  facetHelper.TimeStart = timestart;
		  facetHelper.TimeEnd = timeend;
		  facetHelper.LateStart = latestart;
		  facetHelper.Availability = avail;
		  facetHelper.Credits = numcredits;
      ViewBag.LinkParams = facetHelper.LinkParameters;

      IList<ISectionFacet> facets = facetHelper.CreateSectionFacets();

      YearQuarterModel model = new YearQuarterModel
      {
        ViewingSubjects = new List<SubjectModel>(),
        SubjectLetters = new List<char>(),
        FacetData = facetHelper
      };

      try
      {
        using (OdsRepository repository = new OdsRepository())
        {
          YearQuarter yrq;
          try
          {
            yrq = Helpers.DetermineRegistrationQuarter(YearQuarter, repository.CurrentRegistrationQuarter, RouteData);
          }
          catch (ArgumentOutOfRangeException ex)
          {
            _log.Warn(m => m("Invalid URL attempt: <{0}>", Request.Url), ex);
            return HttpNotFound(string.Format("'{0}' is not a valid quarter.", Encoder.HtmlEncode(YearQuarter)));
          }
          model.ViewingQuarter = yrq;

          model.NavigationQuarters = Helpers.GetYearQuarterListForMenus(repository);

          // set up all the ancillary data we'll need to display the View
          SetCommonViewBagVars(repository, letter);

          // TODO: Refactor the following code to use ApiController.GetSubjectList()
          // after reconciling the noted differences between AllClasses() and YearQuarter() - 4/27/2012, shawn.south@bellevuecollege.edu
          using (ClassScheduleDb db = new ClassScheduleDb())
          {
            // Compile a list of active subjects
            char[] commonCourseChar = _apiSettings.RegexPatterns.CommonCourseChar.ToCharArray();
            IList<string> activePrefixes = repository.GetCourseSubjects(yrq, facets).Select(p => p.Subject).ToList();

            IList<Subject> subjects = new List<Subject>();
            // NOTE: Unable to reduce the following loop to a LINQ statement because it complains about not being able to use TrimEnd(char[]) w/ LINQ-to-Entities
            // (Although it appears to be doing so just fine in the if statement below). - shawn.south@bellevuecollege.edu
            foreach (Subject sub in db.Subjects)
            {
              // TODO: whether the CoursePrefix has active courses or not, any Prefix with a '&' will be included
              //			 because GetCourseSubjects() does not include the common course char.
              if (sub.CoursePrefixes.Select(sp => sp.CoursePrefixID).Any(sp => activePrefixes.Contains(sp.TrimEnd(commonCourseChar))))
              {
                subjects.Add(sub);
              }
            }

            model.SubjectLetters = subjects.Select(s => s.Title.First()).Distinct().ToList();
            model.ViewingSubjects = (letter != null
                                       ? subjects.Where(s => s.Title.StartsWith(letter, StringComparison.OrdinalIgnoreCase)).Distinct()
                                       : subjects
                                    ).Select(s => new SubjectModel
                                            {
                                              Title = s.Title,
                                              Slug = s.Slug,
                                              CoursePrefixes = s.CoursePrefixes.Select(p => p.CoursePrefixID).ToList()
                                            }
                                    ).ToList();

            if (format == "json")
            {
              // NOTE: AllowGet exposes the potential for JSON Hijacking (see http://haacked.com/archive/2009/06/25/json-hijacking.aspx)
              // but is not an issue here because we are receiving and returning public (e.g. non-sensitive) data
              JsonResult json = Json(model, JsonRequestBehavior.AllowGet);
              return json;
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
		[OutputCache(CacheProfile = "YearQuarterSubjectCacheTime")]
		public ActionResult YearQuarterSubject(String YearQuarter, string Subject, string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string avail, string latestart, string numcredits, string format)
		{
      FacetHelper facetHelper = new FacetHelper(Request);
      facetHelper.SetModalities(f_oncampus, f_online, f_hybrid);
      facetHelper.SetDays(day_su, day_m, day_t, day_w, day_th, day_f, day_s);
      facetHelper.TimeStart = timestart;
      facetHelper.TimeEnd = timeend;
      facetHelper.LateStart = latestart;
      facetHelper.Availability = avail;
      facetHelper.Credits = numcredits;
      ViewBag.LinkParams = facetHelper.LinkParameters;
      
      IList<ISectionFacet> facets = facetHelper.CreateSectionFacets();

			using (OdsRepository repository = new OdsRepository())
			{
			  YearQuarter yrq = Helpers.DetermineRegistrationQuarter(YearQuarter, repository.CurrentRegistrationQuarter, RouteData);
        
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

        IList<SectionsBlock> courseBlocks;
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
                                            FacetData = facetHelper
			                                    };

			    if (format == "json")
			    {
			      // NOTE: AllowGet exposes the potential for JSON Hijacking (see http://haacked.com/archive/2009/06/25/json-hijacking.aspx)
			      // but is not an issue here because we are receiving and returning public (e.g. non-sensitive) data
			      return Json(model, JsonRequestBehavior.AllowGet);
			    }

        SetCommonViewBagVars(repository, string.Empty);

				return View(model);
			}
		}


		/// <summary>
		/// GET: /Classes/All/{Subject}/{ClassNum}
		/// </summary>
		///
		[OutputCache(CacheProfile = "ClassDetailsCacheTime")]
		public ActionResult ClassDetails(string Prefix, string ClassNum, string format)
		{
      ICourseID courseID = CourseID.FromString(Prefix, ClassNum);
      ClassDetailsModel model;

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
            learningOutcomes = Helpers.GetCourseOutcome(courseID);
					}
				}

        // Create the model
        model = new ClassDetailsModel
        {
          Courses = courses,
          CurrentQuarter = repository.CurrentYearQuarter,
          NavigationQuarters = navigationQuarters,
          QuartersOffered = quartersOffered,
          CMSFootnote = GetCmsFootnote(courseID),
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

      if (format == "json")
      {
        // NOTE: AllowGet exposes the potential for JSON Hijacking (see http://haacked.com/archive/2009/06/25/json-hijacking.aspx)
        // but is not an issue here because we are receiving and returning public (e.g. non-sensitive) data
        return Json(model, JsonRequestBehavior.AllowGet);
      }
      return View(model);
		}

		#endregion


		#region helper methods
    /// <summary>
    /// Takes a Subject, ClassNum and CourseID and finds the CMS Footnote for the course.
    /// </summary>
    /// <param name="courseID"></param>
    private string GetCmsFootnote(ICourseID courseID)
    {
      using (ClassScheduleDb db = new ClassScheduleDb())
      {
        using (_profiler.Step("Getting app-specific Section records from DB"))
        {
          string fullCourseID = Helpers.BuildCourseID(courseID.Number, courseID.Subject.TrimEnd(), courseID.IsCommonCourse);
          CourseMeta item = db.CourseMetas.Where(s => s.CourseID.Trim().Equals(fullCourseID, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

          if (item != null)
          {
            return item.Footnote;
          }

        }
      }
      return null;
    }

	  /// <summary>
		/// Sets all of the common ViewBag variables
		/// </summary>
		private void SetCommonViewBagVars(OdsRepository repository, string letter)
		{
			ViewBag.ErrorMsg = string.Empty;
			ViewBag.CurrentYearQuarter = repository.CurrentYearQuarter;

			ViewBag.letter = letter;
		}
		#endregion
	}
}
