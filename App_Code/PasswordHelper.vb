Imports System
Imports System.Security.Cryptography
Imports System.Text

' ============================================================
'  PasswordHelper.vb — BCrypt-compatible password hashing
'  Uses SHA256 PBKDF2 to be compatible with ASP.NET Identity v3
' ============================================================
Public Class PasswordHelper
    ' Hash a plain-text password using ASP.NET Identity v3 format (PBKDF2/SHA256)
    Public Shared Function HashPassword(plainText As String) As String
        Dim saltSize As Integer = 16
        Dim iterations As Integer = 10000
        Dim keySize As Integer = 32

        Dim salt(saltSize - 1) As Byte
        Using rng As New RNGCryptoServiceProvider()
            rng.GetBytes(salt)
        End Using

        Dim subkey(keySize - 1) As Byte
        Using pbkdf2 As New Rfc2898DeriveBytes(plainText, salt, iterations, HashAlgorithmName.SHA256)
            subkey = pbkdf2.GetBytes(keySize)
        End Using

        ' Format: [1 byte version][4 bytes iterations big-endian][16 bytes salt][32 bytes key]
        Dim output(1 + 4 + saltSize + keySize - 1) As Byte
        output(0) = 1 ' version marker for PBKDF2-SHA256
        output(1) = CByte((iterations >> 24) And &HFF)
        output(2) = CByte((iterations >> 16) And &HFF)
        output(3) = CByte((iterations >> 8) And &HFF)
        output(4) = CByte(iterations And &HFF)
        Array.Copy(salt, 0, output, 5, saltSize)
        Array.Copy(subkey, 0, output, 5 + saltSize, keySize)
        Return Convert.ToBase64String(output)
    End Function

    ' Verify a plain-text password against a stored hash
    Public Shared Function VerifyPassword(plainText As String, storedHash As String) As Boolean
        Try
            Dim decoded() As Byte = Convert.FromBase64String(storedHash)
            If decoded.Length < 53 OrElse decoded(0) <> 1 Then Return False

            Dim iterations As Integer = (CInt(decoded(1)) << 24) Or (CInt(decoded(2)) << 16) Or (CInt(decoded(3)) << 8) Or CInt(decoded(4))
            Dim saltSize As Integer = 16
            Dim keySize As Integer = 32

            Dim salt(saltSize - 1) As Byte
            Array.Copy(decoded, 5, salt, 0, saltSize)

            Dim storedKey(keySize - 1) As Byte
            Array.Copy(decoded, 5 + saltSize, storedKey, 0, keySize)

            Dim testKey(keySize - 1) As Byte
            Using pbkdf2 As New Rfc2898DeriveBytes(plainText, salt, iterations, HashAlgorithmName.SHA256)
                testKey = pbkdf2.GetBytes(keySize)
            End Using

            Return SlowEquals(storedKey, testKey)
        Catch
            Return False
        End Try
    End Function

    Private Shared Function SlowEquals(a() As Byte, b() As Byte) As Boolean
        Dim diff As UInteger = CUInt(a.Length) Xor CUInt(b.Length)
        Dim len As Integer = Math.Min(a.Length, b.Length)
        For i As Integer = 0 To len - 1
            diff = diff Or (CUInt(a(i)) Xor CUInt(b(i)))
        Next
        Return diff = 0
    End Function
End Class
