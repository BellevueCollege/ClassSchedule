using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ctc.Ods.Types;

namespace CTCClassSchedule.Models
{
  public class SectionsViewModel
  {
    public IList<SectionWithSeats> Sections { get; set; }
    public YearQuarter CurrentQuarter { get; set; }
    public YearQuarter ViewingQuarter { get; set; }
    public IEnumerable<string> CommonFootnotes { get; set; }
  }
}