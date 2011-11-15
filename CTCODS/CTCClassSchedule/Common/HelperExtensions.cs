using System;
using System.IO;
using System.Text.RegularExpressions;
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

		#region FormatWithSearchTerm()
		/// <summary>
		///
		/// </summary>
		/// <param name="html"></param>
		/// <param name="searchTerm"></param>
		/// <param name="buffer"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static IHtmlString FormatWithSearchTerm(this HtmlHelper html, string searchTerm, string buffer, params object[] args)
		{
			if (string.IsNullOrWhiteSpace(buffer) || string.IsNullOrWhiteSpace(searchTerm))
			{
				return html.Raw(buffer);
			}

			if (args != null && args.Length > 0)
			{
				buffer = string.Format(buffer, args);
			}
			string output = Regex.Replace(buffer, searchTerm, @"<em class='keyword'>$&</em>", RegexOptions.IgnoreCase);
			return html.Raw(output);
		}
		#endregion
	}
}