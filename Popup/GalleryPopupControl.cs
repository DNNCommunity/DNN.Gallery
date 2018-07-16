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


using System.Threading;
using System.Globalization;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;


namespace DotNetNuke.Modules.Gallery.PopupControls
{

	public class PopupPageBase : System.Web.UI.Page
	{

		// This code is in large part a duplicate of that in DotNetNuke.Framework.PageBase. Because the AJAX.AddScriptManager
		// method requires that the page have a form element when attempting to inject an AJAX ScriptManager, we cannot inherit
		// from DotNetNuke.Framework.PageBase as we would have liked.

		#region "Private Members"
		private string _localResourceFile;
			#endregion
		private CultureInfo _PageCulture = null;

		#region "Public Properties"
		public PortalSettings PortalSettings {
			get { return PortalController.GetCurrentPortalSettings(); }
		}

		public CultureInfo PageCulture {
			get {
				if (_PageCulture == null) {
					_PageCulture = Localization.GetPageLocale(PortalSettings);
				}
				return _PageCulture;
			}
		}

		public string LocalResourceFile {
			get {
				string fileRoot = null;
				string[] page = Request.ServerVariables["SCRIPT_NAME"].Split('/');

				if (string.IsNullOrEmpty(_localResourceFile)) {
					fileRoot = this.TemplateSourceDirectory + "/" + DotNetNuke.Services.Localization.Localization.LocalResourceDirectory + "/" + page[page.GetUpperBound(0)] + ".resx";
				} else {
					fileRoot = _localResourceFile;
				}
				return fileRoot;
			}
			set { _localResourceFile = value; }
		}

		#endregion

		protected override void OnInit(System.EventArgs e)
		{
			base.OnInit(e);
			if (HttpContext.Current.Request != null && !HttpContext.Current.Request.Url.LocalPath.ToLower().EndsWith("installwizard.aspx")) {
				// Set the current culture
				Thread.CurrentThread.CurrentUICulture = PageCulture;
				Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture;
			}
		}
	}

	//Inherits DotNetNuke.Framework.PageBase

	public class PopupControl : WebControl, INamingContainer
	{
		private bool mInitialised;
			//PopupBaseObject
		private PopupObject mPopupBaseObject;

		private int mPortalID;
		private int mTabID;
		private int mModuleID;
		private int mTargetID;
		private ObjectClass mObjectClass;
		private string mObjectName;
		private string mLocation = "";
		private string mSearchValue = "";

		private string mDataSource = "";
		public PopupControl()
		{
		}
		//New

		protected virtual void Initialise()
		{
			mInitialised = true;
		}
		//Initialise

		protected override void OnLoad(System.EventArgs e)
		{
			Initialise();
		}
		//OnLoad

		protected override void EnsureChildControls()
		{
			if (!mInitialised) {
				Initialise();
			}
			base.EnsureChildControls();
		}
		//EnsureChildControls

		protected virtual void CreateObject()
		{
		}
		//CreateObject

		protected override void CreateChildControls()
		{
			this.CreateObject();
			if ((mPopupBaseObject != null)) {
				mPopupBaseObject.CreateChildControls();
			}
		}
		//CreateChildControls

		protected override void OnPreRender(System.EventArgs e)
		{
			if ((mPopupBaseObject != null)) {
				mPopupBaseObject.OnPreRender();
			}
		}
		//OnPreRender

		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{
			if ((mPopupBaseObject != null)) {
				mPopupBaseObject.Render(writer);
			}
		}
		//Render 

		public int PortalID {
			get { return mPortalID; }
			set { mPortalID = value; }
		}

		public int TabID {
			get { return mTabID; }
			set { mTabID = value; }
		}

		public int ModuleID {
			get { return mModuleID; }
			set { mModuleID = value; }
		}
		public int TargetID {
			get { return mTargetID; }
			set { mTargetID = value; }
		}

		public ObjectClass ObjectClass {
			get { return mObjectClass; }
//Configuration.ObjectClass)
			set { mObjectClass = value; }
		}

		public string ObjectName {
			//Configuration.ObjectName
			get { return mObjectName; }
//Configuration.ObjectClass)
			set { mObjectName = value; }
		}

		public string Location {
			get { return mLocation; }
			set { mLocation = value; }
		}

		public string SearchValue {
			get { return mSearchValue; }
			set { mSearchValue = value; }
		}

		public string DataSource {
			get { return mDataSource; }
			set { mDataSource = value; }
		}

		public int LoggedOnUserID {
			get { return UserController.GetCurrentUserInfo().UserID; }
		}

		protected PopupObject PopupBaseObject {
			//PopupBaseObject
			get { return mPopupBaseObject; }
//PopupBaseObject)
			set { mPopupBaseObject = value; }
		}
	}

	public class PopupObject
	{
		private PopupControl mPopupControl;

		private ArrayList mSearchObjects = new ArrayList();
		public PopupObject(PopupData PopupData)
		{
			mPopupControl = PopupData;
		}
		//New

		public PopupObject(PopupSearch PopupSearch)
		{
			mPopupControl = PopupSearch;
		}
		//New

		protected PopupData PopupDataControl {
			get { return (PopupData)mPopupControl; }
		}

		protected PopupSearch PopupSearchControl {
			get { return (PopupSearch)mPopupControl; }
		}

		public virtual void CreateChildControls()
		{
		}
		//CreateChildControls

		public virtual void OnPreRender()
		{
		}
		//OnPreRender

		public virtual void Render(HtmlTextWriter writer)
		{
		}
		//Render

		public ArrayList SearchObjects {
			get { return mSearchObjects; }
			set { mSearchObjects = value; }
		}
	}

	public class PopupData : PopupControl
	{
		//Implements INamingContainer 'ToDo: Add Implements Clauses for implementation methods of these interface(s)

		private ArrayList mListItems = new ArrayList();
		public PopupData() : base()
		{
		}
		//New


		protected override void CreateObject()
		{
			int intClass = 1;
			// default value is user
			string strLocation = "";
			string strSearchValue = "";
			string strDataSource = "sql";
			int intDisplayMethod = 1;
			// default value is display_name

			PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
			PortalID = _portalSettings.PortalId;

			if ((Page.Request.QueryString["mid"] != null)) {
				ModuleID = int.Parse(Page.Request.QueryString["mid"]);
			}

			if ((Page.Request.QueryString["objectclass"] != null)) {
				intClass = (int)(ObjectClass)Enum.Parse(typeof(ObjectClass), Page.Request.QueryString["objectclass"]);
			} else {
				if ((Page.Request.QueryString["objectclasses"] != null)) {
					string[] strClass = Page.Request.QueryString["objectclasses"].Split(';');
					//If multi type get the first one
					intClass = Int16.Parse(strClass[0]);
				}
			}

			this.ObjectClass = (ObjectClass)intClass;

			if ((Page.Request.QueryString["location"] != null)) {
				strLocation = Page.Request.QueryString["location"];
			}
			this.Location = strLocation;

			if ((Page.Request.QueryString["searchvalue"] != null)) {
				strSearchValue = Page.Request.QueryString["searchvalue"];
			}
			this.SearchValue = strSearchValue;

			if ((Page.Request.QueryString["displaymethod"] != null)) {
				intDisplayMethod = Int16.Parse(Page.Request.QueryString["displaymethod"]);
			}

			PopupBaseObject = new PopupGalleryData(this);

		}

		protected override void Initialise()
		{
			base.Initialise();

		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();
		}

		public ArrayList ListItems {
			get { return mListItems; }
			set { mListItems = value; }
		}

	}
	//PopupData



	public class PopupListItem
	{
		private string mId;
		private string mText;
			//Configuration.ObjectClass
		private int mClass;
		private int mLevel;
		private string mImage;
		private ArrayList mAdditionalProperties = new ArrayList();

		private Hashtable mPropertyLookup = new Hashtable();
		public PopupListItem()
		{
		}

		public PopupListItem(string Id, int ObjectClass, string Text, string Image, int Level)
		{
			mId = Id;
			mClass = ObjectClass;
			mText = Text;
			mImage = Image;
			mLevel = Level;
		}

		public void AddProperty(string Name, string Value, int Size)
		{
			AdditionalProperty prop = new AdditionalProperty();
			int index = 0;
			try {
				prop.SetProperty(Name, Value, Size);
				index = mAdditionalProperties.Add(prop);
				mPropertyLookup.Add(Name, index);
			} catch (Exception ex) {
				//Throw ex
			}

		}

		public AdditionalProperty GetProperty(int index)
		{
			try {
				object prop = null;
				prop = mAdditionalProperties[index];
				return (AdditionalProperty)prop;

			} catch (Exception) {
                // was previously returning null but that is invalid in c# so returning empty arraylist instead
				return new AdditionalProperty();
			}
		}

		public AdditionalProperty GetProperty(string Name)
		{
			int index = 0;
			object prop = null;

			// Do validation first
			try {
				if (mPropertyLookup[Name] == null) {
					return new AdditionalProperty();
				}
			} catch (Exception) {
				return new AdditionalProperty();
			}

			index = Convert.ToInt32(mPropertyLookup[Name]);
			prop = mAdditionalProperties[index];

			return (AdditionalProperty)prop;
		}

		// Additional columns display in result list for more info
		public struct AdditionalProperty
		{
			internal string mName;
			internal string mValue;

			internal int mSize;
			internal void SetProperty(string Name, string Value, int Size)
			{
				mName = Name;
				mValue = Value;
				mSize = Size;
			}

			public string Name {
				get { return mName; }
			}

			public string Value {
				get { return mValue; }
			}

			public int Size {
				get { return mSize; }
			}

		}

		public string DisplayString {
			get {
				StringBuilder sb = new StringBuilder();
				int intCount = 0;
				for (intCount = 1; intCount <= mLevel; intCount++) {
					sb.Append("&nbsp;&nbsp;");
				}
				//sb.Append("<img src=")
				//sb.Append(Config.ImageURL & mImage)
				sb.Append(mImage);
				//sb.Append(" />")
				sb.Append(mText);

				return sb.ToString();
			}
		}

		public string Id {
			get { return mId; }
		}

		public string Text {
			get { return mText; }
		}

		public int ObjectClass {
			//Configuration.ObjectClass
			get { return this.mClass; }
		}

		public string Image {
			get { return mImage; }
		}

		public ArrayList AdditionalProperties {
			get { return mAdditionalProperties; }
		}

	}


	public class RenderPopup
	{


		private PopupData mPopup;
		public RenderPopup()
		{
		}

		public RenderPopup(PopupData Popup)
		{
			mPopup = Popup;
		}

		private void RenderTableBegin(HtmlTextWriter wr)
		{
			wr.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
			wr.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
			wr.AddStyleAttribute("height", "100%");
			wr.AddStyleAttribute("width", "100%");
			wr.RenderBeginTag(HtmlTextWriterTag.Table);

		}

		private void RenderListHeader(HtmlTextWriter wr)
		{
			string imgURL = Common.Globals.AddHTTP(Common.Globals.GetDomainName(HttpContext.Current.Request)) + "/DesktopModules/Gallery/Popup/Images/";
			//ADSIConfig.PopupImageURL


			if (!(mPopup.ListItems.Count == 0)) {
				wr.RenderBeginTag(HtmlTextWriterTag.Tr);

				wr.AddStyleAttribute("height", "10px");
				wr.RenderBeginTag(HtmlTextWriterTag.Td);

				wr.AddAttribute(HtmlTextWriterAttribute.Class, "ListHeader");
				wr.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
				wr.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
				wr.AddStyleAttribute("table-layout", "fixed");
				wr.AddStyleAttribute("width", "100%");
				wr.RenderBeginTag(HtmlTextWriterTag.Table);

				wr.RenderBeginTag(HtmlTextWriterTag.Tr);

				wr.AddAttribute(HtmlTextWriterAttribute.Class, "Column");
				wr.RenderBeginTag(HtmlTextWriterTag.Td);

				wr.Write("&nbsp;" + (Localization.GetString("Column_" + "Name", Common.Globals.ApplicationPath + "/DesktopModules/Gallery" + "/App_LocalResources/SharedResources.resx")));
				wr.RenderEndTag();
				// TD

				// Check to see how many columns should be displayed by getting a listitem from items list
				PopupListItem listItem = (PopupListItem)mPopup.ListItems[0];
				PopupListItem.AdditionalProperty additionalProp = default(PopupListItem.AdditionalProperty);
				foreach (PopupListItem.AdditionalProperty additionalProp_loopVariable in listItem.AdditionalProperties) {
					additionalProp = additionalProp_loopVariable;
					wr.AddAttribute(HtmlTextWriterAttribute.Class, "Column");
					wr.AddStyleAttribute("width", (additionalProp.Size + 25).ToString() + "px");
					wr.RenderBeginTag(HtmlTextWriterTag.Td);
					wr.Write("<img alt='' style=\"vertical-align: middle\" src=\"" + imgURL + "sp_Barline.gif\"  />");
					wr.Write("&nbsp;" + (Localization.GetString("Column_" + additionalProp.Name, Common.Globals.ApplicationPath + "/DesktopModules/Gallery" + "/App_LocalResources/SharedResources.resx")));
					wr.RenderEndTag();
					// td
				}

				wr.RenderEndTag();
				// tr
				wr.RenderEndTag();
				// table
				wr.RenderEndTag();
				// td
				wr.RenderEndTag();
				// tr
			}
		}


		private void RenderPopupData(HtmlTextWriter wr)
		{
			wr.RenderBeginTag(HtmlTextWriterTag.Tr);
			wr.RenderBeginTag(HtmlTextWriterTag.Td);

			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Objects");
			wr.RenderBeginTag(HtmlTextWriterTag.Div);

			wr.AddAttribute("onkeydown", "javascript:listKeyDown(this,event);");
			wr.AddAttribute("ondblclick", "javascript:itemDoubleClick(event);");
			wr.AddAttribute(HtmlTextWriterAttribute.Onclick, "javascript:clickItem(this,event);");

			wr.AddAttribute(HtmlTextWriterAttribute.Id, "tblResults");
			wr.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
			wr.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "2px");
			wr.AddStyleAttribute("table-layout", "fixed");
			wr.AddStyleAttribute("width", "100%");
			wr.RenderBeginTag(HtmlTextWriterTag.Table);

			wr.RenderBeginTag(HtmlTextWriterTag.Colgroup);

			wr.AddAttribute(HtmlTextWriterAttribute.Name, "Name");
			wr.AddAttribute(HtmlTextWriterAttribute.Class, "Row");
			wr.RenderBeginTag(HtmlTextWriterTag.Col);
			wr.RenderEndTag();
			// Col


			if ((!(mPopup.ListItems.Count == 0))) {
				PopupListItem baseItem = (PopupListItem)mPopup.ListItems[0];
				PopupListItem.AdditionalProperty additionalProp = default(PopupListItem.AdditionalProperty);

				foreach (PopupListItem.AdditionalProperty additionalProp_loopVariable in baseItem.AdditionalProperties) {
					additionalProp = additionalProp_loopVariable;
					wr.AddAttribute(HtmlTextWriterAttribute.Name, additionalProp.Name);
					wr.AddStyleAttribute("width", additionalProp.Size.ToString() + "px");
					wr.AddAttribute(HtmlTextWriterAttribute.Class, "Row");
					wr.RenderBeginTag(HtmlTextWriterTag.Col);
					wr.RenderEndTag();
					// Col    

				}
			} else if (mPopup.ListItems.Count == 0) {
				wr.AddStyleAttribute("height", "25%");
				wr.AddStyleAttribute("width", "100%");
				wr.AddAttribute(HtmlTextWriterAttribute.Id, "tblInfo");
				wr.AddAttribute(HtmlTextWriterAttribute.Class, "Message");
				wr.RenderBeginTag(HtmlTextWriterTag.Table);

				wr.RenderBeginTag(HtmlTextWriterTag.Tr);

				wr.AddStyleAttribute("text-align", "center");
				wr.AddAttribute(HtmlTextWriterAttribute.Class, "Message");
				wr.RenderBeginTag(HtmlTextWriterTag.Td);
				wr.Write(Localization.GetString("NoRecordsFound", Common.Globals.ApplicationPath + "/DesktopModules/Gallery" + "/App_LocalResources/SharedResources.resx"));

				wr.RenderEndTag();
				// td
				wr.RenderEndTag();
				// tr
				wr.RenderEndTag();
				// table
			}

			wr.RenderBeginTag(HtmlTextWriterTag.Tbody);

			PopupListItem listItem = null;
			foreach (PopupListItem listItem_loopVariable in mPopup.ListItems) {
				listItem = listItem_loopVariable;
				string sId = listItem.Id;
				//entry.NativeGuid
				string sText = listItem.Text;
				//Tools.ConvertToCanonical(entry.Name, False)
				string sClass = Convert.ToInt32(listItem.ObjectClass).ToString();
				string sImage = listItem.Image;
				string sLocation = listItem.GetProperty("Location").Value;

				wr.RenderBeginTag(HtmlTextWriterTag.Tr);

				wr.AddAttribute(HtmlTextWriterAttribute.Class, "Select");
				wr.AddAttribute(HtmlTextWriterAttribute.Nowrap, "nowrap");
				wr.RenderBeginTag(HtmlTextWriterTag.Td);

				// image
				wr.AddAttribute(HtmlTextWriterAttribute.Border, "0");
				wr.AddAttribute(HtmlTextWriterAttribute.Src, sImage);
				wr.AddAttribute(HtmlTextWriterAttribute.Alt, "");
				wr.RenderBeginTag(HtmlTextWriterTag.Img);
				wr.RenderEndTag();
				// Img

				wr.Write("&nbsp;");
				wr.Write(sText);

				wr.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
				wr.AddAttribute(HtmlTextWriterAttribute.Name, "oname");
				wr.AddAttribute(HtmlTextWriterAttribute.Value, "" + sText);
				wr.RenderBeginTag(HtmlTextWriterTag.Input);
				wr.RenderEndTag();
				// Input

				wr.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
				wr.AddAttribute(HtmlTextWriterAttribute.Name, "otype");
				wr.AddAttribute(HtmlTextWriterAttribute.Value, "" + sClass);
				wr.RenderBeginTag(HtmlTextWriterTag.Input);
				wr.RenderEndTag();
				// Input

				wr.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
				wr.AddAttribute(HtmlTextWriterAttribute.Name, "oid");
				wr.AddAttribute(HtmlTextWriterAttribute.Value, "" + sId);
				wr.RenderBeginTag(HtmlTextWriterTag.Input);
				wr.RenderEndTag();
				// Input

				wr.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
				wr.AddAttribute(HtmlTextWriterAttribute.Name, "olocation");
				wr.AddAttribute(HtmlTextWriterAttribute.Value, "" + sLocation);
				wr.RenderBeginTag(HtmlTextWriterTag.Input);
				wr.RenderEndTag();
				// Input

				wr.RenderEndTag();
				// Td


				PopupListItem.AdditionalProperty additionalProp = default(PopupListItem.AdditionalProperty);
				foreach (PopupListItem.AdditionalProperty additionalProp_loopVariable in listItem.AdditionalProperties) {
					additionalProp = additionalProp_loopVariable;
					wr.AddAttribute(HtmlTextWriterAttribute.Class, "Row");
					wr.AddAttribute(HtmlTextWriterAttribute.Nowrap, "nowrap");
					wr.AddStyleAttribute("width", additionalProp.Size.ToString() + "px");
					wr.RenderBeginTag(HtmlTextWriterTag.Td);
					wr.Write("&nbsp;");
					wr.Write(additionalProp.Value);
					wr.RenderEndTag();
					// Td
				}
				//End If

				wr.RenderEndTag();
				// Tr
			}

			wr.RenderEndTag();
			// tbody
			wr.RenderEndTag();
			// colgroup
			wr.RenderEndTag();
			// table            

			wr.RenderEndTag();
			// div
			wr.RenderEndTag();
			// td
			wr.RenderEndTag();
			// tr

		}
		//RenderPopup

		private void RenderTableEnd(HtmlTextWriter wr)
		{
			//</table>
			wr.RenderEndTag();
			// table

		}


		public void Render(HtmlTextWriter wr)
		{
			RenderTableBegin(wr);
			RenderListHeader(wr);
			RenderPopupData(wr);
			RenderTableEnd(wr);
		}
	}

}
