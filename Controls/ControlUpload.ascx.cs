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
using DotNetNuke.UI;
using static DotNetNuke.UI.Utilities.DNNClientAPI;

namespace DotNetNuke.Modules.Gallery.WebControls
{

	public abstract partial class Upload : GalleryWebControlBase
	{
		// WES Refactored to inherit from new base class not System.Web.UI.UserControl

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

		private GalleryUploadCollection mUploadCollection = null;
		private DotNetNuke.Modules.Gallery.AlbumEdit mAlbumEdit;
		private DotNetNuke.Modules.Gallery.GalleryFolder mAlbumObject;
		private string strFolderInfo;
		private string strTotalSize;

		private bool mEnablePendingForUnauthenticatedUsers = true;
		private const string PendingFileListCacheKeyCookieName = "PendingFileListCacheKey";
		private const string minIcon = "images/minus.gif";

		private const string maxIcon = "images/plus.gif";

		Regex guidValidationRgx = new Regex("^(\\{)?[0-9a-fA-F]{8}\\-([0-9a-fA-F]{4}\\-){3}[0-9a-fA-F]{12}(\\})?$");

		#region "Event Handlers"

		private void Page_Load(System.Object sender, System.EventArgs e)
		{
			if (!GalleryAuthorization.HasItemUploadPermission(AlbumObject)) {
				Response.Redirect(Common.Globals.AccessDeniedURL(HttpUtility.UrlEncode(Localization.GetString("Insufficient_Upload_Permissions", GalleryConfig.SharedResourceFile))), true);
			}

			if (!AlbumObject.IsPopulated) {
				Response.Redirect(Common.Globals.NavigateURL());
			} else {
				AlbumObject.GalleryObjectDeleted += AlbumObject_Deleted;
			}

			if (!IsPostBack) {
				trPendingUploads.Visible = Request.IsAuthenticated || mEnablePendingForUnauthenticatedUsers;

				if (HasPendingFiles) {
					PendingFiles.Clear();
					DotNetNuke.UI.Skins.Skin.AddModuleMessage(ParentContainer, Localization.GetString("GalleryUploadsCollectionCache_Cleared", LocalResourceFile), DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.YellowWarning);
				}
				ShowInstructions("Instructions.Text");
				if (GalleryConfig.IsValidPath) {
					ShowInfo();
					BindCategories();
					Localization.LocalizeDataGrid(ref grdUpload, LocalResourceFile);
					lblUpload.Text = Localization.GetString("No_Uploads_Pending", LocalResourceFile);
				} else {
					//TODO: do something if not IsValidPath
				}
			}
		}

		/// <summary>
		/// Add the file to the list of files we want to upload
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <remarks></remarks>

		private void cmdAdd_Click(System.Object sender, System.EventArgs e)
		{
			PortalSecurity Security = new PortalSecurity();
			StringBuilder ErrorText = new StringBuilder();

			// Get the individual file we want to upload
			FileToUpload UploadFile = new FileToUpload(htmlUploadFile);

			// Set properties on the file to upload
			// William Severance - modified to add PortalSecurity input filter to prevent scripting hacks
			var _with1 = UploadFile;
			_with1.ModuleID = ModuleId;
			_with1.Title = Security.InputFilter(txtTitle.Text, PortalSecurity.FilterFlag.NoScripting);
			_with1.Author = Security.InputFilter(txtAuthor.Text, PortalSecurity.FilterFlag.NoScripting);
			_with1.Client = Security.InputFilter(txtClient.Text, PortalSecurity.FilterFlag.NoScripting);
			_with1.Location = Security.InputFilter(txtLocation.Text, PortalSecurity.FilterFlag.NoScripting);
			_with1.Description = Security.InputFilter(txtDescription.Text, PortalSecurity.FilterFlag.NoScripting);
			_with1.Categories = Security.InputFilter(GetCategories(lstCategories), PortalSecurity.FilterFlag.NoScripting);
			_with1.OwnerID = UserId;
			_with1.GalleryConfig = GalleryConfig;

			// Check file valid size & type
			string validationInfo = UploadFile.ValidateFile();
			if (Strings.Len(validationInfo) > 0) {
				// Something wrong with basics of file.. show user
				ErrorText.AppendLine(validationInfo);
				ClearFields();
			} else {
				if (PendingFiles.ExceedsQuota(UploadFile.ContentLength)) {
					ErrorText.AppendFormat(Localization.GetString("Would_Exceed_Quota", LocalResourceFile) + Constants.vbCrLf, (UploadFile.ContentLength - PendingFiles.SpaceAvailable) / 1024);
				}
				if (PendingFiles.FileExists(UploadFile.FileName)) {
					ErrorText.AppendLine(Localization.GetString("DuplicateFile", this.LocalResourceFile));
				}
			}

			if (ErrorText.Length > 0) {
				divFileError.InnerHtml = "<br />" + ErrorText.Replace(Constants.vbCrLf, "<br />").ToString();
			} else {
				divFileError.InnerHtml = "";
				PendingFiles.AddFileToUpload(UploadFile);
				if (Request.IsAuthenticated || mEnablePendingForUnauthenticatedUsers) {
					BindUploadCollection();
				} else {
					PerformUploads();
				}
				ClearFields();
			}
		}

		/// <summary>
		/// User wants to remove a file from the grid of uploadable files
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		/// <remarks></remarks>

		private void grdUpload_ItemCommand(object source, System.Web.UI.WebControls.DataGridCommandEventArgs e)
		{
			int itemIndex = e.Item.ItemIndex;
			switch ((((ImageButton)e.CommandSource).CommandName)) {
				case "delete":
					// Remove file from list
					PendingFiles.RemoveFileToUpload(itemIndex);
					BindUploadCollection();
					break;
				case "edit":
					break;
				// Not implemented in this version
			}
		}

		/// <summary>
		/// User wants to upload the file(s)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <remarks></remarks>
		private void btnFileUpload_Click(System.Object sender, System.EventArgs e)
		{
			PerformUploads();
		}

		protected void cmdReturnCancel_Click(object sender, EventArgs e)
		{
			if (cmdReturnCancel.CommandName == "Cancel")
				GalleryUploadCollection.ResetList(FileListCacheKey);
			Response.Redirect(ParentContainer.ReturnURL);
		}

		private void Page_PreRender(object sender, System.EventArgs e)
		{
			SetNavigationControls(HasPendingFiles);
		}

		private void grdUpload_ItemCreated(object sender, System.Web.UI.WebControls.DataGridItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Footer) {
				e.Item.Cells[0].ColumnSpan = 4;
				for (int i = 1; i <= 3; i++) {
					e.Item.Cells.RemoveAt(1);
					//remove next 3 cells
				}
				e.Item.Cells[0].Text = Localization.GetString("Total_Size", LocalResourceFile);
				e.Item.Cells[0].HorizontalAlign = HorizontalAlign.Right;
				e.Item.Cells[1].HorizontalAlign = HorizontalAlign.Center;
			}

		}

		private void grdUpload_ItemDataBound(object sender, System.Web.UI.WebControls.DataGridItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item | e.Item.ItemType == ListItemType.AlternatingItem) {
				FileToUpload f = (FileToUpload)e.Item.DataItem;
				Control btnExpandZipDetails = e.Item.FindControl("btnExpandZipDetails");
				if (f.Extension == ".zip") {
					DataGrid grdZipDetails = (DataGrid)e.Item.FindControl("grdZipDetails");
					DotNetNuke.UI.Utilities.DNNClientAPI.EnableMinMax(btnExpandZipDetails, grdZipDetails, false, Page.ResolveUrl(minIcon), Page.ResolveUrl(maxIcon), MinMaxPersistanceType.Page);
					grdZipDetails.Style.Add("padding-top", "12px");
					Localization.LocalizeDataGrid(ref grdZipDetails, LocalResourceFile);
					grdZipDetails.DataSource = f.ZipHeaders.Entries;
					grdZipDetails.DataBind();
				} else {
					btnExpandZipDetails.Visible = false;
				}
			} else if (e.Item.ItemType == ListItemType.Footer) {
				e.Item.Cells[1].Text = string.Format("{0:F}", PendingFiles.Size / 1024);
			}
		}

		private void AlbumObject_Deleted(object sender, GalleryObjectEventArgs e)
		{
			if (((GalleryFolder)sender).Name == AlbumObject.Name && e.UserID == UserId)
				ShowInfo(true);
			//Update the gallery space display
		}

		#endregion

		#region "Private Methods"

		private void ClearFields()
		{
			txtTitle.Text = string.Empty;
			txtAuthor.Text = string.Empty;
			txtClient.Text = string.Empty;
			txtLocation.Text = string.Empty;
			txtDescription.Text = string.Empty;
			lstCategories.ClearSelection();
		}

		private void PerformUploads()
		{
			divFileError.InnerHtml = "";
			StringBuilder ErrorMessage = new StringBuilder();

			try {
				// Do the uploads
				PendingFiles.DoUpload();

				// Get any non-serious errors
				if (PendingFiles.ErrMessage.Length > 0) {
					ErrorMessage.AppendLine(PendingFiles.ErrMessage);
				}
			} catch (Exception exc) {
				Exceptions.LogException(exc);
				// Add some logging to the screen 
				//ErrorMessage.AppendLine(exc.Message) - removed to eliminate exposure of exc details
			}

			if (ErrorMessage.Length > 0) {
				// Add text to the error label on screen
				ErrorMessage.Insert(0, "<br />" + Localization.GetString("Upload_Error", LocalResourceFile));
				divFileError.InnerHtml = ErrorMessage.Replace(Constants.vbCrLf, "<br />").ToString();
			} else {
				if (GalleryConfig.AutoApproval | GalleryAuthorization.HasAdminPermission()) {
					DotNetNuke.UI.Skins.Skin.AddModuleMessage(ParentContainer, Localization.GetString("Upload_Success", LocalResourceFile), DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.GreenSuccess);
				} else {
					DotNetNuke.UI.Skins.Skin.AddModuleMessage(ParentContainer, Localization.GetString("Upload_Success_Pending_Approval", LocalResourceFile), DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.YellowWarning);
				}
				// Reset the screen
				GalleryUploadCollection.ResetList(FileListCacheKey);
				BindUploadCollection();
				//Clear old file list and reset visibility of pending uploads
				Config.ResetGalleryFolder(AlbumObject);

				ParentContainer.RefreshAlbumContentsGrid();
				//Update album contents grid in parent control
				ShowInfo(true);
				//Update available space
			}
		}

		//William Severance - Added method to hide/show ControlGallery Menu and Bread Crumbs in
		//parent album edit control

		private void SetNavigationControls(bool HasPendingFiles)
		{
			Control celGalleryMenu = ParentContainer.FindControl("celGalleryMenu");
			Control celBreadcrumbs = ParentContainer.FindControl("celBreadcrumbs");
			if (celGalleryMenu != null)
				celGalleryMenu.Controls[0].Visible = !HasPendingFiles;
			if (celBreadcrumbs != null)
				((BreadCrumbs)celBreadcrumbs.Controls[0]).Enable(!HasPendingFiles);

			if (HasPendingFiles) {
				cmdReturnCancel.CommandName = "Cancel";
				cmdReturnCancel.Text = Localization.GetString("cmdCancel", LocalResourceFile);
				DotNetNuke.UI.Utilities.ClientAPI.AddButtonConfirm(cmdReturnCancel, Localization.GetString("Confirm_Cancel", LocalResourceFile));
			} else {
				cmdReturnCancel.CommandArgument = "Return";
				cmdReturnCancel.Text = Localization.GetString("cmdReturn", LocalResourceFile);
				cmdReturnCancel.Attributes.Remove("onClick");
			}

		}

		//William Severance - Added method to simplify databinding of upload collection and
		//change of control visibility depending on mUploadCollection.Count
		private void BindUploadCollection()
		{
			if (Request.IsAuthenticated || mEnablePendingForUnauthenticatedUsers) {
				grdUpload.DataSource = PendingFiles;
				grdUpload.DataBind();
				bool hasPending = HasPendingFiles;
				grdUpload.Visible = hasPending;
				lblUpload.Visible = !hasPending;
				btnFileUpload.Visible = hasPending;
				if (GalleryConfig.MaxPendingUploadsSize > 0) {
					if (PendingFiles.Size > (GalleryConfig.MaxPendingUploadsSize * 1024)) {
						cmdAdd.Enabled = false;
						divFileError.InnerHtml = "<br />" + Localization.GetString("MaxPendingUploadsSize_Exceeded", LocalResourceFile);
					} else {
						cmdAdd.Enabled = true;
						divFileError.InnerHtml = "";
					}
				}
			}
		}


		private void BindCategories()
		{
			ArrayList catList = GalleryConfig.Categories;
			string catString = null;

			// Clear existing items in checkboxlist
			lstCategories.Items.Clear();


			foreach (string catString_loopVariable in catList) {
				catString = catString_loopVariable;
				ListItem catItem = new ListItem();
				catItem.Value = catString;
				catItem.Selected = false;

				if (Strings.InStr(1, Strings.LCase(mAlbumObject.Categories), Strings.LCase(catString)) > 0) {
					catItem.Selected = true;
				}
				//list category for current item
				lstCategories.Items.Add(catItem);

			}
		}

		private void ShowInfo()
		{
			ShowInfo(false);
		}

		private void ShowInfo(bool Refresh)
		{
			PortalSettings ps = PortalController.GetCurrentPortalSettings();
			long spaceAvailable = 0;
			if (GalleryConfig.Quota == 0 && ps.HostSpace == 0) {
				spaceAvailable = long.MinValue;
			} else {
				if (Refresh)
					PendingFiles.RefreshSpaceConstraints();
				spaceAvailable = PendingFiles.SpaceAvailable;
				if (spaceAvailable <= 0) {
					cmdAdd.Enabled = false;
					divFileError.InnerHtml = "<br />" + Localization.GetString("No_Available_Space", LocalResourceFile);
				} else {
					cmdAdd.Enabled = true;
					divFileError.InnerHtml = "";
				}
			}
			tdInfo.InnerHtml = Utils.UploadFileInfo(ModuleId, PendingFiles.GallerySpaceUsed, spaceAvailable);
		}

		private string GetCategories(CheckBoxList List)
		{
			ListItem catItem = null;
			// JIMJ Init string as we concat to it later
			string catString = string.Empty;

			foreach (ListItem catItem_loopVariable in List.Items) {
				catItem = catItem_loopVariable;
				if (catItem.Selected) {
					catString += catItem.Value + ";";
				}
			}

			if (Strings.Len(catString) > 0) {
				return catString.TrimEnd(';');
			} else {
				return "";
			}

		}

		private void ShowInstructions(string LocalizationKey)
		{
			ShowInstructions(LocalizationKey, false, true);
		}

		private void ShowInstructions(string LocalizationKey, bool IsError, bool IsExpanded)
		{
			scnInstructions.IsExpanded = IsError || IsExpanded;
			tdInstructions.InnerHtml = Localization.GetString(LocalizationKey, LocalResourceFile);
			if (IsError) {
				tdInstructions.Style.Add("color", "red");
			}
		}

		#endregion

		#region "Public Properties"

		//William Severance - Added lazy loading properties
		public DotNetNuke.Modules.Gallery.AlbumEdit ParentContainer {
			get {
				if (mAlbumEdit == null) {
					mAlbumEdit = (DotNetNuke.Modules.Gallery.AlbumEdit)NamingContainer;
				}
				return mAlbumEdit;
			}
		}

		public GalleryFolder AlbumObject {
			get {
				if (mAlbumObject == null) {
					mAlbumObject = ParentContainer.GalleryAlbum;
				}
				return mAlbumObject;
			}
		}

		public GalleryUploadCollection PendingFiles {
			get {
				if (mUploadCollection == null) {
					mUploadCollection = GalleryUploadCollection.GetList(AlbumObject, ModuleId, FileListCacheKey);
				}
				return mUploadCollection;
			}
		}

		public bool HasPendingFiles {
			get { return (Request.IsAuthenticated || mEnablePendingForUnauthenticatedUsers) && PendingFiles.Count > 0; }
		}

		public string FileListCacheKey {
			get {
				string strGuid = null;
				HttpCookie cookie = HttpContext.Current.Request.Cookies[PendingFileListCacheKeyCookieName];
				// WES modified to include encryption of cookie and GUID pattern match validation
				if (cookie != null) {
					// Note that EncryptParameter will UrlEncode the parameter so must first UrlDecode
					strGuid = UrlUtils.DecryptParameter(HttpUtility.UrlDecode(cookie.Value));
					if (!guidValidationRgx.IsMatch(strGuid)) {
						string msg = string.Format("Invalid PendingFileListCacheKey GUID ({0}) read from cookie.", HttpUtility.HtmlEncode(strGuid));
						Exceptions.LogException(new DotNetNuke.Services.Exceptions.SecurityException(msg));
						strGuid = null;
					}
				}
				if (strGuid == null) {
					strGuid = Guid.NewGuid().ToString();
					cookie = new HttpCookie(PendingFileListCacheKeyCookieName, UrlUtils.EncryptParameter(strGuid));
					HttpContext.Current.Response.AppendCookie(cookie);
				}
				return string.Format("DNN_Gallery|{0}:{1}", ModuleId, strGuid);
			}
		}

		#endregion

		private void Page_Unload(object sender, System.EventArgs e)
		{
			if ((AlbumObject != null))
				AlbumObject.GalleryObjectDeleted -= AlbumObject_Deleted;
		}
		public Upload()
		{
			Unload += Page_Unload;
			PreRender += Page_PreRender;
			Load += Page_Load;
			Init += Page_Init;
		}
	}

}

