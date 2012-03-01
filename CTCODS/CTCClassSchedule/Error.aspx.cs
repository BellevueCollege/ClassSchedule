using System;
using System.Web;

namespace CTCClassSchedule
{
	public partial class ScheduleError : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Exception exception = Application["LastError"] as Exception;
			bool recognized = false;
			string stackTrace;

			if (exception != null)
			{
				// Specific error messages checked for and enabled here
				Message_ValidationError.Visible = recognized = (exception is HttpRequestValidationException);

				stackTrace = exception.ToString();
			}
			else
			{
				stackTrace = "An error occurred, but no error information was found!";
			}

			Message_UnknownError.Visible = !recognized;
			StackTrace.Text = stackTrace;
		}
	}
}