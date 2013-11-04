using System.Collections.Generic;
using CTCClassSchedule.Common;
using CTCClassSchedule.Models;

namespace CTCClassSchedule
{
	public class SearchResultsModel
	{
		public IEnumerable<string> AllSubjects { get; set; }
	  public IList<SectionsBlock> Courses {get;set;}

	  public SearchResultNoSectionModel SearchResultNoSection { get; set; }

    /// <summary>
    /// Total number of search results found
    /// </summary>
	  public int ItemCount {get;set;}
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }

	  public QuarterNavigationModel QuarterNavigation {get;set;}
	}
}