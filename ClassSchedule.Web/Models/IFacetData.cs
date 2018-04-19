/*
This file is part of CtcClassSchedule.

CtcClassSchedule is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

CtcClassSchedule is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with CtcClassSchedule.  If not, see <http://www.gnu.org/licenses/>.
 */
using System.Collections.Generic;
using CTCClassSchedule.Common;

namespace CTCClassSchedule
{
  public interface IFacetData
  {
    IList<GeneralFacetInfo> Days {get;}
    IList<GeneralFacetInfo> Modality {get;}

    /// <summary>
    /// 
    /// </summary>
    string TimeStart {get;set;}

    /// <summary>
    /// 
    /// </summary>
    string TimeEnd {get;set;}

    /// <summary>
    /// 
    /// </summary>
    string Availability {get;set;}

    /// <summary>
    /// 
    /// </summary>
    string LateStart {get;set;}

    /// <summary>
    /// 
    /// </summary>
    string Credits {get;set;}

    /// <summary>
    /// 
    /// </summary>
    int CreditsAny {get;}
  }
}