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
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.Security
Imports System.Text
Imports System.Configuration
Imports System.Collections.Specialized
Imports System.Reflection
Imports DotNetNuke
Imports DotNetNuke.Entities.Users
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.Security.Roles
Imports DotNetNuke.Services.Exceptions

Namespace DotNetNuke.Modules.Gallery.PopupControls

  Public Class PopupGalleryData
    Inherits PopupObject

    Dim mImageUrl As String

    Public Sub New(ByVal Popup As PopupData)
      MyBase.New(Popup)

      Dim GalleryConfig As Config = DotNetNuke.Modules.Gallery.Config.GetGalleryConfig(Popup.ModuleID)
      Dim imgURL As String = AddHTTP(GetDomainName(HttpContext.Current.Request)) & "/DesktopModules/Gallery/Popup/Images/" 'ADSIConfig.PopupImageURL
      Dim searchResult As New ArrayList
      Dim searchString As String = Popup.SearchValue
      Dim searchLocation As String = Popup.Location
      Dim sID As Integer
      Dim sName As String = ""
      Dim popupItem As PopupListItem

      Try
        Select Case Popup.ObjectClass

          Case ObjectClass.DNNRole
            mImageUrl = imgURL & "sp_team.gif"

            Dim ctlRole As New RoleController
            searchResult = ctlRole.GetPortalRoles(Popup.PortalID)

            ' JIMJ Add role glbRoleAllUsersName
            Dim R As New RoleInfo()
            R.RoleID = CInt(glbRoleAllUsers)
            R.RoleName = glbRoleAllUsersName
            searchResult.Add(R)

            Dim role As RoleInfo
            For Each role In searchResult
              If role.RoleName.ToLower.StartsWith(searchString.ToLower) OrElse (searchString.Length = 0) Then
                sID = role.RoleID
                sName = role.RoleName
                popupItem = New PopupListItem(sID.ToString, ObjectClass.DNNRole, sName, mImageUrl, 1)
                Popup.ListItems.Add(popupItem)
              End If
            Next

          Case ObjectClass.DNNUser
            mImageUrl = imgURL & "sp_user.gif"
            searchResult = UserController.GetUsers(Popup.PortalID)

            ' JIMJ Changes to add the names correctly
            Dim user As UserInfo
            For Each user In searchResult
              sID = user.UserID
              sName = user.Username

              ' See if we can find the user or if we want everything
              If (sName.ToLower.StartsWith(searchString.ToLower)) OrElse (searchString.Length = 0) Then
                popupItem = New PopupListItem(sID.ToString, ObjectClass.DNNUser, user.Username, mImageUrl, 1)

                popupItem.AddProperty("Full Name", user.Profile.FullName, 200)
                Popup.ListItems.Add(popupItem)
              End If
            Next

        End Select

      Catch exc As Exception
        LogException(exc)
      End Try

    End Sub

    Public Overrides Sub Render(ByVal writer As HtmlTextWriter)
      Dim objRender As RenderPopup = New RenderPopup(Me.PopupDataControl)
      objRender.Render(writer)
    End Sub

    Public Overrides Sub CreateChildControls()
    End Sub 'CreateChildControls

    Public Overrides Sub OnPreRender()
    End Sub

  End Class 'PopupList

End Namespace