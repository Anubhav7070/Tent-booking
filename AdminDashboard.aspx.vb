Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SQLite
Imports System.Linq
Imports System.Web.Script.Serialization
Imports System.Web.UI

Public Class AdminDashboard
    Inherits Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        AuthHelper.RequireRole(Me, "SuperAdmin", "Admin")
        If Not IsPostBack Then
            LoadDashboard()
        End If
    End Sub

    Private Sub LoadDashboard()
        Dim js As New JavaScriptSerializer()
        Dim allRequests As List(Of RentalRequest) = RentalService.GetAllRequests()
        Dim now As DateTime = DateTime.Now

        lblTotalRequests.Text = allRequests.Count.ToString()
        lblPendingRequests.Text = allRequests.Where(Function(r) r.Status = RequestStatus.Pending).Count().ToString()
        lblApprovedRequests.Text = allRequests.Where(Function(r) r.Status = RequestStatus.Approved).Count().ToString()
        lblRejectedRequests.Text = allRequests.Where(Function(r) r.Status = RequestStatus.Rejected).Count().ToString()

        Dim totalRevenue As Decimal = allRequests.Where(Function(r) r.Status = RequestStatus.Approved).Sum(Function(r) r.GrandTotal)
        Dim monthlyRevenue As Decimal = allRequests.Where(Function(r) r.Status = RequestStatus.Approved AndAlso r.CreatedAt.Month = now.Month AndAlso r.CreatedAt.Year = now.Year).Sum(Function(r) r.GrandTotal)
        lblTotalRevenue.Text = String.Format("{0:N2}", totalRevenue)
        lblMonthlyRevenue.Text = String.Format("{0:N2}", monthlyRevenue)

        Dim allItems As List(Of InventoryItem) = InventoryService.GetAllItems()
        Dim lowStockItems As List(Of InventoryItem) = InventoryService.GetLowStockItems()
        lblLowStock.Text = lowStockItems.Count.ToString()

        ' Monthly revenue chart (last 6 months)
        Dim revLabels As New List(Of String)()
        Dim revData As New List(Of Decimal)()
        Dim bookLabels As New List(Of String)()
        Dim bookData As New List(Of Integer)()
        For i As Integer = 5 To 0 Step -1
            Dim target As DateTime = now.AddMonths(-i)
            revLabels.Add(target.ToString("MMM yy"))
            revData.Add(allRequests.Where(Function(r) r.Status = RequestStatus.Approved AndAlso r.CreatedAt.Month = target.Month AndAlso r.CreatedAt.Year = target.Year).Sum(Function(r) r.GrandTotal))
            bookLabels.Add(target.ToString("MMM yy"))
            bookData.Add(allRequests.Where(Function(r) r.CreatedAt.Month = target.Month AndAlso r.CreatedAt.Year = target.Year).Count())
        Next

        hfRevenueLabels.Value = js.Serialize(revLabels)
        hfRevenueData.Value = js.Serialize(revData)
        hfBookingLabels.Value = js.Serialize(bookLabels)
        hfBookingData.Value = js.Serialize(bookData)

        ' Top 5 items by usage (from request items joined in memory)
        Dim topItems As New Dictionary(Of String, Integer)()
        For Each req In allRequests
            Dim items As List(Of RentalRequestItem) = RentalService.GetRequestItems(req.Id)
            For Each ri In items
                If topItems.ContainsKey(ri.ItemName) Then
                    topItems(ri.ItemName) += ri.RequestedQuantity
                Else
                    topItems(ri.ItemName) = ri.RequestedQuantity
                End If
            Next
        Next
        Dim top5 = topItems.OrderByDescending(Function(kv) kv.Value).Take(5).ToList()
        hfTopItemsLabels.Value = js.Serialize(top5.Select(Function(kv) kv.Key).ToList())
        hfTopItemsData.Value = js.Serialize(top5.Select(Function(kv) kv.Value).ToList())

        ' Inventory status
        Dim totalQty As Integer = allItems.Sum(Function(i) i.TotalQuantity)
        Dim reservedQty As Integer = allItems.Sum(Function(i) i.ReservedQuantity)
        hfInvStatusLabels.Value = js.Serialize(New List(Of String) From {"Available", "Reserved"})
        hfInvStatusData.Value = js.Serialize(New List(Of Integer) From {totalQty - reservedQty, reservedQty})

        ' Recent requests
        rptRecentRequests.DataSource = allRequests.Take(5).ToList()
        rptRecentRequests.DataBind()

        ' Low stock
        If lowStockItems.Count > 0 Then
            rptLowStock.DataSource = lowStockItems
            rptLowStock.DataBind()
            pnlLowStock.Visible = True
        Else
            pnlLowStock.Visible = False
        End If
    End Sub

    Protected Function GetStatusBadge(dataItem As Object) As String
        Dim r As RentalRequest = TryCast(dataItem, RentalRequest)
        If r Is Nothing Then Return ""

        If r.Status = RequestStatus.Pending Then
            Dim stageStr As String = ""
            If r.ApprovalStage = ApprovalStage.PendingHOD Then
                stageStr = "PENDING TO HOD (" & GetHODDisplayName(r.UserDepartment) & ")"
            ElseIf r.ApprovalStage = ApprovalStage.PendingGM Then
                stageStr = "PENDING TO GM (" & GetGMDisplayName() & ")"
            ElseIf r.ApprovalStage = ApprovalStage.PendingHR Then
                stageStr = "PENDING TO HR (" & GetHRDisplayName() & ")"
            Else
                stageStr = "PENDING"
            End If
            Return "<span class=""badge badge-pending iocl-badge"">" & stageStr & "</span>"
        ElseIf r.Status = RequestStatus.Approved Then
            Return "<span class=""badge badge-approved iocl-badge"">APPROVED</span>"
        ElseIf r.Status = RequestStatus.Cancelled Then
            Return "<span class=""badge bg-secondary text-white iocl-badge"">CANCELLED</span>"
        ElseIf r.Status = RequestStatus.Returned Then
            Return "<span class=""badge iocl-badge"" style=""background:#198754;color:#fff;""><i class=""bi bi-arrow-return-left me-1""></i>Returned — Stock Released</span>"
        Else
            Dim reviewer As String = GetUserDisplayNameByEmpId(r.ReviewedByEmployeeId)
            Dim titleText As String = If(Not String.IsNullOrEmpty(r.RejectionReason), " title=""" & System.Web.HttpUtility.HtmlAttributeEncode(r.RejectionReason) & """", "")
            If String.IsNullOrEmpty(reviewer) Then
                Return "<span class=""badge badge-rejected iocl-badge""" & titleText & ">REJECTED</span>"
            Else
                Return "<span class=""badge badge-rejected iocl-badge""" & titleText & ">Rejected by " & reviewer & "</span>"
            End If
        End If
    End Function

    Private Function GetHODDisplayName(dept As String) As String
        If String.IsNullOrEmpty(dept) Then Return ""
        Dim sql As String = "SELECT u.EmployeeId FROM AspNetUsers u " &
                            "JOIN AspNetUserRoles ur ON u.Id = ur.UserId " &
                            "JOIN AspNetRoles r ON ur.RoleId = r.Id " &
                            "WHERE r.Name = 'HOD' AND u.Department = @dept LIMIT 1"
        Dim result As Object = Database.ExecuteScalar(sql, New SQLiteParameter("@dept", dept))
        If result IsNot Nothing AndAlso Not IsDBNull(result) Then
            Return result.ToString()
        End If

        ' Fallback to any HOD if department HOD is not found
        Dim sqlFallback As String = "SELECT u.EmployeeId FROM AspNetUsers u " &
                                    "JOIN AspNetUserRoles ur ON u.Id = ur.UserId " &
                                    "JOIN AspNetRoles r ON ur.RoleId = r.Id " &
                                    "WHERE r.Name = 'HOD' LIMIT 1"
        Dim resultFallback As Object = Database.ExecuteScalar(sqlFallback)
        If resultFallback IsNot Nothing AndAlso Not IsDBNull(resultFallback) Then
            Return resultFallback.ToString()
        End If
        Return "HOD"
    End Function

    Private Function GetGMDisplayName() As String
        Dim sql As String = "SELECT u.EmployeeId FROM AspNetUsers u " &
                            "JOIN AspNetUserRoles ur ON u.Id = ur.UserId " &
                            "JOIN AspNetRoles r ON ur.RoleId = r.Id " &
                            "WHERE r.Name = 'GM' LIMIT 1"
        Dim result As Object = Database.ExecuteScalar(sql)
        If result IsNot Nothing AndAlso Not IsDBNull(result) Then
            Return result.ToString()
        End If
        Return "20000001"
    End Function

    Private Function GetHRDisplayName() As String
        Dim sql As String = "SELECT u.EmployeeId FROM AspNetUsers u " &
                            "JOIN AspNetUserRoles ur ON u.Id = ur.UserId " &
                            "JOIN AspNetRoles r ON ur.RoleId = r.Id " &
                            "WHERE r.Name = 'SuperAdmin' LIMIT 1"
        Dim result As Object = Database.ExecuteScalar(sql)
        If result IsNot Nothing AndAlso Not IsDBNull(result) Then
            Return result.ToString()
        End If
        Return "00000001"
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
