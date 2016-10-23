Imports System.Configuration


Namespace Configuration

    Public Class HMACSettings
        Inherits ConfigurationSection

        <ConfigurationProperty("keys", IsDefaultCollection:=False)>
        Public ReadOnly Property Keys() As HMACKeyCollection
            Get
                Return CType(Me("keys"), HMACKeyCollection)
            End Get
        End Property


    End Class


    <ConfigurationCollection(GetType(HMACKeyCollection), CollectionType:=ConfigurationElementCollectionType.AddRemoveClearMap)>
    Public Class HMACKeyCollection
        Inherits ConfigurationElementCollection
        Public Overrides ReadOnly Property CollectionType() As ConfigurationElementCollectionType
            Get
                Return ConfigurationElementCollectionType.AddRemoveClearMap
            End Get
        End Property

        Protected Overrides Function CreateNewElement() As ConfigurationElement
            Return New HMACKeyElement()
        End Function

        Protected Overrides Function GetElementKey(element As ConfigurationElement) As Object
            Return DirectCast(element, HMACKeyElement).publicKey
        End Function

        Default Public Overloads Property Item(index As Integer) As HMACKeyElement
            Get
                Return DirectCast(MyBase.BaseGet(index), HMACKeyElement)
            End Get
            Set
                If MyBase.BaseGet(index) IsNot Nothing Then
                    MyBase.BaseRemoveAt(index)
                End If
                MyBase.BaseAdd(index, Value)
            End Set
        End Property

        Default Public Overloads ReadOnly Property Item(publicKey As String) As HMACKeyElement
            Get
                If String.IsNullOrEmpty(publicKey) Then
                    Throw New InvalidOperationException("Indexer 'publicKey' cannot be null or empty.")
                End If
                For Each element As HMACKeyElement In Me
                    If element.publicKey.Equals(publicKey, StringComparison.InvariantCultureIgnoreCase) Then
                        Return element
                    End If
                Next
                Throw New InvalidOperationException("Indexer 'publicKey' specified cannot be found in the collection.")
            End Get
        End Property
    End Class


    Public Class HMACKeyElement
        Inherits ConfigurationElement

        <ConfigurationProperty("publicKey", DefaultValue:="", IsRequired:=True)>
        Public Property publicKey() As String
            Get
                Return CType(Me("publicKey"), String)
            End Get
            Set(ByVal value As String)
                Me("publicKey") = value
            End Set
        End Property

        <ConfigurationProperty("privateKey", DefaultValue:="", IsRequired:=True)>
        Public Property privateKey() As String
            Get
                Return CType(Me("privateKey"), String)
            End Get
            Set(ByVal value As String)
                Me("privateKey") = value
            End Set
        End Property

        ''' <summary>
        ''' can be used to identify who the key has been assigned to (e.g. which broker in the Isons Conveyance Portal)
        ''' </summary>
        ''' <returns></returns>
        <ConfigurationProperty("name", DefaultValue:="", IsRequired:=False)>
        Public Property name() As String
            Get
                Return CType(Me("name"), String)
            End Get
            Set(ByVal value As String)
                Me("name") = value
            End Set
        End Property
    End Class

End Namespace