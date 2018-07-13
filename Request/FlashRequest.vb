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

    ''' <summary>
    ''' Class for using the viewer to view galleries
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GalleryFlashRequest
        Inherits BaseRequest

        Private mRequest As HttpRequest

        ' Stored reference to ItemIndex of Browseable Items collection
        Private _currentItem As Integer

#Region "Public Properties"

        Public ReadOnly Property CurrentItem() As GalleryFile
            Get
                Return CType(MyBase.Folder.List.Item(_currentItem), GalleryFile)
            End Get
        End Property

        Public ReadOnly Property CurrentItemNumber() As Integer
            Get
                Return _currentItem + 1
            End Get
        End Property

#End Region

#Region "Public Methods"

        Public Sub New(ByVal ModuleID As Integer)
            MyBase.New(ModuleID)

            ' Don't want to continue processing if this is an invalid path
            If Not GalleryConfig.IsValidPath Then
                Exit Sub
            End If

            mRequest = HttpContext.Current.Request

            If Not mRequest.QueryString("currentitem") Is Nothing Then
                _currentItem = Integer.Parse(mRequest.QueryString("currentitem"))
            Else
                If MyBase.Folder.MediaItems.Count > 0 Then
                    _currentItem = MyBase.Folder.FlashItems(0) 'CType(MyBase.Folder.FlashItems.Item(0), GalleryFile).Index
                End If
            End If

        End Sub

#End Region

    End Class

End Namespace