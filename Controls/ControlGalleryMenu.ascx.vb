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

Imports System.Web.UI.Control
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.UI.Skins
Imports DotNetNuke.Services.Exceptions
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Modules.Gallery.Utils
Imports DotNetNuke.UI.WebControls

Namespace DotNetNuke.Modules.Gallery.WebControls

  Partial Public MustInherit Class GalleryMenu
    Inherits DotNetNuke.UI.Skins.SkinObjectBase

    Private mModuleID As Integer
    Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config
    Private mGalleryRequest As BaseRequest
    Private mGalleryAuthorize As DotNetNuke.Modules.Gallery.Authorization
    Private mCurrentAlbum As GalleryFolder

    Private Enum IconTypes As Integer
      MenuEdit = 1
      MenuEditFile = 2
      Folder = 2
      MenuAddAlbum = 2
      MenuAddFile = 3
      MenuMaintenance = 4
    End Enum


#Region " Web Form Designer Generated Code "

    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

    End Sub

    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
      'CODEGEN: This method call is required by the Web Form Designer
      'Do not modify it using the code editor.
      InitializeComponent()
    End Sub

#End Region

    '*******************************************************
    '
    ' The Page_Load server event handler on this page is used
    ' to populate the role information for the page
    '
    '*******************************************************
    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

      Dim BreakMenuNode As MenuNode = Nothing

      mGalleryConfig = Config.GetGalleryConfig(ModuleID)
      If mGalleryConfig.IsValidPath And Not IsPostBack Then
        mGalleryAuthorize = New DotNetNuke.Modules.Gallery.Authorization(ModuleID)
        mCurrentAlbum = mGalleryRequest.Folder

        Dim ParentMenuNode As MenuNode
        Dim imgPath As String = mGalleryConfig.ImageFolder

        Try
          ' generate dynamic menu
          With ctlDNNMenu
            With .ImageList
              .Add(imgPath & "s_picture.gif")
              .Add(imgPath & "s_edit.gif")
              .Add(imgPath & "s_folder.gif")
              .Add(imgPath & "s_new2.gif")
              .Add(imgPath & "s_bookopen.gif")
            End With
            .MenuCssClass = "GalleryMenu_SubMenu"
            .DefaultChildNodeCssClass = "GalleryMenu_MenuItem"
            .DefaultNodeCssClassOver = "GalleryMenu_MenuItemSel"
            .DefaultIconCssClass = "GalleryMenu_MenuIcon"
          End With

          'Build top level parent menu node

          Dim CurrentAlbumMenuID As String
          If mCurrentAlbum.ID < 0 Then
            CurrentAlbumMenuID = ModuleID.ToString
          Else
            CurrentAlbumMenuID = mCurrentAlbum.ID.ToString & "." & mCurrentAlbum.Index.ToString
          End If

          ParentMenuNode = ctlDNNMenu.MenuNodes(ctlDNNMenu.MenuNodes.Add())

          With ParentMenuNode
            .ID = CurrentAlbumMenuID
            .ClickAction = eClickAction.None
            .ImageIndex = 0
            .Text = ""
            .CSSClass = "GalleryMenu_MenuBar"
            .CssClassOver = "GalleryMenu_MenuBar"
            .CSSIcon = "GalleryMenu_IconButton"
          End With

          ' Build menu nodes for each level of folder hierarchy
          If (mGalleryConfig.IsValidPath) AndAlso (mCurrentAlbum.List.Count > 0) Then
            Dim child As IGalleryObjectInfo
            For Each child In mCurrentAlbum.List
              If TypeOf child Is GalleryFolder AndAlso (mGalleryAuthorize.ItemIsApproved(child) OrElse mGalleryAuthorize.HasItemEditPermission(child)) Then
                BuildAlbumMenuNode(ParentMenuNode, CType(child, GalleryFolder))
              End If
            Next
            If ParentMenuNode.MenuNodes.Count > 0 Then
              ' Add a menu break of any child albums were added to the menu
              BreakMenuNode = ParentMenuNode.MenuNodes(ParentMenuNode.MenuNodes.AddBreak())
              BreakMenuNode.CSSIcon = "GalleryMenu_MenuBreak"
            End If
          End If

          ' Build menu nodes for context sensitive gallery management links
          ' William Severance - GAL-832 make gallery and media context menus more context sensitive
          ' Do not show Edit Album, Add File, Add Album or Maintenance menu items if not in gallery view
          ' Gallery view when QueryString does not contain ctl key

          Dim hasManagementNode As Boolean = False

          If Request.QueryString("ctl") Is Nothing AndAlso mGalleryConfig.IsValidPath Then
            Dim strCurrentStrip As String = Nothing
            If TypeOf mGalleryRequest Is GalleryUserRequest Then
              strCurrentStrip = "currentstrip=" & CType(mGalleryRequest, GalleryUserRequest).CurrentStrip.ToString
            End If

            If mGalleryAuthorize.HasItemEditPermission(mCurrentAlbum) Then
              BuildMenuNode(ParentMenuNode, "MenuEdit", Utils.AppendURLParameter(mCurrentAlbum.EditURL, strCurrentStrip))
              hasManagementNode = True
            End If
            If mGalleryAuthorize.HasItemUploadPermission(mCurrentAlbum) Then
              BuildMenuNode(ParentMenuNode, "MenuAddFile", mCurrentAlbum.AddFileURL)
              hasManagementNode = True
            End If
            If mGalleryAuthorize.HasItemEditPermission(mCurrentAlbum) Then
              BuildMenuNode(ParentMenuNode, "MenuAddAlbum", mCurrentAlbum.AddSubAlbumURL)
              BuildMenuNode(ParentMenuNode, "MenuMaintenance", Utils.AppendURLParameter(mCurrentAlbum.MaintenanceURL, strCurrentStrip))
              hasManagementNode = True
            End If
          End If

          If BreakMenuNode IsNot Nothing AndAlso Not hasManagementNode Then
            ParentMenuNode.MenuNodes.Remove(BreakMenuNode)
          End If

        Catch exc As Exception 'Module failed to load
          ProcessModuleLoadException(Me, exc)
        End Try
      End If

    End Sub

    Private Sub BuildMenuNode(ByVal ParentMenuNode As MenuNode, ByVal CommandName As String, ByVal URL As String)
      Dim strTitle As String = Localization.GetString(CommandName, mGalleryConfig.SharedResourceFile)
      Dim objMenuNode As MenuNode = ParentMenuNode.MenuNodes(ParentMenuNode.MenuNodes.Add())
      With objMenuNode
        .ID = String.Concat(ParentMenuNode.ID, ".", CommandName)
        .Text = strTitle
        .NavigateURL = URL.Replace("~", ApplicationPath)
        .ImageIndex = CType([Enum].Parse(GetType(IconTypes), CommandName), Integer)
        .ToolTip = Localization.GetString(String.Concat(CommandName, ".Tooltip"), mGalleryConfig.SharedResourceFile)
      End With
    End Sub

    Private Sub BuildAlbumMenuNode(ByVal ParentMenuNode As MenuNode, ByVal Folder As GalleryFolder)
      Dim child As IGalleryObjectInfo
      Dim strTitle As String = String.Concat(Folder.Title, " (", Folder.Size.ToString, " ", Localization.GetString("Items", mGalleryConfig.SharedResourceFile), ")")
      Dim AlbumMenuID As String = Folder.ID.ToString & "." & Folder.Index.ToString

      Dim objMenuNode As MenuNode = ParentMenuNode.MenuNodes(ParentMenuNode.MenuNodes.Add())
      With objMenuNode
        .ID = AlbumMenuID
        .Text = strTitle
        .ImageIndex = IconTypes.Folder
        .NavigateURL = Folder.BrowserURL.Replace("~", ApplicationPath)
      End With

      If mGalleryConfig.MultiLevelMenu Then
        For Each child In Folder.List
          If TypeOf child Is GalleryFolder AndAlso (mGalleryAuthorize.ItemIsApproved(child) OrElse mGalleryAuthorize.HasItemEditPermission(child)) Then
            BuildAlbumMenuNode(objMenuNode, CType(child, GalleryFolder))
          End If
        Next
      End If
    End Sub

    Public Property ModuleID() As Integer
      Get
        Return mModuleID
      End Get
      Set(ByVal Value As Integer)
        mModuleID = Value
      End Set
    End Property

    Public Property GalleryRequest() As BaseRequest
      Get
        Return mGalleryRequest
      End Get
      Set(ByVal Value As BaseRequest)
        mGalleryRequest = Value
      End Set
    End Property

  End Class

End Namespace
