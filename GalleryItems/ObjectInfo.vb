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

Namespace DotNetNuke.Modules.Gallery

    Public Interface IGalleryObjectInfo

        ReadOnly Property Index() As Integer
        ReadOnly Property ID() As Integer
        ReadOnly Property Name() As String
        ReadOnly Property Parent() As GalleryFolder
        ReadOnly Property URL() As String
        ReadOnly Property Path() As String
        ReadOnly Property SourceURL() As String
        ReadOnly Property IconURL() As String
        ReadOnly Property ScoreImageURL() As String

        ' Thumbnail & ThumbnailURL should be Read/Write to be modified
        Property Thumbnail() As String
        Property ThumbnailURL() As String

        ' Info from XML also interface implementation
        Property Title() As String
        Property Description() As String
        Property Categories() As String
        Property Author() As String
        Property Client() As String
        Property Location() As String
        Property OwnerID() As Integer
        Property Score() As Double
        Property Width() As Integer
        Property Height() As Integer
        Property ApprovedDate() As DateTime

        ReadOnly Property CreatedDate() As DateTime

        ReadOnly Property BrowserURL() As String
        ReadOnly Property SlideshowURL() As String
        ReadOnly Property EditURL() As String
        ReadOnly Property ExifURL() As String
        ReadOnly Property DownloadURL() As String
        ReadOnly Property VotingURL() As String

    'ReadOnly Property ImageCssClass() As String
    'ReadOnly Property ThumbnailCssClass() As String

        ReadOnly Property ItemInfo() As String
        ReadOnly Property Size() As Long
        ReadOnly Property IsFolder() As Boolean
        ReadOnly Property Type() As Config.ItemType
        ReadOnly Property GalleryConfig() As DotNetNuke.Modules.Gallery.Config

    End Interface

    'William Severance - added class to provide argument for gallery events including
    'GalleryObjectDeleted and GalleryObjectCreated

    Public Class GalleryObjectEventArgs
        Inherits System.EventArgs

        Private _Item As IGalleryObjectInfo
        Private _UserID As Integer

        Public Sub New(ByVal Item As IGalleryObjectInfo, ByVal UserID As Integer)
            _Item = Item
            _UserID = UserID
        End Sub

        Public ReadOnly Property Item() As IGalleryObjectInfo
            Get
                Return _Item
            End Get
        End Property

        Public ReadOnly Property UserID() As Integer
            Get
                Return _UserID
            End Get
        End Property
    End Class

End Namespace