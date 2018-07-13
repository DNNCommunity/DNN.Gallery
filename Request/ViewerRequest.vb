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

Imports System.Web
Imports System.Text
Imports System.IO
Imports DotNetNuke
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.services.Localization
Imports DotNetNuke.Modules.Gallery.Config
Imports DotNetNuke.Modules.Gallery.Utils

Namespace DotNetNuke.Modules.Gallery

    ''' <summary>
    ''' Class for using the viewer and slideshow to view galleries
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GalleryViewerRequest
        Inherits BaseRequest

        Private mRequest As HttpRequest
        Private _browsableItems As New Generic.List(Of IGalleryObjectInfo)
        Private _currentItemIndex As Integer
        Private _currentItemNumber As Integer
        Private _nextItemNumber As Integer
        Private _previousItemNumber As Integer

#Region "Public Properties"

        Public ReadOnly Property BrowsableItems() As List(Of IGalleryObjectInfo)
            Get
                If _browsableItems Is Nothing Then
                    _browsableItems = New List(Of IGalleryObjectInfo)
                End If
                Return _browsableItems
            End Get
        End Property

        Public ReadOnly Property CurrentItem() As GalleryFile
            Get
                'If BrowsableItems.Count = 0 Then
                Return CType(MyBase.Folder.List.Item(_currentItemNumber), GalleryFile)
                'Else
                'Return CType(BrowsableItems.Item(_currentItemIndex), GalleryFile)
                'End If
            End Get
        End Property

        Public ReadOnly Property CurrentItemIndex() As Integer
            Get
                Return _currentItemIndex
            End Get
        End Property

        Public ReadOnly Property CurrentItemNumber() As Integer
            Get
                Return _currentItemNumber '+ 1
            End Get
        End Property

        Public ReadOnly Property NextItemNumber() As Integer
            Get
                Return _nextItemNumber
            End Get
        End Property

        Public ReadOnly Property PreviousItemNumber() As Integer
            Get
                Return _previousItemNumber
            End Get
        End Property

#End Region

#Region "Public Methods"

        Public Sub New(ByVal ModuleID As Integer, Optional ByVal SortType As Config.GallerySort = Config.GallerySort.Name, Optional ByVal SortDescending As Boolean = False)
            MyBase.New(ModuleID, SortType, SortDescending)

            ' Don't want to continue processing if this is an invalid path
            If Not GalleryConfig.IsValidPath Then
                Exit Sub
            End If

            Dim intCounter As Integer

            'For Each intCounter In MyBase.Folder.BrowsableItems
            '    Dim item As GalleryFile = CType(MyBase.Folder.List.Item(intCounter), GalleryFile)
            '    If Not (item.Title.ToLower.IndexOf("icon") > -1) _
            '    AndAlso Not (item.Title.ToLower = "watermark") _
            '    AndAlso (item.ApprovedDate <= DateTime.Today OrElse MyBase.GalleryConfig.AutoApproval) Then
            '        myBrowsableItems.Add(item)
            '    Else
            '    End If
            'Next

            For Each intCounter In MyBase.Folder.BrowsableItems
                Dim item As GalleryFile = CType(MyBase.Folder.List.Item(intCounter), GalleryFile)
                BrowsableItems.Add(item)
            Next

            BrowsableItems.Sort(New Comparer(New String(0) {[Enum].GetName(GetType(Config.GallerySort), SortType)}, SortDescending))
            mRequest = HttpContext.Current.Request

            ' Determine initial item to be viewed.
            If Not mRequest.QueryString("currentitem") Is Nothing Then
                _currentItemNumber = CInt(mRequest.QueryString("currentitem"))
            Else
                _currentItemNumber = BrowsableItems.Item(0).Index
            End If

            ' Grab the index of the item in the folder.list collection
            If MyBase.Folder.IsBrowsable Then
                For intCounter = 0 To BrowsableItems.Count - 1
                    If BrowsableItems(intCounter).Index = _currentItemNumber Then
                        _currentItemIndex = intCounter
                        Exit For
                    End If
                Next
                _nextItemNumber = GetNextItemNumber()
                _previousItemNumber = GetPreviousItemNumber()
            End If
        End Sub

        Private Function GetNextItemNumber() As Integer
            If _currentItemIndex = BrowsableItems.Count - 1 Then
                Return BrowsableItems(0).Index
            Else
                Return BrowsableItems(_currentItemIndex + 1).Index
            End If
        End Function

        Private Function GetPreviousItemNumber() As Integer
            If _currentItemIndex = 0 Then
                Return BrowsableItems(BrowsableItems.Count - 1).Index
            Else
                Return BrowsableItems(_currentItemIndex - 1).Index
            End If
        End Function

        Private Function Constrain(ByVal n As Integer, ByVal min As Integer, ByVal max As Integer) As Integer
            If n < 0 Then
                Return min
            ElseIf n > max Then
                Return min
            End If
            Return n
        End Function

        Public Sub MoveNext()
            _currentItemNumber = _nextItemNumber
            _currentItemIndex = Constrain(_currentItemIndex + 1, 0, BrowsableItems.Count - 1)
            _previousItemNumber = GetPreviousItemNumber()
            _nextItemNumber = GetNextItemNumber()
        End Sub

        Public Sub MovePrevious()
            _currentItemNumber = _previousItemNumber
            _currentItemIndex = Constrain(_currentItemIndex - 1, 0, BrowsableItems.Count - 1)
            _previousItemNumber = GetPreviousItemNumber()
            _previousItemNumber = GetPreviousItemNumber()
        End Sub
#End Region

    End Class

End Namespace