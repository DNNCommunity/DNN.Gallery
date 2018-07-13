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
Imports System.IO
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports DotNetNuke
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Modules.Gallery.Config
Imports DotNetNuke.Modules.Gallery.Utils

Namespace DotNetNuke.Modules.Gallery.Views

  Public Class NoConfigView
    Inherits BaseView

    Public Sub New(ByVal GalleryCont As GalleryControl)
      MyBase.New(GalleryCont)
    End Sub

    Public Overrides Sub CreateChildControls()
    End Sub

    Public Overrides Sub OnPreRender()
    End Sub

    Private Sub RenderGallery(ByVal wr As HtmlTextWriter)
      wr.RenderBeginTag(HtmlTextWriterTag.Tr) ' <tr>

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "NormalRed Gallery_Error")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      wr.Write(GalleryControl.LocalizedText("NoConfiguration"))

      wr.RenderEndTag() ' </td>
      wr.RenderEndTag() ' </tr>
    End Sub

    Public Overrides Sub Render(ByVal wr As HtmlTextWriter)
      RenderTableBegin(wr, "Gallery_Content")
      RenderGallery(wr)
      RenderTableEnd(wr)
    End Sub

  End Class

End Namespace