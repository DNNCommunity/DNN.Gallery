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
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.Security
Imports System.Threading
Imports System.Globalization

Imports DotNetNuke
Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Entities.Users
Imports DotNetNuke.Common
Imports DotNetNuke.Services.Localization


Namespace DotNetNuke.Modules.Gallery.PopupControls

  Public Class PopupPageBase
    Inherits System.Web.UI.Page

    ' This code is in large part a duplicate of that in DotNetNuke.Framework.PageBase. Because the AJAX.AddScriptManager
    ' method requires that the page have a form element when attempting to inject an AJAX ScriptManager, we cannot inherit
    ' from DotNetNuke.Framework.PageBase as we would have liked.

#Region "Private Members"
    Private _localResourceFile As String
    Private _PageCulture As CultureInfo = Nothing
#End Region

#Region "Public Properties"
    Public ReadOnly Property PortalSettings() As PortalSettings
      Get
        PortalSettings = PortalController.GetCurrentPortalSettings
      End Get
    End Property

    Public ReadOnly Property PageCulture() As CultureInfo
      Get
        If _PageCulture Is Nothing Then
          _PageCulture = Localization.GetPageLocale(PortalSettings)
        End If
        Return _PageCulture
      End Get
    End Property

    Public Property LocalResourceFile() As String
      Get
        Dim fileRoot As String
        Dim page As String() = Request.ServerVariables("SCRIPT_NAME").Split("/"c)

        If _localResourceFile = "" Then
          fileRoot = Me.TemplateSourceDirectory & "/" & Services.Localization.Localization.LocalResourceDirectory & "/" & page(page.GetUpperBound(0)) & ".resx"
        Else
          fileRoot = _localResourceFile
        End If
        Return fileRoot
      End Get
      Set(ByVal Value As String)
        _localResourceFile = Value
      End Set
    End Property

#End Region

    Protected Overrides Sub OnInit(ByVal e As System.EventArgs)
      MyBase.OnInit(e)
      If HttpContext.Current.Request IsNot Nothing AndAlso Not HttpContext.Current.Request.Url.LocalPath.ToLower.EndsWith("installwizard.aspx") Then
        ' Set the current culture
        Thread.CurrentThread.CurrentUICulture = PageCulture
        Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture
      End If
    End Sub
  End Class

  Public Class PopupControl
    Inherits WebControl
    'Inherits DotNetNuke.Framework.PageBase

    Implements INamingContainer
    Private mInitialised As Boolean
    Private mPopupBaseObject As PopupObject 'PopupBaseObject

    Private mPortalID As Integer
    Private mTabID As Integer
    Private mModuleID As Integer
    Private mTargetID As Integer
    Private mObjectClass As ObjectClass
    Private mObjectName As String
    Private mLocation As String = ""
    Private mSearchValue As String = ""
    Private mDataSource As String = ""

    Public Sub New()
    End Sub 'New

    Protected Overridable Sub Initialise()
      mInitialised = True
    End Sub 'Initialise

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
      Initialise()
    End Sub 'OnLoad

    Protected Overrides Sub EnsureChildControls()
      If Not mInitialised Then
        Initialise()
      End If
      MyBase.EnsureChildControls()
    End Sub 'EnsureChildControls

    Protected Overridable Sub CreateObject()
    End Sub 'CreateObject

    Protected Overrides Sub CreateChildControls()
      CreateObject()
      If Not (mPopupBaseObject Is Nothing) Then
        mPopupBaseObject.CreateChildControls()
      End If
    End Sub 'CreateChildControls

    Protected Overrides Sub OnPreRender(ByVal e As System.EventArgs)
      If Not (mPopupBaseObject Is Nothing) Then
        mPopupBaseObject.OnPreRender()
      End If
    End Sub 'OnPreRender

    Protected Overrides Sub Render(ByVal writer As System.Web.UI.HtmlTextWriter)
      If Not (mPopupBaseObject Is Nothing) Then
        mPopupBaseObject.Render(writer)
      End If
    End Sub 'Render 

    Public Property PortalID() As Integer
      Get
        Return mPortalID
      End Get
      Set(ByVal Value As Integer)
        mPortalID = Value
      End Set
    End Property

    Public Property TabID() As Integer
      Get
        Return mTabID
      End Get
      Set(ByVal Value As Integer)
        mTabID = Value
      End Set
    End Property

    Public Property ModuleID() As Integer
      Get
        Return mModuleID
      End Get
      Set(ByVal Value As Integer)
        mModuleID = Value
      End Set
    End Property
    Public Property TargetID() As Integer
      Get
        Return mTargetID
      End Get
      Set(ByVal Value As Integer)
        mTargetID = Value
      End Set
    End Property

    Public Property ObjectClass() As ObjectClass
      Get
        Return mObjectClass
      End Get
      Set(ByVal Value As ObjectClass)  'Configuration.ObjectClass)
        mObjectClass = Value
      End Set
    End Property

    Public Property ObjectName() As String 'Configuration.ObjectName
      Get
        Return mObjectName
      End Get
      Set(ByVal Value As String) 'Configuration.ObjectClass)
        mObjectName = Value
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

    Public Property SearchValue() As String
      Get
        Return mSearchValue
      End Get
      Set(ByVal Value As String)
        mSearchValue = Value
      End Set
    End Property

    Public Property DataSource() As String
      Get
        Return mDataSource
      End Get
      Set(ByVal Value As String)
        mDataSource = Value
      End Set
    End Property

    Public ReadOnly Property LoggedOnUserID() As Integer
      Get
        Return UserController.GetCurrentUserInfo.UserID
      End Get
    End Property

    Protected Property PopupBaseObject() As PopupObject 'PopupBaseObject
      Get
        Return mPopupBaseObject
      End Get
      Set(ByVal Value As PopupObject) 'PopupBaseObject)
        mPopupBaseObject = Value
      End Set
    End Property
  End Class

  Public Class PopupObject
    Private mPopupControl As PopupControl
    Private mSearchObjects As New ArrayList

    Public Sub New(ByVal PopupData As PopupData)
      mPopupControl = PopupData
    End Sub 'New

    Public Sub New(ByVal PopupSearch As PopupSearch)
      mPopupControl = PopupSearch
    End Sub 'New

    Protected ReadOnly Property PopupDataControl() As PopupData
      Get
        Return CType(mPopupControl, PopupData)
      End Get
    End Property

    Protected ReadOnly Property PopupSearchControl() As PopupSearch
      Get
        Return CType(mPopupControl, PopupSearch)
      End Get
    End Property

    Public Overridable Sub CreateChildControls()
    End Sub 'CreateChildControls

    Public Overridable Sub OnPreRender()
    End Sub 'OnPreRender

    Public Overridable Sub Render(ByVal writer As HtmlTextWriter)
    End Sub 'Render

    Public Property SearchObjects() As ArrayList
      Get
        Return mSearchObjects
      End Get
      Set(ByVal Value As ArrayList)
        mSearchObjects = Value
      End Set
    End Property
  End Class

  Public Class PopupData
    Inherits PopupControl
    'Implements INamingContainer 'ToDo: Add Implements Clauses for implementation methods of these interface(s)
    Private mListItems As New ArrayList

    Public Sub New()
      MyBase.New()
    End Sub 'New

    Protected Overrides Sub CreateObject()

      Dim intClass As Integer = 1     ' default value is user
      Dim strLocation As String = ""
      Dim strSearchValue As String = ""
      Dim strDataSource As String = "sql"
      Dim intDisplayMethod As Integer = 1 ' default value is display_name

      Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
      PortalID = _portalSettings.PortalId

      If Not Page.Request.QueryString("mid") Is Nothing Then
        ModuleID = Integer.Parse(Page.Request.QueryString("mid"))
      End If

      If Not Page.Request.QueryString("objectclass") Is Nothing Then
        intClass = CType([Enum].Parse(GetType(ObjectClass), Page.Request.QueryString("objectclass")), ObjectClass)
      Else
        If Not Page.Request.QueryString("objectclasses") Is Nothing Then
          Dim strClass As String() = Split(Page.Request.QueryString("objectclasses"), ",", , CompareMethod.Text)
          'If multi type get the first one
          intClass = Int16.Parse(strClass(0))
        End If
      End If

      Me.ObjectClass = CType(intClass, ObjectClass)

      If Not Page.Request.QueryString("location") Is Nothing Then
        strLocation = Page.Request.QueryString("location")
      End If
      Me.Location = strLocation

      If Not Page.Request.QueryString("searchvalue") Is Nothing Then
        strSearchValue = Page.Request.QueryString("searchvalue")
      End If
      Me.SearchValue = strSearchValue

      If Not Page.Request.QueryString("displaymethod") Is Nothing Then
        intDisplayMethod = Int16.Parse(Page.Request.QueryString("displaymethod"))
      End If

      PopupBaseObject = New PopupGalleryData(Me)

    End Sub

    Protected Overrides Sub Initialise()
      MyBase.Initialise()

    End Sub

    Protected Overrides Sub CreateChildControls()
      MyBase.CreateChildControls()
    End Sub

    Public Property ListItems() As ArrayList
      Get
        Return mListItems
      End Get
      Set(ByVal Value As ArrayList)
        mListItems = Value
      End Set
    End Property

  End Class 'PopupData



  Public Class PopupListItem
    Private mId As String
    Private mText As String
    Private mClass As Integer 'Configuration.ObjectClass
    Private mLevel As Integer
    Private mImage As String
    Private mAdditionalProperties As New ArrayList
    Private mPropertyLookup As New Hashtable

    Sub New()
    End Sub

    Sub New(ByVal Id As String, ByVal ObjectClass As Integer, ByVal Text As String, ByVal Image As String, ByVal Level As Integer)
      mId = Id
      mClass = ObjectClass
      mText = Text
      mImage = Image
      mLevel = Level
    End Sub

    Public Sub AddProperty(ByVal Name As String, ByVal Value As String, ByVal Size As Integer)
      Dim prop As New AdditionalProperty
      Dim index As Integer
      Try
        prop.SetProperty(Name, Value, Size)
        index = mAdditionalProperties.Add(prop)
        mPropertyLookup.Add(Name, index)
      Catch ex As Exception
        'Throw ex
      End Try

    End Sub

    Public Function GetProperty(ByVal index As Integer) As AdditionalProperty
      Try
        Dim prop As Object
        prop = mAdditionalProperties.Item(index)
        Return CType(prop, AdditionalProperty)

      Catch Exc As System.Exception
        Return Nothing
      End Try
    End Function

    Public Function GetProperty(ByVal Name As String) As AdditionalProperty
      Dim index As Integer
      Dim prop As Object

      ' Do validation first
      Try
        If mPropertyLookup.Item(Name) Is Nothing Then
          Return Nothing
        End If
      Catch ex As Exception
        Return Nothing
      End Try

      index = CInt(mPropertyLookup.Item(Name))
      prop = mAdditionalProperties.Item(index)

      Return CType(prop, AdditionalProperty)
    End Function

    ' Additional columns display in result list for more info
    Public Structure AdditionalProperty
      Friend mName As String
      Friend mValue As String
      Friend mSize As Integer

      Friend Sub SetProperty(ByVal Name As String, ByVal Value As String, ByVal Size As Integer)
        mName = Name
        mValue = Value
        mSize = Size
      End Sub

      Public ReadOnly Property Name() As String
        Get
          Return mName
        End Get
      End Property

      Public ReadOnly Property Value() As String
        Get
          Return mValue
        End Get
      End Property

      Public ReadOnly Property Size() As Integer
        Get
          Return mSize
        End Get
      End Property

    End Structure

    Public ReadOnly Property DisplayString() As String
      Get
        Dim sb As New StringBuilder
        Dim intCount As Integer
        For intCount = 1 To mLevel
          sb.Append("&nbsp;&nbsp;")
        Next
        'sb.Append("<img src=")
        'sb.Append(Config.ImageURL & mImage)
        sb.Append(mImage)
        'sb.Append(" />")
        sb.Append(mText)

        Return sb.ToString
      End Get
    End Property

    Public ReadOnly Property Id() As String
      Get
        Return mId
      End Get
    End Property

    Public ReadOnly Property Text() As String
      Get
        Return mText
      End Get
    End Property

    Public ReadOnly Property ObjectClass() As Integer 'Configuration.ObjectClass
      Get
        Return Me.mClass
      End Get
    End Property

    Public ReadOnly Property Image() As String
      Get
        Return mImage
      End Get
    End Property

    Public ReadOnly Property AdditionalProperties() As ArrayList
      Get
        Return mAdditionalProperties
      End Get
    End Property

  End Class


  Public Class RenderPopup

    Private mPopup As PopupData

    Sub New()
    End Sub

    Sub New(ByVal Popup As PopupData)
      mPopup = Popup
    End Sub

    Private Sub RenderTableBegin(ByVal wr As HtmlTextWriter)
      wr.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0")
      wr.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0")
      wr.AddStyleAttribute("height", "100%")
      wr.AddStyleAttribute("width", "100%")
      wr.RenderBeginTag(HtmlTextWriterTag.Table)

    End Sub

    Private Sub RenderListHeader(ByVal wr As HtmlTextWriter)
      Dim imgURL As String = AddHTTP(GetDomainName(HttpContext.Current.Request)) & "/DesktopModules/Gallery/Popup/Images/" 'ADSIConfig.PopupImageURL

      If Not (mPopup.ListItems.Count = 0) Then

        wr.RenderBeginTag(HtmlTextWriterTag.Tr)

        wr.AddStyleAttribute("height", "10px")
        wr.RenderBeginTag(HtmlTextWriterTag.Td)

        wr.AddAttribute(HtmlTextWriterAttribute.Class, "ListHeader")
        wr.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0")
        wr.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0")
        wr.AddStyleAttribute("table-layout", "fixed")
        wr.AddStyleAttribute("width", "100%")
        wr.RenderBeginTag(HtmlTextWriterTag.Table)

        wr.RenderBeginTag(HtmlTextWriterTag.Tr)

        wr.AddAttribute(HtmlTextWriterAttribute.Class, "Column")
        wr.RenderBeginTag(HtmlTextWriterTag.Td)

        wr.Write("&nbsp;" & (Localization.GetString("Column_" & "Name", ApplicationPath & "/DesktopModules/Gallery" & "/App_LocalResources/SharedResources.resx")))
        wr.RenderEndTag() ' TD

        ' Check to see how many columns should be displayed by getting a listitem from items list
        Dim listItem As PopupListItem = CType(mPopup.ListItems.Item(0), PopupListItem)
        Dim additionalProp As PopupListItem.AdditionalProperty
        For Each additionalProp In listItem.AdditionalProperties
          wr.AddAttribute(HtmlTextWriterAttribute.Class, "Column")
          wr.AddStyleAttribute("width", (additionalProp.Size + 25).ToString & "px")
          wr.RenderBeginTag(HtmlTextWriterTag.Td)
          wr.Write("<img alt='' style=""vertical-align: middle"" src=""" & imgURL & "sp_Barline.gif""  />")
          wr.Write("&nbsp;" & (Localization.GetString("Column_" & additionalProp.Name, ApplicationPath & "/DesktopModules/Gallery" & "/App_LocalResources/SharedResources.resx")))
          wr.RenderEndTag() ' td
        Next

        wr.RenderEndTag() ' tr
        wr.RenderEndTag() ' table
        wr.RenderEndTag() ' td
        wr.RenderEndTag() ' tr
      End If
    End Sub

    Private Sub RenderPopupData(ByVal wr As HtmlTextWriter)

      wr.RenderBeginTag(HtmlTextWriterTag.Tr)
      wr.RenderBeginTag(HtmlTextWriterTag.Td)

      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Objects")
      wr.RenderBeginTag(HtmlTextWriterTag.Div)

      wr.AddAttribute("onkeydown", "javascript:listKeyDown(this,event);")
      wr.AddAttribute("ondblclick", "javascript:itemDoubleClick(event);")
      wr.AddAttribute(HtmlTextWriterAttribute.Onclick, "javascript:clickItem(this,event);")

      wr.AddAttribute(HtmlTextWriterAttribute.Id, "tblResults")
      wr.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0")
      wr.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "2px")
      wr.AddStyleAttribute("table-layout", "fixed")
      wr.AddStyleAttribute("width", "100%")
      wr.RenderBeginTag(HtmlTextWriterTag.Table)

      wr.RenderBeginTag(HtmlTextWriterTag.Colgroup)

      wr.AddAttribute(HtmlTextWriterAttribute.Name, "Name")
      wr.AddAttribute(HtmlTextWriterAttribute.Class, "Row")
      wr.RenderBeginTag(HtmlTextWriterTag.Col)
      wr.RenderEndTag() ' Col


      If (Not mPopup.ListItems.Count = 0) Then
        Dim baseItem As PopupListItem = CType(mPopup.ListItems.Item(0), PopupListItem)
        Dim additionalProp As PopupListItem.AdditionalProperty
        For Each additionalProp In baseItem.AdditionalProperties

          wr.AddAttribute(HtmlTextWriterAttribute.Name, additionalProp.Name)
          wr.AddStyleAttribute("width", additionalProp.Size.ToString & "px")
          wr.AddAttribute(HtmlTextWriterAttribute.Class, "Row")
          wr.RenderBeginTag(HtmlTextWriterTag.Col)
          wr.RenderEndTag() ' Col    

        Next
      ElseIf mPopup.ListItems.Count = 0 Then
        wr.AddStyleAttribute("height", "25%")
        wr.AddStyleAttribute("width", "100%")
        wr.AddAttribute(HtmlTextWriterAttribute.Id, "tblInfo")
        wr.AddAttribute(HtmlTextWriterAttribute.Class, "Message")
        wr.RenderBeginTag(HtmlTextWriterTag.Table)

        wr.RenderBeginTag(HtmlTextWriterTag.Tr)

        wr.AddStyleAttribute("text-align", "center")
        wr.AddAttribute(HtmlTextWriterAttribute.Class, "Message")
        wr.RenderBeginTag(HtmlTextWriterTag.Td)
        wr.Write(Localization.GetString("NoRecordsFound", ApplicationPath & "/DesktopModules/Gallery" & "/App_LocalResources/SharedResources.resx"))

        wr.RenderEndTag() ' td
        wr.RenderEndTag() ' tr
        wr.RenderEndTag() ' table
      End If

      wr.RenderBeginTag(HtmlTextWriterTag.Tbody)

      Dim listItem As PopupListItem
      For Each listItem In mPopup.ListItems
        Dim sId As String = listItem.Id 'entry.NativeGuid
        Dim sText As String = listItem.Text 'Tools.ConvertToCanonical(entry.Name, False)
        Dim sClass As String = CInt(listItem.ObjectClass).ToString
        Dim sImage As String = listItem.Image
        Dim sLocation As String = listItem.GetProperty("Location").Value

        wr.RenderBeginTag(HtmlTextWriterTag.Tr)

        wr.AddAttribute(HtmlTextWriterAttribute.Class, "Select")
        wr.AddAttribute(HtmlTextWriterAttribute.Nowrap, "nowrap")
        wr.RenderBeginTag(HtmlTextWriterTag.Td)

        ' image
        wr.AddAttribute(HtmlTextWriterAttribute.Border, "0")
        wr.AddAttribute(HtmlTextWriterAttribute.Src, sImage)
        wr.AddAttribute(HtmlTextWriterAttribute.Alt, "")
        wr.RenderBeginTag(HtmlTextWriterTag.Img)
        wr.RenderEndTag() ' Img

        wr.Write("&nbsp;")
        wr.Write(sText)

        wr.AddAttribute(HtmlTextWriterAttribute.Type, "hidden")
        wr.AddAttribute(HtmlTextWriterAttribute.Name, "oname")
        wr.AddAttribute(HtmlTextWriterAttribute.Value, "" + sText)
        wr.RenderBeginTag(HtmlTextWriterTag.Input)
        wr.RenderEndTag() ' Input

        wr.AddAttribute(HtmlTextWriterAttribute.Type, "hidden")
        wr.AddAttribute(HtmlTextWriterAttribute.Name, "otype")
        wr.AddAttribute(HtmlTextWriterAttribute.Value, "" + sClass)
        wr.RenderBeginTag(HtmlTextWriterTag.Input)
        wr.RenderEndTag() ' Input

        wr.AddAttribute(HtmlTextWriterAttribute.Type, "hidden")
        wr.AddAttribute(HtmlTextWriterAttribute.Name, "oid")
        wr.AddAttribute(HtmlTextWriterAttribute.Value, "" + sId)
        wr.RenderBeginTag(HtmlTextWriterTag.Input)
        wr.RenderEndTag() ' Input

        wr.AddAttribute(HtmlTextWriterAttribute.Type, "hidden")
        wr.AddAttribute(HtmlTextWriterAttribute.Name, "olocation")
        wr.AddAttribute(HtmlTextWriterAttribute.Value, "" + sLocation)
        wr.RenderBeginTag(HtmlTextWriterTag.Input)
        wr.RenderEndTag() ' Input

        wr.RenderEndTag() ' Td


        Dim additionalProp As PopupListItem.AdditionalProperty
        For Each additionalProp In listItem.AdditionalProperties
          wr.AddAttribute(HtmlTextWriterAttribute.Class, "Row")
          wr.AddAttribute(HtmlTextWriterAttribute.Nowrap, "nowrap")
          wr.AddStyleAttribute("width", additionalProp.Size.ToString & "px")
          wr.RenderBeginTag(HtmlTextWriterTag.Td)
          wr.Write("&nbsp;")
          wr.Write(additionalProp.Value)
          wr.RenderEndTag() ' Td
        Next
        'End If

        wr.RenderEndTag()   ' Tr
      Next

      wr.RenderEndTag()   ' tbody
      wr.RenderEndTag()   ' colgroup
      wr.RenderEndTag()   ' table            

      wr.RenderEndTag()   ' div
      wr.RenderEndTag()   ' td
      wr.RenderEndTag()   ' tr

    End Sub 'RenderPopup

    Private Sub RenderTableEnd(ByVal wr As HtmlTextWriter)
      '</table>
      wr.RenderEndTag()   ' table

    End Sub

    Public Sub Render(ByVal wr As HtmlTextWriter)

      RenderTableBegin(wr)
      RenderListHeader(wr)
      RenderPopupData(wr)
      RenderTableEnd(wr)
    End Sub
  End Class

End Namespace