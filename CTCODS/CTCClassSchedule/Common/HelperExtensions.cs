using System;
using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using System.Linq;
using Ctc.Ods.Types;

namespace CTCClassSchedule.Common
{
	/// <summary>
	/// Contains extension methods for the <see cref="HtmlHelper"/> class
	/// </summary>
	public static class Html
	{
		const string BEGIN_INCLUDE_TAG = @"<!--#include";
		const string END_INCLUDE_TAG = @"-->";

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

		#region Html.Include()
		/// <summary>
		/// Provides server-side include functionality
		/// </summary>
		/// <param name="html"></param>
		/// <param name="fileAndPath"></param>
		/// <returns>The contents of the file(s) that are requested</returns>
		/// <remarks>
		/// MVC does not support server-side includes on its own, so this method provides that functionality. It will include
		/// the file specified - including any nested server-side includes that are present in that file (and so on, etc.)
		/// </remarks>
		public static IHtmlString Include(this HtmlHelper html, string fileAndPath)
		{

			string fileContents = ProcessIncludeFile(fileAndPath, html.ViewContext.HttpContext.Server);
			return html.Raw(fileContents);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="fileAndPath"></param>
		/// <param name="server"></param>
		/// <returns></returns>
		private static string ProcessIncludeFile(string fileAndPath, HttpServerUtilityBase server)
		{
			string fileContents = File.ReadAllText(fileAndPath);

			int position = 0;
			while (position >= 0 && position < fileContents.Length)
			{
				position = fileContents.IndexOf(BEGIN_INCLUDE_TAG, position);
				if (position >= 0)
				{
					int endPosition = fileContents.IndexOf(END_INCLUDE_TAG, position + 1);
					string includeTag = fileContents.Substring(position, endPosition - position + END_INCLUDE_TAG.Length);

					string[] pair = includeTag.Remove(includeTag.Length - END_INCLUDE_TAG.Length).Remove(0, BEGIN_INCLUDE_TAG.Length).Trim().Split('=');
					if (pair.Length >= 2)
					{
						string attribute = pair[0].Trim();
						string file = pair[1].Trim(' ', '"');
						string nextFile;

						if (attribute.ToUpper() == "FILE")
						{
							string fileWithPath = Path.Combine(Path.GetDirectoryName(fileAndPath) ?? string.Empty, file);
							nextFile = ProcessIncludeFile(fileWithPath, server);
						}
						else	// VIRTUAL
						{
							nextFile = ProcessIncludeFile(server.MapPath(file), server);
						}

						if (!string.IsNullOrWhiteSpace(nextFile))
						{
							fileContents = fileContents.Replace(includeTag, nextFile);
						}
					}

					position = endPosition;
				}
			}

			return fileContents;
		}

		#endregion

		/// <summary>
		/// Wrapper for <see cref="Helpers.FormatWithSearchTerm"/> which returns a Razor string.
		/// </summary>
		/// <param name="html"></param>
		/// <param name="searchTerm"></param>
		/// <param name="text"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static IHtmlString FormatWithSearchTerm(this HtmlHelper html, string searchTerm, string text, params object[] args)
		{
			return html.Raw(Helpers.FormatWithSearchTerm(searchTerm, text, args));
		}

		/// <summary>
		/// Constructs course headings for blocks of sections on pages that display it.
		/// </summary>
		/// <param name="html"></param>
		/// <param name="sec"></param>
		/// <param name="credits"></param>
		/// <param name="searchTerm"></param>
		/// <returns></returns>
		public static IHtmlString SectionCourseHeading(this HtmlHelper html, SectionWithSeats sec, string credits = null, string searchTerm = null)
		{
			string output = string.Format("{0} {1} {2}", Helpers.SubjectWithCommonCourseFlag(sec), sec.CourseNumber, sec.CourseTitle);

			if (!string.IsNullOrWhiteSpace(credits))
			{
				output = string.Concat(output, " &#8226; ", credits);
			}

			if (string.IsNullOrWhiteSpace(searchTerm))
			{
				return html.Raw(output);
			}
			return html.FormatWithSearchTerm(searchTerm, output);
		}

		/// <summary>
		/// Provides access to the MVC HtmlHelper from Razor helpers.
		/// </summary>
		/// <param name="html"></param>
		/// <returns></returns>
		/// <remarks>
		/// See http://stackoverflow.com/questions/4710853/using-mvc-htmlhelper-extensions-from-razor-declarative-views
		/// </remarks>
		public static HtmlHelper PageHelper(this System.Web.WebPages.Html.HtmlHelper html)
		{
		 return ((WebViewPage) WebPageContext.Current.Page).Html;
		}

	}

	#region Misc extensions
	/// <summary>
	///
	/// </summary>
	public static class Miscellaneous
	{
		/// <summary>
		///
		/// </summary>
		/// <param name="nameValues"></param>
		/// <param name="paramName"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static int SafeNameValueToInt32(this NameValueCollection nameValues, string paramName, int defaultValue)
		{
			if (nameValues.AllKeys.Contains(paramName))
			{
				int outValue;
				if (int.TryParse(nameValues[paramName], out outValue))
				{
					return outValue;
				}
			}
			return defaultValue;
		}
	}

	#endregion
}