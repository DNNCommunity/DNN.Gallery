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

using static System.Web.HttpUtility;
using static DotNetNuke.Modules.Gallery.Config;
using static DotNetNuke.Modules.Gallery.Utils;

namespace DotNetNuke.Modules.Gallery.Views
{

	/// <summary>
	/// GUI code inserted into the aspx page that is common across all views
	/// </summary>
	/// <remarks></remarks>
	public class GalleryControl : System.Web.UI.WebControls.WebControl, INamingContainer
	{

		private bool mInitialised;
		private BaseView mBaseView;
		private DotNetNuke.Modules.Gallery.Config mGalleryConfig;
		private DotNetNuke.Modules.Gallery.Authorization mGalleryAuthorization;

		private GalleryUserRequest mUserRequest;
		public GalleryControl()
		{
			if ((HttpContext.Current != null)) {
				if ((HttpContext.Current.Request.QueryString["view"] != null)) {
					View = (GalleryView)Enum.Parse(typeof(GalleryView), HttpContext.Current.Request.QueryString["view"], true);
				} else {
					View = GalleryView.Standard;
				}
			}
		}
		//New

		protected virtual void Initialise()
		{
			mInitialised = true;
		}
		//Initialise

		protected override void OnLoad(System.EventArgs e)
		{
			if (View == GalleryView.ListView) {
				DotNetNuke.Framework.jQuery.RequestRegistration();
			}
			Initialise();
		}
		//OnLoad

		protected override void EnsureChildControls()
		{
			if (!mInitialised) {
				Initialise();
			}
			base.EnsureChildControls();
		}
		//EnsureChildControls

		protected virtual void CreateObject()
		{
			if (GalleryConfig.IsValidPath) {
				switch (View) {
					case GalleryView.Standard:
						// Show the standard view
						mBaseView = new Views.StandardView(this);

						break;
					case GalleryView.ListView:
						// Show the thumbs as a list
						mBaseView = new Views.ListView(this);

						break;
					case GalleryView.CardView:
						// Show thumbs as a card
						mBaseView = new Views.CardView(this);
						break;
				}
			} else {
				mBaseView = new Views.NoConfigView(this);
			}
		}

		protected override void CreateChildControls()
		{
			CreateObject();
			//AndAlso (Not Page.IsPostBack) Then
			if ((mBaseView != null)) {
				mBaseView.CreateChildControls();
			}
		}
		//CreateChildControls

		protected override void OnPreRender(System.EventArgs e)
		{
			if ((mBaseView != null)) {
				mBaseView.OnPreRender();
			}
		}
		//OnPreRender

		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{
			if ((mBaseView != null)) {
				mBaseView.Render(writer);
			}
		}
		//Render 

		public int PortalID {
			get {
				object savedState = ViewState["GalleryPortalID"];
				if (savedState == null) {
					return 0;
				} else {
					return Convert.ToInt32(savedState);
				}
			}
			set { ViewState["GalleryPortalID"] = value; }
		}

		public int TabID {
			get {
				object savedState = ViewState["GalleryTabID"];
				if (savedState == null) {
					return 0;
				} else {
					return Convert.ToInt32(savedState);
				}
			}
			set { ViewState["GalleryTabID"] = value; }
		}

		public int ModuleId {
			get {
				object savedState = ViewState["GalleryModuleID"];
				if (savedState == null) {
					return 0;
				} else {
					return Convert.ToInt32(savedState);
				}
			}
			set { ViewState["GalleryModuleID"] = value; }
		}

		public DotNetNuke.Modules.Gallery.Config GalleryConfig {
			get {
				if (mGalleryConfig == null) {
					mGalleryConfig = DotNetNuke.Modules.Gallery.Config.GetGalleryConfig(ModuleId);
				}
				return mGalleryConfig;
			}
			set { mGalleryConfig = value; }
		}


		public DotNetNuke.Modules.Gallery.Authorization GalleryAuthorize {
			get {
				if (mGalleryAuthorization == null) {
					mGalleryAuthorization = new DotNetNuke.Modules.Gallery.Authorization(ModuleId);
				}
				return mGalleryAuthorization;
			}
			set { mGalleryAuthorization = value; }
		}

		public GalleryUserRequest UserRequest {
			get {
				if (mUserRequest == null) {
					mUserRequest = new GalleryUserRequest(ModuleId, Sort, SortDESC);
				}
				return mUserRequest;
			}
			set { mUserRequest = value; }
		}

		public Config.GallerySort Sort {
			get {
				object savedState = ViewState["Sort"];
				if (savedState == null) {
					return GallerySort.Title;
				} else {
					return (Config.GallerySort)savedState;
				}
			}
			set { ViewState["Sort"] = value; }
		}

		public Config.GalleryView View {
			get {
				object savedState = ViewState["View"];
				if (savedState == null) {
					return GalleryView.Standard;
				} else {
					return (Config.GalleryView)savedState;
				}
			}
			set { ViewState["View"] = value; }
		}

		public bool SortDESC {
			get {
				object savedState = ViewState["SortDESC"];
				if (savedState == null) {
					return false;
				} else {
					return Convert.ToBoolean(savedState);
				}
			}
			set { ViewState["SortDESC"] = value; }
		}

		public string LocalResourceFile {
			get {
				object savedState = ViewState["GalleryResourceFile"];
				if (savedState == null) {
					ViewState["GalleryResourceFile"] = TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/Container.ascx";
				}
				return Convert.ToString(ViewState["GalleryResourceFile"]);
			}
			set { ViewState["GalleryResourceFile"] = value; }
		}

		public string LocalizedText(string key)
		{
			return Localization.GetString(key, LocalResourceFile);
		}

	}

}
