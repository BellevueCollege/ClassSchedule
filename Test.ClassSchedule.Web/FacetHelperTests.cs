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
using System.Linq;
using CTCClassSchedule;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.CtcClassSchedule
{
  [TestClass]
  public class FacetHelperTests
  {
    [TestMethod]
    public void Modality_Initialized()
    {
      FacetHelper fh = new FacetHelper();

      AssertFacetExists(fh, "f_oncampus", "On Campus");
      AssertFacetExists(fh, "f_online", "Online");
      AssertFacetExists(fh, "f_hybrid", "Hybrid");
    }

    [TestMethod]
    public void Modality_OnCampus_Only()
    {
      FacetHelper fh = new FacetHelper();
      fh.SetModalities("true");

      AssertFacetValue(fh, "f_oncampus", "On Campus", true);
      AssertFacetValue(fh, "f_online", "Online", false);
      AssertFacetValue(fh, "f_hybrid", "Hybrid", false);
    }

    #region Private methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fh"></param>
    /// <param name="facetID"></param>
    /// <param name="facetTitle"></param>
    /// <param name="expectedValue"></param>
    private static void AssertFacetExists(FacetHelper fh, string facetID, string facetTitle)
    {
      AssertFacetValue(fh, facetID, facetTitle, false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fh"></param>
    /// <param name="facetID"></param>
    /// <param name="facetTitle"></param>
    /// <param name="expectedValue"></param>
    private static void AssertFacetValue(FacetHelper fh, string facetID, string facetTitle, bool expectedValue)
    {
      int count = fh.Modality.Count(m => m.ID == facetID);
      Assert.AreEqual(1, count, "More than one ({0}) {1} modality facet found.", count, facetTitle);

      bool test = fh.Modality.Any(m => m.ID == facetID && !string.IsNullOrWhiteSpace(m.Value) && bool.Parse(m.Value));
      if (expectedValue)
      {
        Assert.IsTrue(test, "Value of {1} is '{0}'", fh.Modality.First(m => m.ID == facetID).Value, facetID);
      }
      else
      {
        Assert.IsFalse(test, "Value of {1} is '{0}'", fh.Modality.First(m => m.ID == facetID).Value, facetID);
      }
    }

    #endregion
  }
}
