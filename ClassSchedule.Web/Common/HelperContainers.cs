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