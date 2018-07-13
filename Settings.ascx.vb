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
Imports System
Imports DotNetNuke.Security.Roles
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Entities.Users
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.Services.Exceptions
Imports DotNetNuke.Modules.Gallery.Config
Imports DotNetNuke.Modules.Gallery.Utils
Imports DotNetNuke.Services.Localization
Imports System.Web.Configuration

Namespace DotNetNuke.Modules.Gallery

  Partial Class Settings
    Inherits DotNetNuke.Entities.Modules.PortalModuleBase

    Private _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
    Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config

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

#Region "Private Methods"

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

      ' Load the styles
      DirectCast(Page, CDefault).AddStyleSheet(CreateValidID(GalleryConfig.Css()), GalleryConfig.Css())

      'William Severance - added to handle changes of IsPrivate check box/MaxFileSize textbox clientside
      chkPrivate.Attributes.Add("onclick", "javascript:onPrivateChanged(this);")
      txtMaxFileSize.Attributes.Add("onchange", "javascript:onMaxFileSizeChanged(this);")

      If Not Page.IsPostBack Then
        ' Ensure Gallery edit permissions (Andrew Galbraith Ryer)
        Dim galleryAuthorization As Authorization = New Authorization(ModuleConfiguration)
        If Not galleryAuthorization.HasEditPermission Then
          Response.Redirect(AccessDeniedURL(Localization.GetString("Insufficient_Maintenance_Permissions", mGalleryConfig.SharedResourceFile)))
        End If

        rowAdmin.Visible = galleryAuthorization.HasAdminPermission

        ' GAL-9527. RootURL will now be relative to portal home directory not website root and modifiable only by administrators role
        lblHomeDirectory.Text = PortalSettings.HomeDirectory
        RootURL.Text = mGalleryConfig.RootURL.Substring(PortalSettings.HomeDirectory.Length)

        'William Severance - Added primarily to test use of InvariantCulture to save/recall date strings from ModuleSettings
        txtCreatedDate.Text = mGalleryConfig.CreatedDate.ToShortDateString()
        txtGalleryTitle.Text = mGalleryConfig.GalleryTitle
        txtDescription.Text = mGalleryConfig.GalleryDescription
        lblImageExtensions.Text = mGalleryConfig.FileExtensions
        lblMediaExtensions.Text = mGalleryConfig.MovieExtensions
        txtCategoryValues.Text = mGalleryConfig.CategoryValues
        txtStripWidth.Text = mGalleryConfig.StripWidth.ToString
        txtStripHeight.Text = mGalleryConfig.StripHeight.ToString
        txtMaxFileSize.Text = mGalleryConfig.MaxFileSize.ToString
        txtQuota.Text = mGalleryConfig.Quota.ToString
        txtMaxPendingUploadsSize.Text = mGalleryConfig.MaxPendingUploadsSize.ToString

        txtMaxThumbWidth.Text = mGalleryConfig.MaximumThumbWidth.ToString
        ' GAL - 8216, disabling control if any album or file added already - Surjeet Gill 08.19.2008
        If (Not IsNothing(mGalleryConfig.RootFolder)) AndAlso mGalleryConfig.RootFolder.List.Count > 0 Then
          txtMaxThumbWidth.Enabled = False
        End If

        txtMaxThumbHeight.Text = mGalleryConfig.MaximumThumbHeight.ToString
        ' GAL - 8216, disabling control if any album or file added already - Surjeet Gill 08.19.2008
        If (Not IsNothing(mGalleryConfig.RootFolder)) AndAlso mGalleryConfig.RootFolder.List.Count > 0 Then
          txtMaxThumbHeight.Enabled = False
        End If

        chkBuildCacheOnStart.Checked = (mGalleryConfig.BuildCacheonStart = True)

        txtFixedWidth.Text = mGalleryConfig.FixedWidth.ToString
        ' GAL - 8216, disabling control if any album or file added already - Surjeet Gill 08.19.2008
        If (Not IsNothing(mGalleryConfig.RootFolder)) AndAlso mGalleryConfig.RootFolder.List.Count > 0 Then
          txtFixedWidth.Enabled = False
        End If

        txtFixedHeight.Text = mGalleryConfig.FixedHeight.ToString
        ' GAL - 8216, disabling control if any album or file added already - Surjeet Gill 08.19.2008
        If (Not IsNothing(mGalleryConfig.RootFolder)) AndAlso mGalleryConfig.RootFolder.List.Count > 0 Then
          txtFixedHeight.Enabled = False
        End If

        txtEncoderQuality.Text = mGalleryConfig.EncoderQuality.ToString
        txtSlideshowSpeed.Text = mGalleryConfig.SlideshowSpeed.ToString

        'William Severance - added to handle change of IsPrivate checkbox clientside
        If mGalleryConfig.IsPrivate Then
          chkPrivate.Checked = True
          rowOwner.Style.Add("display", "")
        Else
          chkPrivate.Checked = False
          rowOwner.Style.Add("display", "none")
        End If

        chkMultiLevelMenu.Checked = mGalleryConfig.MultiLevelMenu
        chkSlideshow.Checked = mGalleryConfig.AllowSlideshow
        chkPopup.Checked = mGalleryConfig.AllowPopup
        chkDownload.Checked = mGalleryConfig.AllowDownload
        chkWatermark.Checked = mGalleryConfig.UseWatermark
        chkAutoApproval.Checked = mGalleryConfig.AutoApproval
        chkVoting.Checked = mGalleryConfig.AllowVoting
        chkExif.Checked = mGalleryConfig.AllowExif

        BindUserLookup()
        BindDisplayList()
        BindGalleryView()
        BindGallerySort()
        BindDownloadRoles()
        BindThemes()
        BindDefaultFolders()

      End If

    End Sub

    Private Sub BindDisplayList()

      'William Severance (9-1-2010) - Refactored to use bitmapped TextDisplayOptions rather than array of strings

      Dim li As ListItem
      Dim key As String
      Dim galleryDisplayOptionType As Type = GetType(Config.GalleryDisplayOption)
      Dim values As Config.GalleryDisplayOption() = DirectCast([Enum].GetValues(galleryDisplayOptionType), Config.GalleryDisplayOption())

      lstDisplay.Items.Clear()
      For Each value As Config.GalleryDisplayOption In [Enum].GetValues(galleryDisplayOptionType)
        key = value.ToString
        li = New ListItem(Localization.GetString(key, mGalleryConfig.SharedResourceFile), key)
        li.Selected = ((mGalleryConfig.TextDisplayOptions And value) <> 0)
        lstDisplay.Items.Add(li)
      Next
    End Sub

    Private Sub BindGalleryView()
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
        If Not .Items.FindByValue(mGalleryConfig.DefaultView.ToString) Is Nothing Then
          .Items.FindByValue(mGalleryConfig.DefaultView.ToString).Selected = True
        End If
      End With
      Me.chkChangeView.Checked = mGalleryConfig.AllowChangeView

    End Sub

    Private Sub BindGallerySort()
      Dim dict As New System.Collections.Specialized.ListDictionary
      For Each name As String In [Enum].GetNames(GetType(Config.GallerySort))
        dict.Add(name, Localization.GetString("Sort" + name, mGalleryConfig.SharedResourceFile))
      Next

      lstSortProperties.ClearSelection()
      lstSortProperties.DataTextField = "value"
      lstSortProperties.DataValueField = "key"
      lstSortProperties.DataSource = dict
      lstSortProperties.DataBind()

      Dim item As ListItem
      For Each item In lstSortProperties.Items
        If mGalleryConfig.SortProperties.Contains(item.Value.ToString) Then
          item.Selected = True
        End If
      Next

      With ddlGallerySort
        .ClearSelection()
        .DataTextField = "value"
        .DataValueField = "key"
        .DataSource = dict
        .DataBind()
        If Not .Items.FindByValue(mGalleryConfig.DefaultSort.ToString) Is Nothing Then
          .Items.FindByValue(mGalleryConfig.DefaultSort.ToString).Selected = True
        End If
      End With

      chkDESC.Checked = mGalleryConfig.DefaultSortDESC

    End Sub

    'William Severance - modified to use shared method GetUser and to handle possibility of invalid OwnerId

    Private Sub BindUserLookup()
      With ctlOwnerLookup
        .ItemClass = ObjectClass.DNNUser
        .Image = "sp_user.gif"
        .LookupSingle = True
      End With
      If mGalleryConfig.OwnerID <> -1 Then
        Dim owner As UserInfo = New UserController().GetUser(PortalId, ValidUserID(mGalleryConfig.OwnerID))
        If Not owner Is Nothing Then ctlOwnerLookup.AddItem(owner.UserID, owner.Username)
      End If
    End Sub

    Private Sub BindDownloadRoles()
      Dim role As String

      With ctlDownloadRoles
        .ItemClass = ObjectClass.DNNRole
        .Locations = ""
        .Image = "sp_team.gif"
        For Each role In mGalleryConfig.DownloadRoles.Split(";"c)
          If Not role.Length = 0 Then

            Dim RoleName As String
            Select Case CInt(role)
              ' JIMJ add all users role
              Case CInt(glbRoleAllUsers)
                RoleName = glbRoleAllUsersName

              Case Else
                Dim ctlRole As New RoleController
                Dim objRole As RoleInfo = ctlRole.GetRole(Int16.Parse(role), PortalId)

                RoleName = objRole.RoleName
            End Select

            .AddItem(CInt(role), RoleName)
          End If
        Next
      End With

    End Sub

    Private Sub BindThemes()
      Dim themes() As String
      Dim Theme As String
      Dim ThemesPath As String = IO.Path.Combine(Server.MapPath(TemplateSourceDirectory), "Themes")
      themes = Directory.GetDirectories(ThemesPath)

      ddlSkins.Items.Clear()
      For Each Theme In themes
        Dim themeItem As New ListItem
        With themeItem
          .Text = IO.Path.GetFileName(Theme)
          .Value = IO.Path.GetFullPath(Theme)
        End With
        ddlSkins.Items.Add(themeItem)
      Next

      ddlSkins.Items.FindByText(mGalleryConfig.Theme).Selected = True

    End Sub

    Private Sub BindDefaultFolders()
      Dim folders As List(Of GalleryFolder) = Utils.GetChildFolders(mGalleryConfig.RootFolder, Integer.MaxValue)
      ddlDefaultAlbum.Items.Clear()
      ddlDefaultAlbum.Items.Add(New ListItem(String.Format(Localization.GetString("RootAlbum", LocalResourceFile), mGalleryConfig.GalleryTitle), ""))
      For Each folder As GalleryFolder In folders
        ddlDefaultAlbum.Items.Add(folder.GalleryHierarchy)
      Next
      Dim li As ListItem = ddlDefaultAlbum.Items.FindByValue(mGalleryConfig.InitialFolder.GalleryHierarchy)
      If li Is Nothing Then
        ddlDefaultAlbum.SelectedIndex = 0       'In case where previously set default album has been deleted
      Else
        ddlDefaultAlbum.SelectedIndex = ddlDefaultAlbum.Items.IndexOf(li)
      End If
    End Sub

    Private Sub cmdSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSave.Click

      ' WES - Added to determine if DefaultView, AllowChangeView, DefaultSort, or DefaultSortDESC have changed so
      ' that cookies of user making the change are refreshed.

      Dim oldDefaultView As Config.GalleryView = mGalleryConfig.DefaultView 'Utils.GetView(mGalleryConfig)
      Dim oldAllowChangeView As Boolean = mGalleryConfig.AllowChangeView
      Dim oldDefaultSort As Config.GallerySort = mGalleryConfig.DefaultSort 'Utils.GetSort(mGalleryConfig)
      Dim oldDefaultSortDESC As Boolean = mGalleryConfig.DefaultSortDESC 'Utils.GetSortDESC(mGalleryConfig)

      'WES - Added server side Page.IsValid check of validators
      If Page.IsValid Then

        Dim Security As New PortalSecurity()
        Dim ctlModule As New ModuleController

        '<tamttt:note 2 required & hidden for integration >
        ctlModule.UpdateModuleSetting(ModuleId, "Provider", "")
        ctlModule.UpdateModuleSetting(ModuleId, "Type", "Gallery")
        '</tamttt:note>
        ctlModule.UpdateModuleSetting(ModuleId, "GalleryTitle", txtGalleryTitle.Text)
        ctlModule.UpdateModuleSetting(ModuleId, "GalleryDescription", txtDescription.Text)

        ' WES: Modified to restrict RootURL to be portal home directory relative
        Dim homeDirectoryRelativeRootURL As String = Security.InputFilter(RootURL.Text, PortalSecurity.FilterFlag.NoMarkup)
        homeDirectoryRelativeRootURL = Regex.Replace(homeDirectoryRelativeRootURL, "\.{2,}[\\/]{0,1}|[\000-\037:*?""><|&]", "").Replace("\", "/").Replace("//", "/")
        homeDirectoryRelativeRootURL = FileSystemUtils.FormatFolderPath(homeDirectoryRelativeRootURL.TrimStart("/"c))
        RootURL.Text = homeDirectoryRelativeRootURL
        ctlModule.UpdateModuleSetting(ModuleId, "RootURL", homeDirectoryRelativeRootURL)
        ctlModule.UpdateModuleSetting(ModuleId, "InitialGalleryHierarchy", ddlDefaultAlbum.SelectedValue)

        If txtCreatedDate.Enabled Then
          Dim createdDate As DateTime
          Try
            createdDate = DateTime.Parse(txtCreatedDate.Text)
          Catch ex As Exception
            createdDate = DateTime.Now
          End Try
          ctlModule.UpdateModuleSetting(ModuleId, "CreatedDate", createdDate.ToString(Globalization.CultureInfo.InvariantCulture))
        End If
        ctlModule.UpdateModuleSetting(ModuleId, "Theme", ddlSkins.SelectedItem.Text)
        ctlModule.UpdateModuleSetting(ModuleId, "StripWidth", txtStripWidth.Text)
        ctlModule.UpdateModuleSetting(ModuleId, "StripHeight", txtStripHeight.Text)
        ctlModule.UpdateModuleSetting(ModuleId, "Quota", txtQuota.Text)
        ctlModule.UpdateModuleSetting(ModuleId, "MaxFileSize", txtMaxFileSize.Text)
        ctlModule.UpdateModuleSetting(ModuleId, "MaxPendingUploadsSize", txtMaxPendingUploadsSize.Text)
        ctlModule.UpdateModuleSetting(ModuleId, "MaxThumbWidth", txtMaxThumbWidth.Text)
        ctlModule.UpdateModuleSetting(ModuleId, "MaxThumbHeight", txtMaxThumbHeight.Text)
        ctlModule.UpdateModuleSetting(ModuleId, "CategoryValues", txtCategoryValues.Text)
        ctlModule.UpdateModuleSetting(ModuleId, "BuildCacheOnStart", chkBuildCacheOnStart.Checked.ToString)
        ctlModule.UpdateModuleSetting(ModuleId, "FixedWidth", txtFixedWidth.Text)
        ctlModule.UpdateModuleSetting(ModuleId, "FixedHeight", txtFixedHeight.Text)
        ctlModule.UpdateModuleSetting(ModuleId, "EncoderQuality", txtEncoderQuality.Text)
        ctlModule.UpdateModuleSetting(ModuleId, "SlideshowSpeed", txtSlideshowSpeed.Text)
        ctlModule.UpdateModuleSetting(ModuleId, "MultiLevelMenu", chkMultiLevelMenu.Checked.ToString)
        ctlModule.UpdateModuleSetting(ModuleId, "AllowSlideshow", chkSlideshow.Checked.ToString)
        ctlModule.UpdateModuleSetting(ModuleId, "AllowPopup", chkPopup.Checked.ToString)
        ctlModule.UpdateModuleSetting(ModuleId, "AllowDownload", chkDownload.Checked.ToString)
        ctlModule.UpdateModuleSetting(ModuleId, "AllowVoting", chkVoting.Checked.ToString)
        ctlModule.UpdateModuleSetting(ModuleId, "AllowExif", chkExif.Checked.ToString)
        ctlModule.UpdateModuleSetting(ModuleId, "UseWatermark", chkWatermark.Checked.ToString)
        ctlModule.UpdateModuleSetting(ModuleId, "AutoApproval", chkAutoApproval.Checked.ToString)

        'William Severance - modified to handle none specified OwnerID
        Dim OwnerID As String = ctlOwnerLookup.ResultItems.Replace(";", "")
        If chkPrivate.Checked Then
          If OwnerID = "" Then OwnerID = Me.UserId.ToString
        Else
          If OwnerID <> "" Then OwnerID = Config.DefaultOwnerId.ToString
        End If

        ctlModule.UpdateModuleSetting(ModuleId, "OwnerID", OwnerID)
        ctlModule.UpdateModuleSetting(ModuleId, "IsPrivate", chkPrivate.Checked.ToString)
        ctlModule.UpdateModuleSetting(ModuleId, "DefaultSortDESC", chkDESC.Checked.ToString)
        ctlModule.UpdateModuleSetting(ModuleId, "DefaultSort", ddlGallerySort.SelectedItem.Value.ToString)
        ctlModule.UpdateModuleSetting(ModuleId, "DefaultView", ddlGalleryView.SelectedItem.Value.ToString)
        ctlModule.UpdateModuleSetting(ModuleId, "AllowChangeView", chkChangeView.Checked.ToString)

        Dim textDisplay As String = ""
        Dim textDownloadRoles As String = ""
        Dim textSortProperties As String = ""
        Dim item As ListItem

        'Update list display info list
        For Each item In lstDisplay.Items
          If item.Selected Then
            textDisplay &= item.Value & ";"
          End If
        Next
        If Len(textDisplay) > 0 Then
          textDisplay = textDisplay.TrimEnd(";"c)
        End If

        textDownloadRoles = ctlDownloadRoles.ResultItems

        ' Update sort properties list
        For Each item In lstSortProperties.Items
          If item.Selected Then
            textSortProperties &= item.Value & ";"
          End If
        Next
        If Len(textSortProperties) > 0 Then
          textSortProperties = textSortProperties.TrimEnd(";"c)
        End If

        ctlModule.UpdateModuleSetting(ModuleId, "TextDisplayValues", textDisplay)
        ctlModule.UpdateModuleSetting(ModuleId, "DownloadRoles", textDownloadRoles)
        ctlModule.UpdateModuleSetting(ModuleId, "SortPropertyValues", textSortProperties)

        'William Severance - Added below line to expire module cache before call to ResetGalleryConfig
        ModuleController.SynchronizeModule(ModuleId)
        mGalleryConfig = Config.ResetGalleryConfig(ModuleId) 'Reload changed configuration

        'William Severance - Added to set folder read permissions based on DownloadRoles if set or module view
        'permissions if not inheriting from page or page permissions if inheriting.
        'Properly set folder read permissions are necessary now that we are using
        'core LinkClick.aspx for downloads.

        Gallery.Utils.SyncFolderPermissions(textDownloadRoles, ModuleId, TabId)

        'William Severance - Added refresh of logged in user's cookies when change in DefaultView, DefaultSort, DefaultSortDESC
        If ((oldAllowChangeView <> mGalleryConfig.AllowChangeView) AndAlso mGalleryConfig.AllowChangeView) OrElse _
                (oldDefaultView <> mGalleryConfig.DefaultView) Then
          Utils.RefreshCookie(Utils.GALLERY_VIEW, ModuleId, mGalleryConfig.DefaultView)
        End If
        If oldDefaultSort <> mGalleryConfig.DefaultSort Then Utils.RefreshCookie(Utils.GALLERY_SORT, ModuleId, mGalleryConfig.DefaultSort)
        If oldDefaultSortDESC <> mGalleryConfig.DefaultSortDESC Then Utils.RefreshCookie(Utils.GALLERY_SORT_DESC, ModuleId, mGalleryConfig.DefaultSortDESC)

        ' Redirect back to the gallery home page
        GoBack()
      End If
    End Sub

    Private Sub cmdReturn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdReturn.Click
      GoBack()
    End Sub

    Private Sub GoBack()
      Response.Redirect(NavigateURL())
    End Sub

#End Region

#Region "Public Properties"

    Public ReadOnly Property GalleryConfig() As DotNetNuke.Modules.Gallery.Config
      Get
        Return mGalleryConfig
      End Get
    End Property

#End Region

    'William Severance - Added javascript to provide hide/unhide of rowOwner on change if Is Private check box

    Private Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
      Dim sb As New StringBuilder
      sb.AppendLine("function onPrivateChanged(chkPrivate) {")
      sb.AppendLine("    if (document.getElementById) {")
      sb.Append("        var rowOwner = document.getElementById('")
      sb.Append(rowOwner.ClientID)
      sb.AppendLine("');")
      sb.AppendLine("    } else {")
      sb.Append("         var rowOwner = document.all['")
      sb.Append(rowOwner.ClientID)
      sb.AppendLine("'];")
      sb.AppendLine("    }")
      sb.AppendLine("    if ((chkPrivate != null) && (rowOwner != null)) {")
      sb.AppendLine("        if (chkPrivate.checked) {")
      sb.AppendLine("              rowOwner.style.display='block';")
      sb.AppendLine("        } else {")
      sb.AppendLine("              rowOwner.style.display='none';")
      sb.AppendLine("        }")
      sb.AppendLine("    }")
      sb.AppendLine("}")

      If Not Page.ClientScript.IsClientScriptBlockRegistered(Me.GetType, "DNN_Gallery_Settings_IsPrivate") Then
        Page.ClientScript.RegisterClientScriptBlock(Me.GetType, "DNN_Gallery_Settings_IsPrivate", sb.ToString, True)
      End If

      'William Severance - Added code/javascript to provide warning if the Max File Size
      'is set to a value larger than the lesser of the maxRequestLength and RequestLengthDiskThreshold
      'attributes set in the httpRuntime section of the web.config.

      Dim objSection As Object = Nothing

      Try
        objSection = DotNetNuke.Common.Utilities.Config.GetSection("system.web/httpRuntime")
      Catch ex As Exception
        'LogException(ex) 'Do not log error if system.web/httpRuntime section not available.
      End Try

      Dim helpMsg As String = Localization.GetString("MaxFileSize.Help", LocalResourceFile)

      If objSection IsNot Nothing AndAlso TypeOf objSection Is HttpRuntimeSection Then
        Dim maxRequestLength As Integer
        Dim requestLengthDiskThreshold As Integer
        Dim executionTimeout As Double
        With DirectCast(objSection, HttpRuntimeSection)
          maxRequestLength = .MaxRequestLength
          requestLengthDiskThreshold = .RequestLengthDiskThreshold
          executionTimeout = .ExecutionTimeout.TotalSeconds
        End With
        Dim maxFileSizeConstraint As Integer = Math.Min(maxRequestLength, requestLengthDiskThreshold)
        helpMsg &= String.Format(Localization.GetString("MaxFileSizeLimits.Text", LocalResourceFile), maxRequestLength, requestLengthDiskThreshold, executionTimeout)
        lblMaxFileSizeWarning.Style("display") = If(Integer.Parse(txtMaxFileSize.Text) > maxFileSizeConstraint, "block", "none")

        sb.Length = 0
        sb.AppendLine("function onMaxFileSizeChanged(txtMaxFileSize) {")
        sb.AppendLine("   if (document.getElementById) {")
        sb.Append("     var lblMaxFileSizeWarning = document.getElementById ('")
        sb.Append(lblMaxFileSizeWarning.ClientID)
        sb.AppendLine("');")
        sb.AppendLine("   } else {")
        sb.Append("     var lblMaxFileSizeWarning=document.all['")
        sb.Append(lblMaxFileSizeWarning.ClientID)
        sb.AppendLine("'];")
        sb.AppendLine("   }")
        sb.AppendLine("   if (txtMaxFileSize != null && lblMaxFileSizeWarning != null) {")
        sb.Append("        lblMaxFileSizeWarning.style.display = (txtMaxFileSize.value > ")
        sb.Append(maxFileSizeConstraint.ToString)
        sb.AppendLine(" ? 'block' : 'none');")
        sb.AppendLine("   }")
        sb.AppendLine("}")

        If Not Page.ClientScript.IsClientScriptBlockRegistered(Me.GetType, "DNN_Gallery_Settings_MaxFileSizeWarning") Then
          Page.ClientScript.RegisterClientScriptBlock(Me.GetType, "DNN_Gallery_Settings_MaxFileSizeWarning", sb.ToString, True)
        End If
      Else
        lblMaxFileSizeWarning.Style("display") = "none"
      End If

      plMaxFileSize.HelpText = helpMsg

    End Sub

  End Class

End Namespace


