using System;

namespace CTCClassSchedule
{
	public partial class ScheduleError : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (Application["LastError"] != null)
			{
				ErrorMessage.Text = (Application["LastError"] as Exception).ToString();
			}
			else
			{
				ErrorMessage.Text = "An error occurred, but no error information was found!";
			}
		}
	}
}