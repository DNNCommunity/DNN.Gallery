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
using System.Xml;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using static DotNetNuke.Common.Globals;
using DotNetNuke.Entities.Users;
using static DotNetNuke.Modules.Gallery.Utils;
using static DotNetNuke.Modules.Gallery.Config;

namespace DotNetNuke.Modules.Gallery
{

	public abstract partial class Slideshow : DotNetNuke.Entities.Modules.PortalModuleBase, DotNetNuke.Entities.Modules.IActionable
	{


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


		private void Page_Load(System.Object sender, System.EventArgs e)
		{
			// Load the styles
			((CDefault)Page).AddStyleSheet(Common.Globals.CreateValidID(GalleryConfig.Css), GalleryConfig.Css);

			if (!Page.IsPostBack && (ViewState["UrlReferrer"] != null)) {
				ViewState["UrlReferrer"] = Request.UrlReferrer.ToString();
			}

			Title = "Test Image Title";
		}

		private void btnBack_Click(object sender, EventArgs e)
		{
			string url = null;
			if ((ViewState["UrlReferrer"] != null)) {
				url = ViewState["UrlReferrer"].ToString();
				//Request.Url.ToString.Replace("&ctl=Viewer", "&ctl=FileEdit")
			} else {
				url = Utils.GetURL(Page.Request.ServerVariables["URL"], Page, "", "ctl=&mode=&mid=&currentitem=&media=");
				//Request.Url.ToString.Replace("&ctl=Viewer", "")
			}
			Response.Redirect(url);
		}

		public DotNetNuke.Modules.Gallery.Config GalleryConfig {
			get { return mGalleryConfig; }
		}

		public string Title {
			get { return lblTitle.Text; }
			set { lblTitle.Text = value; }
		}

		private void Page_PreRender(object sender, System.EventArgs e)
		{
			int gutterWidth = 60;
			if (GalleryConfig.Theme != "DNN Simple")
				gutterWidth += 20;
			string width = (GalleryConfig.FixedWidth + gutterWidth).ToString() + "px";
			tblViewControl.Style.Add("width", width);
		}
		public Slideshow()
		{
			PreRender += Page_PreRender;
			Load += Page_Load;
			Init += Page_Init;
		}

	}
}


