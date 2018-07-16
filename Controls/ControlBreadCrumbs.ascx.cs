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
using static DotNetNuke.Common.Globals;

namespace DotNetNuke.Modules.Gallery.WebControls
{

	public abstract partial class BreadCrumbs : System.Web.UI.UserControl
	{

		private BaseRequest mGalleryRequest;
		private int mModuleID;

		private DotNetNuke.Modules.Gallery.Config mGalleryConfig = null;
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

		// WES: Refactored to fix breadcrumb separator image not displaying and replace DataList control
		// with lighter weight Repeater control and remove outer table rendering.


		private void Page_Load(System.Object sender, System.EventArgs e)
		{
			if (!IsPostBack) {
				BindBreadcrumbs();
			}
		}

		private void BindBreadcrumbs()
		{
			if (GalleryConfig.IsValidPath) {
				var _with1 = rptFolders;
				_with1.SeparatorTemplate = new CompiledTemplateBuilder(new BuildTemplateMethod(BuildSeparator));
				_with1.DataSource = GalleryRequest.FolderPaths();
				_with1.DataBind();
			} else {
				Visible = false;
			}
		}

		private void BuildSeparator(Control container)
		{
			//container.Controls.Add(New LiteralControl("<img alt='' src='" & Page.ResolveUrl(GalleryConfig.GetImageURL("breadcrumb.gif")) & "' class='Gallery_BreadcrumbSeparator' />"))
			container.Controls.Add(new LiteralControl("<span class='Gallery_BreadcrumbSeparator'>&nbsp;</span>"));
		}

		public void Enable(bool Enabled)
		{
			IsEnabled = Enabled;
			BindBreadcrumbs();
		}

		protected bool IsEnabled {
			get {
				if (ViewState["IsEnabled"] == null) {
					return true;
				} else {
					return Convert.ToBoolean(ViewState["IsEnabled"]);
				}
			}
			set { ViewState["IsEnabled"] = value; }
		}

		public int ModuleID {
			get { return mModuleID; }
			set { mModuleID = value; }
		}

		public Gallery.Config GalleryConfig {
			get {
				if (mGalleryConfig == null) {
					mGalleryConfig = Config.GetGalleryConfig(ModuleID);
				}
				return mGalleryConfig;
			}
		}

		public BaseRequest GalleryRequest {
			get { return mGalleryRequest; }
			set { mGalleryRequest = value; }
		}
	}
}

