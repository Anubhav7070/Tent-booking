Imports System
Imports System.Collections.Generic
Imports System.Web.UI
Imports System.Web.UI.WebControls

Public Class Inventory
    Inherits Page

    Protected Sub Page_Load(s As Object, e As EventArgs) Handles Me.Load
        AuthHelper.RequireRole(Me, "SuperAdmin","Admin")
        If Not IsPostBack Then
            LoadCategories()
            LoadItems()
        End If
    End Sub

    Private Sub LoadCategories()
        Dim cats As List(Of InventoryCategory) = InventoryService.GetAllCategories()
        ddlCategory.Items.Clear()
        ddlCategory.Items.Add(New ListItem("All Categories",""))
        For Each c In cats
            ddlCategory.Items.Add(New ListItem(c.Name, c.Id.ToString()))
        Next
    End Sub

    Public TodayReserved As New Dictionary(Of Integer, Integer)()

    Private Sub LoadItems()
        Dim catId As Integer? = Nothing
        If Not String.IsNullOrEmpty(ddlCategory.SelectedValue) Then catId = Integer.Parse(ddlCategory.SelectedValue)
        Dim items = InventoryService.GetAllItems(txtSearch.Text.Trim(), catId)

        ' Fetch today's reserved quantities
        TodayReserved = InventoryService.GetTodayReserved()

        rptItems.DataSource = items
        rptItems.DataBind()
        lblNoData.Visible = (items.Count = 0)
    End Sub

    Protected Function GetReservedQty(dataItem As Object) As Integer
        Dim item As InventoryItem = TryCast(dataItem, InventoryItem)
        If item Is Nothing Then Return 0
        If TodayReserved.ContainsKey(item.Id) Then
            Return TodayReserved(item.Id)
        End If
        Return 0
    End Function

    Protected Function GetAvailableQty(dataItem As Object) As Integer
        Dim item As InventoryItem = TryCast(dataItem, InventoryItem)
        If item Is Nothing Then Return 0
        Dim reserved = GetReservedQty(item)
        Return Math.Max(0, item.TotalQuantity - reserved)
    End Function

    Protected Sub btnFilter_Click(s As Object, e As EventArgs)
        LoadItems()
    End Sub
    Protected Sub btnClear_Click(s As Object, e As EventArgs)
        txtSearch.Text=""
        ddlCategory.SelectedIndex=0
        LoadItems()
    End Sub

    Protected Sub lnkDelete_Command(s As Object, e As CommandEventArgs)
        Dim id As Integer = Integer.Parse(e.CommandArgument.ToString())
        InventoryService.DeleteItem(id)
        Dim userId As String = AuthHelper.GetCurrentUserId(Context)
        AuditService.Log(userId,"Delete","InventoryItem",id.ToString(),"Item deactivated","","",Request.UserHostAddress)
        Session("Success") = "Item removed from inventory."
        Response.Redirect("Inventory.aspx")
    End Sub
End Class
