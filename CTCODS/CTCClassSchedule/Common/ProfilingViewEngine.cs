using System.Web.Mvc;
using MvcMiniProfiler;

namespace CTCClassSchedule
{
#if ENABLE_PROFILING
	/// <summary>
	/// Enables MvcMiniProfiler for all views
	/// </summary>
	public class ProfilingViewEngine : IViewEngine
	{
		/// <summary>
		///
		/// </summary>
		class WrappedView : IView
		{
			IView wrapped;
			string name;
			bool isPartial;

			/// <summary>
			///
			/// </summary>
			/// <param name="wrapped"></param>
			/// <param name="name"></param>
			/// <param name="isPartial"></param>
			public WrappedView(IView wrapped, string name, bool isPartial)
			{
				this.wrapped = wrapped;
				this.name = name;
				this.isPartial = isPartial;
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="viewContext"></param>
			/// <param name="writer"></param>
			public void Render(ViewContext viewContext, System.IO.TextWriter writer)
			{
				using (MiniProfiler.Current.Step("Render " + (isPartial ? "partial" : "") + ": " + name))
				{
					wrapped.Render(viewContext, writer);
				}
			}
		}

		IViewEngine wrapped;

		/// <summary>
		///
		/// </summary>
		/// <param name="wrapped"></param>
		public ProfilingViewEngine(IViewEngine wrapped)
		{
			this.wrapped = wrapped;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="controllerContext"></param>
		/// <param name="partialViewName"></param>
		/// <param name="useCache"></param>
		/// <returns></returns>
		public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
		{
			var found = wrapped.FindPartialView(controllerContext, partialViewName, useCache);
			if (found != null && found.View != null)
			{
				found = new ViewEngineResult(new WrappedView(found.View, partialViewName, isPartial: true), this);
			}
			return found;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="controllerContext"></param>
		/// <param name="viewName"></param>
		/// <param name="masterName"></param>
		/// <param name="useCache"></param>
		/// <returns></returns>
		public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
		{
			var found = wrapped.FindView(controllerContext, viewName, masterName, useCache);
			if (found != null && found.View != null)
			{
				found = new ViewEngineResult(new WrappedView(found.View, viewName, isPartial: false), this);
			}
			return found;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="controllerContext"></param>
		/// <param name="view"></param>
		public void ReleaseView(ControllerContext controllerContext, IView view)
		{
			wrapped.ReleaseView(controllerContext, view);
		}
	}
#endif
}