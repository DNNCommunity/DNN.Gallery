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


using System.IO;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace DotNetNuke.Modules.Gallery
{

	public class FileToUpload
	{


		PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
		private System.Web.UI.HtmlControls.HtmlInputFile mHtmlInput;
		private int mContentLength;
		private int mModuleID = -1;
		private string mFileName;
		private string mExtension;
		private string mTitle = Null.NullString;
		private string mDescription = Null.NullString;
		private string mCategories = Null.NullString;
		private string mAuthor = Null.NullString;
		private string mClient = Null.NullString;
		private string mLocation = Null.NullString;
		private string mOwner = Null.NullString;
		private int mOwnerID = -1;
		private Config.ItemType mType = Config.ItemType.Image;
		private string mIcon = Null.NullString;
			//Only for zip files else Null
		private ZipHeaderReader mZipHeaders = null;

		private DotNetNuke.Modules.Gallery.Config mGalleryConfig;
		#region "Public Properties"
		public int ModuleID {
			get { return mModuleID; }
			set { mModuleID = value; }
		}

		public string FileName {
			get { return mFileName; }
		}

		public string Extension {
			get { return mExtension; }
		}

		public string ContentType {
			get { return mHtmlInput.PostedFile.ContentType; }
		}

		public int ContentLength {
			get { return mContentLength; }
		}

		public string Title {
			get { return mTitle; }
			set { mTitle = value; }
		}

		public string Description {
			get { return mDescription; }
			set { mDescription = value; }
		}

		public string Author {
			get { return mAuthor; }
			set { mAuthor = value; }
		}

		public string Client {
			get { return mClient; }
			set { mClient = value; }
		}

		public string Location {
			get { return mLocation; }
			set { mLocation = value; }
		}

		public string Categories {
			get { return mCategories; }
			set { mCategories = value; }
		}

		public int OwnerID {
			get { return mOwnerID; }
			set { mOwnerID = value; }
		}

		public string Owner {
			get {
				Entities.Users.UserController uc = new Entities.Users.UserController();
				UserInfo user = uc.GetUser(_portalSettings.PortalId, Utils.ValidUserID(mOwnerID));
				if (user == null) {
					mOwner = Null.NullString;
				} else {
					mOwner = user.Username;
				}
				return mOwner;
			}
		}

		public DateTime ApprovedDate {
			get {
				if (mGalleryConfig.AutoApproval | new Authorization(ModuleID).HasAdminPermission()) {
					return DateTime.Now;
				} else {
					return DateTime.MaxValue;
				}
			}
		}

		public string Icon {
			get { return mIcon; }
		}

		public System.Web.UI.HtmlControls.HtmlInputFile HtmlInput {
			get { return mHtmlInput; }
		}

		public Config.ItemType Type {
			get { return mType; }
		}

		public DotNetNuke.Modules.Gallery.Config GalleryConfig {
			get { return mGalleryConfig; }
			set { mGalleryConfig = value; }
		}

		public ZipHeaderReader ZipHeaders {
			get {
				if (Extension == ".zip" && mZipHeaders == null) {
					mZipHeaders = new ZipHeaderReader(HtmlInput.PostedFile.InputStream);
				}
				return mZipHeaders;
			}
		}

		public bool ExceedsFileSizeLimit(int limit) {
			if (limit == 0)
				return false;
			//no limit specified

			if (mExtension == ".zip") {
				return ZipHeaders.ExceedsFileSizeLimit(limit);
			} else {
				return ContentLength > limit;
			}
		}

		public bool IsValidFileType {
			get {
				bool isValid = true;
				if (mExtension == ".zip") {
					foreach (ZipFileEntry entry in ZipHeaders.Entries) {
						isValid = GalleryConfig.IsValidFileType(entry.Extension);
						if (!isValid)
							break; // TODO: might not be correct. Was : Exit For
					}
				} else {
					isValid = GalleryConfig.IsValidFileType(mExtension);
				}
				return isValid;
			}
		}

		#endregion

		public FileToUpload(System.Web.UI.HtmlControls.HtmlInputFile httpInput)
		{
			mHtmlInput = httpInput;
			mFileName = Utils.MakeValidFilename(Path.GetFileName(httpInput.PostedFile.FileName), "_");
			mExtension = Path.GetExtension(mFileName).ToLower();
			mContentLength = HtmlInput.PostedFile.ContentLength;
			if (mExtension == ".zip") {
				if (ZipHeaders.IsValid & !ZipHeaders.HasError) {
					mContentLength = Convert.ToInt32(ZipHeaders.UncompressedSize);
				}
			}
		}

		/// <summary>
		/// Validate the upload file to see if it meets some general
		/// Criteria we have for valid files
		/// </summary>
		/// <returns></returns>
		/// <remarks></remarks>
		public string ValidateFile()
		{

			if (ContentLength == 0) {
				string str = null;
				str = Localization.GetString("UploadFile_InvalidFile", mGalleryConfig.SharedResourceFile);
				if (string.IsNullOrEmpty(str)) {
					str = "Invalid file";
				}
				return str;
			}

			//William Severance - check now includes test against uncompressed size of each file in zip

			if (ExceedsFileSizeLimit(mGalleryConfig.MaxFileSize * 1024)) {
				string str = null;
				str = Localization.GetString("UploadFile_InvalidSize", mGalleryConfig.SharedResourceFile);
				if (string.IsNullOrEmpty(str)) {
					str = "File size (including size of each file in a zip) must be smaller than {0} kb";
				}
				str = string.Format(str, mGalleryConfig.MaxFileSize.ToString());
				return str;
			}

			if (!IsValidFileType) {
				string str = null;
				str = Localization.GetString("UploadFile_InvalidExtension", mGalleryConfig.SharedResourceFile);
				if (string.IsNullOrEmpty(str)) {
					str = "{0} is not an acceptable file type.";
				}
				str = string.Format(str, mExtension);
				return str;
			} else {
				//Generate icon
				mIcon = Utils.GetFileTypeIcon(mExtension, GalleryConfig, ref mType);
			}
			return "";
		}

	}

}
