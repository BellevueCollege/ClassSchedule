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
		///<param name="yrq"></param>
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
		public ActionResult Subjects(string format, string yrq)
		{
			using (OdsRepository db = new OdsRepository(HttpContext))
			{
				IList<CoursePrefix> data;
				data = string.IsNullOrWhiteSpace(yrq) || yrq.ToUpper() == "ALL" ? db.GetCourseSubjects() : db.GetCourseSubjects(YearQuarter.FromFriendlyName(yrq));

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
						string value = Request.QueryString[key];

						if (key == "yrq" && value.ToUpper() != "ALL")
						{
							ViewBag.YearQuarter =  YearQuarter.FromFriendlyName(value);
						}
						else
						{
							linkParams.Add(key, value);
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
