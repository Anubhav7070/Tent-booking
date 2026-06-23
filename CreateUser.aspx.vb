Imports System
Imports System.Web.UI
Public Class CreateUser
    Inherits Page
    Protected Sub Page_Load(s As Object, e As EventArgs) Handles Me.Load
        AuthHelper.RequireRole(Me,"SuperAdmin")
    End Sub
    Protected Sub btnSave_Click(s As Object, e As EventArgs)
        Dim result = UserService.CreateUser(txtEmpId.Text.Trim(), txtName.Text.Trim(), txtDept.Text.Trim(), txtDesig.Text.Trim(), txtEmail.Text.Trim(), txtPhone.Text.Trim(), txtAddress.Text.Trim(), txtPassword.Text, ddlRole.SelectedValue)
        If result.Success Then
            AuditService.Log(AuthHelper.GetCurrentUserId(Context),"CreateUser","User",txtEmpId.Text,String.Format("Created user {0} ({1}), Role: {2}", txtName.Text, txtEmpId.Text, ddlRole.SelectedValue),"",ddlRole.SelectedValue,Request.UserHostAddress)
            Session("Success") = result.Message
            Response.Redirect("Users.aspx")
        Else
            lblError.Text = result.Message : lblError.Visible=True
        End If
    End Sub
End Class
