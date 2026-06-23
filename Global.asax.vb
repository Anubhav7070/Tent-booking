Imports System
Imports System.Web
Imports System.Web.Routing

Public Class Global_asax
    Inherits System.Web.HttpApplication

    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' Initialize and seed the database on first run
        Try
            Database.InitializeDatabase()
        Catch ex As Exception
            ' Log to console for local hosting visibility
            Console.WriteLine("DB Init Error: " & ex.ToString())
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
    End Sub
End Class
