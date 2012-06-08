using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DotNetCasClient.Security;
using System.Net;
using System.IO;
using System.Web.Security;
using DotNetCasClient;
using CTCClassSchedule.Common;

namespace CTCClassSchedule.Controllers
{
	/* This partial class exists to logically compartmentalize all methods of the
	 * ClassesController which are related to editing data, and thus require
	 * authentication.
	 *
	 * This file and the original ClassesController.cs are compiled into a single
	 * ClassController object when the project is built.
	 *
	 *		- 1/11/2012, shawn.south@bellevuecollege.edu
	 *
	 * Much of the code can be referenced here: https://wiki.jasig.org/pages/viewpage.action?pageId=32210981
	 * -Nathan 2/21/2012
	 *
	 */
	public partial class ClassesController : Controller
    {
			/// <summary>
			/// A stub method for forcing authentication
			/// </summary>
			/// <returns>Nothing. (Not sure what, if anything, the rendering engine actually does with this.)</returns>
			/// <remarks>
			/// Call this action method if you need to authenticate a user in without it being a side effect of
			/// accessing protected data/functionality. For example; in response to the user clicking a "Log in"
			/// button.
			/// </remarks>
			///

			[CASAuthorize]
      public ActionResult Authenticate()
      {
				string url = HttpContext.Session["ReferralUrlForCas"].ToString();
				return Redirect(url);
      }

			public ActionResult Logout()
			{
				string url = Request.UrlReferrer.ToString();
				if (!string.IsNullOrWhiteSpace(CasAuthentication.CasServerUrlPrefix))
				{
					CasAuthentication.SingleSignOut();
				}
				else
				{
					FormsAuthentication.SignOut();
				}

				return Redirect(url);
			}
    }
}
