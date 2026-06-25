Imports System
Imports System.Text
Imports System.Web.UI

Public Class AdminRequestDetails
    Inherits Page

    Private _requestId As Integer = 0

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        AuthHelper.RequireRole(Me, "SuperAdmin","HOD","GM","Admin")
        If Not Integer.TryParse(Request.QueryString("id"), _requestId) Then
            pnlNotFound.Visible = True : Return
        End If
        hfRequestId.Value = _requestId.ToString()
        If Not IsPostBack Then LoadDetails()
    End Sub

    Private Sub LoadDetails()
        Dim req As RentalRequest = RentalService.GetRequestById(_requestId)
        If req Is Nothing Then pnlNotFound.Visible = True : Return
        pnlMain.Visible = True

        lblRequestNumber.Text = req.RequestNumber
        lblStageBadge.Text = GetStageBadge(CInt(req.ApprovalStage))
        lblStatusBadge.Text = GetStatusBadge(req)
        lblCreatedAt.Text = req.CreatedAt.ToString("dd MMM yyyy, HH:mm")
        lblEmployee.Text = req.UserFullName
        lblEmpId.Text = req.UserEmployeeId
        lblDept.Text = req.UserDepartment
        lblRole.Text = req.SubmittedByRole
        lblEventDate.Text = req.EventDate.ToString("dd MMM yyyy")
        lblStartDate.Text = req.StartDate.ToString("dd MMM yyyy")
        lblEndDate.Text = req.EndDate.ToString("dd MMM yyyy")
        lblGrandTotal.Text = String.Format("{0:N2}", req.GrandTotal)

        rptItems.DataSource = req.Items
        rptItems.DataBind()

        If Not String.IsNullOrEmpty(req.InPrincipalDocumentPath) Then
            pnlDocument.Visible = True
            hlDocument.NavigateUrl = req.InPrincipalDocumentPath
        End If

        If req.Status = RequestStatus.Rejected AndAlso Not String.IsNullOrEmpty(req.RejectionReason) Then
            pnlRejection.Visible = True
            lblRejectionReason.Text = req.RejectionReason
        End If

        ' Build timeline
        Dim sb As New StringBuilder()
        sb.Append("<div class=""timeline-list"">")
        sb.Append(String.Format("<div class=""timeline-item""><span class=""badge bg-secondary me-2"">Submitted</span>{0:dd MMM yyyy, HH:mm} by {1} ({2})</div>", req.CreatedAt, req.UserFullName, req.SubmittedByRole))
        If req.HODApprovedAt.HasValue Then sb.Append(String.Format("<div class=""timeline-item""><span class=""badge bg-info me-2"">HOD Approved</span>{0:dd MMM yyyy, HH:mm} by EmpID {1}</div>", req.HODApprovedAt.Value, req.HODApprovedByEmployeeId))
        If req.GMApprovedAt.HasValue Then sb.Append(String.Format("<div class=""timeline-item""><span class=""badge bg-warning text-dark me-2"">GM Approved</span>{0:dd MMM yyyy, HH:mm} by EmpID {1}</div>", req.GMApprovedAt.Value, req.GMApprovedByEmployeeId))
        If req.HRApprovedAt.HasValue Then sb.Append(String.Format("<div class=""timeline-item""><span class=""badge bg-success me-2"">HR Approved</span>{0:dd MMM yyyy, HH:mm} by EmpID {1}</div>", req.HRApprovedAt.Value, req.HRApprovedByEmployeeId))
        sb.Append("</div>")
        litTimeline.Text = sb.ToString()

        ' Show approve/reject only if request is pending and not self
        Dim currentUserId As String = AuthHelper.GetCurrentUserId(Context)
        Dim currentRole As String = AuthHelper.GetCurrentRole(Context)
        Dim isSelf As Boolean = (req.UserId = currentUserId)
        If isSelf Then pnlSelfWarning.Visible = True

        Dim canAct As Boolean = Not isSelf AndAlso req.Status = RequestStatus.Pending AndAlso
            ((currentRole = "HOD" AndAlso req.ApprovalStage = ApprovalStage.PendingHOD) OrElse
             (currentRole = "GM" AndAlso req.ApprovalStage = ApprovalStage.PendingGM) OrElse
             (currentRole = "SuperAdmin" AndAlso (req.ApprovalStage = ApprovalStage.PendingHR OrElse req.ApprovalStage = ApprovalStage.PendingHOD)))
        pnlActions.Visible = canAct
    End Sub

    Protected Sub btnApprove_Click(s As Object, e As EventArgs)
        Integer.TryParse(hfRequestId.Value, _requestId)
        Dim userId As String = AuthHelper.GetCurrentUserId(Context)
        Dim role As String = AuthHelper.GetCurrentRole(Context)
        Dim result = RentalService.ApproveRequest(_requestId, userId, role)
        AuditService.Log(userId, "Approve", "RentalRequest", _requestId.ToString(), result.Message, "", "", Request.UserHostAddress)
        Session("Success") = If(result.Success, "✓ " & result.Message, Nothing)
        Session("Error") = If(Not result.Success, result.Message, Nothing)
        Response.Redirect(Request.RawUrl)
    End Sub

    Protected Sub btnReject_Click(s As Object, e As EventArgs)
        Integer.TryParse(hfRequestId.Value, _requestId)
        If String.IsNullOrWhiteSpace(txtRejectReason.Text) Then
            lblActionError.Text = "Rejection reason is required." : lblActionError.Visible = True : Return
        End If
        Dim userId As String = AuthHelper.GetCurrentUserId(Context)
        Dim empId As String = AuthHelper.GetCurrentEmployeeId(Context)
        RentalService.RejectRequest(_requestId, txtRejectReason.Text.Trim(), empId)
        AuditService.Log(userId, "Reject", "RentalRequest", _requestId.ToString(), "Rejected. Reason: " & txtRejectReason.Text, "", "Rejected", Request.UserHostAddress)
        Session("Success") = "Request rejected."
        Response.Redirect(Request.RawUrl)
    End Sub

    Protected Function GetStatusBadge(req As RentalRequest) As String
        If req.Status = RequestStatus.Approved Then
            If req.InventoryReleased Then
                Return "<span class=""badge"" style=""background:#198754;color:#fff;""><i class=""bi bi-arrow-return-left me-1""></i>Returned — Stock Released</span>"
            Else
                Return "<span class=""badge bg-success"">Approved</span>"
            End If
        End If
        Select Case req.Status
            Case RequestStatus.Rejected : Return "<span class=""badge bg-danger"">Rejected</span>"
            Case RequestStatus.Cancelled : Return "<span class=""badge bg-secondary"">Cancelled</span>"
            Case RequestStatus.Returned : Return "<span class=""badge"" style=""background:#198754;color:#fff;""><i class=""bi bi-arrow-return-left me-1""></i>Returned — Stock Released</span>"
            Case Else : Return "<span class=""badge bg-warning text-dark"">Pending</span>"
        End Select
    End Function

    Protected Function GetStageBadge(stage As Integer) As String
        Select Case CType(stage, ApprovalStage)
            Case ApprovalStage.PendingHOD : Return "<span class=""badge bg-info"">HOD Review</span>"
            Case ApprovalStage.PendingGM : Return "<span class=""badge bg-warning text-dark"">GM Review</span>"
            Case ApprovalStage.PendingHR : Return "<span class=""badge bg-primary"">HR Review</span>"
            Case ApprovalStage.Approved : Return "<span class=""badge bg-success"">Fully Approved</span>"
            Case ApprovalStage.Rejected : Return "<span class=""badge bg-danger"">Rejected</span>"
            Case Else : Return ""
        End Select
    End Function
End Class
