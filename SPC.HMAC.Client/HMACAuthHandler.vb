Imports System.Net.Http
Imports System.Threading
Imports System.Security.Cryptography
Imports System.Text
Imports System.Net.Http.Headers


Friend Class HMACAuthenticationDelegatingHandler
    Inherits DelegatingHandler

    Private _publicKey As String
    Private _privateKey As String


    Public Sub New(publicKey As String, privateKey As String)
        MyBase.New

        _publicKey = publicKey
        _privateKey = privateKey
    End Sub

    Protected Overrides Async Function SendAsync(request As HttpRequestMessage, cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)

        Dim response As HttpResponseMessage = Nothing
        Dim requestContentBase64String As String = String.Empty

        Dim requestUri As String = System.Web.HttpUtility.UrlEncode(request.RequestUri.AbsoluteUri.ToLower())

        Dim requestHttpMethod As String = request.Method.Method

        'UNIX time needed
        Dim epochStart As New DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
        Dim timeSpan As TimeSpan = DateTime.UtcNow - epochStart
        Dim requestTimeStamp As String = Convert.ToUInt64(timeSpan.TotalSeconds).ToString()

        'random code for each request
        Dim uniqueValue As String = Guid.NewGuid().ToString("N")

        'body mat be null for a HTTP GET or DELETE
        If request.Content IsNot Nothing Then
            Dim content As Byte() = Await request.Content.ReadAsByteArrayAsync()
            Dim md5Obj As MD5 = MD5.Create()
            Dim requestContentHash As Byte() = md5Obj.ComputeHash(content)
            requestContentBase64String = Convert.ToBase64String(requestContentHash)
        End If

        Dim signatureRawData As String = [String].Format("{0}{1}{2}{3}{4}{5}", _publicKey, requestHttpMethod, requestUri, requestTimeStamp, uniqueValue, requestContentBase64String)

        Dim secretKeyByteArray = Convert.FromBase64String(_privateKey)

        Dim signature As Byte() = Encoding.UTF8.GetBytes(signatureRawData)

        Using hmac As New HMACSHA256(secretKeyByteArray)
            Dim signatureBytes As Byte() = hmac.ComputeHash(signature)
            Dim requestSignatureBase64String As String = Convert.ToBase64String(signatureBytes)
            request.Headers.Authorization = New AuthenticationHeaderValue("amx", String.Format("{0}:{1}:{2}:{3}", _publicKey, requestSignatureBase64String, uniqueValue, requestTimeStamp))
        End Using

        response = Await MyBase.SendAsync(request, cancellationToken).ConfigureAwait(False)

        Return response
    End Function



End Class


