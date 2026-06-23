Imports System
Imports System.Collections.Generic
Imports System.Web.UI
Public Class PriceManagement
    Inherits Page
    Protected Sub Page_Load(s As Object, e As EventArgs) Handles Me.Load
        AuthHelper.RequireRole(Me,"SuperAdmin","Admin")
        If Not IsPostBack Then
            LoadItems() : LoadHistory()
            txtEffDate.Text = DateTime.Today.ToString("yyyy-MM-dd")
        End If
    End Sub
    Private Sub LoadItems()
        Dim items = InventoryService.GetAllItems()
        ddlItem.DataSource = items : ddlItem.DataTextField = "Name" : ddlItem.DataValueField = "Id" : ddlItem.DataBind()
        If items.Count > 0 Then txtCurrentPrice.Text = items(0).CurrentPrice.ToString("F2")
    End Sub
    Private Sub LoadHistory()
        Dim hist = InventoryService.GetPriceHistory()
        rptHistory.DataSource = hist.Take(20).ToList() : rptHistory.DataBind()
    End Sub
    Protected Sub ddlItem_Changed(s As Object, e As EventArgs)
        Dim id As Integer = Integer.Parse(ddlItem.SelectedValue)
        Dim item = InventoryService.GetItemById(id)
        If item IsNot Nothing Then txtCurrentPrice.Text = item.CurrentPrice.ToString("F2")
    End Sub
    Protected Sub btnUpdate_Click(s As Object, e As EventArgs)
        Dim id As Integer = Integer.Parse(ddlItem.SelectedValue)
        Dim newPrice As Decimal = 0
        If Not Decimal.TryParse(txtNewPrice.Text, newPrice) OrElse newPrice <= 0 Then
            lblError.Text = "Please enter a valid new price." : lblError.Visible=True : Return
        End If
        Dim effDate As DateTime = DateTime.Today
        DateTime.TryParse(txtEffDate.Text, effDate)
        Dim userId As String = AuthHelper.GetCurrentUserId(Context)
        Dim userName As String = AuthHelper.GetCurrentFullName(Context)
        Dim item = InventoryService.GetItemById(id)
        InventoryService.UpdatePrice(id, newPrice, effDate, txtReason.Text.Trim(), userName)
        Dim itemName As String = ""
        If item IsNot Nothing Then
            itemName = item.Name
        End If
        AuditService.Log(userId,"PriceUpdate","InventoryItem",id.ToString(),String.Format("Price updated for {0}: Rs.{1} -> Rs.{2}", itemName, txtCurrentPrice.Text, newPrice),"Rs." & txtCurrentPrice.Text,"Rs." & newPrice,Request.UserHostAddress)
        Session("Success") = String.Format("Price updated for '{0}' to Rs.{1:N2}.", itemName, newPrice)
        Response.Redirect("PriceManagement.aspx")
    End Sub
    Protected Function GetDiffBadge(diff As Decimal) As String
        If diff > 0 Then Return String.Format("<span class=""badge bg-danger"">+₹{0:N2}</span>", diff)
        If diff < 0 Then Return String.Format("<span class=""badge bg-success"">₹{0:N2}</span>", diff)
        Return "<span class=""badge bg-secondary"">No change</span>"
    End Function
End Class
