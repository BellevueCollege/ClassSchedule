using Ctc.Ods;

namespace CTCClassSchedule.Models
{
  public class CrossListedCourseModel
  {
    public CourseID ID {get;set;}
    public string Title {get;set;}
    public decimal Credits {get;set;}
    public bool IsVariableCredits {get;set;}
  }
}