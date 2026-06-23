Imports System
Imports System.Collections.Generic
Imports System.Web.UI
Public Class InventoryCreate
    Inherits Page
    Protected Sub Page_Load(s As Object, e As EventArgs) Handles Me.Load
        AuthHelper.RequireRole(Me,"SuperAdmin","Admin")
        If Not IsPostBack Then
            Dim cats As List(Of InventoryCategory) = InventoryService.GetAllCategories()
            ddlCategory.DataSource = cats : ddlCategory.DataTextField = "Name" : ddlCategory.DataValueField = "Id" : ddlCategory.DataBind()
        End If
    End Sub
    Protected Sub btnSave_Click(s As Object, e As EventArgs)
        If String.IsNullOrWhiteSpace(txtName.Text) Then lblError.Text="Item name is required." : lblError.Visible=True : Return
        Dim qty As Integer = 0 : Integer.TryParse(txtQty.Text, qty)
        Dim price As Decimal = 0 : Decimal.TryParse(txtPrice.Text, price)
        InventoryService.CreateItem(txtName.Text.Trim(), txtDesc.Text.Trim(), Integer.Parse(ddlCategory.SelectedValue), ddlUnit.SelectedValue, qty, price, chkActive.Checked)
        AuditService.Log(AuthHelper.GetCurrentUserId(Context),"Create","InventoryItem","0",String.Format("New item '{0}' created", txtName.Text),"","",String.Format("Price: Rs.{0}", price),Request.UserHostAddress)
        Session("Success") = String.Format("Item '{0}' added successfully.", txtName.Text)
        Response.Redirect("Inventory.aspx")
    End Sub
End Class
