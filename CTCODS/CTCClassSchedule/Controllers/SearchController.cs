using System;
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
		public ActionResult Index(string searchterm, string Subject, string currentQuarter, string quarter, string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string f_telecourse, string avail, string latestart, string numcredits, int p_offset = 0)
		{
			int itemCount = 0;

			if (quarter == "CE")
			{
				Response.Redirect("http://www.campusce.net/BC/Search/Search.aspx?q=" + searchterm, true);
				return null;
			}

			ViewBag.timestart = timestart;
			ViewBag.timeend = timeend;
			ViewBag.day_su = day_su;
			ViewBag.day_m = day_m;
			ViewBag.day_t = day_t;
			ViewBag.day_w = day_w;
			ViewBag.day_th = day_th;
			ViewBag.day_f = day_f;
			ViewBag.day_s = day_s;
			ViewBag.latestart = latestart;
			ViewBag.numcredits = numcredits;

			IList<ModalityFacetInfo> modality = new List<ModalityFacetInfo>(4);
			modality.Add(Helpers.getModalityInfo("f_oncampus", "On Campus", f_oncampus));
			modality.Add(Helpers.getModalityInfo("f_online", "Online", f_online));
			modality.Add(Helpers.getModalityInfo("f_hybrid", "Hybrid", f_hybrid));
			modality.Add(Helpers.getModalityInfo("f_telecourse", "Telecourse", f_telecourse));
			ViewBag.Modality = modality;

			ViewBag.avail = avail;
			ViewBag.p_offset = p_offset;

			//add the dictionary that converts MWF -> Monday/Wednesday/Friday for section display.
			TempData["DayDictionary"] = Helpers.getDayDictionary();

			ViewBag.Subject = Subject;
			ViewBag.searchterm = Regex.Replace(searchterm, @"\s+", " ");	// replace each clump of whitespace w/ a single space (so the database can better handle it)

			IList<ISectionFacet> facets = Helpers.addFacets(timestart, timeend, day_su, day_m, day_t, day_w, day_th, day_f, day_s,
																											f_oncampus, f_online, f_hybrid, f_telecourse, avail, latestart, numcredits);

			// TODO: Add query string info (e.g. facets) to the routeValues dictionary so we can pass it all as one chunk.
			IDictionary<string, object> routeValues = new Dictionary<string, object>(3);
			routeValues.Add("YearQuarterID", quarter);
			ViewBag.RouteValues = routeValues;

			ViewBag.LinkParams = Helpers.getLinkParams(Request);

			if (searchterm == null)
			{
				return View();
			}

			using (OdsRepository repository = new OdsRepository(HttpContext))
			{
				setViewBagVars("", "", "", avail, "", repository);

				YearQuarter YRQ = string.IsNullOrWhiteSpace(quarter) ? repository.CurrentYearQuarter : YearQuarter.FromFriendlyName(quarter);
				ViewBag.YearQuarter = YRQ;
				ViewBag.QuarterNavMenu = Helpers.getYearQuarterListForMenus(repository);
				bool IsPreviousQuarter = false;

				if (ViewBag.QuarterNavMenu[0].ToString() != YRQ.ToString())
				{
					IsPreviousQuarter = true;
				}

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

		///// <summary>
		/////
		///// </summary>
		///// <param name="courseIdPlusYRQ"></param>
		///// <returns></returns>
		//[HttpPost]
		//public ActionResult getSeats(string courseIdPlusYRQ)
		//{
		//  int? seats = null;
		//  string friendlyTime = "";

		//  string classID = courseIdPlusYRQ.Substring(0, 4);
		//  string yrq = courseIdPlusYRQ.Substring(4, 4);


		//  CourseHPQuery query = new CourseHPQuery();
		//  int HPseats = query.findOpenSeats(classID, yrq);

		//  var seatsAvailableLocal = from s in _classScheduleDb.SeatAvailabilities
		//                            where s.ClassID == courseIdPlusYRQ
		//                            select s;
		//  int rows = seatsAvailableLocal.Count();

		//  if (rows == 0)
		//  {
		//    //insert the value
		//    SeatAvailability newseat = new SeatAvailability();
		//    newseat.ClassID = courseIdPlusYRQ;
		//    newseat.SeatsAvailable = HPseats;
		//    newseat.LastUpdated = DateTime.Now;

		//    _classScheduleDb.SeatAvailabilities.AddObject(newseat);
		//  }
		//  else
		//  {
		//    //update the value
		//    foreach (SeatAvailability seat in seatsAvailableLocal)
		//    {
		//      seat.SeatsAvailable = HPseats;
		//      seat.LastUpdated = DateTime.Now;
		//    }

		//  }

		//  _classScheduleDb.SaveChanges();

		//  var seatsAvailable = from s in _classScheduleDb.vw_SeatAvailability
		//                       where s.ClassID == courseIdPlusYRQ
		//                       select s;

		//  foreach (var seat in seatsAvailable)
		//  {
		//    seats = seat.SeatsAvailable;
		//    friendlyTime = Helpers.getFriendlyTime(seat.LastUpdated.GetValueOrDefault());
		//  }

		//  if (friendlyTime.Equals("not yet"))
		//  {
		//    friendlyTime = "0 seconds ago";
		//  }

		//  var jsonReturnValue = seats.ToString() + "|" + friendlyTime;
		//  return Json(jsonReturnValue);
		//}



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
		#endregion
	}
}
