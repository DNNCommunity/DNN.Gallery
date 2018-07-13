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
'
Option Strict On

Imports System
Imports System.Drawing
Imports System.Reflection
Imports System.Collections
Imports System.Text
Imports System.IO
Imports System.Web
Imports System.Web.UI
Imports System.Threading
Imports System.Collections.Generic
Imports DotNetNuke
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.Services
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Modules.Gallery.Config
Imports DotNetNuke.Modules.Gallery.Utils

Namespace DotNetNuke.Modules.Gallery

  Public Class GalleryFolder
    Implements IGalleryObjectInfo

#Region "Private Variables"
    Private mIndex As Integer
    Private mID As Integer = -1
    ' System info
    Private mParent As GalleryFolder
    Private mGalleryHierarchy As String
    Private mName As String
    Private mURL As String
    Private mPath As String
    Private mThumbnail As String
    Private mThumbnailURL As String
    Private mThumbFolderPath As String
    Private mThumbFolderURL As String
    Private mIcon As String
    Private mSourceFolderPath As String
    Private mSourceFolderURL As String

    ' Info from XML also interface implementation
    Private mTitle As String
    Private mDescription As String
    Private mCategories As String
    Private mAuthor As String
    Private mClient As String
    Private mLocation As String
    Private mOwnerID As Integer
    Private mApprovedDate As DateTime
    Private mCreatedDate As DateTime

    ' Interface implementation for display purpose        
    Private mPopupSlideshowURL As String = ""
    Private mItemInfo As String = ""
    Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config

    Private mIsPopulated As Boolean
    Private mWatermarkImage As GalleryFile
    Private mList As New GalleryObjectCollection
    Private mSortList As New List(Of IGalleryObjectInfo)

    Private mBrowsableItems As New List(Of Integer)
    Private mMediaItems As New List(Of Integer)
    Private mFlashItems As New List(Of Integer)
    Private mIconItems As New List(Of IGalleryObjectInfo)
    Private mUnApprovedItems As New List(Of Integer)

#End Region

#Region "Events"
    Public Event GalleryObjectDeleted(ByVal sender As Object, ByVal e As GalleryObjectEventArgs)
    Public Event GalleryObjectCreated(ByVal sender As Object, ByVal e As GalleryObjectEventArgs)
#End Region

#Region "Properties"

    '<tamttt:note identity id for all  module objects>
    Public ReadOnly Property ObjectTypeCode() As Integer
      Get
        Return 501
      End Get
    End Property

    Public ReadOnly Property Index() As Integer Implements IGalleryObjectInfo.Index
      Get
        Return mIndex
      End Get
    End Property

    Public ReadOnly Property ID() As Integer Implements IGalleryObjectInfo.ID
      Get
        Return mID
      End Get
    End Property

    Public ReadOnly Property Parent() As GalleryFolder Implements IGalleryObjectInfo.Parent
      Get
        Return mParent
      End Get
    End Property

    Public ReadOnly Property GalleryHierarchy() As String
      Get
        Return mGalleryHierarchy
      End Get
    End Property

    Public ReadOnly Property Name() As String Implements IGalleryObjectInfo.Name
      Get
        Return mName
      End Get
    End Property

    Public ReadOnly Property URL() As String Implements IGalleryObjectInfo.URL
      Get
        Return mURL
      End Get
    End Property

    Public ReadOnly Property Path() As String Implements IGalleryObjectInfo.Path
      Get
        Return mPath
      End Get
    End Property

    Public Property Thumbnail() As String Implements IGalleryObjectInfo.Thumbnail
      Get
        Return mThumbnail
      End Get
      Set(ByVal Value As String)
        mThumbnail = Value
      End Set
    End Property

    Public Property ThumbnailURL() As String Implements IGalleryObjectInfo.ThumbnailURL
      Get
        Return mThumbnailURL
      End Get
      Set(ByVal Value As String)
        mThumbnailURL = Value
      End Set
    End Property

    Public ReadOnly Property ThumbFolderPath() As String
      Get
        Return mThumbFolderPath
      End Get
    End Property

    Public ReadOnly Property ThumbFolderURL() As String
      Get
        Return mThumbFolderURL
      End Get
    End Property

    ' We actually don't need this property, however keep it here for implementing gallery file
    Public ReadOnly Property SourceURL() As String Implements IGalleryObjectInfo.SourceURL
      Get
        Return mSourceFolderURL
      End Get
    End Property

    Public ReadOnly Property SourceFolderPath() As String
      Get
        Return mSourceFolderPath
      End Get
    End Property

    Public ReadOnly Property SourceFolderURL() As String
      Get
        Return mSourceFolderURL
      End Get
    End Property

    Public ReadOnly Property Icon() As String
      Get
        Return mIcon
      End Get
    End Property

    Public ReadOnly Property IconURL() As String Implements IGalleryObjectInfo.IconURL
      Get
        Return mGalleryConfig.GetImageURL("s_Folder.gif")
      End Get
    End Property

    Public ReadOnly Property ScoreImageURL() As String Implements IGalleryObjectInfo.ScoreImageURL
      Get
        Return ""
      End Get
    End Property

    Public Property Title() As String Implements IGalleryObjectInfo.Title
      Get
        Return mTitle
      End Get
      Set(ByVal Value As String)
        mTitle = Value
      End Set
    End Property

    Public Property Description() As String Implements IGalleryObjectInfo.Description
      Get
        Return mDescription
      End Get
      Set(ByVal Value As String)
        mDescription = Value
      End Set
    End Property

    Public Property Categories() As String Implements IGalleryObjectInfo.Categories
      Get
        Return mCategories
      End Get
      Set(ByVal Value As String)
        mCategories = Value
      End Set
    End Property

    Public Property Author() As String Implements IGalleryObjectInfo.Author
      Get
        Return mAuthor
      End Get
      Set(ByVal Value As String)
        mAuthor = Value
      End Set
    End Property

    Public Property Client() As String Implements IGalleryObjectInfo.Client
      Get
        Return mClient
      End Get
      Set(ByVal Value As String)
        mClient = Value
      End Set
    End Property

    Public Property Location() As String Implements IGalleryObjectInfo.Location
      Get
        Return mLocation
      End Get
      Set(ByVal Value As String)
        mLocation = Value
      End Set
    End Property

    Public Property OwnerID() As Integer Implements IGalleryObjectInfo.OwnerID
      Get
        Return mOwnerID
      End Get
      Set(ByVal Value As Integer)
        mOwnerID = Value
      End Set
    End Property

    Public Property Width() As Integer Implements IGalleryObjectInfo.Width
      Get
        Return -1
      End Get
      Set(ByVal Value As Integer)
        ' Do Nothing
      End Set
    End Property

    Public Property Height() As Integer Implements IGalleryObjectInfo.Height
      Get
        Return -1
      End Get
      Set(ByVal Value As Integer)
        ' Do Nothing
      End Set
    End Property

    Public Property Score() As Double Implements IGalleryObjectInfo.Score
      Get
        Return 0
      End Get
      Set(ByVal Value As Double)
        'Do Nothing
      End Set
    End Property

    Public Property ApprovedDate() As DateTime Implements IGalleryObjectInfo.ApprovedDate
      Get
        Return mApprovedDate
      End Get
      Set(ByVal Value As DateTime)
        mApprovedDate = Value
      End Set
    End Property

    Public ReadOnly Property CreatedDate() As DateTime Implements IGalleryObjectInfo.CreatedDate
      Get
        Return mCreatedDate
      End Get
    End Property

    ' Collection of items that can be viewed using the viewer
    Public ReadOnly Property BrowsableItems() As List(Of Integer)
      Get
        Return mBrowsableItems
      End Get
    End Property

    ' Collection of items that can be played by Media player
    Public ReadOnly Property MediaItems() As List(Of Integer)
      Get
        Return mMediaItems
      End Get
    End Property

    ' Collection of items that can be played by flash player
    Public ReadOnly Property FlashItems() As List(Of Integer)
      Get
        Return mFlashItems
      End Get
    End Property

    ' These items not to be view in GUI
    Public ReadOnly Property IconItems() As List(Of IGalleryObjectInfo)
      Get
        Return mIconItems
      End Get
    End Property

    Public ReadOnly Property UnApprovedItems() As List(Of Integer)
      Get
        Return mUnApprovedItems
      End Get
    End Property

    ' Is this a folder?
    Public ReadOnly Property IsFolder() As Boolean Implements IGalleryObjectInfo.IsFolder
      Get
        Return True
      End Get
    End Property

    Public ReadOnly Property Type() As Config.ItemType Implements IGalleryObjectInfo.Type
      Get
        Return Config.ItemType.Folder
      End Get
    End Property

    ' Collection of objects implementing IGalleryObjectInfo
    Public ReadOnly Property List() As GalleryObjectCollection
      Get
        Return mList
      End Get
    End Property

    Public ReadOnly Property SortList() As List(Of IGalleryObjectInfo)
      Get
        Return mSortList
      End Get
    End Property

    ' Whether this folder has been populated or not
    Public Property IsPopulated() As Boolean
      Get
        Return mIsPopulated
      End Get
      Set(ByVal Value As Boolean)
        mIsPopulated = Value
      End Set
    End Property

    ' Whether this folder has at least one browsable item
    Public ReadOnly Property IsBrowsable() As Boolean
      Get
        Return (mBrowsableItems.Count > 0)
      End Get
    End Property

    Public ReadOnly Property Size() As Long Implements IGalleryObjectInfo.Size
      Get
        Return mList.Count
      End Get
    End Property

    'Public ReadOnly Property ImageCssClass() As String Implements IGalleryObjectInfo.ImageCssClass
    '  Get
    '    Return ""
    '  End Get
    'End Property

    'Public ReadOnly Property ThumbnailCssClass() As String Implements IGalleryObjectInfo.ThumbnailCssClass
    '  Get
    '    Return ""
    '  End Get
    'End Property

    Public ReadOnly Property BrowserURL() As String Implements IGalleryObjectInfo.BrowserURL
      Get
        'WES - A folder/album can never be a popup so don't ever need PopupBrowserURL
        Return GetBrowserURL()
      End Get
    End Property

    Public ReadOnly Property SlideshowURL() As String Implements IGalleryObjectInfo.SlideshowURL
      Get
        If GalleryConfig.AllowPopup Then
          Return GetPopupSlideshowURL()
        Else
          Return GetSlideshowURL()
        End If
      End Get
    End Property

    Public ReadOnly Property EditURL() As String Implements IGalleryObjectInfo.EditURL
      Get
        Return GetEditURL()
      End Get
    End Property

    Public ReadOnly Property MaintenanceURL() As String
      Get
        Return GetMaintenanceURL()
      End Get
    End Property

    Public ReadOnly Property AddSubAlbumURL() As String
      Get
        Return GetAddSubAlbumURL()
      End Get
    End Property

    Public ReadOnly Property AddFileURL() As String
      Get
        Return GetAddFileURL()
      End Get
    End Property

    Public ReadOnly Property ExifURL() As String Implements IGalleryObjectInfo.ExifURL
      Get
        Return ""
      End Get
    End Property

    Public ReadOnly Property DownloadURL() As String Implements IGalleryObjectInfo.DownloadURL
      Get
        Return ""
      End Get
    End Property

    Public ReadOnly Property VotingURL() As String Implements IGalleryObjectInfo.VotingURL
      Get
        Return ""
      End Get
    End Property

    Public ReadOnly Property ItemInfo() As String Implements IGalleryObjectInfo.ItemInfo
      Get
        Return mItemInfo
      End Get
    End Property

    Public ReadOnly Property WatermarkImage() As GalleryFile
      Get
        Return mWatermarkImage
      End Get
    End Property

    Public ReadOnly Property GalleryConfig() As DotNetNuke.Modules.Gallery.Config Implements IGalleryObjectInfo.GalleryConfig
      Get
        Return mGalleryConfig
      End Get
    End Property
#End Region

#Region "Public Methods and Functions"
    Sub New()
      MyBase.New()
    End Sub

    Friend Sub New( _
        ByVal Index As Integer, _
        ByVal ID As Integer, _
        ByVal Parent As GalleryFolder, _
        ByVal GalleryHierarchy As String, _
        ByVal Name As String, _
        ByVal URL As String, _
        ByVal Path As String, _
        ByVal Thumbnail As String, _
        ByVal ThumbnailURL As String, _
        ByVal ThumbFolderPath As String, _
        ByVal ThumbFolderURL As String, _
        ByVal Icon As String, _
        ByVal IconURL As String, _
        ByVal SourceFolderPath As String, _
        ByVal SourceFolderURL As String, _
        ByVal Title As String, _
        ByVal Description As String, _
        ByVal Categories As String, _
        ByVal Author As String, _
        ByVal Client As String, _
        ByVal Location As String, _
        ByVal OwnerID As Integer, _
        ByVal ApprovedDate As DateTime, _
        ByVal CreatedDate As DateTime, _
        ByVal GalleryConfig As DotNetNuke.Modules.Gallery.Config)

      mIndex = Index
      mID = ID
      mParent = Parent
      mGalleryHierarchy = GalleryHierarchy
      mName = Name
      mURL = URL
      mPath = Path
      mThumbnail = Thumbnail
      mThumbnailURL = ThumbnailURL
      mThumbFolderPath = ThumbFolderPath
      mThumbFolderURL = ThumbFolderURL
      mIcon = Icon
      mSourceFolderPath = SourceFolderPath
      mSourceFolderURL = SourceFolderURL
      mTitle = Title
      mDescription = Description
      mCategories = Categories
      mAuthor = Author
      mClient = Client
      mLocation = Location
      mOwnerID = OwnerID
      mApprovedDate = ApprovedDate
      mCreatedDate = CreatedDate
      mGalleryConfig = GalleryConfig
    End Sub

    Public Sub Clear()
      mList.Clear()
      mIsPopulated = False
    End Sub

    Public Sub Save()
      'Verify that the root folders as well as the _source and _thumb folders exist
      'in both the physical file system and the DNN Folders table and create if necessary
      Dim _FolderID As Integer = CreateRootFolders(URL, True)
      If _FolderID <> -1 Then
        mID = _FolderID
        Dim metaData As New GalleryXML(Parent.Path)
        GalleryXML.SaveMetaData(Parent.Path, Name, ID, _
                    Title, Description, Categories, _
                    Author, Location, Client, OwnerID, _
                    CreatedDate, ApprovedDate, 0)

        If Not Parent Is Nothing Then
          Parent.IsPopulated = False
          Utils.PopulateAllFolders(Parent, False)
        Else
          Me.IsPopulated = False
          Utils.PopulateAllFolders(Me, False)
        End If
      End If

    End Sub

    ' GAL-6255
    Public Function ValidateGalleryName(ByVal folderName As String) As Boolean
      ' WES refactored 2-20-09 to use regex with tighter pattern matching re GAL-9402
      ' WES modified 3-20-2011 to allow unicode letters as first character
      Return Regex.IsMatch(folderName, "[^\000-\037\\/:*?#!""><|&%_][^\000-\037\\/:*?#!""><|&%]*")
    End Function

    'William Severance - modified to interface with DNN file system
    Public Function CreateChild( _
        ByVal ChildName As String, _
        ByVal ChildTitle As String, _
        ByVal ChildDescription As String, _
        ByVal Author As String, _
        ByVal Location As String, _
        ByVal Client As String, _
        ByVal ChildCategories As String, _
        ByVal ChildOwnerId As Integer, _
        ByVal ApprovedDate As DateTime) As Integer

      Dim newFolderPath As String = BuildPath(New String(1) {mPath, ChildName}, "\", False, True)
      Dim newFolderID As Integer = -2
      If Not Directory.Exists(newFolderPath) Then
        Dim newFolderURL As String = BuildPath(New String(1) {mURL, ChildName & "/"}, "/", False, False)
        newFolderID = CreateRootFolders(newFolderURL, True)
        If newFolderID >= 0 Then
          Dim metaData As New GalleryXML(Me.Path)
          GalleryXML.SaveMetaData(Path, ChildName, newFolderID, _
                  ChildTitle, ChildDescription, ChildCategories, _
                  Author, Location, Client, ChildOwnerId, _
                  DateTime.Now, ApprovedDate, 0)
          Config.ResetGalleryFolder(Me)
        End If
      End If
      Return newFolderID
    End Function

    'William Severance - modified to interface with DNN file system
    Public Function DeleteChild(ByVal Child As IGalleryObjectInfo) As String
      Dim strInfo As String = ""
      Dim ps As PortalSettings = PortalController.GetCurrentPortalSettings()
      Dim auth As New Gallery.Authorization(GalleryConfig.ModuleID)
      If auth.HasItemEditPermission(Child) Then
        Try
          If Child.IsFolder Then
            Dim di As System.IO.DirectoryInfo = New DirectoryInfo(Child.Path)
            If di.Exists Then
              DeleteFolder(ps.PortalId, di, FileSystemUtils.FormatFolderPath(Child.URL.Replace(ps.HomeDirectory, "")), True)
            End If
          Else
            If File.Exists(Child.Path) Then DeleteFile(Child.Path, ps, True)
            If Child.Type = Config.ItemType.Image Then
              Dim childThumbPath As String = IO.Path.Combine(Me.ThumbFolderPath, Child.Name)
              If File.Exists(childThumbPath) Then DeleteFile(childThumbPath, ps, True)
              Dim childSourcePath As String = IO.Path.Combine(Me.SourceFolderPath, Child.Name)
              If File.Exists(childSourcePath) Then DeleteFile(childSourcePath, ps, True)

              'William Severance - added to reset folder thumbnail to "folder.gif" if its current
              'thumbnail is being deleted. Will then get set to next image thumbnail during populate if there is one.
              If Me.Thumbnail = Child.Name Then ResetThumbnail()
            End If
          End If

          Dim metaData As New GalleryXML(mPath)
          GalleryXML.DeleteMetaData(mPath, Child.Name)

        Catch exc As System.Exception
          strInfo = exc.Message
        End Try

        Config.ResetGalleryFolder(Me) 're-populate the album object
        OnDeleted(New GalleryObjectEventArgs(Child, Users.UserController.GetCurrentUserInfo().UserID))
      Else
        strInfo = Localization.GetString("Insufficient_Delete_Permissions", GalleryConfig.SharedResourceFile)
      End If
      Return strInfo

    End Function

    'William Severance - added to provide deletion of both the physical file and the record from the DNN files table.
    'Note: This is a modified version of the DNN FileSystemUtils.DeleteFile method which since DNN 4.8.3 does check of write permission
    'on the folder before proceeding to delete the file.

    Public Function DeleteFile(ByVal strSourceFile As String, ByVal settings As PortalSettings, ByVal ClearCache As Boolean) As String

      Dim retValue As String = ""
      Try
        Dim folderName As String = Common.Globals.GetSubFolderPath(strSourceFile, settings.PortalId)
        Dim fileName As String = GetFileName(strSourceFile)
        Dim PortalId As Integer = FileSystemUtils.GetFolderPortalId(settings)

        'try and delete the Insecure file
        DeleteFile(strSourceFile)

        'try and delete the Secure file
        DeleteFile(strSourceFile & glbProtectedExtension)

        'Remove file from DataBase
        Dim objFileController As New FileSystem.FileController
        Dim objFolders As New FileSystem.FolderController
        Dim objFolder As FileSystem.FolderInfo = objFolders.GetFolder(PortalId, folderName, False)
        objFileController.DeleteFile(PortalId, fileName, objFolder.FolderID, ClearCache)

      Catch ex As Exception
        retValue = ex.Message
      End Try
      Return retValue
    End Function

    'William Severance - added to provide deletion of both the physical folder and the record from the DNN folders table.
    'Note: This is a modified version of the DNN FileSystemUtils.DeleteFolder method which since DNN 4.8.4 does check of write permission
    'on the folder before proceeding to delete the folder and also does not handle recursive deletion of contained files and folders from the
    'DNN files and folders tables respectively.
    Private Function DeleteFolder(ByVal PortalId As Integer, ByVal folder As System.IO.DirectoryInfo, ByVal folderName As String, ByVal Recursive As Boolean) As String
      Dim retValue As String = ""
      Dim ps As PortalSettings = PortalController.GetCurrentPortalSettings()
      Dim files() As System.IO.FileInfo = folder.GetFiles()
      Dim directories() As System.IO.DirectoryInfo = folder.GetDirectories()

      If Recursive OrElse (directories.Length = 0 And files.Length = 0) Then
        Try
          'Delete files in the folder
          For Each fi As System.IO.FileInfo In files
            DeleteFile(fi.FullName, ps, False)
          Next
          For Each di As System.IO.DirectoryInfo In directories
            DeleteFolder(PortalId, di, FileSystemUtils.FormatFolderPath(di.FullName.Replace(ps.HomeDirectoryMapPath, "").Replace("\", "/")), Recursive)
          Next
          folder.Delete(False)
          'Remove Folderfrom DataBase
          Dim objFolderController As New FileSystem.FolderController
          objFolderController.DeleteFolder(PortalId, folderName)

        Catch ex As Exception
          retValue = ex.Message
        End Try
      Else
        retValue = Localization.GetString("Folder_Contains_Objects", GalleryConfig.SharedResourceFile)
      End If
      Return retValue
    End Function

    Private Shared Function GetFileName(ByVal filePath As String) As String
      Return System.IO.Path.GetFileName(filePath).Replace(glbProtectedExtension, "")
    End Function

    Private Shared Sub DeleteFile(ByVal strFileName As String)
      If File.Exists(strFileName) Then
        File.SetAttributes(strFileName, FileAttributes.Normal)
        File.Delete(strFileName)
      End If
    End Sub

    Public Function DeleteChild(ByVal ChildName As String) As String
      Dim child As IGalleryObjectInfo = CType(List.Item(ChildName), IGalleryObjectInfo)
      Return DeleteChild(child)
    End Function

    Public Sub ChangeThumbnail(ByVal ThumbName As String)
      mThumbnail = ThumbName
      mThumbnailURL = BuildPath(New String(1) {mThumbFolderURL, ThumbName}, "/", False, True)
    End Sub

    'William Severance - added to provide of reset of album thumbnail to "folder.gif" when referenced
    'thumbnail has been deleted or is otherwise missing.
    Public Sub ResetThumbnail()
      mThumbnail = "folder.gif"
      mThumbnailURL = mGalleryConfig.GetImageURL(mThumbnail)
    End Sub

    Private Sub ClearList()
      Me.List.Clear()
      Me.SortList.Clear()
      Me.UnApprovedItems.Clear()
      Me.MediaItems.Clear()
      Me.mBrowsableItems.Clear()
      Me.FlashItems.Clear()
      Me.IconItems.Clear()
    End Sub

    ' Populates the current album data from file system, metadata, and DNN Folders/Files Table
    Public Sub Populate(ByVal ReSync As Boolean)
      Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
      Dim _PortalId As Integer = _portalSettings.PortalId

      ' Populates the folder object with information about subs and files
      Dim gItems() As String
      Dim gCounter As Integer
      Dim gUpdateXML As Boolean

      Dim gHierarchy As String = ""
      Dim gName As String = ""
      Dim gNameEscaped As String = ""

      Dim gURL As String = ""
      Dim gPath As String = ""
      Dim gSize As Long

      ' Thumbnail to be displayed in gallery container
      Dim gThumbnail As String = ""
      Dim gThumbnailURL As String = ""
      Dim gThumbnailPath As String = ""

      ' Thumb folder of galleryFolder
      Dim gThumbFolderPath As String = ""
      Dim gThumbFolderURL As String = ""

      ' Icon to be displayed in grid
      Dim gIcon As String = ""
      'Dim gIconPath As String = ""
      Dim gIconURL As String = ""

      ' Source for album up/downloading
      Dim gSourceFolderPath As String = "" 'Set this default value for file first
      Dim gSourceFolderURL As String = ""

      ' SourceURL for file to be downloaded
      Dim gSourceURL As String = ""
      Dim gSourcePath As String = ""

      ' Info stored in XML
      Dim gID As Integer
      Dim gTitle As String = ""
      Dim gDescription As String = ""
      Dim gCategories As String = ""
      Dim gAuthor As String = ""
      Dim gClient As String = ""
      Dim gLocation As String = ""
      Dim gCreatedDate As DateTime = Null.NullDate
      Dim gApprovedDate As DateTime = Null.NullDate
      Dim gWidth As Integer = 0
      Dim gHeight As Integer = 0
      Dim gScore As Double

      Dim gScoreImage As String
      Dim gScoreImageURL As String

      Dim gAlbumOwnerID As Integer
      Dim gFileOwnerID As Integer

      Dim gFileType As Config.ItemType
      Dim gValidFile As Boolean

      'William Severance - Interface with DNN folder/file tables
      Dim _objFileController As New FileSystem.FileController
      Dim _objFolderController As New FileSystem.FolderController
      Dim _FolderID As Integer = -1

      ' (in case someone decides to call this again without clearing the data first)
      Me.ClearList()

      ' Check existence of thumbs/source folder
      Try
        'Create the album's root folders/synch with folders table if not already existing
        _FolderID = CreateRootFolders(URL, True)
        If Parent Is Nothing Then
          mID = _FolderID 'Top level folder
        End If

      Catch ex As Exception
        LogException(ex)
        Throw
      End Try

      ' Check for metadata for this folder
      Dim metaData As New GalleryXML(Me.Path)

      ' Add sub-folders here.
      gItems = Directory.GetDirectories(mPath)

      For Each gItem As String In gItems
        'Reset values
        gUpdateXML = False
        gID = -1

        ' Check to make sure this folder is valid galleryfolder
        gName = IO.Path.GetFileName(gItem)

        If Not (gName.StartsWith("_") OrElse _
                 (gName = GalleryConfig.ThumbFolder) _
                    OrElse (gName = GalleryConfig.SourceFolder)) Then
          gHierarchy = BuildPath(New String(1) {mGalleryHierarchy, gName}, "/")
          '<tam:note always keep virtual url format - without http://>
          gURL = BuildPath(New String(1) {mURL, gName}, "/", False, False)
          gPath = IO.Path.Combine(mPath, gName)
          gThumbnail = "folder.gif"
          gThumbnailURL = mGalleryConfig.GetImageURL(gThumbnail)
          gThumbFolderPath = IO.Path.Combine(gPath, mGalleryConfig.ThumbFolder)
          gThumbFolderURL = BuildPath(New String(2) {mURL, gName, mGalleryConfig.ThumbFolder}, "/", False, False)
          gIcon = "s_folder.gif"
          gIconURL = mGalleryConfig.GetImageURL(gIcon)
          gSourceFolderPath = IO.Path.Combine(gPath, GalleryConfig.SourceFolder)
          gSourceFolderURL = BuildPath(New String(2) {mURL, gName, mGalleryConfig.SourceFolder}, "/", False, False)
          gID = metaData.ID(gName)
          gDescription = metaData.Description(gName).Replace(vbCrLf, "<br />")
          gCategories = metaData.Categories(gName)
          gAuthor = metaData.Author(gName)
          gClient = metaData.Client(gName)
          gLocation = metaData.Location(gName)
          gAlbumOwnerID = metaData.OwnerID(gName)
          gCreatedDate = metaData.CreatedDate(gName)
          gApprovedDate = metaData.ApprovedDate(gName)

          ' Does the metadata contain as its ID that of the corresponding DNN Folders table item?
          Dim gFolderID As Integer = CreateRootFolders(gURL, False)
          If gID <> gFolderID Then
            gID = gFolderID
            gUpdateXML = True
          End If

          gTitle = metaData.Title(gName)
          If (gTitle = "") OrElse (gTitle.ToLower = "untitled") Then
            gTitle = gName
            gUpdateXML = True
          End If

          If gCreatedDate = Null.NullDate Then
            gCreatedDate = IO.Directory.GetCreationTime(gPath)
            gUpdateXML = True
          End If

          If gAlbumOwnerID <= 0 Then
            gAlbumOwnerID = DotNetNuke.Modules.Gallery.Config.DefaultOwnerId
            gUpdateXML = True
          End If

          If gAlbumOwnerID <> Utils.ValidUserID(gAlbumOwnerID) Then
            gAlbumOwnerID = _portalSettings.AdministratorId
            gUpdateXML = True
          End If

          If GalleryConfig.AutoApproval AndAlso gApprovedDate > DateTime.Now Then
            gApprovedDate = DateTime.Now
            gUpdateXML = True
          End If

          ' Update xmldata if there were any changes
          If gUpdateXML Then
            GalleryXML.SaveMetaData(Me.Path, gName, gID, _
                    gTitle, gDescription, gCategories, _
                    gAuthor, gLocation, gClient, gAlbumOwnerID, _
                    gCreatedDate, gApprovedDate, 0)
          End If

          ' If it's a valid galleryfolder add it to collection
          Dim newFolder As GalleryFolder
          newFolder = New GalleryFolder(gCounter, gID, Me, gHierarchy, gName, gURL, gPath, gThumbnail, gThumbnailURL, gThumbFolderPath, gThumbFolderURL, gIcon, gIconURL, gSourceFolderPath, gSourceFolderURL, gTitle, gDescription, gCategories, gAuthor, gClient, gLocation, gAlbumOwnerID, gApprovedDate, gCreatedDate, GalleryConfig)

          If (newFolder.ApprovedDate <= DateTime.Now OrElse GalleryConfig.AutoApproval) Then
            mSortList.Add(newFolder)
          Else
            mUnApprovedItems.Add(gCounter)
          End If

          mList.Add(gName, newFolder)
          gCounter += 1
        End If
      Next

      ' Add files here
      gItems = System.IO.Directory.GetFiles(mPath)

      '<tamttt:note since 2.0 file will be full url>
      For Each gItem As String In gItems
        ' Reset all values
        gApprovedDate = Null.NullDate
        gCreatedDate = Null.NullDate
        gID = -1
        gUpdateXML = False

        Dim gExt As String = IO.Path.GetExtension(gItem).ToLower 'New IO.FileInfo(gItem).Extension

        ' Check to make sure this file is valid gallery file & also approved
        gName = IO.Path.GetFileName(gItem)
        gNameEscaped = HttpUtility.UrlPathEncode(gName.Replace("%", "%25")).Replace(",", "%2C")

        If (gItem <> "_metadata.resources") AndAlso mGalleryConfig.IsValidMediaType(gExt) Then
          gURL = BuildPath(New String(1) {mURL, gNameEscaped}, "/", False, True)
          gPath = IO.Path.Combine(mPath, gName)
          gSize = New IO.FileInfo(gItem).Length
          gID = metaData.ID(gName)
          gTitle = metaData.Title(gName)
          gDescription = metaData.Description(gName).Replace(vbCrLf, "<br/>")
          gCategories = metaData.Categories(gName)
          gAuthor = metaData.Author(gName)
          gClient = metaData.Client(gName)
          gLocation = metaData.Location(gName)
          gFileOwnerID = metaData.OwnerID(gName)
          gCreatedDate = metaData.CreatedDate(gName)
          gApprovedDate = metaData.ApprovedDate(gName)
          gScore = metaData.Score(gName)

          Dim intScore As Integer = (CInt(Math.Floor(gScore)) + CInt(Math.Ceiling(gScore))) * 5
          gScoreImage = "stars_" & intScore.ToString & ".gif"
          gScoreImageURL = mGalleryConfig.GetImageURL(gScoreImage)

          ' If not title for file exists, create one
          If (gTitle = "") OrElse (gTitle.ToLower = "untitled") Then
            ' Use filename before the LAST "."
            gTitle = Left(gName, gName.LastIndexOf("."))
            gUpdateXML = True
          End If

          ' Update created date if we don't have a date
          If gCreatedDate = Null.NullDate Then
            gCreatedDate = IO.File.GetCreationTime(gPath)
            gUpdateXML = True
          End If

          ' Does the file have an owner?
          If gFileOwnerID <= 0 Then
            ' Assign parent owner to be file Owner
            gFileOwnerID = Me.OwnerID
            gUpdateXML = True
          End If

          ' Host account will not be display in users list, should be changed to Admin
          If gFileOwnerID <> Utils.ValidUserID(gFileOwnerID) Then
            gFileOwnerID = _portalSettings.AdministratorId
            gUpdateXML = True
          End If

          ' Comment out 8/30 - The Approved Date can be ANY date and can
          ' be reset to some date greater than today. This allows the User
          ' to control which images are visible, based on calendar
          ' HZassenhaus Issue GAL 8189

          'If GalleryConfig.AutoApproval AndAlso gApprovedDate > DateTime.Now Then
          '   gApprovedDate = DateTime.Now
          '   gUpdateXML = True
          'End If

          ' Reset file validation value
          gValidFile = False
          gWidth = 0
          gHeight = 0

          Dim gFileInfo As FileSystem.FileInfo = _objFileController.GetFileById(gID, _PortalId)
          If gFileInfo Is Nothing Then
            gID = -1 'Invalidate the ID
            gWidth = 0
            gHeight = 0
          Else
            If gFileInfo.FolderId = Me.mID AndAlso gFileInfo.FileName = gName Then
              gWidth = gFileInfo.Width
              gHeight = gFileInfo.Height
            End If
          End If

          ' Remember that we should add image first
          Select Case mGalleryConfig.TypeFromExtension(gExt)
            Case ItemType.Image
              If (gTitle.ToLower <> "icon") _
                    AndAlso (gTitle.ToLower <> "watermark") _
                    AndAlso (gApprovedDate <= DateTime.Now OrElse GalleryConfig.AutoApproval) Then
                mBrowsableItems.Add(gCounter) ' store reference to index
              End If

              gFileType = Config.ItemType.Image
              gValidFile = True

              gIcon = "s_jpg.gif"
              gIconURL = mGalleryConfig.GetImageURL(gIcon)

              ' Build source path
              gSourcePath = IO.Path.Combine(Me.SourceFolderPath, gName)
              gSourceURL = BuildPath(New String(1) {Me.SourceFolderURL, gNameEscaped}, "/", False, True)

              'Add to DNN Files Table
              If gID = -1 Or ReSync Then
                ' Copy the original file to source folder
                If mGalleryConfig.IsKeepSource Then
                  If Not File.Exists(gSourcePath) Then File.Copy(gPath, gSourcePath)
                  Utils.SaveDNNFile(gSourcePath, 0, 0, False, False) 'Add to the DNN files table if we're not deleting the source
                End If
                If mGalleryConfig.IsFixedSize Then
                  gID = ResizeImage(gPath, gPath, mGalleryConfig.FixedWidth, mGalleryConfig.FixedHeight, mGalleryConfig.EncoderQuality)
                Else
                  gID = SaveDNNFile(gPath, gWidth, gHeight, False, False)
                End If
                'Update the width and height - may have changed
                If gID >= 0 Then
                  gFileInfo = _objFileController.GetFileById(gID, _PortalId)
                  gWidth = gFileInfo.Width
                  gHeight = gFileInfo.Height
                End If
                gUpdateXML = True
              End If

              ' Set this value for file download if user upload directly by FTP into display folder
              ' Note that we change url only, source path keep to upload or Maintenance feature uses
              If Not File.Exists(gSourcePath) Then
                gSourceURL = gURL
              End If

              gThumbnail = gName
              gThumbnailURL = BuildPath(New String(1) {Me.ThumbFolderURL, gNameEscaped}, "/", False, True)

              gThumbnailPath = BuildPath(New String(1) {Me.ThumbFolderPath, gThumbnail}, "\", False, True)
              Try ' Build the thumb on the fly if not exists
                If Not File.Exists(gThumbnailPath) Then
                  ' Only do resize with valid image, to prevent out of memmory exception
                  If gSize > 0 Then
                    ResizeImage(gItem, gThumbnailPath, mGalleryConfig.MaximumThumbWidth, mGalleryConfig.MaximumThumbHeight, mGalleryConfig.EncoderQuality)
                  Else
                    ' Mark as invalid and delete invalid images
                    gValidFile = False
                    FileSystemUtils.DeleteFile(gPath, _portalSettings)
                    ' Delete source
                    If IO.File.Exists(gSourcePath) Then
                      FileSystemUtils.DeleteFile(gSourcePath, _portalSettings)
                    End If

                    'William Severance - Remove folder thumbnail reference to thumbnail being deleted
                    If mThumbnail = gThumbnail Then ResetThumbnail()

                    ' Also delete metadata related to this file
                    GalleryXML.DeleteMetaData(mPath, gName)
                  End If
                Else
                  'Add/Update thumbnail file to DNN Files table
                  If ReSync Then Utils.SaveDNNFile(gThumbnailPath, 0, 0, False, False)
                End If
              Catch ex As Exception
                ' Handle thumbnail exception 
                LogException(ex)
                gThumbnailURL = gIconURL
              End Try

              ' Assign first image of the album to be album icon if not specified
              If gValidFile AndAlso (gApprovedDate <= DateTime.Now OrElse GalleryConfig.AutoApproval) AndAlso mThumbnail = "folder.gif" Then
                mThumbnail = gThumbnail
                mThumbnailURL = gThumbnailURL
              End If

            Case ItemType.Movie
              gIcon = "s_MediaPlayer.gif"
              gIconURL = mGalleryConfig.GetImageURL(gIcon)

              gThumbnail = "MediaPlayer.gif"
              gThumbnailURL = mGalleryConfig.GetImageURL(gThumbnail)

              gSourcePath = IO.Path.Combine(Me.Path, gName)
              gSourceURL = BuildPath(New String(1) {Me.URL, gNameEscaped}, "/", False, True)

              ' Add/Update the source file to the DNN Files Table
              If gID = -1 OrElse ReSync Then
                gWidth = -1
                gHeight = -1
                gID = Utils.SaveDNNFile(gSourcePath, gWidth, gHeight, False, False)
                gUpdateXML = True
              End If

              ' Changing handle method for media player file since 2.0
              mMediaItems.Add(gCounter)

              gFileType = Config.ItemType.Movie
              gValidFile = True

              ' add flash types.
            Case ItemType.Flash
              gIcon = "s_Flash.gif"
              gIconURL = mGalleryConfig.GetImageURL(gIcon)

              gThumbnail = "Flash.gif"
              gThumbnailURL = mGalleryConfig.GetImageURL(gThumbnail)

              gSourcePath = IO.Path.Combine(Me.Path, gName)
              gSourceURL = BuildPath(New String(1) {Me.URL, gNameEscaped}, "/", False, True)

              ' Add/Update the source file to the DNN Files Table
              If gID = -1 OrElse ReSync Then
                gWidth = -1
                gHeight = -1
                gID = Utils.SaveDNNFile(gSourcePath, gWidth, gHeight, False, False)
                gUpdateXML = True
              End If

              ' Changing handle method for media player file since 2.0
              mFlashItems.Add(gCounter)

              gFileType = Config.ItemType.Flash
              gValidFile = True
          End Select

          If gValidFile Then
            ' Does the XML file need updating?
            If gUpdateXML Then
              ' Update it
              GalleryXML.SaveMetaData(Me.Path, gName, gID, _
                      gTitle, gDescription, gCategories, _
                      gAuthor, gLocation, gClient, gFileOwnerID, _
                      gCreatedDate, gApprovedDate, gScore)
            End If

            ' Add the file and increment the counter
            Dim newFile As GalleryFile
            newFile = New GalleryFile(gCounter, gID, Me, gName, gURL, gPath, gThumbnail, gThumbnailURL, gThumbnailPath, gIcon, gIconURL, gScoreImage, gScoreImageURL, gSourcePath, gSourceURL, gSize, gFileType, gTitle, gDescription, gCategories, gAuthor, gClient, gLocation, gFileOwnerID, gApprovedDate, gCreatedDate, gWidth, gHeight, gScore, mGalleryConfig)
            mList.Add(gName, newFile)

            ' Set icon image & watermark for parent folder
            Dim gTitleLowered As String = gTitle.ToLower
            If gTitleLowered.IndexOf("icon") > -1 Then
              mThumbnail = gName
              mThumbnailURL = gThumbnailURL
              mIconItems.Add(newFile)
            ElseIf gTitleLowered = "watermark" Then
              mWatermarkImage = newFile
              mIconItems.Add(newFile)
            Else
              If (newFile.ApprovedDate <= DateTime.Now OrElse GalleryConfig.AutoApproval) Then
                mSortList.Add(newFile)
              Else
                mUnApprovedItems.Add(gCounter)
              End If
            End If
            gCounter += 1
          End If
        End If
      Next

      ' Get global watermark from parent album if it's not found in this album
      If mGalleryConfig.UseWatermark _
          AndAlso WatermarkImage Is Nothing _
          AndAlso Not Parent Is Nothing Then
        If Not Parent.WatermarkImage Is Nothing Then mWatermarkImage = Parent.WatermarkImage
      End If

      If mFlashItems.Count > 0 AndAlso mIconItems.Count > 0 Then
        PopulateFlashIcon()
      End If
      If mMediaItems.Count > 0 AndAlso mIconItems.Count > 0 Then
        PopulateMediaIcon()
      End If

      mItemInfo = GetItemInfo()

      ' Sort the list by default settings first
      mSortList.Sort(New Comparer(New String(0) {[Enum].GetName(GetType(Config.GallerySort), GalleryConfig.DefaultSort)}, GalleryConfig.DefaultSortDESC))
      ' Set the flag so we don't call again
      mIsPopulated = True
    End Sub

    Private Sub PopulateFlashIcon()

      For Each intCounter As Integer In mFlashItems
        Dim flashFile As GalleryFile
        flashFile = CType(mList.Item(intCounter), GalleryFile)

        For Each iconFile As GalleryFile In mIconItems
          If iconFile.Title.ToLower = flashFile.Title.ToLower & "icon" Then
            flashFile.Thumbnail = iconFile.Name
            flashFile.ThumbnailURL = iconFile.URL
          End If
        Next
      Next
    End Sub

    Private Sub PopulateMediaIcon()

      For Each intCounter As Integer In mMediaItems
        Dim mediaFile As GalleryFile
        mediaFile = CType(mList.Item(intCounter), GalleryFile)

        For Each iconFile As GalleryFile In mIconItems
          If iconFile.Title.ToLower = mediaFile.Title.ToLower & "icon" Then
            mediaFile.Thumbnail = iconFile.Name
            mediaFile.ThumbnailURL = iconFile.URL
          End If
        Next
      Next
    End Sub

    Private Function GetBrowserURL() As String
      Dim params As New Generic.List(Of String)
      If Not String.IsNullOrEmpty(GalleryHierarchy) Then params.Add("path=" & FriendlyURLEncode(GalleryHierarchy))
      Return NavigateURL(GalleryConfig.GalleryTabID, "", params.ToArray())
    End Function

    Private Function GetSlideshowURL() As String
      Dim params As New Generic.List(Of String)
      params.Add("mid=" & GalleryConfig.ModuleID.ToString)
      If Not String.IsNullOrEmpty(GalleryHierarchy) Then params.Add("path=" & FriendlyURLEncode(GalleryHierarchy))
      Return NavigateURL(GalleryConfig.GalleryTabID, "Slideshow", params.ToArray())
    End Function

    Private Function GetPopupSlideshowURL() As String
      If mPopupSlideshowURL = "" Then
        Dim sb As New StringBuilder

        sb.Append(GalleryConfig.SourceDirectory)
        sb.Append("/GalleryPage.aspx?ctl=SlideShow")
        sb.Append("&tabid=")
        sb.Append(GalleryConfig.GalleryTabID.ToString)
        sb.Append("&mid=")
        sb.Append(GalleryConfig.ModuleID.ToString)
        If Not String.IsNullOrEmpty(GalleryHierarchy) Then
          sb.Append("&path=")
          sb.Append(FriendlyURLEncode(GalleryHierarchy))
        End If
        mPopupSlideshowURL = BuildPopup(sb.ToString, mGalleryConfig.FixedHeight + 160, mGalleryConfig.FixedWidth + 120)
      End If
      Return mPopupSlideshowURL
    End Function

    Private Function GetEditURL() As String
      Dim params As New Generic.List(Of String)
      params.Add("mid=" & GalleryConfig.ModuleID.ToString)
      If Not String.IsNullOrEmpty(GalleryHierarchy) Then params.Add("path=" & FriendlyURLEncode(GalleryHierarchy))
      Return NavigateURL(GalleryConfig.GalleryTabID, "AlbumEdit", params.ToArray())
    End Function

    Private Function GetMaintenanceURL() As String
      Dim params As New Generic.List(Of String)
      params.Add("mid=" & GalleryConfig.ModuleID.ToString)
      If Not String.IsNullOrEmpty(GalleryHierarchy) Then params.Add("path=" & FriendlyURLEncode(GalleryHierarchy))
      Return NavigateURL(GalleryConfig.GalleryTabID, "Maintenance", params.ToArray())
    End Function

    Private Function GetAddSubAlbumURL() As String
      Dim params As New Generic.List(Of String)
      params.Add("mid=" & GalleryConfig.ModuleID.ToString)
      If Not String.IsNullOrEmpty(GalleryHierarchy) Then params.Add("path=" & FriendlyURLEncode(GalleryHierarchy))
      params.Add("action=addfolder")
      Return NavigateURL(GalleryConfig.GalleryTabID, "AlbumEdit", params.ToArray())
    End Function

    Private Function GetAddFileURL() As String
      Dim params As New Generic.List(Of String)
      params.Add("mid=" & GalleryConfig.ModuleID.ToString)
      If Not String.IsNullOrEmpty(GalleryHierarchy) Then params.Add("path=" & FriendlyURLEncode(GalleryHierarchy))
      params.Add("action=addfile")
      Return NavigateURL(GalleryConfig.GalleryTabID, "AlbumEdit", params.ToArray())
    End Function

    Protected Function GetItemInfo() As String
      Dim sb As New StringBuilder

      ' William Severance (9-10-2010) - Modified to use bitmapped TextDisplayOptions flags rather than iterating through string array for each test.

      If (GalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Name) <> 0 Then
        sb.Append("<span class=""MediaName"">")
        sb.Append(Name)
        sb.Append("</span>")

        If (GalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Size) <> 0 Then
          sb.Append("<span class=""MediaSize"">")
          Dim sizeInfo As String = " " & Localization.GetString("AlbumSizeInfo", GalleryConfig.SharedResourceFile)
          sizeInfo = sizeInfo.Replace("[ItemCount]", (Size - (IconItems.Count + UnApprovedItems.Count)).ToString)
          sb.Append(sizeInfo)
          sb.Append("</span>")
        End If
        sb.Append("<br />")
      End If

      If ((GalleryConfig.TextDisplayOptions And GalleryDisplayOption.Client) <> 0) _
            AndAlso Not Client.Length = 0 Then
        AppendLegendAndField(sb, "Client", Client)
      End If

      If ((GalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Location) <> 0) _
            AndAlso Not Location.Length = 0 Then
        AppendLegendAndField(sb, "Location", Location)
      End If

      If (GalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.CreatedDate) <> 0 Then
        AppendLegendAndField(sb, "CreatedDate", Utils.DateToText(CreatedDate))
      End If

      If (GalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.ApprovedDate) <> 0 Then
        AppendLegendAndField(sb, "ApprovedDate", Utils.DateToText(ApprovedDate))
      End If

      Return sb.ToString()

    End Function

    Private Sub AppendLegendAndField(ByVal sb As StringBuilder, ByVal LegendKey As String, ByVal FieldValue As String)
      sb.Append("<span class=""Legend"">")
      sb.Append(Localization.GetString(LegendKey, GalleryConfig.SharedResourceFile))
      sb.Append("</span>&nbsp;<span class=""Field"">")
      sb.Append(FieldValue)
      sb.Append("</span>")
      sb.Append("<br/>")
    End Sub

    'William Severance - added to raise events upon deletion/creation of gallery objects

    Protected Sub OnDeleted(ByVal e As GalleryObjectEventArgs)
      RaiseEvent GalleryObjectDeleted(Me, e)
    End Sub

    Protected Sub OnCreated(ByVal e As GalleryObjectEventArgs)
      RaiseEvent GalleryObjectCreated(Me, e)
    End Sub

#End Region

#Region "Public Shared Methods"
    ' Validates existance of and if necessary sets up the root folders of a top level gallery or child album
    ' rootURL should be a site relative not physical path.
    ' If addSourceAndThumb is true, the _source and _thumb subfolders will also be added

    ' Returns the FolderId of the root album folder.

    Public Overloads Shared Function CreateRootFolders(ByVal albumURL As String, ByVal addSourceAndThumb As Boolean) As Integer
      Dim folderID As Integer = -1
      Try
        Dim ps As PortalSettings = PortalController.GetCurrentPortalSettings()
        Dim rootPath As String = HttpContext.Current.Server.MapPath(albumURL)
        Dim homeDirectoryRelativeURL As String = FileSystemUtils.FormatFolderPath(albumURL.Substring(ps.HomeDirectory.Length))
        Dim fldController As New DotNetNuke.Services.FileSystem.FolderController
        Dim folder As DotNetNuke.Services.FileSystem.FolderInfo = fldController.GetFolder(ps.PortalId, homeDirectoryRelativeURL, False)
        If Not System.IO.Directory.Exists(rootPath) OrElse folder Is Nothing Then
          Dim parentPath As String = ps.HomeDirectoryMapPath
          FileSystemUtils.AddFolder(ps, parentPath, homeDirectoryRelativeURL)
          folderID = fldController.GetFolder(ps.PortalId, homeDirectoryRelativeURL, True).FolderID
        Else
          folderID = folder.FolderID
        End If
        If addSourceAndThumb Then
          Dim thumbPath As String = FileSystemUtils.FormatFolderPath(Config.DefaultThumbFolder)
          Dim sourcePath As String = FileSystemUtils.FormatFolderPath(Config.DefaultSourceFolder)
          If fldController.GetFolder(ps.PortalId, homeDirectoryRelativeURL & thumbPath, True) Is Nothing Then
            FileSystemUtils.AddFolder(ps, rootPath, thumbPath)
          End If
          If fldController.GetFolder(ps.PortalId, homeDirectoryRelativeURL & sourcePath, True) Is Nothing Then
            FileSystemUtils.AddFolder(ps, rootPath, sourcePath)
          End If
        End If
      Catch ex As Exception
        LogException(ex)
      End Try
      Return folderID
    End Function

    Public Overloads Shared Function CreateRootFolders(ByVal albumURL As String) As Integer
      Return CreateRootFolders(albumURL, False)
    End Function
#End Region
  End Class

End Namespace