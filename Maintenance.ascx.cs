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
using DotNetNuke.Entities.Modules.Actions;
using static DotNetNuke.Common.Globals;
using DotNetNuke.Entities.Users;
using static DotNetNuke.Modules.Gallery.Utils;
using static DotNetNuke.Modules.Gallery.Config;
using DotNetNuke.Modules.Gallery.WebControls;

namespace DotNetNuke.Modules.Gallery
{

	public abstract partial class Maintenance : DotNetNuke.Entities.Modules.PortalModuleBase, DotNetNuke.Entities.Modules.IActionable
	{

		public string lnkViewText = "";
		#region "Private Members"
		// Obtain PortalSettings from Current Context   
		PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
		private GalleryMaintenanceRequest mRequest;
		private DotNetNuke.Modules.Gallery.Config mGalleryConfig;
		private GalleryFolder mFolder;
		private Gallery.Authorization mGalleryAuthorization;
		private bool mSelectAll;

		private string mReturnCtl;
		//For Localization of FileInfo text
		private string mLocalizedAlbumText;
		private string mLocalizedSourceText;
		private string mLocalizedThumbText;
		private string mLocalizedMissingText;
		private string mLocalizedPresentText;
		private string mLocalizedRebuildThumbText;
		private string mLocalizedCopySourceToFileText;
		private string mLocalizedCopyFileToSourceText;
		private string mYellowWarningImageUrl;

		private string mGreenOKImageUrl;
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

			if (mGalleryConfig == null) {
				mGalleryConfig = Config.GetGalleryConfig(ModuleId);
			}
			//mImageFolder = "../" & mGalleryConfig.ImageFolder.Substring(mGalleryConfig.ImageFolder.IndexOf("/DesktopModules"))

		}

		#endregion

		#region "Optional Interfaces"

		public ModuleActionCollection ModuleActions {
			get {
				ModuleActionCollection Actions = new ModuleActionCollection();
				Actions.Add(GetNextActionID(), Localization.GetString("Configuration.Action", LocalResourceFile), ModuleActionType.ModuleSettings, "", "edit.gif", EditUrl(ControlKey: "configuration"), "", false, SecurityAccessLevel.Admin, true,
				false);
				Actions.Add(GetNextActionID(), Localization.GetString("GalleryHome.Action", LocalResourceFile), ModuleActionType.ContentOptions, "", "icon_moduledefinitions_16px.gif", Common.Globals.NavigateURL(), false, SecurityAccessLevel.Edit, true, false);
				return Actions;
			}
		}

		#endregion

		public string ReturnCtl {
			get { return "Maintenance;"; }
		}

		public DotNetNuke.Modules.Gallery.Config GalleryConfig {
			get { return mGalleryConfig; }
		}


		private void Page_Load(System.Object sender, System.EventArgs e)
		{
			mRequest = new GalleryMaintenanceRequest(ModuleId);
			mGalleryConfig = mRequest.GalleryConfig;
			mFolder = mRequest.Folder;

			mGalleryAuthorization = new Authorization(ModuleId);
			if (!mGalleryAuthorization.HasItemEditPermission(mFolder)) {
				Response.Redirect(Common.Globals.AccessDeniedURL(Localization.GetString("Insufficient_Maintenance_Permissions", mGalleryConfig.SharedResourceFile)));
			}

			// Load the styles
			((CDefault)Page).AddStyleSheet(Common.Globals.CreateValidID(GalleryConfig.Css), GalleryConfig.Css);

			lnkViewText = Localization.GetString("lnkView.Text", LocalResourceFile);
			mLocalizedAlbumText = Localization.GetString("Album.Header", LocalResourceFile);
			mLocalizedSourceText = Localization.GetString("Source.Header", LocalResourceFile);
			mLocalizedThumbText = Localization.GetString("Thumb.Header", LocalResourceFile);
			mLocalizedMissingText = Localization.GetString("Missing", LocalResourceFile);
			mLocalizedPresentText = Localization.GetString("Present", LocalResourceFile);
			mLocalizedRebuildThumbText = Localization.GetString("RebuildThumb", LocalResourceFile);

			mYellowWarningImageUrl = "~/DesktopModules/Gallery/Images/yellow-warning.gif";
			mGreenOKImageUrl = "~/DesktopModules/Gallery/Images/green-ok.gif";

			//William Severance - Added condition Not Page.IsPostBack to fix loss of
			//datagrid header localization following postback. LocalizeDataGrid must be called
			//before datagrid is data bound.

			if (!Page.IsPostBack)
				Localization.LocalizeDataGrid(ref grdContent, LocalResourceFile);

			// Load gallery menu, this method allow call request only once
			GalleryMenu galleryMenu = (GalleryMenu)LoadControl("Controls/ControlGalleryMenu.ascx");
			var _with1 = galleryMenu;
			_with1.ModuleID = ModuleId;
			_with1.GalleryRequest = (BaseRequest)mRequest;
			celGalleryMenu.Controls.Add(galleryMenu);

			// Load gallery breadcrumbs, this method allow call request only once
			BreadCrumbs galleryBreadCrumbs = (BreadCrumbs)LoadControl("Controls/ControlBreadCrumbs.ascx");
			var _with2 = galleryBreadCrumbs;
			_with2.ModuleID = ModuleId;
			_with2.GalleryRequest = (BaseRequest)mRequest;
			celBreadcrumbs.Controls.Add(galleryBreadCrumbs);

			if (!Page.IsPostBack && mGalleryConfig.IsValidPath) {
				if (!mFolder.IsPopulated) {
					Response.Redirect(ApplicationURL());
				}

				BindData();

				btnDeleteAll.Attributes.Add("onClick", "javascript: return confirm('Are you sure you wish to delete selected items?')");

			}

		}


		private void BindData()
		{
			var _with3 = mFolder;
			txtPath.Text = mFolder.URL;
			txtName.Text = _with3.Name;

			lblAlbumInfo.Text = AlbumInfo();
			BindChildItems();

		}

		protected string BrowserURL(object DataItem)
		{
			GalleryMaintenanceFile item = (GalleryMaintenanceFile)DataItem;
			if (mGalleryConfig.AllowPopup) {
				return item.URL;
			} else {
				System.Collections.Generic.List<string> @params = new System.Collections.Generic.List<string>();
				@params.Add("returnctl=" + ReturnCtl);
				if ((Request.QueryString["currentstrip"] != null))
					@params.Add("currentstrip=" + Request.QueryString["currentstrip"]);
				return Utils.AppendURLParameters(item.URL, @params.ToArray());
			}
		}

		protected string FileInfo(object DataItem)
		{
			GalleryMaintenanceFile file = (GalleryMaintenanceFile)DataItem;
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			if (!file.SourceExists)
				sb.Append(mLocalizedSourceText);

			if (!file.ThumbExists) {
				if (sb.Length > 0)
					sb.Append(", ");
				sb.Append(mLocalizedThumbText);
			}

			if (!file.FileExists) {
				if (sb.Length > 0)
					sb.Append(", ");
				sb.Append(mLocalizedAlbumText);
			}

			if (sb.Length > 0) {
				sb.Append(" ");
				sb.Append(mLocalizedMissingText);
			}

			return sb.ToString();
		}

		protected string StatusImage(object DataItem, string FileType)
		{
			GalleryMaintenanceFile file = (GalleryMaintenanceFile)DataItem;
			bool exists = false;
			switch (FileType) {
				case "Thumb":
					exists = file.ThumbExists;
					break;
				case "Source":
					exists = file.SourceExists;
					break;
				case "File":
					exists = file.FileExists;
					break;
				default:
					throw new ArgumentException("Must be 'Thumb', Source' or 'File'", "FileType");
			}
			if (exists) {
				return mGreenOKImageUrl;
			} else {
				return mYellowWarningImageUrl;
			}
		}

		protected string Tooltip(object DataItem, string FileType)
		{
			GalleryMaintenanceFile file = (GalleryMaintenanceFile)DataItem;
			string localizedTooltip = mLocalizedPresentText;

			switch (FileType) {
				case "Thumb":
					if (!file.ThumbExists)
						localizedTooltip = mLocalizedRebuildThumbText;
					break;
				case "Source":
					if (!file.SourceExists)
						localizedTooltip = mLocalizedCopyFileToSourceText;
					break;
				case "File":
					if (!file.FileExists)
						localizedTooltip = mLocalizedCopySourceToFileText;
					break;
				default:
					throw new ArgumentException("Must be 'Thumb', Source' or 'File'", "FileType");
			}
			return localizedTooltip;
		}

		protected string AlbumInfo()
		{
			int itemCount = mRequest.ImageList.Count;
			int sourceCount = 0;
			int albumCount = 0;
			int thumbCount = 0;
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			foreach (GalleryMaintenanceFile file in mRequest.ImageList) {
				if (file.SourceExists)
					sourceCount += 1;
				if (file.FileExists)
					albumCount += 1;
				if (file.ThumbExists)
					thumbCount += 1;
			}

			sb.Append(Localization.GetString("AlbumInfo", LocalResourceFile));
			sb.Replace("[ItemCount]", itemCount.ToString());
			sb.Replace("[AlbumName]", mRequest.Folder.Name);
			sb.Replace("[ItemNoSource]", (itemCount - sourceCount).ToString());
			sb.Replace("[ItemNoAlbum]", (itemCount - albumCount).ToString());
			sb.Replace("[ItemNoThumb]", (itemCount - thumbCount).ToString());
			sb.Replace("[ImageSize]", mGalleryConfig.FixedHeight.ToString() + " x " + mGalleryConfig.FixedWidth.ToString());
			sb.Replace("[ThumbSize]", mGalleryConfig.MaximumThumbHeight.ToString() + " x " + mGalleryConfig.MaximumThumbWidth.ToString());

			return sb.ToString();
		}


		private void BindChildItems()
		{
			grdContent.DataSource = mRequest.ImageList;
			grdContent.PageSize = (mRequest.ImageList.Count + 1);
			grdContent.DataBind();

		}


		private void grdContent_ItemCommand(object source, System.Web.UI.WebControls.DataGridCommandEventArgs e)
		{
			int itemIndex = e.Item.ItemIndex;
			GalleryMaintenanceFile file = (GalleryMaintenanceFile)mRequest.ImageList[itemIndex];

			switch ((((ImageButton)e.CommandSource).CommandName)) {
				case "Delete":
					file.DeleteAll();
					break;
				case "RebuildThumb":
					file.RebuildThumbnail();
					break;
				case "CopySourceToFile":
					file.CreateFileFromSource();
					break;
				case "CopyFileToSource":
					file.CreateFileFromSource();
					break;
				default:
					throw new ArgumentException("Invalid maintenance command", "CommandName");
			}

			mRequest.Folder.Populate(true);
			BindData();

		}

		private void cmdReturn_Click(System.Object sender, System.EventArgs e)
		{
			Config.ResetGalleryConfig(ModuleId);
			Goback();
		}

		private void Goback()
		{
			string url = Utils.GetURL(Page.Request.ServerVariables["URL"], Page, "", "ctl=&selectall=&mid=&currentitem=&media=");
			Response.Redirect(url);
		}

		private List<GalleryMaintenanceFile> GetSelectedFiles()
		{
			int i = 0;
			List<GalleryMaintenanceFile> selList = new List<GalleryMaintenanceFile>();

			for (i = 0; i <= grdContent.Items.Count - 1; i++) {
				DataGridItem myListItem = grdContent.Items[i];
				CheckBox myCheck = (CheckBox)myListItem.FindControl("chkSelect");
				if (myCheck.Checked)
					selList.Add(mRequest.ImageList[i]);
			}
			return selList;

		}


		private void RefreshAlbum()
		{
			mRequest.Folder.Clear();
			// refresh gallery folder first ready for repopulate
			mRequest.Populate();
			BindData();

		}

		private void btnCopySource_Click(object sender, System.Web.UI.ImageClickEventArgs e)
		{
			foreach (GalleryMaintenanceFile file in GetSelectedFiles()) {
				file.CreateFileFromSource();
			}

			RefreshAlbum();

		}

		private void btnCopyFile_Click(object sender, System.Web.UI.ImageClickEventArgs e)
		{
			foreach (GalleryMaintenanceFile file in GetSelectedFiles()) {
				file.CreateSourceFromFile();
			}

			RefreshAlbum();

		}

		private void btnCreateThumb_Click(object sender, System.Web.UI.ImageClickEventArgs e)
		{
			foreach (GalleryMaintenanceFile file in GetSelectedFiles()) {
				file.RebuildThumbnail();
			}

			RefreshAlbum();

		}

		private void ClearCache1_Click(object sender, System.Web.UI.ImageClickEventArgs e)
		{
			Config.ResetGalleryConfig(ModuleId);
		}

		private void btnSyncAll_Click(System.Object sender, System.EventArgs e)
		{
			foreach (GalleryMaintenanceFile file in GetSelectedFiles()) {
				file.Synchronize();
			}

			RefreshAlbum();
		}

		private void btnDeleteAll_Click(System.Object sender, System.EventArgs e)
		{
			PortalSettings ps = PortalController.GetCurrentPortalSettings();
			foreach (GalleryMaintenanceFile gmFile in GetSelectedFiles()) {
				try {
					if (File.Exists(gmFile.AlbumPath))
						mFolder.DeleteFile(gmFile.AlbumPath, ps, true);
					if (gmFile.Type == Config.ItemType.Image) {
						if (File.Exists(gmFile.ThumbPath))
							mFolder.DeleteFile(gmFile.ThumbPath, ps, true);
						if (File.Exists(gmFile.SourcePath))
							mFolder.DeleteFile(gmFile.SourcePath, ps, true);

						//Reset folder thumbnail to "folder.gif" if its current
						//thumbnail is being deleted. Will then get set to next image thumbnail during populate if there is one.
						if (mFolder.Thumbnail == gmFile.Name)
							mFolder.ResetThumbnail();
					}

					GalleryXML.DeleteMetaData(mFolder.Path, gmFile.Name);

				} catch {
				}
			}

			RefreshAlbum();
		}
		public Maintenance()
		{
			Load += Page_Load;
			Init += Page_Init;
		}

	}

}

