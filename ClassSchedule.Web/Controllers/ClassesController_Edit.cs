/*
This file is part of CtcClassSchedule.

CtcClassSchedule is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

CtcClassSchedule is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with CtcClassSchedule.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using CTCClassSchedule.Models;
using Common.Logging;
using Ctc.Ods.Data;
using Ctc.Ods.Types;
using CtcApi.Web.Mvc;
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
	 */
	public partial class ClassesController : Controller
  {
        private const string DEFAULT_LOGOUT_URL = "~"; // go back to home page
        private readonly ILog _log = LogManager.GetLogger(typeof(ClassesController));

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
	  [SsoAuthorize]
	  public ActionResult Authenticate()
	  {
	    string originalUrl;
	    if (Session != null)
	    {
        // NOTE: SsoAuthorizeAttribute assumes the SSO system is not populating the REFERRER header
	      originalUrl = Session[SsoAuthorizeAttribute.REFERRER_SESSION_LABEL].ToString();
	    }
	    else
	    {
	      originalUrl = System.Web.HttpContext.Current.Session[SsoAuthorizeAttribute.REFERRER_SESSION_LABEL].ToString();
	    }

	    if (string.IsNullOrWhiteSpace(originalUrl))
	    {
	      originalUrl = Request.UserHostName + Request.ApplicationPath;
	    }

	    return Redirect(originalUrl);
	  }

    /// <summary>
    /// Performs a <em>single sign-out</em>
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// The user is logged out of the entire college namespace.
    /// </remarks>
    public ActionResult Logout()
	  {
      string logoutUrl = ConfigurationManager.AppSettings["LogoutUrl"] ?? DEFAULT_LOGOUT_URL;
      
      _log.Trace(m => m("User '{0}' logging out - redirecting to <{1}>...", User != null ? User.Identity.Name : string.Empty, logoutUrl));
      
      return Redirect(logoutUrl);
	  }

	  #endregion

    #region Editing program/Subject information
    /// <summary>
    /// Displays dialog for editing Subject info, incl. Dept & Division
    /// </summary>
    /// <param name="Slug"></param>
    /// <returns></returns>
    [AuthorizeFromConfig(RoleKey = "ApplicationAdmin")]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public ActionResult ProgramEdit(string Slug)
    {
      if (HttpContext.User.Identity.IsAuthenticated)
      {
        // Get a list of all course prefixes to present the user when they merge/unmerge prefixes to a subject
        IList<string> allPrefixes;
        string commonCourseChar = _apiSettings.RegexPatterns.CommonCourseChar;
        using (OdsRepository repository = new OdsRepository())
        {
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
                                       AllCoursePrefixes = mergablePrefixes,
                                       AllDepartments = db.Departments.Where(d => (d.Title != null && d.Title != "") || d.DepartmentID == programInfo.Department.DepartmentID).ToList(),
                                       AllDivisions = db.Divisions.Where(d => (d.Title != null && d.Title != "") || d.DivisionID == programInfo.Division.DivisionID).ToList()
                                     };

          return PartialView(model);
        }
      }

      // TODO: This is bad practice. Should return a descriptive error.
      return PartialView();
    }

	  ///  <summary>
	  /// 
	  ///  </summary>
	  /// <param name="model"></param>
	  /// <param name="prefixesToMerge"></param>
	  /// <returns></returns>
	  [HttpPost]
    [ValidateInput(false)]
    [AuthorizeFromConfig(RoleKey = "ApplicationAdmin")]
    public ActionResult ProgramEdit(ProgramEditModel model, ICollection<string> prefixesToMerge)
    {
      if (HttpContext.User.Identity.IsAuthenticated)
      {
        if (ModelState.IsValid)
        {
          string username = HttpContext.User.Identity.Name;
          using (ClassScheduleDb db = new ClassScheduleDb())
          {
            // Lookup the subject being edited
            Subject subject = db.Subjects.Where(s => s.Slug.Equals(model.Subject.Slug, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (subject != null)
            {
              // Update the proper fields
              subject.Title = model.Subject.Title;
              subject.Intro = Helpers.StripHtml(model.Subject.Intro);
              subject.LastUpdated = DateTime.Now;
              subject.LastUpdatedBy = username;

              if (subject.Department.DepartmentID != model.Department.DepartmentID)
              {
                subject.DepartmentID = model.Department.DepartmentID;
              }
              // NOTE: Division is not related directly to a Subject - only through a Department

              // If no prefixes are being merged, instantiate an empty collection so LINQ queries don't fail
              if (prefixesToMerge == null)
              {
                prefixesToMerge = new List<string>();
              }

              // Only allow merging/unmerging of prefixes if at least one prefix is assigned to the subject
              if (prefixesToMerge.Count > 0)
              {
                // Unmerge subjects
                IList<SubjectsCoursePrefix> unmergables =
                  subject.CoursePrefixes.Where(s => !prefixesToMerge.Contains(s.CoursePrefixID)).ToList();
                foreach (SubjectsCoursePrefix prefix in unmergables)
                {
                  subject.CoursePrefixes.Remove(prefix);
                }

                // Merge subjects
                IList<string> currentlyMergedPrefixes = subject.CoursePrefixes.Select(s => s.CoursePrefixID).ToList();
                IEnumerable<string> mergables = prefixesToMerge.Where(m => !currentlyMergedPrefixes.Contains(m));
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
            else
            {
              _log.Warn(m => m("Unable to save Subject info: Failed to find Subject data for slug '{0}'", model.Subject.Slug));
            }
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
	  #endregion
  }
}
