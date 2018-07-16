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
using System.Drawing;
using System.Drawing.Imaging;

namespace DotNetNuke.Modules.Gallery
{
	public abstract partial class ImageEdit : System.Web.UI.Page
	{

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

		private int mZoomIndex;
		private int mColorIndex;
		private GalleryFile mGalleryImage;
		private DotNetNuke.Modules.Gallery.Config mGalleryConfig;
		private DotNetNuke.Modules.Gallery.Authorization mGalleryAuthorization;
		private System.Drawing.Image mImage;

		private int mModuleId;
		private void Page_Load(System.Object sender, System.EventArgs e)
		{
			ImageFormat iFormat = null;

			// Set color index, if any.
			if ((Request.QueryString["c"] != null)) {
				mColorIndex = Int32.Parse(Request.QueryString["c"]);
				if (mColorIndex < 0) {
					mColorIndex = 0;
				}
			}

			// Set zoom index, if any. WES - added min/max constraints
			if ((Request.QueryString["zoomindex"] != null)) {
				mZoomIndex = Math.Min(3, Math.Max(-3, Int32.Parse(Request.QueryString["zoomindex"])));
			}

			if ((Request.QueryString["mid"] != null)) {
				mModuleId = Int32.Parse(Request.QueryString["mid"]);
			}

			// Check for View permission added by WES - 2-10-2009
			if (GalleryAuthorization.HasViewPermission()) {
				GalleryViewerRequest _request = new GalleryViewerRequest(mModuleId);
				mGalleryImage = _request.CurrentItem;
				mGalleryConfig = mGalleryImage.GalleryConfig;

				// Check for valid image file extension added by wES - 2-10-2009

				if (mGalleryConfig.IsValidImageType(Path.GetExtension(mGalleryImage.Path))) {
					// Get smaller dimension from gallery config
					int maxDimension = 0;
					if (mGalleryConfig.FixedWidth < mGalleryConfig.FixedHeight) {
						maxDimension = mGalleryConfig.FixedWidth;
					} else {
						maxDimension = mGalleryConfig.FixedHeight;
					}

					Bitmap sourceImage = new Bitmap(mGalleryImage.Path);

					iFormat = sourceImage.RawFormat;

					maxDimension += mZoomIndex * 100;

					// Aspect ratio logic provided by Bryce Jasmer.
					float aspect = sourceImage.PhysicalDimension.Width / sourceImage.PhysicalDimension.Height;
					Size size = default(Size);
					// Portrait
					if (aspect <= 1.0) {
						size = new Size(Convert.ToInt32(maxDimension * aspect), maxDimension);
						// Landscape
					} else {
						size = new Size(maxDimension, Convert.ToInt32(maxDimension / aspect));
					}

					// Resize while retaining image quality.
					// Provided by Jan Emil Christiansen.
					Bitmap newImage = new Bitmap(sourceImage, size);
					sourceImage.Dispose();

					// Complete refactoring of manner in which rotate/flip operations are handled added by
					// WES on 2-10-2009 to simplify code, reduce length of query string, and correct issues
					// with wrong orientation produced after sequence of mixed rotates and flips.

					if ((Request.QueryString["rotateflip"] != null)) {
						int rotateFlip = Math.Max(0, Math.Min(7, Int32.Parse(Request.QueryString["rotateflip"])));
						newImage = GalleryGraphics.FlipImage(newImage, (RotateFlipType)rotateFlip);
					}

					if ((Request.QueryString["color"] != null)) {
						int colorIndex = Int16.Parse(Request.QueryString["color"]);
						if (colorIndex > 0) {
							GalleryGraphics.ToggleColorIndex(newImage, colorIndex);
						}
					}

					// Update image - ItemEditPermission check added by WES 2-10-09
					if ((Request.QueryString["mode"] != null)) {
						if (Request.QueryString["mode"] == "save" && GalleryAuthorization.HasItemEditPermission(mGalleryImage)) {
							UpdateImage(newImage, iFormat);
						}
					}

					if (mGalleryConfig.UseWatermark && ((mGalleryImage.Parent.WatermarkImage != null))) {
                        System.Drawing.Image imgWatermark = System.Drawing.Image.FromFile(mGalleryImage.Parent.WatermarkImage.Path);
						mImage = GalleryGraphics.DrawWatermark(ref newImage, imgWatermark);
						imgWatermark.Dispose();
						newImage.Dispose();
					} else {
						mImage = newImage;
					}

					// Send image to the browser.
					Response.ContentType = GalleryGraphics.GetContentType(iFormat);

					//Added by William Severance to use intermediate memory stream when
					//image type is PNG to avoid generic GDI+ error

					MemoryStream ms = null;
					try {
						if (Response.ContentType == "image/png") {
							ms = new MemoryStream();
							mImage.Save(ms, ImageFormat.Png);
							ms.WriteTo(Response.OutputStream);
						} else {
							mImage.Save(Response.OutputStream, iFormat);
						}
					} finally {
						if ((ms != null))
							ms.Dispose();
						newImage.Dispose();
						mImage.Dispose();
					}
				}
			}
		}

		private void UpdateImage(Bitmap NewImage, ImageFormat iFormat)
		{
			try {
				mGalleryImage.UpdateImage(NewImage, iFormat);
				if ((mGalleryImage.Parent != null)) {
					Config.ResetGalleryFolder(mGalleryImage.Parent);
				} else {
					Config.ResetGalleryConfig(mModuleId);
				}

			} catch (Exception exc) {
				throw;
			}
		}

		public DotNetNuke.Modules.Gallery.Authorization GalleryAuthorization {
			get {
				if (mGalleryAuthorization == null) {
					mGalleryAuthorization = new DotNetNuke.Modules.Gallery.Authorization(mModuleId);
				}
				return mGalleryAuthorization;
			}
		}
		public ImageEdit()
		{
			Load += Page_Load;
			Init += Page_Init;
		}

	}
}



