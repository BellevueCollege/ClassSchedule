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
using System;
using System.Collections.Generic;
using System.Linq;
using CTCClassSchedule.Models;
using System.Web.Mvc;
using System.Text;
using Ctc.Ods.Types;
using Ctc.Ods.Data;

namespace CTCClassSchedule.Common
{
  // A short alias for the data structure used to group Class Schedule data logically
  using GroupedClassScheduleData = Dictionary<Division, Dictionary<Subject, IList<SectionsBlock>>>;
  using Ctc.Ods.Config;
  using System.Configuration;

  /// <summary>
  /// Static class to manage exporting all Class Schedule data to an Adobe InDesign format
  /// which is used for the print version of the Class Schedule.
  /// </summary>
  internal static class ClassScheduleExporter
  {
    /// <summary>
    /// Manages the collection of all Class Schedule data, and outputs it as a file.
    /// </summary>
    /// <remarks>
    /// This is the only public method in the class. It serves as a bootstrap for the export process.
    /// </remarks>
    /// <param name="yearQuarter">The quarter which is being exported.</param>
    /// <returns>A FileResult with a text/plain UTF8 encoded file as an HTML response.</returns>
    public static FileResult GetFile(YearQuarter yearQuarter)
    {
      StringBuilder fileText = new StringBuilder();

      // Convert each object to a node
      Queue<IExportableNode> nodes = GetClassScheduleDataNodes(yearQuarter);

      // Build the contents of the file
      foreach (IExportableNode node in nodes)
      {
        fileText.Append(node);
      }


      // Convert the contents to an actual file
      FileResult file = new FileContentResult(Encoding.UTF8.GetBytes(fileText.ToString()), "text/plain");
      file.FileDownloadName = String.Concat("ClassSchedule-", yearQuarter.ID, "-", DateTime.Now.ToShortDateString(), ".rtf");
      return file;
    }



    /// <summary>
    /// Gets a grouping of all Class Schedule data to export, and converts them to a queue of <see cref="IExportableNode"/>s
    /// which each contain the logic necessary for converting the contained data (ex: <see cref="Division"/>, <see cref="Section"/>, etc)
    /// to a string format.
    /// </summary>
    /// <param name="yearQuarter">The quarter which is being exported.</param>
    /// <returns>A queue of <see cref="IExportableNode"/>s representing Class Schedule data, in the proper sorted order.</returns>
    private static Queue<IExportableNode> GetClassScheduleDataNodes(YearQuarter yearQuarter)
    {
      Queue<IExportableNode> nodes = new Queue<IExportableNode>();
      GroupedClassScheduleData divisionSections = GroupClassScheduleData(yearQuarter);

      // Add a default Legend node which outputs all header info
      nodes.Enqueue(new ExportableLegendNode(yearQuarter));

      // Convert all Division, Subjects, and Sections into exportable nodes
      // Sort each group of data as we convert it to nodes
      foreach (Division div in divisionSections.Keys.OrderBy(d => d.Title)) // Divisions
      {
        foreach (Subject sub in divisionSections[div].Keys.OrderBy(d => d.Title)) // Subjects
        {
          nodes.Enqueue(new ExportableSubjectNode(sub));

          foreach (SectionsBlock block in divisionSections[div][sub]) // Sections
          {
            nodes.Enqueue(new ExportableSectionsBlockNode(block));
          }
        }
      }

      return nodes;
    }

    /// <summary>
    /// Pulls all the Class Schedule data for the given <paramref name="yearQuarter"/>, and groups it into
    /// a logically ordered complex data structure, which allows us to easilyu traverse all the data in
    /// logical order.
    /// </summary>
    /// <param name="yearQuarter">The quarter which is being grouped.</param>
    /// <returns></returns>
    private static GroupedClassScheduleData GroupClassScheduleData(YearQuarter yearQuarter)
    {
      GroupedClassScheduleData divisionSectionsBlock = new GroupedClassScheduleData();

      // Get the common course char
      ApiSettings _apiSettings = ConfigurationManager.GetSection(ApiSettings.SectionName) as ApiSettings;
      string commonCourseChar = _apiSettings.RegexPatterns.CommonCourseChar;

      // Convert Class Schedule data into a List of exportable nodes, which contain
      // rules for exporting the data
      using (ClassScheduleDb db = new ClassScheduleDb())
      {
        // Get all course prefixes grouped by subject
        IEnumerable<Subject> subjects;
        Dictionary<Subject, List<string>> subjectPrefixes;
        subjectPrefixes = (from s in db.Subjects
                           where s.CoursePrefixes.Count > 0
                             && s.Department != null
                             && s.Department.Division != null
                           select s
                          ).ToDictionary(s => s, s => s.CoursePrefixes.Select(p => p.CoursePrefixID).ToList());

        subjects = subjectPrefixes.Keys;


        // Get all Sections grouped by Subject, and then again by Division -- using mainly magic
        IList<SectionWithSeats> sectionsWithSeats;
        using (OdsRepository repository = new OdsRepository())
			  {
          List<Section> sections = new List<Section>();

          sections.AddRange(repository.GetSections(yearQuarter));
          sectionsWithSeats = Helpers.GetSectionsWithSeats(yearQuarter.ID, sections, db);
        }

        // A final LINQ query which converts the returned SectionsWithSeats to the final complex data structure
        divisionSectionsBlock = (from d in subjects.Where(r => sectionsWithSeats.Any(q => r.CoursePrefixes.Any(p => p.CoursePrefixID == (q.IsCommonCourse ? String.Concat(q.CourseSubject, commonCourseChar) : q.CourseSubject))))
                                                   .Select(s => s.Department.Division)
                                                   .Distinct()
                                 select d).ToDictionary(
                                   d => d,
                                   s => s.Departments.SelectMany(de => de.Subjects)
                                                     .Where(r => sectionsWithSeats.Any(q => r.CoursePrefixes.Any(p => p.CoursePrefixID == (q.IsCommonCourse ? String.Concat(q.CourseSubject, commonCourseChar) : q.CourseSubject))))
                                                     .ToDictionary(k => k,
                                                                   v => Helpers.GroupSectionsIntoBlocks(sectionsWithSeats.Where(q => v.CoursePrefixes.Any(p => (q.IsCommonCourse ? String.Concat(q.CourseSubject, commonCourseChar) : q.CourseSubject) == p.CoursePrefixID)).ToList(), db))
                                 );

      }

      return divisionSectionsBlock;
    }



    // TODO: Refactor the inheritance so that ExportableLegendNode does not need to reimplement methods that belong to ExportableNode<T>

    /// <summary>
    /// This interface is empty, and only serves the purpose of allowing all
    /// nodes of different types to be added to the same collect, so long as they
    /// implement this interface.
    ///
    /// It is not necessary to expose any functionality, since the ToString()
    /// method is used to output the node's contents.
    /// </summary>
    private interface IExportableNode
    {
    }

    /// <summary>
    /// A generic node type which implements shared functionality which all nodes require.
    /// This generic allows any data type (ex: <see cref="Subject"/>, <see cref="Section"/>, etc)
    /// to be stored in a node, and hooks up the ToString() method to output a string representation of
    /// the data. The function which actually creates the string representation must be overriden
    /// and implemented in all child classes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    private abstract class ExportableNode<T> : IExportableNode
    {
      /// <summary>
      /// Used to build the string representation of the Node.
      /// </summary>
      protected StringBuilder builder = new StringBuilder();

      /// <summary>
      /// A generic Node, holding whatever type of data needs to be exported.
      /// </summary>
      protected T Node { get; set; }

      /// <summary>
      /// Constructor which enforces an object is passed, and stored in the Node property.
      /// </summary>
      /// <param name="node">The value to store as a node.</param>
      public ExportableNode(T node)
      {
        Node = node;
      }

      /// <summary>
      /// An abstract method which should be overriden in child classes and used
      /// to implement the logic of converting the Node to it's string representation.
      /// </summary>
      /// <returns>A string representation of the Node.</returns>
      protected abstract string ExportNodeAsString();

      /// <summary>
      /// Basic ToString override which returns the string representation of the Node.
      /// The method is sealed so that inheriting classes cannot override this behavior.
      /// </summary>
      /// <returns></returns>
      public sealed override string ToString()
      {
        return ExportNodeAsString();
      }
    }

    /// <summary>
    /// An exportable node class which holds logic for outputting a text representation
    /// of the definition legend.
    /// </summary>
    private class ExportableLegendNode : IExportableNode
    {
      private YearQuarter _yearQuarter;
      private StringBuilder _builder;

      /// <summary>
      /// Constructor for the Legend node.
      /// </summary>
      /// <param name="yearQuarter">The quarter that this Legend represents.</param>
      public ExportableLegendNode(YearQuarter yearQuarter)
      {
        _yearQuarter = yearQuarter;
        _builder = new StringBuilder();
      }

      /// <summary>
      /// Contains the logic used to output the definition Legend
      /// </summary>
      /// <returns>A string representation of the Legend.</returns>
      protected string ExportNodeAsString()
      {
        _builder.AppendLine("* Class Schedule Course Export -- " + _yearQuarter.FriendlyName);
        _builder.AppendLine("*");
        _builder.AppendLine("* Legend:");
        _builder.AppendLine("* <CLS9>   -- Division name");
        _builder.AppendLine("* <CLS1>   -- Program name (subject)");
        _builder.AppendLine("* <CLSP>   -- Program intro");
        _builder.AppendLine("* <CLS2>   -- Course header, or linked section headers");
        _builder.AppendLine("* <CLS3>   -- HP footnotes, CMS course footnotes, and common Section footnotes");
        _builder.AppendLine("* <CLS5>   -- Default section tag");
        _builder.AppendLine("* <CLS6>   -- Evening section");
        _builder.AppendLine("* <CLS7>   -- Weekend section");
        _builder.AppendLine("* <CLSD>   -- Distance Education section");
        _builder.AppendLine("* <CLSA>   -- Section to be arranged");
        _builder.AppendLine("* <CLSY>   -- Automated footnotes");
        _builder.AppendLine("* <CLSN>   -- Section footnotes");
        _builder.AppendLine("* [h]      -- Hybrid section flag");
        _builder.AppendLine("* [online] -- Online section flag");
        _builder.AppendLine("* [-]      -- separates a course title and the number of credits");
        _builder.AppendLine("* D110     -- Default room if no room was assigned to the section");
        _builder.AppendLine("* staff    -- Default instructor name if no instructor is assigned to the section");

        return _builder.ToString();
      }

      /// <summary>
      /// Basic ToString override which returns the string representation of the Node.
      /// The method is sealed so that inheriting classes cannot override this behavior.
      /// </summary>
      /// <returns></returns>
      public sealed override string ToString()
      {
        return ExportNodeAsString();
      }
    }

    /// <summary>
    /// An exportable node class which holds a <see cref="Subject"/> and contains
    /// all the necessary logic and rules for outputting a text representation of it
    /// for file exportation.
    /// </summary>
    private class ExportableSubjectNode : ExportableNode<Subject>
    {
      /// <summary>
      /// Default constructor which passes the <see cref="Subject"/> data
      /// to the base constructor to be stored in the base Node.
      /// </summary>
      /// <param name="node">The <see cref="Subject"/> to store in the base Node.</param>
      public ExportableSubjectNode(Subject node)
        : base(node)
      {
      }

      /// <summary>
      /// Contains the logic used to convert the <see cref="Subject"/> Node into
      /// a string representation for export.
      /// </summary>
      /// <returns>A string representation of the <see cref="Subject"/> Node.</returns>
      protected override string ExportNodeAsString()
      {
        int subjectSeparatedLineBreaks = 4;
        for (int i = 0; i < subjectSeparatedLineBreaks; i++)
        {
          builder.AppendLine();
        }

        builder.AppendFormat("<CLS1>{0}\n", Node.Title);
        builder.AppendFormat("<CLS9>{0}\n", Node.Department.Division.Title);

        if (!String.IsNullOrWhiteSpace(Node.Intro))
        {
          builder.AppendFormat("<CLSP>{0}\n", Node.Intro);
        }

        return builder.ToString();
      }
    }

    /// <summary>
    /// An exportable node class which holds a <see cref="SectionsBlock"/> and contains
    /// all the necessary logic and rules for outputting a text representation of it
    /// for file exportation.
    ///
    /// The logic contatined is complex, and outputs all sections, linked sections,
    /// aggregates common footnotes, and also aggregates automated hybrid footnotes where possible.
    /// </summary>
    private class ExportableSectionsBlockNode : ExportableNode<SectionsBlock>
    {

      /// <summary>
      /// Default constructor which passes the <see cref="SectionsBlock"/> data
      /// to the base constructor to be stored in the base Node.
      /// </summary>
      /// <param name="node">The <see cref="SectionsBlock"/> to store in the base Node.</param>
      public ExportableSectionsBlockNode(SectionsBlock node)
        : base(node)
      {
      }

      /// <summary>
      /// Contains the logic used to convert the <see cref="SectionsBlock"/> Node into
      /// a string representation for export.
      /// </summary>
      /// <returns>A string representation of the <see cref="SectionsBlock"/> Node.</returns>
      protected override string ExportNodeAsString()
      {
        SectionWithSeats primarySection = Node.Sections.First();

        // Section title(s)
        builder.AppendLine();
        builder.AppendLine(GetSectionExportHeader(primarySection));
        if (Node.LinkedSections.Count > 0)
        {
          IList<SectionWithSeats> commonLinkedSections = Helpers.ParseCommonHeadingLinkedSections(Node.LinkedSections);
          foreach (SectionWithSeats linkedSec in commonLinkedSections)
          {
            // Display all the linked section headers
            builder.AppendLine(GetSectionExportHeader(linkedSec));
          }
        }



        // Add Course and HP footnotes
        string footnotes = String.Concat(string.Join(" ", Node.CommonFootnotes), " ", primarySection.CourseFootnotes).Trim();
        if (!String.IsNullOrWhiteSpace(footnotes))
        {
          builder.AppendLine(String.Concat("<CLS3>", footnotes));
        }



        // Output all meeting times for grouped sections
        foreach (SectionWithSeats sec in Node.Sections)
        {
          foreach (OfferedItem item in sec.Offered.OrderBy(o => o.SequenceOrder))
          {
            builder.AppendLine(BuildOfferedItemsExportText(sec, item));
          }

          // Section and course footnotes
          footnotes = GetSectionFootnotes(primarySection.Footnotes, primarySection.SectionFootnotes, Node.CommonFootnotes);
          if (!String.IsNullOrWhiteSpace(footnotes))
          {
            builder.AppendLine(String.Concat("<CLSN>", footnotes));
          }


          // Add Automated footnotes
          // Only display the automated hybrid footnote on the last hybrid section to avoid duplicate footnotes
          string hyrbidFootnote = AutomatedFootnotesConfig.Footnotes("hybrid").Text;
          footnotes = AutomatedFootnotesConfig.getAutomatedFootnotesText(sec);
          if (footnotes.Contains(hyrbidFootnote)
              && sec != Node.Sections.Where(s => s.IsHybrid && sec.CourseNumber == s.CourseNumber && sec.Credits == s.Credits && sec.CourseTitle == s.CourseTitle).LastOrDefault())
          {
            footnotes = footnotes.Replace(hyrbidFootnote, string.Empty);
          }
          if (!String.IsNullOrWhiteSpace(footnotes))
          {
            builder.AppendLine(String.Concat("<CLSY>", footnotes.Trim()));
          }
        }

        return builder.ToString();
      }



      #region Private Helpers
      /// <summary>
      /// Takes a <see cref="SectionWithSeats"/> and constructs the header string representation.
      /// </summary>
      /// <param name="section">The <see cref="SectionWithSeats"/> used to construct the header.</param>
      /// <returns>A string representation of the sections header.</returns>
      private static string GetSectionExportHeader(SectionWithSeats section)
      {
        string creditsText = section.Credits.ToString();
        if (section.Credits == Math.Floor(section.Credits))
        {
          creditsText = creditsText.Remove(creditsText.IndexOf('.'));
        }
        creditsText = String.Concat(section.IsVariableCredits ? "V 1-" : string.Empty, creditsText, " CR");

        return String.Concat("<CLS2>", section.CourseSubject, section.IsCommonCourse ? "&" : string.Empty, " ", section.CourseNumber, "\t", section.CourseTitle, " [-] ", creditsText);
      }

      /// <summary>
      /// Takes section, offered item, and StringBuilder and appends data related to the offered item
      /// in an Adobe InDesign format so that the data can be added to a file. The file is useful
      /// when printing the paper version of the class schedule.
      /// </summary>
      /// <param name="text">The StringBuilder that the data should be added to.</param>
      /// <param name="section">The SectionWithSeats object that the OfferedItem belongs to.</param>
      /// <param name="item">The OfferedItem object whose data should be recorded.</param>
      ///
      private static string BuildOfferedItemsExportText(SectionWithSeats section, OfferedItem item)
      {
        // Configurables
        string onlineDaysStr = "[online]";
        string onlineRoomStr = "D110";
        string arrangedStr = "Arranged";
        string hybridCodeStr = "[h]";
        string instructorDefaultNameStr = "staff";

        // Build a string that represents the DAY and TIME of the course
        string dayTimeStr = String.Concat("\t", arrangedStr.ToLower()); // Default if no date or time is available is "arranged"
        if (item.StartTime != null && item.EndTime != null) // If there is a time available
        {
          string startTimeStr = item.StartTime.Value.ToString("h:mmt").ToLower();
          string endTimeStr = item.EndTime.Value.ToString("h:mmt").ToLower();
          dayTimeStr = String.Concat(item.Days.Equals(arrangedStr) ? arrangedStr.ToLower() : item.Days, "\t", startTimeStr, "-", endTimeStr);
        }
        else if (section.IsOnline) // Online class
        {
          dayTimeStr = String.Concat("\t", onlineDaysStr);
        }

        // Get the tag code that describes the offered item. The tag is determined by the primary offered item
        string tagStr = GetSectionExportTag(section, item);


        // Set or override variable values for tags that have special conditions
        string roomStr = String.Concat("\t", item.Room ?? onlineRoomStr);
        switch (tagStr)
        {
          case "<CLSA>":
            roomStr = String.IsNullOrEmpty(item.Room) ? string.Empty : String.Concat("\t", item.Room);
            break;
          case "<CLSD>":
            roomStr = String.Concat("\t", item.Room);
            break;
        }


        // Construct the finalized tag itself, based on whether or not the current item is the primary
        string line;
        if (item.IsPrimary)
        {
          string instructorName = Helpers.GetShortNameFormat(item.InstructorName) ?? instructorDefaultNameStr;
          string sectionCodeStr = String.Concat((section.IsHybrid ? String.Concat(hybridCodeStr, " ") : string.Empty), section.SectionCode);

          line = String.Concat(tagStr, section.ID.ItemNumber, "\t", sectionCodeStr, "\t", instructorName, "\t", dayTimeStr, roomStr);
        }
        else // Not primary
        {
          line = String.Concat(tagStr, "\t\talso meets\t", dayTimeStr, roomStr);
        }

        // Append the line to the file
        return line;
      }

      /// <summary>
      /// Takes a section and returns the coded tag. This method is only used during a Class Schedule export.
      /// </summary>
      /// <param name="section">The Section being evaluated.</param>
      /// <param name="item">The particular OfferedItem within the given Section being evaluated.</param>
      /// <returns>A string containing the tag code that represents the given Section.</returns>
      private static string GetSectionExportTag(SectionWithSeats section, OfferedItem item)
      {
        string tag = "<CLS5>";
        string distanceEdMinStr = "7000";
        string distanceEdMaxStr = "ZZZZ";
        string arrangedStr = "Arranged";

        OfferedItem primary = section.Offered.Where(o => o.IsPrimary).FirstOrDefault();
        if ((primary.Days.Contains("Sa") || primary.Days.Contains("Su")) && // TODO: Move to helper function
            !(primary.Days.Contains("M") || primary.Days.Contains("T") || primary.Days.Contains("W") || primary.Days.Contains("Th"))) // Weekend course
        {
          tag = "<CLS7>";
        }
        else if (IsEveningCourse(primary)) // Evening course
        {
          tag = "<CLS6>";
        }
        else if (section.ID.ItemNumber.CompareTo(distanceEdMinStr) >= 0 && section.ID.ItemNumber.CompareTo(distanceEdMaxStr) <= 0) // Distance Ed
        {
          tag = "<CLSD>";
        }
        else if (item.Days == arrangedStr && !section.IsOnline) // Arranged course
        {
          tag = "<CLSA>";
        }

        return tag;
      }

      /// <summary>
      /// Takes section footnotes and the CMS footnotes, and returns a single combined string of all footnotes -- taking
      /// into account duplicates, or any specific footnotes which should be ignored (such as footnotes common to the course).
      /// </summary>
      /// <param name="hpFootnotes">List of section footnotes from the HP.</param>
      /// <param name="cmsFootnote">The CMS footnote.</param>
      /// <param name="toIgnore">List of any footnotes which should be ignored.</param>
      /// <returns>A single string aggregate of all footnotes.</returns>
      private static string GetSectionFootnotes(IEnumerable<string> hpFootnotes, string cmsFootnote, IEnumerable<string> toIgnore)
      {
        StringBuilder footnotes = new StringBuilder();

        // Section footnotes
        foreach (string footnote in hpFootnotes.Distinct())
	      {
		      if (!string.IsNullOrWhiteSpace(footnote))
		      {
			      // skip any footnotes in the supplied ignore list (used to avoid duplicates)
			      if (toIgnore == null || toIgnore.Count() <= 0 || !toIgnore.Contains(footnote))
			      {
				      footnotes.Append(footnote);
			      }
		      }
	      }

        // CMS footnotes
		    //    ensure we don't have duplicates
        if (!string.IsNullOrWhiteSpace(cmsFootnote) && !hpFootnotes.Contains(cmsFootnote))
		    {
          footnotes.Append(cmsFootnote);
		    }

        return footnotes.ToString().Trim();
      }

      /// <summary>
      /// Takes an offered item and determines whether the course qualifies as an evening course.
      /// </summary>
      /// <param name="course">The OfferedItem to evaluate.</param>
      /// <returns>True if evening course, otherwise false.</returns>
      private static bool IsEveningCourse(OfferedItem course)
      {
        bool isEvening = false;

        TimeSpan eveningCourse = new TimeSpan(17, 30, 0); // Hard coded evening course definition; TODO: Move to app settings
        if (course.StartTime.HasValue && course.EndTime.HasValue)
        {
          if (new TimeSpan(course.StartTime.Value.Hour, course.StartTime.Value.Minute, course.StartTime.Value.Second) >= eveningCourse)
          {
            isEvening = false;
          }
        }

        return isEvening;
      }
      #endregion
    }
  }
}
