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
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.ComponentModel
Imports System.Configuration
Imports System.Web.Caching
Imports System.IO
Imports System.Collections
Imports System.Xml


Namespace DotNetNuke.Modules.Gallery.Views

    <ToolboxData("<{0}:TemplateList runat=server></{0}:TemplateList>")> _
    Public Class TemplateList
        Inherits System.Web.UI.WebControls.DropDownList

        Public Enum Type
            File
            Folder
        End Enum

        Const NODE_TO_SEARCH As String = "styles/style"

        Private xml As XPath.XPathDocument
        Private xpath As XPath.XPathNavigator

        Public Sub New()
        End Sub 'New

        Protected Overrides Sub OnDataBinding(ByVal e As EventArgs)

            If Not Page.IsPostBack Then
                ' Add first item for empty module
                'Items.Add(New ListItem("(None Specified)", ""))

                If SourceDirectory = "" Then SourceDirectory = "DesktopModules/Gallery/"
                Dim _path As String = Path.Combine(Context.Server.MapPath(SourceDirectory), TargetName)

                Select Case TargetType
                    Case Type.Folder
                        ' Make sure this folder exists
                        If Directory.Exists(_path) Then
                            Dim _folders() As String = Directory.GetDirectories(_path)
                            Dim _folder As String
                            Dim _folderName As String

                            For Each _folder In _folders
                                _folderName = Path.GetFileName(_folder)
                                Items.Add(New ListItem(_folderName, _folderName))
                            Next
                        End If

                        'Case Type.File
                        '    ' Make sure this file exists
                        '    If File.Exists(_path) Then
                        '        Try
                        '            Dim iterator As xpath.XPathNodeIterator
                        '            Dim i As Integer

                        '            xml = New xpath.XPathDocument(_path)
                        '            xpath = xml.CreateNavigator()

                        '            iterator = xpath.Select(NODE_TO_SEARCH)

                        '            For i = 1 To iterator.Count
                        '                If iterator.MoveNext Then
                        '                    Dim styleName As String = iterator.Current.GetAttribute("name", "")
                        '                    Items.Add(New ListItem(styleName, styleName))

                        '                    ' We add selectedItem check here to prevent error
                        '                    'If (SelectedText.Length > 0) AndAlso (styleName = SelectedText) Then
                        '                    '    _validSelect = True
                        '                    'End If
                        '                End If
                        '            Next
                        '        Catch ex As Exception
                        '        End Try
                        '    End If

                End Select
                MyBase.OnDataBinding(e)

            End If
        End Sub

        <Bindable(True), Category("Data"), DefaultValue(Type.File)> Public Property TargetType() As TemplateList.Type
            Get
                'Return mTargetType
                Dim savedState As Object = ViewState("TargetType")
                If savedState Is Nothing Then
                    Return Type.File
                Else
                    Return CType(savedState, TemplateList.Type)
                End If
            End Get
            Set(ByVal Value As Type)
                'mTargetType = Value
                ViewState("TargetType") = Value
            End Set
        End Property

        <Bindable(True), Category("Data"), DefaultValue("DesktopModules/Gallery/")> Public Property SourceDirectory() As String
            Get
                'Return mSourceDirectory
                Dim savedState As Object = ViewState("SourceDirectory")
                If savedState Is Nothing Then
                    Return "DesktopModules/Gallery/"
                Else
                    Return CType(savedState, String)
                End If
            End Get
            Set(ByVal Value As String)
                ViewState("SourceDirectory") = Value
            End Set
        End Property

        <Bindable(True), Category("Data"), DefaultValue("_Galleryemail.xml")> Public Property TargetName() As String
            Get
                Dim savedState As Object = ViewState("TargetName")
                If savedState Is Nothing Then
                    Return "_Galleryemail.xml"
                Else
                    Return CType(savedState, String)
                End If
            End Get
            Set(ByVal Value As String)
                ViewState("TargetName") = Value
            End Set
        End Property

    End Class

End Namespace