'
' DotNetNuke® - http://www.dotnetnuke.com
' Copyright (c) 2002-2011 by DotNetNuke Corp. 

'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
' DEALINGS IN THE SOFTWARE.
'
Option Strict On

Imports System

Namespace DotNetNuke.Modules.Gallery

    Public Class GalleryObjectCollection
    Inherits System.Collections.CollectionBase

        ' Allows access to items by key AND index
        ' WES changed to case insensitive hashtable key comparisons following issues resulting from forced lowercasing of url
        ' querystrings implemented for GAL-9403 and GAL-9404. Also limited refactoring to make collection strongly typed
        ' for IGalleryObjectInfo interface.

        Private keyIndexLookup As Hashtable = New Hashtable(System.StringComparer.InvariantCultureIgnoreCase)

        Public Sub New()
            MyBase.New()
        End Sub

        Friend Shadows Sub Clear()
            keyIndexLookup.Clear()
            MyBase.Clear()
        End Sub

        Public Sub Add(ByVal key As String, ByVal value As IGalleryObjectInfo)
            Dim index As Integer
            Try
                index = MyBase.List.Add(value)
                keyIndexLookup.Add(key, index)
            Catch ex As Exception
                LogException(ex)
                'Throw ex
            End Try

        End Sub

        Public Function Item(ByVal index As Integer) As IGalleryObjectInfo
            Try
                Return CType(MyBase.List.Item(index), IGalleryObjectInfo)
            Catch Exc As System.Exception
                Return Nothing
            End Try
        End Function

        Public Function Item(ByVal key As String) As IGalleryObjectInfo
            Dim obj As Object = keyIndexLookup.Item(key)

            ' Do validation first
            If obj Is Nothing Then
                Throw New ArgumentException("The item with the provided key can not be found.")
            End If

            Return CType(MyBase.List.Item(CType(obj, Integer)), IGalleryObjectInfo)
        End Function
    End Class

End Namespace