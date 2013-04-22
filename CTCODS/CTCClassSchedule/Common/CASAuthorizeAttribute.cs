using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CTCClassSchedule.Common
{
  /// <summary>
  ///
  /// </summary>
	public class CASAuthorizeAttribute : AuthorizeAttribute
  {
    public const string REFERRER_SESSION_LABEL = "ReferralUrlForCas";

    /// <summary>
    ///
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
		  HttpRequestBase request = httpContext.Request;
		  string url;

      // Typically, UrlReferrer will be 'null' if we're returning from the CAS login page
		  if (request.UrlReferrer != null)
			{
			  url = request.UrlReferrer.ToString();

        if (httpContext.Session != null)
        {
          httpContext.Session.Add(REFERRER_SESSION_LABEL, url);
        }
        else
        {
          HttpContext.Current.Session.Add(REFERRER_SESSION_LABEL, url);
        }
      }

      return (base.AuthorizeCore(httpContext));
		}
	}
}