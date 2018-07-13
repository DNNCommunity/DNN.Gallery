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

Imports System.Text

Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Modules.Gallery
Imports DotNetNuke.Modules.Gallery.Utils

Namespace DotNetNuke.Modules.Gallery.WebControls

  Public MustInherit Class Pager
    Inherits System.Web.UI.UserControl

    Private mUserRequest As GalleryUserRequest
    Private mCurrentItems As New ArrayList
    Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config

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
      Try
        Dim mContainer As DotNetNuke.Modules.Gallery.Container = CType(Me.NamingContainer, DotNetNuke.Modules.Gallery.Container)
        mGalleryConfig = mContainer.GalleryConfig

        If mGalleryConfig.IsValidPath Then
          mUserRequest = mContainer.UserRequest
          mCurrentItems = mUserRequest.CurrentItems

          If (Not Page.IsPostBack) AndAlso (mCurrentItems.Count > 0) Then
            If mCurrentItems.Count > 0 Then

              EnableControl(True)
              dlPager.DataSource = mUserRequest.PagerItems

              If mUserRequest.CurrentStrip > 1 Then
                Dim item As PagerDetail
                Dim i As Integer
                For Each item In mUserRequest.PagerItems
                  If item.Strip = mUserRequest.CurrentStrip Then
                    dlPager.SelectedIndex = i 'mUserRequest.CurrentStrip
                    Exit For
                  End If
                  i += 1
                Next

              Else
                dlPager.SelectedIndex = 0
              End If

              dlPager.DataBind()
              lblPageInfo.Text = Localization.GetString("Page", mGalleryConfig.SharedResourceFile)
            Else
              ' some cases when admin delete the last item in strip
              ' it should redirect to the first strip of the album
              'If Not Request.QueryString("currentstrip") Is Nothing Then
              '    Response.Redirect(GetURL(Page.Request.ServerVariables("URL"), Page, "", "currentstrip="))
              'Else
              lblPageInfo.Text = Localization.GetString("AlbumEmpty", mGalleryConfig.SharedResourceFile)
              'End If
            End If
          End If
        Else
          EnableControl(False)
        End If

      Catch exc As Exception
        ' JIMJ Add more error logging
        ProcessModuleLoadException(Me, exc)
      End Try
    End Sub

    Private Sub EnableControl(ByVal IsEnabled As Boolean)
      spnPager.Visible = IsEnabled
    End Sub

  End Class
End Namespace

