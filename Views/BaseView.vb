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
  ''' Base object used by various gallery view code
  ''' </summary>
  ''' <remarks></remarks>
  Public Class BaseView

    Private mGalleryControl As GalleryControl

    Public Sub New(ByVal GalleryControl As GalleryControl)
      mGalleryControl = GalleryControl
    End Sub 'New

    Public ReadOnly Property GalleryControl() As GalleryControl
      Get
        Return mGalleryControl
      End Get
    End Property

    Public ReadOnly Property Controls() As ControlCollection
      Get
        Return mGalleryControl.Controls
      End Get
    End Property

    Public Overridable Sub CreateChildControls()
    End Sub 'CreateChildControls

    Public Overridable Sub OnPreRender()
    End Sub 'OnPreRender

    Public Overridable Sub Render(ByVal wr As HtmlTextWriter)
    End Sub 'Render

    Protected Sub RenderTableBegin(ByVal wr As HtmlTextWriter, ByVal ClassName As String) ' Begin table in which we will render object (cellspacing, cellpadding, borderwidth)

      wr.AddAttribute(HtmlTextWriterAttribute.Class, ClassName)
      wr.AddAttribute(HtmlTextWriterAttribute.Id, "GalleryContent")
      wr.RenderBeginTag(HtmlTextWriterTag.Table)
    End Sub 'RenderTableBegin

    Protected Sub RenderTableEnd(ByVal wr As HtmlTextWriter)
      wr.RenderEndTag()
    End Sub ' End table in which object was rendered

    Protected Sub RenderInfo(ByVal wr As HtmlTextWriter, ByVal Info As String)
      wr.RenderBeginTag(HtmlTextWriterTag.Tr)

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumEmpty")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      wr.Write(Info)
      wr.RenderEndTag() ' </td>

      wr.RenderEndTag() ' </tr>
    End Sub

    Public Shared Sub RenderImage(ByVal wr As HtmlTextWriter, ByVal ImageURL As String, ByVal Tooltip As String, ByVal Css As String)
      wr.AddAttribute(HtmlTextWriterAttribute.Border, "0")
      wr.AddAttribute(HtmlTextWriterAttribute.Src, ImageURL)

      If Css.Length > 0 Then
        wr.AddAttribute(HtmlTextWriterAttribute.Class, Css)
      End If

      'xhtml requires an alt tag, even if it is empty - HWZassenhaus 9/28/2008
      If Tooltip.Length > 0 Then
        wr.AddAttribute(HtmlTextWriterAttribute.Title, Tooltip)
        wr.AddAttribute(HtmlTextWriterAttribute.Alt, Tooltip)
      Else
        wr.AddAttribute(HtmlTextWriterAttribute.Alt, "")
      End If

      wr.RenderBeginTag(HtmlTextWriterTag.Img) '<img>
      wr.RenderEndTag() '/>'

    End Sub

    Public Shared Sub RenderImageButton(ByVal wr As HtmlTextWriter, ByVal URL As String, ByVal ImageURL As String, ByVal Tooltip As String, ByVal Css As String)
      wr.AddAttribute(HtmlTextWriterAttribute.Href, URL.Replace("~/", ""))
      wr.RenderBeginTag(HtmlTextWriterTag.A)
      RenderImage(wr, ImageURL, Tooltip, Css)
      wr.RenderEndTag() '</a>
    End Sub

    Public Shared Sub RenderCommandButton(ByVal wr As HtmlTextWriter, ByVal URL As String, ByVal Text As String, ByVal Css As String)
      wr.AddAttribute(HtmlTextWriterAttribute.Href, URL.Replace("~/", ""))

      If Css.Length > 0 Then
        wr.AddAttribute(HtmlTextWriterAttribute.Class, Css)
      End If

      wr.RenderBeginTag(HtmlTextWriterTag.A)
      wr.Write(Text)
      wr.RenderEndTag() '</a>
    End Sub

  End Class

End Namespace