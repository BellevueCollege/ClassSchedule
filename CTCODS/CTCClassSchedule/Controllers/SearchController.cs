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


namespace CTCClassSchedule.Controllers
{
	public class SearchController : Controller
	{

		private ClassScheduleDevEntities _scheduledb = new ClassScheduleDevEntities();
		private ClassScheduleDevProgramEntities _programdb = new ClassScheduleDevProgramEntities();


		//
		// GET: /Search/
		public ActionResult Index(string searchterm, string Subject, string quarter, string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string f_telecourse, string avail, int p_offset = 0)
		{
			IEnumerable<SectionWithSeats> sectionsEnum;
			IEnumerable<string> titles;
			bool ceRedirect = false;
			int itemCount = 0;

			if (quarter == "CE")
			{
				System.Web.HttpContext.Current.Response.Redirect("http://www.campusce.net/BC/Search/Search.aspx?q=" + searchterm, true);
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

			if (searchterm != null && !ceRedirect)
			{
				SqlParameter[] parms = {
									new SqlParameter("SearchWord", searchterm),
									new SqlParameter("YearQuarterID", YearQuarter.ToYearQuarterID(quarter))
					                       };
				SqlParameter[] parms2 = {
										new SqlParameter("SearchWord", searchterm),
										new SqlParameter("YearQuarterID", YearQuarter.ToYearQuarterID(quarter))
					                        };

				IList<SearchResult> SearchResults = _programdb.ExecuteStoreQuery<SearchResult>("usp_ClassSearch @SearchWord, @YearQuarterID", parms).ToList();
				IList<SearchResultNoSection> NoSectionSearchResults = _programdb.ExecuteStoreQuery<SearchResultNoSection>("usp_CourseSearch @SearchWord, @YearQuarterID", parms2).ToList();

				using (OdsRepository respository = new OdsRepository(HttpContext))
				{
					YearQuarter YRQ = string.IsNullOrWhiteSpace(quarter) ? null : YearQuarter.FromFriendlyName(quarter);

					ViewBag.QuarterNavMenu = Helpers.getYearQuarterListForMenus(respository);

					var seatsAvailableLocal = (from s in _scheduledb.vw_SeatAvailability
																		 where s.ClassID.Substring(4) == YRQ.ID
																		 select s);

					IList<Section> sections = null;


					//since there are multiple search scenarios (e.g. searching All, or Fall2011 Art) we need to have a few if statements.
					if (YRQ == null && Subject == null)
					{
						sections = respository.GetSections(facetOptions: facets);
						sectionsEnum = (
															from c in sections
															join d in seatsAvailableLocal on c.ID.ToString() equals d.ClassID
															join e in SearchResults on c.ID.ToString() equals e.ClassID
															orderby e.SearchRank descending
															select new SectionWithSeats
																			{
																				ParentObject = c,
																				SeatsAvailable = d.SeatsAvailable,
																				LastUpdated = Helpers.getFriendlyTime(d.LastUpdated.GetValueOrDefault()),
																			}
													 );
					}
					else
						if (YRQ != null && Subject == null)
						{
							sections = respository.GetSections(YRQ, facetOptions: facets);
							sectionsEnum = (
																from c in sections
																join d in seatsAvailableLocal on c.ID.ToString() equals d.ClassID
																join e in SearchResults on c.ID.ToString() equals e.ClassID
																orderby e.SearchRank descending
																where c.Yrq.ToString() == YRQ.ToString()
																select new SectionWithSeats
																				{
																					ParentObject = c,
																					SeatsAvailable = d.SeatsAvailable,
																					LastUpdated = Helpers.getFriendlyTime(d.LastUpdated.GetValueOrDefault()),
																				}
														 );
						}
						else
							if (YRQ == null && Subject != null)
							{
								sections = respository.GetSections(Subject, facetOptions: facets);
								sectionsEnum = (
																	from c in sections
																	join d in seatsAvailableLocal on c.ID.ToString() equals d.ClassID
																	join e in SearchResults on c.ID.ToString() equals e.ClassID
																	orderby e.SearchRank descending
																	where c.CourseSubject == Subject.ToUpper()
																	select new SectionWithSeats
																					{
																						ParentObject = c,
																						SeatsAvailable = d.SeatsAvailable,
																						LastUpdated = Helpers.getFriendlyTime(d.LastUpdated.GetValueOrDefault()),
																					}
															 );
							}
							else
							{
								sections = respository.GetSections(Subject, YRQ, facetOptions: facets);
								sectionsEnum = (
																	from c in sections
																	join d in seatsAvailableLocal on c.ID.ToString() equals d.ClassID
																	join e in SearchResults on c.ID.ToString() equals e.ClassID
																	orderby e.SearchRank descending
																	where c.CourseSubject == Subject.ToUpper()
																				&& c.Yrq.ToString() == YRQ.ToString()
																	select new SectionWithSeats
																					{
																						ParentObject = c,
																						SeatsAvailable = d.SeatsAvailable,
																						LastUpdated = Helpers.getFriendlyTime(d.LastUpdated.GetValueOrDefault()),
																					}
															 );

							}

					itemCount = sectionsEnum.Count();
					ViewBag.ItemCount = itemCount;
					titles = (from s in sectionsEnum
										orderby s.CourseSubject ascending
										select s.CourseSubject
									 ).Distinct();

					ViewBag.SubjectCount = titles.Count();
					sectionsEnum = (
														from c in sectionsEnum
														join d in SearchResults on c.ID.ToString() equals d.ClassID
														orderby d.SearchRank descending
														select c).Skip(p_offset * 40).Take(40);

					ViewBag.TotalPages = Math.Ceiling((double)itemCount / 40.0);
					ViewBag.CurrentPage = p_offset + 1;

					var model = new SearchResultsModel
												{
													Section = sectionsEnum,
													SearchResultNoSection = NoSectionSearchResults,
													Titles = titles
												};

					return View(model);
				}
			}
			else
			{
				return View();
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
