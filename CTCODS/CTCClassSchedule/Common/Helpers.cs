using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Net;
using System.Security.Cryptography;



namespace CTCClassSchedule.Common
{
	public static class Helpers
	{
		public static MvcHtmlString IncludePageURL(this HtmlHelper htmlHelper, string url)
		{
			return MvcHtmlString.Create(new WebClient().DownloadString(url));

		}

		// TODO: Jeremy, another optional/BCC specific way of getting data - find another way?
		public static String getProfileURL(string SID)
		{
			Encryption64 en = new Encryption64();

			string returnString = "";
			returnString = "http://bellevuecollege.edu/directory/PersonDetails.aspx?PersonID=" + en.Encrypt(SID,"!#$a54?5");
			return returnString;

		}






	}
}