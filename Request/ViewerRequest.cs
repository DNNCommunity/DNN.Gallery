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

using System.IO;
using static DotNetNuke.Common.Globals;
using static DotNetNuke.Modules.Gallery.Config;
using static DotNetNuke.Modules.Gallery.Utils;

namespace DotNetNuke.Modules.Gallery
{

	/// <summary>
	/// Class for using the viewer and slideshow to view galleries
	/// </summary>
	/// <remarks></remarks>
	public class GalleryViewerRequest : BaseRequest
	{

		private HttpRequest mRequest;
		private System.Collections.Generic.List<IGalleryObjectInfo> _browsableItems = new System.Collections.Generic.List<IGalleryObjectInfo>();
		private int _currentItemIndex;
		private int _currentItemNumber;
		private int _nextItemNumber;

		private int _previousItemNumber;
		#region "Public Properties"

		public List<IGalleryObjectInfo> BrowsableItems {
			get {
				if (_browsableItems == null) {
					_browsableItems = new List<IGalleryObjectInfo>();
				}
				return _browsableItems;
			}
		}

		public GalleryFile CurrentItem {
//If BrowsableItems.Count = 0 Then
				//Else
				//Return CType(BrowsableItems.Item(_currentItemIndex), GalleryFile)
				//End If
			get { return (GalleryFile)base.Folder.List.Item(_currentItemNumber); }
		}

		public int CurrentItemIndex {
			get { return _currentItemIndex; }
		}

		public int CurrentItemNumber {
				//+ 1
			get { return _currentItemNumber; }
		}

		public int NextItemNumber {
			get { return _nextItemNumber; }
		}

		public int PreviousItemNumber {
			get { return _previousItemNumber; }
		}

		#endregion

		#region "Public Methods"

		public GalleryViewerRequest(int ModuleID, Config.GallerySort SortType = Config.GallerySort.Name, bool SortDescending = false) : base(ModuleID, SortType, SortDescending)
		{

			// Don't want to continue processing if this is an invalid path
			if (!GalleryConfig.IsValidPath) {
				return;
			}

			int intCounter = 0;

			//For Each intCounter In MyBase.Folder.BrowsableItems
			//    Dim item As GalleryFile = CType(MyBase.Folder.List.Item(intCounter), GalleryFile)
			//    If Not (item.Title.ToLower.IndexOf("icon") > -1) _
			//    AndAlso Not (item.Title.ToLower = "watermark") _
			//    AndAlso (item.ApprovedDate <= DateTime.Today OrElse MyBase.GalleryConfig.AutoApproval) Then
			//        myBrowsableItems.Add(item)
			//    Else
			//    End If
			//Next

			foreach (int intCounter_loopVariable in base.Folder.BrowsableItems) {
				intCounter = intCounter_loopVariable;
				GalleryFile item = (GalleryFile)base.Folder.List.Item(intCounter);
				BrowsableItems.Add(item);
			}

			BrowsableItems.Sort(new Comparer(new string[1] { Enum.GetName(typeof(Config.GallerySort), SortType) }, SortDescending));
			mRequest = HttpContext.Current.Request;

			// Determine initial item to be viewed.
			if ((mRequest.QueryString["currentitem"] != null)) {
				_currentItemNumber = Convert.ToInt32(mRequest.QueryString["currentitem"]);
			} else {
				_currentItemNumber = BrowsableItems[0].Index;
			}

			// Grab the index of the item in the folder.list collection
			if (base.Folder.IsBrowsable) {
				for (intCounter = 0; intCounter <= BrowsableItems.Count - 1; intCounter++) {
					if (BrowsableItems[intCounter].Index == _currentItemNumber) {
						_currentItemIndex = intCounter;
						break; // TODO: might not be correct. Was : Exit For
					}
				}
				_nextItemNumber = GetNextItemNumber();
				_previousItemNumber = GetPreviousItemNumber();
			}
		}

		private int GetNextItemNumber()
		{
			if (_currentItemIndex == BrowsableItems.Count - 1) {
				return BrowsableItems[0].Index;
			} else {
				return BrowsableItems[_currentItemIndex + 1].Index;
			}
		}

		private int GetPreviousItemNumber()
		{
			if (_currentItemIndex == 0) {
				return BrowsableItems[BrowsableItems.Count - 1].Index;
			} else {
				return BrowsableItems[_currentItemIndex - 1].Index;
			}
		}

		private int Constrain(int n, int min, int max)
		{
			if (n < 0) {
				return min;
			} else if (n > max) {
				return min;
			}
			return n;
		}

		public void MoveNext()
		{
			_currentItemNumber = _nextItemNumber;
			_currentItemIndex = Constrain(_currentItemIndex + 1, 0, BrowsableItems.Count - 1);
			_previousItemNumber = GetPreviousItemNumber();
			_nextItemNumber = GetNextItemNumber();
		}

		public void MovePrevious()
		{
			_currentItemNumber = _previousItemNumber;
			_currentItemIndex = Constrain(_currentItemIndex - 1, 0, BrowsableItems.Count - 1);
			_previousItemNumber = GetPreviousItemNumber();
			_previousItemNumber = GetPreviousItemNumber();
		}
		#endregion

	}

}
