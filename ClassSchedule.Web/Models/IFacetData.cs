using System.Collections.Generic;
using CTCClassSchedule.Common;

namespace CTCClassSchedule
{
  public interface IFacetData
  {
    IList<GeneralFacetInfo> Days {get;}
    IList<GeneralFacetInfo> Modality {get;}
  }
}