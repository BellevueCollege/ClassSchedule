using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using Ctc.Ods.Data;
using Ctc.Ods.Types;
using Ctc.Ods;



namespace CTCClassSchedule.Controllers
{
	public class ClassesController : Controller
	{
		//
		// GET: /Classes/

		[ValidateInput(false)]
		public ActionResult Index(string letter)
		{

			setViewBagVars("", "", "", "", "", letter);
			ViewBag.AlphabetArray = new bool[26];

			using (OdsRepository respository = new OdsRepository())
			{
				IList<CoursePrefix> courses = respository.GetCourseSubjects();
				IEnumerable<String> alphabet;


				ViewBag.WhichClasses = (letter == null ? "All" : letter.ToUpper());

				//capitalize all first letters of words in title
				foreach (CoursePrefix course in courses)
				{
					course.Title = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(course.Title.ToLower());
				}

				alphabet = from c in courses
									 orderby c.Title
									 select c.Title.Substring(0, 1);

				alphabet = alphabet.Distinct();
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

		[ValidateInput(false)]
		public ActionResult All(string Subject)
		{
			return View();
		}

		[ValidateInput(false)]
		public ActionResult Subject(string Subject)
		{
			ViewBag.Subject = Subject;


			using (OdsRepository respository = new OdsRepository())
			{
				IList<Course> courses = respository.GetCourses();


				if (Subject != null)
				{
					IEnumerable<Course> coursesEnum;
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

		[ValidateInput(false)]
		public ActionResult YearQuarter(string YearQuarter, string flex, string time, string days, string avail, string letter)
		{

			setViewBagVars(YearQuarter, flex, time, days, avail, letter);
			ViewBag.AlphabetArray = new bool[26];

			using (OdsRepository respository = new OdsRepository())
			{
				IList<CoursePrefix> courses = respository.GetCourseSubjects();
				IEnumerable<String> alphabet;
				ViewBag.AlphabetArray = new bool[26];

				ViewBag.WhichClasses = (letter == null ? "All" : letter.ToUpper());

				//capitalize all first letters of words in title
				foreach (CoursePrefix course in courses)
				{
					course.Title = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(course.Title.ToLower());
				}

				alphabet = from c in courses
									 orderby c.Title
									 select c.Title.Substring(0, 1);

				alphabet = alphabet.Distinct();
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


		[ValidateInput(false)]
		public ActionResult YearQuarterSubject(String YearQuarter, string Subject, string flex, string time, string days, string avail)
		{
			ViewBag.YearQuarter = YearQuarter;
			ViewBag.Subject = Subject;
			return View();
		}

		[ValidateInput(false)]
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
		[ValidateInput(false)]
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

	private void setViewBagVars(string YearQuarter, string flex, string time, string days, string avail, string letter)
		{
			ViewBag.ErrorMsg = "";

			ViewBag.YearQuarter = YearQuarter;
			if (YearQuarter != "")
			{
				ViewBag.YearQuarterHP = getYRQFromFriendlyDate(YearQuarter);

			}

			ViewBag.YearQuarter_a_to_z = "/" + YearQuarter;
			ViewBag.letter = letter;
			ViewBag.YearQuarter = YearQuarter ?? "all";
			ViewBag.flex = flex ?? "all";
			ViewBag.time = time ?? "all";
			ViewBag.days = days ?? "all";
			ViewBag.avail = avail ?? "all";


			ViewBag.activeClass = " class=active";
			ViewBag.currentUrl = Request.Url.AbsolutePath;


			//create the GET string for links
			ViewBag.queryStringNoFlex = "&time=" + ViewBag.time + "&days=" + ViewBag.days + "&avail=" + ViewBag.avail + "&letter=" + letter;
			ViewBag.queryStringNoTimes = "&flex=" + ViewBag.flex + "&days=" + ViewBag.days + "&avail=" + ViewBag.avail + "&letter=" + letter;
			ViewBag.queryStringNoDays = "&flex=" + ViewBag.flex + "&time=" + ViewBag.time + "&avail=" + ViewBag.avail + "&letter=" + letter;
			ViewBag.queryStringNoAvail = "&flex=" + ViewBag.flex + "&time=" + ViewBag.time + "&days=" + ViewBag.days + "&letter=" + letter;
			ViewBag.queryStringNoLetter = "&flex=" + ViewBag.flex + "&time=" + ViewBag.time + "&days=" + ViewBag.days + "&avail=" + avail;
			ViewBag.queryStringAll = "&flex=" + ViewBag.flex + "&time=" + ViewBag.time + "&days=" + ViewBag.days + "&avail=" + ViewBag.avail + "&letter=" + letter;


			ViewBag.ActiveFlexAll = "";
			ViewBag.ActiveFlexOnline = "";
			ViewBag.ActiveFlexHybrid = "";
			ViewBag.ActiveFlexTelecourse = "";
			ViewBag.ActiveFlexReducedTrip = "";
			ViewBag.ActiveFlexAll = "";
		}

	private string getYRQFromFriendlyDate(string friendlyDate)
	{
		//Example: Winter 2008 = A783

		//summer: xxx1
		//fall:   xxx2
		//winter: xxx3
		//spring:	xxx4

		//Academic year 2006-2007: x67x
		//Academic year 2011-2012: x12x

		string year = friendlyDate.Substring(friendlyDate.Length - 4);  //2011
		string quarterFriendly = friendlyDate.Substring(0, friendlyDate.Length - 4); //Spring
		string decade = "";
		int yearBaseTen = 0;
		string YearQuarter1 = "";
		string YearQuarter23 = "";
		string quarter = "";
		bool isLastTwoQuarters = false;
		bool badlyFormedQuarter = false;
		bool badlyFormedYear = false;

		if (IsInteger(year))
		{
			if (Convert.ToInt16(year) < 1975 || Convert.ToInt16(year) > 2030)
			{
				badlyFormedYear = true;
			}
			else
			{
				 yearBaseTen = Convert.ToInt16(year.Substring(3, 1));
				 decade = year.Substring(0, 3); //201
			}
		}

		//determine the quarter in string form "1", "2", etc... essentially the xxx2 character
		switch (quarterFriendly.ToLower())
		{
			case "summer":
				quarter = "1";
				break;
			case "fall":
				quarter = "2";
				break;
			case "winter":
				quarter = "3";
				break;
			case "spring":
				quarter = "4";
				break;
			default:
				badlyFormedQuarter = true;
				break;

		}

		if(!badlyFormedQuarter && !badlyFormedYear) {




			//is the year an overlapping year? e.g. spring 2000 = 9904 but summer 2000 = A011
			if (yearBaseTen == 0 && (quarter == "3" || quarter == "4"))
			{
				isLastTwoQuarters = true;
			}

			//find out which decade it is in, to determine Axxx (first character in string)
			switch (decade)
			{
				case "195":
					YearQuarter1 = isLastTwoQuarters == true ? "6" : "5";
					break;
				case "196":
					YearQuarter1 = isLastTwoQuarters == true ? "7" : "6";
					break;
				case "197":
					YearQuarter1 = isLastTwoQuarters == true ? "8" : "7";
					break;
				case "198":
					YearQuarter1 = isLastTwoQuarters == true ? "9" : "8";
					break;
				case "199":
					YearQuarter1 = isLastTwoQuarters == true ? "A" : "9";
					break;
				case "200":
					YearQuarter1 = isLastTwoQuarters == true ? "B" : "A";
					break;
				case "201":
					YearQuarter1 = isLastTwoQuarters == true ? "C" : "B";
					break;


			}

			//figure out what the x23x portion of the YRQ is
			if(quarter == "1" || quarter == "2")
			{
				YearQuarter23 = Convert.ToString(yearBaseTen) + Convert.ToString((yearBaseTen + 1));

			}
			else if (quarter == "3" || quarter == "4")
			{
				int tempYearBaseTen;
				if (yearBaseTen == 0)
				{
					tempYearBaseTen = 10;
				}
				else
				{
					tempYearBaseTen = yearBaseTen;
				}

				YearQuarter23 = Convert.ToString(tempYearBaseTen-1) + Convert.ToString((yearBaseTen));
			}

			return YearQuarter1 + YearQuarter23 + quarter;

		}

		if (badlyFormedYear == true)
		{
			ViewBag.ErrorMsg = ViewBag.ErrorMsg + "<li>Badly formed year, please enter a new year in the URL in the format 'Quarter2011'</li>";
		}

		if (badlyFormedQuarter == true)
		{
			ViewBag.ErrorMsg = ViewBag.ErrorMsg + "<li>Badly formed quarter, please enter a new Quarter in the URL in the format 'Fall20XX'</li>";
		}

		return "Z999";



	}


	public static bool IsInteger(string value)
	{
		try
		{
			Convert.ToInt32(value);
			return true;
		}
		catch
		{
			return false;
		}

	}




	}
}
