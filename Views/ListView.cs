using DotNetNuke;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Services;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;
using System.Web.Security;
using System.Web.Profile;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
//
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2011 by DotNetNuke Corp. 

//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
//

using System.IO;
using static DotNetNuke.Modules.Gallery.Config;
using static DotNetNuke.Modules.Gallery.Utils;

namespace DotNetNuke.Modules.Gallery.Views
{

	public class ListView : BaseView
	{

		private GalleryUserRequest mUserRequest;
		private ArrayList mCurrentItems = new ArrayList();
		private DotNetNuke.Modules.Gallery.Config mGalleryConfig;

		private DotNetNuke.Modules.Gallery.Authorization mGalleryAuthorize;
		public ListView(GalleryControl GalleryCont) : base(GalleryCont)
		{

			mUserRequest = GalleryCont.UserRequest;
			mCurrentItems = mUserRequest.CurrentItems();
			mGalleryAuthorize = GalleryCont.GalleryAuthorize;
			mGalleryConfig = GalleryCont.GalleryConfig;
		}

		public override void CreateChildControls()
		{
		}

		public override void OnPreRender()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("function equalHeight(panel) {");
			sb.AppendLine("   panel.each(function() {");
			sb.AppendLine(" \t   var tallest = 0;");
			sb.AppendLine("      var divs = jQuery(this).children();");
			sb.AppendLine("      divs.each(function() {");
			sb.AppendLine("         var el = jQuery(this);");
			sb.AppendLine("         var thisHeight = el.outerHeight();");
			//sb.AppendLine("         alert('classname = ' + el.attr('class') + '\r\nouterHeight = ' + thisHeight + '\r\nheight = ' + el.height());")
			sb.AppendLine("         if(thisHeight > tallest) {");
			sb.AppendLine("              tallest = thisHeight;");
			sb.AppendLine("         }");
			sb.AppendLine("      });");
			sb.AppendLine("      divs.each(function() {");
			sb.AppendLine("         el = jQuery(this);");
			sb.AppendLine("         var newHeight = tallest - (el.innerHeight()-el.height());");
			sb.AppendLine("         if (el.attr('class') == 'Gallery_ListItemInfo') { newHeight = newHeight + 2;}");
			sb.AppendLine("         el.height(newHeight);");
			//sb.AppendLine("         alert('New height for ' + el.attr('class') + '=' + el.height() + '\r\nNew outerHeight = ' + el.outerHeight());")
			sb.AppendLine("      })");
			sb.AppendLine("   });");
			sb.AppendLine("}");
			sb.AppendLine();
			sb.AppendLine("jQuery(window).load(function() {equalHeight(jQuery('div.Gallery_ListItemPanel'))});");
			ScriptManager.RegisterStartupScript(GalleryControl, typeof(GalleryControl), "ListViewHeightFix", sb.ToString(), true);
		}

		private void RenderGallery(HtmlTextWriter wr)
		{
			IGalleryObjectInfo item = null;
			GalleryFolder album = null;
			GalleryFile file = null;

			string listItemPanelWidth = null;
			string listItemPanelSize = null;
			string listItemThumbSize = null;

			if (mCurrentItems.Count == 0) {
				RenderInfo(wr, Localization.GetString("AlbumEmpty", mGalleryConfig.SharedResourceFile));
				return;
			}

			listItemThumbSize = "width:" + Convert.ToString(mGalleryConfig.MaximumThumbWidth) + "px; height:" + Convert.ToString(mGalleryConfig.MaximumThumbHeight) + "px";

			listItemPanelWidth = "width:" + Convert.ToString(mGalleryConfig.MaximumThumbWidth + 50) + "px;";

			listItemPanelSize = listItemPanelWidth + " height:" + Convert.ToString(mGalleryConfig.MaximumThumbHeight + 60) + "px;";

			foreach (IGalleryObjectInfo item_loopVariable in mCurrentItems) {
				item = item_loopVariable;
				if (item is GalleryFile) {
					file = (GalleryFile)item;
					// Make compiler happy by assigning a value
					album = null;
				} else {
					// Make compiler happy by assigning a value
					file = null;
					album = (GalleryFolder)item;
				}

				wr.RenderBeginTag(HtmlTextWriterTag.Tr);

				wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_ListCell");
				wr.RenderBeginTag(HtmlTextWriterTag.Td);

				wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_ListItem");
				wr.RenderBeginTag(HtmlTextWriterTag.Table);
				// table container

				wr.RenderBeginTag(HtmlTextWriterTag.Tr);

				wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_ListItemHeader");
				wr.RenderBeginTag(HtmlTextWriterTag.Td);

				wr.RenderBeginTag(HtmlTextWriterTag.Table);
				// table title & commands

				wr.RenderBeginTag(HtmlTextWriterTag.Tr);

				wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_ListItemTitle");
				wr.RenderBeginTag(HtmlTextWriterTag.Td);
				// column for title   

				string strCurrentStrip = "currentstrip=" + mUserRequest.CurrentStrip.ToString();

				//William Severance - Modified to append CurrentStrip parameter to URL as appropriate
				if (!mGalleryConfig.AllowPopup) {
					RenderImageButton(wr, Utils.AppendURLParameter(item.BrowserURL, strCurrentStrip), item.IconURL, GalleryControl.LocalizedText("Open"), "");
					RenderCommandButton(wr, Utils.AppendURLParameter(item.BrowserURL, strCurrentStrip), item.Title, "Gallery_AltHeaderText");
				} else {
					RenderImageButton(wr, item.BrowserURL, item.IconURL, GalleryControl.LocalizedText("Open"), "");
					RenderCommandButton(wr, item.BrowserURL, item.Title, "Gallery_AltHeaderText");
				}

				wr.RenderEndTag();
				// td

				wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_ListItemCommands");
				wr.RenderBeginTag(HtmlTextWriterTag.Td);
				// column for commands

				RenderCommands(wr, item);

				if (item.IsFolder)
					RenderAlbumCommands(wr, album);

				wr.RenderEndTag();
				// </td> commands
				wr.RenderEndTag();
				// </tr>
				wr.RenderEndTag();
				// </table> title & commands

				wr.RenderEndTag();
				// </td>
				wr.RenderEndTag();
				// tr

				wr.RenderBeginTag(HtmlTextWriterTag.Tr);
				// Gallery_Row for image & info

				wr.RenderBeginTag(HtmlTextWriterTag.Td);

				wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_ListItemPanel");
				wr.RenderBeginTag(HtmlTextWriterTag.Div);

				wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_ListItemLeftPanel");
				wr.AddAttribute(HtmlTextWriterAttribute.Style, listItemPanelWidth);
				wr.RenderBeginTag(HtmlTextWriterTag.Div);

				if (item is GalleryFolder) {
					RenderAlbum(wr, item, listItemThumbSize);
				} else {
					RenderFile(wr, item, listItemThumbSize);
				}

				wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Vote");
				wr.RenderBeginTag(HtmlTextWriterTag.Span);
				if (!item.IsFolder) {
					if (mGalleryAuthorize.ItemCanVote(item)) {
						string voteInfo = GalleryControl.LocalizedText("VoteInfo");
						voteInfo = voteInfo.Replace("[VoteValue]", item.Score.ToString());
						voteInfo = voteInfo.Replace("[VoteCount]", file.Votes.Count.ToString());

						if (!mGalleryConfig.AllowPopup) {
							RenderImageButton(wr, Utils.AppendURLParameter(item.VotingURL, strCurrentStrip), item.ScoreImageURL, voteInfo, "");
						} else {
							RenderImageButton(wr, item.VotingURL, item.ScoreImageURL, voteInfo, "");
						}
					}
				// if this is an album render item count, instead of rating image
				} else if (album.Size > 0) {
					string sizeInfo = GalleryControl.LocalizedText("AlbumSizeInfo");
					sizeInfo = sizeInfo.Replace("[ItemCount]", (album.Size - (album.IconItems.Count + album.UnApprovedItems.Count)).ToString());
					wr.Write(sizeInfo);
				}
				wr.RenderEndTag();
				// span voting/album item count
				wr.RenderEndTag();
				// div

				wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_ListItemInfo");
				wr.RenderBeginTag(HtmlTextWriterTag.Table);
				// table for item info

				if ((mGalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.Name) != 0) {
					StringBuilder sb = new StringBuilder(item.Name);
					if ((mGalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.Size) != 0) {
						sb.Append(" ");
						string sizeInfo = Localization.GetString("FileSizeInfo", mGalleryConfig.SharedResourceFile);
						sizeInfo = sizeInfo.Replace("[FileSize]", Math.Ceiling((double)item.Size / 1024).ToString());
						sb.Append(sizeInfo);
					}
					RenderItemInfo(wr, GalleryControl.LocalizedText("Name"), sb.ToString());
				}

				if ((mGalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.Author) != 0) {
					RenderItemInfo(wr, GalleryControl.LocalizedText("Author"), item.Author);
				}

				if ((mGalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.Client) != 0) {
					RenderItemInfo(wr, GalleryControl.LocalizedText("Client"), item.Client);
				}

				if ((mGalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.Location) != 0) {
					RenderItemInfo(wr, GalleryControl.LocalizedText("Location"), item.Location);
				}

				if ((mGalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.CreatedDate) != 0) {
					RenderItemInfo(wr, GalleryControl.LocalizedText("CreatedDate"), Utils.DateToText(item.CreatedDate));
				}

				if ((mGalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.ApprovedDate) != 0) {
					RenderItemInfo(wr, GalleryControl.LocalizedText("ApprovedDate"), Utils.DateToText(item.ApprovedDate));
				}

				string value = null;
				if (((mGalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.Description) != 0) && item.Description.Length > 0) {
					value = "<p class=\"Gallery_Description\">" + item.Description + "</p>";
				} else {
					value = "&nbsp";
				}
				RenderItemInfo(wr, "", value);

				wr.RenderEndTag();
				// table item info
				wr.RenderEndTag();
				// div
				wr.RenderEndTag();
				// td
				wr.RenderEndTag();
				// tr
				wr.RenderEndTag();
				// table listitem
				wr.RenderEndTag();
				// td
				wr.RenderEndTag();
				// tr
			}

		}


		private void RenderAlbum(HtmlTextWriter wr, IGalleryObjectInfo Album, string ThumbSize)
		{
			string newHeightA = string.Empty;
			string newWidthA = string.Empty;

			// table album
			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Album Centered");
			wr.RenderBeginTag(HtmlTextWriterTag.Table);
			// table image & details

			wr.RenderBeginTag(HtmlTextWriterTag.Tr);

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumTL");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			RenderImage(wr, mGalleryConfig.GetImageURL("spacer_left.gif"), "", "");
			wr.RenderEndTag();
			// td
			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumTC");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			wr.Write("&nbsp;");
			wr.RenderEndTag();
			// td
			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumTR");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			RenderImage(wr, mGalleryConfig.GetImageURL("spacer_right.gif"), "", "");
			wr.RenderEndTag();
			// td

			wr.RenderEndTag();
			// tr

			wr.RenderBeginTag(HtmlTextWriterTag.Tr);

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumML");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			wr.Write("&nbsp;");
			wr.RenderEndTag();
			// td

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumMC");
			wr.AddAttribute(HtmlTextWriterAttribute.Style, ThumbSize);
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			RenderImageButton(wr, Album.BrowserURL, Album.ThumbnailURL, Album.Description, "");
			wr.RenderEndTag();
			// td            

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumMR");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			wr.Write("&nbsp;");
			wr.RenderEndTag();
			// td

			wr.RenderEndTag();
			// tr

			wr.RenderBeginTag(HtmlTextWriterTag.Tr);

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumBL");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			wr.Write("&nbsp;");
			wr.RenderEndTag();
			// td

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumBC");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			wr.Write("&nbsp;");
			wr.RenderEndTag();
			// td

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumBR");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			wr.Write("&nbsp;");
			wr.RenderEndTag();
			// td

			wr.RenderEndTag();
			// tr
			wr.RenderEndTag();
			// table            

		}

		private void RenderFile(HtmlTextWriter wr, IGalleryObjectInfo File, string ThumbSize)
		{
			// table File            
			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_File Centered DropShadow");
			wr.RenderBeginTag(HtmlTextWriterTag.Table);
			// table image & details

			wr.RenderBeginTag(HtmlTextWriterTag.Tr);

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileTL");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			RenderImage(wr, mGalleryConfig.GetImageURL("spacer_left.gif"), "", "");
			wr.RenderEndTag();
			// td
			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileTC");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			wr.Write("&nbsp;");
			wr.RenderEndTag();
			// td
			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileTR");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			RenderImage(wr, mGalleryConfig.GetImageURL("spacer_right.gif"), "", "");
			wr.RenderEndTag();
			// td

			wr.RenderEndTag();
			// tr

			wr.RenderBeginTag(HtmlTextWriterTag.Tr);

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileML");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			wr.Write("&nbsp;");
			wr.RenderEndTag();
			// td

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileMC");
			wr.AddAttribute(HtmlTextWriterAttribute.Style, ThumbSize);
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			//William Severance - Modifed to append CurrentStrip parameter as appropriate
			if (!mGalleryConfig.AllowPopup) {
				RenderImageButton(wr, Utils.AppendURLParameter(File.BrowserURL, "currentstrip=" + mUserRequest.CurrentStrip.ToString()), File.ThumbnailURL, File.Description, "");
			} else {
				RenderImageButton(wr, File.BrowserURL, File.ThumbnailURL, File.Description, "");
			}

			wr.RenderEndTag();
			// td            

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileMR");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			wr.Write("&nbsp;");
			wr.RenderEndTag();
			// td

			wr.RenderEndTag();
			// tr

			wr.RenderBeginTag(HtmlTextWriterTag.Tr);

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileBL");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			wr.Write("&nbsp;");
			wr.RenderEndTag();
			// td

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileBC");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			wr.Write("&nbsp;");
			wr.RenderEndTag();
			// td

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileBR");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			wr.Write("&nbsp;");
			wr.RenderEndTag();
			// td

			wr.RenderEndTag();
			// tr
			wr.RenderEndTag();
			// table            

		}


		private void RenderCommands(HtmlTextWriter wr, IGalleryObjectInfo Item)
		{
			if (mGalleryAuthorize.ItemCanSlideshow(Item)) {
				//William Severance - Modified to append CurrentStrip paramete to URL
				if (!mGalleryConfig.AllowPopup) {
					RenderImageButton(wr, Utils.AppendURLParameter(Item.SlideshowURL, "currentstrip=" + mUserRequest.CurrentStrip.ToString()), mGalleryConfig.GetImageURL("s_movie.gif"), Localization.GetString("Slideshow.Tooltip", mGalleryConfig.SharedResourceFile), "");
				} else {
					RenderImageButton(wr, Item.SlideshowURL, mGalleryConfig.GetImageURL("s_movie.gif"), Localization.GetString("Slideshow.Tooltip", mGalleryConfig.SharedResourceFile), "");
				}
				wr.Write("&nbsp;");
			}

			if (mGalleryAuthorize.ItemCanViewExif(Item)) {
				if (!mGalleryConfig.AllowPopup) {
					RenderImageButton(wr, Utils.AppendURLParameter(Item.ExifURL, "currentstrip=" + mUserRequest.CurrentStrip.ToString()), mGalleryConfig.GetImageURL("s_exif.gif"), Localization.GetString("EXIFData.Tooltip", mGalleryConfig.SharedResourceFile), "");
				} else {
					RenderImageButton(wr, Item.ExifURL, mGalleryConfig.GetImageURL("s_exif.gif"), Localization.GetString("EXIFData.Tooltip", mGalleryConfig.SharedResourceFile), "");
				}
				wr.Write("&nbsp;");
			}

			if (mGalleryAuthorize.ItemCanDownload(Item)) {
				RenderImageButton(wr, Item.DownloadURL, mGalleryConfig.GetImageURL("s_download.gif"), Localization.GetString("Download.Tooltip", mGalleryConfig.SharedResourceFile), "");
				wr.Write("&nbsp;");
			}

			if (mGalleryAuthorize.ItemCanEdit(Item)) {
				string editTooltip = null;
				if (Item.IsFolder) {
					editTooltip = Localization.GetString("MenuEdit.Tooltip", mGalleryConfig.SharedResourceFile);
				} else {
					editTooltip = Localization.GetString("MenuEditFile.Tooltip", mGalleryConfig.SharedResourceFile);
				}
				if (!mGalleryConfig.AllowPopup) {
					RenderImageButton(wr, Utils.AppendURLParameter(Item.EditURL, "currentstrip=" + mUserRequest.CurrentStrip.ToString()), mGalleryConfig.GetImageURL("s_edit.gif"), editTooltip, "");
				} else {
					RenderImageButton(wr, Item.EditURL, mGalleryConfig.GetImageURL("s_edit.gif"), editTooltip, "");
				}
				wr.Write("&nbsp;");
			}

		}

		private void RenderAlbumCommands(HtmlTextWriter wr, GalleryFolder Album)
		{
			if (mGalleryAuthorize.HasItemEditPermission(Album)) {
				RenderImageButton(wr, Album.AddSubAlbumURL, mGalleryConfig.GetImageURL("s_folder.gif"), Localization.GetString("MenuAddAlbum.Tooltip", mGalleryConfig.SharedResourceFile), "");
				wr.Write("&nbsp;");
			}
			if (mGalleryAuthorize.HasItemUploadPermission(Album)) {
				RenderImageButton(wr, Album.AddFileURL, mGalleryConfig.GetImageURL("s_new2.gif"), Localization.GetString("MenuAddFile.Tooltip", mGalleryConfig.SharedResourceFile), "");
				wr.Write("&nbsp;");
			}
			if (mGalleryAuthorize.HasItemEditPermission(Album)) {
				RenderImageButton(wr, Utils.AppendURLParameter(Album.MaintenanceURL, "currentstrip=" + mUserRequest.CurrentStrip.ToString()), mGalleryConfig.GetImageURL("s_bookopen.gif"), Localization.GetString("MenuMaintenance.Tooltip", mGalleryConfig.SharedResourceFile), "");
			}
		}

		private void RenderItemInfo(HtmlTextWriter wr, string PropertyName, string PropertyValue)
		{
			wr.RenderBeginTag(HtmlTextWriterTag.Tr);

			if (string.IsNullOrEmpty(PropertyName)) {
				wr.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
			} else {
				wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_ListItemProperty");
				wr.RenderBeginTag(HtmlTextWriterTag.Td);
				wr.Write(PropertyName);
				wr.RenderEndTag();
				// td
			}

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_ListItemValue");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			wr.Write(PropertyValue);
			wr.RenderEndTag();
			// td

			wr.RenderEndTag();
			// tr

		}

		public override void Render(HtmlTextWriter wr)
		{
			RenderTableBegin(wr, "Gallery_Content_List");
			RenderGallery(wr);
			RenderTableEnd(wr);
			//
		}

	}

}
