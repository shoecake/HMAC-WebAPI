Imports System.Security.Cryptography

Public Class Admin

    Public Shared Function GeneratePrivateKey()
        Using cryptoProvider = New RNGCryptoServiceProvider()
            Dim secretKeyByteArray As Byte() = New Byte(31) {}
            '256 bit
            cryptoProvider.GetBytes(secretKeyByteArray)
            Return Convert.ToBase64String(secretKeyByteArray)
        End Using
    End Function
End Class
