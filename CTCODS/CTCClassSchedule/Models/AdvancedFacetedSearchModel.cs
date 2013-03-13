using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ctc.Ods.Types;

namespace CTCClassSchedule.Models
{
  public class AdvancedFacetedSearchModel
  {
    public YearQuarter ViewingQuarter { get; set; }
    public IEnumerable<string> Subjects { get; set; } // Collection of Subject Slugs
    public string SelectedSubject { get; set; } // Current Subject Slug
    public bool IsSearch { get; set; }
  }
}