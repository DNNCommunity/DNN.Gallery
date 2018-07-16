// VBConversions Note: VB project level imports
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Common.Utilities;
using System.Text.RegularExpressions;
using System;
using System.Web.UI.WebControls;
using DotNetNuke.Framework;
using DotNetNuke;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI;
using DotNetNuke.Common;
using System.Collections;
using System.Web.Profile;
using System.Collections.Specialized;
using Microsoft.VisualBasic;
using System.Diagnostics;
using DotNetNuke.Security;
using DotNetNuke.Entities;
using DotNetNuke.Services;
using System.Web.SessionState;
using System.Configuration;
using System.Web;
using DotNetNuke.Services.Localization;
using System.Web.Security;
using System.Web.Caching;
using System.Web.UI.WebControls.WebParts;
using System.Collections.Generic;
// End of VB project level imports

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

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



namespace DotNetNuke.Modules.Gallery
{

    //Class to read and analyze the header of an Adobe flash file (SWF)
    //Written by William Severance - 9-5-08
    //Based on Adobe SWF Format Specification v 9. See http://www.adobe.com/devnet/swf/pdf/swf_file_format_spec_v9.pdf

    //To use:
    //   Dim swfProperties As New SWFHeaderReader(filePath)
    //   Properties exposed:
    //      swfProperties.IsValid bool - True if valid SWF file
    //      swfProperties.IsCompressed bool - True if SWF data is compressed
    //      swfProperties.Version byte - Adobe SWF version #
    //      swfProperties.FileLength integer - number of bytes in file (uncompressed length)
    //      swfProperties.Width integer - Frame Width in pixels
    //      swfProperties.Height integer - Frame Height in pixels
    //      swfProperties.FrameRate double - Frame Rate
    //      swfProperties.FrameCount integer - number of frames in movie
    //      swfProperties.HasError bool - True if error in reading/analyzing file
    //      swfProperties.LastError string - last exception message and stack trace

    public class SWFHeaderReader
    {
        private string filePath;
        private int _height;
        private int _width;
        private bool _isValid;
        private bool _isCompressed;
        private double _frameRate;
        private int _frameCount;
        private byte _version;
        private int _fileLength;
        private string _lastError = string.Empty;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private class GZip : object
        {
            // [FieldOffset(0)]    Cannot use this attribute with LayoutKind.Sequential
            private byte[] mBytes; //Overlayed byte array

            // [FieldOffset(0)]    Cannot use this attribute with LayoutKind.Sequential
            private byte[] mHeader = new byte[10]; //Header info - 10 bytes

            private byte[] mData; //Data bytes


            public byte[] Header
            {
                get
                {
                    return mHeader;
                }
                set
                {
                    if (value.Length == 10)
                    {
                        mHeader = value;
                    }
                    else
                    {
                        throw (new ArrayTypeMismatchException("GZip header must be 10 bytes in length"));
                    }
                }
            }

            public byte[] Data
            {
                get
                {
                    return mData;
                }
                set
                {
                    mData = value;
                }
            }

            public byte[] Bytes
            {
                get
                {
                    return mBytes;
                }
                set
                {
                    mBytes = value;
                }
            }

            public byte Byte0
            {
                get
                {
                    return mBytes[0];
                }
                set
                {
                    mBytes[0] = value;
                }
            }

            public int DataLength
            {
                get
                {
                    return mData.Length;
                }
                set
                {
                    Array.Resize(ref mData, value);
                }
            }

            public byte PopByte()
            {
                byte byte0 = mData[0];
                Array.Reverse(mData);
                DataLength--;
                Array.Reverse(mData);
                return byte0;
            }
        }

        public SWFHeaderReader(string filepath)
        {
            this.filePath = filepath;
            AnalyzeSWF();
        }

        public bool IsValid
        {
            get
            {
                return _isValid;
            }
        }

        public bool IsCompressed
        {
            get
            {
                return _isValid && _isCompressed;
            }
        }

        public double FrameRate
        {
            get
            {
                return _frameRate;
            }
        }

        public int FrameCount
        {
            get
            {
                return _frameCount;
            }
        }

        public int FileLength
        {
            get
            {
                return _fileLength;
            }
        }

        public byte Version
        {
            get
            {
                return _version;
            }
        }

        public int Width
        {
            get
            {
                return _width;
            }
        }

        public int Height
        {
            get
            {
                return _height;
            }
        }

        public string LastError
        {
            get
            {
                return _lastError;
            }
        }

        public bool HasError
        {
            get
            {
                return _lastError.Length > 0;
            }
        }

        private void AnalyzeSWF()
        {
            FileStream fs = null;
            BinaryReader reader = null;
            int i = 0;
            int j = 0;

            const byte Mask00000111 = 0x7;
            const byte Mask10000000 = 0x80;

            try
            {
                fs = File.Open(filePath, FileMode.Open);
            }
            catch (System.IO.IOException ex)
            {
                _lastError = ex.Message + "\r\n" + ex.StackTrace;
                return;
            }

            try
            {
                reader = new BinaryReader(fs);

                string signature = new string(reader.ReadChars(3));
                switch (signature)
                {
                    case "FWS":
                        _isValid = true;
                        break;
                    case "CWS":
                        _isValid = true;
                        _isCompressed = true;
                        break;
                    default:
                        return;
                }

                _version = reader.ReadByte();

                //Read next 4 bytes as fileLength LSB...MSB (little endian)
                _fileLength = 0;
                long b = 0;
                for (i = 0; i <= 3; i++)
                {
                    b = reader.ReadByte();
                    _fileLength += Convert.ToInt32(b << (8 * i));
                }

                GZip buffer = new GZip();

                //Read the rest of the file
                buffer.Data = reader.ReadBytes(_fileLength);

                if (IsCompressed)
                {
                    //data is compressed (gzip format - RFC 1952) - so first decompress
                    //set 10-byte GZip header which GZipStream can process see http://www.faqs.org/rfcs/rfc1952.html

                    byte[] header = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0 };
                    header.CopyTo(buffer.Header, 0);

                    MemoryStream ms = new MemoryStream(buffer.Bytes);

                    System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress);

                    byte[] decompressedBuffer = new byte[buffer.DataLength + 1000000 + 1];

                    int gzipLength = Utils.ReadAllBytesFromStream(gzip, decompressedBuffer);

                    gzip.Close();
                    ms.Close();

                    buffer.DataLength = gzipLength;
                    Array.Resize(ref decompressedBuffer, gzipLength);
                    decompressedBuffer.CopyTo(buffer.Data, 0);
                    decompressedBuffer = null;

                }

                //Read the variable length FrameSize RECT (4 values)

                byte byte0 = buffer.PopByte();
                int nBits = Convert.ToInt32(byte0 >> 3);

                BitArray bits = new BitArray(nBits, false);

                //Current byte
                byte0 = (byte)(byte0 & Mask00000111);
                byte0 <<= (byte)5;

                //current bit (first byte is already shifted)
                int bitPtr = 2;

                //Must get all 4 values in the RECT
                for (i = 0; i <= 3; i++)
                {
                    for (j = 0; j <= bits.Count - 1; j++)
                    {
                        if ((byte0 & Mask10000000) > 0)
                        {
                            bits[j] = true;
                        }
                        byte0 <<= (byte)1;
                        //byte0 = byte0 And Mask11111111
                        bitPtr--;

                        //get next byte if we've run out of bits in current byte
                        if (bitPtr < 0)
                        {
                            byte0 = buffer.PopByte();
                            bitPtr = 7;
                        }
                    }

                    //got the value now calculate
                    int c = 1;
                    int value = 0;

                    for (j = bits.Count - 1; j >= 0; j--)
                    {
                        if (bits[j])
                        {
                            value += c;
                        }
                        c <<= 1;
                    }

                    value /= 20; //Convert from twipps to px where 1 px = 20 twipps

                    switch (i)
                    {
                        case 0: //X1
                            _width = value;
                            break;
                        case 1: //X2
                            _width = value - _width;
                            break;
                        case 2: //Y1
                            _height = value;
                            break;
                        case 3: //Y2
                            _height = value - _height;
                            break;
                    }

                    bits.SetAll(false);
                }

                //now get the frame rate
                _frameRate = Convert.ToDouble(buffer.Data[1]);
                _frameRate += Convert.ToDouble(buffer.Data[0] / 100);

                //and the frame count
                _frameCount = BitConverter.ToInt16(buffer.Data, 2);

            }
            catch (Exception ex)
            {
                _lastError = ex.Message + "\r\n" + ex.StackTrace;
            }
            finally
            {
                if (!ReferenceEquals(reader, null))
                {
                    reader.Close();
                }
                if (!ReferenceEquals(fs, null))
                {
                    fs.Close();
                }
                reader = null;
                fs = null;
            }
        }

    }

    //Classes to read and analyze the headers of files compressed into a zip folder
    //Written by William Severance - 9-9-08
    //Based on PKWare Zip Format Specification. See http://www.pkware.com/documents/casestudies/APPNOTE.TXT


    public class ZipFileEntry : object
    {

        private string _Signature; //Signature - 4 chars = (0x04034b50)
        private byte[] _Version = new byte[2]; //Version to extract - 2 bytes
        private byte[] _Flags = new byte[2]; //General Purpose Flags - 2 bytes
        private byte[] _CompressionMethod = new byte[2]; //Compression Method - 2 bytes
        private byte[] _ModifiedDateTime = new byte[4]; //Last Modified Time and Date - 4 bytes (MS-DOS Format)
        private byte[] _CRC32 = new byte[4]; //CRC-32 - 4 bytes
        private long _CompressedSize; //Compressed Size - 4 bytes
        private long _UncompressedSize; //Uncompressed Size - 4 bytes
        private int _FileNameLength; //Filename Length - 2 bytes
        private byte[] _ExtraFieldLength = new byte[2]; //Extra Field Length - 2 bytes
        private string _FileName; //FileName - varible length
        private byte[] _ExtraField; //ExtraField - variable length
        private bool _Selected = true; //WESJr. Reserved for future use - to select files for extraction.

        const string ExpectedSignature = "50-4B-03-04";

        public string Signature
        {
            get
            {
                return _Signature;
            }
        }

        public bool IsValid
        {
            get
            {
                return _Signature == ExpectedSignature;
            }
        }

        public int Version
        {
            get
            {
                return Utils.ConvertToInt32(_Version, 2);
            }
        }

        public BitArray Flags
        {
            get
            {
                return new BitArray(_Flags);
            }
        }

        public int CompressionMethod
        {
            get
            {
                return Utils.ConvertToInt32(_CompressionMethod, 2);
            }
        }

        public DateTime LastModified
        {
            get
            {
                return Utils.ConvertToDateTime(_ModifiedDateTime);
            }
        }

        public long CRC
        {
            get
            {
                return Utils.ConvertToInt64(_CRC32, 4);
            }
        }

        public long CompressedSize
        {
            get
            {
                return _CompressedSize;
            }
        }

        public long UncompressedSize
        {
            get
            {
                return _UncompressedSize;
            }
        }

        public int FileNameLength
        {
            get
            {
                return _FileNameLength;
            }
        }

        public int ExtraFieldLength
        {
            get
            {
                return Utils.ConvertToInt32(_ExtraFieldLength, 2);
            }
        }

        public string FileName
        {
            get
            {
                return _FileName;
            }
        }

        public string Extension
        {
            get
            {
                return Path.GetExtension(FileName).ToLower();
            }
        }

        public string Icon(Config GalleryConfig)
        {
            Config.ItemType mItemType = default(Config.ItemType);
            return Utils.GetFileTypeIcon(Extension, GalleryConfig, ref mItemType);
        }

        public byte[] ExtraField
        {
            get
            {
                return _ExtraField;
            }
        }

        public bool Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                _Selected = value;
            }
        }

        public ZipFileEntry(BinaryReader reader)
        {

            byte[] bytes = reader.ReadBytes(4);
            _Signature = BitConverter.ToString(bytes, 0, 4);

            if (IsValid)
            {
                _Version = reader.ReadBytes(2);
                _CompressionMethod = reader.ReadBytes(2);
                _Flags = reader.ReadBytes(2);
                _ModifiedDateTime = reader.ReadBytes(4);
                _ExtraField = reader.ReadBytes(ExtraFieldLength);
                _CRC32 = reader.ReadBytes(4);
                _CompressedSize = Utils.ConvertToInt64(reader.ReadBytes(4), 4);
                _UncompressedSize = Utils.ConvertToInt64(reader.ReadBytes(4), 4);
                _FileNameLength = Utils.ConvertToInt32(reader.ReadBytes(2), 2);
                _ExtraFieldLength = reader.ReadBytes(2);

                char[] chars = reader.ReadChars(_FileNameLength);
                StringBuilder sb = new StringBuilder(_FileNameLength);
                for (int i = 0; i <= _FileNameLength - 1; i++)
                {
                    sb.Append(chars[i]);
                }
                _FileName = Utils.MakeValidFilename(sb.ToString(), "_");

                _ExtraField = reader.ReadBytes(ExtraFieldLength);
            }
        }
    }

    public class ZipHeaderReader
    {
        private string _filePath;
        private BinaryReader _reader = null;
        private System.IO.Stream _fs = null;
        private bool _isValid;
        private string _lastError = string.Empty;
        private System.Collections.Generic.List<ZipFileEntry> _Entries;
        private bool _IncludeUnselectedInSize = false;
        private int _DefaultCodePage = 0;

        public ZipHeaderReader(string filepath)
        {
            _filePath = filepath;
            AnalyzeZIP(_filePath);
        }

        public ZipHeaderReader(System.IO.Stream fs)
        {
            _fs = fs;
            AnalyzeZIP(_fs);
        }

        public ZipHeaderReader(string filepath, int DefaultCodePage)
        {
            _filePath = filepath;
            _DefaultCodePage = DefaultCodePage;
            AnalyzeZIP(_filePath);
        }

        public ZipHeaderReader(System.IO.Stream fs, int DefaultCodePage)
        {
            _fs = fs;
            _DefaultCodePage = DefaultCodePage;
            AnalyzeZIP(_fs);
        }

        public System.Collections.Generic.List<ZipFileEntry> Entries
        {
            get
            {
                if (ReferenceEquals(_Entries, null))
                {
                    _Entries = new System.Collections.Generic.List<ZipFileEntry>();
                }
                return _Entries;
            }
        }

        public bool IsValid
        {
            get
            {
                bool val = false;
                foreach (ZipFileEntry entry in Entries)
                {
                    val = entry.IsValid;
                    if (!val)
                    {
                        break;
                    }
                }
                return val;
            }
        }

        public int Version
        {
            get
            {
                if (IsValid)
                {
                    return Entries[0].Version;
                }
                else
                {
                    return 0;
                }
            }
        }

        public bool IncludeUnselectedInSize
        {
            get
            {
                return _IncludeUnselectedInSize;
            }
            set
            {
                _IncludeUnselectedInSize = value;
            }
        }

        public long CompressedSize
        {
            get
            {
                long sum = 0;
                if (_IncludeUnselectedInSize)
                {
                    foreach (ZipFileEntry entry in Entries)
                    {
                        sum += entry.CompressedSize;
                    }
                }
                else
                {
                    foreach (ZipFileEntry entry in Entries)
                    {
                        if (entry.Selected)
                        {
                            sum += entry.CompressedSize;
                        }
                    }
                }
                return sum;
            }
        }

        public long UncompressedSize
        {
            get
            {
                long sum = 0;
                if (_IncludeUnselectedInSize)
                {
                    foreach (ZipFileEntry entry in Entries)
                    {
                        sum += entry.UncompressedSize;
                    }
                }
                else
                {
                    foreach (ZipFileEntry entry in Entries)
                    {
                        if (entry.Selected)
                        {
                            sum += entry.UncompressedSize;
                        }
                    }
                }
                return sum;
            }
        }

        public double CompressionRatio
        {
            get
            {
                long _uncompressedSize = UncompressedSize;
                return System.Convert.ToDouble(System.Convert.ToInt32(_uncompressedSize - CompressedSize) / _uncompressedSize);
            }
        }

        public int FileCount
        {
            get
            {
                return Entries.Count;
            }
        }

        public DateTime LastModified
        {
            get
            {
                DateTime maxDate = DateTime.MaxValue;
                foreach (ZipFileEntry entry in Entries)
                {
                    if (entry.LastModified > maxDate)
                    {
                        maxDate = entry.LastModified;
                    }
                }
                return maxDate;
            }
        }

        public string LastError
        {
            get
            {
                return _lastError;
            }
        }

        public bool HasError
        {
            get
            {
                return _lastError.Length > 0;
            }
        }

        public bool ExceedsFileSizeLimit(int Limit)
        {
            foreach (ZipFileEntry entry in Entries)
            {
                if (entry.UncompressedSize > Limit)
                {
                    return true;
                }
            }
            return false;
        }

        private void AnalyzeZIP(string filepath)
        {
            try
            {
                _fs = File.Open(filepath, FileMode.Open);
                AnalyzeZIP(_fs);
            }
            catch (System.IO.IOException ex)
            {
                _lastError = ex.Message + "\r\n" + ex.StackTrace;
                return;
            }
        }

        private void AnalyzeZIP(System.IO.Stream fs)
        {
            try
            {
                if (_DefaultCodePage == 0)
                {
                    _DefaultCodePage = System.Threading.Thread.CurrentThread.CurrentUICulture.TextInfo.OEMCodePage;
                }
                Encoding zipEncoding = Encoding.GetEncoding(_DefaultCodePage);
                _reader = new BinaryReader(fs, zipEncoding);
                while (true)
                {
                    ZipFileEntry entry = new ZipFileEntry(_reader);
                    if (!entry.IsValid)
                    {
                        break;
                    }
                    Entries.Add(entry);
                    //Skip over the data and DataDescriptors
                    if (_reader.BaseStream.CanSeek)
                    {
                        _reader.BaseStream.Seek(entry.CompressedSize, SeekOrigin.Current);
                        if (entry.Flags[2])
                        {
                            _reader.BaseStream.Seek(12, SeekOrigin.Current);
                        }
                    }
                    else
                    {
                        _reader.ReadBytes(Convert.ToInt32(entry.CompressedSize));
                        if (entry.Flags[2])
                        {
                            _reader.ReadBytes(12); //Skip over 12 byte optional data descriptor
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                _lastError = ex.Message + "\r\n" + ex.StackTrace;
            }
            finally
            {
                //NOTE: Cannot close the reader as it will also close the underlying stream
                //which we must keep open and reset to 0 position for zip extraction later.

                if (!ReferenceEquals(_fs, null))
                {
                    if (_fs.CanSeek)
                    {
                        _fs.Seek(0, SeekOrigin.Begin);
                    }
                    else
                    {
                        _fs.Close();
                        _fs = null;
                    }
                }
            }
        }

    }

}
