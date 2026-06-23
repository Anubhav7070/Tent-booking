Imports System
Imports System.Web.UI
Public Class EditUser
    Inherits Page
    Protected Sub Page_Load(s As Object, e As EventArgs) Handles Me.Load
        AuthHelper.RequireRole(Me,"SuperAdmin")
        If Not IsPostBack Then
            Dim userId As String = Request.QueryString("id")
            If String.IsNullOrEmpty(userId) Then Response.Redirect("Users.aspx") : Return
            hfUserId.Value = userId
            Dim user = UserService.GetUserById(userId)
            If user Is Nothing Then Response.Redirect("Users.aspx") : Return
            txtEmpId.Text = user.EmployeeId : txtName.Text = user.FullName
            txtDept.Text = user.Department : txtEmail.Text = user.Email
            ddlRole.SelectedValue = user.Role
            Dim emp = UserService.GetUserByEmployeeId(user.EmployeeId)
        End If
    End Sub
    Protected Sub btnSave_Click(s As Object, e As EventArgs)
        If String.IsNullOrWhiteSpace(txtName.Text) Then lblError.Text="Name is required." : lblError.Visible=True : Return
        UserService.UpdateUser(hfUserId.Value, txtName.Text.Trim(), txtDept.Text.Trim(), txtDesig.Text.Trim(), txtEmail.Text.Trim(), txtPhone.Text.Trim(), txtAddress.Text.Trim(), ddlRole.SelectedValue)
        AuditService.Log(AuthHelper.GetCurrentUserId(Context),"UpdateUser","User",hfUserId.Value,String.Format("Updated user {0}", txtName.Text),"",ddlRole.SelectedValue,Request.UserHostAddress)
        Session("Success") = "User account updated."
        Response.Redirect("Users.aspx")
    End Sub
End Class
