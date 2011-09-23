
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using MvcMiniProfiler;

namespace CTCClassSchedule
{
#if ENABLE_PROFILING
	/// <summary>
	/// Enables MvcMiniProfiler for all controller actions
	/// </summary>
	/// <remarks>
	/// The code for this filter was obtained from the following blog post:
	/// <a href="http://samsaffron.com/archive/2011/07/25/Automatically+instrumenting+an+MVC3+app">http://samsaffron.com/archive/2011/07/25/Automatically+instrumenting+an+MVC3+app</a>
	/// </remarks>
	public class ProfilingActionFilter : ActionFilterAttribute
	{
    const string stackKey = "ProfilingActionFilterStack";

    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var mp = MiniProfiler.Current;
        if (mp != null)
        {
            var stack = HttpContext.Current.Items[stackKey] as Stack<IDisposable>;
            if (stack == null)
            {
                stack = new Stack<IDisposable>();
                HttpContext.Current.Items[stackKey] = stack;
            }

            var prof = MiniProfiler.Current.Step("Controller: " + filterContext.Controller.ToString() + "." + filterContext.ActionDescriptor.ActionName);
            stack.Push(prof);

        }
        base.OnActionExecuting(filterContext);
    }

		/// <summary>
		///
		/// </summary>
		/// <param name="filterContext"></param>
    public override void OnActionExecuted(ActionExecutedContext filterContext)
    {
        base.OnActionExecuted(filterContext);
        var stack = HttpContext.Current.Items[stackKey] as Stack<IDisposable>;
        if (stack != null && stack.Count > 0)
        {
            stack.Pop().Dispose();
        }
    }
	}
#endif
}