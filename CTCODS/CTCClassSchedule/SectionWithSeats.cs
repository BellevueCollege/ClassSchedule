using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ctc.Ods.Types;
using Ctc.Ods.Data;

namespace CTCClassSchedule
{
	public class SectionWithSeats : Section
	{
		public int? SeatsAvailable { get; set; }
		public int  WaitlistCount { get; set; }
		public string LastUpdated { get; set; }
	}
}