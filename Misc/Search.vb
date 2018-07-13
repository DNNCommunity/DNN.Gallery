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
Imports DotNetNuke
Imports DotNetNuke.Services.Search
Imports DotNetNuke.Entities.Modules

Namespace DotNetNuke.Modules.Gallery

  Public Class Controller
    Implements ISearchable
    Implements IUpgradeable

    Private Const MAX_DESCRIPTION_LENGTH As Integer = 100

    Public Function GetSearchItems(ByVal ModInfo As Entities.Modules.ModuleInfo) As Services.Search.SearchItemInfoCollection Implements Entities.Modules.ISearchable.GetSearchItems

      Dim SearchItemCollection As New SearchItemInfoCollection
      'Dim config As DotNetNuke.Modules.Gallery.Config = config.GetGalleryConfig(ModInfo.ModuleID)
      'PopulateSearch(config.RootFolder, SearchItemCollection)
      Return SearchItemCollection

    End Function

    ' When BuildCacheOnStart is set to true, this recursively populates the folder objects
    Public Shared Sub PopulateSearch(ByVal rootFolder As GalleryFolder, ByVal SearchCollection As SearchItemInfoCollection)
      Dim folder As Object

      If Not rootFolder.IsPopulated Then
        rootFolder.Populate(False)
        For Each item As IGalleryObjectInfo In rootFolder.List
          Dim strDescription As String = HtmlUtils.Shorten(HtmlUtils.Clean(item.Description, False), MAX_DESCRIPTION_LENGTH, "...")
          Dim SearchItem As SearchItemInfo = New SearchItemInfo(item.Title, item.ItemInfo, item.OwnerID, item.CreatedDate, rootFolder.GalleryConfig.ModuleID, item.Name, strDescription, item.ID)
          SearchCollection.Add(SearchItem)
        Next
      End If

      For Each folder In rootFolder.List
        If TypeOf folder Is GalleryFolder AndAlso Not CType(folder, GalleryFolder).IsPopulated Then
          CType(folder, GalleryFolder).Populate(False)
          PopulateSearch(CType(folder, GalleryFolder), SearchCollection)
        End If
      Next
    End Sub

    Public Function UpgradeModule(ByVal Version As String) As String Implements Entities.Modules.IUpgradeable.UpgradeModule
      Dim Result As String = Version
      If Version = "04.03.03" Then
        'Truncate the value of setting RootURL for all instances of the module to be relative to the portal's
        'home directory as fix to invalid RootURL when migrating site to a new url

        Dim dmi As DesktopModuleInfo = DesktopModuleController.GetDesktopModuleByModuleName("DNN_Gallery", -1)
        If dmi Is Nothing Then
          Result &= " - Upgrade module(v 04.03.03) failure - unable to obtain DesktopModuleID for module name DNN_Gallery"
        Else
          Dim mdi As Definitions.ModuleDefinitionInfo = Definitions.ModuleDefinitionController.GetModuleDefinitionByFriendlyName(dmi.FriendlyName, dmi.DesktopModuleID)
          If mdi Is Nothing Then
            Result &= (" - Upgrade module failure - unable to obtain ModuleDefinitionID for module definition friendly name " & dmi.FriendlyName)
          Else
            Dim mi As ModuleInfo
            Dim mc As New ModuleController
            Dim rgx As New Regex("^(.*Portals/\d+/)(.+)$", RegexOptions.IgnoreCase)
            Dim cntFixed As Integer = 0
            Dim cntProcessed As Integer = 0
            For Each mi In mc.GetAllModules()
              If mi.ModuleDefID = mdi.ModuleDefID Then
                Dim settings As Hashtable = mc.GetModuleSettings(mi.ModuleID)
                Dim rootURL As String = Utils.GetValue(settings("RootURL"), "")
                If rootURL <> "" Then
                  Dim rootURLFixed As String = rgx.Replace(rootURL, "$2")
                  If rootURLFixed <> rootURL Then
                    mc.UpdateModuleSetting(mi.ModuleID, "RootURL", rootURLFixed)
                    cntFixed += 1
                  End If
                  cntProcessed += 1
                End If
              End If
            Next
            Result &= String.Format(" - Gallery modules upgraded: {0} of {1}", cntFixed, cntProcessed)
          End If
        End If
      End If
      Return Result
    End Function
  End Class
End Namespace
