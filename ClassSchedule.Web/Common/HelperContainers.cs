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
/**********************************************************************************
 * This file contains classes for holding subsets of data that need to be passed
 * around the application.
 **********************************************************************************/
using Ctc.Ods;

namespace CTCClassSchedule.Common
{
	public class GeneralFacetInfo
	{
	  private string _value;

	  public string ID{get;set;}
		public string Title{get;set;}

	  public string Value
	  {
	    get {return _value;}
	    set
	    {
	      _value = value;
	      Selected = Utility.SafeConvertToBool(_value);
	    }
	  }

	  public bool Selected{get;set;}
	}

	public class CourseHeading
	{
		public string ID{get;set;}
		public string Subject{get;set;}
		public string Number{get;set;}
		public string Title{get;set;}
		public decimal Credits{get;set;}
	}
}