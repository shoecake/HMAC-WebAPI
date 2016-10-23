Imports System.Web
Imports System.Collections.Specialized

Namespace Extensions
    ' leave in, used by clients of this DLL
    Public Module UriBuilderExtensionMethods

        <System.Runtime.CompilerServices.Extension>
        Public Sub AddQueryArgument(b As UriBuilder, key As String, value As String)
            ' escaping done in the ToString call
            '  key = Uri.EscapeDataString(key)
            '  value = Uri.EscapeDataString(value)


            Dim x As NameValueCollection = HttpUtility.ParseQueryString(b.Query)
            If Not String.IsNullOrEmpty(x.Get(key)) Then
                Throw New ArgumentNullException("Key " & key & " already exists")
            End If
            x.Add(key, value)
            b.Query = x.ToString()
        End Sub

        <System.Runtime.CompilerServices.Extension>
        Public Sub EditQueryArgument(b As UriBuilder, key As String, value As String)
            ' escaping done in the ToString call
            '    key = Uri.EscapeDataString(key)
            '     value = Uri.EscapeDataString(value)

            Dim x As NameValueCollection = HttpUtility.ParseQueryString(b.Query)
            If Not String.IsNullOrEmpty(x.Get(key)) Then
                x(key) = value
            Else
                Throw New ArgumentNullException("Key " & key & " does not exist")
            End If
            b.Query = x.ToString()
        End Sub

        <System.Runtime.CompilerServices.Extension>
        Public Sub AddOrEditQueryArgument(b As UriBuilder, key As String, value As String)
            ' escaping done in the ToString call
            '     key = Uri.EscapeDataString(key)
            '      value = Uri.EscapeDataString(value)

            Dim x As NameValueCollection = HttpUtility.ParseQueryString(b.Query)
            If Not String.IsNullOrEmpty(x.Get(key)) Then
                x(key) = value
            Else
                x.Add(key, value)
            End If
            b.Query = x.ToString()
        End Sub

        <System.Runtime.CompilerServices.Extension>
        Public Sub DeleteQueryArgument(b As UriBuilder, key As String)
            key = Uri.EscapeDataString(key)
            Dim x As NameValueCollection = HttpUtility.ParseQueryString(b.Query)
            If Not String.IsNullOrEmpty(x.Get(key)) Then
                x.Remove(key)
            End If
            b.Query = x.ToString()
        End Sub



    End Module

End Namespace