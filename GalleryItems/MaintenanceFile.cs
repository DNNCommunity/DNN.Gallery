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
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using static DotNetNuke.Common.Globals;
using static DotNetNuke.Modules.Gallery.Config;
using static DotNetNuke.Modules.Gallery.Utils;

namespace DotNetNuke.Modules.Gallery
{

	public class GalleryMaintenanceFile
	{
		private GalleryFolder mParent;
		private string mName;

		private Config.ItemType mType;
		private string mSourcePath;
		private string mAlbumPath;
		private string mThumbPath;
		private string mIconURL;

		private string mURL;
		private bool mSourceExists;
		private bool mFileExists;

		private bool mThumbExists;
		public GalleryFolder Parent {
			get { return mParent; }
		}

		public string Name {
			get { return mName; }
		}

		public Config.ItemType Type {
			get { return mType; }
		}

		public string URL {
			get { return mURL; }
		}

		public string SourcePath {
			get { return mSourcePath; }
		}

		public string AlbumPath {
			get { return mAlbumPath; }
		}

		public string ThumbPath {
			get { return mThumbPath; }
		}

		public string IconURL {
			get { return mIconURL; }
			set { mIconURL = value; }
		}

		public bool SourceExists {
			get { return mSourceExists; }
			set { mSourceExists = value; }
		}

		public bool FileExists {
			get { return mFileExists; }
			set { mFileExists = value; }
		}

		public bool ThumbExists {
			get { return mThumbExists; }
			set { mThumbExists = value; }
		}


		public GalleryMaintenanceFile(GalleryFolder Parent, string Name, Config.ItemType Type, string URL)
		{
			mParent = Parent;
			mName = Name;
			mType = Type;
			mURL = URL;

			DotNetNuke.Modules.Gallery.Config config = Parent.GalleryConfig;

			switch (Type) {
				case Config.ItemType.Image:
					mIconURL = config.GetImageURL("s_jpg.gif");
					//imageIcon
					break;
				case Config.ItemType.Flash:
					mIconURL = config.GetImageURL("s_flash.gif");
					break;
				case Config.ItemType.Movie:
					mIconURL = config.GetImageURL("s_mediaplayer.gif");
					break;
			}

			PopulateProperties();

		}

		private void PopulateProperties()
		{
			mSourcePath = System.IO.Path.Combine(mParent.SourceFolderPath, mName);
			mAlbumPath = System.IO.Path.Combine(mParent.Path, mName);
			mThumbPath = System.IO.Path.Combine(mParent.ThumbFolderPath, mName);
		}


		public void Synchronize()
		{
			// Copy from source to album
			if (mSourceExists) {
				CreateFileFromSource();
			//Copy from album to source
			} else {
				CreateSourceFromFile();
			}

			RebuildThumbnail();

		}


		public void CreateSourceFromFile()
		{
			//William Severance - modified to interface with DNN file system.
			PortalSettings ps = PortalController.GetCurrentPortalSettings();
			if (mFileExists) {
				FileSystemUtils.CopyFile(mAlbumPath, mSourcePath, ps);
				mFileExists = true;
			}
		}

		public void CreateFileFromSource()
		{
			if (mSourceExists) {
				var _with1 = mParent.GalleryConfig;
				GalleryGraphics.ResizeImage(mSourcePath, mAlbumPath, _with1.FixedWidth, _with1.FixedHeight, _with1.EncoderQuality);
				mFileExists = true;
			}
		}

		public void DeleteAll()
		{
			mParent.DeleteChild(mName);
		}

		public void RebuildThumbnail()
		{
			DeleteThumbnail();
			CreateThumbnail();
		}

		public void DeleteThumbnail()
		{
			PortalSettings ps = PortalController.GetCurrentPortalSettings();
			if (File.Exists(mThumbPath))
				mParent.DeleteFile(mThumbPath, ps, false);
			mThumbExists = false;
		}

		public void CreateThumbnail()
		{
			string sourcePath = null;
			if (mSourceExists) {
				sourcePath = mSourcePath;
			} else if (mFileExists) {
				sourcePath = mAlbumPath;
			}
			if (sourcePath == null) {
				mThumbExists = false;
			} else {
				var _with2 = mParent.GalleryConfig;
				GalleryGraphics.ResizeImage(sourcePath, mThumbPath, _with2.MaximumThumbWidth, _with2.MaximumThumbHeight, _with2.EncoderQuality);
				ThumbExists = true;
			}
		}

	}

}
