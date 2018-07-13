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
Imports System.Web
Imports System.Text
Imports System.Configuration
Imports System.Collections.Specialized
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.Security
Imports DotNetNuke
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Services.Localization



Namespace DotNetNuke.Modules.Gallery.PopupControls



    Friend Class LocationListItem
        'Inherits DotNetNuke.Framework.PageBase
        '' Changed Inherits System.Web.UI.Page to DNN Framework PageBase by M. Schlomann
        Private mText As String
        Private mValue As String
        Private mLevel As Integer

        Sub New()
        End Sub

        Sub New(ByVal Text As String, ByVal Value As String, ByVal Level As Integer)
            mText = Text
            mValue = Value
            mLevel = Level
        End Sub

        Public ReadOnly Property DisplayString() As String
            Get
                Dim sb As New StringBuilder
                Dim intCount As Integer
                For intCount = 1 To mLevel
                    sb.Append("&nbsp;&nbsp;")
                Next
                'sb.Append("<img src=")
                'sb.Append("Images/" & mImage)
                'sb.Append(" />")
                sb.Append(mText)

                Return sb.ToString
            End Get
        End Property

        Public ReadOnly Property Value() As String
            Get
                Return mValue
            End Get
        End Property
    End Class


    Public Class PopupSearch
        Inherits PopupControl

        Public Sub New()
        End Sub 'New

        Protected Overrides Sub CreateObject()

            'Dim objClass As Integer = 0
            Dim strObjectClasses As String = ""
            Dim strObjectClass As String = ""
            Dim objClasses As New ArrayList
            Dim strLocations As String = ""

            Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
            Me.PortalID = _portalSettings.PortalId

            If Not Page.Request.QueryString("mid") Is Nothing Then
                ModuleID = Integer.Parse(Page.Request.QueryString("mid"))
            End If

            If Not Page.Request.QueryString("targetid") Is Nothing Then
                TargetID = Int16.Parse(Page.Request.QueryString("targetid"))
            End If

            ' provide data for dropdownlist of object classes
            If Not Page.Request.QueryString("objectclasses") Is Nothing Then
                strObjectClasses = Page.Request.QueryString("objectclasses")
                If Len(strObjectClasses) > 0 Then
                    For Each strObjectClass In Split(strObjectClasses, ",", , CompareMethod.Text)
                        Dim strName As String = [Enum].GetName(GetType(ObjectClass), Int16.Parse(strObjectClass))
                        objClasses.Add(New ListItem(strName, strObjectClass))
                    Next
                End If
            End If

            'provide data for dropdownlist of adsi domain
            If Not Page.Request.QueryString("locations") Is Nothing Then
                strLocations = Page.Request.QueryString("locations")
            End If

            PopupBaseObject = New SearchControlInfo(Me, objClasses, strLocations)

        End Sub

        Protected Overrides Sub Initialise()

            MyBase.Initialise()

        End Sub

        Protected Overrides Sub CreateChildControls()
            MyBase.CreateChildControls()
        End Sub
    End Class


    Public Class SearchControlInfo
        Inherits PopupObject

        Private mObjectClasses As New ArrayList
        Private mLocations As New ArrayList

        Public Sub New(ByVal SearchControl As PopupSearch, ByVal ObjectClasses As ArrayList, ByVal Locations As String)
            MyBase.New(SearchControl)

            mObjectClasses = ObjectClasses

            PopulateLocations(Locations)

        End Sub 'New

        Private ReadOnly Property SharedResourceFile() As String
            Get
                Return ApplicationPath & "/DesktopModules/Gallery/" & Localization.LocalResourceDirectory & "/" & Localization.LocalSharedResourceFile
            End Get
        End Property

        Private Sub PopulateLocations(ByVal Locations As String)
            Dim strLocation As String
            If Locations.Length > 0 Then
                For Each strLocation In Locations.Split(";"c)
                    If strLocation.Length > 0 Then
                        mLocations.Add(New LocationListItem(strLocation, strLocation, 1))
                    End If
                Next
            End If

        End Sub

        Private Function GetLocalizedItemText(ByVal item As ListItem) As String
            Dim localizedItemText As String = Localization.GetString(item.Text, SharedResourceFile)
            If localizedItemText Is Nothing Then
                localizedItemText = item.Text
            End If
            Return localizedItemText
        End Function

        Public Overrides Sub CreateChildControls()
        End Sub 'CreateChildControls

        Public Overrides Sub OnPreRender()
        End Sub 'OnPreRender

        Private Sub RenderTableBegin(ByVal wr As HtmlTextWriter)
            wr.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0")
            wr.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "2px")
            'wr.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
            wr.AddStyleAttribute("width", "100%")
            'wr.AddAttribute(HtmlTextWriterAttribute.Class, "Border")
            wr.RenderBeginTag(HtmlTextWriterTag.Table)

            wr.RenderBeginTag(HtmlTextWriterTag.Colgroup)
            wr.RenderBeginTag(HtmlTextWriterTag.Col)
            wr.RenderEndTag() ' Col
            'wr.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
            wr.AddStyleAttribute("width", "100%")
            wr.RenderBeginTag(HtmlTextWriterTag.Col)
            wr.RenderEndTag() ' Col
            wr.RenderBeginTag(HtmlTextWriterTag.Col)
            wr.RenderEndTag() ' Col
            wr.RenderBeginTag(HtmlTextWriterTag.Tbody)

        End Sub 'RenderPopup

        Private Sub RenderLocations(ByVal wr As HtmlTextWriter)
            If mLocations.Count > 0 Then
                Dim intCount As Integer

                wr.RenderBeginTag(HtmlTextWriterTag.Tr)
                'wr.AddAttribute(HtmlTextWriterAttribute.Height, "25px")
                wr.AddStyleAttribute("height", "25px")
                wr.RenderBeginTag(HtmlTextWriterTag.Td)
                'wr.AddAttribute(HtmlTextWriterAttribute.Align, "right")
                wr.AddStyleAttribute("text-align", "right")
                wr.RenderBeginTag(HtmlTextWriterTag.B)
                wr.Write("Search:")
                wr.RenderEndTag() ' B
                wr.RenderEndTag() ' TD  

                If mLocations.Count > 1 Then

                    wr.AddAttribute(HtmlTextWriterAttribute.Colspan, "2")
                    wr.RenderBeginTag(HtmlTextWriterTag.Td)

                    wr.AddAttribute(HtmlTextWriterAttribute.Class, "SelectBox")
                    wr.AddAttribute(HtmlTextWriterAttribute.Name, "selLocation")
                    wr.AddAttribute(HtmlTextWriterAttribute.Value, "")
                    wr.AddAttribute("changehandler", "selectChange")
                    wr.AddAttribute("tabbingindex", "1")
                    wr.RenderBeginTag(HtmlTextWriterTag.Span)

                    wr.AddAttribute(HtmlTextWriterAttribute.Style, "display: none")
                    wr.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0")
                    wr.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "2px")
                    wr.RenderBeginTag(HtmlTextWriterTag.Table)
                    wr.RenderBeginTag(HtmlTextWriterTag.Tbody)

                    For intCount = 0 To (mLocations.Count - 1)
                        wr.RenderBeginTag(HtmlTextWriterTag.Tr)
                        wr.AddAttribute("val", CType(mLocations.Item(intCount), LocationListItem).Value)
                        wr.RenderBeginTag(HtmlTextWriterTag.Td)
                        wr.Write(CType(mLocations.Item(intCount), LocationListItem).DisplayString)

                        wr.RenderEndTag() ' TD 
                        wr.RenderEndTag() ' TR 
                    Next

                    wr.RenderEndTag() ' </tbody> 
                    wr.RenderEndTag() ' </table> 
                    wr.RenderEndTag() ' </span> 
                    wr.RenderEndTag() ' </td>                

                Else ' Only one object to lookup

                    wr.AddAttribute(HtmlTextWriterAttribute.Colspan, "2")
                    wr.RenderBeginTag(HtmlTextWriterTag.Td)

                    wr.AddAttribute(HtmlTextWriterAttribute.Id, "selLocation")
                    wr.AddAttribute(HtmlTextWriterAttribute.Type, "hidden")
                    Try
                        wr.AddAttribute("value", CType(mLocations.Item(0), LocationListItem).Value)
                    Catch exc As Exception
                        exc.ToString()
                    End Try
                    wr.RenderBeginTag(HtmlTextWriterTag.Input)
                    wr.RenderEndTag() ' Input
                    Try
                        wr.Write(CType(mLocations.Item(intCount), LocationListItem).DisplayString)
                    Catch exc As Exception
                        exc.ToString()
                    End Try
                    wr.RenderEndTag() ' td

                End If
                wr.RenderEndTag() ' tr 
            End If
        End Sub

        Private Sub RenderObjectClasses(ByVal wr As HtmlTextWriter)
            Dim intCount As Integer

            wr.RenderBeginTag(HtmlTextWriterTag.Tr)
            'wr.AddAttribute(HtmlTextWriterAttribute.Height, "25px")
            'wr.AddAttribute(HtmlTextWriterAttribute.Align, "right")
            wr.AddStyleAttribute("height", "25px")
            wr.AddStyleAttribute("text-align", "right")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)

            '============This render to prevent error in case we don't render location
            If mLocations.Count = 0 Then
                wr.AddAttribute(HtmlTextWriterAttribute.Id, "selLocation")
                wr.AddAttribute(HtmlTextWriterAttribute.Type, "hidden")
                wr.AddAttribute("returnvalue", "")
                wr.RenderBeginTag(HtmlTextWriterTag.Input)
                wr.RenderEndTag() ' Input
            End If
            '==========================================================================
            wr.RenderBeginTag(HtmlTextWriterTag.B)
            wr.Write(Localization.GetString("Search_Lookup", SharedResourceFile))
            wr.RenderEndTag() ' B
            wr.RenderEndTag() ' TD            

            ' Render dropdownlist if user has to select which object to lookup
            If mObjectClasses.Count > 1 Then

                wr.AddAttribute(HtmlTextWriterAttribute.Colspan, "2")
                wr.RenderBeginTag(HtmlTextWriterTag.Td)

                wr.AddAttribute(HtmlTextWriterAttribute.Class, "SelectBox")
                wr.AddAttribute(HtmlTextWriterAttribute.Name, "selObject")
                wr.AddAttribute(HtmlTextWriterAttribute.Value, "")
                wr.AddAttribute("changehandler", "selectChange")
                wr.AddAttribute("tabbingindex", "1")
                wr.RenderBeginTag(HtmlTextWriterTag.Span)

                wr.AddAttribute(HtmlTextWriterAttribute.Style, "display: none")
                wr.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0")
                wr.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "2px")
                wr.RenderBeginTag(HtmlTextWriterTag.Table)
                wr.RenderBeginTag(HtmlTextWriterTag.Tbody)

                For intCount = 0 To (mObjectClasses.Count - 1)
                    Dim objectClassItem As ListItem = CType(mObjectClasses.Item(intCount), ListItem)
                    wr.RenderBeginTag(HtmlTextWriterTag.Tr)
                    wr.AddAttribute("val", objectClassItem.Value)
                    wr.RenderBeginTag(HtmlTextWriterTag.Td)
                    wr.Write(GetLocalizedItemText(objectClassItem))
                    wr.RenderEndTag() ' td 
                    wr.RenderEndTag() ' tr 
                Next

                wr.RenderEndTag() ' </tbody> 
                wr.RenderEndTag() ' </table> 
                wr.RenderEndTag() ' </span> 
                wr.RenderEndTag() ' </td>                

            Else ' Only one object to lookup

                Dim objectClassItem As ListItem = CType(mObjectClasses.Item(0), ListItem)
                wr.AddAttribute(HtmlTextWriterAttribute.Colspan, "2")
                wr.RenderBeginTag(HtmlTextWriterTag.Td)

                wr.AddAttribute(HtmlTextWriterAttribute.Id, "selObject")
                wr.AddAttribute(HtmlTextWriterAttribute.Type, "hidden")
                wr.AddAttribute("value", objectClassItem.Value)
                wr.RenderBeginTag(HtmlTextWriterTag.Input)
                wr.RenderEndTag() ' Input
               
                wr.Write("<span style=""font-weight: bold"">" & GetLocalizedItemText(objectClassItem) & "</span>")
                wr.RenderEndTag() ' TD

            End If

            wr.RenderEndTag() ' tr 
        End Sub

        Private Sub RenderSearchBox(ByVal wr As HtmlTextWriter)
            wr.RenderBeginTag(HtmlTextWriterTag.Tr)

            'wr.AddAttribute(HtmlTextWriterAttribute.Height, "25px")
            'wr.AddAttribute(HtmlTextWriterAttribute.Align, "right")
            wr.AddStyleAttribute("height", "25px")
            wr.AddStyleAttribute("text-align", "right")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            wr.RenderBeginTag(HtmlTextWriterTag.B)
            wr.Write(Localization.GetString("Search_Find", SharedResourceFile))
            wr.RenderEndTag() ' B
            wr.RenderEndTag() ' TD    

            'wr.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
            wr.AddStyleAttribute("width", "100%")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)

            wr.AddAttribute(HtmlTextWriterAttribute.Id, "findValue")
            wr.AddAttribute("onkeydown", "javascript:findValueKeyDown(event);")
            wr.AddAttribute(HtmlTextWriterAttribute.Type, "text")
            wr.AddAttribute(HtmlTextWriterAttribute.Tabindex, "2")
            wr.AddAttribute(HtmlTextWriterAttribute.Maxlength, "100")
            wr.AddAttribute(HtmlTextWriterAttribute.Style, "border-right: #7b9ebd 1px solid; border-top: #7b9ebd 1px solid; font-size: 11px; border-left: #7b9ebd 1px solid; width: 100%; border-bottom: #7b9ebd 1px solid")
            wr.RenderBeginTag(HtmlTextWriterTag.Input)

            wr.RenderEndTag() ' input  
            wr.RenderEndTag() ' td           

            wr.RenderBeginTag(HtmlTextWriterTag.Td)

            wr.AddAttribute(HtmlTextWriterAttribute.Id, "btnGo")
            wr.AddAttribute(HtmlTextWriterAttribute.Title, Localization.GetString("Search_Go_info", SharedResourceFile))
            wr.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled")
            wr.AddAttribute(HtmlTextWriterAttribute.Onclick, "javascript:search();")

            wr.AddAttribute(HtmlTextWriterAttribute.Tabindex, "3")
            wr.AddAttribute(HtmlTextWriterAttribute.Type, "button")
            wr.RenderBeginTag(HtmlTextWriterTag.Button)
            wr.WriteLine(Localization.GetString("Search_Go", SharedResourceFile))

            wr.RenderEndTag() ' button  
            wr.RenderEndTag() ' td

            wr.RenderEndTag() ' tr
        End Sub

        Private Sub RenderTableEnd(ByVal wr As HtmlTextWriter)

            wr.RenderEndTag() ' tbody
            wr.RenderEndTag() ' colgroup 
            wr.RenderEndTag() ' table     

        End Sub

        Public Overrides Sub Render(ByVal writer As HtmlTextWriter)

            RenderTableBegin(writer)
            RenderObjectClasses(writer)
            RenderLocations(writer)
            RenderSearchBox(writer)
            RenderTableEnd(writer)

        End Sub

    End Class 'PopupList

End Namespace