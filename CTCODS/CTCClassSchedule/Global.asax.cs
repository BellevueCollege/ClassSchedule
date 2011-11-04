using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using MvcMiniProfiler;

namespace CTCClassSchedule
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode,
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}

		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			routes.IgnoreRoute("ScheduleError.aspx");

			// API calls the application exposes
			routes.MapRoute("ApiSubjects", "Api/Subjects", new {controller = "Api", action = "Subjects"});

			// default application routes
			routes.MapRoute("getSeats", "{controller}/getseats", new { controller = "Classes", action = "getSeats" });
			routes.MapRoute("ClassDetails", "{controller}/{YearQuarterID}/{Subject}/{ClassNum}", new { controller = "Classes", action = "ClassDetails" });
			routes.MapRoute("Subject", "{controller}/All/{Subject}", new { controller = "Classes", action = "Subject" });
			routes.MapRoute("YearQuarterSubject", "{controller}/{YearQuarter}/{Subject}", new { controller = "Classes", action = "YearQuarterSubject"});
			routes.MapRoute("AllClasses", "{controller}/All", new { controller = "Classes", action = "AllClasses" });
			routes.MapRoute("YearQuarter", "{controller}/{YearQuarter}", new { controller = "Classes", action = "YearQuarter"});
			routes.MapRoute("Default", "{controller}", new { controller = "Classes", action = "Index" });
			routes.MapRoute("Search", "{controller}/{quarter}", new { controller = "Search", action = "Index" });

		}

		/// <summary>
		///
		/// </summary>
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();

#if ENABLE_PROFILING
			// Add profiling view engine
			var copy = ViewEngines.Engines.ToList();
			ViewEngines.Engines.Clear();
			foreach (var item in copy)
			{
					ViewEngines.Engines.Add(new ProfilingViewEngine(item));
			}

			// Add profiling action filter
			GlobalFilters.Filters.Add(new ProfilingActionFilter());
#endif

			RegisterGlobalFilters(GlobalFilters.Filters);
			RegisterRoutes(RouteTable.Routes);
		}

		/// <summary>
		///
		/// </summary>
		protected void Application_BeginRequest()
		{
#if ENABLE_PROFILING
			MiniProfiler.Start();
#endif
		}

		/// <summary>
		///
		/// </summary>
		protected void Application_EndRequest()
		{
#if ENABLE_PROFILING
			MiniProfiler.Stop();
#endif
		}

		/// <summary>
		/// Part of the non-MVC error handling system
		/// </summary>
		/// <remarks>
		/// This method is part of the non-MVC error handling. For MVC-specific error handling, see <see cref="RegisterGlobalFilters"/>.
		/// </remarks>
		/// <seealso cref="RegisterGlobalFilters"/>
		protected void Application_Error()
		{
			if (Server.GetLastError() != null)
			{
// ReSharper disable ConstantNullCoalescingCondition
				Exception ex = Server.GetLastError().GetBaseException() ?? Server.GetLastError();
// ReSharper restore ConstantNullCoalescingCondition

				Application["LastError"] = ex;
			}
		}
	}
}
