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

using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using static DotNetNuke.Common.Globals;
using System.IO;

namespace DotNetNuke.Modules.Gallery
{

	public class Authorization
	{

		private PortalSettings mPortalSettings;
		private ModuleInfo mModuleSettings;
		private DotNetNuke.Modules.Gallery.Config mGalleryConfig;
		private int mLoggedOnUserId = -1;
		private string mLoggedOnUserName = "";

		private bool mIsAuthenticated = false;
		public Authorization(ModuleInfo ModuleSettings)
		{
			mPortalSettings = PortalController.GetCurrentPortalSettings();
			mModuleSettings = ModuleSettings;

			mGalleryConfig = Config.GetGalleryConfig(mModuleSettings.ModuleID);

			if (HttpContext.Current.Request.IsAuthenticated) {
				mIsAuthenticated = true;
			}
			UserInfo objUser = UserController.GetCurrentUserInfo();
			mLoggedOnUserId = objUser.UserID;
			//UserController.GetCurrentUserInfo.UserID
			mLoggedOnUserName = objUser.Username;
		}

		public Authorization(int ModuleId)
		{
			mPortalSettings = PortalController.GetCurrentPortalSettings();

			ModuleController objModuleController = new ModuleController();
			mModuleSettings = objModuleController.GetModule(ModuleId, mPortalSettings.ActiveTab.TabID);

			mGalleryConfig = Config.GetGalleryConfig(ModuleId);

			if (HttpContext.Current.Request.IsAuthenticated) {
				mIsAuthenticated = true;
			}

			UserInfo objUser = UserController.GetCurrentUserInfo();
			mLoggedOnUserId = objUser.UserID;
			mLoggedOnUserName = objUser.Username;
		}

		public DotNetNuke.Modules.Gallery.Config GalleryConfig {
			get { return mGalleryConfig; }
		}

		public int LoggedOnUserId {
			get { return mLoggedOnUserId; }
		}

		public string LoggedOnUserName {
			get { return mLoggedOnUserName; }
		}

		public bool IsAuthenticated {
			get { return mIsAuthenticated; }
		}

		public bool HasEditPermission()
		{
			return HasUploadPermission() & IsAuthenticated;
		}

		// Created by Andrew Galbraith Ryer to allow unauthenticated users to upload but not edit the gallery.
		public bool HasUploadPermission()
		{
			if (GalleryConfig.IsPrivate) {
				if (HasAdminPermission() || IsGalleryOwner()) {
					return true;
				}
			} else {
				return DotNetNuke.Security.Permissions.ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "EDIT", mModuleSettings);
			}
			return false;
		}

		public bool HasItemEditPermission(object DataItem)
		{
			return HasItemUploadPermission(DataItem) & IsAuthenticated;
		}

		public bool HasItemUploadPermission(object DataItem)
		{
			if (GalleryConfig.IsPrivate) {
				if (IsGalleryOwner() || ((IGalleryObjectInfo)DataItem).OwnerID == LoggedOnUserId) {
					return true;
				}
			} else {
				return DotNetNuke.Security.Permissions.ModulePermissionController.CanEditModuleContent(mModuleSettings);
			}
			return false;

		}

		public bool IsGalleryOwner()
		{
			if (!IsAuthenticated) {
				return false;
			} else {
				if (HasAdminPermission() || mGalleryConfig.OwnerID == mLoggedOnUserId) {
					return true;
				}
				return false;
			}
		}

		public bool IsItemOwner(object DataItem)
		{
			if (mGalleryConfig.IsPrivate) {
				if (IsGalleryOwner() || ((IGalleryObjectInfo)DataItem).OwnerID == LoggedOnUserId) {
					return true;
				}
			} else {
				if (this.HasUploadPermission()) {
					return true;
				}
			}
			return false;
		}

		public bool ItemCanEdit(object DataItem)
		{
			return HasItemEditPermission(DataItem);
		}

		//William Severance - Added new authorization test

		public bool HasItemApprovalPermission(object DataItem)
		{
			if (mGalleryConfig.IsPrivate) {
				if (IsGalleryOwner() || (((DataItem != null)) && ((IGalleryObjectInfo)DataItem).OwnerID == LoggedOnUserId)) {
					return true;
				}
			} else {
				return HasAdminPermission();
			}
			return false;

		}

		public bool ItemIsApproved(object DataItem)
		{
			if ((((IGalleryObjectInfo)DataItem).ApprovedDate <= DateTime.Today) || (GalleryConfig.AutoApproval)) {
				return true;
			}
			return false;
		}

		public bool ItemCanSlideshow(object DataItem)
		{
			if (mGalleryConfig.AllowSlideshow) {
				if (((IGalleryObjectInfo)DataItem).IsFolder) {
					return ((GalleryFolder)DataItem).BrowsableItems.Count > 0;
				} else {
					return (((GalleryFile)DataItem).Parent.BrowsableItems.Count > 0 && ((GalleryFile)DataItem).Type == Config.ItemType.Image);
				}
			}
			return false;
		}

		public bool ItemCanDownload(object DataItem)
		{
			if (!((IGalleryObjectInfo)DataItem).IsFolder && mGalleryConfig.AllowDownload && (mGalleryConfig.HasDownloadPermission || IsItemOwner(DataItem))) {
				return true;
			}
			return false;
		}

		public bool ItemCanVote(object DataItem)
		{
			if (!((IGalleryObjectInfo)DataItem).IsFolder && mGalleryConfig.AllowVoting) {
				return true;
			}
			return false;

		}

		public bool ItemIsValidImage(object DataItem)
		{
			if (((IGalleryObjectInfo)DataItem).Type == Config.ItemType.Image) {
				return true;
			}
            return false;
		}

		public bool ItemCanViewExif(object DataItem)
		{
			if (ItemIsValidImage(DataItem) && mGalleryConfig.AllowExif) {
				return true;
			}
			return false;
		}

		public bool ItemCanBrowse(object DataItem)
		{
			//for item
			if (((IGalleryObjectInfo)DataItem).IsFolder) {
				if (((GalleryFolder)DataItem).BrowsableItems.Count > 0) {
					return true;
				}
			}
            return false;
		}

		public bool HasAdminPermission()
		{
			return DotNetNuke.Security.Permissions.ModulePermissionController.CanAdminModule(mModuleSettings);
		}

		// Called to test view permission on popup pages.
		public bool HasViewPermission()
		{
			return DotNetNuke.Security.Permissions.ModulePermissionController.CanViewModule(mModuleSettings);
		}

	}

}
