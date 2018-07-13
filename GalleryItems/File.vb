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
Imports DotNetNuke
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Modules.Gallery.Config
Imports DotNetNuke.Modules.Gallery.Utils

Namespace DotNetNuke.Modules.Gallery

  Public Class GalleryFile
    Implements IGalleryObjectInfo

#Region "Private Variables"
    Private mIndex As Integer
    Private mID As Integer
    ' System info
    Private mParent As GalleryFolder
    Private mName As String
    Private mURL As String
    Private mPath As String
    Private mThumbnail As String
    Private mThumbnailURL As String
    Private mThumbnailPath As String
    Private mIcon As String
    Private mIconURL As String
    Private mScoreImage As String
    Private mScoreImageURL As String
    Private mSourceURL As String
    Private mSourcePath As String

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
    Private mWidth As Integer
    Private mHeight As Integer

    ' Interface implementation for display purpose        
    Private mBrowserURL As String = ""
    Private mPopupBrowserURL As String = ""
    Private mPopupExifURL As String = ""
    Private mPopupEditImageURL As String = ""
    Private mDownloadURL As String = ""
    Private mItemInfo As String = ""
    Private mSize As Long = 0
    Private mType As Config.ItemType
    Private mScore As Double = 0
    Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config

#End Region

#Region "Properties"
    '<tamttt:note identity id for all  module objects>
    Public ReadOnly Property ObjectTypeCode() As Integer
      Get
        Return 502
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

    Public ReadOnly Property ThumbnailPath() As String
      Get
        Return mThumbnailPath
      End Get
    End Property

    Public ReadOnly Property SourceURL() As String Implements IGalleryObjectInfo.SourceURL
      Get
        Return mSourceURL
      End Get
    End Property

    Public ReadOnly Property SourcePath() As String
      Get
        Return mSourcePath
      End Get
    End Property

    Public ReadOnly Property Icon() As String
      Get
        Return mIcon
      End Get
    End Property

    Public ReadOnly Property IconURL() As String Implements IGalleryObjectInfo.IconURL
      Get
        Return mIconURL
      End Get
    End Property

    Public ReadOnly Property ScoreImage() As String
      Get
        Return mScoreImage
      End Get
    End Property

    Public ReadOnly Property ScoreImageURL() As String Implements IGalleryObjectInfo.ScoreImageURL
      Get
        Return mScoreImageURL
      End Get
    End Property

    Public ReadOnly Property Size() As Long Implements IGalleryObjectInfo.Size
      Get
        Return mSize
      End Get
    End Property

    Public ReadOnly Property Type() As Config.ItemType Implements IGalleryObjectInfo.Type
      Get
        Return mType
      End Get
    End Property

    Public ReadOnly Property IsFolder() As Boolean Implements IGalleryObjectInfo.IsFolder
      Get
        Return False
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

    ''Public ReadOnly Property ImageCssClass() As String Implements IGalleryObjectInfo.ImageCssClass
    ''  Get
    ''    Return "Gallery_Image"
    ''  End Get
    ''End Property

    ''Public ReadOnly Property ThumbnailCssClass() As String Implements IGalleryObjectInfo.ThumbnailCssClass
    ''  Get
    ''    If mType = Config.ItemType.Image Then
    ''      Return "Gallery_Thumb"
    ''    Else
    ''      Return ""
    ''    End If
    ''  End Get
    ''End Property

    Public Property OwnerID() As Integer Implements IGalleryObjectInfo.OwnerID
      Get
        Return mOwnerID
      End Get
      Set(ByVal Value As Integer)
        mOwnerID = Value
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

    Public Property Width() As Integer Implements IGalleryObjectInfo.Width
      Get
        Return mWidth
      End Get
      Set(ByVal value As Integer)
        mWidth = value
      End Set
    End Property

    Public Property Height() As Integer Implements IGalleryObjectInfo.Height
      Get
        Return mHeight
      End Get
      Set(ByVal value As Integer)
        mHeight = value
      End Set
    End Property

    Public Property Score() As Double Implements IGalleryObjectInfo.Score
      Get
        Return mScore
      End Get
      Set(ByVal Value As Double)
        mScore = Value
      End Set
    End Property

    Public ReadOnly Property BrowserURL() As String Implements IGalleryObjectInfo.BrowserURL
      Get
        If mGalleryConfig.AllowPopup Then
          Return mPopupBrowserURL
        Else
          Return GetBrowserURL()
        End If
      End Get
    End Property

    Public ReadOnly Property SlideshowURL() As String Implements IGalleryObjectInfo.SlideshowURL
      Get
        If mGalleryConfig.AllowPopup Then
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

    Public ReadOnly Property EditImageURL() As String
      Get
        If mGalleryConfig.AllowPopup Then
          Return mPopupEditImageURL
        Else
          Dim params As New Generic.List(Of String)
          params.Add("mid=" & GalleryConfig.ModuleID.ToString)
          If Not String.IsNullOrEmpty(Parent.GalleryHierarchy) Then
            params.Add("path=" & FriendlyURLEncode(Parent.GalleryHierarchy))
          End If
          params.Add("currentitem=" & Me.Index.ToString)
          params.Add("mode=edit")
          params.Add("media=" & Name)
          Return NavigateURL(GalleryConfig.GalleryTabID, "Viewer", params.ToArray())
        End If
      End Get
    End Property

    Public ReadOnly Property ExifURL() As String Implements IGalleryObjectInfo.ExifURL
      Get
        If Type <> Config.ItemType.Image Then Return ""
        If mGalleryConfig.AllowPopup Then
          Return mPopupExifURL
        Else
          Return GetExifURL()
        End If
      End Get
    End Property

    Public ReadOnly Property DownloadURL() As String Implements IGalleryObjectInfo.DownloadURL
      Get
        Return mDownloadURL
      End Get
    End Property

    Public ReadOnly Property VotingURL() As String Implements IGalleryObjectInfo.VotingURL
      Get
        Return GetVotingURL()
      End Get
    End Property

    Public ReadOnly Property ItemInfo() As String Implements IGalleryObjectInfo.ItemInfo
      Get
        Return mItemInfo
      End Get
    End Property

    Public ReadOnly Property Votes() As VoteCollection
      Get
        Return VoteCollection.GetVoting(Me.Parent.Path, mName)
      End Get
    End Property

    Public ReadOnly Property GalleryConfig() As DotNetNuke.Modules.Gallery.Config Implements IGalleryObjectInfo.GalleryConfig
      Get
        Return mGalleryConfig
      End Get
    End Property

#End Region

#Region "Methods and Functions"

    Friend Sub New( _
      ByVal Index As Integer, _
        ByVal ID As Integer, _
        ByVal Parent As GalleryFolder, _
        ByVal Name As String, _
        ByVal URL As String, _
        ByVal Path As String, _
        ByVal Thumbnail As String, _
        ByVal ThumbnailURL As String, _
        ByVal ThumbnailPath As String, _
        ByVal Icon As String, _
        ByVal IconURL As String, _
        ByVal ScoreImage As String, _
        ByVal ScoreImageURL As String, _
        ByVal SourcePath As String, _
        ByVal SourceURL As String, _
        ByVal Size As Long, _
        ByVal Type As Config.ItemType, _
        ByVal Title As String, _
        ByVal Description As String, _
        ByVal Categories As String, _
        ByVal Author As String, _
        ByVal Client As String, _
        ByVal Location As String, _
        ByVal OwnerID As Integer, _
        ByVal ApprovedDate As DateTime, _
        ByVal CreatedDate As DateTime, _
        ByVal Width As Integer, _
        ByVal Height As Integer, _
        ByVal Score As Double, _
        ByVal GalleryConfig As DotNetNuke.Modules.Gallery.Config)

      mIndex = Index
      mID = ID
      mParent = Parent
      mName = Name
      mURL = URL
      mPath = Path
      mThumbnail = Thumbnail
      mThumbnailURL = ThumbnailURL
      mThumbnailPath = ThumbnailPath
      mIcon = Icon
      mIconURL = IconURL
      mScoreImage = ScoreImage
      mScoreImageURL = ScoreImageURL
      mSourcePath = SourcePath
      mSourceURL = SourceURL
      mSize = Size
      mType = Type
      mTitle = Title
      mDescription = Description
      mCategories = Categories
      mAuthor = Author
      mClient = Client
      mLocation = Location
      mOwnerID = OwnerID
      mApprovedDate = ApprovedDate
      mCreatedDate = CreatedDate
      mWidth = Width
      mHeight = Height
      mScore = Score
      mGalleryConfig = GalleryConfig
      mPopupBrowserURL = GetPopupBrowserURL()
      mPopupExifURL = GetPopupExifURL()
      mPopupEditImageURL = GetPopupEditImageURL()
      mDownloadURL = GetDownloadURL()
      mItemInfo = GetItemInfo()
    End Sub

    Public Sub UpdateImage(ByVal NewImage As Bitmap, ByVal iFormat As Imaging.ImageFormat)
      If Not Me.Type = Config.ItemType.Image OrElse NewImage Is Nothing Then Exit Sub
      Try
        File.Delete(Path)
        File.Delete(ThumbnailPath)
        NewImage.Save(Path, iFormat)
        Utils.SaveDNNFile(Path, NewImage.Width, NewImage.Height, False, True)
        ResizeImage(Me.Path, Me.ThumbnailPath, GalleryConfig.MaximumThumbWidth, GalleryConfig.MaximumThumbHeight, GalleryConfig.EncoderQuality)
      Catch exc As Exception
        Throw
      End Try
    End Sub

    ' Not currently used anywhere in the module
    Public Sub RestoreImage()
      If Not Me.Type = Config.ItemType.Image Then Exit Sub
      Try
        ResizeImage(Me.SourcePath, Me.Path, GalleryConfig.FixedWidth, GalleryConfig.FixedHeight, GalleryConfig.EncoderQuality)
      Catch ex As Exception
      End Try
    End Sub

    Public Sub UpdateVotes(ByVal NewVote As Vote)
      GalleryXML.AddVote(Parent.Path, NewVote)
      VoteCollection.ResetCollection(Parent.Path, Name)

      Dim votes As VoteCollection = VoteCollection.GetVoting(Parent.Path, Name)
      Dim newScore As Double = votes.Score

      ' Save new score back to file metadata, it would be un-nessesary, but give a better performance when populate file
      GalleryXML.SaveMetaData(Parent.Path, Name, ID, _
                  Title, Description, Categories, _
                  Author, Location, Client, OwnerID, _
                  CreatedDate, ApprovedDate, newScore)

      Score = newScore
      Config.ResetGalleryFolder(Me.Parent)
    End Sub

    Private Function GetBrowserURL() As String
      Dim params As New Generic.List(Of String)
      params.Add("mid=" & GalleryConfig.ModuleID.ToString)
      If Not String.IsNullOrEmpty(Parent.GalleryHierarchy) Then
        params.Add("path=" & FriendlyURLEncode(Parent.GalleryHierarchy))
      End If
      params.Add("currentitem=" & Index.ToString)

      Dim ctlKey As String = ""
      Select Case mType
        Case Config.ItemType.Image
          ctlKey = "Viewer"
        Case Config.ItemType.Flash
          ctlKey = "FlashPlayer"
        Case Config.ItemType.Movie
          ctlKey = "MediaPlayer"
        Case Else
          Throw New Exception("Unknown mType, " & mType.ToString)
      End Select
      Return NavigateURL(GalleryConfig.GalleryTabID, ctlKey, params.ToArray())
    End Function

    Private Function GetPopupBrowserURL() As String

      Dim sb As New StringBuilder

      sb.Append(GalleryConfig.SourceDirectory)
      sb.Append("/GalleryPage.aspx?ctl=")

      Select Case mType
        Case Config.ItemType.Image
          sb.Append("Viewer")
        Case Config.ItemType.Movie
          sb.Append("MediaPlayer")
        Case Config.ItemType.Flash
          sb.Append("FlashPlayer")
      End Select
      sb.Append("&tabid=")
      ' <notes: found & fixed by Vicenç Masanas> 
      sb.Append(GalleryConfig.GalleryTabID.ToString)
      sb.Append("&mid=")
      sb.Append(GalleryConfig.ModuleID.ToString)
      ' </notes> 
      If Not String.IsNullOrEmpty(Parent.GalleryHierarchy) Then
        sb.Append("&path=")
        sb.Append(JSEncode(Parent.GalleryHierarchy))
      End If
      sb.Append(FriendlyURLEncode(URLParams))

      Return BuildPopup(sb.ToString, GalleryConfig.FixedHeight + 200, GalleryConfig.FixedWidth + 100)

    End Function

    Private Function GetSlideshowURL() As String
      If mType <> Config.ItemType.Image Then
        Return Parent.SlideshowURL
      Else
        Dim params As New Generic.List(Of String)
        params.Add("mid=" & GalleryConfig.ModuleID.ToString)
        If Not String.IsNullOrEmpty(Parent.GalleryHierarchy) Then
          params.Add("path=" & FriendlyURLEncode(Parent.GalleryHierarchy))
        End If
        params.Add("currentitem=" & Index.ToString)

        Dim ctlKey As String = "SlideShow"
        Return NavigateURL(GalleryConfig.GalleryTabID, ctlKey, params.ToArray())
      End If
    End Function

    Private Function GetPopupSlideshowURL() As String
      If mType <> Config.ItemType.Image Then
        Return Parent.SlideshowURL
      Else
        Dim sb As New StringBuilder
        sb.Append(GalleryConfig.SourceDirectory)
        sb.Append("/GalleryPage.aspx?ctl=SlideShow")
        sb.Append("&mid=")
        sb.Append(GalleryConfig.ModuleID.ToString)
        sb.Append("&tabid=")
        sb.Append(GalleryConfig.GalleryTabID.ToString)
        If Not String.IsNullOrEmpty(Parent.GalleryHierarchy) Then
          sb.Append("&path=")
          sb.Append(FriendlyURLEncode(Parent.GalleryHierarchy))
        End If
        sb.Append("&currentitem=")
        sb.Append(Index.ToString)
        Return BuildPopup(sb.ToString, mGalleryConfig.FixedHeight + 160, mGalleryConfig.FixedWidth + 120)
      End If
    End Function

    Private Function GetExifURL() As String
      Dim params As New Generic.List(Of String)
      params.Add("mid=" & GalleryConfig.ModuleID.ToString)
      If Not String.IsNullOrEmpty(Parent.GalleryHierarchy) Then
        params.Add("path=" & FriendlyURLEncode(Parent.GalleryHierarchy))
      End If
      params.Add("currentitem=" & Me.Index.ToString)
      params.Add("media=" & Name)
      Return NavigateURL(GalleryConfig.GalleryTabID, "Exif", params.ToArray())
    End Function

    Private Function GetPopupExifURL() As String
      Dim sb As New StringBuilder

      sb.Append(GalleryConfig.SourceDirectory)
      sb.Append("/GalleryPage.aspx?ctl=Exif")
      sb.Append("&tabid=")
      sb.Append(GalleryConfig.GalleryTabID.ToString)
      sb.Append("&mid=")
      sb.Append(GalleryConfig.ModuleID.ToString)
      If Not String.IsNullOrEmpty(Parent.GalleryHierarchy) Then
        sb.Append("&path=")
        sb.Append(FriendlyURLEncode(Parent.GalleryHierarchy))
      End If
      sb.Append(FriendlyURLEncode(URLParams))

      Return BuildPopup(sb.ToString, mGalleryConfig.FixedHeight, mGalleryConfig.FixedWidth)
    End Function

    Private Function GetEditURL() As String
      Dim params As New Generic.List(Of String)
      params.Add("mid=" & GalleryConfig.ModuleID.ToString)
      If Not String.IsNullOrEmpty(Parent.GalleryHierarchy) Then
        params.Add("path=" & FriendlyURLEncode(Parent.GalleryHierarchy))
      End If
      params.Add("currentitem=" & Me.Index.ToString)
      Return NavigateURL(GalleryConfig.GalleryTabID, "FileEdit", params.ToArray())
    End Function

    Private Function GetPopupEditImageURL() As String
      Dim sb As New StringBuilder

      sb.Append(GalleryConfig.SourceDirectory)
      sb.Append("/GalleryPage.aspx?ctl=Viewer")
      sb.Append("&tabid=")
      sb.Append(GalleryConfig.GalleryTabID.ToString)
      sb.Append("&mid=")
      sb.Append(GalleryConfig.ModuleID.ToString)
      If Not String.IsNullOrEmpty(Parent.GalleryHierarchy) Then
        sb.Append("&path=")
        sb.Append(FriendlyURLEncode(Parent.GalleryHierarchy))
      End If
      sb.Append("&mode=edit")
      sb.Append(FriendlyURLEncode(URLParams))
      Return BuildPopup(sb.ToString, GalleryConfig.FixedHeight + 100, GalleryConfig.FixedWidth + 100)
    End Function

    Private Function GetVotingURL() As String
      Dim params As String() = New String(3) {"mid=" & GalleryConfig.ModuleID.ToString, "path=" & FriendlyURLEncode(Parent.GalleryHierarchy), "currentitem=" & Me.Index.ToString, "media=" & Name} ', "currentstrip=" & CurrentStrip.ToString}
      Return NavigateURL(GalleryConfig.GalleryTabID, "Voting", Utils.RemoveEmptyParams(params))
    End Function

    ' Replaced module's own Download.aspx with core LinkClick.aspx - WES 2-13-2009
    Private Function GetDownloadURL() As String
      Dim ps As PortalSettings = PortalController.GetCurrentPortalSettings()
      Dim filePath As String = SourcePath.Replace(ps.HomeDirectoryMapPath, "").Replace("\", "/")
      Dim fileID As Integer
      If ID >= 0 Then
        fileID = ID
      Else
        Dim fc As New DotNetNuke.Services.FileSystem.FileController
        fileID = fc.ConvertFilePathToFileId(filePath, ps.PortalId)
      End If
      Return DotNetNuke.Common.Globals.LinkClick("FileID=" & fileID.ToString, GalleryConfig.GalleryTabID, GalleryConfig.ModuleID, False, True)
    End Function

    Private Function URLParams() As String
      Dim sb As New StringBuilder
      sb.Append("&currentitem=")
      sb.Append(Index.ToString)
      Return sb.ToString
    End Function

    Protected Function GetItemInfo() As String
      Return GetItemInfo(True)
    End Function

    Friend Function GetItemInfo(ByVal HtmlFormatted As Boolean) As String
      Dim sb As New StringBuilder

      ' William Severance (9-10-2010) - Modified to use bitmapped TextDisplayOptions flags rather than iterating through string array for each test.

      If HtmlFormatted Then
        If (GalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Name) <> 0 Then
          sb.Append("<span class=""MediaName"">")
          sb.Append(Name)
          sb.Append("</span>")

          If (GalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Size) <> 0 Then
            sb.Append("<span class=""MediaSize"">")
            Dim sizeInfo As String = " " & Localization.GetString("FileSizeInfo", GalleryConfig.SharedResourceFile)
            sizeInfo = sizeInfo.Replace("[FileSize]", Math.Ceiling(Size / 1024).ToString)
            sb.Append(sizeInfo)
            sb.Append("</span>")
          End If
          sb.Append("<br />")
        End If

        If ((GalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Author) <> 0) _
              AndAlso Not Author.Length = 0 Then
          AppendLegendAndFieldHtmlFormatted(sb, "Author", Author)
        End If

        If ((GalleryConfig.TextDisplayOptions And GalleryDisplayOption.Client) <> 0) _
              AndAlso Not Client.Length = 0 Then
          AppendLegendAndFieldHtmlFormatted(sb, "Client", Client)
        End If

        If ((GalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Location) <> 0) _
              AndAlso Not Location.Length = 0 Then
          AppendLegendAndFieldHtmlFormatted(sb, "Location", Location)
        End If

        If (GalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.CreatedDate) <> 0 Then
          AppendLegendAndFieldHtmlFormatted(sb, "CreatedDate", Utils.DateToText(CreatedDate))
        End If

        If (GalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.ApprovedDate) <> 0 Then
          AppendLegendAndFieldHtmlFormatted(sb, "ApprovedDate", Utils.DateToText(ApprovedDate))
        End If
      Else
        If (GalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Name) <> 0 Then
          sb.Append(Name)
          If (GalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Size) <> 0 Then
            sb.Append(" ")
            Dim sizeInfo As String = " " & Localization.GetString("FileSizeInfo", GalleryConfig.SharedResourceFile)
            sizeInfo = sizeInfo.Replace("[FileSize]", Math.Ceiling(Size / 1024).ToString)
            sb.Append(sizeInfo)
          End If
          sb.AppendLine()
        End If

        If ((GalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Author) <> 0) _
              AndAlso Not Author.Length = 0 Then
          AppendLegendAndField(sb, "Author", Author)
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

      End If
      Return sb.ToString()

    End Function

    Private Sub AppendLegendAndFieldHtmlFormatted(ByVal sb As StringBuilder, ByVal LegendKey As String, ByVal FieldValue As String)
      sb.Append("<span class=""Legend"">")
      sb.Append(Localization.GetString(LegendKey, GalleryConfig.SharedResourceFile))
      sb.Append("</span><span class=""Field"">")
      sb.Append(FieldValue)
      sb.Append("</span>")
      sb.Append("<br/>")
    End Sub

    Private Sub AppendLegendAndField(ByVal sb As StringBuilder, ByVal LegendKey As String, ByVal FieldValue As String)
      sb.Append(Localization.GetString(LegendKey, GalleryConfig.SharedResourceFile))
      sb.Append(" ")
      sb.AppendLine(FieldValue)
    End Sub

#End Region

  End Class

End Namespace