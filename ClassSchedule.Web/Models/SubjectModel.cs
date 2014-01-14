using System.Collections.Generic;

namespace CTCClassSchedule.Models
{
  public class SubjectModel
  {
    public string Title {get;set;}
    public string Slug {get;set;}
    public IList<string> CoursePrefixes {get;set;}
  }
}