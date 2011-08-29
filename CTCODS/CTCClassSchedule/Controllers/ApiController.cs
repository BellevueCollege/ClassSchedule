using System.Collections.Generic;
using System.Web.Mvc;
using Ctc.Ods.Data;
using Ctc.Ods.Types;

namespace CTCClassSchedule.Controllers
{
	public class ApiController : Controller
	{
		//
		// GET: /Api/
		[HttpGet]
		public ActionResult Subjects()
		{
			using (OdsRepository db = new OdsRepository())
			{
				IList<CoursePrefix> data = db.GetCourseSubjects();

				return PartialView(data);
			}
		}
	}
}
