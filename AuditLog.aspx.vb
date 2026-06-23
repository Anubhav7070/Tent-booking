Imports System
Imports System.Web.UI
Public Class AuditLog
    Inherits Page
    Private _page As Integer = 1
    Private Const PAGE_SIZE As Integer = 20
    Protected Sub Page_Load(s As Object, e As EventArgs) Handles Me.Load
        AuthHelper.RequireRole(Me,"SuperAdmin")
        If Not IsPostBack Then LoadLogs()
    End Sub
    Private Sub LoadLogs()
        If Not Integer.TryParse(txtPage.Text, _page) OrElse _page < 1 Then _page = 1
        Dim result = AuditService.GetLogs(_page, PAGE_SIZE)
        rptLogs.DataSource = result.Logs : rptLogs.DataBind()
        lblNoData.Visible = (result.Logs.Count = 0)
        Dim totalPages As Integer = CInt(Math.Ceiling(CDbl(result.Total) / PAGE_SIZE))
        lblCurrentPage.Text = _page.ToString()
        lblTotalPages.Text = totalPages.ToString()
        lblTotal.Text = result.Total.ToString()
        txtPage.Text = _page.ToString()
    End Sub
    Protected Sub btnGo_Click(s As Object, e As EventArgs)
        LoadLogs()
    End Sub
    Protected Sub btnPrev_Click(s As Object, e As EventArgs)
        If Not Integer.TryParse(txtPage.Text, _page) Then _page = 1
        txtPage.Text = Math.Max(1, _page - 1).ToString() : LoadLogs()
    End Sub
    Protected Sub btnNext_Click(s As Object, e As EventArgs)
        If Not Integer.TryParse(txtPage.Text, _page) Then _page = 1
        txtPage.Text = (_page + 1).ToString() : LoadLogs()
    End Sub
End Class
