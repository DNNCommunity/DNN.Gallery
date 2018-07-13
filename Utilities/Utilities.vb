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
Option Strict On

Imports System.Text
Imports System.Reflection
Imports System.IO
Imports DotNetNuke
Imports DotNetNuke.Entities.Portals
Imports System.Drawing
Imports DotNetNuke.Entities.Users

Namespace DotNetNuke.Modules.Gallery

  ' Utility routines for various repeated functions.   

  Public Class Utils
    Public Const GALLERY_VIEW As String = "galleryview"
    Public Const GALLERY_SORT As String = "gallerysort"
    Public Const GALLERY_SORT_DESC As String = "gallerysortdesc"

    'William Severance - modified to call new overload which allows specification of
    'the depth of sub-albums to which folders will be populated to fix Gemini
    'issue GAL-6168. This modification enables population on demand when the configuration
    'option BuildCacheOnStart is false. Note that because of the manner in which the cover photos
    'of albums are set, the current folder AND all sub-folders of the current folder must be
    'populated.

    Public Overloads Shared Sub PopulateAllFolders(ByVal rootFolder As GalleryFolder, ByVal ReSync As Boolean)
      'Populate all folders to maximum depth
      PopulateAllFolders(rootFolder, Integer.MaxValue, False)
    End Sub

    Public Overloads Shared Sub PopulateAllFolders(ByVal rootFolder As GalleryFolder, ByVal Depth As Integer, ByVal ReSync As Boolean)

      If Depth < 1 Then Return

      If Not rootFolder.IsPopulated Then
        rootFolder.Populate(ReSync)
      End If

      For Each folder As Object In rootFolder.List
        If TypeOf folder Is GalleryFolder AndAlso Not CType(folder, GalleryFolder).IsPopulated Then
          CType(folder, GalleryFolder).Populate(ReSync)
          If Depth > 1 Then PopulateAllFolders(CType(folder, GalleryFolder), Depth - 1, ReSync)
        End If
      Next

    End Sub

    Public Shared Function GetChildFolders(ByVal rootFolder As GalleryFolder, ByVal depth As Integer) As List(Of GalleryFolder)
      Dim folders As New List(Of GalleryFolder)
      If Not rootFolder.IsPopulated Then
        rootFolder.Populate(True)
      End If
      For Each child As IGalleryObjectInfo In rootFolder.List
        If child.IsFolder Then
          Dim folder As GalleryFolder = CType(child, GalleryFolder)
          folders.Add(folder)
          If depth > 1 Then
            folders.AddRange(GetChildFolders(folder, depth - 1))
          End If
        End If
      Next
      Return folders
    End Function

    Public Shared Function GetRootRelativeFolder(ByVal rootFolder As GalleryFolder, ByVal folderPath As String) As GalleryFolder
      Dim folder As GalleryFolder = rootFolder

      If Not String.IsNullOrEmpty(folderPath) Then
        Try
          Dim paths As String() = Split(folderPath, "/")
          For depth As Integer = 0 To paths.Length - 1
            folder = CType(folder.List.Item(paths(depth)), GalleryFolder)
            If Not folder.IsPopulated Then folder.Populate(False)
          Next
        Catch ex As Exception
          folder = Nothing        'folder not found/invalid folderPath
        End Try
      End If
      Return folder
    End Function

    Public Shared Function AddHost(ByVal URL As String) As String
      Dim strReturn As String
      Dim strHost As String = HttpContext.Current.Request.ServerVariables("HTTP_HOST")

      ' Check if this URL has already included host
      If Not URL.ToLower.IndexOf(strHost.ToLower) >= 0 Then
        If strHost.EndsWith("/") Then
          strHost = strHost.TrimEnd("/"c)
        End If
        If Not URL.StartsWith("/") Then
          URL = "/" & URL
        End If
        strReturn = AddHTTP(strHost & URL)
      Else
        strReturn = URL
      End If
      Dim test As String = Path.Combine(strHost, URL)
      Return strReturn

    End Function

    Public Shared Function BuildPopup(ByVal URL As String, ByVal PageHeight As Integer, ByVal PageWidth As Integer) As String
      Dim sb As New StringBuilder
      'javascript:window.open
      sb.Append("javascript:DestroyWnd;CreateWnd('")
      '  sb.Append("javascript:CreateWnd('")
      sb.Append(URL)
      sb.Append("', ")
      sb.Append(CStr(PageWidth))
      sb.Append(", ")
      sb.Append(CStr(PageHeight))
      sb.Append(", true);")
      Return sb.ToString
    End Function

    Private Overloads Shared Function CreateHashtableFromQueryString(ByVal page As Page) As Hashtable
      Dim ht As New Hashtable

      Dim query As String
      For Each query In page.Request.QueryString
        If Len(query) > 0 Then
          ht.Add(query, page.Request.QueryString(query))
        End If
      Next query
      Return ht
    End Function 'CreateHashtableFromQueryString

    Private Shared Function CreateQueryString(ByVal current As Hashtable, ByVal add As Hashtable, ByVal remove As Hashtable) As String
      Dim myEnumerator As IDictionaryEnumerator = add.GetEnumerator()
      While myEnumerator.MoveNext()
        If current.ContainsKey(myEnumerator.Key) Then
          current.Remove(myEnumerator.Key)
        End If
        current.Add(myEnumerator.Key, myEnumerator.Value)
      End While

      myEnumerator = remove.GetEnumerator()
      While myEnumerator.MoveNext()
        Dim removeKey As String = CStr(myEnumerator.Key)

        If current.ContainsKey(removeKey) Then
          Dim removeValue As String = CStr(myEnumerator.Value)

          If CStr(current(removeKey)) = removeValue OrElse removeValue = String.Empty Then
            current.Remove(removeKey)
          End If
        End If
      End While
      Dim count As Integer = 0
      Dim sb As New StringBuilder
      myEnumerator = current.GetEnumerator()
      While myEnumerator.MoveNext()
        If count = 0 Then
          sb.Append("?")
        Else
          sb.Append("&")
        End If
        sb.Append(myEnumerator.Key)
        sb.Append("=")
        sb.Append(myEnumerator.Value)
        count += 1
      End While

      Return sb.ToString()
    End Function 'CreateQueryString

    Private Overloads Shared Function CreateHashtableFromQueryString(ByVal query As String) As Hashtable
      Dim ht As New Hashtable

      Dim startIndex As Integer = 0
      While startIndex >= 0
        Dim oldStartIndex As Integer = startIndex
        Dim equalIndex As Integer = query.IndexOf("=", startIndex)
        startIndex = query.IndexOf("&", startIndex)
        If startIndex >= 0 Then
          startIndex += 1
        End If
        If equalIndex >= 0 Then
          Dim lengthValue As Integer = 0
          If startIndex >= 0 Then
            lengthValue = startIndex - equalIndex - 2
          Else
            lengthValue = query.Length - equalIndex - 1
          End If
          Dim key As String = query.Substring(oldStartIndex, equalIndex - oldStartIndex)
          Dim val As String = query.Substring(equalIndex + 1, lengthValue)

          ht.Add(key, val)
        End If
      End While

      Return ht
    End Function 'CreateHashtableFromQueryString

    Public Overloads Shared Function GetURL(ByVal baseURL As String, ByVal page As Page, ByVal add As String, ByVal remove As String) As String
      If remove <> String.Empty Then
        remove += "&"
      End If
      remove += "DocumentID="

      Dim currentQueries As Hashtable = CreateHashtableFromQueryString(page)
      Dim addQueries As Hashtable = CreateHashtableFromQueryString(add)
      Dim removeQueries As Hashtable = CreateHashtableFromQueryString(remove)

      Dim newQueryString As String = CreateQueryString(currentQueries, addQueries, removeQueries)

      Return baseURL + newQueryString
    End Function 'GetURL

    Public Overloads Shared Function GetURL(ByVal URL As String, ByVal AdditionalParameters As String()) As String
      Dim sb As New StringBuilder(URL)

      If URL.Contains("?") Then
        sb.Append("&")
      Else
        sb.Append("?")
      End If

      ' Check if tabid param exists
      If Not URL.ToLower.Contains("tabid") And (AdditionalParameters Is Nothing OrElse Not Array.Exists(AdditionalParameters, AddressOf MatchTabId)) Then
        Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
        sb.Append("tabid=" & _portalSettings.ActiveTab.TabID.ToString & "&")
      End If
      For Each param As String In AdditionalParameters
        If Not String.IsNullOrEmpty(param.Split("="c)(1)) Then sb.Append(param & "&")
      Next
      sb.Length -= 1
      Return sb.ToString

    End Function

    Private Shared Function MatchTabId(ByVal param As String) As Boolean
      Return param.Contains("tabid")
    End Function

    ' Provides more functionality than the path object static functions
    Public Shared Function BuildPath(ByVal Input() As String, ByVal Delimiter As String, ByVal StripInitial As Boolean, ByVal StripFinal As Boolean) As String
      Dim output As StringBuilder = New StringBuilder

      output.Append(Join(Input, Delimiter))

      ' JIMJ
      ' If the beginning of the output begins with \\ 
      ' then we are dealing with a UNC path.  
      ' Double the beginning slashes (\\) to compensate for the following Replace call.
      If output.ToString().Substring(0, 2) = "\\" Then
        output.Insert(0, "\\")
      End If

      output.Replace(Delimiter & Delimiter, Delimiter)

      If StripInitial Then
        If Left(output.ToString(), Len(Delimiter)) = Delimiter Then
          output.Remove(0, Len(Delimiter))
        End If
      End If

      If StripFinal Then
        If Right(output.ToString, Len(Delimiter)) = Delimiter Then
          output.Remove(output.Length - Len(Delimiter), Len(Delimiter))
        End If
      End If

      Return output.ToString()

    End Function

    ' Alternate signature for the BuildPath function
    Public Shared Function BuildPath(ByVal Input() As String, ByVal Delimiter As String) As String

      Return BuildPath(Input, Delimiter, True, False)

    End Function

    Public Shared Function SolpartEncode(ByVal Source As String) As String
      ' If it has been encode in javascript, no need to do it again
      If Source.IndexOf("javascript") > -1 Then
        Return Source
      End If
      Return JSEncode(Source)

    End Function

    Public Shared Function JSEncode(ByVal Source As String) As String
      Source = Source.Replace("'", "\'")
      '<JJK. handle double quotes as well. /> 
      Source = Source.Replace(Chr(34), "\" & Chr(34))
      Source = Source.Replace("/", "\/")
      'Source = ReplaceCaseInsensitive(Source, "/", "|")
      Return Source
    End Function

    Public Shared Function JSDecode(ByVal Source As String) As String
      'Source = ReplaceCaseInsensitive(Source, "^", "'") 
      '<JJK. handle double quotes as well. /> 
      Source = Source.Replace("\'", "'")
      Source = Source.Replace("\" & Chr(34), Chr(34))
      Source = Source.Replace("\/", "/")
      ' to solve problem with friendlyURL
      'Source = ReplaceCaseInsensitive(Source, "|", "/")
      Return Source
    End Function

    Public Shared Function FriendlyURLEncode(ByVal Source As String) As String
      Dim output As String = HttpUtility.UrlEncodeUnicode(Source).Replace("/", "!")
      Return output
    End Function

    Public Shared Function FriendlyURLDecode(ByVal Source As String) As String
      Dim output As String = HttpUtility.UrlDecode(Source).Replace("!", "/")
      If output = " " Then
        output = ""
      End If
      Return output
    End Function

    Public Shared Function GetValue(ByVal Input As Object, ByVal DefaultValue As String) As String
      ' Used to determine if a valid input is provided, if not, return default value
      If Input Is Nothing Then
        Return DefaultValue
      Else
        Return CStr(Input)
      End If
    End Function

    Public Shared Function ValidUserID(ByVal UserID As Integer) As Integer
      Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings

      Dim user As UserInfo = New UserController().GetUser(_portalSettings.PortalId, UserID)

      If (Not user Is Nothing) AndAlso (user.IsSuperUser) Then
        UserID = _portalSettings.AdministratorId
      End If

      Return UserID

    End Function

    Public Shared Function UploadFileInfo(ByVal ModuleID As Integer, ByVal SpaceUsed As Long, ByVal SpaceAvailable As Long) As String
      Dim GalleryConfig As DotNetNuke.Modules.Gallery.Config = Config.GetGalleryConfig(ModuleID)
      Dim sb As New System.Text.StringBuilder

      sb.Append("<p class=""Normal"">")
      sb.Append("<img alt='' src='" & GalleryConfig.GetImageURL("s_zip.gif") & "' />&nbsp;")
      sb.Append("Zip&nbsp;(.zip) ")
      sb.Append("&nbsp;&nbsp;<img alt='' src='" & GalleryConfig.GetImageURL("s_jpg.gif") & "' />&nbsp;")
      sb.Append(Localization.GetString("Image", GalleryConfig.SharedResourceFile))
      '<daniel file extension to use core>
      sb.Append("&nbsp;(" & Replace(GalleryConfig.FileExtensions, ";", ", "))
      If sb.Chars(sb.Length - 1) = "," Then sb.Remove(sb.Length - 1, 1)
      sb.Append(")")
      sb.Append("&nbsp;&nbsp;<img alt='' src='" & GalleryConfig.GetImageURL("s_mediaplayer.gif") & "' />&nbsp;")
      sb.Append(Localization.GetString("Movie", GalleryConfig.SharedResourceFile))
      '<daniel file extension to use core>
      sb.Append("&nbsp;(" & Replace(GalleryConfig.MovieExtensions, ";", ", "))
      If sb.Chars(sb.Length - 1) = "," Then sb.Remove(sb.Length - 1, 1)
      sb.Append(")")
      sb.Append("&nbsp;&nbsp;<img alt='' src='" & GalleryConfig.GetImageURL("s_flash.gif") & "' />&nbsp;")
      sb.Append(Localization.GetString("Flash", GalleryConfig.SharedResourceFile))
      sb.Append("&nbsp;(.swf)</p><p class=""Normal"">")
      sb.Append(Localization.GetString("MaxFileSize", GalleryConfig.SharedResourceFile))
      sb.Append("&nbsp;" & GalleryConfig.MaxFileSize.ToString & " kb <br />")
      sb.Append(Localization.GetString("Gallery_Quota", GalleryConfig.SharedResourceFile) & "&nbsp;")
      If GalleryConfig.Quota = 0 Then
        sb.Append(Localization.GetString("No_Gallery_Quota_Imposed", GalleryConfig.SharedResourceFile) & "<br />")
      Else
        sb.Append(GalleryConfig.Quota.ToString & " kb <br />")
      End If
      sb.Append(Localization.GetString("Portal_Quota", GalleryConfig.SharedResourceFile) & "&nbsp;")
      Dim ps As PortalSettings = PortalController.GetCurrentPortalSettings()
      If ps.HostSpace = 0 Then
        sb.Append(Localization.GetString("No_Portal_Quota_Imposed", GalleryConfig.SharedResourceFile) & "<br />")
      Else
        sb.Append((ps.HostSpace * 1024).ToString & " kb <br />")
      End If
      sb.Append(Localization.GetString("Space_Used", GalleryConfig.SharedResourceFile) & "&nbsp;")
      sb.AppendFormat("{0:F} kb <br />", (SpaceUsed / 1024))
      sb.Append(Localization.GetString("Space_Available", GalleryConfig.SharedResourceFile) & "&nbsp;")
      If SpaceAvailable = Long.MinValue Then
        sb.Append(Localization.GetString("No_Quota_Imposed", GalleryConfig.SharedResourceFile))
      Else
        If SpaceAvailable <= 0 Then
          sb.AppendFormat(Localization.GetString("Space_Available_Exceeded_By", GalleryConfig.SharedResourceFile), -(SpaceAvailable / 1024))
        Else
          sb.AppendFormat("{0:F} kb", (SpaceAvailable / 1024))
        End If

      End If
      sb.Append("</p>")
      Return sb.ToString
    End Function

    ' From 3.0 version using cookies to store view and sort order
    ' William Severance - modified to use configuration settings DefaultView, DefaultSort and DefaultSortDESC
    ' for the initial view/sort order for first-time visitor and to relate cookies values to specific ModuleID to avoid
    ' interaction of these settings between multiple Gallery modules in same portal (same domain name actually).
    ' Note that if user is not permitted to change view, the configuration DefaultView will be returned and no
    ' cookie will be used to maintain the current view.

    Public Shared Function GetView(ByVal GalleryConfiguration As Config) As Config.GalleryView

      If GalleryConfiguration.AllowChangeView Then
        Dim cookie As HttpCookie = HttpContext.Current.Request.Cookies(GALLERY_VIEW)

        If Not cookie Is Nothing Then
          Dim value As String = cookie.Values(GalleryConfiguration.ModuleID.ToString)
          If Not String.IsNullOrEmpty(value) Then
            Try
              Return CType([Enum].Parse(GetType(Config.GalleryView), value), Config.GalleryView)
            Catch
            End Try
          End If
        End If
      End If
      Return GalleryConfiguration.DefaultView

    End Function

    Public Shared Function GetSort(ByVal GalleryConfiguration As Config) As Config.GallerySort

      Dim cookie As HttpCookie = HttpContext.Current.Request.Cookies(GALLERY_SORT)

      If Not cookie Is Nothing Then
        Dim value As String = cookie.Values(GalleryConfiguration.ModuleID.ToString)
        If Not String.IsNullOrEmpty(value) Then
          Try
            Return CType([Enum].Parse(GetType(Config.GallerySort), value), Config.GallerySort)
          Catch
          End Try
        End If
      End If
      Return GalleryConfiguration.DefaultSort
    End Function

    Public Shared Function GetSortDESC(ByVal GalleryConfiguration As Config) As Boolean
      Dim cookie As HttpCookie = HttpContext.Current.Request.Cookies(GALLERY_SORT_DESC)

      If Not cookie Is Nothing Then
        Dim value As String = cookie.Values(GalleryConfiguration.ModuleID.ToString)
        If Not String.IsNullOrEmpty(value) Then
          Dim sortDESC As Boolean
          If Boolean.TryParse(value, sortDESC) Then Return sortDESC
        End If
      End If
      Return GalleryConfiguration.DefaultSortDESC
    End Function

    Public Shared Sub RefreshCookie(ByVal Name As String, ByVal ModuleID As Integer, ByVal Value As Object)
      Dim cookie As HttpCookie = HttpContext.Current.Request.Cookies(Name)
      If cookie Is Nothing Then
        cookie = New HttpCookie(Name)
        cookie.Expires = DateTime.MaxValue ' never expires
        HttpContext.Current.Response.AppendCookie(cookie)
      End If
      cookie.Item(ModuleID.ToString) = CType(Value, String)
      HttpContext.Current.Response.SetCookie(cookie)
    End Sub

    ''' <summary>
    ''' Added by Stefan Cullman to workaround a Core problem in FURL's
    ''' </summary>
    ''' <param name="params">The parameters in the request querystring</param>
    ''' <returns>a string of parameters with the empty parameter cleaned up</returns>
    ''' <remarks>This is placed inside methods that use params from the qs</remarks>
    Public Shared Function RemoveEmptyParams(ByVal params As String()) As String()
      Dim newparams As New Generic.List(Of String)
      For Each parameter As String In params
        If Not parameter.EndsWith("=") Then newparams.Add(parameter)
      Next
      Return newparams.ToArray
    End Function

    'William Severance - added method to append a parameter to an existing URL
    '                  - modified to fix GAL-8352 to remove Uri class constructor as it required
    '                  - absolute not relative URL

    Public Shared Function AppendURLParameter(ByVal URL As String, ByVal param As String) As String
      If param Is Nothing OrElse param.Length = 0 Then
        Return URL
      Else
        If URL.Contains("?") Then
          Return String.Concat(URL, "&", param)
        Else
          Return String.Concat(URL, "?", param)
        End If
      End If
    End Function

    Public Shared Function AppendURLParameters(ByVal URL As String, ByVal params As String()) As String
      If params Is Nothing OrElse params.Length < 1 Then
        Return URL
      Else
        Dim sb As New StringBuilder(URL)
        If URL.Contains("?") Then
          sb.Append("&")
        Else
          sb.Append("?")
        End If
        For Each param As String In params
          If Not String.IsNullOrEmpty(param.Split("="c)(1)) Then sb.Append(param & "&")
        Next
        sb.Length -= 1
        Return sb.ToString
      End If
    End Function

    'William Severance - added methods to handle formatting/parsing of dates where Date.MaxValue
    'signifies the date not being set

    Public Overloads Shared Function DateToText(ByVal d As DateTime, ByVal format As String) As String
      If d = Date.MaxValue Then
        Return String.Empty
      Else
        Return String.Format(format, d)
      End If
    End Function

    Public Overloads Shared Function DateToText(ByVal d As DateTime) As String
      If d = Date.MaxValue Then
        Return String.Empty
      Else
        Return d.ToShortDateString
      End If
    End Function

    Public Overloads Shared Function TextToDate(ByVal s As String, ByVal provider As IFormatProvider) As DateTime
      If String.IsNullOrEmpty(s) Then
        Return DateTime.MaxValue
      Else
        Return DateTime.Parse(s, provider)
      End If
    End Function

    Public Overloads Shared Function TextToDate(ByVal s As String) As DateTime
      If String.IsNullOrEmpty(s) Then
        Return DateTime.MaxValue
      Else
        Return DateTime.Parse(s)
      End If
    End Function

    ' Added by Andrew Galbraith Ryer for use in Quota checking.
    Public Overloads Shared Function GetDirectorySize(ByVal di As DirectoryInfo) As Long
      Dim size As Long = 0L

      For Each fi As FileInfo In di.GetFiles()
        size += fi.Length
      Next

      For Each subdir As DirectoryInfo In di.GetDirectories()
        size += GetDirectorySize(subdir)
      Next

      Return size
    End Function

    'Overload added by William Severance
    Public Overloads Shared Function GetDirectorySize(ByVal path As String) As Long
      Dim di As New DirectoryInfo(path)
      If di.Exists Then
        Return GetDirectorySize(di)
      Else
        Return 0L
      End If
    End Function

    'Added by William Severance - used in various file header analysis classes
    Public Shared Function ReadAllBytesFromStream(ByVal s As Stream, ByVal buffer As Byte()) As Integer
      Dim offset, bytesRead, totalCount As Integer
      While (True)
        bytesRead = s.Read(buffer, offset, 100)
        If bytesRead = 0 Then Exit While
        offset += bytesRead
        totalCount += bytesRead
      End While
      Return totalCount
    End Function

    'Added by William Severance - used in various file header analysis classes
    'Converts one or two byte value in LSB..MSB (little endian) order to Int32. n must be 1 or 2
    Public Shared Function ConvertToInt32(ByVal bytes() As Byte, ByVal n As Integer) As Int32
      'Read next 1 or 2 bytes LSB...MSB (little endian) as Int32
      If n < 1 Or n > 2 Then
        Throw New ArgumentOutOfRangeException("n", "nBytes must be 1 or 2")
      Else
        Dim val As Int32 = 0
        Dim i As Integer
        Dim b As Integer
        For i = 0 To n - 1
          b = bytes(i)
          val += (b << (8 * i))
        Next i
        Return val
      End If
    End Function

    'Added by William Severance - used in various file header analysis classes
    'Converts one, two, three or four byte value in LSB..MSB (little endian) order to Int64. n must be 1-4
    Public Shared Function ConvertToInt64(ByVal bytes() As Byte, ByVal n As Integer) As Int64
      'Read next 1 or 2 bytes LSB...MSB (little endian) as Int32
      If n < 1 Or n > 4 Then
        Throw New ArgumentOutOfRangeException("n", "nBytes must be 1 through 4")
      Else
        Dim val As Int64 = 0L
        Dim i As Integer
        Dim b As Integer
        For i = 0 To n - 1
          b = bytes(i)
          val += (b << (8 * i))
        Next i
        Return val
      End If
    End Function

    'Added by William Severance - used in various file header analysis classes
    'Converts 4 byte (32 bit) MS-DOS File Time/Date to DateTime
    Public Shared Function ConvertToDateTime(ByVal bytes() As Byte) As DateTime

      Dim parts() As Integer = {5, 6, 5, 5, 4, 7}
      Dim partPtr As Integer
      Dim val As Integer
      Dim yr, mo, da, hr, min, sec As Integer

      Dim bytePtr As Integer
      Dim workingReg As Int64 = 0
      Dim mask As Int64

      If bytes Is Nothing OrElse bytes.Length <> 4 Then Return DateTime.MinValue

      For bytePtr = 0 To 3
        Dim b As Int64 = bytes(bytePtr)
        workingReg += (b << (8 * bytePtr))
      Next
      For partPtr = 0 To 5
        mask = Convert.ToInt64(2 ^ (parts(partPtr)) - 1)
        val = Convert.ToInt32(workingReg And mask)
        workingReg >>= parts(partPtr)
        Select Case partPtr
          Case 0
            sec = val
          Case 1
            min = val
          Case 2
            hr = val
          Case 3
            da = val
          Case 4
            mo = val
          Case 5
            yr = val
        End Select
      Next
      Return New DateTime(yr + 1980, mo, da, hr, min, sec)
    End Function

    'Added by William Severance to facilite fetch of ItemType and FileIcon
    'Returns the item type by reference as ItemType
    'and Icon url as function return value
    Public Shared Function GetFileTypeIcon(ByVal Ext As String, ByVal GalleryConfig As Config, ByRef ItemType As Config.ItemType) As String

      Dim mImageURL As String = GalleryConfig.GetImageURL("")
      Dim mIcon As String = String.Empty

      If GalleryConfig.IsValidFlashType(Ext) Then
        ItemType = Config.ItemType.Flash
        mIcon = mImageURL & "s_flash.gif"
      ElseIf GalleryConfig.IsValidImageType(Ext) Then
        ItemType = Config.ItemType.Image
        mIcon = mImageURL & "s_jpg.gif"
      ElseIf GalleryConfig.IsValidMovieType(Ext) Then
        ItemType = Config.ItemType.Movie
        mIcon = mImageURL & "s_mediaplayer.gif"
      ElseIf Ext = ".zip" Then
        ItemType = Config.ItemType.Zip
        mIcon = mImageURL & "s_zip.gif"
      End If
      Return mIcon
    End Function

    'William Severance - added method to add file and optionally a non-existing folder to the DNN files and or folders table(s)
    'If AddNonExistingFolder is false, sub exits without adding file if folder is missing from DNN folders table, else will first
    'create an entry in the folders table inheriting the parent folder's permissions.

    Public Shared Function SaveDNNFile(ByVal Filepath As String, ByVal Width As Integer, ByVal Height As Integer, ByVal AddNonExistingFolder As Boolean, ByVal ClearCache As Boolean) As Integer
      Dim fileId As Integer = -1

      If Not String.IsNullOrEmpty(Filepath) Then
        Dim ps As Portals.PortalSettings = Portals.PortalController.GetCurrentPortalSettings()
        Dim fi As New System.IO.FileInfo(Filepath)
        If fi.Exists Then
          Dim folderPath As String = fi.Directory.FullName.Replace(ps.HomeDirectoryMapPath, "").Replace("\", "/")
          Dim fileName As String = fi.Name
          Dim fileExt As String = fi.Extension.TrimStart("."c).ToLower()
          Dim fileSize As Long = fi.Length

          Dim PortalId As Integer = ps.PortalId

          Dim folderId As Integer = -1
          Dim objFolderController As New Services.FileSystem.FolderController
          Dim objFolderInfo As Services.FileSystem.FolderInfo = objFolderController.GetFolder(PortalId, folderPath, False)
          'Is the folder already in the DNN folders table? If not, should we create it and continue?
          If objFolderInfo Is Nothing Then
            If AddNonExistingFolder Then
              objFolderInfo = New Services.FileSystem.FolderInfo
              With objFolderInfo
                .UniqueId = Guid.NewGuid()
                .VersionGuid = Guid.NewGuid()
                .PortalID = PortalId
                .FolderPath = folderPath
                .StorageLocation = Services.FileSystem.FolderController.StorageLocationTypes.InsecureFileSystem
                .IsProtected = False
                .IsCached = False
              End With
              folderId = objFolderController.AddFolder(objFolderInfo)
              If folderId <> -1 Then
                'WES - Set Folder Permissions to inherit from parent
                FileSystemUtils.SetFolderPermissions(PortalId, folderId, folderPath)
              End If
            End If
          Else
            folderId = objFolderInfo.FolderID
          End If
          If folderId <> -1 Then
            Dim objFileController As New Services.FileSystem.FileController
            Dim objFileInfo As Services.FileSystem.FileInfo = objFileController.GetFile(fileName, PortalId, folderId)

            Dim contentType As String = FileSystemUtils.GetContentType(fileExt)
            If contentType.StartsWith("image") AndAlso (Width = 0 OrElse Height = 0) Then
              Dim img As Bitmap = Nothing
              Try
                img = New Bitmap(Filepath)
                Width = img.Width
                Height = img.Height
              Catch
                'eat the exception if we can load bitmap - will have width=height=0
              Finally
                If Not img Is Nothing Then img.Dispose()
                img = Nothing
              End Try
            End If
            'Is the file already in the DNN files table in which case we update rather than add
            If objFileInfo Is Nothing Then
              objFileInfo = New Services.FileSystem.FileInfo
              With objFileInfo
                .PortalId = PortalId
                .FileName = fileName
                .Extension = fileExt
                .Size = Convert.ToInt32(fileSize)
                .Width = Width
                .Height = Height
                .ContentType = contentType
                .Folder = FileSystemUtils.FormatFolderPath(folderPath)
                .FolderId = folderId
                .IsCached = ClearCache
                .UniqueId = Guid.NewGuid
                .VersionGuid = Guid.NewGuid
              End With
              fileId = objFileController.AddFile(objFileInfo)
              If ClearCache Then
                objFileController.GetAllFilesRemoveCache()
              End If
            Else
              With objFileInfo
                .Size = Convert.ToInt32(fileSize)
                .Width = Width
                .Height = Height
              End With
              fileId = objFileInfo.FileId
              objFileController.UpdateFile(objFileInfo)
            End If
          End If
        End If
      End If
      Return fileId
    End Function

    ' Sets the READ permissions of the root gallery folder (/Portals/<PortalID>/Gallery/<ModuleID>/) and all of its subfolders to match either the
    ' special permissions if supplied or the VIEW permissions of the associated ModuleID or the VIEW permissions of the TabID if the module
    ' is set to inherit view permissions from the page
    Public Shared Sub SyncFolderPermissions(ByVal specialPermissions As String, ByVal ModuleId As Integer, ByVal TabId As Integer)
      Dim ps As PortalSettings = PortalController.GetCurrentPortalSettings()
      Dim modController As New Entities.Modules.ModuleController
      Dim moduleInfo As Entities.Modules.ModuleInfo = modController.GetModule(ModuleId, TabId)
      If Not moduleInfo Is Nothing Then
        Dim portalID As Integer = ps.PortalId
        If String.IsNullOrEmpty(specialPermissions) Then
          If moduleInfo.InheritViewPermissions Then
            Dim tabPermissions As Security.Permissions.TabPermissionCollection = Security.Permissions.TabPermissionController.GetTabPermissions(TabId, portalID)
            specialPermissions = tabPermissions.ToString("VIEW")
          Else
            Dim modPermissions As Security.Permissions.ModulePermissionCollection = Security.Permissions.ModulePermissionController.GetModulePermissions(ModuleId, TabId)
            specialPermissions = modPermissions.ToString("VIEW")
          End If
        Else
          'Administrator role always has read permission on folder even if not included in specialPermissions
          If Not specialPermissions.Contains(";" & ps.AdministratorRoleId.ToString) AndAlso Not specialPermissions.Contains(";" & ps.AdministratorRoleName) Then
            specialPermissions &= ";" & ps.AdministratorRoleName
          End If
        End If

        Dim rootFolderPath As String = Config.GetGalleryConfig(ModuleId).RootURL.Replace(ps.HomeDirectory, "") '"Gallery/" & ModuleId.ToString & "/"
        Dim fldController As New Services.FileSystem.FolderController
        Dim fldInfo As Services.FileSystem.FolderInfo = fldController.GetFolder(portalID, rootFolderPath, True)
        If Not fldInfo Is Nothing Then
          Dim folderID As Integer = fldInfo.FolderID
          Dim newPermissions As New Security.Permissions.FolderPermissionCollection
          AddRolesToFolderPermissionsCollection(newPermissions, specialPermissions, "READ", portalID, folderID)

          'Dim fldPermController As New Security.Permissions.FolderPermissionController
          Dim fldPermInfo As Security.Permissions.FolderPermissionInfo
          Dim writePermissions As Security.Permissions.FolderPermissionCollection
          'Try
          ' WES: Due to breaking change in DNN 5.00.00 and 5.00.01 which made GetFolderPermissionsCollectionByFolderPath a shared rather than
          ' instance method, the following line will throw a method not found exception in these two versions only. This will be fixed for DNN 5.01.00.
          ' The work around in the catch block is much less efficient.
          writePermissions = GetFolderWritePermissionsCollectionByFolderPath(moduleInfo.PortalID, rootFolderPath)
          'Must be DNN 4.9.x or > DNN 5.01.00 so we can copy over write permissions the easy way
          For Each fldPermInfo In writePermissions
            newPermissions.Add(fldPermInfo)
          Next
          'Catch ex As MissingMethodException
          'Must be DNN 5.00.00 or 5.00.01 as exception was thrown above so copy over write permissions the hard way.
          'Dim writeRoles As String = FileSystemUtils.GetRoles(rootFolderPath, portalID, "WRITE")
          'AddRolesToFolderPermissionsCollection(newPermissions, writeRoles, "WRITE", portalID, folderID)
          'End Try

          ' Should test if newPermissions <> oldPermissions but not easily done efficiently
          ' so we will assume they have changed and delete ALL old permissions first

          fldInfo.FolderPermissions.Clear()
          fldInfo.FolderPermissions.AddRange(newPermissions)
          Security.Permissions.FolderPermissionController.SaveFolderPermissions(fldInfo)

          'fldPermController.DeleteFolderPermissionsByFolder(portalID, rootFolderPath)
          'For Each fldPermInfo In newPermissions
          '  fldPermController.AddFolderPermission(fldPermInfo)
          'Next
          SetSubFolderPermissions(portalID, rootFolderPath, True)
          ' DataCache.ClearFolderPermissionsCache(portalID) ' Not needed in DNN 5.x
        End If
      End If
    End Sub

    ' WES: In DNN 5.00.00 and 5.00.01, this will throw a MissingMethodException due to the instance method GetFolderPermissionsCollectionByFolderPath
    ' having been deprecated and replaced by a shared method of the same signature. In DNN 5.01.00, the deprecation will be rolled back and
    ' the shared method will be given a different name.

    Private Shared Function GetFolderWritePermissionsCollectionByFolderPath(ByVal portalID As Integer, ByVal folderPath As String) As Permissions.FolderPermissionCollection
      Dim fldPermController As New Permissions.FolderPermissionController
      Dim writePermissions As New Permissions.FolderPermissionCollection

      For Each fPI As Permissions.FolderPermissionInfo In Permissions.FolderPermissionController.GetFolderPermissionsCollectionByFolder(portalID, folderPath)
        If fPI.PermissionCode = "SYSTEM_FOLDER" AndAlso fPI.PermissionKey = "WRITE" Then
          writePermissions.Add(fPI)
        End If
      Next
      Return writePermissions
    End Function

    Private Shared Sub AddRolesToFolderPermissionsCollection(ByVal permissions As Security.Permissions.FolderPermissionCollection, ByVal roles As String, ByVal permissionKey As String, ByVal portalID As Integer, ByVal folderID As Integer)
      Dim permController As New DotNetNuke.Security.Permissions.PermissionController
      Dim permission As Security.Permissions.PermissionInfo = CType(permController.GetPermissionByCodeAndKey("SYSTEM_FOLDER", permissionKey)(0), Security.Permissions.PermissionInfo)
      If permission IsNot Nothing AndAlso Not String.IsNullOrEmpty(roles) Then
        Dim permissionID As Integer = permission.PermissionID
        Dim fldPermInfo As Security.Permissions.FolderPermissionInfo
        Dim rolesToAdd As String() = roles.Trim(";"c).Split(";"c)
        Dim userIDRegex As New Regex("^\[(\d+)\]$")
        Dim roleController As New Security.Roles.RoleController
        Dim nullRoleID As Integer = Integer.Parse(glbRoleNothing)
        Dim allUsersRoleID As Integer = Integer.Parse(glbRoleAllUsers)
        For Each role As String In rolesToAdd
          Dim m As RegularExpressions.Match = userIDRegex.Match(role, 0)
          Dim userID As Integer = -1
          Dim roleID As Integer = nullRoleID
          Dim roleInfo As Security.Roles.RoleInfo = Nothing
          If m.Success Then
            userID = Integer.Parse(m.Groups(1).Value)
          Else
            Dim n As Integer
            If Integer.TryParse(role, n) Then
              roleID = n
            Else
              If role = glbRoleAllUsersName Then
                roleID = allUsersRoleID
              Else
                roleInfo = roleController.GetRoleByName(portalID, role)
                If Not roleInfo Is Nothing Then
                  roleID = roleInfo.RoleID
                End If
              End If
            End If
          End If
          If roleID <> nullRoleID OrElse userID <> -1 Then
            fldPermInfo = New Security.Permissions.FolderPermissionInfo()
            With fldPermInfo
              .FolderID = folderID
              .PermissionID = permissionID
              .PortalID = portalID
              .RoleID = roleID
              .UserID = userID
              .AllowAccess = True
            End With
            permissions.Add(fldPermInfo)
          End If
        Next
      End If
    End Sub

    ' Added by WES
    ' Sets the permissions of FolderID in PortalID with relativePath to that of its parent folder
    ' If isRecursive is True, will apply parent permissions recursively to ALL subfolders of the parent.
    Public Shared Sub SetSubFolderPermissions(ByVal PortalID As Integer, ByVal relativePath As String, ByVal isRecursive As Boolean)
      Dim subFolders As ArrayList = FileSystemUtils.GetFoldersByParentFolder(PortalID, relativePath)
      Dim fldPermController As New Security.Permissions.FolderPermissionController
      For Each fldInfo As Services.FileSystem.FolderInfo In subFolders
        'fldPermController.DeleteFolderPermissionsByFolder(PortalID, fldInfo.FolderPath) - NOT Needed in DNN 5.x
        FileSystemUtils.SetFolderPermissions(PortalID, fldInfo.FolderID, fldInfo.FolderPath)
        If isRecursive AndAlso Not fldInfo.FolderName.StartsWith("_") Then
          SetSubFolderPermissions(PortalID, fldInfo.FolderPath, True)
        End If
      Next
    End Sub

    ' Replace invalid characters in a filename
    Public Shared Function MakeValidFilename(ByVal inputName As String, ByVal replacementCharacter As String) As String
      Dim invalidCharacterReplacementRegex As New Regex("[\000-\037""*:<>?\\/|]")
      inputName = inputName.Trim(New Char() {" "c, "."c}) ' should not start or end with space or period
      inputName = invalidCharacterReplacementRegex.Replace(inputName, replacementCharacter)
      Return inputName
    End Function

    '' Added By Quinn
    '' Multiple controls have identical ReturnURL functions
    '' Updates are needed for security fixes (GAL-9404 and GAL-9403) so I am moving to and modifying here
    Public Shared Function ReturnURL(ByVal TabId As Integer, ByVal ModuleId As Integer, ByVal Request As System.Web.HttpRequest) As String
      Dim params As New Generic.List(Of String)
      If Not Request.QueryString("path") Is Nothing Then params.Add("path=" & Request.QueryString("path"))
      If Not Request.QueryString("currentstrip") Is Nothing Then params.Add("currentstrip=" & Request.QueryString("currentstrip"))
      If Not Request.QueryString("currentitem") Is Nothing Then params.Add("currentitem=" & Request.QueryString("currentitem"))
      Dim ctl As String = ""
      Dim remove As String = ""
      If Not Request.QueryString("returnctl") Is Nothing Then
        Dim returnCtls As String() = Request.QueryString("returnctl").Split(";"c)
        If returnCtls.Length > 1 Then
          ctl = returnCtls(returnCtls.Length - 2)
          Dim rtnctl As String = Request.QueryString("returnctl").Replace(ctl & ";", "")
          If Not String.IsNullOrEmpty(rtnctl) Then params.Add("returnctl=" & rtnctl) Else remove += "returnctl="
          params.Add("mid=" & ModuleId.ToString)
        End If
      End If
      If ctl IsNot "" Then
        params.Add("ctl=" & ctl)
      End If

      If remove.Length > 0 Then remove += "&ctl=" Else remove = "ctl="

      Return SanitizedRawUrl(TabId, ModuleId, params.ToArray(), remove)

    End Function

    '' Added By Quinn
    '' For GAL-9404
    '' The AdditionalParameters parameter should be formated as key=value
    Public Shared Function SanitizedRawUrl(ByVal TabId As Integer, ByVal ModuleId As Integer, ByVal addAdditionalParameters As String()) As String
      Return SanitizedRawUrl(TabId, ModuleId, addAdditionalParameters, "")
    End Function

    Public Shared Function SanitizedRawUrl(ByVal TabId As Integer, ByVal ModuleId As Integer, ByVal addAdditionalParameters As String(), ByVal remove As String) As String
      Dim add As String = ""

      For Each param As String In addAdditionalParameters ' In the form key=value
        If Not String.IsNullOrEmpty(param.Split("="c)(1)) Then
          add += param + "&"
        End If
      Next

      Return SanitizedRawUrl(TabId, ModuleId, add, remove, Nothing)
    End Function

    '' Added By Quinn
    '' Fix for GAL-9403
    Public Shared Function SanitizedRawUrl(ByVal TabId As Integer, ByVal ModuleId As Integer) As String
      Return SanitizedRawUrl(TabId, ModuleId, "", "", Nothing)
    End Function

    '' Added By Quinn
    '' Sanitizes the URL - A fix for GAL-9403 and GAL-9404
    '' Remove removes from the provided request.querystring only
    Public Shared Function SanitizedRawUrl(ByVal TabId As Integer, ByVal ModuleId As Integer, ByVal add As String, ByVal remove As String, ByVal request As HttpRequest) As String

      Dim rawParamCollection As NameValueCollection
      Dim sanitizedParams As New ArrayList

      Dim ps As New PortalSecurity
      Dim ctl As String = ""

      Dim addQueries As Hashtable = New Hashtable
      If add IsNot String.Empty Then
        addQueries = CreateHashtableFromQueryString(add.ToLower())
      End If
      Dim removeQueries As Hashtable = New Hashtable
      If remove IsNot String.Empty Then
        removeQueries = CreateHashtableFromQueryString(remove.ToLower())
      End If

      Dim modParamCollection As NameValueCollection = New NameValueCollection

      If request IsNot Nothing Then
        rawParamCollection = request.QueryString

        For Each key As String In rawParamCollection
          If key IsNot "mid" And Not removeQueries.ContainsKey(key) Then
            Dim value As String = rawParamCollection.GetValues(key)(0)
            modParamCollection.Add(key, value)
          End If
        Next

      End If

      For Each key As String In addQueries.Keys
        modParamCollection.Remove(key)
        Dim value As String = addQueries.Item(key).ToString()
        modParamCollection.Add(key, value)
      Next


      For Each key As String In modParamCollection
        Dim newValue As String = Nothing

        key = key.ToLower() 'Makes sure all comparrisons will be in lower case

        Select Case key
          Case "tabid", "language", "portalid"
            Continue For
          Case "mid"
            newValue = ModuleId.ToString
          Case "ctl"
            ctl = ps.InputFilter(modParamCollection.Item(key).ToString(), PortalSecurity.FilterFlag.NoMarkup Or PortalSecurity.FilterFlag.NoSQL)
            Continue For
          Case "returnctl"
            newValue = request.QueryString(key)
            Dim newNewValue As String = ""

            For Each strVal As String In newValue.Split(";"c)

              Dim isValid As Boolean = False

              For Each retCtl As String In ReturnCtlValues
                If retCtl.ToLower() = strVal.ToLower() Then
                  isValid = True
                  Exit For
                End If
              Next
              ' UrlEncode is applied to be sure about the security check
              If isValid Then newNewValue += HttpUtility.UrlEncode(strVal) & ";"
            Next

            newValue = newNewValue
          Case Else
            If Regex.IsMatch(key, "[^a-zA-Z0-9_\-]") Then
              Throw New HttpException(403, "Invalid characters in querystring parameter key")
            Else
              newValue = modParamCollection.Item(key).ToString()
              If Not Regex.IsMatch(newValue, "^\d+$") Then
                newValue = ps.InputFilter(newValue, PortalSecurity.FilterFlag.NoScripting Or PortalSecurity.FilterFlag.NoSQL)
                newValue = newValue.Trim()
                If newValue.Length > 0 AndAlso Regex.IsMatch(newValue, "[\040!*'();:@&=+$,?%#\[\]]") Then
                  newValue = HttpUtility.UrlEncode(newValue)
                End If
              End If
            End If
        End Select

        If Not String.IsNullOrEmpty(newValue) Then
          sanitizedParams.Add(key & "=" & newValue)
        End If
      Next

      Return NavigateURL(TabId, ctl, CType(sanitizedParams.ToArray(GetType(String)), String()))
    End Function

    '' Added By Quinn
    '' This is for valid ReturnCTL query string values. Any not in this list are dropped when using SanitizedRawUrl
    Public Shared ReturnCtlValues() As String = {"FileEdit", "Viewer", "AlbumEdit", "Maintenance"}

    Public Shared Function GetGalleryObjectMenuID(ByVal GalleryObject As IGalleryObjectInfo) As String
      Return String.Format("{0}.{1}", GalleryObject.Type, GalleryObject.ID)
    End Function

    Public Shared Function GetGalleryObjectMenuNodeID(ByVal GalleryObject As IGalleryObjectInfo) As String
      Return String.Format("{0}.{1}.{2}", GalleryObject.Type, GalleryObject.ID, GalleryObject.Index)
    End Function

  End Class

#Region "Comparer"

  ' Original class in C# by Diego Mijelshon
  ' Converted to generic form - 10-22-2010 by William Severance

  <Serializable()> _
  Public Class Comparer
    Implements IComparer(Of IGalleryObjectInfo)

    Public Function Compare(ByVal x As IGalleryObjectInfo, ByVal y As IGalleryObjectInfo) As Integer _
        Implements System.Collections.Generic.IComparer(Of IGalleryObjectInfo).Compare

      Dim typex As Type = x.GetType()
      Dim typey As Type = y.GetType()

      Dim i As Integer
      For i = 0 To MyFields.Length - 1
        Dim pix As PropertyInfo = typex.GetProperty(MyFields(i))
        Dim piy As PropertyInfo = typey.GetProperty(MyFields(i))

        Dim pvalx As IComparable = CType(pix.GetValue(x, Nothing), IComparable)
        Dim pvaly As Object = piy.GetValue(y, Nothing)

        'Compare values, using IComparable interface of the property's type
        Dim iResult As Integer = pvalx.CompareTo(pvaly)
        If iResult <> 0 Then
          'Return if not equal
          If MyDescending Then
            'Invert order
            Return -iResult
          Else
            Return iResult
          End If
        End If
      Next i
      'Objects have the same sort order
      Return 0
    End Function 'Compare

    Public Sub New(ByVal ParamArray fields() As String)
      MyClass.New(fields, False)
    End Sub 'New

    Public Sub New(ByVal Fields() As String, ByVal Descending As Boolean)
      MyFields = Fields
      MyDescending = Descending
    End Sub 'New

    Protected MyFields() As String
    Protected MyDescending As Boolean

  End Class 'GalleryComparer

#End Region

End Namespace
