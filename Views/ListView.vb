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

  Public Class ListView
    Inherits BaseView

    Private mUserRequest As GalleryUserRequest
    Private mCurrentItems As New ArrayList
    Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config
    Private mGalleryAuthorize As DotNetNuke.Modules.Gallery.Authorization

    Public Sub New(ByVal GalleryCont As GalleryControl)
      MyBase.New(GalleryCont)

      mUserRequest = GalleryCont.UserRequest
      mCurrentItems = mUserRequest.CurrentItems
      mGalleryAuthorize = GalleryCont.GalleryAuthorize
      mGalleryConfig = GalleryCont.GalleryConfig
    End Sub

    Public Overrides Sub CreateChildControls()
    End Sub

    Public Overrides Sub OnPreRender()
      Dim sb As New StringBuilder
      sb.AppendLine("function equalHeight(panel) {")
      sb.AppendLine("   panel.each(function() {")
      sb.AppendLine(" 	   var tallest = 0;")
      sb.AppendLine("      var divs = jQuery(this).children();")
      sb.AppendLine("      divs.each(function() {")
      sb.AppendLine("         var el = jQuery(this);")
      sb.AppendLine("         var thisHeight = el.outerHeight();")
      'sb.AppendLine("         alert('classname = ' + el.attr('class') + '\r\nouterHeight = ' + thisHeight + '\r\nheight = ' + el.height());")
      sb.AppendLine("         if(thisHeight > tallest) {")
      sb.AppendLine("              tallest = thisHeight;")
      sb.AppendLine("         }")
      sb.AppendLine("      });")
      sb.AppendLine("      divs.each(function() {")
      sb.AppendLine("         el = jQuery(this);")
      sb.AppendLine("         var newHeight = tallest - (el.innerHeight()-el.height());")
      sb.AppendLine("         if (el.attr('class') == 'Gallery_ListItemInfo') { newHeight = newHeight + 2;}")
      sb.AppendLine("         el.height(newHeight);")
      'sb.AppendLine("         alert('New height for ' + el.attr('class') + '=' + el.height() + '\r\nNew outerHeight = ' + el.outerHeight());")
      sb.AppendLine("      })")
      sb.AppendLine("   });")
      sb.AppendLine("}")
      sb.AppendLine()
      sb.AppendLine("jQuery(window).load(function() {equalHeight(jQuery('div.Gallery_ListItemPanel'))});")
      ScriptManager.RegisterStartupScript(GalleryControl, GetType(GalleryControl), "ListViewHeightFix", sb.ToString, True)
    End Sub

    Private Sub RenderGallery(ByVal wr As HtmlTextWriter)
      Dim item As IGalleryObjectInfo
      Dim album As GalleryFolder
      Dim file As GalleryFile

      Dim listItemPanelWidth As String
      Dim listItemPanelSize As String
      Dim listItemThumbSize As String

      If mCurrentItems.Count = 0 Then
        RenderInfo(wr, Localization.GetString("AlbumEmpty", mGalleryConfig.SharedResourceFile))
        Return
      End If

      listItemThumbSize = "width:" & Convert.ToString(mGalleryConfig.MaximumThumbWidth) & "px; height:" _
                              & Convert.ToString(mGalleryConfig.MaximumThumbHeight) & "px"

      listItemPanelWidth = "width:" & Convert.ToString(mGalleryConfig.MaximumThumbWidth + 50) + "px;"

      listItemPanelSize = listItemPanelWidth & " height:" _
                              & Convert.ToString(mGalleryConfig.MaximumThumbHeight + 60) + "px;"

      For Each item In mCurrentItems
        If TypeOf item Is GalleryFile Then
          file = CType(item, GalleryFile)
          ' Make compiler happy by assigning a value
          album = Nothing
        Else
          ' Make compiler happy by assigning a value
          file = Nothing
          album = CType(item, GalleryFolder)
        End If

        wr.RenderBeginTag(HtmlTextWriterTag.Tr)

        wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_ListCell")
        wr.RenderBeginTag(HtmlTextWriterTag.Td)

        wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_ListItem")
        wr.RenderBeginTag(HtmlTextWriterTag.Table) ' table container

        wr.RenderBeginTag(HtmlTextWriterTag.Tr)

        wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_ListItemHeader")
        wr.RenderBeginTag(HtmlTextWriterTag.Td)

        wr.RenderBeginTag(HtmlTextWriterTag.Table) ' table title & commands

        wr.RenderBeginTag(HtmlTextWriterTag.Tr)

        wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_ListItemTitle")
        wr.RenderBeginTag(HtmlTextWriterTag.Td) ' column for title   

        Dim strCurrentStrip As String = "currentstrip=" & mUserRequest.CurrentStrip.ToString

        'William Severance - Modified to append CurrentStrip parameter to URL as appropriate
        If Not mGalleryConfig.AllowPopup Then
          RenderImageButton(wr, Utils.AppendURLParameter(item.BrowserURL, strCurrentStrip), item.IconURL, GalleryControl.LocalizedText("Open"), "")
          RenderCommandButton(wr, Utils.AppendURLParameter(item.BrowserURL, strCurrentStrip), item.Title, "Gallery_AltHeaderText")
        Else
          RenderImageButton(wr, item.BrowserURL, item.IconURL, GalleryControl.LocalizedText("Open"), "")
          RenderCommandButton(wr, item.BrowserURL, item.Title, "Gallery_AltHeaderText")
        End If

        wr.RenderEndTag() ' td

        wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_ListItemCommands")
        wr.RenderBeginTag(HtmlTextWriterTag.Td) ' column for commands

        RenderCommands(wr, item)

        If item.IsFolder Then RenderAlbumCommands(wr, album)

        wr.RenderEndTag() ' </td> commands
        wr.RenderEndTag() ' </tr>
        wr.RenderEndTag() ' </table> title & commands

        wr.RenderEndTag() ' </td>
        wr.RenderEndTag() ' tr

        wr.RenderBeginTag(HtmlTextWriterTag.Tr) ' Gallery_Row for image & info

        wr.RenderBeginTag(HtmlTextWriterTag.Td)

        wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_ListItemPanel")
        wr.RenderBeginTag(HtmlTextWriterTag.Div)

        wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_ListItemLeftPanel")
        wr.AddAttribute(HtmlTextWriterAttribute.Style, listItemPanelWidth)
        wr.RenderBeginTag(HtmlTextWriterTag.Div)

        If TypeOf item Is GalleryFolder Then
          RenderAlbum(wr, item, listItemThumbSize)
        Else
          RenderFile(wr, item, listItemThumbSize)
        End If

        wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Vote")
        wr.RenderBeginTag(HtmlTextWriterTag.Span)
        If Not item.IsFolder Then
          If mGalleryAuthorize.ItemCanVote(item) Then
            Dim voteInfo As String = GalleryControl.LocalizedText("VoteInfo")
            voteInfo = voteInfo.Replace("[VoteValue]", item.Score.ToString)
            voteInfo = voteInfo.Replace("[VoteCount]", file.Votes.Count.ToString)

            If Not mGalleryConfig.AllowPopup Then
              RenderImageButton(wr, Utils.AppendURLParameter(item.VotingURL, strCurrentStrip), item.ScoreImageURL, voteInfo, "")
            Else
              RenderImageButton(wr, item.VotingURL, item.ScoreImageURL, voteInfo, "")
            End If
          End If
        ElseIf album.Size > 0 Then ' if this is an album render item count, instead of rating image
          Dim sizeInfo As String = GalleryControl.LocalizedText("AlbumSizeInfo")
          sizeInfo = sizeInfo.Replace("[ItemCount]", (album.Size - (album.IconItems.Count + album.UnApprovedItems.Count)).ToString)
          wr.Write(sizeInfo)
        End If
        wr.RenderEndTag() ' span voting/album item count
        wr.RenderEndTag() ' div

        wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_ListItemInfo")
        wr.RenderBeginTag(HtmlTextWriterTag.Table) ' table for item info

        If (mGalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Name) <> 0 Then
          Dim sb As New StringBuilder(item.Name)
          If (mGalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Size) <> 0 Then
            sb.Append(" ")
            Dim sizeInfo As String = Localization.GetString("FileSizeInfo", mGalleryConfig.SharedResourceFile)
            sizeInfo = sizeInfo.Replace("[FileSize]", Math.Ceiling(item.Size / 1024).ToString)
            sb.Append(sizeInfo)
          End If
          RenderItemInfo(wr, GalleryControl.LocalizedText("Name"), sb.ToString)
        End If

        If (mGalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Author) <> 0 Then
          RenderItemInfo(wr, GalleryControl.LocalizedText("Author"), item.Author)
        End If

        If (mGalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Client) <> 0 Then
          RenderItemInfo(wr, GalleryControl.LocalizedText("Client"), item.Client)
        End If

        If (mGalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Location) <> 0 Then
          RenderItemInfo(wr, GalleryControl.LocalizedText("Location"), item.Location)
        End If

        If (mGalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.CreatedDate) <> 0 Then
          RenderItemInfo(wr, GalleryControl.LocalizedText("CreatedDate"), DateToText(item.CreatedDate))
        End If

        If (mGalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.ApprovedDate) <> 0 Then
          RenderItemInfo(wr, GalleryControl.LocalizedText("ApprovedDate"), DateToText(item.ApprovedDate))
        End If

        Dim value As String
        If ((mGalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Description) <> 0) _
                AndAlso item.Description.Length > 0 Then
          value = "<p class=""Gallery_Description"">" & item.Description & "</p>"
        Else
          value = "&nbsp"
        End If
        RenderItemInfo(wr, "", value)

        wr.RenderEndTag() ' table item info
        wr.RenderEndTag() ' div
        wr.RenderEndTag() ' td
        wr.RenderEndTag() ' tr
        wr.RenderEndTag() ' table listitem
        wr.RenderEndTag() ' td
        wr.RenderEndTag() ' tr
      Next

    End Sub

    Private Sub RenderAlbum(ByVal wr As HtmlTextWriter, ByVal Album As IGalleryObjectInfo, ByVal ThumbSize As String)

      Dim newHeightA As String = String.Empty
      Dim newWidthA As String = String.Empty

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
      wr.AddAttribute(HtmlTextWriterAttribute.Style, ThumbSize)
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      RenderImageButton(wr, Album.BrowserURL, Album.ThumbnailURL, Album.Description, "")
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

    Private Sub RenderFile(ByVal wr As HtmlTextWriter, ByVal File As IGalleryObjectInfo, ByVal ThumbSize As String)
      ' table File            
      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_File Centered DropShadow")
      wr.RenderBeginTag(HtmlTextWriterTag.Table) ' table image & details

      wr.RenderBeginTag(HtmlTextWriterTag.Tr)

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileTL")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      RenderImage(wr, mGalleryConfig.GetImageURL("spacer_left.gif"), "", "")
      wr.RenderEndTag() ' td
      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileTC")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      wr.Write("&nbsp;")
      wr.RenderEndTag() ' td
      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileTR")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      RenderImage(wr, mGalleryConfig.GetImageURL("spacer_right.gif"), "", "")
      wr.RenderEndTag() ' td

      wr.RenderEndTag() ' tr

      wr.RenderBeginTag(HtmlTextWriterTag.Tr)

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileML")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      wr.Write("&nbsp;")
      wr.RenderEndTag() ' td

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileMC")
      wr.AddAttribute(HtmlTextWriterAttribute.Style, ThumbSize)
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      'William Severance - Modifed to append CurrentStrip parameter as appropriate
      If Not mGalleryConfig.AllowPopup Then
        RenderImageButton(wr, Utils.AppendURLParameter(File.BrowserURL, "currentstrip=" & mUserRequest.CurrentStrip.ToString), File.ThumbnailURL, File.Description, "")
      Else
        RenderImageButton(wr, File.BrowserURL, File.ThumbnailURL, File.Description, "")
      End If

      wr.RenderEndTag() ' td            

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileMR")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      wr.Write("&nbsp;")
      wr.RenderEndTag() ' td

      wr.RenderEndTag() ' tr

      wr.RenderBeginTag(HtmlTextWriterTag.Tr)

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileBL")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      wr.Write("&nbsp;")
      wr.RenderEndTag() ' td

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileBC")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      wr.Write("&nbsp;")
      wr.RenderEndTag() ' td

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileBR")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      wr.Write("&nbsp;")
      wr.RenderEndTag() ' td

      wr.RenderEndTag() ' tr
      wr.RenderEndTag() ' table            

    End Sub

    Private Sub RenderCommands(ByVal wr As HtmlTextWriter, ByVal Item As IGalleryObjectInfo)

      If mGalleryAuthorize.ItemCanSlideshow(Item) Then
        'William Severance - Modified to append CurrentStrip paramete to URL
        If Not mGalleryConfig.AllowPopup Then
          RenderImageButton(wr, AppendURLParameter(Item.SlideshowURL, "currentstrip=" & mUserRequest.CurrentStrip.ToString), mGalleryConfig.GetImageURL("s_movie.gif"), Localization.GetString("Slideshow.Tooltip", mGalleryConfig.SharedResourceFile), "")
        Else
          RenderImageButton(wr, Item.SlideshowURL, mGalleryConfig.GetImageURL("s_movie.gif"), Localization.GetString("Slideshow.Tooltip", mGalleryConfig.SharedResourceFile), "")
        End If
        wr.Write("&nbsp;")
      End If

      If mGalleryAuthorize.ItemCanViewExif(Item) Then
        If Not mGalleryConfig.AllowPopup Then
          RenderImageButton(wr, AppendURLParameter(Item.ExifURL, "currentstrip=" & mUserRequest.CurrentStrip.ToString), mGalleryConfig.GetImageURL("s_exif.gif"), Localization.GetString("EXIFData.Tooltip", mGalleryConfig.SharedResourceFile), "")
        Else
          RenderImageButton(wr, Item.ExifURL, mGalleryConfig.GetImageURL("s_exif.gif"), Localization.GetString("EXIFData.Tooltip", mGalleryConfig.SharedResourceFile), "")
        End If
        wr.Write("&nbsp;")
      End If

      If mGalleryAuthorize.ItemCanDownload(Item) Then
        RenderImageButton(wr, Item.DownloadURL, mGalleryConfig.GetImageURL("s_download.gif"), Localization.GetString("Download.Tooltip", mGalleryConfig.SharedResourceFile), "")
        wr.Write("&nbsp;")
      End If

      If mGalleryAuthorize.ItemCanEdit(Item) Then
        Dim editTooltip As String
        If Item.IsFolder Then
          editTooltip = Localization.GetString("MenuEdit.Tooltip", mGalleryConfig.SharedResourceFile)
        Else
          editTooltip = Localization.GetString("MenuEditFile.Tooltip", mGalleryConfig.SharedResourceFile)
        End If
        If Not mGalleryConfig.AllowPopup Then
          RenderImageButton(wr, AppendURLParameter(Item.EditURL, "currentstrip=" & mUserRequest.CurrentStrip.ToString), mGalleryConfig.GetImageURL("s_edit.gif"), editTooltip, "")
        Else
          RenderImageButton(wr, Item.EditURL, mGalleryConfig.GetImageURL("s_edit.gif"), editTooltip, "")
        End If
        wr.Write("&nbsp;")
      End If

    End Sub

    Private Sub RenderAlbumCommands(ByVal wr As HtmlTextWriter, ByVal Album As GalleryFolder)
      If mGalleryAuthorize.HasItemEditPermission(Album) Then
        RenderImageButton(wr, Album.AddSubAlbumURL, mGalleryConfig.GetImageURL("s_folder.gif"), Localization.GetString("MenuAddAlbum.Tooltip", mGalleryConfig.SharedResourceFile), "")
        wr.Write("&nbsp;")
      End If
      If mGalleryAuthorize.HasItemUploadPermission(Album) Then
        RenderImageButton(wr, Album.AddFileURL, mGalleryConfig.GetImageURL("s_new2.gif"), Localization.GetString("MenuAddFile.Tooltip", mGalleryConfig.SharedResourceFile), "")
        wr.Write("&nbsp;")
      End If
      If mGalleryAuthorize.HasItemEditPermission(Album) Then
        RenderImageButton(wr, Utils.AppendURLParameter(Album.MaintenanceURL, "currentstrip=" & mUserRequest.CurrentStrip.ToString), mGalleryConfig.GetImageURL("s_bookopen.gif"), Localization.GetString("MenuMaintenance.Tooltip", mGalleryConfig.SharedResourceFile), "")
      End If
    End Sub

    Private Sub RenderItemInfo(ByVal wr As HtmlTextWriter, ByVal PropertyName As String, ByVal PropertyValue As String)
      wr.RenderBeginTag(HtmlTextWriterTag.Tr)

      If String.IsNullOrEmpty(PropertyName) Then
        wr.AddAttribute(HtmlTextWriterAttribute.Colspan, "2")
      Else
        wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_ListItemProperty")
        wr.RenderBeginTag(HtmlTextWriterTag.Td)
        wr.Write(PropertyName)
        wr.RenderEndTag() ' td
      End If

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_ListItemValue")
      wr.RenderBeginTag(HtmlTextWriterTag.Td)
      wr.Write(PropertyValue)
      wr.RenderEndTag() ' td

      wr.RenderEndTag() ' tr

    End Sub

    Public Overrides Sub Render(ByVal wr As HtmlTextWriter)
      RenderTableBegin(wr, "Gallery_Content_List")
      RenderGallery(wr)
      RenderTableEnd(wr) '
    End Sub

  End Class

End Namespace