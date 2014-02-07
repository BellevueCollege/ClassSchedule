using System.Collections.Generic;
using Ctc.Ods.Types;
using CTCClassSchedule.Common;

namespace CTCClassSchedule.Models
{
  public class AdvancedFacetedSearchModel
  {
    public YearQuarter ViewingQuarter { get; set; }
    public IEnumerable<string> Subjects { get; set; } // Collection of Subject Slugs
    public string SelectedSubject { get; set; } // Current Subject Slug
    public bool IsSearch { get; set; }
    public IList<GeneralFacetInfo> Days {get;set;}
    public IList<GeneralFacetInfo> Modality {get;set;}
  }
}