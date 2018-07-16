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
using DotNetNuke.Security.Roles;
using DotNetNuke.Entities.Portals;
using static DotNetNuke.Common.Globals;
using static DotNetNuke.Modules.Gallery.Utils;
using DotNetNuke.Entities.Users;

namespace DotNetNuke.Modules.Gallery
{

	public enum ObjectClass : int
	{
		None = 0,
		DNNUser = 101,
		DNNRole = 102
	}

	public class Config
	{

		#region "Enumerators"
		[Flags()]
		public enum GalleryDisplayOption
		{
			Title = 1,
			Name = 2,
			Size = 4,
			Client = 8,
			Author = 16,
			Location = 32,
			Description = 64,
			CreatedDate = 128,
			ApprovedDate = 256
		}

		public enum ItemType : int
		{
			Folder = 1,
			Image,
			Movie,
			Flash,
			Zip
		}

		public enum GallerySort : int
		{
			Name = 1,
			Size,
			Title,
			Author,
			Location,
			Score,
			OwnerID,
			CreatedDate,
			ApprovedDate
		}

		public enum GalleryView : int
		{
			CardView = 0,
			ListView,
			Standard
		}

		#endregion

		#region "Private Variables"
		private const string GalleryConfigCacheKeyPrefix = "GalleryConfig_{0}";
			//minutes
		private const int GalleryConfigCacheTimeOut = 5;

		// Holds a reference to the root gallery folder
		private string mSourceDirectory = DefaultSourceDirectory;
		private string mSharedResourceFile = DefaultSharedResourceFile;
		private GalleryFolder mRootFolder;
			// Root filesystem Path
		private string mRootPath;
		private GalleryFolder mInitialFolder;
		private string mSourcePath;
		private string mThumbPath;
		private string mRootURL;
		private string mTheme;
		private string mThumbFolder = DefaultThumbFolder;
		private string mSourceFolder = DefaultSourceFolder;
		private string mTempFolder = DefaultTempFolder;
		private string mGalleryTitle;

		private string mGalleryDescription;
		// Number of pics per Gallery_Row on each page view & Number of rows per page view
		private int mStripWidth;
		private int mStripHeight;
		// Acceptable file types
		//<daniel-- DefaultFileExtensions now gets the image file extensions from the core host extensions>
		private string mFileExtensions;
		private string mMovieExtensions;
		private string mFlashExtensions;
		private string mCategoryValues;

		private Config.GalleryDisplayOption mTextDisplayOptions;
		private ArrayList mFileTypes = new ArrayList();
		private ArrayList mMovieTypes = new ArrayList();
		private ArrayList mFlashTypes = new ArrayList();
		private ArrayList mCategories = new ArrayList();

		private ArrayList mTextDisplay = new ArrayList();
		private bool mBuildCacheonStart;
		private bool mFixedSize = true;
		private int mFixedWidth;
		private int mFixedHeight;
		private long mEncoderQuality = DefaultEncoderQuality;
		private bool mKeepSource = DefaultIsKeepSource;
		private int mSlideshowSpeed;
		private bool mIsPrivate;
		private int mGalleryTabID;
		private int mMaxFileSize;
		private int mQuota;

		private int mMaxPendingUploadsSize;
		// Thumbnail creation
		private int mMaxThumbWidth;

		private int mMaxThumbHeight;
		// Created Date
		private DateTime mCreatedDate;

		private bool mAutoApproval;
		// ModuleID and TabModuleID for these settings
		private int mModuleID;

		private int mTabModuleID;
		// IsValidPath return whether the root gallery path is valid for this machine

		private bool mIsValidPath = true;
		// Owner - for User Gallery feature

		private int mOwnerID;
		// Gallery view feature settings
		private GalleryView mDefaultView;
		private bool mAllowChangeView;
		private bool mUseWatermark;
		private bool mMultiLevelMenu;
		private bool mAllowSlideshow;
		private string mInitialGalleryHierarchy;
		private bool mAllowPopup;
		private bool mAllowVoting;

		private bool mAllowExif;
		// Sort feature settings
		private string mSortPropertyValues;
		private ArrayList mSortProperties = new ArrayList();
		private GallerySort mDefaultSort;

		private bool mDefaultSortDESC;
		// AllowDownload feature
		private bool mAllowDownload = true;

		private string mDownloadRoles = "";
		//Display Option - Removed as not used
		//Private mCellPadding As Integer = DefaultCellPadding
		#endregion

		#region "Default Properties"
		public static string DefaultSourceDirectory {
			get { return Common.Globals.ApplicationPath + "/DesktopModules/Gallery"; }
		}

		public static string DefaultSharedResourceFile {
			get { return string.Join("/", new string[] {DefaultSourceDirectory,Localization.LocalResourceDirectory,Localization.LocalSharedResourceFile}); }
		}

		public static string DefaultRootURL {
			get { return "Gallery/"; }
		}

		public static string DefaultRootPath {
			get { return "Gallery\\"; }
		}

		public static string DefaultTheme {
			get { return "DNNSimple"; }
		}

		public static string DefaultThumbFolder {
			get { return "_thumbs"; }
		}

		public static string DefaultSourceFolder {
			get { return "_source"; }
		}

		public static bool DefaultIsKeepSource {
			get { return true; }
		}

		public static string DefaultTempFolder {
			get { return "_temp"; }
		}

		public static string DefaultGalleryTitle {
			get { return "Gallery"; }
		}

		public static string DefaultGalleryDescription {
			get { return "Gallery"; }
		}

		public static bool DefaultBuildCacheOnStart {
			get { return true; }
		}

		public static int DefaultStripWidth {
			get { return 3; }
		}

		public static int DefaultStripHeight {
			get { return 2; }
		}

		public static int DefaultMaxThumbWidth {
			get { return 100; }
		}

		public static int DefaultMaxThumbHeight {
			get { return 100; }
		}

		//<daniel-- removed to be replaced by the core host extensions11/22/05>
		public static string DefaultFileExtensions {
			get { return permittedFileTypes(".jpg;.jpeg;.gif;.bmp;.png;.tif"); }
		}

		public static string DefaultMovieExtensions {
			get { return permittedFileTypes(".mov;.wmv;.wma;.mpg;.avi;.asf;.asx;.mpe;.mpeg;.mid;.midi;.wav;.aiff;.mp3;.mp4;.m4v"); }
		}

		public static string DefaultFlashExtensions {
			get { return permittedFileTypes(".swf"); }
		}

		public static string DefaultCategoryValues {
			get { return "Image;Movie;Music;Flash"; }
		}

		public static string DefaultTextDisplayValues {
			get { return "Name;Description;Size;Title"; }
		}

		public static string DefaultSortPropertyValues {
			get { return "Name;Title;Size;Author;CreatedDate"; }
		}

		public static int DefaultMaxFileSize {
			get { return 1000; }
		}

		public static int DefaultQuota {
				//0 means no quota
			get { return 0; }
		}

		public static int DefaultMaxPendingUploadsSize {
				//0 means no maximum.
			get { return 0; }
		}

		public static int DefaultFixedWidth {
			get { return 500; }
		}

		public static int DefaultFixedHeight {
			get { return 500; }
		}

		public static long DefaultEncoderQuality {
			get { return 80L; }
		}

		public static int DefaultSlideshowSpeed {
			get { return 3000; }
		}

		public static bool DefaultIsPrivate {
			get { return false; }
		}

		public static int DefaultOwnerId {
				//Signifies not specified
				//Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
				//Return _portalSettings.AdministratorId
			get { return -1; }
		}

		//Public Shared ReadOnly Property DefaultCellPadding() As Integer
		//  Get
		//    Return 15
		//  End Get
		//End Property

		public static bool DefaultMultiLevelMenu {
			get { return true; }
		}

		public static bool DefaultAllowSlideShow {
			get { return true; }
		}

		public static string DefaultInitialGalleryHierarchy {
			get { return ""; }
		}

		public static bool DefaultAllowPopup {
			get { return true; }
		}

		public static bool DefaultAllowVoting {
			get { return true; }
		}

		public static bool DefaultAllowExif {
			get { return true; }
		}

		public static bool DefaultAllowDownload {
			get { return true; }
		}

		public static string DefaultDownloadRoles {
			get { return string.Empty; }
		}

		public static bool DefaultAutoApproval {
			get { return true; }
		}

		public static GalleryView DefaultDefaultView {
			get { return GalleryView.Standard; }
		}

		public static bool DefaultAllowChangeView {
			get { return true; }
		}

		public static bool DefaultDefaultSortDESC {
			get { return false; }
		}

		public static GallerySort DefaultDefaultSort {
			get { return GallerySort.Title; }
		}

		public static bool DefaultDisplayWhatsNew {
			get { return true; }
		}

		public static bool DefaultUseWatermark {
			get { return true; }
		}
		#endregion

		#region "Shared Methods"

		// WES - 9/6/2011 - Refactored to use new DataCache callback pattern available in DNN 5

		public static DotNetNuke.Modules.Gallery.Config GetGalleryConfig(int ModuleID)
		{
			DotNetNuke.Modules.Gallery.Config config = null;
			string key = GetConfigCacheKey(ModuleID);
			CacheItemArgs cacheItemArgs = new CacheItemArgs(key, GalleryConfigCacheTimeOut, CacheItemPriority.Normal, ModuleID);
			return DataCache.GetCachedData<DotNetNuke.Modules.Gallery.Config>(cacheItemArgs, GalleryConfigExpiredCallback);
		}

		private static object GalleryConfigExpiredCallback(CacheItemArgs args)
		{
			int ModuleID = (int)args.Params[0];
			return new DotNetNuke.Modules.Gallery.Config(ModuleID);
		}

		//William Severance - changed sub to function to facilitate return of new GalleryConfig when configuration
		//settings control has caused change of configuration requiring update of folders, files, etc.

		public static DotNetNuke.Modules.Gallery.Config ResetGalleryConfig(int ModuleID)
		{

			string key = GetConfigCacheKey(ModuleID);
			DataCache.RemoveCache(key);
			// Call this to force storage of settings
			return GetGalleryConfig(ModuleID);
		}

		public static void ResetGalleryFolder(GalleryFolder Album)
		{
			try {
				Album.List.Clear();
				Album.IsPopulated = false;
				// Repopulate this album
				Utils.PopulateAllFolders(Album, true);
			} catch (Exception ex) {
				ResetGalleryConfig(Album.GalleryConfig.ModuleID);
			}

		}
		#endregion

		#region "Public Properties"
		public GalleryFolder RootFolder {
			get { return mRootFolder; }
		}

		public string SourceDirectory(bool IncludeHost = false) {
			if (IncludeHost) {
				return Common.Globals.AddHTTP(HttpContext.Current.Request.ServerVariables["HTTP_HOST"]) + mSourceDirectory;
			} else {
				return mSourceDirectory;
			}
		}

		public string SharedResourceFile {
			get { return mSharedResourceFile; }
		}

		public int ItemsPerStrip {
//Return CType((mStripWidth * mStripHeight), Integer)  'William Severance - no need for conversion!
			get { return mStripWidth * mStripHeight; }
		}

		public string RootPath {
			get { return mRootPath; }
		}

		public string RootURL {
			get { return mRootURL; }
		}

		public string ImageFolder(bool IncludeHost = false) {
			string folder = SourceDirectory() + "/Themes/" + Theme + "/Images/";
			if (IncludeHost) {
				return Utils.AddHost(folder);
			} else {
				return folder;
			}
		}

		public string Theme {
			get { return mTheme; }
		}

		public string Css {
			get { return SourceDirectory() + "/Themes/" + Theme + "/" + Theme.ToLower() + ".css"; }
		}

		public string GetImageURL(string Image, bool IncludeHost = false)
		{
			if (IncludeHost) {
				return ImageFolder(true) + Image;
			} else {
				return ImageFolder() + Image;
			}

		}

		public string ThumbFolder {
			get { return mThumbFolder; }
		}

		public string SourceFolder {
			get { return mSourceFolder; }
		}

		public string TempFolder {
			get { return mTempFolder; }
		}

		public string GalleryTitle {
			get { return mGalleryTitle; }
		}

		public string GalleryDescription {
			get { return mGalleryDescription; }
		}

		public bool BuildCacheonStart {
			get { return mBuildCacheonStart; }
		}

		public int StripWidth {
			get { return mStripWidth; }
		}

		public int StripHeight {
			get { return mStripHeight; }
		}

		public int MaximumThumbWidth {
			get { return mMaxThumbWidth; }
		}

		public int MaximumThumbHeight {
			get { return mMaxThumbHeight; }
		}

		public Config.ItemType TypeFromExtension(string FileExtension) {
			if (mFileTypes.Contains((FileExtension).ToLower()))
				return (ItemType)Config.ItemType.Image;
			if (mMovieTypes.Contains((FileExtension).ToLower()))
				return (ItemType)Config.ItemType.Movie;
			if (mFlashTypes.Contains((FileExtension).ToLower()))
				return (ItemType)Config.ItemType.Flash;
            // Ogiginal code: not all code paths return a value, I suppose returning zip would force a download?
            return (ItemType)Config.ItemType.Zip;
		}

		public bool IsValidImageType(string FileExtension) {
			return (mFileTypes.Contains((FileExtension).ToLower()));
		}

		public bool IsValidMovieType(string FileExtension)
        {
			return mMovieTypes.Contains((FileExtension).ToLower());
		}

		public bool IsValidFlashType(string FileExtension)
        {
			return mFlashTypes.Contains((FileExtension).ToLower());
		}

		public bool IsValidMediaType(string FileExtension)
        {
			return (IsValidImageType(FileExtension) || IsValidMovieType(FileExtension) || IsValidFlashType(FileExtension));
		}

		public bool IsValidFileType(string FileExtension)
        {
			return (IsValidMediaType(FileExtension) || (FileExtension.ToLower() == ".zip"));
		}

		public string FileExtensions {
			get { return mFileExtensions; }
		}

		public string MovieExtensions {
			get { return mMovieExtensions; }
		}

		public ArrayList MovieTypes {
			get { return this.mMovieTypes; }
		}

		public string CategoryValues {
			get { return mCategoryValues; }
		}

		public ArrayList Categories {
			get { return mCategories; }
		}

		public Config.GalleryDisplayOption TextDisplayOptions {
			get { return mTextDisplayOptions; }
		}

		public ArrayList SortProperties {
			get { return mSortProperties; }
		}

		public bool AutoApproval {
			get { return mAutoApproval; }
		}

		public bool IsValidPath {
			get { return mIsValidPath; }
		}

		public int ModuleID {
			get { return mModuleID; }
		}

		public bool IsFixedSize {
			get { return mFixedSize; }
		}

		public int FixedWidth {
			get { return mFixedWidth; }
		}

		public int MaxFileSize {
			get { return mMaxFileSize; }
		}

		public int Quota {
			get { return mQuota; }
		}

		public int MaxPendingUploadsSize {
			get { return mMaxPendingUploadsSize; }
		}

		public int FixedHeight {
			get { return mFixedHeight; }
		}

		public long EncoderQuality {
			get { return mEncoderQuality; }
		}

		public bool IsKeepSource {
			get { return mKeepSource; }
		}

		public int SlideshowSpeed {
			get { return mSlideshowSpeed; }
		}

		public bool IsPrivate {
			get { return mIsPrivate; }
		}

		public int GalleryTabID {
			get { return mGalleryTabID; }
		}

		public int OwnerID {
			get { return mOwnerID; }
		}

		public DateTime CreatedDate {
			get { return mCreatedDate; }
		}

		public bool MultiLevelMenu {
			get { return mMultiLevelMenu; }
		}

		public bool AllowSlideshow {
			get { return mAllowSlideshow; }
		}

		public string InitialGalleryHierarchy {
			get { return mInitialGalleryHierarchy; }
		}

		public GalleryFolder InitialFolder {
			get { return mInitialFolder; }
		}

		public bool AllowPopup {
			get { return mAllowPopup; }
		}

		public bool AllowVoting {
			get { return mAllowVoting; }
		}

		public bool AllowExif {
			get { return mAllowExif; }
		}

		public bool AllowDownload {
			get { return mAllowDownload; }
		}

		public string DownloadRoles {
			get { return mDownloadRoles; }
		}

		public bool HasDownloadPermission {
			get {
				// JIMJ add a loop to check the roles by name instead of by id
				foreach (string Role in DownloadRoles.Split(';')) {

					if (!(Role.Length == 0)) {
						string RoleName = null;
                        if (Convert.ToInt32((Role)) == Convert.ToInt32((Common.Globals.glbRoleAllUsers)))
                            {
                            // Global role
                            RoleName = Common.Globals.glbRoleAllUsersName;
                        }
                        else { 							
							// All other roles
							PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
							RoleController ctlRole = new RoleController();
							RoleInfo objRole = ctlRole.GetRole(Int16.Parse(Role), _portalSettings.PortalId);

							RoleName = objRole.RoleName;
						}

						if (PortalSecurity.IsInRole(RoleName)) {
							return true;
						}
					}
				}

				// Not found
				return false;

			}
		}

		public GalleryView DefaultView {
			get { return mDefaultView; }
		}

		public bool AllowChangeView {
			get { return mAllowChangeView; }
		}

		public GallerySort DefaultSort {
			get { return mDefaultSort; }
		}

		public bool DefaultSortDESC {
			get { return mDefaultSortDESC; }
		}

		public bool UseWatermark {
			get { return mUseWatermark; }
		}

		#endregion

		#region "Public Methods, Functions"


		static Config()
		{
		}

		private Config(int ModuleId) : this(ModuleId, false)
		{
		}

		private Config(int ModuleId, bool ReSync)
		{
			HttpContext _context = HttpContext.Current;
			string fileExtension = null;
			string catValue = null;
			string textValue = null;
			string sortProperty = null;

			// Grab the moduleID
			mModuleID = ModuleId;

			// Save the TabId
			PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
			mGalleryTabID = _portalSettings.ActiveTab.TabID;

			// Grab settings from the database
			DotNetNuke.Entities.Modules.ModuleController mc = new DotNetNuke.Entities.Modules.ModuleController();
			Hashtable settings = mc.GetModuleSettings(ModuleId);

			if (settings.Count == 0) {
				// PreConfigure the Module
				int mUserID = 0;
				if (_context.Request.IsAuthenticated) {
					mUserID = UserController.GetCurrentUserInfo().UserID;

					GalleryPreConfig.PreConfig(ModuleId, mUserID);

					Hashtable newsettings = mc.GetModuleSettings(ModuleId);

					if (newsettings.Count > 0) {
						settings = newsettings;
					}
				}
			}

			// Now iterate through all the values and init local variables
			mRootURL = _portalSettings.HomeDirectory + GetValue(settings["RootURL"], DefaultRootURL + ModuleId.ToString() + "/");
			mGalleryTitle = GetValue(settings["GalleryTitle"], DefaultGalleryTitle);

			try {
				mRootPath = HttpContext.Current.Request.MapPath(mRootURL);
				mSourcePath = System.IO.Path.Combine(mRootPath, DefaultSourceFolder);
				mThumbPath = System.IO.Path.Combine(mRootPath, DefaultThumbFolder);
				mIsValidPath = GalleryFolder.CreateRootFolders(mRootURL) != -1;

			} catch (Exception) {
			}

			mTheme = GetValue(settings["Theme"], DefaultTheme);
			mGalleryDescription = GetValue(settings["GalleryDescription"], DefaultGalleryDescription);
			mBuildCacheonStart = GetValue(settings["BuildCacheOnStart"], DefaultBuildCacheOnStart);
			mStripWidth = GetValue(settings["StripWidth"], DefaultStripWidth);
			mStripHeight = GetValue(settings["StripHeight"], DefaultStripHeight);
			mMaxThumbWidth = GetValue(settings["MaxThumbWidth"], DefaultMaxThumbWidth);
			mMaxThumbHeight = GetValue(settings["MaxThumbHeight"], DefaultMaxThumbHeight);

			mQuota = GetValue(settings["Quota"], DefaultQuota);
			mMaxFileSize = GetValue(settings["MaxFileSize"], DefaultMaxFileSize);
			mMaxPendingUploadsSize = GetValue(settings["MaxPendingUploadsSize"], DefaultMaxPendingUploadsSize);
			mFixedWidth = GetValue(settings["FixedWidth"], DefaultFixedWidth);
			mFixedHeight = GetValue(settings["FixedHeight"], DefaultFixedHeight);
			mEncoderQuality = GetValue(settings["EncoderQuality"], DefaultEncoderQuality);
			mSlideshowSpeed = GetValue(settings["SlideshowSpeed"], DefaultSlideshowSpeed);
			mIsPrivate = GetValue(settings["IsPrivate"], DefaultIsPrivate);
			mMultiLevelMenu = GetValue(settings["MultiLevelMenu"], DefaultMultiLevelMenu);
			mAllowSlideshow = GetValue(settings["AllowSlideshow"], DefaultAllowSlideShow);
			mAllowPopup = GetValue(settings["AllowPopup"], DefaultAllowPopup);
			mAllowVoting = GetValue(settings["AllowVoting"], DefaultAllowVoting);
			mAllowExif = GetValue(settings["AllowExif"], DefaultAllowExif);

			mAllowDownload = GetValue(settings["AllowDownload"], DefaultAllowDownload);
			mDownloadRoles = GetValue(settings["DownloadRoles"], DefaultDownloadRoles);

			mAutoApproval = GetValue(settings["AutoApproval"], DefaultAutoApproval);
			mDefaultSortDESC = GetValue(settings["DefaultSortDESC"], DefaultDefaultSortDESC);
			mDefaultSort = GetValue(settings["DefaultSort"], DefaultDefaultSort);

			// Gallery view settings
			mDefaultView = GetValue(settings["DefaultView"], DefaultDefaultView);
			mAllowChangeView = GetValue(settings["AllowChangeView"], DefaultAllowChangeView);

			mUseWatermark = GetValue(settings["UseWatermark"], DefaultUseWatermark);

			mOwnerID = GetValue(settings["OwnerID"], DefaultOwnerId);
			mCreatedDate = GetValue(settings["CreatedDate"], DateTime.Today);

			mFileExtensions = DefaultFileExtensions;

			//' Iterate through the file extensions and create the collection
			foreach (string fileExtension_loopVariable in mFileExtensions.Split(';')) {
                fileExtension = fileExtension_loopVariable;
                mFileTypes.Add((fileExtension).ToLower());
			}

			mMovieExtensions = DefaultMovieExtensions;

			// Iterate through the movie extensions and create the collection
			foreach (string fileExtension_loopVariable in mMovieExtensions.Split(';')) {
				fileExtension = fileExtension_loopVariable;
				mMovieTypes.Add((fileExtension).ToLower());
			}

			//<daniel s-- removed to be replaced by the core host extensions11/22/05>
			//William Severance - restored line as it need to be there for flash swf extensions
			mFlashExtensions = DefaultFlashExtensions;

			foreach (string fileExtension_loopVariable in mFlashExtensions.Split(';')) {
				fileExtension = fileExtension_loopVariable;
				mFlashTypes.Add((fileExtension).ToLower());
			}

			mCategoryValues = GetValue(settings["CategoryValues"], DefaultCategoryValues);
			foreach (string catValue_loopVariable in mCategoryValues.Split(';')) {
				catValue = catValue_loopVariable;
				//Added Localization for Checkboxes by M. Schlomann
				mCategories.Add(GetLocalization(catValue));
			}

			// William Severance (9-10-2010) - added additional display options and changed option persistance from array of
			// option names to bitmapped flags for each option. See Config.GalleryDispayOption for bit definitions.

			mTextDisplayOptions = 0;
			//Note: mTextDisplayOptions contains bitmapped flags for each option
			Config.GalleryDisplayOption optionValue = default(Config.GalleryDisplayOption);
			foreach (string textValue_loopVariable in GetValue(settings["TextDisplayValues"], "").Split(';')) {
				textValue = textValue_loopVariable;
				if (!string.IsNullOrEmpty(textValue)) {
					try {
						optionValue = (Config.GalleryDisplayOption)Enum.Parse(typeof(Config.GalleryDisplayOption), textValue);
						mTextDisplayOptions = mTextDisplayOptions | optionValue;
					} catch (Exception) {
						//Eat the exception - unrecognized option name in string.
					}
				}
			}

			mSortPropertyValues = GetValue(settings["SortPropertyValues"], DefaultSortPropertyValues);
			foreach (string sortProperty_loopVariable in mSortPropertyValues.Split(';')) {
				sortProperty = sortProperty_loopVariable;
				//mSortProperties.Add(LCase(sortProperty))
				// <tamttt:note sort properties to be display in GUI so we keep original case>
				mSortProperties.Add(sortProperty);
			}

			string mSourceURL = Utils.BuildPath(new string[2] {
				mRootURL,
				mSourceFolder
			}, "/", false, false);
			string mThumbURL = Utils.BuildPath(new string[2] {
				mRootURL,
				mThumbFolder
			}, "/", false, false);

			// Only run if we've got a valid filesystem path
			if (mIsValidPath) {
				// Initialize the root folder object
				//Dim rd As New Random
				int ID = -1;
				//ID = rd.Next
				mRootFolder = new GalleryFolder(0, ID, null, "", mGalleryTitle, mRootURL, mRootPath, "", "", mThumbPath,
				mThumbURL, "", "", mSourcePath, mSourceURL, mGalleryTitle, mGalleryDescription, mCategoryValues, "", "",
				"", mOwnerID, DateTime.MinValue, DateTime.MinValue, this);
				// Build the cache at once if required.
				if (BuildCacheonStart) {
					// Populate all folders and sub-folders of the root
					Utils.PopulateAllFolders(mRootFolder, ReSync);
				} else {
					// Populate only the root folder and immediate sub-folders (if any) of the root.
					Utils.PopulateAllFolders(mRootFolder, 2, ReSync);
				}
				mInitialGalleryHierarchy = GetValue(settings["InitialGalleryHierarchy"], DefaultInitialGalleryHierarchy);
				mInitialFolder = Utils.GetRootRelativeFolder(mRootFolder, mInitialGalleryHierarchy);
			}
		}

		#endregion

		#region "Private Functions"
		// Localization for Checkboxes added by M. Schlomann
		public static string GetLocalization(string catValue, string Setting = "")
		{
			if (catValue == string.Empty)
				return string.Empty;
			//Dim LocalResourceFile As String = System.Web.HttpContext.Current.Request.ApplicationPath & "/DesktopModules/Gallery/" & Services.Localization.Localization.LocalResourceDirectory & "/SharedResources.resx"
			if ((Setting != string.Empty))
				catValue = string.Concat(catValue, "_", Setting);
			//Return Localization.GetString(catValue & "Categories", LocalResourceFile)
			//William Severance - modified to return catValue if no localization provided in SharedResources.resx
			string localizedCatValue = Localization.GetString(catValue + "Categories", DefaultSharedResourceFile);
			if (string.IsNullOrEmpty(localizedCatValue)) {
				return catValue;
			} else {
				return localizedCatValue;
			}
		}

		//<daniel added to use core extensions>
		private static string permittedFileTypes(string Extensions)
		{

			string FileExtension = null;

			// break up the images into an array
			string coreImageTypesArray = (Extensions.Split(';').Length + 1).ToString();
			long Index = 0;

			// get  dnn core file extensions
			string dnnPermittedFileExtensions = DotNetNuke.Entities.Host.Host.FileExtensions.ToUpper();

			// step thru each and if they are permitted then keep it in the list
			// otherwise remove it
			string newStandardExtensions = "";

			foreach (string FileExtension_loopVariable in Extensions.Split(';')) {
				FileExtension = FileExtension_loopVariable;
				if (Strings.InStr(1, dnnPermittedFileExtensions, FileExtension.Remove(0, 1).ToUpper()) != 0) {
					// build a new list
					if (newStandardExtensions.Length > 0) {
						newStandardExtensions += ";";
					}
					newStandardExtensions += FileExtension;
				}
			}

			return newStandardExtensions;
		}

		//William Severance - Replaced GetValue with .Net generics method

		private T GetValue<T>(object Input, T DefaultValue)
		{
			if (Input == null || ((Input is System.String) && ((string)Input == string.Empty))) {
				return DefaultValue;
			} else {
				if (DefaultValue is System.Enum) {
					try {
						return (T)Enum.Parse(typeof(T), Convert.ToString(Input));
					} catch (ArgumentException ex) {
						return DefaultValue;
					}
				} else if (DefaultValue is System.DateTime) {
					object objDateTime = null;
					try {
						objDateTime = DateTime.Parse(Convert.ToString(Input));
					} catch (FormatException ex) {
						DateTime dt = default(DateTime);
						if (!DateTime.TryParse(Convert.ToString(Input), System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None, out dt)) {
							dt = DateTime.Now;
						}
						objDateTime = dt;
					}
					return (T)objDateTime;
				} else {
					return (T)Input;
				}
			}
		}

		private static string GetConfigCacheKey(int ModuleID)
		{
			return string.Format(GalleryConfigCacheKeyPrefix, ModuleID);
		}

		#endregion

	}

}
