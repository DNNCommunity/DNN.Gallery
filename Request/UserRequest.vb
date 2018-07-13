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
Imports System.Collections.Generic

Namespace DotNetNuke.Modules.Gallery

  <Serializable()> Public Class GalleryUserRequest
    Inherits GalleryRequest

    'Private mRemovedItems As Integer
    Private mStripCount As Integer
    Private mEndItem As Integer
    Private mPagerItems As New ArrayList
    Private mSortList As List(Of IGalleryObjectInfo)


    Public Sub New(ByVal ModuleID As Integer)
      MyBase.New(ModuleID)
      mSortList = MyBase.Folder.SortList

    End Sub

    Public Sub New(ByVal ModuleID As Integer, ByVal Sort As Config.GallerySort, ByVal SortDESC As Boolean, Optional ByVal Path As String = "")
      MyBase.New(ModuleID, Sort, SortDESC, Path)
      If MyBase.GalleryConfig.IsValidPath Then
        mSortList = MyBase.Folder.SortList
      End If

      mEndItem = MyBase.EndItem

    End Sub

    Public ReadOnly Property SortList() As List(Of IGalleryObjectInfo)
      Get
        Return mSortList
      End Get
    End Property

    Public Overrides ReadOnly Property StripCount() As Integer
      Get
        Return mStripCount
      End Get
    End Property

    Public Overrides ReadOnly Property EndItem() As Integer
      Get
        Return mEndItem
      End Get
    End Property

    Public Overrides Function PagerItems() As ArrayList

      Return mPagerItems

    End Function

    Public Function ValidImages() As ArrayList
      Dim result As New ArrayList
      Dim item As IGalleryObjectInfo

      'PopulateSortList()
      For Each item In SortList
        If item.Type = Config.ItemType.Image Then
          result.Add(item)
        End If
      Next

      Return result

    End Function

    Public Overrides Function CurrentItems() As ArrayList
      Dim _portalSettings As PortalSettings = DotNetNuke.Entities.Portals.PortalController.GetCurrentPortalSettings
      Dim result As New ArrayList
      Dim intCounter As Integer
      Dim newPagerDetail As PagerDetail


      If StartItem > 0 Then
        ' Do sorting if it's request by visitor
        SortList.Sort(New Comparer(New String(0) {[Enum].GetName(GetType(Config.GallerySort), SortType)}, SortDescending))

        Dim validItems As Integer = MyBase.Folder.SortList.Count
        If validItems < mEndItem Then
          ' Decrease number of current items
          mEndItem = validItems
        End If

        mStripCount = CInt(System.Math.Ceiling(validItems / MyBase.GalleryConfig.ItemsPerStrip))

        ' these current items to be displayed in GUI, so we make icon invisible
        For intCounter = StartItem To mEndItem '(MyBase.EndItem - MyBase.Folder.IconItems.Count)
          result.Add(SortList.Item(intCounter - 1))
          'End If
        Next

        intCounter = 0
        mPagerItems.Clear()
        ' Creates the pager items / Create the previous item
        Dim url As String
        If mStripCount > 1 AndAlso MyBase.CurrentStrip > 1 Then
          url = NavigateURL(_portalSettings.ActiveTab.TabID, "", Utils.RemoveEmptyParams(New String(1) {"path=" & Path, "currentstrip=" & (MyBase.CurrentStrip - 1).ToString}))
          newPagerDetail = New PagerDetail
          newPagerDetail.Strip = MyBase.CurrentStrip - 1
          newPagerDetail.Text = Localization.GetString("Previous", MyBase.GalleryConfig.SharedResourceFile)
          newPagerDetail.URL = url
          mPagerItems.Add(newPagerDetail)
        End If

        ' Since this version, pager has been changed to handle folder with so many items, it made pager wrapped in previous version
        Dim displayPage As Integer = MyBase.CurrentStrip
        For intCounter = 1 To mStripCount
          url = NavigateURL(_portalSettings.ActiveTab.TabID, "", Utils.RemoveEmptyParams(New String(1) {"path=" & Path, "currentstrip=" & intCounter.ToString}))
          If (intCounter < 3) OrElse (intCounter > displayPage - 2 AndAlso intCounter < displayPage + 2) OrElse (intCounter > mStripCount - 2) Then
            newPagerDetail = New PagerDetail
            newPagerDetail.Strip = intCounter
            newPagerDetail.Text = CStr(intCounter)
            newPagerDetail.URL = url
            mPagerItems.Add(newPagerDetail)
          Else
            'If ((intCounter = 3) OrElse (intCounter = mStripCount - 2)) Then
            If (intCounter = displayPage + 2) OrElse (intCounter = displayPage - 2) Then
              newPagerDetail = New PagerDetail
              newPagerDetail.Strip = intCounter
              newPagerDetail.Text = "..."
              newPagerDetail.URL = url
              mPagerItems.Add(newPagerDetail)
            End If
          End If
        Next

        ' Creates the next item
        If mStripCount > 1 AndAlso MyBase.CurrentStrip < mStripCount Then
          url = NavigateURL(_portalSettings.ActiveTab.TabID, "", Utils.RemoveEmptyParams(New String(1) {"path=" & Path, "currentstrip=" & (MyBase.CurrentStrip + 1).ToString}))
          newPagerDetail = New PagerDetail
          newPagerDetail.Strip = MyBase.CurrentStrip + 1
          newPagerDetail.Text = Localization.GetString("Next", MyBase.GalleryConfig.SharedResourceFile)
          newPagerDetail.URL = url 'ApplicationURL() & "&path=" & Path & "&currentstrip=" & (MyBase.CurrentStrip + 1).ToString
          mPagerItems.Add(newPagerDetail)
        End If
        'End If
      End If

      Return result

    End Function

  End Class

End Namespace