/*
This file is part of CtcClassSchedule.

CtcClassSchedule is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

CtcClassSchedule is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with CtcClassSchedule.  If not, see <http://www.gnu.org/licenses/>.
 */
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CTCClassSchedule.Controllers;
using CTCClassSchedule.Models;
using CtcApi.Extensions;
using Ctc.Ods.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Web.Mvc;

namespace Test.CtcClassSchedule
{


  /// <summary>
  ///This is a test class for ApiControllerTest and is intended
  ///to contain all ApiControllerTest Unit Tests
  ///</summary>
  [TestClass()]
  public class ApiControllerTest
  {


    private TestContext testContextInstance;

    /// <summary>
    ///Gets or sets the test context which provides
    ///information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext
    {
      get
      {
        return testContextInstance;
      }
      set
      {
        testContextInstance = value;
      }
    }

    #region Additional test attributes
    //
    //You can use the following additional attributes as you write your tests:
    //
    //Use ClassInitialize to run code before running the first test in the class
    //[ClassInitialize()]
    //public static void MyClassInitialize(TestContext testContext)
    //{
    //}
    //
    //Use ClassCleanup to run code after all tests in a class have run
    //[ClassCleanup()]
    //public static void MyClassCleanup()
    //{
    //}
    //
    //Use TestInitialize to run code before running each test
    //[TestInitialize()]
    //public void MyTestInitialize()
    //{
    //}
    //
    //Use TestCleanup to run code after each test has run
    //[TestCleanup()]
    //public void MyTestCleanup()
    //{
    //}
    //
    #endregion


    [TestMethod()]
    [Ignore]  // TODO: Refactor test to not rely on existence of certain records
    public void CrossListedCourses_NotCommonCourse()
    {
      ApiController target = new ApiController();
      //      string courseID = ConstructCourseID(CourseID.FromString("MATH","255"), false);
      string courseID = ConstructCourseID(CourseID.FromString("ACCT", "101"), false);
      JsonResult actual = target.CrossListedCourses(courseID, "** INVALID YRQ **");

      Assert.IsNotNull(actual, "Returned Result is NULL");
      Assert.IsNotNull(actual.Data, "JSON data is NULL");
      Assert.IsInstanceOfType(actual.Data, typeof(IEnumerable<CrossListedCourseModel>));

      IEnumerable<CrossListedCourseModel> obj = actual.Data as IEnumerable<CrossListedCourseModel>;
      Assert.IsNotNull(obj);

      string[] sectionIDs = obj.Select(o => o.SectionID.ToString()).ToArray();

      if (sectionIDs.Length > 0)
      {
        using (ClassScheduleDb db = new ClassScheduleDb())
        {
          IQueryable<SectionCourseCrosslisting> crosslistings = from x in db.SectionCourseCrosslistings
                                                                where sectionIDs.Contains(x.ClassID)
                                                                select x;
          Assert.IsTrue(crosslistings.Any(), "Did not find matching cross-listing record in the Class Schedule database. ('{0}' => [{1}]", courseID, sectionIDs.Mash());
        }
      }
      else
      {
        Assert.Inconclusive("The method did not return any cross-linked SectionIDs for '{0}'", courseID);
      }
    }

    [TestMethod()]
    [Ignore]  // TODO: Refactor test to not rely on existence of certain records
    public void CrossListedCourses_CommonCourse()
    {
      ApiController target = new ApiController();
      string courseID = ConstructCourseID(CourseID.FromString("ART", "107"), true);
      JsonResult actual = target.CrossListedCourses(courseID, "** INVALID YRQ **");

      Assert.IsNotNull(actual, "Returned Result is NULL");
      Assert.IsNotNull(actual.Data, "JSON data is NULL");
      Assert.IsInstanceOfType(actual.Data, typeof(IEnumerable<CrossListedCourseModel>));

      IEnumerable<CrossListedCourseModel> obj = actual.Data as IEnumerable<CrossListedCourseModel>;
      Assert.IsNotNull(obj);

      string[] sectionIDs = obj.Select(o => o.SectionID.ToString()).ToArray();

      if (sectionIDs.Length > 0)
      {
        using (ClassScheduleDb db = new ClassScheduleDb())
        {
          IQueryable<SectionCourseCrosslisting> crosslistings = from x in db.SectionCourseCrosslistings
                                                                where sectionIDs.Contains(x.ClassID)
                                                                select x;
          Assert.IsTrue(crosslistings.Any(), "Did not find matching cross-listing record in the Class Schedule database. ('{0}' => [{1}]", courseID, sectionIDs.Mash());
        }
      }
      else
      {
        Assert.Inconclusive("The method did not return any cross-linked SectionIDs for '{0}'", courseID);
      }
    }

    #region Courses tests
    [TestMethod()]
    public void Courses_OneSubject_LowerCase()
    {
      ApiController target = new ApiController();
      ActionResult actual = target.Courses("engl");

      Assert.IsNotNull(actual, "Returned Result is NULL");
      Assert.IsInstanceOfType(actual, typeof(JsonResult));

      JsonResult json = actual as JsonResult;
      Assert.IsNotNull(json.Data, "JSON data is NULL");
      Assert.IsInstanceOfType(json.Data, typeof(IEnumerable<Course>));

      IEnumerable<Course> courses = json.Data as IEnumerable<Course>;
      foreach (var course in courses)
      {
        Console.Out.WriteLine(course.CourseID);
      }

      Assert.IsTrue(courses.Any(), "Did not receive any courses.");
    }

    [TestMethod()]
    public void Courses_TwoSubjects_LowerCase()
    {
      ApiController target = new ApiController();
      ActionResult actual = target.Courses("engl", "biol");

      Assert.IsNotNull(actual, "Returned Result is NULL");
      Assert.IsInstanceOfType(actual, typeof(JsonResult));

      JsonResult json = actual as JsonResult;
      Assert.IsNotNull(json.Data, "JSON data is NULL");
      Assert.IsInstanceOfType(json.Data, typeof(IEnumerable<Course>));

      IEnumerable<Course> courses = json.Data as IEnumerable<Course>;
      foreach (var course in courses)
      {
        Console.Out.WriteLine(course.CourseID);
      }

      int count = courses.Count();
      Assert.IsTrue(count > 1, "Expected more than one Course, received [{0}]", count);
    }

    [TestMethod()]
    public void Courses_TooManyPrefixes()
    {
      IList<string> prefixes = new List<string>(ApiController.MAX_COURSE_PREFIXES + 1);
      for (int i = 0; i < ApiController.MAX_COURSE_PREFIXES + 1; i++)
      {
        prefixes.Add(string.Format("PRE{0}", i));
      }

      ApiController target = new ApiController();
      ActionResult actual = target.Courses(prefixes.ToArray());

      Assert.IsNotNull(actual);
      Assert.IsInstanceOfType(actual, typeof(HttpStatusCodeResult));

      HttpStatusCodeResult http = actual as HttpStatusCodeResult;
      Assert.AreEqual((int)HttpStatusCode.BadRequest, http.StatusCode);
    }

    [TestMethod()]
    public void Courses_NoPrefixes()
    {
      ApiController target = new ApiController();
      ActionResult actual = target.Courses();

      Assert.IsNotNull(actual);
      Assert.IsInstanceOfType(actual, typeof(HttpStatusCodeResult));

      HttpStatusCodeResult http = actual as HttpStatusCodeResult;
      Assert.AreEqual((int)HttpStatusCode.BadRequest, http.StatusCode);
    }

    [TestMethod()]
    public void Courses_EmptyStringPrefix()
    {
      ApiController target = new ApiController();
      ActionResult actual = target.Courses("");

      Assert.IsNotNull(actual, "Returned Result is NULL");
      Assert.IsInstanceOfType(actual, typeof(JsonResult));

      JsonResult json = actual as JsonResult;
      Assert.IsNotNull(json.Data, "JSON data is NULL");
      Assert.IsInstanceOfType(json.Data, typeof(IEnumerable<Course>));

      IEnumerable<Course> courses = json.Data as IEnumerable<Course>;
      int count = courses.Count();
      Assert.IsTrue(count == 0, "Expected an empty Course list, received [{0}]", count);
    }

    #endregion

    /// <summary>
    /// Constructs a CourseID string in the format "ENGL& 101"
    /// </summary>
    /// <param name="id"></param>
    /// <param name="isCommonCourse"></param>
    /// <returns></returns>
    // HACK: isCommonCourse parameter is a work-around until CtcApi is fixed to correctly set the flag when instantiating from another ICourseID object.
    private string ConstructCourseID(ICourseID id, bool isCommonCourse)
    {
      string subject = string.Format("{0}{1}", id.Subject, isCommonCourse ? "&" : string.Empty);
      if (subject.Length < 6)
      {
        subject = string.Concat(subject, " ");
      }
      return string.Concat(subject, id.Number);
    }
  }
}
