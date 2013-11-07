using System.Web.Mvc;
using Ctc.Ods.Types;

namespace CTCClassSchedule.Models
{
  /// <summary>
  /// Provides an impelmentation of <see cref="ISectionID"/> with a custom <see cref="ModelBinderAttribute"/>
  /// </summary>
  /// <seealso cref="SectionIdModelBinder"/>
  [ModelBinder(typeof(SectionIdModelBinder))]
  public class ClassScheduleSectionID : SectionID
  {
    /// <summary>
    /// Creates a new <see cref="T:Ctc.Ods.Types.SectionID"/> from an item # and YRQ
    /// </summary>
    /// <param name="itemNumber"/><param name="yrq"/>
    protected ClassScheduleSectionID(string itemNumber, string yrq) : base(itemNumber, yrq)
    {
    }

    /// <summary>
    /// Creates a new <see cref="T:Ctc.Ods.Types.SectionID"/> from an item # and YRQ
    /// </summary>
    public ClassScheduleSectionID(ISectionID sectionID) : base(sectionID.ItemNumber, sectionID.YearQuarter)
    {
    }
  }
}