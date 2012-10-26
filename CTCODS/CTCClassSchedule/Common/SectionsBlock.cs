using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ctc.Ods.Types;

namespace CTCClassSchedule
{
	public class SectionsBlock
	{
		/// <summary>
		/// Collection of all sections of the same course, grouped into a block
		/// </summary>
		public IEnumerable<SectionWithSeats> Sections { get; set; }

		/// <summary>
		/// Collection of all linked sections.
		/// </summary>
		public IEnumerable<SectionWithSeats> LinkedSections { get; set; }

		/// <summary>
		/// Collection of footnotes shared by all sections of the block
		/// </summary>
		public IEnumerable<string> CommonFootnotes { get; set; }
	}
}