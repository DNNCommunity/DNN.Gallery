Option Strict On
Option Explicit On

Imports DotNetNuke.Entities.Users

Namespace DotNetNuke.Modules.Gallery

  ' Class from which all gallery pop-up pages (Viewer.aspx, Slideshow.aspx, FlashPlayer.aspx,
  ' MediaPlayer.aspx, ExifMetaData.aspx inherit.

  Partial Public Class GalleryPageBase
    Inherits DotNetNuke.Framework.PageBase

    Private _GalleryConfig As DotNetNuke.Modules.Gallery.Config
    Private _GalleryAuthorization As DotNetNuke.Modules.Gallery.Authorization
    Private _CurrentRequest As DotNetNuke.Modules.Gallery.BaseRequest
    Private _TabId As Integer = -1
    Private _ModuleId As Integer = -1
    Private _Ctl As String = ""
    Private _localResourceFile As String = ""

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

    Public Property CurrentRequest() As BaseRequest
      Get
        Return _CurrentRequest
      End Get
      Set(ByVal value As BaseRequest)
        _CurrentRequest = value
      End Set
    End Property

    Public ReadOnly Property PortalId() As Integer
      Get
        Return PortalSettings.PortalId
      End Get
    End Property

    Public ReadOnly Property Ctl() As String
      Get
        Return _Ctl
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

    Public Shadows Property LocalResourceFile() As String
      Get
        If _localResourceFile = "" Then
          'Hack to adjust for legacy use of 'exif' rather than 'exifmetadata' as ControlKey for
          'ExifMetaData.ascx in module definitions
          Dim ctlName As String
          If _Ctl.ToLower = "exif" Then
            ctlName = "ExifMetaData"
          Else
            ctlName = _Ctl
          End If
          _localResourceFile = Me.TemplateSourceDirectory & "/" & Services.Localization.Localization.LocalResourceDirectory & "/" & ctlName & ".ascx.resx"
        End If
        Return _localResourceFile
      End Get
      Set(ByVal Value As String)
        _localResourceFile = Value
      End Set
    End Property

    Public Property ControlTitle As String
      Get
        Return lblTitle.Text
      End Get
      Set(value As String)
        lblTitle.Text = value
      End Set
    End Property

    Public Sub AddStyleSheet(ByVal id As String, ByVal href As String, ByVal isFirst As Boolean)
      'Find the placeholder control
      Dim objCSS As Control = Me.Header.FindControl("CSS")

      If Not objCSS Is Nothing Then
        'First see if we have already added the <LINK> control
        Dim objCtrl As Control = Page.Header.FindControl(id)

        If objCtrl Is Nothing Then
          Dim objLink As New HtmlLink()
          objLink.ID = id
          objLink.Attributes("rel") = "stylesheet"
          objLink.Attributes("type") = "text/css"
          objLink.Href = href

          If isFirst Then
            'Find the first HtmlLink
            Dim iLink As Integer
            For iLink = 0 To objCSS.Controls.Count - 1
              If TypeOf objCSS.Controls(iLink) Is HtmlLink Then
                Exit For
              End If
            Next
            objCSS.Controls.AddAt(iLink, objLink)
          Else
            objCSS.Controls.Add(objLink)
          End If
        End If
      End If
    End Sub

    Private Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
      If Not Request.QueryString("mid") Is Nothing Then
        _ModuleId = Integer.Parse(Request.QueryString("mid"))
      End If
      If Not Request.QueryString("tabid") Is Nothing Then
        _TabId = Integer.Parse(Request.QueryString("tabid"))
        If _TabId <> PortalSettings.ActiveTab.TabID Then
          Response.Redirect(AccessDeniedURL(), True)
        End If
      Else
        _TabId = PortalSettings.ActiveTab.TabID
      End If

      Dim modController As New Entities.Modules.ModuleController
      Dim modDictionary As Generic.Dictionary(Of Integer, Entities.Modules.ModuleInfo)
      modDictionary = modController.GetTabModules(_TabId)
      If Not modDictionary.ContainsKey(ModuleId) OrElse Not modDictionary(_ModuleId).DesktopModule.ModuleName = "DNN_Gallery" Then
        Response.Redirect(AccessDeniedURL(), True)
      End If

      If Not Request.QueryString("ctl") Is Nothing Then
        _Ctl = Request.QueryString("ctl")
      End If

      If GalleryAuthorization.HasViewPermission Then
        AddStyleSheet(GalleryConfig.Theme & "_stylesheet", GalleryConfig.Css(), True)
        Dim ctlFileName As String = ""
        Select Case _Ctl.ToLower
          Case "viewer" : ctlFileName = "ControlViewer.ascx"
          Case "slideshow" : ctlFileName = "ControlSlideshow.ascx"
          Case "flashplayer" : ctlFileName = "ControlFlashPlayer.ascx"
          Case "mediaplayer" : ctlFileName = "ControlMediaPlayer.ascx"
          Case "exif" : ctlFileName = "ControlExif.ascx"
          Case Else
            Response.Redirect(AccessDeniedURL(), True)
        End Select
        Dim ctlToLoad As System.Web.UI.Control = LoadControl("Controls/" & ctlFileName)
        If Not ctlToLoad Is Nothing Then
          ctlToLoad.ID = "ctl" & _Ctl
          phControl.Controls.Add(ctlToLoad)
          If _Ctl.ToLower = "viewer" AndAlso GalleryAuthorization.HasEditPermission Then
            Title = Localization.GetString("ControlTitle_editor", LocalResourceFile)
          Else
            Title = Localization.GetString("ControlTitle_" & _Ctl.ToLower, LocalResourceFile)
          End If
          btnClose.Text = Localization.GetString("btnClose", GalleryConfig.SharedResourceFile)
        End If
      Else
        Response.Redirect(AccessDeniedURL(), True)
      End If
    End Sub
  End Class
End Namespace