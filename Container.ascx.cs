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
using DotNetNuke.Entities.Modules.Actions;
using static DotNetNuke.Common.Globals;
using static DotNetNuke.Modules.Gallery.Config;
using static DotNetNuke.Modules.Gallery.Utils;
using DotNetNuke.Modules.Gallery.WebControls;

namespace DotNetNuke.Modules.Gallery
{

	public abstract partial class Container : Entities.Modules.PortalModuleBase, Entities.Modules.IActionable, Entities.Modules.ISearchable
	{

		private DotNetNuke.Modules.Gallery.Config mGalleryConfig;
		private DotNetNuke.Modules.Gallery.Authorization mGalleryAuthorize;
		private GalleryUserRequest mUserRequest;
		private ArrayList mCurrentItems = new ArrayList();
		private Config.GallerySort mSort;
		private Config.GalleryView mView;
		private bool mSortDESC;

		private bool mValidGallery;
		#region "Private Methods"


		private void EnableControl(bool IsEnabled)
		{
			ClearCache.Visible = GalleryAuthorize.IsItemOwner(mUserRequest.Folder);

			lblView.Visible = (IsEnabled && mGalleryConfig.AllowChangeView);
			ddlGalleryView.Visible = lblView.Visible;
			lblSortBy.Visible = IsEnabled;
			ddlGallerySort.Visible = IsEnabled;
			chkDESC.Visible = IsEnabled;
			lblStats.Visible = IsEnabled;

		}

		private void BindGalleryView()
		{
			if (!mGalleryConfig.AllowChangeView) {
				return;
			}

			System.Collections.Specialized.ListDictionary dict = new System.Collections.Specialized.ListDictionary();
			foreach (string name in Enum.GetNames(typeof(Config.GalleryView))) {
				dict.Add(name, Localization.GetString("View" + name, mGalleryConfig.SharedResourceFile));
			}

			var _with1 = ddlGalleryView;
			_with1.ClearSelection();
			_with1.DataTextField = "value";
			_with1.DataValueField = "key";
			_with1.DataSource = dict;
			_with1.DataBind();
			if ((_with1.Items.FindByValue(mView.ToString()) != null)) {
				_with1.Items.FindByValue(mView.ToString()).Selected = true;
			}

		}

		private void BindGallerySort()
		{
			System.Collections.Specialized.ListDictionary dict = new System.Collections.Specialized.ListDictionary();
			foreach (string name in mGalleryConfig.SortProperties) {
				dict.Add(name, Localization.GetString("Sort" + name, mGalleryConfig.SharedResourceFile));
			}

			var _with2 = ddlGallerySort;
			_with2.ClearSelection();
			_with2.DataTextField = "value";
			_with2.DataValueField = "key";
			_with2.DataSource = dict;
			_with2.DataBind();
			if ((_with2.Items.FindByValue(mSort.ToString()) != null)) {
				_with2.Items.FindByValue(mSort.ToString()).Selected = true;
			}

			chkDESC.Checked = mSortDESC;

		}

		private string GetStats()
		{

			//William Severance (9/11/10) - Refactored to avoid null reference errors, display of unapproved item count to non-admin/non-owner users
			string stats = "";
			if (mUserRequest != null && mUserRequest.Folder != null && mUserRequest.CurrentItems().Count > 0) {
				int iconCount = mUserRequest.Folder.IconItems.Count;
				int iunApprovedCount = mUserRequest.Folder.UnApprovedItems.Count;


				if ((iunApprovedCount == 0) || (!mGalleryAuthorize.IsItemOwner(mUserRequest.Folder))) {
					stats = Localization.GetString("GalleryStatsApproved", LocalResourceFile);
					stats = stats.Replace("[TotalItem]", (mUserRequest.Folder.List.Count - iconCount - iunApprovedCount).ToString());
					// - mUserRequest.RemoveItems).ToString)
				} else {
					stats = Localization.GetString("GalleryStatsUnApproved", LocalResourceFile);
					stats = stats.Replace("[TotalItem]", (mUserRequest.Folder.List.Count - iconCount).ToString());
					//- mUserRequest.RemoveItems).ToString)
					stats = stats.Replace("[Unapproved]", iunApprovedCount.ToString());
				}

				stats = stats.Replace("[StartItem]", (mUserRequest.StartItem).ToString());
				stats = stats.Replace("[EndItem]", (mUserRequest.CurrentItems().Count + mUserRequest.StartItem - 1).ToString());

			}

			return stats;

		}


		private void Initialize()
		{
			try {
				if (mGalleryConfig == null) {
					mGalleryConfig = Config.GetGalleryConfig(ModuleId);
				}

				mGalleryAuthorize = new Authorization(ModuleConfiguration);
				mView = Utils.GetView(mGalleryConfig);
				mSort = Utils.GetSort(mGalleryConfig);
				mSortDESC = Utils.GetSortDESC(mGalleryConfig);
				mUserRequest = new GalleryUserRequest(ModuleId, mSort, mSortDESC);

				var _with3 = ctlGallery;
				_with3.PortalID = PortalSettings.PortalId;
				_with3.TabID = PortalSettings.ActiveTab.TabID;
				_with3.ModuleId = ModuleId;
				_with3.GalleryConfig = mGalleryConfig;
				_with3.GalleryAuthorize = mGalleryAuthorize;
				_with3.LocalResourceFile = this.LocalResourceFile;
				_with3.View = mView;
				_with3.Sort = mSort;
				_with3.SortDESC = mSortDESC;

				_with3.UserRequest = mUserRequest;
				_with3.UserRequest.FolderPaths();

				// Load gallery menu, this method allow call request only once
				GalleryMenu galleryMenu = (GalleryMenu)LoadControl("Controls/ControlGalleryMenu.ascx");
				var _with4 = galleryMenu;
				_with4.ModuleID = ModuleId;
				_with4.GalleryRequest = (BaseRequest)mUserRequest;
				celGalleryMenu.Controls.Add(galleryMenu);

				// Load gallery breadcrumbs, this method allow call request only once
				BreadCrumbs galleryBreadCrumbs = (BreadCrumbs)LoadControl("Controls/ControlBreadCrumbs.ascx");
				var _with5 = galleryBreadCrumbs;
				_with5.ModuleID = ModuleId;
				_with5.GalleryRequest = (BaseRequest)mUserRequest;
				celBreadcrumbs.Controls.Add(galleryBreadCrumbs);

				string title = ((CDefault)Page).Title;
				foreach (FolderDetail itm in mUserRequest.FolderPaths()) {
					if (title != itm.Name) {
						title += (" > " + itm.Name);
					}
				}
				((CDefault)Page).Title = title;

			} catch (Exception ex) {
				// JIMJ Add logging when we can't init things,
				// Could be if we can't write to the xml file
				Exceptions.LogException(ex);
				throw;
			}

		}

		#endregion

		#region "Properties"

		public DotNetNuke.Modules.Gallery.Config GalleryConfig {
			get { return mGalleryConfig; }
		}

		public DotNetNuke.Modules.Gallery.Authorization GalleryAuthorize {
			get { return mGalleryAuthorize; }
		}

		public GalleryUserRequest UserRequest {
			get { return mUserRequest; }
		}

		public ArrayList CurrentItems {
			get { return mCurrentItems; }
		}

		public Config.GalleryView View {
			get { return mView; }
		}

		#endregion

		#region "Event Handlers"

		private void Page_Load(System.Object sender, System.EventArgs e)
		{
			try {
				// Load the styles
				((CDefault)Page).AddStyleSheet(Common.Globals.CreateValidID(GalleryConfig.Css), GalleryConfig.Css);

				if (mGalleryConfig.IsValidPath) {
					mValidGallery = true;

					if ((!Page.IsPostBack)) {
						BindGalleryView();
						BindGallerySort();
						EnableControl(mGalleryConfig.RootFolder.List.Count > 0);
						lblStats.Text = GetStats();
					}

				} else {
					mValidGallery = false;
					EnableControl(false);
					lblStats.Text = GetStats() + Localization.GetString("GalleryEmpty", LocalResourceFile);
				}

			} catch (Exception exc) {
				Exceptions.ProcessModuleLoadException(this, exc);
			}

		}

		//William Severance - Modified following three methods to include new parameter ModuleID in call to RefreshCookie method
		//which was revised to provide independent behavior in these settings when multiple gallery modules exist i the same portal.

		// Quinn Gil - Changed Response.Redirect(Request.Url.ToString) to Response.Redirect(SanitizedRawURL(...)) for GAL-9403

		private void ddlGalleryView_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			mView = (Config.GalleryView)Enum.Parse(typeof(Config.GalleryView), ddlGalleryView.SelectedItem.Value);
			Utils.RefreshCookie(Utils.GALLERY_VIEW, ModuleId, mView);
			Response.Redirect(Utils.SanitizedRawUrl(TabId, ModuleId, "", "currentstrip=", Request));
		}

		private void ddlGallerySort_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			mSort = (Config.GallerySort)Enum.Parse(typeof(Config.GallerySort), ddlGallerySort.SelectedItem.Value);
			Utils.RefreshCookie(Utils.GALLERY_SORT, ModuleId, mSort);
			Response.Redirect(Utils.SanitizedRawUrl(TabId, ModuleId, "", "currentstrip=", Request));
		}

		private void chkDESC_CheckedChanged(object sender, System.EventArgs e)
		{
			mSortDESC = chkDESC.Checked;
			Utils.RefreshCookie(Utils.GALLERY_SORT_DESC, ModuleId, mSortDESC);
			Response.Redirect(Utils.SanitizedRawUrl(TabId, ModuleId, "", "currentstrip=", Request));
		}

		private void ClearCache_Click(object sender, System.Web.UI.ImageClickEventArgs e)
		{
			Config.ResetGalleryFolder(mUserRequest.Folder);
			Response.Redirect(Utils.SanitizedRawUrl(TabId, ModuleId));
		}

		#endregion

		#region "Optional Interfaces"
		public ModuleActionCollection ModuleActions {
			//Implements Entities.Modules.IActionable.ModuleActions
			get {
				ModuleActionCollection Actions = new ModuleActionCollection();
				Actions.Add(GetNextActionID(), Localization.GetString("Configuration.Action", LocalResourceFile), ModuleActionType.ModuleSettings, "", "edit.gif", EditUrl(ControlKey: "configuration"), "", false, SecurityAccessLevel.Admin, true,
				false);
				return Actions;
			}
		}

		public Services.Search.SearchItemInfoCollection GetSearchItems(Entities.Modules.ModuleInfo ModInfo)
		{
			//Implements Entities.Modules.ISearchable.GetSearchItems
			// included as a stub only so that the core knows this module Implements Entities.Modules.ISearchable
			return null;
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

			Initialize();

		}
		public Container()
		{
			Init += Page_Init;
			Load += Page_Load;
		}

		#endregion

	}

}

