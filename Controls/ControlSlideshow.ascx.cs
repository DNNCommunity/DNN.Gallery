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
using static DotNetNuke.Common.Globals;
using static DotNetNuke.Modules.Gallery.Utils;

namespace DotNetNuke.Modules.Gallery.WebControls
{

	public partial class Slideshow : GalleryWebControlBase
	{


		private GalleryViewerRequest _CurrentRequest;
		#region "Public Properties"
		public GalleryViewerRequest CurrentRequest {
			get { return _CurrentRequest; }
			set { _CurrentRequest = value; }
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

		#endregion

		private void Page_Load(System.Object sender, System.EventArgs e)
		{
			ErrorMessage.Visible = false;

			_CurrentRequest = new GalleryViewerRequest(ModuleId, Utils.GetSort(GalleryConfig), Utils.GetSortDESC(GalleryConfig));

			if (CurrentRequest == null || !CurrentRequest.Folder.IsPopulated) {
				Response.Redirect(ApplicationURL());
			}
			if (!IsPostBack) {

				if (CurrentRequest.Folder.IsBrowsable) {


					celPicture.Width = GalleryConfig.FixedWidth.ToString();
					celPicture.Height = GalleryConfig.FixedHeight.ToString();
					//WES - localized loading message
					ErrorMessage.Text = Localization.GetString("Loading", GalleryConfig.SharedResourceFile);

					string albumPath = CurrentRequest.Folder.Path;
					string slideSpeed = GalleryConfig.SlideshowSpeed.ToString();


					//Generate Clientside Javascript for Slideshow

					bool isPopup = this.Parent.TemplateControl.AppRelativeVirtualPath.EndsWith("aspx");

					if (isPopup) {
						DotNetNuke.Framework.jQuery.RegisterScript(Page);
					} else {
						DotNetNuke.Framework.jQuery.RequestRegistration();
					}

					StringBuilder sb = new StringBuilder();

					sb.Append("    var baseTitle = '");
					if (isPopup) {
						sb.Append(((GalleryPageBase)this.Parent.TemplateControl).Title);
					} else {
						sb.Append(((CDefault)this.Page).Title);
					}
					sb.AppendLine("';");

					// Write all of the image data out
					GalleryFile image = null;
					int count = 0;
					int startItemNumber = CurrentRequest.CurrentItemNumber;
					int totalItemCount = CurrentRequest.BrowsableItems.Count;
					string startImageURL = CurrentRequest.CurrentItem.URL;
					sb.Append("    jQuery('#");
					sb.Append(celPicture.ClientID);
					sb.Append("').data('pics', [");
					do {
						image = CurrentRequest.CurrentItem;
						AppendPicData(sb, count, image);
						count += 1;
						CurrentRequest.MoveNext();
					} while (!(CurrentRequest.CurrentItemNumber == startItemNumber));
					sb.Remove(sb.Length - 3, 3);
					// remove trailing command and vbCrlf characters
					sb.AppendLine("]);");
					sb.Append("    runSlideShow(0, ");
					sb.Append(slideSpeed.ToString());
					sb.AppendLine(", 500);");

					Page.ClientScript.RegisterStartupScript(this.GetType(), ClientID + "_SlideShow", sb.ToString(), true);
				} else {
					ErrorMessage.Visible = true;
					//WES - localized album empty message
					ErrorMessage.Text = Localization.GetString("AlbumEmpty", GalleryConfig.SharedResourceFile);
					//Album contains no images!
				}
			}
		}

		private void AppendPicData(StringBuilder sb, int i, GalleryFile data)
		{
			var _with1 = sb;
			_with1.Append("{src:'");
			sb.Append(Utils.JSEncode(data.URL));
			sb.Append("', title:'");
			sb.Append(Utils.JSEncode(data.Title.Replace(Constants.vbCrLf, "<br />")));
			sb.Append("', desc:'");
			sb.Append(Utils.JSEncode(data.Description.Replace(Constants.vbCrLf, "<br />")));
			sb.AppendLine("'},");
		}
		public Slideshow()
		{
			Load += Page_Load;
			Init += Page_Init;
		}
	}

}
