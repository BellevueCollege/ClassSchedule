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

    [TestMethod]
    public void SectionFacets_Time_Start_No_End()
    {
      throw new NotImplementedException();
    }

    [TestMethod]
    public void SectionFacets_Time_End_No_Start()
    {
      throw new NotImplementedException();
    }

    [TestMethod]
    public void SectionFacets_Time_Start_And_End()
    {
      throw new NotImplementedException();
    }

    #endregion

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
