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
	 *
	 * Much of the code can be referenced here: https://wiki.jasig.org/pages/viewpage.action?pageId=32210981
	 * -Nathan 2/21/2012
	 *
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
	  ///
	  [CASAuthorize]
	  public ActionResult Authenticate()
	  {
	    string url = HttpContext.Session["ReferralUrlForCas"].ToString();
	    return Redirect(url);
	  }

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
	  /// <param name="subject"></param>
	  /// <param name="classNum"></param>
	  /// <returns></returns>
	  [AuthorizeFromConfig(RoleKey = "ApplicationEditor")]
	  [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
	  public ActionResult SectionEdit(string itemNumber, string yrq, string subject, string classNum)
	  {
	    string classID = itemNumber + yrq;

	    if (HttpContext.User.Identity.IsAuthenticated)
	    {

	      using (OdsRepository respository = new OdsRepository(HttpContext))
	      {
	        IList<YearQuarter> yrqRange = Helpers.getYearQuarterListForMenus(respository);
	        ViewBag.QuarterNavMenu = yrqRange;

	        ICourseID courseID = CourseID.FromString(subject, classNum);
	        IList<Section> sections;
	        sections = respository.GetSections(courseID);

	        Section editSection = null;
	        foreach (Section section in sections)
	        {
	          if (section.ID.ToString() == classID)
	          {
	            editSection = section;
	            break;
	          }
	        }

	        sections.Clear();
	        sections.Add(editSection);

	        IEnumerable<SectionWithSeats> sectionsEnum;
	        SectionsMeta itemToUpdate = null;
	        using (ClassScheduleDb db = new ClassScheduleDb())
	        {
	          sectionsEnum = Helpers.GetSectionsWithSeats(yrqRange[0].ID, sections, db);

	          if (db.SectionsMetas.Any(s => s.ClassID == classID))
	          {
	            itemToUpdate = db.SectionsMetas.Single(s => s.ClassID == classID);
	          }

	          List<SectionWithSeats> localSections = (from s in sectionsEnum
	                                                  select new SectionWithSeats
	                                                           {
	                                                             ParentObject = s,
	                                                             SectionFootnotes =
	                                                               MvcApplication.SafePropertyToString(itemToUpdate,
	                                                                                                   "Footnote",
	                                                                                                   string.Empty),
	                                                             LastUpdated =
	                                                               MvcApplication.SafePropertyToString(itemToUpdate,
	                                                                                                   "LastUpdated",
	                                                                                                   string.Empty),
	                                                             LastUpdatedBy =
	                                                               MvcApplication.SafePropertyToString(itemToUpdate,
	                                                                                                   "LastUpdatedBy",
	                                                                                                   string.Empty),
	                                                             CustomTitle =
	                                                               MvcApplication.SafePropertyToString(itemToUpdate,
	                                                                                                   "Title",
	                                                                                                   string.Empty),
	                                                             CustomDescription =
	                                                               MvcApplication.SafePropertyToString(itemToUpdate,
	                                                                                                   "Description",
	                                                                                                   string.Empty),
	                                                           }).ToList();

	          return PartialView(localSections);
	        }
	      }
	    }
	    return PartialView();
	  }

	  /// <summary>
	  ///
	  /// </summary>
	  /// <param name="collection"></param>
	  /// <returns></returns>
	  [HttpPost]
	  [ValidateInput(false)]
	  [AuthorizeFromConfig(RoleKey = "ApplicationEditor")]
	  public ActionResult SectionEdit(FormCollection collection)
	  {
	    string referrer = collection["referrer"];

	    if (HttpContext.User.Identity.IsAuthenticated == true)
	    {
	      string itemNumber = collection["ItemNumber"];
	      string yrq = collection["Yrq"];
	      string username = HttpContext.User.Identity.Name;
	      string sectionFootnotes = collection["section.SectionFootnotes"];
	      string classID = itemNumber + yrq;
	      string customTitle = collection["section.CustomTitle"];
	      string customDescription = collection["section.CustomDescription"];


	      customDescription = StripHtml(customDescription);
	      sectionFootnotes = StripHtml(sectionFootnotes);

	      SectionsMeta itemToUpdate;

	      if (ModelState.IsValid)
	      {
	        using (ClassScheduleDb db = new ClassScheduleDb())
	        {
	          itemToUpdate = GetItemToUpdate(db.SectionsMetas, s => s.ClassID == classID);

	          itemToUpdate.ClassID = classID;
	          itemToUpdate.Footnote = sectionFootnotes;
	          itemToUpdate.LastUpdated = DateTime.Now;
	          itemToUpdate.LastUpdatedBy = username;
	          itemToUpdate.Title = customTitle == string.Empty ? null : customTitle;
	          itemToUpdate.Description = customDescription;

	          db.SaveChanges();
	        }
	      }
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
