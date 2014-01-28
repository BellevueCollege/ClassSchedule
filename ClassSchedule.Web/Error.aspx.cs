/*
This file is part of CtcClassSchedule.

CtcClassSchedule is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

CtcClassSchedule is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with CtcClassSchedule.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Common.Logging;

namespace CTCClassSchedule
{
	public partial class ScheduleError : Page
	{
	  private readonly ILog _log = LogManager.GetCurrentClassLogger();

	  protected void Page_Load(object sender, EventArgs e)
		{
			Exception exception = Application["LastError"] as Exception;

			if (exception != null)
			{
				// Specific error messages checked for and enabled here
				Message_ValidationError.Visible = (exception is HttpRequestValidationException);
				Message_DatabaseError.Visible = (exception is SqlException || exception is DataException);

        // If an exception waas thrown, it will be recorded/reported by ELMAH
      }
			else
			{
			  _log.Error(m => m("Error.aspx is being displayed, but no exception was detected."));
			}

			// Display a generic error message if Exception is not one of those specified above.
			Message_UnknownError.Visible = Controls.OfType<Panel>().Where(p => p.ID.StartsWith("Message_") && p.Visible).Count() <= 0;
		}
	}
}