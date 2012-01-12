using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CTCClassSchedule.Controllers
{
    public partial class ClassesController : Controller
    {
			[Authorize]
      public ActionResult Authenticate()
			{
				return Content("Success!");
			}
    }
}
