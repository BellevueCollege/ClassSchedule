using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTCClassSchedule.Models;
using Ctc.Ods;
using Ctc.Ods.Data;
using Ctc.Ods.Types;
using CtcApi.Web.Mvc;
using DotNetCasClient.Security;
using System.Net;
using System.IO;
using System.Web.Security;
using DotNetCasClient;
using CTCClassSchedule.Common;

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

    // TODO: Move to Edit methods to ClassesController_Edit.cs
    //Generation of the Section Edit dialog box
    /// <summary>
    ///
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
                                                      SectionFootnotes = MvcApplication.SafePropertyToString(itemToUpdate, "Footnote", string.Empty),
                                                      LastUpdated = MvcApplication.SafePropertyToString(itemToUpdate, "LastUpdated", string.Empty),
                                                      LastUpdatedBy = MvcApplication.SafePropertyToString(itemToUpdate, "LastUpdatedBy", string.Empty),
                                                      CustomTitle = MvcApplication.SafePropertyToString(itemToUpdate, "Title", string.Empty),
                                                      CustomDescription = MvcApplication.SafePropertyToString(itemToUpdate, "Description", string.Empty),
                                                    }).ToList();

            return PartialView(localSections);
          }
        }
      }
      return PartialView();
    }
    }
}
