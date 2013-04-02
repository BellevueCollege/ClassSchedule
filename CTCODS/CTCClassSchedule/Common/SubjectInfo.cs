using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CTCClassSchedule.Models;
using System.Text.RegularExpressions;
using Common.Logging;

namespace CTCClassSchedule.Common
{
	/// <summary>
	/// Static helper class to return data related to a given subject.
	/// </summary>
	/// <remarks>
	/// These helper methods handle common pitfalls such as <see cref="NullReferenceException"/>s for entities which do not exist.
	/// </remarks>
	public static class SubjectInfo
	{
	  private static ILog _log = LogManager.GetCurrentClassLogger();

	  /// <summary>
		/// Finds a <see cref="Subject"/> based on its <paramref name="slug"/> identifier
		/// </summary>
		/// <param name="slug">The slug identifier for a given <see cref="Subject"/></param>
		/// <returns><see cref="Subject"/> if found, otherwise <see langword="null" /></returns>
		public static Subject GetSubject(string slug)
		{
			using (ClassScheduleDb context = new ClassScheduleDb())
			{
				Subject subject = context.Subjects.Where(s => s.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
				return subject;
			}
		}

		/// <summary>
		/// Finds a <see cref="Subject"/> based on its <paramref name="slug"/> identifier and returns it using the provided <see cref="ClassScheduleDb"/>
		/// </summary>
		/// <param name="slug">The slug identifier for a given <see cref="Subject"/></param>
		/// <param name="context">The database context to use during the lookup</param>
		/// <returns><see cref="Subject"/> if found, otherwise null</returns>
		public static Subject GetSubject(string slug, ClassScheduleDb context)
		{
			Subject subject = context.Subjects.Where(s => s.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
			return subject;
		}

    /// <summary>
    /// Takes a course <paramref name="prefix"/> parameter and returns all <see cref="Subject"/>s which own the prefix <see cref="Subject"/> entity.
    /// </summary>
    /// <param name="prefix"></param>
    /// <returns>The a list of the matching <see cref="Subject"/> entities</returns>
    public static Subject GetSubjectFromPrefix(string prefix)
    {
      Subject result = null;
      using (ClassScheduleDb db = new ClassScheduleDb())
      {
        // The CoursePrefixID is the primary key, so we know there will not be more than one result
        result = db.SubjectsCoursePrefixes.Where(s => s.CoursePrefixID.Equals(prefix, StringComparison.OrdinalIgnoreCase))
                                           .Select(s => s.Subject).FirstOrDefault();
      }

      if (result == null)
      {
        _log.Warn(m => m("Failed to retrieve Subject record for PrefixID '{0}'", prefix));
        return result;
      }

      return result;
    }

		/// <summary>
		/// Gathers a <see cref="Subject"/>'s related <see cref="Department"/>, <see cref="Division"/>, and <see cref="SubjectsCoursePrefix"/>s
		/// </summary>
		/// <param name="slug">The slug identifier for a given <see cref="Subject"/></param>
		/// <returns>An instance of <see cref="SubjectInfoResult"/> containing data related to the <see cref="Subject"/>, or null if the subject was not found.</returns>
		public static SubjectInfoResult GetSubjectInfo(string slug)
		{
			SubjectInfoResult result = null;
			using (ClassScheduleDb context = new ClassScheduleDb())
			{
				Subject subject = GetSubject(slug, context);
				if (subject == null)
				{
					return result;
				}

        result = new SubjectInfoResult
        {
          Subject = subject,
          CoursePrefixes = subject.CoursePrefixes.ToList(),
          Department = subject.Department ?? new Department(),
        };
        result.Division = result.Department.Division ?? new Division();

        // If the url is a fully qualified url (e.g. http://continuinged.bellevuecollege.edu/about)
        // or empty just return it, otherwise prepend with the current school url.
        string deptUrl = result.Department.URL ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(deptUrl) && !Regex.IsMatch(deptUrl, @"^https?://"))
        {
          result.Department.URL = deptUrl;
        }
			}

			return result;
		}

		/// <summary>
		/// Gathers a <see cref="Subject"/>'s related <see cref="Department"/>, <see cref="Division"/>, and <see cref="SubjectsCoursePrefix"/>s
		/// </summary>
		/// <param name="prefix">The prefix identifier for a given <see cref="Subject"/></param>
		/// <returns>An instance of <see cref="SubjectInfoResult"/> containing data related to the <see cref="Subject"/>, or null if the subject was not found.</returns>
		public static SubjectInfoResult GetSubjectInfoFromPrefix(string prefix)
		{
			SubjectInfoResult result = null;
			using (ClassScheduleDb context = new ClassScheduleDb())
			{
			  Subject subject = (from s in context.Subjects
			                    join p in context.SubjectsCoursePrefixes on s.SubjectID equals p.SubjectID into j
			                    from sub in j
			                    where sub.CoursePrefixID == prefix
			                    select s).FirstOrDefault();

				if (subject == null)
				{
          _log.Warn(m => m("Failed to retrieve Subject record for PrefixID '{0}'", prefix));
					return result;
				}

        result = new SubjectInfoResult
        {
          Subject = subject,
          CoursePrefixes = subject.CoursePrefixes.ToList(),
          Department = subject.Department ?? new Department(),
        };
        result.Division = result.Department.Division ?? new Division();

        // If the url is a fully qualified url (e.g. http://continuinged.bellevuecollege.edu/about)
        // or empty just return it, otherwise prepend with the current school url.
        string deptUrl = result.Department.URL ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(deptUrl) && !Regex.IsMatch(deptUrl, @"^https?://"))
        {
          result.Department.URL = deptUrl;
        }
			}

			return result;
		}

		/// <summary>
		/// Gathers a List of <see cref="SubjectsCoursePrefix"/> based on the <see cref="Subject"/>'s <paramref name="slug"/> identifier
		/// </summary>
		/// <param name="slug">The slug identifier for a given <see cref="Subject"/></param>
		/// <returns>A List of all <see cref="SubjectsCoursePrefix"/>s located for the given <see cref="Subject"/></returns>
		public static IList<SubjectsCoursePrefix> GetSubjectPrefixes(string slug)
		{
			IList<SubjectsCoursePrefix> result = new List<SubjectsCoursePrefix>();
			using (ClassScheduleDb context = new ClassScheduleDb())
			{
				Subject subject = GetSubject(slug, context);
				if (subject == null)
				{
					return result;
				}

        result = subject.CoursePrefixes.ToList();
			}

			return result;
		}
	}
}