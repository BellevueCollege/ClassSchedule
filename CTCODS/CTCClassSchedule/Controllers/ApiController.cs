using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Ctc.Ods.Data;
using Ctc.Ods.Types;
using CTCClassSchedule.Common;
using CTCClassSchedule.Models;

namespace CTCClassSchedule.Controllers
{
	public class ApiController : Controller
	{
		public ApiController()
		{
			ViewBag.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		}

		//
		// GET: /Api/Subjects
		/// <summary>
		/// Returns an array of <see cref="Course"/> Subjects
		/// </summary>
		/// <param name="format"></param>
		///<param name="YearQuarter"></param>
		///<returns>
		///		Either a <see cref="PartialViewResult"/> which can be embedded in an MVC View,
		///		or the list of <see cref="Course"/> Subjects as a JSON array.
		/// </returns>
		/// <remarks>
		///		To receive the list as a JSON array call this method with <i>format=json</i>:
		///		<example>
		///			http://localhost/Api/Subjects?format=json
		///		</example>
		/// </remarks>
		[HttpGet]
		public ActionResult Subjects(string format, string YearQuarter)
		{
			using (OdsRepository db = new OdsRepository(HttpContext))
			{
				IList<CoursePrefix> data;
				data = string.IsNullOrWhiteSpace(YearQuarter) || YearQuarter.ToUpper() == "ALL" ? db.GetCourseSubjects() : db.GetCourseSubjects(Ctc.Ods.Types.YearQuarter.FromFriendlyName(YearQuarter));

				IList<vw_ProgramInformation> progInfo;
				using (ClassScheduleDb classScheduleDb = new ClassScheduleDb())
				{
					progInfo = (from s in classScheduleDb.vw_ProgramInformation select s).ToList();
				}
				IList<ScheduleCoursePrefix> subjectList = (from p in progInfo
																									where data.Select(c => c.Subject).Contains(p.Abbreviation.TrimEnd('&'))
																									select new ScheduleCoursePrefix
																														{
																															Subject = p.URL,
																															Title = p.Title
																														})
																									.OrderBy(s => s.Title)
																									.Distinct()
																									.ToList();

				if (format == "json")
				{
					// NOTE: AllowGet exposes the potential for JSON Hijacking (see http://haacked.com/archive/2009/06/25/json-hijacking.aspx)
					// but is not an issue here because we are receiving and returning public (e.g. non-sensitive) data
					return Json(subjectList, JsonRequestBehavior.AllowGet);
				}

				ViewBag.LinkParams = Helpers.getLinkParams(Request);
				ViewBag.SubjectsColumns = 2;

				return PartialView(subjectList);
			}
		}
	}
}
