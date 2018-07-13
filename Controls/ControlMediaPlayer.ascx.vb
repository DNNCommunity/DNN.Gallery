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

Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Modules.Gallery

Namespace DotNetNuke.Modules.Gallery.WebControls
  Public Class MediaPlayer
    Inherits GalleryWebControlBase

    Private _MovieURL As String = ""
    Private _CurrentRequest As GalleryMediaRequest = Nothing

    Public Property CurrentRequest() As GalleryMediaRequest
      Get
        Return _CurrentRequest
      End Get
      Set(ByVal value As GalleryMediaRequest)
        _CurrentRequest = value
      End Set
    End Property

    Protected ReadOnly Property MovieURL() As String
      Get
        Return _MovieURL
      End Get
    End Property

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
      _CurrentRequest = New DotNetNuke.Modules.Gallery.GalleryMediaRequest(ModuleId)
      If Not CurrentRequest Is Nothing AndAlso Not CurrentRequest.CurrentItem Is Nothing Then
        Dim mPath As String = CurrentRequest.CurrentItem.Path
        If GalleryConfig.IsValidMovieType(System.IO.Path.GetExtension(mPath)) Then
          _MovieURL = Trim(CurrentRequest.CurrentItem.URL)
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
            Dim namingContainer As Gallery.MediaPlayer = CType(Me.NamingContainer, Gallery.MediaPlayer)
            With namingContainer
              .Title = CurrentRequest.CurrentItem.Title
              .ViewControlWidth = New Unit(GalleryConfig.FixedWidth + 58)
            End With
          End If
          RenderPlayer()
          litInfo.Text = "<span>" & CurrentRequest.CurrentItem.ItemInfo & "</span>"
          litDescription.Text = "<p class=""Gallery_Description"">" & CurrentRequest.CurrentItem.Description & "</p>"
        End If
      End If
    End Sub

    Private Sub RenderPlayer()
      If _MovieURL <> "" Then
        'WES - Added "px" and style attributes for xhtml compliancy
        Dim strH As String = (GalleryConfig.FixedHeight).ToString & "px"
        Dim strW As String = (GalleryConfig.FixedWidth).ToString & "px"
        Dim ext As String = System.IO.Path.GetExtension(CurrentRequest.CurrentItem.Name).ToLower

        Dim sb As New StringBuilder

        If ext = ".mov" OrElse ext = ".mp4" OrElse ext = ".m4v" Then
          sb.AppendLine("<object id=""objQuickTime"" name=""objQuickTime"" height=""" & strH & """ width=""" & strW & """ codebase=""http://www.apple.com/qtactivex/qtplugin.cab"" classid=""clsid:02BF25D5-8C17-4B23-BC80-D3488ABDDC6B"">")
          sb.AppendLine("     <param name=""src"" VALUE=""" & MovieURL & """></param>")
          sb.AppendLine("     <param name=""bgcolor"" value=""transparent""></param>")
          sb.AppendLine("     <param name=""scale"" value=""1.0""></param>")
          sb.AppendLine("     <param name=""volume"" value=""100""></param>")
          sb.AppendLine("     <param name=""enablejavascript"" value=""False""></param>")
          sb.AppendLine("     <param name=""autoplay"" value=""True""></param>")
          sb.AppendLine("     <param name=""cache"" value=""True""></param>")
          sb.AppendLine("     <param name=""targetcache"" value=""False""></param>")
          sb.AppendLine("     <param name=""kioskmode"" value=""False""></param>")
          sb.AppendLine("     <param name=""loop"" value=""True""></param>")
          sb.AppendLine("     <param name=""playeveryframe"" value=""False""></param>")
          sb.AppendLine("     <param name=""controller"" value=""true""></param>")
          sb.AppendLine("     <param name=""href"" value=""""></param>")
          sb.AppendLine("  <embed src=""" & MovieURL & """ height=""" & strH & """ width=""" & strW & """ autoplay=""true"" type=""video/quicktime"" pluginspage=""http://www.apple.com/quicktime/download/"" controller=""false"" href=""" & MovieURL & """ target=""myself""></embed>")
        Else
          'WES - Added browser detection and alternate rendering if not MS IE browser
          Dim isMSIE As Boolean = Request.UserAgent.Contains("MSIE")
          If isMSIE Then
            sb.AppendLine("<object id=""Player"" type=""video/x-ms-wmv"" classid=""CLSID:6BF52A52-394A-11d3-B153-00C04F79FAA6"" height=""" & strH & """ width=""" & strW & """ VIEWASTEXT>")
            sb.AppendLine("     <param name=""url"" value=""" & MovieURL & """ valuetype=""ref"" type=""video/x-ms-wmv"">")
          Else
            sb.AppendLine("<object id=""Player"" type=""video/x-ms-wmv"" data=""" & MovieURL & """ width=""" & strW & """ height=""" & strH & """>")
            sb.AppendLine("     <param name=""src"" value=""" & MovieURL & """ valuetype=""ref"" type=""" & MovieURL & """>")
          End If
          sb.AppendLine("     <param name=""animationatStart"" value=""1"">")
          sb.AppendLine("     <param name=""transparentatStart"" value=""1"">")
          sb.AppendLine("     <param name=""autoStart"" value=""1"">")
          sb.AppendLine("     <param name=""displaysize"" value=""0"">")
          sb.AppendLine("   <a href=""http://www.microsoft.com/windows/windowsmedia/download/AllDownloads.aspx"">" & Localization.GetString("GetWindowsMediaPlayerPlugIn", GalleryConfig.SharedResourceFile) & "</a>")
        End If
        sb.AppendLine("</object>")
        ctrlMediaPlayer.Text = sb.ToString()
      End If
    End Sub
  End Class
End Namespace


