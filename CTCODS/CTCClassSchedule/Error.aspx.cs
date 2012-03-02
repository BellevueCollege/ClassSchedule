using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CTCClassSchedule
{
	public partial class ScheduleError : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Exception exception = Application["LastError"] as Exception;
			string stackTrace;

			if (exception != null)
			{
				// Specific error messages checked for and enabled here
				Message_ValidationError.Visible = (exception is HttpRequestValidationException);
				Message_DatabaseError.Visible = (exception is SqlException || exception is DataException);

				stackTrace = exception.ToString();
			}
			else
			{
				stackTrace = "An error occurred, but no error information was found!";
			}

			// Display a generic error message if Exception is not one of those specified above.
			Message_UnknownError.Visible = Controls.OfType<Panel>().Where(p => p.ID.StartsWith("Message_") && p.Visible).Count() <= 0;

			// Display the detailed Exception dump
			StackTrace.Text = stackTrace;
		}
	}
}