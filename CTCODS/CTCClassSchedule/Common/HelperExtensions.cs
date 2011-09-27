using System.Web;
using System.Web.Mvc;
using Ctc.Ods.Types;

namespace CTCClassSchedule.Common
{
	/// <summary>
	/// Contains extension methods for the <see cref="HtmlHelper"/> class
	/// </summary>
	public static class Html
	{
		#region CreditsValue()
		/// <summary>
		/// Formats a <see cref="Course"/> credits for display to the user
		/// </summary>
		/// <param name="html"></param>
		/// <param name="credits">The credits value to format</param>
		/// <param name="nullMessage">Optional HTML to display if <paramref name="credits"/> is <i>null</i></param>
		/// <returns>An integer value if only zeroes follow the decimal point. Otherwise, the original decimal value.</returns>
		/// <remarks>
		/// This method removes decimal information if the value is a whole number.
		/// </remarks>
		public static IHtmlString CreditsValue(this HtmlHelper html, decimal? credits, string nullMessage = null)
		{
			string output = credits.HasValue ? TrimCreditsValue(credits.Value) : nullMessage ?? string.Empty;
			return html.Raw(output);
		}

		/// <summary>
		/// Formats a <see cref="Course"/> credits for display to the user
		/// </summary>
		/// <param name="html"></param>
		/// <param name="credits">The credits value to format</param>
		/// <returns></returns>
		/// <remarks>
		/// This method removes decimal information if the value is a whole number.
		/// </remarks>
		public static IHtmlString CreditsValue(this HtmlHelper html, decimal credits)
		{
			string output = TrimCreditsValue(credits);
			return html.Raw(output);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="credits"></param>
		/// <returns></returns>
		private static string TrimCreditsValue(decimal credits)
		{
			return credits.ToString().TrimEnd('0').TrimEnd('.');
		}

		#endregion
	}
}