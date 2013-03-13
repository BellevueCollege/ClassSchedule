using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CTCClassSchedule.Models;
using System.Text.RegularExpressions;

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
    /// Takes a course <paramref name="prefix"/> parameter and attempts to return a single matching <see cref="Subject"/> entity.
    /// </summary>
    /// <remarks>This method is not absolutely secure. The database schema says that prefixes-to-subjects is a many-to-many relationship,
    /// so theoretically a single <paramref name="prefix"/> could return one or more <see cref="Subject"/>s. This makes it impossible to
    /// determine which <see cref="Subject"/>s a course belongs to based solely on course prefixes. However, realistically no course prefix
    /// is (currently) merged with more than a single <see cref="Subject"/>. Therefore, we can abuse the fact that in reality, there is
    /// effectively a many-to-one relationship between prefixes and <see cref="Subject"/>s.
    /// TODO: In the future we may want to modify the database schema to make this many-to-one relationship explicit rather than implied.</remarks>
    /// <param name="prefix"></param>
    /// <returns>The matching <see cref="Subject"/> entity, or null if no match was found</returns>
    /// <exception cref="Exception">Throws a general exception if more than one subject is matched to the specified <paramref name="prefix"/></exception>
    public static Subject GetSubjectFromPrefix(string prefix)
    {
      Subject subject = null;
      using (ClassScheduleDb db = new ClassScheduleDb())
      {
        IList<Subject> matches = db.SubjectsCoursePrefixes.Where(s => s.CoursePrefixID.Equals(prefix, StringComparison.OrdinalIgnoreCase))
                                                          .Select(s => s.Subject).ToList();
        if (matches.Count > 1)
        {
          string subjectList = matches.Select(s => s.Title).Aggregate((i, j) => i + ", " + j);
          throw new Exception(String.Concat("The course prefix ", prefix, " belongs to more than one subject: ", subjectList, "."));
        }
        subject = matches.FirstOrDefault();
      }

      return subject;
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
          SubjectCoursePrefixes = subject.SubjectsCoursePrefixes.ToList(),
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

				result = subject.SubjectsCoursePrefixes.ToList();
			}

			return result;
		}
	}
}