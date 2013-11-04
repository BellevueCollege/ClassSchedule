using System.Collections.Generic;
using System.Linq;
using CTCClassSchedule.Common;
using Ctc.Ods.Types;

namespace CTCClassSchedule.Models
{
  public class YearQuarterModel
  {
    public IList<YearQuarter> NavigationQuarters {get;set;}

    public string WhichClass {get;set;}

    public IList<GeneralFacetInfo> Modality {get;set;}

    public IList<GeneralFacetInfo> Days {get;set;}

    public IDictionary<string, object> LinkParams {get;set;}

    public List<Subject> ViewingSubjects {get;set;}

    public IList<char> SubjectLetters {get;set;}

    public YearQuarter ViewingQuarter {get;set;}
  }
}