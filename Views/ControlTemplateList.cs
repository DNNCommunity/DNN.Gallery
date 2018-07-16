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

using System.ComponentModel;
using System.IO;
using System.Xml;


namespace DotNetNuke.Modules.Gallery.Views
{

	[ToolboxData("<{0}:TemplateList runat=server></{0}:TemplateList>")]
	public class TemplateList : System.Web.UI.WebControls.DropDownList
	{

		public enum Type
		{
			File,
			Folder
		}


		const string NODE_TO_SEARCH = "styles/style";
		private System.Xml.XPath.XPathDocument xml;

		private System.Xml.XPath.XPathNavigator xpath;
		public TemplateList()
		{
		}
		//New


		protected override void OnDataBinding(EventArgs e)
		{
			if (!Page.IsPostBack) {
				// Add first item for empty module
				//Items.Add(New ListItem("(None Specified)", ""))

				if (string.IsNullOrEmpty(SourceDirectory))
					SourceDirectory = "DesktopModules/Gallery/";
				string _path = Path.Combine(Context.Server.MapPath(SourceDirectory), TargetName);

				switch (TargetType) {
					case Type.Folder:
						// Make sure this folder exists
						if (Directory.Exists(_path)) {
							string[] _folders = Directory.GetDirectories(_path);
							string _folder = null;
							string _folderName = null;

							foreach (string _folder_loopVariable in _folders) {
								_folder = _folder_loopVariable;
								_folderName = Path.GetFileName(_folder);
								Items.Add(new ListItem(_folderName, _folderName));
							}
						}

						break;
					//Case Type.File
					//    ' Make sure this file exists
					//    If File.Exists(_path) Then
					//        Try
					//            Dim iterator As xpath.XPathNodeIterator
					//            Dim i As Integer

					//            xml = New xpath.XPathDocument(_path)
					//            xpath = xml.CreateNavigator()

					//            iterator = xpath.Select(NODE_TO_SEARCH)

					//            For i = 1 To iterator.Count
					//                If iterator.MoveNext Then
					//                    Dim styleName As String = iterator.Current.GetAttribute("name", "")
					//                    Items.Add(New ListItem(styleName, styleName))

					//                    ' We add selectedItem check here to prevent error
					//                    'If (SelectedText.Length > 0) AndAlso (styleName = SelectedText) Then
					//                    '    _validSelect = True
					//                    'End If
					//                End If
					//            Next
					//        Catch ex As Exception
					//        End Try
					//    End If

				}
				base.OnDataBinding(e);

			}
		}

		[Bindable(true), Category("Data"), DefaultValue(Type.File)]
		public TemplateList.Type TargetType {
			get {
				//Return mTargetType
				object savedState = ViewState["TargetType"];
				if (savedState == null) {
					return Type.File;
				} else {
					return (TemplateList.Type)savedState;
				}
			}
//mTargetType = Value
			set { ViewState["TargetType"] = value; }
		}

		[Bindable(true), Category("Data"), DefaultValue("DesktopModules/Gallery/")]
		public string SourceDirectory {
			get {
				//Return mSourceDirectory
				object savedState = ViewState["SourceDirectory"];
				if (savedState == null) {
					return "DesktopModules/Gallery/";
				} else {
					return Convert.ToString(savedState);
				}
			}
			set { ViewState["SourceDirectory"] = value; }
		}

		[Bindable(true), Category("Data"), DefaultValue("_Galleryemail.xml")]
		public string TargetName {
			get {
				object savedState = ViewState["TargetName"];
				if (savedState == null) {
					return "_Galleryemail.xml";
				} else {
					return Convert.ToString(savedState);
				}
			}
			set { ViewState["TargetName"] = value; }
		}

	}

}
