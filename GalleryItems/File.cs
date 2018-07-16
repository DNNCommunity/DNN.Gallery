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

	public class GalleryFile : IGalleryObjectInfo
	{

		#region "Private Variables"
		private int mIndex;
		private int mID;
		// System info
		private GalleryFolder mParent;
		private string mName;
		private string mURL;
		private string mPath;
		private string mThumbnail;
		private string mThumbnailURL;
		private string mThumbnailPath;
		private string mIcon;
		private string mIconURL;
		private string mScoreImage;
		private string mScoreImageURL;
		private string mSourceURL;

		private string mSourcePath;
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
		private int mWidth;

		private int mHeight;
		// Interface implementation for display purpose        
		private string mBrowserURL = "";
		private string mPopupBrowserURL = "";
		private string mPopupExifURL = "";
		private string mPopupEditImageURL = "";
		private string mDownloadURL = "";
		private string mItemInfo = "";
		private long mSize = 0;
		private Config.ItemType mType;
		private double mScore = 0;

		private DotNetNuke.Modules.Gallery.Config mGalleryConfig;
		#endregion

		#region "Properties"
		//<tamttt:note identity id for all  module objects>
		public int ObjectTypeCode {
			get { return 502; }
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

		public string ThumbnailPath {
			get { return mThumbnailPath; }
		}

		public string SourceURL {
			get { return mSourceURL; }
		}

		public string SourcePath {
			get { return mSourcePath; }
		}

		public string Icon {
			get { return mIcon; }
		}

		public string IconURL {
			get { return mIconURL; }
		}

		public string ScoreImage {
			get { return mScoreImage; }
		}

		public string ScoreImageURL {
			get { return mScoreImageURL; }
		}

		public long Size {
			get { return mSize; }
		}

		public Config.ItemType Type {
			get { return mType; }
		}

		public bool IsFolder {
			get { return false; }
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

		//'Public ReadOnly Property ImageCssClass() As String Implements IGalleryObjectInfo.ImageCssClass
		//'  Get
		//'    Return "Gallery_Image"
		//'  End Get
		//'End Property

		//'Public ReadOnly Property ThumbnailCssClass() As String Implements IGalleryObjectInfo.ThumbnailCssClass
		//'  Get
		//'    If mType = Config.ItemType.Image Then
		//'      Return "Gallery_Thumb"
		//'    Else
		//'      Return ""
		//'    End If
		//'  End Get
		//'End Property

		public int OwnerID {
			get { return mOwnerID; }
			set { mOwnerID = value; }
		}

		public DateTime ApprovedDate {
			get { return mApprovedDate; }
			set { mApprovedDate = value; }
		}

		public DateTime CreatedDate {
			get { return mCreatedDate; }
		}

		public int Width {
			get { return mWidth; }
			set { mWidth = value; }
		}

		public int Height {
			get { return mHeight; }
			set { mHeight = value; }
		}

		public double Score {
			get { return mScore; }
			set { mScore = value; }
		}

		public string BrowserURL {
			get {
				if (mGalleryConfig.AllowPopup) {
					return mPopupBrowserURL;
				} else {
					return GetBrowserURL();
				}
			}
		}

		public string SlideshowURL {
			get {
				if (mGalleryConfig.AllowPopup) {
					return GetPopupSlideshowURL();
				} else {
					return GetSlideshowURL();
				}
			}
		}

		public string EditURL {
			get { return GetEditURL(); }
		}

		public string EditImageURL {
			get {
				if (mGalleryConfig.AllowPopup) {
					return mPopupEditImageURL;
				} else {
					System.Collections.Generic.List<string> @params = new System.Collections.Generic.List<string>();
					@params.Add("mid=" + GalleryConfig.ModuleID.ToString());
					if (!string.IsNullOrEmpty(Parent.GalleryHierarchy)) {
						@params.Add("path=" + Utils.FriendlyURLEncode(Parent.GalleryHierarchy));
					}
					@params.Add("currentitem=" + this.Index.ToString());
					@params.Add("mode=edit");
					@params.Add("media=" + Name);
					return Common.Globals.NavigateURL(GalleryConfig.GalleryTabID, "Viewer", @params.ToArray());
				}
			}
		}

		public string ExifURL {
			get {
				if (Type != Config.ItemType.Image)
					return "";
				if (mGalleryConfig.AllowPopup) {
					return mPopupExifURL;
				} else {
					return GetExifURL();
				}
			}
		}

		public string DownloadURL {
			get { return mDownloadURL; }
		}

		public string VotingURL {
			get { return GetVotingURL(); }
		}

		public string ItemInfo {
			get { return mItemInfo; }
		}

		public VoteCollection Votes {
			get { return VoteCollection.GetVoting(this.Parent.Path, mName); }
		}

		public DotNetNuke.Modules.Gallery.Config GalleryConfig {
			get { return mGalleryConfig; }
		}

		#endregion

		#region "Methods and Functions"

		internal GalleryFile(int Index, int ID, GalleryFolder Parent, string Name, string URL, string Path, string Thumbnail, string ThumbnailURL, string ThumbnailPath, string Icon,
		string IconURL, string ScoreImage, string ScoreImageURL, string SourcePath, string SourceURL, long Size, Config.ItemType Type, string Title, string Description, string Categories,

		string Author, string Client, string Location, int OwnerID, DateTime ApprovedDate, DateTime CreatedDate, int Width, int Height, double Score, DotNetNuke.Modules.Gallery.Config GalleryConfig)
		{
			mIndex = Index;
			mID = ID;
			mParent = Parent;
			mName = Name;
			mURL = URL;
			mPath = Path;
			mThumbnail = Thumbnail;
			mThumbnailURL = ThumbnailURL;
			mThumbnailPath = ThumbnailPath;
			mIcon = Icon;
			mIconURL = IconURL;
			mScoreImage = ScoreImage;
			mScoreImageURL = ScoreImageURL;
			mSourcePath = SourcePath;
			mSourceURL = SourceURL;
			mSize = Size;
			mType = Type;
			mTitle = Title;
			mDescription = Description;
			mCategories = Categories;
			mAuthor = Author;
			mClient = Client;
			mLocation = Location;
			mOwnerID = OwnerID;
			mApprovedDate = ApprovedDate;
			mCreatedDate = CreatedDate;
			mWidth = Width;
			mHeight = Height;
			mScore = Score;
			mGalleryConfig = GalleryConfig;
			mPopupBrowserURL = GetPopupBrowserURL();
			mPopupExifURL = GetPopupExifURL();
			mPopupEditImageURL = GetPopupEditImageURL();
			mDownloadURL = GetDownloadURL();
			mItemInfo = GetItemInfo();
		}

		public void UpdateImage(Bitmap NewImage, System.Drawing.Imaging.ImageFormat iFormat)
		{
			if (!(this.Type == Config.ItemType.Image) || NewImage == null)
				return;
			try {
				File.Delete(Path);
				File.Delete(ThumbnailPath);
				NewImage.Save(Path, iFormat);
				Utils.SaveDNNFile(Path, NewImage.Width, NewImage.Height, false, true);
				GalleryGraphics.ResizeImage(this.Path, this.ThumbnailPath, GalleryConfig.MaximumThumbWidth, GalleryConfig.MaximumThumbHeight, GalleryConfig.EncoderQuality);
			} catch (Exception exc) {
				throw;
			}
		}

		// Not currently used anywhere in the module
		public void RestoreImage()
		{
			if (!(this.Type == Config.ItemType.Image))
				return;
			try {
				GalleryGraphics.ResizeImage(this.SourcePath, this.Path, GalleryConfig.FixedWidth, GalleryConfig.FixedHeight, GalleryConfig.EncoderQuality);
			} catch (Exception ex) {
			}
		}

		public void UpdateVotes(Vote NewVote)
		{
			GalleryXML.AddVote(Parent.Path, NewVote);
			VoteCollection.ResetCollection(Parent.Path, Name);

			VoteCollection votes = VoteCollection.GetVoting(Parent.Path, Name);
			double newScore = votes.Score;

			// Save new score back to file metadata, it would be un-nessesary, but give a better performance when populate file
			GalleryXML.SaveMetaData(Parent.Path, Name, ID, Title, Description, Categories, Author, Location, Client, OwnerID,
			CreatedDate, ApprovedDate, newScore);

			Score = newScore;
			Config.ResetGalleryFolder(this.Parent);
		}

		private string GetBrowserURL()
		{
			System.Collections.Generic.List<string> @params = new System.Collections.Generic.List<string>();
			@params.Add("mid=" + GalleryConfig.ModuleID.ToString());
			if (!string.IsNullOrEmpty(Parent.GalleryHierarchy)) {
				@params.Add("path=" + Utils.FriendlyURLEncode(Parent.GalleryHierarchy));
			}
			@params.Add("currentitem=" + Index.ToString());

			string ctlKey = "";
			switch (mType) {
				case Config.ItemType.Image:
					ctlKey = "Viewer";
					break;
				case Config.ItemType.Flash:
					ctlKey = "FlashPlayer";
					break;
				case Config.ItemType.Movie:
					ctlKey = "MediaPlayer";
					break;
				default:
					throw new Exception("Unknown mType, " + mType.ToString());
			}
			return Common.Globals.NavigateURL(GalleryConfig.GalleryTabID, ctlKey, @params.ToArray());
		}

		private string GetPopupBrowserURL()
		{

			StringBuilder sb = new StringBuilder();

			sb.Append(GalleryConfig.SourceDirectory());
			sb.Append("/GalleryPage.aspx?ctl=");

			switch (mType) {
				case Config.ItemType.Image:
					sb.Append("Viewer");
					break;
				case Config.ItemType.Movie:
					sb.Append("MediaPlayer");
					break;
				case Config.ItemType.Flash:
					sb.Append("FlashPlayer");
					break;
			}
			sb.Append("&tabid=");
			// <notes: found & fixed by Vicenç Masanas> 
			sb.Append(GalleryConfig.GalleryTabID.ToString());
			sb.Append("&mid=");
			sb.Append(GalleryConfig.ModuleID.ToString());
			// </notes> 
			if (!string.IsNullOrEmpty(Parent.GalleryHierarchy)) {
				sb.Append("&path=");
				sb.Append(Utils.JSEncode(Parent.GalleryHierarchy));
			}
			sb.Append(Utils.FriendlyURLEncode(URLParams()));

			return Utils.BuildPopup(sb.ToString(), GalleryConfig.FixedHeight + 200, GalleryConfig.FixedWidth + 100);

		}

		private string GetSlideshowURL()
		{
			if (mType != Config.ItemType.Image) {
				return Parent.SlideshowURL;
			} else {
				System.Collections.Generic.List<string> @params = new System.Collections.Generic.List<string>();
				@params.Add("mid=" + GalleryConfig.ModuleID.ToString());
				if (!string.IsNullOrEmpty(Parent.GalleryHierarchy)) {
					@params.Add("path=" + Utils.FriendlyURLEncode(Parent.GalleryHierarchy));
				}
				@params.Add("currentitem=" + Index.ToString());

				string ctlKey = "SlideShow";
				return Common.Globals.NavigateURL(GalleryConfig.GalleryTabID, ctlKey, @params.ToArray());
			}
		}

		private string GetPopupSlideshowURL()
		{
			if (mType != Config.ItemType.Image) {
				return Parent.SlideshowURL;
			} else {
				StringBuilder sb = new StringBuilder();
				sb.Append(GalleryConfig.SourceDirectory());
				sb.Append("/GalleryPage.aspx?ctl=SlideShow");
				sb.Append("&mid=");
				sb.Append(GalleryConfig.ModuleID.ToString());
				sb.Append("&tabid=");
				sb.Append(GalleryConfig.GalleryTabID.ToString());
				if (!string.IsNullOrEmpty(Parent.GalleryHierarchy)) {
					sb.Append("&path=");
					sb.Append(Utils.FriendlyURLEncode(Parent.GalleryHierarchy));
				}
				sb.Append("&currentitem=");
				sb.Append(Index.ToString());
				return Utils.BuildPopup(sb.ToString(), mGalleryConfig.FixedHeight + 160, mGalleryConfig.FixedWidth + 120);
			}
		}

		private string GetExifURL()
		{
			System.Collections.Generic.List<string> @params = new System.Collections.Generic.List<string>();
			@params.Add("mid=" + GalleryConfig.ModuleID.ToString());
			if (!string.IsNullOrEmpty(Parent.GalleryHierarchy)) {
				@params.Add("path=" + Utils.FriendlyURLEncode(Parent.GalleryHierarchy));
			}
			@params.Add("currentitem=" + this.Index.ToString());
			@params.Add("media=" + Name);
			return Common.Globals.NavigateURL(GalleryConfig.GalleryTabID, "Exif", @params.ToArray());
		}

		private string GetPopupExifURL()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(GalleryConfig.SourceDirectory());
			sb.Append("/GalleryPage.aspx?ctl=Exif");
			sb.Append("&tabid=");
			sb.Append(GalleryConfig.GalleryTabID.ToString());
			sb.Append("&mid=");
			sb.Append(GalleryConfig.ModuleID.ToString());
			if (!string.IsNullOrEmpty(Parent.GalleryHierarchy)) {
				sb.Append("&path=");
				sb.Append(Utils.FriendlyURLEncode(Parent.GalleryHierarchy));
			}
			sb.Append(Utils.FriendlyURLEncode(URLParams()));

			return Utils.BuildPopup(sb.ToString(), mGalleryConfig.FixedHeight, mGalleryConfig.FixedWidth);
		}

		private string GetEditURL()
		{
			System.Collections.Generic.List<string> @params = new System.Collections.Generic.List<string>();
			@params.Add("mid=" + GalleryConfig.ModuleID.ToString());
			if (!string.IsNullOrEmpty(Parent.GalleryHierarchy)) {
				@params.Add("path=" + Utils.FriendlyURLEncode(Parent.GalleryHierarchy));
			}
			@params.Add("currentitem=" + this.Index.ToString());
			return Common.Globals.NavigateURL(GalleryConfig.GalleryTabID, "FileEdit", @params.ToArray());
		}

		private string GetPopupEditImageURL()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(GalleryConfig.SourceDirectory());
			sb.Append("/GalleryPage.aspx?ctl=Viewer");
			sb.Append("&tabid=");
			sb.Append(GalleryConfig.GalleryTabID.ToString());
			sb.Append("&mid=");
			sb.Append(GalleryConfig.ModuleID.ToString());
			if (!string.IsNullOrEmpty(Parent.GalleryHierarchy)) {
				sb.Append("&path=");
				sb.Append(Utils.FriendlyURLEncode(Parent.GalleryHierarchy));
			}
			sb.Append("&mode=edit");
			sb.Append(Utils.FriendlyURLEncode(URLParams()));
			return Utils.BuildPopup(sb.ToString(), GalleryConfig.FixedHeight + 100, GalleryConfig.FixedWidth + 100);
		}

		private string GetVotingURL()
		{
			string[] @params = new string[4] {
				"mid=" + GalleryConfig.ModuleID.ToString(),
				"path=" + Utils.FriendlyURLEncode(Parent.GalleryHierarchy),
				"currentitem=" + this.Index.ToString(),
				"media=" + Name
			};
			//, "currentstrip=" & CurrentStrip.ToString}
			return Common.Globals.NavigateURL(GalleryConfig.GalleryTabID, "Voting", Utils.RemoveEmptyParams(@params));
		}

		// Replaced module's own Download.aspx with core LinkClick.aspx - WES 2-13-2009
		private string GetDownloadURL()
		{
			PortalSettings ps = PortalController.GetCurrentPortalSettings();
			string filePath = SourcePath.Replace(ps.HomeDirectoryMapPath, "").Replace("\\", "/");
			int fileID = 0;
			if (ID >= 0) {
				fileID = ID;
			} else {
				DotNetNuke.Services.FileSystem.FileController fc = new DotNetNuke.Services.FileSystem.FileController();
				fileID = fc.ConvertFilePathToFileId(filePath, ps.PortalId);
			}
			return DotNetNuke.Common.Globals.LinkClick("FileID=" + fileID.ToString(), GalleryConfig.GalleryTabID, GalleryConfig.ModuleID, false, true);
		}

		private string URLParams()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("&currentitem=");
			sb.Append(Index.ToString());
			return sb.ToString();
		}

		protected string GetItemInfo()
		{
			return GetItemInfo(true);
		}

		internal string GetItemInfo(bool HtmlFormatted)
		{
			StringBuilder sb = new StringBuilder();

			// William Severance (9-10-2010) - Modified to use bitmapped TextDisplayOptions flags rather than iterating through string array for each test.

			if (HtmlFormatted) {
				if ((GalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.Name) != 0) {
					sb.Append("<span class=\"MediaName\">");
					sb.Append(Name);
					sb.Append("</span>");

					if ((GalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.Size) != 0) {
						sb.Append("<span class=\"MediaSize\">");
						string sizeInfo = " " + Localization.GetString("FileSizeInfo", GalleryConfig.SharedResourceFile);
						sizeInfo = sizeInfo.Replace("[FileSize]", Math.Ceiling((double)Size / 1024).ToString());
						sb.Append(sizeInfo);
						sb.Append("</span>");
					}
					sb.Append("<br />");
				}

				if (((GalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.Author) != 0) && !(Author.Length == 0)) {
					AppendLegendAndFieldHtmlFormatted(sb, "Author", Author);
				}

				if (((GalleryConfig.TextDisplayOptions & GalleryDisplayOption.Client) != 0) && !(Client.Length == 0)) {
					AppendLegendAndFieldHtmlFormatted(sb, "Client", Client);
				}

				if (((GalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.Location) != 0) && !(Location.Length == 0)) {
					AppendLegendAndFieldHtmlFormatted(sb, "Location", Location);
				}

				if ((GalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.CreatedDate) != 0) {
					AppendLegendAndFieldHtmlFormatted(sb, "CreatedDate", Utils.DateToText(CreatedDate));
				}

				if ((GalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.ApprovedDate) != 0) {
					AppendLegendAndFieldHtmlFormatted(sb, "ApprovedDate", Utils.DateToText(ApprovedDate));
				}
			} else {
				if ((GalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.Name) != 0) {
					sb.Append(Name);
					if ((GalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.Size) != 0) {
						sb.Append(" ");
						string sizeInfo = " " + Localization.GetString("FileSizeInfo", GalleryConfig.SharedResourceFile);
						sizeInfo = sizeInfo.Replace("[FileSize]", Math.Ceiling((double)Size / 1024).ToString());
						sb.Append(sizeInfo);
					}
					sb.AppendLine();
				}

				if (((GalleryConfig.TextDisplayOptions & Config.GalleryDisplayOption.Author) != 0) && !(Author.Length == 0)) {
					AppendLegendAndField(sb, "Author", Author);
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

			}
			return sb.ToString();

		}

		private void AppendLegendAndFieldHtmlFormatted(StringBuilder sb, string LegendKey, string FieldValue)
		{
			sb.Append("<span class=\"Legend\">");
			sb.Append(Localization.GetString(LegendKey, GalleryConfig.SharedResourceFile));
			sb.Append("</span><span class=\"Field\">");
			sb.Append(FieldValue);
			sb.Append("</span>");
			sb.Append("<br/>");
		}

		private void AppendLegendAndField(StringBuilder sb, string LegendKey, string FieldValue)
		{
			sb.Append(Localization.GetString(LegendKey, GalleryConfig.SharedResourceFile));
			sb.Append(" ");
			sb.AppendLine(FieldValue);
		}

		#endregion

	}

}
