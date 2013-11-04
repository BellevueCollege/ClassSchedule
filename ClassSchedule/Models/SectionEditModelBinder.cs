using System.Web.Mvc;
using Ctc.Ods.Types;

namespace CTCClassSchedule.Models
{
  public class SectionEditModelBinder : DefaultModelBinder
  {
    /// <summary>
    /// Provides custom logic for creating an <see cref="ISectionID"/> object from a string.
    /// </summary>
    /// <param name="controllerContext"></param>
    /// <param name="bindingContext"></param>
    /// <param name="propertyDescriptor"></param>
    /// <param name="propertyBinder"></param>
    /// <returns></returns>
    protected override object GetPropertyValue(ControllerContext controllerContext, ModelBindingContext bindingContext, System.ComponentModel.PropertyDescriptor propertyDescriptor, IModelBinder propertyBinder)
    {
      if (propertyDescriptor.PropertyType.IsSubclassOf(typeof(Section)))
      {
        return base.GetPropertyValue(controllerContext, bindingContext, propertyDescriptor, new SectionIdModelBinder());
      }
      // TODO: Account for a property that is a collection of ISection objects
      return base.GetPropertyValue(controllerContext, bindingContext, propertyDescriptor, propertyBinder);
    }
  }
}