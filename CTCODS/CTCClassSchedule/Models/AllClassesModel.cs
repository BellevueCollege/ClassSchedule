using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ctc.Ods.Types;

namespace CTCClassSchedule.Models
{
  public class AllClassesModel
  {
    public YearQuarter CurrentQuarter { get; set; }
    public IList<Subject> Subjects { get; set; }
    public IList<YearQuarter> NavigationQuarters { get; set; }
    public IList<char> LettersList { get; set; }
    public char? ViewingLetter { get; set; }
  }
}