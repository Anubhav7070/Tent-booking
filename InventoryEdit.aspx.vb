Imports System
Imports System.Collections.Generic
Imports System.Web.UI
Public Class InventoryEdit
    Inherits Page
    Protected Sub Page_Load(s As Object, e As EventArgs) Handles Me.Load
        AuthHelper.RequireRole(Me,"SuperAdmin","Admin")
        If Not IsPostBack Then
            Dim id As Integer = 0
            If Not Integer.TryParse(Request.QueryString("id"), id) Then Response.Redirect("Inventory.aspx") : Return
            hfId.Value = id.ToString()
            Dim cats As List(Of InventoryCategory) = InventoryService.GetAllCategories()
            ddlCategory.DataSource = cats : ddlCategory.DataTextField = "Name" : ddlCategory.DataValueField = "Id" : ddlCategory.DataBind()
            Dim item As InventoryItem = InventoryService.GetItemById(id)
            If item Is Nothing Then Response.Redirect("Inventory.aspx") : Return
            txtName.Text = item.Name : txtDesc.Text = item.Description : txtQty.Text = item.TotalQuantity.ToString()
            txtPrice.Text = item.CurrentPrice.ToString("F2") : chkActive.Checked = item.IsActive
            ddlCategory.SelectedValue = item.CategoryId.ToString()
            ddlUnit.SelectedValue = item.UnitType
        End If
    End Sub
    Protected Sub btnSave_Click(s As Object, e As EventArgs)
        Dim id As Integer = Integer.Parse(hfId.Value)
        If String.IsNullOrWhiteSpace(txtName.Text) Then lblError.Text="Item name is required." : lblError.Visible=True : Return
        Dim qty As Integer = 0 : Integer.TryParse(txtQty.Text, qty)
        Dim price As Decimal = 0 : Decimal.TryParse(txtPrice.Text, price)
        Dim userId As String = AuthHelper.GetCurrentUserId(Context)
        Dim userName As String = AuthHelper.GetCurrentFullName(Context)
        InventoryService.UpdateItem(id, txtName.Text.Trim(), txtDesc.Text.Trim(), Integer.Parse(ddlCategory.SelectedValue), ddlUnit.SelectedValue, qty, price, chkActive.Checked, userName)
        AuditService.Log(userId,"Update","InventoryItem",id.ToString(),String.Format("Item '{0}' updated", txtName.Text),"","",String.Format("Price: Rs.{0}", price),Request.UserHostAddress)
        Session("Success") = String.Format("Item '{0}' updated successfully.", txtName.Text)
        Response.Redirect("Inventory.aspx")
    End Sub
End Class
