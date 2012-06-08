using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CTCClassSchedule.Common
{
	public class CASAuthorizeAttribute : AuthorizeAttribute
	{
		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			if (httpContext.Request.UrlReferrer != null)
			{
				string url = httpContext.Request.UrlReferrer.ToString();
				httpContext.Session.Add("ReferralUrlForCas", url);
			}
			return (base.AuthorizeCore(httpContext));
		}
	}
}