using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
		public static string getAutomatedFootnotesText(SectionWithSeats section)
		{
			string footnoteTextResult = string.Empty;
			string dateParam = "{DATE}";
			string dateText;
			AutomatedFootnoteElement footnote;
			string wSpace = section.Footnotes.Count() == 0 ? string.Empty : " ";

			// If the section has a late start
			if (section.IsLateStart)
			{
				footnote = Footnotes("lateStart");
				dateText = section.StartDate.GetValueOrDefault(DateTime.Now).ToString(footnote.StringFormat);
				footnoteTextResult += wSpace + Footnotes("lateStart").Text.Replace(dateParam, dateText);
			}

			// If the section has a different end date than usual
			if (section.IsDifferentEndDate)
			{
				footnote = Footnotes("lateStart");
				dateText = section.EndDate.GetValueOrDefault(DateTime.Now).ToString(footnote.StringFormat);
				footnoteTextResult += wSpace + Footnotes("endDate").Text.Replace(dateParam, dateText);
			}

			// If the section is a hybrid section
			if (section.IsHybrid)
			{
				footnoteTextResult += wSpace + Footnotes("hybrid").Text;
			}

			// If the section is a continuous enrollment section
			if (section.IsContinuousEnrollment)
			{
				footnote = Footnotes("continuousEnrollment");
				dateText = section.LastRegistrationDate != DateTime.MinValue ? section.LastRegistrationDate.ToString(footnote.StringFormat) : "*UNK*";
				footnoteTextResult += wSpace + Footnotes("continuousEnrollment").Text.Replace(dateParam, dateText);
			}

			return footnoteTextResult;
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