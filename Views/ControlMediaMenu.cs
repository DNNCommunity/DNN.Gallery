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
// the rights to use, copy, modify, merge, publish, dfistribute, sublicense, and/or sell copies of the Software, and 
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

using System.ComponentModel;
using System.IO;
using DotNetNuke.UI.WebControls;

[assembly: TagPrefix(".Gallery.Views.MediaMenu", "DotNetNuke.Modules")]
namespace DotNetNuke.Modules.Gallery.Views
{

	[ToolboxData("<{0}:GalleryMenu runat=server></{0}:GalleryMenu>")]
	public class MediaMenu : DNNMenu
	{

		private enum IconTypes : int
		{
			Slideshow = 0,
			EXIFData = 1,
			Download = 2,
			MenuEdit = 3,
			MenuEditFile = 3,
			MenuAddAlbum = 4,
			MenuAddFile = 5,
			MenuMaintenance = 6
		}

		private DotNetNuke.Modules.Gallery.Config mGalleryConfig = null;
		private DotNetNuke.Modules.Gallery.Authorization mGalleryAuthorize = null;
		private IGalleryObjectInfo mGalleryObject;
		private int mCurrentStrip = 0;

		private string mImgPath;
		public MediaMenu() : base()
		{
		}
		//New

		public MediaMenu(int ModuleId, IGalleryObjectInfo ObjectInfo) : base()
		{
			this.ModuleID = ModuleId;
			mGalleryObject = ObjectInfo;
			mImgPath = GalleryConfig.ImageFolder();
			this.ID = Utils.GetGalleryObjectMenuID(GalleryObject);
		}
		//New

		private string AppendCurrentStripParameter(string URL, bool Append)
		{
			if (Append && mCurrentStrip > 1) {
				return Utils.AppendURLParameter(URL, string.Format("currentstrip={0}", mCurrentStrip));
			} else {
				return URL;
			}
		}

		protected override void OnLoad(System.EventArgs e)
		{
			base.OnLoad(e);
			if (!Page.IsPostBack) {
				try {
					Orientation = DotNetNuke.UI.WebControls.Orientation.Horizontal;

					if (((Page.Request.QueryString["currentstrip"] != null))) {
						mCurrentStrip = int.Parse(Page.Request.QueryString["currentstrip"]);
					}

					var _with1 = ImageList;
					_with1.Add(mImgPath + "s_movie.gif");
					_with1.Add(mImgPath + "s_exif.gif");
					_with1.Add(mImgPath + "s_download.gif");
					_with1.Add(mImgPath + "s_edit.gif");
					_with1.Add(mImgPath + "s_folder.gif");
					_with1.Add(mImgPath + "s_new2.gif");
					_with1.Add(mImgPath + "s_bookopen.gif");

					MenuCssClass = "MediaMenu_SubMenu";
					DefaultChildNodeCssClass = "MediaMenu_MenuItem";
					DefaultNodeCssClassOver = "MediaMenu_MenuItemSel";
					DefaultIconCssClass = "MediaMenu_MenuIcon";

					//generate clickable top node to navigate to viewer

					MenuNode ParentMenuNode = MenuNodes[MenuNodes.Add()];

					var _with2 = ParentMenuNode;
					// WES - Fix for Issue - Multiple controls of same ID when FolderID and FileID are the same.
					_with2.ID = Utils.GetGalleryObjectMenuNodeID(GalleryObject);
					_with2.Text = "";
					_with2.CSSClass = "MediaMenu_MenuItem";
					_with2.CssClassOver = "MediaMenu_MenuItem";
					_with2.CSSIcon = "MediaMenu_Thumbnail";
					_with2.ToolTip = Path.GetFileName(GalleryObject.Thumbnail);
					_with2.Image = GalleryObject.ThumbnailURL;
					_with2.NavigateURL = AppendCurrentStripParameter(GalleryObject.BrowserURL, (!GalleryConfig.AllowPopup) && (GalleryObject.Type == Config.ItemType.Image));

					// Add Slideshow
					if (GalleryAuthorize.ItemCanSlideshow(GalleryObject)) {
						BuildMenuNode(ParentMenuNode, "Slideshow", AppendCurrentStripParameter(GalleryObject.SlideshowURL, (!GalleryConfig.AllowPopup) && (GalleryObject.Type == Config.ItemType.Image)));
					}

					// Add Exif Viewer
					if (GalleryAuthorize.ItemCanViewExif(GalleryObject)) {
						BuildMenuNode(ParentMenuNode, "EXIFData", AppendCurrentStripParameter(GalleryObject.ExifURL, (!GalleryConfig.AllowPopup) && (GalleryObject.Type == Config.ItemType.Image)));
					}

					// Add Download
					if (GalleryAuthorize.ItemCanDownload(GalleryObject)) {
						BuildMenuNode(ParentMenuNode, "Download", GalleryObject.DownloadURL);
					}

					bool hasEdit = GalleryAuthorize.HasItemEditPermission(GalleryObject);
					bool hasUpload = GalleryAuthorize.HasItemUploadPermission(GalleryObject);

					if (GalleryObject.IsFolder) {
						GalleryFolder objAlbum = (GalleryFolder)GalleryObject;

						if (hasEdit) {
							BuildMenuNode(ParentMenuNode, "MenuEdit", AppendCurrentStripParameter(GalleryObject.EditURL, true));
							BuildMenuNode(ParentMenuNode, "MenuAddAlbum", objAlbum.AddSubAlbumURL);
							BuildMenuNode(ParentMenuNode, "MenuMaintenance", AppendCurrentStripParameter(objAlbum.MaintenanceURL, true));
						}

						if (hasUpload) {
							BuildMenuNode(ParentMenuNode, "MenuAddFile", objAlbum.AddFileURL);
						}
					} else {
						if (hasEdit) {
							BuildMenuNode(ParentMenuNode, "MenuEditFile", AppendCurrentStripParameter(GalleryObject.EditURL, true));
						}
					}

				//Module failed to load
				} catch (Exception exc) {
					Exceptions.LogException(exc);
				}
			}
		}

		private void BuildMenuNode(MenuNode ParentMenuNode, string CommandName, string URL)
		{
			string strTitle = Localization.GetString(CommandName, GalleryConfig.SharedResourceFile);
			MenuNode objMenuNode = ParentMenuNode.MenuNodes[ParentMenuNode.MenuNodes.Add()];
			var _with3 = objMenuNode;
			_with3.ID = string.Concat(ParentMenuNode.ID, ".", CommandName);
			_with3.Text = strTitle;
			_with3.NavigateURL = URL.Replace("~", Common.Globals.ApplicationPath);
			_with3.ImageIndex = Convert.ToInt32(Enum.Parse(typeof(IconTypes), CommandName));
			_with3.ToolTip = Localization.GetString(string.Concat(CommandName, ".Tooltip"), GalleryConfig.SharedResourceFile);
		}

		[Bindable(true), Category("Data"), DefaultValue("0")]
		public int ModuleID {
			get {
				object savedState = ViewState["ModuleID"];
				if (savedState == null) {
					return 0;
				} else {
					return Convert.ToInt32(savedState);
				}
			}
			set { ViewState["ModuleID"] = value; }
		}

		public DotNetNuke.Modules.Gallery.Config GalleryConfig {
			get {
				if (mGalleryConfig == null) {
					mGalleryConfig = DotNetNuke.Modules.Gallery.Config.GetGalleryConfig(ModuleID);
				}
				return mGalleryConfig;
			}
		}

		public IGalleryObjectInfo GalleryObject {
			get { return mGalleryObject; }
			set { mGalleryObject = value; }
		}

		public DotNetNuke.Modules.Gallery.Authorization GalleryAuthorize {
			get {
				if (mGalleryAuthorize == null) {
					mGalleryAuthorize = new DotNetNuke.Modules.Gallery.Authorization(ModuleID);
				}
				return mGalleryAuthorize;
			}
		}

	}

}
