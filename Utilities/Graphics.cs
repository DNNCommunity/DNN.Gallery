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

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace DotNetNuke.Modules.Gallery
{

	// Utility routines for various repeated functions.
	public static class GalleryGraphics
	{

		//added by Tam - to avoid images have low-resolution after resize using CreateThumb
		//WES - interfaced with DNN file system and changed sub to integer to return DNN FileID
		//WES - added parameter for JPEG compression level (EncoderQuality)

		public static int ResizeImage(string Source, string Destination, int MaxWidth, int MaxHeight, long EncoderQuality)
		{
			int lWidth = 0;
			int lHeight = 0;
			float sRatio = 0;
			System.Drawing.Image mImage = null;
			System.Drawing.Image newImage = null;
			System.Drawing.Imaging.ImageFormat iFormat = null;
			int fileID = -1;

			try {
				mImage = System.Drawing.Image.FromFile(Source);
				iFormat = mImage.RawFormat;
				lWidth = mImage.Width;
				lHeight = mImage.Height;

				if (!(lWidth <= MaxWidth && lHeight <= MaxHeight)) {
					sRatio = Convert.ToSingle(lHeight / lWidth);
					// Bounded by height
					if (sRatio > Convert.ToSingle(MaxHeight / MaxWidth)) {
						lWidth = Convert.ToInt32(MaxHeight / sRatio);
						lHeight = MaxHeight;
					//Bounded by width
					} else {
						lWidth = MaxWidth;
						lHeight = Convert.ToInt32(MaxWidth * sRatio);
					}
					newImage = new Bitmap(FixedSize(mImage, lWidth, lHeight));
					mImage.Dispose();
					mImage = null;
					SaveImage(newImage, Destination, iFormat, EncoderQuality);
				} else {
					//Added intermediate image to allow save to Destination = Source
					newImage = new Bitmap(mImage);
					mImage.Dispose();
					mImage = null;
					SaveImage(newImage, Destination, iFormat, EncoderQuality);
				}

				//William Severance - added to interface with DNN file system
				fileID = Utils.SaveDNNFile(Destination, lWidth, lHeight, false, true);

			} catch (Exception ex) {
				// Eat the exception
			} finally {
				if (mImage != null)
					mImage.Dispose();
				if (newImage != null)
					newImage.Dispose();
			}

			return fileID;
		}

		public static void SaveImage(System.Drawing.Image src, string Destination, System.Drawing.Imaging.ImageFormat Format, long EncoderQuality)
		{
			switch (Path.GetExtension(Destination).ToLower()) {
				case ".jpg":
				case "jpeg":
					SaveJPEG(src, EncoderQuality, Destination);
					break;
				default:
					src.Save(Destination, Format);
					break;
			}
		}

		//Return an ImageCodecInfo object for the image/jpeg mime type
		private static ImageCodecInfo GetJpegCodec()
		{
			//Setup a JPEG codec
			foreach (ImageCodecInfo Codec in ImageCodecInfo.GetImageEncoders()) {
				if (Codec.MimeType == "image/jpeg") {
					return Codec;
				}
			}
			//Supposedly this cannot fail as image/jpg is built-in to GDI+ encoders
			return null;
		}

		//Return the EncoderParameters object for JPEG encoding having a specified EncoderQuality
		private static EncoderParameters GetJPegEncoderQualityParameters(long EncoderQuality)
		{
			EncoderParameters EncParams = new EncoderParameters(1);
			EncParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, EncoderQuality);
			return EncParams;
		}

		//Saves a JPEG image src to the supplied stream with specified encoder quality
		public static void SaveJPegImage(System.Drawing.Image src, long EncoderQuality, Stream stream)
		{
			src.Save(stream, GetJpegCodec(), GetJPegEncoderQualityParameters(EncoderQuality));
		}

		//Saves a JPEG image src to the specified destination filepath with specified encoder quality
		public static void SaveJPEG(System.Drawing.Image src, long EncoderQuality, string Destination)
		{
			src.Save(Destination, GetJpegCodec(), GetJPegEncoderQualityParameters(EncoderQuality));
		}

		// Code by Kenneth Courtney: using Bicubic Rescaling
		private static System.Drawing.Image FixedSize(System.Drawing.Image imgPhoto, int Width, int Height)
		{
			int sourceWidth = imgPhoto.Width;
			int sourceHeight = imgPhoto.Height;
			int sourceX = 0;
			int sourceY = 0;
			int destX = 0;
			int destY = 0;

			float nPercent = 0;
			float nPercentW = 0;
			float nPercentH = 0;

			nPercentW = (Convert.ToSingle(Width) / Convert.ToSingle(sourceWidth));
			nPercentH = (Convert.ToSingle(Height) / Convert.ToSingle(sourceHeight));

			if (nPercentH < nPercentW) {
				nPercent = nPercentH;
				destX = System.Convert.ToInt16((Width - (sourceWidth * nPercent)) / 2);
			} else {
				nPercent = nPercentW;
				destY = System.Convert.ToInt16((Height - (sourceHeight * nPercent)) / 2);
			}

			int destWidth = Convert.ToInt32((sourceWidth * nPercent));
			int destHeight = Convert.ToInt32((sourceHeight * nPercent));

			Bitmap bmPhoto = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
			bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

			Graphics grPhoto = Graphics.FromImage(bmPhoto);
			grPhoto.Clear(Color.White);
			grPhoto.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

			grPhoto.DrawImage(imgPhoto, new Rectangle(destX, destY, destWidth, destHeight), new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight), GraphicsUnit.Pixel);
			grPhoto.Dispose();
			return bmPhoto;

		}

		// Provided by Jan Emil Christiansen.
		public static Bitmap DrawWatermark(ref System.Drawing.Bitmap sourceImage, System.Drawing.Image Watermark)
		{
			int wmWidth = Watermark.Width;
			int wmHeight = Watermark.Height;

			int xPosOfWm = sourceImage.Width - wmWidth - 2;
			int yPosOfWm = 2;

			Bitmap bmWatermark = new Bitmap(sourceImage);
			bmWatermark.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);

			Graphics grWatermark = Graphics.FromImage(bmWatermark);

			ImageAttributes imageAttributes = new ImageAttributes();
			ColorMap colorMap = new ColorMap();

			colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
			colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);
			ColorMap[] remapTable = { colorMap };

			imageAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

			float[][] colorMatrixElements = {
				new float[] {
					1f,
					0f,
					0f,
					0f,
					0f
				},
				new float[] {
					0f,
					1f,
					0f,
					0f,
					0f
				},
				new float[] {
					0f,
					0f,
					1f,
					0f,
					0f
				},
				new float[] {
					0f,
					0f,
					0f,
					0.3f,
					0f
				},
				new float[] {
					0f,
					0f,
					0f,
					0f,
					1f
				}
			};

			ColorMatrix wmColorMatrix = new ColorMatrix(colorMatrixElements);

			imageAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

			grWatermark.DrawImage(Watermark, new Rectangle(xPosOfWm, yPosOfWm, wmWidth, wmHeight), 0, 0, wmWidth, wmHeight, GraphicsUnit.Pixel, imageAttributes);

			return bmWatermark;

		}
		//DrawWatermark

		public static string GetContentType(ImageFormat iFormat)
		{
			string contentType = null;

			if (iFormat.Guid.Equals(ImageFormat.Jpeg.Guid)) {
				contentType = "image/jpeg";
			} else if (iFormat.Guid.Equals(ImageFormat.Gif.Guid)) {
				contentType = "image/gif";
			} else if (iFormat.Guid.Equals(ImageFormat.Png.Guid)) {
				contentType = "image/png";
			} else if (iFormat.Guid.Equals(ImageFormat.Tiff.Guid)) {
				contentType = "image/tiff";
			} else if (iFormat.Guid.Equals(ImageFormat.Bmp.Guid)) {
				contentType = "image/x-ms-bmp";
			} else if (iFormat.Guid.Equals(ImageFormat.Wmf.Guid)) {
				contentType = "image/x-ms-wmf";
			} else if (iFormat.Guid.Equals(ImageFormat.Emf.Guid)) {
				contentType = "image/x-emf";
			} else {
				contentType = "image/jpeg";
			}

			return contentType;

		}
		//GetContentType

		public static Bitmap FlipImage(System.Drawing.Bitmap image, RotateFlipType FlipType)
		{
			image.RotateFlip(FlipType);
			return image;
		}

		//As Bitmap
		public static void ToggleColorIndex(Bitmap image, int ColorIndex)
		{
			// Toggle color index, i.e., switch color images to black and white.
			// Provided by Jan Emil Christiansen, modified by Tam for more features
			int x = 0;
			try {
				for (x = 0; x <= image.Width - 1; x++) {
					int y = 0;
					for (y = 0; y <= image.Height - 1; y++) {
						Color c = image.GetPixel(x, y);
						// For better performance save some actions

						int value = Convert.ToInt32((Convert.ToInt32(c.R) + Convert.ToInt32(c.G) + Convert.ToInt32(c.B)) / 3);

						// more white scale
						if (value > ColorIndex) {
							value = 255;
						}

						//If (Not value = c.R) AndAlso (Not value = c.G) AndAlso (Not value = c.B) Then
						image.SetPixel(x, y, Color.FromArgb(value, value, value));
						//End If
					}
				}
			} catch (Exception ex) {
			}

		}
		//ToggleColorIndex

	}
}
