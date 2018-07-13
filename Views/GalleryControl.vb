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
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.HttpUtility
Imports System.Web.Security
Imports System.Text
Imports System.Configuration
Imports DotNetNuke
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Modules.Gallery.Config
Imports DotNetNuke.Modules.Gallery.Utils

Namespace DotNetNuke.Modules.Gallery.Views

    ''' <summary>
    ''' GUI code inserted into the aspx page that is common across all views
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GalleryControl
        Inherits System.Web.UI.WebControls.WebControl
        Implements INamingContainer

        Private mInitialised As Boolean
        Private mBaseView As BaseView
        Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config
        Private mGalleryAuthorization As DotNetNuke.Modules.Gallery.Authorization
        Private mUserRequest As GalleryUserRequest

        Public Sub New()
            If Not HttpContext.Current Is Nothing Then
                If Not HttpContext.Current.Request.QueryString("view") Is Nothing Then
                    View = CType([Enum].Parse(GetType(GalleryView), HttpContext.Current.Request.QueryString("view"), True), GalleryView)
                Else
                    View = GalleryView.Standard
                End If
            End If
        End Sub 'New

        Protected Overridable Sub Initialise()
            mInitialised = True
        End Sub 'Initialise

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
      If View = GalleryView.ListView Then
        DotNetNuke.Framework.jQuery.RequestRegistration()
      End If
      Initialise()
    End Sub 'OnLoad

        Protected Overrides Sub EnsureChildControls()
            If Not mInitialised Then
                Initialise()
            End If
            MyBase.EnsureChildControls()
        End Sub 'EnsureChildControls

        Protected Overridable Sub CreateObject()
            If GalleryConfig.IsValidPath Then
                Select Case View
                    Case GalleryView.Standard   ' Show the standard view
                        mBaseView = New Views.StandardView(Me)

                    Case GalleryView.ListView   ' Show the thumbs as a list
                        mBaseView = New Views.ListView(Me)

                    Case GalleryView.CardView   ' Show thumbs as a card
                        mBaseView = New Views.CardView(Me)
                End Select
            Else
                mBaseView = New Views.NoConfigView(Me)
            End If
        End Sub

        Protected Overrides Sub CreateChildControls()
            CreateObject()
            If Not (mBaseView Is Nothing) Then 'AndAlso (Not Page.IsPostBack) Then
                mBaseView.CreateChildControls()
            End If
        End Sub 'CreateChildControls

        Protected Overrides Sub OnPreRender(ByVal e As System.EventArgs)
            If Not (mBaseView Is Nothing) Then
                mBaseView.OnPreRender()
            End If
        End Sub 'OnPreRender

        Protected Overrides Sub Render(ByVal writer As System.Web.UI.HtmlTextWriter)
            If Not (mBaseView Is Nothing) Then
                mBaseView.Render(writer)
            End If
        End Sub 'Render 

        Public Property PortalID() As Integer
            Get
                Dim savedState As Object = ViewState("GalleryPortalID")
                If savedState Is Nothing Then
                    Return 0
                Else
                    Return CType(savedState, Integer)
                End If
            End Get
            Set(ByVal Value As Integer)
                ViewState("GalleryPortalID") = Value
            End Set
        End Property

        Public Property TabID() As Integer
            Get
                Dim savedState As Object = ViewState("GalleryTabID")
                If savedState Is Nothing Then
                    Return 0
                Else
                    Return CType(savedState, Integer)
                End If
            End Get
            Set(ByVal Value As Integer)
                ViewState("GalleryTabID") = Value
            End Set
        End Property

        Public Property ModuleId() As Integer
            Get
                Dim savedState As Object = ViewState("GalleryModuleID")
                If savedState Is Nothing Then
                    Return 0
                Else
                    Return CType(savedState, Integer)
                End If
            End Get
            Set(ByVal Value As Integer)
                ViewState("GalleryModuleID") = Value
            End Set
        End Property

        Public Property GalleryConfig() As DotNetNuke.Modules.Gallery.Config
            Get
                If mGalleryConfig Is Nothing Then
                    mGalleryConfig = DotNetNuke.Modules.Gallery.Config.GetGalleryConfig(ModuleId)
                End If
                Return mGalleryConfig
            End Get
            Set(ByVal Value As DotNetNuke.Modules.Gallery.Config)
                mGalleryConfig = Value
            End Set

        End Property

        Public Property GalleryAuthorize() As DotNetNuke.Modules.Gallery.Authorization
            Get
                If mGalleryAuthorization Is Nothing Then
                    mGalleryAuthorization = New DotNetNuke.Modules.Gallery.Authorization(ModuleId)
                End If
                Return mGalleryAuthorization
            End Get
            Set(ByVal Value As DotNetNuke.Modules.Gallery.Authorization)
                mGalleryAuthorization = Value
            End Set
        End Property

        Public Property UserRequest() As GalleryUserRequest
            Get
                If mUserRequest Is Nothing Then
                    mUserRequest = New GalleryUserRequest(ModuleId, Sort, SortDESC)
                End If
                Return mUserRequest
            End Get
            Set(ByVal Value As GalleryUserRequest)
                mUserRequest = Value
            End Set
        End Property

        Public Property Sort() As Config.GallerySort
            Get
                Dim savedState As Object = ViewState("Sort")
                If savedState Is Nothing Then
                    Return GallerySort.Title
                Else
                    Return CType(savedState, Config.GallerySort)
                End If
            End Get
            Set(ByVal Value As Config.GallerySort)
                ViewState("Sort") = Value
            End Set
        End Property

        Public Property View() As Config.GalleryView
            Get
                Dim savedState As Object = ViewState("View")
                If savedState Is Nothing Then
                    Return GalleryView.Standard
                Else
                    Return CType(savedState, Config.GalleryView)
                End If
            End Get
            Set(ByVal Value As Config.GalleryView)
                ViewState("View") = Value
            End Set
        End Property

        Public Property SortDESC() As Boolean
            Get
                Dim savedState As Object = ViewState("SortDESC")
                If savedState Is Nothing Then
                    Return False
                Else
                    Return CType(savedState, Boolean)
                End If
            End Get
            Set(ByVal Value As Boolean)
                ViewState("SortDESC") = Value
            End Set
        End Property

        Public Property LocalResourceFile() As String
            Get
                Dim savedState As Object = ViewState("GalleryResourceFile")
                If savedState Is Nothing Then
                    ViewState("GalleryResourceFile") = TemplateSourceDirectory & "/" & Localization.LocalResourceDirectory & "/Container.ascx"
                End If
                Return CType(ViewState("GalleryResourceFile"), String)
            End Get
            Set(ByVal Value As String)
                ViewState("GalleryResourceFile") = Value
            End Set
        End Property

        Public Function LocalizedText(ByVal key As String) As String
            Return Localization.GetString(key, LocalResourceFile)
        End Function

    End Class

End Namespace