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
Option Explicit On

Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Entities.Users
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.Common.Utilities
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Services.Exceptions

Namespace DotNetNuke.Modules.Gallery.WebControls
  Public Class FlashPlayer
    Inherits GalleryWebControlBase

    Private _CurrentRequest As GalleryMediaRequest
    Private mFlashURL As String = ""
    Private mFlashProperties As SWFHeaderReader
    Private mWidth As Integer
    Private mHeight As Integer

    Public Property CurrentRequest() As GalleryMediaRequest
      Get
        Return _CurrentRequest
      End Get
      Set(ByVal value As GalleryMediaRequest)
        _CurrentRequest = value
      End Set
    End Property

    Public ReadOnly Property FlashUrl() As String
      Get
        Return mFlashURL
      End Get
    End Property

    Public ReadOnly Property FixedWidth() As String
      Get
        Return mWidth.ToString & "px"
      End Get
    End Property

    Public ReadOnly Property FixedHeight() As String
      Get
        Return mHeight.ToString & "px"
      End Get
    End Property

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

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

      _CurrentRequest = New GalleryMediaRequest(ModuleId)

      If Not CurrentRequest Is Nothing AndAlso Not CurrentRequest.CurrentItem Is Nothing Then
        Dim mPath As String = CurrentRequest.CurrentItem.Path
        If GalleryConfig.IsValidFlashType(System.IO.Path.GetExtension(mPath)) Then
          mFlashURL = CurrentRequest.CurrentItem.URL
          mFlashProperties = New SWFHeaderReader(mPath)
          Dim maxW As Integer = GalleryConfig.FixedWidth
          Dim maxH As Integer = GalleryConfig.FixedHeight
          If mFlashProperties.HasError OrElse _
               (mFlashProperties.Width = 0 OrElse mFlashProperties.Height = 0) Then
            mWidth = maxW
            mHeight = maxW
          Else
            mWidth = mFlashProperties.Width
            mHeight = mFlashProperties.Height
            If mWidth > maxW OrElse mHeight > maxH Then
              Dim ratio As Double = mHeight / mWidth
              If ratio > maxH / maxW Then ' Bounded by height
                mWidth = Convert.ToInt32(maxH / ratio)
                mHeight = maxH
              Else 'Bounded by width
                mWidth = maxW
                mHeight = Convert.ToInt32(maxW * ratio)
              End If
            End If
          End If
        End If
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
          Dim namingContainer As Gallery.FlashPlayer = CType(Me.NamingContainer, Gallery.FlashPlayer)
          With namingContainer
            .Title = CurrentRequest.CurrentItem.Title
            .ViewControlWidth = New Unit(mWidth + 58)
          End With
        End If
        litInfo.Text = "<span>" & CurrentRequest.CurrentItem.ItemInfo & "</span>"
        litDescription.Text = "<p class=""Gallery_Description"">" & CurrentRequest.CurrentItem.Description & "</p>"
      End If
    End Sub
  End Class
End Namespace


