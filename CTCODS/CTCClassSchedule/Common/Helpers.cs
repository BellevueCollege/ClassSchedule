using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Net;



namespace CTCClassSchedule.Common
{
	public static class Helpers
	{
		public static MvcHtmlString IncludePageURL(this HtmlHelper htmlHelper, string url)
		{
			return MvcHtmlString.Create(new WebClient().DownloadString(url));

		}
	}
}