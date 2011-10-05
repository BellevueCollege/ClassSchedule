using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using Ctc.Ods;
using Ctc.Ods.Data;
using Ctc.Ods.Types;

namespace Ctc.Wcf
{
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
	public class Service : IService
	{
		IList<Course> IService.GetCoursesByCourseID(string courseID)
		{
			// TODO: include RegistrationQuartersFacet in GetSections() call - to limit range of quarters returned
            using (OdsRepository repository = new OdsRepository())
            {
                return repository.GetCourses(CourseID.FromString(courseID));
            }
		}

	}
}
