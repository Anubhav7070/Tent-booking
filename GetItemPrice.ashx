<%@ WebHandler Language="VB" Class="GetItemPrice" %>

Imports System
Imports System.Web
Imports System.Web.Script.Serialization

Public Class GetItemPrice : Implements IHttpHandler
    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        
        Dim itemIdStr As String = context.Request.QueryString("itemId")
        Dim startDateStr As String = context.Request.QueryString("startDate")
        Dim endDateStr As String = context.Request.QueryString("endDate")
        
        Dim itemId As Integer
        If Not Integer.TryParse(itemIdStr, itemId) Then
            ReturnError(context)
            Return
        End If
        
        Dim item As InventoryItem = InventoryService.GetItemById(itemId)
        If item Is Nothing Then
            ReturnError(context)
            Return
        End If
        
        Dim available As Integer = item.AvailableQuantity
        Dim startDate As DateTime, endDate As DateTime
        If DateTime.TryParse(startDateStr, startDate) AndAlso DateTime.TryParse(endDateStr, endDate) Then
            available = RentalService.GetAvailableQuantityForDates(itemId, startDate, endDate, 0)
        End If
        
        Dim js As New JavaScriptSerializer()
        Dim responseObj = New With {
            .success = True,
            .price = item.CurrentPrice,
            .available = available,
            .unit = item.UnitType
        }
        
        context.Response.Write(js.Serialize(responseObj))
    End Sub
    
    Private Sub ReturnError(ByVal context As HttpContext)
        Dim js As New JavaScriptSerializer()
        context.Response.Write(js.Serialize(New With {.success = False}))
    End Sub
    
    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return True
        End Get
    End Property
End Class
