using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CTCClassSchedule.Models
{
  public class AToZViewModel
  {
    public IList<char> LettersList { get; set; }
    public char? ViewingLetter { get; set; }
  }
}