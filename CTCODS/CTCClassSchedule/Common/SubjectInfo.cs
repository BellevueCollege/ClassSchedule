using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CTCClassSchedule.Models;
using System.Text.RegularExpressions;

namespace CTCClassSchedule.Common
{
	// TODO: Add GetSubjectsFromPrefix methods accepting either a CoursePrefix, SubjectCoursePrefix, ScheduleCoursePrefix, or String?
	//		   Should return a list of Subjects which are using the specified Prefix

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

				result = new SubjectInfoResult();
				result.Subject = subject;
				result.SubjectCoursePrefixes = subject.SubjectsCoursePrefixes.ToList();
				result.Department = subject.Department;

				if (result.Department != null)
				{
					// If the url is a fully qualified url (e.g. http://continuinged.bellevuecollege.edu/about)
					// or empty just return it, otherwise prepend with the current school url.
					string deptUrl = result.Department.URL ?? string.Empty;
					if (!string.IsNullOrWhiteSpace(deptUrl) && !Regex.IsMatch(deptUrl, @"^https?://"))
					{
						result.Department.URL = deptUrl;
					}

					result.Division = result.Department.Division;
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