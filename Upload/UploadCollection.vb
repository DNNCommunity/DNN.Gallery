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

Imports System.IO
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Entities.Users
Imports DotNetNuke.Modules.Gallery.Utils
Imports ICSharpCode.SharpZipLib.Zip

' Base objects for gallery content
' William Severance Converted to ASP.Net 2 Generic.List (Of FileToUpload)
Namespace DotNetNuke.Modules.Gallery

  Public Class GalleryUploadCollection
    Inherits Generic.List(Of FileToUpload)

    Private Const SlidingCacheDuration As Integer = 10 'Minutes

    Private mAlbum As GalleryFolder
    'Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config
    Private mUserID As Integer
    Private mFileListCacheKey As String
    Private mErrMessage As String
    Private mGallerySpaceUsed As Long
    Private mSize As Long
    Private mSpaceAvailable As Long

    Public Shared Function GetList(ByVal Album As GalleryFolder, ByVal ModuleID As Integer, ByVal FileListCacheKey As String) As GalleryUploadCollection

      'Refactored to utilize DataCache.GetCachedData method available in DNN 5.x
      Dim args As New CacheItemArgs(FileListCacheKey, SlidingCacheDuration, CacheItemPriority.AboveNormal, Album, ModuleID)
      Dim pendingUploads As GalleryUploadCollection = DataCache.GetCachedData(Of GalleryUploadCollection)(args, AddressOf GalleryUploadCollectionCacheExpiredCallback)
      pendingUploads.Album = Album 'reset album in case album changed but cached object is still valid
      pendingUploads.RefreshSpaceConstraints()
      Return pendingUploads
    End Function

    Private Shared Function GalleryUploadCollectionCacheExpiredCallback(ByVal args As CacheItemArgs) As Object
      Dim album As GalleryFolder = DirectCast(args.Params(0), GalleryFolder)
      Dim moduleID As Integer = DirectCast(args.Params(1), Integer)
      Return New GalleryUploadCollection(album, moduleID, args.CacheKey)
    End Function

    'William Severance - Modified to take supplied cache key
    Public Shared Sub ResetList(ByVal FileListCacheKey As String)
      DataCache.RemoveCache(FileListCacheKey)
    End Sub

    'William Severance - Modified to take supplied cache key as additional parameter
    Public Sub New(ByVal Album As GalleryFolder, ByVal ModuleID As Integer, ByVal FileListCacheKey As String)
      mAlbum = Album
      mUserID = UserController.GetCurrentUserInfo().UserID
      mFileListCacheKey = FileListCacheKey
      RefreshSpaceConstraints()
    End Sub 'New

    Public Property Album() As GalleryFolder
      Get
        Return mAlbum
      End Get
      Set(ByVal Value As GalleryFolder)
        mAlbum = Value
      End Set
    End Property

    Public ReadOnly Property ErrMessage() As String
      Get
        Return mErrMessage
      End Get
    End Property

    Public ReadOnly Property GalleryConfig() As DotNetNuke.Modules.Gallery.Config
      Get
        'Return mGalleryConfig
        Return mAlbum.GalleryConfig
      End Get
    End Property

    Public ReadOnly Property UserID() As Integer
      Get
        Return mUserID
      End Get
    End Property

    Public ReadOnly Property FileListCacheKey() As String
      Get
        Return mFileListCacheKey
      End Get
    End Property

    Public ReadOnly Property GallerySpaceUsed() As Long
      Get
        Return mGallerySpaceUsed
      End Get
    End Property

    Public ReadOnly Property SpaceAvailable() As Long
      Get
        Return mSpaceAvailable
      End Get
    End Property

    Public ReadOnly Property Size() As Long
      Get
        Return mSize
      End Get
    End Property

    Public Sub AddFileToUpload(ByVal uploadFile As Gallery.FileToUpload)
      MyBase.Add(uploadFile)
      mSpaceAvailable -= uploadFile.ContentLength
      mSize += uploadFile.ContentLength
    End Sub

    Public Sub RemoveFileToUpload(ByVal index As Integer)
      If index >= 0 And index < Count Then
        Dim uploadFile As Gallery.FileToUpload = Item(index)
        mSpaceAvailable += uploadFile.ContentLength
        mSize -= uploadFile.ContentLength
        RemoveAt(index)
      End If
    End Sub

    Public Function FileExists(ByVal FileName As String) As Boolean

      Dim filePath As String = Path.Combine(mAlbum.Path, FileName)
      ' search in file system
      If IO.File.Exists(filePath) Then
        Return True
      End If

      ' search in this collection
      Dim i As Integer

      For i = 0 To Count - 1
        If Item(i).FileName = FileName Then Return True
        If Item(i).Extension = ".zip" Then
          For Each entry As ZipFileEntry In Item(i).ZipHeaders.Entries
            If entry.FileName = FileName Then Return True
          Next
        End If
      Next
      Return False
    End Function

    Public Overloads Function ExceedsQuota() As Boolean
      Return mSpaceAvailable < 0
    End Function

    Public Overloads Function ExceedsQuota(ByVal contentLength As Integer) As Boolean
      Return mSpaceAvailable - contentLength < 0
    End Function

    Public Sub DoUpload()
      Dim uploadFile As FileToUpload
      Dim uploadPath As String
      Dim uploadFilePath As String
      Dim albumFilePath As String
      Dim msgLocalized As String = ""
      mErrMessage = ""
      RefreshSpaceConstraints() 'Refresh the available space based on current storage use
      If ExceedsQuota() Then
        msgLocalized = Localization.GetString("Uploads_ExceedQuota", GalleryConfig.SharedResourceFile)
        If String.IsNullOrEmpty(msgLocalized) Then
          msgLocalized = "The available gallery space of {0:F} kb would be exceeded where these pending uploads to be committed"
        End If
        mErrMessage = String.Format(msgLocalized, (SpaceAvailable + Size) / 1024)
        Exit Sub
      Else
        For i As Integer = Count - 1 To 0 Step -1 ' Go backward to make sure correct item will be remove after uploading
          ' Create object to store file we are uploading
          ' William Severance - changed begin/end of loop to avoid (i-1) inside loop
          uploadFile = Item(i) 'CType(Item(i - 1), FileToUpload)

          Select Case uploadFile.Type
            Case Config.ItemType.Flash, Config.ItemType.Movie
              ' Normal upload, put file right in album directory
              uploadPath = mAlbum.Path

            Case Config.ItemType.Zip
              ' Zip file, put file into temp directory
              uploadPath = BuildPath(New String(1) {mAlbum.Path, GalleryConfig.TempFolder}, "\", False, False)

            Case Else
              ' Image type, put file into _source folder
              uploadPath = mAlbum.SourceFolderPath
          End Select

          uploadFilePath = Path.Combine(uploadPath, uploadFile.FileName)
          albumFilePath = Path.Combine(Album.Path, uploadFile.FileName)

          ' Do upload - create folder if it doesn't exists then upload file
          Try
            If Not Directory.Exists(uploadPath) Then
              Directory.CreateDirectory(uploadPath)
            End If

            uploadFile.HtmlInput.PostedFile.SaveAs(uploadFilePath)

          Catch Exc As System.Exception
            LogException(Exc)
            msgLocalized = Localization.GetString("UploadFile_ProcessError", GalleryConfig.SharedResourceFile)
            If String.IsNullOrEmpty(msgLocalized) Then
              msgLocalized = "An error occured while attempting to process the uploaded file {0}"
            End If
            mErrMessage &= String.Format(msgLocalized, uploadFile.FileName)
            ' Couldn't even upload the file, not much use in continuing
            Exit Sub 'WES: Removed Throw as we do NOT want to generate a second exception
          End Try

          ' If an image, resize it
          If GalleryConfig.IsValidImageType(uploadFile.Extension) Then
            DoResize(uploadFilePath, albumFilePath)
          End If

          ' Update XMLData - except Zip file
          ' If uploaded file is not Zip then update XML data - If it's a Zip then do Unzip
          If Not uploadFile.Type = Config.ItemType.Zip Then
            ' Regular file, save meta data
            Dim metaData As New GalleryXML(mAlbum.Path)
            GalleryXML.SaveMetaData(mAlbum.Path, uploadFile.FileName, -1, _
                        uploadFile.Title, uploadFile.Description, _
                        uploadFile.Categories, uploadFile.Author, _
                        uploadFile.Location, uploadFile.Client, _
                        uploadFile.OwnerID, _
                        Null.NullDate, uploadFile.ApprovedDate, _
                        0)

          Else
            ' Unzip the files
            UnzipFiles(uploadFile, uploadPath, uploadFilePath, albumFilePath)

            ' Delete temp files & folder
            IO.Directory.Delete(uploadPath, True)

          End If 'Upzip

          ' Remove uploaded file off the collection
          RemoveFileToUpload(i) 'RemoveAt(i - 1)
        Next
        RefreshSpaceConstraints()
      End If
    End Sub

    Public Sub RefreshSpaceConstraints()
      Dim ps As PortalSettings = PortalController.GetCurrentPortalSettings()
      mGallerySpaceUsed = Utils.GetDirectorySize(GalleryConfig.RootPath)
      Dim hostSpace As Long
      If ps.HostSpace = 0 Then
        hostSpace = Long.MaxValue
      Else
        hostSpace = CLng(ps.HostSpace) * 1024L * 1024L   'Convert portal hostspace in MB to bytes
      End If
      If GalleryConfig.Quota = 0 Then
        mSpaceAvailable = hostSpace - mGallerySpaceUsed
      Else
        mSpaceAvailable = Math.Min(GalleryConfig.Quota * 1024L, hostSpace) - mGallerySpaceUsed
      End If
      mSize = GetCollectionSize()
      mSpaceAvailable -= mSize
    End Sub

    Private Function GetCollectionSize() As Long
      Dim sz As Long = 0L
      For Each UploadFile As FileToUpload In Me
        sz += UploadFile.ContentLength
      Next
      Return sz
    End Function

    ''' <summary>
    ''' Unzip a bunch of files inside of a zip file
    ''' </summary>
    ''' <param name="UploadFile"></param>
    ''' <param name="UploadPath"></param>
    ''' <param name="UploadFilePath"></param>
    ''' <param name="AlbumFilePath"></param>
    ''' <remarks></remarks>
    Private Sub UnzipFiles(ByVal UploadFile As FileToUpload, _
                           ByVal UploadPath As String, _
                           ByVal UploadFilePath As String, _
                           ByVal AlbumFilePath As String)

      'William Severance = Set the default code page of the ICSharpCode.SharpZipLib to that of the
      'OEMCodePage of the CurrentUICulture to properly process extended character sets such as those found
      'in Latin 1 and Latin 2 cultures (Deutch (de-DE) umlaut, etc.)

      Dim msgLocalized As String = ""
      Dim OEMCodePage As Integer = Threading.Thread.CurrentThread.CurrentUICulture.TextInfo.OEMCodePage
      ICSharpCode.SharpZipLib.Zip.ZipConstants.DefaultCodePage = OEMCodePage

      Dim objZipEntry As ICSharpCode.SharpZipLib.Zip.ZipEntry
      Dim objZipInputStream As ICSharpCode.SharpZipLib.Zip.ZipInputStream

      ' Try unzipping it
      Try
        objZipInputStream = New ZipInputStream(File.OpenRead(UploadFilePath))
      Catch exc As Exception
        LogException(exc)
        msgLocalized = Localization.GetString("UploadFile_OpenZipError", GalleryConfig.SharedResourceFile)
        If String.IsNullOrEmpty(msgLocalized) Then
          msgLocalized = "An error occured while attempting to open the uploaded zip archive file {0}"
        End If
        mErrMessage &= String.Format(msgLocalized, UploadFile.FileName)
        Exit Sub 'WES: Removed Throw as we do not want to generate another exception
      End Try

      objZipEntry = objZipInputStream.GetNextEntry
      While Not objZipEntry Is Nothing
        Dim unzipFile As String = Unzip(objZipEntry, objZipInputStream, UploadPath)
        Dim strExtension As String = Path.GetExtension(unzipFile)

        UploadFilePath = Path.Combine(UploadPath, unzipFile)
        AlbumFilePath = Path.Combine(mAlbum.Path, unzipFile)

        If GalleryConfig.IsValidFlashType(strExtension) _
            OrElse GalleryConfig.IsValidMovieType(strExtension) Then
          ' Normal upload
          File.Copy(UploadFilePath, AlbumFilePath)
          Utils.SaveDNNFile(AlbumFilePath, 0, 0, False, False)

        ElseIf GalleryConfig.IsValidImageType(strExtension) Then

          Dim sourceFilePath As String = Path.Combine(mAlbum.SourceFolderPath, unzipFile)
          File.Copy(UploadFilePath, sourceFilePath)
          DoResize(sourceFilePath, AlbumFilePath)
        End If

        ' Save the meta/xml data
        Dim metaData As New GalleryXML(mAlbum.Path)
        GalleryXML.SaveMetaData(mAlbum.Path, unzipFile, -1, UploadFile.Title, _
                    UploadFile.Description, UploadFile.Categories, _
                    UploadFile.Author, UploadFile.Location, _
                    UploadFile.Client, UploadFile.OwnerID, _
                    Null.NullDate, UploadFile.ApprovedDate, _
                    0)

        ' Do next file in zip
        objZipEntry = objZipInputStream.GetNextEntry

      End While
    End Sub

    ''' <summary>
    ''' Unzip an individual file and save it to a directory
    ''' </summary>
    ''' <param name="ZipEntry"></param>
    ''' <param name="InputStream"></param>
    ''' <param name="TempDir"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function Unzip(ByVal ZipEntry As ZipEntry, ByVal InputStream As ZipInputStream, ByVal TempDir As String) As String
      Dim strFileName As String = ""
      Dim strFileNamePath As String
      Dim localizedError As String = ""

      Try
        strFileName = Utils.MakeValidFilename(Path.GetFileName(ZipEntry.Name), "_")

        If strFileName <> "" Then
          strFileNamePath = Path.Combine(TempDir, strFileName)

          If File.Exists(strFileNamePath) Then
            File.Delete(strFileNamePath)
          End If

          Dim objFileStream As FileStream = File.Create(strFileNamePath)
          Dim intSize As Integer = 2048
          Dim arrData(2048) As Byte

          intSize = InputStream.Read(arrData, 0, arrData.Length)
          While intSize > 0
            objFileStream.Write(arrData, 0, intSize)
            intSize = InputStream.Read(arrData, 0, arrData.Length)
          End While

          objFileStream.Close()

          Return strFileName
        End If

      Catch Exc As System.Exception
        LogException(Exc)
        localizedError = Localization.GetString("UploadFile_UnZipError", GalleryConfig.SharedResourceFile)
        If String.IsNullOrEmpty(localizedError) Then
          localizedError = "An error occured while attempting to unzip file {0} from the uploaded archive"
        End If
        mErrMessage &= String.Format(localizedError, strFileName)
      End Try

      ' JIMJ We couldn't get the filename
      Return String.Empty
    End Function

    ''' <summary>
    ''' Resize the uploaded image and put it into the album's folder
    ''' </summary>
    ''' <param name="UploadFile">File to resize</param>
    ''' <param name="AlbumFile">Where to store the resized file</param>
    ''' <remarks></remarks>
    Private Sub DoResize(ByVal UploadFile As String, ByVal AlbumFile As String)

      Dim localizedError As String = ""

      Try
        ' Do Resize
        If GalleryConfig.IsFixedSize Then
          ' Save it to album folder for display
          With GalleryConfig
            ResizeImage(UploadFile, AlbumFile, .FixedWidth, .FixedHeight, .EncoderQuality)
          End With
        Else
          File.Copy(UploadFile, AlbumFile)
          Utils.SaveDNNFile(AlbumFile, 0, 0, False, False)
        End If

        ' Delete original source file to save disk space
        If Not GalleryConfig.IsKeepSource Then
          File.Delete(UploadFile)
        Else
          Utils.SaveDNNFile(UploadFile, 0, 0, False, False) 'Add to the DNN files table if we're not deleting the source
        End If
      Catch exc As System.Exception
        LogException(exc)
        localizedError = Localization.GetString("UploadFile_ResizeSaveError", GalleryConfig.SharedResourceFile)
        If String.IsNullOrEmpty(localizedError) Then
          localizedError = "An error occured while attempting to resize and save the uploaded file {0}"
        End If
        mErrMessage &= String.Format(localizedError, Path.GetFileName(UploadFile))
      End Try
    End Sub

  End Class

End Namespace