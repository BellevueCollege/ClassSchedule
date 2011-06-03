using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using Ctc.Ods.Data;
using Ctc.Ods.Types;
using Ctc.Ods;
using System.Text;
using System.Net;
using System.IO;



namespace CTCClassSchedule.Controllers
{
	public class ClassesController : Controller
	{
		//
		// GET: /Classes/


		public ActionResult Index(string letter)
		{

			setViewBagVars("", "", "", "", "", letter);
			ViewBag.WhichClasses = (letter == null ? "All" : letter.ToUpper());
			ViewBag.AlphabetArray = new bool[26];
			ViewBag.AlphabetCharacter = 0;



			using (OdsRepository respository = new OdsRepository())
			{
				IList<CoursePrefix> courses = respository.GetCourseSubjects();
				IEnumerable<String> alphabet;

				ViewBag.AlphabetArray = new bool[26];
				ViewBag.WhichClasses = (letter == null ? "All" : letter.ToUpper());

				//capitalize all first letters of words in title
				foreach (CoursePrefix course in courses)
				{
					//course.Title = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(course.Title.ToLower());
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


		public ActionResult YearQuarter(string YearQuarter, string flex, string time, string days, string avail, string letter)
		{
			ViewBag.WhichClasses = (letter == null ? "All" : letter.ToUpper());
			setViewBagVars(YearQuarter, flex, time, days, avail, letter);
			ViewBag.AlphabetArray = new bool[26];
			ViewBag.AlphabetCharacter = 0;
			ViewBag.YRQ = Ctc.Ods.Types.YearQuarter.FromString(getYRQFromFriendlyDate(YearQuarter)).ToString();
			ViewBag.FriendlyYRQ = getFriendlyDateFromYRQ(Ctc.Ods.Types.YearQuarter.FromString(getYRQFromFriendlyDate(YearQuarter)));

			using (OdsRepository respository = new OdsRepository())
			{
				YearQuarter YRQ = Ctc.Ods.Types.YearQuarter.FromString(getYRQFromFriendlyDate(YearQuarter));

				IList<CoursePrefix> courses = respository.GetCourseSubjects(YRQ);
				IEnumerable<String> alphabet;
				ViewBag.AlphabetArray = new bool[26];



				//capitalize all first letters of words in title
				foreach (CoursePrefix course in courses)
				{
					//course.Title = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(course.Title.ToLower());
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

					coursesEnum = coursesEnum.Distinct();

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
			setViewBagVars(YearQuarter, flex, time, days, avail, "");
			ViewBag.displayedCourseNum = 0;
			ViewBag.Title = ViewBag.Subject + "classes for " + @ViewBag.Yearquarter;

			using (OdsRepository respository = new OdsRepository())
			{
				YearQuarter YRQ = Ctc.Ods.Types.YearQuarter.FromString(getYRQFromFriendlyDate(YearQuarter));
				IList<Section> sections = respository.GetSections(Subject, YRQ);



				//capitalize all first letters of words in title
				foreach (Section section in sections)
				{
					section.CourseTitle = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(section.CourseTitle.ToLower());
				}

				IEnumerable<Section> sectionsEnum;
				sectionsEnum = from c in sections
											where c.CourseSubject == Subject.ToUpper()
											select c;

				return View(sectionsEnum);


			}
			return View();



		}


		public ActionResult YRQClassDetails(string YearQuarterID, string Subject, string ClassNum)
		{

			string courseID = string.Concat(Subject, string.Concat(" ", ClassNum));
			ViewBag.displayedCourseNum = 0;




			using (OdsRepository respository = new OdsRepository())
			{

				if (courseID != null)
				{
					IList<Section> sections = respository.GetSections(CourseID.FromString(courseID),  Ctc.Ods.Types.YearQuarter.FromString(YearQuarterID));

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
			ViewBag.titleDisplayed = false;
			ViewBag.CourseOutcome = getCourseOutcome(Subject, ClassNum);

			using (OdsRepository respository = new OdsRepository())
			{

				if (courseID != null)
				{
					IList<CourseDescription> courseDescriptions = respository.GetCourseDescription(CourseID.FromString(courseID));

					foreach(CourseDescription desc in courseDescriptions) {
						ViewBag.CourseDescription = desc.Description;

					}

					if (YearQuarterID != "All")
					{
						IList<Section> sections = respository.GetSections(CourseID.FromString(courseID),  Ctc.Ods.Types.YearQuarter.FromString(YearQuarterID));
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


		private dynamic getCourseOutcome(string Subject, string ClassNum)
		{
			string url = "http://bellevuecollege.edu/courseoutcomes/?CourseID=" + Subject + "%20" + ClassNum;
			StringBuilder sb = new StringBuilder();

			byte[] buffer = new byte[8000];
			HttpWebRequest request = (HttpWebRequest)
			WebRequest.Create(url);

			// execute the request
			HttpWebResponse response = (HttpWebResponse)
				request.GetResponse();

			// we will read data via the response stream
			Stream resStream = response.GetResponseStream();

			string tempString = null;
			int count = 0;

			do
			{
				// fill the buffer with data
				count = resStream.Read(buffer, 0, buffer.Length);

				// make sure we read some data
				if (count != 0)
				{
					// translate from bytes to ASCII text
					tempString = Encoding.ASCII.GetString(buffer, 0, count);

					// continue building the string
					sb.Append(tempString);
				}
			}
			while (count > 0); // any more data to read?

			// return course outcome page source
			return sb.ToString();



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



			ViewBag.QuarterURL = YearQuarter;



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
		int yearBaseTenPlusOne = 0;
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

		if (!badlyFormedQuarter && !badlyFormedYear)
		{




			//is the year an overlapping year? e.g. spring 2000 = 9904 but summer 2000 = A011
			if (yearBaseTen == 0 && (quarter == "3" || quarter == "4"))
			{
				isLastTwoQuarters = true;
			}

			//find out which decade it is in, to determine Axxx (first character in string)
			switch (decade)
			{
				case "197":
					YearQuarter1 = isLastTwoQuarters == true ? "6" : "7";
					break;
				case "198":
					YearQuarter1 = isLastTwoQuarters == true ? "7" : "8";
					break;
				case "199":
					YearQuarter1 = isLastTwoQuarters == true ? "8" : "9";
					break;
				case "200":
					YearQuarter1 = isLastTwoQuarters == true ? "9" : "A";
					break;
				case "201":
					YearQuarter1 = isLastTwoQuarters == true ? "A" : "B";
					break;
				case "202":
					YearQuarter1 = isLastTwoQuarters == true ? "B" : "C";
					break;


			}

			//figure out what the x23x portion of the YRQ is
			if (quarter == "1" || quarter == "2")
			{
				if (yearBaseTen + 1 > 9)
				{
					yearBaseTenPlusOne = yearBaseTen + 1 - 10;
				}
				else
				{
					yearBaseTenPlusOne = yearBaseTen + 1;
				}
				YearQuarter23 = Convert.ToString(yearBaseTen) + Convert.ToString((yearBaseTenPlusOne));

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

				YearQuarter23 = Convert.ToString(tempYearBaseTen - 1) + Convert.ToString((yearBaseTen));
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

		private String getFriendlyDateFromYRQ(YearQuarter YRQ)
	{
		//Example: Winter 2008 = A783

		//summer: xxx1
		//fall:   xxx2
		//winter: xxx3
		//spring:	xxx4

		//Academic year 2006-2007: x67x
		//Academic year 2011-2012: x12x

		string stringYRQ = YRQ.ID.ToString();

		string year1 = stringYRQ.Substring(stringYRQ.Length - 3, 1); //x6xx = 2006
		string year2 = stringYRQ.Substring(stringYRQ.Length - 2, 1); //xx7x = 2007
		string quarter = stringYRQ.Substring(stringYRQ.Length - 1, 1); //Spring
		string decade = stringYRQ.Substring(stringYRQ.Length - 4, 1); //Axxx = 2000's
		string strQuarter = "";
		bool isLastTwoQuarters = false;
		bool badlyFormedQuarter = false;
		bool badlyFormedYear = false;

		string year = "";

				//is the year an overlapping year? e.g. spring 2000 = 9904 but summer 2000 = A011
			if (year2 == "0" && (quarter == "3" || quarter == "4"))
			{
				isLastTwoQuarters = true;
			}

		//determine the quarter in string form "1", "2", etc... essentially the xxx2 character
		switch (quarter)
		{
			case "1":
				strQuarter = "Summer";
				year = getYearHelper(quarter, year1, year2, decade, isLastTwoQuarters);
				break;
			case "2":
				strQuarter = "Fall";
				year = getYearHelper(quarter, year1, year2, decade, isLastTwoQuarters);
				break;
			case "3":
				strQuarter = "Winter";
				year = getYearHelper(quarter, year1, year2, decade, isLastTwoQuarters);
				break;
			case "4":
				strQuarter = "Spring";
				year = getYearHelper(quarter, year1, year2, decade, isLastTwoQuarters);
				break;
			default:
				badlyFormedQuarter = true;
				break;

		}
		if (IsInteger(year))
		{
			if (Convert.ToInt16(year) < 1975 || Convert.ToInt16(year) > 2030)
			{
				badlyFormedYear = true;
			}
		}

		string returnFriendly = "You have entered a badly formed quarter/year.";

		if (!badlyFormedQuarter || !badlyFormedYear)
		{
			returnFriendly = strQuarter + " " + year;
		}

		return returnFriendly;




	}

		private string getYearHelper(string quarter, string year1, string year2, string decade, bool isLastTwoQuarters)
	{
		string first2OfYear = "";
		string last2OfYear = "";
		string ThirdOfYear = "";

		int intYear1 = Convert.ToInt16(year1);
		int intYear2 = Convert.ToInt16(year2);

		switch (decade)
		{
			case "7":
				first2OfYear = "19";
				break;
			case "8":
				first2OfYear = "19";
				break;
			case "9":
				first2OfYear = isLastTwoQuarters == true ? "20" : "19";
				break;
			case "A":
				first2OfYear = "20";
				break;
			case "B":
				first2OfYear = "20";
				break;
			case "C":
				first2OfYear = "20";
				break;
			case "D":
				first2OfYear = "20";
				break;
			default:
				break;

		}

		switch (quarter)
		{
			case "1":
				last2OfYear = getDecadeIntegerFromString(decade) + intYear1.ToString();
				break;
			case "2":
				last2OfYear = getDecadeIntegerFromString(decade) + intYear1.ToString();
				break;
			case "3":
				ThirdOfYear = isLastTwoQuarters == true ? getDecadeIntegerFromString(getNextDecade(decade)) : getDecadeIntegerFromString(decade);
				last2OfYear = ThirdOfYear + intYear2.ToString();
				break;
			case "4":
				ThirdOfYear = isLastTwoQuarters == true ? getDecadeIntegerFromString(getNextDecade(decade)) : getDecadeIntegerFromString(decade);
				last2OfYear = ThirdOfYear + intYear2.ToString();
				break;
			default:

				break;

		}

		return first2OfYear + last2OfYear;

	}

		private string getDecadeIntegerFromString(string decade)
	{
		switch (decade)
		{
			case "7":
				return "7";
				break;
			case "8":
				return "8";
				break;
			case "9":
				return "9";
				break;
			case "A":
				return "0";
				break;
			case "B":
				return "1";
				break;
			case "C":
				return "2";
				break;
			case "D":
				return "3";
				break;
		}
		return "";
	}

		private string getNextDecade(string decade)
		{
			switch (decade)
			{
				case "7":
					return "8";
					break;
				case "8":
					return "9";
					break;
				case "9":
					return "A";
					break;
				case "A":
					return "B";
					break;
				case "B":
					return "C";
					break;
				case "C":
					return "D";
					break;
				case "D":
					return "E";
					break;
			}
			return "";

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
