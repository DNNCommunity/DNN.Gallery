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

Imports DotNetNuke.Entities.Users

Namespace DotNetNuke.Modules.Gallery.WebControls
    Public Class GalleryWebControlBase
        Inherits DotNetNuke.Framework.UserControlBase

        Private _GalleryConfig As DotNetNuke.Modules.Gallery.Config
        Private _GalleryAuthorization As DotNetNuke.Modules.Gallery.Authorization
        Private _TabId As Integer = -1
        Private _ModuleId As Integer = -1
        Private _localResourceFile As String = ""

        Public PageTitle As String

        Public ReadOnly Property GalleryConfig() As Gallery.Config
            Get
                If _GalleryConfig Is Nothing Then
                    _GalleryConfig = Config.GetGalleryConfig(ModuleId)
                End If
                Return _GalleryConfig
            End Get
        End Property

        Public ReadOnly Property GalleryAuthorization() As Gallery.Authorization
            Get
                If _GalleryAuthorization Is Nothing Then
                    _GalleryAuthorization = New Gallery.Authorization(ModuleId)
                End If
                Return _GalleryAuthorization
            End Get
        End Property

        Public ReadOnly Property PortalId() As Integer
            Get
                Return PortalSettings.PortalId
            End Get
        End Property

        Public ReadOnly Property TabId() As Integer
            Get
                Return _TabId
            End Get
        End Property

        Public ReadOnly Property ModuleId() As Integer
            Get
                Return _ModuleId
            End Get
        End Property

        Public ReadOnly Property UserInfo() As UserInfo
            Get
                Return UserController.GetCurrentUserInfo
            End Get
        End Property

        Public ReadOnly Property UserId() As Integer
            Get
                If HttpContext.Current.Request.IsAuthenticated Then
                    UserId = UserInfo.UserID
                Else
                    UserId = -1
                End If
            End Get
        End Property

        Public Property LocalResourceFile() As String
            Get
                If _localResourceFile = "" Then
                    _localResourceFile = ControlPath & Services.Localization.Localization.LocalResourceDirectory & "/" & ControlName
                End If
                Return _localResourceFile
            End Get
            Set(ByVal Value As String)
                _localResourceFile = Value
            End Set
        End Property

        Public ReadOnly Property ControlPath() As String
            Get
                Return Me.TemplateSourceDirectory & "/"
            End Get
        End Property

        Public ReadOnly Property ControlName() As String
            Get
                Return Me.AppRelativeVirtualPath.Substring(Me.AppRelativeTemplateSourceDirectory.Length)
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

            If Not Request.QueryString("mid") Is Nothing Then
                _ModuleId = Integer.Parse(Request.QueryString("mid"))
            End If

            If Not Request.QueryString("tabid") Is Nothing Then
                _TabId = Integer.Parse(Request.QueryString("tabid"))
            Else
                _TabId = PortalSettings.ActiveTab.TabID
            End If

            If Not GalleryAuthorization.HasViewPermission Then
                Response.Redirect(AccessDeniedURL(), True)
            End If
        End Sub

#End Region

    End Class
End Namespace
