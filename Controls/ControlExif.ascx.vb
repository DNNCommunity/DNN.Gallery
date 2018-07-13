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

Imports System
Imports System.Configuration
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.UI.UserControls
Imports DotNetNuke.Services.Localization
Imports Gallery.Exif

Namespace DotNetNuke.Modules.Gallery.WebControls

  Public Class Exif
    Inherits GalleryWebControlBase

#Region "Private Members"
    Private _CurrentRequest As GalleryViewerRequest = Nothing

    '/ <summary>An instance of the PhotoProperties class</summary>
    Private mPhotoProps As PhotoProperties
    '/ <summary>Has the PhotoProperties instance successfully initialized?</summary>
    Private mPhotoPropsInitialized As Boolean
    Private mIsAnalysisSuccessful As Boolean
    Private mPhotoPath As String = ""
    Private mCurrentItem As GalleryFile
#End Region

#Region "Public Properties"
    Public Property CurrentRequest() As GalleryViewerRequest
      Get
        Return _CurrentRequest
      End Get
      Set(ByVal value As GalleryViewerRequest)
        _CurrentRequest = value
      End Set
    End Property
#End Region

#Region "Private Methods"
    Protected Function ExifHelp(ByVal tagDatumName As String) As String
      Return Localization.GetString(tagDatumName & ".Help", Me.LocalResourceFile)
    End Function

    Protected Function ExifText(ByVal tagDatumName As String) As String
      Return Localization.GetString(tagDatumName & ".Text", Me.LocalResourceFile)
    End Function

    Private Sub BindList()
      Services.Localization.Localization.LocalizeDataGrid(grdExif, Me.LocalResourceFile)
      grdExif.DataSource = mPhotoProps.PhotoMetaData
      grdExif.DataBind()
    End Sub

    '/ <summary>
    '/ Initializes the PhotoProperties tag properties using asynchronous 
    '/ method invocation of the initialization.</summary>
    Private Sub InitializePhotoProperties(ByVal initXmlFile As String)
      ' Create an instance of the PhotoProperties
      mPhotoProps = New PhotoProperties
      mPhotoPropsInitialized = mPhotoProps.Initialize(initXmlFile)

    End Sub 'InitializePhotoProperties

    Private Function AnalyzeImageFile(ByVal fileName As String) As Boolean
      Dim isAnalyzed As Boolean = False
      Try
        mPhotoProps.Analyze(fileName)
        isAnalyzed = True
      Catch ex As InvalidOperationException
      End Try
      Return isAnalyzed
    End Function 'AnalyzeImageFile

    Private Sub InitExifData()
      Dim xmlFile As String = IO.Path.Combine(Server.MapPath(GalleryConfig.SourceDirectory & "/Resources"), "_photoMetadata.xml")
      InitializePhotoProperties(xmlFile)

      mIsAnalysisSuccessful = AnalyzeImageFile(mPhotoPath)
      If mIsAnalysisSuccessful Then
        BindList()
      End If

    End Sub
#End Region

#Region "Event Handlers"
    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
      CurrentRequest = New GalleryViewerRequest(ModuleId)
      mCurrentItem = CurrentRequest.CurrentItem
      mPhotoPath = mCurrentItem.SourcePath

      If Not IO.File.Exists(mPhotoPath) Then
        mPhotoPath = mCurrentItem.Path
      End If

      InitExifData()
      Dim isPopup As Boolean = Me.Parent.TemplateControl.AppRelativeVirtualPath.EndsWith("aspx")

      If isPopup Then
        Dim GalleryPage As GalleryPageBase = CType(Me.Parent.TemplateControl, GalleryPageBase)
        With GalleryPage
          .Title = GalleryPage.Title & " > " & CurrentRequest.CurrentItem.Title
          .ControlTitle = CurrentRequest.CurrentItem.Title
        End With
      Else
        Dim SitePage As CDefault = CType(Me.Page, CDefault)
        SitePage.Title = SitePage.Title & " > " & CurrentRequest.CurrentItem.Title
        Dim namingContainer As Gallery.ExifMetaData = CType(Me.NamingContainer, Gallery.ExifMetaData)
        With namingContainer
          .Title = CurrentRequest.CurrentItem.Title
        End With
      End If
      imgExif.ImageUrl = mCurrentItem.ThumbnailURL
      litInfo.Text = "<span>" & CurrentRequest.CurrentItem.ItemInfo & "</span>"
      litDescription.Text = "<p class=""Gallery_Description"">" & CurrentRequest.CurrentItem.Description & "</p>"
    End Sub

    Private Sub grdExif_ItemDataBound(sender As Object, e As System.Web.UI.WebControls.DataGridItemEventArgs) Handles grdExif.ItemDataBound
      If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Then
        Dim dataItem As PhotoTagDatum = DirectCast(e.Item.DataItem, PhotoTagDatum)
        If dataItem IsNot Nothing AndAlso dataItem.Id = 37500 Then
          'Remove MakerNote Tag (ID=37500) as it can cantain thousands of Hex values which are propriatary to the camera maker and often encrypted
          'Including the MakerNote Tag value in the grid causes a several minute delay before the grid is rendered with some photo images
          e.Item.Cells(3).Text = "&nbsp;"
        End If
      End If
    End Sub
#End Region

#Region " Web Form Designer Generated Code "

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

    End Sub


    'NOTE: The following placeholder declaration is required by the Web Form Designer.
    'Do not delete or move it.
    Private designerPlaceholderDeclaration As System.Object

    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
      'CODEGEN: This method call is required by the Web Form Designer
      'Do not modify it using the code editor.
      InitializeComponent()
    End Sub

#End Region

  End Class

End Namespace