Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web.UI

Public Class AdminRequests
    Inherits Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        AuthHelper.RequireRole(Me, "SuperAdmin","HOD","GM","Admin")
        If Not IsPostBack Then LoadRequests()
    End Sub

    Private Sub LoadRequests()
        Dim role As String = AuthHelper.GetCurrentRole(Context)
        Dim userId As String = AuthHelper.GetCurrentUserId(Context)
        Dim all As List(Of RentalRequest) = RentalService.GetAllRequests()
        Dim requests As IEnumerable(Of RentalRequest) = all

        If role = "HOD" Then
            ' HOD sees all requests (can only approve PendingHOD ones)
        ElseIf role = "GM" Then
            requests = all.Where(Function(r) r.SubmittedByRole = "GM" OrElse (r.GrandTotal > 10000 AndAlso r.SubmittedByRole <> "GM"))
        ElseIf role = "SuperAdmin" OrElse role = "Admin" Then
            ' See all requests where total<=10000 and not pending HOD/GM
        End If

        ' Status filter
        If Not String.IsNullOrEmpty(ddlStatus.SelectedValue) Then
            Dim statusVal As Integer = Integer.Parse(ddlStatus.SelectedValue)
            requests = requests.Where(Function(r) CInt(r.Status) = statusVal)
        End If

        ' Search filter
        Dim q As String = txtSearch.Text.Trim().ToLower()
        If Not String.IsNullOrEmpty(q) Then
            requests = requests.Where(Function(r) r.RequestNumber.ToLower().Contains(q) OrElse r.UserFullName.ToLower().Contains(q))
        End If

        Dim list As List(Of RentalRequest) = requests.ToList()
        rptRequests.DataSource = list
        rptRequests.DataBind()
        lblNoData.Visible = (list.Count = 0)
    End Sub

    Protected Sub btnFilter_Click(s As Object, e As EventArgs)
        LoadRequests()
    End Sub
    Protected Sub btnClear_Click(s As Object, e As EventArgs)
        ddlStatus.SelectedIndex = 0
        txtSearch.Text = ""
        LoadRequests()
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
            Case ApprovalStage.PendingHOD : Return "<span class=""badge bg-info"">HOD Review</span>"
            Case ApprovalStage.PendingGM : Return "<span class=""badge bg-warning text-dark"">GM Review</span>"
            Case ApprovalStage.PendingHR : Return "<span class=""badge bg-primary"">HR Review</span>"
            Case ApprovalStage.Approved : Return "<span class=""badge bg-success"">Complete</span>"
            Case ApprovalStage.Rejected : Return "<span class=""badge bg-danger"">Rejected</span>"
            Case Else : Return "<span class=""badge bg-light text-dark"">—</span>"
        End Select
    End Function
End Class
