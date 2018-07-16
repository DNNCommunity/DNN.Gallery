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
using DotNetNuke.Security.Roles;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using static DotNetNuke.Common.Globals;
using static DotNetNuke.Modules.Gallery.Config;
using static DotNetNuke.Modules.Gallery.Utils;
using System.Web.Configuration;

namespace DotNetNuke.Modules.Gallery
{

	partial class Settings : DotNetNuke.Entities.Modules.PortalModuleBase
	{

		private PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();

		private DotNetNuke.Modules.Gallery.Config mGalleryConfig;
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

		#region "Private Methods"


		private void Page_Load(System.Object sender, System.EventArgs e)
		{
			// Load the styles
			((CDefault)Page).AddStyleSheet(Common.Globals.CreateValidID(GalleryConfig.Css), GalleryConfig.Css);

			//William Severance - added to handle changes of IsPrivate check box/MaxFileSize textbox clientside
			chkPrivate.Attributes.Add("onclick", "javascript:onPrivateChanged(this);");
			txtMaxFileSize.Attributes.Add("onchange", "javascript:onMaxFileSizeChanged(this);");

			if (!Page.IsPostBack) {
				// Ensure Gallery edit permissions (Andrew Galbraith Ryer)
				Authorization galleryAuthorization = new Authorization(ModuleConfiguration);
				if (!galleryAuthorization.HasEditPermission()) {
					Response.Redirect(Common.Globals.AccessDeniedURL(Localization.GetString("Insufficient_Maintenance_Permissions", mGalleryConfig.SharedResourceFile)));
				}

				rowAdmin.Visible = galleryAuthorization.HasAdminPermission();

				// GAL-9527. RootURL will now be relative to portal home directory not website root and modifiable only by administrators role
				lblHomeDirectory.Text = PortalSettings.HomeDirectory;
				RootURL.Text = mGalleryConfig.RootURL.Substring(PortalSettings.HomeDirectory.Length);

				//William Severance - Added primarily to test use of InvariantCulture to save/recall date strings from ModuleSettings
				txtCreatedDate.Text = mGalleryConfig.CreatedDate.ToShortDateString();
				txtGalleryTitle.Text = mGalleryConfig.GalleryTitle;
				txtDescription.Text = mGalleryConfig.GalleryDescription;
				lblImageExtensions.Text = mGalleryConfig.FileExtensions;
				lblMediaExtensions.Text = mGalleryConfig.MovieExtensions;
				txtCategoryValues.Text = mGalleryConfig.CategoryValues;
				txtStripWidth.Text = mGalleryConfig.StripWidth.ToString();
				txtStripHeight.Text = mGalleryConfig.StripHeight.ToString();
				txtMaxFileSize.Text = mGalleryConfig.MaxFileSize.ToString();
				txtQuota.Text = mGalleryConfig.Quota.ToString();
				txtMaxPendingUploadsSize.Text = mGalleryConfig.MaxPendingUploadsSize.ToString();

				txtMaxThumbWidth.Text = mGalleryConfig.MaximumThumbWidth.ToString();
				// GAL - 8216, disabling control if any album or file added already - Surjeet Gill 08.19.2008
				if (((mGalleryConfig.RootFolder != null)) && mGalleryConfig.RootFolder.List.Count > 0) {
					txtMaxThumbWidth.Enabled = false;
				}

				txtMaxThumbHeight.Text = mGalleryConfig.MaximumThumbHeight.ToString();
				// GAL - 8216, disabling control if any album or file added already - Surjeet Gill 08.19.2008
				if (((mGalleryConfig.RootFolder != null)) && mGalleryConfig.RootFolder.List.Count > 0) {
					txtMaxThumbHeight.Enabled = false;
				}

				chkBuildCacheOnStart.Checked = (mGalleryConfig.BuildCacheonStart == true);

				txtFixedWidth.Text = mGalleryConfig.FixedWidth.ToString();
				// GAL - 8216, disabling control if any album or file added already - Surjeet Gill 08.19.2008
				if (((mGalleryConfig.RootFolder != null)) && mGalleryConfig.RootFolder.List.Count > 0) {
					txtFixedWidth.Enabled = false;
				}

				txtFixedHeight.Text = mGalleryConfig.FixedHeight.ToString();
				// GAL - 8216, disabling control if any album or file added already - Surjeet Gill 08.19.2008
				if (((mGalleryConfig.RootFolder != null)) && mGalleryConfig.RootFolder.List.Count > 0) {
					txtFixedHeight.Enabled = false;
				}

				txtEncoderQuality.Text = mGalleryConfig.EncoderQuality.ToString();
				txtSlideshowSpeed.Text = mGalleryConfig.SlideshowSpeed.ToString();

				//William Severance - added to handle change of IsPrivate checkbox clientside
				if (mGalleryConfig.IsPrivate) {
					chkPrivate.Checked = true;
					rowOwner.Style.Add("display", "");
				} else {
					chkPrivate.Checked = false;
					rowOwner.Style.Add("display", "none");
				}

				chkMultiLevelMenu.Checked = mGalleryConfig.MultiLevelMenu;
				chkSlideshow.Checked = mGalleryConfig.AllowSlideshow;
				chkPopup.Checked = mGalleryConfig.AllowPopup;
				chkDownload.Checked = mGalleryConfig.AllowDownload;
				chkWatermark.Checked = mGalleryConfig.UseWatermark;
				chkAutoApproval.Checked = mGalleryConfig.AutoApproval;
				chkVoting.Checked = mGalleryConfig.AllowVoting;
				chkExif.Checked = mGalleryConfig.AllowExif;

				BindUserLookup();
				BindDisplayList();
				BindGalleryView();
				BindGallerySort();
				BindDownloadRoles();
				BindThemes();
				BindDefaultFolders();

			}

		}


		private void BindDisplayList()
		{
			//William Severance (9-1-2010) - Refactored to use bitmapped TextDisplayOptions rather than array of strings

			ListItem li = null;
			string key = null;
			Type galleryDisplayOptionType = typeof(Config.GalleryDisplayOption);
			Config.GalleryDisplayOption[] values = (Config.GalleryDisplayOption[])Enum.GetValues(galleryDisplayOptionType);

			lstDisplay.Items.Clear();
			foreach (Config.GalleryDisplayOption value in Enum.GetValues(galleryDisplayOptionType)) {
				key = value.ToString();
				li = new ListItem(Localization.GetString(key, mGalleryConfig.SharedResourceFile), key);
				li.Selected = ((mGalleryConfig.TextDisplayOptions & value) != 0);
				lstDisplay.Items.Add(li);
			}
		}

		private void BindGalleryView()
		{
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
			if ((_with1.Items.FindByValue(mGalleryConfig.DefaultView.ToString()) != null)) {
				_with1.Items.FindByValue(mGalleryConfig.DefaultView.ToString()).Selected = true;
			}
			this.chkChangeView.Checked = mGalleryConfig.AllowChangeView;

		}

		private void BindGallerySort()
		{
			System.Collections.Specialized.ListDictionary dict = new System.Collections.Specialized.ListDictionary();
			foreach (string name in Enum.GetNames(typeof(Config.GallerySort))) {
				dict.Add(name, Localization.GetString("Sort" + name, mGalleryConfig.SharedResourceFile));
			}

			lstSortProperties.ClearSelection();
			lstSortProperties.DataTextField = "value";
			lstSortProperties.DataValueField = "key";
			lstSortProperties.DataSource = dict;
			lstSortProperties.DataBind();

			ListItem item = null;
			foreach (ListItem item_loopVariable in lstSortProperties.Items) {
				item = item_loopVariable;
				if (mGalleryConfig.SortProperties.Contains(item.Value.ToString())) {
					item.Selected = true;
				}
			}

			var _with2 = ddlGallerySort;
			_with2.ClearSelection();
			_with2.DataTextField = "value";
			_with2.DataValueField = "key";
			_with2.DataSource = dict;
			_with2.DataBind();
			if ((_with2.Items.FindByValue(mGalleryConfig.DefaultSort.ToString()) != null)) {
				_with2.Items.FindByValue(mGalleryConfig.DefaultSort.ToString()).Selected = true;
			}

			chkDESC.Checked = mGalleryConfig.DefaultSortDESC;

		}

		//William Severance - modified to use shared method GetUser and to handle possibility of invalid OwnerId

		private void BindUserLookup()
		{
			var _with3 = ctlOwnerLookup;
			_with3.ItemClass = ObjectClass.DNNUser;
			_with3.Image = "sp_user.gif";
			_with3.LookupSingle = true;
			if (mGalleryConfig.OwnerID != -1) {
				UserInfo owner = new UserController().GetUser(PortalId, Utils.ValidUserID(mGalleryConfig.OwnerID));
				if ((owner != null))
					ctlOwnerLookup.AddItem(owner.UserID, owner.Username);
			}
		}

		private void BindDownloadRoles()
		{
			string role = null;

			var _with4 = ctlDownloadRoles;
			_with4.ItemClass = ObjectClass.DNNRole;
			_with4.Locations = "";
			_with4.Image = "sp_team.gif";
			foreach (string role_loopVariable in mGalleryConfig.DownloadRoles.Split(';')) {
				role = role_loopVariable;

				if (!(role.Length == 0)) {
					string RoleName = null;
					switch (role) {
						// JIMJ add all users role
						case Common.Globals.glbRoleAllUsers:
							RoleName = Common.Globals.glbRoleAllUsersName;

							break;
						default:
							RoleController ctlRole = new RoleController();
							RoleInfo objRole = ctlRole.GetRole(Int16.Parse(role), PortalId);

							RoleName = objRole.RoleName;
							break;
					}

					_with4.AddItem(Convert.ToInt32(role), RoleName);
				}
			}

		}

		private void BindThemes()
		{
			string[] themes = null;
			string Theme = null;
			string ThemesPath = System.IO.Path.Combine(Server.MapPath(TemplateSourceDirectory), "Themes");
			themes = Directory.GetDirectories(ThemesPath);

			ddlSkins.Items.Clear();
			foreach (string Theme_loopVariable in themes) {
				Theme = Theme_loopVariable;
				ListItem themeItem = new ListItem();
				var _with5 = themeItem;
				_with5.Text = System.IO.Path.GetFileName(Theme);
				_with5.Value = System.IO.Path.GetFullPath(Theme);
				ddlSkins.Items.Add(themeItem);
			}

			ddlSkins.Items.FindByText(mGalleryConfig.Theme).Selected = true;

		}

		private void BindDefaultFolders()
		{
			List<GalleryFolder> folders = Utils.GetChildFolders(mGalleryConfig.RootFolder, int.MaxValue);
			ddlDefaultAlbum.Items.Clear();
			ddlDefaultAlbum.Items.Add(new ListItem(string.Format(Localization.GetString("RootAlbum", LocalResourceFile), mGalleryConfig.GalleryTitle), ""));
			foreach (GalleryFolder folder in folders) {
				ddlDefaultAlbum.Items.Add(folder.GalleryHierarchy);
			}
			ListItem li = ddlDefaultAlbum.Items.FindByValue(mGalleryConfig.InitialFolder.GalleryHierarchy);
			if (li == null) {
				ddlDefaultAlbum.SelectedIndex = 0;
				//In case where previously set default album has been deleted
			} else {
				ddlDefaultAlbum.SelectedIndex = ddlDefaultAlbum.Items.IndexOf(li);
			}
		}


		private void cmdSave_Click(System.Object sender, System.EventArgs e)
		{
			// WES - Added to determine if DefaultView, AllowChangeView, DefaultSort, or DefaultSortDESC have changed so
			// that cookies of user making the change are refreshed.

			Config.GalleryView oldDefaultView = mGalleryConfig.DefaultView;
			//Utils.GetView(mGalleryConfig)
			bool oldAllowChangeView = mGalleryConfig.AllowChangeView;
			Config.GallerySort oldDefaultSort = mGalleryConfig.DefaultSort;
			//Utils.GetSort(mGalleryConfig)
			bool oldDefaultSortDESC = mGalleryConfig.DefaultSortDESC;
			//Utils.GetSortDESC(mGalleryConfig)

			//WES - Added server side Page.IsValid check of validators

			if (Page.IsValid) {
				PortalSecurity Security = new PortalSecurity();
				ModuleController ctlModule = new ModuleController();

				//<tamttt:note 2 required & hidden for integration >
				ctlModule.UpdateModuleSetting(ModuleId, "Provider", "");
				ctlModule.UpdateModuleSetting(ModuleId, "Type", "Gallery");
				//</tamttt:note>
				ctlModule.UpdateModuleSetting(ModuleId, "GalleryTitle", txtGalleryTitle.Text);
				ctlModule.UpdateModuleSetting(ModuleId, "GalleryDescription", txtDescription.Text);

				// WES: Modified to restrict RootURL to be portal home directory relative
				string homeDirectoryRelativeRootURL = Security.InputFilter(RootURL.Text, PortalSecurity.FilterFlag.NoMarkup);
				homeDirectoryRelativeRootURL = Regex.Replace(homeDirectoryRelativeRootURL, "\\.{2,}[\\\\/]{0,1}|[\\000-\\037:*?\"><|&]", "").Replace("\\", "/").Replace("//", "/");
				homeDirectoryRelativeRootURL = FileSystemUtils.FormatFolderPath(homeDirectoryRelativeRootURL.TrimStart('/'));
				RootURL.Text = homeDirectoryRelativeRootURL;
				ctlModule.UpdateModuleSetting(ModuleId, "RootURL", homeDirectoryRelativeRootURL);
				ctlModule.UpdateModuleSetting(ModuleId, "InitialGalleryHierarchy", ddlDefaultAlbum.SelectedValue);

				if (txtCreatedDate.Enabled) {
					DateTime createdDate = default(DateTime);
					try {
						createdDate = DateTime.Parse(txtCreatedDate.Text);
					} catch (Exception ex) {
						createdDate = DateTime.Now;
					}
					ctlModule.UpdateModuleSetting(ModuleId, "CreatedDate", createdDate.ToString(System.Globalization.CultureInfo.InvariantCulture));
				}
				ctlModule.UpdateModuleSetting(ModuleId, "Theme", ddlSkins.SelectedItem.Text);
				ctlModule.UpdateModuleSetting(ModuleId, "StripWidth", txtStripWidth.Text);
				ctlModule.UpdateModuleSetting(ModuleId, "StripHeight", txtStripHeight.Text);
				ctlModule.UpdateModuleSetting(ModuleId, "Quota", txtQuota.Text);
				ctlModule.UpdateModuleSetting(ModuleId, "MaxFileSize", txtMaxFileSize.Text);
				ctlModule.UpdateModuleSetting(ModuleId, "MaxPendingUploadsSize", txtMaxPendingUploadsSize.Text);
				ctlModule.UpdateModuleSetting(ModuleId, "MaxThumbWidth", txtMaxThumbWidth.Text);
				ctlModule.UpdateModuleSetting(ModuleId, "MaxThumbHeight", txtMaxThumbHeight.Text);
				ctlModule.UpdateModuleSetting(ModuleId, "CategoryValues", txtCategoryValues.Text);
				ctlModule.UpdateModuleSetting(ModuleId, "BuildCacheOnStart", chkBuildCacheOnStart.Checked.ToString());
				ctlModule.UpdateModuleSetting(ModuleId, "FixedWidth", txtFixedWidth.Text);
				ctlModule.UpdateModuleSetting(ModuleId, "FixedHeight", txtFixedHeight.Text);
				ctlModule.UpdateModuleSetting(ModuleId, "EncoderQuality", txtEncoderQuality.Text);
				ctlModule.UpdateModuleSetting(ModuleId, "SlideshowSpeed", txtSlideshowSpeed.Text);
				ctlModule.UpdateModuleSetting(ModuleId, "MultiLevelMenu", chkMultiLevelMenu.Checked.ToString());
				ctlModule.UpdateModuleSetting(ModuleId, "AllowSlideshow", chkSlideshow.Checked.ToString());
				ctlModule.UpdateModuleSetting(ModuleId, "AllowPopup", chkPopup.Checked.ToString());
				ctlModule.UpdateModuleSetting(ModuleId, "AllowDownload", chkDownload.Checked.ToString());
				ctlModule.UpdateModuleSetting(ModuleId, "AllowVoting", chkVoting.Checked.ToString());
				ctlModule.UpdateModuleSetting(ModuleId, "AllowExif", chkExif.Checked.ToString());
				ctlModule.UpdateModuleSetting(ModuleId, "UseWatermark", chkWatermark.Checked.ToString());
				ctlModule.UpdateModuleSetting(ModuleId, "AutoApproval", chkAutoApproval.Checked.ToString());

				//William Severance - modified to handle none specified OwnerID
				string OwnerID = ctlOwnerLookup.ResultItems.Replace(";", "");
				if (chkPrivate.Checked) {
					if (string.IsNullOrEmpty(OwnerID))
						OwnerID = this.UserId.ToString();
				} else {
					if (!string.IsNullOrEmpty(OwnerID))
						OwnerID = Config.DefaultOwnerId.ToString();
				}

				ctlModule.UpdateModuleSetting(ModuleId, "OwnerID", OwnerID);
				ctlModule.UpdateModuleSetting(ModuleId, "IsPrivate", chkPrivate.Checked.ToString());
				ctlModule.UpdateModuleSetting(ModuleId, "DefaultSortDESC", chkDESC.Checked.ToString());
				ctlModule.UpdateModuleSetting(ModuleId, "DefaultSort", ddlGallerySort.SelectedItem.Value.ToString());
				ctlModule.UpdateModuleSetting(ModuleId, "DefaultView", ddlGalleryView.SelectedItem.Value.ToString());
				ctlModule.UpdateModuleSetting(ModuleId, "AllowChangeView", chkChangeView.Checked.ToString());

				string textDisplay = "";
				string textDownloadRoles = "";
				string textSortProperties = "";
				ListItem item = null;

				//Update list display info list
				foreach (ListItem item_loopVariable in lstDisplay.Items) {
					item = item_loopVariable;
					if (item.Selected) {
						textDisplay += item.Value + ";";
					}
				}
				if (Strings.Len(textDisplay) > 0) {
					textDisplay = textDisplay.TrimEnd(';');
				}

				textDownloadRoles = ctlDownloadRoles.ResultItems;

				// Update sort properties list
				foreach (ListItem item_loopVariable in lstSortProperties.Items) {
					item = item_loopVariable;
					if (item.Selected) {
						textSortProperties += item.Value + ";";
					}
				}
				if (Strings.Len(textSortProperties) > 0) {
					textSortProperties = textSortProperties.TrimEnd(';');
				}

				ctlModule.UpdateModuleSetting(ModuleId, "TextDisplayValues", textDisplay);
				ctlModule.UpdateModuleSetting(ModuleId, "DownloadRoles", textDownloadRoles);
				ctlModule.UpdateModuleSetting(ModuleId, "SortPropertyValues", textSortProperties);

				//William Severance - Added below line to expire module cache before call to ResetGalleryConfig
				ModuleController.SynchronizeModule(ModuleId);
				mGalleryConfig = Config.ResetGalleryConfig(ModuleId);
				//Reload changed configuration

				//William Severance - Added to set folder read permissions based on DownloadRoles if set or module view
				//permissions if not inheriting from page or page permissions if inheriting.
				//Properly set folder read permissions are necessary now that we are using
				//core LinkClick.aspx for downloads.

				DotNetNuke.Modules.Gallery.Utils.SyncFolderPermissions(textDownloadRoles, ModuleId, TabId);

				//William Severance - Added refresh of logged in user's cookies when change in DefaultView, DefaultSort, DefaultSortDESC
				if (((oldAllowChangeView != mGalleryConfig.AllowChangeView) && mGalleryConfig.AllowChangeView) || (oldDefaultView != mGalleryConfig.DefaultView)) {
					Utils.RefreshCookie(Utils.GALLERY_VIEW, ModuleId, mGalleryConfig.DefaultView);
				}
				if (oldDefaultSort != mGalleryConfig.DefaultSort)
					Utils.RefreshCookie(Utils.GALLERY_SORT, ModuleId, mGalleryConfig.DefaultSort);
				if (oldDefaultSortDESC != mGalleryConfig.DefaultSortDESC)
					Utils.RefreshCookie(Utils.GALLERY_SORT_DESC, ModuleId, mGalleryConfig.DefaultSortDESC);

				// Redirect back to the gallery home page
				GoBack();
			}
		}

		private void cmdReturn_Click(System.Object sender, System.EventArgs e)
		{
			GoBack();
		}

		private void GoBack()
		{
			Response.Redirect(Common.Globals.NavigateURL());
		}

		#endregion

		#region "Public Properties"

		public DotNetNuke.Modules.Gallery.Config GalleryConfig {
			get { return mGalleryConfig; }
		}

		#endregion

		//William Severance - Added javascript to provide hide/unhide of rowOwner on change if Is Private check box

		private void Page_PreRender(object sender, System.EventArgs e)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("function onPrivateChanged(chkPrivate) {");
			sb.AppendLine("    if (document.getElementById) {");
			sb.Append("        var rowOwner = document.getElementById('");
			sb.Append(rowOwner.ClientID);
			sb.AppendLine("');");
			sb.AppendLine("    } else {");
			sb.Append("         var rowOwner = document.all['");
			sb.Append(rowOwner.ClientID);
			sb.AppendLine("'];");
			sb.AppendLine("    }");
			sb.AppendLine("    if ((chkPrivate != null) && (rowOwner != null)) {");
			sb.AppendLine("        if (chkPrivate.checked) {");
			sb.AppendLine("              rowOwner.style.display='block';");
			sb.AppendLine("        } else {");
			sb.AppendLine("              rowOwner.style.display='none';");
			sb.AppendLine("        }");
			sb.AppendLine("    }");
			sb.AppendLine("}");

			if (!Page.ClientScript.IsClientScriptBlockRegistered(this.GetType(), "DNN_Gallery_Settings_IsPrivate")) {
				Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "DNN_Gallery_Settings_IsPrivate", sb.ToString(), true);
			}

			//William Severance - Added code/javascript to provide warning if the Max File Size
			//is set to a value larger than the lesser of the maxRequestLength and RequestLengthDiskThreshold
			//attributes set in the httpRuntime section of the web.config.

			object objSection = null;

			try {
				objSection = DotNetNuke.Common.Utilities.Config.GetSection("system.web/httpRuntime");
			} catch (Exception ex) {
				//LogException(ex) 'Do not log error if system.web/httpRuntime section not available.
			}

			string helpMsg = Localization.GetString("MaxFileSize.Help", LocalResourceFile);

			if (objSection != null && objSection is HttpRuntimeSection) {
				int maxRequestLength = 0;
				int requestLengthDiskThreshold = 0;
				double executionTimeout = 0;
				var _with6 = (HttpRuntimeSection)objSection;
				maxRequestLength = _with6.MaxRequestLength;
				requestLengthDiskThreshold = _with6.RequestLengthDiskThreshold;
				executionTimeout = _with6.ExecutionTimeout.TotalSeconds;
				int maxFileSizeConstraint = Math.Min(maxRequestLength, requestLengthDiskThreshold);
				helpMsg += string.Format(Localization.GetString("MaxFileSizeLimits.Text", LocalResourceFile), maxRequestLength, requestLengthDiskThreshold, executionTimeout);
				lblMaxFileSizeWarning.Style["display"] = int.Parse(txtMaxFileSize.Text) > maxFileSizeConstraint ? "block" : "none";

				sb.Length = 0;
				sb.AppendLine("function onMaxFileSizeChanged(txtMaxFileSize) {");
				sb.AppendLine("   if (document.getElementById) {");
				sb.Append("     var lblMaxFileSizeWarning = document.getElementById ('");
				sb.Append(lblMaxFileSizeWarning.ClientID);
				sb.AppendLine("');");
				sb.AppendLine("   } else {");
				sb.Append("     var lblMaxFileSizeWarning=document.all['");
				sb.Append(lblMaxFileSizeWarning.ClientID);
				sb.AppendLine("'];");
				sb.AppendLine("   }");
				sb.AppendLine("   if (txtMaxFileSize != null && lblMaxFileSizeWarning != null) {");
				sb.Append("        lblMaxFileSizeWarning.style.display = (txtMaxFileSize.value > ");
				sb.Append(maxFileSizeConstraint.ToString());
				sb.AppendLine(" ? 'block' : 'none');");
				sb.AppendLine("   }");
				sb.AppendLine("}");

				if (!Page.ClientScript.IsClientScriptBlockRegistered(this.GetType(), "DNN_Gallery_Settings_MaxFileSizeWarning")) {
					Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "DNN_Gallery_Settings_MaxFileSizeWarning", sb.ToString(), true);
				}
			} else {
				lblMaxFileSizeWarning.Style["display"] = "none";
			}

			plMaxFileSize.HelpText = helpMsg;

		}

	}

}


