Imports System
Imports System.Web.UI
Public Class Notifications
    Inherits Page
    Protected Sub Page_Load(s As Object, e As EventArgs) Handles Me.Load
        AuthHelper.RequireLogin(Me)
        If Request.QueryString("markAll") = "1" Then
            NotificationService.MarkAllAsRead(AuthHelper.GetCurrentUserId(Context))
            Response.Redirect("Notifications.aspx")
        End If
        If Not IsPostBack Then LoadNotifs()
    End Sub
    Private Sub LoadNotifs()
        Dim userId As String = AuthHelper.GetCurrentUserId(Context)
        Dim notifs = NotificationService.GetUserNotifications(userId)
        rptNotifs.DataSource = notifs : rptNotifs.DataBind()
        lblNoData.Visible = (notifs.Count = 0)
    End Sub
    Protected Sub btnMarkAll_Click(s As Object, e As EventArgs)
        NotificationService.MarkAllAsRead(AuthHelper.GetCurrentUserId(Context))
        Response.Redirect("Notifications.aspx")
    End Sub
End Class
