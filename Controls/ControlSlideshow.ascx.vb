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

Imports System.Text
Imports System.IO
Imports System.Xml
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.Modules.Gallery.Utils

Namespace DotNetNuke.Modules.Gallery.WebControls

  Public Class Slideshow
    Inherits GalleryWebControlBase

    Private _CurrentRequest As GalleryViewerRequest

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

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
      ErrorMessage.Visible = False

      _CurrentRequest = New GalleryViewerRequest(ModuleId, Utils.GetSort(GalleryConfig), Utils.GetSortDESC(GalleryConfig))

      If CurrentRequest Is Nothing OrElse Not CurrentRequest.Folder.IsPopulated Then
        Response.Redirect(ApplicationURL)
      End If
      If Not IsPostBack Then
        If CurrentRequest.Folder.IsBrowsable Then



          celPicture.Width = GalleryConfig.FixedWidth.ToString
          celPicture.Height = GalleryConfig.FixedHeight.ToString
          'WES - localized loading message
          ErrorMessage.Text = Localization.GetString("Loading", GalleryConfig.SharedResourceFile)

          Dim albumPath As String = CurrentRequest.Folder.Path
          Dim slideSpeed As String = GalleryConfig.SlideshowSpeed.ToString


          'Generate Clientside Javascript for Slideshow

          Dim isPopup As Boolean = Me.Parent.TemplateControl.AppRelativeVirtualPath.EndsWith("aspx")

          If isPopup Then
            DotNetNuke.Framework.jQuery.RegisterScript(Page)
          Else
            DotNetNuke.Framework.jQuery.RequestRegistration()
          End If

          Dim sb As New StringBuilder

          sb.Append("    var baseTitle = '")
          If isPopup Then
            sb.Append(CType(Me.Parent.TemplateControl, GalleryPageBase).Title)
          Else
            sb.Append(CType(Me.Page, CDefault).Title)
          End If
          sb.AppendLine("';")

          ' Write all of the image data out
          Dim image As GalleryFile
          Dim count As Integer
          Dim startItemNumber As Integer = CurrentRequest.CurrentItemNumber
          Dim totalItemCount As Integer = CurrentRequest.BrowsableItems.Count
          Dim startImageURL As String = CurrentRequest.CurrentItem.URL
          sb.Append("    jQuery('#")
          sb.Append(celPicture.ClientID)
          sb.Append("').data('pics', [")
          Do
            image = CurrentRequest.CurrentItem
            AppendPicData(sb, count, image)
            count += 1
            CurrentRequest.MoveNext()
          Loop Until CurrentRequest.CurrentItemNumber = startItemNumber
          sb.Remove(sb.Length - 3, 3) ' remove trailing command and vbCrlf characters
          sb.AppendLine("]);")
          sb.Append("    runSlideShow(0, ")
          sb.Append(slideSpeed.ToString)
          sb.AppendLine(", 500);")

          Page.ClientScript.RegisterStartupScript(Me.GetType, ClientID & "_SlideShow", sb.ToString, True)
        Else
          ErrorMessage.Visible = True
          'WES - localized album empty message
          ErrorMessage.Text = Localization.GetString("AlbumEmpty", GalleryConfig.SharedResourceFile) 'Album contains no images!
        End If
      End If
    End Sub

    Private Sub AppendPicData(ByVal sb As StringBuilder, ByVal i As Integer, ByVal data As GalleryFile)
      With sb
        .Append("{src:'")
        sb.Append(JSEncode(data.URL))
        sb.Append("', title:'")
        sb.Append(JSEncode(data.Title.Replace(vbCrLf, "<br />")))
        sb.Append("', desc:'")
        sb.Append(JSEncode(data.Description.Replace(vbCrLf, "<br />")))
        sb.AppendLine("'},")
      End With
    End Sub
  End Class

End Namespace