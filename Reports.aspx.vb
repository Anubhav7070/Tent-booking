Imports System
Imports System.Web.UI
Public Class Reports
    Inherits Page
    Protected Sub Page_Load(s As Object, e As EventArgs) Handles Me.Load
        AuthHelper.RequireRole(Me,"SuperAdmin","Admin")
        If Not IsPostBack Then
            For y As Integer = DateTime.Now.Year To DateTime.Now.Year - 5 Step -1
                ddlYear.Items.Add(y.ToString())
            Next
            ddlMonth.SelectedValue = DateTime.Now.Month.ToString()
            txtFrom.Text = DateTime.Today.AddMonths(-1).ToString("yyyy-MM-dd")
            txtTo.Text = DateTime.Today.ToString("yyyy-MM-dd")
        End If
    End Sub
    Protected Sub ddlType_Changed(s As Object, e As EventArgs)
        pnlMonthly.Visible = (ddlType.SelectedValue <> "Custom")
        pnlCustom.Visible = (ddlType.SelectedValue = "Custom")
    End Sub
    Protected Sub btnGenerate_Click(s As Object, e As EventArgs)
        Dim year As Integer = Integer.Parse(ddlYear.SelectedValue)
        Dim month As Integer = Integer.Parse(ddlMonth.SelectedValue)
        Dim sd As DateTime? = Nothing, ed As DateTime? = Nothing
        If ddlType.SelectedValue = "Custom" Then
            Dim s1 As DateTime, e1 As DateTime
            If DateTime.TryParse(txtFrom.Text, s1) Then sd = s1
            If DateTime.TryParse(txtTo.Text, e1) Then ed = e1
        End If
        Dim vm = ReportService.GenerateReport(ddlType.SelectedValue, year, month, sd, ed)
        pnlResults.Visible = True
        lblReportTitle.Text = vm.ReportTitle
        lblTotal.Text = vm.TotalRequests.ToString()
        lblRevenue.Text = String.Format("{0:N2}", vm.TotalAmount)
        lblApproved.Text = vm.ApprovedCount.ToString()
        lblRejected.Text = (vm.RejectedCount + vm.PendingCount).ToString()
        rptReport.DataSource = vm.Requests : rptReport.DataBind()
    End Sub
    Protected Function GetStatusBadge(status As Integer) As String
        Select Case CType(status, RequestStatus)
            Case RequestStatus.Approved : Return "<span class=""badge bg-success"">Approved</span>"
            Case RequestStatus.Rejected : Return "<span class=""badge bg-danger"">Rejected</span>"
            Case RequestStatus.Cancelled : Return "<span class=""badge bg-secondary"">Cancelled</span>"
            Case Else : Return "<span class=""badge bg-warning text-dark"">Pending</span>"
        End Select
    End Function
End Class
