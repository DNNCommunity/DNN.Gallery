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

using System.Drawing;
using System.Reflection;
using System.IO;
using System.Threading;
using DotNetNuke.Entities.Portals;
using static DotNetNuke.Common.Globals;
using static DotNetNuke.Modules.Gallery.Config;
using static DotNetNuke.Modules.Gallery.Utils;

namespace DotNetNuke.Modules.Gallery
{

	public class GalleryFolder : IGalleryObjectInfo
	{

		#region "Private Variables"
		private int mIndex;
		private int mID = -1;
		// System info
		private GalleryFolder mParent;
		private string mGalleryHierarchy;
		private string mName;
		private string mURL;
		private string mPath;
		private string mThumbnail;
		private string mThumbnailURL;
		private string mThumbFolderPath;
		private string mThumbFolderURL;
		private string mIcon;
		private string mSourceFolderPath;

		private string mSourceFolderURL;
		// Info from XML also interface implementation
		private string mTitle;
		private string mDescription;
		private string mCategories;
		private string mAuthor;
		private string mClient;
		private string mLocation;
		private int mOwnerID;
		private DateTime mApprovedDate;

		private DateTime mCreatedDate;
		// Interface implementation for display purpose        
		private string mPopupSlideshowURL = "";
		private string mItemInfo = "";

		private DotNetNuke.Modules.Gallery.Config mGalleryConfig;
		private bool mIsPopulated;
		private GalleryFile mWatermarkImage;
		private GalleryObjectCollection mList = new GalleryObjectCollection();

		private List<IGalleryObjectInfo> mSortList = new List<IGalleryObjectInfo>();
		private List<int> mBrowsableItems = new List<int>();
		private List<int> mMediaItems = new List<int>();
		private List<int> mFlashItems = new List<int>();
		private List<IGalleryObjectInfo> mIconItems = new List<IGalleryObjectInfo>();

		private List<int> mUnApprovedItems = new List<int>();
		#endregion

		#region "Events"
		public event GalleryObjectDeletedEventHandler GalleryObjectDeleted;
		public delegate void GalleryObjectDeletedEventHandler(object sender, GalleryObjectEventArgs e);
		public event GalleryObjectCreatedEventHandler GalleryObjectCreated;
		public delegate void GalleryObjectCreatedEventHandler(object sender, GalleryObjectEventArgs e);
		#endregion

		#region "Properties"

		//<tamttt:note identity id for all  module objects>
		public int ObjectTypeCode {
			get { return 501; }
		}

		public int Index {
			get { return mIndex; }
		}

		public int ID {
			get { return mID; }
		}

		public GalleryFolder Parent {
			get { return mParent; }
		}

		public string GalleryHierarchy {
			get { return mGalleryHierarchy; }
		}

		public string Name {
			get { return mName; }
		}

		public string URL {
			get { return mURL; }
		}

		public string Path {
			get { return mPath; }
		}

		public string Thumbnail {
			get { return mThumbnail; }
			set { mThumbnail = value; }
		}

		public string ThumbnailURL {
			get { return mThumbnailURL; }
			set { mThumbnailURL = value; }
		}

		public string ThumbFolderPath {
			get { return mThumbFolderPath; }
		}

		public string ThumbFolderURL {
			get { return mThumbFolderURL; }
		}

		// We actually don't need this property, however keep it here for implementing gallery file
		public string SourceURL {
			get { return mSourceFolderURL; }
		}

		public string SourceFolderPath {
			get { return mSourceFolderPath; }
		}

		public string SourceFolderURL {
			get { return mSourceFolderURL; }
		}

		public string Icon {
			get { return mIcon; }
		}

		public string IconURL {
			get { return mGalleryConfig.GetImageURL("s_Folder.gif"); }
		}

		public string ScoreImageURL {
			get { return ""; }
		}

		public string Title {
			get { return mTitle; }
			set { mTitle = value; }
		}

		public string Description {
			get { return mDescription; }
			set { mDescription = value; }
		}

		public string Categories {
			get { return mCategories; }
			set { mCategories = value; }
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

		public int OwnerID {
			get { return mOwnerID; }
			set { mOwnerID = value; }
		}

		public int Width {
			get { return -1; }
				// Do Nothing
			set { }
		}

		public int Height {
			get { return -1; }
				// Do Nothing
			set { }
		}

		public double Score {
			get { return 0; }
				//Do Nothing
			set { }
		}

		public DateTime ApprovedDate {
			get { return mApprovedDate; }
			set { mApprovedDate = value; }
		}

		public DateTime CreatedDate {
			get { return mCreatedDate; }
		}

		// Collection of items that can be viewed using the viewer
		public List<int> BrowsableItems {
			get { return mBrowsableItems; }
		}

		// Collection of items that can be played by Media player
		public List<int> MediaItems {
			get { return mMediaItems; }
		}

		// Collection of items that can be played by flash player
		public List<int> FlashItems {
			get { return mFlashItems; }
		}

		// These items not to be view in GUI
		public List<IGalleryObjectInfo> IconItems {
			get { return mIconItems; }
		}

		public List<int> UnApprovedItems {
			get { return mUnApprovedItems; }
		}

		// Is this a folder?
		public bool IsFolder {
			get { return true; }
		}

		public Config.ItemType Type {
			get { return Config.ItemType.Folder; }
		}

		// Collection of objects implementing IGalleryObjectInfo
		public GalleryObjectCollection List {
			get { return mList; }
		}

		public List<IGalleryObjectInfo> SortList {
			get { return mSortList; }
		}

		// Whether this folder has been populated or not
		public bool IsPopulated {
			get { return mIsPopulated; }
			set { mIsPopulated = value; }
		}

		// Whether this folder has at least one browsable item
		public bool IsBrowsable {
			get { return (mBrowsableItems.Count > 0); }
		}

		public long Size {
			get { return mList.Count; }
		}

		//Public ReadOnly Property ImageCssClass() As String Implements IGalleryObjectInfo.ImageCssClass
		//  Get
		//    Return ""
		//  End Get
		//End Property

		//Public ReadOnly Property ThumbnailCssClass() As String Implements IGalleryObjectInfo.ThumbnailCssClass
		//  Get
		//    Return ""
		//  End Get
		//End Property

		public string BrowserURL {
//WES - A folder/album can never be a popup so don't ever need PopupBrowserURL
			get { return GetBrowserURL(); }
		}

		public string SlideshowURL {
			get {
				if (GalleryConfig.AllowPopup) {
					return GetPopupSlideshowURL();
				} else {
					return GetSlideshowURL();
				}
			}
		}

		public string EditURL {
			get { return GetEditURL(); }
		}

		public string MaintenanceURL {
			get { return GetMaintenanceURL(); }
		}

		public string AddSubAlbumURL {
			get { return GetAddSubAlbumURL(); }
		}

		public string AddFileURL {
			get { return GetAddFileURL(); }
		}

		public string ExifURL {
			get { return ""; }
		}

		public string DownloadURL {
			get { return ""; }
		}

		public string VotingURL {
			get { return ""; }
		}

		public string ItemInfo {
			get { return mItemInfo; }
		}

		public GalleryFile WatermarkImage {
			get { return mWatermarkImage; }
		}

		public DotNetNuke.Modules.Gallery.Config GalleryConfig {
			get { return mGalleryConfig; }
		}
		#endregion

		#region "Public Methods and Functions"
		public GalleryFolder() : base()
		{
		}

		internal GalleryFolder(int Index, int ID, GalleryFolder Parent, string GalleryHierarchy, string Name, string URL, string Path, string Thumbnail, string ThumbnailURL, string ThumbFolderPath,
		string ThumbFolderURL, string Icon, string IconURL, string SourceFolderPath, string SourceFolderURL, string Title, string Description, string Categories, string Author, string Client,

		string Location, int OwnerID, DateTime ApprovedDate, DateTime CreatedDate, DotNetNuke.Modules.Gallery.Config GalleryConfig)
		{
			mIndex = Index;
			mID = ID;
			mParent = Parent;
			mGalleryHierarchy = GalleryHierarchy;
			mName = Name;
			mURL = URL;
			mPath = Path;
			mThumbnail = Thumbnail;
			mThumbnailURL = ThumbnailURL;
			mThumbFolderPath = ThumbFolderPath;
			mThumbFolderURL = ThumbFolderURL;
			mIcon = Icon;
			mSourceFolderPath = SourceFolderPath;
			mSourceFolderURL = SourceFolderURL;
			mTitle = Title;
			mDescription = Description;
			mCategories = Categories;
			mAuthor = Author;
			mClient = Client;
			mLocation = Location;
			mOwnerID = OwnerID;
			mApprovedDate = ApprovedDate;
			mCreatedDate = CreatedDate;
			mGalleryConfig = GalleryConfig;
		}

		public void Clear()
		{
			mList.Clear();
			mIsPopulated = false;
		}

		public void Save()
		{
			//Verify that the root folders as well as the _source and _thumb folders exist
			//in both the physical file system and the DNN Folders table and create if necessary
			int _FolderID = CreateRootFolders(URL, true);
			if (_FolderID != -1) {
				mID = _FolderID;
				GalleryXML metaData = new GalleryXML(Parent.Path);
				GalleryXML.SaveMetaData(Parent.Path, Name, ID, Title, Description, Categories, Author, Location, Client, OwnerID,
				CreatedDate, ApprovedDate, 0);

				if ((Parent != null)) {
					Parent.IsPopulated = false;
					Utils.PopulateAllFolders(Parent, false);
				} else {
					this.IsPopulated = false;
					Utils.PopulateAllFolders(this, false);
				}
			}

		}

		// GAL-6255
		public bool ValidateGalleryName(string folderName)
		{
			// WES refactored 2-20-09 to use regex with tighter pattern matching re GAL-9402
			// WES modified 3-20-2011 to allow unicode letters as first character
			return Regex.IsMatch(folderName, "[^\\000-\\037\\\\/:*?#!\"><|&%_][^\\000-\\037\\\\/:*?#!\"><|&%]*");
		}

		//William Severance - modified to interface with DNN file system
		public int CreateChild(string ChildName, string ChildTitle, string ChildDescription, string Author, string Location, string Client, string ChildCategories, int ChildOwnerId, DateTime ApprovedDate)
		{

			string newFolderPath = Utils.BuildPath(new string[2] {
				mPath,
				ChildName
			}, "\\", false, true);
			int newFolderID = -2;
			if (!Directory.Exists(newFolderPath)) {
				string newFolderURL = Utils.BuildPath(new string[2] {
					mURL,
					ChildName + "/"
				}, "/", false, false);
				newFolderID = CreateRootFolders(newFolderURL, true);
				if (newFolderID >= 0) {
					GalleryXML metaData = new GalleryXML(this.Path);
					GalleryXML.SaveMetaData(Path, ChildName, newFolderID, ChildTitle, ChildDescription, ChildCategories, Author, Location, Client, ChildOwnerId,
					DateTime.Now, ApprovedDate, 0);
					Config.ResetGalleryFolder(this);
				}
			}
			return newFolderID;
		}

		//William Severance - modified to interface with DNN file system
		public string DeleteChild(IGalleryObjectInfo Child)
		{
			string strInfo = "";
			PortalSettings ps = PortalController.GetCurrentPortalSettings();
			Gallery.Authorization auth = new Gallery.Authorization(GalleryConfig.ModuleID);
			if (auth.HasItemEditPermission(Child)) {
				try {
					if (Child.IsFolder) {
						System.IO.DirectoryInfo di = new DirectoryInfo(Child.Path);
						if (di.Exists) {
							DeleteFolder(ps.PortalId, di, FileSystemUtils.FormatFolderPath(Child.URL.Replace(ps.HomeDirectory, "")), true);
						}
					} else {
						if (File.Exists(Child.Path))
							DeleteFile(Child.Path, ps, true);
						if (Child.Type == Config.ItemType.Image) {
							string childThumbPath = System.IO.Path.Combine(this.ThumbFolderPath, Child.Name);
							if (File.Exists(childThumbPath))
								DeleteFile(childThumbPath, ps, true);
							string childSourcePath = System.IO.Path.Combine(this.SourceFolderPath, Child.Name);
							if (File.Exists(childSourcePath))
								DeleteFile(childSourcePath, ps, true);

							//William Severance - added to reset folder thumbnail to "folder.gif" if its current
							//thumbnail is being deleted. Will then get set to next image thumbnail during populate if there is one.
							if (this.Thumbnail == Child.Name)
								ResetThumbnail();
						}
					}

					GalleryXML metaData = new GalleryXML(mPath);
					GalleryXML.DeleteMetaData(mPath, Child.Name);

				} catch (System.Exception exc) {
					strInfo = exc.Message;
				}

				Config.ResetGalleryFolder(this);
				//re-populate the album object
				OnDeleted(new GalleryObjectEventArgs(Child, DotNetNuke.Entities.Users.UserController.GetCurrentUserInfo().UserID));
			} else {
				strInfo = Localization.GetString("Insufficient_Delete_Permissions", GalleryConfig.SharedResourceFile);
			}
			return strInfo;

		}

		//William Severance - added to provide deletion of both the physical file and the record from the DNN files table.
		//Note: This is a modified version of the DNN FileSystemUtils.DeleteFile method which since DNN 4.8.3 does check of write permission
		//on the folder before proceeding to delete the file.

		public string DeleteFile(string strSourceFile, PortalSettings settings, bool ClearCache)
		{

			string retValue = "";
			try {
				string folderName = DotNetNuke.Common.Globals.GetSubFolderPath(strSourceFile, settings.PortalId);
				string fileName = GetFileName(strSourceFile);
				int PortalId = FileSystemUtils.GetFolderPortalId(settings);

				//try and delete the Insecure file
				DeleteFile(strSourceFile);

				//try and delete the Secure file
				DeleteFile(strSourceFile + Common.Globals.glbProtectedExtension);

				//Remove file from DataBase
				DotNetNuke.Services.FileSystem.FileController objFileController = new DotNetNuke.Services.FileSystem.FileController();
				DotNetNuke.Services.FileSystem.FolderController objFolders = new DotNetNuke.Services.FileSystem.FolderController();
				DotNetNuke.Services.FileSystem.FolderInfo objFolder = objFolders.GetFolder(PortalId, folderName, false);
				objFileController.DeleteFile(PortalId, fileName, objFolder.FolderID, ClearCache);

			} catch (Exception ex) {
				retValue = ex.Message;
			}
			return retValue;
		}

		//William Severance - added to provide deletion of both the physical folder and the record from the DNN folders table.
		//Note: This is a modified version of the DNN FileSystemUtils.DeleteFolder method which since DNN 4.8.4 does check of write permission
		//on the folder before proceeding to delete the folder and also does not handle recursive deletion of contained files and folders from the
		//DNN files and folders tables respectively.
		private string DeleteFolder(int PortalId, System.IO.DirectoryInfo folder, string folderName, bool Recursive)
		{
			string retValue = "";
			PortalSettings ps = PortalController.GetCurrentPortalSettings();
			System.IO.FileInfo[] files = folder.GetFiles();
			System.IO.DirectoryInfo[] directories = folder.GetDirectories();

			if (Recursive || (directories.Length == 0 & files.Length == 0)) {
				try {
					//Delete files in the folder
					foreach (System.IO.FileInfo fi in files) {
						DeleteFile(fi.FullName, ps, false);
					}
					foreach (System.IO.DirectoryInfo di in directories) {
						DeleteFolder(PortalId, di, FileSystemUtils.FormatFolderPath(di.FullName.Replace(ps.HomeDirectoryMapPath, "").Replace("\\", "/")), Recursive);
					}
					folder.Delete(false);
					//Remove Folderfrom DataBase
					DotNetNuke.Services.FileSystem.FolderController objFolderController = new DotNetNuke.Services.FileSystem.FolderController();
					objFolderController.DeleteFolder(PortalId, folderName);

				} catch (Exception ex) {
					retValue = ex.Message;
				}
			} else {
				retValue = Localization.GetString("Folder_Contains_Objects", GalleryConfig.SharedResourceFile);
			}
			return retValue;
		}

		private static string GetFileName(string filePath)
		{
			return System.IO.Path.GetFileName(filePath).Replace(Common.Globals.glbProtectedExtension, "");
		}

		private static void DeleteFile(string strFileName)
		{
			if (File.Exists(strFileName)) {
				File.SetAttributes(strFileName, FileAttributes.Normal);
				File.Delete(strFileName);
			}
		}

		public string DeleteChild(string ChildName)
		{
			IGalleryObjectInfo child = (IGalleryObjectInfo)List.Item(ChildName);
			return DeleteChild(child);
		}

		public void ChangeThumbnail(string ThumbName)
		{
			mThumbnail = ThumbName;
			mThumbnailURL = Utils.BuildPath(new string[2] {
				mThumbFolderURL,
				ThumbName
			}, "/", false, true);
		}

		//William Severance - added to provide of reset of album thumbnail to "folder.gif" when referenced
		//thumbnail has been deleted or is otherwise missing.
		public void ResetThumbnail()
		{
			mThumbnail = "folder.gif";
			mThumbnailURL = mGalleryConfig.GetImageURL(mThumbnail);
		}

		private void ClearList()
		{
			this.List.Clear();
			this.SortList.Clear();
			this.UnApprovedItems.Clear();
			this.MediaItems.Clear();
			this.mBrowsableItems.Clear();
			this.FlashItems.Clear();
			this.IconItems.Clear();
		}

		// Populates the current album data from file system, metadata, and DNN Folders/Files Table
		public void Populate(bool ReSync)
		{
			PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
			int _PortalId = _portalSettings.PortalId;

			// Populates the folder object with information about subs and files
			string[] gItems = null;
			int gCounter = 0;
			bool gUpdateXML = false;

			string gHierarchy = "";
			string gName = "";
			string gNameEscaped = "";

			string gURL = "";
			string gPath = "";
			long gSize = 0;

			// Thumbnail to be displayed in gallery container
			string gThumbnail = "";
			string gThumbnailURL = "";
			string gThumbnailPath = "";

			// Thumb folder of galleryFolder
			string gThumbFolderPath = "";
			string gThumbFolderURL = "";

			// Icon to be displayed in grid
			string gIcon = "";
			//Dim gIconPath As String = ""
			string gIconURL = "";

			// Source for album up/downloading
			string gSourceFolderPath = "";
			//Set this default value for file first
			string gSourceFolderURL = "";

			// SourceURL for file to be downloaded
			string gSourceURL = "";
			string gSourcePath = "";

			// Info stored in XML
			int gID = 0;
			string gTitle = "";
			string gDescription = "";
			string gCategories = "";
			string gAuthor = "";
			string gClient = "";
			string gLocation = "";
			DateTime gCreatedDate = Null.NullDate;
			DateTime gApprovedDate = Null.NullDate;
			int gWidth = 0;
			int gHeight = 0;
			double gScore = 0;

			string gScoreImage = null;
			string gScoreImageURL = null;

			int gAlbumOwnerID = 0;
			int gFileOwnerID = 0;

			Config.ItemType gFileType = default(Config.ItemType);
			bool gValidFile = false;

			//William Severance - Interface with DNN folder/file tables
			DotNetNuke.Services.FileSystem.FileController _objFileController = new DotNetNuke.Services.FileSystem.FileController();
			DotNetNuke.Services.FileSystem.FolderController _objFolderController = new DotNetNuke.Services.FileSystem.FolderController();
			int _FolderID = -1;

			// (in case someone decides to call this again without clearing the data first)
			this.ClearList();

			// Check existence of thumbs/source folder
			try {
				//Create the album's root folders/synch with folders table if not already existing
				_FolderID = CreateRootFolders(URL, true);
				if (Parent == null) {
					mID = _FolderID;
					//Top level folder
				}

			} catch (Exception ex) {
				DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
				throw;
			}

			// Check for metadata for this folder
			GalleryXML metaData = new GalleryXML(this.Path);

			// Add sub-folders here.
			gItems = Directory.GetDirectories(mPath);

			foreach (string gItem in gItems) {
				//Reset values
				gUpdateXML = false;
				gID = -1;

				// Check to make sure this folder is valid galleryfolder
				gName = System.IO.Path.GetFileName(gItem);

				if (!(gName.StartsWith("_") || (gName == GalleryConfig.ThumbFolder) || (gName == GalleryConfig.SourceFolder))) {
					gHierarchy = Utils.BuildPath(new string[2] {
						mGalleryHierarchy,
						gName
					}, "/");
					//<tam:note always keep virtual url format - without http://>
					gURL = Utils.BuildPath(new string[2] {
						mURL,
						gName
					}, "/", false, false);
					gPath = System.IO.Path.Combine(mPath, gName);
					gThumbnail = "folder.gif";
					gThumbnailURL = mGalleryConfig.GetImageURL(gThumbnail);
					gThumbFolderPath = System.IO.Path.Combine(gPath, mGalleryConfig.ThumbFolder);
					gThumbFolderURL = Utils.BuildPath(new string[3] {
						mURL,
						gName,
						mGalleryConfig.ThumbFolder
					}, "/", false, false);
					gIcon = "s_folder.gif";
					gIconURL = mGalleryConfig.GetImageURL(gIcon);
					gSourceFolderPath = System.IO.Path.Combine(gPath, GalleryConfig.SourceFolder);
					gSourceFolderURL = Utils.BuildPath(new string[3] {
						mURL,
						gName,
						mGalleryConfig.SourceFolder
					}, "/", false, false);
					gID = metaData.ID(gName);
					gDescription = metaData.Description(gName).Replace(Constants.vbCrLf, "<br />");
					gCategories = metaData.Categories(gName);
					gAuthor = metaData.Author(gName);
					gClient = metaData.Client(gName);
					gLocation = metaData.Location(gName);
					gAlbumOwnerID = metaData.OwnerID(gName);
					gCreatedDate = metaData.CreatedDate(gName);
					gApprovedDate = metaData.ApprovedDate(gName);

					// Does the metadata contain as its ID that of the corresponding DNN Folders table item?
					int gFolderID = CreateRootFolders(gURL, false);
					if (gID != gFolderID) {
						gID = gFolderID;
						gUpdateXML = true;
					}

					gTitle = metaData.Title(gName);
					if ((string.IsNullOrEmpty(gTitle)) || (gTitle.ToLower() == "untitled")) {
						gTitle = gName;
						gUpdateXML = true;
					}

					if (gCreatedDate == Null.NullDate) {
						gCreatedDate = System.IO.Directory.GetCreationTime(gPath);
						gUpdateXML = true;
					}

					if (gAlbumOwnerID <= 0) {
						gAlbumOwnerID = DotNetNuke.Modules.Gallery.Config.DefaultOwnerId;
						gUpdateXML = true;
					}

					if (gAlbumOwnerID != Utils.ValidUserID(gAlbumOwnerID)) {
						gAlbumOwnerID = _portalSettings.AdministratorId;
						gUpdateXML = true;
					}

					if (GalleryConfig.AutoApproval && gApprovedDate > DateTime.Now) {
						gApprovedDate = DateTime.Now;
						gUpdateXML = true;
					}

					// Update xmldata if there were any changes
					if (gUpdateXML) {
						GalleryXML.SaveMetaData(this.Path, gName, gID, gTitle, gDescription, gCategories, gAuthor, gLocation, gClient, gAlbumOwnerID,
						gCreatedDate, gApprovedDate, 0);
					}

					// If it's a valid galleryfolder add it to collection
					GalleryFolder newFolder = null;
					newFolder = new GalleryFolder(gCounter, gID, this, gHierarchy, gName, gURL, gPath, gThumbnail, gThumbnailURL, gThumbFolderPath,
					gThumbFolderURL, gIcon, gIconURL, gSourceFolderPath, gSourceFolderURL, gTitle, gDescription, gCategories, gAuthor, gClient,
					gLocation, gAlbumOwnerID, gApprovedDate, gCreatedDate, GalleryConfig);

					if ((newFolder.ApprovedDate <= DateTime.Now || GalleryConfig.AutoApproval)) {
						mSortList.Add(newFolder);
					} else {
						mUnApprovedItems.Add(gCounter);
					}

					mList.Add(gName, newFolder);
					gCounter += 1;
				}
			}

			// Add files here
			gItems = System.IO.Directory.GetFiles(mPath);

			//<tamttt:note since 2.0 file will be full url>
			foreach (string gItem in gItems) {
				// Reset all values
				gApprovedDate = Null.NullDate;
				gCreatedDate = Null.NullDate;
				gID = -1;
				gUpdateXML = false;

				string gExt = System.IO.Path.GetExtension(gItem).ToLower();
				//New IO.FileInfo(gItem).Extension

				// Check to make sure this file is valid gallery file & also approved
				gName = System.IO.Path.GetFileName(gItem);
				gNameEscaped = HttpUtility.UrlPathEncode(gName.Replace("%", "%25")).Replace(",", "%2C");

				if ((gItem != "_metadata.resources") && mGalleryConfig.IsValidMediaType(gExt)) {
					gURL = Utils.BuildPath(new string[2] {
						mURL,
						gNameEscaped
					}, "/", false, true);
					gPath = System.IO.Path.Combine(mPath, gName);
					gSize = new System.IO.FileInfo(gItem).Length;
					gID = metaData.ID(gName);
					gTitle = metaData.Title(gName);
					gDescription = metaData.Description(gName).Replace(Constants.vbCrLf, "<br/>");
					gCategories = metaData.Categories(gName);
					gAuthor = metaData.Author(gName);
					gClient = metaData.Client(gName);
					gLocation = metaData.Location(gName);
					gFileOwnerID = metaData.OwnerID(gName);
					gCreatedDate = metaData.CreatedDate(gName);
					gApprovedDate = metaData.ApprovedDate(gName);
					gScore = metaData.Score(gName);

					int intScore = (Convert.ToInt32(Math.Floor(gScore)) + Convert.ToInt32(Math.Ceiling(gScore))) * 5;
					gScoreImage = "stars_" + intScore.ToString() + ".gif";
					gScoreImageURL = mGalleryConfig.GetImageURL(gScoreImage);

					// If not title for file exists, create one
					if ((string.IsNullOrEmpty(gTitle)) || (gTitle.ToLower() == "untitled")) {
						// Use filename before the LAST "."
						gTitle = Strings.Left(gName, gName.LastIndexOf("."));
						gUpdateXML = true;
					}

					// Update created date if we don't have a date
					if (gCreatedDate == Null.NullDate) {
						gCreatedDate = System.IO.File.GetCreationTime(gPath);
						gUpdateXML = true;
					}

					// Does the file have an owner?
					if (gFileOwnerID <= 0) {
						// Assign parent owner to be file Owner
						gFileOwnerID = this.OwnerID;
						gUpdateXML = true;
					}

					// Host account will not be display in users list, should be changed to Admin
					if (gFileOwnerID != Utils.ValidUserID(gFileOwnerID)) {
						gFileOwnerID = _portalSettings.AdministratorId;
						gUpdateXML = true;
					}

					// Comment out 8/30 - The Approved Date can be ANY date and can
					// be reset to some date greater than today. This allows the User
					// to control which images are visible, based on calendar
					// HZassenhaus Issue GAL 8189

					//If GalleryConfig.AutoApproval AndAlso gApprovedDate > DateTime.Now Then
					//   gApprovedDate = DateTime.Now
					//   gUpdateXML = True
					//End If

					// Reset file validation value
					gValidFile = false;
					gWidth = 0;
					gHeight = 0;

					DotNetNuke.Services.FileSystem.FileInfo gFileInfo = _objFileController.GetFileById(gID, _PortalId);
					if (gFileInfo == null) {
						gID = -1;
						//Invalidate the ID
						gWidth = 0;
						gHeight = 0;
					} else {
						if (gFileInfo.FolderId == this.mID && gFileInfo.FileName == gName) {
							gWidth = gFileInfo.Width;
							gHeight = gFileInfo.Height;
						}
					}

					// Remember that we should add image first
					switch (mGalleryConfig.TypeFromExtension(gExt)) {
						case ItemType.Image:
							if ((gTitle.ToLower() != "icon") && (gTitle.ToLower() != "watermark") && (gApprovedDate <= DateTime.Now || GalleryConfig.AutoApproval)) {
								mBrowsableItems.Add(gCounter);
								// store reference to index
							}

							gFileType = Config.ItemType.Image;
							gValidFile = true;

							gIcon = "s_jpg.gif";
							gIconURL = mGalleryConfig.GetImageURL(gIcon);

							// Build source path
							gSourcePath = System.IO.Path.Combine(this.SourceFolderPath, gName);
							gSourceURL = Utils.BuildPath(new string[2] {
								this.SourceFolderURL,
								gNameEscaped
							}, "/", false, true);

							//Add to DNN Files Table
							if (gID == -1 | ReSync) {
								// Copy the original file to source folder
								if (mGalleryConfig.IsKeepSource) {
									if (!File.Exists(gSourcePath))
										File.Copy(gPath, gSourcePath);
									Utils.SaveDNNFile(gSourcePath, 0, 0, false, false);
									//Add to the DNN files table if we're not deleting the source
								}
								if (mGalleryConfig.IsFixedSize) {
									gID = GalleryGraphics.ResizeImage(gPath, gPath, mGalleryConfig.FixedWidth, mGalleryConfig.FixedHeight, mGalleryConfig.EncoderQuality);
								} else {
									gID = Utils.SaveDNNFile(gPath, gWidth, gHeight, false, false);
								}
								//Update the width and height - may have changed
								if (gID >= 0) {
									gFileInfo = _objFileController.GetFileById(gID, _PortalId);
									gWidth = gFileInfo.Width;
									gHeight = gFileInfo.Height;
								}
								gUpdateXML = true;
							}

							// Set this value for file download if user upload directly by FTP into display folder
							// Note that we change url only, source path keep to upload or Maintenance feature uses
							if (!File.Exists(gSourcePath)) {
								gSourceURL = gURL;
							}

							gThumbnail = gName;
							gThumbnailURL = Utils.BuildPath(new string[2] {
								this.ThumbFolderURL,
								gNameEscaped
							}, "/", false, true);

							gThumbnailPath = Utils.BuildPath(new string[2] {
								this.ThumbFolderPath,
								gThumbnail
							}, "\\", false, true);
							// Build the thumb on the fly if not exists
							try {
								if (!File.Exists(gThumbnailPath)) {
									// Only do resize with valid image, to prevent out of memmory exception
									if (gSize > 0) {
										GalleryGraphics.ResizeImage(gItem, gThumbnailPath, mGalleryConfig.MaximumThumbWidth, mGalleryConfig.MaximumThumbHeight, mGalleryConfig.EncoderQuality);
									} else {
										// Mark as invalid and delete invalid images
										gValidFile = false;
										FileSystemUtils.DeleteFile(gPath, _portalSettings);
										// Delete source
										if (System.IO.File.Exists(gSourcePath)) {
											FileSystemUtils.DeleteFile(gSourcePath, _portalSettings);
										}

										//William Severance - Remove folder thumbnail reference to thumbnail being deleted
										if (mThumbnail == gThumbnail)
											ResetThumbnail();

										// Also delete metadata related to this file
										GalleryXML.DeleteMetaData(mPath, gName);
									}
								} else {
									//Add/Update thumbnail file to DNN Files table
									if (ReSync)
										Utils.SaveDNNFile(gThumbnailPath, 0, 0, false, false);
								}
							} catch (Exception ex) {
								// Handle thumbnail exception 
								DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
								gThumbnailURL = gIconURL;
							}

							// Assign first image of the album to be album icon if not specified
							if (gValidFile && (gApprovedDate <= DateTime.Now || GalleryConfig.AutoApproval) && mThumbnail == "folder.gif") {
								mThumbnail = gThumbnail;
								mThumbnailURL = gThumbnailURL;
							}

							break;
						case ItemType.Movie:
							gIcon = "s_MediaPlayer.gif";
							gIconURL = mGalleryConfig.GetImageURL(gIcon);

							gThumbnail = "MediaPlayer.gif";
							gThumbnailURL = mGalleryConfig.GetImageURL(gThumbnail);

							gSourcePath = System.IO.Path.Combine(this.Path, gName);
							gSourceURL = Utils.BuildPath(new string[2] {
								this.URL,
								gNameEscaped
							}, "/", false, true);

							// Add/Update the source file to the DNN Files Table
							if (gID == -1 || ReSync) {
								gWidth = -1;
								gHeight = -1;
								gID = Utils.SaveDNNFile(gSourcePath, gWidth, gHeight, false, false);
								gUpdateXML = true;
							}

							// Changing handle method for media player file since 2.0
							mMediaItems.Add(gCounter);

							gFileType = Config.ItemType.Movie;
							gValidFile = true;

							break;
						// add flash types.
						case ItemType.Flash:
							gIcon = "s_Flash.gif";
							gIconURL = mGalleryConfig.GetImageURL(gIcon);

							gThumbnail = "Flash.gif";
							gThumbnailURL = mGalleryConfig.GetImageURL(gThumbnail);

							gSourcePath = System.IO.Path.Combine(this.Path, gName);
							gSourceURL = Utils.BuildPath(new string[2] {
								this.URL,
								gNameEscaped
							}, "/", false, true);

							// Add/Update the source file to the DNN Files Table
							if (gID == -1 || ReSync) {
								gWidth = -1;
								gHeight = -1;
								gID = Utils.SaveDNNFile(gSourcePath, gWidth, gHeight, false, false);
								gUpdateXML = true;
							}

							// Changing handle method for media player file since 2.0
							mFlashItems.Add(gCounter);

							gFileType = Config.ItemType.Flash;
							gValidFile = true;
							break;
					}

					if (gValidFile) {
						// Does the XML file need updating?
						if (gUpdateXML) {
							// Update it
							GalleryXML.SaveMetaData(this.Path, gName, gID, gTitle, gDescription, gCategories, gAuthor, gLocation, gClient, gFileOwnerID,
							gCreatedDate, gApprovedDate, gScore);
						}

						// Add the file and increment the counter
						GalleryFile newFile = null;
						newFile = new GalleryFile(gCounter, gID, this, gName, gURL, gPath, gThumbnail, gThumbnailURL, gThumbnailPath, gIcon,
						gIconURL, gScoreImage, gScoreImageURL, gSourcePath, gSourceURL, gSize, gFileType, gTitle, gDescription, gCategories,
						gAuthor, gClient, gLocation, gFileOwnerID, gApprovedDate, gCreatedDate, gWidth, gHeight, gScore, mGalleryConfig);
						mList.Add(gName, newFile);

						// Set icon image & watermark for parent folder
						string gTitleLowered = gTitle.ToLower();
						if (gTitleLowered.IndexOf("icon") > -1) {
							mThumbnail = gName;
							mThumbnailURL = gThumbnailURL;
							mIconItems.Add(newFile);
						} else if (gTitleLowered == "watermark") {
							mWatermarkImage = newFile;
							mIconItems.Add(newFile);
						} else {
							if ((newFile.ApprovedDate <= DateTime.Now || GalleryConfig.AutoApproval)) {
								mSortList.Add(newFile);
							} else {
								mUnApprovedItems.Add(gCounter);
							}
						}
						gCounter += 1;
					}
				}
			}

			// Get global watermark from parent album if it's not found in this album
			if (mGalleryConfig.UseWatermark && WatermarkImage == null && (Parent != null)) {
				if ((Parent.WatermarkImage != null))
					mWatermarkImage = Parent.WatermarkImage;
			}

			if (mFlashItems.Count > 0 && mIconItems.Count > 0) {
				PopulateFlashIcon();
			}
			if (mMediaItems.Count > 0 && mIconItems.Count > 0) {
				PopulateMediaIcon();
			}

			mItemInfo = GetItemInfo();

			// Sort the list by default settings first
			mSortList.Sort(new Comparer(new string[1] { Enum.GetName(typeof(Config.GallerySort), GalleryConfig.DefaultSort) }, GalleryConfig.DefaultSortDESC));
			// Set the flag so we don't call again
			mIsPopulated = true;
		}


		private void PopulateFlashIcon()
		{
			foreach (int intCounter in mFlashItems) {
				GalleryFile flashFile = null;
				flashFile = (GalleryFile)mList.Item(intCounter);

				foreach (GalleryFile iconFile in mIconItems) {
					if (iconFile.Title.ToLower() == flashFile.Title.ToLower() + "icon") {
						flashFile.Thumbnail = iconFile.Name;
						flashFile.ThumbnailURL = iconFile.URL;
					}
				}
			}
		}


		private void PopulateMediaIcon()
		{
			foreach (int intCounter in mMediaItems) {
				GalleryFile mediaFile = null;
				mediaFile = (GalleryFile)mList.Item(intCounter);

				foreach (GalleryFile iconFile in mIconItems) {
					if (iconFile.Title.ToLower() == mediaFile.Title.ToLower() + "icon") {
						mediaFile.Thumbnail = iconFile.Name;
						mediaFile.ThumbnailURL = iconFile.URL;
					}
				}
			}
		}

		private string GetBrowserURL()
		{
			System.Collections.Generic.List<string> @params = new System.Collections.Generic.List<string>();
			if (!string.IsNullOrEmpty(GalleryHierarchy))
				@params.Add("path=" + Utils.FriendlyURLEncode(GalleryHierarchy));
			return Common.Globals.NavigateURL(GalleryConfig.GalleryTabID, "", @params.ToArray());
		}

		private string GetSlideshowURL()
		{
			System.Collections.Generic.List<string> @params = new System.Collections.Generic.List<string>();
			@params.Add("mid=" + GalleryConfig.ModuleID.ToString());
			if (!string.IsNullOrEmpty(GalleryHierarchy))
				@params.Add("path=" + Utils.FriendlyURLEncode(GalleryHierarchy));
			return Common.Globals.NavigateURL(GalleryConfig.GalleryTabID, "Slideshow", @params.ToArray());
		}

		private string GetPopupSlideshowURL()
		{
			if (string.IsNullOrEmpty(mPopupSlideshowURL)) {
				StringBuilder sb = new StringBuilder();

				sb.Append(GalleryConfig.SourceDirectory());
				sb.Append("/GalleryPage.aspx?ctl=SlideShow");
				sb.Append("&tabid=");
				sb.Append(GalleryConfig.GalleryTabID.ToString());
				sb.Append("&mid=");
				sb.Append(GalleryConfig.ModuleID.ToString());
				if (!string.IsNullOrEmpty(GalleryHierarchy)) {
					sb.Append("&path=");
					sb.Append(Utils.FriendlyURLEncode(GalleryHierarchy));
				}
				mPopupSlideshowURL = Utils.BuildPopup(sb.ToString(), mGalleryConfig.FixedHeight + 160, mGalleryConfig.FixedWidth + 120);
			}
			return mPopupSlideshowURL;
		}

		private string GetEditURL()
		{
			System.Collections.Generic.List<string> @params = new System.Collections.Generic.List<string>();
			@params.Add("mid=" + GalleryConfig.ModuleID.ToString());
			if (!string.IsNullOrEmpty(GalleryHierarchy))
				@params.Add("path=" + Utils.FriendlyURLEncode(GalleryHierarchy));
			return Common.Globals.NavigateURL(GalleryConfig.GalleryTabID, "AlbumEdit", @params.ToArray());
		}

		private string GetMaintenanceURL()
		{
			System.Collections.Generic.List<string> @params = new System.Collections.Generic.List<string>();
			@params.Add("mid=" + GalleryConfig.ModuleID.ToString());
			if (!string.IsNullOrEmpty(GalleryHierarchy))
				@params.Add("path=" + Utils.FriendlyURLEncode(GalleryHierarchy));
			return Common.Globals.NavigateURL(GalleryConfig.GalleryTabID, "Maintenance", @params.ToArray());
		}

		private string GetAddSubAlbumURL()
		{
			System.Collections.Generic.List<string> @params = new System.Collections.Generic.List<string>();
			@params.Add("mid=" + GalleryConfig.ModuleID.ToString());
			if (!string.IsNullOrEmpty(GalleryHierarchy))
				@params.Add("path=" + Utils.FriendlyURLEncode(GalleryHierarchy));
			@params.Add("action=addfolder");
			return Common.Globals.NavigateURL(GalleryConfig.GalleryTabID, "AlbumEdit", @params.ToArray());
		}

		private string GetAddFileURL()
		{
			System.Collections.Generic.List<string> @params = new System.Collections.Generic.List<string>();
			@params.Add("mid=" + GalleryConfig.ModuleID.ToString());
			if (!string.IsNullOrEmpty(GalleryHierarchy))
				@params.Add("path=" + Utils.FriendlyURLEncode(GalleryHierarchy));
			@params.Add("action=addfile");
			return Common.Globals.NavigateURL(GalleryConfig.GalleryTabID, "AlbumEdit", @params.ToArray());
		}

		protected string GetItemInfo()
		{
			StringBuilder sb = new StringBuilder();

			// William Severance (9-10-2010) - Modified to use bitmapped TextDisplayOptions flags rather than iterating through string array for each test.

			if ((GalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.Name) != 0) {
				sb.Append("<span class=\"MediaName\">");
				sb.Append(Name);
				sb.Append("</span>");

				if ((GalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.Size) != 0) {
					sb.Append("<span class=\"MediaSize\">");
					string sizeInfo = " " + Localization.GetString("AlbumSizeInfo", GalleryConfig.SharedResourceFile);
					sizeInfo = sizeInfo.Replace("[ItemCount]", (Size - (IconItems.Count + UnApprovedItems.Count)).ToString());
					sb.Append(sizeInfo);
					sb.Append("</span>");
				}
				sb.Append("<br />");
			}

			if (((GalleryConfig.TextDisplayOptions & GalleryDisplayOption.Client) != 0) && !(Client.Length == 0)) {
				AppendLegendAndField(sb, "Client", Client);
			}

			if (((GalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.Location) != 0) && !(Location.Length == 0)) {
				AppendLegendAndField(sb, "Location", Location);
			}

			if ((GalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.CreatedDate) != 0) {
				AppendLegendAndField(sb, "CreatedDate", Utils.DateToText(CreatedDate));
			}

			if ((GalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.ApprovedDate) != 0) {
				AppendLegendAndField(sb, "ApprovedDate", Utils.DateToText(ApprovedDate));
			}

			return sb.ToString();

		}

		private void AppendLegendAndField(StringBuilder sb, string LegendKey, string FieldValue)
		{
			sb.Append("<span class=\"Legend\">");
			sb.Append(Localization.GetString(LegendKey, GalleryConfig.SharedResourceFile));
			sb.Append("</span>&nbsp;<span class=\"Field\">");
			sb.Append(FieldValue);
			sb.Append("</span>");
			sb.Append("<br/>");
		}

		//William Severance - added to raise events upon deletion/creation of gallery objects

		protected void OnDeleted(GalleryObjectEventArgs e)
		{
			if (GalleryObjectDeleted != null) {
				GalleryObjectDeleted(this, e);
			}
		}

		protected void OnCreated(GalleryObjectEventArgs e)
		{
			if (GalleryObjectCreated != null) {
				GalleryObjectCreated(this, e);
			}
		}

		#endregion

		#region "Public Shared Methods"
		// Validates existance of and if necessary sets up the root folders of a top level gallery or child album
		// rootURL should be a site relative not physical path.
		// If addSourceAndThumb is true, the _source and _thumb subfolders will also be added

		// Returns the FolderId of the root album folder.

		public static int CreateRootFolders(string albumURL, bool addSourceAndThumb)
		{
			int folderID = -1;
			try {
				PortalSettings ps = PortalController.GetCurrentPortalSettings();
				string rootPath = HttpContext.Current.Server.MapPath(albumURL);
				string homeDirectoryRelativeURL = FileSystemUtils.FormatFolderPath(albumURL.Substring(ps.HomeDirectory.Length));
				DotNetNuke.Services.FileSystem.FolderController fldController = new DotNetNuke.Services.FileSystem.FolderController();
				DotNetNuke.Services.FileSystem.FolderInfo folder = fldController.GetFolder(ps.PortalId, homeDirectoryRelativeURL, false);
				if (!System.IO.Directory.Exists(rootPath) || folder == null) {
					string parentPath = ps.HomeDirectoryMapPath;
					FileSystemUtils.AddFolder(ps, parentPath, homeDirectoryRelativeURL);
					folderID = fldController.GetFolder(ps.PortalId, homeDirectoryRelativeURL, true).FolderID;
				} else {
					folderID = folder.FolderID;
				}
				if (addSourceAndThumb) {
					string thumbPath = FileSystemUtils.FormatFolderPath(Config.DefaultThumbFolder);
					string sourcePath = FileSystemUtils.FormatFolderPath(Config.DefaultSourceFolder);
					if (fldController.GetFolder(ps.PortalId, homeDirectoryRelativeURL + thumbPath, true) == null) {
						FileSystemUtils.AddFolder(ps, rootPath, thumbPath);
					}
					if (fldController.GetFolder(ps.PortalId, homeDirectoryRelativeURL + sourcePath, true) == null) {
						FileSystemUtils.AddFolder(ps, rootPath, sourcePath);
					}
				}
			} catch (Exception ex) {
				DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
			}
			return folderID;
		}

		public static int CreateRootFolders(string albumURL)
		{
			return CreateRootFolders(albumURL, false);
		}
		#endregion
	}

}
