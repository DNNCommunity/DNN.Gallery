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

	// Class for using the viewer to view galleries
	public class GalleryMaintenanceRequest : BaseRequest
	{

			//GalleryObjectCollection
		private System.Collections.Generic.List<GalleryMaintenanceFile> mImageList;

		public List<GalleryMaintenanceFile> ImageList {
			//GalleryObjectCollection
			get {
				if (mImageList == null) {
					mImageList = new System.Collections.Generic.List<GalleryMaintenanceFile>();
				}
				return mImageList;
			}
            set
            {
                mImageList = value;
            }
		}


		public GalleryMaintenanceRequest(int ModuleID) : base(ModuleID)
		{

			// Don't want to continue processing if this is an invalid path
			if (!GalleryConfig.IsValidPath) {
				return;
			}

			Populate();

		}

		public void Populate()
		{
			string mAlbumPath = base.Folder.Path;
			string mSourcePath = base.Folder.SourceFolderPath;
			string mThumbPath = base.Folder.ThumbFolderPath;
			string file = null;

			// Clear imageList first
			ImageList.Clear();

			// Repopulate gallery folder to make sure all new files to be populated
			if (!Folder.IsPopulated) {
				Folder.Populate(true);
			}

			string[] mSourceFiles = System.IO.Directory.GetFiles(mSourcePath);
			ArrayList Sources = new ArrayList();

			//Build sources arraylist
			foreach (string file_loopVariable in mSourceFiles) {
				file = file_loopVariable;
				Sources.Add(Strings.LCase(System.IO.Path.GetFileName(file)));
			}

			// Build thumbs arraylist
			string[] mThumbFiles = System.IO.Directory.GetFiles(mThumbPath);
			ArrayList Thumbs = new ArrayList();
			foreach (string file_loopVariable in mThumbFiles) {
				file = file_loopVariable;
				Thumbs.Add(Strings.LCase(System.IO.Path.GetFileName(file)));
			}

			IGalleryObjectInfo item = null;
			string fileName = null;
			string fileExtension = null;
			Config.ItemType fileType = default(Config.ItemType);

			//Populate all items in this folder
			foreach (IGalleryObjectInfo item_loopVariable in base.Folder.List) {
				item = item_loopVariable;
				if (!item.IsFolder) {
					fileName = item.Name;
					GalleryMaintenanceFile fileItem = new GalleryMaintenanceFile(base.Folder, fileName, item.Type, item.BrowserURL);

					fileItem.FileExists = true;

					// Check if file exists in source folder
					if (Sources.Contains(Strings.LCase(fileName))) {
						fileItem.SourceExists = true;
						// For checking, remove this file from arraylist
						Sources.Remove(Strings.LCase(fileName));
					}

					// Check if file exists in thumb folder
					if (Thumbs.Contains(Strings.LCase(fileName))) {
						fileItem.ThumbExists = true;
						Thumbs.Remove(Strings.LCase(fileName));
					}

					//Add to filelist, it's not nessesary for this version, however will be used in next version
					//mFileList.Add(fileName, fileItem)

					// Add to imagelist
					if (item.Type == Config.ItemType.Image && !(Strings.LCase(item.Title) == "icon") && !(Strings.LCase(item.Title) == "watermark")) {
						ImageList.Add(fileItem);
					}

				}
			}

			// List file exists in source & missing in album
			foreach (string fileName_loopVariable in Sources) {
				fileName = fileName_loopVariable;
				fileExtension = System.IO.Path.GetExtension(fileName);
				fileType = GalleryConfig.TypeFromExtension(fileExtension);

				// Add to file list, reserved for next version feature
				//mFileList.Add(fileName, fileItem)

				if (fileType == Config.ItemType.Image) {
					GalleryMaintenanceFile fileItem = new GalleryMaintenanceFile(base.Folder, fileName, fileType, "");
					fileItem.SourceExists = true;

					// Check if file exists in thumb folder
					if (Thumbs.Contains(Strings.LCase(fileName))) {
						fileItem.ThumbExists = true;
						Thumbs.Remove(Strings.LCase(fileName));
					}

					ImageList.Add(fileItem);
				}
			}

			// Check in thumb folder, for clean up unused thumbnail
			foreach (string fileName_loopVariable in Thumbs) {
				fileName = fileName_loopVariable;
				fileExtension = System.IO.Path.GetExtension(fileName);
				fileType = GalleryConfig.TypeFromExtension(fileExtension);

				if (fileType == Config.ItemType.Image) {
					GalleryMaintenanceFile fileItem = new GalleryMaintenanceFile(base.Folder, fileName, fileType, "");
					fileItem.ThumbExists = true;
					ImageList.Add(fileItem);
				}

			}
		}

	}

}
