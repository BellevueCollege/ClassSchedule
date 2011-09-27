using System.Web;
using System.Web.Mvc;

namespace CTCClassSchedule.Common
{
	public static class Html
	{
		public static IHtmlString CreditsValue(this HtmlHelper html, decimal? value, string errorHtml)
		{
			string output = value.HasValue ? value.Value.ToString().TrimEnd('0').TrimEnd('.') : errorHtml;
			return html.Raw(output);
		}
	}
}