<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="CTCClassSchedule.ScheduleError" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">

<head runat="server">
	<link rel="stylesheet" href="http://bellevuecollege.edu/globals/btheme_v0.1/css/btheme_v0.1.1.css" type="text/css" />
	<link rel="stylesheet" href="~/Content/grids.css" type="text/css" />
	<link rel="stylesheet" href="~/Content/automatic-image-slider.css" type="text/css" />
	<link rel="stylesheet" href="~/Content/jquery-ui-1.8.17.custom.css" type="text/css" />
	<link rel="stylesheet" href="~/Content/ClassSchedule.css?v=1" type="text/css" />

  <title>Oops, an error occurred</title>
</head>
<body>
	<!--#include file="/globals/btheme_v0.1/asp/header_big.asp" -->

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

<!-- Exception stack trace ******************************************************************************************

<asp:Literal ID="StackTrace" runat="server"></asp:Literal>

****************************************************************************************************************** -->
	  </div>
  </form>

	<!--#include file="/globals/btheme_v0.1/asp/footer_big.asp" -->
</body>
</html>
