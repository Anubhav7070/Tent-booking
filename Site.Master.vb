Imports System
Imports System.Web.UI
Imports System.Collections.Generic

Public Class SiteMaster
    Inherits MasterPage

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If AuthHelper.IsAuthenticated(Context) Then
            Dim userId As String = AuthHelper.GetCurrentUserId(Context)
            lblUserName.Text = AuthHelper.GetCurrentFullName(Context)
            Dim role As String = AuthHelper.GetCurrentRole(Context)
            lblUserRole.Text = GetRoleLabel(role)
            If Session("Email") IsNot Nothing Then lblEmail.Text = Session("Email").ToString()
            If Session("Department") IsNot Nothing Then lblDept.Text = Session("Department").ToString()
            Dim name As String = AuthHelper.GetCurrentFullName(Context)
            lblInitials.Text = If(name.Length > 0, name.Substring(0, 1).ToUpper(), "U")

            ' Load notifications
            Dim notifs As List(Of AppNotification) = NotificationService.GetUserNotifications(userId)
            Dim unread As Integer = NotificationService.GetUnreadCount(userId)
            rptNotifs.DataSource = notifs.Take(5).ToList()
            rptNotifs.DataBind()
            If unread > 0 Then
                lblNotifBadge.Text = If(unread > 99, "99+", unread.ToString())
                lblNotifBadge.Visible = True
            End If
        End If
    End Sub

    Protected Sub lnkLogout_Click(ByVal sender As Object, ByVal e As EventArgs)
        AuthHelper.Logout(Context)
        Response.Redirect("~/Login.aspx")
    End Sub

    Public Function GetActiveCss(pageName As String) As String
        Dim currentPage As String = System.IO.Path.GetFileName(Request.PhysicalPath)
        If String.Equals(currentPage, pageName, StringComparison.OrdinalIgnoreCase) Then
            Return "active-link"
        End If
        Return ""
    End Function

    Private Function GetRoleLabel(role As String) As String
        Select Case role
            Case "SuperAdmin" : Return "Super Admin"
            Case "GM" : Return "General Manager"
            Case "HOD" : Return "Head of Department"
            Case Else : Return "Employee"
        End Select
    End Function
End Class
