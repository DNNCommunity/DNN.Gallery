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
	/// Base object used by various gallery view code
	/// </summary>
	/// <remarks></remarks>
	public class BaseView
	{


		private GalleryControl mGalleryControl;
		public BaseView(GalleryControl GalleryControl)
		{
			mGalleryControl = GalleryControl;
		}
		//New

		public GalleryControl GalleryControl {
			get { return mGalleryControl; }
		}

		public ControlCollection Controls {
			get { return mGalleryControl.Controls; }
		}

		public virtual void CreateChildControls()
		{
		}
		//CreateChildControls

		public virtual void OnPreRender()
		{
		}
		//OnPreRender

		public virtual void Render(HtmlTextWriter wr)
		{
		}
		//Render

		// Begin table in which we will render object (cellspacing, cellpadding, borderwidth)
		protected void RenderTableBegin(HtmlTextWriter wr, string ClassName)
		{

			wr.AddAttribute(HtmlTextWriterAttribute.Class, ClassName);
			wr.AddAttribute(HtmlTextWriterAttribute.Id, "GalleryContent");
			wr.RenderBeginTag(HtmlTextWriterTag.Table);
		}
		//RenderTableBegin

		protected void RenderTableEnd(HtmlTextWriter wr)
		{
			wr.RenderEndTag();
		}
		// End table in which object was rendered

		protected void RenderInfo(HtmlTextWriter wr, string Info)
		{
			wr.RenderBeginTag(HtmlTextWriterTag.Tr);

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumEmpty");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			wr.Write(Info);
			wr.RenderEndTag();
			// </td>

			wr.RenderEndTag();
			// </tr>
		}

		public static void RenderImage(HtmlTextWriter wr, string ImageURL, string Tooltip, string Css)
		{
			wr.AddAttribute(HtmlTextWriterAttribute.Border, "0");
			wr.AddAttribute(HtmlTextWriterAttribute.Src, ImageURL);

			if (Css.Length > 0) {
				wr.AddAttribute(HtmlTextWriterAttribute.Class, Css);
			}

			//xhtml requires an alt tag, even if it is empty - HWZassenhaus 9/28/2008
			if (Tooltip.Length > 0) {
				wr.AddAttribute(HtmlTextWriterAttribute.Title, Tooltip);
				wr.AddAttribute(HtmlTextWriterAttribute.Alt, Tooltip);
			} else {
				wr.AddAttribute(HtmlTextWriterAttribute.Alt, "");
			}

			wr.RenderBeginTag(HtmlTextWriterTag.Img);
			//<img>
			wr.RenderEndTag();
			///>'

		}

		public static void RenderImageButton(HtmlTextWriter wr, string URL, string ImageURL, string Tooltip, string Css)
		{
			wr.AddAttribute(HtmlTextWriterAttribute.Href, URL.Replace("~/", ""));
			wr.RenderBeginTag(HtmlTextWriterTag.A);
			RenderImage(wr, ImageURL, Tooltip, Css);
			wr.RenderEndTag();
			//</a>
		}

		public static void RenderCommandButton(HtmlTextWriter wr, string URL, string Text, string Css)
		{
			wr.AddAttribute(HtmlTextWriterAttribute.Href, URL.Replace("~/", ""));

			if (Css.Length > 0) {
				wr.AddAttribute(HtmlTextWriterAttribute.Class, Css);
			}

			wr.RenderBeginTag(HtmlTextWriterTag.A);
			wr.Write(Text);
			wr.RenderEndTag();
			//</a>
		}

	}

}
