using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ctc.Ods.Types;
using CTCClassSchedule.Common;

namespace CTCClassSchedule.Models
{
  public class YearQuarterSubjectModel
  {
    public IList<SectionsBlock> Courses { get; set; }
    public YearQuarter CurrentRegistrationQuarter { get; set; }
    public YearQuarter CurrentQuarter { get; set; }
    public IList<YearQuarter> NavigationQuarters { get; set; }
    public YearQuarter ViewingQuarter { get; set; }
    public string Slug { get; set; }
    public string SubjectTitle { get; set; }
    public string SubjectIntro { get; set; }
    public string DepartmentTitle { get; set; }
    public string DepartmentURL { get; set; }
    public IFacetData FacetData {get;set;}
  }
}