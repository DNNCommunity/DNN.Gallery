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
using DotNetNuke.UI.UserControls;
using Gallery.Exif;

namespace DotNetNuke.Modules.Gallery.WebControls
{

	public partial class Exif : GalleryWebControlBase
	{

		#region "Private Members"

		private GalleryViewerRequest _CurrentRequest = null;
		/// <summary>An instance of the PhotoProperties class</summary>
		private PhotoProperties mPhotoProps;
		/// <summary>Has the PhotoProperties instance successfully initialized?</summary>
		private bool mPhotoPropsInitialized;
		private bool mIsAnalysisSuccessful;
		private string mPhotoPath = "";
			#endregion
		private GalleryFile mCurrentItem;

		#region "Public Properties"
		public GalleryViewerRequest CurrentRequest {
			get { return _CurrentRequest; }
			set { _CurrentRequest = value; }
		}
		#endregion

		#region "Private Methods"
		protected string ExifHelp(string tagDatumName)
		{
			return Localization.GetString(tagDatumName + ".Help", this.LocalResourceFile);
		}

		protected string ExifText(string tagDatumName)
		{
			return Localization.GetString(tagDatumName + ".Text", this.LocalResourceFile);
		}

		private void BindList()
		{
			DotNetNuke.Services.Localization.Localization.LocalizeDataGrid(ref grdExif, this.LocalResourceFile);
			grdExif.DataSource = mPhotoProps.PhotoMetaData;
			grdExif.DataBind();
		}

		/// <summary>
		/// Initializes the PhotoProperties tag properties using asynchronous 
		/// method invocation of the initialization.</summary>
		private void InitializePhotoProperties(string initXmlFile)
		{
			// Create an instance of the PhotoProperties
			mPhotoProps = new PhotoProperties();
			mPhotoPropsInitialized = mPhotoProps.Initialize(initXmlFile);

		}
		//InitializePhotoProperties

		private bool AnalyzeImageFile(string fileName)
		{
			bool isAnalyzed = false;
			try {
				mPhotoProps.Analyze(fileName);
				isAnalyzed = true;
			} catch (InvalidOperationException ex) {
			}
			return isAnalyzed;
		}
		//AnalyzeImageFile

		private void InitExifData()
		{
			string xmlFile = System.IO.Path.Combine(Server.MapPath(GalleryConfig.SourceDirectory() + "/Resources"), "_photoMetadata.xml");
			InitializePhotoProperties(xmlFile);

			mIsAnalysisSuccessful = AnalyzeImageFile(mPhotoPath);
			if (mIsAnalysisSuccessful) {
				BindList();
			}

		}
		#endregion

		#region "Event Handlers"
		private void Page_Load(System.Object sender, System.EventArgs e)
		{
			CurrentRequest = new GalleryViewerRequest(ModuleId);
			mCurrentItem = CurrentRequest.CurrentItem;
			mPhotoPath = mCurrentItem.SourcePath;

			if (!System.IO.File.Exists(mPhotoPath)) {
				mPhotoPath = mCurrentItem.Path;
			}

			InitExifData();
			bool isPopup = this.Parent.TemplateControl.AppRelativeVirtualPath.EndsWith("aspx");

			if (isPopup) {
				GalleryPageBase GalleryPage = (GalleryPageBase)this.Parent.TemplateControl;
				var _with1 = GalleryPage;
				_with1.Title = GalleryPage.Title + " > " + CurrentRequest.CurrentItem.Title;
				_with1.ControlTitle = CurrentRequest.CurrentItem.Title;
			} else {
				CDefault SitePage = (CDefault)this.Page;
				SitePage.Title = SitePage.Title + " > " + CurrentRequest.CurrentItem.Title;
				Gallery.ExifMetaData namingContainer = (Gallery.ExifMetaData)this.NamingContainer;
				var _with2 = namingContainer;
				_with2.Title = CurrentRequest.CurrentItem.Title;
			}
			imgExif.ImageUrl = mCurrentItem.ThumbnailURL;
			litInfo.Text = "<span>" + CurrentRequest.CurrentItem.ItemInfo + "</span>";
			litDescription.Text = "<p class=\"Gallery_Description\">" + CurrentRequest.CurrentItem.Description + "</p>";
		}

		private void grdExif_ItemDataBound(object sender, System.Web.UI.WebControls.DataGridItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item | e.Item.ItemType == ListItemType.AlternatingItem) {
				PhotoTagDatum dataItem = (PhotoTagDatum)e.Item.DataItem;
				if (dataItem != null && dataItem.Id == 37500) {
					//Remove MakerNote Tag (ID=37500) as it can cantain thousands of Hex values which are propriatary to the camera maker and often encrypted
					//Including the MakerNote Tag value in the grid causes a several minute delay before the grid is rendered with some photo images
					e.Item.Cells[3].Text = "&nbsp;";
				}
			}
		}
		#endregion

		#region " Web Form Designer Generated Code "

		//This call is required by the Web Form Designer.
		[System.Diagnostics.DebuggerStepThrough()]

		private void InitializeComponent()
		{
		}


		//NOTE: The following placeholder declaration is required by the Web Form Designer.
		//Do not delete or move it.

		private System.Object designerPlaceholderDeclaration;
		private void Page_Init(System.Object sender, System.EventArgs e)
		{
			//CODEGEN: This method call is required by the Web Form Designer
			//Do not modify it using the code editor.
			InitializeComponent();
		}
		public Exif()
		{
			Init += Page_Init;
			Load += Page_Load;
		}

		#endregion

	}

}
