using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
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
		public IList<Course> GetCourses()
		{
			IList<Course> courses = new OdsRepository().GetCourses();
			return courses;
		}

	}
}
