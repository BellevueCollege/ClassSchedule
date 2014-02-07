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
using Ctc.Ods.Types;

namespace CTCClassSchedule.Models
{
  public class AdvancedFacetedSearchModel
  {
    public YearQuarter ViewingQuarter { get; set; }
    public IEnumerable<string> Subjects { get; set; } // Collection of Subject Slugs
    public string SelectedSubject { get; set; } // Current Subject Slug
    public bool IsSearch { get; set; }
    public IFacetData FacetData {get;set;}
  }
}