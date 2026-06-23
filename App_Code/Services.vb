Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SQLite
Imports System.Linq
Imports System.Text
Imports System.Web

' ============================================================
'  Services.vb — All business logic services
'  Replaces MVC Services layer (RentalService, InventoryService,
'  AuditService, NotificationService, ReportService, UserService)
' ============================================================

' ── UserService ───────────────────────────────────────────────────────────────
Public Class UserService
    Public Shared Function GetUserById(userId As String) As ApplicationUser
        Dim dt As DataTable = Database.ExecuteDataTable(
            "SELECT u.*, r.Name AS Role FROM AspNetUsers u LEFT JOIN AspNetUserRoles ur ON u.Id=ur.UserId LEFT JOIN AspNetRoles r ON ur.RoleId=r.Id WHERE u.Id=@id",
            New SQLiteParameter("@id", userId))
        If dt.Rows.Count = 0 Then Return Nothing
        Return MapUser(dt.Rows(0))
    End Function

    Public Shared Function GetUserByEmployeeId(empId As String) As ApplicationUser
        Dim dt As DataTable = Database.ExecuteDataTable(
            "SELECT u.*, r.Name AS Role FROM AspNetUsers u LEFT JOIN AspNetUserRoles ur ON u.Id=ur.UserId LEFT JOIN AspNetRoles r ON ur.RoleId=r.Id WHERE u.EmployeeId=@eid",
            New SQLiteParameter("@eid", empId))
        If dt.Rows.Count = 0 Then Return Nothing
        Return MapUser(dt.Rows(0))
    End Function

    Public Shared Function GetAllUsers() As List(Of ApplicationUser)
        Dim dt As DataTable = Database.ExecuteDataTable(
            "SELECT u.*, r.Name AS Role FROM AspNetUsers u LEFT JOIN AspNetUserRoles ur ON u.Id=ur.UserId LEFT JOIN AspNetRoles r ON ur.RoleId=r.Id ORDER BY u.EmployeeId")
        Dim list As New List(Of ApplicationUser)()
        For Each row As DataRow In dt.Rows
            list.Add(MapUser(row))
        Next
        Return list
    End Function

    Public Shared Function CreateUser(employeeId As String, fullName As String, department As String, designation As String,
                                       email As String, phoneNumber As String, quarterAddress As String,
                                       password As String, role As String) As ServiceResult
        ' Validate employee ID
        If String.IsNullOrWhiteSpace(employeeId) OrElse employeeId.Length <> 8 OrElse Not employeeId.All(Function(c) Char.IsDigit(c)) Then
            Return New ServiceResult(False, "Employee ID must be exactly 8 numeric digits.")
        End If

        ' Check uniqueness
        Dim existingUser As Object = Database.ExecuteScalar("SELECT COUNT(*) FROM AspNetUsers WHERE UserName=@eid", New SQLiteParameter("@eid", employeeId))
        If Convert.ToInt32(existingUser) > 0 Then
            Return New ServiceResult(False, "Employee ID '" & employeeId & "' is already registered.")
        End If

        Dim existingEmp As Object = Database.ExecuteScalar("SELECT COUNT(*) FROM Employees WHERE EmployeeId=@eid", New SQLiteParameter("@eid", employeeId))
        If Convert.ToInt32(existingEmp) > 0 Then
            Return New ServiceResult(False, "Employee record for ID '" & employeeId & "' already exists.")
        End If

        Dim userId As String = Guid.NewGuid().ToString()
        Dim hash As String = PasswordHelper.HashPassword(password)
        Dim assignRole As String = If(String.IsNullOrEmpty(role), "User", role)

        ' Insert employee
        Database.ExecuteNonQuery("INSERT INTO Employees(EmployeeId,EmployeeName,Department,Designation,Email,PhoneNumber,QuarterAddress,Status) VALUES(@eid,@en,@dep,@des,@em,@ph,@qa,'Active')",
            New SQLiteParameter("@eid", employeeId),
            New SQLiteParameter("@en", If(fullName, "")),
            New SQLiteParameter("@dep", If(department, "")),
            New SQLiteParameter("@des", If(designation, "")),
            New SQLiteParameter("@em", If(email, "")),
            New SQLiteParameter("@ph", If(phoneNumber, "")),
            New SQLiteParameter("@qa", If(quarterAddress, "")))

        ' Insert auth user
        Database.ExecuteNonQuery("INSERT INTO AspNetUsers(Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,FullName,EmployeeId,Department,IsActive,MustChangePassword,CreatedAt) VALUES(@id,@un,@nun,@em,@nem,1,@ph,@fn,@eid,@dep,1,1,@ca)",
            New SQLiteParameter("@id", userId),
            New SQLiteParameter("@un", employeeId),
            New SQLiteParameter("@nun", employeeId.ToUpper()),
            New SQLiteParameter("@em", If(email, "")),
            New SQLiteParameter("@nem", If(email, "").ToUpper()),
            New SQLiteParameter("@ph", hash),
            New SQLiteParameter("@fn", If(fullName, "")),
            New SQLiteParameter("@eid", employeeId),
            New SQLiteParameter("@dep", If(department, "")),
            New SQLiteParameter("@ca", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")))

        ' Assign role
        Dim roleId As Object = Database.ExecuteScalar("SELECT Id FROM AspNetRoles WHERE NormalizedName=@rn", New SQLiteParameter("@rn", assignRole.ToUpper()))
        If roleId IsNot Nothing Then
            Database.ExecuteNonQuery("INSERT INTO AspNetUserRoles(UserId,RoleId) VALUES(@uid,@rid)",
                New SQLiteParameter("@uid", userId),
                New SQLiteParameter("@rid", roleId.ToString()))
        End If

        Return New ServiceResult(True, "Account for '" & fullName & "' (ID: " & employeeId & ") created successfully.")
    End Function

    Public Shared Sub UpdateUser(userId As String, fullName As String, department As String, designation As String,
                                  email As String, phoneNumber As String, quarterAddress As String, newRole As String)
        ' Update auth user
        Database.ExecuteNonQuery("UPDATE AspNetUsers SET FullName=@fn, Department=@dep, Email=@em WHERE Id=@id",
            New SQLiteParameter("@fn", If(fullName, "")),
            New SQLiteParameter("@dep", If(department, "")),
            New SQLiteParameter("@em", If(email, "")),
            New SQLiteParameter("@id", userId))

        ' Get employee ID
        Dim empId As Object = Database.ExecuteScalar("SELECT EmployeeId FROM AspNetUsers WHERE Id=@id", New SQLiteParameter("@id", userId))
        If empId IsNot Nothing Then
            Database.ExecuteNonQuery("UPDATE Employees SET EmployeeName=@fn, Department=@dep, Designation=@des, Email=@em, PhoneNumber=@ph, QuarterAddress=@qa, UpdatedDate=@ud WHERE EmployeeId=@eid",
                New SQLiteParameter("@fn", If(fullName, "")),
                New SQLiteParameter("@dep", If(department, "")),
                New SQLiteParameter("@des", If(designation, "")),
                New SQLiteParameter("@em", If(email, "")),
                New SQLiteParameter("@ph", If(phoneNumber, "")),
                New SQLiteParameter("@qa", If(quarterAddress, "")),
                New SQLiteParameter("@ud", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")),
                New SQLiteParameter("@eid", empId.ToString()))
        End If

        ' Update role if provided
        If Not String.IsNullOrEmpty(newRole) Then
            Dim roleId As Object = Database.ExecuteScalar("SELECT Id FROM AspNetRoles WHERE NormalizedName=@rn", New SQLiteParameter("@rn", newRole.ToUpper()))
            If roleId IsNot Nothing Then
                Database.ExecuteNonQuery("DELETE FROM AspNetUserRoles WHERE UserId=@uid", New SQLiteParameter("@uid", userId))
                Database.ExecuteNonQuery("INSERT INTO AspNetUserRoles(UserId,RoleId) VALUES(@uid,@rid)",
                    New SQLiteParameter("@uid", userId),
                    New SQLiteParameter("@rid", roleId.ToString()))
            End If
        End If
    End Sub

    Public Shared Sub ToggleActive(userId As String)
        Database.ExecuteNonQuery("UPDATE AspNetUsers SET IsActive = CASE WHEN IsActive=1 THEN 0 ELSE 1 END WHERE Id=@id",
            New SQLiteParameter("@id", userId))
    End Sub

    Public Shared Function ResetPassword(userId As String, newPassword As String) As ServiceResult
        If String.IsNullOrWhiteSpace(newPassword) OrElse newPassword.Length < 6 Then
            Return New ServiceResult(False, "Password must be at least 6 characters.")
        End If
        Dim hash As String = PasswordHelper.HashPassword(newPassword)
        Database.ExecuteNonQuery("UPDATE AspNetUsers SET PasswordHash=@h, MustChangePassword=1 WHERE Id=@id",
            New SQLiteParameter("@h", hash),
            New SQLiteParameter("@id", userId))
        Return New ServiceResult(True, "Password reset successfully.")
    End Function

    Public Shared Function ChangePassword(userId As String, currentPassword As String, newPassword As String) As ServiceResult
        If String.IsNullOrWhiteSpace(currentPassword) OrElse String.IsNullOrWhiteSpace(newPassword) Then
            Return New ServiceResult(False, "All fields are required.")
        End If
        Dim hash As Object = Database.ExecuteScalar("SELECT PasswordHash FROM AspNetUsers WHERE Id=@id", New SQLiteParameter("@id", userId))
        If hash Is Nothing OrElse Not PasswordHelper.VerifyPassword(currentPassword, hash.ToString()) Then
            Return New ServiceResult(False, "Current password is incorrect.")
        End If
        If newPassword.Length < 6 Then Return New ServiceResult(False, "New password must be at least 6 characters.")
        Dim newHash As String = PasswordHelper.HashPassword(newPassword)
        Database.ExecuteNonQuery("UPDATE AspNetUsers SET PasswordHash=@h, MustChangePassword=0 WHERE Id=@id",
            New SQLiteParameter("@h", newHash),
            New SQLiteParameter("@id", userId))
        Return New ServiceResult(True, "Password changed successfully.")
    End Function

    Private Shared Function MapUser(row As DataRow) As ApplicationUser
        Return New ApplicationUser With {
            .Id = row("Id").ToString(),
            .UserName = row("UserName").ToString(),
            .Email = If(row("Email") Is DBNull.Value, "", row("Email").ToString()),
            .FullName = If(row("FullName") Is DBNull.Value, "", row("FullName").ToString()),
            .EmployeeId = If(row("EmployeeId") Is DBNull.Value, "", row("EmployeeId").ToString()),
            .Department = If(row("Department") Is DBNull.Value, "", row("Department").ToString()),
            .IsActive = Convert.ToBoolean(row("IsActive")),
            .MustChangePassword = Convert.ToBoolean(row("MustChangePassword")),
            .PasswordHash = If(row("PasswordHash") Is DBNull.Value, "", row("PasswordHash").ToString()),
            .Role = If(row("Role") Is DBNull.Value, "User", row("Role").ToString())
        }
    End Function
End Class

' ── InventoryService ──────────────────────────────────────────────────────────
Public Class InventoryService
    Public Shared Function GetAllCategories() As List(Of InventoryCategory)
        Dim dt As DataTable = Database.ExecuteDataTable("SELECT * FROM InventoryCategories ORDER BY Name")
        Dim list As New List(Of InventoryCategory)()
        For Each row As DataRow In dt.Rows
            list.Add(New InventoryCategory With {
                .Id = Convert.ToInt32(row("Id")),
                .Name = row("Name").ToString(),
                .Description = If(row("Description") Is DBNull.Value, "", row("Description").ToString())
            })
        Next
        Return list
    End Function

    Public Shared Function GetAllItems(Optional search As String = "", Optional categoryId As Integer? = Nothing) As List(Of InventoryItem)
        Dim sql As String = "SELECT i.*, c.Name AS CategoryName FROM InventoryItems i LEFT JOIN InventoryCategories c ON i.CategoryId=c.Id WHERE i.IsActive=1"
        Dim params As New List(Of SQLiteParameter)()
        If Not String.IsNullOrEmpty(search) Then
            sql &= " AND i.Name LIKE @search"
            params.Add(New SQLiteParameter("@search", "%" & search & "%"))
        End If
        If categoryId.HasValue Then
            sql &= " AND i.CategoryId=@cat"
            params.Add(New SQLiteParameter("@cat", categoryId.Value))
        End If
        sql &= " ORDER BY i.Name"
        Dim dt As DataTable = Database.ExecuteDataTable(sql, params.ToArray())
        Return MapItems(dt)
    End Function

    Public Shared Function GetItemById(id As Integer) As InventoryItem
        Dim dt As DataTable = Database.ExecuteDataTable(
            "SELECT i.*, c.Name AS CategoryName FROM InventoryItems i LEFT JOIN InventoryCategories c ON i.CategoryId=c.Id WHERE i.Id=@id",
            New SQLiteParameter("@id", id))
        If dt.Rows.Count = 0 Then Return Nothing
        Return MapItem(dt.Rows(0))
    End Function

    Public Shared Function GetInventoryForRequest() As List(Of InventoryItem)
        Dim dt As DataTable = Database.ExecuteDataTable(
            "SELECT i.*, c.Name AS CategoryName FROM InventoryItems i LEFT JOIN InventoryCategories c ON i.CategoryId=c.Id WHERE i.IsActive=1 AND (i.TotalQuantity - i.ReservedQuantity) > 0 ORDER BY i.Name")
        Return MapItems(dt)
    End Function

    Public Shared Function GetLowStockItems() As List(Of InventoryItem)
        Dim threshold As Integer = 10
        Dim dt As DataTable = Database.ExecuteDataTable(
            "SELECT i.*, c.Name AS CategoryName FROM InventoryItems i LEFT JOIN InventoryCategories c ON i.CategoryId=c.Id WHERE i.IsActive=1 AND (i.TotalQuantity - i.ReservedQuantity) < @t ORDER BY i.Name",
            New SQLiteParameter("@t", threshold))
        Return MapItems(dt)
    End Function

    Public Shared Sub CreateItem(name As String, description As String, categoryId As Integer, unitType As String,
                                  totalQuantity As Integer, currentPrice As Decimal, isActive As Boolean)
        Database.ExecuteNonQuery("INSERT INTO InventoryItems(Name,Description,CategoryId,UnitType,TotalQuantity,CurrentPrice,IsActive,CreatedAt,UpdatedAt) VALUES(@n,@d,@c,@u,@tq,@cp,@ia,@now,@now)",
            New SQLiteParameter("@n", name),
            New SQLiteParameter("@d", If(description, "")),
            New SQLiteParameter("@c", categoryId),
            New SQLiteParameter("@u", If(unitType, "Nos")),
            New SQLiteParameter("@tq", totalQuantity),
            New SQLiteParameter("@cp", currentPrice),
            New SQLiteParameter("@ia", If(isActive, 1, 0)),
            New SQLiteParameter("@now", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")))
    End Sub

    Public Shared Sub UpdateItem(id As Integer, name As String, description As String, categoryId As Integer,
                                  unitType As String, totalQuantity As Integer, currentPrice As Decimal, isActive As Boolean, updatedBy As String)
        Dim old As InventoryItem = GetItemById(id)
        Database.ExecuteNonQuery("UPDATE InventoryItems SET Name=@n, Description=@d, CategoryId=@c, UnitType=@u, TotalQuantity=@tq, CurrentPrice=@cp, IsActive=@ia, UpdatedAt=@now WHERE Id=@id",
            New SQLiteParameter("@n", name),
            New SQLiteParameter("@d", If(description, "")),
            New SQLiteParameter("@c", categoryId),
            New SQLiteParameter("@u", If(unitType, "Nos")),
            New SQLiteParameter("@tq", totalQuantity),
            New SQLiteParameter("@cp", currentPrice),
            New SQLiteParameter("@ia", If(isActive, 1, 0)),
            New SQLiteParameter("@now", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")),
            New SQLiteParameter("@id", id))
        ' Log price change if price changed
        If old IsNot Nothing AndAlso old.CurrentPrice <> currentPrice Then
            UpdatePrice(id, currentPrice, DateTime.Today, "Direct item update", updatedBy)
        End If
    End Sub

    Public Shared Sub DeleteItem(id As Integer)
        Database.ExecuteNonQuery("UPDATE InventoryItems SET IsActive=0, UpdatedAt=@now WHERE Id=@id",
            New SQLiteParameter("@now", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")),
            New SQLiteParameter("@id", id))
    End Sub

    Public Shared Sub UpdatePrice(itemId As Integer, newPrice As Decimal, effectiveDate As DateTime, reason As String, updatedBy As String)
        Dim old As InventoryItem = GetItemById(itemId)
        Dim oldPrice As Decimal = If(old IsNot Nothing, old.CurrentPrice, 0)
        Database.ExecuteNonQuery("UPDATE InventoryItems SET CurrentPrice=@p, UpdatedAt=@now WHERE Id=@id",
            New SQLiteParameter("@p", newPrice),
            New SQLiteParameter("@now", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")),
            New SQLiteParameter("@id", itemId))
        Database.ExecuteNonQuery("INSERT INTO PriceHistories(InventoryItemId,PreviousPrice,UpdatedPrice,EffectiveDate,Reason,UpdatedBy,UpdatedAt) VALUES(@iid,@pp,@np,@ed,@r,@ub,@now)",
            New SQLiteParameter("@iid", itemId),
            New SQLiteParameter("@pp", oldPrice),
            New SQLiteParameter("@np", newPrice),
            New SQLiteParameter("@ed", effectiveDate.ToString("yyyy-MM-dd")),
            New SQLiteParameter("@r", If(reason, "")),
            New SQLiteParameter("@ub", If(updatedBy, "")),
            New SQLiteParameter("@now", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")))
    End Sub

    Public Shared Function GetPriceHistory(Optional itemId As Integer? = Nothing) As List(Of PriceHistory)
        Dim sql As String = "SELECT ph.*, i.Name AS ItemName FROM PriceHistories ph JOIN InventoryItems i ON ph.InventoryItemId=i.Id"
        Dim params As New List(Of SQLiteParameter)()
        If itemId.HasValue Then
            sql &= " WHERE ph.InventoryItemId=@iid"
            params.Add(New SQLiteParameter("@iid", itemId.Value))
        End If
        sql &= " ORDER BY ph.UpdatedAt DESC"
        Dim dt As DataTable = Database.ExecuteDataTable(sql, params.ToArray())
        Dim list As New List(Of PriceHistory)()
        For Each row As DataRow In dt.Rows
            list.Add(New PriceHistory With {
                .Id = Convert.ToInt32(row("Id")),
                .InventoryItemId = Convert.ToInt32(row("InventoryItemId")),
                .ItemName = row("ItemName").ToString(),
                .PreviousPrice = Convert.ToDecimal(row("PreviousPrice")),
                .UpdatedPrice = Convert.ToDecimal(row("UpdatedPrice")),
                .EffectiveDate = If(row("EffectiveDate") Is DBNull.Value, DateTime.MinValue, DateTime.Parse(row("EffectiveDate").ToString())),
                .Reason = If(row("Reason") Is DBNull.Value, "", row("Reason").ToString()),
                .UpdatedBy = If(row("UpdatedBy") Is DBNull.Value, "", row("UpdatedBy").ToString()),
                .UpdatedAt = If(row("UpdatedAt") Is DBNull.Value, DateTime.MinValue, DateTime.Parse(row("UpdatedAt").ToString()))
            })
        Next
        Return list
    End Function

    Private Shared Function MapItems(dt As DataTable) As List(Of InventoryItem)
        Dim list As New List(Of InventoryItem)()
        For Each row As DataRow In dt.Rows
            list.Add(MapItem(row))
        Next
        Return list
    End Function

    Private Shared Function MapItem(row As DataRow) As InventoryItem
        Return New InventoryItem With {
            .Id = Convert.ToInt32(row("Id")),
            .Name = row("Name").ToString(),
            .Description = If(row("Description") Is DBNull.Value, "", row("Description").ToString()),
            .CategoryId = If(row("CategoryId") Is DBNull.Value, 0, Convert.ToInt32(row("CategoryId"))),
            .CategoryName = If(row("CategoryName") Is DBNull.Value, "", row("CategoryName").ToString()),
            .UnitType = If(row("UnitType") Is DBNull.Value, "Nos", row("UnitType").ToString()),
            .TotalQuantity = Convert.ToInt32(row("TotalQuantity")),
            .ReservedQuantity = Convert.ToInt32(row("ReservedQuantity")),
            .CurrentPrice = Convert.ToDecimal(row("CurrentPrice")),
            .IsActive = Convert.ToBoolean(row("IsActive"))
        }
    End Function
End Class

' ── RentalService ─────────────────────────────────────────────────────────────
Public Class RentalService
    Public Shared Function GetAllRequests() As List(Of RentalRequest)
        Dim sql As String = "SELECT r.*, u.FullName AS UserFullName, u.EmployeeId AS UserEmployeeId, u.Department AS UserDepartment FROM RentalRequests r LEFT JOIN AspNetUsers u ON r.UserId=u.Id ORDER BY r.CreatedAt DESC"
        Return MapRequests(Database.ExecuteDataTable(sql))
    End Function

    Public Shared Function GetUserRequests(userId As String) As List(Of RentalRequest)
        Dim sql As String = "SELECT r.*, u.FullName AS UserFullName, u.EmployeeId AS UserEmployeeId, u.Department AS UserDepartment FROM RentalRequests r LEFT JOIN AspNetUsers u ON r.UserId=u.Id WHERE r.UserId=@uid ORDER BY r.CreatedAt DESC"
        Return MapRequests(Database.ExecuteDataTable(sql, New SQLiteParameter("@uid", userId)))
    End Function

    Public Shared Function GetRequestById(id As Integer) As RentalRequest
        Dim sql As String = "SELECT r.*, u.FullName AS UserFullName, u.EmployeeId AS UserEmployeeId, u.Department AS UserDepartment FROM RentalRequests r LEFT JOIN AspNetUsers u ON r.UserId=u.Id WHERE r.Id=@id"
        Dim dt As DataTable = Database.ExecuteDataTable(sql, New SQLiteParameter("@id", id))
        If dt.Rows.Count = 0 Then Return Nothing
        Dim req As RentalRequest = MapRequest(dt.Rows(0))
        req.Items = GetRequestItems(id)
        Return req
    End Function

    Public Shared Function GetRequestItems(requestId As Integer) As List(Of RentalRequestItem)
        Dim dt As DataTable = Database.ExecuteDataTable(
            "SELECT ri.*, i.Name AS ItemName, i.UnitType FROM RentalRequestItems ri JOIN InventoryItems i ON ri.InventoryItemId=i.Id WHERE ri.RentalRequestId=@rid",
            New SQLiteParameter("@rid", requestId))
        Dim list As New List(Of RentalRequestItem)()
        For Each row As DataRow In dt.Rows
            list.Add(New RentalRequestItem With {
                .Id = Convert.ToInt32(row("Id")),
                .RentalRequestId = Convert.ToInt32(row("RentalRequestId")),
                .InventoryItemId = Convert.ToInt32(row("InventoryItemId")),
                .ItemName = row("ItemName").ToString(),
                .UnitType = row("UnitType").ToString(),
                .RequestedQuantity = Convert.ToInt32(row("RequestedQuantity")),
                .UnitPriceAtRequest = Convert.ToDecimal(row("UnitPriceAtRequest"))
            })
        Next
        Return list
    End Function

    Public Shared Function CreateRequest(userId As String, submitterRole As String,
                                          eventDate As DateTime, startDate As DateTime, endDate As DateTime,
                                          itemIds() As Integer, quantities() As Integer, documentPath As String) As RentalRequest
        ' Validate date range
        If startDate.Date < DateTime.Today Then
            Throw New InvalidOperationException("Start date cannot be in the past.")
        End If
        If endDate.Date < startDate.Date Then
            Throw New InvalidOperationException("Item Required Until date cannot be earlier than Item Required From date.")
        End If

        ' Validate that there are no duplicate items within this request
        Dim activeItemIds As New List(Of Integer)()
        For i As Integer = 0 To itemIds.Length - 1
            If quantities(i) > 0 Then
                activeItemIds.Add(itemIds(i))
            End If
        Next
        If activeItemIds.Count <> activeItemIds.Distinct().Count() Then
            Throw New InvalidOperationException("Duplicate items are not allowed in the same rental request.")
        End If

        ' Calculate duration in days (matching frontend logic)
        Dim days As Integer = 1
        Dim timeDiff = endDate.Date - startDate.Date
        If timeDiff.Days > 0 Then days = timeDiff.Days

        ' Pre-calculate grand total and validate stock availability for the date range
        Dim grandTotal As Decimal = 0
        For i As Integer = 0 To itemIds.Length - 1
            If quantities(i) > 0 Then
                Dim item As InventoryItem = InventoryService.GetItemById(itemIds(i))
                If item IsNot Nothing Then
                    Dim available = GetAvailableQuantityForDates(itemIds(i), startDate, endDate, 0)
                    If quantities(i) > available Then
                        Throw New InvalidOperationException("Requested quantity (" & quantities(i) & ") for item '" & item.Name & "' exceeds available stock (" & available & ") for the selected date range.")
                    End If
                    grandTotal += item.CurrentPrice * quantities(i)
                End If
            End If
        Next

        grandTotal = grandTotal * days

        Dim reqNumber As String = "REQ-" & DateTime.Now.ToString("yyyyMMdd-HHmmss") & "-" & New Random().Next(100, 999).ToString()

        ' Determine initial approval stage and status based on submitter role and grand total (matching MVC)
        Dim stage As ApprovalStage = ApprovalStage.PendingHOD
        Dim status As RequestStatus = RequestStatus.Pending
        Dim isAutoApprove As Boolean = False

        Select Case submitterRole
            Case "User"
                stage = ApprovalStage.PendingHOD
            Case "HOD"
                If grandTotal <= 10000 Then
                    stage = ApprovalStage.PendingHR
                Else
                    stage = ApprovalStage.PendingGM
                End If
            Case "GM"
                stage = ApprovalStage.Approved
                status = RequestStatus.Approved
                isAutoApprove = True
            Case "SuperAdmin"
                If grandTotal <= 10000 Then
                    stage = ApprovalStage.Approved
                    status = RequestStatus.Approved
                    isAutoApprove = True
                Else
                    stage = ApprovalStage.PendingGM
                End If
        End Select

        Database.ExecuteNonQuery("INSERT INTO RentalRequests(RequestNumber,UserId,SubmittedByRole,EventDate,StartDate,EndDate,InPrincipalDocumentPath,GrandTotal,Status,ApprovalStage,CreatedAt) VALUES(@rn,@uid,@sr,@ed,@sd,@end,@doc,@gt,@st,@as,@ca)",
            New SQLiteParameter("@rn", reqNumber),
            New SQLiteParameter("@uid", userId),
            New SQLiteParameter("@sr", submitterRole),
            New SQLiteParameter("@ed", eventDate.ToString("yyyy-MM-dd")),
            New SQLiteParameter("@sd", startDate.ToString("yyyy-MM-dd")),
            New SQLiteParameter("@end", endDate.ToString("yyyy-MM-dd")),
            New SQLiteParameter("@doc", If(documentPath, "")),
            New SQLiteParameter("@gt", grandTotal),
            New SQLiteParameter("@st", CInt(status)),
            New SQLiteParameter("@as", CInt(stage)),
            New SQLiteParameter("@ca", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")))

        Dim newId As Object = Database.ExecuteScalar("SELECT last_insert_rowid()")
        Dim requestId As Integer = Convert.ToInt32(newId)

        ' Insert request items
        For i As Integer = 0 To itemIds.Length - 1
            If quantities(i) > 0 Then
                Dim item As InventoryItem = InventoryService.GetItemById(itemIds(i))
                If item IsNot Nothing Then
                    Database.ExecuteNonQuery("INSERT INTO RentalRequestItems(RentalRequestId,InventoryItemId,RequestedQuantity,UnitPriceAtRequest) VALUES(@rid,@iid,@qty,@price)",
                        New SQLiteParameter("@rid", requestId),
                        New SQLiteParameter("@iid", itemIds(i)),
                        New SQLiteParameter("@qty", quantities(i)),
                        New SQLiteParameter("@price", item.CurrentPrice))

                    ' Reserve inventory
                    Database.ExecuteNonQuery("UPDATE InventoryItems SET ReservedQuantity=ReservedQuantity+@qty, UpdatedAt=@now WHERE Id=@iid",
                        New SQLiteParameter("@qty", quantities(i)),
                        New SQLiteParameter("@now", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")),
                        New SQLiteParameter("@iid", itemIds(i)))

                    Dim allocationStatus As String = "Reserved"
                    If status = RequestStatus.Approved Then
                        allocationStatus = "Approved"
                    End If

                    Database.ExecuteNonQuery("INSERT INTO InventoryAllocations(RequestId,InventoryItemId,AllocatedQuantity,Status) VALUES(@rid,@iid,@qty,@status)",
                        New SQLiteParameter("@rid", requestId),
                        New SQLiteParameter("@iid", itemIds(i)),
                        New SQLiteParameter("@qty", quantities(i)),
                        New SQLiteParameter("@status", allocationStatus))

                    ' Log transaction
                    Dim performedBy As String = If(status = RequestStatus.Approved, "Auto-Approved (" & submitterRole & ")", "Reserved (" & submitterRole & ")")
                    Database.ExecuteNonQuery("INSERT INTO InventoryTransactions (InventoryItemId, TransactionType, Quantity, Notes, CreatedAt) VALUES (@itemId, 'Allocation', @qty, @notes, @now)",
                                             New SQLiteParameter("@itemId", itemIds(i)),
                                             New SQLiteParameter("@qty", quantities(i)),
                                             New SQLiteParameter("@notes", performedBy & " request " & reqNumber),
                                             New SQLiteParameter("@now", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")))
                End If
            End If
        Next

        Return GetRequestById(requestId)
    End Function

    Public Shared Function GetAvailableQuantityForDates(itemId As Integer, startDate As DateTime, endDate As DateTime, excludeRequestId As Integer) As Integer
        Dim totalQty As Integer = 0
        Dim dtItem As DataTable = Database.ExecuteDataTable("SELECT TotalQuantity FROM InventoryItems WHERE Id=@id", New SQLiteParameter("@id", itemId))
        If dtItem.Rows.Count > 0 Then
            totalQty = Convert.ToInt32(dtItem.Rows(0)("TotalQuantity"))
        Else
            Return 0
        End If

        Dim maxReserved As Integer = 0
        Dim sql As String = "SELECT a.AllocatedQuantity, r.StartDate, r.EndDate " &
                            "FROM InventoryAllocations a " &
                            "JOIN RentalRequests r ON a.RequestId = r.Id " &
                            "WHERE a.InventoryItemId = @itemId " &
                            "  AND a.RequestId <> @excludeId " &
                            "  AND (a.Status = 'Approved' OR a.Status = 'Reserved')"
        Dim dtAlloc As DataTable = Database.ExecuteDataTable(sql, 
            New SQLiteParameter("@itemId", itemId),
            New SQLiteParameter("@excludeId", excludeRequestId))

        Dim currentDate As DateTime = startDate.Date
        Dim finalDate As DateTime = endDate.Date
        While currentDate <= finalDate
            Dim reservedOnDay As Integer = 0
            For Each row As DataRow In dtAlloc.Rows
                Dim allocStart As DateTime = DateTime.Parse(row("StartDate").ToString())
                Dim allocEnd As DateTime = DateTime.Parse(row("EndDate").ToString())
                If allocStart <= currentDate AndAlso allocEnd >= currentDate Then
                    reservedOnDay += Convert.ToInt32(row("AllocatedQuantity"))
                End If
            Next
            If reservedOnDay > maxReserved Then
                maxReserved = reservedOnDay
            End If
            currentDate = currentDate.AddDays(1)
        End While

        Return Math.Max(0, totalQty - maxReserved)
    End Function

    Public Shared Function ReleaseExpiredAllocations() As Integer
        Dim todayStr As String = DateTime.Today.ToString("yyyy-MM-dd")
        Dim sqlExpired As String = "SELECT a.AllocationId, a.RequestId, a.InventoryItemId, a.AllocatedQuantity, r.RequestNumber, r.UserId, r.Status " &
                                   "FROM InventoryAllocations a " &
                                   "JOIN RentalRequests r ON a.RequestId = r.Id " &
                                   "WHERE (a.Status = 'Approved' OR a.Status = 'Reserved') " &
                                   "  AND r.EndDate < @today"
        Dim dtExpired As DataTable = Database.ExecuteDataTable(sqlExpired, New SQLiteParameter("@today", todayStr))
        
        If dtExpired.Rows.Count = 0 Then Return 0
        
        Dim releasedCount As Integer = 0
        Dim requestsToUpdate As New Dictionary(Of Integer, List(Of DataRow))()
        For Each row As DataRow In dtExpired.Rows
            Dim reqId As Integer = Convert.ToInt32(row("RequestId"))
            If Not requestsToUpdate.ContainsKey(reqId) Then
                requestsToUpdate(reqId) = New List(Of DataRow)()
            End If
            requestsToUpdate(reqId).Add(row)
        Next
        
        Dim nowStr As String = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        
        For Each kvp In requestsToUpdate
            Dim reqId As Integer = kvp.Key
            Dim rows As List(Of DataRow) = kvp.Value
            Dim requestNumber As String = rows(0)("RequestNumber").ToString()
            Dim userId As String = rows(0)("UserId").ToString()
            Dim reqStatus As Integer = Convert.ToInt32(rows(0)("Status"))
            
            For Each row In rows
                Dim allocId As Integer = Convert.ToInt32(row("AllocationId"))
                Dim itemId As Integer = Convert.ToInt32(row("InventoryItemId"))
                Dim qty As Integer = Convert.ToInt32(row("AllocatedQuantity"))
                
                Database.ExecuteNonQuery("UPDATE InventoryItems SET ReservedQuantity = MAX(0, ReservedQuantity - @qty), UpdatedAt = @now WHERE Id = @itemId",
                                         New SQLiteParameter("@qty", qty),
                                         New SQLiteParameter("@now", nowStr),
                                         New SQLiteParameter("@itemId", itemId))
                                         
                Database.ExecuteNonQuery("UPDATE InventoryAllocations SET Status = 'Released' WHERE AllocationId = @allocId",
                                         New SQLiteParameter("@allocId", allocId))
                                         
                Database.ExecuteNonQuery("INSERT INTO InventoryTransactions (InventoryItemId, TransactionType, Quantity, Notes, CreatedAt) VALUES (@itemId, 'Release', @qty, @notes, @now)",
                                         New SQLiteParameter("@itemId", itemId),
                                         New SQLiteParameter("@qty", qty),
                                         New SQLiteParameter("@notes", "Auto-released: allocation expired. Request #" & requestNumber),
                                         New SQLiteParameter("@now", nowStr))
                                         
                releasedCount += 1
            Next
            
            If reqStatus = CInt(RequestStatus.Approved) OrElse reqStatus = CInt(RequestStatus.Pending) Then
                Database.ExecuteNonQuery("UPDATE RentalRequests SET Status = @status, ReviewedAt = @now, ReviewedByEmployeeId = 'SYSTEM' WHERE Id = @id",
                                         New SQLiteParameter("@status", CInt(RequestStatus.Returned)),
                                         New SQLiteParameter("@now", nowStr),
                                         New SQLiteParameter("@id", reqId))
                                         
                NotificationService.CreateNotification(userId, 
                                                       "Rental Period Ended — Items Released ✓", 
                                                       "Your rental request #" & requestNumber & " has ended. All allocated inventory has been automatically released back to stock. Thank you!")
            End If
        Next
        
        Return releasedCount
    End Function

    Public Shared Function ApproveRequest(requestId As Integer, approverId As String, approverRole As String) As ServiceResult
        Dim req As RentalRequest = GetRequestById(requestId)
        If req Is Nothing Then Return New ServiceResult(False, "Request not found.")
        If req.Status <> RequestStatus.Pending AndAlso req.Status <> RequestStatus.Waitlisted Then
            Return New ServiceResult(False, "Request is not in a pending state.")
        End If
        ' Self-approval block
        If req.UserId = approverId Then Return New ServiceResult(False, "Self-approval is not allowed.")

        Dim approverEmpId As Object = Database.ExecuteScalar("SELECT EmployeeId FROM AspNetUsers WHERE Id=@id", New SQLiteParameter("@id", approverId))
        Dim empId As String = If(approverEmpId IsNot Nothing, approverEmpId.ToString(), "")
        Dim now As String = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")

        If approverRole = "HOD" AndAlso req.ApprovalStage = ApprovalStage.PendingHOD Then
            If req.GrandTotal > 10000 Then
                ' Escalate to GM
                Database.ExecuteNonQuery("UPDATE RentalRequests SET ApprovalStage=@st, HODApprovedAt=@now, HODApprovedByEmployeeId=@eid WHERE Id=@id",
                    New SQLiteParameter("@st", CInt(ApprovalStage.PendingGM)),
                    New SQLiteParameter("@now", now),
                    New SQLiteParameter("@eid", empId),
                    New SQLiteParameter("@id", requestId))
                NotificationService.CreateNotification(req.UserId, "Request Escalated to GM", "Your request " & req.RequestNumber & " has been approved by HOD and escalated to GM.")
                Return New ServiceResult(True, "Approved by HOD. Escalated to GM for high-value request.")
            Else
                ' Escalate to HR (SuperAdmin)
                Database.ExecuteNonQuery("UPDATE RentalRequests SET ApprovalStage=@st, HODApprovedAt=@now, HODApprovedByEmployeeId=@eid WHERE Id=@id",
                    New SQLiteParameter("@st", CInt(ApprovalStage.PendingHR)),
                    New SQLiteParameter("@now", now),
                    New SQLiteParameter("@eid", empId),
                    New SQLiteParameter("@id", requestId))
                NotificationService.CreateNotification(req.UserId, "HOD Approved", "Your request " & req.RequestNumber & " has been approved by HOD. Awaiting HR approval.")
                Return New ServiceResult(True, "Approved by HOD. Awaiting HR (SuperAdmin) final approval.")
            End If

        ElseIf approverRole = "GM" AndAlso req.ApprovalStage = ApprovalStage.PendingGM Then
            ' GM Approval is terminal for requests above threshold (matching MVC)
            Database.ExecuteNonQuery("UPDATE RentalRequests SET Status=@st, ApprovalStage=@as, GMApprovedAt=@now, GMApprovedByEmployeeId=@eid, ReviewedAt=@now, ReviewedByEmployeeId=@eid WHERE Id=@id",
                New SQLiteParameter("@st", CInt(RequestStatus.Approved)),
                New SQLiteParameter("@as", CInt(ApprovalStage.Approved)),
                New SQLiteParameter("@now", now),
                New SQLiteParameter("@eid", empId),
                New SQLiteParameter("@id", requestId))
            ' Convert reserved allocations to approved
            Database.ExecuteNonQuery("UPDATE InventoryAllocations SET Status='Approved' WHERE RequestId=@rid", New SQLiteParameter("@rid", requestId))
            NotificationService.CreateNotification(req.UserId, "Request Approved", "Your request " & req.RequestNumber & " has been fully approved by GM!")
            Return New ServiceResult(True, "Request fully approved by GM.")

        ElseIf approverRole = "SuperAdmin" AndAlso (req.ApprovalStage = ApprovalStage.PendingHR OrElse req.ApprovalStage = ApprovalStage.PendingHOD) Then
            Database.ExecuteNonQuery("UPDATE RentalRequests SET Status=@st, ApprovalStage=@as, HRApprovedAt=@now, HRApprovedByEmployeeId=@eid, ReviewedAt=@now, ReviewedByEmployeeId=@eid WHERE Id=@id",
                New SQLiteParameter("@st", CInt(RequestStatus.Approved)),
                New SQLiteParameter("@as", CInt(ApprovalStage.Approved)),
                New SQLiteParameter("@now", now),
                New SQLiteParameter("@eid", empId),
                New SQLiteParameter("@id", requestId))
            ' Convert reserved allocations to approved
            Database.ExecuteNonQuery("UPDATE InventoryAllocations SET Status='Approved' WHERE RequestId=@rid", New SQLiteParameter("@rid", requestId))
            NotificationService.CreateNotification(req.UserId, "Request Approved", "Your request " & req.RequestNumber & " has been fully approved!")
            Return New ServiceResult(True, "Request fully approved.")
        End If

        Return New ServiceResult(False, "Cannot approve at this stage with your current role.")
    End Function

    Public Shared Function RejectRequest(requestId As Integer, reason As String, rejectedByEmpId As String) As Boolean
        If String.IsNullOrWhiteSpace(reason) Then Return False
        Dim req As RentalRequest = GetRequestById(requestId)
        If req Is Nothing Then Return False
        Dim now As String = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        Database.ExecuteNonQuery("UPDATE RentalRequests SET Status=@st, ApprovalStage=@as, RejectionReason=@r, ReviewedAt=@now, ReviewedByEmployeeId=@eid WHERE Id=@id",
            New SQLiteParameter("@st", CInt(RequestStatus.Rejected)),
            New SQLiteParameter("@as", CInt(ApprovalStage.Rejected)),
            New SQLiteParameter("@r", reason),
            New SQLiteParameter("@now", now),
            New SQLiteParameter("@eid", rejectedByEmpId),
            New SQLiteParameter("@id", requestId))
        ' Release reserved inventory
        ReleaseReservations(requestId)
        NotificationService.CreateNotification(req.UserId, "Request Rejected", "Your request " & req.RequestNumber & " was rejected. Reason: " & reason)
        Return True
    End Function

    Public Shared Function CancelRequest(requestId As Integer, cancelledByUserName As String) As Boolean
        Dim now As String = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        Database.ExecuteNonQuery("UPDATE RentalRequests SET Status=@st, ReviewedAt=@now, ReviewedByEmployeeId=@eid WHERE Id=@id",
            New SQLiteParameter("@st", CInt(RequestStatus.Cancelled)),
            New SQLiteParameter("@now", now),
            New SQLiteParameter("@eid", cancelledByUserName),
            New SQLiteParameter("@id", requestId))
        ReleaseReservations(requestId)
        Return True
    End Function
    Private Shared Sub ReleaseReservations(requestId As Integer)
        Dim items As List(Of RentalRequestItem) = GetRequestItems(requestId)
        Dim nowStr As String = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        For Each item As RentalRequestItem In items
            Database.ExecuteNonQuery("UPDATE InventoryItems SET ReservedQuantity=MAX(0,ReservedQuantity-@qty), UpdatedAt=@now WHERE Id=@iid",
                New SQLiteParameter("@qty", item.RequestedQuantity),
                New SQLiteParameter("@now", nowStr),
                New SQLiteParameter("@iid", item.InventoryItemId))
                
            Database.ExecuteNonQuery("INSERT INTO InventoryTransactions (InventoryItemId, TransactionType, Quantity, Notes, CreatedAt) VALUES (@itemId, 'Release', @qty, @notes, @now)",
                                     New SQLiteParameter("@itemId", item.InventoryItemId),
                                     New SQLiteParameter("@qty", item.RequestedQuantity),
                                     New SQLiteParameter("@notes", "Released allocation due to rejection/cancellation of request ID " & requestId),
                                     New SQLiteParameter("@now", nowStr))
        Next
        Database.ExecuteNonQuery("UPDATE InventoryAllocations SET Status='Cancelled' WHERE RequestId=@rid", New SQLiteParameter("@rid", requestId))
    End Sub
    Private Shared Function MapRequests(dt As DataTable) As List(Of RentalRequest)
        Dim list As New List(Of RentalRequest)()
        For Each row As DataRow In dt.Rows
            list.Add(MapRequest(row))
        Next
        Return list
    End Function

    Private Shared Function MapRequest(row As DataRow) As RentalRequest
        Return New RentalRequest With {
            .Id = Convert.ToInt32(row("Id")),
            .RequestNumber = row("RequestNumber").ToString(),
            .UserId = row("UserId").ToString(),
            .UserFullName = If(row("UserFullName") Is DBNull.Value, "", row("UserFullName").ToString()),
            .UserEmployeeId = If(row("UserEmployeeId") Is DBNull.Value, "", row("UserEmployeeId").ToString()),
            .UserDepartment = If(row("UserDepartment") Is DBNull.Value, "", row("UserDepartment").ToString()),
            .SubmittedByRole = If(row("SubmittedByRole") Is DBNull.Value, "User", row("SubmittedByRole").ToString()),
            .EventDate = DateTime.Parse(row("EventDate").ToString()),
            .StartDate = DateTime.Parse(row("StartDate").ToString()),
            .EndDate = DateTime.Parse(row("EndDate").ToString()),
            .InPrincipalDocumentPath = If(row("InPrincipalDocumentPath") Is DBNull.Value, "", row("InPrincipalDocumentPath").ToString()),
            .GrandTotal = Convert.ToDecimal(row("GrandTotal")),
            .Status = CType(Convert.ToInt32(row("Status")), RequestStatus),
            .ApprovalStage = CType(Convert.ToInt32(row("ApprovalStage")), ApprovalStage),
            .HODApprovedAt = If(row("HODApprovedAt") Is DBNull.Value, Nothing, CType(DateTime.Parse(row("HODApprovedAt").ToString()), DateTime?)),
            .HODApprovedByEmployeeId = If(row("HODApprovedByEmployeeId") Is DBNull.Value, "", row("HODApprovedByEmployeeId").ToString()),
            .GMApprovedAt = If(row("GMApprovedAt") Is DBNull.Value, Nothing, CType(DateTime.Parse(row("GMApprovedAt").ToString()), DateTime?)),
            .GMApprovedByEmployeeId = If(row("GMApprovedByEmployeeId") Is DBNull.Value, "", row("GMApprovedByEmployeeId").ToString()),
            .HRApprovedAt = If(row("HRApprovedAt") Is DBNull.Value, Nothing, CType(DateTime.Parse(row("HRApprovedAt").ToString()), DateTime?)),
            .HRApprovedByEmployeeId = If(row("HRApprovedByEmployeeId") Is DBNull.Value, "", row("HRApprovedByEmployeeId").ToString()),
            .CreatedAt = DateTime.Parse(row("CreatedAt").ToString()),
            .ReviewedAt = If(row("ReviewedAt") Is DBNull.Value, Nothing, CType(DateTime.Parse(row("ReviewedAt").ToString()), DateTime?)),
            .ReviewedByEmployeeId = If(row("ReviewedByEmployeeId") Is DBNull.Value, "", row("ReviewedByEmployeeId").ToString()),
            .RejectionReason = If(row("RejectionReason") Is DBNull.Value, "", row("RejectionReason").ToString())
        }
    End Function
End Class

' ── NotificationService ───────────────────────────────────────────────────────
Public Class NotificationService
    Public Shared Sub CreateNotification(userId As String, title As String, message As String)
        Database.ExecuteNonQuery("INSERT INTO Notifications(UserId,Title,Message,IsRead,CreatedAt) VALUES(@uid,@t,@m,0,@ca)",
            New SQLiteParameter("@uid", userId),
            New SQLiteParameter("@t", title),
            New SQLiteParameter("@m", message),
            New SQLiteParameter("@ca", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")))
    End Sub

    Public Shared Function GetUserNotifications(userId As String) As List(Of AppNotification)
        Dim dt As DataTable = Database.ExecuteDataTable(
            "SELECT * FROM Notifications WHERE UserId=@uid ORDER BY CreatedAt DESC LIMIT 50",
            New SQLiteParameter("@uid", userId))
        Return MapNotifications(dt)
    End Function

    Public Shared Function GetUnreadCount(userId As String) As Integer
        Dim result As Object = Database.ExecuteScalar("SELECT COUNT(*) FROM Notifications WHERE UserId=@uid AND IsRead=0", New SQLiteParameter("@uid", userId))
        Return Convert.ToInt32(result)
    End Function

    Public Shared Sub MarkAsRead(notifId As Integer)
        Database.ExecuteNonQuery("UPDATE Notifications SET IsRead=1 WHERE Id=@id", New SQLiteParameter("@id", notifId))
    End Sub

    Public Shared Sub MarkAllAsRead(userId As String)
        Database.ExecuteNonQuery("UPDATE Notifications SET IsRead=1 WHERE UserId=@uid", New SQLiteParameter("@uid", userId))
    End Sub

    Private Shared Function MapNotifications(dt As DataTable) As List(Of AppNotification)
        Dim list As New List(Of AppNotification)()
        For Each row As DataRow In dt.Rows
            list.Add(New AppNotification With {
                .Id = Convert.ToInt32(row("Id")),
                .UserId = row("UserId").ToString(),
                .Title = row("Title").ToString(),
                .Message = If(row("Message") Is DBNull.Value, "", row("Message").ToString()),
                .IsRead = Convert.ToBoolean(row("IsRead")),
                .CreatedAt = DateTime.Parse(row("CreatedAt").ToString())
            })
        Next
        Return list
    End Function
End Class

' ── AuditService ─────────────────────────────────────────────────────────────
Public Class AuditService
    Public Shared Sub Log(userId As String, action As String, entityName As String, entityId As String,
                           description As String, oldValue As String, newValue As String, ipAddress As String)
        Database.ExecuteNonQuery("INSERT INTO AuditLogs(UserId,Action,EntityName,EntityId,Description,OldValue,NewValue,IpAddress,Timestamp) VALUES(@uid,@a,@en,@eid,@d,@ov,@nv,@ip,@ts)",
            New SQLiteParameter("@uid", If(userId, "")),
            New SQLiteParameter("@a", action),
            New SQLiteParameter("@en", If(entityName, "")),
            New SQLiteParameter("@eid", If(entityId, "")),
            New SQLiteParameter("@d", If(description, "")),
            New SQLiteParameter("@ov", If(oldValue, "")),
            New SQLiteParameter("@nv", If(newValue, "")),
            New SQLiteParameter("@ip", If(ipAddress, "")),
            New SQLiteParameter("@ts", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")))
    End Sub

    Public Shared Function GetLogs(page As Integer, pageSize As Integer) As LogResult
        Dim total As Integer = Convert.ToInt32(Database.ExecuteScalar("SELECT COUNT(*) FROM AuditLogs"))
        Dim offset As Integer = (page - 1) * pageSize
        Dim dt As DataTable = Database.ExecuteDataTable(
            "SELECT al.*, u.FullName AS UserName FROM AuditLogs al LEFT JOIN AspNetUsers u ON al.UserId=u.Id ORDER BY al.Timestamp DESC LIMIT @ps OFFSET @off",
            New SQLiteParameter("@ps", pageSize),
            New SQLiteParameter("@off", offset))
        Dim list As New List(Of AuditLog)()
        For Each row As DataRow In dt.Rows
            list.Add(New AuditLog With {
                .Id = Convert.ToInt32(row("Id")),
                .UserId = If(row("UserId") Is DBNull.Value, "", row("UserId").ToString()),
                .UserName = If(row("UserName") Is DBNull.Value, "System", row("UserName").ToString()),
                .Action = row("Action").ToString(),
                .EntityName = If(row("EntityName") Is DBNull.Value, "", row("EntityName").ToString()),
                .EntityId = If(row("EntityId") Is DBNull.Value, "", row("EntityId").ToString()),
                .Description = If(row("Description") Is DBNull.Value, "", row("Description").ToString()),
                .OldValue = If(row("OldValue") Is DBNull.Value, "", row("OldValue").ToString()),
                .NewValue = If(row("NewValue") Is DBNull.Value, "", row("NewValue").ToString()),
                .IpAddress = If(row("IpAddress") Is DBNull.Value, "", row("IpAddress").ToString()),
                .Timestamp = DateTime.Parse(row("Timestamp").ToString())
            })
        Next
        Return New LogResult(list, total)
    End Function
End Class

' ── ReportService ─────────────────────────────────────────────────────────────
Public Class ReportService
    Public Shared Function GenerateReport(reportType As String, selectedYear As Integer, selectedMonth As Integer,
                                           startDate As DateTime?, endDate As DateTime?) As ReportViewModel
        Dim vm As New ReportViewModel With {
            .ReportType = reportType,
            .SelectedYear = selectedYear,
            .SelectedMonth = selectedMonth,
            .StartDate = startDate,
            .EndDate = endDate
        }

        Dim sql As String
        Dim params As New List(Of SQLiteParameter)()

        If reportType = "Monthly" Then
            sql = "SELECT r.*, u.FullName AS UserFullName, u.EmployeeId AS UserEmployeeId, u.Department AS UserDepartment FROM RentalRequests r LEFT JOIN AspNetUsers u ON r.UserId=u.Id WHERE strftime('%Y',r.CreatedAt)=@y AND strftime('%m',r.CreatedAt)=@m ORDER BY r.CreatedAt DESC"
            params.Add(New SQLiteParameter("@y", selectedYear.ToString("0000")))
            params.Add(New SQLiteParameter("@m", selectedMonth.ToString("00")))
            vm.ReportTitle = New DateTime(selectedYear, selectedMonth, 1).ToString("MMMM yyyy") & " Report"
        ElseIf reportType = "Annual" Then
            sql = "SELECT r.*, u.FullName AS UserFullName, u.EmployeeId AS UserEmployeeId, u.Department AS UserDepartment FROM RentalRequests r LEFT JOIN AspNetUsers u ON r.UserId=u.Id WHERE strftime('%Y',r.CreatedAt)=@y ORDER BY r.CreatedAt DESC"
            params.Add(New SQLiteParameter("@y", selectedYear.ToString("0000")))
            vm.ReportTitle = "Annual Report " & selectedYear.ToString()
        Else
            ' Custom date range
            sql = "SELECT r.*, u.FullName AS UserFullName, u.EmployeeId AS UserEmployeeId, u.Department AS UserDepartment FROM RentalRequests r LEFT JOIN AspNetUsers u ON r.UserId=u.Id WHERE r.CreatedAt>=@sd AND r.CreatedAt<=@ed ORDER BY r.CreatedAt DESC"
            Dim sd As DateTime = If(startDate.HasValue, startDate.Value, DateTime.Today.AddMonths(-1))
            Dim ed As DateTime = If(endDate.HasValue, endDate.Value, DateTime.Today)
            params.Add(New SQLiteParameter("@sd", sd.ToString("yyyy-MM-dd")))
            params.Add(New SQLiteParameter("@ed", ed.AddDays(1).ToString("yyyy-MM-dd")))
            vm.ReportTitle = "Custom Report: " & sd.ToString("dd MMM yyyy") & " - " & ed.ToString("dd MMM yyyy")
        End If

        Dim allRequests As List(Of RentalRequest) = Nothing
        Dim dt As DataTable = Database.ExecuteDataTable(sql, params.ToArray())
        Dim requests As New List(Of RentalRequest)()
        For Each row As DataRow In dt.Rows
            requests.Add(New RentalRequest With {
                .Id = Convert.ToInt32(row("Id")),
                .RequestNumber = row("RequestNumber").ToString(),
                .UserId = row("UserId").ToString(),
                .UserFullName = If(row("UserFullName") Is DBNull.Value, "", row("UserFullName").ToString()),
                .UserEmployeeId = If(row("UserEmployeeId") Is DBNull.Value, "", row("UserEmployeeId").ToString()),
                .UserDepartment = If(row("UserDepartment") Is DBNull.Value, "", row("UserDepartment").ToString()),
                .EventDate = DateTime.Parse(row("EventDate").ToString()),
                .StartDate = DateTime.Parse(row("StartDate").ToString()),
                .EndDate = DateTime.Parse(row("EndDate").ToString()),
                .GrandTotal = Convert.ToDecimal(row("GrandTotal")),
                .Status = CType(Convert.ToInt32(row("Status")), RequestStatus),
                .CreatedAt = DateTime.Parse(row("CreatedAt").ToString())
            })
        Next

        vm.Requests = requests
        vm.TotalRequests = requests.Count
        vm.ApprovedCount = requests.Where(Function(r) r.Status = RequestStatus.Approved).Count()
        vm.RejectedCount = requests.Where(Function(r) r.Status = RequestStatus.Rejected).Count()
        vm.PendingCount = requests.Where(Function(r) r.Status = RequestStatus.Pending).Count()
        vm.TotalAmount = requests.Where(Function(r) r.Status = RequestStatus.Approved).Sum(Function(r) r.GrandTotal)
        Return vm
    End Function
End Class
