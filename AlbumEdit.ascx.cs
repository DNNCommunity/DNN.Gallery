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
using static System.Web.UI.Control;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Modules.Actions;
using static DotNetNuke.Common.Globals;
using static DotNetNuke.Modules.Gallery.Config;
using static DotNetNuke.Modules.Gallery.Utils;
using DotNetNuke.Modules.Gallery.WebControls;

namespace DotNetNuke.Modules.Gallery
{

	/// -----------------------------------------------------------------------------
	/// <summary>
	/// The AlbumEdit Class provides
	/// </summary>
	/// <remarks>
	/// </remarks>
	/// <history>
	/// </history>
	/// -----------------------------------------------------------------------------
	public abstract partial class AlbumEdit : Entities.Modules.PortalModuleBase, Entities.Modules.IActionable
	{
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

		}

		#endregion

		private GalleryRequest mRequest;
		private DotNetNuke.Modules.Gallery.Authorization mGalleryAuthorize;
		private DotNetNuke.Modules.Gallery.Config mGalleryConfig;
		private GalleryFolder mGalleryAlbum;
		private int mCurrentStrip = 1;
		private int mUserID = -1;
		private string mAction = "";

		private bool mIsRootFolder;
		#region "Public Properties"
		public GalleryFolder GalleryAlbum {
			get { return mGalleryAlbum; }
			set { mGalleryAlbum = value; }
		}

		public DotNetNuke.Modules.Gallery.Authorization GalleryAuthorize {
			get { return mGalleryAuthorize; }
			set { mGalleryAuthorize = value; }
		}

		public DotNetNuke.Modules.Gallery.Config GalleryConfig {
			get { return mGalleryConfig; }
		}

		public string ReturnCtl {
			get {
				if ((ViewState["ReturnCtl"] != null)) {
					return Convert.ToString(ViewState["ReturnCtl"]);
				} else {
					return "AlbumEdit;";
				}
			}
			set { ViewState["ReturnCtl"] = value; }
		}

		public string ReturnURL {
			get {
				System.Collections.Generic.List<string> @params = new System.Collections.Generic.List<string>();
				if ((mRequest.Folder.Parent != null) && !string.IsNullOrEmpty(mRequest.Folder.Parent.GalleryHierarchy)) {
					@params.Add("path=" + Utils.FriendlyURLEncode(mRequest.Folder.Parent.GalleryHierarchy));
				}
				if (mRequest.CurrentStrip > 0)
					@params.Add("currentstrip=" + mRequest.CurrentStrip.ToString());
				return Common.Globals.NavigateURL("", @params.ToArray());
			}
		}

		#endregion

		#region "Public Methods"
		//William Severance - Added to permit child controls of album (such as file upload control) to refresh the
		//grid of album's contents
		public void RefreshAlbumContentsGrid()
		{
			if (rowAlbumGrid.Visible) {
				ControlAlbum1.BindData();
			}
		}
		#endregion

		#region "Private Methods"
		private void BindData()
		{
			var _with1 = mGalleryAlbum;
			txtName.Text = _with1.Name;
			txtTitle.Text = _with1.Title;
			txtAuthor.Text = _with1.Author;
			txtClient.Text = _with1.Client;
			txtLocation.Text = _with1.Location;
			txtApprovedDate.Text = Utils.DateToText(_with1.ApprovedDate);
			//WES - Show Date.MaxValue As blank
			txtDescription.Text = _with1.Description;

			//William Severance - Added to display gallery owner when private gallery and user is Admin
			BindOwnerLookup(_with1.OwnerID);

			//William Severance - uncommenting this line will cause the Approved Date field and
			//calendar hyperlink to be hidden to all except the 1) Gallery owner, 2) the parent album owner, or
			//3) the module administrators. Leaving them commented keeps the same behavior as in previous
			//versions in which the Approved Date field is visible to all with edit permission on the module.

			//Hans Zassenhaus - Updated to include that the row will only be displayed if the Album configururation
			//does NOT include AutpApproval.
			if (mGalleryAuthorize.HasItemApprovalPermission(mGalleryAlbum.Parent) && !mGalleryConfig.AutoApproval) {
				rowApprovedDate.Visible = true;
			} else {
				rowApprovedDate.Visible = false;
			}

		}


		private void BindCategories()
		{
			ArrayList catList = mGalleryConfig.Categories;
			string catString = null;

			// Clear existing items in checkboxlist
			lstCategories.Items.Clear();


			foreach (string catString_loopVariable in catList) {
				catString = catString_loopVariable;
				ListItem catItem = new ListItem();
				catItem.Value = catString;
				catItem.Selected = false;

				if (Strings.InStr(1, Strings.LCase(mGalleryAlbum.Categories), Strings.LCase(catString)) > 0) {
					catItem.Selected = true;
				}
				//list category for current item
				lstCategories.Items.Add(catItem);
			}

		}

		//William Severance - Modified to allow specification of per album OwnerID


		private void BindOwnerLookup(int OwnerID)
		{
			if (mGalleryConfig.IsPrivate && mGalleryAuthorize.HasAdminPermission()) {
				rowOwner.Visible = true;

				//William Severance - Modified to use shared method GetUser and to handle possibility of invalid OwnerId

				var _with2 = ctlOwnerLookup;
				_with2.ItemClass = ObjectClass.DNNUser;
				_with2.Image = "sp_user.gif";
				_with2.LookupSingle = true;
				if (OwnerID != -1) {
					Entities.Users.UserController uc = new Entities.Users.UserController();
					Entities.Users.UserInfo owner = uc.GetUser(PortalId, Utils.ValidUserID(OwnerID));
					if ((owner != null))
						ctlOwnerLookup.AddItem(owner.UserID, owner.Username);
				}
			} else {
				rowOwner.Visible = false;
			}

		}

		private void EnableRootFolderView()
		{
			this.rowDetails.Visible = false;
			this.rowAlbumGrid.Visible = true;
			this.rowUpload.Visible = false;
			this.rowRefuse.Visible = false;
			this.lblInfo.Text = DotNetNuke.Services.Localization.Localization.GetString("RootFolderDetails", LocalResourceFile);
			cmdUpdate.Visible = false;
			cmdCancel.Visible = true;
		}

		private void EnableViewFolder(bool Enable)
		{
			this.rowDetails.Visible = Enable;
			this.rowAlbumGrid.Visible = Enable;
			this.rowUpload.Visible = (!Enable);
			this.rowRefuse.Visible = (!Enable);
			this.lblInfo.Text = DotNetNuke.Services.Localization.Localization.GetString("AlbumDetails", LocalResourceFile);
			//"AlbumDetails"
			cmdUpdate.Visible = Enable;
			cmdCancel.Visible = true;
			BindData();
			BindCategories();
		}

		private void EnableAddFile(bool Enable)
		{
			rowUpload.Visible = Enable;
			this.rowAlbumGrid.Visible = Enable;
			this.rowDetails.Visible = (!Enable);
			this.rowRefuse.Visible = (!Enable);
			this.lblInfo.Text = DotNetNuke.Services.Localization.Localization.GetString("AddFiles", LocalResourceFile);
			//"Add Files"
			cmdUpdate.Visible = false;
			cmdCancel.Visible = false;
		}

		private void EnableAddFolder(bool Enable)
		{
			this.txtName.Enabled = Enable;
			this.rowDetails.Visible = Enable;
			this.rowUpload.Visible = (!Enable);
			this.rowAlbumGrid.Visible = (!Enable);
			this.rowRefuse.Visible = (!Enable);
			this.lblInfo.Text = DotNetNuke.Services.Localization.Localization.GetString("AddFolders", LocalResourceFile);
			//"Add Folders"
			this.cmdUpdate.Visible = Enable;
			this.cmdCancel.Visible = true;
			BindCategories();
			//William Severance - Added to display/select owner when private gallery and user is Admin
			BindOwnerLookup(mGalleryConfig.OwnerID);

		}

		private string GetCategories(CheckBoxList List)
		{
			ListItem catItem = null;
			// JIMJ init string
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

		#endregion

		#region "Event Handlers"

		private void Page_Load(System.Object sender, System.EventArgs e)
		{
			// Load the styles
			((CDefault)Page).AddStyleSheet(Common.Globals.CreateValidID(GalleryConfig.Css), GalleryConfig.Css);

			mRequest = new GalleryRequest(ModuleId);

			// Load gallery menu, this method allow call request only once
			GalleryMenu galleryMenu = (GalleryMenu)LoadControl("Controls/ControlGalleryMenu.ascx");
			var _with3 = galleryMenu;
			_with3.ModuleID = ModuleId;
			_with3.GalleryRequest = (BaseRequest)mRequest;
			celGalleryMenu.Controls.Add(galleryMenu);

			// Load gallery breadcrumbs, this method allow call request only once
			BreadCrumbs galleryBreadCrumbs = (BreadCrumbs)LoadControl("Controls/ControlBreadCrumbs.ascx");
			var _with4 = galleryBreadCrumbs;
			_with4.ModuleID = ModuleId;
			_with4.GalleryRequest = (BaseRequest)mRequest;
			celBreadcrumbs.Controls.Add(galleryBreadCrumbs);

			mGalleryAuthorize = new DotNetNuke.Modules.Gallery.Authorization(ModuleConfiguration);
			mGalleryAlbum = mRequest.Folder;

			if (!mGalleryAuthorize.IsItemOwner(mGalleryAlbum)) {
				this.cmdUpdate.Visible = false;
				this.rowDetails.Visible = false;
				this.rowAlbumGrid.Visible = false;
				this.rowUpload.Visible = false;
				this.rowRefuse.Visible = true;
				this.lblRefuse.Text = "<br /><br />" + Localization.GetString("lblRefuse", this.LocalResourceFile) + "<br /><br />";
				return;
			}

			//<tamttt:note if this folder is root folder, no info to be updated
			//because we should do it on settings page
			if (mGalleryAlbum.Path == mGalleryConfig.RootFolder.Path) {
				mIsRootFolder = true;
			}

			mUserID = Utils.ValidUserID(DotNetNuke.Entities.Users.UserController.GetCurrentUserInfo().UserID);

			if ((Request.QueryString["action"] != null)) {
				mAction = Request.QueryString["action"];
			}

			if ((Request.QueryString["currentstrip"] != null)) {
				mCurrentStrip = Int16.Parse(Request.QueryString["currentstrip"]);
			}

			//WES - Added for date range validator
			valApprovedDate.MinimumValue = DateTime.Today.AddYears(-5).ToShortDateString();
			valApprovedDate.MaximumValue = DateTime.Today.AddYears(5).ToShortDateString();

			cmdApprovedDate.NavigateUrl = DotNetNuke.Common.Utilities.Calendar.InvokePopupCal(txtApprovedDate);

			// Ensure Gallery edit permissions (Andrew Galbraith Ryer)
			//mGalleryAuthorize.HasEditPermission Then
			if (mAction != "addfile" & !mGalleryAuthorize.HasItemEditPermission(mGalleryAlbum)) {
				Response.Redirect(Common.Globals.AccessDeniedURL(Localization.GetString("Insufficient_Edit_Permissions", mGalleryConfig.SharedResourceFile)));
			}

			if (!Page.IsPostBack && mGalleryConfig.IsValidPath) {
				switch (mAction.ToLower()) {
					case "addfile":
						EnableAddFile(true);
						break;
					case "addfolder":
						EnableAddFolder(true);
						break;
					default:
						if (!mIsRootFolder) {
							EnableViewFolder(true);
						} else {
							EnableRootFolderView();
						}
						break;
				}
			}

		}

		private void cmdCancel_Click(System.Object sender, System.EventArgs e)
		{
			// William Severance - GAL-8353 added to remove cached GalleryUploadCollection upon cancelation from adding files.
			if (rowUpload.Visible && (galControlUpload != null)) {
				GalleryUploadCollection.ResetList(galControlUpload.FileListCacheKey);
			}
			Response.Redirect(ReturnURL);
		}


		private void cmdUpdate_Click(System.Object sender, System.EventArgs e)
		{
			if (Page.IsValid) {
				DateTime approvedDate = default(DateTime);
				PortalSecurity Security = new PortalSecurity();

				if (txtApprovedDate.Text.Length > 0) {
					approvedDate = DateTime.Parse(txtApprovedDate.Text);
				} else {
					//William Severance - Modification to auto-approve album creation by Gallery owner,
					//or parent album owner as well as Administrators. Remove comment on first line and
					//comment out second line below to apply change:

					GalleryFolder parentAlbum = null;
					if (mAction.ToLower() == "addfolder") {
						parentAlbum = mGalleryAlbum;
					} else {
						parentAlbum = mGalleryAlbum.Parent;
					}
					if (mGalleryConfig.AutoApproval || mGalleryAuthorize.HasItemApprovalPermission(parentAlbum)) {
						//If mGalleryConfig.AutoApproval Or Authorization.HasAdminPermission() Then
						approvedDate = DateTime.Today;
					} else {
						approvedDate = DateTime.MaxValue;
					}
				}

				//GAL-6255
				lblAlbumName.Text = "";

				//William Severance - modified to add PortalSecurity input filter to prevent scripting hacks
				//  also added range validator of type = "Date" to ApprovedDate text box.

				string Title = Security.InputFilter(txtTitle.Text, PortalSecurity.FilterFlag.NoScripting);
				string Author = Security.InputFilter(txtAuthor.Text, PortalSecurity.FilterFlag.NoScripting);
				string Location = Security.InputFilter(txtLocation.Text, PortalSecurity.FilterFlag.NoScripting);
				string Client = Security.InputFilter(txtClient.Text, PortalSecurity.FilterFlag.NoScripting);
				string Description = Security.InputFilter(txtDescription.Text, PortalSecurity.FilterFlag.NoScripting);
				int OwnerID = 0;

				if (rowOwner.Visible) {
					string sOwnerID = ctlOwnerLookup.ResultItems.Replace(";", "");
					if (sOwnerID == string.Empty) {
						OwnerID = mGalleryConfig.OwnerID;
					} else {
						OwnerID = int.Parse(sOwnerID);
					}
				} else {
					OwnerID = mUserID;
				}

				if (mAction.ToLower() == "addfolder") {
					if (!mGalleryAlbum.ValidateGalleryName(txtName.Text)) {
						lblAlbumName.Text = Localization.GetString("validateCharacters4txtName.ErrorMessage", this.LocalResourceFile);
						return;
					}
					int newFolderID = mGalleryAlbum.CreateChild(txtName.Text, Title, Description, Author, Location, Client, GetCategories(lstCategories), OwnerID, approvedDate);
					switch (newFolderID) {
						case -1:
							//Unspecified error in creating new child album
							lblAlbumName.Text = Localization.GetString("CreateChildAlbumFailed.ErrorMessage", LocalResourceFile);
							break;
						case -2:
							//Child album name already exists.
							lblAlbumName.Text = Localization.GetString("DuplicateAlbumName.ErrorMessage", LocalResourceFile);
							break;
						default:
							Response.Redirect(Utils.GetURL(Page.Request.ServerVariables["URL"], Page, "", "action="));
							break;
					}
				} else {
					var _with5 = mGalleryAlbum;
					_with5.Title = Title;
					_with5.Author = Author;
					_with5.Location = Location;
					_with5.Client = Client;
					_with5.Description = Description;
					_with5.Categories = GetCategories(lstCategories);
					_with5.ApprovedDate = approvedDate;
					if (rowOwner.Visible) {
						_with5.OwnerID = OwnerID;
					}
					mGalleryAlbum.Save();
					//William Severance - Added to return to gallery page or maintenance control
					Response.Redirect(ReturnURL);
				}
			}

		}

		#endregion

		#region "Optional Interfaces"

		public ModuleActionCollection ModuleActions {
			//Implements Entities.Modules.IActionable.ModuleActions
			get {
				ModuleActionCollection Actions = new ModuleActionCollection();
				Actions.Add(GetNextActionID(), Localization.GetString("Configuration.Action", LocalResourceFile), ModuleActionType.ModuleSettings, "", "edit.gif", EditUrl(ControlKey: "configuration"), "", false, SecurityAccessLevel.Admin, true,
				false);
				Actions.Add(GetNextActionID(), Localization.GetString("GalleryHome.Action", LocalResourceFile), ModuleActionType.ContentOptions, "", "icon_moduledefinitions_16px.gif", Common.Globals.NavigateURL(), false, SecurityAccessLevel.Edit, true, false);
				return Actions;
			}
		}
		public AlbumEdit()
		{
			Load += Page_Load;
			Init += Page_Init;
		}

		#endregion

	}

}

