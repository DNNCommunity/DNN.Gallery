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

using DotNetNuke.Entities.Users;

namespace DotNetNuke.Modules.Gallery.WebControls
{
	public class GalleryWebControlBase : DotNetNuke.Framework.UserControlBase
	{

		private DotNetNuke.Modules.Gallery.Config _GalleryConfig;
		private DotNetNuke.Modules.Gallery.Authorization _GalleryAuthorization;
		private int _TabId = -1;
		private int _ModuleId = -1;

		private string _localResourceFile = "";

		public string PageTitle;
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

		public int PortalId {
			get { return PortalSettings.PortalId; }
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

		public string LocalResourceFile {
			get {
				if (string.IsNullOrEmpty(_localResourceFile)) {
					_localResourceFile = ControlPath + DotNetNuke.Services.Localization.Localization.LocalResourceDirectory + "/" + ControlName;
				}
				return _localResourceFile;
			}
			set { _localResourceFile = value; }
		}

		public string ControlPath {
			get { return this.TemplateSourceDirectory + "/"; }
		}

		public string ControlName {
			get { return this.AppRelativeVirtualPath.Substring(this.AppRelativeTemplateSourceDirectory.Length); }
		}

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

			if ((Request.QueryString["mid"] != null)) {
				_ModuleId = int.Parse(Request.QueryString["mid"]);
			}

			if ((Request.QueryString["tabid"] != null)) {
				_TabId = int.Parse(Request.QueryString["tabid"]);
			} else {
				_TabId = PortalSettings.ActiveTab.TabID;
			}

			if (!GalleryAuthorization.HasViewPermission()) {
				Response.Redirect(Common.Globals.AccessDeniedURL(), true);
			}
		}
		public GalleryWebControlBase()
		{
			Init += Page_Init;
		}

		#endregion

	}
}
