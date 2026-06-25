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

    Protected Function GetStatusBadge(dataItem As Object) As String
        Dim r As RentalRequest = TryCast(dataItem, RentalRequest)
        If r Is Nothing Then Return ""
        If r.Status = RequestStatus.Approved Then
            If r.InventoryReleased Then
                Return "<span class=""badge"" style=""background:#198754;color:#fff;""><i class=""bi bi-arrow-return-left me-1""></i>Returned — Stock Released</span>"
            Else
                Return "<span class=""badge bg-success"">Approved</span>"
            End If
        End If
        Select Case r.Status
            Case RequestStatus.Rejected : Return "<span class=""badge bg-danger"">Rejected</span>"
            Case RequestStatus.Cancelled : Return "<span class=""badge bg-secondary"">Cancelled</span>"
            Case RequestStatus.Returned : Return "<span class=""badge"" style=""background:#198754;color:#fff;""><i class=""bi bi-arrow-return-left me-1""></i>Returned — Stock Released</span>"
            Case Else : Return "<span class=""badge bg-warning text-dark"">Pending</span>"
        End Select
    End Function

    Protected Function GetStageBadge(dataItem As Object) As String
        Dim r As RentalRequest = TryCast(dataItem, RentalRequest)
        If r Is Nothing Then Return ""
        Select Case r.ApprovalStage
            Case ApprovalStage.PendingHOD : Return "<span class=""badge bg-info"">HOD Review (" & AuthHelper.GetHODDisplayName(r.UserDepartment) & ")</span>"
            Case ApprovalStage.PendingGM : Return "<span class=""badge bg-warning text-dark"">GM Review (" & AuthHelper.GetGMDisplayName() & ")</span>"
            Case ApprovalStage.PendingHR : Return "<span class=""badge bg-primary"">HR Review (" & AuthHelper.GetHRDisplayName() & ")</span>"
            Case ApprovalStage.Approved : Return "<span class=""badge bg-success"">Complete</span>"
            Case ApprovalStage.Rejected : Return "<span class=""badge bg-danger"">Rejected</span>"
            Case Else : Return "<span class=""badge bg-light text-dark"">—</span>"
        End Select
    End Function
End Class
