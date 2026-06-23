Imports System
Imports System.Web.UI

Public Class Login
    Inherits Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        ' If already logged in, redirect appropriately
        If AuthHelper.IsAuthenticated(Context) Then
            Dim role As String = AuthHelper.GetCurrentRole(Context)
            If role = "SuperAdmin" OrElse role = "Admin" Then
                Response.Redirect("~/AdminDashboard.aspx")
            Else
                Response.Redirect("~/UserDashboard.aspx")
            End If
        End If
    End Sub

    Protected Sub btnLogin_Click(ByVal sender As Object, ByVal e As EventArgs)
        Dim employeeId As String = txtEmployeeId.Text.Trim()
        Dim password As String = txtPassword.Text

        Dim result = AuthHelper.Login(employeeId, password)

        If result.Success Then
            AuthHelper.SetSession(Context, result.User)

            ' Check if must change password
            If result.User.MustChangePassword Then
                Response.Redirect("~/ChangePassword.aspx?force=1")
                Return
            End If

            ' Role-based redirect
            Dim role As String = result.User.Role
            If role = "SuperAdmin" OrElse role = "Admin" Then
                Response.Redirect("~/AdminDashboard.aspx")
            Else
                Response.Redirect("~/UserDashboard.aspx")
            End If
        Else
            pnlError.Visible = True
            lblError.Text = result.Message
        End If
    End Sub
End Class
