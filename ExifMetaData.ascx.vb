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
Imports System.Text
Imports DotNetNuke
Imports DotNetNuke.Security
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Entities.Modules.Actions
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.Modules.Gallery.Utils
Imports DotNetNuke.Modules.Gallery.Config

Namespace DotNetNuke.Modules.Gallery

    Public MustInherit Class ExifMetaData
        Inherits DotNetNuke.Entities.Modules.PortalModuleBase

#Region "Private Members"
        Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config
#End Region

#Region "Public Properties"
        Public ReadOnly Property GalleryConfig() As DotNetNuke.Modules.Gallery.Config
            Get
                Return mGalleryConfig
            End Get
    End Property

    Public Property Title As String
      Get
        Return lblTitle.Text
      End Get
      Set(value As String)
        lblTitle.Text = value
      End Set
    End Property

    Public Property ViewControlWidth As Unit
      Get
        Return New Unit(tblViewControl.Style("width"))
      End Get
      Set(value As Unit)
        tblViewControl.Style.Add("width", value.ToString())
      End Set
    End Property

#End Region

#Region "Event Handlers"
        Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

            mGalleryConfig = Config.GetGalleryConfig(ModuleId)

            ' Load the styles
            DirectCast(Page, CDefault).AddStyleSheet(CreateValidID(GalleryConfig.Css()), GalleryConfig.Css())

            If Not Page.IsPostBack AndAlso Not ViewState("UrlReferrer") Is Nothing Then
                ViewState("UrlReferrer") = Request.UrlReferrer.ToString()
            End If

        End Sub

        Private Sub btnBack_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBack.Click
            Dim url As String
            If Not ViewState("UrlReferrer") Is Nothing Then
                url = ViewState("UrlReferrer").ToString 'Request.Url.ToString.Replace("&ctl=Viewer", "&ctl=FileEdit")
            Else
                url = GetURL(Page.Request.ServerVariables("URL"), Page, "", "ctl=&mode=&mid=&currentitem=&media=")  'Request.Url.ToString.Replace("&ctl=Viewer", "")
            End If
            Response.Redirect(url)
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
        End Sub

#End Region

    End Class
End Namespace
