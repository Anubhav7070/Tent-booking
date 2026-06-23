Imports System
Imports System.Linq
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.Script.Serialization

Public Class CreateRequest
    Inherits Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        AuthHelper.RequireLogin(Me)
        
        ' Enable file upload functionality on the form dynamically
        If Page.Form IsNot Nothing Then
            Page.Form.Enctype = "multipart/form-data"
        End If

        If Not IsPostBack Then
            EventDate.Text = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd")
            StartDate.Text = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd")
            EndDate.Text = DateTime.Today.AddDays(8).ToString("yyyy-MM-dd")
        End If
    End Sub

    Protected Sub btnSubmit_Click(ByVal sender As Object, ByVal e As EventArgs)
        Dim itemsStr As String = hfSelectedItems.Value
        Dim qtysStr As String = hfSelectedQtys.Value

        If String.IsNullOrWhiteSpace(itemsStr) Then
            lblError.Text = "Please select at least one item."
            lblError.Visible = True
            Return
        End If

        Dim evDate As DateTime, stDate As DateTime, enDate As DateTime
        If Not DateTime.TryParse(EventDate.Text, evDate) OrElse
           Not DateTime.TryParse(StartDate.Text, stDate) OrElse
           Not DateTime.TryParse(EndDate.Text, enDate) Then
            lblError.Text = "Please select valid dates."
            lblError.Visible = True
            Return
        End If

        If evDate < DateTime.Today Then
            lblError.Text = "Event date cannot be in the past."
            lblError.Visible = True
            Return
        End If

        If stDate < DateTime.Today Then
            lblError.Text = "Start date cannot be in the past."
            lblError.Visible = True
            Return
        End If

        If enDate < stDate Then
            lblError.Text = "Item Required Until date cannot be earlier than Item Required From date."
            lblError.Visible = True
            Return
        End If

        ' Handle In-Principal Approval Document Upload
        If Not fileDocument.HasFile Then
            lblError.Text = "Please upload an In-Principal Approval Document."
            lblError.Visible = True
            Return
        End If

        Dim ext As String = System.IO.Path.GetExtension(fileDocument.FileName).ToLower()
        Dim allowedExtensions() As String = {".pdf", ".png", ".jpg", ".jpeg"}
        If Not allowedExtensions.Contains(ext) Then
            lblError.Text = "Only PDF, PNG, JPG, and JPEG files are allowed for the In-Principal Approval Document."
            lblError.Visible = True
            Return
        End If

        If fileDocument.PostedFile.ContentLength > 5 * 1024 * 1024 Then
            lblError.Text = "In-Principal Approval Document size must not exceed 5MB."
            lblError.Visible = True
            Return
        End If

        Dim docPath As String = ""
        Try
            Dim uploadsFolder As String = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uploads", "documents")
            If Not System.IO.Directory.Exists(uploadsFolder) Then
                System.IO.Directory.CreateDirectory(uploadsFolder)
            End If
            
            Dim fileName As String = Guid.NewGuid().ToString() & ext
            Dim filePath As String = System.IO.Path.Combine(uploadsFolder, fileName)
            fileDocument.SaveAs(filePath)
            docPath = "/uploads/documents/" & fileName
        Catch ex As Exception
            lblError.Text = "Error saving uploaded file: " & ex.Message
            lblError.Visible = True
            Return
        End Try

        Try
            Dim itemIds() As Integer = itemsStr.Split(",").Select(Function(x) Integer.Parse(x.Trim())).ToArray()
            Dim quantities() As Integer = qtysStr.Split(",").Select(Function(x) If(Integer.TryParse(x.Trim(), Nothing), Integer.Parse(x.Trim()), 1)).ToArray()
            Dim userId As String = AuthHelper.GetCurrentUserId(Context)
            Dim role As String = AuthHelper.GetCurrentRole(Context)
            
            Dim req = RentalService.CreateRequest(userId, role, evDate, stDate, enDate, itemIds, quantities, docPath)
            
            AuditService.Log(userId, "CreateRequest", "RentalRequest", req.Id.ToString(), "New request " & req.RequestNumber & " submitted", "", "", Request.UserHostAddress)
            NotificationService.CreateNotification(userId, "Request Submitted", "Your request " & req.RequestNumber & " has been submitted for approval.")
            
            Session("Success") = "Request " & req.RequestNumber & " submitted successfully! Grand Total: ₹" & String.Format("{0:N2}", req.GrandTotal)
            Response.Redirect("MyRequests.aspx")
        Catch ex As Exception
            lblError.Text = "Error submitting request: " & ex.Message
            lblError.Visible = True
        End Try
    End Sub

    Public Function GetAvailableItemsJson() As String
        Dim items = InventoryService.GetInventoryForRequest()
        Dim js As New JavaScriptSerializer()
        Dim mapped = items.Select(Function(i) New With {
            .Id = i.Id,
            .Name = i.Name,
            .AvailableQuantity = i.AvailableQuantity,
            .CurrentPrice = i.CurrentPrice,
            .UnitType = i.UnitType
        }).ToList()
        Return js.Serialize(mapped)
    End Function
End Class
