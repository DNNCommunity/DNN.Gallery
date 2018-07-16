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

	public partial class AlbumEdit
	{

		///<summary>
		///trNavigation control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.HtmlControls.HtmlTableRow trNavigation;
		///<summary>
		///celHeaderLeft control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.HtmlControls.HtmlTableCell celHeaderLeft;
		///<summary>
		///celGalleryMenu control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.HtmlControls.HtmlTableCell celGalleryMenu;
		///<summary>
		///celBreadcrumbs control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.HtmlControls.HtmlTableCell celBreadcrumbs;
		///<summary>
		///celHeaderRight control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.HtmlControls.HtmlTableCell celHeaderRight;
		///<summary>
		///lblInfo control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.Label lblInfo;
		///<summary>
		///rowDetails control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.HtmlControls.HtmlTableRow rowDetails;
		///<summary>
		///plName control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::DotNetNuke.UI.UserControls.LabelControl plName;
		///<summary>
		///txtName control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.TextBox txtName;
		///<summary>
		///lblAlbumName control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.Label lblAlbumName;
		///<summary>
		///rqdFieldValidatorTxtName control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.RequiredFieldValidator rqdFieldValidatorTxtName;
		///<summary>
		///validateCharacters4txtName control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.RegularExpressionValidator validateCharacters4txtName;
		///<summary>
		///plTitle control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::DotNetNuke.UI.UserControls.LabelControl plTitle;
		///<summary>
		///txtTitle control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.TextBox txtTitle;
		///<summary>
		///rqdFieldValidatorTxtTitle control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.RequiredFieldValidator rqdFieldValidatorTxtTitle;
		///<summary>
		///plAuthor control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::DotNetNuke.UI.UserControls.LabelControl plAuthor;
		///<summary>
		///txtAuthor control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.TextBox txtAuthor;
		///<summary>
		///plClient control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::DotNetNuke.UI.UserControls.LabelControl plClient;
		///<summary>
		///txtClient control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.TextBox txtClient;
		///<summary>
		///plLocation control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::DotNetNuke.UI.UserControls.LabelControl plLocation;
		///<summary>
		///txtLocation control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.TextBox txtLocation;
		///<summary>
		///plDescription control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::DotNetNuke.UI.UserControls.LabelControl plDescription;
		///<summary>
		///txtDescription control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.TextBox txtDescription;
		///<summary>
		///rowApprovedDate control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.HtmlControls.HtmlTableRow rowApprovedDate;
		///<summary>
		///plApprovedDate control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::DotNetNuke.UI.UserControls.LabelControl plApprovedDate;
		///<summary>
		///txtApprovedDate control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.TextBox txtApprovedDate;
		///<summary>
		///cmdApprovedDate control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.HyperLink cmdApprovedDate;
		///<summary>
		///valApprovedDate control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.RangeValidator valApprovedDate;
		///<summary>
		///rowOwner control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.HtmlControls.HtmlTableRow rowOwner;
		///<summary>
		///plOwner control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::DotNetNuke.UI.UserControls.LabelControl plOwner;
		///<summary>
		///ctlOwnerLookup control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::DotNetNuke.Modules.Gallery.WebControls.LookupControl ctlOwnerLookup;
		///<summary>
		///plCategories control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::DotNetNuke.UI.UserControls.LabelControl plCategories;
		///<summary>
		///lstCategories control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.WebControls.CheckBoxList lstCategories;
		///<summary>
		///rowUpload control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.HtmlControls.HtmlTableRow rowUpload;
		///<summary>
		///galControlUpload control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::DotNetNuke.Modules.Gallery.WebControls.Upload galControlUpload;
		///<summary>
		///cmdUpdate control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>
		private global::System.Web.UI.WebControls.LinkButton withEventsField_cmdUpdate;
		protected global::System.Web.UI.WebControls.LinkButton cmdUpdate {
			get { return withEventsField_cmdUpdate; }
			set {
				if (withEventsField_cmdUpdate != null) {
					withEventsField_cmdUpdate.Click -= cmdUpdate_Click;
				}
				withEventsField_cmdUpdate = value;
				if (withEventsField_cmdUpdate != null) {
					withEventsField_cmdUpdate.Click += cmdUpdate_Click;
				}
			}

		}
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
		///rowAlbumGrid control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.HtmlControls.HtmlTableRow rowAlbumGrid;
		///<summary>
		///ControlAlbum1 control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::DotNetNuke.Modules.Gallery.WebControls.Album ControlAlbum1;
		///<summary>
		///rowRefuse control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::System.Web.UI.HtmlControls.HtmlTableRow rowRefuse;
		///<summary>
		///plRefuseTitle control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>

		protected global::DotNetNuke.UI.UserControls.LabelControl plRefuseTitle;
		///<summary>
		///lblRefuse control.
		///</summary>
		///<remarks>
		///Auto-generated field.
		///To modify move field declaration from designer file to code-behind file.
		///</remarks>
		protected global::System.Web.UI.WebControls.Label lblRefuse;
	}
}