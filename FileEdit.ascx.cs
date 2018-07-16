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

	public abstract partial class FileEdit : DotNetNuke.Entities.Modules.PortalModuleBase
	{

		#region "Private Members"

		private DotNetNuke.Modules.Gallery.Config mGalleryConfig;
		private GalleryRequest mRequest;
		private GalleryViewerRequest mRequestViewer;
		private int mCurrentItem;
		private GalleryFolder mFolder;

		private GalleryFile mFile;
		#endregion

		#region "Private Methods"

		public DotNetNuke.Modules.Gallery.Config GalleryConfig {
			get { return mGalleryConfig; }
		}

		public string ReturnCtl {
			get {
				if ((ViewState["ReturnCtl"] != null)) {
					return Convert.ToString(ViewState["ReturnCtl"]);
				} else {
					return "FileEdit;";
				}
			}
			set { ViewState["ReturnCtl"] = value; }
		}

		private void BindData()
		{
			var _with1 = mFile;
			txtPath.Text = _with1.URL;
			txtName.Text = _with1.Name;
			txtAuthor.Text = _with1.Author;
			txtTitle.Text = _with1.Title;
			txtLocation.Text = _with1.Location;
			txtClient.Text = _with1.Client;
			txtDescription.Text = _with1.Description;

			txtApprovedDate.Text = Utils.DateToText(_with1.ApprovedDate);
			//WES - show Date.MaxValue as empty string
			txtCreatedDate.Text = _with1.CreatedDate.ToShortDateString();

			lblCurrentFile.Text = _with1.Name;
			tdFileEditImage.Attributes.Add("style", "width:" + (mGalleryConfig.MaximumThumbWidth + 50).ToString() + "px;");
			imgFile.ImageUrl = _with1.ThumbnailURL;

			BindCategories();

			//William Severance - uncommenting these two lines will cause the Approved Date field and
			//calendar hyperlink to be hidden to all except the 1) Gallery owner, 2) the parent album owner, or
			//3) the module administrators. Leaving them commented keeps the same behavior as in previous
			//versions in which the Approved Date field is visible to all with edit permission on the module.

			//HansZassenhaus - updated to show Item Approval date and calendar control only if
			//Configuration for the album does not include Auto Approval, i.e. Moderated Album.

			DotNetNuke.Modules.Gallery.Authorization auth = new DotNetNuke.Modules.Gallery.Authorization(ModuleConfiguration);
			if (auth.HasItemApprovalPermission(mFile.Parent) && !mGalleryConfig.AutoApproval) {
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

				if (Strings.InStr(1, mFile.Categories, catString) > 0) {
					catItem.Selected = true;
				}
				//list category for current item
				lstCategories.Items.Add(catItem);
			}

		}

		private string GetCategories(CheckBoxList List)
		{
			ListItem catItem = null;
			// JIMJ init string as we concat to it later
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

		private void BindNavigation()
		{
			System.Collections.Generic.List<string> @params = new System.Collections.Generic.List<string>();
			string url = null;

			//William Severance - changed mFolder.Name to mFolder.GalleryHierachy and added FriendlyURLEncode processsing to path values
			// Next button to edit next image in this gallery
			if (!string.IsNullOrEmpty(mFolder.GalleryHierarchy)) {
				@params.Add("path=" + Utils.FriendlyURLEncode(mFolder.GalleryHierarchy));
			}
			@params.Add("mid=" + ModuleId.ToString());
			@params.Add("returnctl=" + ReturnCtl);

			// Next button to edit next image in this gallery
			@params.Add("currentitem=" + mRequestViewer.NextItemNumber.ToString());
			url = Common.Globals.NavigateURL(TabId, "FileEdit", @params.ToArray());
			cmdMoveNext.NavigateUrl = url;

			// Previous button to edit previous image in this gallery
			@params[@params.Count - 1] = "currentitem=" + mRequestViewer.PreviousItemNumber.ToString();
			url = Common.Globals.NavigateURL(TabId, "FileEdit", @params.ToArray());
			cmdMovePrevious.NavigateUrl = url;

			// Image Edit Image Button and Link
			@params = new System.Collections.Generic.List<string>();
			if (!mGalleryConfig.AllowPopup) {
				@params.Add("returnctl=" + ReturnCtl);
				if (mRequest.CurrentStrip > 1)
					@params.Add("currentstrip=" + mRequest.CurrentStrip.ToString());
			}
			if (mFile.Type == Config.ItemType.Image) {
				imgFile.NavigateUrl = Utils.AppendURLParameters(mFile.EditImageURL, @params.ToArray());
				lnkEditImage.Visible = true;
				lnkEditImage.NavigateUrl = imgFile.NavigateUrl;
			} else {
				imgFile.NavigateUrl = Utils.AppendURLParameters(mFile.BrowserURL, @params.ToArray());
				lnkEditImage.Visible = false;
			}
		}

		#endregion

		#region "Event Handlers"


		private void Page_Load(System.Object sender, System.EventArgs e)
		{
			// Load the styles
			((CDefault)Page).AddStyleSheet(Common.Globals.CreateValidID(GalleryConfig.Css), GalleryConfig.Css);

			//Sort feature
			//William Severance modified to pass new parameter for GalleryConfig
			Config.GallerySort sort = Utils.GetSort(mGalleryConfig);
			bool sortDesc = Utils.GetSortDESC(mGalleryConfig);

			mRequestViewer = new GalleryViewerRequest(ModuleId, sort, sortDesc);
			mRequest = new GalleryRequest(ModuleId);
			mFolder = mRequest.Folder;
			if ((Request.QueryString["currentitem"] != null)) {
				mCurrentItem = Int32.Parse(Request.QueryString["currentitem"]);
				mFile = (GalleryFile)mRequest.Folder.List.Item(mCurrentItem);
			}

			// Ensure Gallery edit permissions (Andrew Galbraith Ryer)
			Authorization galleryAuthorization = new Authorization(ModuleConfiguration);
			if (!galleryAuthorization.HasItemEditPermission(mFile)) {
				Response.Redirect(Common.Globals.AccessDeniedURL(Localization.GetString("Insufficient_Edit_Permissions", mGalleryConfig.SharedResourceFile)));
			}

			// Load gallery breadcrumbs, this method allow call request only once
			BreadCrumbs galleryBreadCrumbs = (BreadCrumbs)LoadControl("Controls/ControlBreadCrumbs.ascx");
			var _with2 = galleryBreadCrumbs;
			_with2.ModuleID = ModuleId;
			_with2.GalleryRequest = (BaseRequest)mRequest;
			celBreadcrumbs.Controls.Add(galleryBreadCrumbs);

			//WES - Added for date range validator
			valApprovedDate.MinimumValue = DateTime.Today.AddYears(-5).ToShortDateString();
			valApprovedDate.MaximumValue = DateTime.Today.AddYears(5).ToShortDateString();
			valCreatedDate.MinimumValue = DateTime.Today.AddYears(-150).ToShortDateString();
			valCreatedDate.MaximumValue = DateTime.Today.ToShortDateString();

			cmdApprovedDate.NavigateUrl = Common.Utilities.Calendar.InvokePopupCal(txtApprovedDate);
			cmdCreatedDate.NavigateUrl = Common.Utilities.Calendar.InvokePopupCal(txtCreatedDate);

			cmdMovePrevious.Text = "";
			//xhtml requirement for alt text - GAL-8522
			cmdMovePrevious.ImageUrl = mGalleryConfig.GetImageURL("s_previous.gif");
			cmdMovePrevious.ToolTip = Localization.GetString("MovePrevious", mGalleryConfig.SharedResourceFile);
			cmdMovePrevious.Visible = true;

			cmdMoveNext.ImageUrl = mGalleryConfig.GetImageURL("s_next.gif");
			cmdMovePrevious.Text = "";
			//xhtml requirement for alt text - GAL-8522
			cmdMoveNext.ToolTip = Localization.GetString("MoveNext", mGalleryConfig.SharedResourceFile);
			cmdMoveNext.Visible = true;


			if (!Page.IsPostBack && mGalleryConfig.IsValidPath) {
				if (!mFolder.IsPopulated) {
					Response.Redirect(ApplicationURL());
				}
				BindData();

				string rtnCtl = Request.QueryString["returnctl"];
				if (string.IsNullOrEmpty(rtnCtl) || !rtnCtl.EndsWith("FileEdit;")) {
					ReturnCtl = rtnCtl + "FileEdit;";
				} else {
					ReturnCtl = rtnCtl;
				}

				BindNavigation();
			}
		}

		private void cmdSave_Click(System.Object sender, System.EventArgs e)
		{
			string directory = mRequest.Folder.Path;
			PortalSecurity Security = new PortalSecurity();

			//William Severance - modified to add PortalSecurity input filter to prevent scripting hacks
			//  also added range validators of type = "Date" to CreatedDate and ApprovedDate text boxes.

			if (Page.IsValid) {
				string name = mFile.Name;
				string title = Security.InputFilter(txtTitle.Text, PortalSecurity.FilterFlag.NoScripting);
				string author = Security.InputFilter(txtAuthor.Text, PortalSecurity.FilterFlag.NoScripting);
				string location = Security.InputFilter(txtLocation.Text, PortalSecurity.FilterFlag.NoScripting);
				string client = Security.InputFilter(txtClient.Text, PortalSecurity.FilterFlag.NoScripting);
				string description = Security.InputFilter(txtDescription.Text, PortalSecurity.FilterFlag.NoScripting);
				string categories = Security.InputFilter(GetCategories(lstCategories), PortalSecurity.FilterFlag.NoScripting);
				DateTime createdDate = System.DateTime.Parse(txtCreatedDate.Text);
				DateTime approvedDate = Utils.TextToDate(txtApprovedDate.Text);
				//WES - convert empty string to DateTime.MaxValue
				//WES - corrected reversal of approvedDate and createdDate parameters
				GalleryXML.SaveMetaData(directory, name, mFile.ID, title, description, categories, author, location, client, mFile.OwnerID,
				createdDate, approvedDate, mFile.Score);

				if ((mFile.Parent != null)) {
					GalleryFolder parent = mFile.Parent;
					Config.ResetGalleryFolder(parent);
				} else {
					Config.ResetGalleryConfig(ModuleId);
				}
			}

			// Updated to use the new function in Utilities.vb by Quinn - 2/20/2009
			Response.Redirect(Utils.ReturnURL(TabId, ModuleId, Request));

		}

		private void cmdReturn_Click(System.Object sender, System.EventArgs e)
		{
			Response.Redirect(Utils.ReturnURL(TabId, ModuleId, Request));
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

			if (mGalleryConfig == null) {
				mGalleryConfig = Config.GetGalleryConfig(ModuleId);
			}

		}
		public FileEdit()
		{
			Init += Page_Init;
			Load += Page_Load;
		}

		#endregion

	}
}

