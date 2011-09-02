using System.Collections.Generic;
using System.Web.Mvc;
using Ctc.Ods.Data;
using Ctc.Ods.Types;

namespace CTCClassSchedule.Controllers
{
	public class ApiController : Controller
	{
		//
		// GET: /Api/Subjects
		/// <summary>
		/// Returns an array of <see cref="Course"/> Subjects
		/// </summary>
		/// <param name="format"></param>
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
		public ActionResult Subjects(string format)
		{
			using (OdsRepository db = new OdsRepository())
			{
				IList<CoursePrefix> data = db.GetCourseSubjects();

				if (format == "json")
				{
					// NOTE: AllowGet exposes a potential for JSON Hijacking (see http://haacked.com/archive/2009/06/25/json-hijacking.aspx)
					// but is not an issue here because we are receiving and returning public (e.g. non-sensitive) data
					return Json(data, JsonRequestBehavior.AllowGet);
				}

				IDictionary<string, object> linkParams = new Dictionary<string, object>(Request.QueryString.Count);
				foreach (string key in Request.QueryString.AllKeys)
				{
					if (key != "X-Requested-With")
					{
						if (key == "yrq") {
							ViewBag.YearQuarter = Request.QueryString[key];
						} else {
							linkParams.Add(key, Request.QueryString[key]);
						}
					}
				}
				ViewBag.LinkParams = linkParams;
				ViewBag.SubjectsColumns = 2;

				return PartialView(data);
			}
		}
	}
}
