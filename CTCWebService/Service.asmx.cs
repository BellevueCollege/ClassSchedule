using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Linq;
using System.Xml.Serialization;
using System.Web;
using System.Web.Services;
using System.ServiceModel;
using System.ServiceModel.Web;
using Ctc.Ods.Data;
using Ctc.Ods.Types;

namespace Ctc.WebService
{
	/// <summary>
	/// Web Service for accessing the CTCODS Data Access API
	/// </summary>
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
	 [System.Web.Script.Services.ScriptService]

	public class Service : DataService<OdsRepository> //System.Web.Services.WebService
	{
		// This method is called only once to initialize service-wide policies.
		public static void InitializeService(DataServiceConfiguration config)
		{
			//
			config.SetEntitySetAccessRule("*", EntitySetRights.AllRead);
			config.SetServiceOperationAccessRule("*", ServiceOperationRights.AllRead);
			// Set paging to retrieve 25 at a time
			config.SetEntitySetPageSize("*", 25);
			//config.DataServiceBehavior.MaxProtocolVersion = dataservice DataServiceProtocolVersion.V2;
		}

		protected override void OnStartProcessingRequest(ProcessRequestArgs args)
		{
			base.OnStartProcessingRequest(args);
			//cache for a minute based on query string
			HttpContext context = HttpContext.Current;
			HttpCachePolicy c = HttpContext.Current.Response.Cache;
			c.SetCacheability(HttpCacheability.ServerAndPrivate);
			c.SetExpires(HttpContext.Current.Timestamp.AddSeconds(60));
			c.VaryByHeaders["Accept"] = true;
			c.VaryByHeaders["Accept-Charset"] = true;
			c.VaryByHeaders["Accept-Encoding"] = true;
			c.VaryByParams["*"] = true;
		}

		[WebMethod]
		public List<Footnote> GetFootnotes(string id, string id2 = null)
		{
			OdsRepository repo = new OdsRepository();
			return repo.GetFootnote(id, id2).ToList();
		}


		//[WebMethod]
		//public List<Section> GetSections()
		//{
		//  using (OdsRepository repository = new OdsRepository())
		//  {
		//    IList<Section> sections = repository.GetSections();
		//    return sections.ToList(); //new List<Section>();
		//  }
		//}
	}
}