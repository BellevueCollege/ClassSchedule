using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace CtcApi.Extensions
{
  public static class BoolExtensions
  {
    public static bool InclusiveOr(this bool bln, bool expression)
    {
// ReSharper disable RedundantAssignment
      bln = (bln || expression);
// ReSharper restore RedundantAssignment
      return expression;
    }
  }
}