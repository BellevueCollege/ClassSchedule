using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ctc.Ods.Types;

namespace CTCClassSchedule.Models
{
  public class ClassDetailsModel
  {
    public IList<Course> Courses { get; set; }
    public YearQuarter CurrentQuarter { get; set; }
    public IList<YearQuarter> NavigationQuarters { get; set; }
    public IList<YearQuarter> QuartersOffered { get; set; }
    public string Slug { get; set; }
    public string SubjectTitle { get; set; }
    public string SubjectIntro { get; set; }
    public string DepartmentTitle { get; set; }
    public string DepartmentURL { get; set; }
    public string CMSFootnote { get; set; }
    public string LearningOutcomes { get; set; }
  }
}