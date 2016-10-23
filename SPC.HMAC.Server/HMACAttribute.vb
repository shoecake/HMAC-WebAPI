Imports System.Net.Http
Imports System.Web.Http.Filters
Imports System.Web.Http.Controllers
Imports System.Security.Cryptography
Imports System.Threading
Imports System.Text
Imports System.Web
Imports System.Security.Principal
Imports System.Web.Http.Results
Imports System.Net.Http.Headers
Imports System.Web.Http
Imports System.Net



Public Class HMACAuthenticationAttribute
    Inherits Attribute
    Implements IAuthenticationFilter

    Private Shared allowedApps As New Dictionary(Of String, String)() ' load only once
    Private Const requestMaxAgeInSeconds As Int64 = 300       ' 5 mins
    Private Const authenticationScheme As String = "amx"

    Public Property allowedPublicKeys As String() = Nothing

    Public Sub New()
        If allowedApps.Count = 0 Then ' create once only
            Dim section = System.Configuration.ConfigurationManager.GetSection("SPC.HMAC.Keys")
            If section IsNot Nothing Then
                Dim Keys = TryCast(section, Configuration.HMACSettings).Keys
                For Each key As Configuration.HMACKeyElement In Keys
                    allowedApps.Add(key.publicKey, key.privateKey)
                Next
            End If
        End If
    End Sub

    Public Function AuthenticateAsync(context As HttpAuthenticationContext, cancellationToken As CancellationToken) As Task Implements IAuthenticationFilter.AuthenticateAsync
        Dim req = context.Request

        If req.Headers.Authorization IsNot Nothing AndAlso authenticationScheme.Equals(req.Headers.Authorization.Scheme, StringComparison.OrdinalIgnoreCase) Then
            Dim rawAuthzHeader = req.Headers.Authorization.Parameter

            Dim autherizationHeaderArray = GetAutherizationHeaderValues(rawAuthzHeader)

            If autherizationHeaderArray IsNot Nothing Then
                Dim APPId = autherizationHeaderArray(0)
                Dim incomingBase64Signature = autherizationHeaderArray(1)
                Dim nonce = autherizationHeaderArray(2)
                Dim requestTimeStamp = autherizationHeaderArray(3)

                Dim isValid = isValidRequest(req, APPId, incomingBase64Signature, nonce, requestTimeStamp)

                If isValid.Result Is Nothing Then
                    Dim currentPrincipal = New GenericPrincipal(New GenericIdentity(APPId), Nothing)
                    context.Principal = currentPrincipal
                Else
                    context.ErrorResult = isValid.Result
                End If
            Else
                context.ErrorResult = New UnauthorizedResult(New AuthenticationHeaderValue(-1) {}, context.Request)
            End If
        Else
            context.ErrorResult = New UnauthorizedResult(New AuthenticationHeaderValue(-1) {}, context.Request)
        End If

        Return Task.FromResult(0)
    End Function

    Public Function ChallengeAsync(context As HttpAuthenticationChallengeContext, cancellationToken As CancellationToken) As Task Implements IAuthenticationFilter.ChallengeAsync
        context.Result = New ResultWithChallenge(context.Result)
        Return Task.FromResult(0)
    End Function

    Public ReadOnly Property AllowMultiple() As Boolean Implements IFilter.AllowMultiple
        Get
            Return False
        End Get
    End Property

    Private Function GetAutherizationHeaderValues(rawAuthzHeader As String) As String()

        Dim credArray = rawAuthzHeader.Split(":"c)

        If credArray.Length = 4 Then
            Return credArray
        Else
            Return Nothing
        End If

    End Function

    Private Async Function isValidRequest(req As HttpRequestMessage, APPId As String, incomingBase64Signature As String, nonce As String, requestTimeStamp As String) As Task(Of IHttpActionResult)
        Dim requestContentBase64String As String = ""
        Dim requestUri As String = HttpUtility.UrlEncode(req.RequestUri.AbsoluteUri.ToLower())
        Dim requestHttpMethod As String = req.Method.Method

        ' check if this key is allowed for this specific class/method
        ' if allowedPublicKeys is nothing then all keys allowed
        If allowedPublicKeys IsNot Nothing AndAlso Not allowedPublicKeys.Contains(APPId) Then
            Return New AuthenticationFailureResult("This public key is not allowed.", req)
        End If

        If Not allowedApps.ContainsKey(APPId) Then
            Return New AuthenticationFailureResult("This public key does not exist.", req)
        End If

        Dim sharedKey = allowedApps(APPId)

        If isReplayRequest(nonce, requestTimeStamp) Then
            Return New AuthenticationFailureResult("Failed HMAC authentication.", req)
        End If

        Dim hash As Byte() = Await ComputeHash(req.Content)

        If hash IsNot Nothing Then
            requestContentBase64String = Convert.ToBase64String(hash)
        End If

        Dim data As String = [String].Format("{0}{1}{2}{3}{4}{5}", APPId, requestHttpMethod, requestUri, requestTimeStamp, nonce, requestContentBase64String)

        Dim secretKeyBytes = Convert.FromBase64String(sharedKey)

        Dim signature As Byte() = Encoding.UTF8.GetBytes(data)

        Using hmac As New HMACSHA256(secretKeyBytes)
            Dim signatureBytes As Byte() = hmac.ComputeHash(signature)
            If (incomingBase64Signature.Equals(Convert.ToBase64String(signatureBytes), StringComparison.Ordinal)) Then
                Return Nothing ' successfully authenticated
            Else
                Return New AuthenticationFailureResult("Failed HMAC authentication.", req)
            End If
        End Using

    End Function

    Private Function isReplayRequest(nonce As String, requestTimeStamp As String) As Boolean



        If HttpRuntime.Cache.Get("SPC_API_hmac_nonce" & nonce) IsNot Nothing Then
            Return True
        End If

        Dim epochStart As New DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
        Dim currentTs As TimeSpan = DateTime.UtcNow - epochStart

        Dim serverTotalSeconds = Convert.ToInt64(currentTs.TotalSeconds)
        Dim requestTotalSeconds = Convert.ToInt64(requestTimeStamp)

        If (serverTotalSeconds - requestTotalSeconds) > requestMaxAgeInSeconds Then
            Return True
        End If

        HttpRuntime.Cache.Add("SPC_API_hmac_nonce" & nonce, requestTimeStamp, Nothing, Now.AddSeconds(requestMaxAgeInSeconds), Caching.Cache.NoSlidingExpiration, Caching.CacheItemPriority.Default, Nothing)
        Return False
    End Function

    Private Shared Async Function ComputeHash(httpContent As HttpContent) As Task(Of Byte())
        Using md5__1 As MD5 = MD5.Create()
            Dim hash As Byte() = Nothing
            Dim content = Await httpContent.ReadAsByteArrayAsync()
            If content.Length <> 0 Then
                hash = md5__1.ComputeHash(content)
            End If
            Return hash
        End Using
    End Function

End Class


Public Class ResultWithChallenge
    Implements IHttpActionResult
    Private ReadOnly authenticationScheme As String = "amx"
    Private ReadOnly [next] As IHttpActionResult

    Public Sub New([next] As IHttpActionResult)
        Me.[next] = [next]
    End Sub

    Public Async Function ExecuteAsync(cancellationToken As CancellationToken) As Task(Of HttpResponseMessage) Implements IHttpActionResult.ExecuteAsync
        Dim response = Await [next].ExecuteAsync(cancellationToken)

        If response.StatusCode = HttpStatusCode.Unauthorized Then
            response.Headers.WwwAuthenticate.Add(New AuthenticationHeaderValue(authenticationScheme))
        End If

        Return response
    End Function
End Class


Public Class AuthenticationFailureResult
    Implements IHttpActionResult

    Public Property ReasonPhrase() As String
    Public Property Request() As HttpRequestMessage

    Public Sub New(reasonPhrase As String, request As HttpRequestMessage)
        Me.ReasonPhrase = reasonPhrase
        Me.Request = request
    End Sub

    Public Function ExecuteAsync(cancellationToken As CancellationToken) As Task(Of HttpResponseMessage) Implements IHttpActionResult.ExecuteAsync
        Return Task.FromResult(Execute())
    End Function

    Private Function Execute() As HttpResponseMessage
        Dim response As New HttpResponseMessage(HttpStatusCode.Unauthorized)
        response.RequestMessage = Request
        response.ReasonPhrase = ReasonPhrase
        Return response
    End Function

End Class
