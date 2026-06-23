Option Strict On
Imports System
Imports System.IO
Imports System.Web.Hosting
Imports System.Threading

Public Class Server
    Private Shared _host As ASPNetHost
    Private Shared _virtualDir As String = "/"
    Private Shared _physicalDir As String
    Private Shared _port As Integer = 5051

    Public Shared Sub Main()
        Console.WriteLine("=================================================")
        Console.WriteLine("IOCL Community Hall - Local Web Forms Server")
        Console.WriteLine("=================================================")

        Dim baseDir As String = AppDomain.CurrentDomain.BaseDirectory
        If baseDir.EndsWith("\bin\", StringComparison.OrdinalIgnoreCase) Then
            _physicalDir = baseDir.Substring(0, baseDir.Length - 4)
        ElseIf baseDir.EndsWith("\bin", StringComparison.OrdinalIgnoreCase) Then
            _physicalDir = baseDir.Substring(0, baseDir.Length - 3)
        Else
            _physicalDir = baseDir
        End If
        Console.WriteLine("Physical Directory: " & _physicalDir)

        ' Create the ASP.NET Application Host
        Try
            Dim hostType As Type = GetType(ASPNetHost)
            _host = DirectCast(ApplicationHost.CreateApplicationHost(hostType, _virtualDir, _physicalDir), ASPNetHost)
            _host.Start(_port, _virtualDir, _physicalDir)
            
            ' Open default browser
            Try
                System.Diagnostics.Process.Start("http://localhost:" & _port & "/Login.aspx")
            Catch
            End Try
        Catch ex As Exception
            Console.WriteLine("Error creating ASP.NET host: " & ex.Message)
            If ex.InnerException IsNot Nothing Then
                Console.WriteLine("Inner error: " & ex.InnerException.Message)
            End If
            Return
        End Try

        ' Keep main thread alive
        Dim waitHandle As New AutoResetEvent(False)
        AddHandler Console.CancelKeyPress, Sub(sender, e)
                                               e.Cancel = True
                                               waitHandle.Set()
                                           End Sub
        waitHandle.WaitOne()

        Console.WriteLine("Shutting down server...")
        _host.StopServer()
    End Sub
End Class
