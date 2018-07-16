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
using DotNetNuke.Entities.Users;
using static DotNetNuke.Modules.Gallery.Utils;
using static DotNetNuke.Modules.Gallery.Config;

namespace DotNetNuke.Modules.Gallery
{

	public abstract partial class Voting : DotNetNuke.Entities.Modules.PortalModuleBase, DotNetNuke.Entities.Modules.IActionable
	{

			//WES - Later make a configuration setting?
		private const int maxStars = 5;

		// Obtain PortalSettings from Current Context   

		PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
		private DotNetNuke.Modules.Gallery.Config mGalleryConfig;
		private GalleryRequest mRequest;
		private GalleryFolder mFolder;
		private GalleryFile mFile;
		private string mMode = "";
		private VoteCollection mVoteCollection = new VoteCollection();
		private int mUserID = 0;

		private string mVoteText = "{0} out of {1} stars";
		#region " Web Form Designer Generated Code "

		//This call is required by the Web Form Designer.
		[System.Diagnostics.DebuggerStepThrough()]

		private void InitializeComponent()
		{
		}

		private void Page_Init(System.Object sender, System.EventArgs e)
		{
			//CODEGEN: This method call is required by the Web Form Designer
			//Do not modify it using the code editor.
			InitializeComponent();

			if (mGalleryConfig == null) {
				mGalleryConfig = Config.GetGalleryConfig(ModuleId);
			}
		}

		#endregion

		#region "Optional Interfaces"

		public ModuleActionCollection ModuleActions {
			get {
				ModuleActionCollection Actions = new ModuleActionCollection();
				Actions.Add(GetNextActionID(), Localization.GetString("Configuration.Action", LocalResourceFile), ModuleActionType.ModuleSettings, "", "edit.gif", EditUrl("configuration"), "", false, SecurityAccessLevel.Admin, true,
				false);
				Actions.Add(GetNextActionID(), Localization.GetString("GalleryHome.Action", LocalResourceFile), "", "", "icon_moduledefinitions_16px.gif", Common.Globals.NavigateURL(), false, SecurityAccessLevel.Anonymous, true, false);
				return Actions;
			}
		}

		#endregion

		private void Page_Load(System.Object sender, System.EventArgs e)
		{
			//Put user code to initialize the page here

			// Load the styles
			((CDefault)Page).AddStyleSheet(Common.Globals.CreateValidID(GalleryConfig.Css), GalleryConfig.Css);

			if ((Request.QueryString["mode"] != null)) {
				mMode = Request.QueryString["mode"];
			}

			mRequest = new GalleryRequest(ModuleId);
			mFolder = mRequest.Folder;

			if ((Request.QueryString["currentitem"] != null)) {
				int mCurrentItem = Int32.Parse(Request.QueryString["currentitem"]);
				mFile = (GalleryFile)mRequest.Folder.List.Item(mCurrentItem);
				mVoteCollection = mFile.Votes;
			}

			mUserID = UserController.GetCurrentUserInfo().UserID;

			//WES - Added localization

			if (!IsPostBack) {
				if (mMode == "add") {
					mVoteText = Localization.GetString("Vote", LocalResourceFile);
					tdTitle.InnerText = Localization.GetString("Voting", LocalResourceFile);
					tdYourRating.InnerText = Localization.GetString("Your_Rating", LocalResourceFile);
				} else {
					tdTitle.InnerText = Localization.GetString("Rating", LocalResourceFile);
					tdResult.InnerText = Localization.GetString("Rating_Summary", LocalResourceFile);
					tdName.InnerText = Localization.GetString("Name", LocalResourceFile);
					tdCreatedDate.InnerText = Localization.GetString("Created_Date", LocalResourceFile);
					tdAuthor.InnerText = Localization.GetString("Author", LocalResourceFile);
					tdDescription.InnerText = Localization.GetString("Description", LocalResourceFile);
				}

				if (mGalleryConfig.IsValidPath) {
					BindData();
					if ((ViewState["UrlReferrer"] != null)) {
						ViewState["UrlReferrer"] = Request.UrlReferrer.ToString();
					}
				}
			}
		}


		private void BindData()
		{
			tdFilePreview.Style.Add("width", (GalleryConfig.MaximumThumbWidth + 50).ToString() + "px");
			imgTitle.ImageUrl = mFile.IconURL;
			imgVoteSummary.ImageUrl = mFile.ScoreImageURL;

			lnkTitle.NavigateUrl = mFile.BrowserURL;
			lnkTitle.Text = mFile.Title;

			lnkThumb.NavigateUrl = mFile.BrowserURL;
			lnkThumb.ImageUrl = mFile.ThumbnailURL;
			lnkThumb.ToolTip = mFile.GetItemInfo(false);

			btnAddVote.Visible = mVoteCollection.UserCanVote(mUserID);
			if (mMode == "add") {
				divAddVote.Visible = true;
				divDetails.Visible = false;
				BindVoteList();
			} else {
				BindInfo();
			}

			if (mVoteCollection.Count > 0) {
				trVotes.Visible = true;
				lblVotes.Text = Localization.GetString("RatingsAndComments", LocalResourceFile);
				dlVotes.DataSource = mVoteCollection;
				dlVotes.DataBind();
			} else {
				trVotes.Visible = false;
			}

		}

		private void BindInfo()
		{
			this.lblName.Text = mFile.Name;
			string currentRating = string.Empty;
			if (mVoteCollection.Count == 0) {
				currentRating = Localization.GetString("Not_Rated.Text", LocalResourceFile);
			} else {
				currentRating = string.Format(Localization.GetString("Result.Text", LocalResourceFile), mFile.Score, maxStars, mVoteCollection.Count);
			}
			lblResult.Text = currentRating;
			imgVoteSummary.ToolTip = currentRating;
			imgVoteSummary.AlternateText = currentRating;

			this.lblAuthor.Text = mFile.Author;
			this.lblDate.Text = mFile.CreatedDate.ToShortDateString();
			this.lblDescription.Text = mFile.Description;
		}

		private void BindVoteList()
		{
			lstVote.Items.Clear();

			int intCount = 0;

			for (intCount = maxStars; intCount >= 1; intCount += -1) {
				string altTitle = string.Format(mVoteText, intCount, maxStars);
				string imgURL = "<img src='DesktopModules/Gallery/Themes/" + Config.DefaultTheme + "/Images/stars_" + (intCount * 10).ToString() + ".gif' alt='" + altTitle + "' title='" + altTitle + "' />";
				ListItem lstItem = new ListItem(imgURL, intCount.ToString());
				lstVote.Items.Add(lstItem);
			}
			lstVote.SelectedIndex = 0;
		}

		private void Goback()
		{
			string url = Utils.GetURL(Page.Request.ServerVariables["URL"], Page, "", "ctl=&mode=&mid=&media=&currentitem=");
			Response.Redirect(url);
		}

		protected string GetUserName(object DataItem)
		{
			Vote voteItem = (Vote)DataItem;
			return new UserController().GetUser(_portalSettings.PortalId, voteItem.UserID).DisplayName;
		}

		protected string ScoreImage(object DataItem)
		{
			Vote objVote = (Vote)DataItem;
			int intScore = objVote.Score * 10;
			string imageName = "stars_" + intScore.ToString() + ".gif";
			return mGalleryConfig.GetImageURL(imageName);
		}

		public DotNetNuke.Modules.Gallery.Config GalleryConfig {
			get { return mGalleryConfig; }
		}

		private void btnSave_Click(System.Object sender, System.EventArgs e)
		{
			Vote newVote = new Vote();
			PortalSecurity Security = new PortalSecurity();

			var _with1 = newVote;
			_with1.UserID = mUserID;
			_with1.FileName = mFile.Name;
			_with1.CreatedDate = DateTime.Today;
			_with1.Score = Int16.Parse(lstVote.SelectedItem.Value);
			//WES - Added InputFilter to strip markup and SQL
			_with1.Comment = Security.InputFilter(txtComment.Text, PortalSecurity.FilterFlag.NoMarkup | PortalSecurity.FilterFlag.NoScripting);
			mFile.UpdateVotes(newVote);
			Goback();
		}

		private void cmdCancel_Click(System.Object sender, System.EventArgs e)
		{
			//Modified by WES to permit cancelation of voting without returning back to gallery
			string url = Utils.GetURL(Page.Request.ServerVariables["URL"], Page, "", "mode=");
			Response.Redirect(url);
		}

		private void btnAddVote_Click(System.Object sender, System.EventArgs e)
		{
			string url = Utils.GetURL(Page.Request.ServerVariables["URL"], Page, "mode=add", "");
			Response.Redirect(url);
		}

		private void btnBack_Click(System.Object sender, System.EventArgs e)
		{
			Goback();
		}
		public Voting()
		{
			Load += Page_Load;
			Init += Page_Init;
		}

	}

}

