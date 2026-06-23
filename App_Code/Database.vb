Imports System
Imports System.IO
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SQLite
Imports System.Web
Imports System.Configuration
Imports System.Security.Cryptography
Imports System.Text

' ============================================================
'  Database.vb — SQLite ADO.NET helper for IOCL Community Hall
'  Matches the schema created by the ASP.NET Core EF migration
'  (SQLite database: iocl_community_hall.db)
' ============================================================
Public Class Database
    Shared Sub New()
        Dim appPath As String = AppDomain.CurrentDomain.BaseDirectory
        Dim appDataPath As String = Path.Combine(appPath, "App_Data")
        If Not Directory.Exists(appDataPath) Then
            Directory.CreateDirectory(appDataPath)
        End If
        AppDomain.CurrentDomain.SetData("DataDirectory", appDataPath)
    End Sub

    Public Shared Function GetConnectionString() As String
        Return ConfigurationManager.ConnectionStrings("SQLiteDB").ConnectionString
    End Function

    Public Shared Function GetConnection() As SQLiteConnection
        Dim conn As New SQLiteConnection(GetConnectionString())
        conn.Open()
        Return conn
    End Function

    Public Shared Function ExecuteNonQuery(ByVal sql As String, ByVal ParamArray parameters() As SQLiteParameter) As Integer
        Using conn As SQLiteConnection = GetConnection()
            Using cmd As New SQLiteCommand(sql, conn)
                If parameters IsNot Nothing Then cmd.Parameters.AddRange(parameters)
                Return cmd.ExecuteNonQuery()
            End Using
        End Using
    End Function

    Public Shared Function ExecuteScalar(ByVal sql As String, ByVal ParamArray parameters() As SQLiteParameter) As Object
        Using conn As SQLiteConnection = GetConnection()
            Using cmd As New SQLiteCommand(sql, conn)
                If parameters IsNot Nothing Then cmd.Parameters.AddRange(parameters)
                Return cmd.ExecuteScalar()
            End Using
        End Using
    End Function

    Public Shared Function ExecuteDataTable(ByVal sql As String, ByVal ParamArray parameters() As SQLiteParameter) As DataTable
        Dim dt As New DataTable()
        Using conn As SQLiteConnection = GetConnection()
            Using cmd As New SQLiteCommand(sql, conn)
                If parameters IsNot Nothing Then cmd.Parameters.AddRange(parameters)
                Using adapter As New SQLiteDataAdapter(cmd)
                    adapter.Fill(dt)
                End Using
            End Using
        End Using
        Return dt
    End Function

    ' ── Schema Initialisation ─────────────────────────────────────────────────
    Public Shared Sub InitializeDatabase()
        Using conn As SQLiteConnection = GetConnection()
            Using trans = conn.BeginTransaction()
                Try
                    ' AspNetUsers (ASP.NET Identity compatible)
                    ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS AspNetUsers (Id TEXT PRIMARY KEY, UserName TEXT, NormalizedUserName TEXT, Email TEXT, NormalizedEmail TEXT, EmailConfirmed INTEGER, PasswordHash TEXT, SecurityStamp TEXT, ConcurrencyStamp TEXT, PhoneNumber TEXT, PhoneNumberConfirmed INTEGER, TwoFactorEnabled INTEGER, LockoutEnd TEXT, LockoutEnabled INTEGER, AccessFailedCount INTEGER, FullName TEXT, EmployeeId TEXT, Department TEXT, IsActive INTEGER DEFAULT 1, MustChangePassword INTEGER DEFAULT 0, CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP, LastLoginAt TEXT);")

                    ' AspNetRoles
                    ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS AspNetRoles (Id TEXT PRIMARY KEY, Name TEXT, NormalizedName TEXT, ConcurrencyStamp TEXT);")

                    ' AspNetUserRoles
                    ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS AspNetUserRoles (UserId TEXT NOT NULL, RoleId TEXT NOT NULL, PRIMARY KEY(UserId, RoleId), FOREIGN KEY(UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE, FOREIGN KEY(RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE);")

                    ' Employees
                    ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS Employees (EmployeeId TEXT PRIMARY KEY, EmployeeName TEXT NOT NULL, Department TEXT, Designation TEXT, Email TEXT, PhoneNumber TEXT, QuarterAddress TEXT, Status TEXT DEFAULT 'Active', CreatedDate TEXT DEFAULT CURRENT_TIMESTAMP, UpdatedDate TEXT);")

                    ' InventoryCategories
                    ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS InventoryCategories (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Description TEXT, CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP);")

                    ' InventoryItems
                    ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS InventoryItems (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Description TEXT, CategoryId INTEGER, UnitType TEXT DEFAULT 'Nos', TotalQuantity INTEGER DEFAULT 0, ReservedQuantity INTEGER DEFAULT 0, CurrentPrice DECIMAL(10,2) DEFAULT 0, IsActive INTEGER DEFAULT 1, ImagePath TEXT, CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP, UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY(CategoryId) REFERENCES InventoryCategories(Id));")

                    ' PriceHistories
                    ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS PriceHistories (Id INTEGER PRIMARY KEY AUTOINCREMENT, InventoryItemId INTEGER NOT NULL, PreviousPrice DECIMAL(10,2), UpdatedPrice DECIMAL(10,2) NOT NULL, EffectiveDate TEXT, Reason TEXT, UpdatedBy TEXT, UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY(InventoryItemId) REFERENCES InventoryItems(Id));")

                    ' RentalRequests
                    ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS RentalRequests (Id INTEGER PRIMARY KEY AUTOINCREMENT, RequestNumber TEXT UNIQUE, UserId TEXT NOT NULL, SubmittedByRole TEXT DEFAULT 'User', EventDate TEXT NOT NULL, StartDate TEXT NOT NULL, EndDate TEXT NOT NULL, InPrincipalDocumentPath TEXT, GrandTotal DECIMAL(12,2) DEFAULT 0, Status INTEGER DEFAULT 0, ApprovalStage INTEGER DEFAULT 0, HODApprovedAt TEXT, HODApprovedByEmployeeId TEXT, GMApprovedAt TEXT, GMApprovedByEmployeeId TEXT, HRApprovedAt TEXT, HRApprovedByEmployeeId TEXT, CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP, ReviewedAt TEXT, ReviewedByEmployeeId TEXT, RejectionReason TEXT, FOREIGN KEY(UserId) REFERENCES AspNetUsers(Id));")

                    ' RentalRequestItems
                    ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS RentalRequestItems (Id INTEGER PRIMARY KEY AUTOINCREMENT, RentalRequestId INTEGER NOT NULL, InventoryItemId INTEGER NOT NULL, RequestedQuantity INTEGER DEFAULT 1, UnitPriceAtRequest DECIMAL(10,2) DEFAULT 0, FOREIGN KEY(RentalRequestId) REFERENCES RentalRequests(Id) ON DELETE CASCADE, FOREIGN KEY(InventoryItemId) REFERENCES InventoryItems(Id));")

                    ' InventoryAllocations
                    ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS InventoryAllocations (AllocationId INTEGER PRIMARY KEY AUTOINCREMENT, RequestId INTEGER NOT NULL, InventoryItemId INTEGER NOT NULL, AllocatedQuantity INTEGER DEFAULT 0, Status TEXT DEFAULT 'Reserved', AllocatedAt TEXT DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY(RequestId) REFERENCES RentalRequests(Id) ON DELETE CASCADE, FOREIGN KEY(InventoryItemId) REFERENCES InventoryItems(Id));")

                    ' Notifications
                    ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS Notifications (Id INTEGER PRIMARY KEY AUTOINCREMENT, UserId TEXT NOT NULL, Title TEXT NOT NULL, Message TEXT, IsRead INTEGER DEFAULT 0, CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY(UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE);")

                    ' AuditLogs
                    ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS AuditLogs (Id INTEGER PRIMARY KEY AUTOINCREMENT, UserId TEXT, Action TEXT NOT NULL, EntityName TEXT, EntityId TEXT, Description TEXT, OldValue TEXT, NewValue TEXT, IpAddress TEXT, Timestamp TEXT DEFAULT CURRENT_TIMESTAMP);")

                    ' InventoryTransactions
                    ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS InventoryTransactions (Id INTEGER PRIMARY KEY AUTOINCREMENT, InventoryItemId INTEGER NOT NULL, TransactionType TEXT, Quantity INTEGER, Notes TEXT, CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY(InventoryItemId) REFERENCES InventoryItems(Id));")

                    trans.Commit()
                Catch ex As Exception
                    trans.Rollback()
                    Throw
                End Try
            End Using
        End Using

        SeedRolesAndAdmin()
    End Sub

    Private Shared Sub SeedRolesAndAdmin()
        ' Seed roles if not present
        Dim roles() As String = {"SuperAdmin", "GM", "HOD", "User"}
        For Each roleName As String In roles
            Dim exists As Object = ExecuteScalar("SELECT COUNT(*) FROM AspNetRoles WHERE NormalizedName=@n", New SQLiteParameter("@n", roleName.ToUpper()))
            If Convert.ToInt32(exists) = 0 Then
                Dim roleId As String = Guid.NewGuid().ToString()
                ExecuteNonQuery("INSERT INTO AspNetRoles(Id,Name,NormalizedName,ConcurrencyStamp) VALUES(@id,@n,@nn,@cs)",
                    New SQLiteParameter("@id", roleId),
                    New SQLiteParameter("@n", roleName),
                    New SQLiteParameter("@nn", roleName.ToUpper()),
                    New SQLiteParameter("@cs", Guid.NewGuid().ToString()))
            End If
        Next

        ' Seed Employees
        Dim employees As New List(Of SeedEmployee) From {
            New SeedEmployee("00000001", "Super Administrator", "IT Administration", "System Administrator / HR", "sysadmin@iocl.co.in", "0000000000", "IOCL Township Admin Block"),
            New SeedEmployee("20000001", "Suresh Patel", "General Management", "General Manager", "suresh.patel@iocl.co.in", "9800000001", "Block A, GM Quarter 01"),
            New SeedEmployee("30000001", "Arjun Mehta", "Engineering & Projects", "Head of Department", "arjun.mehta@iocl.co.in", "9800000002", "Block B, HOD Quarter 05"),
            New SeedEmployee("10000001", "Rajesh Sharma", "Engineering & Projects", "Deputy Manager", "rajesh.sharma@iocl.co.in", "9876543210", "Block A, Quarter 101"),
            New SeedEmployee("12345678", "Ramesh Kumar", "Refinery Operations", "Senior Engineer", "ramesh.kumar@iocl.co.in", "9876500001", "Block C, Quarter 305"),
            New SeedEmployee("23456789", "Priya Singh", "Human Resources", "HR Executive", "priya.singh@iocl.co.in", "9876500002", "Block B, Quarter 210")
        }

        For Each emp In employees
            Dim empExists As Object = ExecuteScalar("SELECT COUNT(*) FROM Employees WHERE EmployeeId=@eid", New SQLiteParameter("@eid", emp.EmpId))
            If Convert.ToInt32(empExists) = 0 Then
                ExecuteNonQuery("INSERT INTO Employees(EmployeeId,EmployeeName,Department,Designation,Email,PhoneNumber,QuarterAddress,Status) VALUES(@eid,@name,@dept,@desg,@email,@phone,@address,'Active')",
                    New SQLiteParameter("@eid", emp.EmpId),
                    New SQLiteParameter("@name", emp.Name),
                    New SQLiteParameter("@dept", emp.Dept),
                    New SQLiteParameter("@desg", emp.Desg),
                    New SQLiteParameter("@email", emp.Email),
                    New SQLiteParameter("@phone", emp.Phone),
                    New SQLiteParameter("@address", emp.Address))
            End If
        Next

        ' Seed AspNetUsers
        Dim users As New List(Of SeedUser) From {
            New SeedUser("00000001", "superadmin", "Super Administrator", "sysadmin@iocl.co.in", "IT Administration", "Admin@123", "SuperAdmin"),
            New SeedUser("20000001", "20000001", "Suresh Patel", "suresh.patel@iocl.co.in", "General Management", "Gm@12345", "GM"),
            New SeedUser("30000001", "30000001", "Arjun Mehta", "arjun.mehta@iocl.co.in", "Engineering & Projects", "Hod@1234", "HOD"),
            New SeedUser("10000001", "10000001", "Rajesh Sharma", "rajesh.sharma@iocl.co.in", "Engineering & Projects", "Admin@1234", "User"),
            New SeedUser("12345678", "12345678", "Ramesh Kumar", "ramesh.kumar@iocl.co.in", "Refinery Operations", "User@1234", "User"),
            New SeedUser("23456789", "23456789", "Priya Singh", "priya.singh@iocl.co.in", "Human Resources", "User@1234", "User")
        }

        For Each usr In users
            Dim userCheck As Object = ExecuteScalar("SELECT COUNT(*) FROM AspNetUsers WHERE UserName=@un", New SQLiteParameter("@un", usr.Username))
            If Convert.ToInt32(userCheck) = 0 Then
                Dim userId As String = Guid.NewGuid().ToString()
                Dim hash As String = PasswordHelper.HashPassword(usr.Password)
                ExecuteNonQuery("INSERT INTO AspNetUsers(Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,FullName,EmployeeId,Department,IsActive,MustChangePassword,CreatedAt) VALUES(@id,@un,@nun,@email,@nemail,1,@hash,@name,@eid,@dept,1,0,@ca)",
                    New SQLiteParameter("@id", userId),
                    New SQLiteParameter("@un", usr.Username),
                    New SQLiteParameter("@nun", usr.Username.ToUpper()),
                    New SQLiteParameter("@email", usr.Email),
                    New SQLiteParameter("@nemail", usr.Email.ToUpper()),
                    New SQLiteParameter("@hash", hash),
                    New SQLiteParameter("@name", usr.Name),
                    New SQLiteParameter("@eid", usr.EmpId),
                    New SQLiteParameter("@dept", usr.Dept),
                    New SQLiteParameter("@ca", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")))

                ' Assign role
                Dim roleId As Object = ExecuteScalar("SELECT Id FROM AspNetRoles WHERE NormalizedName=@r", New SQLiteParameter("@r", usr.Role.ToUpper()))
                If roleId IsNot Nothing Then
                    ExecuteNonQuery("INSERT INTO AspNetUserRoles(UserId,RoleId) VALUES(@uid,@rid)",
                        New SQLiteParameter("@uid", userId),
                        New SQLiteParameter("@rid", roleId.ToString()))
                End If
            End If
        Next

        ' Seed default inventory categories
        Dim categories As New List(Of SeedCategory) From {
            New SeedCategory("Furniture", "Chairs, tables, sofas, etc."),
            New SeedCategory("Linen & Bedding", "Mattresses, bedsheets, blankets, pillows"),
            New SeedCategory("Utensils & Crockery", "Plates, cups, cooking vessels"),
            New SeedCategory("Electronics & AV", "Sound systems, mikes, projectors, lights"),
            New SeedCategory("Tents & Canopies", "Tents, shamiana, mandap material"),
            New SeedCategory("Decoration", "Flower pots, banners, stage decoration items")
        }

        For Each cat In categories
            Dim catExists As Object = ExecuteScalar("SELECT COUNT(*) FROM InventoryCategories WHERE Name=@name", New SQLiteParameter("@name", cat.Name))
            If Convert.ToInt32(catExists) = 0 Then
                ExecuteNonQuery("INSERT INTO InventoryCategories(Name,Description) VALUES(@name,@desc)",
                    New SQLiteParameter("@name", cat.Name),
                    New SQLiteParameter("@desc", cat.Desc))
            End If
        Next

        ' Seed default inventory items
        Dim itemsCount As Object = ExecuteScalar("SELECT COUNT(*) FROM InventoryItems")
        If Convert.ToInt32(itemsCount) = 0 Then
            ' Get category IDs
            Dim catIds As New Dictionary(Of String, Integer)()
            Using conn As SQLiteConnection = GetConnection()
                Using cmd As New SQLiteCommand("SELECT Id, Name FROM InventoryCategories", conn)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            catIds(reader("Name").ToString()) = Convert.ToInt32(reader("Id"))
                        End While
                    End Using
                End Using
            End Using

            Dim items As New List(Of SeedItem) From {
                New SeedItem("Plastic Chair", "Furniture", 500, 5, "Nos", "White plastic chairs for events"),
                New SeedItem("Folding Table (6 ft)", "Furniture", 100, 50, "Nos", "6-foot folding tables"),
                New SeedItem("Stage Chair (Cushioned)", "Furniture", 50, 20, "Nos", "Cushioned chairs for stage/VIP"),
                New SeedItem("Single Mattress", "Linen & Bedding", 100, 30, "Nos", "Single mattresses for overnight stays"),
                New SeedItem("Bedsheet (Single)", "Linen & Bedding", 200, 10, "Nos", "Single bedsheets"),
                New SeedItem("Blanket", "Linen & Bedding", 100, 15, "Nos", "Woollen blankets"),
                New SeedItem("Steel Plate (Thali)", "Utensils & Crockery", 300, 3, "Nos", "Steel dinner plates"),
                New SeedItem("Steel Glass", "Utensils & Crockery", 300, 2, "Nos", "Steel drinking glasses"),
                New SeedItem("Big Cooking Vessel (Patila)", "Utensils & Crockery", 20, 100, "Nos", "Large cooking vessels"),
                New SeedItem("Sound System (PA Set)", "Electronics & AV", 5, 500, "Set", "Full PA sound system with speakers and mixer"),
                New SeedItem("Wireless Mike Set", "Electronics & AV", 10, 200, "Set", "2-piece wireless microphone sets"),
                New SeedItem("LED Light String (10m)", "Electronics & AV", 50, 50, "Nos", "Decorative LED light strings"),
                New SeedItem("Projector (Full HD)", "Electronics & AV", 3, 800, "Nos", "Full HD projector with screen"),
                New SeedItem("Shamiana (Small, 20x20ft)", "Tents & Canopies", 10, 1000, "Nos", "Small shamiana tent 20x20 feet"),
                New SeedItem("Shamiana (Large, 40x60ft)", "Tents & Canopies", 5, 3000, "Nos", "Large shamiana tent 40x60 feet"),
                New SeedItem("Stage Platform (4x4ft section)", "Decoration", 30, 150, "Section", "Modular stage platform sections"),
                New SeedItem("Flower Pot (Decorative)", "Decoration", 40, 25, "Nos", "Decorative flower pots for stage/entrance")
            }

            For Each itm In items
                If catIds.ContainsKey(itm.CatName) Then
                    ExecuteNonQuery("INSERT INTO InventoryItems(Name,Description,CategoryId,UnitType,TotalQuantity,ReservedQuantity,CurrentPrice,IsActive) VALUES(@name,@desc,@catId,@unit,@total,0,@price,1)",
                        New SQLiteParameter("@name", itm.Name),
                        New SQLiteParameter("@desc", itm.Desc),
                        New SQLiteParameter("@catId", catIds(itm.CatName)),
                        New SQLiteParameter("@unit", itm.Unit),
                        New SQLiteParameter("@total", itm.Qty),
                        New SQLiteParameter("@price", itm.Price))
                End If
            Next
        End If
    End Sub

    Private Shared Sub ExecuteSql(ByVal conn As SQLiteConnection, ByVal sql As String)
        Using cmd As New SQLiteCommand(sql, conn)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Private Class SeedEmployee
        Public EmpId As String
        Public Name As String
        Public Dept As String
        Public Desg As String
        Public Email As String
        Public Phone As String
        Public Address As String
        Public Sub New(e As String, n As String, dp As String, ds As String, em As String, ph As String, ad As String)
            EmpId = e : Name = n : Dept = dp : Desg = ds : Email = em : Phone = ph : Address = ad
        End Sub
    End Class

    Private Class SeedUser
        Public EmpId As String
        Public Username As String
        Public Name As String
        Public Email As String
        Public Dept As String
        Public Password As String
        Public Role As String
        Public Sub New(e As String, u As String, n As String, em As String, dp As String, pw As String, r As String)
            EmpId = e : Username = u : Name = n : Email = em : Dept = dp : Password = pw : Role = r
        End Sub
    End Class

    Private Class SeedCategory
        Public Name As String
        Public Desc As String
        Public Sub New(n As String, d As String)
            Name = n : Desc = d
        End Sub
    End Class

    Private Class SeedItem
        Public Name As String
        Public CatName As String
        Public Qty As Integer
        Public Price As Decimal
        Public Unit As String
        Public Desc As String
        Public Sub New(n As String, c As String, q As Integer, p As Decimal, u As String, d As String)
            Name = n : CatName = c : Qty = q : Price = p : Unit = u : Desc = d
        End Sub
    End Class
End Class
