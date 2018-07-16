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

using DotNetNuke.Services.Search;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Modules.Gallery
{

	public class Controller : ISearchable, IUpgradeable
	{


		private const int MAX_DESCRIPTION_LENGTH = 100;
		public Services.Search.SearchItemInfoCollection GetSearchItems(Entities.Modules.ModuleInfo ModInfo)
		{

			SearchItemInfoCollection SearchItemCollection = new SearchItemInfoCollection();
			//Dim config As DotNetNuke.Modules.Gallery.Config = config.GetGalleryConfig(ModInfo.ModuleID)
			//PopulateSearch(config.RootFolder, SearchItemCollection)
			return SearchItemCollection;

		}

		// When BuildCacheOnStart is set to true, this recursively populates the folder objects
		public static void PopulateSearch(GalleryFolder rootFolder, SearchItemInfoCollection SearchCollection)
		{
			object folder = null;

			if (!rootFolder.IsPopulated) {
				rootFolder.Populate(false);
				foreach (IGalleryObjectInfo item in rootFolder.List) {
					string strDescription = HtmlUtils.Shorten(HtmlUtils.Clean(item.Description, false), MAX_DESCRIPTION_LENGTH, "...");
					SearchItemInfo SearchItem = new SearchItemInfo(item.Title, item.ItemInfo, item.OwnerID, item.CreatedDate, rootFolder.GalleryConfig.ModuleID, item.Name, strDescription, item.ID);
					SearchCollection.Add(SearchItem);
				}
			}

			foreach (object folder_loopVariable in rootFolder.List) {
				folder = folder_loopVariable;
				if (folder is GalleryFolder && !((GalleryFolder)folder).IsPopulated) {
					((GalleryFolder)folder).Populate(false);
					PopulateSearch((GalleryFolder)folder, SearchCollection);
				}
			}
		}

		public string UpgradeModule(string Version)
		{
			string Result = Version;
			if (Version == "04.03.03") {
				//Truncate the value of setting RootURL for all instances of the module to be relative to the portal's
				//home directory as fix to invalid RootURL when migrating site to a new url

				DesktopModuleInfo dmi = DesktopModuleController.GetDesktopModuleByModuleName("DNN_Gallery", -1);
				if (dmi == null) {
					Result += " - Upgrade module(v 04.03.03) failure - unable to obtain DesktopModuleID for module name DNN_Gallery";
				} else {
					DotNetNuke.Entities.Modules.Definitions.ModuleDefinitionInfo mdi = DotNetNuke.Entities.Modules.Definitions.ModuleDefinitionController.GetModuleDefinitionByFriendlyName(dmi.FriendlyName, dmi.DesktopModuleID);
					if (mdi == null) {
						Result += (" - Upgrade module failure - unable to obtain ModuleDefinitionID for module definition friendly name " + dmi.FriendlyName);
					} else {
						ModuleInfo mi = null;
						ModuleController mc = new ModuleController();
						Regex rgx = new Regex("^(.*Portals/\\d+/)(.+)$", RegexOptions.IgnoreCase);
						int cntFixed = 0;
						int cntProcessed = 0;
						foreach (ModuleInfo mi_loopVariable in mc.GetAllModules()) {
							mi = mi_loopVariable;
							if (mi.ModuleDefID == mdi.ModuleDefID) {
								Hashtable settings = mc.GetModuleSettings(mi.ModuleID);
								string rootURL = Utils.GetValue(settings["RootURL"], "");
								if (!string.IsNullOrEmpty(rootURL)) {
									string rootURLFixed = rgx.Replace(rootURL, "$2");
									if (rootURLFixed != rootURL) {
										mc.UpdateModuleSetting(mi.ModuleID, "RootURL", rootURLFixed);
										cntFixed += 1;
									}
									cntProcessed += 1;
								}
							}
						}
						Result += string.Format(" - Gallery modules upgraded: {0} of {1}", cntFixed, cntProcessed);
					}
				}
			}
			return Result;
		}
	}
}
