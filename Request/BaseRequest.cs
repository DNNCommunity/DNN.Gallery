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
using static DotNetNuke.Common.Globals;
using DotNetNuke.Entities.Tabs;
using static DotNetNuke.Modules.Gallery.Config;
using static DotNetNuke.Modules.Gallery.Utils;

namespace DotNetNuke.Modules.Gallery
{

	/// <summary>
	/// Base class for generic gallery browsing
	/// </summary>
	/// <remarks></remarks>
	public class BaseRequest
	{

		private int mModuleId;

		private DotNetNuke.Modules.Gallery.Config mGalleryConfig;

		private HttpRequest mRequest;
		private ArrayList mFolderPaths = new ArrayList();
		private string mPath = "";
		private GalleryFolder mFolder;
		private Config.GallerySort mSortType;

		private bool mSortDescending;
		#region "Public Properties"
		public DotNetNuke.Modules.Gallery.Config GalleryConfig {
			get { return mGalleryConfig; }
		}

		public int ModuleId {
			get { return mModuleId; }
		}

		public GalleryFolder Folder {
			// Instance of current gallery folder being requested
			get { return mFolder; }
		}

		public Config.GallerySort SortType {
			get { return mSortType; }
			set { mSortType = value; }
		}

		public bool SortDescending {
			get { return mSortDescending; }
			set { mSortDescending = value; }
		}

		public string Path {
			get { return mPath; }
			set { mPath = value; }
		}
		#endregion

		#region "Public Functions"

		public BaseRequest(int ModuleId, Config.GallerySort SortType = Config.GallerySort.Name, bool SortDescending = false, string GalleryPath = "")
		{
			//Dim some vars for this section only
			string[] paths = null;
			int pathCounter = 0;
			FolderDetail newFolderDetail = null;

			mModuleId = ModuleId;
			mSortType = SortType;
			mSortDescending = SortDescending;
			mGalleryConfig = Config.GetGalleryConfig(ModuleId);

			if (!mGalleryConfig.IsValidPath) {
				// Dont want to continue processing if the path is invalid.
				return;
			}

			// Grab the current request context
			mRequest = HttpContext.Current.Request;

			// Check if path has been assigned
			if ((mRequest.QueryString["path"] != null)) {
				mPath = HttpUtility.UrlDecode(mRequest.QueryString["path"]);
				mPath = Utils.FriendlyURLDecode(mPath);
			} else {
				if (mRequest.QueryString["ctl"] != null | IsActiveTab(mRequest.UrlReferrer)) {
					mPath = Utils.FriendlyURLDecode(GalleryPath);
				} else {
					mPath = mGalleryConfig.InitialGalleryHierarchy;
				}
			}
			// Init the root folder            
			mFolder = mGalleryConfig.RootFolder;

			// Create the root path information
			newFolderDetail = new FolderDetail();
			newFolderDetail.Name = mGalleryConfig.GalleryTitle;
			newFolderDetail.CurrentFolder = mFolder;
			newFolderDetail.URL = Common.Globals.NavigateURL();
			mFolderPaths.Add(newFolderDetail);

			// Logic to determine path structure
			if ((mPath != null) && mPath.Length > 0) {
				try {
					// Split the input into distinct paths
					paths = Strings.Split(mPath, "/");

					// Navigate the path structure
					for (pathCounter = 0; pathCounter <= paths.GetUpperBound(0); pathCounter++) {
						mFolder = (GalleryFolder)mFolder.List.Item(paths[pathCounter]);

						// Create the folder details for this folder
						newFolderDetail = new FolderDetail();
						newFolderDetail.Name = mFolder.Title;
						newFolderDetail.CurrentFolder = mFolder;
						newFolderDetail.URL = mFolder.BrowserURL;
						//newFolderDetail.URL = intermediatePaths(pathCounter)
						mFolderPaths.Add(newFolderDetail);

						//William Severance modified to fix Gemini issue GAL-6168 to
						//permit population of folders on demand when BuildCacheonStart is
						//false.

						if (!mFolder.IsPopulated) {
							if (mGalleryConfig.BuildCacheonStart) {
								Utils.PopulateAllFolders(mFolder, false);
							} else {
								Utils.PopulateAllFolders(mFolder, 2, false);
							}
						}
					}
				// an incorrect folder structure probably returned
				} catch (Exception ex) {
					// Keep the last known good folder
				}

			} else {
			}

		}

		public ArrayList FolderPaths()
		{
			// Returns hierarchy of folder paths to current path
			return mFolderPaths;

		}

		private bool IsActiveTab(System.Uri url)
		{

			if (url == null)
				return false;
			string urlreferrer = url.ToString();
			DotNetNuke.Entities.Tabs.TabInfo currentTab = TabController.CurrentPage;
			int tabid = -1;
			string pageName = "";
			Match m = null;
			m = Regex.Match(urlreferrer, "tabid[/=](?<tabid>\\d+)", RegexOptions.IgnoreCase);
			if (m.Success)
				tabid = int.Parse(m.Groups["tabid"].Value);
			m = Regex.Match(urlreferrer, "/(?<pagename>\\w+)\\.aspx\\??", RegexOptions.IgnoreCase);
			if (m.Success)
				pageName = m.Groups["pagename"].Value.ToLowerInvariant();
			if (tabid == -1) {
				return pageName == currentTab.TabName.ToLowerInvariant();
			} else {
				return tabid == currentTab.TabID && pageName == "default";
			}
		}

		#endregion

	}

}
