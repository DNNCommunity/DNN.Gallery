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

Imports System.IO
Imports DotNetNuke
Imports DotNetNuke.Security
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Entities.Modules.Actions
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.Entities.Users
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Modules.Gallery.Utils
Imports DotNetNuke.Modules.Gallery.Config
Imports DotNetNuke.Modules.Gallery.WebControls

Namespace DotNetNuke.Modules.Gallery

  Public MustInherit Class Maintenance
    Inherits DotNetNuke.Entities.Modules.PortalModuleBase
    Implements DotNetNuke.Entities.Modules.IActionable
    Public lnkViewText As String = ""

#Region "Private Members"
    ' Obtain PortalSettings from Current Context   
    Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
    Private mRequest As GalleryMaintenanceRequest
    Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config
    Private mFolder As GalleryFolder
    Private mGalleryAuthorization As Gallery.Authorization
    Private mSelectAll As Boolean
    Private mReturnCtl As String

    'For Localization of FileInfo text
    Private mLocalizedAlbumText As String
    Private mLocalizedSourceText As String
    Private mLocalizedThumbText As String
    Private mLocalizedMissingText As String
    Private mLocalizedPresentText As String
    Private mLocalizedRebuildThumbText As String
    Private mLocalizedCopySourceToFileText As String
    Private mLocalizedCopyFileToSourceText As String
    Private mYellowWarningImageUrl As String
    Private mGreenOKImageUrl As String

#End Region

#Region " Web Form Designer Generated Code "

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

    End Sub

    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
      'CODEGEN: This method call is required by the Web Form Designer
      'Do not modify it using the code editor.
      InitializeComponent()

      If mGalleryConfig Is Nothing Then
        mGalleryConfig = GetGalleryConfig(ModuleId)
      End If
      'mImageFolder = "../" & mGalleryConfig.ImageFolder.Substring(mGalleryConfig.ImageFolder.IndexOf("/DesktopModules"))

    End Sub

#End Region

#Region "Optional Interfaces"

    Public ReadOnly Property ModuleActions() As ModuleActionCollection Implements Entities.Modules.IActionable.ModuleActions
      Get
        Dim Actions As New ModuleActionCollection
        Actions.Add(GetNextActionID, Localization.GetString("Configuration.Action", LocalResourceFile), ModuleActionType.ModuleSettings, "", "edit.gif", EditUrl(ControlKey:="configuration"), "", False, SecurityAccessLevel.Admin, True, False)
        Actions.Add(GetNextActionID, Localization.GetString("GalleryHome.Action", LocalResourceFile), ModuleActionType.ContentOptions, "", "icon_moduledefinitions_16px.gif", NavigateURL(), False, SecurityAccessLevel.Edit, True, False)
        Return Actions
      End Get
    End Property

#End Region

    Public ReadOnly Property ReturnCtl() As String
      Get
        Return "Maintenance;"
      End Get
    End Property

    Public ReadOnly Property GalleryConfig() As DotNetNuke.Modules.Gallery.Config
      Get
        Return mGalleryConfig
      End Get
    End Property

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

      mRequest = New GalleryMaintenanceRequest(ModuleId)
      mGalleryConfig = mRequest.GalleryConfig
      mFolder = mRequest.Folder

      mGalleryAuthorization = New Authorization(ModuleId)
      If Not mGalleryAuthorization.HasItemEditPermission(mFolder) Then
        Response.Redirect(AccessDeniedURL(Localization.GetString("Insufficient_Maintenance_Permissions", mGalleryConfig.SharedResourceFile)))
      End If

      ' Load the styles
      DirectCast(Page, CDefault).AddStyleSheet(CreateValidID(GalleryConfig.Css()), GalleryConfig.Css())

      lnkViewText = Localization.GetString("lnkView.Text", LocalResourceFile)
      mLocalizedAlbumText = Localization.GetString("Album.Header", LocalResourceFile)
      mLocalizedSourceText = Localization.GetString("Source.Header", LocalResourceFile)
      mLocalizedThumbText = Localization.GetString("Thumb.Header", LocalResourceFile)
      mLocalizedMissingText = Localization.GetString("Missing", LocalResourceFile)
      mLocalizedPresentText = Localization.GetString("Present", LocalResourceFile)
      mLocalizedRebuildThumbText = Localization.GetString("RebuildThumb", LocalResourceFile)

      mYellowWarningImageUrl = "~/DesktopModules/Gallery/Images/yellow-warning.gif"
      mGreenOKImageUrl = "~/DesktopModules/Gallery/Images/green-ok.gif"

      'William Severance - Added condition Not Page.IsPostBack to fix loss of
      'datagrid header localization following postback. LocalizeDataGrid must be called
      'before datagrid is data bound.

      If Not Page.IsPostBack Then Localization.LocalizeDataGrid(grdContent, LocalResourceFile)

      ' Load gallery menu, this method allow call request only once
      Dim galleryMenu As GalleryMenu = CType(LoadControl("Controls/ControlGalleryMenu.ascx"), GalleryMenu)
      With galleryMenu
        .ModuleID = ModuleId
        .GalleryRequest = CType(mRequest, BaseRequest)
      End With
      celGalleryMenu.Controls.Add(galleryMenu)

      ' Load gallery breadcrumbs, this method allow call request only once
      Dim galleryBreadCrumbs As BreadCrumbs = CType(LoadControl("Controls/ControlBreadCrumbs.ascx"), BreadCrumbs)
      With galleryBreadCrumbs
        .ModuleID = ModuleId
        .GalleryRequest = CType(mRequest, BaseRequest)
      End With
      celBreadcrumbs.Controls.Add(galleryBreadCrumbs)

      If Not Page.IsPostBack AndAlso mGalleryConfig.IsValidPath Then
        If Not mFolder.IsPopulated Then
          Response.Redirect(ApplicationURL)
        End If

        BindData()

        btnDeleteAll.Attributes.Add("onClick", "javascript: return confirm('Are you sure you wish to delete selected items?')")

      End If

    End Sub

    Private Sub BindData()

      With mFolder
        txtPath.Text = mFolder.URL
        txtName.Text = .Name
      End With

      lblAlbumInfo.Text = AlbumInfo()
      BindChildItems()

    End Sub

    Protected Function BrowserURL(ByVal DataItem As Object) As String
      Dim item As GalleryMaintenanceFile = CType(DataItem, GalleryMaintenanceFile)
      If mGalleryConfig.AllowPopup Then
        Return item.URL
      Else
        Dim params As New Generic.List(Of String)
        params.Add("returnctl=" & ReturnCtl)
        If Not Request.QueryString("currentstrip") Is Nothing Then params.Add("currentstrip=" & Request.QueryString("currentstrip"))
        Return Utils.AppendURLParameters(item.URL, params.ToArray())
      End If
    End Function

    Protected Function FileInfo(ByVal DataItem As Object) As String
      Dim file As GalleryMaintenanceFile = CType(DataItem, GalleryMaintenanceFile)
      Dim sb As New System.Text.StringBuilder

      If Not file.SourceExists Then sb.Append(mLocalizedSourceText)

      If Not file.ThumbExists Then
        If sb.Length > 0 Then sb.Append(", ")
        sb.Append(mLocalizedThumbText)
      End If

      If Not file.FileExists Then
        If sb.Length > 0 Then sb.Append(", ")
        sb.Append(mLocalizedAlbumText)
      End If

      If sb.Length > 0 Then
        sb.Append(" ")
        sb.Append(mLocalizedMissingText)
      End If

      Return sb.ToString
    End Function

    Protected Function StatusImage(ByVal DataItem As Object, ByVal FileType As String) As String
      Dim file As GalleryMaintenanceFile = CType(DataItem, GalleryMaintenanceFile)
      Dim exists As Boolean = False
      Select Case FileType
        Case "Thumb"
          exists = file.ThumbExists
        Case "Source"
          exists = file.SourceExists
        Case "File"
          exists = file.FileExists
        Case Else
          Throw New ArgumentException("Must be 'Thumb', Source' or 'File'", "FileType")
      End Select
      If exists Then
        Return mGreenOKImageUrl
      Else
        Return mYellowWarningImageUrl
      End If
    End Function

    Protected Function Tooltip(ByVal DataItem As Object, ByVal FileType As String) As String
      Dim file As GalleryMaintenanceFile = CType(DataItem, GalleryMaintenanceFile)
      Dim localizedTooltip As String = mLocalizedPresentText

      Select Case FileType
        Case "Thumb"
          If Not file.ThumbExists Then localizedTooltip = mLocalizedRebuildThumbText
        Case "Source"
          If Not file.SourceExists Then localizedTooltip = mLocalizedCopyFileToSourceText
        Case "File"
          If Not file.FileExists Then localizedTooltip = mLocalizedCopySourceToFileText
        Case Else
          Throw New ArgumentException("Must be 'Thumb', Source' or 'File'", "FileType")
      End Select
      Return localizedTooltip
    End Function

    Protected Function AlbumInfo() As String
      Dim itemCount As Integer = mRequest.ImageList.Count
      Dim sourceCount As Integer = 0
      Dim albumCount As Integer = 0
      Dim thumbCount As Integer = 0
      Dim sb As New System.Text.StringBuilder

      For Each file As GalleryMaintenanceFile In mRequest.ImageList
        If file.SourceExists Then sourceCount += 1
        If file.FileExists Then albumCount += 1
        If file.ThumbExists Then thumbCount += 1
      Next

      sb.Append(Localization.GetString("AlbumInfo", LocalResourceFile))
      sb.Replace("[ItemCount]", itemCount.ToString)
      sb.Replace("[AlbumName]", mRequest.Folder.Name)
      sb.Replace("[ItemNoSource]", (itemCount - sourceCount).ToString)
      sb.Replace("[ItemNoAlbum]", (itemCount - albumCount).ToString)
      sb.Replace("[ItemNoThumb]", (itemCount - thumbCount).ToString)
      sb.Replace("[ImageSize]", mGalleryConfig.FixedHeight.ToString & " x " & mGalleryConfig.FixedWidth.ToString)
      sb.Replace("[ThumbSize]", mGalleryConfig.MaximumThumbHeight.ToString & " x " & mGalleryConfig.MaximumThumbWidth.ToString)

      Return sb.ToString
    End Function

    Private Sub BindChildItems()

      grdContent.DataSource = mRequest.ImageList
      grdContent.PageSize = (mRequest.ImageList.Count + 1)
      grdContent.DataBind()

    End Sub

    Private Sub grdContent_ItemCommand(ByVal source As Object, ByVal e As System.Web.UI.WebControls.DataGridCommandEventArgs) Handles grdContent.ItemCommand

      Dim itemIndex As Integer = e.Item.ItemIndex
      Dim file As GalleryMaintenanceFile = CType(mRequest.ImageList.Item(itemIndex), GalleryMaintenanceFile)

      Select Case (CType(e.CommandSource, ImageButton).CommandName)
        Case "Delete"
          file.DeleteAll()
        Case "RebuildThumb"
          file.RebuildThumbnail()
        Case "CopySourceToFile"
          file.CreateFileFromSource()
        Case "CopyFileToSource"
          file.CreateFileFromSource()
        Case Else
          Throw New ArgumentException("Invalid maintenance command", "CommandName")
      End Select

      mRequest.Folder.Populate(True)
      BindData()

    End Sub

    Private Sub cmdReturn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdReturn.Click
      Config.ResetGalleryConfig(ModuleId)
      Goback()
    End Sub

    Private Sub Goback()
      Dim url As String = GetURL(Page.Request.ServerVariables("URL"), Page, "", "ctl=&selectall=&mid=&currentitem=&media=")
      Response.Redirect(url)
    End Sub

    Private Function GetSelectedFiles() As List(Of GalleryMaintenanceFile)
      Dim i As Integer
      Dim selList As New List(Of GalleryMaintenanceFile)

      For i = 0 To grdContent.Items.Count - 1
        Dim myListItem As DataGridItem = grdContent.Items(i)
        Dim myCheck As CheckBox = DirectCast(myListItem.FindControl("chkSelect"), CheckBox)
        If myCheck.Checked Then selList.Add(mRequest.ImageList(i))
      Next
      Return selList

    End Function

    Private Sub RefreshAlbum()

      mRequest.Folder.Clear() ' refresh gallery folder first ready for repopulate
      mRequest.Populate()
      BindData()

    End Sub

    Private Sub btnCopySource_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs)
      For Each file As GalleryMaintenanceFile In GetSelectedFiles()
        file.CreateFileFromSource()
      Next

      RefreshAlbum()

    End Sub

    Private Sub btnCopyFile_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs)
      For Each file As GalleryMaintenanceFile In GetSelectedFiles()
        file.CreateSourceFromFile()
      Next

      RefreshAlbum()

    End Sub

    Private Sub btnCreateThumb_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs)
      For Each file As GalleryMaintenanceFile In GetSelectedFiles()
        file.RebuildThumbnail()
      Next

      RefreshAlbum()

    End Sub

    Private Sub ClearCache1_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles ClearCache1.Click
      Config.ResetGalleryConfig(ModuleId)
    End Sub

    Private Overloads Sub btnSyncAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSyncAll.Click
      For Each file As GalleryMaintenanceFile In GetSelectedFiles()
        file.Synchronize()
      Next

      RefreshAlbum()
    End Sub

    Private Overloads Sub btnDeleteAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDeleteAll.Click
      Dim ps As PortalSettings = PortalController.GetCurrentPortalSettings()
      For Each gmFile As GalleryMaintenanceFile In GetSelectedFiles()
        Try
          If File.Exists(gmFile.AlbumPath) Then mFolder.DeleteFile(gmFile.AlbumPath, ps, True)
          If gmFile.Type = Config.ItemType.Image Then
            If File.Exists(gmFile.ThumbPath) Then mFolder.DeleteFile(gmFile.ThumbPath, ps, True)
            If File.Exists(gmFile.SourcePath) Then mFolder.DeleteFile(gmFile.SourcePath, ps, True)

            'Reset folder thumbnail to "folder.gif" if its current
            'thumbnail is being deleted. Will then get set to next image thumbnail during populate if there is one.
            If mFolder.Thumbnail = gmFile.Name Then mFolder.ResetThumbnail()
          End If

          GalleryXML.DeleteMetaData(mFolder.Path, gmFile.Name)
        Catch

        End Try
      Next

      RefreshAlbum()
    End Sub

  End Class

End Namespace

