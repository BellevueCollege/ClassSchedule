using System.Collections.Generic;
using CTCClassSchedule.Common;
using Ctc.Ods.Types;

namespace CTCClassSchedule.Models
{
  public class SearchResultNoSectionModel
  {
    public IEnumerable<SearchResultNoSection> NoSectionSearchResults { get; set; }
    public YearQuarter SearchedYearQuarter { get; set; }
  }
}