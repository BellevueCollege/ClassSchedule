using System.Collections.Generic;
using System.Linq;

namespace CTCClassSchedule.Models
{
  public class SectionEditModel
  {
    public SectionWithSeats SectionToEdit {get;set;}

    public IList<string> CrossListedCourses {get;set;}
  }
}