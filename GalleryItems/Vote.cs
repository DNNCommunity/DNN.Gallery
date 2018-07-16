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

namespace DotNetNuke.Modules.Gallery
{

	public class Vote
	{
		private string mFileName;
		private int mUserID;
		private int mScore;
		private string mComment;

		private DateTime mCreatedDate;
		public Vote()
		{
		}

		#region "Properties"
		public string FileName {
			get { return mFileName; }
			set { mFileName = value; }
		}

		public int UserID {
			get { return mUserID; }
			set { mUserID = value; }
		}

		public int Score {
			get { return mScore; }
			set { mScore = value; }
		}

		public string Comment {
			get { return mComment; }
			set { mComment = value; }
		}

		public DateTime CreatedDate {
			get { return mCreatedDate; }
			set { mCreatedDate = value; }
		}

		public string ScoreImageURL {
			get {
				int intScore = Score * 10;
				return Config.DefaultTheme + "/stars_" + intScore.ToString() + ".gif";
			}
		}
		#endregion
	}

	public class VoteCollection : ArrayList
	{

		const string GalleryVotingCacheKeyPrefix = "GalleryVoting";
		private string mParentPath;
		private string mFileName;
		private double mScore;

		private ArrayList mUserList = new ArrayList();
		public VoteCollection() : base()
		{
		}

		public VoteCollection(string ParentPath, string FileName) : base()
		{
			mParentPath = ParentPath;
			mFileName = FileName;
			PopulateCollection();
		}

		public VoteCollection(ICollection c) : base(c)
		{
		}
		//New

		private void PopulateCollection()
		{
			// Get all of the votes
			GalleryXML VoteData = new GalleryXML(mParentPath);
			ArrayList Votes = VoteData.GetVotings(mFileName);
			this.AddRange(Votes);

			int totalScore = 0;
			foreach (Vote voteItem in Votes) {
				totalScore += voteItem.Score;
				mUserList.Add(voteItem.UserID);
			}
            if (Votes.Count > 0)
            {
                mScore = totalScore / Votes.Count;
            }
            else
            {
                mScore = 0f;
            }
		}

		public static VoteCollection GetVoting(string ParentPath, string FileName)
		{
			HttpApplicationState app = HttpContext.Current.Application;

			if (app[GalleryVotingCacheKeyPrefix + "-" + ParentPath + "-" + FileName] == null) {
				VoteCollection colVoting = new VoteCollection(ParentPath, FileName);
				app.Add(GalleryVotingCacheKeyPrefix + "-" + ParentPath + "-" + FileName, colVoting);
			}

			return (VoteCollection)app[GalleryVotingCacheKeyPrefix + "-" + ParentPath + "-" + FileName];

		}
		//GetVoting

		public static void ResetCollection(string ParentPath, string FileName)
		{
			HttpApplicationState app = HttpContext.Current.Application;
			app.Remove(GalleryVotingCacheKeyPrefix + "-" + ParentPath + "-" + FileName);

		}

		public double Score {
			get { return mScore; }
		}

		public ArrayList UserList {
			get { return mUserList; }
		}

		public bool UserCanVote(int UserID)
		{
			if (UserID > 0) {
				return (!UserList.Contains(UserID));
			}
			return false;
		}

		public string ScoreImage {
			get {
				int intScore = (Convert.ToInt32(Math.Floor(Score)) + Convert.ToInt32(Math.Ceiling(Score))) * 5;
				return Config.DefaultTheme + "/stars_" + intScore.ToString() + ".gif";
			}
		}

	}

	public class GalleryVoteCollection
	{

		private ArrayList mGalleryVoteList = new ArrayList();
		public GalleryVoteCollection(int ModuleID)
		{
			DotNetNuke.Modules.Gallery.Config mGalleryConfig = Config.GetGalleryConfig(ModuleID);
			GalleryFolder mRootFolder = mGalleryConfig.RootFolder;
			mGalleryVoteList.Clear();

			PopuplateFolderVote(mRootFolder);

		}

		private void PopuplateFolderVote(GalleryFolder Folder)
		{
			IGalleryObjectInfo item = null;

			foreach (IGalleryObjectInfo item_loopVariable in Folder.List) {
				item = item_loopVariable;
				GalleryFile fileItem = null;
				GalleryFolder folderItem = null;
				if (!item.IsFolder) {
					fileItem = (GalleryFile)item;
					mGalleryVoteList.AddRange(PopulateFileVote(fileItem));
				} else {
					folderItem = (GalleryFolder)item;
					PopuplateFolderVote(folderItem);
				}
			}
		}

		private ArrayList PopulateFileVote(GalleryFile File)
		{
			VoteCollection colFileVote = VoteCollection.GetVoting(File.Parent.Path, File.Name);
			return colFileVote;
		}

		public ArrayList GalleryVoteList {
			get { return mGalleryVoteList; }
		}
	}

}
