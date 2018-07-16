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
using DotNetNuke.Entities.Modules;
using DotNetNuke.Modules.Gallery;

namespace DotNetNuke.Modules.Gallery.WebControls
{
	public partial class MediaPlayer : GalleryWebControlBase
	{

		private string _MovieURL = "";

		private GalleryMediaRequest _CurrentRequest = null;
		public GalleryMediaRequest CurrentRequest {
			get { return _CurrentRequest; }
			set { _CurrentRequest = value; }
		}

		protected string MovieURL {
			get { return _MovieURL; }
		}

		private void Page_Load(System.Object sender, System.EventArgs e)
		{
			_CurrentRequest = new DotNetNuke.Modules.Gallery.GalleryMediaRequest(ModuleId);
			if ((CurrentRequest != null) && (CurrentRequest.CurrentItem != null)) {
				string mPath = CurrentRequest.CurrentItem.Path;
				if (GalleryConfig.IsValidMovieType(System.IO.Path.GetExtension(mPath))) {
					_MovieURL = Strings.Trim(CurrentRequest.CurrentItem.URL);
					bool isPopup = this.Parent.TemplateControl.AppRelativeVirtualPath.EndsWith("aspx");
					if (isPopup) {
						GalleryPageBase GalleryPage = (GalleryPageBase)this.Parent.TemplateControl;
						var _with1 = GalleryPage;
						_with1.Title = GalleryPage.Title + " > " + CurrentRequest.CurrentItem.Title;
						_with1.ControlTitle = CurrentRequest.CurrentItem.Title;
					} else {
						CDefault SitePage = (CDefault)this.Page;
						SitePage.Title = SitePage.Title + " > " + CurrentRequest.CurrentItem.Title;
						Gallery.MediaPlayer namingContainer = (Gallery.MediaPlayer)this.NamingContainer;
						var _with2 = namingContainer;
						_with2.Title = CurrentRequest.CurrentItem.Title;
						_with2.ViewControlWidth = new Unit(GalleryConfig.FixedWidth + 58);
					}
					RenderPlayer();
					litInfo.Text = "<span>" + CurrentRequest.CurrentItem.ItemInfo + "</span>";
					litDescription.Text = "<p class=\"Gallery_Description\">" + CurrentRequest.CurrentItem.Description + "</p>";
				}
			}
		}

		private void RenderPlayer()
		{
			if (!string.IsNullOrEmpty(_MovieURL)) {
				//WES - Added "px" and style attributes for xhtml compliancy
				string strH = (GalleryConfig.FixedHeight).ToString() + "px";
				string strW = (GalleryConfig.FixedWidth).ToString() + "px";
				string ext = System.IO.Path.GetExtension(CurrentRequest.CurrentItem.Name).ToLower();

				StringBuilder sb = new StringBuilder();

				if (ext == ".mov" || ext == ".mp4" || ext == ".m4v") {
					sb.AppendLine("<object id=\"objQuickTime\" name=\"objQuickTime\" height=\"" + strH + "\" width=\"" + strW + "\" codebase=\"http://www.apple.com/qtactivex/qtplugin.cab\" classid=\"clsid:02BF25D5-8C17-4B23-BC80-D3488ABDDC6B\">");
					sb.AppendLine("     <param name=\"src\" VALUE=\"" + MovieURL + "\"></param>");
					sb.AppendLine("     <param name=\"bgcolor\" value=\"transparent\"></param>");
					sb.AppendLine("     <param name=\"scale\" value=\"1.0\"></param>");
					sb.AppendLine("     <param name=\"volume\" value=\"100\"></param>");
					sb.AppendLine("     <param name=\"enablejavascript\" value=\"False\"></param>");
					sb.AppendLine("     <param name=\"autoplay\" value=\"True\"></param>");
					sb.AppendLine("     <param name=\"cache\" value=\"True\"></param>");
					sb.AppendLine("     <param name=\"targetcache\" value=\"False\"></param>");
					sb.AppendLine("     <param name=\"kioskmode\" value=\"False\"></param>");
					sb.AppendLine("     <param name=\"loop\" value=\"True\"></param>");
					sb.AppendLine("     <param name=\"playeveryframe\" value=\"False\"></param>");
					sb.AppendLine("     <param name=\"controller\" value=\"true\"></param>");
					sb.AppendLine("     <param name=\"href\" value=\"\"></param>");
					sb.AppendLine("  <embed src=\"" + MovieURL + "\" height=\"" + strH + "\" width=\"" + strW + "\" autoplay=\"true\" type=\"video/quicktime\" pluginspage=\"http://www.apple.com/quicktime/download/\" controller=\"false\" href=\"" + MovieURL + "\" target=\"myself\"></embed>");
				} else {
					//WES - Added browser detection and alternate rendering if not MS IE browser
					bool isMSIE = Request.UserAgent.Contains("MSIE");
					if (isMSIE) {
						sb.AppendLine("<object id=\"Player\" type=\"video/x-ms-wmv\" classid=\"CLSID:6BF52A52-394A-11d3-B153-00C04F79FAA6\" height=\"" + strH + "\" width=\"" + strW + "\" VIEWASTEXT>");
						sb.AppendLine("     <param name=\"url\" value=\"" + MovieURL + "\" valuetype=\"ref\" type=\"video/x-ms-wmv\">");
					} else {
						sb.AppendLine("<object id=\"Player\" type=\"video/x-ms-wmv\" data=\"" + MovieURL + "\" width=\"" + strW + "\" height=\"" + strH + "\">");
						sb.AppendLine("     <param name=\"src\" value=\"" + MovieURL + "\" valuetype=\"ref\" type=\"" + MovieURL + "\">");
					}
					sb.AppendLine("     <param name=\"animationatStart\" value=\"1\">");
					sb.AppendLine("     <param name=\"transparentatStart\" value=\"1\">");
					sb.AppendLine("     <param name=\"autoStart\" value=\"1\">");
					sb.AppendLine("     <param name=\"displaysize\" value=\"0\">");
					sb.AppendLine("   <a href=\"http://www.microsoft.com/windows/windowsmedia/download/AllDownloads.aspx\">" + Localization.GetString("GetWindowsMediaPlayerPlugIn", GalleryConfig.SharedResourceFile) + "</a>");
				}
				sb.AppendLine("</object>");
				ctrlMediaPlayer.Text = sb.ToString();
			}
		}
		public MediaPlayer()
		{
			Load += Page_Load;
		}
	}
}


