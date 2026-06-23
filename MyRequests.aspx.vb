Imports System
Imports System.Web.UI
Public Class MyRequests
    Inherits Page
    Protected Sub Page_Load(s As Object, e As EventArgs) Handles Me.Load
        AuthHelper.RequireLogin(Me)
        If Not IsPostBack Then
            Dim userId As String = AuthHelper.GetCurrentUserId(Context)
            Dim reqs = RentalService.GetUserRequests(userId)
            rptRequests.DataSource = reqs : rptRequests.DataBind()
            lblNoData.Visible = (reqs.Count = 0)
        End If
    End Sub
    Protected Function GetStatusBadge(status As Integer) As String
        Select Case CType(status, RequestStatus)
            Case RequestStatus.Approved : Return "<span class=""badge bg-success"">Approved</span>"
            Case RequestStatus.Rejected : Return "<span class=""badge bg-danger"">Rejected</span>"
            Case RequestStatus.Cancelled : Return "<span class=""badge bg-secondary"">Cancelled</span>"
            Case Else : Return "<span class=""badge bg-warning text-dark"">Pending</span>"
        End Select
    End Function
    Protected Function GetStageBadge(stage As Integer) As String
        Select Case CType(stage, ApprovalStage)
            Case ApprovalStage.PendingHOD : Return "<span class=""badge bg-info"">Pending HOD</span>"
            Case ApprovalStage.PendingGM : Return "<span class=""badge bg-warning text-dark"">Pending GM</span>"
            Case ApprovalStage.PendingHR : Return "<span class=""badge bg-primary"">Pending HR</span>"
            Case ApprovalStage.Approved : Return "<span class=""badge bg-success"">All Approved</span>"
            Case ApprovalStage.Rejected : Return "<span class=""badge bg-danger"">Rejected</span>"
            Case Else : Return "<span class=""badge bg-secondary"">—</span>"
        End Select
    End Function
End Class
