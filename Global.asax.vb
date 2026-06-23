Imports System
Imports System.Web
Imports System.Web.Routing

Public Class Global_asax
    Inherits System.Web.HttpApplication

    Private Shared _releaseTimer As System.Threading.Timer

    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' Initialize and seed the database on first run
        Try
            Database.InitializeDatabase()
        Catch ex As Exception
            ' Log to console for local hosting visibility
            Console.WriteLine("DB Init Error: " & ex.ToString())
        End Try

        ' Start the background stock-release task: runs immediately, then every hour
        _releaseTimer = New System.Threading.Timer(AddressOf AutoReleaseExpiredAllocations, Nothing, TimeSpan.Zero, TimeSpan.FromHours(1))
    End Sub

    Private Shared Sub AutoReleaseExpiredAllocations(state As Object)
        Try
            Dim count As Integer = RentalService.ReleaseExpiredAllocations()
            If count > 0 Then
                Console.WriteLine("[Background Service] Auto-released " & count & " expired allocation(s).")
            End If
        Catch ex As Exception
            Console.WriteLine("[Background Service] Error: " & ex.ToString())
        End Try
    End Sub

    Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
    End Sub

    Sub Application_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)
    End Sub

    Sub Application_AuthenticateRequest(ByVal sender As Object, ByVal e As EventArgs)
    End Sub

    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
        Dim ex As Exception = Server.GetLastError()
        If ex IsNot Nothing Then
            System.Diagnostics.Debug.WriteLine("Application Error: " & ex.Message)
        End If
    End Sub

    Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)
    End Sub

    Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)
        If _releaseTimer IsNot Nothing Then
            _releaseTimer.Dispose()
        End If
    End Sub
End Class
