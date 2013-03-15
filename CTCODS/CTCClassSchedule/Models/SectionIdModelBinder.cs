using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Ctc.Ods.Types;

namespace CTCClassSchedule.Models
{
  public class SectionIdModelBinder : DefaultModelBinder
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
      Type propertyType = propertyDescriptor.PropertyType;
      if (propertyType == typeof(SectionID) || propertyType.IsSubclassOf(typeof(SectionID)))
      {
        return SectionID.FromString(bindingContext.ValueProvider.GetValue(bindingContext.ModelName).AttemptedValue);
      }
      return base.GetPropertyValue(controllerContext, bindingContext, propertyDescriptor, propertyBinder);
    }
  }
}