Imports System
Imports System.Web.UI
Imports System.Web.UI.WebControls
Public Class Users
    Inherits Page
    Protected Sub Page_Load(s As Object, e As EventArgs) Handles Me.Load
        AuthHelper.RequireRole(Me,"SuperAdmin")
        If Not IsPostBack Then LoadUsers()
    End Sub
    Private Sub LoadUsers()
        rptUsers.DataSource = UserService.GetAllUsers() : rptUsers.DataBind()
    End Sub
    Protected Sub lnkToggle_Command(s As Object, e As CommandEventArgs)
        UserService.ToggleActive(e.CommandArgument.ToString())
        Session("Success") = "User status updated."
        Response.Redirect("Users.aspx")
    End Sub
    Protected Sub lnkReset_Command(s As Object, e As CommandEventArgs)
        UserService.ResetPassword(e.CommandArgument.ToString(),"Admin@123")
        Session("Success") = "Password reset to Admin@123. User must change on next login."
        Response.Redirect("Users.aspx")
    End Sub
    Protected Function GetRoleBadge(role As String) As String
        Select Case role
            Case "SuperAdmin" : Return "<span class=""badge bg-danger"">Super Admin</span>"
            Case "HOD" : Return "<span class=""badge bg-warning text-dark"">HOD</span>"
            Case "GM" : Return "<span class=""badge bg-info"">GM</span>"
            Case Else : Return "<span class=""badge bg-light text-dark border"">User</span>"
        End Select
    End Function
End Class
