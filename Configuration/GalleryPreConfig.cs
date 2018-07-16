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

using DotNetNuke.Entities.Modules;
using static DotNetNuke.Common.Globals;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Modules.Gallery
{

	public class GalleryPreConfig
	{

		public static void PreConfig(int ModuleId, int UserId)
		{
			// Update settings in the database
			ModuleController ctlModule = new ModuleController();
			string GalleryURLPortalHome = Config.DefaultRootURL + ModuleId.ToString() + "/";

			ctlModule.UpdateModuleSetting(ModuleId, "Provider", "");
			ctlModule.UpdateModuleSetting(ModuleId, "Type", "Gallery");
			ctlModule.UpdateModuleSetting(ModuleId, "GalleryTitle", Config.DefaultGalleryTitle);
			ctlModule.UpdateModuleSetting(ModuleId, "InitialGalleryHierarchy", Config.DefaultInitialGalleryHierarchy);
			ctlModule.UpdateModuleSetting(ModuleId, "GalleryDescription", Config.DefaultGalleryDescription);
			ctlModule.UpdateModuleSetting(ModuleId, "RootURL", GalleryURLPortalHome);
			ctlModule.UpdateModuleSetting(ModuleId, "Theme", Config.DefaultTheme);
			ctlModule.UpdateModuleSetting(ModuleId, "StripWidth", Config.DefaultStripWidth.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "StripHeight", Config.DefaultStripHeight.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "Quota", Config.DefaultQuota.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "MaxFileSize", Config.DefaultMaxFileSize.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "MaxPendingUploadsSize", Config.DefaultMaxPendingUploadsSize.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "MaxThumbWidth", Config.DefaultMaxThumbWidth.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "MaxThumbHeight", Config.DefaultMaxThumbHeight.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "CategoryValues", Config.DefaultCategoryValues);
			ctlModule.UpdateModuleSetting(ModuleId, "BuildCacheOnStart", Config.DefaultBuildCacheOnStart.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "FixedWidth", Config.DefaultFixedWidth.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "FixedHeight", Config.DefaultFixedHeight.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "EncoderQuality", Config.DefaultEncoderQuality.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "SlideshowSpeed", Config.DefaultSlideshowSpeed.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "IsPrivate", Config.DefaultIsPrivate.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "MultiLevelMenu", Config.DefaultMultiLevelMenu.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "AllowSlideshow", Config.DefaultAllowSlideShow.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "AllowPopup", Config.DefaultAllowPopup.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "AllowDownload", Config.DefaultAllowDownload.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "AllowVoting", Config.DefaultAllowVoting.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "AllowExif", Config.DefaultAllowExif.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "UseWatermark", Config.DefaultUseWatermark.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "AutoApproval", Config.DefaultAutoApproval.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "OwnerID", Config.DefaultOwnerId.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "CreatedDate", DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture));
			ctlModule.UpdateModuleSetting(ModuleId, "DefaultSortDESC", Config.DefaultDefaultSortDESC.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "DefaultSort", Config.DefaultDefaultSort.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "DefaultView", Config.DefaultDefaultView.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "AllowChangeView", Config.DefaultAllowChangeView.ToString());
			ctlModule.UpdateModuleSetting(ModuleId, "TextDisplayValues", Config.DefaultTextDisplayValues);
			ctlModule.UpdateModuleSetting(ModuleId, "DownloadRoles", Config.DefaultDownloadRoles);
			ctlModule.UpdateModuleSetting(ModuleId, "SortPropertyValues", Config.DefaultSortPropertyValues);
		}
	}
}
