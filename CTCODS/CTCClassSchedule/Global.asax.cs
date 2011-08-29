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

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();

			RegisterGlobalFilters(GlobalFilters.Filters);
			RegisterRoutes(RouteTable.Routes);
		}
	}
}