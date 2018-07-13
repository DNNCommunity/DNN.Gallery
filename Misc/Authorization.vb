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
Imports System.Collections.Specialized
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Entities.Users
Imports DotNetNuke.Security
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.Common.Utilities
Imports System.IO

Namespace DotNetNuke.Modules.Gallery

  Public Class Authorization

    Private mPortalSettings As PortalSettings
    Private mModuleSettings As ModuleInfo
    Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config
    Private mLoggedOnUserId As Integer = -1
    Private mLoggedOnUserName As String = ""
    Private mIsAuthenticated As Boolean = False

    Public Sub New(ByVal ModuleSettings As ModuleInfo)
      mPortalSettings = PortalController.GetCurrentPortalSettings
      mModuleSettings = ModuleSettings

      mGalleryConfig = Config.GetGalleryConfig(mModuleSettings.ModuleID)

      If HttpContext.Current.Request.IsAuthenticated Then
        mIsAuthenticated = True
      End If
      Dim objUser As UserInfo = UserController.GetCurrentUserInfo
      mLoggedOnUserId = objUser.UserID 'UserController.GetCurrentUserInfo.UserID
      mLoggedOnUserName = objUser.Username
    End Sub

    Public Sub New(ByVal ModuleId As Integer)
      mPortalSettings = PortalController.GetCurrentPortalSettings

      Dim objModuleController As New ModuleController
      mModuleSettings = objModuleController.GetModule(ModuleId, mPortalSettings.ActiveTab.TabID)

      mGalleryConfig = Config.GetGalleryConfig(ModuleId)

      If HttpContext.Current.Request.IsAuthenticated Then
        mIsAuthenticated = True
      End If

      Dim objUser As UserInfo = UserController.GetCurrentUserInfo
      mLoggedOnUserId = objUser.UserID
      mLoggedOnUserName = objUser.Username
    End Sub

    Public ReadOnly Property GalleryConfig() As DotNetNuke.Modules.Gallery.Config
      Get
        Return mGalleryConfig
      End Get
    End Property

    Public ReadOnly Property LoggedOnUserId() As Integer
      Get
        Return mLoggedOnUserId
      End Get
    End Property

    Public ReadOnly Property LoggedOnUserName() As String
      Get
        Return mLoggedOnUserName
      End Get
    End Property

    Public ReadOnly Property IsAuthenticated() As Boolean
      Get
        Return mIsAuthenticated
      End Get
    End Property

    Public Function HasEditPermission() As Boolean
      Return HasUploadPermission() And IsAuthenticated
    End Function

    ' Created by Andrew Galbraith Ryer to allow unauthenticated users to upload but not edit the gallery.
    Public Function HasUploadPermission() As Boolean
      If GalleryConfig.IsPrivate Then
        If HasAdminPermission() OrElse IsGalleryOwner() Then
          Return True
        End If
      Else
        Return Permissions.ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "EDIT", mModuleSettings)
      End If
      Return False
    End Function

    Public Function HasItemEditPermission(ByVal DataItem As Object) As Boolean
      Return HasItemUploadPermission(DataItem) And IsAuthenticated
    End Function

    Public Function HasItemUploadPermission(ByVal DataItem As Object) As Boolean
      If GalleryConfig.IsPrivate Then
        If IsGalleryOwner() OrElse CType(DataItem, IGalleryObjectInfo).OwnerID = LoggedOnUserId Then
          Return True
        End If
      Else
        Return Permissions.ModulePermissionController.CanEditModuleContent(mModuleSettings)
      End If
      Return False

    End Function

    Public Function IsGalleryOwner() As Boolean
      If Not IsAuthenticated Then
        Return False
      Else
        If HasAdminPermission() OrElse mGalleryConfig.OwnerID = mLoggedOnUserId Then
          Return True
        End If
        Return False
      End If
    End Function

    Public Function IsItemOwner(ByVal DataItem As Object) As Boolean
      If mGalleryConfig.IsPrivate Then
        If IsGalleryOwner() OrElse CType(DataItem, IGalleryObjectInfo).OwnerID = LoggedOnUserId Then
          Return True
        End If
      Else
        If Me.HasUploadPermission Then
          Return True
        End If
      End If
      Return False
    End Function

    Public Function ItemCanEdit(ByVal DataItem As Object) As Boolean
      Return HasItemEditPermission(DataItem)
    End Function

    'William Severance - Added new authorization test

    Public Function HasItemApprovalPermission(ByVal DataItem As Object) As Boolean
      If mGalleryConfig.IsPrivate Then
        If IsGalleryOwner() OrElse ((Not DataItem Is Nothing) AndAlso CType(DataItem, IGalleryObjectInfo).OwnerID = LoggedOnUserId) Then
          Return True
        End If
      Else
        Return HasAdminPermission()
      End If
      Return False

    End Function

    Public Function ItemIsApproved(ByVal DataItem As Object) As Boolean
      If (CType(DataItem, IGalleryObjectInfo).ApprovedDate <= DateTime.Today) OrElse (GalleryConfig.AutoApproval) Then
        Return True
      End If
      Return False
    End Function

    Public Function ItemCanSlideshow(ByVal DataItem As Object) As Boolean
      If mGalleryConfig.AllowSlideshow Then
        If CType(DataItem, IGalleryObjectInfo).IsFolder Then
          Return CType(DataItem, GalleryFolder).BrowsableItems.Count > 0
        Else
          Return (CType(DataItem, GalleryFile).Parent.BrowsableItems.Count > 0 AndAlso CType(DataItem, GalleryFile).Type = Config.ItemType.Image)
        End If
      End If
      Return False
    End Function

    Public Function ItemCanDownload(ByVal DataItem As Object) As Boolean
      If Not CType(DataItem, IGalleryObjectInfo).IsFolder _
      AndAlso mGalleryConfig.AllowDownload _
      AndAlso (mGalleryConfig.HasDownloadPermission OrElse IsItemOwner(DataItem)) Then
        Return True
      End If
      Return False
    End Function

    Public Function ItemCanVote(ByVal DataItem As Object) As Boolean
      If Not CType(DataItem, IGalleryObjectInfo).IsFolder _
      AndAlso mGalleryConfig.AllowVoting Then
        Return True
      End If
      Return False

    End Function

    Public Function ItemIsValidImage(ByVal DataItem As Object) As Boolean
      If CType(DataItem, IGalleryObjectInfo).Type = Config.ItemType.Image Then
        Return True
      End If
    End Function

    Public Function ItemCanViewExif(ByVal DataItem As Object) As Boolean
      If ItemIsValidImage(DataItem) AndAlso mGalleryConfig.AllowExif Then
        Return True
      End If
      Return False
    End Function

    Public Function ItemCanBrowse(ByVal DataItem As Object) As Boolean 'for item
      If CType(DataItem, IGalleryObjectInfo).IsFolder Then
        If CType(DataItem, GalleryFolder).BrowsableItems.Count > 0 Then
          Return True
        End If
      End If
    End Function

    Public Function HasAdminPermission() As Boolean
      Return Permissions.ModulePermissionController.CanAdminModule(mModuleSettings)
    End Function

    ' Called to test view permission on popup pages.
    Public Function HasViewPermission() As Boolean
      Return Permissions.ModulePermissionController.CanViewModule(mModuleSettings)
    End Function

  End Class

End Namespace
