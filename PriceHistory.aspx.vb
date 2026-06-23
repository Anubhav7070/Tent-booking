Imports System
Imports System.Collections.Generic
Imports System.Web.UI
Public Class PriceHistory
    Inherits Page
    Protected Sub Page_Load(s As Object, e As EventArgs) Handles Me.Load
        AuthHelper.RequireRole(Me,"SuperAdmin","Admin")
        If Not IsPostBack Then
            Dim items = InventoryService.GetAllItems()
            For Each i In items
                ddlItem.Items.Add(New System.Web.UI.WebControls.ListItem(i.Name, i.Id.ToString()))
            Next
            If Request.QueryString("itemId") IsNot Nothing Then
                ddlItem.SelectedValue = Request.QueryString("itemId")
            End If
            LoadHistory()
        End If
    End Sub
    Private Sub LoadHistory()
        Dim itemId As Integer? = Nothing
        If Not String.IsNullOrEmpty(ddlItem.SelectedValue) Then itemId = Integer.Parse(ddlItem.SelectedValue)
        Dim hist = InventoryService.GetPriceHistory(itemId)
        rptHistory.DataSource = hist : rptHistory.DataBind()
        lblNoData.Visible = (hist.Count = 0)
    End Sub
    Protected Sub ddlItem_Changed(s As Object, e As EventArgs)
        LoadHistory()
    End Sub
    Protected Function GetDiffBadge(diff As Decimal) As String
        If diff > 0 Then Return String.Format("<span class=""badge bg-danger"">+₹{0:N2}</span>", diff)
        If diff < 0 Then Return String.Format("<span class=""badge bg-success"">₹{0:N2}</span>", diff)
        Return "<span class=""badge bg-secondary"">No change</span>"
    End Function
End Class
