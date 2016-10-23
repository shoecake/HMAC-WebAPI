




Imports System.Configuration
Imports System.Net.Http
Imports System.Web
Imports System.Web.Http.Controllers
Imports System.Web.Http.Filters

Public Class RequireHttpsAttribute
    Inherits AuthorizationFilterAttribute
    Public Overrides Sub OnAuthorization(actionContext As HttpActionContext)
        MyBase.OnAuthorization(actionContext)

        ' allow dev and test environments 
        If Not HttpContext.Current.IsDebuggingEnabled Then
            If Not [String].Equals(actionContext.Request.RequestUri.Scheme, "https", StringComparison.OrdinalIgnoreCase) Then
                actionContext.Response = New HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) With {.Content = New StringContent("HTTPS Required"), .ReasonPhrase = "HTTPS Required"}
                Return
            End If
        End If
    End Sub
End Class


Public Class ClientIpValidationAttribute
    Inherits AuthorizationFilterAttribute

    '    Private _appSettingsKey As String = "SPC_WebAPI_Valid_IPs"

    Public Sub New()
    End Sub

    Public Property appSettingsKey As String = "SPC_WebAPI_Valid_IPs"



    Public Overrides Sub OnAuthorization(actionContext As HttpActionContext)
        MyBase.OnAuthorization(actionContext)
        Dim context As System.Web.HttpContextBase = TryCast(actionContext.Request.Properties("MS_HttpContext"), System.Web.HttpContextBase)
        If context Is Nothing Then
            actionContext.Response = New HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) With {.Content = New StringContent("HTTP Context is missing (IP Validation)"), .ReasonPhrase = "HTTP Context is missing (IP Validation)"}
            Return
        End If

        If ConfigurationManager.AppSettings(appSettingsKey) Is Nothing Then
            actionContext.Response = New HttpResponseMessage(System.Net.HttpStatusCode.Forbidden) With {.Content = New StringContent("App Settings Entry is missing (IP Validation)"), .ReasonPhrase = "App Settings Entry is missing (IP Validation)"}
            Return
        End If

        Dim userIP As String = context.Request.UserHostAddress
        Dim result As String = ConfigurationManager.AppSettings(appSettingsKey).Split(",").AsQueryable.FirstOrDefault(Function(x) x = userIP)
        If result Is Nothing Then
            actionContext.Response = New HttpResponseMessage(System.Net.HttpStatusCode.Forbidden) With {.Content = New StringContent("Unauthorized IP Address (IP Validation)"), .ReasonPhrase = "Unauthorized IP Address (IP Validation)"}
            Return
        End If
    End Sub

End Class

''' <summary>
''' Obsolete - use HMAC authentication
''' </summary>
''' 
<Obsolete>
Public Class SPCAuthTokenAttribute
    Inherits AuthorizationFilterAttribute


    Public Overrides Sub OnAuthorization(actionContext As HttpActionContext)
        MyBase.OnAuthorization(actionContext)

        Dim token As String
        If Not actionContext.Request.Headers.Contains("SPC_Authorization_Token") Then
            actionContext.Response = New HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) With {.Content = New StringContent("Missing Authorization Token"), .ReasonPhrase = "Missing Authorization Token"}
            Return
        End If

        token = actionContext.Request.Headers.GetValues("SPC_Authorization_Token").First
        If Not [String].Equals(token, System.Configuration.ConfigurationManager.AppSettings("SPC_WebAPI_Authorization_Token"), StringComparison.OrdinalIgnoreCase) Then
            actionContext.Response = New HttpResponseMessage(System.Net.HttpStatusCode.Forbidden) With {.Content = New StringContent("Unauthorized User"), .ReasonPhrase = "Unauthorized User"}
            Return
        End If
    End Sub



End Class
