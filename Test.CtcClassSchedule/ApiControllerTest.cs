using System.Collections.Generic;
using System.Linq;
using CTCClassSchedule.Controllers;
using CTCClassSchedule.Models;
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
    public void CrossListedCoursesTest()
    {
      ApiController target = new ApiController();
      string sectionID = "3003B234";
      JsonResult actual = target.CrossListedCourses(sectionID);

      Assert.IsNotNull(actual, "Returned Result is NULL");
      Assert.IsNotNull(actual.Data, "JSON data is NULL");
      Assert.IsInstanceOfType(actual.Data, typeof(IEnumerable<CrossListedCourseModel>));

      IEnumerable<CrossListedCourseModel> obj = actual.Data as IEnumerable<CrossListedCourseModel>;
      Assert.IsNotNull(obj);

      string[] courseIDs = obj.Select(o => ConstructCourseID(o.ID, o.IsCommonCourse)).ToArray();

      if (courseIDs.Length > 0)
      {
        using (ClassScheduleDb db = new ClassScheduleDb())
        {
          IQueryable<SectionCourseCrosslisting> crosslistings = from x in db.SectionCourseCrosslistings
                                                                where courseIDs.Contains(x.CourseID)
                                                                select x;
          Assert.IsTrue(crosslistings.Any(), "Did not find matching cross-listing record in the Class Schedule database. ('{0}' => [{1}]", sectionID, courseIDs.Mash());
        }
      }
      else
      {
        Assert.Inconclusive("The method did not return any cross-linked CourseIDs for '{0}'", sectionID);
      }
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
        return string.Concat(string.Format("{0}{1} ", id.Subject, isCommonCourse ? "&" : string.Empty).Substring(0, 6), id.Number);
      }
  }
}
