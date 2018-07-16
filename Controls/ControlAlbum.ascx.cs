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
// Added for Localization by M. Schlomann
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Modules.Gallery.WebControls
{

	//Inherits System.Web.UI.UserControl
	public abstract partial class Album : Entities.Modules.PortalModuleBase
	{

		private PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
		private string strFolderInfo;
		private DotNetNuke.Modules.Gallery.Config mGalleryConfig;
		private GalleryFolder mAlbumObject;
		private DotNetNuke.Modules.Gallery.Authorization mGalleryAuthorize;
		//Private mModuleID As Integer
			// Needed for Localization.  But I'm not sure if it is needed longer, but then have some resx data moved to the SharedResource file.
		private string MyFileName = "ControlAlbum.ascx";

		private DotNetNuke.Modules.Gallery.AlbumEdit mAlbumEdit;
		private string albumDeleteConfirmationText;

		private string fileDeleteConfirmationText;
		#region "Controls"
		protected System.Web.UI.WebControls.TextBox txtName;
		protected System.Web.UI.WebControls.TextBox txtTitle;
		protected System.Web.UI.WebControls.TextBox txtAuthor;
		protected System.Web.UI.WebControls.TextBox txtClient;
		protected System.Web.UI.WebControls.TextBox txtLocation;
		protected System.Web.UI.WebControls.TextBox txtApprovedDate;
		protected System.Web.UI.WebControls.TextBox txtDescription;
		protected System.Web.UI.WebControls.CheckBoxList lstCategories;
			#endregion
		protected System.Web.UI.WebControls.ImageButton btnFolderSave;

		#region "Private Methods"
		public void BindData()
		{
			if (mAlbumObject.List.Count > 0) {
				albumDeleteConfirmationText = Localization.GetString("AlbumDeleteConfirmation", LocalResourceFile);
				fileDeleteConfirmationText = Localization.GetString("FileDeleteConfirmation", LocalResourceFile);
				rowContent.Visible = true;
				grdContent.DataSource = mAlbumObject.List;
				grdContent.DataBind();
			} else {
				rowContent.Visible = false;
			}
			//Dim myDate As DateTime = DateTime.Today
		}

		protected bool CanEdit(object DataItem)
		{
			return mGalleryAuthorize.HasItemEditPermission(DataItem);
		}

		#endregion

		#region "Event Handlers"
		private void Page_Load(System.Object sender, System.EventArgs e)
		{
			// Added Localization for DataGrid colums by M. Schlomann
			Localization.LocalizeDataGrid(ref grdContent, LocalResourceFile);
			var _with1 = mAlbumEdit;
			mAlbumObject = _with1.GalleryAlbum;
			mGalleryAuthorize = _with1.GalleryAuthorize;
			//New Authorization(mModuleID)
			mGalleryConfig = _with1.GalleryConfig;
			BindData();

		}

		private void grdContent_ItemCommand(object source, System.Web.UI.WebControls.DataGridCommandEventArgs e)
		{
			int itemIndex = Int16.Parse((((ImageButton)e.CommandSource).CommandArgument));
			IGalleryObjectInfo selItem = (IGalleryObjectInfo)mAlbumObject.List.Item(itemIndex);

			switch ((((ImageButton)e.CommandSource).CommandName).ToLower()) {
				case "delete":
					try {
						mAlbumObject.DeleteChild(selItem);
						BindData();
					} catch (Exception ex) {
						Config.ResetGalleryConfig(ModuleId);
						//Config.ResetGalleryConfig(mModuleID)
					}

					break;
				case "edit":
					System.Collections.Generic.List<string> @params = new System.Collections.Generic.List<string>();
					if (!string.IsNullOrEmpty(Request.QueryString["currentstrip"])) {
						@params.Add("currentstrip=" + Request.QueryString["currentstrip"]);
					}
					@params.Add("returnctl=" + mAlbumEdit.ReturnCtl);
					string url = Utils.AppendURLParameters(selItem.EditURL, @params.ToArray());

					if (url.Length > 0) {
						Response.Redirect(url);
					} else {
						//lblInfo.Text = "An error occur while trying open this item for editing"
					}

					break;
			}
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
			mAlbumEdit = (DotNetNuke.Modules.Gallery.AlbumEdit)this.NamingContainer;
			ModuleConfiguration = mAlbumEdit.ModuleConfiguration;
			LocalResourceFile = Localization.GetResourceFile(this, MyFileName);
		}

		#endregion

		private void grdContent_ItemDataBound(object sender, System.Web.UI.WebControls.DataGridItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item | e.Item.ItemType == ListItemType.AlternatingItem) {
				WebControl btnDelete = (WebControl)e.Item.FindControl("btnDelete");
				if (btnDelete != null) {
					IGalleryObjectInfo galleryObject = (IGalleryObjectInfo)e.Item.DataItem;
					if (galleryObject.IsFolder) {
						DotNetNuke.UI.Utilities.ClientAPI.AddButtonConfirm(btnDelete, string.Format(albumDeleteConfirmationText, galleryObject.Name));
					} else {
						DotNetNuke.UI.Utilities.ClientAPI.AddButtonConfirm(btnDelete, string.Format(fileDeleteConfirmationText, galleryObject.Name));
					}
				}
			}
		}
		public Album()
		{
			Init += Page_Init;
			Load += Page_Load;
		}
	}
}

