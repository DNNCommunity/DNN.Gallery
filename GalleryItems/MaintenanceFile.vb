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

Imports System.Web
Imports System.Text
Imports System.IO
Imports DotNetNuke
Imports DotNetNuke.Security
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Entities.Modules.Actions
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Modules.Gallery.Config
Imports DotNetNuke.Modules.Gallery.Utils

Namespace DotNetNuke.Modules.Gallery

  Public Class GalleryMaintenanceFile
    Private mParent As GalleryFolder
    Private mName As String
    Private mType As Config.ItemType

    Private mSourcePath As String
    Private mAlbumPath As String
    Private mThumbPath As String
    Private mIconURL As String
    Private mURL As String

    Private mSourceExists As Boolean
    Private mFileExists As Boolean
    Private mThumbExists As Boolean

    Public ReadOnly Property Parent() As GalleryFolder
      Get
        Return mParent
      End Get
    End Property

    Public ReadOnly Property Name() As String
      Get
        Return mName
      End Get
    End Property

    Public ReadOnly Property Type() As Config.ItemType
      Get
        Return mType
      End Get
    End Property

    Public ReadOnly Property URL() As String
      Get
        Return mURL
      End Get
    End Property

    Public ReadOnly Property SourcePath() As String
      Get
        Return mSourcePath
      End Get
    End Property

    Public ReadOnly Property AlbumPath() As String
      Get
        Return mAlbumPath
      End Get
    End Property

    Public ReadOnly Property ThumbPath() As String
      Get
        Return mThumbPath
      End Get
    End Property

    Public Property IconURL() As String
      Get
        Return mIconURL
      End Get
      Set(ByVal Value As String)
        mIconURL = Value
      End Set
    End Property

    Public Property SourceExists() As Boolean
      Get
        Return mSourceExists
      End Get
      Set(ByVal Value As Boolean)
        mSourceExists = Value
      End Set
    End Property

    Public Property FileExists() As Boolean
      Get
        Return mFileExists
      End Get
      Set(ByVal Value As Boolean)
        mFileExists = Value
      End Set
    End Property

    Public Property ThumbExists() As Boolean
      Get
        Return mThumbExists
      End Get
      Set(ByVal Value As Boolean)
        mThumbExists = Value
      End Set
    End Property

    Sub New( _
        ByVal Parent As GalleryFolder, _
        ByVal Name As String, _
        ByVal Type As Config.ItemType, _
        ByVal URL As String)

      mParent = Parent
      mName = Name
      mType = Type
      mURL = URL

      Dim config As DotNetNuke.Modules.Gallery.Config = Parent.GalleryConfig

      Select Case Type
        Case config.ItemType.Image
          mIconURL = config.GetImageURL("s_jpg.gif") 'imageIcon
        Case config.ItemType.Flash
          mIconURL = config.GetImageURL("s_flash.gif")
        Case config.ItemType.Movie
          mIconURL = config.GetImageURL("s_mediaplayer.gif")
      End Select

      PopulateProperties()

    End Sub

    Private Sub PopulateProperties()
      mSourcePath = IO.Path.Combine(mParent.SourceFolderPath, mName)
      mAlbumPath = IO.Path.Combine(mParent.Path, mName)
      mThumbPath = IO.Path.Combine(mParent.ThumbFolderPath, mName)
    End Sub

    Public Sub Synchronize()

      If mSourceExists Then ' Copy from source to album
        CreateFileFromSource()
      Else 'Copy from album to source
        CreateSourceFromFile()
      End If

      RebuildThumbnail()

    End Sub

    Public Sub CreateSourceFromFile()

      'William Severance - modified to interface with DNN file system.
      Dim ps As PortalSettings = PortalController.GetCurrentPortalSettings()
      If mFileExists Then
        FileSystemUtils.CopyFile(mAlbumPath, mSourcePath, ps)
        mFileExists = True
      End If
    End Sub

    Public Sub CreateFileFromSource()
      If mSourceExists Then
        With mParent.GalleryConfig
          ResizeImage(mSourcePath, mAlbumPath, .FixedWidth, .FixedHeight, .EncoderQuality)
        End With
        mFileExists = True
      End If
    End Sub

    Public Sub DeleteAll()
      mParent.DeleteChild(mName)
    End Sub

    Public Sub RebuildThumbnail()
      DeleteThumbnail()
      CreateThumbnail()
    End Sub

    Public Sub DeleteThumbnail()
      Dim ps As PortalSettings = PortalController.GetCurrentPortalSettings()
      If File.Exists(mThumbPath) Then mParent.DeleteFile(mThumbPath, ps, False)
      mThumbExists = False
    End Sub

    Public Sub CreateThumbnail()
      Dim sourcePath As String = Nothing
      If mSourceExists Then
        sourcePath = mSourcePath
      ElseIf mFileExists Then
        sourcePath = mAlbumPath
      End If
      If sourcePath Is Nothing Then
        mThumbExists = False
      Else
        With mParent.GalleryConfig
          ResizeImage(sourcePath, mThumbPath, .MaximumThumbWidth, .MaximumThumbHeight, .EncoderQuality)
        End With
        ThumbExists = True
      End If
    End Sub

  End Class

End Namespace