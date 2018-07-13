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
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.HtmlControls
Imports System.Text
Imports DotNetNuke
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.Modules.Gallery.Config
Imports DotNetNuke.Modules.Gallery.Utils
Imports DotNetNuke.Services.Localization


Namespace DotNetNuke.Modules.Gallery.WebControls

    Public MustInherit Class LookupControl
        Inherits System.Web.UI.UserControl

        'Private mItemClass As .Gallery.ObjectClass
        'Private mLookupSingle As Boolean = False
        'Private mLocations As String = ""
        'Private mImage As String = "sp_forum.gif"
        'Protected WithEvents txtItemsID As System.Web.UI.WebControls.TextBox
        'Protected WithEvents txtInputItemsID As System.Web.UI.WebControls.TextBox
        'Protected WithEvents rowLookup As System.Web.UI.HtmlControls.HtmlTableRow
        'Protected WithEvents celLookupData As System.Web.UI.HtmlControls.HtmlTableCell
        'Protected WithEvents tblLookup As System.Web.UI.HtmlControls.HtmlTable
        'Protected WithEvents celLookupItems As System.Web.UI.HtmlControls.HtmlTableCell

        'Localisation added by M. Schlomann/William Severance
        Public AddText As String = ""
        Public RemoveText As String = ""
        Public notSpecified As String = ""
        Private mLocalResourceFile As String = Nothing

#Region " Web Form Designer Generated Code "

        'This call is required by the Web Form Designer.
        <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

        End Sub

        Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
            'CODEGEN: This method call is required by the Web Form Designer
            'Do not modify it using the code editor.
            InitializeComponent()
        End Sub

#End Region

        'William Severance - Added property to provide localization
        Public Property LocalResourceFile() As String
            Get
                If mLocalResourceFile Is Nothing Then
                    mLocalResourceFile = Localization.GetResourceFile(Me, "ControlLookup.ascx")
                End If
                Return mLocalResourceFile
            End Get
            Set(ByVal value As String)
                mLocalResourceFile = value
            End Set
        End Property

        Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
            Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings

            ' Localisation Added by M. Schlomann/William Severance
            AddText = Localization.GetString("Add", LocalResourceFile)
            notSpecified = DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(Localization.GetString("notSpecified", LocalResourceFile))
            RemoveText = Localization.GetString("Remove", LocalResourceFile)
            If TypeOf Me.BindingContainer Is PortalModuleBase AndAlso ModuleID = 0 Then
                ModuleID = CType(Me.NamingContainer, PortalModuleBase).ModuleId
            End If

            ' JIMJ try looking up the module id from the querystring
            If ModuleID = 0 AndAlso Not Request.QueryString("mid") Is Nothing Then
                ModuleID = Integer.Parse(Request.QueryString("mid"))
            End If

            Dim lstItems As String = txtItemsID.Value 'txtItemsID.Text
            Dim itemExists As Boolean = False
            'lstItems =

            celLookupItems.InnerHtml = ""
            For Each item As String In lstItems.Split(";"c)
                If Not item.Length = 0 Then
                    Dim itemID As String = Left(item, item.IndexOf(":"c))
                    Dim itemName As String = Right(item, item.Length - (item.IndexOf(":"c) + 1))
                    RenderItems(itemID, itemName)
                    itemExists = True
                    'lblitemName = (Localization.GetString(itemName, Request.QueryString("itemName") & "Add", ApplicationPath & "/DesktopModules/Gallery/Controls" & "/App_LocalResources/ControlLookup.ascx.resx")))
                End If
            Next

            If Not itemExists Then
                celLookupItems.InnerHtml = "<span class='normaltextbox'>" + notSpecified + "</span>"
            Else
                If LookupSingle Then
                    'Me.btnRemove.Visible = True
                End If
            End If
            
        End Sub

        Private Sub BindItems()

        End Sub

        Private Sub RenderItems(ByVal ItemID As String, ByVal ItemName As String)
            Dim imgURL As String = ""
            If Image.Length > 0 Then
                imgURL = AddHTTP(GetDomainName(HttpContext.Current.Request)) & "/DesktopModules/Gallery/Popup/Images/" & Image
            End If
            Dim sb As New StringBuilder


            sb.Append("<span class='normaltextbox'")
            sb.Append(" oid='" & ItemID & "'")
            sb.Append(" otype='" & ItemClass & "'")
            sb.Append(" olocation='>")
            'sb.Append(JSDecode(mLocations))
            sb.Append("'>")
            If Image.Length > 0 Then
                sb.Append("<img src=""")
                sb.Append(imgURL)
                sb.Append(""" alt='' border='0'/>&nbsp;")
            End If
            sb.Append(ItemName)
            sb.Append("</span><br/>")

            Dim litControl As New LiteralControl(sb.ToString)
            celLookupItems.Controls.Add(litControl)

        End Sub

        Protected Function ItemLookup() As String
            Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings

            If (Locations.Length > 0) AndAlso Not (Locations.StartsWith("&locations=")) Then
                Locations = JSEncode("&locations=" & Locations)
            End If

            Dim url As String
            Dim features As String
            If LookupSingle Then
                url = Page.ResolveUrl("DesktopModules/Gallery/Popup/PopupSelectSingle.aspx")
                features = "dialogHeight:340px;dialogWidth:480px;resizable:yes;center:yes;status:no;help:no;scroll:no;"
            Else
                url = Page.ResolveUrl("DesktopModules/Gallery/Popup/PopupSelectMulti.aspx")
                features = "dialogHeight:480px;dialogWidth:480px;resizable:yes;center:yes;status:no;help:no;scroll:no;"
            End If

            Dim sb As New System.Text.StringBuilder
            sb.Append("javascript:OpenGalleryPopup(")
            sb.Append("this")
            sb.Append(", '")
            sb.Append(url)
            sb.Append("?datasource=gallery")
            sb.Append(Locations)
            sb.Append("&objectclasses=")
            sb.Append(ItemClass)
            sb.Append("&tabid=")
            sb.Append(_portalSettings.ActiveTab.TabID.ToString)
            sb.Append("&mid=")
            sb.Append(ModuleID.ToString)
            sb.Append("', '")
            sb.Append(features)
            sb.Append("')")

            Return sb.ToString

        End Function

        'Protected Function ItemLookup0() As String
        '    Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
        '    If (Locations.Length > 0) AndAlso Not (Locations.StartsWith("&locations=")) Then
        '        Locations = "&locations=" & Locations
        '    End If

        '    Dim sb As New System.Text.StringBuilder
        '    sb.Append("javascript:OpenGalleryPopup(")
        '    sb.Append(ItemClass)
        '    sb.Append(", this")
        '    sb.Append(", '")
        '    sb.Append(JSEncode(Locations))
        '    sb.Append("&tabid=")
        '    sb.Append(_portalSettings.ActiveTab.TabID.ToString)
        '    sb.Append("&mid=")
        '    sb.Append(ModuleID.ToString)
        '    If LookupSingle Then
        '        sb.Append("', '")
        '        sb.Append("true")
        '    End If
        '    sb.Append("')")

        '    Return sb.ToString

        'End Function

        Protected Function CanRemove() As Boolean
            Return (LookupSingle AndAlso Me.txtItemsID.Value.Length > 0) 'Me.txtItemsID.Text.Length > 0)
        End Function

        Public Overloads Sub AddItem(ByVal ID As Integer, ByVal Name As String)
            txtItemsID.Value &= ";" & ID.ToString & ":" & Name 'txtItemsID.Text += ";" & ID.ToString & ":" & Name
            txtInputItemsID.Value &= ";" & ID.ToString

        End Sub

        Public Overloads Sub AddItem(ByVal ID As String, ByVal Name As String)
            txtItemsID.Value &= ";" & ID & ":" & Name 'txtItemsID.Text += ";" & ID & ":" & Name
            txtInputItemsID.Value &= ";" & ID
        End Sub

        Public ReadOnly Property RemovedItems() As String
            Get
                Dim strItems As String = txtItemsID.Value 'txtItemsID.Text
                Dim strReturn As String = ""
                Dim item As String
                If strItems.Length = 0 Then
                    strReturn = txtInputItemsID.Value
                Else
                    For Each item In txtInputItemsID.Value.Split(";"c)
                        If item.Length > 0 AndAlso strItems.IndexOf(";" & item & ":") < 0 Then
                            strReturn &= ";" & item
                        End If
                    Next
                End If
                Return strReturn
            End Get
        End Property

        Public ReadOnly Property AddedItems() As String
            Get
                Dim strItems As String = txtItemsID.Value 'txtItemsID.Text
                Dim strReturn As String = ""
                Dim item As String
                If txtInputItemsID.Value.Length = 0 Then
                    For Each item In strItems.Split(";"c)
                        If item.Length > 0 Then
                            strReturn &= ";" & Left(item, item.IndexOf(":"c))
                        End If
                    Next
                Else
                    For Each item In strItems.Split(";"c)
                        If item.Length > 0 AndAlso txtInputItemsID.Value.IndexOf(";" & Left(item, item.IndexOf(":"c))) < 0 Then
                            strReturn &= ";" & Left(item, item.IndexOf(":"c))
                        End If
                    Next
                End If
                Return strReturn
            End Get
        End Property

        Public ReadOnly Property ResultItems() As String
            Get
                Dim strItems As String = txtItemsID.Value 'txtItemsID.Text
                Dim strReturn As String = ""
                Dim item As String
                For Each item In strItems.Split(";"c)
                    If item.Length > 0 Then
                        strReturn &= ";" & Left(item, item.IndexOf(":"c))
                    End If
                Next
                Return strReturn
            End Get
        End Property

        Public Property Locations() As String
            Get
                Dim savedState As Object = ViewState("Locations")
                If savedState Is Nothing Then
                    Return ""
                Else
                    Return CType(savedState, String)
                End If
            End Get
            Set(ByVal Value As String)
                'mLocations = Value
                ViewState("Locations") = Value
            End Set
        End Property

        Public Property Image() As String
            Get
                'Return mImage
                Dim savedState As Object = ViewState("Image")
                If savedState Is Nothing Then
                    Return "sp_gallery.gif"
                Else
                    Return CType(savedState, String)
                End If
            End Get
            Set(ByVal Value As String)
                'mImage = Value
                ViewState("Image") = Value
            End Set
        End Property

        Public Property ItemClass() As DotNetNuke.Modules.Gallery.ObjectClass
            Get
                'Return mItemClass
                Dim savedState As Object = ViewState("ItemClass")
                If savedState Is Nothing Then
                    Return ObjectClass.DNNRole
                Else
                    Return CType(savedState, DotNetNuke.Modules.Gallery.ObjectClass)
                End If
            End Get
            Set(ByVal Value As DotNetNuke.Modules.Gallery.ObjectClass)
                ViewState("ItemClass") = Value
            End Set
        End Property

        Public Property LookupSingle() As Boolean
            Get
                Dim savedState As Object = ViewState("LookupSingle")
                If savedState Is Nothing Then
                    Return False
                Else
                    Return CType(savedState, Boolean)
                End If
            End Get
            Set(ByVal Value As Boolean)
                ViewState("LookupSingle") = Value
            End Set
        End Property

        Public Property ModuleID() As Integer
            Get
                Return CType(ViewState("ModuleID"), Integer)
            End Get
            Set(ByVal Value As Integer)
                ViewState("ModuleID") = Value
            End Set
        End Property

        'William Severance - Added to pass notSpecified localization and NamingContainer.ClientID to scripts
        Private Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
            Dim sb As New StringBuilder
            sb.Append("var notSpecified = '")
            sb.Append(notSpecified)
            sb.AppendLine("';")
            sb.Append("var namingContainer = '")
            sb.Append(Me.NamingContainer.ClientID)
            sb.AppendLine("';")
            If Not Page.ClientScript.IsClientScriptBlockRegistered(NamingContainer.GetType(), "ControlLookupVars") Then
                Page.ClientScript.RegisterClientScriptBlock(NamingContainer.GetType(), "ControlLookupVars", sb.ToString, True)
            End If
        End Sub
    End Class

End Namespace
