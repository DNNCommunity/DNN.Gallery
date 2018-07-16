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
using static DotNetNuke.Common.Globals;
using static DotNetNuke.Modules.Gallery.Config;
using static DotNetNuke.Modules.Gallery.Utils;

namespace DotNetNuke.Modules.Gallery
{

	/// <summary>
	/// Class for using the viewer to view galleries
	/// </summary>
	/// <remarks></remarks>
	public class GalleryFlashRequest : BaseRequest
	{


		private HttpRequest mRequest;
		// Stored reference to ItemIndex of Browseable Items collection

		private int _currentItem;
		#region "Public Properties"

		public GalleryFile CurrentItem {
			get { return (GalleryFile)base.Folder.List.Item(_currentItem); }
		}

		public int CurrentItemNumber {
			get { return _currentItem + 1; }
		}

		#endregion

		#region "Public Methods"

		public GalleryFlashRequest(int ModuleID) : base(ModuleID)
		{

			// Don't want to continue processing if this is an invalid path
			if (!GalleryConfig.IsValidPath) {
				return;
			}

			mRequest = HttpContext.Current.Request;

			if ((mRequest.QueryString["currentitem"] != null)) {
				_currentItem = int.Parse(mRequest.QueryString["currentitem"]);
			} else {
				if (base.Folder.MediaItems.Count > 0) {
					_currentItem = base.Folder.FlashItems[0];
					//CType(MyBase.Folder.FlashItems.Item(0), GalleryFile).Index
				}
			}

		}

		#endregion

	}

}
