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
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated. 
// </auto-generated>
//------------------------------------------------------------------------------


namespace DotNetNuke.Modules.Gallery
{

	public partial class Voting
	{

		///<summary>
		///tdTitle control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.HtmlControls.HtmlTableCell tdTitle;
		///<summary>
		///tdFilePreview control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.HtmlControls.HtmlTableCell tdFilePreview;
		///<summary>
		///imgTitle control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.Image imgTitle;
		///<summary>
		///lnkTitle control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.HyperLink lnkTitle;
		///<summary>
		///lnkThumb control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.HyperLink lnkThumb;
		///<summary>
		///imgVoteSummary control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.Image imgVoteSummary;
		///<summary>
		///divDetails control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.HtmlControls.HtmlGenericControl divDetails;
		///<summary>
		///tdResult control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.HtmlControls.HtmlTableCell tdResult;
		///<summary>
		///lblResult control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.Label lblResult;
		///<summary>
		///tdName control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.HtmlControls.HtmlTableCell tdName;
		///<summary>
		///lblName control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.Label lblName;
		///<summary>
		///tdCreatedDate control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.HtmlControls.HtmlTableCell tdCreatedDate;
		///<summary>
		///lblDate control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.Label lblDate;
		///<summary>
		///tdAuthor control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.HtmlControls.HtmlTableCell tdAuthor;
		///<summary>
		///lblAuthor control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.Label lblAuthor;
		///<summary>
		///tdDescription control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.HtmlControls.HtmlTableCell tdDescription;
		///<summary>
		///lblDescription control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.Label lblDescription;
		///<summary>
		///btnAddVote control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>
		private global::System.Web.UI.WebControls.LinkButton withEventsField_btnAddVote;
		protected global::System.Web.UI.WebControls.LinkButton btnAddVote {
			get { return withEventsField_btnAddVote; }
			set {
				if (withEventsField_btnAddVote != null) {
					withEventsField_btnAddVote.Click -= btnAddVote_Click;
				}
				withEventsField_btnAddVote = value;
				if (withEventsField_btnAddVote != null) {
					withEventsField_btnAddVote.Click += btnAddVote_Click;
				}
			}

		}
		///<summary>
		///divAddVote control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.HtmlControls.HtmlGenericControl divAddVote;
		///<summary>
		///tdYourRating control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.HtmlControls.HtmlTableCell tdYourRating;
		///<summary>
		///lstVote control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.RadioButtonList lstVote;
		///<summary>
		///plComments control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::DotNetNuke.UI.UserControls.LabelControl plComments;
		///<summary>
		///txtComment control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.TextBox txtComment;
		///<summary>
		///cmdCancel control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>
		private global::System.Web.UI.WebControls.LinkButton withEventsField_cmdCancel;
		protected global::System.Web.UI.WebControls.LinkButton cmdCancel {
			get { return withEventsField_cmdCancel; }
			set {
				if (withEventsField_cmdCancel != null) {
					withEventsField_cmdCancel.Click -= cmdCancel_Click;
				}
				withEventsField_cmdCancel = value;
				if (withEventsField_cmdCancel != null) {
					withEventsField_cmdCancel.Click += cmdCancel_Click;
				}
			}

		}
		///<summary>
		///btnSave control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>
		private global::System.Web.UI.WebControls.LinkButton withEventsField_btnSave;
		protected global::System.Web.UI.WebControls.LinkButton btnSave {
			get { return withEventsField_btnSave; }
			set {
				if (withEventsField_btnSave != null) {
					withEventsField_btnSave.Click -= btnSave_Click;
				}
				withEventsField_btnSave = value;
				if (withEventsField_btnSave != null) {
					withEventsField_btnSave.Click += btnSave_Click;
				}
			}

		}
		///<summary>
		///trVotes control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.HtmlControls.HtmlTableRow trVotes;
		///<summary>
		///lblVotes control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.Label lblVotes;
		///<summary>
		///dlVotes control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.DataList dlVotes;
		///<summary>
		///btnBack control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>
		private global::System.Web.UI.WebControls.LinkButton withEventsField_btnBack;
		protected global::System.Web.UI.WebControls.LinkButton btnBack {
			get { return withEventsField_btnBack; }
			set {
				if (withEventsField_btnBack != null) {
					withEventsField_btnBack.Click -= btnBack_Click;
				}
				withEventsField_btnBack = value;
				if (withEventsField_btnBack != null) {
					withEventsField_btnBack.Click += btnBack_Click;
				}
			}
		}
	}
}
