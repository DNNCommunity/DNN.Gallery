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


using static System.Web.UI.Control;
using DotNetNuke.Entities.Portals;
using static DotNetNuke.Common.Globals;
using DotNetNuke.Entities.Modules;
using DotNetNuke.UI.Skins;
using static DotNetNuke.Modules.Gallery.Utils;
using DotNetNuke.UI.WebControls;

namespace DotNetNuke.Modules.Gallery.WebControls
{

	public abstract partial class GalleryMenu : DotNetNuke.UI.Skins.SkinObjectBase
	{

		private int mModuleID;
		private DotNetNuke.Modules.Gallery.Config mGalleryConfig;
		private BaseRequest mGalleryRequest;
		private DotNetNuke.Modules.Gallery.Authorization mGalleryAuthorize;

		private GalleryFolder mCurrentAlbum;
		private enum IconTypes : int
		{
			MenuEdit = 1,
			MenuEditFile = 2,
			Folder = 2,
			MenuAddAlbum = 2,
			MenuAddFile = 3,
			MenuMaintenance = 4
		}


		#region " Web Form Designer Generated Code "

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

		//*******************************************************
		//
		// The Page_Load server event handler on this page is used
		// to populate the role information for the page
		//
		//*******************************************************

		private void Page_Load(System.Object sender, System.EventArgs e)
		{
			MenuNode BreakMenuNode = null;

			mGalleryConfig = Config.GetGalleryConfig(ModuleID);
			if (mGalleryConfig.IsValidPath & !IsPostBack) {
				mGalleryAuthorize = new DotNetNuke.Modules.Gallery.Authorization(ModuleID);
				mCurrentAlbum = mGalleryRequest.Folder;

				MenuNode ParentMenuNode = null;
				string imgPath = mGalleryConfig.ImageFolder(false);

				try {
					// generate dynamic menu
					var _with1 = ctlDNNMenu;
					var _with2 = _with1.ImageList;
					_with2.Add(imgPath + "s_picture.gif");
					_with2.Add(imgPath + "s_edit.gif");
					_with2.Add(imgPath + "s_folder.gif");
					_with2.Add(imgPath + "s_new2.gif");
					_with2.Add(imgPath + "s_bookopen.gif");
					_with1.MenuCssClass = "GalleryMenu_SubMenu";
					_with1.DefaultChildNodeCssClass = "GalleryMenu_MenuItem";
					_with1.DefaultNodeCssClassOver = "GalleryMenu_MenuItemSel";
					_with1.DefaultIconCssClass = "GalleryMenu_MenuIcon";

					//Build top level parent menu node

					string CurrentAlbumMenuID = null;
					if (mCurrentAlbum.ID < 0) {
						CurrentAlbumMenuID = ModuleID.ToString();
					} else {
						CurrentAlbumMenuID = mCurrentAlbum.ID.ToString() + "." + mCurrentAlbum.Index.ToString();
					}

					ParentMenuNode = ctlDNNMenu.MenuNodes[ctlDNNMenu.MenuNodes.Add()];

					var _with3 = ParentMenuNode;
					_with3.ID = CurrentAlbumMenuID;
					_with3.ClickAction = eClickAction.None;
					_with3.ImageIndex = 0;
					_with3.Text = "";
					_with3.CSSClass = "GalleryMenu_MenuBar";
					_with3.CssClassOver = "GalleryMenu_MenuBar";
					_with3.CSSIcon = "GalleryMenu_IconButton";

					// Build menu nodes for each level of folder hierarchy
					if ((mGalleryConfig.IsValidPath) && (mCurrentAlbum.List.Count > 0)) {
						IGalleryObjectInfo child = null;
						foreach (IGalleryObjectInfo child_loopVariable in mCurrentAlbum.List) {
							child = child_loopVariable;
							if (child is GalleryFolder && (mGalleryAuthorize.ItemIsApproved(child) || mGalleryAuthorize.HasItemEditPermission(child))) {
								BuildAlbumMenuNode(ParentMenuNode, (GalleryFolder)child);
							}
						}
						if (ParentMenuNode.MenuNodes.Count > 0) {
							// Add a menu break of any child albums were added to the menu
							BreakMenuNode = ParentMenuNode.MenuNodes[ParentMenuNode.MenuNodes.AddBreak()];
							BreakMenuNode.CSSIcon = "GalleryMenu_MenuBreak";
						}
					}

					// Build menu nodes for context sensitive gallery management links
					// William Severance - GAL-832 make gallery and media context menus more context sensitive
					// Do not show Edit Album, Add File, Add Album or Maintenance menu items if not in gallery view
					// Gallery view when QueryString does not contain ctl key

					bool hasManagementNode = false;

					if (Request.QueryString["ctl"] == null && mGalleryConfig.IsValidPath) {
						string strCurrentStrip = null;
						if (mGalleryRequest is GalleryUserRequest) {
							strCurrentStrip = "currentstrip=" + ((GalleryUserRequest)mGalleryRequest).CurrentStrip.ToString();
						}

						if (mGalleryAuthorize.HasItemEditPermission(mCurrentAlbum)) {
							BuildMenuNode(ParentMenuNode, "MenuEdit", Utils.AppendURLParameter(mCurrentAlbum.EditURL, strCurrentStrip));
							hasManagementNode = true;
						}
						if (mGalleryAuthorize.HasItemUploadPermission(mCurrentAlbum)) {
							BuildMenuNode(ParentMenuNode, "MenuAddFile", mCurrentAlbum.AddFileURL);
							hasManagementNode = true;
						}
						if (mGalleryAuthorize.HasItemEditPermission(mCurrentAlbum)) {
							BuildMenuNode(ParentMenuNode, "MenuAddAlbum", mCurrentAlbum.AddSubAlbumURL);
							BuildMenuNode(ParentMenuNode, "MenuMaintenance", Utils.AppendURLParameter(mCurrentAlbum.MaintenanceURL, strCurrentStrip));
							hasManagementNode = true;
						}
					}

					if (BreakMenuNode != null && !hasManagementNode) {
						ParentMenuNode.MenuNodes.Remove(BreakMenuNode);
					}

				//Module failed to load
				} catch (Exception exc) {
					Exceptions.ProcessModuleLoadException(this, exc);
				}
			}

		}

		private void BuildMenuNode(MenuNode ParentMenuNode, string CommandName, string URL)
		{
			string strTitle = Localization.GetString(CommandName, mGalleryConfig.SharedResourceFile);
			MenuNode objMenuNode = ParentMenuNode.MenuNodes[ParentMenuNode.MenuNodes.Add()];
			var _with4 = objMenuNode;
			_with4.ID = string.Concat(ParentMenuNode.ID, ".", CommandName);
			_with4.Text = strTitle;
			_with4.NavigateURL = URL.Replace("~", Common.Globals.ApplicationPath);
			_with4.ImageIndex = Convert.ToInt32(Enum.Parse(typeof(IconTypes), CommandName));
			_with4.ToolTip = Localization.GetString(string.Concat(CommandName, ".Tooltip"), mGalleryConfig.SharedResourceFile);
		}

		private void BuildAlbumMenuNode(MenuNode ParentMenuNode, GalleryFolder Folder)
		{
			IGalleryObjectInfo child = null;
			string strTitle = string.Concat(Folder.Title, " (", Folder.Size.ToString(), " ", Localization.GetString("Items", mGalleryConfig.SharedResourceFile), ")");
			string AlbumMenuID = Folder.ID.ToString() + "." + Folder.Index.ToString();

			MenuNode objMenuNode = ParentMenuNode.MenuNodes[ParentMenuNode.MenuNodes.Add()];
			var _with5 = objMenuNode;
			_with5.ID = AlbumMenuID;
			_with5.Text = strTitle;
			_with5.ImageIndex = (int)IconTypes.Folder;
			_with5.NavigateURL = Folder.BrowserURL.Replace("~", Common.Globals.ApplicationPath);

			if (mGalleryConfig.MultiLevelMenu) {
				foreach (IGalleryObjectInfo child_loopVariable in Folder.List) {
					child = child_loopVariable;
					if (child is GalleryFolder && (mGalleryAuthorize.ItemIsApproved(child) || mGalleryAuthorize.HasItemEditPermission(child))) {
						BuildAlbumMenuNode(objMenuNode, (GalleryFolder)child);
					}
				}
			}
		}

		public int ModuleID {
			get { return mModuleID; }
			set { mModuleID = value; }
		}

		public BaseRequest GalleryRequest {
			get { return mGalleryRequest; }
			set { mGalleryRequest = value; }
		}
		public GalleryMenu()
		{
			Load += Page_Load;
			Init += Page_Init;
		}

	}

}
