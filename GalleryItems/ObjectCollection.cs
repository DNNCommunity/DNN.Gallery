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


namespace DotNetNuke.Modules.Gallery
{

	public class GalleryObjectCollection : System.Collections.CollectionBase
	{

		// Allows access to items by key AND index
		// WES changed to case insensitive hashtable key comparisons following issues resulting from forced lowercasing of url
		// querystrings implemented for GAL-9403 and GAL-9404. Also limited refactoring to make collection strongly typed
		// for IGalleryObjectInfo interface.


		private Hashtable keyIndexLookup = new Hashtable(System.StringComparer.InvariantCultureIgnoreCase);
		public GalleryObjectCollection() : base()
		{
		}

		internal new void Clear()
		{
			keyIndexLookup.Clear();
			base.Clear();
		}

		public void Add(string key, IGalleryObjectInfo value)
		{
			int index = 0;
			try {
				index = base.List.Add(value);
				keyIndexLookup.Add(key, index);
			} catch (Exception ex) {
				Exceptions.LogException(ex);
				//Throw ex
			}

		}

		public IGalleryObjectInfo Item(int index)
		{
			try {
				return (IGalleryObjectInfo)base.List[index];
			} catch (System.Exception Exc) {
				return null;
			}
		}

		public IGalleryObjectInfo Item(string key)
		{
			object obj = keyIndexLookup[key];

			// Do validation first
			if (obj == null) {
				throw new ArgumentException("The item with the provided key can not be found.");
			}

			return (IGalleryObjectInfo)base.List[Convert.ToInt32(obj)];
		}
	}

}
