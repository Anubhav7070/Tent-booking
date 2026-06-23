Imports System
Imports System.Collections.Generic

' ============================================================
'  Models.vb — Plain entity classes matching the DB schema
'  No EF attributes — pure POCOs for Web Forms use
' ============================================================

' ── ApplicationUser ───────────────────────────────────────────────────────────
Public Class ApplicationUser
    Public Property Id As String = String.Empty
    Public Property UserName As String = String.Empty
    Public Property Email As String = String.Empty
    Public Property FullName As String = String.Empty
    Public Property EmployeeId As String = String.Empty
    Public Property Department As String = String.Empty
    Public Property IsActive As Boolean = True
    Public Property MustChangePassword As Boolean = False
    Public Property CreatedAt As DateTime = DateTime.UtcNow
    Public Property LastLoginAt As DateTime?
    Public Property PasswordHash As String = String.Empty
    Public Property Role As String = "User"
End Class

' ── Employee ──────────────────────────────────────────────────────────────────
Public Class Employee
    Public Property EmployeeId As String = String.Empty
    Public Property EmployeeName As String = String.Empty
    Public Property Department As String = String.Empty
    Public Property Designation As String = String.Empty
    Public Property Email As String = String.Empty
    Public Property PhoneNumber As String = String.Empty
    Public Property QuarterAddress As String = String.Empty
    Public Property Status As String = "Active"
    Public Property CreatedDate As DateTime = DateTime.UtcNow
    Public Property UpdatedDate As DateTime?
End Class

' ── InventoryCategory ─────────────────────────────────────────────────────────
Public Class InventoryCategory
    Public Property Id As Integer
    Public Property Name As String = String.Empty
    Public Property Description As String = String.Empty
    Public Property CreatedAt As DateTime = DateTime.UtcNow
End Class

' ── InventoryItem ─────────────────────────────────────────────────────────────
Public Class InventoryItem
    Public Property Id As Integer
    Public Property Name As String = String.Empty
    Public Property Description As String = String.Empty
    Public Property CategoryId As Integer
    Public Property CategoryName As String = String.Empty
    Public Property UnitType As String = "Nos"
    Public Property TotalQuantity As Integer
    Public Property ReservedQuantity As Integer
    Public Property CurrentPrice As Decimal
    Public Property IsActive As Boolean = True
    Public Property ImagePath As String = String.Empty
    Public Property CreatedAt As DateTime = DateTime.UtcNow
    Public Property UpdatedAt As DateTime = DateTime.UtcNow
    Public ReadOnly Property AvailableQuantity As Integer
        Get
            Return TotalQuantity - ReservedQuantity
        End Get
    End Property
End Class

' ── PriceHistory ──────────────────────────────────────────────────────────────
Public Class PriceHistory
    Public Property Id As Integer
    Public Property InventoryItemId As Integer
    Public Property ItemName As String = String.Empty
    Public Property PreviousPrice As Decimal
    Public Property UpdatedPrice As Decimal
    Public Property EffectiveDate As DateTime
    Public Property Reason As String = String.Empty
    Public Property UpdatedBy As String = String.Empty
    Public Property UpdatedAt As DateTime = DateTime.UtcNow
    Public ReadOnly Property PriceDifference As Decimal
        Get
            Return UpdatedPrice - PreviousPrice
        End Get
    End Property
End Class

' ── RentalRequest ─────────────────────────────────────────────────────────────
Public Class RentalRequest
    Public Property Id As Integer
    Public Property RequestNumber As String = String.Empty
    Public Property UserId As String = String.Empty
    Public Property UserFullName As String = String.Empty
    Public Property UserEmployeeId As String = String.Empty
    Public Property UserDepartment As String = String.Empty
    Public Property SubmittedByRole As String = "User"
    Public Property EventDate As DateTime
    Public Property StartDate As DateTime
    Public Property EndDate As DateTime
    Public Property InPrincipalDocumentPath As String = String.Empty
    Public Property GrandTotal As Decimal
    Public Property Status As RequestStatus = RequestStatus.Pending
    Public Property ApprovalStage As ApprovalStage = ApprovalStage.PendingHOD
    Public Property HODApprovedAt As DateTime?
    Public Property HODApprovedByEmployeeId As String = String.Empty
    Public Property GMApprovedAt As DateTime?
    Public Property GMApprovedByEmployeeId As String = String.Empty
    Public Property HRApprovedAt As DateTime?
    Public Property HRApprovedByEmployeeId As String = String.Empty
    Public Property CreatedAt As DateTime = DateTime.UtcNow
    Public Property ReviewedAt As DateTime?
    Public Property ReviewedByEmployeeId As String = String.Empty
    Public Property RejectionReason As String = String.Empty
    Public Property Items As List(Of RentalRequestItem) = New List(Of RentalRequestItem)()
End Class

Public Enum RequestStatus
    Pending = 0
    Approved = 1
    Rejected = 2
    Cancelled = 3
    Waitlisted = 4
End Enum

Public Enum ApprovalStage
    PendingHOD = 0
    PendingGM = 1
    PendingHR = 2
    Approved = 3
    Rejected = 4
End Enum

' ── RentalRequestItem ─────────────────────────────────────────────────────────
Public Class RentalRequestItem
    Public Property Id As Integer
    Public Property RentalRequestId As Integer
    Public Property InventoryItemId As Integer
    Public Property ItemName As String = String.Empty
    Public Property UnitType As String = String.Empty
    Public Property RequestedQuantity As Integer
    Public Property UnitPriceAtRequest As Decimal
    Public ReadOnly Property LineTotal As Decimal
        Get
            Return RequestedQuantity * UnitPriceAtRequest
        End Get
    End Property
End Class

' ── InventoryAllocation ───────────────────────────────────────────────────────
Public Class InventoryAllocation
    Public Property AllocationId As Integer
    Public Property RequestId As Integer
    Public Property InventoryItemId As Integer
    Public Property AllocatedQuantity As Integer
    Public Property Status As String = "Reserved"
    Public Property AllocatedAt As DateTime = DateTime.UtcNow
End Class

' ── Notification ──────────────────────────────────────────────────────────────
Public Class AppNotification
    Public Property Id As Integer
    Public Property UserId As String = String.Empty
    Public Property Title As String = String.Empty
    Public Property Message As String = String.Empty
    Public Property IsRead As Boolean = False
    Public Property CreatedAt As DateTime = DateTime.UtcNow
End Class

' ── AuditLog ─────────────────────────────────────────────────────────────────
Public Class AuditLog
    Public Property Id As Integer
    Public Property UserId As String = String.Empty
    Public Property UserName As String = String.Empty
    Public Property Action As String = String.Empty
    Public Property EntityName As String = String.Empty
    Public Property EntityId As String = String.Empty
    Public Property Description As String = String.Empty
    Public Property OldValue As String = String.Empty
    Public Property NewValue As String = String.Empty
    Public Property IpAddress As String = String.Empty
    Public Property Timestamp As DateTime = DateTime.UtcNow
End Class

' ── Dashboard ViewModel ───────────────────────────────────────────────────────
Public Class DashboardViewModel
    Public Property TotalRequests As Integer
    Public Property PendingRequests As Integer
    Public Property ApprovedRequests As Integer
    Public Property RejectedRequests As Integer
    Public Property TotalRevenue As Decimal
    Public Property MonthlyRevenue As Decimal
    Public Property TotalInventoryItems As Integer
    Public Property LowStockItemCount As Integer
    Public Property RecentRequests As List(Of RentalRequest) = New List(Of RentalRequest)()
    Public Property LowStockItems As List(Of InventoryItem) = New List(Of InventoryItem)()
    Public Property MonthlyRevenueLabels As List(Of String) = New List(Of String)()
    Public Property MonthlyRevenueData As List(Of Decimal) = New List(Of Decimal)()
    Public Property BookingTrendLabels As List(Of String) = New List(Of String)()
    Public Property BookingTrendData As List(Of Integer) = New List(Of Integer)()
    Public Property TopItemNames As List(Of String) = New List(Of String)()
    Public Property TopItemUsage As List(Of Integer) = New List(Of Integer)()
    Public Property InventoryStatusLabels As List(Of String) = New List(Of String)()
    Public Property InventoryStatusData As List(Of Integer) = New List(Of Integer)()
End Class

' ── Report ViewModel ──────────────────────────────────────────────────────────
Public Class ReportViewModel
    Public Property ReportType As String = "Monthly"
    Public Property SelectedYear As Integer = DateTime.Today.Year
    Public Property SelectedMonth As Integer = DateTime.Today.Month
    Public Property StartDate As DateTime?
    Public Property EndDate As DateTime?
    Public Property Requests As List(Of RentalRequest) = New List(Of RentalRequest)()
    Public Property TotalAmount As Decimal
    Public Property TotalRequests As Integer
    Public Property ApprovedCount As Integer
    Public Property RejectedCount As Integer
    Public Property PendingCount As Integer
    Public Property ReportTitle As String = String.Empty
End Class

Public Class ServiceResult
    Public Property Success As Boolean
    Public Property Message As String
    
    Public Sub New(success As Boolean, message As String)
        Me.Success = success
        Me.Message = message
    End Sub
End Class

Public Class LogResult
    Public Property Logs As List(Of AuditLog)
    Public Property Total As Integer
    
    Public Sub New(logs As List(Of AuditLog), total As Integer)
        Me.Logs = logs
        Me.Total = total
    End Sub
End Class

Public Class LoginResult
    Public Property Success As Boolean
    Public Property Message As String
    Public Property User As ApplicationUser
    
    Public Sub New(success As Boolean, message As String, user As ApplicationUser)
        Me.Success = success
        Me.Message = message
        Me.User = user
    End Sub
End Class
