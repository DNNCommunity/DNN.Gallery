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

Imports System.IO
Imports System.Text
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Entities.Users
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Modules.Gallery.Utils
Imports System.Drawing

Namespace DotNetNuke.Modules.Gallery.WebControls

  Public Class Viewer
    Inherits GalleryWebControlBase

    Private _CurrentRequest As GalleryViewerRequest
    Private mZoomIndex As Integer = 0
    Private mRotateFlip As RotateFlipType = RotateFlipType.RotateNoneFlipNone
    Private mModeText As String = ""
    Private mColor As Integer = 0
    Private mCurrentIndex As Integer = -1
    Private mCurrentItem As GalleryFile
    Private isModified As Boolean = False
    Private mSort As Config.GallerySort
    Private mSortDESC As Boolean
    Private mPopupURL As String
    Private isPopup As Boolean = False

    Private mCurrentStrip As Integer = 0
    Private mReturnCtl As String = ""

    ' Code extensively refactored by WES on 2-10-2009 to eliminate repeated blocks of identical
    ' code, reduce length of query strings, fix issues with sequences of mixed rotate and flip operations
    ' producing incorrect orientation and tighten security, parameter validation.

    Private Enum RotateFlipAction As Integer
      RotateRight = 0
      RotateLeft = 1
      FlipX = 2
      FlipY = 3
    End Enum

    Private RotateFlipMatrix(,) As RotateFlipType = {{RotateFlipType.Rotate90FlipNone, RotateFlipType.Rotate270FlipNone, RotateFlipType.RotateNoneFlipX, RotateFlipType.RotateNoneFlipY}, _
                                                    {RotateFlipType.Rotate180FlipNone, RotateFlipType.RotateNoneFlipNone, RotateFlipType.Rotate90FlipX, RotateFlipType.Rotate90FlipY}, _
                                                    {RotateFlipType.Rotate270FlipNone, RotateFlipType.Rotate90FlipNone, RotateFlipType.Rotate180FlipX, RotateFlipType.Rotate180FlipY}, _
                                                    {RotateFlipType.RotateNoneFlipNone, RotateFlipType.Rotate180FlipNone, RotateFlipType.Rotate270FlipX, RotateFlipType.Rotate270FlipY}, _
                                                    {RotateFlipType.Rotate270FlipX, RotateFlipType.Rotate90FlipX, RotateFlipType.RotateNoneFlipNone, RotateFlipType.RotateNoneFlipXY}, _
                                                    {RotateFlipType.RotateNoneFlipX, RotateFlipType.Rotate180FlipX, RotateFlipType.Rotate90FlipNone, RotateFlipType.Rotate90FlipXY}, _
                                                    {RotateFlipType.Rotate90FlipX, RotateFlipType.Rotate270FlipX, RotateFlipType.Rotate180FlipNone, RotateFlipType.RotateNoneFlipNone}, _
                                                    {RotateFlipType.RotateNoneFlipY, RotateFlipType.Rotate180FlipY, RotateFlipType.Rotate90FlipXY, RotateFlipType.Rotate90FlipNone}}

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

#Region "Event Handlers"

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

      'Sort feature - WES modified to pass new parameter for GalleryConfig
      mSort = Utils.GetSort(GalleryConfig)
      mSortDESC = Utils.GetSortDESC(GalleryConfig)

      CurrentRequest = New GalleryViewerRequest(ModuleId, mSort, mSortDESC)

      mPopupURL = Page.ResolveUrl(GalleryConfig.SourceDirectory & "/GalleryPage.aspx") & "?ctl=Viewer"

      'WES - Added to fix determination of whether we're in Viewer.ascx (in place)
      '      or Viewer.aspx (in pop-up window) and to add image title to page title

      isPopup = Me.Parent.TemplateControl.AppRelativeVirtualPath.EndsWith("aspx")

      If isPopup Then
        Dim GalleryPage As GalleryPageBase = CType(Me.Parent.TemplateControl, GalleryPageBase)
        With GalleryPage
          .Title = .Title & " > " & CurrentRequest.CurrentItem.Title
          .ControlTitle = CurrentRequest.CurrentItem.Title
        End With
      Else
        Dim SitePage As CDefault = CType(Me.Page, CDefault)
        SitePage.Title = SitePage.Title & " > " & CurrentRequest.CurrentItem.Title
        Dim namingContainer As Gallery.Viewer = CType(Me.NamingContainer, Gallery.Viewer)
        With namingContainer
          mReturnCtl = .ReturnCtl
          .Title = CurrentRequest.CurrentItem.Title
        End With
      End If

      If Not Request.QueryString("zoomindex") Is Nothing Then
        mZoomIndex = Int32.Parse(Request.QueryString("zoomindex"))
        isModified = True
      End If

      If Not Request.QueryString("rotateflip") Is Nothing Then
        mRotateFlip = CType(Int32.Parse(Request.QueryString("rotateflip")), RotateFlipType)
        isModified = True
      End If

      If Not Request.QueryString("color") Is Nothing Then
        mColor = Int32.Parse(Request.QueryString("color"))
        isModified = True
      End If

      If Not Request.QueryString("currentitem") Is Nothing Then
        mCurrentIndex = Int32.Parse(Request.QueryString("currentitem"))
      End If

      If Not Request.QueryString("currentstrip") Is Nothing Then
        mCurrentStrip = Int32.Parse(Request.QueryString("currentstrip"))
      End If

      'Modified for GAL-972 -- Ensure that the update control is visible only
      'when the user is authorized to edit the image AND there has been a change
      If GalleryAuthorization.HasEditPermission Then
        If isModified = True Then
          Me.UpdateButton.Visible = True
        End If
      Else
        Me.UpdateButton.Visible = False
      End If

      mCurrentItem = CurrentRequest.CurrentItem

      If GalleryAuthorization.HasEditPermission Then
        If Not Request.QueryString("mode") Is Nothing Then
          Select Case Request.QueryString("mode")
            Case "edit"
              Me.UpdateButton.Visible = True
              Me.MoveNext.Visible = False
              Me.MovePrevious.Visible = False
              mModeText = "Edit"
            Case "save"
              mModeText = "Save"
          End Select
        End If
      End If

      If Not CurrentRequest.Folder.IsPopulated Then Response.Redirect(NavigateURL)

      SetHyperLink(MovePrevious, "s_previous.gif", "MovePrevious", CurrentRequest.PreviousItemNumber)
      SetHyperLink(MoveNext, "s_next.gif", "MoveNext", CurrentRequest.NextItemNumber)

      ' Allow zooming +3 or -3 times only
      If Not mZoomIndex > 2 Then
        SetHyperLink(ZoomIn, "s_zoomin.gif", "ZoomIn", mCurrentIndex, mModeText, mZoomIndex + 1, mRotateFlip, mColor)
      Else
        ZoomIn.Visible = False
      End If

      If Not mZoomIndex < -2 Then
        SetHyperLink(ZoomOut, "s_zoomout.gif", "ZoomOut", mCurrentIndex, mModeText, mZoomIndex - 1, mRotateFlip, mColor)
      Else
        ZoomOut.Visible = False
      End If

      SetHyperLink(RotateRight, "s_rotateright.gif", "RotateRight", mCurrentIndex, mModeText, mZoomIndex, RotateFlipMatrix(mRotateFlip, RotateFlipAction.RotateRight), mColor)
      SetHyperLink(RotateLeft, "s_rotateleft.gif", "RotateLeft", mCurrentIndex, mModeText, mZoomIndex, RotateFlipMatrix(mRotateFlip, RotateFlipAction.RotateLeft), mColor)

      SetHyperLink(FlipX, "s_flipx.gif", "FlipX", mCurrentIndex, mModeText, mZoomIndex, RotateFlipMatrix(mRotateFlip, RotateFlipAction.FlipX), mColor)
      SetHyperLink(FlipY, "s_flipy.gif", "FlipY", mCurrentIndex, mModeText, mZoomIndex, RotateFlipMatrix(mRotateFlip, RotateFlipAction.FlipY), mColor)

      Dim mColorPlus As Integer = ((1 - CInt(Math.Ceiling(mColor / 256))) * 256) + mColor - (32 * CInt(Math.Ceiling(mColor / 256)))
      Dim mColorMinus As Integer = ((1 - CInt(Math.Ceiling(mColor / 256))) * 256) + mColor + (32 * CInt(Math.Ceiling(mColor / 256)))

      BWPlus.Visible = (Not mColorPlus = 0)
      BWMinus.Visible = ((Not mColorMinus > 256) AndAlso (Not mColor = 0))

      SetHyperLink(Color, "s_color.gif", "Color", mCurrentIndex, mModeText, mZoomIndex, mRotateFlip, 0)
      SetHyperLink(BWMinus, "s_bwminus.gif", "BWMinus", mCurrentIndex, mModeText, mZoomIndex, mRotateFlip, mColorMinus)
      SetHyperLink(BWPlus, "s_bwplus.gif", "BWPlus", mCurrentIndex, mModeText, mZoomIndex, mRotateFlip, mColorPlus)

      SetHyperLink(UpdateButton, "m_save.gif", "UpdateButton", mCurrentIndex, "Save", mZoomIndex, mRotateFlip, mColor)

      DotNetNuke.UI.Utilities.ClientAPI.AddButtonConfirm(UpdateButton, Localization.GetString("Save_Confirm", GalleryConfig.SharedResourceFile))

      litInfo.Text = "<span>" & CurrentRequest.CurrentItem.ItemInfo & "</span>"
      litDescription.Text = "<p class=""Gallery_Description"">" & CurrentRequest.CurrentItem.Description & "</p>"
    End Sub

#End Region

#Region "Private/Protected Methods"

    Private Sub SetHyperLink(ByVal ctl As System.Web.UI.WebControls.HyperLink, ByVal iconFilename As String, ByVal toolTipKey As String, _
                              ByVal currentItem As Integer)
      Dim params As New ArrayList
      AddNonDefaultParameter(params, "mid", ModuleId, -1)
      AddNonDefaultParameter(params, "path", CurrentRequest.Path, "")
      AddNonDefaultParameter(params, "currentitem", currentItem, -1)

      SetHyperLink(ctl, iconFilename, toolTipKey, params)
    End Sub

    Private Sub SetHyperLink(ByVal ctl As System.Web.UI.WebControls.HyperLink, ByVal iconFilename As String, ByVal toolTipKey As String, _
                              ByVal currentItem As Integer, ByVal modeText As String, ByVal zoomIndex As Integer, ByVal rotateFlip As RotateFlipType, _
                              ByVal color As Integer)

      Dim params As New ArrayList
      AddNonDefaultParameter(params, "mid", ModuleId, -1)
      AddNonDefaultParameter(params, "path", CurrentRequest.Path, "")
      AddNonDefaultParameter(params, "currentitem", currentItem, -1)
      AddNonDefaultParameter(params, "mode", modeText.ToLower, "")
      AddNonDefaultParameter(params, "zoomindex", zoomIndex, 0)
      AddNonDefaultParameter(params, "rotateflip", rotateFlip, 0)
      AddNonDefaultParameter(params, "color", color, 0)
      AddNonDefaultParameter(params, "currentstrip", mCurrentStrip, 0)
      AddNonDefaultParameter(params, "returnctl", mReturnCtl, "")

      SetHyperLink(ctl, iconFilename, toolTipKey, params)
    End Sub

    Private Sub SetHyperLink(ByVal ctl As System.Web.UI.WebControls.HyperLink, ByVal iconFilename As String, ByVal toolTipKey As String, _
                             ByVal params As ArrayList)

      If isPopup Then
        ctl.NavigateUrl = GetURL(mPopupURL, CType(params.ToArray(GetType(String)), String()))
      Else
        ctl.NavigateUrl = NavigateURL(TabId, "Viewer", CType(params.ToArray(GetType(String)), String()))
      End If
      ctl.ImageUrl = GalleryConfig.GetImageURL(iconFilename)
      ctl.ToolTip = Localization.GetString(toolTipKey, GalleryConfig.SharedResourceFile)
    End Sub

    Private Sub AddNonDefaultParameter(Of T)(ByVal params As ArrayList, ByVal key As String, ByVal value As T, ByVal [default] As T)
      If Not value.Equals([default]) Then
        params.Add(key & "=" & value.ToString)
      End If
    End Sub

    Protected Function ImageURL() As String
      Dim RequestURL As String = Request.Url.ToString
      Dim url As String
      If (Not isModified) AndAlso (mCurrentItem.Parent.WatermarkImage Is Nothing) Then
        url = mCurrentItem.URL
      Else
        If isPopup Then
          url = "Image.aspx?" & Right(RequestURL, Len(RequestURL) - InStr(RequestURL, "?"))
        Else
          url = Page.ResolveUrl("DesktopModules/Gallery/Image.aspx?") & Right(RequestURL, Len(RequestURL) - InStr(RequestURL, "?"))
        End If
      End If
      Return url
    End Function

    Protected Function Name() As String
      Return mCurrentItem.Name
    End Function

#End Region

  End Class
End Namespace
