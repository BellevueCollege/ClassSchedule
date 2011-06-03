using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using Ctc.Ods;
using Ctc.Ods.Data;
using Ctc.Ods.Types;

namespace Ctc.Wcf
{
	[ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
	public class Service
	{

		[WebGet(UriTemplate = "Footnote/{id}")]
		public IList<Footnote> GetFootnotes(string id)
		{
			IList<Footnote> footNotes = new OdsRepository().GetFootnote(id);
			return footNotes;
		}


		[WebGet(UriTemplate = "Course/")]
		public IList<Course> GetAllCourses()
		{
			string YearQuarterID = "B014"; // TODO: add a way to get current ActiveYQ as default
			IList<Course> courses = new OdsRepository().GetCourses(YearQuarter.FromString(YearQuarterID));
			//courses = courses as Course;

			return courses;
		}
		[WebGet(UriTemplate = "Course/{YearQuarterID}")]
		public IList<Course> GetCoursesByYQ(string YearQuarterID = "B014")
		{
			IList<Course> courses = new OdsRepository().GetCourses(YearQuarter.FromString(YearQuarterID));
			//courses = courses as Course;

			return courses;
		}

		 //5/25/2011 - Not currently returning results
		[WebGet(UriTemplate = "Course/{YearQuarterID}/{courseID}")]
		public IList<Section> GetSectionsByYQcourseId(string YearQuarterID, string courseID)
		{
		  IList<Section> sections = new OdsRepository().GetSections(CourseID.FromString(courseID), YearQuarter.FromString(YearQuarterID));

		  return sections;
		}

	}
}
