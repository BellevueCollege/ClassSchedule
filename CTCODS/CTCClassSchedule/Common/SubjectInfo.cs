using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CTCClassSchedule.Models;
using System.Text.RegularExpressions;

namespace CTCClassSchedule.Common
{
	// TODO:
	// (1) Add GetSubjectsFromPrefix methods accepting either a CoursePrefix, SubjectCoursePrefix, ScheduleCoursePrefix, or String?
	//		 Should return a list of Subjects which are using the specified Prefix
	// (2) Add summary documentation to class and members
	// (3) Include error handling if the subject is not found
	public static class SubjectInfo
	{
		public static Subject GetSubject(string slug)
		{
			using (ClassScheduleDb context = new ClassScheduleDb())
			{
				Subject subject = context.Subjects.Where(s => s.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
				return subject;
			}
		}
		public static Subject GetSubject(string slug, ClassScheduleDb context)
		{
			Subject subject = context.Subjects.Where(s => s.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
			return subject;
		}

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
				result.Department = subject.Department;
				result.Division = result.Department.Division;
				result.SubjectCoursePrefixes = subject.SubjectsCoursePrefixes.ToList();

				// If the url is a fully qualified url (e.g. http://continuinged.bellevuecollege.edu/about)
				// or empty just return it, otherwise prepend iwth the current school url.
				string deptUrl = result.Department.URL ?? string.Empty;
				if (!string.IsNullOrWhiteSpace(deptUrl) && !Regex.IsMatch(deptUrl, @"^https?://"))
				{
					result.Department.URL = deptUrl;
				}
			}

			return result;
		}

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