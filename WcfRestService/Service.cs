using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
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
		public IList<ICourse> GetCourses()
		{
			IList<ICourse> courses = new OdsRepository().GetCourses();
			return courses;
		}

	}
}
