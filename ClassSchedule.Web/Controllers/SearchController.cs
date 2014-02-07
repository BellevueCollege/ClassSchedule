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
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Ctc.Ods;
using Ctc.Ods.Data;
using Ctc.Ods.Types;
using CTCClassSchedule.Common;
using CTCClassSchedule.Models;
using MvcMiniProfiler;

namespace CTCClassSchedule.Controllers
{
	public class SearchController : Controller
	{
    const int ITEMS_PER_PAGE = 40;
    readonly private MiniProfiler _profiler = MiniProfiler.Current;

	  public SearchController()
		{
		  ViewBag.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		}

	  //
		// GET: /Search/
		public ActionResult Index(string searchterm, string Subject, string quarter, string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string avail, string latestart, string numcredits, int p_offset = 0)
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

      // TODO: replace ViewBag calls w/ ref to FacetHelper
			ViewBag.Subject = Subject;
			ViewBag.searchterm = Regex.Replace(searchterm, @"\s+", " ");	// replace each clump of whitespace w/ a single space (so the database can better handle it)
      ViewBag.ErrorMsg = string.Empty;

      FacetHelper facetHelper = new FacetHelper(Request, "submit");
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
				YearQuarter yrq = string.IsNullOrWhiteSpace(quarter) ? repository.CurrentYearQuarter : YearQuarter.FromFriendlyName(quarter);
        IList<YearQuarter> menuQuarters = Helpers.GetYearQuarterListForMenus(repository);
			  QuarterNavigationModel quarterNavigation = new QuarterNavigationModel
			                                               {
			                                                 NavigationQuarters = menuQuarters,
			                                                 CurrentQuarter = menuQuarters[0],
			                                                 ViewingQuarter = yrq,
			                                               };

				IList<Section> sections;
				using (_profiler.Step("API::GetSections()"))
				{
					if (string.IsNullOrWhiteSpace(Subject))
					{
						sections = repository.GetSections(yrq, facets);
					}
					else
					{
            IList<string> prefixes = SubjectInfo.GetSubjectPrefixes(Subject);
						sections = repository.GetSections(prefixes, yrq, facets);
					}
				}

        int currentPage;
        int totalPages;
        int itemCount;
        IList<SectionWithSeats> sectionsEnum;
				IList<SearchResult> searchResults;
        SearchResultNoSectionModel noSectionSearchResults;
			  IList<SectionsBlock> courseBlocks;
			  using (ClassScheduleDb db = new ClassScheduleDb())
				{
					searchResults = GetSearchResults(db, searchterm, quarter);
					noSectionSearchResults = GetNoSectionSearchResults(db, searchterm, yrq);

          sections = (from s in sections
                      join r in searchResults on s.ID.ToString() equals r.ClassID
                      select s).ToList();

					sectionsEnum = Helpers.GetSectionsWithSeats(yrq.ID, sections, db);

          // do not count Linked sections (since we don't display them)
          itemCount = sectionsEnum.Count(s => !s.IsLinked);

				  totalPages = (int)Math.Round((itemCount / ITEMS_PER_PAGE) + 0.5);
				  currentPage = p_offset + 1;

          using (_profiler.Step("Getting just records for page"))
          {
            if (currentPage > totalPages && totalPages > 0)
            {
              currentPage = totalPages;
            }
            sectionsEnum = sectionsEnum.Skip(p_offset * ITEMS_PER_PAGE).Take(ITEMS_PER_PAGE).ToList();
          }

          courseBlocks = Helpers.GroupSectionsIntoBlocks(sectionsEnum, db);
				}

				IEnumerable<string> allSubjects;
				using (_profiler.Step("Getting distinct list of subjects"))
				{
					allSubjects = sectionsEnum.Select(c => c.CourseSubject).Distinct().OrderBy(c => c);
				}

        SearchResultsModel model = new SearchResultsModel
			                               {
			                                 ItemCount = itemCount,
                                       TotalPages = totalPages,
                                       CurrentPage = currentPage,
                                       Courses = courseBlocks,
			                                 SearchResultNoSection = noSectionSearchResults,
			                                 AllSubjects = allSubjects,
                                       QuarterNavigation = quarterNavigation,
                                       FacetData = facetHelper
			                               };
				return View(model);
			}
		}

		#region helper methods
	  /// <summary>
	  ///
	  /// </summary>
	  /// <param name="db"></param>
	  /// <param name="searchterm"></param>
	  /// <param name="yrq"></param>
	  /// <returns></returns>
    private SearchResultNoSectionModel GetNoSectionSearchResults(ClassScheduleDb db, string searchterm, YearQuarter yrq)
		{
			SqlParameter[] parms = {
							new SqlParameter("SearchWord", searchterm),
							new SqlParameter("YearQuarterID", yrq.ID)
			                       };

      SearchResultNoSectionModel model = new SearchResultNoSectionModel {SearchedYearQuarter = yrq};
	    using (_profiler.Step("Executing 'other classes' stored procedure"))
      {
        model.NoSectionSearchResults = db.ExecuteStoreQuery<SearchResultNoSection>("usp_CourseSearch @SearchWord, @YearQuarterID", parms).ToList();
      }
      return model;
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
		#endregion
	}
}
