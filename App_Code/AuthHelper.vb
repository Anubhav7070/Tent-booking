Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SQLite
Imports System.Web

' ============================================================
'  AuthHelper.vb — Session-based authentication helper
'  Replaces ASP.NET Core Identity / SignInManager
' ============================================================
Public Class AuthHelper
    ' Session key constants
    Public Const SESSION_USER_ID As String = "UserId"
    Public Const SESSION_USER_NAME As String = "UserName"
    Public Const SESSION_FULL_NAME As String = "FullName"
    Public Const SESSION_EMPLOYEE_ID As String = "EmployeeId"
    Public Const SESSION_ROLE As String = "UserRole"
    Public Const SESSION_DEPARTMENT As String = "Department"
    Public Const SESSION_EMAIL As String = "Email"

    ' Login: validate credentials and populate session
    Public Shared Function Login(employeeId As String, password As String) As LoginResult
        If String.IsNullOrWhiteSpace(employeeId) OrElse String.IsNullOrWhiteSpace(password) Then
            Return New LoginResult(False, "Employee ID and password are required.", Nothing)
        End If

        Dim sql As String = "SELECT u.Id, u.UserName, u.Email, u.FullName, u.EmployeeId, u.Department, u.IsActive, u.MustChangePassword, u.PasswordHash, r.Name AS Role " &
                            "FROM AspNetUsers u " &
                            "LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId " &
                            "LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id " &
                            "WHERE u.UserName = @eid OR u.EmployeeId = @eid"

        Dim dt As DataTable = Database.ExecuteDataTable(sql,
            New SQLiteParameter("@eid", employeeId))

        If dt.Rows.Count = 0 Then
            Return New LoginResult(False, "Invalid credentials or account is inactive.", Nothing)
        End If

        Dim row As DataRow = dt.Rows(0)
        Dim user As New ApplicationUser With {
            .Id = row("Id").ToString(),
            .UserName = row("UserName").ToString(),
            .Email = row("Email").ToString(),
            .FullName = row("FullName").ToString(),
            .EmployeeId = row("EmployeeId").ToString(),
            .Department = row("Department").ToString(),
            .IsActive = Convert.ToBoolean(row("IsActive")),
            .MustChangePassword = Convert.ToBoolean(row("MustChangePassword")),
            .PasswordHash = row("PasswordHash").ToString(),
            .Role = If(row("Role") Is DBNull.Value, "User", row("Role").ToString())
        }

        If Not user.IsActive Then
            Return New LoginResult(False, "Your account is inactive. Please contact administrator.", Nothing)
        End If

        If Not PasswordHelper.VerifyPassword(password, user.PasswordHash) Then
            Return New LoginResult(False, "Invalid Employee ID or password.", Nothing)
        End If

        ' Update last login timestamp
        Database.ExecuteNonQuery("UPDATE AspNetUsers SET LastLoginAt=@dt WHERE Id=@id",
            New SQLiteParameter("@dt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")),
            New SQLiteParameter("@id", user.Id))

        Return New LoginResult(True, "Login successful.", user)
    End Function

    ' Set session variables after successful login
    Public Shared Sub SetSession(ctx As HttpContext, user As ApplicationUser)
        ctx.Session(SESSION_USER_ID) = user.Id
        ctx.Session(SESSION_USER_NAME) = user.UserName
        ctx.Session(SESSION_FULL_NAME) = user.FullName
        ctx.Session(SESSION_EMPLOYEE_ID) = user.EmployeeId
        ctx.Session(SESSION_ROLE) = user.Role
        ctx.Session(SESSION_DEPARTMENT) = user.Department
        ctx.Session(SESSION_EMAIL) = user.Email
    End Sub

    ' Clear all session data
    Public Shared Sub Logout(ctx As HttpContext)
        ctx.Session.Clear()
        ctx.Session.Abandon()
    End Sub

    ' Check if user is authenticated
    Public Shared Function IsAuthenticated(ctx As HttpContext) As Boolean
        Return ctx.Session(SESSION_USER_ID) IsNot Nothing
    End Function

    ' Get current user ID from session
    Public Shared Function GetCurrentUserId(ctx As HttpContext) As String
        If ctx.Session(SESSION_USER_ID) IsNot Nothing Then Return ctx.Session(SESSION_USER_ID).ToString()
        Return String.Empty
    End Function

    ' Get current user role
    Public Shared Function GetCurrentRole(ctx As HttpContext) As String
        If ctx.Session(SESSION_ROLE) IsNot Nothing Then Return ctx.Session(SESSION_ROLE).ToString()
        Return String.Empty
    End Function

    ' Get current employee ID
    Public Shared Function GetCurrentEmployeeId(ctx As HttpContext) As String
        If ctx.Session(SESSION_EMPLOYEE_ID) IsNot Nothing Then Return ctx.Session(SESSION_EMPLOYEE_ID).ToString()
        Return String.Empty
    End Function

    ' Get current user full name
    Public Shared Function GetCurrentFullName(ctx As HttpContext) As String
        If ctx.Session(SESSION_FULL_NAME) IsNot Nothing Then Return ctx.Session(SESSION_FULL_NAME).ToString()
        Return String.Empty
    End Function

    ' Is the current user a SuperAdmin
    Public Shared Function IsSuperAdmin(ctx As HttpContext) As Boolean
        Return GetCurrentRole(ctx) = "SuperAdmin"
    End Function

    ' Is the current user an approver (SuperAdmin, HOD, or GM)
    Public Shared Function IsApprover(ctx As HttpContext) As Boolean
        Dim role As String = GetCurrentRole(ctx)
        Return role = "SuperAdmin" OrElse role = "HOD" OrElse role = "GM"
    End Function

    ' Require login — redirect to Login.aspx if not authenticated
    Public Shared Sub RequireLogin(page As System.Web.UI.Page)
        If Not IsAuthenticated(HttpContext.Current) Then
            page.Response.Redirect("~/Login.aspx", True)
        End If
    End Sub

    ' Require specific roles — redirect to AccessDenied.aspx if not in role
    Public Shared Sub RequireRole(page As System.Web.UI.Page, ParamArray allowedRoles() As String)
        RequireLogin(page)
        Dim currentRole As String = GetCurrentRole(HttpContext.Current)
        For Each r As String In allowedRoles
            If currentRole = r Then Return
        Next
        page.Response.Redirect("~/AccessDenied.aspx", True)
    End Sub

    ' Get full ApplicationUser object from session/DB
    Public Shared Function GetCurrentUser(ctx As HttpContext) As ApplicationUser
        Dim userId As String = GetCurrentUserId(ctx)
        If String.IsNullOrEmpty(userId) Then Return Nothing
        Return UserService.GetUserById(userId)
    End Function
End Class
