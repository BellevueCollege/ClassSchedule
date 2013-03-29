using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CTCClassSchedule.Models;
using System.Web.Mvc;
using System.Text;
using Ctc.Ods.Types;
using Ctc.Ods.Data;

namespace CTCClassSchedule.Common
{
  using GroupedClassScheduleData = Dictionary<Division, Dictionary<Subject, IList<SectionsBlock>>>;
  using CTCClassSchedule.Controllers;

  public static class ClassScheduleExporter
  {

    public static FileResult GetFile(YearQuarter yearQuarter)
    {
      // Convert each object to a node
      IList<IExportableNode> nodes = GetClassScheduleDataNodes(yearQuarter);

      // Build the contents of the file
      StringBuilder fileText = new StringBuilder();
      foreach (IExportableNode node in nodes)
      {
        fileText.Append(node);
      }


      // Convert the contents to an actual file
      FileResult file = new FileContentResult(Encoding.UTF8.GetBytes(fileText.ToString()), "text/plain");
      file.FileDownloadName = String.Concat("ClassSchedule-", yearQuarter.ID, "-", DateTime.Now.ToShortDateString(), ".rtf");
      return file;
    }


    private static IList<IExportableNode> GetClassScheduleDataNodes(YearQuarter yearQuarter)
    {
      IList<IExportableNode> nodes = new List<IExportableNode>();
      GroupedClassScheduleData divisionSections = GroupClassScheduleData(yearQuarter);


      // Convert all Division, Departments, Subjects, and Sections into nodes
      // Sort each group of data as we convert it to nodes
      foreach (Division div in divisionSections.Keys.OrderBy(d => d.Title)) // Divisions
      {
        foreach (Subject sub in divisionSections[div].Keys.OrderBy(d => d.Title)) // Subjects
        {
          nodes.Add(new ExportableSubjectNode(sub));

          foreach (SectionsBlock block in divisionSections[div][sub]) // Sections
          {
            nodes.Add(new ExportableSectionsBlockNode(block));
          }
        }
      }

      return nodes;
    }
    private static GroupedClassScheduleData GroupClassScheduleData(YearQuarter yearQuarter)
    {
      GroupedClassScheduleData divisionSectionsBlock = new GroupedClassScheduleData();

      // Convert Class Schedule data into a List of exportable nodes, which contain
      // rules for exporting the data
      using (ClassScheduleDb db = new ClassScheduleDb())
      {
        // Get all course prefixes
        //List<string> allPrefixes = db.SubjectsCoursePrefixes.Select(p => p.CoursePrefixID).Distinct().ToList();

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
          IList<string> divisionPrefixes = subjectPrefixes.Values.SelectMany(p => p).Distinct().ToList();
          List<Section> sections = new List<Section>();

          // TODO: Make this more efficient -- passing all prefixes at once throws a LINQ error because the query is too complex
          int batch = 20;
          for (int i = 0; i < (divisionPrefixes.Count / batch)+1; i++)
          {
            IList<string> prefixes = divisionPrefixes.Skip(i * batch).Take(batch).ToList();
            sections.AddRange(repository.GetSections(prefixes, yearQuarter));
          }

          sectionsWithSeats = Helpers.GetSectionsWithSeats(yearQuarter.ID, sections, db);
        }

        divisionSectionsBlock = (from d in subjects.Where(r => sectionsWithSeats.Any(q => r.CoursePrefixes.Any(p => p.CoursePrefixID == q.CourseSubject)))
                                                   .Select(s => s.Department.Division)
                                                   .Distinct()
                                 select d).ToDictionary(
                                   d => d,
                                   s => s.Departments.SelectMany(de => de.Subjects)
                                                     .Where(r => sectionsWithSeats.Any(q => r.CoursePrefixes.Any(p => p.CoursePrefixID == q.CourseSubject)))
                                                     .ToDictionary(k => k, v => Helpers.groupSectionsIntoBlocks(sectionsWithSeats.Where(q => v.CoursePrefixes.Any(p => q.CourseSubject == p.CoursePrefixID)).ToList(), db))
                                );

      }

      return divisionSectionsBlock;
    }



    private interface IExportableNode
    {
    }
    private abstract class ExportableNode<T> : IExportableNode
    {
      protected StringBuilder builder = new StringBuilder();
      protected T Node;

      public ExportableNode(T node)
      {
        Node = node;
      }

      protected abstract string ExportNodeAsString();

      public override string ToString()
      {
        return ExportNodeAsString();
      }
    }

    private class ExportableSubjectNode : ExportableNode<Subject>
    {

      public ExportableSubjectNode(Subject node)
        : base(node)
      {
      }

      protected override string ExportNodeAsString()
      {
        int subjectSeparatedLineBreaks = 4;
        for (int i = 0; i < subjectSeparatedLineBreaks; i++)
        {
          builder.AppendLine();
        }

        builder.AppendFormat("<CLS1>{0}\n", Node.Title);
        builder.AppendFormat("<CLS9>{0}\n", Node.Department.Division.Title);
        builder.AppendFormat("<CLSP>{0}\n", Node.Intro);

        return builder.ToString();
      }
    }
    private class ExportableSectionsBlockNode : ExportableNode<SectionsBlock>
    {

      public ExportableSectionsBlockNode(SectionsBlock node)
        : base(node)
      {
      }


      protected override string ExportNodeAsString()
      {
        SectionWithSeats primarySection = Node.Sections.First();

        // Section title
        builder.AppendLine();
        if (primarySection.IsLinked)
        {
          // How do we display linked courses?
        }

        builder.AppendLine(getSectionExportHeader(primarySection));
        IList<SectionWithSeats> commonLinkedSections = ClassesController.ParseCommonHeadingLinkedSections(Node.LinkedSections);
        foreach (SectionWithSeats linkedSec in commonLinkedSections)
        {
          // TODO: How do we display linked courses?
          //getSectionExportHeader(linkedSec)
        }



        // Add Course and HP footnotes
        string footnotes = String.Concat(Node.CommonFootnotes, " ", primarySection.CourseFootnotes);
        if (String.IsNullOrWhiteSpace(footnotes))
        {
          builder.AppendLine(String.Concat("<CLS3>", footnotes));
        }



        // Output all meeting times for grouped sections
        foreach (SectionWithSeats sec in Node.Sections)
        {
          // Both Sections includes a master and its subordinate linked section(s)
          IEnumerable<SectionWithSeats> groupedSections = new List<SectionWithSeats>() { sec };
          IList<SectionWithSeats> linked = Node.LinkedSections.Where(l => l.LinkedTo == sec.ID.ItemNumber).ToList();

          if (linked.Count > 0)
          {
            groupedSections = groupedSections.Union(linked).ToList();
          }

          foreach (SectionWithSeats currentSection in groupedSections)
          {
            foreach (OfferedItem item in currentSection.Offered.OrderBy(o => o.SequenceOrder))
            {
              builder.AppendLine(buildOfferedItemsExportText(currentSection, item));
            }

            // Section and course footnotes
            footnotes = currentSection.SectionFootnotes;
            if (!String.IsNullOrWhiteSpace(footnotes))
            {
              builder.AppendLine(String.Concat("<CLSN>", footnotes.Trim()));
            }


            // Add Automated footnotes
            // Only display the automated hybrid footnote on the last hybrid section to avoid duplicate footnotes
            string hyrbidFootnote = AutomatedFootnotesConfig.Footnotes("hybrid").Text;
            footnotes = AutomatedFootnotesConfig.getAutomatedFootnotesText(currentSection);
            if (footnotes.Contains(hyrbidFootnote)
                && currentSection != Node.Sections.Where(s => s.IsHybrid && currentSection.CourseNumber == s.CourseNumber && currentSection.Credits == s.Credits && currentSection.CourseTitle == s.CourseTitle && currentSection.CustomTitle == s.CustomTitle).LastOrDefault())
            {
              footnotes = footnotes.Replace(hyrbidFootnote, string.Empty);
            }
            if (!String.IsNullOrWhiteSpace(footnotes))
            {
              builder.AppendLine(String.Concat("<CLSY>", footnotes.Trim()));
            }
          }
        }

        return builder.ToString();
      }



      #region Private Helpers
      private static string getSectionExportHeader(SectionWithSeats section)
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
      private static string buildOfferedItemsExportText(SectionWithSeats section, OfferedItem item)
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
        string tagStr = getSectionExportTag(section, item);


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
          string instructorName = Helpers.getShortNameFormat(item.InstructorName) ?? instructorDefaultNameStr;
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
      private static string getSectionExportTag(SectionWithSeats section, OfferedItem item)
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
        else if (isEveningCourse(primary)) // Evening course
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
      /// Takes an offered item and determines whether the course qualifies as an evening course.
      /// </summary>
      /// <param name="course">The OfferedItem to evaluate.</param>
      /// <returns>True if evening course, otherwise false.</returns>
      private static bool isEveningCourse(OfferedItem course)
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