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

Imports System.Text
Imports System
Imports System.Collections
Imports System.IO
Imports DotNetNuke
Imports DotNetNuke.Security
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Entities.Modules.Actions
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Modules.Gallery.Config
Imports DotNetNuke.Modules.Gallery.Utils
Imports DotNetNuke.Modules.Gallery.WebControls
Imports DotNetNuke.Services.Exceptions

Namespace DotNetNuke.Modules.Gallery

  Partial Public MustInherit Class Container
    Inherits Entities.Modules.PortalModuleBase
    Implements Entities.Modules.IActionable
    Implements Entities.Modules.ISearchable

    Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config
    Private mGalleryAuthorize As DotNetNuke.Modules.Gallery.Authorization
    Private mUserRequest As GalleryUserRequest
    Private mCurrentItems As New ArrayList
    Private mSort As Config.GallerySort
    Private mView As Config.GalleryView
    Private mSortDESC As Boolean
    Private mValidGallery As Boolean

#Region "Private Methods"

    Private Sub EnableControl(ByVal IsEnabled As Boolean)

      ClearCache.Visible = GalleryAuthorize.IsItemOwner(mUserRequest.Folder)

      lblView.Visible = (IsEnabled AndAlso mGalleryConfig.AllowChangeView)
      ddlGalleryView.Visible = lblView.Visible
      lblSortBy.Visible = IsEnabled
      ddlGallerySort.Visible = IsEnabled
      chkDESC.Visible = IsEnabled
      lblStats.Visible = IsEnabled

    End Sub

    Private Sub BindGalleryView()
      If Not mGalleryConfig.AllowChangeView Then
        Exit Sub
      End If

      Dim dict As New System.Collections.Specialized.ListDictionary
      For Each name As String In [Enum].GetNames(GetType(Config.GalleryView))
        dict.Add(name, Localization.GetString("View" + name, mGalleryConfig.SharedResourceFile))
      Next

      With ddlGalleryView
        .ClearSelection()
        .DataTextField = "value"
        .DataValueField = "key"
        .DataSource = dict
        .DataBind()
        If Not .Items.FindByValue(mView.ToString) Is Nothing Then
          .Items.FindByValue(mView.ToString).Selected = True
        End If
      End With

    End Sub

    Private Sub BindGallerySort()
      Dim dict As New System.Collections.Specialized.ListDictionary
      For Each name As String In mGalleryConfig.SortProperties
        dict.Add(name, Localization.GetString("Sort" + name, mGalleryConfig.SharedResourceFile))
      Next

      With ddlGallerySort
        .ClearSelection()
        .DataTextField = "value"
        .DataValueField = "key"
        .DataSource = dict
        .DataBind()
        If Not .Items.FindByValue(mSort.ToString) Is Nothing Then
          .Items.FindByValue(mSort.ToString).Selected = True
        End If
      End With

      chkDESC.Checked = mSortDESC

    End Sub

    Private Function GetStats() As String

      'William Severance (9/11/10) - Refactored to avoid null reference errors, display of unapproved item count to non-admin/non-owner users
      Dim stats As String = ""
      If mUserRequest IsNot Nothing AndAlso mUserRequest.Folder IsNot Nothing AndAlso mUserRequest.CurrentItems.Count > 0 Then
        Dim iconCount As Integer = mUserRequest.Folder.IconItems.Count
        Dim iunApprovedCount As Integer = mUserRequest.Folder.UnApprovedItems.Count


        If (iunApprovedCount = 0) OrElse (Not mGalleryAuthorize.IsItemOwner(mUserRequest.Folder)) Then
          stats = Localization.GetString("GalleryStatsApproved", LocalResourceFile)
          stats = stats.Replace("[TotalItem]", (mUserRequest.Folder.List.Count - iconCount - iunApprovedCount).ToString) ' - mUserRequest.RemoveItems).ToString)
        Else
          stats = Localization.GetString("GalleryStatsUnApproved", LocalResourceFile)
          stats = stats.Replace("[TotalItem]", (mUserRequest.Folder.List.Count - iconCount).ToString) '- mUserRequest.RemoveItems).ToString)
          stats = stats.Replace("[Unapproved]", iunApprovedCount.ToString)
        End If

        stats = stats.Replace("[StartItem]", (mUserRequest.StartItem).ToString)
        stats = stats.Replace("[EndItem]", (mUserRequest.CurrentItems.Count + mUserRequest.StartItem - 1).ToString)

      End If

      Return stats

    End Function

    Private Sub Initialize()

      Try
        If mGalleryConfig Is Nothing Then
          mGalleryConfig = GetGalleryConfig(ModuleId)
        End If

        mGalleryAuthorize = New Authorization(ModuleConfiguration)
        mView = GetView(mGalleryConfig)
        mSort = GetSort(mGalleryConfig)
        mSortDESC = GetSortDESC(mGalleryConfig)
        mUserRequest = New GalleryUserRequest(ModuleId, mSort, mSortDESC)

        With ctlGallery
          .PortalID = PortalSettings.PortalId
          .TabID = PortalSettings.ActiveTab.TabID
          .ModuleId = ModuleId
          .GalleryConfig = mGalleryConfig
          .GalleryAuthorize = mGalleryAuthorize
          .LocalResourceFile = Me.LocalResourceFile
          .View = mView
          .Sort = mSort
          .SortDESC = mSortDESC

          .UserRequest = mUserRequest
          .UserRequest.FolderPaths()
        End With

        ' Load gallery menu, this method allow call request only once
        Dim galleryMenu As GalleryMenu = CType(LoadControl("Controls/ControlGalleryMenu.ascx"), GalleryMenu)
        With galleryMenu
          .ModuleID = ModuleId
          .GalleryRequest = CType(mUserRequest, BaseRequest)
        End With
        celGalleryMenu.Controls.Add(galleryMenu)

        ' Load gallery breadcrumbs, this method allow call request only once
        Dim galleryBreadCrumbs As BreadCrumbs = CType(LoadControl("Controls/ControlBreadCrumbs.ascx"), BreadCrumbs)
        With galleryBreadCrumbs
          .ModuleID = ModuleId
          .GalleryRequest = CType(mUserRequest, BaseRequest)
        End With
        celBreadcrumbs.Controls.Add(galleryBreadCrumbs)

        Dim title As String = CType(Page, CDefault).Title
        For Each itm As FolderDetail In mUserRequest.FolderPaths
          If title <> itm.Name Then
            title &= (" > " & itm.Name)
          End If
        Next
        CType(Page, CDefault).Title = title

      Catch ex As Exception
        ' JIMJ Add logging when we can't init things,
        ' Could be if we can't write to the xml file
        LogException(ex)
        Throw
      End Try

    End Sub

#End Region

#Region "Properties"

    Public ReadOnly Property GalleryConfig() As DotNetNuke.Modules.Gallery.Config
      Get
        Return mGalleryConfig
      End Get
    End Property

    Public ReadOnly Property GalleryAuthorize() As DotNetNuke.Modules.Gallery.Authorization
      Get
        Return mGalleryAuthorize
      End Get
    End Property

    Public ReadOnly Property UserRequest() As GalleryUserRequest
      Get
        Return mUserRequest
      End Get
    End Property

    Public ReadOnly Property CurrentItems() As ArrayList
      Get
        Return mCurrentItems
      End Get
    End Property

    Public ReadOnly Property View() As Config.GalleryView
      Get
        Return mView
      End Get
    End Property

#End Region

#Region "Event Handlers"

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
      Try
        ' Load the styles
        DirectCast(Page, CDefault).AddStyleSheet(CreateValidID(GalleryConfig.Css()), GalleryConfig.Css())

        If mGalleryConfig.IsValidPath Then
          mValidGallery = True

          If (Not Page.IsPostBack) Then
            BindGalleryView()
            BindGallerySort()
            EnableControl(mGalleryConfig.RootFolder.List.Count > 0)
            lblStats.Text = GetStats()
          End If

        Else
          mValidGallery = False
          EnableControl(False)
          lblStats.Text = GetStats() & Localization.GetString("GalleryEmpty", LocalResourceFile)
        End If

      Catch exc As Exception
        ProcessModuleLoadException(Me, exc)
      End Try

    End Sub

    'William Severance - Modified following three methods to include new parameter ModuleID in call to RefreshCookie method
    'which was revised to provide independent behavior in these settings when multiple gallery modules exist i the same portal.

    ' Quinn Gil - Changed Response.Redirect(Request.Url.ToString) to Response.Redirect(SanitizedRawURL(...)) for GAL-9403

    Private Sub ddlGalleryView_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlGalleryView.SelectedIndexChanged
      mView = CType([Enum].Parse(GetType(Config.GalleryView), ddlGalleryView.SelectedItem.Value), Config.GalleryView)
      RefreshCookie(GALLERY_VIEW, ModuleId, mView)
      Response.Redirect(SanitizedRawUrl(TabId, ModuleId, "", "currentstrip=", Request))
    End Sub

    Private Sub ddlGallerySort_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlGallerySort.SelectedIndexChanged
      mSort = CType([Enum].Parse(GetType(Config.GallerySort), ddlGallerySort.SelectedItem.Value), Config.GallerySort)
      RefreshCookie(GALLERY_SORT, ModuleId, mSort)
      Response.Redirect(SanitizedRawUrl(TabId, ModuleId, "", "currentstrip=", Request))
    End Sub

    Private Sub chkDESC_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkDESC.CheckedChanged
      mSortDESC = chkDESC.Checked
      RefreshCookie(GALLERY_SORT_DESC, ModuleId, mSortDESC)
      Response.Redirect(SanitizedRawUrl(TabId, ModuleId, "", "currentstrip=", Request))
    End Sub

    Private Sub ClearCache_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles ClearCache.Click
      Config.ResetGalleryFolder(mUserRequest.Folder)
      Response.Redirect(SanitizedRawUrl(TabId, ModuleId))
    End Sub

#End Region

#Region "Optional Interfaces"
    Public ReadOnly Property ModuleActions() As ModuleActionCollection Implements Entities.Modules.IActionable.ModuleActions
      'Implements Entities.Modules.IActionable.ModuleActions
      Get
        Dim Actions As New ModuleActionCollection
        Actions.Add(GetNextActionID, Localization.GetString("Configuration.Action", LocalResourceFile), ModuleActionType.ModuleSettings, "", "edit.gif", EditUrl(ControlKey:="configuration"), "", False, SecurityAccessLevel.Admin, True, False)
        Return Actions
      End Get
    End Property

    Public Function GetSearchItems(ByVal ModInfo As Entities.Modules.ModuleInfo) As Services.Search.SearchItemInfoCollection Implements Entities.Modules.ISearchable.GetSearchItems
      'Implements Entities.Modules.ISearchable.GetSearchItems
      ' included as a stub only so that the core knows this module Implements Entities.Modules.ISearchable
      Return Nothing
    End Function

#End Region

#Region " Web Form Designer Generated Code "

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

    End Sub

    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
      'CODEGEN: This method call is required by the Web Form Designer
      'Do not modify it using the code editor.
      InitializeComponent()

      Initialize()

    End Sub

#End Region

  End Class

End Namespace

