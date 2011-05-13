﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;


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

			routes.MapRoute("ClassDetails", "{controller}/{YearQuarter}/{Subject}/{ClassNum}", new { controller = "Classes", action = "ClassDetails" });
			routes.MapRoute("Subject", "{controller}/All/{Subject}", new { controller = "Classes", action = "Subject" });
			routes.MapRoute("YearQuarterSubject", "{controller}/{YearQuarter}/{Subject}", new { controller = "Classes", action = "YearQuarterSubject"});
			routes.MapRoute("All", "{controller}/All", new { controller = "Classes", action = "All" });
			routes.MapRoute("YearQuarter", "{controller}/{YearQuarter}", new { controller = "Classes", action = "YearQuarter"});
			routes.MapRoute("Default", "{controller}/{action}", new { controller = "Classes", action = "Index"});
		//  routes.MapRoute(
		//    "Default", // Route name
		//    "{controller}/{action}/{id}", // URL with parameters
		//    new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
		//);

		}

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();

			RegisterGlobalFilters(GlobalFilters.Filters);
			RegisterRoutes(RouteTable.Routes);
		}
	}
}