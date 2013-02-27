using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CTCClassSchedule.Models;

namespace CTCClassSchedule.Common
{
	// TODO:
	// (1) Add GetSubjectsFromPrefix methods accepting either a CoursePrefix, SubjectCoursePrefix, ScheduleCoursePrefix, or String?
	//		 Should return a list of Subjects which are using specified Prefix
	// (2) Add summary documentation to class and members
	// (3) Include error handling if the subject is not found
	public static class SubjectInfo
	{
		#region GetSubject
		public static Subject GetSubject(int subjectId, ClassScheduleDb context = null)
		{
			using (context = context ?? new ClassScheduleDb())
			{
				Subject subject = context.Subjects.Where(s => s.SubjectID == subjectId).FirstOrDefault();
				return subject;
			}
		}
		public static Subject GetSubject(string slug, ClassScheduleDb context = null)
		{
			using (context = context ?? new ClassScheduleDb())
			{
				Subject subject = context.Subjects.Where(s => s.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
				return subject;
			}
		}
		#endregion

		#region GetSubjectInfo
		public static SubjectInfoResult GetSubjectInfo(int subjectId, ClassScheduleDb context = null)
		{
			using (context = context ?? new ClassScheduleDb())
			{
				Subject subject = GetSubject(subjectId, context);
				return GetSubjectInfo(subject, context);
			}
		}
		public static SubjectInfoResult GetSubjectInfo(string slug, ClassScheduleDb context = null)
		{
			using (context = context ?? new ClassScheduleDb())
			{
				Subject subject = GetSubject(slug, context);
				return GetSubjectInfo(subject, context);
			}
		}

		private static SubjectInfoResult GetSubjectInfo(Subject subject, ClassScheduleDb context)
		{
			SubjectInfoResult result = null;
			if (subject == null)
			{
				return result;
			}

			result = new SubjectInfoResult();
			result.Subject = subject;
			//result.SubjectCoursePrefixes = subject.SubjectsCoursePrefixes.ToList();
			result.Department = subject.Departments.First(); // TODO: Subjects-to-Departments should be a Many-To-One relationship.
																											 // IE: A Department can have many subjects, but a Subject should only belong to a single Department
			result.Division = result.Department.Division;

			return result;
		}
		#endregion

		#region GetSubjectPrefixes
		public static IList<SubjectsCoursePrefix> GetSubjectPrefixes(int subjectId, ClassScheduleDb context = null)
		{
			using (context = context ?? new ClassScheduleDb())
			{
				Subject subject = GetSubject(subjectId, context);
				return GetSubjectPrefixes(subject, context);
			}
		}

		public static IList<SubjectsCoursePrefix> GetSubjectPrefixes(string slug, ClassScheduleDb context = null)
		{
			using (context = context ?? new ClassScheduleDb())
			{
				Subject subject = GetSubject(slug, context);
				return GetSubjectPrefixes(subject, context);
			}
		}

		private static IList<SubjectsCoursePrefix> GetSubjectPrefixes(Subject subject, ClassScheduleDb context)
		{
			IList<SubjectsCoursePrefix> result = new List<SubjectsCoursePrefix>();
			if (subject == null)
			{
				return result;
			}

			result = subject.SubjectsCoursePrefixes.ToList();

			return result;
		}
		#endregion
	}


	public class SubjectInfoResult
	{
		public Subject Subject { get; set; }
		//public IList<SubjectsCoursePrefix> SubjectCoursePrefixes { get; set; } // TODO: Is it worthwile to provide this? Do we need it anywhere?
		public Department Department { get; set; }
		public Division Division { get; set; }
	}
}