using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using Ctc.Ods;
using Ctc.Ods.Data;
using Ctc.Ods.Types;
using CTCClassSchedule.Common;
using CTCClassSchedule.Models;
using Ctc.Ods.Config;
using System.Configuration;


namespace CTCClassSchedule.Controllers
{
	public class SearchController : Controller
	{
		private ApiSettings _apiSettings = ConfigurationManager.GetSection(ApiSettings.SectionName) as ApiSettings;
		private ClassScheduleDevEntities _scheduledb = new ClassScheduleDevEntities();
		private ClassScheduleDevProgramEntities _programdb = new ClassScheduleDevProgramEntities();
		private ClassScheduleFootnoteEntities _footnotedb = new ClassScheduleFootnoteEntities();


		//
		// GET: /Search/
		public ActionResult Index(string searchterm, string Subject, string quarter, string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string f_telecourse, string avail, int p_offset = 0)
		{
			int itemCount = 0;

			if (quarter == "CE")
			{
				Response.Redirect("http://www.campusce.net/BC/Search/Search.aspx?q=" + searchterm, true);
				return null;
			}

			setViewBagVars("", "", "", avail, "");

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
			modality.Add(Helpers.getModalityInfo("f_oncampus", "On Campus", f_oncampus));
			modality.Add(Helpers.getModalityInfo("f_online", "Online", f_online));
			modality.Add(Helpers.getModalityInfo("f_hybrid", "Hybrid", f_hybrid));
			modality.Add(Helpers.getModalityInfo("f_telecourse", "Telecourse", f_telecourse));
			ViewBag.Modality = modality;

			ViewBag.avail = avail;
			ViewBag.p_offset = p_offset;


			ViewBag.Subject = Subject;
			ViewBag.searchterm = searchterm;

			IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s,
																											f_oncampus, f_online, f_hybrid, f_telecourse, avail);

			// TODO: Add query string info (e.g. facets) to the routeValues dictionary so we can pass it all as one chunk.
			IDictionary<string, object> routeValues = new Dictionary<string, object>(3);
			routeValues.Add("YearQuarterID", quarter);
			ViewBag.RouteValues = routeValues;

			ViewBag.LinkParams = Helpers.getLinkParams(Request);

			if (searchterm == null)
			{
				return View();
			}

			// We have a valid searchterm - continue processing the search
			SqlParameter[] parms = {
							new SqlParameter("SearchWord", searchterm),
							new SqlParameter("YearQuarterID", YearQuarter.ToYearQuarterID(quarter))
			                       };
			SqlParameter[] parms2 = {
								new SqlParameter("SearchWord", searchterm),
								new SqlParameter("YearQuarterID", YearQuarter.ToYearQuarterID(quarter))
			                        };

			IList<SearchResult> SearchResults =
					_programdb.ExecuteStoreQuery<SearchResult>("usp_ClassSearch @SearchWord, @YearQuarterID", parms).ToList();
			IList<SearchResultNoSection> NoSectionSearchResults =
					_programdb.ExecuteStoreQuery<SearchResultNoSection>("usp_CourseSearch @SearchWord, @YearQuarterID", parms2).ToList
							();

			using (OdsRepository respository = new OdsRepository(HttpContext))
			{
				YearQuarter YRQ = string.IsNullOrWhiteSpace(quarter) ? respository.CurrentYearQuarter : YearQuarter.FromFriendlyName(quarter);
				ViewBag.YearQuarter = YRQ;
				ViewBag.QuarterNavMenu = Helpers.getYearQuarterListForMenus(respository);



				IList<ISectionID> searchSections = new List<ISectionID>();
				List<String> searchResultsClassIDs = new List<String>();
				//iterate through the search results, building a list of course ids
				foreach(SearchResult result in SearchResults) {
					searchSections.Add(SectionID.FromString(result.ClassID));
					searchResultsClassIDs.Add(result.ClassID);
				}


				IQueryable<vw_SeatAvailability> seatsAvailableLocal = from s in _scheduledb.vw_SeatAvailability
																															where searchResultsClassIDs.Contains(s.ClassID)
																															select s;


				IList<Section> sections;
				sections = respository.GetSections(searchSections, facets);

				//IEnumerable<SectionWithSeats>  sectionsEnumGeneric = Helpers.getSectionsWithSeats(YRQ.ID, sections);

				//IEnumerable<SectionWithSeats> sectionsEnum = (from c in sectionsEnumGeneric
				//                                              join e in SearchResults on c.ID.ToString() equals e.ClassID
				//                                              orderby e.SearchRank descending
				//                                              select new SectionWithSeats
				//                                              {
				//                                                ParentObject = c,
				//                                                SeatsAvailable = c.SeatsAvailable,
				//                                                LastUpdated = c.LastUpdated,
				//                                                CourseFootnotes = c.CourseFootnotes,
				//                                                SectionFootnotes = c.SectionFootnotes,
				//                                              }).ToList();

				IEnumerable<SectionWithSeats> sectionsEnum =
																			from c in sections
																			join d in seatsAvailableLocal on c.ID.ToString() equals d.ClassID into cd
																			join e in SearchResults on c.ID.ToString() equals e.ClassID
																			from d in cd.DefaultIfEmpty()	// include all sections, even if don't have an associated seatsAvailable

																			orderby c.Yrq.ID descending
																			select new SectionWithSeats
																			{
																				ParentObject = c,
																				SeatsAvailable = d == null ? int.MinValue : d.SeatsAvailable,	// allows us to identify past quarters (with no availability info)
																				LastUpdated = d == null ? string.Empty : Helpers.getFriendlyTime(d.LastUpdated.GetValueOrDefault()),
																				// retrieve all the Section and Course footnotes and flatten them into one string
																				SectionFootnotes = string.Join(" ", _footnotedb.SectionFootnote.Where(f => f.ClassID == string.Concat(c.ID.ItemNumber, c.ID.YearQuarter))
																																																			 .Select(f => f.Footnote)
																																																			 .Distinct()),
																				CourseFootnotes = string.Join(" ", _footnotedb.CourseFootnote.Where(f => f.CourseID.Substring(0, 5).Trim() == (c.IsCommonCourse
																																																									? string.Concat(c.CourseSubject, _apiSettings.RegexPatterns.CommonCourseChar)
																																																									: c.CourseSubject) && f.CourseID.Substring(5).Trim() == c.CourseNumber)
																																																			.Distinct()
																																																			.Select(f => f.Footnote))
																			};




















				itemCount = sectionsEnum.Count();
				ViewBag.ItemCount = itemCount;

				IList<ProgramInformation> progInfo = _programdb.ProgramInformation.ToList();

				// NOTE: the following LINQ statement could modify sectionsEnum, so we need to make a copy to work with
				IEnumerable<string> sectionsCopy = sectionsEnum.Select(s => s.CourseSubject).Distinct();

				IList<ScheduleCoursePrefix> titles = (from p in progInfo
																							where sectionsCopy.Contains(p.Abbreviation.TrimEnd('&'))
																							select new ScheduleCoursePrefix
																							{
																								Subject = p.URL,
																								Title = p.Title
                                              }
																							).Distinct().ToList();

				ViewBag.SubjectCount = titles.Count;
				sectionsEnum = sectionsEnum.Skip(p_offset * 40).Take(40);

				ViewBag.TotalPages = Math.Ceiling(itemCount / 40.0);
				ViewBag.CurrentPage = p_offset + 1;

				SearchResultsModel model = new SearchResultsModel
														{
															Section = sectionsEnum,
															SearchResultNoSection = NoSectionSearchResults,
															Titles = titles
														};

				return View(model);
			}
		}

		#region helper methods
		/// <summary>
		/// Sets all of the common ViewBag variables
		/// </summary>
		private void setViewBagVars(string flex, string time, string days, string avail, string letter)
		{
			ViewBag.ErrorMsg = "";

			ViewBag.letter = letter;
			ViewBag.flex = flex ?? "all";
			ViewBag.time = time ?? "all";
			ViewBag.days = days ?? "all";
			ViewBag.avail = avail ?? "all";

			ViewBag.activeClass = " class=active";
		}
		#endregion
	}
}
