using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
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
		private ClassScheduleDevEntities _scheduledb = new ClassScheduleDevEntities();
		private ClassScheduleDevProgramEntities _programdb = new ClassScheduleDevProgramEntities();
		private ClassScheduleFootnoteEntities _footnotedb = new ClassScheduleFootnoteEntities();
		private ClassScheduleDataEntities _scheduledatadb = new ClassScheduleDataEntities();


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

			IList<SearchResult> SearchResults;
			using (_profiler.Step("Executing search stored procedure"))
			{
				SearchResults = _programdb.ExecuteStoreQuery<SearchResult>("usp_ClassSearch @SearchWord, @YearQuarterID", parms).ToList();
			}
			IList<SearchResultNoSection> NoSectionSearchResults;
			using (_profiler.Step("Executing 'other classes' stored procedure"))
			{
				NoSectionSearchResults = _programdb.ExecuteStoreQuery<SearchResultNoSection>("usp_CourseSearch @SearchWord, @YearQuarterID", parms2).ToList();
			}

			using (OdsRepository respository = new OdsRepository(HttpContext))
			{
				YearQuarter YRQ = string.IsNullOrWhiteSpace(quarter) ? respository.CurrentYearQuarter : YearQuarter.FromFriendlyName(quarter);
				ViewBag.YearQuarter = YRQ;
				ViewBag.QuarterNavMenu = Helpers.getYearQuarterListForMenus(respository);

				IList<Section> sections;
				using (_profiler.Step("API::GetSections()"))
				{
					if (string.IsNullOrWhiteSpace(Subject))
					{
						sections = respository.GetSections(YRQ, facets);
					}
					else
					{
						IList<string> subjects = new List<string> {Subject, string.Concat(Subject, _apiSettings.RegexPatterns.CommonCourseChar)};
						sections = respository.GetSections(subjects, YRQ, facets);
					}
				}

				IList<vw_ClassScheduleData> classScheduleData;
				using (_profiler.Step("API::Get Class Schedule Specific Data()"))
				{
				classScheduleData = (from c in _scheduledatadb.vw_ClassScheduleData
														 where c.YearQuarterID == YRQ.ID
														 select c
															).ToList();
				}

				IList<SectionWithSeats> sectionsEnum;
				using (_profiler.Step("Retrieving joined SectionWithSeats"))
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
					                      CourseFootnotes = d != null ? d.CourseFootnote : string.Empty
					                  }).ToList();

				}
				itemCount = sectionsEnum.Count;
				ViewBag.ItemCount = itemCount;

				//DO A COUNT OF THE SECTION OBJECT HERE
				ViewBag.SubjectCount = 0;
				string courseid = "";

				using (_profiler.Step("Counting subjects (courseIDs)"))
				{
					foreach (SectionWithSeats temp in sectionsEnum)
					{
						if(temp.CourseID != courseid) {
							ViewBag.SubjectCount++;
						}
						courseid = temp.CourseID;
					}
				}

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



		/// <summary>
		///
		/// </summary>
		/// <param name="courseIdPlusYRQ"></param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult getSeats(string courseIdPlusYRQ)
		{
			int? seats = null;
			string friendlyTime = "";

			string classID = courseIdPlusYRQ.Substring(0, 4);
			string yrq = courseIdPlusYRQ.Substring(4, 4);


			CourseHPQuery query = new CourseHPQuery();
			int HPseats = query.findOpenSeats(classID, yrq);

			var seatsAvailableLocal = from s in _scheduledb.SeatAvailabilities
																where s.ClassID == courseIdPlusYRQ
																select s;
			int rows = seatsAvailableLocal.Count();

			if (rows == 0)
			{
				//insert the value
				SeatAvailability newseat = new SeatAvailability();
				newseat.ClassID = courseIdPlusYRQ;
				newseat.SeatsAvailable = HPseats;
				newseat.LastUpdated = DateTime.Now;

				_scheduledb.SeatAvailabilities.AddObject(newseat);
			}
			else
			{
				//update the value
				foreach (SeatAvailability seat in seatsAvailableLocal)
				{
					seat.SeatsAvailable = HPseats;
					seat.LastUpdated = DateTime.Now;
				}

			}

			_scheduledb.SaveChanges();

			var seatsAvailable = from s in _scheduledb.vw_SeatAvailability
													 where s.ClassID == courseIdPlusYRQ
													 select s;

			foreach (var seat in seatsAvailable)
			{
				seats = seat.SeatsAvailable;
				friendlyTime = Helpers.getFriendlyTime(seat.LastUpdated.GetValueOrDefault());
			}


			var jsonReturnValue = seats.ToString() + "|" + friendlyTime;
			return Json(jsonReturnValue);
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
