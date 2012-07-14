using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ctc.Ods.Types;

namespace CTCClassSchedule
{
	/// <summary>
	/// A handler class for the Automated Footnote settings in the web.config
	/// </summary>
	public class AutomatedFootnotesConfig
	{
		/// <summary>
		/// Private dictionary that maps the name of each automated footnote message
		/// to the footnote element itself.
		/// </summary>
		protected static Dictionary<string, AutomatedFootnoteElement> _footnoteInstances;

		/// <summary>
		/// Constructor. Initiates the serialization of the automated footnotes section in the
		/// web.config file.
		/// </summary>
		static AutomatedFootnotesConfig()
		{
			AutomatedFootnotesSection sec = (AutomatedFootnotesSection)System.Configuration.ConfigurationManager.GetSection("ctcAutomatedFootnoteSettings");
			_footnoteInstances = GetFootnoteInstances(sec.FootnoteInstances);
		}

		/// <summary>
		/// Returns a footnote message element based on the name.
		/// </summary>
		/// <param name="footnoteName">The anme of the element you want.</param>
		/// <returns>A single footnote element if it exists.</returns>
		public static AutomatedFootnoteElement Footnotes(string footnoteName)
		{
			return _footnoteInstances[footnoteName];
		}

		/// <summary>
		/// Takes a Section and produces all automated messages that the section should display.
		/// </summary>
		/// <param name="section">The section to base the generated footnote messages on.</param>
		/// <returns>A string of all automated footnote messages concatenated.</returns>
		public static string getAutomatedFootnotesText(Section section)
		{
			string wSpace = section.Footnotes.Count() == 0 ? string.Empty : " ";
			string footenoteText = buildFootnoteText(section.IsDifferentStartDate,
																							 section.IsDifferentEndDate,
																							 section.IsHybrid,
																							 section.IsTelecourse,
																							 section.StartDate.GetValueOrDefault(DateTime.Now),
																							 section.EndDate.GetValueOrDefault(DateTime.Now));
			return wSpace + footenoteText;
		}

		/// Gets automated footnotes based on boolean values passed to the method.
		/// This is useful if you are handling either Section or SectionWithSeats objects.
		/// </summary>
		/// <param name="differentStartFlag">Is the course a late start course.</param>
		/// <param name="differentEndFlag">Does the course have a different end date than normal.</param>
		/// <param name="hybridFlag">Is this a hybrid course?</param>
		/// /// <param name="telecourseFlag">Is this a telecourse?</param>
		/// <param name="startDate">The course's scheduled start date.</param>
		/// <param name="endDate">The courses scheduled end date.</param>
		/// <returns>All relevant automated footnotes in one concatenated string.</returns>
		private static string buildFootnoteText(Boolean differentStartFlag, Boolean differentEndFlag, Boolean hybridFlag, Boolean telecourseFlag, DateTime startDate, DateTime endDate)
		{
			string startDateParam = "{STARTDATE}";
			string endDateParam = "{ENDDATE}";
			string footnoteTextResult = string.Empty;

			// Build the footnotes applicable to the start/end date
			AutomatedFootnoteElement footnote = getApplicableDateFootnote(differentStartFlag, differentEndFlag, startDate, endDate);
			if (footnote != null)
			{
				string startDateText = startDate.ToString(footnote.StringFormat);
				string endDateText = endDate.ToString(footnote.StringFormat);

				footnoteTextResult = footnote.Text.Replace(startDateParam, startDateText);
				footnoteTextResult = footnoteTextResult.Replace(endDateParam, endDateText) + " ";
			}

			// If the section is a hybrid section
			if (hybridFlag)
			{
				footnoteTextResult += Footnotes("hybrid").Text + " ";
			}

			// If the section is a telecourse
			if (telecourseFlag)
			{
				footnoteTextResult += Footnotes("telecourse").Text;
			}

			return footnoteTextResult.Trim();
		}

		/// <summary>
		/// Uses a section's start/end date to determine an applicable date footnote.
		/// By design, at most one date related footnote will be relevant to any
		/// given section. This method determines and returns the applicable footnote,
		/// should one exist.
		/// </summary>
		/// <param name="differentStartFlag">Is the course a late start course?</param>
		/// <param name="differentEndFlag">Does the course have a different end date than normal?</param>
		/// <param name="startDate">The course's scheduled start date.</param>
		/// <param name="endDate">The courses scheduled end date.</param>
		/// <returns>A AutomatedFootnoteElement object, or null if no applicable footnote exists.</returns>
		private static AutomatedFootnoteElement getApplicableDateFootnote(Boolean differentStartFlag, Boolean differentEndFlag, DateTime startDate, DateTime endDate)
		{
			AutomatedFootnoteElement footnote = null;

			// If the section start and end date are identical
			if (startDate.Date.Equals(endDate.Date))
			{
				footnote = Footnotes("identicalStartAndEndDates");
			}
			else
			{
				// If the section has both a different start and end date than the default
				if (differentStartFlag && differentEndFlag)
				{
					footnote = Footnotes("differentStartAndEndDates");
				}
				else if (differentStartFlag) // If the section only has a different start date
				{
					footnote = Footnotes("startDate");
				}
				else if (differentEndFlag) // If the section has a different end date
				{
					footnote = Footnotes("endDate");
				}
			}

			return footnote;
		}

		/// <summary>
		/// Returns a dictionary of all elements within a given collection of footnote messages.
		/// The key is the name of the footnote element, the value is the element itself.
		/// </summary>
		/// <param name="collection">The collection to convert to a dictionary.</param>
		/// <returns>A dictionary of footnote elements mapped by the element name.</returns>
		private static Dictionary<string, AutomatedFootnoteElement> GetFootnoteInstances(AutomatedFootnoteCollection collection)
		{
			Dictionary<string, AutomatedFootnoteElement> instances = new Dictionary<string, AutomatedFootnoteElement>();

			foreach (AutomatedFootnoteElement i in collection)
			{
				instances.Add(i.Name, i);
			}

			return instances;
		}
	}
}
