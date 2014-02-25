using System.Collections.Generic;
using System.Linq;
using CTCClassSchedule.Controllers;
using CTCClassSchedule.Models;
using CtcApi.Extensions;
using Ctc.Ods;
using Ctc.Ods.Types;
using CtcApi.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
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
      string courseID = ConstructCourseID(CourseID.FromString("CEO", "196"), false);
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

    [TestMethod()]
    public void Courses_OneSubject_LowerCase_NoQuarter()
    {
      ApiController target = new ApiController();
      ActionResult actual = target.Courses("engl");

      Assert.IsNotNull(actual, "Returned Result is NULL");
      
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
    public void Courses_TwoSubjects_LowerCase_NoQuarter()
    {
      ApiController target = new ApiController();
      ActionResult actual = target.Courses("engl", "biol");

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
