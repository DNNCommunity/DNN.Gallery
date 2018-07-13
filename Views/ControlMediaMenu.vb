'
' DotNetNuke® - http://www.dotnetnuke.com
' Copyright (c) 2002-2011 by DotNetNuke Corp. 

'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, dfistribute, sublicense, and/or sell copies of the Software, and 
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
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.ComponentModel
Imports System.IO
Imports DotNetNuke
Imports DotNetNuke.Services.Exceptions
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.UI.WebControls

<Assembly: TagPrefix(".Gallery.Views.MediaMenu", "DotNetNuke.Modules")> 
Namespace DotNetNuke.Modules.Gallery.Views

  <ToolboxData("<{0}:GalleryMenu runat=server></{0}:GalleryMenu>")> _
  Public Class MediaMenu
    Inherits DNNMenu

    Private Enum IconTypes As Integer
      Slideshow = 0
      EXIFData = 1
      Download = 2
      MenuEdit = 3
      MenuEditFile = 3
      MenuAddAlbum = 4
      MenuAddFile = 5
      MenuMaintenance = 6
    End Enum

    Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config = Nothing
    Private mGalleryAuthorize As DotNetNuke.Modules.Gallery.Authorization = Nothing
    Private mGalleryObject As IGalleryObjectInfo
    Private mCurrentStrip As Integer = 0
    Private mImgPath As String

    Public Sub New()
      MyBase.New()
    End Sub 'New

    Public Sub New(ByVal ModuleId As Integer, ByVal ObjectInfo As IGalleryObjectInfo)
      MyBase.New()
      Me.ModuleID = ModuleId
      mGalleryObject = ObjectInfo
      mImgPath = GalleryConfig.ImageFolder
      Me.ID = Utils.GetGalleryObjectMenuID(GalleryObject)
    End Sub 'New

    Private Function AppendCurrentStripParameter(ByVal URL As String, ByVal Append As Boolean) As String
      If Append AndAlso mCurrentStrip > 1 Then
        Return Utils.AppendURLParameter(URL, String.Format("currentstrip={0}", mCurrentStrip))
      Else
        Return URL
      End If
    End Function

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
      MyBase.OnLoad(e)
      If Not Page.IsPostBack Then
        Try
          Orientation = UI.WebControls.Orientation.Horizontal

          If (Not Page.Request.QueryString("currentstrip") Is Nothing) Then
            mCurrentStrip = Integer.Parse(Page.Request.QueryString("currentstrip"))
          End If

          With ImageList
            .Add(mImgPath & "s_movie.gif")
            .Add(mImgPath & "s_exif.gif")
            .Add(mImgPath & "s_download.gif")
            .Add(mImgPath & "s_edit.gif")
            .Add(mImgPath & "s_folder.gif")
            .Add(mImgPath & "s_new2.gif")
            .Add(mImgPath & "s_bookopen.gif")
          End With

          MenuCssClass = "MediaMenu_SubMenu"
          DefaultChildNodeCssClass = "MediaMenu_MenuItem"
          DefaultNodeCssClassOver = "MediaMenu_MenuItemSel"
          DefaultIconCssClass = "MediaMenu_MenuIcon"

          'generate clickable top node to navigate to viewer

          Dim ParentMenuNode As MenuNode = MenuNodes(MenuNodes.Add())

          With ParentMenuNode
            ' WES - Fix for Issue - Multiple controls of same ID when FolderID and FileID are the same.
            .ID = Utils.GetGalleryObjectMenuNodeID(GalleryObject)
            .Text = ""
            .CSSClass = "MediaMenu_MenuItem"
            .CssClassOver = "MediaMenu_MenuItem"
            .CSSIcon = "MediaMenu_Thumbnail"
            .ToolTip = Path.GetFileName(GalleryObject.Thumbnail)
            .Image = GalleryObject.ThumbnailURL
            .NavigateURL = AppendCurrentStripParameter(GalleryObject.BrowserURL, (Not GalleryConfig.AllowPopup) AndAlso (GalleryObject.Type = Config.ItemType.Image))
          End With

          ' Add Slideshow
          If GalleryAuthorize.ItemCanSlideshow(GalleryObject) Then
            BuildMenuNode(ParentMenuNode, "Slideshow", AppendCurrentStripParameter(GalleryObject.SlideshowURL, (Not GalleryConfig.AllowPopup) AndAlso (GalleryObject.Type = Config.ItemType.Image)))
          End If

          ' Add Exif Viewer
          If GalleryAuthorize.ItemCanViewExif(GalleryObject) Then
            BuildMenuNode(ParentMenuNode, "EXIFData", AppendCurrentStripParameter(GalleryObject.ExifURL, (Not GalleryConfig.AllowPopup) AndAlso (GalleryObject.Type = Config.ItemType.Image)))
          End If

          ' Add Download
          If GalleryAuthorize.ItemCanDownload(GalleryObject) Then
            BuildMenuNode(ParentMenuNode, "Download", GalleryObject.DownloadURL)
          End If

          Dim hasEdit As Boolean = GalleryAuthorize.HasItemEditPermission(GalleryObject)
          Dim hasUpload As Boolean = GalleryAuthorize.HasItemUploadPermission(GalleryObject)

          If GalleryObject.IsFolder Then
            Dim objAlbum As GalleryFolder = CType(GalleryObject, GalleryFolder)

            If hasEdit Then
              BuildMenuNode(ParentMenuNode, "MenuEdit", AppendCurrentStripParameter(GalleryObject.EditURL, True))
              BuildMenuNode(ParentMenuNode, "MenuAddAlbum", objAlbum.AddSubAlbumURL)
              BuildMenuNode(ParentMenuNode, "MenuMaintenance", AppendCurrentStripParameter(objAlbum.MaintenanceURL, True))
            End If

            If hasUpload Then
              BuildMenuNode(ParentMenuNode, "MenuAddFile", objAlbum.AddFileURL)
            End If
          Else
            If hasEdit Then
              BuildMenuNode(ParentMenuNode, "MenuEditFile", AppendCurrentStripParameter(GalleryObject.EditURL, True))
            End If
          End If

        Catch exc As Exception 'Module failed to load
          LogException(exc)
        End Try
      End If
    End Sub

    Private Sub BuildMenuNode(ByVal ParentMenuNode As MenuNode, ByVal CommandName As String, ByVal URL As String)
      Dim strTitle As String = Localization.GetString(CommandName, GalleryConfig.SharedResourceFile)
      Dim objMenuNode As MenuNode = ParentMenuNode.MenuNodes(ParentMenuNode.MenuNodes.Add())
      With objMenuNode
        .ID = String.Concat(ParentMenuNode.ID, ".", CommandName)
        .Text = strTitle
        .NavigateURL = URL.Replace("~", ApplicationPath)
        .ImageIndex = CType([Enum].Parse(GetType(IconTypes), CommandName), Integer)
        .ToolTip = Localization.GetString(String.Concat(CommandName, ".Tooltip"), GalleryConfig.SharedResourceFile)
      End With
    End Sub

    <Bindable(True), Category("Data"), DefaultValue("0")> Public Property ModuleID() As Integer
      Get
        Dim savedState As Object = ViewState("ModuleID")
        If savedState Is Nothing Then
          Return 0
        Else
          Return CType(savedState, Integer)
        End If
      End Get
      Set(ByVal Value As Integer)
        ViewState("ModuleID") = Value
      End Set
    End Property

    Public ReadOnly Property GalleryConfig() As DotNetNuke.Modules.Gallery.Config
      Get
        If mGalleryConfig Is Nothing Then
          mGalleryConfig = DotNetNuke.Modules.Gallery.Config.GetGalleryConfig(ModuleID)
        End If
        Return mGalleryConfig
      End Get
    End Property

    Public Property GalleryObject() As IGalleryObjectInfo
      Get
        Return mGalleryObject
      End Get
      Set(ByVal Value As IGalleryObjectInfo)
        mGalleryObject = Value
      End Set
    End Property

    Public ReadOnly Property GalleryAuthorize() As DotNetNuke.Modules.Gallery.Authorization
      Get
        If mGalleryAuthorize Is Nothing Then
          mGalleryAuthorize = New DotNetNuke.Modules.Gallery.Authorization(ModuleID)
        End If
        Return mGalleryAuthorize
      End Get
    End Property

  End Class

End Namespace