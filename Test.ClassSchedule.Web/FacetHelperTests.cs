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

using System;
using System.Collections.Generic;
using System.Linq;
using Ctc.Ods;
using CTCClassSchedule;
using CTCClassSchedule.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.CtcClassSchedule
{
  [TestClass]
  public class FacetHelperTests
  {
    [TestMethod]
    public void StartingValuesNotNull()
    {
      FacetHelper fh = new FacetHelper();

      Assert.IsNotNull(fh.Availability, "Availability is null.");
      Assert.IsNotNull(fh.Credits, "Credits is null.");
      Assert.IsNotNull(fh.Days, "Days is null.");
      Assert.IsNotNull(fh.LateStart, "LateStart is null.");
      Assert.IsNotNull(fh.Modality, "Modality is null.");
      Assert.IsNotNull(fh.TimeEnd, "TimeEnd is null.");
      Assert.IsNotNull(fh.TimeStart, "TimeStart is null.");
    }


    #region Time tests
    [TestMethod]
    public void Time_Invalid_Start()
    {
      string[] invalidTimes =
      {
        "12:",
        ":30"
      };
      foreach (string invalidTime in invalidTimes)
      {
        FacetHelper fh = new FacetHelper
                         {
                           TimeStart = invalidTime,
                         };

        Assert.IsNotNull(fh.TimeStart);
        Assert.IsTrue(string.IsNullOrEmpty(fh.TimeStart), "TimeStart is not empty: '{0}'", fh.TimeStart);

        IList<ISectionFacet> facets = fh.CreateSectionFacets();
        AssertSectionFacetTypeCount(facets, typeof(TimeFacet), 0);
      }
    }

    [TestMethod]
    public void Time_Invalid_End()
    {
      string[] invalidTimes =
      {
        "12:",
        ":30"
      };
      foreach (string invalidTime in invalidTimes)
      {
        FacetHelper fh = new FacetHelper
                         {
                           TimeEnd = invalidTime,
                         };

        Assert.IsNotNull(fh.TimeEnd);
        Assert.IsTrue(string.IsNullOrEmpty(fh.TimeEnd), "TimeEnd is not empty: '{0}'", fh.TimeEnd);

        IList<ISectionFacet> facets = fh.CreateSectionFacets();
        AssertSectionFacetTypeCount(facets, typeof(TimeFacet), 0);
      }
    }
    
    /**
     * Test that the ToTime() method of FacetHelper properly translates 
     * times added by a user to filter courses to the correct 24-hour 
     * time TimeSpan object values
     **/
    [TestMethod]
    public void Time_Test_ToTime()
    {
        TimeSpan morningTimeSpan = FacetHelper.ToTime("8:30am");
        TimeSpan middayTimeSpan = FacetHelper.ToTime("12:30pm");
        TimeSpan afternoonTimeSpan = FacetHelper.ToTime("2:45pm");
        TimeSpan endTimeSpan = FacetHelper.ToTime("23:59 PM");

        Assert.IsTrue(morningTimeSpan.Hours == 8 && morningTimeSpan.Minutes == 30, "Time of 8:30am incorrectly translated to TimeSpan value of '{0}'", morningTimeSpan.ToString());
        Assert.IsTrue(middayTimeSpan.Hours == 12 && middayTimeSpan.Minutes == 30, "Time of 12:30pm incorrectly translated to TimeSpan value of '{0}'", middayTimeSpan.ToString());
        Assert.IsTrue(afternoonTimeSpan.Hours == 14 && afternoonTimeSpan.Minutes == 45, "Time of 2:45pm incorrectly translated to TimeSpan value of '{0}'", afternoonTimeSpan.ToString());
        Assert.IsTrue(endTimeSpan.Hours == 23 && endTimeSpan.Minutes == 59, "Time of 23:59 PM (used by FacetHelper as the default end time) incorrectly translated to TimeSpan value of '{0}'", endTimeSpan.ToString());
    }
    #endregion


    #region Modality tests
    [TestMethod]
    public void Modality_Initialized()
    {
      FacetHelper fh = new FacetHelper();

      AssertFacetExists(fh.Modality, "f_oncampus", "On Campus");
      AssertFacetExists(fh.Modality, "f_online", "Online");
      AssertFacetExists(fh.Modality, "f_hybrid", "Hybrid");
    }

    [TestMethod]
    public void Modality_OnCampus_Only()
    {
      FacetHelper fh = new FacetHelper();
      fh.SetModalities("true");

      AssertFacetValue(fh.Modality, "f_oncampus", "On Campus", true);
      
      AssertFacetValue(fh.Modality, "f_online", "Online", false);
      AssertFacetValue(fh.Modality, "f_hybrid", "Hybrid", false);
    }


    #endregion

    #region Days tests
    [TestMethod]
    public void Days_Initialized()
    {
      FacetHelper fh = new FacetHelper();

      AssertFacetExists(fh.Days, "day_su", "Sun");
      AssertFacetExists(fh.Days, "day_m", "Mon");
      AssertFacetExists(fh.Days, "day_t", "Tue");
      AssertFacetExists(fh.Days, "day_w", "Wed");
      AssertFacetExists(fh.Days, "day_th", "Thur");
      AssertFacetExists(fh.Days, "day_f", "Fri");
      AssertFacetExists(fh.Days, "day_s", "Sat");
    }

    [TestMethod]
    public void Days_Sunday_Only()
    {
      FacetHelper fh = new FacetHelper();
      fh.SetDays("true");

      AssertFacetValue(fh.Days, "day_su", "Sun", true);

      AssertFacetValue(fh.Days, "day_m", "Mon", false);
      AssertFacetValue(fh.Days, "day_t", "Tue", false);
      AssertFacetValue(fh.Days, "day_w", "Wed", false);
      AssertFacetValue(fh.Days, "day_th", "Thur", false);
      AssertFacetValue(fh.Days, "day_f", "Fri", false);
      AssertFacetValue(fh.Days, "day_s", "Sat", false);
    }

    #endregion

    #region SectionFacet results
    [TestMethod]
    public void SectionFacets_None()
    {
      FacetHelper fh = new FacetHelper();

      IList<ISectionFacet> facets = fh.CreateSectionFacets();

      AssertSectionFacetTypeCount(facets, typeof(ModalityFacet), 0);
      AssertSectionFacetTypeCount(facets, typeof(AvailabilityFacet), 0);
      AssertSectionFacetTypeCount(facets, typeof(DaysFacet), 0);
      AssertSectionFacetTypeCount(facets, typeof(TimeFacet), 0);
      AssertSectionFacetTypeCount(facets, typeof(LateStartFacet), 0);
      AssertSectionFacetTypeCount(facets, typeof(CreditsFacet), 0);
    }

    // **************************************************************
    // Observed injection attempts
    // See https://dev.bellevuecollege.edu/fogbugz/default.asp?4895
    // **************************************************************

    [TestMethod]
    public void InjectionAttempts()
    {
      string[] attackStrings =
      {
        "'/*N*/and/*N*/'3'='2",
        "')/*N*/and/*N*/('3'='3/*N*/",
        "LNzfENfNLhuGioCF",
        "IUQJwJKLT",
        "Caleb"
      };

      foreach (string attackString in attackStrings)
      {
        FacetHelper fh = new FacetHelper
        {
          TimeStart = attackString,
          TimeEnd = attackString,
          Availability = attackString,
          Credits = attackString,
          LateStart = attackString
        };

        IList<ISectionFacet> facets = fh.CreateSectionFacets();

        // invalid values should be ignored
        AssertSectionFacetTypeCount(facets, typeof(AvailabilityFacet), 0);
        AssertSectionFacetTypeCount(facets, typeof(TimeFacet), 0);
        AssertSectionFacetTypeCount(facets, typeof(LateStartFacet), 0);
        AssertSectionFacetTypeCount(facets, typeof(CreditsFacet), 0);
      }
    }

    #region Time facets
    [TestMethod]
    public void SectionFacets_Time_Start_No_End()
    {
      FacetHelper fh = new FacetHelper
                       {
                         TimeStart = "11:00 AM",
                       };

      IList<ISectionFacet> facets = fh.CreateSectionFacets();

      // use default end time if not provided
      AssertSectionFacetTypeCount(facets, typeof(TimeFacet));
    }

    [TestMethod]
    public void SectionFacets_Time_End_No_Start()
    {
      FacetHelper fh = new FacetHelper
                       {
                         TimeEnd = "5:00 PM",
                       };

      IList<ISectionFacet> facets = fh.CreateSectionFacets();

      // use default start time if not provided
      AssertSectionFacetTypeCount(facets, typeof(TimeFacet));
    }

    [TestMethod]
    public void SectionFacets_Time_Start_And_End()
    {
      FacetHelper fh = new FacetHelper
                       {
                         TimeStart = "11:00 AM",
                         TimeEnd = "5:00 PM",
                       };

      IList<ISectionFacet> facets = fh.CreateSectionFacets();

      AssertSectionFacetTypeCount(facets, typeof(TimeFacet));
      Assert.AreEqual("11:00 AM", fh.TimeStart);
      Assert.AreEqual("5:00 PM", fh.TimeEnd);
    }

    #endregion

    #region AvailabilityFacet
    [TestMethod]
    public void SectionFacets_Availability_All()
    {
      FacetHelper fh = new FacetHelper
                       {
                         Availability = "All"
                       };

      IList<ISectionFacet> facets = fh.CreateSectionFacets();

      AssertSectionFacetTypeCount(facets, typeof(AvailabilityFacet));
    }

    [TestMethod]
    public void SectionFacets_Availability_Open()
    {
      FacetHelper fh = new FacetHelper
                       {
                         Availability = "Open"
                       };

      IList<ISectionFacet> facets = fh.CreateSectionFacets();

      AssertSectionFacetTypeCount(facets, typeof(AvailabilityFacet));
    }

    #endregion

    #region ModalityFacet
    [TestMethod]
    public void SectionFacets_Modality_OnCampus()
    {
      FacetHelper fh = new FacetHelper();
      fh.SetModalities("true");

      IList<ISectionFacet> facets = fh.CreateSectionFacets();

      AssertSectionFacetTypeCount(facets, typeof(ModalityFacet));
    }

    [TestMethod]
    public void SectionFacets_Modality_Online()
    {
      FacetHelper fh = new FacetHelper();
      fh.SetModalities(fOnline:"true");

      IList<ISectionFacet> facets = fh.CreateSectionFacets();

      AssertSectionFacetTypeCount(facets, typeof(ModalityFacet));
    }

    [TestMethod]
    public void SectionFacets_Modality_Hybrid()
    {
      FacetHelper fh = new FacetHelper();
      fh.SetModalities(fHybrid:"true");

      IList<ISectionFacet> facets = fh.CreateSectionFacets();

      AssertSectionFacetTypeCount(facets, typeof(ModalityFacet));
    }

    [TestMethod]
    public void SectionFacets_Modality_OnCampusAndOnline()
    {
      FacetHelper fh = new FacetHelper();
      fh.SetModalities("true", "true");

      IList<ISectionFacet> facets = fh.CreateSectionFacets();

      AssertSectionFacetTypeCount(facets, typeof(ModalityFacet));
    }

    [TestMethod]
    public void SectionFacets_Modality_OnlineAndHybrid()
    {
      FacetHelper fh = new FacetHelper();
      fh.SetModalities(fOnline:"true", fHybrid:"true");

      IList<ISectionFacet> facets = fh.CreateSectionFacets();

      AssertSectionFacetTypeCount(facets, typeof(ModalityFacet));
    }

    [TestMethod]
    public void SectionFacets_Modality_HybridAndOnCampus()
    {
      FacetHelper fh = new FacetHelper();
      fh.SetModalities("true", fHybrid:"true");

      IList<ISectionFacet> facets = fh.CreateSectionFacets();

      AssertSectionFacetTypeCount(facets, typeof(ModalityFacet));
    }

    #endregion

    #endregion

    #region Methods that will be moved into the CtcApi
    [TestMethod]
    public void ToTime_String_1159pm_WithSpace()
    {
      TimeSpan result = FacetHelper.ToTime("11:59 PM");

      Assert.IsNotNull(result);
      Assert.IsTrue(0 == result.Days, "Found <{0}> days.", result.Days);
      Assert.IsTrue(0 == result.Seconds, "Found <{0}> seconds.", result.Seconds);
      Assert.IsTrue(23 == result.Hours, "Found <{0}> hours.", result.Hours);
      Assert.IsTrue(59 == result.Minutes, "Found <{0}> minutes.", result.Minutes);
    }

    [TestMethod]
    public void ToTime_String_1159pm_NoSpace()
    {
      TimeSpan result = FacetHelper.ToTime("11:59PM");

      Assert.IsNotNull(result);
      Assert.IsTrue(0 == result.Days, "Found <{0}> days.", result.Days);
      Assert.IsTrue(0 == result.Seconds, "Found <{0}> seconds.", result.Seconds);
      Assert.IsTrue(23 == result.Hours, "Found <{0}> hours.", result.Hours);
      Assert.IsTrue(59 == result.Minutes, "Found <{0}> minutes.", result.Minutes);
    }



    #endregion

    #region Private methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="facets"></param>
    /// <param name="facetType"></param>
    /// <param name="expectedCount"></param>
    private static void AssertSectionFacetTypeCount(IList<ISectionFacet> facets, Type facetType, int expectedCount = 1)
    {
      int facetCount = facets.Count(f => facetType.IsInstanceOfType(f));
      Assert.IsTrue(expectedCount == facetCount, "Expected <{0}> {2}s, but found <{1}>.", expectedCount, facetCount, facetType.Name);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="list"></param>
    /// <param name="facetID"></param>
    /// <param name="facetTitle"></param>
    /// <param name="expectedValue"></param>
    private static void AssertFacetExists(IList<GeneralFacetInfo> list, string facetID, string facetTitle)
    {
      AssertFacetValue(list, facetID, facetTitle, false);
    }

    /// <summary>
    /// 
    /// </summary>
    private void AssertFacetDoesNotExist(IList<GeneralFacetInfo> list, string facetID)
    {
      Assert.IsFalse(list.Any(m => m.ID == facetID && !string.IsNullOrWhiteSpace(m.Value) && bool.Parse(m.Value)));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="list"></param>
    /// <param name="facetID"></param>
    /// <param name="facetTitle"></param>
    /// <param name="expectedValue"></param>
    private static void AssertFacetValue(IList<GeneralFacetInfo> list, string facetID, string facetTitle, bool expectedValue)
    {
      int count = list.Count(m => m.ID == facetID);
      Assert.AreEqual(1, count, "More than one ({0}) {1} modality facet found.", count, facetTitle);

      bool test = list.Any(m => m.ID == facetID && !string.IsNullOrWhiteSpace(m.Value) && bool.Parse(m.Value));
      if (expectedValue)
      {
        Assert.IsTrue(test, "Value of {1} is '{0}'", list.First(m => m.ID == facetID).Value, facetID);
      }
      else
      {
        Assert.IsFalse(test, "Value of {1} is '{0}'", list.First(m => m.ID == facetID).Value, facetID);
      }
    }

    #endregion
  }
}
