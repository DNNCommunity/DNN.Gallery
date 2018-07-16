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

	public class StandardView : BaseView
	{

		private GalleryUserRequest mUserRequest;
		private ArrayList mCurrentItems = new ArrayList();
		private DotNetNuke.Modules.Gallery.Config mGalleryConfig;
		private DotNetNuke.Modules.Gallery.Authorization mGalleryAuthorize;

		private MediaMenu ctlMenu;
		public StandardView(GalleryControl GalleryCont) : base(GalleryCont)
		{

			mUserRequest = GalleryCont.UserRequest;
			mCurrentItems = mUserRequest.CurrentItems();
			mGalleryAuthorize = GalleryCont.GalleryAuthorize;
			mGalleryConfig = GalleryCont.GalleryConfig;

		}

		public override void CreateChildControls()
		{
			IGalleryObjectInfo item = null;
			foreach (IGalleryObjectInfo item_loopVariable in mCurrentItems) {
				item = item_loopVariable;
				ctlMenu = new MediaMenu(GalleryControl.ModuleId, item);
				Controls.Add(ctlMenu);
			}
		}

		public override void OnPreRender()
		{
		}

		private void RenderGallery(HtmlTextWriter wr)
		{
			IGalleryObjectInfo item = null;
			GalleryFolder album = null;
			GalleryFile file = null;
			int rowCount = 0;
			int row = 0;
			int itm = 0;
			int w = 0;
			string cellStyle = null;
			string thumbStyle = null;

			if (mCurrentItems.Count == 0) {
				RenderInfo(wr, Localization.GetString("AlbumEmpty", mGalleryConfig.SharedResourceFile));
				return;
			}

			//Percentage width must be an integer xhtml 1.0 - HWZassenhaus 9/24/2008
			w = Convert.ToInt32(100 / mGalleryConfig.StripWidth);
			cellStyle = "width:" + Unit.Percentage(w).ToString();
			thumbStyle = "width:" + (mGalleryConfig.MaximumThumbWidth).ToString() + "px; height:" + (mGalleryConfig.MaximumThumbWidth).ToString() + "px;";

			rowCount = Convert.ToInt32(Math.Ceiling((double)mCurrentItems.Count / (double)mGalleryConfig.StripWidth));

			if (!(rowCount == 0)) {
				for (row = 0; row <= rowCount - 1; row++) {
					wr.RenderBeginTag(HtmlTextWriterTag.Tr);
					int itmFrom = row * mGalleryConfig.StripWidth;
					int itmTo = itmFrom + mGalleryConfig.StripWidth - 1;

					for (itm = itmFrom; itm <= itmTo; itm++) {
						// image column
						wr.AddAttribute(HtmlTextWriterAttribute.Class, "Body Gallery_StandardCell");
						wr.AddAttribute(HtmlTextWriterAttribute.Style, cellStyle);
						wr.RenderBeginTag(HtmlTextWriterTag.Td);

						if (itm < mCurrentItems.Count) {
							item = (IGalleryObjectInfo)mCurrentItems[itm];
							wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Standard");
							wr.RenderBeginTag(HtmlTextWriterTag.Table);
							// table image & details

							// Gallery_Row for thumbnail image
							wr.RenderBeginTag(HtmlTextWriterTag.Tr);
							wr.RenderBeginTag(HtmlTextWriterTag.Td);

							if (item is GalleryFile) {
								file = (GalleryFile)mCurrentItems[itm];
								RenderFile(wr, file, thumbStyle);
							} else {
								file = null;
								album = (GalleryFolder)mCurrentItems[itm];
								RenderAlbum(wr, album, thumbStyle);
							}

							wr.RenderEndTag();
							// td thumb
							wr.RenderEndTag();
							// tr thumb

							//voting
							if (mGalleryAuthorize.ItemCanVote(item)) {
								wr.RenderBeginTag(HtmlTextWriterTag.Tr);
								// voting
								wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Vote");
								wr.RenderBeginTag(HtmlTextWriterTag.Td);

								string voteInfo = GalleryControl.LocalizedText("VoteInfo");
								voteInfo = voteInfo.Replace("[VoteValue]", file.Score.ToString());
								voteInfo = voteInfo.Replace("[VoteCount]", file.Votes.Count.ToString());
								RenderImageButton(wr, Utils.AppendURLParameter(item.VotingURL, "currentstrip=" + mUserRequest.CurrentStrip.ToString()), item.ScoreImageURL, voteInfo, "");
								wr.RenderEndTag();
								// td voting
								wr.RenderEndTag();
								// tr voting
							}



							if ((mGalleryConfig.TextDisplayOptions & GalleryDisplayOption.Title) != 0) {
								wr.RenderBeginTag(HtmlTextWriterTag.Tr);
								// title
								wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Title");
								wr.RenderBeginTag(HtmlTextWriterTag.Td);
								if (!mGalleryConfig.AllowPopup) {
									RenderCommandButton(wr, Utils.AppendURLParameter(item.BrowserURL, "currentstrip=" + mUserRequest.CurrentStrip.ToString()), item.Title, "Gallery_AltHeaderText");
								} else {
									RenderCommandButton(wr, item.BrowserURL, item.Title, "Gallery_AltHeaderText");
								}
								wr.RenderEndTag();
								// td title
								wr.RenderEndTag();
								// tr title
							}
							wr.RenderBeginTag(HtmlTextWriterTag.Tr);
							// info
							wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Info");
							wr.RenderBeginTag(HtmlTextWriterTag.Td);
							wr.Write(item.ItemInfo);
							wr.RenderEndTag();
							// td info
							wr.RenderEndTag();
							// tr info

							wr.RenderBeginTag(HtmlTextWriterTag.Tr);
							// description
							wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Description");
							wr.RenderBeginTag(HtmlTextWriterTag.Td);

							if (((mGalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.Description) != 0) && !(item.Description.Length == 0)) {
								wr.Write(item.Description);
							} else {
								wr.Write("&nbsp;");
							}

							wr.RenderEndTag();
							// td description
							wr.RenderEndTag();
							// tr description
							wr.RenderEndTag();
							// table image & details
						}
						wr.RenderEndTag();
						// td
					}
					wr.RenderEndTag();
					// tr
				}
			}
		}


		private void RenderAlbum(HtmlTextWriter wr, IGalleryObjectInfo Album, string ThumbStyle)
		{
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
			wr.AddAttribute(HtmlTextWriterAttribute.Style, ThumbStyle);
			wr.RenderBeginTag(HtmlTextWriterTag.Td);

			Control objControl = null;
			foreach (Control objControl_loopVariable in Controls) {
				objControl = objControl_loopVariable;
				if (objControl is MediaMenu) {
					MediaMenu objMediaMenu = (MediaMenu)objControl;
					if (objMediaMenu.ID == Utils.GetGalleryObjectMenuID(Album)) {
						objMediaMenu.RenderControl(wr);
					}
				}
			}

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

		private void RenderFile(HtmlTextWriter wr, IGalleryObjectInfo File, string ThumbStyle)
		{
			// table File            

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_File Centered Gallery_DropShadow");
			wr.RenderBeginTag(HtmlTextWriterTag.Table);
			// table image & details

			wr.RenderBeginTag(HtmlTextWriterTag.Tr);

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileTL");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			wr.RenderEndTag();
			// td
			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileTC");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			wr.RenderEndTag();
			// td
			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileTR");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			wr.RenderEndTag();
			// td

			wr.RenderEndTag();
			// tr

			wr.RenderBeginTag(HtmlTextWriterTag.Tr);

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileML");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			wr.RenderEndTag();
			// td

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileMC");
			wr.AddAttribute(HtmlTextWriterAttribute.Style, ThumbStyle);
			wr.RenderBeginTag(HtmlTextWriterTag.Td);

			Control objControl = null;
			foreach (Control objControl_loopVariable in Controls) {
				objControl = objControl_loopVariable;
				MediaMenu objMediaMenu = (MediaMenu)objControl;
				if (objMediaMenu.ID == Utils.GetGalleryObjectMenuID(File)) {
					objMediaMenu.RenderControl(wr);
				}
			}

			wr.RenderEndTag();
			// td            

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileMR");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			wr.RenderEndTag();
			// td

			wr.RenderEndTag();
			// tr

			wr.RenderBeginTag(HtmlTextWriterTag.Tr);

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileBL");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			wr.RenderEndTag();
			// td

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileBC");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			wr.RenderEndTag();
			// td

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileBR");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			//wr.Write("&nbsp;")
			wr.RenderEndTag();
			// td

			wr.RenderEndTag();
			// tr
			wr.RenderEndTag();
			// table            

		}

		// Note: The RenderCommand method is never called in the StandardView

		private void RenderCommand(HtmlTextWriter wr, IGalleryObjectInfo Item)
		{
			wr.RenderBeginTag(HtmlTextWriterTag.Tr);
			// command
			wr.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
			wr.AddAttribute(HtmlTextWriterAttribute.Align, "center");
			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_RowHighLight");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);

			if (mGalleryAuthorize.ItemCanSlideshow(Item)) {
				//William Severance - Modified to append CurrentStrip paramete to URL
				if (!mGalleryConfig.AllowPopup) {
					RenderImageButton(wr, Utils.AppendURLParameter(Item.SlideshowURL, "currentstrip=" + mUserRequest.CurrentStrip.ToString()), mGalleryConfig.GetImageURL("s_movie.gif"), "View slideshow...", "");
				} else {
					RenderImageButton(wr, Item.SlideshowURL, mGalleryConfig.GetImageURL("s_movie.gif"), "View slideshow...", "");
				}
				wr.Write("&nbsp;");
			}

			if (mGalleryAuthorize.ItemCanViewExif(Item)) {
				if (!mGalleryConfig.AllowPopup) {
					RenderImageButton(wr, Utils.AppendURLParameter(Item.ExifURL, "currentstrip=" + mUserRequest.CurrentStrip.ToString()), mGalleryConfig.GetImageURL("s_exif.gif"), "View Exif Metadata...", "");
				} else {
					RenderImageButton(wr, Item.ExifURL, mGalleryConfig.GetImageURL("s_exif.gif"), "View Exif Metadata...", "");
				}
				wr.Write("&nbsp;");
			}

			if (mGalleryAuthorize.ItemCanDownload(Item)) {
				RenderImageButton(wr, Item.DownloadURL, mGalleryConfig.GetImageURL("s_download.gif"), "Download...", "");
				wr.Write("&nbsp;");
			}

			if (mGalleryAuthorize.ItemCanEdit(Item)) {
				if (!mGalleryConfig.AllowPopup) {
					RenderImageButton(wr, Utils.AppendURLParameter(Item.EditURL, "currentstrip=" + mUserRequest.CurrentStrip.ToString()), mGalleryConfig.GetImageURL("s_edit.gif"), "Edit...", "");
				} else {
					RenderImageButton(wr, Item.EditURL, mGalleryConfig.GetImageURL("s_edit.gif"), "Edit...", "");
				}
				wr.Write("&nbsp;");
			}

			wr.RenderEndTag();
			// td command
			wr.RenderEndTag();
			// tr command

		}

		// Note: The RenderAlbumCommand method is never called in the StandardView


		private void RenderAlbumCommand(HtmlTextWriter wr, GalleryFolder Album)
		{
			RenderImageButton(wr, Album.AddSubAlbumURL, mGalleryConfig.GetImageURL("s_folder.gif"), "Add sub album...", "");
			wr.Write("&nbsp;");
			RenderImageButton(wr, Album.AddFileURL, mGalleryConfig.GetImageURL("s_new2.gif"), "Add files...", "");
			wr.Write("&nbsp;");

			RenderImageButton(wr, Utils.AppendURLParameter(Album.MaintenanceURL, "currentstrip=" + mUserRequest.CurrentStrip.ToString()), mGalleryConfig.GetImageURL("s_bookopen.gif"), "Maintenance...", "");
		}

		public override void Render(HtmlTextWriter wr)
		{
			RenderTableBegin(wr, "Gallery_Content_Standard");
			RenderGallery(wr);
			RenderTableEnd(wr);
			//
		}

	}

}
