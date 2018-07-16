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
using static DotNetNuke.Common.Globals;
using static DotNetNuke.Modules.Gallery.Config;
using static DotNetNuke.Modules.Gallery.Utils;

namespace DotNetNuke.Modules.Gallery
{

	/// <summary>
	/// Class for traditional gallery viewing (i.e., 'strip' viewing)
	/// </summary>
	/// <remarks></remarks>
	public class GalleryRequest : BaseRequest
	{


		private HttpRequest mRequest;
		private int _currentStrip;
		private int _stripCount;
		private int _startItem;
		private int _endItem;
		private int _firstImageIndex = -1;

		private ArrayList _pagerItems = new ArrayList();
		#region "Public Properties"

		public int StartItem {
			// Beginning item of this page
			get { return _startItem; }
		}

		public virtual int EndItem {
			// Ending item of this page
			get { return _endItem; }
		}

		public int CurrentStrip {
			// Current page being viewed
			get { return _currentStrip; }
		}

		public virtual int StripCount {
			// Count of pages existing in this folder
			get { return _stripCount; }
		}

		public virtual int FirstImageIndex {
			get { return _firstImageIndex; }
		}

		public virtual ArrayList PagerItems()
		{
			// Collection of pages to be clicked on for navigation

			return _pagerItems;

		}

		public virtual ArrayList CurrentItems()
		{
			// Collection of items for current page only

			ArrayList result = new ArrayList();
			int intCounter = 0;

			if (_startItem > 0) {
				for (intCounter = _startItem; intCounter <= _endItem; intCounter++) {
					// New since v2.0 for using a image to be album icon                    
					result.Add(base.Folder.List.Item(intCounter - 1));
					//End If

					// find the first image in these current items
					if (base.Folder.List.Item(intCounter - 1).Type == Config.ItemType.Image && _firstImageIndex == -1) {
						_firstImageIndex = (intCounter - 1);
					}
				}
			}

			return result;

		}

		// WES Note: Function SubAlbumItems is never called in current code

		public List<GalleryFolder> SubAlbumItems()
		{
			// Collection of items for current page only

			List<GalleryFolder> result = new List<GalleryFolder>();

			for (int intCounter = 0; intCounter <= base.Folder.List.Count - 1; intCounter++) {
				if (base.Folder.List.Item(intCounter).IsFolder) {
					result.Add((GalleryFolder)base.Folder.List.Item(intCounter));
				}
			}

			return result;

		}

		// WES Note - Function FileItems is never called in current code

		public List<GalleryFile> FileItems()
		{
			// Collection of items for current page only

			List<GalleryFile> result = new List<GalleryFile>();

			for (int intCounter = 0; intCounter <= base.Folder.List.Count - 1; intCounter++) {
				if (!base.Folder.List.Item(intCounter).IsFolder) {
					result.Add((GalleryFile)base.Folder.List.Item(intCounter));
				}
			}

			return result;

		}

		#endregion

		#region "Public Methods"
		// Constructor method for the traditional gallery request
		public GalleryRequest(int ModuleID, Config.GallerySort SortType = Config.GallerySort.Name, bool SortDescending = false, string GalleryPath = "") : base(ModuleID, SortType, SortDescending, GalleryPath)
		{
			PortalSettings _portalSettings = DotNetNuke.Entities.Portals.PortalController.GetCurrentPortalSettings();

			// Don't want to continue processing if this is an invalid path
			if (!GalleryConfig.IsValidPath) {
				return;
			}

			PagerDetail newPagerDetail = null;
			int pagerCounter = 0;

			// Grab current request context
			mRequest = HttpContext.Current.Request;

			// Logic to determine paging
			_currentStrip = Convert.ToInt32(mRequest.QueryString["CurrentStrip"]);

			try {
				// Get the count of pages for this folder
				_stripCount = Convert.ToInt32(System.Math.Ceiling((double)base.Folder.List.Count / (double)base.GalleryConfig.ItemsPerStrip));

			} catch (Exception ex) {
				_stripCount = 1;
			}

			// Do a little validation
			if (_currentStrip == 0 || (_currentStrip > _stripCount)) {
				_currentStrip = 1;
			}

			// Calculate the starting item
			if (base.Folder.List.Count == 0) {
				_startItem = 0;
			} else {
				_startItem = (_currentStrip - 1) * base.GalleryConfig.ItemsPerStrip + 1;
			}
			// and calculate the ending item
			_endItem = _startItem + base.GalleryConfig.ItemsPerStrip - 1;
			if (_endItem > base.Folder.List.Count) {
				_endItem = base.Folder.List.Count;
			}

			// Creates the pager items / Create the previous item
			string url = null;
			if (_stripCount > 1 && _currentStrip > 1) {
				url = Common.Globals.NavigateURL(_portalSettings.ActiveTab.TabID, "", Utils.RemoveEmptyParams(new string[2] {
					"path=" + Path,
					"currentstrip=" + (_currentStrip - 1).ToString()
				}));
				newPagerDetail = new PagerDetail();
				newPagerDetail.Strip = _currentStrip - 1;
				newPagerDetail.Text = Localization.GetString("Previous", base.GalleryConfig.SharedResourceFile);
				newPagerDetail.URL = url;
				_pagerItems.Add(newPagerDetail);
			}

			// Creates folder items
			for (pagerCounter = 1; pagerCounter <= _stripCount; pagerCounter++) {
				newPagerDetail = new PagerDetail();
				newPagerDetail.Strip = pagerCounter;
				newPagerDetail.Text = Convert.ToString(pagerCounter);
				if (string.IsNullOrEmpty(Path)) {
					newPagerDetail.URL = Common.Globals.ApplicationURL() + "&currentstrip=" + pagerCounter.ToString();
				} else {
					newPagerDetail.URL = Common.Globals.ApplicationURL() + "&path=" + Path + "&currentstrip=" + pagerCounter.ToString();
				}
				_pagerItems.Add(newPagerDetail);
			}

			// Creates the next item
			if (_stripCount > 1 && _currentStrip < _stripCount) {
				url = Common.Globals.NavigateURL(_portalSettings.ActiveTab.TabID, "", Utils.RemoveEmptyParams(new string[2] {
					"path=" + Path,
					"currentstrip=" + (_currentStrip + 1).ToString()
				}));
				newPagerDetail = new PagerDetail();
				newPagerDetail.Strip = _currentStrip + 1;
				newPagerDetail.Text = "Next";
				newPagerDetail.URL = url;
				_pagerItems.Add(newPagerDetail);
			}

		}

		#endregion

	}

}
