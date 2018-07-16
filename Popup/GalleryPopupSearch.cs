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



namespace DotNetNuke.Modules.Gallery.PopupControls
{



	internal class LocationListItem
	{
		//Inherits DotNetNuke.Framework.PageBase
		//' Changed Inherits System.Web.UI.Page to DNN Framework PageBase by M. Schlomann
		private string mText;
		private string mValue;

		private int mLevel;
		public LocationListItem()
		{
		}

		public LocationListItem(string Text, string Value, int Level)
		{
			mText = Text;
			mValue = Value;
			mLevel = Level;
		}

		public string DisplayString {
			get {
				StringBuilder sb = new StringBuilder();
				int intCount = 0;
				for (intCount = 1; intCount <= mLevel; intCount++) {
					sb.Append("&nbsp;&nbsp;");
				}
				//sb.Append("<img src=")
				//sb.Append("Images/" & mImage)
				//sb.Append(" />")
				sb.Append(mText);

				return sb.ToString();
			}
		}

		public string Value {
			get { return mValue; }
		}
	}


	public class PopupSearch : PopupControl
	{

		public PopupSearch()
		{
		}
		//New


		protected override void CreateObject()
		{
			//Dim objClass As Integer = 0
			string strObjectClasses = "";
			string strObjectClass = "";
			ArrayList objClasses = new ArrayList();
			string strLocations = "";

			PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
			this.PortalID = _portalSettings.PortalId;

			if ((Page.Request.QueryString["mid"] != null)) {
				ModuleID = int.Parse(Page.Request.QueryString["mid"]);
			}

			if ((Page.Request.QueryString["targetid"] != null)) {
				TargetID = Int16.Parse(Page.Request.QueryString["targetid"]);
			}

			// provide data for dropdownlist of object classes
			if ((Page.Request.QueryString["objectclasses"] != null)) {
				strObjectClasses = Page.Request.QueryString["objectclasses"];
				if (Strings.Len(strObjectClasses) > 0) {
					foreach (string strObjectClass_loopVariable in strObjectClasses.Split(';')) {
						strObjectClass = strObjectClass_loopVariable;
						string strName = Enum.GetName(typeof(ObjectClass), Int16.Parse(strObjectClass));
						objClasses.Add(new ListItem(strName, strObjectClass));
					}
				}
			}

			//provide data for dropdownlist of adsi domain
			if ((Page.Request.QueryString["locations"] != null)) {
				strLocations = Page.Request.QueryString["locations"];
			}

			PopupBaseObject = new SearchControlInfo(this, objClasses, strLocations);

		}


		protected override void Initialise()
		{
			base.Initialise();

		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();
		}
	}


	public class SearchControlInfo : PopupObject
	{

		private ArrayList mObjectClasses = new ArrayList();

		private ArrayList mLocations = new ArrayList();
		public SearchControlInfo(PopupSearch SearchControl, ArrayList ObjectClasses, string Locations) : base(SearchControl)
		{

			mObjectClasses = ObjectClasses;

			PopulateLocations(Locations);

		}
		//New

		private string SharedResourceFile {
			get { return Common.Globals.ApplicationPath + "/DesktopModules/Gallery/" + Localization.LocalResourceDirectory + "/" + Localization.LocalSharedResourceFile; }
		}

		private void PopulateLocations(string Locations)
		{
			string strLocation = null;
			if (Locations.Length > 0) {
				foreach (string strLocation_loopVariable in Locations.Split(';')) {
					strLocation = strLocation_loopVariable;
					if (strLocation.Length > 0) {
						mLocations.Add(new LocationListItem(strLocation, strLocation, 1));
					}
				}
			}

		}

		private string GetLocalizedItemText(ListItem item)
		{
			string localizedItemText = Localization.GetString(item.Text, SharedResourceFile);
			if (localizedItemText == null) {
				localizedItemText = item.Text;
			}
			return localizedItemText;
		}

		public override void CreateChildControls()
		{
		}
		//CreateChildControls

		public override void OnPreRender()
		{
		}
		//OnPreRender

		private void RenderTableBegin(HtmlTextWriter wr)
		{
			wr.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
			wr.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "2px");
			//wr.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
			wr.AddStyleAttribute("width", "100%");
			//wr.AddAttribute(HtmlTextWriterAttribute.Class, "Border")
			wr.RenderBeginTag(HtmlTextWriterTag.Table);

			wr.RenderBeginTag(HtmlTextWriterTag.Colgroup);
			wr.RenderBeginTag(HtmlTextWriterTag.Col);
			wr.RenderEndTag();
			// Col
			//wr.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
			wr.AddStyleAttribute("width", "100%");
			wr.RenderBeginTag(HtmlTextWriterTag.Col);
			wr.RenderEndTag();
			// Col
			wr.RenderBeginTag(HtmlTextWriterTag.Col);
			wr.RenderEndTag();
			// Col
			wr.RenderBeginTag(HtmlTextWriterTag.Tbody);

		}
		//RenderPopup

		private void RenderLocations(HtmlTextWriter wr)
		{
			if (mLocations.Count > 0) {
				int intCount = 0;

				wr.RenderBeginTag(HtmlTextWriterTag.Tr);
				//wr.AddAttribute(HtmlTextWriterAttribute.Height, "25px")
				wr.AddStyleAttribute("height", "25px");
				wr.RenderBeginTag(HtmlTextWriterTag.Td);
				//wr.AddAttribute(HtmlTextWriterAttribute.Align, "right")
				wr.AddStyleAttribute("text-align", "right");
				wr.RenderBeginTag(HtmlTextWriterTag.B);
				wr.Write("Search:");
				wr.RenderEndTag();
				// B
				wr.RenderEndTag();
				// TD  


				if (mLocations.Count > 1) {
					wr.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
					wr.RenderBeginTag(HtmlTextWriterTag.Td);

					wr.AddAttribute(HtmlTextWriterAttribute.Class, "SelectBox");
					wr.AddAttribute(HtmlTextWriterAttribute.Name, "selLocation");
					wr.AddAttribute(HtmlTextWriterAttribute.Value, "");
					wr.AddAttribute("changehandler", "selectChange");
					wr.AddAttribute("tabbingindex", "1");
					wr.RenderBeginTag(HtmlTextWriterTag.Span);

					wr.AddAttribute(HtmlTextWriterAttribute.Style, "display: none");
					wr.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
					wr.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "2px");
					wr.RenderBeginTag(HtmlTextWriterTag.Table);
					wr.RenderBeginTag(HtmlTextWriterTag.Tbody);

					for (intCount = 0; intCount <= (mLocations.Count - 1); intCount++) {
						wr.RenderBeginTag(HtmlTextWriterTag.Tr);
						wr.AddAttribute("val", ((LocationListItem)mLocations[intCount]).Value);
						wr.RenderBeginTag(HtmlTextWriterTag.Td);
						wr.Write(((LocationListItem)mLocations[intCount]).DisplayString);

						wr.RenderEndTag();
						// TD 
						wr.RenderEndTag();
						// TR 
					}

					wr.RenderEndTag();
					// </tbody> 
					wr.RenderEndTag();
					// </table> 
					wr.RenderEndTag();
					// </span> 
					wr.RenderEndTag();
					// </td>                

				// Only one object to lookup
				} else {

					wr.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
					wr.RenderBeginTag(HtmlTextWriterTag.Td);

					wr.AddAttribute(HtmlTextWriterAttribute.Id, "selLocation");
					wr.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
					try {
						wr.AddAttribute("value", ((LocationListItem)mLocations[0]).Value);
					} catch (Exception exc) {
						exc.ToString();
					}
					wr.RenderBeginTag(HtmlTextWriterTag.Input);
					wr.RenderEndTag();
					// Input
					try {
						wr.Write(((LocationListItem)mLocations[intCount]).DisplayString);
					} catch (Exception exc) {
						exc.ToString();
					}
					wr.RenderEndTag();
					// td

				}
				wr.RenderEndTag();
				// tr 
			}
		}

		private void RenderObjectClasses(HtmlTextWriter wr)
		{
			int intCount = 0;

			wr.RenderBeginTag(HtmlTextWriterTag.Tr);
			//wr.AddAttribute(HtmlTextWriterAttribute.Height, "25px")
			//wr.AddAttribute(HtmlTextWriterAttribute.Align, "right")
			wr.AddStyleAttribute("height", "25px");
			wr.AddStyleAttribute("text-align", "right");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);

			//============This render to prevent error in case we don't render location
			if (mLocations.Count == 0) {
				wr.AddAttribute(HtmlTextWriterAttribute.Id, "selLocation");
				wr.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
				wr.AddAttribute("returnvalue", "");
				wr.RenderBeginTag(HtmlTextWriterTag.Input);
				wr.RenderEndTag();
				// Input
			}
			//==========================================================================
			wr.RenderBeginTag(HtmlTextWriterTag.B);
			wr.Write(Localization.GetString("Search_Lookup", SharedResourceFile));
			wr.RenderEndTag();
			// B
			wr.RenderEndTag();
			// TD            

			// Render dropdownlist if user has to select which object to lookup

			if (mObjectClasses.Count > 1) {
				wr.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
				wr.RenderBeginTag(HtmlTextWriterTag.Td);

				wr.AddAttribute(HtmlTextWriterAttribute.Class, "SelectBox");
				wr.AddAttribute(HtmlTextWriterAttribute.Name, "selObject");
				wr.AddAttribute(HtmlTextWriterAttribute.Value, "");
				wr.AddAttribute("changehandler", "selectChange");
				wr.AddAttribute("tabbingindex", "1");
				wr.RenderBeginTag(HtmlTextWriterTag.Span);

				wr.AddAttribute(HtmlTextWriterAttribute.Style, "display: none");
				wr.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
				wr.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "2px");
				wr.RenderBeginTag(HtmlTextWriterTag.Table);
				wr.RenderBeginTag(HtmlTextWriterTag.Tbody);

				for (intCount = 0; intCount <= (mObjectClasses.Count - 1); intCount++) {
					ListItem objectClassItem = (ListItem)mObjectClasses[intCount];
					wr.RenderBeginTag(HtmlTextWriterTag.Tr);
					wr.AddAttribute("val", objectClassItem.Value);
					wr.RenderBeginTag(HtmlTextWriterTag.Td);
					wr.Write(GetLocalizedItemText(objectClassItem));
					wr.RenderEndTag();
					// td 
					wr.RenderEndTag();
					// tr 
				}

				wr.RenderEndTag();
				// </tbody> 
				wr.RenderEndTag();
				// </table> 
				wr.RenderEndTag();
				// </span> 
				wr.RenderEndTag();
				// </td>                

			// Only one object to lookup
			} else {

				ListItem objectClassItem = (ListItem)mObjectClasses[0];
				wr.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
				wr.RenderBeginTag(HtmlTextWriterTag.Td);

				wr.AddAttribute(HtmlTextWriterAttribute.Id, "selObject");
				wr.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
				wr.AddAttribute("value", objectClassItem.Value);
				wr.RenderBeginTag(HtmlTextWriterTag.Input);
				wr.RenderEndTag();
				// Input

				wr.Write("<span style=\"font-weight: bold\">" + GetLocalizedItemText(objectClassItem) + "</span>");
				wr.RenderEndTag();
				// TD

			}

			wr.RenderEndTag();
			// tr 
		}

		private void RenderSearchBox(HtmlTextWriter wr)
		{
			wr.RenderBeginTag(HtmlTextWriterTag.Tr);

			//wr.AddAttribute(HtmlTextWriterAttribute.Height, "25px")
			//wr.AddAttribute(HtmlTextWriterAttribute.Align, "right")
			wr.AddStyleAttribute("height", "25px");
			wr.AddStyleAttribute("text-align", "right");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);
			wr.RenderBeginTag(HtmlTextWriterTag.B);
			wr.Write(Localization.GetString("Search_Find", SharedResourceFile));
			wr.RenderEndTag();
			// B
			wr.RenderEndTag();
			// TD    

			//wr.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
			wr.AddStyleAttribute("width", "100%");
			wr.RenderBeginTag(HtmlTextWriterTag.Td);

			wr.AddAttribute(HtmlTextWriterAttribute.Id, "findValue");
			wr.AddAttribute("onkeydown", "javascript:findValueKeyDown(event);");
			wr.AddAttribute(HtmlTextWriterAttribute.Type, "text");
			wr.AddAttribute(HtmlTextWriterAttribute.Tabindex, "2");
			wr.AddAttribute(HtmlTextWriterAttribute.Maxlength, "100");
			wr.AddAttribute(HtmlTextWriterAttribute.Style, "border-right: #7b9ebd 1px solid; border-top: #7b9ebd 1px solid; font-size: 11px; border-left: #7b9ebd 1px solid; width: 100%; border-bottom: #7b9ebd 1px solid");
			wr.RenderBeginTag(HtmlTextWriterTag.Input);

			wr.RenderEndTag();
			// input  
			wr.RenderEndTag();
			// td           

			wr.RenderBeginTag(HtmlTextWriterTag.Td);

			wr.AddAttribute(HtmlTextWriterAttribute.Id, "btnGo");
			wr.AddAttribute(HtmlTextWriterAttribute.Title, Localization.GetString("Search_Go_info", SharedResourceFile));
			wr.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
			wr.AddAttribute(HtmlTextWriterAttribute.Onclick, "javascript:search();");

			wr.AddAttribute(HtmlTextWriterAttribute.Tabindex, "3");
			wr.AddAttribute(HtmlTextWriterAttribute.Type, "button");
			wr.RenderBeginTag(HtmlTextWriterTag.Button);
			wr.WriteLine(Localization.GetString("Search_Go", SharedResourceFile));

			wr.RenderEndTag();
			// button  
			wr.RenderEndTag();
			// td

			wr.RenderEndTag();
			// tr
		}


		private void RenderTableEnd(HtmlTextWriter wr)
		{
			wr.RenderEndTag();
			// tbody
			wr.RenderEndTag();
			// colgroup 
			wr.RenderEndTag();
			// table     

		}


		public override void Render(HtmlTextWriter writer)
		{
			RenderTableBegin(writer);
			RenderObjectClasses(writer);
			RenderLocations(writer);
			RenderSearchBox(writer);
			RenderTableEnd(writer);

		}

	}
	//PopupList

}
