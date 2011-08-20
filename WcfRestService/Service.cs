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
		//5/25/2011 - Not currently returning results
		[WebGet(UriTemplate = "Course/{YearQuarterID}/{courseID}")]
		public IList<Section> GetSectionsByYQcourseId(string YearQuarterID, string courseID)
		{
			// TODO: include RegistrationQuartersFacet in GetSections() call - to limit range of quarters returned
			IList<Section> sections = new OdsRepository().GetSections(CourseID.FromString(courseID), YearQuarter.FromString(YearQuarterID));

			return sections;
		}

	}
}
