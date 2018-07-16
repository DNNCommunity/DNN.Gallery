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

	public class CardView : BaseView
	{

		private GalleryUserRequest mUserRequest;
		private ArrayList mCurrentItems = new ArrayList();
		private DotNetNuke.Modules.Gallery.Config mGalleryConfig;
		private DotNetNuke.Modules.Gallery.Authorization mGalleryAuthorize;

		private MediaMenu ctlMenu;
		public CardView(GalleryControl GalleryCont) : base(GalleryCont)
		{

			mUserRequest = GalleryCont.UserRequest;
			//New GalleryUserRequest(ModuleID, Gallery.Sort, Gallery.SortDESC)
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
						wr.AddAttribute(HtmlTextWriterAttribute.Class, "Body Gallery_CardCell");
						wr.AddAttribute(HtmlTextWriterAttribute.Style, cellStyle);
						wr.RenderBeginTag(HtmlTextWriterTag.Td);

						if (itm < mCurrentItems.Count) {
							item = (IGalleryObjectInfo)mCurrentItems[itm];

							if (item is GalleryFile) {
								file = (GalleryFile)mCurrentItems[itm];
							} else {
								file = null;
								album = (GalleryFolder)mCurrentItems[itm];
							}

							wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Card");
							wr.RenderBeginTag(HtmlTextWriterTag.Table);
							// table image & details

							wr.RenderBeginTag(HtmlTextWriterTag.Tr);
							// title                           
							wr.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
							wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_CardHeader");
							wr.RenderBeginTag(HtmlTextWriterTag.Td);

							RenderImageButton(wr, item.BrowserURL, item.IconURL, Localization.GetString("Open", mGalleryConfig.SharedResourceFile), "");
							wr.Write("&nbsp;");
							RenderCommandButton(wr, item.BrowserURL, item.Title, "Gallery_CardHeaderText");

							wr.RenderEndTag();
							// td
							wr.RenderEndTag();
							// tr

							wr.RenderBeginTag(HtmlTextWriterTag.Tr);
							// Gallery_Row for image and item info
							wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_CardLeft");
							wr.RenderBeginTag(HtmlTextWriterTag.Td);
							// image column

							if (mCurrentItems[itm] is GalleryFolder) {
								RenderAlbum(wr, item, thumbStyle);
							} else {
								RenderFile(wr, item, thumbStyle);
							}

							wr.RenderEndTag();
							// td  

							wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_CardRight");
							wr.RenderBeginTag(HtmlTextWriterTag.Td);

							if (mGalleryAuthorize.ItemCanVote(item)) {
								// Voting is enabled
								string voteInfo = GalleryControl.LocalizedText("VoteInfo");
								voteInfo = voteInfo.Replace("[VoteValue]", file.Score.ToString());
								voteInfo = voteInfo.Replace("[VoteCount]", file.Votes.Count.ToString());
								wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Vote");
								wr.RenderBeginTag(HtmlTextWriterTag.P);
								RenderImageButton(wr, Utils.AppendURLParameter(item.VotingURL, "currentstrip=" + mUserRequest.CurrentStrip.ToString()), item.ScoreImageURL, voteInfo, "");
								wr.RenderEndTag();
								// p
							}

							wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Info");
							wr.RenderBeginTag(HtmlTextWriterTag.P);
							wr.Write(item.ItemInfo);
							wr.RenderEndTag();
							// p info
							wr.RenderEndTag();
							// td voting and info

							wr.RenderEndTag();
							// tr ' close Gallery_Row info and image                            

							wr.RenderBeginTag(HtmlTextWriterTag.Tr);
							// description

							wr.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
							wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_CardHighlight");
							wr.RenderBeginTag(HtmlTextWriterTag.Td);

							//id tag must start with an alphabetic character xhtml Validation HWZassenhaus 9/24/2008
							wr.AddAttribute(HtmlTextWriterAttribute.Id, "description_" + item.ID);
							wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Description");
							wr.RenderBeginTag(HtmlTextWriterTag.P);

							if (((mGalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.Description) != 0) && !(item.Description.Length == 0)) {
								wr.Write(item.Description);
							} else {
								wr.Write("&nbsp;");
							}

							wr.RenderEndTag();
							// p description
							wr.RenderEndTag();
							// td description
							wr.RenderEndTag();
							// tr description

							wr.RenderEndTag();
							// table

						}
						wr.RenderEndTag();
						// td
					}
					wr.RenderEndTag();
					// tr
				}
			}

		}
		//RenderGallery


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

		public override void Render(HtmlTextWriter wr)
		{
			RenderTableBegin(wr, "Gallery_Content_Card");
			RenderGallery(wr);
			RenderTableEnd(wr);
			//
		}

	}

}
