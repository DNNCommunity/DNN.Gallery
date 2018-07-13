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
Imports DotNetNuke.Security
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Modules.Gallery.Config
Imports DotNetNuke.Modules.Gallery.Utils

Namespace DotNetNuke.Modules.Gallery

  ''' <summary>
  ''' Class for traditional gallery viewing (i.e., 'strip' viewing)
  ''' </summary>
  ''' <remarks></remarks>
  Public Class GalleryRequest
    Inherits BaseRequest

    Private mRequest As HttpRequest

    Private _currentStrip As Integer
    Private _stripCount As Integer
    Private _startItem As Integer
    Private _endItem As Integer
    Private _firstImageIndex As Integer = -1
    Private _pagerItems As New ArrayList

#Region "Public Properties"

    Public ReadOnly Property StartItem() As Integer
      ' Beginning item of this page
      Get
        Return _startItem
      End Get
    End Property

    Public Overridable ReadOnly Property EndItem() As Integer
      ' Ending item of this page
      Get
        Return _endItem
      End Get
    End Property

    Public ReadOnly Property CurrentStrip() As Integer
      ' Current page being viewed
      Get
        Return _currentStrip
      End Get
    End Property

    Public Overridable ReadOnly Property StripCount() As Integer
      ' Count of pages existing in this folder
      Get
        Return _stripCount
      End Get
    End Property

    Public Overridable ReadOnly Property FirstImageIndex() As Integer
      Get
        Return _firstImageIndex
      End Get
    End Property

    Public Overridable Function PagerItems() As ArrayList
      ' Collection of pages to be clicked on for navigation

      Return _pagerItems

    End Function

    Public Overridable Function CurrentItems() As ArrayList
      ' Collection of items for current page only

      Dim result As New ArrayList
      Dim intCounter As Integer

      If _startItem > 0 Then
        For intCounter = _startItem To _endItem
          ' New since v2.0 for using a image to be album icon                    
          result.Add(MyBase.Folder.List.Item(intCounter - 1))
          'End If

          ' find the first image in these current items
          If MyBase.Folder.List.Item(intCounter - 1).Type = Config.ItemType.Image _
              AndAlso _firstImageIndex = -1 Then
            _firstImageIndex = (intCounter - 1)
          End If
        Next
      End If

      Return result

    End Function

    ' WES Note: Function SubAlbumItems is never called in current code

    Public Function SubAlbumItems() As List(Of GalleryFolder)
      ' Collection of items for current page only

      Dim result As New List(Of GalleryFolder)

      For intCounter As Integer = 0 To MyBase.Folder.List.Count - 1
        If MyBase.Folder.List.Item(intCounter).IsFolder Then
          result.Add(CType(MyBase.Folder.List.Item(intCounter), GalleryFolder))
        End If
      Next

      Return result

    End Function

    ' WES Note - Function FileItems is never called in current code

    Public Function FileItems() As List(Of GalleryFile)
      ' Collection of items for current page only

      Dim result As New List(Of GalleryFile)

      For intCounter As Integer = 0 To MyBase.Folder.List.Count - 1
        If Not MyBase.Folder.List.Item(intCounter).IsFolder Then
          result.Add(CType(MyBase.Folder.List.Item(intCounter), GalleryFile))
        End If
      Next

      Return result

    End Function

#End Region

#Region "Public Methods"
    Public Sub New(ByVal ModuleID As Integer, Optional ByVal SortType As Config.GallerySort = Config.GallerySort.Name, Optional ByVal SortDescending As Boolean = False, Optional ByVal GalleryPath As String = "")
      ' Constructor method for the traditional gallery request
      MyBase.New(ModuleID, SortType, SortDescending, GalleryPath)
      Dim _portalSettings As PortalSettings = DotNetNuke.Entities.Portals.PortalController.GetCurrentPortalSettings

      ' Don't want to continue processing if this is an invalid path
      If Not GalleryConfig.IsValidPath Then
        Exit Sub
      End If

      Dim newPagerDetail As PagerDetail
      Dim pagerCounter As Integer

      ' Grab current request context
      mRequest = HttpContext.Current.Request

      ' Logic to determine paging
      _currentStrip = CInt(mRequest.QueryString("CurrentStrip"))

      Try
        ' Get the count of pages for this folder
        _stripCount = CInt(System.Math.Ceiling(MyBase.Folder.List.Count / MyBase.GalleryConfig.ItemsPerStrip))

      Catch ex As Exception
        _stripCount = 1
      End Try

      ' Do a little validation
      If _currentStrip = 0 OrElse (_currentStrip > _stripCount) Then
        _currentStrip = 1
      End If

      ' Calculate the starting item
      If MyBase.Folder.List.Count = 0 Then
        _startItem = 0
      Else
        _startItem = (_currentStrip - 1) * MyBase.GalleryConfig.ItemsPerStrip + 1
      End If
      ' and calculate the ending item
      _endItem = _startItem + MyBase.GalleryConfig.ItemsPerStrip - 1
      If _endItem > MyBase.Folder.List.Count Then
        _endItem = MyBase.Folder.List.Count
      End If

      ' Creates the pager items / Create the previous item
      Dim url As String
      If _stripCount > 1 AndAlso _currentStrip > 1 Then
        url = NavigateURL(_portalSettings.ActiveTab.TabID, "", Utils.RemoveEmptyParams(New String(1) {"path=" & Path, "currentstrip=" & (_currentStrip - 1).ToString}))
        newPagerDetail = New PagerDetail
        newPagerDetail.Strip = _currentStrip - 1
        newPagerDetail.Text = Localization.GetString("Previous", MyBase.GalleryConfig.SharedResourceFile)
        newPagerDetail.URL = url
        _pagerItems.Add(newPagerDetail)
      End If

      ' Creates folder items
      For pagerCounter = 1 To _stripCount
        newPagerDetail = New PagerDetail
        newPagerDetail.Strip = pagerCounter
        newPagerDetail.Text = CStr(pagerCounter)
        If String.IsNullOrEmpty(Path) Then
          newPagerDetail.URL = ApplicationURL() & "&currentstrip=" & pagerCounter.ToString
        Else
          newPagerDetail.URL = ApplicationURL() & "&path=" & Path & "&currentstrip=" & pagerCounter.ToString
        End If
        _pagerItems.Add(newPagerDetail)
      Next

      ' Creates the next item
      If _stripCount > 1 AndAlso _currentStrip < _stripCount Then
        url = NavigateURL(_portalSettings.ActiveTab.TabID, "", Utils.RemoveEmptyParams(New String(1) {"path=" & Path, "currentstrip=" & (_currentStrip + 1).ToString}))
        newPagerDetail = New PagerDetail
        newPagerDetail.Strip = _currentStrip + 1
        newPagerDetail.Text = "Next"
        newPagerDetail.URL = url
        _pagerItems.Add(newPagerDetail)
      End If

    End Sub

#End Region

  End Class

End Namespace