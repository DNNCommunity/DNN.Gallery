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
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.services.Localization
Imports DotNetNuke.Modules.Gallery.Config
Imports DotNetNuke.Modules.Gallery.Utils

Namespace DotNetNuke.Modules.Gallery

    Public Class GalleryMaintenanceRequest
        ' Class for using the viewer to view galleries
        Inherits BaseRequest

        Private mImageList As Generic.List(Of GalleryMaintenanceFile) 'GalleryObjectCollection

        Public ReadOnly Property ImageList() As Generic.List(Of GalleryMaintenanceFile) 'GalleryObjectCollection
            Get
                If mImageList Is Nothing Then
                    mImageList = New Generic.List(Of GalleryMaintenanceFile)
                End If
                Return mImageList
            End Get
        End Property

        Public Sub New(ByVal ModuleID As Integer)

            MyBase.New(ModuleID)

            ' Don't want to continue processing if this is an invalid path
            If Not GalleryConfig.IsValidPath Then
                Exit Sub
            End If

            Populate()

        End Sub

        Public Sub Populate()
            Dim mAlbumPath As String = MyBase.Folder.Path
            Dim mSourcePath As String = MyBase.Folder.SourceFolderPath
            Dim mThumbPath As String = MyBase.Folder.ThumbFolderPath
            Dim file As String

            ' Clear imageList first
            ImageList.Clear()

            ' Repopulate gallery folder to make sure all new files to be populated
            If Not Folder.IsPopulated Then
                Folder.Populate(True)
            End If

            Dim mSourceFiles As String() = System.IO.Directory.GetFiles(mSourcePath)
            Dim Sources As New ArrayList

            'Build sources arraylist
            For Each file In mSourceFiles
                Sources.Add(LCase(System.IO.Path.GetFileName(file)))
            Next

            ' Build thumbs arraylist
            Dim mThumbFiles As String() = System.IO.Directory.GetFiles(mThumbPath)
            Dim Thumbs As New ArrayList
            For Each file In mThumbFiles
                Thumbs.Add(LCase(System.IO.Path.GetFileName(file)))
            Next

            Dim item As IGalleryObjectInfo
            Dim fileName As String
            Dim fileExtension As String
            Dim fileType As Config.ItemType

            'Populate all items in this folder
            For Each item In MyBase.Folder.List
                If Not item.IsFolder Then
                    fileName = item.Name
                    Dim fileItem As New GalleryMaintenanceFile(MyBase.Folder, fileName, item.Type, item.BrowserURL)

                    fileItem.FileExists = True

                    ' Check if file exists in source folder
                    If Sources.Contains(LCase(fileName)) Then
                        fileItem.SourceExists = True
                        ' For checking, remove this file from arraylist
                        Sources.Remove(LCase(fileName))
                    End If

                    ' Check if file exists in thumb folder
                    If Thumbs.Contains(LCase(fileName)) Then
                        fileItem.ThumbExists = True
                        Thumbs.Remove(LCase(fileName))
                    End If

                    'Add to filelist, it's not nessesary for this version, however will be used in next version
                    'mFileList.Add(fileName, fileItem)

                    ' Add to imagelist
                    If item.Type = Config.ItemType.Image _
                    AndAlso Not LCase(item.Title) = "icon" _
                    AndAlso Not LCase(item.Title) = "watermark" Then
                        ImageList.Add(fileItem)
                    End If

                End If
            Next

            ' List file exists in source & missing in album
            For Each fileName In Sources
                fileExtension = IO.Path.GetExtension(fileName)
                fileType = GalleryConfig.TypeFromExtension(fileExtension)

                ' Add to file list, reserved for next version feature
                'mFileList.Add(fileName, fileItem)

                If fileType = Config.ItemType.Image Then
                    Dim fileItem As New GalleryMaintenanceFile(MyBase.Folder, fileName, fileType, "")
                    fileItem.SourceExists = True

                    ' Check if file exists in thumb folder
                    If Thumbs.Contains(LCase(fileName)) Then
                        fileItem.ThumbExists = True
                        Thumbs.Remove(LCase(fileName))
                    End If

                    ImageList.Add(fileItem)
                End If
            Next

            ' Check in thumb folder, for clean up unused thumbnail
            For Each fileName In Thumbs
                fileExtension = IO.Path.GetExtension(fileName)
                fileType = GalleryConfig.TypeFromExtension(fileExtension)

                If fileType = Config.ItemType.Image Then
                    Dim fileItem As New GalleryMaintenanceFile(MyBase.Folder, fileName, fileType, "")
                    fileItem.ThumbExists = True
                    ImageList.Add(fileItem)
                End If

            Next
        End Sub

    End Class

End Namespace