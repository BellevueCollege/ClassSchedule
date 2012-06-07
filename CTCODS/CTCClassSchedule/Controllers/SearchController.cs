﻿using System;
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
		public ActionResult Index(string searchterm, string Subject, string quarter, string timestart, string timeend, string[] days, string[] classformat, string avail, string latestart, string numcredits, int p_offset = 0)
		{
			// We don't currently support quoted phrases. - 4/19/2012, shawn.south@bellevuecollege.edu
			searchterm = searchterm.Replace("\"", string.Empty);

			if (quarter == "CE")
			{
				Response.Redirect("http://www.campusce.net/BC/Search/Search.aspx?q=" + searchterm, true);
				return null;
			}

			ViewBag.timestart = timestart;
			ViewBag.timeend = timeend;

			ViewBag.latestart = latestart;
			ViewBag.numcredits = numcredits;

			ViewBag.Modality = Helpers.ConstructModalityList(classformat);
			ViewBag.Days = Helpers.ConstructDaysList(days);

			ViewBag.avail = avail;
			ViewBag.p_offset = p_offset;

			//add the dictionary that converts MWF -> Monday/Wednesday/Friday for section display.
			TempData["DayDictionary"] = Helpers.getDayDictionary();

			ViewBag.Subject = Subject;
			ViewBag.searchterm = Regex.Replace(searchterm, @"\s+", " ");	// replace each clump of whitespace w/ a single space (so the database can better handle it)

			IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, days, classformat, avail, latestart, numcredits);

			// TODO: Add query string info (e.g. facets) to the routeValues dictionary so we can pass it all as one chunk.
			IDictionary<string, object> routeValues = new Dictionary<string, object>(3);
			routeValues.Add("YearQuarterID", quarter);
			ViewBag.RouteValues = routeValues;
			// TODO: Aren't link parameters part of the route values? Let's merge these two somehow.
			// (Is RouteValues even being used?)
			ViewBag.LinkParams = Helpers.getLinkParams(Request, "submit");

			using (OdsRepository repository = new OdsRepository(HttpContext))
			{
				setViewBagVars("", "", "", avail, "", repository);

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

				IList<vw_ClassScheduleData> classScheduleData;
				IList<SearchResult> SearchResults;
				IList<SearchResultNoSection> NoSectionSearchResults;

				using (ClassScheduleDb db = new ClassScheduleDb())
				{
					SearchResults = GetSearchResults(db, searchterm, quarter);
					NoSectionSearchResults = GetNoSectionSearchResults(db, searchterm, quarter);

					using (_profiler.Step("API::Get Class Schedule Specific Data()"))
					{
						classScheduleData = (from c in db.vw_ClassScheduleData
						                     where c.YearQuarterID == YRQ.ID
						                     select c
						                    ).ToList();
					}
				}
				IList<SectionWithSeats> sectionsEnum;
				using (_profiler.Step("Joining SectionWithSeats (in memory)"))
				{
					sectionsEnum = (from c in sections
					                join d in classScheduleData on c.ID.ToString() equals d.ClassID into cd
					                from d in cd.DefaultIfEmpty()
					                join e in SearchResults on c.ID.ToString() equals e.ClassID into ce
					                from e in ce
					                where e.ClassID == c.ID.ToString()
					                orderby c.Yrq.ID descending
					                select new SectionWithSeats
								{
										ParentObject = c,
										SeatsAvailable = d != null ? d.SeatsAvailable : int.MinValue,	// MinValue allows us to identify past quarters (with no availability info)
										LastUpdated = Helpers.getFriendlyTime(d != null ? d.LastUpdated.GetValueOrDefault() : DateTime.MinValue),
										SectionFootnotes = d != null ? d.SectionFootnote : string.Empty,
										CourseFootnotes = d != null ? d.CourseFootnote : string.Empty,
										CourseTitle = d.CustomTitle != null && d.CustomTitle != string.Empty ? d.CustomTitle : c.CourseTitle,
										CustomTitle = d.CustomTitle != null ? d.CustomTitle : string.Empty,
										CustomDescription = d.CustomDescription != null ? d.CustomDescription : string.Empty
								}).OrderBy(x => x.CourseNumber).ThenBy(x => x.CourseTitle).ToList();

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
					allSubjects = sectionsEnum.Select(c => c.CourseSubject).Distinct();
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
			ViewBag.days = days ?? "all";
			ViewBag.avail = avail ?? "all";

			ViewBag.activeClass = " class=active";
		}

		private string[] setDayFacetViewbags(string[] days)
		{
			if (days != null)
			{
				foreach (string day in days)
				{
					if (day == "Su")
						ViewBag.day_su = true;
					else if (day == "M")
						ViewBag.day_m = true;
					else if (day == "T")
						ViewBag.day_t = true;
					else if (day == "W")
						ViewBag.day_w = true;
					else if (day == "Th")
						ViewBag.day_th = true;
					else if (day == "F")
						ViewBag.day_f = true;
					else if (day == "Sa")
						ViewBag.day_s = true;
				}
			}

			return days;
		}

		#endregion
	}
}
