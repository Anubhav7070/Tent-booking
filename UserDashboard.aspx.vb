Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SQLite
Imports System.Linq
Imports System.Web.UI

Public Class UserDashboard
    Inherits Page

    Public TodayReserved As New Dictionary(Of Integer, Integer)()

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        AuthHelper.RequireLogin(Me)
        If Not IsPostBack Then
            Dim userId As String = AuthHelper.GetCurrentUserId(Context)
            lblWelcome.Text = AuthHelper.GetCurrentFullName(Context)

            ' Fetch today's reserved stock mapping
            TodayReserved = InventoryService.GetTodayReserved()

            ' Load user requests
            Dim reqs = RentalService.GetUserRequests(userId)
            rptRequests.DataSource = reqs.Take(5).ToList()
            rptRequests.DataBind()
            phNoReqs.Visible = (reqs.Count = 0)

            ' Load available inventory
            Dim availInventory = InventoryService.GetAllItems().Where(Function(i) i.IsActive).ToList()
            rptAvailableInventory.DataSource = availInventory
            rptAvailableInventory.DataBind()
            lblInvCount.Text = availInventory.Count.ToString()
            phNoInventory.Visible = (availInventory.Count = 0)
        End If
    End Sub

    Protected Function GetAvailableQty(dataItem As Object) As Integer
        Dim item As InventoryItem = TryCast(dataItem, InventoryItem)
        If item Is Nothing Then Return 0
        Dim reserved As Integer = 0
        If TodayReserved.ContainsKey(item.Id) Then
            reserved = TodayReserved(item.Id)
        End If
        Return Math.Max(0, item.TotalQuantity - reserved)
    End Function

    Protected Function GetStatusBadge(dataItem As Object) As String
        Dim r As RentalRequest = TryCast(dataItem, RentalRequest)
        If r Is Nothing Then Return ""

        If r.Status = RequestStatus.Pending Then
            Dim stageStr As String = ""
            If r.ApprovalStage = ApprovalStage.PendingHOD Then
                stageStr = "Pending to HOD (" & AuthHelper.GetHODDisplayName(r.UserDepartment) & ")"
            ElseIf r.ApprovalStage = ApprovalStage.PendingGM Then
                stageStr = "Pending to GM (" & AuthHelper.GetGMDisplayName() & ")"
            ElseIf r.ApprovalStage = ApprovalStage.PendingHR Then
                stageStr = "Pending to HR (" & AuthHelper.GetHRDisplayName() & ")"
            Else
                stageStr = "Pending"
            End If
            Return "<span class=""badge badge-pending iocl-badge"">" & stageStr & "</span>"
        ElseIf r.Status = RequestStatus.Approved Then
            If r.InventoryReleased Then
                Return "<span class=""badge iocl-badge"" style=""background:#198754;color:#fff;""><i class=""bi bi-arrow-return-left me-1""></i>Returned — Stock Released</span>"
            Else
                Return "<span class=""badge badge-approved iocl-badge"">Approved</span>"
            End If
        ElseIf r.Status = RequestStatus.Cancelled Then
            Return "<span class=""badge bg-secondary text-white iocl-badge"">Cancelled</span>"
        Else
            Dim reviewer As String = GetUserDisplayNameByEmpId(r.ReviewedByEmployeeId)
            Dim titleText As String = If(Not String.IsNullOrEmpty(r.RejectionReason), " title=""" & System.Web.HttpUtility.HtmlAttributeEncode(r.RejectionReason) & """", "")
            If String.IsNullOrEmpty(reviewer) Then
                Return "<span class=""badge badge-rejected iocl-badge""" & titleText & ">Rejected</span>"
            Else
                Return "<span class=""badge badge-rejected iocl-badge""" & titleText & ">Rejected by " & reviewer & "</span>"
            End If
        End If
    End Function


    Private Function GetUserDisplayNameByEmpId(empId As String) As String
        If String.IsNullOrEmpty(empId) Then Return ""
        If empId = "SYSTEM" Then Return "SYSTEM"
        Dim sql As String = "SELECT u.FullName, r.Name AS RoleName FROM AspNetUsers u " &
                            "LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId " &
                            "LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id " &
                            "WHERE u.EmployeeId = @empId OR u.UserName = @empId LIMIT 1"
        Dim dt As DataTable = Database.ExecuteDataTable(sql, New SQLiteParameter("@empId", empId))
        If dt.Rows.Count > 0 Then
            Dim row As DataRow = dt.Rows(0)
            Dim roleName As String = If(row("RoleName") Is DBNull.Value, "", row("RoleName").ToString())
            Dim designation As String = "Employee"
            If roleName = "SuperAdmin" Then
                designation = "HR"
            ElseIf roleName = "GM" Then
                designation = "GM"
            ElseIf roleName = "HOD" Then
                designation = "HOD"
            End If
            Return designation & " - " & empId
        End If
        If empId.StartsWith("3") Then Return "HOD - " & empId
        If empId.StartsWith("2") Then Return "GM - " & empId
        If empId.StartsWith("0") Then Return "HR - " & empId
        Return "Employee - " & empId
    End Function
End Class
