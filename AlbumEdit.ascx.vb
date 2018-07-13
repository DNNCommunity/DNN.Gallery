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
Imports System.Web.UI.Control
Imports DotNetNuke
Imports DotNetNuke.Security
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Entities.Modules.Actions
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Modules.Gallery.Config
Imports DotNetNuke.Modules.Gallery.Utils
Imports DotNetNuke.Modules.Gallery.WebControls

Namespace DotNetNuke.Modules.Gallery

  ''' -----------------------------------------------------------------------------
  ''' <summary>
  ''' The AlbumEdit Class provides
  ''' </summary>
  ''' <remarks>
  ''' </remarks>
  ''' <history>
  ''' </history>
  ''' -----------------------------------------------------------------------------
  Partial Public MustInherit Class AlbumEdit
    Inherits Entities.Modules.PortalModuleBase
    Implements Entities.Modules.IActionable
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

    End Sub

#End Region

    Private mRequest As GalleryRequest
    Private mGalleryAuthorize As DotNetNuke.Modules.Gallery.Authorization
    Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config
    Private mGalleryAlbum As GalleryFolder
    Private mCurrentStrip As Integer = 1
    Private mUserID As Integer = -1
    Private mAction As String = ""
    Private mIsRootFolder As Boolean

#Region "Public Properties"
    Public Property GalleryAlbum() As GalleryFolder
      Get
        Return mGalleryAlbum
      End Get
      Set(ByVal Value As GalleryFolder)
        mGalleryAlbum = Value
      End Set
    End Property

    Public Property GalleryAuthorize() As DotNetNuke.Modules.Gallery.Authorization
      Get
        Return mGalleryAuthorize
      End Get
      Set(ByVal Value As DotNetNuke.Modules.Gallery.Authorization)
        mGalleryAuthorize = Value
      End Set
    End Property

    Public ReadOnly Property GalleryConfig() As DotNetNuke.Modules.Gallery.Config
      Get
        Return mGalleryConfig
      End Get
    End Property

    Public Property ReturnCtl() As String
      Get
        If Not ViewState("ReturnCtl") Is Nothing Then
          Return CType(ViewState("ReturnCtl"), String)
        Else
          Return "AlbumEdit;"
        End If
      End Get
      Set(ByVal value As String)
        ViewState("ReturnCtl") = value
      End Set
    End Property

    Public ReadOnly Property ReturnURL() As String
      Get
        Dim params As New Generic.List(Of String)
        If Not mRequest.Folder.Parent Is Nothing AndAlso mRequest.Folder.Parent.GalleryHierarchy <> "" Then
          params.Add("path=" & Utils.FriendlyURLEncode(mRequest.Folder.Parent.GalleryHierarchy))
        End If
        If mRequest.CurrentStrip > 0 Then params.Add("currentstrip=" & mRequest.CurrentStrip.ToString)
        Return NavigateURL("", params.ToArray())
      End Get

    End Property
#End Region

#Region "Public Methods"
    'William Severance - Added to permit child controls of album (such as file upload control) to refresh the
    'grid of album's contents
    Public Sub RefreshAlbumContentsGrid()
      If rowAlbumGrid.Visible Then
        ControlAlbum1.BindData()
      End If
    End Sub
#End Region

#Region "Private Methods"
    Private Sub BindData()
      With mGalleryAlbum
        txtName.Text = .Name
        txtTitle.Text = .Title
        txtAuthor.Text = .Author
        txtClient.Text = .Client
        txtLocation.Text = .Location
        txtApprovedDate.Text = Utils.DateToText(.ApprovedDate) 'WES - Show Date.MaxValue As blank
        txtDescription.Text = .Description

        'William Severance - Added to display gallery owner when private gallery and user is Admin
        BindOwnerLookup(.OwnerID)
      End With

      'William Severance - uncommenting this line will cause the Approved Date field and
      'calendar hyperlink to be hidden to all except the 1) Gallery owner, 2) the parent album owner, or
      '3) the module administrators. Leaving them commented keeps the same behavior as in previous
      'versions in which the Approved Date field is visible to all with edit permission on the module.

      'Hans Zassenhaus - Updated to include that the row will only be displayed if the Album configururation
      'does NOT include AutpApproval.
      If mGalleryAuthorize.HasItemApprovalPermission(mGalleryAlbum.Parent) AndAlso Not mGalleryConfig.AutoApproval Then
        rowApprovedDate.Visible = True
      Else
        rowApprovedDate.Visible = False
      End If

    End Sub

    Private Sub BindCategories()

      Dim catList As ArrayList = mGalleryConfig.Categories
      Dim catString As String

      ' Clear existing items in checkboxlist
      lstCategories.Items.Clear()

      For Each catString In catList

        Dim catItem As New ListItem
        catItem.Value = catString
        catItem.Selected = False

        If InStr(1, LCase(mGalleryAlbum.Categories), LCase(catString)) > 0 Then
          catItem.Selected = True
        End If
        'list category for current item
        lstCategories.Items.Add(catItem)
      Next

    End Sub

    'William Severance - Modified to allow specification of per album OwnerID

    Private Sub BindOwnerLookup(ByVal OwnerID As Integer)

      If mGalleryConfig.IsPrivate AndAlso mGalleryAuthorize.HasAdminPermission() Then
        rowOwner.Visible = True

        'William Severance - Modified to use shared method GetUser and to handle possibility of invalid OwnerId

        With ctlOwnerLookup
          .ItemClass = ObjectClass.DNNUser
          .Image = "sp_user.gif"
          .LookupSingle = True
        End With
        If OwnerID <> -1 Then
          Dim uc As New Entities.Users.UserController
          Dim owner As Entities.Users.UserInfo = uc.GetUser(PortalId, ValidUserID(OwnerID))
          If Not owner Is Nothing Then ctlOwnerLookup.AddItem(owner.UserID, owner.Username)
        End If
      Else
        rowOwner.Visible = False
      End If

    End Sub

    Private Sub EnableRootFolderView()
      Me.rowDetails.Visible = False
      Me.rowAlbumGrid.Visible = True
      Me.rowUpload.Visible = False
      Me.rowRefuse.Visible = False
      Me.lblInfo.Text = Services.Localization.Localization.GetString("RootFolderDetails", LocalResourceFile)
      cmdUpdate.Visible = False
      cmdCancel.Visible = True
    End Sub

    Private Sub EnableViewFolder(ByVal Enable As Boolean)
      Me.rowDetails.Visible = Enable
      Me.rowAlbumGrid.Visible = Enable
      Me.rowUpload.Visible = (Not Enable)
      Me.rowRefuse.Visible = (Not Enable)
      Me.lblInfo.Text = Services.Localization.Localization.GetString("AlbumDetails", LocalResourceFile) '"AlbumDetails"
      cmdUpdate.Visible = Enable
      cmdCancel.Visible = True
      BindData()
      BindCategories()
    End Sub

    Private Sub EnableAddFile(ByVal Enable As Boolean)
      rowUpload.Visible = Enable
      Me.rowAlbumGrid.Visible = Enable
      Me.rowDetails.Visible = (Not Enable)
      Me.rowRefuse.Visible = (Not Enable)
      Me.lblInfo.Text = Services.Localization.Localization.GetString("AddFiles", LocalResourceFile) '"Add Files"
      cmdUpdate.Visible = False
      cmdCancel.Visible = False
    End Sub

    Private Sub EnableAddFolder(ByVal Enable As Boolean)
      Me.txtName.Enabled = Enable
      Me.rowDetails.Visible = Enable
      Me.rowUpload.Visible = (Not Enable)
      Me.rowAlbumGrid.Visible = (Not Enable)
      Me.rowRefuse.Visible = (Not Enable)
      Me.lblInfo.Text = Services.Localization.Localization.GetString("AddFolders", LocalResourceFile) '"Add Folders"
      Me.cmdUpdate.Visible = Enable
      Me.cmdCancel.Visible = True
      BindCategories()
      'William Severance - Added to display/select owner when private gallery and user is Admin
      BindOwnerLookup(mGalleryConfig.OwnerID)

    End Sub

    Private Function GetCategories(ByVal List As CheckBoxList) As String
      Dim catItem As ListItem
      ' JIMJ init string
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

#End Region

#Region "Event Handlers"
    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

      ' Load the styles
      DirectCast(Page, CDefault).AddStyleSheet(CreateValidID(GalleryConfig.Css()), GalleryConfig.Css())

      mRequest = New GalleryRequest(ModuleId)

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

      mGalleryAuthorize = New DotNetNuke.Modules.Gallery.Authorization(ModuleConfiguration)
      mGalleryAlbum = mRequest.Folder

      If Not mGalleryAuthorize.IsItemOwner(mGalleryAlbum) Then
        Me.cmdUpdate.Visible = False
        Me.rowDetails.Visible = False
        Me.rowAlbumGrid.Visible = False
        Me.rowUpload.Visible = False
        Me.rowRefuse.Visible = True
        Me.lblRefuse.Text = "<br /><br />" & Localization.GetString("lblRefuse", Me.LocalResourceFile) & "<br /><br />"
        Exit Sub
      End If

      '<tamttt:note if this folder is root folder, no info to be updated
      'because we should do it on settings page
      If mGalleryAlbum.Path = mGalleryConfig.RootFolder.Path Then
        mIsRootFolder = True
      End If

      mUserID = Utils.ValidUserID(DotNetNuke.Entities.Users.UserController.GetCurrentUserInfo.UserID)

      If Not Request.QueryString("action") Is Nothing Then
        mAction = Request.QueryString("action")
      End If

      If Not Request.QueryString("currentstrip") Is Nothing Then
        mCurrentStrip = Int16.Parse(Request.QueryString("currentstrip"))
      End If

      'WES - Added for date range validator
      valApprovedDate.MinimumValue = DateTime.Today.AddYears(-5).ToShortDateString()
      valApprovedDate.MaximumValue = DateTime.Today.AddYears(5).ToShortDateString()

      cmdApprovedDate.NavigateUrl = DotNetNuke.Common.Utilities.Calendar.InvokePopupCal(txtApprovedDate)

      ' Ensure Gallery edit permissions (Andrew Galbraith Ryer)
      If mAction <> "addfile" And Not mGalleryAuthorize.HasItemEditPermission(mGalleryAlbum) Then 'mGalleryAuthorize.HasEditPermission Then
        Response.Redirect(AccessDeniedURL(Localization.GetString("Insufficient_Edit_Permissions", mGalleryConfig.SharedResourceFile)))
      End If

      If Not Page.IsPostBack AndAlso mGalleryConfig.IsValidPath Then
        Select Case mAction.ToLower
          Case "addfile"
            EnableAddFile(True)
          Case "addfolder"
            EnableAddFolder(True)
          Case Else
            If Not mIsRootFolder Then
              EnableViewFolder(True)
            Else
              EnableRootFolderView()
            End If
        End Select
      End If

    End Sub

    Private Sub cmdCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCancel.Click
      ' William Severance - GAL-8353 added to remove cached GalleryUploadCollection upon cancelation from adding files.
      If rowUpload.Visible AndAlso Not galControlUpload Is Nothing Then
        GalleryUploadCollection.ResetList(galControlUpload.FileListCacheKey)
      End If
      Response.Redirect(ReturnURL)
    End Sub

    Private Sub cmdUpdate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdUpdate.Click

      If Page.IsValid Then
        Dim approvedDate As DateTime
        Dim Security As New PortalSecurity()

        If txtApprovedDate.Text.Length > 0 Then
          approvedDate = DateTime.Parse(txtApprovedDate.Text)
        Else
          'William Severance - Modification to auto-approve album creation by Gallery owner,
          'or parent album owner as well as Administrators. Remove comment on first line and
          'comment out second line below to apply change:

          Dim parentAlbum As GalleryFolder
          If mAction.ToLower = "addfolder" Then
            parentAlbum = mGalleryAlbum
          Else
            parentAlbum = mGalleryAlbum.Parent
          End If
          If mGalleryConfig.AutoApproval OrElse mGalleryAuthorize.HasItemApprovalPermission(parentAlbum) Then
            'If mGalleryConfig.AutoApproval Or Authorization.HasAdminPermission() Then
            approvedDate = DateTime.Today
          Else
            approvedDate = DateTime.MaxValue
          End If
        End If

        'GAL-6255
        lblAlbumName.Text = ""

        'William Severance - modified to add PortalSecurity input filter to prevent scripting hacks
        '  also added range validator of type = "Date" to ApprovedDate text box.

        Dim Title As String = Security.InputFilter(txtTitle.Text, PortalSecurity.FilterFlag.NoScripting)
        Dim Author As String = Security.InputFilter(txtAuthor.Text, PortalSecurity.FilterFlag.NoScripting)
        Dim Location As String = Security.InputFilter(txtLocation.Text, PortalSecurity.FilterFlag.NoScripting)
        Dim Client As String = Security.InputFilter(txtClient.Text, PortalSecurity.FilterFlag.NoScripting)
        Dim Description As String = Security.InputFilter(txtDescription.Text, PortalSecurity.FilterFlag.NoScripting)
        Dim OwnerID As Integer

        If rowOwner.Visible Then
          Dim sOwnerID As String = ctlOwnerLookup.ResultItems.Replace(";", "")
          If sOwnerID = String.Empty Then
            OwnerID = mGalleryConfig.OwnerID
          Else
            OwnerID = Integer.Parse(sOwnerID)
          End If
        Else
          OwnerID = mUserID
        End If

        If mAction.ToLower = "addfolder" Then
          If Not mGalleryAlbum.ValidateGalleryName(txtName.Text) Then
            lblAlbumName.Text = Localization.GetString("validateCharacters4txtName.ErrorMessage", Me.LocalResourceFile)
            Return
          End If
          Dim newFolderID As Integer = mGalleryAlbum.CreateChild(txtName.Text, Title, Description, Author, Location, Client, GetCategories(lstCategories), OwnerID, approvedDate)
          Select Case newFolderID
            Case -1 'Unspecified error in creating new child album
              lblAlbumName.Text = Localization.GetString("CreateChildAlbumFailed.ErrorMessage", LocalResourceFile)
            Case -2 'Child album name already exists.
              lblAlbumName.Text = Localization.GetString("DuplicateAlbumName.ErrorMessage", LocalResourceFile)
            Case Else
              Response.Redirect(GetURL(Page.Request.ServerVariables("URL"), Page, "", "action="))
          End Select
        Else
          With mGalleryAlbum
            .Title = Title
            .Author = Author
            .Location = Location
            .Client = Client
            .Description = Description
            .Categories = GetCategories(lstCategories)
            .ApprovedDate = approvedDate
            If rowOwner.Visible Then
              .OwnerID = OwnerID
            End If
          End With
          mGalleryAlbum.Save()
          'William Severance - Added to return to gallery page or maintenance control
          Response.Redirect(ReturnURL)
        End If
      End If

    End Sub

#End Region

#Region "Optional Interfaces"

    Public ReadOnly Property ModuleActions() As ModuleActionCollection Implements Entities.Modules.IActionable.ModuleActions
      'Implements Entities.Modules.IActionable.ModuleActions
      Get
        Dim Actions As New ModuleActionCollection
        Actions.Add(GetNextActionID, Localization.GetString("Configuration.Action", LocalResourceFile), ModuleActionType.ModuleSettings, "", "edit.gif", EditUrl(ControlKey:="configuration"), "", False, SecurityAccessLevel.Admin, True, False)
        Actions.Add(GetNextActionID, Localization.GetString("GalleryHome.Action", LocalResourceFile), ModuleActionType.ContentOptions, "", "icon_moduledefinitions_16px.gif", NavigateURL(), False, SecurityAccessLevel.Edit, True, False)
        Return Actions
      End Get
    End Property

#End Region

  End Class

End Namespace

