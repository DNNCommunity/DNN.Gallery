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



using DotNetNuke.Entities.Portals;
using DotNetNuke.Modules.Gallery;
using static DotNetNuke.Modules.Gallery.Utils;

namespace DotNetNuke.Modules.Gallery.WebControls
{

	public abstract partial class Pager : System.Web.UI.UserControl
	{

		private GalleryUserRequest mUserRequest;
		private ArrayList mCurrentItems = new ArrayList();

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
		}

		#endregion

		private void Page_Load(System.Object sender, System.EventArgs e)
		{
			try {
				DotNetNuke.Modules.Gallery.Container mContainer = (DotNetNuke.Modules.Gallery.Container)this.NamingContainer;
				mGalleryConfig = mContainer.GalleryConfig;

				if (mGalleryConfig.IsValidPath) {
					mUserRequest = mContainer.UserRequest;
					mCurrentItems = mUserRequest.CurrentItems();

					if ((!Page.IsPostBack) && (mCurrentItems.Count > 0)) {

						if (mCurrentItems.Count > 0) {
							EnableControl(true);
							dlPager.DataSource = mUserRequest.PagerItems();

							if (mUserRequest.CurrentStrip > 1) {
								PagerDetail item = null;
								int i = 0;
								foreach (PagerDetail item_loopVariable in mUserRequest.PagerItems()) {
									item = item_loopVariable;
									if (item.Strip == mUserRequest.CurrentStrip) {
										dlPager.SelectedIndex = i;
										//mUserRequest.CurrentStrip
										break; // TODO: might not be correct. Was : Exit For
									}
									i += 1;
								}

							} else {
								dlPager.SelectedIndex = 0;
							}

							dlPager.DataBind();
							lblPageInfo.Text = Localization.GetString("Page", mGalleryConfig.SharedResourceFile);
						} else {
							// some cases when admin delete the last item in strip
							// it should redirect to the first strip of the album
							//If Not Request.QueryString("currentstrip") Is Nothing Then
							//    Response.Redirect(GetURL(Page.Request.ServerVariables("URL"), Page, "", "currentstrip="))
							//Else
							lblPageInfo.Text = Localization.GetString("AlbumEmpty", mGalleryConfig.SharedResourceFile);
							//End If
						}
					}
				} else {
					EnableControl(false);
				}

			} catch (Exception exc) {
				// JIMJ Add more error logging
				Exceptions.ProcessModuleLoadException(this, exc);
			}
		}

		private void EnableControl(bool IsEnabled)
		{
			spnPager.Visible = IsEnabled;
		}
		public Pager()
		{
			Load += Page_Load;
			Init += Page_Init;
		}

	}
}

