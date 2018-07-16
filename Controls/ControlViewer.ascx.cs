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
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using static DotNetNuke.Common.Globals;
using static DotNetNuke.Modules.Gallery.Utils;
using System.Drawing;

namespace DotNetNuke.Modules.Gallery.WebControls
{

	public partial class Viewer : GalleryWebControlBase
	{

		private GalleryViewerRequest _CurrentRequest;
		private int mZoomIndex = 0;
		private RotateFlipType mRotateFlip = RotateFlipType.RotateNoneFlipNone;
		private string mModeText = "";
		private int mColor = 0;
		private int mCurrentIndex = -1;
		private GalleryFile mCurrentItem;
		private bool isModified = false;
		private Config.GallerySort mSort;
		private bool mSortDESC;
		private string mPopupURL;

		private bool isPopup = false;
		private int mCurrentStrip = 0;

		private string mReturnCtl = "";
		// Code extensively refactored by WES on 2-10-2009 to eliminate repeated blocks of identical
		// code, reduce length of query strings, fix issues with sequences of mixed rotate and flip operations
		// producing incorrect orientation and tighten security, parameter validation.

		private enum RotateFlipAction : int
		{
			RotateRight = 0,
			RotateLeft = 1,
			FlipX = 2,
			FlipY = 3
		}

		private RotateFlipType[,] RotateFlipMatrix = {
			{
				RotateFlipType.Rotate90FlipNone,
				RotateFlipType.Rotate270FlipNone,
				RotateFlipType.RotateNoneFlipX,
				RotateFlipType.RotateNoneFlipY
			},
			{
				RotateFlipType.Rotate180FlipNone,
				RotateFlipType.RotateNoneFlipNone,
				RotateFlipType.Rotate90FlipX,
				RotateFlipType.Rotate90FlipY
			},
			{
				RotateFlipType.Rotate270FlipNone,
				RotateFlipType.Rotate90FlipNone,
				RotateFlipType.Rotate180FlipX,
				RotateFlipType.Rotate180FlipY
			},
			{
				RotateFlipType.RotateNoneFlipNone,
				RotateFlipType.Rotate180FlipNone,
				RotateFlipType.Rotate270FlipX,
				RotateFlipType.Rotate270FlipY
			},
			{
				RotateFlipType.Rotate270FlipX,
				RotateFlipType.Rotate90FlipX,
				RotateFlipType.RotateNoneFlipNone,
				RotateFlipType.RotateNoneFlipXY
			},
			{
				RotateFlipType.RotateNoneFlipX,
				RotateFlipType.Rotate180FlipX,
				RotateFlipType.Rotate90FlipNone,
				RotateFlipType.Rotate90FlipXY
			},
			{
				RotateFlipType.Rotate90FlipX,
				RotateFlipType.Rotate270FlipX,
				RotateFlipType.Rotate180FlipNone,
				RotateFlipType.RotateNoneFlipNone
			},
			{
				RotateFlipType.RotateNoneFlipY,
				RotateFlipType.Rotate180FlipY,
				RotateFlipType.Rotate90FlipXY,
				RotateFlipType.Rotate90FlipNone
			}

		};
		#region "Public Properties"

		public GalleryViewerRequest CurrentRequest {
			get { return _CurrentRequest; }
			set { _CurrentRequest = value; }
		}

		#endregion

		#region " Web Form Designer Generated Code "

		//This call is required by the Web Form Designer.
		[System.Diagnostics.DebuggerStepThrough()]
		private void InitializeComponent()
		{
		}

		private void Page_Init(System.Object sender, System.EventArgs e)
		{
			//CODEGEN: This method call is required by the Web Form Designer
			//Do not modify it using the code editor.
			InitializeComponent();
		}

		#endregion

		#region "Event Handlers"


		private void Page_Load(System.Object sender, System.EventArgs e)
		{
			//Sort feature - WES modified to pass new parameter for GalleryConfig
			mSort = Utils.GetSort(GalleryConfig);
			mSortDESC = Utils.GetSortDESC(GalleryConfig);

			CurrentRequest = new GalleryViewerRequest(ModuleId, mSort, mSortDESC);

			mPopupURL = Page.ResolveUrl(GalleryConfig.SourceDirectory() + "/GalleryPage.aspx") + "?ctl=Viewer";

			//WES - Added to fix determination of whether we're in Viewer.ascx (in place)
			//      or Viewer.aspx (in pop-up window) and to add image title to page title

			isPopup = this.Parent.TemplateControl.AppRelativeVirtualPath.EndsWith("aspx");

			if (isPopup) {
				GalleryPageBase GalleryPage = (GalleryPageBase)this.Parent.TemplateControl;
				var _with1 = GalleryPage;
				_with1.Title = _with1.Title + " > " + CurrentRequest.CurrentItem.Title;
				_with1.ControlTitle = CurrentRequest.CurrentItem.Title;
			} else {
				CDefault SitePage = (CDefault)this.Page;
				SitePage.Title = SitePage.Title + " > " + CurrentRequest.CurrentItem.Title;
				Gallery.Viewer namingContainer = (Gallery.Viewer)this.NamingContainer;
				var _with2 = namingContainer;
				mReturnCtl = _with2.ReturnCtl;
				_with2.Title = CurrentRequest.CurrentItem.Title;
			}

			if ((Request.QueryString["zoomindex"] != null)) {
				mZoomIndex = Int32.Parse(Request.QueryString["zoomindex"]);
				isModified = true;
			}

			if ((Request.QueryString["rotateflip"] != null)) {
				mRotateFlip = (RotateFlipType)Int32.Parse(Request.QueryString["rotateflip"]);
				isModified = true;
			}

			if ((Request.QueryString["color"] != null)) {
				mColor = Int32.Parse(Request.QueryString["color"]);
				isModified = true;
			}

			if ((Request.QueryString["currentitem"] != null)) {
				mCurrentIndex = Int32.Parse(Request.QueryString["currentitem"]);
			}

			if ((Request.QueryString["currentstrip"] != null)) {
				mCurrentStrip = Int32.Parse(Request.QueryString["currentstrip"]);
			}

			//Modified for GAL-972 -- Ensure that the update control is visible only
			//when the user is authorized to edit the image AND there has been a change
			if (GalleryAuthorization.HasEditPermission()) {
				if (isModified == true) {
					this.UpdateButton.Visible = true;
				}
			} else {
				this.UpdateButton.Visible = false;
			}

			mCurrentItem = CurrentRequest.CurrentItem;

			if (GalleryAuthorization.HasEditPermission()) {
				if ((Request.QueryString["mode"] != null)) {
					switch (Request.QueryString["mode"]) {
						case "edit":
							this.UpdateButton.Visible = true;
							this.MoveNext.Visible = false;
							this.MovePrevious.Visible = false;
							mModeText = "Edit";
							break;
						case "save":
							mModeText = "Save";
							break;
					}
				}
			}

			if (!CurrentRequest.Folder.IsPopulated)
				Response.Redirect(NavigateURL());

			SetHyperLink(MovePrevious, "s_previous.gif", "MovePrevious", CurrentRequest.PreviousItemNumber);
			SetHyperLink(MoveNext, "s_next.gif", "MoveNext", CurrentRequest.NextItemNumber);

			// Allow zooming +3 or -3 times only
			if (!(mZoomIndex > 2)) {
				SetHyperLink(ZoomIn, "s_zoomin.gif", "ZoomIn", mCurrentIndex, mModeText, mZoomIndex + 1, mRotateFlip, mColor);
			} else {
				ZoomIn.Visible = false;
			}

			if (!(mZoomIndex < -2)) {
				SetHyperLink(ZoomOut, "s_zoomout.gif", "ZoomOut", mCurrentIndex, mModeText, mZoomIndex - 1, mRotateFlip, mColor);
			} else {
				ZoomOut.Visible = false;
			}

			SetHyperLink(RotateRight, "s_rotateright.gif", "RotateRight", mCurrentIndex, mModeText, mZoomIndex, RotateFlipMatrix[(int)mRotateFlip, (int)RotateFlipAction.RotateRight], mColor);
			SetHyperLink(RotateLeft, "s_rotateleft.gif", "RotateLeft", mCurrentIndex, mModeText, mZoomIndex, RotateFlipMatrix[(int)mRotateFlip, (int)RotateFlipAction.RotateLeft], mColor);

			SetHyperLink(FlipX, "s_flipx.gif", "FlipX", mCurrentIndex, mModeText, mZoomIndex, RotateFlipMatrix[(int)mRotateFlip, (int)RotateFlipAction.FlipX], mColor);
			SetHyperLink(FlipY, "s_flipy.gif", "FlipY", mCurrentIndex, mModeText, mZoomIndex, RotateFlipMatrix[(int)mRotateFlip, (int)RotateFlipAction.FlipY], mColor);

			int mColorPlus = ((1 - Convert.ToInt32(Math.Ceiling((double)mColor / 256))) * 256) + mColor - (32 * Convert.ToInt32(Math.Ceiling((double)mColor / 256)));
			int mColorMinus = ((1 - Convert.ToInt32(Math.Ceiling((double)mColor / 256))) * 256) + mColor + (32 * Convert.ToInt32(Math.Ceiling((double)mColor / 256)));

			BWPlus.Visible = (!(mColorPlus == 0));
			BWMinus.Visible = ((!(mColorMinus > 256)) && (!(mColor == 0)));

			SetHyperLink(Color, "s_color.gif", "Color", mCurrentIndex, mModeText, mZoomIndex, mRotateFlip, 0);
			SetHyperLink(BWMinus, "s_bwminus.gif", "BWMinus", mCurrentIndex, mModeText, mZoomIndex, mRotateFlip, mColorMinus);
			SetHyperLink(BWPlus, "s_bwplus.gif", "BWPlus", mCurrentIndex, mModeText, mZoomIndex, mRotateFlip, mColorPlus);

			SetHyperLink(UpdateButton, "m_save.gif", "UpdateButton", mCurrentIndex, "Save", mZoomIndex, mRotateFlip, mColor);

			DotNetNuke.UI.Utilities.ClientAPI.AddButtonConfirm(UpdateButton, Localization.GetString("Save_Confirm", GalleryConfig.SharedResourceFile));

			litInfo.Text = "<span>" + CurrentRequest.CurrentItem.ItemInfo + "</span>";
			litDescription.Text = "<p class=\"Gallery_Description\">" + CurrentRequest.CurrentItem.Description + "</p>";
		}

		#endregion

		#region "Private/Protected Methods"

		private void SetHyperLink(System.Web.UI.WebControls.HyperLink ctl, string iconFilename, string toolTipKey, int currentItem)
		{
			ArrayList @params = new ArrayList();
			AddNonDefaultParameter(@params, "mid", ModuleId, -1);
			AddNonDefaultParameter(@params, "path", CurrentRequest.Path, "");
			AddNonDefaultParameter(@params, "currentitem", currentItem, -1);

			SetHyperLink(ctl, iconFilename, toolTipKey, @params);
		}


		private void SetHyperLink(System.Web.UI.WebControls.HyperLink ctl, string iconFilename, string toolTipKey, int currentItem, string modeText, int zoomIndex, RotateFlipType rotateFlip, int color)
		{
			ArrayList @params = new ArrayList();
			AddNonDefaultParameter(@params, "mid", ModuleId, -1);
			AddNonDefaultParameter(@params, "path", CurrentRequest.Path, "");
			AddNonDefaultParameter(@params, "currentitem", currentItem, -1);
			AddNonDefaultParameter(@params, "mode", modeText.ToLower(), "");
			AddNonDefaultParameter(@params, "zoomindex", zoomIndex, 0);
			AddNonDefaultParameter(@params, "rotateflip", (int)rotateFlip, 0);
			AddNonDefaultParameter(@params, "color", color, 0);
			AddNonDefaultParameter(@params, "currentstrip", mCurrentStrip, 0);
			AddNonDefaultParameter(@params, "returnctl", mReturnCtl, "");

			SetHyperLink(ctl, iconFilename, toolTipKey, @params);
		}


		private void SetHyperLink(System.Web.UI.WebControls.HyperLink ctl, string iconFilename, string toolTipKey, ArrayList @params)
		{
			if (isPopup) {
                ctl.NavigateUrl = Utils.GetURL(mPopupURL, @params.ToArray(typeof(string)) as string[]);
			} else {
				ctl.NavigateUrl = Common.Globals.NavigateURL(TabId, "Viewer", Convert.ToString(@params.ToArray(typeof(string))));
			}
			ctl.ImageUrl = GalleryConfig.GetImageURL(iconFilename);
			ctl.ToolTip = Localization.GetString(toolTipKey, GalleryConfig.SharedResourceFile);
		}

		private void AddNonDefaultParameter<T>(ArrayList @params, string key, T value, T @default)
		{
			if (!value.Equals(@default)) {
				@params.Add(key + "=" + value.ToString());
			}
		}

		protected string ImageURL()
		{
			string RequestURL = Request.Url.ToString();
			string url = null;
			if ((!isModified) && (mCurrentItem.Parent.WatermarkImage == null)) {
				url = mCurrentItem.URL;
			} else {
				if (isPopup) {
					url = "Image.aspx?" + Strings.Right(RequestURL, Strings.Len(RequestURL) - Strings.InStr(RequestURL, "?"));
				} else {
					url = Page.ResolveUrl("DesktopModules/Gallery/Image.aspx?") + Strings.Right(RequestURL, Strings.Len(RequestURL) - Strings.InStr(RequestURL, "?"));
				}
			}
			return url;
		}

		protected string Name()
		{
			return mCurrentItem.Name;
		}

		#endregion

	}
}
