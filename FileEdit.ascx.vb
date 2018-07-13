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
Imports DotNetNuke.Common.Utilities
Imports DotNetNuke.Entities.Users
Imports DotNetNuke.Modules.Gallery.Utils
Imports DotNetNuke.Modules.Gallery.Config
Imports DotNetNuke.Modules.Gallery.WebControls
Imports DotNetNuke.Services.Localization

Namespace DotNetNuke.Modules.Gallery

  Public MustInherit Class FileEdit
    Inherits DotNetNuke.Entities.Modules.PortalModuleBase

#Region "Private Members"

    Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config
    Private mRequest As GalleryRequest
    Private mRequestViewer As GalleryViewerRequest
    Private mCurrentItem As Integer
    Private mFolder As GalleryFolder
    Private mFile As GalleryFile

#End Region

#Region "Private Methods"

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
          Return "FileEdit;"
        End If
      End Get
      Set(ByVal value As String)
        ViewState("ReturnCtl") = value
      End Set
    End Property

    Private Sub BindData()
      With mFile
        txtPath.Text = .URL
        txtName.Text = .Name
        txtAuthor.Text = .Author
        txtTitle.Text = .Title
        txtLocation.Text = .Location
        txtClient.Text = .Client
        txtDescription.Text = .Description

        txtApprovedDate.Text = Utils.DateToText(.ApprovedDate) 'WES - show Date.MaxValue as empty string
        txtCreatedDate.Text = .CreatedDate.ToShortDateString

        lblCurrentFile.Text = .Name
        tdFileEditImage.Attributes.Add("style", "width:" & (mGalleryConfig.MaximumThumbWidth + 50).ToString & "px;")
        imgFile.ImageUrl = .ThumbnailURL
      End With

      BindCategories()

      'William Severance - uncommenting these two lines will cause the Approved Date field and
      'calendar hyperlink to be hidden to all except the 1) Gallery owner, 2) the parent album owner, or
      '3) the module administrators. Leaving them commented keeps the same behavior as in previous
      'versions in which the Approved Date field is visible to all with edit permission on the module.

      'HansZassenhaus - updated to show Item Approval date and calendar control only if
      'Configuration for the album does not include Auto Approval, i.e. Moderated Album.

      Dim auth As New DotNetNuke.Modules.Gallery.Authorization(ModuleConfiguration)
      If auth.HasItemApprovalPermission(mFile.Parent) AndAlso Not mGalleryConfig.AutoApproval Then
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

        If InStr(1, mFile.Categories, catString) > 0 Then
          catItem.Selected = True
        End If
        'list category for current item
        lstCategories.Items.Add(catItem)
      Next

    End Sub

    Private Function GetCategories(ByVal List As CheckBoxList) As String
      Dim catItem As ListItem
      ' JIMJ init string as we concat to it later
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

    Private Sub BindNavigation()
      Dim params As New Generic.List(Of String)
      Dim url As String

      'William Severance - changed mFolder.Name to mFolder.GalleryHierachy and added FriendlyURLEncode processsing to path values
      ' Next button to edit next image in this gallery
      If Not String.IsNullOrEmpty(mFolder.GalleryHierarchy) Then
        params.Add("path=" & Utils.FriendlyURLEncode(mFolder.GalleryHierarchy))
      End If
      params.Add("mid=" & ModuleId.ToString)
      params.Add("returnctl=" & ReturnCtl)

      ' Next button to edit next image in this gallery
      params.Add("currentitem=" & mRequestViewer.NextItemNumber.ToString)
      url = NavigateURL(TabId, "FileEdit", params.ToArray())
      cmdMoveNext.NavigateUrl = url

      ' Previous button to edit previous image in this gallery
      params(params.Count - 1) = "currentitem=" & mRequestViewer.PreviousItemNumber.ToString
      url = NavigateURL(TabId, "FileEdit", params.ToArray())
      cmdMovePrevious.NavigateUrl = url

      ' Image Edit Image Button and Link
      params = New Generic.List(Of String)
      If Not mGalleryConfig.AllowPopup Then
        params.Add("returnctl=" & ReturnCtl)
        If mRequest.CurrentStrip > 1 Then params.Add("currentstrip=" & mRequest.CurrentStrip.ToString)
      End If
      If mFile.Type = Config.ItemType.Image Then
        imgFile.NavigateUrl = Utils.AppendURLParameters(mFile.EditImageURL, params.ToArray())
        lnkEditImage.Visible = True
        lnkEditImage.NavigateUrl = imgFile.NavigateUrl
      Else
        imgFile.NavigateUrl = Utils.AppendURLParameters(mFile.BrowserURL(), params.ToArray())
        lnkEditImage.Visible = False
      End If
    End Sub

#End Region

#Region "Event Handlers"

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

      ' Load the styles
      DirectCast(Page, CDefault).AddStyleSheet(CreateValidID(GalleryConfig.Css()), GalleryConfig.Css())

      'Sort feature
      'William Severance modified to pass new parameter for GalleryConfig
      Dim sort As Config.GallerySort = Utils.GetSort(mGalleryConfig)
      Dim sortDesc As Boolean = Utils.GetSortDESC(mGalleryConfig)

      mRequestViewer = New GalleryViewerRequest(ModuleId, sort, sortDesc)
      mRequest = New GalleryRequest(ModuleId)
      mFolder = mRequest.Folder
      If Not Request.QueryString("currentitem") Is Nothing Then
        mCurrentItem = Int32.Parse(Request.QueryString("currentitem"))
        mFile = CType(mRequest.Folder.List.Item(mCurrentItem), GalleryFile)
      End If

      ' Ensure Gallery edit permissions (Andrew Galbraith Ryer)
      Dim galleryAuthorization As Authorization = New Authorization(ModuleConfiguration)
      If Not galleryAuthorization.HasItemEditPermission(mFile) Then
        Response.Redirect(AccessDeniedURL(Localization.GetString("Insufficient_Edit_Permissions", mGalleryConfig.SharedResourceFile)))
      End If

      ' Load gallery breadcrumbs, this method allow call request only once
      Dim galleryBreadCrumbs As BreadCrumbs = CType(LoadControl("Controls/ControlBreadCrumbs.ascx"), BreadCrumbs)
      With galleryBreadCrumbs
        .ModuleID = ModuleId
        .GalleryRequest = CType(mRequest, BaseRequest)
      End With
      celBreadcrumbs.Controls.Add(galleryBreadCrumbs)

      'WES - Added for date range validator
      valApprovedDate.MinimumValue = DateTime.Today.AddYears(-5).ToShortDateString()
      valApprovedDate.MaximumValue = DateTime.Today.AddYears(5).ToShortDateString()
      valCreatedDate.MinimumValue = DateTime.Today.AddYears(-150).ToShortDateString()
      valCreatedDate.MaximumValue = DateTime.Today.ToShortDateString()

      cmdApprovedDate.NavigateUrl = Calendar.InvokePopupCal(txtApprovedDate)
      cmdCreatedDate.NavigateUrl = Calendar.InvokePopupCal(txtCreatedDate)

      cmdMovePrevious.Text = "" 'xhtml requirement for alt text - GAL-8522
      cmdMovePrevious.ImageUrl = mGalleryConfig.GetImageURL("s_previous.gif")
      cmdMovePrevious.ToolTip = Localization.GetString("MovePrevious", mGalleryConfig.SharedResourceFile)
      cmdMovePrevious.Visible = True

      cmdMoveNext.ImageUrl = mGalleryConfig.GetImageURL("s_next.gif")
      cmdMovePrevious.Text = "" 'xhtml requirement for alt text - GAL-8522
      cmdMoveNext.ToolTip = Localization.GetString("MoveNext", mGalleryConfig.SharedResourceFile)
      cmdMoveNext.Visible = True

      If Not Page.IsPostBack AndAlso mGalleryConfig.IsValidPath Then

        If Not mFolder.IsPopulated Then
          Response.Redirect(ApplicationURL)
        End If
        BindData()

        Dim rtnCtl As String = Request.QueryString("returnctl")
        If String.IsNullOrEmpty(rtnCtl) OrElse Not rtnCtl.EndsWith("FileEdit;") Then
          ReturnCtl = rtnCtl & "FileEdit;"
        Else
          ReturnCtl = rtnCtl
        End If

        BindNavigation()
      End If
    End Sub

    Private Sub cmdSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSave.Click
      Dim directory As String = mRequest.Folder.Path
      Dim Security As New PortalSecurity()

      'William Severance - modified to add PortalSecurity input filter to prevent scripting hacks
      '  also added range validators of type = "Date" to CreatedDate and ApprovedDate text boxes.

      If Page.IsValid Then
        Dim name As String = mFile.Name
        Dim title As String = Security.InputFilter(txtTitle.Text, PortalSecurity.FilterFlag.NoScripting)
        Dim author As String = Security.InputFilter(txtAuthor.Text, PortalSecurity.FilterFlag.NoScripting)
        Dim location As String = Security.InputFilter(txtLocation.Text, PortalSecurity.FilterFlag.NoScripting)
        Dim client As String = Security.InputFilter(txtClient.Text, PortalSecurity.FilterFlag.NoScripting)
        Dim description As String = Security.InputFilter(txtDescription.Text, PortalSecurity.FilterFlag.NoScripting)
        Dim categories As String = Security.InputFilter(GetCategories(lstCategories), PortalSecurity.FilterFlag.NoScripting)
        Dim createdDate As DateTime = Date.Parse(txtCreatedDate.Text)
        Dim approvedDate As DateTime = TextToDate(txtApprovedDate.Text) 'WES - convert empty string to DateTime.MaxValue
        'WES - corrected reversal of approvedDate and createdDate parameters
        GalleryXML.SaveMetaData(directory, name, mFile.ID, title, description, categories, author, location, client, mFile.OwnerID, createdDate, approvedDate, mFile.Score)

        If Not mFile.Parent Is Nothing Then
          Dim parent As GalleryFolder = mFile.Parent
          Config.ResetGalleryFolder(parent)
        Else
          Config.ResetGalleryConfig(ModuleId)
        End If
      End If

      ' Updated to use the new function in Utilities.vb by Quinn - 2/20/2009
      Response.Redirect(ReturnURL(TabId, ModuleId, Request))

    End Sub

    Private Sub cmdReturn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdReturn.Click
      Response.Redirect(ReturnURL(TabId, ModuleId, Request))
    End Sub
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

    End Sub

#End Region

  End Class
End Namespace

