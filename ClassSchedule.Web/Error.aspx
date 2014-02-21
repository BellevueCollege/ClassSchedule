<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="CTCClassSchedule.ScheduleError" %>
<%@ Import Namespace="CTCClassSchedule.Common" %>
<%--
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
--%>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
	<link rel="stylesheet" href="<%=Helpers.GlobalsHttp("btheme_v0.1/css/btheme_v0.1.1.css") %>" type="text/css" />
	<link rel="stylesheet" href="~/Content/grids.css" type="text/css" />
	<link rel="stylesheet" href="~/Content/automatic-image-slider.css" type="text/css" />
	<link rel="stylesheet" href="~/Content/jquery-ui-1.8.17.custom.css" type="text/css" />
	<link rel="stylesheet" href="~/Content/ClassSchedule.css?v=2" type="text/css" />

  <title>Oops, an error occurred</title>
</head>
<body>
  <%=Html.Include(Server, Server.MapPath(Helpers.GlobalsFile("btheme_v0.1/asp/header_big.asp"))) %>

  <form id="form1" runat="server">
		<div id="bodyWrapper">

			<h2>Oops, an error has occurred</h2>

			<%--
				For additional error messages, create a new invisible Panel and activate it in the code behind.
				NOTE: Panel messages cannot be nested and ID must start with "Message_..."
			--%>
			<asp:Panel ID="Message_ValidationError" runat="server" Visible="false">
				<h3>Invalid characters were detected. Please do not include HTML and/or other scripting when entering data.</h3>
			</asp:Panel>

			<asp:Panel ID="Message_DatabaseError" runat="server" Visible="false">
				<h3>There was a problem communicating with the database. If you continue to experience this error, please contact the Help Desk.</h3>
			</asp:Panel>

			<asp:Panel ID="Message_UnknownError" runat="server" Visible="false">
				<h3>(Unknown)</h3>
			</asp:Panel>

      <div>
        If this error persists please <a href="https://bellevuecollege.edu/requestcenter/Requests/NewRequest.aspx?CategoryID=233&TaskTypeID=382">submit a ticket</a>.
      </div>
	  </div>
  </form>

  <%=Html.Include(Server, Server.MapPath(Helpers.GlobalsFile("btheme_v0.1/asp/footer_big.asp"))) %>
</body>
</html>
