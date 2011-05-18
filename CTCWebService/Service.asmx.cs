using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Services;
using System.Web;
using System.Web.Services;
using System.ServiceModel;
using System.ServiceModel.Web;
using Ctc.Ods.Data;
using Ctc.Ods.Types;

namespace Ctc.WebService
{
	/// <summary>
	/// Summary description for Service1
	/// </summary>
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
	// [System.Web.Script.Services.ScriptService]

	public class Service : System.Web.Services.WebService
	{


		[WebMethod]
		public List<Footnote> GetFootnotes(string id, string id2 = null)
		{
			OdsRepository repo = new OdsRepository();
			return repo.GetFootnote(id, id2).ToList();
		}
	}
}