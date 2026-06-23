Imports System
Imports System.Web.UI
Public Class ChangePassword
    Inherits Page
    Protected Sub Page_Load(s As Object, e As EventArgs) Handles Me.Load
        AuthHelper.RequireLogin(Me)
        If Request.QueryString("force") = "1" Then pnlForced.Visible = True
    End Sub
    Protected Sub btnChange_Click(s As Object, e As EventArgs)
        If txtNew.Text <> txtConfirm.Text Then
            lblError.Text = "New password and confirm password do not match." : lblError.Visible=True : Return
        End If
        Dim userId As String = AuthHelper.GetCurrentUserId(Context)
        Dim result = UserService.ChangePassword(userId, txtCurrent.Text, txtNew.Text)
        If result.Success Then
            Session("Success") = "Password changed successfully."
            Dim role As String = AuthHelper.GetCurrentRole(Context)
            Response.Redirect(If(role="SuperAdmin" OrElse role="Admin", "AdminDashboard.aspx","UserDashboard.aspx"))
        Else
            lblError.Text = result.Message : lblError.Visible=True
        End If
    End Sub
End Class
