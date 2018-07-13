'
' DotNetNuke® - http://www.dotnetnuke.com
' Copyright (c) 2002-2011 by DotNetNuke Corp. 

'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
' DEALINGS IN THE SOFTWARE.

Option Strict On
Option Explicit On

Imports System.Text
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Runtime.InteropServices
Imports DotNetNuke

Namespace DotNetNuke.Modules.Gallery

    'Class to read and analyze the header of an Adobe flash file (SWF)
    'Written by William Severance - 9-5-08
    'Based on Adobe SWF Format Specification v 9. See http://www.adobe.com/devnet/swf/pdf/swf_file_format_spec_v9.pdf

    'To use:
    '   Dim swfProperties As New SWFHeaderReader(filePath)
    '   Properties exposed:
    '      swfProperties.IsValid bool - True if valid SWF file
    '      swfProperties.IsCompressed bool - True if SWF data is compressed
    '      swfProperties.Version byte - Adobe SWF version #
    '      swfProperties.FileLength integer - number of bytes in file (uncompressed length)
    '      swfProperties.Width integer - Frame Width in pixels
    '      swfProperties.Height integer - Frame Height in pixels
    '      swfProperties.FrameRate double - Frame Rate
    '      swfProperties.FrameCount integer - number of frames in movie
    '      swfProperties.HasError bool - True if error in reading/analyzing file
    '      swfProperties.LastError string - last exception message and stack trace

    Public Class SWFHeaderReader
        Private filePath As String
        Private _height As Integer
        Private _width As Integer
        Private _isValid As Boolean
        Private _isCompressed As Boolean
        Private _frameRate As Double
        Private _frameCount As Integer
        Private _version As Byte
        Private _fileLength As Integer
        Private _lastError As String = String.Empty

        <StructLayout(LayoutKind.Sequential, Pack:=1)> _
        Private Class GZip
            Inherits Object
            <FieldOffset(0)> Private mBytes() As Byte           'Overlayed byte array
            <FieldOffset(0)> Private mHeader(9) As Byte         'Header info - 10 bytes
            Private mData() As Byte                             'Data bytes


            Public Property Header() As Byte()
                Get
                    Return mHeader
                End Get
                Set(ByVal value As Byte())
                    If value.Length = 10 Then
                        mHeader = value
                    Else
                        Throw New ArrayTypeMismatchException("GZip header must be 10 bytes in length")
                    End If
                End Set
            End Property

            Public Property Data() As Byte()
                Get
                    Return mData
                End Get
                Set(ByVal value As Byte())
                    mData = value
                End Set
            End Property

            Public Property Bytes() As Byte()
                Get
                    Return mBytes
                End Get
                Set(ByVal value As Byte())
                    mBytes = value
                End Set
            End Property

            Public Property Byte0() As Byte
                Get
                    Return mBytes(0)
                End Get
                Set(ByVal value As Byte)
                    mBytes(0) = value
                End Set
            End Property

            Public Property DataLength() As Integer
                Get
                    Return mData.Length
                End Get
                Set(ByVal value As Integer)
                    Array.Resize(mData, value)
                End Set
            End Property

            Public Function PopByte() As Byte
                Dim byte0 As Byte = mData(0)
                Array.Reverse(mData)
                DataLength -= 1
                Array.Reverse(mData)
                Return byte0
            End Function
        End Class

        Public Sub New(ByVal filepath As String)
            Me.filePath = filepath
            AnalyzeSWF()
        End Sub

        Public ReadOnly Property IsValid() As Boolean
            Get
                Return _isValid
            End Get
        End Property

        Public ReadOnly Property IsCompressed() As Boolean
            Get
                Return _isValid AndAlso _isCompressed
            End Get
        End Property

        Public ReadOnly Property FrameRate() As Double
            Get
                Return _frameRate
            End Get
        End Property

        Public ReadOnly Property FrameCount() As Integer
            Get
                Return _frameCount
            End Get
        End Property

        Public ReadOnly Property FileLength() As Integer
            Get
                Return _fileLength
            End Get
        End Property

        Public ReadOnly Property Version() As Byte
            Get
                Return _version
            End Get
        End Property

        Public ReadOnly Property Width() As Integer
            Get
                Return _width
            End Get
        End Property

        Public ReadOnly Property Height() As Integer
            Get
                Return _height
            End Get
        End Property

        Public ReadOnly Property LastError() As String
            Get
                Return _lastError
            End Get
        End Property

        Public ReadOnly Property HasError() As Boolean
            Get
                Return _lastError.Length > 0
            End Get
        End Property

        Private Sub AnalyzeSWF()
            Dim fs As FileStream = Nothing
            Dim reader As BinaryReader = Nothing
            Dim i, j As Integer

            Const Mask00000111 As Byte = &H7
            Const Mask10000000 As Byte = &H80

            Try
                fs = File.Open(filePath, FileMode.Open)
            Catch ex As System.IO.IOException
                _lastError = ex.Message & vbCrLf & ex.StackTrace
                Exit Sub
            End Try

            Try
                reader = New BinaryReader(fs)

                Dim signature As String = New String(reader.ReadChars(3))
                Select Case signature
                    Case "FWS"
                        _isValid = True
                    Case "CWS"
                        _isValid = True
                        _isCompressed = True
                    Case Else
                        Exit Sub
                End Select

                _version = reader.ReadByte()

                'Read next 4 bytes as fileLength LSB...MSB (little endian)
                _fileLength = 0
                Dim b As Int64
                For i = 0 To 3
                    b = reader.ReadByte()
                    _fileLength += Convert.ToInt32((b << (8 * i)))
                Next i

                Dim buffer As New GZip

                'Read the rest of the file
                buffer.Data = reader.ReadBytes(_fileLength)

                If IsCompressed Then
                    'data is compressed (gzip format - RFC 1952) - so first decompress
                    'set 10-byte GZip header which GZipStream can process see http://www.faqs.org/rfcs/rfc1952.html

                    Dim header() As Byte = New Byte() {31, 139, 8, 0, 0, 0, 0, 0, 4, 0}
                    header.CopyTo(buffer.Header, 0)

                    Dim ms As New MemoryStream(buffer.Bytes)

                    Dim gzip As New System.IO.Compression.GZipStream(ms, Compression.CompressionMode.Decompress)

                    Dim decompressedBuffer(buffer.DataLength + 1000000) As Byte

                    Dim gzipLength As Integer = Utils.ReadAllBytesFromStream(gzip, decompressedBuffer)

                    gzip.Close()
                    ms.Close()

                    buffer.DataLength = gzipLength
                    Array.Resize(decompressedBuffer, gzipLength)
                    decompressedBuffer.CopyTo(buffer.Data, 0)
                    decompressedBuffer = Nothing

                End If

                'Read the variable length FrameSize RECT (4 values)

                Dim byte0 As Byte = buffer.PopByte()
                Dim nBits As Integer = Convert.ToInt32(byte0 >> 3)

                Dim bits As BitArray = New BitArray(nBits, False)

                'Current byte
                byte0 = byte0 And Mask00000111
                byte0 <<= 5

                'current bit (first byte is already shifted)
                Dim bitPtr As Integer = 2

                'Must get all 4 values in the RECT
                For i = 0 To 3
                    For j = 0 To bits.Count - 1
                        If (byte0 And Mask10000000) > 0 Then bits(j) = True
                        byte0 <<= 1
                        'byte0 = byte0 And Mask11111111
                        bitPtr -= 1

                        'get next byte if we've run out of bits in current byte
                        If bitPtr < 0 Then
                            byte0 = buffer.PopByte()
                            bitPtr = 7
                        End If
                    Next j

                    'got the value now calculate
                    Dim c As Integer = 1
                    Dim value As Integer = 0

                    For j = bits.Count - 1 To 0 Step -1
                        If bits(j) Then value += c
                        c <<= 1
                    Next j

                    value \= 20 'Convert from twipps to px where 1 px = 20 twipps

                    Select Case i
                        Case 0 'X1
                            _width = value
                        Case 1 'X2
                            _width = value - _width
                        Case 2 'Y1
                            _height = value
                        Case 3 'Y2
                            _height = value - _height
                    End Select

                    bits.SetAll(False)
                Next

                'now get the frame rate
                _frameRate = Convert.ToDouble(buffer.Data(1))
                _frameRate += Convert.ToDouble(buffer.Data(0) / 100)

                'and the frame count
                _frameCount = BitConverter.ToInt16(buffer.Data, 2)

            Catch ex As Exception
                _lastError = ex.Message & vbCrLf & ex.StackTrace
            Finally
                If Not reader Is Nothing Then reader.Close()
                If Not fs Is Nothing Then fs.Close()
                reader = Nothing
                fs = Nothing
            End Try
        End Sub

    End Class

    'Classes to read and analyze the headers of files compressed into a zip folder
    'Written by William Severance - 9-9-08
    'Based on PKWare Zip Format Specification. See http://www.pkware.com/documents/casestudies/APPNOTE.TXT


    Public Class ZipFileEntry
        Inherits Object

        Private _Signature As String           'Signature - 4 chars = (0x04034b50)
        Private _Version(1) As Byte            'Version to extract - 2 bytes
        Private _Flags(1) As Byte              'General Purpose Flags - 2 bytes
        Private _CompressionMethod(1) As Byte  'Compression Method - 2 bytes
        Private _ModifiedDateTime(3) As Byte   'Last Modified Time and Date - 4 bytes (MS-DOS Format)
        Private _CRC32(3) As Byte              'CRC-32 - 4 bytes
        Private _CompressedSize As Int64       'Compressed Size - 4 bytes
        Private _UncompressedSize As Int64     'Uncompressed Size - 4 bytes
        Private _FileNameLength As Integer     'Filename Length - 2 bytes
        Private _ExtraFieldLength(1) As Byte   'Extra Field Length - 2 bytes
        Private _FileName As String            'FileName - varible length
        Private _ExtraField() As Byte          'ExtraField - variable length
        Private _Selected As Boolean = True    'WESJr. Reserved for future use - to select files for extraction.

        Const ExpectedSignature As String = "50-4B-03-04"

        Public ReadOnly Property Signature() As String
            Get
                Return _Signature
            End Get
        End Property

        Public ReadOnly Property IsValid() As Boolean
            Get
                Return _Signature = ExpectedSignature
            End Get
        End Property

        Public ReadOnly Property Version() As Int32
            Get
                Return Utils.ConvertToInt32(_Version, 2)
            End Get
        End Property

        Public ReadOnly Property Flags() As BitArray
            Get
                Return New BitArray(_Flags)
            End Get
        End Property

        Public ReadOnly Property CompressionMethod() As Int32
            Get
                Return Utils.ConvertToInt32(_CompressionMethod, 2)
            End Get
        End Property

        Public ReadOnly Property LastModified() As DateTime
            Get
                Return Utils.ConvertToDateTime(_ModifiedDateTime)
            End Get
        End Property

        Public ReadOnly Property CRC() As Int64
            Get
                Return Utils.ConvertToInt64(_CRC32, 4)
            End Get
        End Property

        Public ReadOnly Property CompressedSize() As Int64
            Get
                Return _CompressedSize
            End Get
        End Property

        Public ReadOnly Property UncompressedSize() As Int64
            Get
                Return _UncompressedSize
            End Get
        End Property

        Public ReadOnly Property FileNameLength() As Int32
            Get
                Return _FileNameLength
            End Get
        End Property

        Public ReadOnly Property ExtraFieldLength() As Int32
            Get
                Return Utils.ConvertToInt32(_ExtraFieldLength, 2)
            End Get
        End Property

        Public ReadOnly Property FileName() As String
            Get
                Return _FileName
            End Get
        End Property

        Public ReadOnly Property Extension() As String
            Get
                Return Path.GetExtension(FileName).ToLower
            End Get
        End Property

        Public ReadOnly Property Icon(ByVal GalleryConfig As Config) As String
            Get
                Dim mItemType As Config.ItemType
                Return Utils.GetFileTypeIcon(Extension, GalleryConfig, mItemType)
            End Get
        End Property

        Public ReadOnly Property ExtraField() As Byte()
            Get
                Return _ExtraField
            End Get
        End Property

        Public Property Selected() As Boolean
            Get
                Return _Selected
            End Get
            Set(ByVal value As Boolean)
                _Selected = value
            End Set
        End Property

        Public Sub New(ByVal reader As BinaryReader)

            Dim bytes() As Byte = reader.ReadBytes(4)
            _Signature = BitConverter.ToString(bytes, 0, 4)

            If IsValid Then
                _Version = reader.ReadBytes(2)
                _CompressionMethod = reader.ReadBytes(2)
                _Flags = reader.ReadBytes(2)
                _ModifiedDateTime = reader.ReadBytes(4)
                _ExtraField = reader.ReadBytes(ExtraFieldLength)
                _CRC32 = reader.ReadBytes(4)
                _CompressedSize = Utils.ConvertToInt64(reader.ReadBytes(4), 4)
                _UncompressedSize = Utils.ConvertToInt64(reader.ReadBytes(4), 4)
                _FileNameLength = Utils.ConvertToInt32(reader.ReadBytes(2), 2)
                _ExtraFieldLength = reader.ReadBytes(2)

                Dim chars() As Char = reader.ReadChars(_FileNameLength)
                Dim sb As New StringBuilder(_FileNameLength)
                For i As Integer = 0 To _FileNameLength - 1
                    sb.Append(chars(i))
                Next
                _FileName = Utils.MakeValidFilename(sb.ToString, "_")

                _ExtraField = reader.ReadBytes(ExtraFieldLength)
            End If
        End Sub
    End Class

    Public Class ZipHeaderReader
        Private _filePath As String
        Private _reader As BinaryReader = Nothing
        Private _fs As System.IO.Stream = Nothing
        Private _isValid As Boolean
        Private _lastError As String = String.Empty
        Private _Entries As Generic.List(Of ZipFileEntry)
        Private _IncludeUnselectedInSize As Boolean = False
        Private _DefaultCodePage As Integer = 0

        Public Sub New(ByVal filepath As String)
            _filePath = filepath
            AnalyzeZIP(_filePath)
        End Sub

        Public Sub New(ByVal fs As System.IO.Stream)
            _fs = fs
            AnalyzeZIP(_fs)
        End Sub

        Public Sub New(ByVal filepath As String, ByVal DefaultCodePage As Integer)
            _filePath = filepath
            _DefaultCodePage = DefaultCodePage
            AnalyzeZIP(_filePath)
        End Sub

        Public Sub New(ByVal fs As System.IO.Stream, ByVal DefaultCodePage As Integer)
            _fs = fs
            _DefaultCodePage = DefaultCodePage
            AnalyzeZIP(_fs)
        End Sub

        Public ReadOnly Property Entries() As Generic.List(Of ZipFileEntry)
            Get
                If _Entries Is Nothing Then
                    _Entries = New Generic.List(Of ZipFileEntry)
                End If
                Return _Entries
            End Get
        End Property

        Public ReadOnly Property IsValid() As Boolean
            Get
                Dim val As Boolean = False
                For Each entry As ZipFileEntry In Entries
                    val = entry.IsValid
                    If Not val Then Exit For
                Next
                Return val
            End Get
        End Property

        Public ReadOnly Property Version() As Integer
            Get
                If IsValid Then
                    Return Entries(0).Version
                Else
                    Return 0
                End If
            End Get
        End Property

        Public Property IncludeUnselectedInSize() As Boolean
            Get
                Return _IncludeUnselectedInSize
            End Get
            Set(ByVal value As Boolean)
                _IncludeUnselectedInSize = value
            End Set
        End Property

        Public ReadOnly Property CompressedSize() As Long
            Get
                Dim sum As Long = 0L
                If _IncludeUnselectedInSize Then
                    For Each entry As ZipFileEntry In Entries
                        sum += entry.CompressedSize
                    Next
                Else
                    For Each entry As ZipFileEntry In Entries
                        If entry.Selected Then sum += entry.CompressedSize
                    Next
                End If
                Return sum
            End Get
        End Property

        Public ReadOnly Property UncompressedSize() As Long
            Get
                Dim sum As Long = 0L
                If _IncludeUnselectedInSize Then
                    For Each entry As ZipFileEntry In Entries
                        sum += entry.UncompressedSize
                    Next
                Else
                    For Each entry As ZipFileEntry In Entries
                        If entry.Selected Then sum += entry.UncompressedSize
                    Next
                End If
                Return sum
            End Get
        End Property

        Public ReadOnly Property CompressionRatio() As Double
            Get
                Dim _uncompressedSize As Long = UncompressedSize
                Return (_uncompressedSize - CompressedSize) / _uncompressedSize
            End Get
        End Property

        Public ReadOnly Property FileCount() As Integer
            Get
                Return Entries.Count
            End Get
        End Property

        Public ReadOnly Property LastModified() As DateTime
            Get
                Dim maxDate As DateTime = DateTime.MaxValue
                For Each entry As ZipFileEntry In Entries
                    If entry.LastModified > maxDate Then maxDate = entry.LastModified
                Next
                Return maxDate
            End Get
        End Property

        Public ReadOnly Property LastError() As String
            Get
                Return _lastError
            End Get
        End Property

        Public ReadOnly Property HasError() As Boolean
            Get
                Return _lastError.Length > 0
            End Get
        End Property

        Public Function ExceedsFileSizeLimit(ByVal Limit As Integer) As Boolean
            For Each entry As ZipFileEntry In Entries
                If entry.UncompressedSize > Limit Then
                    Return True
                End If
            Next
            Return False
        End Function

        Private Overloads Sub AnalyzeZIP(ByVal filepath As String)
            Try
                _fs = File.Open(filepath, FileMode.Open)
                AnalyzeZIP(_fs)
            Catch ex As System.IO.IOException
                _lastError = ex.Message & vbCrLf & ex.StackTrace
                Exit Sub
            End Try
        End Sub

        Private Overloads Sub AnalyzeZIP(ByVal fs As System.IO.Stream)
            Try
                If _DefaultCodePage = 0 Then
                    _DefaultCodePage = Threading.Thread.CurrentThread.CurrentUICulture.TextInfo.OEMCodePage
                End If
                Dim zipEncoding As Encoding = Encoding.GetEncoding(_DefaultCodePage)
                _reader = New BinaryReader(fs, zipEncoding)
                While True
                    Dim entry As ZipFileEntry = New ZipFileEntry(_reader)
                    If Not entry.IsValid Then Exit While
                    Entries.Add(entry)
                    'Skip over the data and DataDescriptors
                    If _reader.BaseStream.CanSeek Then
                        _reader.BaseStream.Seek(entry.CompressedSize, SeekOrigin.Current)
                        If entry.Flags(2) Then
                            _reader.BaseStream.Seek(12, SeekOrigin.Current)
                        End If
                    Else
                        _reader.ReadBytes(Convert.ToInt32(entry.CompressedSize))
                        If entry.Flags(2) Then
                            _reader.ReadBytes(12) 'Skip over 12 byte optional data descriptor
                        End If
                    End If
                End While

            Catch ex As Exception
                _lastError = ex.Message & vbCrLf & ex.StackTrace
            Finally
                'NOTE: Cannot close the reader as it will also close the underlying stream
                'which we must keep open and reset to 0 position for zip extraction later.

                If Not _fs Is Nothing Then
                    If _fs.CanSeek Then
                        _fs.Seek(0, SeekOrigin.Begin)
                    Else
                        _fs.Close()
                        _fs = Nothing
                    End If
                End If
            End Try
        End Sub

    End Class

End Namespace