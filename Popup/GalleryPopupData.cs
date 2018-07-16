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

using System.Reflection;
using DotNetNuke.Entities.Users;
using static DotNetNuke.Common.Globals;
using DotNetNuke.Security.Roles;

namespace DotNetNuke.Modules.Gallery.PopupControls
{

	public class PopupGalleryData : PopupObject
	{


		string mImageUrl;
		public PopupGalleryData(PopupData Popup) : base(Popup)
		{

			Config GalleryConfig = DotNetNuke.Modules.Gallery.Config.GetGalleryConfig(Popup.ModuleID);
			string imgURL = Common.Globals.AddHTTP(Common.Globals.GetDomainName(HttpContext.Current.Request)) + "/DesktopModules/Gallery/Popup/Images/";
			//ADSIConfig.PopupImageURL
			ArrayList searchResult = new ArrayList();
			string searchString = Popup.SearchValue;
			string searchLocation = Popup.Location;
			int sID = 0;
			string sName = "";
			PopupListItem popupItem = null;

			try {
				switch (Popup.ObjectClass) {

					case ObjectClass.DNNRole:
						mImageUrl = imgURL + "sp_team.gif";

						RoleController ctlRole = new RoleController();
						searchResult = ctlRole.GetPortalRoles(Popup.PortalID);

						// JIMJ Add role glbRoleAllUsersName
						RoleInfo R = new RoleInfo();
						R.RoleID = Convert.ToInt32(Common.Globals.glbRoleAllUsers);
						R.RoleName = Common.Globals.glbRoleAllUsersName;
						searchResult.Add(R);

						RoleInfo role = null;
						foreach (RoleInfo role_loopVariable in searchResult) {
							role = role_loopVariable;
							if (role.RoleName.ToLower().StartsWith(searchString.ToLower()) || (searchString.Length == 0)) {
								sID = role.RoleID;
								sName = role.RoleName;
								popupItem = new PopupListItem(sID.ToString(), (int)ObjectClass.DNNRole, sName, mImageUrl, 1);
								Popup.ListItems.Add(popupItem);
							}
						}


						break;
					case ObjectClass.DNNUser:
						mImageUrl = imgURL + "sp_user.gif";
						searchResult = UserController.GetUsers(Popup.PortalID);

						// JIMJ Changes to add the names correctly
						UserInfo user = null;
						foreach (UserInfo user_loopVariable in searchResult) {
							user = user_loopVariable;
							sID = user.UserID;
							sName = user.Username;

							// See if we can find the user or if we want everything
							if ((sName.ToLower().StartsWith(searchString.ToLower())) || (searchString.Length == 0)) {
								popupItem = new PopupListItem(sID.ToString(), (int)ObjectClass.DNNUser, user.Username, mImageUrl, 1);

								popupItem.AddProperty("Full Name", user.Profile.FullName, 200);
								Popup.ListItems.Add(popupItem);
							}
						}


						break;
				}

			} catch (Exception exc) {
				Exceptions.LogException(exc);
			}

		}

		public override void Render(HtmlTextWriter writer)
		{
			RenderPopup objRender = new RenderPopup(this.PopupDataControl);
			objRender.Render(writer);
		}

		public override void CreateChildControls()
		{
		}
		//CreateChildControls

		public override void OnPreRender()
		{
		}

	}
	//PopupList

}
