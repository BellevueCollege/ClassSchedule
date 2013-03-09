using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using CTCClassSchedule.Models;
using Common.Logging;
using Ctc.Ods;
using Ctc.Ods.Data;
using Ctc.Ods.Types;
using CtcApi.Web.Mvc;
using System.Web.Security;
using DotNetCasClient;
using CTCClassSchedule.Common;
using Microsoft.Security.Application;

namespace CTCClassSchedule.Controllers
{
	/* This partial class exists to logically compartmentalize all methods of the
	 * ClassesController which are related to editing data, and thus require
	 * authentication.
	 *
	 * This file and the original ClassesController.cs are compiled into a single
	 * ClassController object when the project is built.
	 *
	 *		- 1/11/2012, shawn.south@bellevuecollege.edu
	 */
	public partial class ClassesController : Controller
  {
    private ILog _log = LogManager.GetCurrentClassLogger();

    #region Authentication actions
	  /// <summary>
	  /// A stub method for forcing authentication
	  /// </summary>
	  /// <returns>Nothing. (Not sure what, if anything, the rendering engine actually does with this.)</returns>
	  /// <remarks>
	  /// Call this action method if you need to authenticate a user in without it being a side effect of
	  /// accessing protected data/functionality. For example; in response to the user clicking a "Log in"
	  /// button.
	  /// </remarks>
    /// <seealso cref="https://wiki.jasig.org/pages/viewpage.action?pageId=32210981">Jasig - ASP.NET MVC + CAS</seealso>
	  [CASAuthorize]
	  public ActionResult Authenticate()
	  {
	    string url = HttpContext.Session["ReferralUrlForCas"].ToString();
	    return Redirect(url);
	  }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    /// <seealso cref="https://wiki.jasig.org/pages/viewpage.action?pageId=32210981">Jasig - ASP.NET MVC + CAS</seealso>
    public ActionResult Logout()
	  {

	    string url = Request.UrlReferrer == null ? CasAuthentication.ServerName : Request.UrlReferrer.ToString();

	    if (!string.IsNullOrWhiteSpace(CasAuthentication.CasServerUrlPrefix))
	    {
	      CasAuthentication.SingleSignOut();
	    }
	    else
	    {
	      FormsAuthentication.SignOut();
	    }

	    return Redirect(url);
	  }

	  #endregion

	  #region Editing Section information
	  /// <summary>
	  /// Displays the Section Edit dialog box
	  /// </summary>
	  /// <param name="itemNumber"></param>
	  /// <param name="yrq"></param>
	  /// <returns></returns>
	  [AuthorizeFromConfig(RoleKey = "ApplicationEditor")]
	  [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
	  public ActionResult SectionEdit(string itemNumber, string yrq)
	  {
	    if (HttpContext.User.Identity.IsAuthenticated)
	    {
	      try
	      {
	        ISectionID sectionID = SectionID.FromString(string.Concat(itemNumber, yrq));

	        using (OdsRepository respository = new OdsRepository(HttpContext))
	        {
	          IList<Section> sections = respository.GetSections(new List<ISectionID> {sectionID});

	          if (sections != null && sections.Count > 0)
	          {
	            using (ClassScheduleDb db = new ClassScheduleDb())
	            {
	              IList<SectionWithSeats> sectionsEnum = Helpers.GetSectionsWithSeats(yrq, sections, db);

	              if (sectionsEnum != null && sectionsEnum.Count > 0)
	              {
	                if (sectionsEnum.Count > 1)
	                {
	                  _log.Warn(m => m("Expected to only receive one Section to Edit, but received {0}. Using the first.", sectionsEnum.Count));
	                }

	                SectionWithSeats section = sectionsEnum.First();
	                string secID = section.ID.ToString();

	                SectionEditModel sectionModel = new SectionEditModel
	                                                  {
	                                                    SectionToEdit = section,
	                                                    CrossListedCourses = db.SectionCourseCrosslistings.Where(x => x.ClassID == secID)
	                                                                           .Select(x => x.CourseID)
                                                                             .Distinct()
	                                                                           .ToList()
	                                                  };

	                return PartialView(sectionModel);
	              }
	              _log.Warn(m => m("Cannot Edit Section '{0}' - no records found.", sectionID));
	              return PartialView(null);
	            }
	          }
	          _log.Debug(m => m("Unable to Edit Section '{0}' - the API returned no records.", sectionID));
	        }
	      }
	      catch (Exception ex)
	      {
	        _log.Error(m => m("An exception occurred while attempting to display Edit dialog for Section '{0}':\n{1}", string.Concat(itemNumber,yrq), ex ));
	      }
	    }
	    else
	    {
        _log.Warn(m => m("Unable to display Section Edit dialog - User is not authenticated."));
      }

	    return PartialView(null);
	  }

	  /// <summary>
	  /// Saves the updated <see cref="Section"/> information
	  /// </summary>
	  /// <param name="collection"></param>
	  /// <returns></returns>
	  [HttpPost]
    // BUG: I'm concerned that form input is not being validated by the following line
    // It looks like this was added to work around common issues with passing FormCollection objects in MVC3,
    // but I'm sure there is a better way to handle this - such that input from the form is being validated.
    //  - shawn.south@bellevuecollege.edu, 3/7/2013
	  [ValidateInput(false)]
	  [AuthorizeFromConfig(RoleKey = "ApplicationEditor")]
	  public ActionResult SectionEdit(FormCollection collection)
	  {

/* TODO: Refactor saving Section information
 *
 *  1.  Receive a collection of course crosslistings (see Api/ProgramEdit - can we just pass the collection,
 *      or do we need to pass the Model too?
 *
 *  2.  If needed, refactor other SectionEdit to pass a SectionMeta to the view.
 *
 *  3.  Remove ValidateInputAttribute once we've replaced FormCollection parameter.
 */
	    string referrer = collection["referrer"];

	    if (HttpContext.User.Identity.IsAuthenticated)
	    {
	      string itemNumber = collection["ItemNumber"];
	      string yrq = collection["Yrq"];
	      string username = HttpContext.User.Identity.Name;
	      string sectionFootnotes = StripHtml(collection["section.SectionFootnotes"]);
	      string classID = itemNumber + yrq;
	      string customTitle = collection["section.CustomTitle"];
	      string customDescription = StripHtml(collection["section.CustomDescription"]);

	      if (ModelState.IsValid)
	      {
	        using (ClassScheduleDb db = new ClassScheduleDb())
	        {
	          try
	          {
	            SectionsMeta itemToUpdate = GetItemToUpdate(db.SectionsMetas, s => s.ClassID == classID);

	            itemToUpdate.ClassID = classID;
	            itemToUpdate.Footnote = sectionFootnotes;
	            itemToUpdate.LastUpdated = DateTime.Now;
	            itemToUpdate.LastUpdatedBy = username;
	            itemToUpdate.Title = customTitle == string.Empty ? null : customTitle;
	            itemToUpdate.Description = customDescription;

	            db.SaveChanges();
	          }
	          catch (Exception ex)
	          {
	            _log.Error(m => m("Section changes NOT saved - {0}", ex));
	          }
	        }
	      }
	      else
	      {
	        _log.Warn(m => m("Section changes NOT saved - ModelState is not Valid."));
	      }
	    }
      else
      {
        _log.Warn(m => m("Section changes NOT saved - User is not authenticated."));
      }

	    return Redirect(referrer);
	  }

	  #endregion

	  #region Private methods
	  /// <summary>
	  ///
	  /// </summary>
	  /// <typeparam name="T"></typeparam>
	  /// <param name="entities"></param>
	  /// <param name="expression"></param>
	  /// <returns>Either a <typeparamref name="T"/> object that meets the specified <paramref name="expression"/>, or a new instance.</returns>
	  private static T GetItemToUpdate<T>(ObjectSet<T> entities, Expression<Func<T, bool>> expression)
	    where T : EntityObject, new()
	  {
	    return entities.Any(expression) ? entities.Single(expression) : new T();
	  }

	  /// <summary>
	  ///
	  /// </summary>
	  /// <param name="withHtml"></param>
	  /// <returns></returns>
	  private string StripHtml(string withHtml)
	  {
	    string stripped;
	    // BUG: The appSetting value "CMSHtmlParsingAllowedElements" is not present
	    string whitelist = ConfigurationManager.AppSettings["CMSHtmlParsingAllowedElements"];

	    try
	    {
	      string pattern = @"</?(?(?=" + whitelist +
	                       @")notag|[a-zA-Z0-9]+)(?:\s[a-zA-Z0-9\-]+=?(?:(["",']?).*?\1?)?)*\s*/?>";
	      stripped = Regex.Replace(withHtml, pattern, string.Empty);
	    }
	    catch (Exception ex)
	    {
	      stripped = Encoder.HtmlEncode(withHtml);
	      _log.Warn(
	        m => m("Unable to remove HTML from string '{0}'\nReturning HTML-encoded string instead.\n{1}", withHtml, ex));
	    }
	    return stripped;
	  }

	  #endregion
    }
}
