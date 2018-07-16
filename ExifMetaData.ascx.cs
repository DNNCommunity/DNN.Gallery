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
using static DotNetNuke.Modules.Gallery.Utils;
using static DotNetNuke.Modules.Gallery.Config;

namespace DotNetNuke.Modules.Gallery
{

	public abstract partial class ExifMetaData : DotNetNuke.Entities.Modules.PortalModuleBase
	{

		#region "Private Members"
			#endregion
		private DotNetNuke.Modules.Gallery.Config mGalleryConfig;

		#region "Public Properties"
		public DotNetNuke.Modules.Gallery.Config GalleryConfig {
			get { return mGalleryConfig; }
		}

		public string Title {
			get { return lblTitle.Text; }
			set { lblTitle.Text = value; }
		}

		public Unit ViewControlWidth {
			get { return new Unit(tblViewControl.Style["width"]); }
			set { tblViewControl.Style.Add("width", value.ToString()); }
		}

		#endregion

		#region "Event Handlers"

		private void Page_Load(System.Object sender, System.EventArgs e)
		{
			mGalleryConfig = Config.GetGalleryConfig(ModuleId);

			// Load the styles
			((CDefault)Page).AddStyleSheet(Common.Globals.CreateValidID(GalleryConfig.Css), GalleryConfig.Css);

			if (!Page.IsPostBack && (ViewState["UrlReferrer"] != null)) {
				ViewState["UrlReferrer"] = Request.UrlReferrer.ToString();
			}

		}

		private void btnBack_Click(System.Object sender, System.EventArgs e)
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
		}
		public ExifMetaData()
		{
			Init += Page_Init;
			Load += Page_Load;
		}

		#endregion

	}
}
