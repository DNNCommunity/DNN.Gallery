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
Imports System
Imports System.Collections
Imports System.IO
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Common.Globals

Namespace DotNetNuke.Modules.Gallery.WebControls

  Public MustInherit Class BreadCrumbs
    Inherits System.Web.UI.UserControl

    Private mGalleryRequest As BaseRequest
    Private mModuleID As Integer
    Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config = Nothing

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

    ' WES: Refactored to fix breadcrumb separator image not displaying and replace DataList control
    ' with lighter weight Repeater control and remove outer table rendering.

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

      If Not IsPostBack Then
        BindBreadcrumbs()
      End If
    End Sub

    Private Sub BindBreadcrumbs()
      If GalleryConfig.IsValidPath Then
        With rptFolders
          .SeparatorTemplate = New CompiledTemplateBuilder(New BuildTemplateMethod(AddressOf BuildSeparator))
          .DataSource = GalleryRequest.FolderPaths
          .DataBind()
        End With
      Else
        Visible = False
      End If
    End Sub

    Private Sub BuildSeparator(ByVal container As Control)
      'container.Controls.Add(New LiteralControl("<img alt='' src='" & Page.ResolveUrl(GalleryConfig.GetImageURL("breadcrumb.gif")) & "' class='Gallery_BreadcrumbSeparator' />"))
      container.Controls.Add(New LiteralControl("<span class='Gallery_BreadcrumbSeparator'>&nbsp;</span>"))
    End Sub

    Public Sub Enable(ByVal Enabled As Boolean)
      IsEnabled = Enabled
      BindBreadcrumbs()
    End Sub

    Protected Property IsEnabled As Boolean
      Get
        If ViewState("IsEnabled") Is Nothing Then
          Return True
        Else
          Return CType(ViewState("IsEnabled"), Boolean)
        End If
      End Get
      Set(value As Boolean)
        ViewState("IsEnabled") = value
      End Set
    End Property

    Public Property ModuleID() As Integer
      Get
        Return mModuleID
      End Get
      Set(ByVal Value As Integer)
        mModuleID = Value
      End Set
    End Property

    Public ReadOnly Property GalleryConfig() As Gallery.Config
      Get
        If mGalleryConfig Is Nothing Then
          mGalleryConfig = Config.GetGalleryConfig(ModuleID)
        End If
        Return mGalleryConfig
      End Get
    End Property

    Public Property GalleryRequest() As BaseRequest
      Get
        Return mGalleryRequest
      End Get
      Set(ByVal Value As BaseRequest)
        mGalleryRequest = Value
      End Set
    End Property
  End Class
End Namespace

