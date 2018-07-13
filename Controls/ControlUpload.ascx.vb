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
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.UI
Imports DotNetNuke.UI.Utilities.DNNClientAPI

Namespace DotNetNuke.Modules.Gallery.WebControls

  Public MustInherit Class Upload
    Inherits GalleryWebControlBase ' WES Refactored to inherit from new base class not System.Web.UI.UserControl

#Region " Web Form Designer Generated Code "

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

    End Sub

    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
      'CODEGEN: This method call is required by the Web Form Designer
      'Do not modify it using the code editor.
      InitializeComponent()
    End Sub

#End Region

    Private mUploadCollection As GalleryUploadCollection = Nothing
    Private mAlbumEdit As DotNetNuke.Modules.Gallery.AlbumEdit
    Private mAlbumObject As DotNetNuke.Modules.Gallery.GalleryFolder
    Private strFolderInfo As String
    Private strTotalSize As String
    Private mEnablePendingForUnauthenticatedUsers As Boolean = True

    Private Const PendingFileListCacheKeyCookieName As String = "PendingFileListCacheKey"
    Private Const minIcon As String = "images/minus.gif"
    Private Const maxIcon As String = "images/plus.gif"

    Dim guidValidationRgx As New Regex("^(\{)?[0-9a-fA-F]{8}\-([0-9a-fA-F]{4}\-){3}[0-9a-fA-F]{12}(\})?$")


#Region "Event Handlers"
    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

      If Not GalleryAuthorization.HasItemUploadPermission(AlbumObject) Then
        Response.Redirect(Common.AccessDeniedURL(HttpUtility.UrlEncode(Localization.GetString("Insufficient_Upload_Permissions", GalleryConfig.SharedResourceFile))), True)
      End If

      If Not AlbumObject.IsPopulated Then
        Response.Redirect(NavigateURL)
      Else
        AddHandler AlbumObject.GalleryObjectDeleted, AddressOf AlbumObject_Deleted
      End If

      If Not IsPostBack Then
        trPendingUploads.Visible = Request.IsAuthenticated OrElse mEnablePendingForUnauthenticatedUsers

        If HasPendingFiles Then
          PendingFiles.Clear()
          Skins.Skin.AddModuleMessage(ParentContainer, Localization.GetString("GalleryUploadsCollectionCache_Cleared", LocalResourceFile), Skins.Controls.ModuleMessage.ModuleMessageType.YellowWarning)
        End If
        ShowInstructions("Instructions.Text")
        If GalleryConfig.IsValidPath Then
          ShowInfo()
          BindCategories()
          Localization.LocalizeDataGrid(grdUpload, LocalResourceFile)
          lblUpload.Text = Localization.GetString("No_Uploads_Pending", LocalResourceFile)
        Else
          'TODO: do something if not IsValidPath
        End If
      End If
    End Sub

    ''' <summary>
    ''' Add the file to the list of files we want to upload
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub cmdAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdAdd.Click

      Dim Security As New PortalSecurity()
      Dim ErrorText As New StringBuilder

      ' Get the individual file we want to upload
      Dim UploadFile As New FileToUpload(htmlUploadFile)

      ' Set properties on the file to upload
      ' William Severance - modified to add PortalSecurity input filter to prevent scripting hacks
      With UploadFile
        .ModuleID = ModuleId
        .Title = Security.InputFilter(txtTitle.Text, PortalSecurity.FilterFlag.NoScripting)
        .Author = Security.InputFilter(txtAuthor.Text, PortalSecurity.FilterFlag.NoScripting)
        .Client = Security.InputFilter(txtClient.Text, PortalSecurity.FilterFlag.NoScripting)
        .Location = Security.InputFilter(txtLocation.Text, PortalSecurity.FilterFlag.NoScripting)
        .Description = Security.InputFilter(txtDescription.Text, PortalSecurity.FilterFlag.NoScripting)
        .Categories = Security.InputFilter(GetCategories(lstCategories), PortalSecurity.FilterFlag.NoScripting)
        .OwnerID = UserId
        .GalleryConfig = GalleryConfig
      End With

      ' Check file valid size & type
      Dim validationInfo As String = UploadFile.ValidateFile()
      If Len(validationInfo) > 0 Then
        ' Something wrong with basics of file.. show user
        ErrorText.AppendLine(validationInfo)
        ClearFields()
      Else
        If PendingFiles.ExceedsQuota(UploadFile.ContentLength) Then
          ErrorText.AppendFormat(Localization.GetString("Would_Exceed_Quota", LocalResourceFile) & vbCrLf, (UploadFile.ContentLength - PendingFiles.SpaceAvailable) / 1024)
        End If
        If PendingFiles.FileExists(UploadFile.FileName) Then
          ErrorText.AppendLine(Localization.GetString("DuplicateFile", Me.LocalResourceFile))
        End If
      End If

      If ErrorText.Length > 0 Then
        divFileError.InnerHtml = "<br />" & ErrorText.Replace(vbCrLf, "<br />").ToString
      Else
        divFileError.InnerHtml = ""
        PendingFiles.AddFileToUpload(UploadFile)
        If Request.IsAuthenticated OrElse mEnablePendingForUnauthenticatedUsers Then
          BindUploadCollection()
        Else
          PerformUploads()
        End If
        ClearFields()
      End If
    End Sub

    ''' <summary>
    ''' User wants to remove a file from the grid of uploadable files
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub grdUpload_ItemCommand(ByVal source As Object, ByVal e As System.Web.UI.WebControls.DataGridCommandEventArgs) Handles grdUpload.ItemCommand

      Dim itemIndex As Integer = e.Item.ItemIndex
      Select Case (CType(e.CommandSource, ImageButton).CommandName)
        Case "delete"
          ' Remove file from list
          PendingFiles.RemoveFileToUpload(itemIndex)
          BindUploadCollection()
        Case "edit"
          ' Not implemented in this version
      End Select
    End Sub

    ''' <summary>
    ''' User wants to upload the file(s)
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub btnFileUpload_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFileUpload.Click
      PerformUploads()
    End Sub

    Protected Sub cmdReturnCancel_Click(ByVal sender As Object, ByVal e As EventArgs) Handles cmdReturnCancel.Click
      If cmdReturnCancel.CommandName = "Cancel" Then GalleryUploadCollection.ResetList(FileListCacheKey)
      Response.Redirect(ParentContainer.ReturnURL)
    End Sub

    Private Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
      SetNavigationControls(HasPendingFiles)
    End Sub

    Private Sub grdUpload_ItemCreated(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.DataGridItemEventArgs) Handles grdUpload.ItemCreated
      If e.Item.ItemType = ListItemType.Footer Then
        e.Item.Cells(0).ColumnSpan = 4
        For i As Integer = 1 To 3
          e.Item.Cells.RemoveAt(1) 'remove next 3 cells
        Next
        e.Item.Cells(0).Text = Localization.GetString("Total_Size", LocalResourceFile)
        e.Item.Cells(0).HorizontalAlign = HorizontalAlign.Right
        e.Item.Cells(1).HorizontalAlign = HorizontalAlign.Center
      End If

    End Sub

    Private Sub grdUpload_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.DataGridItemEventArgs) Handles grdUpload.ItemDataBound
      If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Then
        Dim f As FileToUpload = CType(e.Item.DataItem, FileToUpload)
        Dim btnExpandZipDetails As Control = e.Item.FindControl("btnExpandZipDetails")
        If f.Extension = ".zip" Then
          Dim grdZipDetails As DataGrid = CType(e.Item.FindControl("grdZipDetails"), DataGrid)
          EnableMinMax(btnExpandZipDetails, grdZipDetails, False, Page.ResolveUrl(minIcon), Page.ResolveUrl(maxIcon), MinMaxPersistanceType.Page)
          grdZipDetails.Style.Add("padding-top", "12px")
          Localization.LocalizeDataGrid(grdZipDetails, LocalResourceFile)
          grdZipDetails.DataSource = f.ZipHeaders.Entries
          grdZipDetails.DataBind()
        Else
          btnExpandZipDetails.Visible = False
        End If
      ElseIf e.Item.ItemType = ListItemType.Footer Then
        e.Item.Cells(1).Text = String.Format("{0:F}", PendingFiles.Size / 1024)
      End If
    End Sub

    Private Sub AlbumObject_Deleted(ByVal sender As Object, ByVal e As GalleryObjectEventArgs)
      If CType(sender, GalleryFolder).Name = AlbumObject.Name AndAlso e.UserID = UserId Then ShowInfo(True) 'Update the gallery space display
    End Sub

#End Region

#Region "Private Methods"

    Private Sub ClearFields()
      txtTitle.Text = String.Empty
      txtAuthor.Text = String.Empty
      txtClient.Text = String.Empty
      txtLocation.Text = String.Empty
      txtDescription.Text = String.Empty
      lstCategories.ClearSelection()
    End Sub

    Private Sub PerformUploads()
      divFileError.InnerHtml = ""
      Dim ErrorMessage As New StringBuilder

      Try
        ' Do the uploads
        PendingFiles.DoUpload()

        ' Get any non-serious errors
        If PendingFiles.ErrMessage.Length > 0 Then
          ErrorMessage.AppendLine(PendingFiles.ErrMessage)
        End If
      Catch exc As Exception
        LogException(exc)
        ' Add some logging to the screen 
        'ErrorMessage.AppendLine(exc.Message) - removed to eliminate exposure of exc details
      End Try

      If ErrorMessage.Length > 0 Then
        ' Add text to the error label on screen
        ErrorMessage.Insert(0, "<br />" & Localization.GetString("Upload_Error", LocalResourceFile))
        divFileError.InnerHtml = ErrorMessage.Replace(vbCrLf, "<br />").ToString
      Else
        If GalleryConfig.AutoApproval Or GalleryAuthorization.HasAdminPermission Then
          Skins.Skin.AddModuleMessage(ParentContainer, Localization.GetString("Upload_Success", LocalResourceFile), Skins.Controls.ModuleMessage.ModuleMessageType.GreenSuccess)
        Else
          Skins.Skin.AddModuleMessage(ParentContainer, Localization.GetString("Upload_Success_Pending_Approval", LocalResourceFile), Skins.Controls.ModuleMessage.ModuleMessageType.YellowWarning)
        End If
        ' Reset the screen
        GalleryUploadCollection.ResetList(FileListCacheKey)
        BindUploadCollection()      'Clear old file list and reset visibility of pending uploads
        Config.ResetGalleryFolder(AlbumObject)

        ParentContainer.RefreshAlbumContentsGrid() 'Update album contents grid in parent control
        ShowInfo(True) 'Update available space
      End If
    End Sub

    'William Severance - Added method to hide/show ControlGallery Menu and Bread Crumbs in
    'parent album edit control

    Private Sub SetNavigationControls(ByVal HasPendingFiles As Boolean)
      Dim celGalleryMenu As Control = ParentContainer.FindControl("celGalleryMenu")
      Dim celBreadcrumbs As Control = ParentContainer.FindControl("celBreadcrumbs")
      If celGalleryMenu IsNot Nothing Then celGalleryMenu.Controls(0).Visible = Not HasPendingFiles
      If celBreadcrumbs IsNot Nothing Then CType(celBreadcrumbs.Controls(0), BreadCrumbs).Enable(Not HasPendingFiles)

      If HasPendingFiles Then
        cmdReturnCancel.CommandName = "Cancel"
        cmdReturnCancel.Text = Localization.GetString("cmdCancel", LocalResourceFile)
        DotNetNuke.UI.Utilities.ClientAPI.AddButtonConfirm(cmdReturnCancel, Localization.GetString("Confirm_Cancel", LocalResourceFile))
      Else
        cmdReturnCancel.CommandArgument = "Return"
        cmdReturnCancel.Text = Localization.GetString("cmdReturn", LocalResourceFile)
        cmdReturnCancel.Attributes.Remove("onClick")
      End If

    End Sub

    'William Severance - Added method to simplify databinding of upload collection and
    'change of control visibility depending on mUploadCollection.Count
    Private Sub BindUploadCollection()
      If Request.IsAuthenticated OrElse mEnablePendingForUnauthenticatedUsers Then
        grdUpload.DataSource = PendingFiles
        grdUpload.DataBind()
        Dim hasPending As Boolean = HasPendingFiles
        grdUpload.Visible = hasPending
        lblUpload.Visible = Not hasPending
        btnFileUpload.Visible = hasPending
        If GalleryConfig.MaxPendingUploadsSize > 0 Then
          If PendingFiles.Size > (GalleryConfig.MaxPendingUploadsSize * 1024) Then
            cmdAdd.Enabled = False
            divFileError.InnerHtml = "<br />" & Localization.GetString("MaxPendingUploadsSize_Exceeded", LocalResourceFile)
          Else
            cmdAdd.Enabled = True
            divFileError.InnerHtml = ""
          End If
        End If
      End If
    End Sub

    Private Sub BindCategories()

      Dim catList As ArrayList = GalleryConfig.Categories
      Dim catString As String

      ' Clear existing items in checkboxlist
      lstCategories.Items.Clear()

      For Each catString In catList

        Dim catItem As New ListItem
        catItem.Value = catString
        catItem.Selected = False

        If InStr(1, LCase(mAlbumObject.Categories), LCase(catString)) > 0 Then
          catItem.Selected = True
        End If
        'list category for current item
        lstCategories.Items.Add(catItem)

      Next
    End Sub

    Private Sub ShowInfo()
      ShowInfo(False)
    End Sub

    Private Sub ShowInfo(ByVal Refresh As Boolean)
      Dim ps As PortalSettings = PortalController.GetCurrentPortalSettings()
      Dim spaceAvailable As Long
      If GalleryConfig.Quota = 0 AndAlso ps.HostSpace = 0 Then
        spaceAvailable = Long.MinValue
      Else
        If Refresh Then PendingFiles.RefreshSpaceConstraints()
        spaceAvailable = PendingFiles.SpaceAvailable
        If spaceAvailable <= 0 Then
          cmdAdd.Enabled = False
          divFileError.InnerHtml = "<br />" & Localization.GetString("No_Available_Space", LocalResourceFile)
        Else
          cmdAdd.Enabled = True
          divFileError.InnerHtml = ""
        End If
      End If
      tdInfo.InnerHtml = Utils.UploadFileInfo(ModuleId, PendingFiles.GallerySpaceUsed, spaceAvailable)
    End Sub

    Private Function GetCategories(ByVal List As CheckBoxList) As String
      Dim catItem As ListItem
      ' JIMJ Init string as we concat to it later
      Dim catString As String = String.Empty

      For Each catItem In List.Items
        If catItem.Selected Then
          catString += catItem.Value & ";"
        End If
      Next

      If Len(catString) > 0 Then
        Return catString.TrimEnd(";"c)
      Else
        Return ""
      End If

    End Function

    Private Overloads Sub ShowInstructions(ByVal LocalizationKey As String)
      ShowInstructions(LocalizationKey, False, True)
    End Sub

    Private Overloads Sub ShowInstructions(ByVal LocalizationKey As String, ByVal IsError As Boolean, ByVal IsExpanded As Boolean)
      scnInstructions.IsExpanded = IsError OrElse IsExpanded
      tdInstructions.InnerHtml = Localization.GetString(LocalizationKey, LocalResourceFile)
      If IsError Then
        tdInstructions.Style.Add("color", "red")
      End If
    End Sub

#End Region

#Region "Public Properties"

    'William Severance - Added lazy loading properties
    Public ReadOnly Property ParentContainer() As DotNetNuke.Modules.Gallery.AlbumEdit
      Get
        If mAlbumEdit Is Nothing Then
          mAlbumEdit = CType(NamingContainer, DotNetNuke.Modules.Gallery.AlbumEdit)
        End If
        Return mAlbumEdit
      End Get
    End Property

    Public ReadOnly Property AlbumObject() As GalleryFolder
      Get
        If mAlbumObject Is Nothing Then
          mAlbumObject = ParentContainer.GalleryAlbum
        End If
        Return mAlbumObject
      End Get
    End Property

    Public ReadOnly Property PendingFiles() As GalleryUploadCollection
      Get
        If mUploadCollection Is Nothing Then
          mUploadCollection = GalleryUploadCollection.GetList(AlbumObject, ModuleId, FileListCacheKey)
        End If
        Return mUploadCollection
      End Get
    End Property

    Public ReadOnly Property HasPendingFiles() As Boolean
      Get
        Return (Request.IsAuthenticated OrElse mEnablePendingForUnauthenticatedUsers) AndAlso PendingFiles.Count > 0
      End Get
    End Property

    Public ReadOnly Property FileListCacheKey() As String
      Get
        Dim strGuid As String = Nothing
        Dim cookie As HttpCookie = HttpContext.Current.Request.Cookies(PendingFileListCacheKeyCookieName)
        ' WES modified to include encryption of cookie and GUID pattern match validation
        If cookie IsNot Nothing Then
          ' Note that EncryptParameter will UrlEncode the parameter so must first UrlDecode
          strGuid = UrlUtils.DecryptParameter(HttpUtility.UrlDecode(cookie.Value))
          If Not guidValidationRgx.IsMatch(strGuid) Then
            Dim msg As String = String.Format("Invalid PendingFileListCacheKey GUID ({0}) read from cookie.", HttpUtility.HtmlEncode(strGuid))
            LogException(New DotNetNuke.Services.Exceptions.SecurityException(msg))
            strGuid = Nothing
          End If
        End If
        If strGuid Is Nothing Then
          strGuid = Guid.NewGuid().ToString
          cookie = New HttpCookie(PendingFileListCacheKeyCookieName, UrlUtils.EncryptParameter(strGuid))
          HttpContext.Current.Response.AppendCookie(cookie)
        End If
        Return String.Format("DNN_Gallery|{0}:{1}", ModuleId, strGuid)
      End Get
    End Property

#End Region

    Private Sub Page_Unload(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Unload
      If Not AlbumObject Is Nothing Then RemoveHandler AlbumObject.GalleryObjectDeleted, AddressOf AlbumObject_Deleted
    End Sub
  End Class

End Namespace

