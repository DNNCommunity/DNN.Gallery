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

Imports System.Xml
Imports System.IO
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Common.Utilities
Imports DotNetNuke.Modules.Gallery.Utils

Namespace DotNetNuke.Modules.Gallery

  Public Class GalleryXML

    ' XML Filename
    Const xmlFileName As String = "_metadata.resources"
    Private xml As XPath.XPathDocument
    Private xpath As XPath.XPathNavigator

    Private bmetaDataExists As Boolean = False

    ' Determines whether the metadata files exists; allows us to speed up processing
    ' if it doesn't
    Public ReadOnly Property MetaDataExists() As Boolean
      Get
        Return bmetaDataExists
      End Get
    End Property

    ' Creator function
    Public Sub New(ByVal Directory As String)
      Dim xmlPath As String = BuildPath(New String(1) {Directory, xmlFileName}, "\", False, True)

      ' Check for existence of file and build if necessary
      If Not File.Exists(xmlPath) Then
        CreateMetaDataFile(Directory)
      End If

      ' Often if xml file gets corrupted exception will fire so now we just recreate the file on exception
      ' Find out if metadata file exists; if it does, init some vars
      If File.Exists(xmlPath) Then
        bmetaDataExists = True
        Try
          ' Open it
          xml = New XPath.XPathDocument(BuildPath(New String(1) {Directory, xmlFileName}, "\", False, True))
        Catch ex As Exception
          ' Error... Try deleting it, creating it, and reopening it
          File.Delete(xmlPath)
          CreateMetaDataFile(Directory)
          xml = New XPath.XPathDocument(BuildPath(New String(1) {Directory, xmlFileName}, "\", False, True))
        End Try

        xpath = xml.CreateNavigator()
      End If

    End Sub

    ' Gets the folder or file ID data for a given filename in a folder
    Public Function ID(ByVal FileName As String) As Integer

      If bmetaDataExists Then
        ' Only run this if we have an XML file

        Dim iterator As XPath.XPathNodeIterator

        ' Get the iterator object for given Xpath search
        iterator = xpath.Select("/files/file[@name=""" & FileName & """]/id")

        If iterator.MoveNext() Then
          ' If we moved, then we retrieved a result
          Return Int32.Parse(iterator.Current.Value())
        Else
          Return -1
        End If

      Else
        Return -1
      End If

    End Function

    ' Gets the title data for a given filename is a folder
    Public Function Title(ByVal FileName As String) As String

      If bmetaDataExists Then
        ' Only run this if we have an XML file

        Dim iterator As XPath.XPathNodeIterator

        ' Get the iterator object for given Xpath search
        iterator = xpath.Select("/files/file[@name=""" & FileName & """]/title")

        If iterator.MoveNext() Then
          ' If we moved, then we retrieved a result
          Return iterator.Current.Value()
        Else
          Return Null.NullString
        End If

      Else
        Return Null.NullString
      End If

    End Function

    ' Gets the description data for a given filename is a folder
    Public Function Description(ByVal FileName As String) As String

      If bmetaDataExists Then
        ' Only run this if we have an XML file

        Dim iterator As XPath.XPathNodeIterator

        ' Get the iterator object for given Xpath search
        iterator = xpath.Select("/files/file[@name=""" & FileName & """]/description")

        If iterator.MoveNext() Then
          ' If we moved, then we retrieved a result
          Return iterator.Current.Value()
        Else
          Return Null.NullString
        End If

      Else
        Return Null.NullString
      End If

    End Function

    ' Gets the category data for a given filename is a folder
    Public Function Categories(ByVal FileName As String) As String

      If bmetaDataExists Then
        ' Only run this if we have an XML file

        Dim iterator As XPath.XPathNodeIterator

        ' Get the iterator object for given Xpath search
        iterator = xpath.Select("/files/file[@name=""" & FileName & """]/categories")

        If iterator.MoveNext() Then
          ' If we moved, then we retrieved a result
          Return iterator.Current.Value()
        Else
          Return Null.NullString
        End If

      Else
        Return Null.NullString
      End If

    End Function

    Public Function Author(ByVal FileName As String) As String

      If bmetaDataExists Then
        ' Only run this if we have an XML file

        Dim iterator As XPath.XPathNodeIterator

        ' Get the iterator object for given Xpath search
        iterator = xpath.Select("/files/file[@name=""" & FileName & """]/author")

        If iterator.MoveNext() Then
          ' If we moved, then we retrieved a result
          Return iterator.Current.Value()
        Else
          Return Null.NullString
        End If

      Else
        Return Null.NullString
      End If

    End Function

    Public Function Location(ByVal FileName As String) As String

      If bmetaDataExists Then
        ' Only run this if we have an XML file

        Dim iterator As XPath.XPathNodeIterator

        ' Get the iterator object for given Xpath search
        iterator = xpath.Select("/files/file[@name=""" & FileName & """]/location")

        If iterator.MoveNext() Then
          ' If we moved, then we retrieved a result
          Return iterator.Current.Value()
        Else
          Return Null.NullString
        End If

      Else
        Return Null.NullString
      End If

    End Function

    Public Function Client(ByVal FileName As String) As String

      If bmetaDataExists Then
        ' Only run this if we have an XML file

        Dim iterator As XPath.XPathNodeIterator

        ' Get the iterator object for given Xpath search
        iterator = xpath.Select("/files/file[@name=""" & FileName & """]/client")

        If iterator.MoveNext() Then
          ' If we moved, then we retrieved a result
          Return iterator.Current.Value()
        Else
          Return Null.NullString
        End If

      Else
        Return Null.NullString
      End If

    End Function

    ' Gets the owner data for a given filename is a folder
    Public Function OwnerID(ByVal FileName As String) As Integer

      If bmetaDataExists Then
        ' Only run this if we have an XML file

        Dim iterator As XPath.XPathNodeIterator

        ' Get the iterator object for given Xpath search
        iterator = xpath.Select("/files/file[@name=""" & FileName & """]/ownerid")

        If iterator.MoveNext() Then
          ' If we moved, then we retrieved a result
          ' William Severance - Added check for empty value and replacement by default -1
          Dim s As String = iterator.Current.Value()
          If Not String.IsNullOrEmpty(s) Then Return Integer.Parse(s)
        End If
      End If
      Return -1 'Default OwnerID for non-specified, non-private gallery object
    End Function

    ' Gets the owner data for a given filename is a folder
    Public Function CreatedDate(ByVal FileName As String) As DateTime

      If bmetaDataExists Then
        ' Only run this if we have an XML file

        Dim iterator As XPath.XPathNodeIterator

        ' Get the iterator object for given Xpath search
        iterator = xpath.Select("/files/file[@name=""" & FileName & """]/createddate")

        If iterator.MoveNext() Then
          ' If we moved, then we retrieved a result
          ' Add a try around the date conversion in case it fails
          Try
            Return XmlConvert.ToDateTime(iterator.Current.Value(), XmlDateTimeSerializationMode.Local)
          Catch
            Return Null.NullDate
          End Try
        Else
          Return Null.NullDate
        End If

      Else
        Return Null.NullDate
      End If

    End Function

    ' Gets the approved date data for a given filename in a folder
    Public Function ApprovedDate(ByVal FileName As String) As DateTime

      If bmetaDataExists Then
        ' Only run this if we have an XML file

        Dim iterator As XPath.XPathNodeIterator

        ' Get the iterator object for given Xpath search
        iterator = xpath.Select("/files/file[@name=""" & FileName & """]/approveddate")

        If iterator.MoveNext() Then
          ' If we moved, then we retrieved a result
          ' Added a try around the date conversion in case it fails
          Try
            Return XmlConvert.ToDateTime(iterator.Current.Value(), XmlDateTimeSerializationMode.Local)
          Catch
            Return Date.MaxValue
          End Try
        Else
          Return Date.MaxValue ' <to be used with approval feature>
        End If

      Else
        Return Date.MaxValue
      End If

    End Function

    ' Gets the approved date data for a given filename is a folder
    Public Function Score(ByVal FileName As String) As Double

      If bmetaDataExists Then
        ' Only run this if we have an XML file

        Dim iterator As XPath.XPathNodeIterator

        ' Get the iterator object for given Xpath search
        iterator = xpath.Select("/files/file[@name=""" & FileName & """]/score")

        If iterator.MoveNext() Then
          ' If we moved, then we retrieved a result
          Return Convert.ToDouble(iterator.Current.Value())
        Else
          Return 0
        End If

      Else
        Return 0
      End If

    End Function

    Public Shared Sub DeleteMetaData(ByVal Directory As String, ByVal Name As String)
      Dim xml As New XmlDocument
      Dim root, fileNode As XmlNode

      ' Check for existence of file 
      If Not File.Exists(BuildPath(New String(1) {Directory, xmlFileName}, "\", False, True)) Then
        Return
      End If

      ' Load the XML and init a few items
      xml.Load(BuildPath(New String(1) {Directory, xmlFileName}, "\", False, True))
      root = xml.DocumentElement
      fileNode = xml.SelectSingleNode("/files/file[@name=""" & Name & """]")

      If Not fileNode Is Nothing Then
        root.RemoveChild(fileNode)
      End If

      ' Now save the file back so that the updates are saved.
      xml.Save(BuildPath(New String(1) {Directory, xmlFileName}, "\", False, True))
    End Sub

    ''' <summary>
    ''' Save the metadata to the XML file
    ''' </summary>
    ''' <param name="Directory"></param>
    ''' <param name="FileName"></param>
    ''' <param name="ID"></param>
    ''' <param name="Title"></param>
    ''' <param name="Description"></param>
    ''' <param name="Categories"></param>
    ''' <param name="Author"></param>
    ''' <param name="Location"></param>
    ''' <param name="Client"></param>
    ''' <param name="OwnerID"></param>
    ''' <param name="CreatedDate"></param>
    ''' <param name="ApprovedDate"></param>
    ''' <param name="Score"></param>
    ''' <remarks></remarks>
    Public Shared Sub SaveMetaData( _
        ByVal Directory As String, _
        ByVal FileName As String, _
        ByVal ID As Integer, _
        ByVal Title As String, _
        ByVal Description As String, _
        ByVal Categories As String, _
        ByVal Author As String, _
        ByVal Location As String, _
        ByVal Client As String, _
        ByVal OwnerID As Integer, _
        ByVal CreatedDate As DateTime, _
        ByVal ApprovedDate As DateTime, _
        ByVal Score As Double)

      Dim root, fileNode, newNode, subNode As XmlNode
      Dim attr As XmlAttribute
      Dim xml As New XmlDocument

      ' Check for existence of file and build if necessary
      If Not File.Exists(BuildPath(New String(1) {Directory, xmlFileName}, "\", False, True)) Then
        CreateMetaDataFile(Directory)
      End If

      ' Load the XML and init a few items
      xml.Load(BuildPath(New String(1) {Directory, xmlFileName}, "\", False, True))
      root = xml.DocumentElement

      ' Regardless of the circumstance, we need to create a new node for the update
      newNode = xml.CreateElement("file")

      attr = xml.CreateAttribute("name")
      attr.Value = FileName
      newNode.Attributes.Append(attr)

      If ID <= 0 Then ' New item
        Dim rd As New System.Random
        ID = rd.Next
      End If

      subNode = xml.CreateElement("id")
      subNode.InnerText = ID.ToString
      newNode.AppendChild(subNode)

      subNode = xml.CreateElement("title")
      subNode.InnerText = Title
      newNode.AppendChild(subNode)

      subNode = xml.CreateElement("description")
      subNode.InnerText = Description
      newNode.AppendChild(subNode)

      subNode = xml.CreateElement("categories")
      subNode.InnerText = Categories
      newNode.AppendChild(subNode)

      subNode = xml.CreateElement("author")
      subNode.InnerText = Author
      newNode.AppendChild(subNode)

      subNode = xml.CreateElement("location")
      subNode.InnerText = Location
      newNode.AppendChild(subNode)

      subNode = xml.CreateElement("client")
      subNode.InnerText = Client
      newNode.AppendChild(subNode)

      subNode = xml.CreateElement("ownerid")
      subNode.InnerText = OwnerID.ToString
      newNode.AppendChild(subNode)

      ' Write date in XML format
      subNode = xml.CreateElement("createddate")
      subNode.InnerText = XmlConvert.ToString(CreatedDate, XmlDateTimeSerializationMode.Local)   ' ISO 8601 date format
      newNode.AppendChild(subNode)

      ' Write date in XML format
      subNode = xml.CreateElement("approveddate")
      subNode.InnerText = XmlConvert.ToString(ApprovedDate, XmlDateTimeSerializationMode.Local)  ' ISO 8601 date format
      newNode.AppendChild(subNode)

      subNode = xml.CreateElement("score")
      subNode.InnerText = Score.ToString
      newNode.AppendChild(subNode)

      ' We now have the complete node to add/update

      ' Look for the existence of this node already
      fileNode = xml.SelectSingleNode("/files/file[@name=""" & FileName & """]")

      If fileNode Is Nothing Then
        ' Node was not found.  Add it
        root.AppendChild(newNode)
      Else
        ' Node was found.  Need to update instead
        root.ReplaceChild(newNode, fileNode)
      End If

      ' Now save the file back so that the updates are saved.
      xml.Save(BuildPath(New String(1) {Directory, xmlFileName}, "\", False, True))
    End Sub

    Private Shared Sub AddCatetory(ByVal Directory As String, ByVal Value As String)

      Dim root, catNode, newNode, oldNode As XmlNode
      Dim attr As XmlAttribute
      Dim xml As New XmlDocument

      ' Check for existence of file and build if necessary
      If Not File.Exists(BuildPath(New String(1) {Directory, xmlFileName}, "\", False, True)) Then
        CreateMetaDataFile(Directory)
      End If

      ' Load the XML and init a few items
      xml.Load(BuildPath(New String(1) {Directory, xmlFileName}, "\", False, True))
      root = xml.DocumentElement
      catNode = root.SelectSingleNode("categories")

      'Regardless of the circumstance, we need to create a new node for the update
      newNode = xml.CreateElement("category")

      attr = xml.CreateAttribute("value")
      attr.Value = Value
      newNode.Attributes.Append(attr)

      ' We now have the complete node to add/update

      ' Look for the existence of this node already
      oldNode = root.SelectSingleNode("/categories/category[@value='" & Value & "']")

      If oldNode Is Nothing Then
        ' Node was not found.  Add it
        catNode.AppendChild(newNode)
      Else
        ' Node was found.  Need to update instead
        catNode.ReplaceChild(newNode, oldNode)
      End If

      ' Now save the file back so that the updates are saved.
      xml.Save(BuildPath(New String(1) {Directory, xmlFileName}, "\", False, True))
    End Sub

    Public Shared Sub AddVote(ByVal Directory As String, ByVal vote As Vote)

      Dim root, fileVotesNode, newNode As XmlNode
      Dim attrName, attrUserID, attrScore, attrCreatedDate As XmlAttribute
      Dim xml As New XmlDocument

      ' Load the XML and init a few items
      xml.Load(BuildPath(New String(1) {Directory, xmlFileName}, "\", False, True))
      root = xml.DocumentElement
      ' JIMJ declare it here, until we are finally are ready for it
      'Dim fileNode As XmlNode
      'fileNode = root.SelectSingleNode("files")

      If root.SelectSingleNode("votes[@name='" & vote.FileName & "']") Is Nothing Then
        Dim newVotesNode As XmlNode = xml.CreateElement("votes")
        attrName = xml.CreateAttribute("name")
        attrName.Value = vote.FileName
        newVotesNode.Attributes.Append(attrName)
        root.AppendChild(newVotesNode)

        fileVotesNode = newVotesNode
      Else
        fileVotesNode = root.SelectSingleNode("votes[@name='" & vote.FileName & "']")
      End If

      'fileVotesNode = fileNode.SelectSingleNode("files/votes[@name='" & vote.FileName & "']")

      'Dim xml As New XmlDocument
      'Dim rootNode, newNode As XmlNode

      '' Create the declaration node and root node and add them
      'newNode = xml.CreateNode(XmlNodeType.XmlDeclaration, "xml", Nothing)
      'rootNode = xml.CreateElement("files")

      'xml.AppendChild(newNode)
      'xml.AppendChild(rootNode)

      ' create a new node for the update
      newNode = xml.CreateElement("vote")

      attrUserID = xml.CreateAttribute("userid")
      attrUserID.Value = vote.UserID.ToString
      newNode.Attributes.Append(attrUserID)

      attrScore = xml.CreateAttribute("score")
      attrScore.Value = vote.Score.ToString
      newNode.Attributes.Append(attrScore)

      attrCreatedDate = xml.CreateAttribute("createddate")
      attrCreatedDate.Value = XmlConvert.ToString(vote.CreatedDate, XmlDateTimeSerializationMode.Local)  ' ISO 8601 date format
      newNode.Attributes.Append(attrCreatedDate)

      newNode.InnerText = vote.Comment

      'newNode.Value = vote.Comment

      ' We now have the complete node to add/update
      fileVotesNode.AppendChild(newNode)

      ' Now save the file back so that the updates are saved.
      xml.Save(BuildPath(New String(1) {Directory, xmlFileName}, "\", False, True))

    End Sub

    Public Function GetVotings(ByVal FileName As String) As ArrayList
      ' Declare so that it always exists
      Dim voteList As New ArrayList

      If bmetaDataExists Then
        ' Only run this if we have an XML file
        Dim iterator As XPath.XPathNodeIterator
        Dim i As Integer

        ' Get the iterator object for given Xpath search
        iterator = xpath.Select("/files/votes[@name=""" & FileName & """]")
        If iterator.MoveNext Then
          Dim childIterator As XPath.XPathNodeIterator = iterator.Current.SelectChildren("vote", "")

          For i = 0 To childIterator.Count '.Current.SelectChildren(XPath.XPathNodeType.Element .Count - 1
            If childIterator.MoveNext Then
              'Dim voteIterator As xpath.XPathNodeIterator = childIterator.Current.SelectChildren("vote", "")
              Dim voteNav As XPath.XPathNavigator = childIterator.Current

              Dim vote As New Vote
              With vote
                .UserID = Integer.Parse(GetValue(voteNav.GetAttribute("userid", ""), "0"))
                .Score = Int16.Parse(GetValue(voteNav.GetAttribute("score", ""), "0"))
                Try
                  .CreatedDate = XmlConvert.ToDateTime(GetValue(voteNav.GetAttribute("createddate", ""), XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.Local)), XmlDateTimeSerializationMode.Local)
                Catch ex As Exception
                  ' Couldn't convert date, use nothing
                  .CreatedDate = Null.NullDate
                End Try
                .Comment = GetValue(voteNav.Value, "")
              End With
              voteList.Add(vote)
            End If
          Next
        End If
      End If

      ' Return a new'd votelist even if empty
      Return voteList
    End Function

    'William Severance - Does not appear to be called from anywhere in project. Leave as is for now.
    Private Shared Sub AddGalleryOwner(ByVal Directory As String, ByVal Value As String)
      Dim root, newNode, oldNode As XmlNode
      Dim xml As New XmlDocument

      ' Check for existence of file and build if necessary
      If Not File.Exists(BuildPath(New String(1) {Directory, xmlFileName}, "\", False, True)) Then
        CreateMetaDataFile(Directory)
      End If

      ' Load the XML and init a few items
      xml.Load(BuildPath(New String(1) {Directory, xmlFileName}, "\", False, True))
      root = xml.DocumentElement

      'Regardless of the circumstance, we need to create a new node for the update
      newNode = xml.CreateElement("owner")
      newNode.InnerText = Value

      ' We now have the complete node to add/update

      ' Look for the existence of this node already
      oldNode = root.SelectSingleNode("owner")

      If oldNode Is Nothing Then
        ' Node was not found.  Add it
        root.AppendChild(newNode)
      Else
        ' Node was found.  Need to update instead
        root.ReplaceChild(newNode, oldNode)
      End If

      ' Now save the file back so that the updates are saved.
      xml.Save(BuildPath(New String(1) {Directory, xmlFileName}, "\", False, True))
    End Sub

    Private Shared Sub CreateMetaDataFile(ByVal Directory As String)
      ' Creates new XML metadata file for a particular folder
      Dim xml As New XmlDocument
      Dim rootNode, newNode As XmlNode

      ' Create the declaration node and root node and add them
      newNode = xml.CreateNode(XmlNodeType.XmlDeclaration, "xml", Nothing)
      rootNode = xml.CreateElement("files")

      xml.AppendChild(newNode)
      xml.AppendChild(rootNode)

      ' Add error logging around the save as that is where it fails
      ' When the directory/file doesn't have permissions
      ' Now, save the file
      Try
        xml.Save(BuildPath(New String(1) {Directory, xmlFileName}, "\", False, True))
      Catch ex As Exception
        LogException(ex)
        Throw
      End Try
    End Sub

  End Class

End Namespace