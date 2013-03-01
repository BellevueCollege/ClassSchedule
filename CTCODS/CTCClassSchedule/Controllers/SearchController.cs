using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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

namespace CTCClassSchedule.Controllers
{
	public class SearchController : Controller
	{
		readonly private MiniProfiler _profiler = MiniProfiler.Current;
		private ApiSettings _apiSettings = ConfigurationManager.GetSection(ApiSettings.SectionName) as ApiSettings;

		public SearchController()
		{
			ViewBag.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		}

		//
		// GET: /Search/
		public ActionResult Index(string searchterm, string Subject, string quarter, string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string f_telecourse, string avail, string latestart, string numcredits, int p_offset = 0)
		{
			// We don't currently support quoted phrases. - 4/19/2012, shawn.south@bellevuecollege.edu
			searchterm = searchterm.Replace("\"", string.Empty);

			// TODO: This needs to be configurable
			if (quarter == "CE")
			{
				Response.Redirect("http://www.campusce.net/BC/Search/Search.aspx?q=" + searchterm, true);
				return null;
			}

			if (String.IsNullOrEmpty(searchterm.Trim()))
			{
				return RedirectToAction("AllClasses", "Classes", new { YearQuarterID = quarter });
			}

			ViewBag.timestart = timestart;
			ViewBag.timeend = timeend;
			ViewBag.Modality = Helpers.ConstructModalityList(f_oncampus, f_online, f_hybrid, f_telecourse);
			ViewBag.Days = Helpers.ConstructDaysList(day_su, day_m, day_t, day_w, day_th, day_f, day_s);
			ViewBag.avail = avail;
			ViewBag.p_offset = p_offset;
			ViewBag.Subject = Subject;
			ViewBag.searchterm = Regex.Replace(searchterm, @"\s+", " ");	// replace each clump of whitespace w/ a single space (so the database can better handle it)

			//add the dictionary that converts MWF -> Monday/Wednesday/Friday for section display.
			TempData["DayDictionary"] = Helpers.getDayDictionary();

			IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s,
			                                                f_oncampus, f_online, f_hybrid, f_telecourse, avail, latestart, numcredits);

			// TODO: Add query string info (e.g. facets) to the routeValues dictionary so we can pass it all as one chunk.
			IDictionary<string, object> routeValues = new Dictionary<string, object>(3);
			routeValues.Add("YearQuarterID", quarter);
			ViewBag.RouteValues = routeValues;
			// TODO: Aren't link parameters part of the route values? Let's merge these two somehow.
			// (Is RouteValues even being used?)
			ViewBag.LinkParams = Helpers.getLinkParams(Request, "submit");

			using (OdsRepository repository = new OdsRepository(HttpContext))
			{
				setViewBagVars(string.Empty, string.Empty, string.Empty, avail, string.Empty, repository);

				YearQuarter YRQ = string.IsNullOrWhiteSpace(quarter) ? repository.CurrentYearQuarter : YearQuarter.FromFriendlyName(quarter);
				ViewBag.YearQuarter = YRQ;
				IList<YearQuarter> menuQuarters = Helpers.getYearQuarterListForMenus(repository);
				ViewBag.QuarterNavMenu = menuQuarters;
				ViewBag.CurrentRegistrationQuarter = menuQuarters[0];

				IList<Section> sections;
				using (_profiler.Step("API::GetSections()"))
				{
					if (string.IsNullOrWhiteSpace(Subject))
					{
						sections = repository.GetSections(YRQ, facets);
					}
					else
					{
						IList<string> subjects = new List<string> {Subject, string.Concat(Subject, _apiSettings.RegexPatterns.CommonCourseChar)};
						sections = repository.GetSections(subjects, YRQ, facets);
					}
				}

				IList<SectionWithSeats> sectionsEnum;
				IList<SearchResult> SearchResults;
				IList<SearchResultNoSection> NoSectionSearchResults;
				using (ClassScheduleDb db = new ClassScheduleDb())
				{
					SearchResults = GetSearchResults(db, searchterm, quarter);
					NoSectionSearchResults = GetNoSectionSearchResults(db, searchterm, quarter);

					sections = (from s in sections
											join r in SearchResults on s.ID.ToString() equals r.ClassID
											select s).ToList();

					sectionsEnum = Helpers.GetSectionsWithSeats(YRQ.ID, sections, db)
																		.OrderBy(x => x.CourseNumber)
																		.ThenBy(x => x.CourseTitle)
																		.ThenBy(s => s.IsTelecourse)
																		.ThenBy(s => s.IsOnline)
																		.ThenBy(s => s.IsHybrid)
																		.ThenBy(s => s.IsOnCampus)
																		.ThenBy(s => s.SectionCode).ToList();;
				}


				// do not count Linked sections (since we don't display them)
				IEnumerable<SectionWithSeats> countedSections = sectionsEnum.Where(s => !s.IsLinked);
				int itemCount = countedSections.Count();
				ViewBag.ItemCount = itemCount;

				int courseCount;
				using (_profiler.Step("Getting count of courses"))
				{
					courseCount = countedSections.Select(s => s.CourseID).Distinct().Count();
				}
				ViewBag.CourseCount = courseCount;

				IEnumerable<string> allSubjects;
				using (_profiler.Step("Getting distinct list of subjects"))
				{
					allSubjects = sectionsEnum.Select(c => c.CourseSubject).Distinct().OrderBy(c => c);
				}

				using (_profiler.Step("Getting just records for page"))
				{
					sectionsEnum = sectionsEnum.Skip(p_offset * 40).Take(40).ToList();
				}

				ViewBag.TotalPages = Math.Ceiling(itemCount / 40.0);

				SearchResultsModel model = new SearchResultsModel
								{
										Section = sectionsEnum,
										SearchResultNoSection = NoSectionSearchResults,
										Subjects = allSubjects
								};

				ViewBag.CurrentPage = p_offset + 1;

				return View(model);
			}
		}

		#region helper methods
		/// <summary>
		///
		/// </summary>
		/// <param name="db"></param>
		/// <param name="searchterm"></param>
		/// <param name="quarter"></param>
		/// <returns></returns>
		private IList<SearchResultNoSection> GetNoSectionSearchResults(ClassScheduleDb db, string searchterm, string quarter)
		{
			SqlParameter[] parms = {
							new SqlParameter("SearchWord", searchterm),
							new SqlParameter("YearQuarterID", YearQuarter.ToYearQuarterID(quarter))
			                       };

			using (_profiler.Step("Executing 'other classes' stored procedure"))
			{
				return db.ExecuteStoreQuery<SearchResultNoSection>("usp_CourseSearch @SearchWord, @YearQuarterID", parms).ToList();
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="db"></param>
		/// <param name="searchterm"></param>
		/// <param name="quarter"></param>
		/// <returns></returns>
		private IList<SearchResult> GetSearchResults(ClassScheduleDb db, string searchterm, string quarter)
		{
			SqlParameter[] parms = {
							new SqlParameter("SearchWord", searchterm),
							new SqlParameter("YearQuarterID", YearQuarter.ToYearQuarterID(quarter))
			                       };

			using (_profiler.Step("Executing search stored procedure"))
			{
				return db.ExecuteStoreQuery<SearchResult>("usp_ClassSearch @SearchWord, @YearQuarterID", parms).ToList();
			}
		}

		/// <summary>
		/// Sets all of the common ViewBag variables
		/// </summary>
		private void setViewBagVars(string flex, string time, string days, string avail, string letter, OdsRepository repository)
		{
			ViewBag.ErrorMsg = "";
			ViewBag.CurrentYearQuarter = repository.CurrentYearQuarter;

			ViewBag.letter = letter;
			ViewBag.flex = flex ?? "all";
			ViewBag.time = time ?? "all";
			ViewBag.avail = avail ?? "all";

			ViewBag.activeClass = " class=active";
		}
		#endregion
	}
}
