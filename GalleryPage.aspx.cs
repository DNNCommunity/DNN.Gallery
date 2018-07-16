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

using DotNetNuke.Entities.Users;

namespace DotNetNuke.Modules.Gallery
{

	// Class from which all gallery pop-up pages (Viewer.aspx, Slideshow.aspx, FlashPlayer.aspx,
	// MediaPlayer.aspx, ExifMetaData.aspx inherit.

	public partial class GalleryPageBase : DotNetNuke.Framework.PageBase
	{

		private DotNetNuke.Modules.Gallery.Config _GalleryConfig;
		private DotNetNuke.Modules.Gallery.Authorization _GalleryAuthorization;
		private DotNetNuke.Modules.Gallery.BaseRequest _CurrentRequest;
		private int _TabId = -1;
		private int _ModuleId = -1;
		private string _Ctl = "";

		private string _localResourceFile = "";
		public Gallery.Config GalleryConfig {
			get {
				if (_GalleryConfig == null) {
					_GalleryConfig = Config.GetGalleryConfig(ModuleId);
				}
				return _GalleryConfig;
			}
		}

		public Gallery.Authorization GalleryAuthorization {
			get {
				if (_GalleryAuthorization == null) {
					_GalleryAuthorization = new Gallery.Authorization(ModuleId);
				}
				return _GalleryAuthorization;
			}
		}

		public BaseRequest CurrentRequest {
			get { return _CurrentRequest; }
			set { _CurrentRequest = value; }
		}

		public int PortalId {
			get { return PortalSettings.PortalId; }
		}

		public string Ctl {
			get { return _Ctl; }
		}

		public int TabId {
			get { return _TabId; }
		}

		public int ModuleId {
			get { return _ModuleId; }
		}

		public UserInfo UserInfo {
			get { return UserController.GetCurrentUserInfo(); }
		}

		public int UserId {
			get {
				int functionReturnValue = 0;
				if (HttpContext.Current.Request.IsAuthenticated) {
					functionReturnValue = UserInfo.UserID;
				} else {
					functionReturnValue = -1;
				}
				return functionReturnValue;
			}
		}

		public new string LocalResourceFile {
			get {
				if (string.IsNullOrEmpty(_localResourceFile)) {
					//Hack to adjust for legacy use of 'exif' rather than 'exifmetadata' as ControlKey for
					//ExifMetaData.ascx in module definitions
					string ctlName = null;
					if (_Ctl.ToLower() == "exif") {
						ctlName = "ExifMetaData";
					} else {
						ctlName = _Ctl;
					}
					_localResourceFile = this.TemplateSourceDirectory + "/" + DotNetNuke.Services.Localization.Localization.LocalResourceDirectory + "/" + ctlName + ".ascx.resx";
				}
				return _localResourceFile;
			}
			set { _localResourceFile = value; }
		}

		public string ControlTitle {
			get { return lblTitle.Text; }
			set { lblTitle.Text = value; }
		}

		public void AddStyleSheet(string id, string href, bool isFirst)
		{
			//Find the placeholder control
			Control objCSS = this.Header.FindControl("CSS");

			if ((objCSS != null)) {
				//First see if we have already added the <LINK> control
				Control objCtrl = Page.Header.FindControl(id);

				if (objCtrl == null) {
					HtmlLink objLink = new HtmlLink();
					objLink.ID = id;
					objLink.Attributes["rel"] = "stylesheet";
					objLink.Attributes["type"] = "text/css";
					objLink.Href = href;

					if (isFirst) {
						//Find the first HtmlLink
						int iLink = 0;
						for (iLink = 0; iLink <= objCSS.Controls.Count - 1; iLink++) {
							if (objCSS.Controls[iLink] is HtmlLink) {
								break; // TODO: might not be correct. Was : Exit For
							}
						}
						objCSS.Controls.AddAt(iLink, objLink);
					} else {
						objCSS.Controls.Add(objLink);
					}
				}
			}
		}

		private void Page_Load(object sender, System.EventArgs e)
		{
			if ((Request.QueryString["mid"] != null)) {
				_ModuleId = int.Parse(Request.QueryString["mid"]);
			}
			if ((Request.QueryString["tabid"] != null)) {
				_TabId = int.Parse(Request.QueryString["tabid"]);
				if (_TabId != PortalSettings.ActiveTab.TabID) {
					Response.Redirect(Common.Globals.AccessDeniedURL(), true);
				}
			} else {
				_TabId = PortalSettings.ActiveTab.TabID;
			}

			Entities.Modules.ModuleController modController = new Entities.Modules.ModuleController();
			System.Collections.Generic.Dictionary<int, Entities.Modules.ModuleInfo> modDictionary = null;
			modDictionary = modController.GetTabModules(_TabId);
			if (!modDictionary.ContainsKey(ModuleId) || !(modDictionary[_ModuleId].DesktopModule.ModuleName == "DNN_Gallery")) {
				Response.Redirect(Common.Globals.AccessDeniedURL(), true);
			}

			if ((Request.QueryString["ctl"] != null)) {
				_Ctl = Request.QueryString["ctl"];
			}

			if (GalleryAuthorization.HasViewPermission()) {
				AddStyleSheet(GalleryConfig.Theme + "_stylesheet", GalleryConfig.Css, true);
				string ctlFileName = "";
				switch (_Ctl.ToLower()) {
					case "viewer":
						ctlFileName = "ControlViewer.ascx";
						break;
					case "slideshow":
						ctlFileName = "ControlSlideshow.ascx";
						break;
					case "flashplayer":
						ctlFileName = "ControlFlashPlayer.ascx";
						break;
					case "mediaplayer":
						ctlFileName = "ControlMediaPlayer.ascx";
						break;
					case "exif":
						ctlFileName = "ControlExif.ascx";
						break;
					default:
						Response.Redirect(Common.Globals.AccessDeniedURL(), true);
						break;
				}
				System.Web.UI.Control ctlToLoad = LoadControl("Controls/" + ctlFileName);
				if ((ctlToLoad != null)) {
					ctlToLoad.ID = "ctl" + _Ctl;
					phControl.Controls.Add(ctlToLoad);
					if (_Ctl.ToLower() == "viewer" && GalleryAuthorization.HasEditPermission()) {
						Title = Localization.GetString("ControlTitle_editor", LocalResourceFile);
					} else {
						Title = Localization.GetString("ControlTitle_" + _Ctl.ToLower(), LocalResourceFile);
					}
					btnClose.Text = Localization.GetString("btnClose", GalleryConfig.SharedResourceFile);
				}
			} else {
				Response.Redirect(Common.Globals.AccessDeniedURL(), true);
			}
		}
		public GalleryPageBase()
		{
			Load += Page_Load;
		}
	}
}
