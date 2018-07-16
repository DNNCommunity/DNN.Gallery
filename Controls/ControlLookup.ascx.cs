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


using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Modules;
using static DotNetNuke.Common.Globals;
using static DotNetNuke.Modules.Gallery.Config;
using static DotNetNuke.Modules.Gallery.Utils;


namespace DotNetNuke.Modules.Gallery.WebControls
{

	public abstract partial class LookupControl : System.Web.UI.UserControl
	{

		//Private mItemClass As .Gallery.ObjectClass
		//Private mLookupSingle As Boolean = False
		//Private mLocations As String = ""
		//Private mImage As String = "sp_forum.gif"
		//Protected WithEvents txtItemsID As System.Web.UI.WebControls.TextBox
		//Protected WithEvents txtInputItemsID As System.Web.UI.WebControls.TextBox
		//Protected WithEvents rowLookup As System.Web.UI.HtmlControls.HtmlTableRow
		//Protected WithEvents celLookupData As System.Web.UI.HtmlControls.HtmlTableCell
		//Protected WithEvents tblLookup As System.Web.UI.HtmlControls.HtmlTable
		//Protected WithEvents celLookupItems As System.Web.UI.HtmlControls.HtmlTableCell

		//Localisation added by M. Schlomann/William Severance
		public string AddText = "";
		public string RemoveText = "";
		public string notSpecified = "";

		private string mLocalResourceFile = null;
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
		}

		#endregion

		//William Severance - Added property to provide localization
		public string LocalResourceFile {
			get {
				if (mLocalResourceFile == null) {
					mLocalResourceFile = Localization.GetResourceFile(this, "ControlLookup.ascx");
				}
				return mLocalResourceFile;
			}
			set { mLocalResourceFile = value; }
		}

		private void Page_Load(System.Object sender, System.EventArgs e)
		{
			PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();

			// Localisation Added by M. Schlomann/William Severance
			AddText = Localization.GetString("Add", LocalResourceFile);
			notSpecified = DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(Localization.GetString("notSpecified", LocalResourceFile));
			RemoveText = Localization.GetString("Remove", LocalResourceFile);
			if (this.BindingContainer is PortalModuleBase && ModuleID == 0) {
				ModuleID = ((PortalModuleBase)this.NamingContainer).ModuleId;
			}

			// JIMJ try looking up the module id from the querystring
			if (ModuleID == 0 && (Request.QueryString["mid"] != null)) {
				ModuleID = int.Parse(Request.QueryString["mid"]);
			}

			string lstItems = txtItemsID.Value;
			//txtItemsID.Text
			bool itemExists = false;
			//lstItems =

			celLookupItems.InnerHtml = "";
			foreach (string item in lstItems.Split(';')) {
				if (!(item.Length == 0)) {
					string itemID = Strings.Left(item, item.IndexOf(':'));
					string itemName = Strings.Right(item, item.Length - (item.IndexOf(':') + 1));
					RenderItems(itemID, itemName);
					itemExists = true;
					//lblitemName = (Localization.GetString(itemName, Request.QueryString("itemName") & "Add", ApplicationPath & "/DesktopModules/Gallery/Controls" & "/App_LocalResources/ControlLookup.ascx.resx")))
				}
			}

			if (!itemExists) {
				celLookupItems.InnerHtml = "<span class='normaltextbox'>" + notSpecified + "</span>";
			} else {
				if (LookupSingle) {
					//Me.btnRemove.Visible = True
				}
			}

		}


		private void BindItems()
		{
		}

		private void RenderItems(string ItemID, string ItemName)
		{
			string imgURL = "";
			if (Image.Length > 0) {
				imgURL = Common.Globals.AddHTTP(Common.Globals.GetDomainName(HttpContext.Current.Request)) + "/DesktopModules/Gallery/Popup/Images/" + Image;
			}
			StringBuilder sb = new StringBuilder();


			sb.Append("<span class='normaltextbox'");
			sb.Append(" oid='" + ItemID + "'");
			sb.Append(" otype='" + ItemClass + "'");
			sb.Append(" olocation='>");
			//sb.Append(JSDecode(mLocations))
			sb.Append("'>");
			if (Image.Length > 0) {
				sb.Append("<img src=\"");
				sb.Append(imgURL);
				sb.Append("\" alt='' border='0'/>&nbsp;");
			}
			sb.Append(ItemName);
			sb.Append("</span><br/>");

			LiteralControl litControl = new LiteralControl(sb.ToString());
			celLookupItems.Controls.Add(litControl);

		}

		protected string ItemLookup()
		{
			PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();

			if ((Locations.Length > 0) && !(Locations.StartsWith("&locations="))) {
				Locations = Utils.JSEncode("&locations=" + Locations);
			}

			string url = null;
			string features = null;
			if (LookupSingle) {
				url = Page.ResolveUrl("DesktopModules/Gallery/Popup/PopupSelectSingle.aspx");
				features = "dialogHeight:340px;dialogWidth:480px;resizable:yes;center:yes;status:no;help:no;scroll:no;";
			} else {
				url = Page.ResolveUrl("DesktopModules/Gallery/Popup/PopupSelectMulti.aspx");
				features = "dialogHeight:480px;dialogWidth:480px;resizable:yes;center:yes;status:no;help:no;scroll:no;";
			}

			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append("javascript:OpenGalleryPopup(");
			sb.Append("this");
			sb.Append(", '");
			sb.Append(url);
			sb.Append("?datasource=gallery");
			sb.Append(Locations);
			sb.Append("&objectclasses=");
			sb.Append(ItemClass);
			sb.Append("&tabid=");
			sb.Append(_portalSettings.ActiveTab.TabID.ToString());
			sb.Append("&mid=");
			sb.Append(ModuleID.ToString());
			sb.Append("', '");
			sb.Append(features);
			sb.Append("')");

			return sb.ToString();

		}

		//Protected Function ItemLookup0() As String
		//    Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
		//    If (Locations.Length > 0) AndAlso Not (Locations.StartsWith("&locations=")) Then
		//        Locations = "&locations=" & Locations
		//    End If

		//    Dim sb As New System.Text.StringBuilder
		//    sb.Append("javascript:OpenGalleryPopup(")
		//    sb.Append(ItemClass)
		//    sb.Append(", this")
		//    sb.Append(", '")
		//    sb.Append(JSEncode(Locations))
		//    sb.Append("&tabid=")
		//    sb.Append(_portalSettings.ActiveTab.TabID.ToString)
		//    sb.Append("&mid=")
		//    sb.Append(ModuleID.ToString)
		//    If LookupSingle Then
		//        sb.Append("', '")
		//        sb.Append("true")
		//    End If
		//    sb.Append("')")

		//    Return sb.ToString

		//End Function

		protected bool CanRemove()
		{
			return (LookupSingle && this.txtItemsID.Value.Length > 0);
			//Me.txtItemsID.Text.Length > 0)
		}

		public void AddItem(int ID, string Name)
		{
			txtItemsID.Value += ";" + ID.ToString() + ":" + Name;
			//txtItemsID.Text += ";" & ID.ToString & ":" & Name
			txtInputItemsID.Value += ";" + ID.ToString();

		}

		public void AddItem(string ID, string Name)
		{
			txtItemsID.Value += ";" + ID + ":" + Name;
			//txtItemsID.Text += ";" & ID & ":" & Name
			txtInputItemsID.Value += ";" + ID;
		}

		public string RemovedItems {
			get {
				string strItems = txtItemsID.Value;
				//txtItemsID.Text
				string strReturn = "";
				string item = null;
				if (strItems.Length == 0) {
					strReturn = txtInputItemsID.Value;
				} else {
					foreach (string item_loopVariable in txtInputItemsID.Value.Split(';')) {
						item = item_loopVariable;
						if (item.Length > 0 && strItems.IndexOf(";" + item + ":") < 0) {
							strReturn += ";" + item;
						}
					}
				}
				return strReturn;
			}
		}

		public string AddedItems {
			get {
				string strItems = txtItemsID.Value;
				//txtItemsID.Text
				string strReturn = "";
				string item = null;
				if (txtInputItemsID.Value.Length == 0) {
					foreach (string item_loopVariable in strItems.Split(';')) {
						item = item_loopVariable;
						if (item.Length > 0) {
							strReturn += ";" + Strings.Left(item, item.IndexOf(':'));
						}
					}
				} else {
					foreach (string item_loopVariable in strItems.Split(';')) {
						item = item_loopVariable;
						if (item.Length > 0 && txtInputItemsID.Value.IndexOf(";" + Strings.Left(item, item.IndexOf(':'))) < 0) {
							strReturn += ";" + Strings.Left(item, item.IndexOf(':'));
						}
					}
				}
				return strReturn;
			}
		}

		public string ResultItems {
			get {
				string strItems = txtItemsID.Value;
				//txtItemsID.Text
				string strReturn = "";
				string item = null;
				foreach (string item_loopVariable in strItems.Split(';')) {
					item = item_loopVariable;
					if (item.Length > 0) {
						strReturn += ";" + Strings.Left(item, item.IndexOf(':'));
					}
				}
				return strReturn;
			}
		}

		public string Locations {
			get {
				object savedState = ViewState["Locations"];
				if (savedState == null) {
					return "";
				} else {
					return Convert.ToString(savedState);
				}
			}
//mLocations = Value
			set { ViewState["Locations"] = value; }
		}

		public string Image {
			get {
				//Return mImage
				object savedState = ViewState["Image"];
				if (savedState == null) {
					return "sp_gallery.gif";
				} else {
					return Convert.ToString(savedState);
				}
			}
//mImage = Value
			set { ViewState["Image"] = value; }
		}

		public DotNetNuke.Modules.Gallery.ObjectClass ItemClass {
			get {
				//Return mItemClass
				object savedState = ViewState["ItemClass"];
				if (savedState == null) {
					return ObjectClass.DNNRole;
				} else {
					return (DotNetNuke.Modules.Gallery.ObjectClass)savedState;
				}
			}
			set { ViewState["ItemClass"] = value; }
		}

		public bool LookupSingle {
			get {
				object savedState = ViewState["LookupSingle"];
				if (savedState == null) {
					return false;
				} else {
					return Convert.ToBoolean(savedState);
				}
			}
			set { ViewState["LookupSingle"] = value; }
		}

		public int ModuleID {
			get { return Convert.ToInt32(ViewState["ModuleID"]); }
			set { ViewState["ModuleID"] = value; }
		}

		//William Severance - Added to pass notSpecified localization and NamingContainer.ClientID to scripts
		private void Page_PreRender(object sender, System.EventArgs e)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("var notSpecified = '");
			sb.Append(notSpecified);
			sb.AppendLine("';");
			sb.Append("var namingContainer = '");
			sb.Append(this.NamingContainer.ClientID);
			sb.AppendLine("';");
			if (!Page.ClientScript.IsClientScriptBlockRegistered(NamingContainer.GetType(), "ControlLookupVars")) {
				Page.ClientScript.RegisterClientScriptBlock(NamingContainer.GetType(), "ControlLookupVars", sb.ToString(), true);
			}
		}
		public LookupControl()
		{
			PreRender += Page_PreRender;
			Load += Page_Load;
			Init += Page_Init;
		}
	}

}
