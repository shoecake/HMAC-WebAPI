Friend Module MiscExtensions
    <System.Runtime.CompilerServices.Extension>
    Public Function EnsureEndsWith(value As String, suffix As String) As String
        Return If(value.EndsWith(suffix), value, String.Concat(value, suffix))
    End Function
End Module
