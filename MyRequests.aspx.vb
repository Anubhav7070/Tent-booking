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
            Case ApprovalStage.PendingHOD : Return "<span class=""badge bg-info"">Pending HOD (" & AuthHelper.GetHODDisplayName(r.UserDepartment) & ")</span>"
            Case ApprovalStage.PendingGM : Return "<span class=""badge bg-warning text-dark"">Pending GM (" & AuthHelper.GetGMDisplayName() & ")</span>"
            Case ApprovalStage.PendingHR : Return "<span class=""badge bg-primary"">Pending HR (" & AuthHelper.GetHRDisplayName() & ")</span>"
            Case ApprovalStage.Approved : Return "<span class=""badge bg-success"">All Approved</span>"
            Case ApprovalStage.Rejected : Return "<span class=""badge bg-danger"">Rejected</span>"
            Case Else : Return "<span class=""badge bg-secondary"">—</span>"
        End Select
    End Function

    Protected Sub rptRequests_ItemCommand(source As Object, e As RepeaterCommandEventArgs) Handles rptRequests.ItemCommand
        If e.CommandName = "CancelRequest" Then
            Try
                Dim reqId As Integer = Convert.ToInt32(e.CommandArgument)
                Dim username As String = AuthHelper.GetCurrentFullName(Context)
                
                Dim success As Boolean = RentalService.CancelRequest(reqId, username)
                If success Then
                    Dim userId As String = AuthHelper.GetCurrentUserId(Context)
                    AuditService.Log(userId, "Cancel", "RentalRequest", reqId.ToString(), "Rental request cancelled", "", "", Request.UserHostAddress)
                    NotificationService.CreateNotification(userId, "Request Cancelled", "Your rental request has been cancelled.")
                    Session("Success") = "Request cancelled successfully."
                Else
                    Session("Error") = "Failed to cancel request."
                End If
            Catch ex As Exception
                Session("Error") = "Error canceling request: " & ex.Message
            End Try
            Response.Redirect("MyRequests.aspx")
        End If
    End Sub
End Class
