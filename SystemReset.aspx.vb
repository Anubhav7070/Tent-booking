Imports System
Imports System.IO
Imports System.Data
Imports System.Data.SQLite
Imports System.Collections.Generic
Imports System.Web.UI

Public Class SystemReset
    Inherits Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        AuthHelper.RequireRole(Me, "SuperAdmin")
    End Sub

    Protected Sub btnReset_Click(ByVal sender As Object, ByVal e As EventArgs)
        Dim appPath As String = AppDomain.CurrentDomain.BaseDirectory
        Dim dbPath As String = Path.Combine(appPath, "App_Data", "iocl_community_hall.db")
        Dim backupDir As String = Path.Combine(appPath, "backup_local")

        If Not Directory.Exists(backupDir) Then
            Directory.CreateDirectory(backupDir)
        End If

        Dim backupFileName As String = "iocl_community_hall_backup_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".db"
        Dim backupFilePath As String = Path.Combine(backupDir, backupFileName)

        Try
            If File.Exists(dbPath) Then
                File.Copy(dbPath, backupFilePath, True)
            End If
        Catch ex As Exception
            pnlError.Visible = True
            lblError.Text = "Failed to create database backup: " & ex.Message
            Return
        End Try

        ' Read stats before deleting
        Dim requestsDeleted As Integer = Convert.ToInt32(Database.ExecuteScalar("SELECT COUNT(*) FROM RentalRequests"))
        Dim allocationsDeleted As Integer = Convert.ToInt32(Database.ExecuteScalar("SELECT COUNT(*) FROM InventoryAllocations"))
        Dim transactionsDeleted As Integer = Convert.ToInt32(Database.ExecuteScalar("SELECT COUNT(*) FROM InventoryTransactions"))
        Dim auditLogsDeleted As Integer = Convert.ToInt32(Database.ExecuteScalar("SELECT COUNT(*) FROM AuditLogs WHERE EntityName = 'RentalRequest'"))
        Dim notificationsDeleted As Integer = Convert.ToInt32(Database.ExecuteScalar("SELECT COUNT(*) FROM Notifications"))

        ' Perform reset atomically in a transaction
        Using conn As SQLiteConnection = Database.GetConnection()
            Using trans = conn.BeginTransaction()
                Try
                    ' Turn off foreign keys temporarily for truncation
                    Using cmd As New SQLiteCommand("PRAGMA foreign_keys = OFF;", conn, trans)
                        cmd.ExecuteNonQuery()
                    End Using

                    ' Clear tables
                    ExecuteSqlInTransaction(conn, trans, "DELETE FROM RentalRequestItems;")
                    ExecuteSqlInTransaction(conn, trans, "DELETE FROM InventoryAllocations;")
                    ExecuteSqlInTransaction(conn, trans, "DELETE FROM InventoryTransactions;")
                    ExecuteSqlInTransaction(conn, trans, "DELETE FROM AuditLogs WHERE EntityName = 'RentalRequest';")
                    ExecuteSqlInTransaction(conn, trans, "DELETE FROM Notifications;")
                    ExecuteSqlInTransaction(conn, trans, "DELETE FROM RentalRequests;")

                    ' Reset sequence tables
                    ExecuteSqlInTransaction(conn, trans, "DELETE FROM sqlite_sequence WHERE name IN ('RentalRequests', 'RentalRequestItems', 'InventoryAllocations', 'InventoryTransactions', 'Notifications', 'AuditLogs');")

                    ' Reset all inventory quantities to 100
                    Dim nowStr As String = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                    Using cmd As New SQLiteCommand("UPDATE InventoryItems SET TotalQuantity = 100, ReservedQuantity = 0, UpdatedAt = @now;", conn, trans)
                        cmd.Parameters.AddWithValue("@now", nowStr)
                        cmd.ExecuteNonQuery()
                    End Using

                    Using cmd As New SQLiteCommand("PRAGMA foreign_keys = ON;", conn, trans)
                        cmd.ExecuteNonQuery()
                    End Using
                    Using cmd As New SQLiteCommand("PRAGMA foreign_key_check;", conn, trans)
                        cmd.ExecuteNonQuery()
                    End Using

                    trans.Commit()
                Catch ex As Exception
                    trans.Rollback()
                    pnlError.Visible = True
                    lblError.Text = "Database reset failed during transaction: " & ex.Message
                    Return
                End Try
            End Using
        End Using

        ' Write audit log
        Try
            Dim currentUserId As String = AuthHelper.GetCurrentUserId(Context)
            AuditService.Log(currentUserId, "SystemReset", "System", "0", "Database reset performed. Backup: " & backupFileName, "Active State", "Reset State", Request.UserHostAddress)
        Catch ex As Exception
            Console.WriteLine("Failed to write system reset audit log: " & ex.Message)
        End Try

        ' Fetch final inventory items list
        Dim dt As DataTable = Database.ExecuteDataTable("SELECT Name, TotalQuantity, ReservedQuantity FROM InventoryItems")
        Dim finalInventoryList As New List(Of Tuple(Of String, Integer, Integer))()
        For Each row As DataRow In dt.Rows
            finalInventoryList.Add(Tuple.Create(
                row("Name").ToString(),
                Convert.ToInt32(row("TotalQuantity")),
                Convert.ToInt32(row("ReservedQuantity"))
            ))
        Next

        ' Populate report parameters
        lblRequestsDeleted.Text = requestsDeleted.ToString()
        lblAllocationsDeleted.Text = allocationsDeleted.ToString()
        lblNotificationsDeleted.Text = notificationsDeleted.ToString()
        lblAuditLogsDeleted.Text = auditLogsDeleted.ToString()
        lblBackupFileName.Text = backupFileName

        rptFinalInventory.DataSource = finalInventoryList
        rptFinalInventory.DataBind()

        pnlWarning.Visible = False
        pnlReport.Visible = True
        pnlSuccess.Visible = True
        lblSuccess.Text = "System reset completed successfully! Database is back to clean slate."
    End Sub

    Private Sub ExecuteSqlInTransaction(ByVal conn As SQLiteConnection, ByVal trans As SQLiteTransaction, ByVal sql As String)
        Using cmd As New SQLiteCommand(sql, conn, trans)
            cmd.ExecuteNonQuery()
        End Using
    End Sub
End Class
