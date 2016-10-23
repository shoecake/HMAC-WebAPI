Imports System.Net.Http
Imports System.Runtime.Serialization
Imports System.IO

Public Class HTTPClientSPC
        Inherits HttpClient

        '    Private WebAPIURL As String
        Private pUriBuilder As UriBuilder
        Private content As StringContent

        Public Sub New(baseURL As String, controller As String, method As String, publicKey As String, privateKey As String)

            MyBase.New(HttpClientFactory.CreatePipeline(New HttpClientHandler(), Enumerable.Repeat(Of DelegatingHandler)(New HMACAuthenticationDelegatingHandler(publicKey, privateKey), 1)))

            Me.Timeout = New TimeSpan(0, 0, 600)
            pUriBuilder = New UriBuilder(baseURL.EnsureEndsWith("/") & controller.EnsureEndsWith("/") & method)

        End Sub

        Public ReadOnly Property UriBuilder As UriBuilder
            Get
                Return pUriBuilder
            End Get
        End Property

        Public Sub SetContent(contentType As Type, srcContent As Object)
            Dim ser As New Json.DataContractJsonSerializer(contentType)
            Dim ms As New MemoryStream
            ser.WriteObject(ms, srcContent)
            content = New StringContent(Text.Encoding.Default.GetString(ms.ToArray()))
            content.Headers.ContentType = Headers.MediaTypeHeaderValue.Parse("application/json")
        End Sub

        Public Function GetAsyncSPC() As HttpResponseMessage
            Return Task.Run(Function() MyBase.GetAsync(pUriBuilder.Uri)).Result
        End Function

        Public Function PutAsyncSPC() As HttpResponseMessage
            Return Task.Run(Function() MyBase.PutAsync(pUriBuilder.Uri, content)).Result
        End Function

        Public Function PostAsyncSPC() As HttpResponseMessage
            Return Task.Run(Function() MyBase.PostAsync(pUriBuilder.Uri, content)).Result
        End Function

        Public Function DeleteAsyncSPC() As HttpResponseMessage
            Return Task.Run(Function() MyBase.DeleteAsync(pUriBuilder.Uri)).Result
        End Function

    End Class


