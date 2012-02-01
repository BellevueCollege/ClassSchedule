using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Ctc.Ods;
using Ctc.Ods.Data;
using Ctc.Ods.Types;
using CTCClassSchedule.Common;
using CTCClassSchedule.Models;
using CTCClassSchedule.Properties;
using System;
using Ctc.Web.Security;








namespace CTCClassSchedule.Controllers
{
	public class ApiController : Controller
	{
		public ApiController()
		{
			ViewBag.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		}

		//
		// GET: /Api/Subjects
		/// <summary>
		/// Returns an array of <see cref="Course"/> Subjects
		/// </summary>
		/// <param name="format"></param>
		///<param name="YearQuarter"></param>
		///<returns>
		///		Either a <see cref="PartialViewResult"/> which can be embedded in an MVC View,
		///		or the list of <see cref="Course"/> Subjects as a JSON array.
		/// </returns>
		/// <remarks>
		///		To receive the list as a JSON array call this method with <i>format=json</i>:
		///		<example>
		///			http://localhost/Api/Subjects?format=json
		///		</example>
		/// </remarks>
		[HttpGet]
		public ActionResult Subjects(string format, string YearQuarter)
		{
			using (OdsRepository db = new OdsRepository(HttpContext))
			{
				IList<CoursePrefix> data;
				data = string.IsNullOrWhiteSpace(YearQuarter) || YearQuarter.ToUpper() == "ALL" ? db.GetCourseSubjects() : db.GetCourseSubjects(Ctc.Ods.Types.YearQuarter.FromFriendlyName(YearQuarter));

				IList<vw_ProgramInformation> progInfo;
				using (ClassScheduleDb classScheduleDb = new ClassScheduleDb())
				{
					progInfo = (from s in classScheduleDb.vw_ProgramInformation select s).ToList();
				}
				IList<ScheduleCoursePrefix> subjectList = (from p in progInfo
																									where data.Select(c => c.Subject).Contains(p.Abbreviation.TrimEnd('&'))
																									select new ScheduleCoursePrefix
																														{
																															Subject = p.URL,
																															Title = p.Title
																														})
																									.OrderBy(s => s.Title)
																									.Distinct()
																									.ToList();

				if (format == "json")
				{
					// NOTE: AllowGet exposes the potential for JSON Hijacking (see http://haacked.com/archive/2009/06/25/json-hijacking.aspx)
					// but is not an issue here because we are receiving and returning public (e.g. non-sensitive) data
					return Json(subjectList, JsonRequestBehavior.AllowGet);
				}

				ViewBag.LinkParams = Helpers.getLinkParams(Request);
				ViewBag.SubjectsColumns = 2;

				return PartialView(subjectList);
			}
		}


		//Generation of the Section Edit dialog box
		[Authorize(Roles = "Developers")]
		public ActionResult SectionEdit(string itemNumber, string yrq, string subject, string classNum)
		{
			string classID = itemNumber + yrq;

			if (HttpContext.User.Identity.IsAuthenticated == true)
			{

				using (OdsRepository respository = new OdsRepository(HttpContext))
				{
					IList<YearQuarter> yrqRange = Helpers.getYearQuarterListForMenus(respository);
					ViewBag.QuarterNavMenu = yrqRange;

					ICourseID courseID = CourseID.FromString(subject, classNum);
					IList<Section> sections;
					sections = respository.GetSections(courseID);

					Section editSection = null;
					foreach (Section section in sections)
					{
						if (section.ID.ToString() == classID)
						{
							editSection = section;
						}
					}

					sections.Clear();
					sections.Add(editSection);

					IEnumerable<SectionWithSeats> sectionsEnum;
					SectionFootnote itemToUpdate = null;
					using (ClassScheduleDb db = new ClassScheduleDb())
					{
						sectionsEnum = Helpers.getSectionsWithSeats(yrqRange[0].ID, sections, db);

						try
						{
							itemToUpdate = db.SectionFootnotes.Single(s => s.ClassID == classID);
						}
						catch
						{

						}
						var LocalSections = (from s in sectionsEnum
																 select new SectionWithSeats
																 {
																	 ParentObject = s,
																	 SectionFootnotes = itemToUpdate != null ? itemToUpdate.Footnote ?? string.Empty : string.Empty,
																	 LastUpdated = itemToUpdate != null ? itemToUpdate.LastUpdated.ToString() ?? string.Empty : string.Empty
																 }).ToList();

						return PartialView(LocalSections);
					}
				}
			}
			return PartialView();
		}




		//
		// POST after submit is clicked

		[HttpPost]
		[Authorize(Roles = "Developers")]
		public ActionResult SectionEdit(FormCollection collection)
		{
			string referrer = collection["referrer"];

			if (HttpContext.User.Identity.IsAuthenticated == true)
			{
				string ItemNumber = collection["ItemNumber"];
				string Yrq = collection["Yrq"];
				string Username = HttpContext.User.Identity.Name;
				string SectionFootnotes = collection["section.SectionFootnotes"];
				string classID = ItemNumber + Yrq;


				SectionFootnote itemToUpdate = new SectionFootnote();

				bool itemFound = false;
				if (ModelState.IsValid)
				{
					using (ClassScheduleDb db = new ClassScheduleDb())
					{
						try
						{
							itemToUpdate = db.SectionFootnotes.Single(s => s.ClassID == classID);
							itemFound = true;
						}
						catch
						{
						}

						itemToUpdate.ClassID = classID;
						itemToUpdate.Footnote = SectionFootnotes;
						itemToUpdate.LastUpdated = DateTime.Now;
						itemToUpdate.LastUpdatedBy = Username;

						if (itemFound == false)
						{
							db.AddToSectionFootnotes(itemToUpdate);
						}

						db.SaveChanges();
					}
				}


			}
			return Redirect(referrer);
		}


		//Generation of the Class Edit dialog box
		[Authorize(Roles = "Developers")]  //TODO: Make this configurable
		public ActionResult ClassEdit(string CourseNumber, string Subject, bool IsCommonCourse)
		{

			if (HttpContext.User.Identity.IsAuthenticated == true)
			{
				ICourseID courseID = CourseID.FromString(Subject, CourseNumber);
				string UpdatingCourseID = courseID.ToString(); //IsCommonCourse ? Subject + "&" + CourseNumber : Subject + " " + CourseNumber;
				CourseFootnote itemToUpdate = null;
				var HPFootnotes = "";
				using (ClassScheduleDb db = new ClassScheduleDb())
				{
					try
					{
						itemToUpdate = db.CourseFootnotes.Single(s => s.CourseID == UpdatingCourseID);
						//var footnotesHP  = db.
					}
					catch
					{

					}

					using (OdsRepository respository = new OdsRepository(HttpContext))
					{
						var QuarterNavMenu = Helpers.getYearQuarterListForMenus(respository);
						Course coursesEnum = new Course();
						try
						{
							coursesEnum = respository.GetCourses().Single(s => s.CourseID == UpdatingCourseID);
							foreach (CourseDescription footnote in coursesEnum.Descriptions)
							{
								HPFootnotes += footnote.Description + " ";
							}
						}
						catch
						{

						}


					}

					ClassFootnote LocalClass = new ClassFootnote();
					LocalClass.CourseID = itemToUpdate != null ? itemToUpdate.CourseID : "";
					LocalClass.Footnote = itemToUpdate != null ? itemToUpdate.Footnote : "";
					LocalClass.HPFootnote = HPFootnotes;
					LocalClass.LastUpdated = itemToUpdate != null ? Convert.ToString(itemToUpdate.LastUpdated) : "";
					LocalClass.LastUpdatedBy = itemToUpdate != null ? itemToUpdate.LastUpdatedBy : "";




					return PartialView(LocalClass);
				}
			}

			return PartialView();
		}






		//
		// POST after submit is clicked

		[HttpPost]
		[Authorize(Roles = "Developers")]  //TODO: Make this configurable
		public ActionResult ClassEdit(FormCollection collection)
		{
			string referrer = collection["referrer"];

			if (HttpContext.User.Identity.IsAuthenticated == true)
			{
				string CourseID = collection["CourseID"];
				string Username = HttpContext.User.Identity.Name;
				string Footnote = collection["Footnote"];




				CourseFootnote itemToUpdate = new CourseFootnote();
				bool itemFound = false;
				if (ModelState.IsValid)
				{
					using (ClassScheduleDb db = new ClassScheduleDb())
					{
						try
						{
							itemToUpdate = db.CourseFootnotes.Single(s => s.CourseID == CourseID);

							itemFound = true;
						}
						catch
						{
						}

						itemToUpdate.CourseID = CourseID;
						itemToUpdate.Footnote = Footnote;
						itemToUpdate.LastUpdated = DateTime.Now;
						itemToUpdate.LastUpdatedBy = Username;

						if (itemFound == false)
						{
							db.AddToCourseFootnotes(itemToUpdate);
						}

						db.SaveChanges();
					}
				}
			}

			return Redirect(referrer);
		}



		//Generation of the Program Edit dialog box
		[Authorize(Roles = "Developers")]  //TODO: Make this configurable
		public ActionResult ProgramEdit(string Subject)
		{

			if (HttpContext.User.Identity.IsAuthenticated == true)
			{
				//ProgramInformation itemToUpdate = new ProgramInformation();
				using (ClassScheduleDb db = new ClassScheduleDb())
				{
					try
					{
						var itemToUpdate = db.ProgramInformations.Single(s => s.URL == Subject);
						return PartialView(itemToUpdate);
					}
					catch
					{

					}


				}
			}

			return PartialView();
		}






		//
		// POST after submit is clicked

		[HttpPost]
		[Authorize(Roles = "Developers")]  //TODO: Make this configurable
		public ActionResult ProgramEdit(FormCollection collection)
		{
			string referrer = collection["referrer"];

			if (HttpContext.User.Identity.IsAuthenticated == true)
			{

				string Username = HttpContext.User.Identity.Name;


				string DivisionURL = collection["DivisionURL"];
				string Division = collection["Division"];
				string ProgramURL = collection["ProgramURL"];
				string AcademicProgram = collection["AcademicProgram"];
				string Intro = collection["Intro"];
				string Title = collection["Title"];
				string Abbreviation	 = collection["Abbreviation"];
				string URL = collection["URL"];


				ProgramInformation itemToUpdate = new ProgramInformation();
				bool itemFound = false;
				if (ModelState.IsValid)
				{
					using (ClassScheduleDb db = new ClassScheduleDb())
					{
						try
						{
							itemToUpdate = db.ProgramInformations.Single(s => s.URL == URL);

							itemFound = true;
						}
						catch
						{
						}

						itemToUpdate.LastUpdated = DateTime.Now;
						itemToUpdate.LastUpdatedBy = Username;
						itemToUpdate.DivisionURL = DivisionURL;
						itemToUpdate.Division = Division;
						itemToUpdate.ProgramURL = ProgramURL;
						itemToUpdate.AcademicProgram = AcademicProgram;
						itemToUpdate.Intro = Intro;
						itemToUpdate.Title = Title;
						itemToUpdate.Abbreviation = Abbreviation;
						itemToUpdate.URL = URL;

						//add the item to the database if it doesn't exist.
						if (itemFound == false)
						{
							db.AddToProgramInformations(itemToUpdate);
						}

						//save to the db (whether item existed or not)
						db.SaveChanges();
					}
				}
			}

			return Redirect(referrer);
		}





	}
}
