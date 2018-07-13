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
' Added for Localization by M. Schlomann
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Entities.Portals

Namespace DotNetNuke.Modules.Gallery.WebControls

  Public MustInherit Class Album
    'Inherits System.Web.UI.UserControl
    Inherits Entities.Modules.PortalModuleBase

    Private _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
    Private strFolderInfo As String
    Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config
    Private mAlbumObject As GalleryFolder
    Private mGalleryAuthorize As DotNetNuke.Modules.Gallery.Authorization
    'Private mModuleID As Integer
    Private MyFileName As String = "ControlAlbum.ascx" ' Needed for Localization.  But I'm not sure if it is needed longer, but then have some resx data moved to the SharedResource file.
    Private mAlbumEdit As DotNetNuke.Modules.Gallery.AlbumEdit

    Private albumDeleteConfirmationText As String
    Private fileDeleteConfirmationText As String

#Region "Controls"
    Protected WithEvents txtName As System.Web.UI.WebControls.TextBox
    Protected WithEvents txtTitle As System.Web.UI.WebControls.TextBox
    Protected WithEvents txtAuthor As System.Web.UI.WebControls.TextBox
    Protected WithEvents txtClient As System.Web.UI.WebControls.TextBox
    Protected WithEvents txtLocation As System.Web.UI.WebControls.TextBox
    Protected WithEvents txtApprovedDate As System.Web.UI.WebControls.TextBox
    Protected WithEvents txtDescription As System.Web.UI.WebControls.TextBox
    Protected WithEvents lstCategories As System.Web.UI.WebControls.CheckBoxList
    Protected WithEvents btnFolderSave As System.Web.UI.WebControls.ImageButton
#End Region

#Region "Private Methods"
    Public Sub BindData()
      If mAlbumObject.List.Count > 0 Then
        albumDeleteConfirmationText = Localization.GetString("AlbumDeleteConfirmation", LocalResourceFile)
        fileDeleteConfirmationText = Localization.GetString("FileDeleteConfirmation", LocalResourceFile)
        rowContent.Visible = True
        grdContent.DataSource = mAlbumObject.List
        grdContent.DataBind()
      Else
        rowContent.Visible = False
      End If
      'Dim myDate As DateTime = DateTime.Today
    End Sub

    Protected Function CanEdit(ByVal DataItem As Object) As Boolean
      Return mGalleryAuthorize.HasItemEditPermission(DataItem)
    End Function

#End Region

#Region "Event Handlers"
    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
      ' Added Localization for DataGrid colums by M. Schlomann
      Localization.LocalizeDataGrid(grdContent, LocalResourceFile)
      With mAlbumEdit
        mAlbumObject = .GalleryAlbum
        mGalleryAuthorize = .GalleryAuthorize 'New Authorization(mModuleID)
        mGalleryConfig = .GalleryConfig
      End With
      BindData()

    End Sub
    Private Sub grdContent_ItemCommand(ByVal source As Object, ByVal e As System.Web.UI.WebControls.DataGridCommandEventArgs) Handles grdContent.ItemCommand

      Dim itemIndex As Integer = Int16.Parse((CType(e.CommandSource, ImageButton).CommandArgument))
      Dim selItem As IGalleryObjectInfo = CType(mAlbumObject.List.Item(itemIndex), IGalleryObjectInfo)

      Select Case (CType(e.CommandSource, ImageButton).CommandName).ToLower
        Case "delete"
          Try
            mAlbumObject.DeleteChild(selItem)
            BindData()
          Catch ex As Exception
            Config.ResetGalleryConfig(ModuleId) 'Config.ResetGalleryConfig(mModuleID)
          End Try

        Case "edit"
          Dim params As New Generic.List(Of String)
          If Not String.IsNullOrEmpty(Request.QueryString("currentstrip")) Then
            params.Add("currentstrip=" & Request.QueryString("currentstrip"))
          End If
          params.Add("returnctl=" & mAlbumEdit.ReturnCtl)
          Dim url As String = Utils.AppendURLParameters(selItem.EditURL, params.ToArray())

          If url.Length > 0 Then
            Response.Redirect(url)
          Else
            'lblInfo.Text = "An error occur while trying open this item for editing"
          End If

      End Select
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
      mAlbumEdit = CType(Me.NamingContainer, DotNetNuke.Modules.Gallery.AlbumEdit)
      ModuleConfiguration = mAlbumEdit.ModuleConfiguration
      LocalResourceFile = Localization.GetResourceFile(Me, MyFileName)
    End Sub

#End Region

    Private Sub grdContent_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.DataGridItemEventArgs) Handles grdContent.ItemDataBound
      If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Then
        Dim btnDelete As WebControl = CType(e.Item.FindControl("btnDelete"), WebControl)
        If btnDelete IsNot Nothing Then
          Dim galleryObject As IGalleryObjectInfo = CType(e.Item.DataItem, IGalleryObjectInfo)
          If galleryObject.IsFolder Then
            DotNetNuke.UI.Utilities.ClientAPI.AddButtonConfirm(btnDelete, String.Format(albumDeleteConfirmationText, galleryObject.Name))
          Else
            DotNetNuke.UI.Utilities.ClientAPI.AddButtonConfirm(btnDelete, String.Format(fileDeleteConfirmationText, galleryObject.Name))
          End If
        End If
      End If
    End Sub
  End Class
End Namespace

