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

Namespace DotNetNuke.Modules.Gallery

  Public Class FileToUpload

    Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings

    Private mHtmlInput As HtmlControls.HtmlInputFile
    Private mContentLength As Integer
    Private mModuleID As Integer = -1
    Private mFileName As String
    Private mExtension As String
    Private mTitle As String = Null.NullString
    Private mDescription As String = Null.NullString
    Private mCategories As String = Null.NullString
    Private mAuthor As String = Null.NullString
    Private mClient As String = Null.NullString
    Private mLocation As String = Null.NullString
    Private mOwner As String = Null.NullString
    Private mOwnerID As Integer = -1
    Private mType As Config.ItemType = Config.ItemType.Image
    Private mIcon As String = Null.NullString
    Private mZipHeaders As ZipHeaderReader = Nothing     'Only for zip files else Null
    Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config

#Region "Public Properties"
    Public Property ModuleID() As Integer
      Get
        Return mModuleID
      End Get
      Set(ByVal Value As Integer)
        mModuleID = Value
      End Set
    End Property

    Public ReadOnly Property FileName() As String
      Get
        Return mFileName
      End Get
    End Property

    Public ReadOnly Property Extension() As String
      Get
        Return mExtension
      End Get
    End Property

    Public ReadOnly Property ContentType() As String
      Get
        Return mHtmlInput.PostedFile.ContentType
      End Get
    End Property

    Public ReadOnly Property ContentLength() As Integer
      Get
        Return mContentLength
      End Get
    End Property

    Public Property Title() As String
      Get
        Return mTitle
      End Get
      Set(ByVal Value As String)
        mTitle = Value
      End Set
    End Property

    Public Property Description() As String
      Get
        Return mDescription
      End Get
      Set(ByVal Value As String)
        mDescription = Value
      End Set
    End Property

    Public Property Author() As String
      Get
        Return mAuthor
      End Get
      Set(ByVal Value As String)
        mAuthor = Value
      End Set
    End Property

    Public Property Client() As String
      Get
        Return mClient
      End Get
      Set(ByVal Value As String)
        mClient = Value
      End Set
    End Property

    Public Property Location() As String
      Get
        Return mLocation
      End Get
      Set(ByVal Value As String)
        mLocation = Value
      End Set
    End Property

    Public Property Categories() As String
      Get
        Return mCategories
      End Get
      Set(ByVal Value As String)
        mCategories = Value
      End Set
    End Property

    Public Property OwnerID() As Integer
      Get
        Return mOwnerID
      End Get
      Set(ByVal Value As Integer)
        mOwnerID = Value
      End Set
    End Property

    Public ReadOnly Property Owner() As String
      Get
        Dim uc As New Entities.Users.UserController
        Dim user As UserInfo = uc.GetUser(_portalSettings.PortalId, Utils.ValidUserID(mOwnerID))
        If user Is Nothing Then
          mOwner = Null.NullString
        Else
          mOwner = user.Username
        End If
        Return mOwner
      End Get
    End Property

    Public ReadOnly Property ApprovedDate() As DateTime
      Get
        If mGalleryConfig.AutoApproval Or New Authorization(ModuleID).HasAdminPermission Then
          Return DateTime.Now
        Else
          Return DateTime.MaxValue
        End If
      End Get
    End Property

    Public ReadOnly Property Icon() As String
      Get
        Return mIcon
      End Get
    End Property

    Public ReadOnly Property HtmlInput() As HtmlControls.HtmlInputFile
      Get
        Return mHtmlInput
      End Get
    End Property

    Public ReadOnly Property Type() As Config.ItemType
      Get
        Return mType
      End Get
    End Property

    Public Property GalleryConfig() As DotNetNuke.Modules.Gallery.Config
      Get
        Return mGalleryConfig
      End Get
      Set(ByVal Value As DotNetNuke.Modules.Gallery.Config)
        mGalleryConfig = Value
      End Set
    End Property

    Public ReadOnly Property ZipHeaders() As ZipHeaderReader
      Get
        If Extension = ".zip" AndAlso mZipHeaders Is Nothing Then
          mZipHeaders = New ZipHeaderReader(HtmlInput.PostedFile.InputStream)
        End If
        Return mZipHeaders
      End Get
    End Property

    Public ReadOnly Property ExceedsFileSizeLimit(ByVal limit As Integer) As Boolean
      Get
        If limit = 0 Then Return False 'no limit specified

        If mExtension = ".zip" Then
          Return ZipHeaders.ExceedsFileSizeLimit(limit)
        Else
          Return ContentLength > limit
        End If
      End Get
    End Property

    Public ReadOnly Property IsValidFileType() As Boolean
      Get
        Dim isValid As Boolean = True
        If mExtension = ".zip" Then
          For Each entry As ZipFileEntry In ZipHeaders.Entries
            isValid = GalleryConfig.IsValidFileType(entry.Extension)
            If Not isValid Then Exit For
          Next
        Else
          isValid = GalleryConfig.IsValidFileType(mExtension)
        End If
        Return isValid
      End Get
    End Property

#End Region

    Public Sub New(ByVal httpInput As HtmlControls.HtmlInputFile)
      mHtmlInput = httpInput
      mFileName = Utils.MakeValidFilename(Path.GetFileName(httpInput.PostedFile.FileName), "_")
      mExtension = Path.GetExtension(mFileName).ToLower
      mContentLength = HtmlInput.PostedFile.ContentLength
      If mExtension = ".zip" Then
        If ZipHeaders.IsValid And Not ZipHeaders.HasError Then
          mContentLength = Convert.ToInt32(ZipHeaders.UncompressedSize)
        End If
      End If
    End Sub

    ''' <summary>
    ''' Validate the upload file to see if it meets some general
    ''' Criteria we have for valid files
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ValidateFile() As String

      If ContentLength = 0 Then
        Dim str As String
        str = Localization.GetString("UploadFile_InvalidFile", mGalleryConfig.SharedResourceFile)
        If String.IsNullOrEmpty(str) Then
          str = "Invalid file"
        End If
        Return str
      End If

      'William Severance - check now includes test against uncompressed size of each file in zip

      If ExceedsFileSizeLimit(mGalleryConfig.MaxFileSize * 1024) Then
        Dim str As String
        str = Localization.GetString("UploadFile_InvalidSize", mGalleryConfig.SharedResourceFile)
        If String.IsNullOrEmpty(str) Then
          str = "File size (including size of each file in a zip) must be smaller than {0} kb"
        End If
        str = String.Format(str, mGalleryConfig.MaxFileSize.ToString)
        Return str
      End If

      If Not IsValidFileType Then
        Dim str As String
        str = Localization.GetString("UploadFile_InvalidExtension", mGalleryConfig.SharedResourceFile)
        If String.IsNullOrEmpty(str) Then
          str = "{0} is not an acceptable file type."
        End If
        str = String.Format(str, mExtension)
        Return str
      Else
        'Generate icon
        mIcon = Utils.GetFileTypeIcon(mExtension, GalleryConfig, mType)
      End If
      Return ""
    End Function

  End Class

End Namespace