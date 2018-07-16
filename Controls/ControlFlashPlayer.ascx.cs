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


using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using static DotNetNuke.Common.Globals;

namespace DotNetNuke.Modules.Gallery.WebControls
{
	public partial class FlashPlayer : GalleryWebControlBase
	{

		private GalleryMediaRequest _CurrentRequest;
		private string mFlashURL = "";
		private SWFHeaderReader mFlashProperties;
		private int mWidth;

		private int mHeight;
		public GalleryMediaRequest CurrentRequest {
			get { return _CurrentRequest; }
			set { _CurrentRequest = value; }
		}

		public string FlashUrl {
			get { return mFlashURL; }
		}

		public string FixedWidth {
			get { return mWidth.ToString() + "px"; }
		}

		public string FixedHeight {
			get { return mHeight.ToString() + "px"; }
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
		}

		#endregion


		private void Page_Load(System.Object sender, System.EventArgs e)
		{
			_CurrentRequest = new GalleryMediaRequest(ModuleId);

			if ((CurrentRequest != null) && (CurrentRequest.CurrentItem != null)) {
				string mPath = CurrentRequest.CurrentItem.Path;
				if (GalleryConfig.IsValidFlashType(System.IO.Path.GetExtension(mPath))) {
					mFlashURL = CurrentRequest.CurrentItem.URL;
					mFlashProperties = new SWFHeaderReader(mPath);
					int maxW = GalleryConfig.FixedWidth;
					int maxH = GalleryConfig.FixedHeight;
					if (mFlashProperties.HasError || (mFlashProperties.Width == 0 || mFlashProperties.Height == 0)) {
						mWidth = maxW;
						mHeight = maxW;
					} else {
						mWidth = mFlashProperties.Width;
						mHeight = mFlashProperties.Height;
						if (mWidth > maxW || mHeight > maxH) {
							double ratio = mHeight / mWidth;
							// Bounded by height
							if (ratio > maxH / maxW) {
								mWidth = Convert.ToInt32(maxH / ratio);
								mHeight = maxH;
							//Bounded by width
							} else {
								mWidth = maxW;
								mHeight = Convert.ToInt32(maxW * ratio);
							}
						}
					}
				}
				bool isPopup = this.Parent.TemplateControl.AppRelativeVirtualPath.EndsWith("aspx");

				if (isPopup) {
					GalleryPageBase GalleryPage = (GalleryPageBase)this.Parent.TemplateControl;
					var _with1 = GalleryPage;
					_with1.Title = GalleryPage.Title + " > " + CurrentRequest.CurrentItem.Title;
					_with1.ControlTitle = CurrentRequest.CurrentItem.Title;
				} else {
					CDefault SitePage = (CDefault)this.Page;
					SitePage.Title = SitePage.Title + " > " + CurrentRequest.CurrentItem.Title;
					Gallery.FlashPlayer namingContainer = (Gallery.FlashPlayer)this.NamingContainer;
					var _with2 = namingContainer;
					_with2.Title = CurrentRequest.CurrentItem.Title;
					_with2.ViewControlWidth = new Unit(mWidth + 58);
				}
				litInfo.Text = "<span>" + CurrentRequest.CurrentItem.ItemInfo + "</span>";
				litDescription.Text = "<p class=\"Gallery_Description\">" + CurrentRequest.CurrentItem.Description + "</p>";
			}
		}
		public FlashPlayer()
		{
			Load += Page_Load;
			Init += Page_Init;
		}
	}
}


