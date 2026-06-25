Imports System
Imports System.Web.UI
Imports System.Collections.Generic
Imports System.Data.SQLite

Public Class SiteMaster
    Inherits MasterPage

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If AuthHelper.IsAuthenticated(Context) Then
            Dim userId As String = AuthHelper.GetCurrentUserId(Context)

            ' Force password change redirect if flag is set
            Dim mustChange As Boolean = False
            Dim resultObj As Object = Database.ExecuteScalar("SELECT MustChangePassword FROM AspNetUsers WHERE Id=@id", New SQLiteParameter("@id", userId))
            If resultObj IsNot Nothing AndAlso Not IsDBNull(resultObj) Then
                mustChange = Convert.ToBoolean(resultObj)
            End If

            If mustChange Then
                Dim currentPage As String = System.IO.Path.GetFileName(Request.PhysicalPath)
                If Not String.Equals(currentPage, "ChangePassword.aspx", StringComparison.OrdinalIgnoreCase) Then
                    Response.Redirect("~/ChangePassword.aspx?force=1")
                    Return
                End If
            End If

            ' Trigger auto-release check on page load to ensure stock availability updates dynamically
            Try
                RentalService.ReleaseExpiredAllocations()
            Catch ex As Exception
                ' Silent catch: do not block page load due to release errors
            End Try

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
