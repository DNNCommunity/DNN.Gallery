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

Imports System.Diagnostics
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

  Public Class StandardView
    Inherits BaseView

    Private mUserRequest As GalleryUserRequest
    Private mCurrentItems As New ArrayList
    Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config
    Private mGalleryAuthorize As DotNetNuke.Modules.Gallery.Authorization
    Private ctlMenu As MediaMenu

    Public Sub New(ByVal GalleryCont As GalleryControl)
      MyBase.New(GalleryCont)

      mUserRequest = GalleryCont.UserRequest
      mCurrentItems = mUserRequest.CurrentItems
      mGalleryAuthorize = GalleryCont.GalleryAuthorize
      mGalleryConfig = GalleryCont.GalleryConfig

    End Sub

    Public Overrides Sub CreateChildControls()
      Dim item As IGalleryObjectInfo
      For Each item In mCurrentItems
        ctlMenu = New MediaMenu(GalleryControl.ModuleId, item)
        Controls.Add(ctlMenu)
      Next
    End Sub

    Public Overrides Sub OnPreRender()
    End Sub

    Private Sub RenderGallery(ByVal wr As HtmlTextWriter)
      Dim item As IGalleryObjectInfo
      Dim album As GalleryFolder
      Dim file As GalleryFile
      Dim rowCount As Integer = 0
      Dim row As Integer
      Dim itm As Integer
      Dim w As Integer
      Dim cellStyle As String
      Dim thumbStyle As String

      If mCurrentItems.Count = 0 Then
        RenderInfo(wr, Localization.GetString("AlbumEmpty", mGalleryConfig.SharedResourceFile))
        Exit Sub
      End If

      'Percentage width must be an integer xhtml 1.0 - HWZassenhaus 9/24/2008
      w = Convert.ToInt32(100 / mGalleryConfig.StripWidth)
      cellStyle = "width:" & Unit.Percentage(w).ToString
      thumbStyle = "width:" & (mGalleryConfig.MaximumThumbWidth).ToString & "px; height:" & (mGalleryConfig.MaximumThumbWidth).ToString & "px;"

      rowCount = CType(Math.Ceiling(mCurrentItems.Count / mGalleryConfig.StripWidth), Integer)

      If Not rowCount = 0 Then
        For row = 0 To rowCount - 1
          wr.RenderBeginTag(HtmlTextWriterTag.Tr)
          Dim itmFrom As Integer = row * mGalleryConfig.StripWidth
          Dim itmTo As Integer = itmFrom + mGalleryConfig.StripWidth - 1

          For itm = itmFrom To itmTo
            ' image column
            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Body Gallery_StandardCell")
            wr.AddAttribute(HtmlTextWriterAttribute.Style, cellStyle)
            wr.RenderBeginTag(HtmlTextWriterTag.Td)

            If itm < mCurrentItems.Count Then
              item = CType(mCurrentItems.Item(itm), IGalleryObjectInfo)
              wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Standard")
              wr.RenderBeginTag(HtmlTextWriterTag.Table) ' table image & details

              ' Gallery_Row for thumbnail image
              wr.RenderBeginTag(HtmlTextWriterTag.Tr)
              wr.RenderBeginTag(HtmlTextWriterTag.Td)

              If TypeOf item Is GalleryFile Then
                file = CType(mCurrentItems.Item(itm), GalleryFile)
                RenderFile(wr, file, thumbStyle)
              Else
                file = Nothing
                album = CType(mCurrentItems.Item(itm), GalleryFolder)
                RenderAlbum(wr, album, thumbStyle)
              End If

              wr.RenderEndTag() ' td thumb
              wr.RenderEndTag() ' tr thumb

              'voting
              If mGalleryAuthorize.ItemCanVote(item) Then
                wr.RenderBeginTag(HtmlTextWriterTag.Tr) ' voting
                wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Vote")
                wr.RenderBeginTag(HtmlTextWriterTag.Td)

                Dim voteInfo As String = GalleryControl.LocalizedText("VoteInfo")
                voteInfo = voteInfo.Replace("[VoteValue]", file.Score.ToString)
                voteInfo = voteInfo.Replace("[VoteCount]", file.Votes.Count.ToString)
                RenderImageButton(wr, AppendURLParameter(item.VotingURL, "currentstrip=" & mUserRequest.CurrentStrip.ToString), item.ScoreImageURL, voteInfo, "")
                wr.RenderEndTag() ' td voting
                wr.RenderEndTag() ' tr voting
              End If



              If (mGalleryConfig.TextDisplayOptions And GalleryDisplayOption.Title) <> 0 Then
                wr.RenderBeginTag(HtmlTextWriterTag.Tr) ' title
                wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Title")
                wr.RenderBeginTag(HtmlTextWriterTag.Td)
                If Not mGalleryConfig.AllowPopup Then
                  RenderCommandButton(wr, Utils.AppendURLParameter(item.BrowserURL, "currentstrip=" & mUserRequest.CurrentStrip.ToString), item.Title, "Gallery_AltHeaderText")
                Else
                  RenderCommandButton(wr, item.BrowserURL, item.Title, "Gallery_AltHeaderText")
                End If
                wr.RenderEndTag() ' td title
                wr.RenderEndTag() ' tr title
              End If
              wr.RenderBeginTag(HtmlTextWriterTag.Tr) ' info
              wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Info")
              wr.RenderBeginTag(HtmlTextWriterTag.Td)
              wr.Write(item.ItemInfo)
              wr.RenderEndTag() ' td info
              wr.RenderEndTag() ' tr info

              wr.RenderBeginTag(HtmlTextWriterTag.Tr) ' description
              wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Description")
              wr.RenderBeginTag(HtmlTextWriterTag.Td)

              If ((mGalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Description) <> 0) _
                      AndAlso Not item.Description.Length = 0 Then
                wr.Write(item.Description)
              Else
                wr.Write("&nbsp;")
              End If

              wr.RenderEndTag() ' td description
              wr.RenderEndTag() ' tr description
              wr.RenderEndTag() ' table image & details
            End If
            wr.RenderEndTag() ' td
          Next
          wr.RenderEndTag() ' tr
        Next
      End If
    End Sub

    Private Sub RenderAlbum(ByVal wr As HtmlTextWriter, ByVal Album As IGalleryObjectInfo, ByVal ThumbStyle As String)

      ' table album

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Album Centered")
      wr.RenderBeginTag(HtmlTextWriterTag.Table) ' table image & details

      wr.RenderBeginTag(HtmlTextWriterTag.Tr)

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumTL")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      RenderImage(wr, mGalleryConfig.GetImageURL("spacer_left.gif"), "", "")
      wr.RenderEndTag() ' td
      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumTC")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      wr.Write("&nbsp;")
      wr.RenderEndTag() ' td
      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumTR")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      RenderImage(wr, mGalleryConfig.GetImageURL("spacer_right.gif"), "", "")
      wr.RenderEndTag() ' td

      wr.RenderEndTag() ' tr

      wr.RenderBeginTag(HtmlTextWriterTag.Tr)

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumML")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      wr.Write("&nbsp;")
      wr.RenderEndTag() ' td

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumMC")
      wr.AddAttribute(HtmlTextWriterAttribute.Style, ThumbStyle)
      wr.RenderBeginTag(HtmlTextWriterTag.Td)

      Dim objControl As Control
      For Each objControl In Controls
        If TypeOf objControl Is MediaMenu Then
          Dim objMediaMenu As MediaMenu = CType(objControl, MediaMenu)
          If objMediaMenu.ID = GetGalleryObjectMenuID(Album) Then
            objMediaMenu.RenderControl(wr)
          End If
        End If
      Next

      wr.RenderEndTag() ' td            

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumMR")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      wr.Write("&nbsp;")
      wr.RenderEndTag() ' td

      wr.RenderEndTag() ' tr

      wr.RenderBeginTag(HtmlTextWriterTag.Tr)

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumBL")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      wr.Write("&nbsp;")
      wr.RenderEndTag() ' td

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumBC")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      wr.Write("&nbsp;")
      wr.RenderEndTag() ' td

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumBR")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      wr.Write("&nbsp;")
      wr.RenderEndTag() ' td

      wr.RenderEndTag() ' tr
      wr.RenderEndTag() ' table            

    End Sub

    Private Sub RenderFile(ByVal wr As HtmlTextWriter, ByVal File As IGalleryObjectInfo, ByVal ThumbStyle As String)
      ' table File            

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_File Centered Gallery_DropShadow")
      wr.RenderBeginTag(HtmlTextWriterTag.Table) ' table image & details

      wr.RenderBeginTag(HtmlTextWriterTag.Tr)

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileTL")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      wr.RenderEndTag() ' td
      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileTC")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      wr.RenderEndTag() ' td
      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileTR")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      wr.RenderEndTag() ' td

      wr.RenderEndTag() ' tr

      wr.RenderBeginTag(HtmlTextWriterTag.Tr)

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileML")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      wr.RenderEndTag() ' td

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileMC")
      wr.AddAttribute(HtmlTextWriterAttribute.Style, ThumbStyle)
      wr.RenderBeginTag(HtmlTextWriterTag.Td)

      Dim objControl As Control
      For Each objControl In Controls
        Dim objMediaMenu As MediaMenu = CType(objControl, MediaMenu)
        If objMediaMenu.ID = GetGalleryObjectMenuID(File) Then
          objMediaMenu.RenderControl(wr)
        End If
      Next

      wr.RenderEndTag() ' td            

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileMR")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      wr.RenderEndTag() ' td

      wr.RenderEndTag() ' tr

      wr.RenderBeginTag(HtmlTextWriterTag.Tr)

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileBL")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      wr.RenderEndTag() ' td

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileBC")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      wr.RenderEndTag() ' td

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileBR")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      'wr.Write("&nbsp;")
      wr.RenderEndTag() ' td

      wr.RenderEndTag() ' tr
      wr.RenderEndTag() ' table            

    End Sub

    ' Note: The RenderCommand method is never called in the StandardView

    Private Sub RenderCommand(ByVal wr As HtmlTextWriter, ByVal Item As IGalleryObjectInfo)
      wr.RenderBeginTag(HtmlTextWriterTag.Tr) ' command
      wr.AddAttribute(HtmlTextWriterAttribute.Valign, "top")
      wr.AddAttribute(HtmlTextWriterAttribute.Align, "center")
      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_RowHighLight")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)

      If mGalleryAuthorize.ItemCanSlideshow(Item) Then
        'William Severance - Modified to append CurrentStrip paramete to URL
        If Not mGalleryConfig.AllowPopup Then
          RenderImageButton(wr, AppendURLParameter(Item.SlideshowURL, "currentstrip=" & mUserRequest.CurrentStrip.ToString), mGalleryConfig.GetImageURL("s_movie.gif"), "View slideshow...", "")
        Else
          RenderImageButton(wr, Item.SlideshowURL, mGalleryConfig.GetImageURL("s_movie.gif"), "View slideshow...", "")
        End If
        wr.Write("&nbsp;")
      End If

      If mGalleryAuthorize.ItemCanViewExif(Item) Then
        If Not mGalleryConfig.AllowPopup Then
          RenderImageButton(wr, AppendURLParameter(Item.ExifURL, "currentstrip=" & mUserRequest.CurrentStrip.ToString), mGalleryConfig.GetImageURL("s_exif.gif"), "View Exif Metadata...", "")
        Else
          RenderImageButton(wr, Item.ExifURL, mGalleryConfig.GetImageURL("s_exif.gif"), "View Exif Metadata...", "")
        End If
        wr.Write("&nbsp;")
      End If

      If mGalleryAuthorize.ItemCanDownload(Item) Then
        RenderImageButton(wr, Item.DownloadURL, mGalleryConfig.GetImageURL("s_download.gif"), "Download...", "")
        wr.Write("&nbsp;")
      End If

      If mGalleryAuthorize.ItemCanEdit(Item) Then
        If Not mGalleryConfig.AllowPopup Then
          RenderImageButton(wr, AppendURLParameter(Item.EditURL, "currentstrip=" & mUserRequest.CurrentStrip.ToString), mGalleryConfig.GetImageURL("s_edit.gif"), "Edit...", "")
        Else
          RenderImageButton(wr, Item.EditURL, mGalleryConfig.GetImageURL("s_edit.gif"), "Edit...", "")
        End If
        wr.Write("&nbsp;")
      End If

      wr.RenderEndTag() ' td command
      wr.RenderEndTag() ' tr command

    End Sub

    ' Note: The RenderAlbumCommand method is never called in the StandardView

    Private Sub RenderAlbumCommand(ByVal wr As HtmlTextWriter, ByVal Album As GalleryFolder)

      RenderImageButton(wr, Album.AddSubAlbumURL, mGalleryConfig.GetImageURL("s_folder.gif"), "Add sub album...", "")
      wr.Write("&nbsp;")
      RenderImageButton(wr, Album.AddFileURL, mGalleryConfig.GetImageURL("s_new2.gif"), "Add files...", "")
      wr.Write("&nbsp;")

      RenderImageButton(wr, Utils.AppendURLParameter(Album.MaintenanceURL, "currentstrip=" & mUserRequest.CurrentStrip.ToString), mGalleryConfig.GetImageURL("s_bookopen.gif"), "Maintenance...", "")
    End Sub

    Public Overrides Sub Render(ByVal wr As HtmlTextWriter)
      RenderTableBegin(wr, "Gallery_Content_Standard")
      RenderGallery(wr)
      RenderTableEnd(wr) '
    End Sub

  End Class

End Namespace