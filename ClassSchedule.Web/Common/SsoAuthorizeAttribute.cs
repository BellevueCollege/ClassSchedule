/*
This file is part of CtcClassSchedule.

CtcClassSchedule is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

CtcClassSchedule is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with CtcClassSchedule.  If not, see <http://www.gnu.org/licenses/>.
 */
using System.Web;
using System.Web.Mvc;

namespace CTCClassSchedule.Common
{
  /// <summary>
  ///
  /// </summary>
	public class SsoAuthorizeAttribute : AuthorizeAttribute
  {
    /// <summary>
    /// 
    /// </summary>
    public const string REFERRER_SESSION_LABEL = "ReferralUrlForSso";

    /// <summary>
    ///
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
		  HttpRequestBase request = httpContext.Request;

      // TODO: confirm the SSO provider is not including a Referrer that points to itself
		  if (request.UrlReferrer != null)
		  {
		    string url = request.UrlReferrer.ToString();

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