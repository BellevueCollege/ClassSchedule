using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using CTCClassSchedule.Models;
using Common.Logging;
using Ctc.Ods;
using Ctc.Ods.Data;
using Ctc.Ods.Types;
using CtcApi.Extensions;
using CtcApi.Web.Mvc;
using System.Web.Security;
using DotNetCasClient;
using CTCClassSchedule.Common;
using Encoder = Microsoft.Security.Application.Encoder;

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
	  /// <returns>An HTTP redirect to the saved referral page.</returns>
	  /// <remarks>
	  /// Call this action method if you need to authenticate a user in without it being a side effect of
	  /// accessing protected data/functionality. For example; in response to the user clicking a "Log in"
	  /// button.
	  /// </remarks>
    /// <seealso cref="https://wiki.jasig.org/pages/viewpage.action?pageId=32210981">Jasig - ASP.NET MVC + CAS</seealso>
	  [CASAuthorize]
	  public ActionResult Authenticate()
	  {
      string url;

      if (HttpContext.Session != null)
	    {
	      url = HttpContext.Session[CASAuthorizeAttribute.REFERRER_SESSION_LABEL].ToString();
	    }
      else
      {
        url = System.Web.HttpContext.Current.Session[CASAuthorizeAttribute.REFERRER_SESSION_LABEL].ToString();
      }

      // if we didn't find a Referral URL, default to the home page.
	    if (string.IsNullOrWhiteSpace(url))
	    {
	      url = HttpContext.Request.ApplicationPath;
	    }

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

    #region Editing program/Subject information
    /// <summary>
    ///
    /// </summary>
    /// <param name="Abbreviation"></param>
    /// <returns></returns>
    [AuthorizeFromConfig(RoleKey = "ApplicationAdmin")]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public ActionResult ProgramEdit(string Slug)
    {
      //*******************************************************************************************************************************************
      // TODO: Do we need the ProgramEdit method any more? Or should we split its functionality into new methods that handle those specific areas?
      // -- Yes, eventually it will make sense to break this into 3 edit modals (or areas) for Subject, Department, and Division.
      //    However I don't believe we are currently displaying lists of Departments or Divisions anywhere yet. - Andrew C. (3/4/13)
      //*******************************************************************************************************************************************
      if (HttpContext.User.Identity.IsAuthenticated)
      {
        // Get a list of all course prefixes to present the user when they merge/unmerge prefixes to a subject
        IList<string> allPrefixes;
        string commonCourseChar = _apiSettings.RegexPatterns.CommonCourseChar;
        using (OdsRepository repository = new OdsRepository())
        {
          // TODO: This list strips out the '&' char, so we can't differentiate between ACCT& and ACCT.
          //			 It also only returns prefixes for which contains future sections - Andrew C (3/4/2013)
          //allPrefixes = repository.GetCourseSubjects().Select(s => s.Subject).ToList();

          // TODO: This is a temp workaround for getting a list of all Course Prefixes since the GetCourseSubjects() method
          //			 does not return the expected results. - Andrew C (3/4/2013)
          allPrefixes = repository.GetCourses().Select(c => String.Concat(c.Subject, c.IsCommonCourse ? commonCourseChar : string.Empty))
                                               .Distinct()
                                               .ToList();
        }


        // Construct the model and return it
        using (ClassScheduleDb db = new ClassScheduleDb())
        {
          // Exclude all prefixes which have already been merged -- a prefix can only belong ot a single subject
          IList<string> mergablePrefixes = allPrefixes.Except(db.SubjectsCoursePrefixes.Select(p => p.CoursePrefixID)).ToList();

          SubjectInfoResult programInfo = SubjectInfo.GetSubjectInfo(Slug);
          ProgramEditModel model = new ProgramEditModel
          {
            Subject = programInfo.Subject,
            Department = programInfo.Department,
            Division = programInfo.Division,
            AllCoursePrefixes = mergablePrefixes
          };

          return PartialView(model);
        }
      }

      // TODO: This is bad practice. Should return a descriptive error.
      return PartialView();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="MergeSubjects"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateInput(false)]
    [AuthorizeFromConfig(RoleKey = "ApplicationAdmin")]
    public ActionResult ProgramEdit(ProgramEditModel Model, ICollection<string> PrefixesToMerge)
    {
      if (HttpContext.User.Identity.IsAuthenticated == true)
      {
        if (ModelState.IsValid)
        {
          // If no prefixes are being merged, instantiate an empty collection so LINQ queries don't fail
          if (PrefixesToMerge == null)
          {
            PrefixesToMerge = new List<string>();
          }

          string username = HttpContext.User.Identity.Name;
          using (ClassScheduleDb db = new ClassScheduleDb())
          {
            // Lookup the subject being edited
            Subject subject = db.Subjects.Where(s => s.Slug.Equals(Model.Subject.Slug, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            // Update the proper fields
            subject.Title = Model.Subject.Title;
            subject.Intro = StripHtml(Model.Subject.Intro);
            subject.LastUpdated = DateTime.Now;
            subject.LastUpdatedBy = username;

            // If the department does not already exist, create it
            // TODO: ProgramEdit ideally should not handle creation of Departments
            Department department = subject.Department;
            if (department == null)
            {
              department = new Department
              {
                Title = Model.Department.Title,
                URL = Model.Department.URL,
                LastUpdated = DateTime.Now,
                LastUpdatedBy = username
              };

              db.Departments.AddObject(department);
              subject.DepartmentID = department.DepartmentID;
            }
            else
            {
              department.Title = Model.Department.Title;
              department.URL = Model.Department.URL;
            }

            // If the division does not already exist, create it
            // TODO: ProgramEdit ideally should not handle creation of Divisions
            Division division = department.Division;
            if (division == null)
            {
              division = new Division
              {
                Title = Model.Division.Title,
                URL = Model.Division.URL,
                LastUpdated = DateTime.Now,
                LastUpdatedBy = username
              };

              db.Divisions.AddObject(division);
              department.DivisionID = division.DivisionID;
            }
            else
            {
              division.Title = Model.Division.Title;
              division.URL = Model.Division.URL;
            }


            // Only allow merging/unmerging of prefixes if at least one prefix is assigned to the subject
            if (PrefixesToMerge.Count > 0)
            {
              // Unmerge subjects
              IList<SubjectsCoursePrefix> unmergables = subject.CoursePrefixes.Where(s => !PrefixesToMerge.Contains(s.CoursePrefixID)).ToList();
              foreach (SubjectsCoursePrefix prefix in unmergables)
              {
                subject.CoursePrefixes.Remove(prefix);
              }

              // Merge subjects
              IList<string> currentlyMergedPrefixes = subject.CoursePrefixes.Select(s => s.CoursePrefixID).ToList();
              IEnumerable<string> mergables = PrefixesToMerge.Where(m => !currentlyMergedPrefixes.Contains(m));
              foreach (string prefix in mergables)
              {
                subject.CoursePrefixes.Add(new SubjectsCoursePrefix
                {
                  SubjectID = subject.SubjectID,
                  CoursePrefixID = prefix
                });
              }
            }

            // Commit changes to database
            db.SaveChanges();
          }
        }

      }

      // Refresh the page to display the updated info
      return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
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
	  /// <param name="Model"></param>
	  /// <param name="sectionID"></param>
	  /// <param name="CrossListedCourses"></param>
	  /// <returns></returns>
	  [HttpPost]
	  [AuthorizeFromConfig(RoleKey = "ApplicationEditor")]
    public ActionResult SectionEdit([ModelBinder(typeof(SectionEditModelBinder))]SectionEditModel Model, string sectionID, ICollection<string> CrossListedCourses)
	  {
      string referrer = (Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : string.Empty);

	    if (HttpContext.User.Identity.IsAuthenticated)
	    {
	      if (ModelState.IsValid)
	      {
	        using (ClassScheduleDb db = new ClassScheduleDb())
	        {
	          try
	          {
              string username = HttpContext.User.Identity.Name;

              // HACK: sectionID value is explicitly/separately passed to this Action method.
              // The nested custom ModelBinders (see ModelBinderAttribute in method signature) seem to work, but
              // the ID value still comes through as NULL. When there is more time I'd like to continue trying
              // to get the ModelBinders to work properly.
              //  - shawn.south@bellevuecollege.edu, 3/14/2013
              IQueryable<SectionsMeta> sects = db.SectionsMetas.Where(m => m.ClassID == sectionID);
	            int sectCount = sects.Count();

              SectionsMeta sectionMeta;

	            if (sectCount > 0)
	            {
	              if (sectCount == 1)
	              {
	                sectionMeta = sects.Single();
	              }
	              else
	              {
	                _log.Warn(m => m("More than one SectionsMeta record found for '{0}'. Using the first and ignoring others."));
	                sectionMeta = sects.First();
	              }

                // update values, if they've changed
	              bool changed = false;

                // TODO: Strip HTML from input before assigning
                if (TestAndUpdate(Model.SectionToEdit.CourseTitle != sectionMeta.Title, ref changed))
                {
	                sectionMeta.Title = Model.SectionToEdit.CourseTitle;
	              }
                if (TestAndUpdate(Model.SectionToEdit.CustomDescription != sectionMeta.Description, ref changed))
	              {
                  sectionMeta.Description = Model.SectionToEdit.CustomDescription;
	              }
                if (TestAndUpdate(Model.SectionToEdit.SectionFootnotes != sectionMeta.Footnote, ref changed))
                {
                  sectionMeta.Footnote = Model.SectionToEdit.SectionFootnotes;
	              }

	              if (changed)
	              {
	                sectionMeta.LastUpdated = DateTime.Now;
	                sectionMeta.LastUpdatedBy = username;
	              }
	            }
	            else
	            {
	              sectionMeta = new SectionsMeta
	                               {
	                                 ClassID = sectionID,
	                                 Title = Model.SectionToEdit.CourseTitle,
	                                 Description = Model.SectionToEdit.CustomDescription,
	                                 Footnote = Model.SectionToEdit.SectionFootnotes,
	                                 LastUpdated = DateTime.Now,
                                   LastUpdatedBy = username
	                               };

                db.SectionsMetas.AddObject(sectionMeta);
	            }

              IEnumerable<SectionCourseCrosslisting> currentSectionCrosslistings = db.SectionCourseCrosslistings.Where(x => x.ClassID == sectionID);
              int currentCrosslistCount = currentSectionCrosslistings.Count();

	            if (CrossListedCourses != null && CrossListedCourses.Count > 0)
	            {
                // remove existing cross-listings from the database they weren't passed
                if (currentCrosslistCount > 0)
	              {
	                IList<SectionCourseCrosslisting> unmergables = currentSectionCrosslistings.Where(x => !CrossListedCourses.Contains(x.CourseID)).ToList();
	                foreach (SectionCourseCrosslisting course in unmergables)
	                {
	                  db.SectionCourseCrosslistings.DeleteObject(course);
	                }
	              }

                // add any new cross-listings
	              IEnumerable<string> crosslistingsToAdd = CrossListedCourses; // start with the full list...
	              if (currentCrosslistCount > 0)
	              {
                  // ...then, if we already have records in the db, replace with just those items that aren't there yet
	                crosslistingsToAdd = CrossListedCourses.Where(c => !currentSectionCrosslistings.Select(x => x.CourseID).Contains(c));
	              }

                foreach (string prefix in crosslistingsToAdd)
                {
                  db.SectionCourseCrosslistings.AddObject(new SectionCourseCrosslisting
                  {
                    ClassID = sectionID,
                    CourseID = prefix
                  });

                }
              }
	            else
	            {
                // Since no CrossListedCourses were passed, delete all existing ones from the database
	              if (currentCrosslistCount > 0)
	              {
                  // TODO: There's got to be a better way to delete multiple records than looping through them.
	                foreach (SectionCourseCrosslisting crosslistRecord in currentSectionCrosslistings)
	                {
	                  db.SectionCourseCrosslistings.DeleteObject(crosslistRecord);
	                }
	              }
	            }

              // saves ALL additions/deletions/changes made earlier in this method
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
          // Collect all ModelError messages...
          StringBuilder msg = new StringBuilder();
          foreach (string key in ModelState.Keys)
          {
            foreach (ModelError err in ModelState[key].Errors)
            {
              // ...so we can display them.
              msg.AppendFormat("[{0}] - {1}\n", key, string.IsNullOrWhiteSpace(err.ErrorMessage) ? err.Exception.ToString() : err.ErrorMessage);
            }
          }
	        _log.Warn(m => m("Section changes NOT saved - ModelState is not Valid.\n{0}", msg));
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
	  /// Tests <paramref name="expression"/> and updates a <paramref name="mutable"/> status flag
	  /// </summary>
	  /// <param name="expression"></param>
	  /// <param name="mutable">
	  ///   A flag which maintains the status of <paramref name="expression"/> inclusively OR'd with its previous value.
	  ///   THE VALUE OF <paramref name="mutable"/> MAY CHANGE.
	  /// </param>
	  /// <remarks>
    ///   This method supports performing multiple successive Boolean tests while keeping track of if <i>any</i> of them
    ///   were true. Especially useful for determining if changes were made to any of a collection of data fields.
    /// </remarks>
	  /// <returns>
	  ///   The Boolean evaluation of <paramref name="expression"/>
	  /// </returns>
	  private static bool TestAndUpdate(bool expression, ref bool mutable)
    {
      mutable = (mutable || expression);
      return expression;
    }

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
