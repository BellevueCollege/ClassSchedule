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
		/// <returns>
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
					// NOTE: AllowGet exposes a potential for JSON Hijacking (http://haacked.com/archive/2009/06/25/json-hijacking.aspx)
					// but is not an issue here because we are returning public (e.g. non-sensitive) data
					return Json(data, JsonRequestBehavior.AllowGet);
				}

				ViewBag.SubjectsColumns = 2;
				return PartialView(data);
			}
		}
	}
}
