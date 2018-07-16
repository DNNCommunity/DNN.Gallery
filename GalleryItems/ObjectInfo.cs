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


namespace DotNetNuke.Modules.Gallery
{

	public interface IGalleryObjectInfo
	{

		int Index { get; }
		int ID { get; }
		string Name { get; }
		GalleryFolder Parent { get; }
		string URL { get; }
		string Path { get; }
		string SourceURL { get; }
		string IconURL { get; }

		string ScoreImageURL { get; }
		// Thumbnail & ThumbnailURL should be Read/Write to be modified
		string Thumbnail { get; set; }

		string ThumbnailURL { get; set; }
		// Info from XML also interface implementation
		string Title { get; set; }
		string Description { get; set; }
		string Categories { get; set; }
		string Author { get; set; }
		string Client { get; set; }
		string Location { get; set; }
		int OwnerID { get; set; }
		double Score { get; set; }
		int Width { get; set; }
		int Height { get; set; }

		DateTime ApprovedDate { get; set; }

		DateTime CreatedDate { get; }
		string BrowserURL { get; }
		string SlideshowURL { get; }
		string EditURL { get; }
		string ExifURL { get; }
		string DownloadURL { get; }

		string VotingURL { get; }
		//ReadOnly Property ImageCssClass() As String
		//ReadOnly Property ThumbnailCssClass() As String

		string ItemInfo { get; }
		long Size { get; }
		bool IsFolder { get; }
		Config.ItemType Type { get; }

		DotNetNuke.Modules.Gallery.Config GalleryConfig { get; }
	}

	//William Severance - added class to provide argument for gallery events including
	//GalleryObjectDeleted and GalleryObjectCreated

	public class GalleryObjectEventArgs : System.EventArgs
	{

		private IGalleryObjectInfo _Item;

		private int _UserID;
		public GalleryObjectEventArgs(IGalleryObjectInfo Item, int UserID)
		{
			_Item = Item;
			_UserID = UserID;
		}

		public IGalleryObjectInfo Item {
			get { return _Item; }
		}

		public int UserID {
			get { return _UserID; }
		}
	}

}
