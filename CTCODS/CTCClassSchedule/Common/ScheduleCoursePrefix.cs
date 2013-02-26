using System;
using Ctc.Ods.Types;

namespace CTCClassSchedule.Common
{
	public class ScheduleCoursePrefix : ICoursePrefix, IEquatable<ScheduleCoursePrefix>
	{
		/// <summary>
		/// The five-character abbreviation which identifies a course of study (e.g. ENGL)
		/// </summary>
		/// <seealso cref="ICoursePrefix.Title"/>
		public int Subject { get; set; }

		/// <summary>
		/// The full name of a course of study (e.g. English)
		/// </summary>
		/// <seealso cref="ICoursePrefix.Subject"/>
		public string Title { get;set;}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(ScheduleCoursePrefix other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}
			return Equals(other.Subject, Subject) && Equals(other.Title, Title);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(ICoursePrefix other)
		{
			return Equals(other as ScheduleCoursePrefix);
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj.GetType() != typeof(ScheduleCoursePrefix))
			{
				return false;
			}
			return Equals((ScheduleCoursePrefix)obj);
		}

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode()
		{
			unchecked
			{
				return ((Subject != null ? Subject.GetHashCode() : 0) * 397) ^ (Title != null ? Title.GetHashCode() : 0);
			}
		}

		public static bool operator ==(ScheduleCoursePrefix left, ScheduleCoursePrefix right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(ScheduleCoursePrefix left, ScheduleCoursePrefix right)
		{
			return !Equals(left, right);
		}
	}
}