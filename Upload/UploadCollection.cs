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
using static DotNetNuke.Modules.Gallery.Utils;
using ICSharpCode.SharpZipLib.Zip;

// Base objects for gallery content
// William Severance Converted to ASP.Net 2 Generic.List (Of FileToUpload)
namespace DotNetNuke.Modules.Gallery
{

	public class GalleryUploadCollection : System.Collections.Generic.List<FileToUpload>
	{

			//Minutes
		private const int SlidingCacheDuration = 10;

		private GalleryFolder mAlbum;
		//Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config
		private int mUserID;
		private string mFileListCacheKey;
		private string mErrMessage;
		private long mGallerySpaceUsed;
		private long mSize;

		private long mSpaceAvailable;
		public static GalleryUploadCollection GetList(GalleryFolder Album, int ModuleID, string FileListCacheKey)
		{

			//Refactored to utilize DataCache.GetCachedData method available in DNN 5.x
			CacheItemArgs args = new CacheItemArgs(FileListCacheKey, SlidingCacheDuration, CacheItemPriority.AboveNormal, Album, ModuleID);
			GalleryUploadCollection pendingUploads = DataCache.GetCachedData<GalleryUploadCollection>(args, GalleryUploadCollectionCacheExpiredCallback);
			pendingUploads.Album = Album;
			//reset album in case album changed but cached object is still valid
			pendingUploads.RefreshSpaceConstraints();
			return pendingUploads;
		}

		private static object GalleryUploadCollectionCacheExpiredCallback(CacheItemArgs args)
		{
			GalleryFolder album = (GalleryFolder)args.Params[0];
			int moduleID = (int)args.Params[1];
			return new GalleryUploadCollection(album, moduleID, args.CacheKey);
		}

		//William Severance - Modified to take supplied cache key
		public static void ResetList(string FileListCacheKey)
		{
			DataCache.RemoveCache(FileListCacheKey);
		}

		//William Severance - Modified to take supplied cache key as additional parameter
		public GalleryUploadCollection(GalleryFolder Album, int ModuleID, string FileListCacheKey)
		{
			mAlbum = Album;
			mUserID = UserController.GetCurrentUserInfo().UserID;
			mFileListCacheKey = FileListCacheKey;
			RefreshSpaceConstraints();
		}
		//New

		public GalleryFolder Album {
			get { return mAlbum; }
			set { mAlbum = value; }
		}

		public string ErrMessage {
			get { return mErrMessage; }
		}

		public DotNetNuke.Modules.Gallery.Config GalleryConfig {
//Return mGalleryConfig
			get { return mAlbum.GalleryConfig; }
		}

		public int UserID {
			get { return mUserID; }
		}

		public string FileListCacheKey {
			get { return mFileListCacheKey; }
		}

		public long GallerySpaceUsed {
			get { return mGallerySpaceUsed; }
		}

		public long SpaceAvailable {
			get { return mSpaceAvailable; }
		}

		public long Size {
			get { return mSize; }
		}

		public void AddFileToUpload(Gallery.FileToUpload uploadFile)
		{
			base.Add(uploadFile);
			mSpaceAvailable -= uploadFile.ContentLength;
			mSize += uploadFile.ContentLength;
		}

		public void RemoveFileToUpload(int index)
		{
			if (index >= 0 & index < Count) {
                Gallery.FileToUpload uploadFile = this[index];
				mSpaceAvailable += uploadFile.ContentLength;
				mSize -= uploadFile.ContentLength;
				RemoveAt(index);
			}
		}

		public bool FileExists(string FileName)
		{

			string filePath = Path.Combine(mAlbum.Path, FileName);
			// search in file system
			if (System.IO.File.Exists(filePath)) {
				return true;
			}

			// search in this collection
			int i = 0;

			for (i = 0; i <= Count - 1; i++) {
				if (this[i].FileName == FileName)
					return true;
				if (this[i].Extension == ".zip")
                {
					foreach (ZipFileEntry entry in this[i].ZipHeaders.Entries) {
						if (entry.FileName == FileName)
							return true;
					}
				}
			}
			return false;
		}

		public bool ExceedsQuota()
		{
			return mSpaceAvailable < 0;
		}

		public bool ExceedsQuota(int contentLength)
		{
			return mSpaceAvailable - contentLength < 0;
		}

		public void DoUpload()
		{
			FileToUpload uploadFile = null;
			string uploadPath = null;
			string uploadFilePath = null;
			string albumFilePath = null;
			string msgLocalized = "";
			mErrMessage = "";
			RefreshSpaceConstraints();
			//Refresh the available space based on current storage use
			if (ExceedsQuota()) {
				msgLocalized = Localization.GetString("Uploads_ExceedQuota", GalleryConfig.SharedResourceFile);
				if (string.IsNullOrEmpty(msgLocalized)) {
					msgLocalized = "The available gallery space of {0:F} kb would be exceeded where these pending uploads to be committed";
				}
				mErrMessage = string.Format(msgLocalized, (SpaceAvailable + Size) / 1024);
				return;
			} else {
				// Go backward to make sure correct item will be remove after uploading
				for (int i = Count - 1; i >= 0; i += -1) {
					// Create object to store file we are uploading
					// William Severance - changed begin/end of loop to avoid (i-1) inside loop
					uploadFile = this[i];
					//CType(Item(i - 1), FileToUpload)

					switch (uploadFile.Type) {
						case Config.ItemType.Flash:
						case Config.ItemType.Movie:
							// Normal upload, put file right in album directory
							uploadPath = mAlbum.Path;

							break;
						case Config.ItemType.Zip:
							// Zip file, put file into temp directory
							uploadPath = Utils.BuildPath(new string[2] {
								mAlbum.Path,
								GalleryConfig.TempFolder
							}, "\\", false, false);

							break;
						default:
							// Image type, put file into _source folder
							uploadPath = mAlbum.SourceFolderPath;
							break;
					}

					uploadFilePath = Path.Combine(uploadPath, uploadFile.FileName);
					albumFilePath = Path.Combine(Album.Path, uploadFile.FileName);

					// Do upload - create folder if it doesn't exists then upload file
					try {
						if (!Directory.Exists(uploadPath)) {
							Directory.CreateDirectory(uploadPath);
						}

						uploadFile.HtmlInput.PostedFile.SaveAs(uploadFilePath);

					} catch (System.Exception Exc) {
						Exceptions.LogException(Exc);
						msgLocalized = Localization.GetString("UploadFile_ProcessError", GalleryConfig.SharedResourceFile);
						if (string.IsNullOrEmpty(msgLocalized)) {
							msgLocalized = "An error occured while attempting to process the uploaded file {0}";
						}
						mErrMessage += string.Format(msgLocalized, uploadFile.FileName);
						// Couldn't even upload the file, not much use in continuing
						return;
						//WES: Removed Throw as we do NOT want to generate a second exception
					}

					// If an image, resize it
					if (GalleryConfig.IsValidImageType(uploadFile.Extension)) {
						DoResize(uploadFilePath, albumFilePath);
					}

					// Update XMLData - except Zip file
					// If uploaded file is not Zip then update XML data - If it's a Zip then do Unzip
					if (!(uploadFile.Type == Config.ItemType.Zip)) {
						// Regular file, save meta data
						GalleryXML metaData = new GalleryXML(mAlbum.Path);
						GalleryXML.SaveMetaData(mAlbum.Path, uploadFile.FileName, -1, uploadFile.Title, uploadFile.Description, uploadFile.Categories, uploadFile.Author, uploadFile.Location, uploadFile.Client, uploadFile.OwnerID,
						Null.NullDate, uploadFile.ApprovedDate, 0);

					} else {
						// Unzip the files
						UnzipFiles(uploadFile, uploadPath, uploadFilePath, albumFilePath);

						// Delete temp files & folder
						System.IO.Directory.Delete(uploadPath, true);

					}
					//Upzip

					// Remove uploaded file off the collection
					RemoveFileToUpload(i);
					//RemoveAt(i - 1)
				}
				RefreshSpaceConstraints();
			}
		}

		public void RefreshSpaceConstraints()
		{
			PortalSettings ps = PortalController.GetCurrentPortalSettings();
			mGallerySpaceUsed = Utils.GetDirectorySize(GalleryConfig.RootPath);
			long hostSpace = 0;
			if (ps.HostSpace == 0) {
				hostSpace = long.MaxValue;
			} else {
				hostSpace = Convert.ToInt64(ps.HostSpace) * 1024L * 1024L;
				//Convert portal hostspace in MB to bytes
			}
			if (GalleryConfig.Quota == 0) {
				mSpaceAvailable = hostSpace - mGallerySpaceUsed;
			} else {
				mSpaceAvailable = Math.Min(GalleryConfig.Quota * 1024L, hostSpace) - mGallerySpaceUsed;
			}
			mSize = GetCollectionSize();
			mSpaceAvailable -= mSize;
		}

		private long GetCollectionSize()
		{
			long sz = 0L;
			foreach (FileToUpload UploadFile in this) {
				sz += UploadFile.ContentLength;
			}
			return sz;
		}

		/// <summary>
		/// Unzip a bunch of files inside of a zip file
		/// </summary>
		/// <param name="UploadFile"></param>
		/// <param name="UploadPath"></param>
		/// <param name="UploadFilePath"></param>
		/// <param name="AlbumFilePath"></param>
		/// <remarks></remarks>

		private void UnzipFiles(FileToUpload UploadFile, string UploadPath, string UploadFilePath, string AlbumFilePath)
		{
			//William Severance = Set the default code page of the ICSharpCode.SharpZipLib to that of the
			//OEMCodePage of the CurrentUICulture to properly process extended character sets such as those found
			//in Latin 1 and Latin 2 cultures (Deutch (de-DE) umlaut, etc.)

			string msgLocalized = "";
			int OEMCodePage = System.Threading.Thread.CurrentThread.CurrentUICulture.TextInfo.OEMCodePage;
			ICSharpCode.SharpZipLib.Zip.ZipConstants.DefaultCodePage = OEMCodePage;

			ICSharpCode.SharpZipLib.Zip.ZipEntry objZipEntry = null;
			ICSharpCode.SharpZipLib.Zip.ZipInputStream objZipInputStream = null;

			// Try unzipping it
			try {
				objZipInputStream = new ZipInputStream(File.OpenRead(UploadFilePath));
			} catch (Exception exc) {
				Exceptions.LogException(exc);
				msgLocalized = Localization.GetString("UploadFile_OpenZipError", GalleryConfig.SharedResourceFile);
				if (string.IsNullOrEmpty(msgLocalized)) {
					msgLocalized = "An error occured while attempting to open the uploaded zip archive file {0}";
				}
				mErrMessage += string.Format(msgLocalized, UploadFile.FileName);
				return;
				//WES: Removed Throw as we do not want to generate another exception
			}

			objZipEntry = objZipInputStream.GetNextEntry();
			while ((objZipEntry != null)) {
				string unzipFile = Unzip(objZipEntry, objZipInputStream, UploadPath);
				string strExtension = Path.GetExtension(unzipFile);

				UploadFilePath = Path.Combine(UploadPath, unzipFile);
				AlbumFilePath = Path.Combine(mAlbum.Path, unzipFile);

				if (GalleryConfig.IsValidFlashType(strExtension) || GalleryConfig.IsValidMovieType(strExtension)) {
					// Normal upload
					File.Copy(UploadFilePath, AlbumFilePath);
					Utils.SaveDNNFile(AlbumFilePath, 0, 0, false, false);


				} else if (GalleryConfig.IsValidImageType(strExtension)) {
					string sourceFilePath = Path.Combine(mAlbum.SourceFolderPath, unzipFile);
					File.Copy(UploadFilePath, sourceFilePath);
					DoResize(sourceFilePath, AlbumFilePath);
				}

				// Save the meta/xml data
				GalleryXML metaData = new GalleryXML(mAlbum.Path);
				GalleryXML.SaveMetaData(mAlbum.Path, unzipFile, -1, UploadFile.Title, UploadFile.Description, UploadFile.Categories, UploadFile.Author, UploadFile.Location, UploadFile.Client, UploadFile.OwnerID,
				Null.NullDate, UploadFile.ApprovedDate, 0);

				// Do next file in zip
				objZipEntry = objZipInputStream.GetNextEntry();

			}
		}

		/// <summary>
		/// Unzip an individual file and save it to a directory
		/// </summary>
		/// <param name="ZipEntry"></param>
		/// <param name="InputStream"></param>
		/// <param name="TempDir"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		private string Unzip(ZipEntry ZipEntry, ZipInputStream InputStream, string TempDir)
		{
			string strFileName = "";
			string strFileNamePath = null;
			string localizedError = "";

			try {
				strFileName = Utils.MakeValidFilename(Path.GetFileName(ZipEntry.Name), "_");

				if (!string.IsNullOrEmpty(strFileName)) {
					strFileNamePath = Path.Combine(TempDir, strFileName);

					if (File.Exists(strFileNamePath)) {
						File.Delete(strFileNamePath);
					}

					FileStream objFileStream = File.Create(strFileNamePath);
					int intSize = 2048;
					byte[] arrData = new byte[2049];

					intSize = InputStream.Read(arrData, 0, arrData.Length);
					while (intSize > 0) {
						objFileStream.Write(arrData, 0, intSize);
						intSize = InputStream.Read(arrData, 0, arrData.Length);
					}

					objFileStream.Close();

					return strFileName;
				}

			} catch (System.Exception Exc) {
				Exceptions.LogException(Exc);
				localizedError = Localization.GetString("UploadFile_UnZipError", GalleryConfig.SharedResourceFile);
				if (string.IsNullOrEmpty(localizedError)) {
					localizedError = "An error occured while attempting to unzip file {0} from the uploaded archive";
				}
				mErrMessage += string.Format(localizedError, strFileName);
			}

			// JIMJ We couldn't get the filename
			return string.Empty;
		}

		/// <summary>
		/// Resize the uploaded image and put it into the album's folder
		/// </summary>
		/// <param name="UploadFile">File to resize</param>
		/// <param name="AlbumFile">Where to store the resized file</param>
		/// <remarks></remarks>

		private void DoResize(string UploadFile, string AlbumFile)
		{
			string localizedError = "";

			try {
				// Do Resize
				if (GalleryConfig.IsFixedSize) {
					// Save it to album folder for display
					var _with1 = GalleryConfig;
					GalleryGraphics.ResizeImage(UploadFile, AlbumFile, _with1.FixedWidth, _with1.FixedHeight, _with1.EncoderQuality);
				} else {
					File.Copy(UploadFile, AlbumFile);
					Utils.SaveDNNFile(AlbumFile, 0, 0, false, false);
				}

				// Delete original source file to save disk space
				if (!GalleryConfig.IsKeepSource) {
					File.Delete(UploadFile);
				} else {
					Utils.SaveDNNFile(UploadFile, 0, 0, false, false);
					//Add to the DNN files table if we're not deleting the source
				}
			} catch (System.Exception exc) {
				Exceptions.LogException(exc);
				localizedError = Localization.GetString("UploadFile_ResizeSaveError", GalleryConfig.SharedResourceFile);
				if (string.IsNullOrEmpty(localizedError)) {
					localizedError = "An error occured while attempting to resize and save the uploaded file {0}";
				}
				mErrMessage += string.Format(localizedError, Path.GetFileName(UploadFile));
			}
		}

	}

}
