using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Ctc.Ods.Types;

namespace CTCClassSchedule.Models
{
  public class SectionEditModel
  {
    public SectionWithSeats SectionToEdit {get;set;}

    public IList<string> CrossListedCourses {get;set;}
  }
}