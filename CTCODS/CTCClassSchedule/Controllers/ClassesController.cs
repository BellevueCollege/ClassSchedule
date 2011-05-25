using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Ctc.Ods;
using Ctc.Ods.Data;
using Ctc.Ods.Types;



namespace CTCClassSchedule.Controllers
{
	public class ClassesController : Controller
	{
		//
		// GET: /Classes/

		public ActionResult Index(string letter)
		{

			//ViewBag.AlphabetArray =



			ViewBag.i = 0;
			using (OdsRepository respository = new OdsRepository())
			{
				IList<CoursePrefix> courses = respository.GetCourseSubjects();
				List<char> alphabet = new List<char>();
				ViewBag.AlphabetArray = new bool[26];

				ViewBag.WhichClasses = (letter == null ? "All" : letter.ToUpper());

				//capitalize all first letters of words in title
				foreach (CoursePrefix course in courses)
				{
					var tempChar = Convert.ToChar(course.Title.Substring(0,1));
					alphabet.Add(tempChar);

					course.Title = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(course.Title.ToLower());
				}


				ViewBag.Alphabet = alphabet;

				if (letter != null)
				{
					IEnumerable<CoursePrefix> coursesEnum;
					coursesEnum = from c in courses
												where c.Title.StartsWith(letter, StringComparison.OrdinalIgnoreCase)
												select c;


					return View(coursesEnum);
				}
				else
				{
					View(courses);

				}
				//build the string to tell the user what letter they clicked on


			}
			return View();

		}

		public ActionResult All(string Subject)
		{
			return View();
		}


		public ActionResult Subject(string Subject)
		{
			ViewBag.Subject = Subject;


			using (OdsRepository respository = new OdsRepository())
			{
				IList<ICourse> courses = respository.GetCourses();


				if (Subject != null)
				{
					IEnumerable<ICourse> coursesEnum;
					coursesEnum = from c in courses
												where c.Subject == Subject
												select c;

					return View(coursesEnum);
				}
				else
				{
					View();

				}



			}
			return View();

		}

		public ActionResult YearQuarter(string YearQuarter, string flex, string time, string days, string avail, string letter)
		{
			ViewBag.YearQuarter = YearQuarter;
			ViewBag.YearQuarter_a_to_z = "/" + YearQuarter;
			ViewBag.letter = letter;
			ViewBag.YearQuarter = YearQuarter;
			ViewBag.flex = flex;

			ViewBag.AlphabetArray = new bool[26];
			ViewBag.i = 0;
			using (OdsRepository respository = new OdsRepository())
			{
				IList<CoursePrefix> courses = respository.GetCourseSubjects();
				ViewBag.WhichClasses = (letter == null ? "All" : letter.ToUpper());


				//capitalize all first letters of words in title
				foreach (CoursePrefix course in courses)
				{
					course.Title = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(course.Title.ToLower());
				}


				if (letter != null)
				{
					IEnumerable<CoursePrefix> coursesEnum;
					coursesEnum = from c in courses
												where c.Title.StartsWith(letter, StringComparison.OrdinalIgnoreCase)
												select c;

					return View(coursesEnum);
				}
				else
				{
					View(courses);

				}
				//build the string to tell the user what letter they clicked on


			}
			return View();



		}

		public ActionResult YearQuarterSubject(String YearQuarter, string Subject, string flex, string time, string days, string avail)
		{
			ViewBag.YearQuarter = YearQuarter;
			ViewBag.Subject = Subject;
			return View();
		}


		public ActionResult YRQClassDetails(string YearQuarterID, string Subject, string ClassNum)
		{

			string courseID = string.Concat(Subject, string.Concat(" ", ClassNum));
			//ViewBag.ClassTitle = courseID;




			using (OdsRepository respository = new OdsRepository())
			{

				if (courseID != null)
				{
					IList<Section> sections = respository.GetSections(CourseID.FromString(courseID), YearQuarterID);

					return View(sections);
				}
				else
				{
					return View();

				}



			}
			return View();
		}



		//
		// GET: /Classes/All/{Subject}/{ClassNum}

		public ActionResult ClassDetails(string YearQuarterID, string Subject, string ClassNum)
		{
			string courseID = string.Concat(Subject, string.Concat(" ", ClassNum));
			//ViewBag.ClassTitle = courseID;




			using (OdsRepository respository = new OdsRepository())
			{

				if (courseID != null)
				{
					if (YearQuarterID != "All")
					{
						IList<Section> sections = respository.GetSections(CourseID.FromString(courseID), YearQuarterID);
						return View(sections);
					}
					else
					{
						IList<Section> sections = respository.GetSections(CourseID.FromString(courseID));
						return View(sections);
					}



				}
				else
				{
					return View();

				}



			}
			return View();

		}


	}
}
