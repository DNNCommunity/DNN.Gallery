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

using System.Reflection;
using System.IO;
using DotNetNuke.Entities.Portals;
using System.Drawing;
using DotNetNuke.Entities.Users;

namespace DotNetNuke.Modules.Gallery
{

	// Utility routines for various repeated functions.   

	public class Utils
	{
		public const string GALLERY_VIEW = "galleryview";
		public const string GALLERY_SORT = "gallerysort";

		public const string GALLERY_SORT_DESC = "gallerysortdesc";
		//William Severance - modified to call new overload which allows specification of
		//the depth of sub-albums to which folders will be populated to fix Gemini
		//issue GAL-6168. This modification enables population on demand when the configuration
		//option BuildCacheOnStart is false. Note that because of the manner in which the cover photos
		//of albums are set, the current folder AND all sub-folders of the current folder must be
		//populated.

		public static void PopulateAllFolders(GalleryFolder rootFolder, bool ReSync)
		{
			//Populate all folders to maximum depth
			PopulateAllFolders(rootFolder, int.MaxValue, false);
		}


		public static void PopulateAllFolders(GalleryFolder rootFolder, int Depth, bool ReSync)
		{
			if (Depth < 1)
				return;

			if (!rootFolder.IsPopulated) {
				rootFolder.Populate(ReSync);
			}

			foreach (object folder in rootFolder.List) {
				if (folder is GalleryFolder && !((GalleryFolder)folder).IsPopulated) {
					((GalleryFolder)folder).Populate(ReSync);
					if (Depth > 1)
						PopulateAllFolders((GalleryFolder)folder, Depth - 1, ReSync);
				}
			}

		}

		public static List<GalleryFolder> GetChildFolders(GalleryFolder rootFolder, int depth)
		{
			List<GalleryFolder> folders = new List<GalleryFolder>();
			if (!rootFolder.IsPopulated) {
				rootFolder.Populate(true);
			}
			foreach (IGalleryObjectInfo child in rootFolder.List) {
				if (child.IsFolder) {
					GalleryFolder folder = (GalleryFolder)child;
					folders.Add(folder);
					if (depth > 1) {
						folders.AddRange(GetChildFolders(folder, depth - 1));
					}
				}
			}
			return folders;
		}

		public static GalleryFolder GetRootRelativeFolder(GalleryFolder rootFolder, string folderPath)
		{
			GalleryFolder folder = rootFolder;

			if (!string.IsNullOrEmpty(folderPath)) {
				try {
					string[] paths = Strings.Split(folderPath, "/");
					for (int depth = 0; depth <= paths.Length - 1; depth++) {
						folder = (GalleryFolder)folder.List.Item(paths[depth]);
						if (!folder.IsPopulated)
							folder.Populate(false);
					}
				} catch (Exception ex) {
					folder = null;
					//folder not found/invalid folderPath
				}
			}
			return folder;
		}

		public static string AddHost(string URL)
		{
			string strReturn = null;
			string strHost = HttpContext.Current.Request.ServerVariables["HTTP_HOST"];

			// Check if this URL has already included host
			if (!(URL.ToLower().IndexOf(strHost.ToLower()) >= 0)) {
				if (strHost.EndsWith("/")) {
					strHost = strHost.TrimEnd('/');
				}
				if (!URL.StartsWith("/")) {
					URL = "/" + URL;
				}
				strReturn = Common.Globals.AddHTTP(strHost + URL);
			} else {
				strReturn = URL;
			}
			string test = Path.Combine(strHost, URL);
			return strReturn;

		}

		public static string BuildPopup(string URL, int PageHeight, int PageWidth)
		{
			StringBuilder sb = new StringBuilder();
			//javascript:window.open
			sb.Append("javascript:DestroyWnd;CreateWnd('");
			//  sb.Append("javascript:CreateWnd('")
			sb.Append(URL);
			sb.Append("', ");
			sb.Append(Convert.ToString(PageWidth));
			sb.Append(", ");
			sb.Append(Convert.ToString(PageHeight));
			sb.Append(", true);");
			return sb.ToString();
		}

		private static Hashtable CreateHashtableFromQueryString(Page page)
		{
			Hashtable ht = new Hashtable();

			string query = null;
			foreach (string query_loopVariable in page.Request.QueryString) {
				query = query_loopVariable;
				if (Strings.Len(query) > 0) {
					ht.Add(query, page.Request.QueryString[query]);
				}
			}
			return ht;
		}
		//CreateHashtableFromQueryString

		private static string CreateQueryString(Hashtable current, Hashtable @add, Hashtable @remove)
		{
			IDictionaryEnumerator myEnumerator = @add.GetEnumerator();
			while (myEnumerator.MoveNext()) {
				if (current.ContainsKey(myEnumerator.Key)) {
					current.Remove(myEnumerator.Key);
				}
				current.Add(myEnumerator.Key, myEnumerator.Value);
			}

			myEnumerator = @remove.GetEnumerator();
			while (myEnumerator.MoveNext()) {
				string removeKey = Convert.ToString(myEnumerator.Key);

				if (current.ContainsKey(removeKey)) {
					string removeValue = Convert.ToString(myEnumerator.Value);

					if (Convert.ToString(current[removeKey]) == removeValue || removeValue == string.Empty) {
						current.Remove(removeKey);
					}
				}
			}
			int count = 0;
			StringBuilder sb = new StringBuilder();
			myEnumerator = current.GetEnumerator();
			while (myEnumerator.MoveNext()) {
				if (count == 0) {
					sb.Append("?");
				} else {
					sb.Append("&");
				}
				sb.Append(myEnumerator.Key);
				sb.Append("=");
				sb.Append(myEnumerator.Value);
				count += 1;
			}

			return sb.ToString();
		}
		//CreateQueryString

		private static Hashtable CreateHashtableFromQueryString(string query)
		{
			Hashtable ht = new Hashtable();

			int startIndex = 0;
			while (startIndex >= 0) {
				int oldStartIndex = startIndex;
				int equalIndex = query.IndexOf("=", startIndex);
				startIndex = query.IndexOf("&", startIndex);
				if (startIndex >= 0) {
					startIndex += 1;
				}
				if (equalIndex >= 0) {
					int lengthValue = 0;
					if (startIndex >= 0) {
						lengthValue = startIndex - equalIndex - 2;
					} else {
						lengthValue = query.Length - equalIndex - 1;
					}
					string key = query.Substring(oldStartIndex, equalIndex - oldStartIndex);
					string val = query.Substring(equalIndex + 1, lengthValue);

					ht.Add(key, val);
				}
			}

			return ht;
		}
		//CreateHashtableFromQueryString

		public static string GetURL(string baseURL, Page page, string @add, string @remove)
		{
			if (@remove != string.Empty) {
				@remove += "&";
			}
			@remove += "DocumentID=";

			Hashtable currentQueries = CreateHashtableFromQueryString(page);
			Hashtable addQueries = CreateHashtableFromQueryString(@add);
			Hashtable removeQueries = CreateHashtableFromQueryString(@remove);

			string newQueryString = CreateQueryString(currentQueries, addQueries, removeQueries);

			return baseURL + newQueryString;
		}
		//GetURL

		public static string GetURL(string URL, string[] AdditionalParameters)
		{
			StringBuilder sb = new StringBuilder(URL);

			if (URL.Contains("?")) {
				sb.Append("&");
			} else {
				sb.Append("?");
			}

			// Check if tabid param exists
			if (!URL.ToLower().Contains("tabid") & (AdditionalParameters == null || !Array.Exists(AdditionalParameters, MatchTabId))) {
				PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
				sb.Append("tabid=" + _portalSettings.ActiveTab.TabID.ToString() + "&");
			}
			foreach (string param in AdditionalParameters) {
				if (!string.IsNullOrEmpty(param.Split('=')[1]))
					sb.Append(param + "&");
			}
			sb.Length -= 1;
			return sb.ToString();

		}

		private static bool MatchTabId(string param)
		{
			return param.Contains("tabid");
		}

		// Provides more functionality than the path object static functions
		public static string BuildPath(string[] Input, string Delimiter, bool StripInitial, bool StripFinal)
		{
			StringBuilder output = new StringBuilder();

			output.Append(Strings.Join(Input, Delimiter));

			// JIMJ
			// If the beginning of the output begins with \\ 
			// then we are dealing with a UNC path.  
			// Double the beginning slashes (\\) to compensate for the following Replace call.
			if (output.ToString().Substring(0, 2) == "\\\\") {
				output.Insert(0, "\\\\");
			}

			output.Replace(Delimiter + Delimiter, Delimiter);

			if (StripInitial) {
				if (Strings.Left(output.ToString(), Strings.Len(Delimiter)) == Delimiter) {
					output.Remove(0, Strings.Len(Delimiter));
				}
			}

			if (StripFinal) {
				if (Strings.Right(output.ToString(), Strings.Len(Delimiter)) == Delimiter) {
					output.Remove(output.Length - Strings.Len(Delimiter), Strings.Len(Delimiter));
				}
			}

			return output.ToString();

		}

		// Alternate signature for the BuildPath function
		public static string BuildPath(string[] Input, string Delimiter)
		{

			return BuildPath(Input, Delimiter, true, false);

		}

		public static string SolpartEncode(string Source)
		{
			// If it has been encode in javascript, no need to do it again
			if (Source.IndexOf("javascript") > -1) {
				return Source;
			}
			return JSEncode(Source);

		}

		public static string JSEncode(string Source)
		{
			Source = Source.Replace("'", "\\'");
			//<JJK. handle double quotes as well. /> 
			Source = Source.Replace(Strings.Chr(34).ToString(), "\\" + Strings.Chr(34).ToString());
			Source = Source.Replace("/", "\\/");
			//Source = ReplaceCaseInsensitive(Source, "/", "|")
			return Source;
		}

		public static string JSDecode(string Source)
		{
			//Source = ReplaceCaseInsensitive(Source, "^", "'") 
			//<JJK. handle double quotes as well. /> 
			Source = Source.Replace("\\'", "'");
			Source = Source.Replace("\\" + Strings.Chr(34).ToString(), Strings.Chr(34).ToString());
			Source = Source.Replace("\\/", "/");
			// to solve problem with friendlyURL
			//Source = ReplaceCaseInsensitive(Source, "|", "/")
			return Source;
		}

		public static string FriendlyURLEncode(string Source)
		{
			string output = HttpUtility.UrlEncodeUnicode(Source).Replace("/", "!");
			return output;
		}

		public static string FriendlyURLDecode(string Source)
		{
			string output = HttpUtility.UrlDecode(Source).Replace("!", "/");
			if (output == " ") {
				output = "";
			}
			return output;
		}

		public static string GetValue(object Input, string DefaultValue)
		{
			// Used to determine if a valid input is provided, if not, return default value
			if (Input == null) {
				return DefaultValue;
			} else {
				return Convert.ToString(Input);
			}
		}

		public static int ValidUserID(int UserID)
		{
			PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();

			UserInfo user = new UserController().GetUser(_portalSettings.PortalId, UserID);

			if (((user != null)) && (user.IsSuperUser)) {
				UserID = _portalSettings.AdministratorId;
			}

			return UserID;

		}

		public static string UploadFileInfo(int ModuleID, long SpaceUsed, long SpaceAvailable)
		{
			DotNetNuke.Modules.Gallery.Config GalleryConfig = Config.GetGalleryConfig(ModuleID);
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			sb.Append("<p class=\"Normal\">");
			sb.Append("<img alt='' src='" + GalleryConfig.GetImageURL("s_zip.gif") + "' />&nbsp;");
			sb.Append("Zip&nbsp;(.zip) ");
			sb.Append("&nbsp;&nbsp;<img alt='' src='" + GalleryConfig.GetImageURL("s_jpg.gif") + "' />&nbsp;");
			sb.Append(Localization.GetString("Image", GalleryConfig.SharedResourceFile));
			//<daniel file extension to use core>
			sb.Append("&nbsp;(" + Strings.Replace(GalleryConfig.FileExtensions, ";", ", "));
			if (sb[sb.Length - 1] == ',')
				sb.Remove(sb.Length - 1, 1);
			sb.Append(")");
			sb.Append("&nbsp;&nbsp;<img alt='' src='" + GalleryConfig.GetImageURL("s_mediaplayer.gif") + "' />&nbsp;");
			sb.Append(Localization.GetString("Movie", GalleryConfig.SharedResourceFile));
			//<daniel file extension to use core>
			sb.Append("&nbsp;(" + Strings.Replace(GalleryConfig.MovieExtensions, ";", ", "));
			if (sb[sb.Length - 1] == ',')
				sb.Remove(sb.Length - 1, 1);
			sb.Append(")");
			sb.Append("&nbsp;&nbsp;<img alt='' src='" + GalleryConfig.GetImageURL("s_flash.gif") + "' />&nbsp;");
			sb.Append(Localization.GetString("Flash", GalleryConfig.SharedResourceFile));
			sb.Append("&nbsp;(.swf)</p><p class=\"Normal\">");
			sb.Append(Localization.GetString("MaxFileSize", GalleryConfig.SharedResourceFile));
			sb.Append("&nbsp;" + GalleryConfig.MaxFileSize.ToString() + " kb <br />");
			sb.Append(Localization.GetString("Gallery_Quota", GalleryConfig.SharedResourceFile) + "&nbsp;");
			if (GalleryConfig.Quota == 0) {
				sb.Append(Localization.GetString("No_Gallery_Quota_Imposed", GalleryConfig.SharedResourceFile) + "<br />");
			} else {
				sb.Append(GalleryConfig.Quota.ToString() + " kb <br />");
			}
			sb.Append(Localization.GetString("Portal_Quota", GalleryConfig.SharedResourceFile) + "&nbsp;");
			PortalSettings ps = PortalController.GetCurrentPortalSettings();
			if (ps.HostSpace == 0) {
				sb.Append(Localization.GetString("No_Portal_Quota_Imposed", GalleryConfig.SharedResourceFile) + "<br />");
			} else {
				sb.Append((ps.HostSpace * 1024).ToString() + " kb <br />");
			}
			sb.Append(Localization.GetString("Space_Used", GalleryConfig.SharedResourceFile) + "&nbsp;");
			sb.AppendFormat("{0:F} kb <br />", (SpaceUsed / 1024));
			sb.Append(Localization.GetString("Space_Available", GalleryConfig.SharedResourceFile) + "&nbsp;");
			if (SpaceAvailable == long.MinValue) {
				sb.Append(Localization.GetString("No_Quota_Imposed", GalleryConfig.SharedResourceFile));
			} else {
				if (SpaceAvailable <= 0) {
					sb.AppendFormat(Localization.GetString("Space_Available_Exceeded_By", GalleryConfig.SharedResourceFile), -(SpaceAvailable / 1024));
				} else {
					sb.AppendFormat("{0:F} kb", (SpaceAvailable / 1024));
				}

			}
			sb.Append("</p>");
			return sb.ToString();
		}

		// From 3.0 version using cookies to store view and sort order
		// William Severance - modified to use configuration settings DefaultView, DefaultSort and DefaultSortDESC
		// for the initial view/sort order for first-time visitor and to relate cookies values to specific ModuleID to avoid
		// interaction of these settings between multiple Gallery modules in same portal (same domain name actually).
		// Note that if user is not permitted to change view, the configuration DefaultView will be returned and no
		// cookie will be used to maintain the current view.

		public static Config.GalleryView GetView(Config GalleryConfiguration)
		{

			if (GalleryConfiguration.AllowChangeView) {
				HttpCookie cookie = HttpContext.Current.Request.Cookies[GALLERY_VIEW];

				if ((cookie != null)) {
					string value = cookie.Values[GalleryConfiguration.ModuleID.ToString()];
					if (!string.IsNullOrEmpty(value)) {
						try {
							return (Config.GalleryView)Enum.Parse(typeof(Config.GalleryView), value);
						} catch {
						}
					}
				}
			}
			return GalleryConfiguration.DefaultView;

		}

		public static Config.GallerySort GetSort(Config GalleryConfiguration)
		{

			HttpCookie cookie = HttpContext.Current.Request.Cookies[GALLERY_SORT];

			if ((cookie != null)) {
				string value = cookie.Values[GalleryConfiguration.ModuleID.ToString()];
				if (!string.IsNullOrEmpty(value)) {
					try {
						return (Config.GallerySort)Enum.Parse(typeof(Config.GallerySort), value);
					} catch {
					}
				}
			}
			return GalleryConfiguration.DefaultSort;
		}

		public static bool GetSortDESC(Config GalleryConfiguration)
		{
			HttpCookie cookie = HttpContext.Current.Request.Cookies[GALLERY_SORT_DESC];

			if ((cookie != null)) {
				string value = cookie.Values[GalleryConfiguration.ModuleID.ToString()];
				if (!string.IsNullOrEmpty(value)) {
					bool sortDESC = false;
					if (bool.TryParse(value, out sortDESC))
						return sortDESC;
				}
			}
			return GalleryConfiguration.DefaultSortDESC;
		}

		public static void RefreshCookie(string Name, int ModuleID, object Value)
		{
			HttpCookie cookie = HttpContext.Current.Request.Cookies[Name];
			if (cookie == null) {
				cookie = new HttpCookie(Name);
				cookie.Expires = DateTime.MaxValue;
				// never expires
				HttpContext.Current.Response.AppendCookie(cookie);
			}
			cookie[ModuleID.ToString()] = Convert.ToString(Value);
			HttpContext.Current.Response.SetCookie(cookie);
		}

		/// <summary>
		/// Added by Stefan Cullman to workaround a Core problem in FURL's
		/// </summary>
		/// <param name="params">The parameters in the request querystring</param>
		/// <returns>a string of parameters with the empty parameter cleaned up</returns>
		/// <remarks>This is placed inside methods that use params from the qs</remarks>
		public static string[] RemoveEmptyParams(string[] @params)
		{
			System.Collections.Generic.List<string> newparams = new System.Collections.Generic.List<string>();
			foreach (string parameter in @params) {
				if (!parameter.EndsWith("="))
					newparams.Add(parameter);
			}
			return newparams.ToArray();
		}

		//William Severance - added method to append a parameter to an existing URL
		//                  - modified to fix GAL-8352 to remove Uri class constructor as it required
		//                  - absolute not relative URL

		public static string AppendURLParameter(string URL, string param)
		{
			if (param == null || param.Length == 0) {
				return URL;
			} else {
				if (URL.Contains("?")) {
					return string.Concat(URL, "&", param);
				} else {
					return string.Concat(URL, "?", param);
				}
			}
		}

		public static string AppendURLParameters(string URL, string[] @params)
		{
			if (@params == null || @params.Length < 1) {
				return URL;
			} else {
				StringBuilder sb = new StringBuilder(URL);
				if (URL.Contains("?")) {
					sb.Append("&");
				} else {
					sb.Append("?");
				}
				foreach (string param in @params) {
					if (!string.IsNullOrEmpty(param.Split('=')[1]))
						sb.Append(param + "&");
				}
				sb.Length -= 1;
				return sb.ToString();
			}
		}

		//William Severance - added methods to handle formatting/parsing of dates where Date.MaxValue
		//signifies the date not being set

		public static string DateToText(DateTime d, string format)
		{
			if (d == System.DateTime.MaxValue) {
				return string.Empty;
			} else {
				return string.Format(format, d);
			}
		}

		public static string DateToText(DateTime d)
		{
			if (d == System.DateTime.MaxValue) {
				return string.Empty;
			} else {
				return d.ToShortDateString();
			}
		}

		public static DateTime TextToDate(string s, IFormatProvider provider)
		{
			if (string.IsNullOrEmpty(s)) {
				return DateTime.MaxValue;
			} else {
				return DateTime.Parse(s, provider);
			}
		}

		public static DateTime TextToDate(string s)
		{
			if (string.IsNullOrEmpty(s)) {
				return DateTime.MaxValue;
			} else {
				return DateTime.Parse(s);
			}
		}

		// Added by Andrew Galbraith Ryer for use in Quota checking.
		public static long GetDirectorySize(DirectoryInfo di)
		{
			long size = 0L;

			foreach (FileInfo fi in di.GetFiles()) {
				size += fi.Length;
			}

			foreach (DirectoryInfo subdir in di.GetDirectories()) {
				size += GetDirectorySize(subdir);
			}

			return size;
		}

		//Overload added by William Severance
		public static long GetDirectorySize(string path)
		{
			DirectoryInfo di = new DirectoryInfo(path);
			if (di.Exists) {
				return GetDirectorySize(di);
			} else {
				return 0L;
			}
		}

		//Added by William Severance - used in various file header analysis classes
		public static int ReadAllBytesFromStream(Stream s, byte[] buffer)
		{
			int offset = 0;
			int bytesRead = 0;
			int totalCount = 0;
			while ((true)) {
				bytesRead = s.Read(buffer, offset, 100);
				if (bytesRead == 0)
					break; // TODO: might not be correct. Was : Exit While
				offset += bytesRead;
				totalCount += bytesRead;
			}
			return totalCount;
		}

		//Added by William Severance - used in various file header analysis classes
		//Converts one or two byte value in LSB..MSB (little endian) order to Int32. n must be 1 or 2
		public static Int32 ConvertToInt32(byte[] bytes, int n)
		{
			//Read next 1 or 2 bytes LSB...MSB (little endian) as Int32
			if (n < 1 | n > 2) {
				throw new ArgumentOutOfRangeException("n", "nBytes must be 1 or 2");
			} else {
				Int32 val = 0;
				int i = 0;
				int b = 0;
				for (i = 0; i <= n - 1; i++) {
					b = bytes[i];
					val += (b << (8 * i));
				}
				return val;
			}
		}

		//Added by William Severance - used in various file header analysis classes
		//Converts one, two, three or four byte value in LSB..MSB (little endian) order to Int64. n must be 1-4
		public static Int64 ConvertToInt64(byte[] bytes, int n)
		{
			//Read next 1 or 2 bytes LSB...MSB (little endian) as Int32
			if (n < 1 | n > 4) {
				throw new ArgumentOutOfRangeException("n", "nBytes must be 1 through 4");
			} else {
				Int64 val = 0L;
				int i = 0;
				int b = 0;
				for (i = 0; i <= n - 1; i++) {
					b = bytes[i];
					val += (b << (8 * i));
				}
				return val;
			}
		}

		//Added by William Severance - used in various file header analysis classes
		//Converts 4 byte (32 bit) MS-DOS File Time/Date to DateTime
		public static DateTime ConvertToDateTime(byte[] bytes)
		{

			int[] parts = {
				5,
				6,
				5,
				5,
				4,
				7
			};
			int partPtr = 0;
			int val = 0;
			int yr = 0;
			int mo = 0;
			int da = 0;
			int hr = 0;
			int min = 0;
			int sec = 0;

			int bytePtr = 0;
			Int64 workingReg = 0;
			Int64 mask = default(Int64);

			if (bytes == null || bytes.Length != 4)
				return DateTime.MinValue;

			for (bytePtr = 0; bytePtr <= 3; bytePtr++) {
				Int64 b = bytes[bytePtr];
				workingReg += (b << (8 * bytePtr));
			}
			for (partPtr = 0; partPtr <= 5; partPtr++) {
				mask = Convert.ToInt64(Math.Pow(2, (parts[partPtr])) - 1);
				val = Convert.ToInt32(workingReg & mask);
				workingReg >>= parts[partPtr];
				switch (partPtr) {
					case 0:
						sec = val;
						break;
					case 1:
						min = val;
						break;
					case 2:
						hr = val;
						break;
					case 3:
						da = val;
						break;
					case 4:
						mo = val;
						break;
					case 5:
						yr = val;
						break;
				}
			}
			return new DateTime(yr + 1980, mo, da, hr, min, sec);
		}

		//Added by William Severance to facilite fetch of ItemType and FileIcon
		//Returns the item type by reference as ItemType
		//and Icon url as function return value
		public static string GetFileTypeIcon(string Ext, Config GalleryConfig, ref Config.ItemType ItemType)
		{

			string mImageURL = GalleryConfig.GetImageURL("");
			string mIcon = string.Empty;

			if (GalleryConfig.IsValidFlashType(Ext)) {
				ItemType = Config.ItemType.Flash;
				mIcon = mImageURL + "s_flash.gif";
			} else if (GalleryConfig.IsValidImageType(Ext)) {
				ItemType = Config.ItemType.Image;
				mIcon = mImageURL + "s_jpg.gif";
			} else if (GalleryConfig.IsValidMovieType(Ext)) {
				ItemType = Config.ItemType.Movie;
				mIcon = mImageURL + "s_mediaplayer.gif";
			} else if (Ext == ".zip") {
				ItemType = Config.ItemType.Zip;
				mIcon = mImageURL + "s_zip.gif";
			}
			return mIcon;
		}

		//William Severance - added method to add file and optionally a non-existing folder to the DNN files and or folders table(s)
		//If AddNonExistingFolder is false, sub exits without adding file if folder is missing from DNN folders table, else will first
		//create an entry in the folders table inheriting the parent folder's permissions.

		public static int SaveDNNFile(string Filepath, int Width, int Height, bool AddNonExistingFolder, bool ClearCache)
		{
			int fileId = -1;

			if (!string.IsNullOrEmpty(Filepath)) {
				DotNetNuke.Entities.Portals.PortalSettings ps = DotNetNuke.Entities.Portals.PortalController.GetCurrentPortalSettings();
				System.IO.FileInfo fi = new System.IO.FileInfo(Filepath);
				if (fi.Exists) {
					string folderPath = fi.Directory.FullName.Replace(ps.HomeDirectoryMapPath, "").Replace("\\", "/");
					string fileName = fi.Name;
					string fileExt = fi.Extension.TrimStart('.').ToLower();
					long fileSize = fi.Length;

					int PortalId = ps.PortalId;

					int folderId = -1;
					Services.FileSystem.FolderController objFolderController = new Services.FileSystem.FolderController();
					Services.FileSystem.FolderInfo objFolderInfo = objFolderController.GetFolder(PortalId, folderPath, false);
					//Is the folder already in the DNN folders table? If not, should we create it and continue?
					if (objFolderInfo == null) {
						if (AddNonExistingFolder) {
							objFolderInfo = new Services.FileSystem.FolderInfo();
							var _with1 = objFolderInfo;
							_with1.UniqueId = Guid.NewGuid();
							_with1.VersionGuid = Guid.NewGuid();
							_with1.PortalID = PortalId;
							_with1.FolderPath = folderPath;
							_with1.StorageLocation = (int)DotNetNuke.Services.FileSystem.FolderController.StorageLocationTypes.InsecureFileSystem;
							_with1.IsProtected = false;
							_with1.IsCached = false;
							folderId = objFolderController.AddFolder(objFolderInfo);
							if (folderId != -1) {
								//WES - Set Folder Permissions to inherit from parent
								FileSystemUtils.SetFolderPermissions(PortalId, folderId, folderPath);
							}
						}
					} else {
						folderId = objFolderInfo.FolderID;
					}
					if (folderId != -1) {
						Services.FileSystem.FileController objFileController = new Services.FileSystem.FileController();
						Services.FileSystem.FileInfo objFileInfo = objFileController.GetFile(fileName, PortalId, folderId);

						string contentType = FileSystemUtils.GetContentType(fileExt);
						if (contentType.StartsWith("image") && (Width == 0 || Height == 0)) {
							Bitmap img = null;
							try {
								img = new Bitmap(Filepath);
								Width = img.Width;
								Height = img.Height;
							} catch {
								//eat the exception if we can load bitmap - will have width=height=0
							} finally {
								if ((img != null))
									img.Dispose();
								img = null;
							}
						}
						//Is the file already in the DNN files table in which case we update rather than add
						if (objFileInfo == null) {
							objFileInfo = new Services.FileSystem.FileInfo();
							var _with2 = objFileInfo;
							_with2.PortalId = PortalId;
							_with2.FileName = fileName;
							_with2.Extension = fileExt;
							_with2.Size = Convert.ToInt32(fileSize);
							_with2.Width = Width;
							_with2.Height = Height;
							_with2.ContentType = contentType;
							_with2.Folder = FileSystemUtils.FormatFolderPath(folderPath);
							_with2.FolderId = folderId;
							_with2.IsCached = ClearCache;
							_with2.UniqueId = Guid.NewGuid();
							_with2.VersionGuid = Guid.NewGuid();
							fileId = objFileController.AddFile(objFileInfo);
							if (ClearCache) {
								objFileController.GetAllFilesRemoveCache();
							}
						} else {
							var _with3 = objFileInfo;
							_with3.Size = Convert.ToInt32(fileSize);
							_with3.Width = Width;
							_with3.Height = Height;
							fileId = objFileInfo.FileId;
							objFileController.UpdateFile(objFileInfo);
						}
					}
				}
			}
			return fileId;
		}

		// Sets the READ permissions of the root gallery folder (/Portals/<PortalID>/Gallery/<ModuleID>/) and all of its subfolders to match either the
		// special permissions if supplied or the VIEW permissions of the associated ModuleID or the VIEW permissions of the TabID if the module
		// is set to inherit view permissions from the page
		public static void SyncFolderPermissions(string specialPermissions, int ModuleId, int TabId)
		{
			PortalSettings ps = PortalController.GetCurrentPortalSettings();
			Entities.Modules.ModuleController modController = new Entities.Modules.ModuleController();
			Entities.Modules.ModuleInfo moduleInfo = modController.GetModule(ModuleId, TabId);
			if ((moduleInfo != null)) {
				int portalID = ps.PortalId;
				if (string.IsNullOrEmpty(specialPermissions)) {
					if (moduleInfo.InheritViewPermissions) {
						Security.Permissions.TabPermissionCollection tabPermissions = DotNetNuke.Security.Permissions.TabPermissionController.GetTabPermissions(TabId, portalID);
						specialPermissions = tabPermissions.ToString("VIEW");
					} else {
						Security.Permissions.ModulePermissionCollection modPermissions = DotNetNuke.Security.Permissions.ModulePermissionController.GetModulePermissions(ModuleId, TabId);
						specialPermissions = modPermissions.ToString("VIEW");
					}
				} else {
					//Administrator role always has read permission on folder even if not included in specialPermissions
					if (!specialPermissions.Contains(";" + ps.AdministratorRoleId.ToString()) && !specialPermissions.Contains(";" + ps.AdministratorRoleName)) {
						specialPermissions += ";" + ps.AdministratorRoleName;
					}
				}

				string rootFolderPath = Config.GetGalleryConfig(ModuleId).RootURL.Replace(ps.HomeDirectory, "");
				//"Gallery/" & ModuleId.ToString & "/"
				Services.FileSystem.FolderController fldController = new Services.FileSystem.FolderController();
				Services.FileSystem.FolderInfo fldInfo = fldController.GetFolder(portalID, rootFolderPath, true);
				if ((fldInfo != null)) {
					int folderID = fldInfo.FolderID;
					Security.Permissions.FolderPermissionCollection newPermissions = new Security.Permissions.FolderPermissionCollection();
					AddRolesToFolderPermissionsCollection(newPermissions, specialPermissions, "READ", portalID, folderID);

					//Dim fldPermController As New Security.Permissions.FolderPermissionController
					Security.Permissions.FolderPermissionInfo fldPermInfo = null;
					Security.Permissions.FolderPermissionCollection writePermissions = null;
					//Try
					// WES: Due to breaking change in DNN 5.00.00 and 5.00.01 which made GetFolderPermissionsCollectionByFolderPath a shared rather than
					// instance method, the following line will throw a method not found exception in these two versions only. This will be fixed for DNN 5.01.00.
					// The work around in the catch block is much less efficient.
					writePermissions = GetFolderWritePermissionsCollectionByFolderPath(moduleInfo.PortalID, rootFolderPath);
					//Must be DNN 4.9.x or > DNN 5.01.00 so we can copy over write permissions the easy way
					foreach (DotNetNuke.Security.Permissions.FolderPermissionInfo fldPermInfo_loopVariable in writePermissions) {
						fldPermInfo = fldPermInfo_loopVariable;
						newPermissions.Add(fldPermInfo);
					}
					//Catch ex As MissingMethodException
					//Must be DNN 5.00.00 or 5.00.01 as exception was thrown above so copy over write permissions the hard way.
					//Dim writeRoles As String = FileSystemUtils.GetRoles(rootFolderPath, portalID, "WRITE")
					//AddRolesToFolderPermissionsCollection(newPermissions, writeRoles, "WRITE", portalID, folderID)
					//End Try

					// Should test if newPermissions <> oldPermissions but not easily done efficiently
					// so we will assume they have changed and delete ALL old permissions first

					fldInfo.FolderPermissions.Clear();
					fldInfo.FolderPermissions.AddRange(newPermissions);
					DotNetNuke.Security.Permissions.FolderPermissionController.SaveFolderPermissions(fldInfo);

					//fldPermController.DeleteFolderPermissionsByFolder(portalID, rootFolderPath)
					//For Each fldPermInfo In newPermissions
					//  fldPermController.AddFolderPermission(fldPermInfo)
					//Next
					SetSubFolderPermissions(portalID, rootFolderPath, true);
					// DataCache.ClearFolderPermissionsCache(portalID) ' Not needed in DNN 5.x
				}
			}
		}

		// WES: In DNN 5.00.00 and 5.00.01, this will throw a MissingMethodException due to the instance method GetFolderPermissionsCollectionByFolderPath
		// having been deprecated and replaced by a shared method of the same signature. In DNN 5.01.00, the deprecation will be rolled back and
		// the shared method will be given a different name.

		private static Security.Permissions.FolderPermissionCollection GetFolderWritePermissionsCollectionByFolderPath(int portalID, string folderPath)
		{
			DotNetNuke.Security.Permissions.FolderPermissionController fldPermController = new DotNetNuke.Security.Permissions.FolderPermissionController();
			DotNetNuke.Security.Permissions.FolderPermissionCollection writePermissions = new DotNetNuke.Security.Permissions.FolderPermissionCollection();

			foreach (DotNetNuke.Security.Permissions.FolderPermissionInfo fPI in DotNetNuke.Security.Permissions.FolderPermissionController.GetFolderPermissionsCollectionByFolder(portalID, folderPath)) {
				if (fPI.PermissionCode == "SYSTEM_FOLDER" && fPI.PermissionKey == "WRITE") {
					writePermissions.Add(fPI);
				}
			}
			return writePermissions;
		}

		private static void AddRolesToFolderPermissionsCollection(Security.Permissions.FolderPermissionCollection permissions, string roles, string permissionKey, int portalID, int folderID)
		{
			DotNetNuke.Security.Permissions.PermissionController permController = new DotNetNuke.Security.Permissions.PermissionController();
			Security.Permissions.PermissionInfo permission = (Security.Permissions.PermissionInfo)permController.GetPermissionByCodeAndKey("SYSTEM_FOLDER", permissionKey)[0];
			if (permission != null && !string.IsNullOrEmpty(roles)) {
				int permissionID = permission.PermissionID;
				Security.Permissions.FolderPermissionInfo fldPermInfo = null;
				string[] rolesToAdd = roles.Trim(';').Split(';');
				Regex userIDRegex = new Regex("^\\[(\\d+)\\]$");
				Security.Roles.RoleController roleController = new Security.Roles.RoleController();
				int nullRoleID = int.Parse(Common.Globals.glbRoleNothing);
				int allUsersRoleID = int.Parse(Common.Globals.glbRoleAllUsers);
				foreach (string role in rolesToAdd) {
					System.Text.RegularExpressions.Match m = userIDRegex.Match(role, 0);
					int userID = -1;
					int roleID = nullRoleID;
					Security.Roles.RoleInfo roleInfo = null;
					if (m.Success) {
						userID = int.Parse(m.Groups[1].Value);
					} else {
						int n = 0;
						if (int.TryParse(role, out n)) {
							roleID = n;
						} else {
							if (role == Common.Globals.glbRoleAllUsersName) {
								roleID = allUsersRoleID;
							} else {
								roleInfo = roleController.GetRoleByName(portalID, role);
								if ((roleInfo != null)) {
									roleID = roleInfo.RoleID;
								}
							}
						}
					}
					if (roleID != nullRoleID || userID != -1) {
						fldPermInfo = new Security.Permissions.FolderPermissionInfo();
						var _with4 = fldPermInfo;
						_with4.FolderID = folderID;
						_with4.PermissionID = permissionID;
						_with4.PortalID = portalID;
						_with4.RoleID = roleID;
						_with4.UserID = userID;
						_with4.AllowAccess = true;
						permissions.Add(fldPermInfo);
					}
				}
			}
		}

		// Added by WES
		// Sets the permissions of FolderID in PortalID with relativePath to that of its parent folder
		// If isRecursive is True, will apply parent permissions recursively to ALL subfolders of the parent.
		public static void SetSubFolderPermissions(int PortalID, string relativePath, bool isRecursive)
		{
			ArrayList subFolders = FileSystemUtils.GetFoldersByParentFolder(PortalID, relativePath);
			Security.Permissions.FolderPermissionController fldPermController = new Security.Permissions.FolderPermissionController();
			foreach (Services.FileSystem.FolderInfo fldInfo in subFolders) {
				//fldPermController.DeleteFolderPermissionsByFolder(PortalID, fldInfo.FolderPath) - NOT Needed in DNN 5.x
				FileSystemUtils.SetFolderPermissions(PortalID, fldInfo.FolderID, fldInfo.FolderPath);
				if (isRecursive && !fldInfo.FolderName.StartsWith("_")) {
					SetSubFolderPermissions(PortalID, fldInfo.FolderPath, true);
				}
			}
		}

		// Replace invalid characters in a filename
		public static string MakeValidFilename(string inputName, string replacementCharacter)
		{
			Regex invalidCharacterReplacementRegex = new Regex("[\\000-\\037\"*:<>?\\\\/|]");
			inputName = inputName.Trim(new char[] {
				' ',
				'.'
			});
			// should not start or end with space or period
			inputName = invalidCharacterReplacementRegex.Replace(inputName, replacementCharacter);
			return inputName;
		}

		//' Added By Quinn
		//' Multiple controls have identical ReturnURL functions
		//' Updates are needed for security fixes (GAL-9404 and GAL-9403) so I am moving to and modifying here
		public static string ReturnURL(int TabId, int ModuleId, System.Web.HttpRequest Request)
		{
			System.Collections.Generic.List<string> @params = new System.Collections.Generic.List<string>();
			if ((Request.QueryString["path"] != null))
				@params.Add("path=" + Request.QueryString["path"]);
			if ((Request.QueryString["currentstrip"] != null))
				@params.Add("currentstrip=" + Request.QueryString["currentstrip"]);
			if ((Request.QueryString["currentitem"] != null))
				@params.Add("currentitem=" + Request.QueryString["currentitem"]);
			string ctl = "";
			string @remove = "";
			if ((Request.QueryString["returnctl"] != null)) {
				string[] returnCtls = Request.QueryString["returnctl"].Split(';');
				if (returnCtls.Length > 1) {
					ctl = returnCtls[returnCtls.Length - 2];
					string rtnctl = Request.QueryString["returnctl"].Replace(ctl + ";", "");
					if (!string.IsNullOrEmpty(rtnctl))
						@params.Add("returnctl=" + rtnctl);
					else
						@remove += "returnctl=";
					@params.Add("mid=" + ModuleId.ToString());
				}
			}
			if (!object.ReferenceEquals(ctl, "")) {
				@params.Add("ctl=" + ctl);
			}

			if (@remove.Length > 0)
				@remove += "&ctl=";
			else
				@remove = "ctl=";

			return SanitizedRawUrl(TabId, ModuleId, @params.ToArray(), @remove);

		}

		//' Added By Quinn
		//' For GAL-9404
		//' The AdditionalParameters parameter should be formated as key=value
		public static string SanitizedRawUrl(int TabId, int ModuleId, string[] addAdditionalParameters)
		{
			return SanitizedRawUrl(TabId, ModuleId, addAdditionalParameters, "");
		}

		public static string SanitizedRawUrl(int TabId, int ModuleId, string[] addAdditionalParameters, string @remove)
		{
			string @add = "";

			// In the form key=value
			foreach (string param in addAdditionalParameters) {
				if (!string.IsNullOrEmpty(param.Split('=')[1])) {
					@add += param + "&";
				}
			}

			return SanitizedRawUrl(TabId, ModuleId, @add, @remove, null);
		}

		//' Added By Quinn
		//' Fix for GAL-9403
		public static string SanitizedRawUrl(int TabId, int ModuleId)
		{
			return SanitizedRawUrl(TabId, ModuleId, "", "", null);
		}

		//' Added By Quinn
		//' Sanitizes the URL - A fix for GAL-9403 and GAL-9404
		//' Remove removes from the provided request.querystring only
		public static string SanitizedRawUrl(int TabId, int ModuleId, string @add, string @remove, HttpRequest request)
		{

			NameValueCollection rawParamCollection = null;
			ArrayList sanitizedParams = new ArrayList();

			PortalSecurity ps = new PortalSecurity();
			string ctl = "";

			Hashtable addQueries = new Hashtable();
			if (!object.ReferenceEquals(@add, string.Empty)) {
				addQueries = CreateHashtableFromQueryString(@add.ToLower());
			}
			Hashtable removeQueries = new Hashtable();
			if (!object.ReferenceEquals(@remove, string.Empty)) {
				removeQueries = CreateHashtableFromQueryString(@remove.ToLower());
			}

			NameValueCollection modParamCollection = new NameValueCollection();

			if (request != null) {
				rawParamCollection = request.QueryString;

				foreach (string key in rawParamCollection) {
					if (!object.ReferenceEquals(key, "mid") & !removeQueries.ContainsKey(key)) {
						string value = rawParamCollection.GetValues(key)[0];
						modParamCollection.Add(key, value);
					}
				}

			}

			foreach (string key in addQueries.Keys) {
				modParamCollection.Remove(key);
				string value = addQueries[key].ToString();
				modParamCollection.Add(key, value);
			}


			foreach (string key in modParamCollection) {
				string newValue = null;

				string keyLower = key.ToLower();
				//Makes sure all comparrisons will be in lower case

				switch (keyLower) {
					case "tabid":
					case "language":
					case "portalid":
						continue;
					case "mid":
						newValue = ModuleId.ToString();
						break;
					case "ctl":
						ctl = ps.InputFilter(modParamCollection[keyLower].ToString(), PortalSecurity.FilterFlag.NoMarkup | PortalSecurity.FilterFlag.NoSQL);
						continue;
					case "returnctl":
						newValue = request.QueryString[keyLower];
						string newNewValue = "";


						foreach (string strVal in newValue.Split(';')) {
							bool isValid = false;

							foreach (string retCtl in ReturnCtlValues) {
								if (retCtl.ToLower() == strVal.ToLower()) {
									isValid = true;
									break; // TODO: might not be correct. Was : Exit For
								}
							}
							// UrlEncode is applied to be sure about the security check
							if (isValid)
								newNewValue += HttpUtility.UrlEncode(strVal) + ";";
						}


						newValue = newNewValue;
						break;
					default:
						if (Regex.IsMatch(keyLower, "[^a-zA-Z0-9_\\-]")) {
							throw new HttpException(403, "Invalid characters in querystring parameter key");
						} else {
							newValue = modParamCollection[key].ToString();
							if (!Regex.IsMatch(newValue, "^\\d+$")) {
								newValue = ps.InputFilter(newValue, PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoSQL);
								newValue = newValue.Trim();
								if (newValue.Length > 0 && Regex.IsMatch(newValue, "[\\040!*'();:@&=+$,?%#\\[\\]]")) {
									newValue = HttpUtility.UrlEncode(newValue);
								}
							}
						}
						break;
				}

				if (!string.IsNullOrEmpty(newValue)) {
					sanitizedParams.Add(key + "=" + newValue);
				}
			}

			return Common.Globals.NavigateURL(TabId, ctl, Convert.ToString(sanitizedParams.ToArray(typeof(string))));
		}

		//' Added By Quinn
		//' This is for valid ReturnCTL query string values. Any not in this list are dropped when using SanitizedRawUrl
		public static string[] ReturnCtlValues = {
			"FileEdit",
			"Viewer",
			"AlbumEdit",
			"Maintenance"

		};
		public static string GetGalleryObjectMenuID(IGalleryObjectInfo GalleryObject)
		{
			return string.Format("{0}.{1}", GalleryObject.Type, GalleryObject.ID);
		}

		public static string GetGalleryObjectMenuNodeID(IGalleryObjectInfo GalleryObject)
		{
			return string.Format("{0}.{1}.{2}", GalleryObject.Type, GalleryObject.ID, GalleryObject.Index);
		}

	}

	#region "Comparer"

	// Original class in C# by Diego Mijelshon
	// Converted to generic form - 10-22-2010 by William Severance

	[Serializable()]
	public class Comparer : IComparer<IGalleryObjectInfo>
	{

		public int Compare(IGalleryObjectInfo x, IGalleryObjectInfo y)
		{

			Type typex = x.GetType();
			Type typey = y.GetType();

			int i = 0;
			for (i = 0; i <= MyFields.Length - 1; i++) {
				PropertyInfo pix = typex.GetProperty(MyFields[i]);
				PropertyInfo piy = typey.GetProperty(MyFields[i]);

				IComparable pvalx = (IComparable)pix.GetValue(x, null);
				object pvaly = piy.GetValue(y, null);

				//Compare values, using IComparable interface of the property's type
				int iResult = pvalx.CompareTo(pvaly);
				if (iResult != 0) {
					//Return if not equal
					if (MyDescending) {
						//Invert order
						return -iResult;
					} else {
						return iResult;
					}
				}
			}
			//Objects have the same sort order
			return 0;
		}
		//Compare

		public Comparer(params string[] fields) : this(fields, false)
		{
		}
		//New

		public Comparer(string[] Fields, bool Descending)
		{
			MyFields = Fields;
			MyDescending = Descending;
		}
		//New

		protected string[] MyFields;

		protected bool MyDescending;
	}
	//GalleryComparer

	#endregion

}
