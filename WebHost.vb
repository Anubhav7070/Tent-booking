Imports System
Imports System.IO
Imports System.Net
Imports System.Web
Imports System.Web.Hosting
Imports System.Threading
Imports System.Collections.Generic

Public Class HttpListenerWorkerRequest
    Inherits SimpleWorkerRequest

    Private _context As HttpListenerContext
    Private _virtualDir As String
    Private _physicalDir As String
    Private _preloadedBody() As Byte
    Private _preloadedRead As Boolean
    Private _writer As System.IO.StringWriter

    Public Sub New(ByVal context As HttpListenerContext, ByVal virtualDir As String, ByVal physicalDir As String)
        Me.New(context, virtualDir, physicalDir, New System.IO.StringWriter())
    End Sub

    Private Sub New(ByVal context As HttpListenerContext, ByVal virtualDir As String, ByVal physicalDir As String, ByVal writer As System.IO.StringWriter)
        MyBase.New(context.Request.Url.LocalPath, context.Request.Url.Query, writer)
        _context = context
        _virtualDir = virtualDir
        _physicalDir = physicalDir
        _preloadedRead = False
        _writer = writer
    End Sub

    Private Sub PreloadBody()
        If _preloadedRead Then Return
        _preloadedRead = True
        If Not _context.Request.HasEntityBody Then Return
        Using ms As New MemoryStream()
            _context.Request.InputStream.CopyTo(ms)
            _preloadedBody = ms.ToArray()
        End Using
    End Sub

    Public Overrides Function IsEntireEntityBodyIsPreloaded() As Boolean
        Return True
    End Function

    Public Overrides Function GetPreloadedEntityBody() As Byte()
        PreloadBody()
        Return _preloadedBody
    End Function

    Public Overrides Sub EndOfRequest()
        Try
            Dim text As String = _writer.ToString()
            ASPNetHost.DebugLog("HttpListenerWorkerRequest.EndOfRequest entered. Captured Output Length: " & text.Length)
            If Not String.IsNullOrEmpty(text) Then
                ASPNetHost.DebugLog("Captured Output text: " & text)
                Try
                    Dim bytes() As Byte = System.Text.Encoding.UTF8.GetBytes(text)
                    _context.Response.OutputStream.Write(bytes, 0, bytes.Length)
                Catch ex As Exception
                    ASPNetHost.DebugLog("Error writing captured output to response: " & ex.Message)
                End Try
            End If
            _context.Response.OutputStream.Flush()
            _context.Response.Close()
        Catch ex As Exception
            ASPNetHost.DebugLog("Error in EndOfRequest: " & ex.Message)
        End Try
    End Sub

    Public Overrides Function GetAppPath() As String
        Return _virtualDir
    End Function

    Public Overrides Function GetAppPathTranslated() As String
        Return _physicalDir
    End Function

    Public Overrides Function GetFilePath() As String
        Dim path As String = _context.Request.Url.LocalPath
        If path.StartsWith(_virtualDir, StringComparison.OrdinalIgnoreCase) Then
            path = path.Substring(_virtualDir.Length)
        End If
        If Not path.StartsWith("/") Then
            path = "/" & path
        End If

        Dim dotIndex As Integer = -1
        For Each ext As String In New String() {".aspx/", ".ashx/", ".asmx/", ".axd/"}
            Dim idx As Integer = path.IndexOf(ext, StringComparison.OrdinalIgnoreCase)
            If idx >= 0 Then
                dotIndex = idx + ext.Length - 1
                Exit For
            End If
        Next

        If dotIndex >= 0 Then
            path = path.Substring(0, dotIndex)
        End If

        Return path.Replace("/"c, "\"c)
    End Function

    Public Overrides Function GetPathInfo() As String
        Dim path As String = _context.Request.Url.LocalPath
        Dim dotIndex As Integer = -1
        For Each ext As String In New String() {".aspx/", ".ashx/", ".asmx/", ".axd/"}
            Dim idx As Integer = path.IndexOf(ext, StringComparison.OrdinalIgnoreCase)
            If idx >= 0 Then
                dotIndex = idx + ext.Length - 1
                Exit For
            End If
        Next

        If dotIndex >= 0 Then
            Return path.Substring(dotIndex)
        Else
            Return String.Empty
        End If
    End Function

    Public Overrides Function GetFilePathTranslated() As String
        Dim path As String = GetFilePath()
        If path.StartsWith("\") Then path = path.Substring(1)
        Return System.IO.Path.Combine(_physicalDir, path)
    End Function

    Public Overrides Function GetHttpVerbName() As String
        Return _context.Request.HttpMethod
    End Function

    Public Overrides Function GetHttpVersion() As String
        Return "HTTP/" & _context.Request.ProtocolVersion.ToString()
    End Function

    Public Overrides Function GetLocalAddress() As String
        Return _context.Request.LocalEndPoint.Address.ToString()
    End Function

    Public Overrides Function GetLocalPort() As Integer
        Return _context.Request.LocalEndPoint.Port
    End Function

    Public Overrides Function GetQueryString() As String
        Dim raw As String = _context.Request.RawUrl
        Dim index As Integer = raw.IndexOf("?")
        If index >= 0 Then
            Return raw.Substring(index + 1)
        Else
            Return String.Empty
        End If
    End Function

    Public Overrides Function GetRawUrl() As String
        Return _context.Request.RawUrl
    End Function

    Public Overrides Function GetRemoteAddress() As String
        Return _context.Request.RemoteEndPoint.Address.ToString()
    End Function

    Public Overrides Function GetRemotePort() As Integer
        Return _context.Request.RemoteEndPoint.Port
    End Function

    Public Overrides Function GetUriPath() As String
        Return _context.Request.Url.LocalPath
    End Function

    Public Overrides Sub SendKnownResponseHeader(ByVal index As Integer, ByVal value As String)
        Dim headerName As String = GetKnownResponseHeaderName(index)
        Try
            _context.Response.Headers(headerName) = value
        Catch
            ' Ignore headers set after transmission
        End Try
    End Sub

    Public Overrides Sub SendUnknownResponseHeader(ByVal name As String, ByVal value As String)
        Try
            _context.Response.Headers(name) = value
        Catch
            ' Ignore headers set after transmission
        End Try
    End Sub

    Public Overrides Sub SendResponseFromFile(ByVal handle As IntPtr, ByVal offset As Long, ByVal length As Long)
        ASPNetHost.DebugLog("SendResponseFromFile(IntPtr handle) called. Offset=" & offset & ", Length=" & length)
    End Sub

    Public Overrides Sub SendResponseFromFile(ByVal filename As String, ByVal offset As Long, ByVal length As Long)
        Try
            ASPNetHost.DebugLog("SendResponseFromFile(String filename) called: " & filename & ", Offset=" & offset & ", Length=" & length)
            Using fs As New FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)
                fs.Seek(offset, SeekOrigin.Begin)
                Dim buffer(8192) As Byte
                Dim bytesRead As Integer
                Dim remaining As Long = length
                While remaining > 0
                    bytesRead = fs.Read(buffer, 0, CInt(Math.Min(buffer.Length, remaining)))
                    If bytesRead = 0 Then Exit While
                    _context.Response.OutputStream.Write(buffer, 0, bytesRead)
                    remaining -= bytesRead
                End While
            End Using
        Catch ex As Exception
            ASPNetHost.DebugLog("Error in SendResponseFromFile(String): " & ex.Message)
        End Try
    End Sub

    Public Overrides Sub SendResponseFromMemory(ByVal data() As Byte, ByVal length As Integer)
        Try
            ASPNetHost.DebugLog("SendResponseFromMemory(Byte()) called. Length=" & length)
            _context.Response.OutputStream.Write(data, 0, length)
        Catch ex As Exception
            ASPNetHost.DebugLog("Error in SendResponseFromMemory(Byte()): " & ex.Message)
        End Try
    End Sub

    Public Overrides Sub SendResponseFromMemory(ByVal data As IntPtr, ByVal length As Integer)
        Try
            ASPNetHost.DebugLog("SendResponseFromMemory(IntPtr) called. Length=" & length)
            If data <> IntPtr.Zero AndAlso length > 0 Then
                Dim buffer(length - 1) As Byte
                System.Runtime.InteropServices.Marshal.Copy(data, buffer, 0, length)
                _context.Response.OutputStream.Write(buffer, 0, length)
            End If
        Catch ex As Exception
            ASPNetHost.DebugLog("Error in SendResponseFromMemory(IntPtr): " & ex.Message)
        End Try
    End Sub


    Public Overrides Sub SendStatus(ByVal statusCode As Integer, ByVal statusDescription As String)
        Try
            ASPNetHost.DebugLog("SendStatus called. StatusCode=" & statusCode & ", StatusDescription=" & statusDescription)
            _context.Response.StatusCode = statusCode
            _context.Response.StatusDescription = statusDescription
        Catch ex As Exception
            ASPNetHost.DebugLog("Error in SendStatus: " & ex.Message)
        End Try
    End Sub

    Public Overrides Sub FlushResponse(ByVal finalFlush As Boolean)
        Try
            ASPNetHost.DebugLog("FlushResponse called. FinalFlush=" & finalFlush)
            _context.Response.OutputStream.Flush()
        Catch ex As Exception
            ASPNetHost.DebugLog("Error in FlushResponse: " & ex.Message)
        End Try
    End Sub


    Public Overrides Function GetKnownRequestHeader(ByVal index As Integer) As String
        Dim name As String = GetKnownRequestHeaderName(index)
        Return _context.Request.Headers(name)
    End Function

    Public Overrides Function GetUnknownRequestHeader(ByVal name As String) As String
        Return _context.Request.Headers(name)
    End Function

    Public Overrides Function GetUnknownRequestHeaders() As String()()
        Dim headers As New List(Of String())()
        For Each key As String In _context.Request.Headers.AllKeys
            Dim index As Integer = GetKnownRequestHeaderIndex(key)
            If index < 0 Then
                Dim val As String = _context.Request.Headers(key)
                headers.Add(New String() {key, val})
            End If
        Next
        Return headers.ToArray()
    End Function
End Class

Public Class ASPNetHost
    Inherits MarshalByRefObject

    Public Shared Sub DebugLog(ByVal msg As String)
        Try
            Dim baseDir As String = AppDomain.CurrentDomain.BaseDirectory
            Dim logDir As String = baseDir
            If logDir.EndsWith("\bin\", StringComparison.OrdinalIgnoreCase) Then
                logDir = logDir.Substring(0, logDir.Length - 4)
            ElseIf logDir.EndsWith("\bin", StringComparison.OrdinalIgnoreCase) Then
                logDir = logDir.Substring(0, logDir.Length - 3)
            End If
            Dim logPath As String = System.IO.Path.Combine(logDir, "server_debug.log")
            System.IO.File.AppendAllText(logPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & " - " & msg & Environment.NewLine)
        Catch
        End Try
    End Sub

    Private _listener As HttpListener
    Private _virtualDir As String
    Private _physicalDir As String
    Private _port As Integer

    Public Sub Start(ByVal port As Integer, ByVal virtualDir As String, ByVal physicalDir As String)
        _port = port
        _virtualDir = virtualDir
        _physicalDir = physicalDir

        DebugLog("ASPNetHost.Start called. Port=" & port & ", VirtualDir=" & virtualDir & ", PhysicalDir=" & physicalDir)

        Try
            _listener = New HttpListener()
            _listener.Prefixes.Add("http://localhost:" & port & "/")
            _listener.Prefixes.Add("http://127.0.0.1:" & port & "/")
            _listener.Start()

            Console.WriteLine("Server started successfully on http://localhost:" & port & "/")
            Console.WriteLine("Press Ctrl+C to terminate.")
            DebugLog("HttpListener started successfully.")

            ThreadPool.QueueUserWorkItem(AddressOf Listen)
        Catch ex As Exception
            DebugLog("Error starting HttpListener: " & ex.ToString())
            Throw
        End Try
    End Sub

    Public Sub StopServer()
        DebugLog("ASPNetHost.StopServer called.")
        If _listener IsNot Nothing Then
            Try
                _listener.Stop()
                DebugLog("HttpListener stopped.")
            Catch ex As Exception
                DebugLog("Error stopping HttpListener: " & ex.Message)
            End Try
        End If
    End Sub

    Private Sub Listen(ByVal state As Object)
        DebugLog("ASPNetHost.Listen loop started.")
        While _listener.IsListening
            Try
                Dim context As HttpListenerContext = _listener.GetContext()
                DebugLog("HttpListener accepted request: " & context.Request.Url.ToString())
                ThreadPool.QueueUserWorkItem(AddressOf HandleRequestCallback, context)
            Catch ex As Exception
                DebugLog("Listen loop exception: " & ex.ToString())
            End Try
        End While
        DebugLog("ASPNetHost.Listen loop exited.")
    End Sub

    Private Sub HandleRequestCallback(ByVal state As Object)
        Try
            Dim context As HttpListenerContext = DirectCast(state, HttpListenerContext)
            DebugLog("HandleRequestCallback entered for request: " & context.Request.Url.LocalPath)
            HandleRequest(context)
        Catch ex As Exception
            DebugLog("HandleRequestCallback exception: " & ex.ToString())
        End Try
    End Sub

    Private Sub HandleRequest(ByVal context As HttpListenerContext)
        Try
            Dim urlPath As String = context.Request.Url.LocalPath
            DebugLog("HandleRequest entered. Path=" & urlPath)
            
            ' Serve static files directly to avoid ASP.NET mime mapping issues
            If IsStaticFile(urlPath) Then
                DebugLog("Serving static file: " & urlPath)
                ServeStaticFile(context, urlPath)
                Return
            End If

            ' Otherwise, delegate to the ASP.NET runtime in the same AppDomain
            DebugLog("Delegating to HttpRuntime.ProcessRequest. Path=" & urlPath)
            Dim wr As New HttpListenerWorkerRequest(context, _virtualDir, _physicalDir)
            HttpRuntime.ProcessRequest(wr)
            DebugLog("HttpRuntime.ProcessRequest returned for Path=" & urlPath)
        Catch ex As Exception
            DebugLog("HandleRequest exception: " & ex.ToString())
            Try
                context.Response.StatusCode = 500
                context.Response.ContentType = "text/html"
                Using writer As New StreamWriter(context.Response.OutputStream)
                    writer.WriteLine("<h1>Internal Server Error</h1>")
                    writer.WriteLine("<pre>" & ex.ToString() & "</pre>")
                End Using
                context.Response.Close()
            Catch ex2 As Exception
                DebugLog("Error sending 500 response: " & ex2.Message)
            End Try
        End Try
    End Sub

    Private Function IsStaticFile(ByVal path As String) As Boolean
        Dim ext As String = System.IO.Path.GetExtension(path).ToLower()
        If String.IsNullOrEmpty(ext) Then Return False
        Return ext <> ".aspx" AndAlso ext <> ".ashx" AndAlso ext <> ".asmx" AndAlso ext <> ".axd"
    End Function

    Private Sub ServeStaticFile(ByVal context As HttpListenerContext, ByVal urlPath As String)
        Dim localPath As String = urlPath.Replace("/"c, "\"c)
        If localPath.StartsWith("\") Then localPath = localPath.Substring(1)
        Dim fullPath As String = Path.Combine(_physicalDir, localPath)

        If Not File.Exists(fullPath) Then
            context.Response.StatusCode = 404
            context.Response.Close()
            Return
        End If

        Try
            Dim ext As String = Path.GetExtension(fullPath).ToLower()
            Dim mime As String = "application/octet-stream"
            Select Case ext
                Case ".css"
                    mime = "text/css"
                Case ".js"
                    mime = "application/javascript"
                Case ".png"
                    mime = "image/png"
                Case ".jpg", ".jpeg"
                    mime = "image/jpeg"
                Case ".gif"
                    mime = "image/gif"
                Case ".pdf"
                    mime = "application/pdf"
                Case ".txt"
                    mime = "text/plain"
                Case ".html", ".htm"
                    mime = "text/html"
            End Select

            context.Response.ContentType = mime
            Dim fileBytes() As Byte = File.ReadAllBytes(fullPath)
            context.Response.ContentLength64 = fileBytes.Length
            context.Response.OutputStream.Write(fileBytes, 0, fileBytes.Length)
            context.Response.Close()
        Catch
            context.Response.StatusCode = 500
            context.Response.Close()
        End Try
    End Sub
End Class
