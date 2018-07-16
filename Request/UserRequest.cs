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

	[Serializable()]
	public class GalleryUserRequest : GalleryRequest
	{

		//Private mRemovedItems As Integer
		private int mStripCount;
		private int mEndItem;
		private ArrayList mPagerItems = new ArrayList();

		private List<IGalleryObjectInfo> mSortList;

		public GalleryUserRequest(int ModuleID) : base(ModuleID)
		{
			mSortList = base.Folder.SortList;

		}

		public GalleryUserRequest(int ModuleID, Config.GallerySort Sort, bool SortDESC, string Path = "") : base(ModuleID, Sort, SortDESC, Path)
		{
			if (base.GalleryConfig.IsValidPath) {
				mSortList = base.Folder.SortList;
			}

			mEndItem = base.EndItem;

		}

		public List<IGalleryObjectInfo> SortList {
			get { return mSortList; }
		}

		public override int StripCount {
			get { return mStripCount; }
		}

		public override int EndItem {
			get { return mEndItem; }
		}

		public override ArrayList PagerItems()
		{

			return mPagerItems;

		}

		public ArrayList ValidImages()
		{
			ArrayList result = new ArrayList();
			IGalleryObjectInfo item = null;

			//PopulateSortList()
			foreach (IGalleryObjectInfo item_loopVariable in SortList) {
				item = item_loopVariable;
				if (item.Type == Config.ItemType.Image) {
					result.Add(item);
				}
			}

			return result;

		}

		public override ArrayList CurrentItems()
		{
			PortalSettings _portalSettings = DotNetNuke.Entities.Portals.PortalController.GetCurrentPortalSettings();
			ArrayList result = new ArrayList();
			int intCounter = 0;
			PagerDetail newPagerDetail = null;


			if (StartItem > 0) {
				// Do sorting if it's request by visitor
				SortList.Sort(new Comparer(new string[1] { Enum.GetName(typeof(Config.GallerySort), SortType) }, SortDescending));

				int validItems = base.Folder.SortList.Count;
				if (validItems < mEndItem) {
					// Decrease number of current items
					mEndItem = validItems;
				}

				mStripCount = Convert.ToInt32(System.Math.Ceiling((double)validItems / base.GalleryConfig.ItemsPerStrip));

				// these current items to be displayed in GUI, so we make icon invisible
				//(MyBase.EndItem - MyBase.Folder.IconItems.Count)
				for (intCounter = StartItem; intCounter <= mEndItem; intCounter++) {
					result.Add(SortList[intCounter - 1]);
					//End If
				}

				intCounter = 0;
				mPagerItems.Clear();
				// Creates the pager items / Create the previous item
				string url = null;
				if (mStripCount > 1 && base.CurrentStrip > 1) {
					url = Common.Globals.NavigateURL(_portalSettings.ActiveTab.TabID, "", Utils.RemoveEmptyParams(new string[2] {
						"path=" + Path,
						"currentstrip=" + (base.CurrentStrip - 1).ToString()
					}));
					newPagerDetail = new PagerDetail();
					newPagerDetail.Strip = base.CurrentStrip - 1;
					newPagerDetail.Text = Localization.GetString("Previous", base.GalleryConfig.SharedResourceFile);
					newPagerDetail.URL = url;
					mPagerItems.Add(newPagerDetail);
				}

				// Since this version, pager has been changed to handle folder with so many items, it made pager wrapped in previous version
				int displayPage = base.CurrentStrip;
				for (intCounter = 1; intCounter <= mStripCount; intCounter++) {
					url = Common.Globals.NavigateURL(_portalSettings.ActiveTab.TabID, "", Utils.RemoveEmptyParams(new string[2] {
						"path=" + Path,
						"currentstrip=" + intCounter.ToString()
					}));
					if ((intCounter < 3) || (intCounter > displayPage - 2 && intCounter < displayPage + 2) || (intCounter > mStripCount - 2)) {
						newPagerDetail = new PagerDetail();
						newPagerDetail.Strip = intCounter;
						newPagerDetail.Text = Convert.ToString(intCounter);
						newPagerDetail.URL = url;
						mPagerItems.Add(newPagerDetail);
					} else {
						//If ((intCounter = 3) OrElse (intCounter = mStripCount - 2)) Then
						if ((intCounter == displayPage + 2) || (intCounter == displayPage - 2)) {
							newPagerDetail = new PagerDetail();
							newPagerDetail.Strip = intCounter;
							newPagerDetail.Text = "...";
							newPagerDetail.URL = url;
							mPagerItems.Add(newPagerDetail);
						}
					}
				}

				// Creates the next item
				if (mStripCount > 1 && base.CurrentStrip < mStripCount) {
					url = Common.Globals.NavigateURL(_portalSettings.ActiveTab.TabID, "", Utils.RemoveEmptyParams(new string[2] {
						"path=" + Path,
						"currentstrip=" + (base.CurrentStrip + 1).ToString()
					}));
					newPagerDetail = new PagerDetail();
					newPagerDetail.Strip = base.CurrentStrip + 1;
					newPagerDetail.Text = Localization.GetString("Next", base.GalleryConfig.SharedResourceFile);
					newPagerDetail.URL = url;
					//ApplicationURL() & "&path=" & Path & "&currentstrip=" & (MyBase.CurrentStrip + 1).ToString
					mPagerItems.Add(newPagerDetail);
				}
				//End If
			}

			return result;

		}

	}

}
