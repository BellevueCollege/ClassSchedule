using Ctc.Ods;
using Ctc.Ods.Types;

namespace CTCClassSchedule.Models
{
  public class CrossListedCourseModel
  {
    public ICourseID CourseID {get;set;}
    public string Title {get;set;}
    public decimal Credits {get;set;}
    public bool IsVariableCredits {get;set;}
    public bool IsCommonCourse {get;set;}
    public ISectionID SectionID {get;set;}
  }
}