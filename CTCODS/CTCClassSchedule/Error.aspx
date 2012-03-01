<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="CTCClassSchedule.ScheduleError" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Oops, an error occurred</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
		<%-- For additional error messages, create a new invisible Panel and activate it in the code behind --%>
		<asp:Panel ID="Message_ValidationError" runat="server" Visible="false">
			<h3>Invalid characters were detected. Please do not include HTML and/or other scripting when entering data.</h3>
		</asp:Panel>
		<asp:Panel ID="Message_UnknownError" runat="server" Visible="false">
			<h3>An unexpected error occurred.</h3>
		</asp:Panel>
<!--
<asp:Literal ID="StackTrace" runat="server"></asp:Literal>
-->
    </div>
    </form>
</body>
</html>
