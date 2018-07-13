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
Imports System.Drawing
Imports System.Reflection
Imports System.Collections
Imports System.Text
Imports System.IO
Imports System.Web
Imports System.Web.UI
Imports System.Threading

Namespace DotNetNuke.Modules.Gallery

    Public Class Vote
        Private mFileName As String
        Private mUserID As Integer
        Private mScore As Integer
        Private mComment As String
        Private mCreatedDate As DateTime

        Sub New()
        End Sub

#Region "Properties"
        Public Property FileName() As String
            Get
                Return mFileName
            End Get
            Set(ByVal Value As String)
                mFileName = Value
            End Set
        End Property

        Public Property UserID() As Integer
            Get
                Return mUserID
            End Get
            Set(ByVal Value As Integer)
                mUserID = Value
            End Set
        End Property

        Public Property Score() As Integer
            Get
                Return mScore
            End Get
            Set(ByVal Value As Integer)
                mScore = Value
            End Set
        End Property

        Public Property Comment() As String
            Get
                Return mComment
            End Get
            Set(ByVal Value As String)
                mComment = Value
            End Set
        End Property

        Public Property CreatedDate() As DateTime
            Get
                Return mCreatedDate
            End Get
            Set(ByVal Value As DateTime)
                mCreatedDate = Value
            End Set
        End Property

        Public ReadOnly Property ScoreImageURL() As String
            Get
                Dim intScore As Integer = Score * 10
                Return Config.DefaultTheme & "/stars_" & intScore.ToString & ".gif"
            End Get
        End Property
#End Region
    End Class

    Public Class VoteCollection
        Inherits ArrayList

        Const GalleryVotingCacheKeyPrefix As String = "GalleryVoting"
        Private mParentPath As String
        Private mFileName As String
        Private mScore As Double
        Private mUserList As New ArrayList

        Sub New()
            MyBase.New()
        End Sub

        Sub New(ByVal ParentPath As String, ByVal FileName As String)
            MyBase.New()
            mParentPath = ParentPath
            mFileName = FileName
            PopulateCollection()
        End Sub

        Public Sub New(ByVal c As ICollection)
            MyBase.New(c)
        End Sub 'New

        Private Sub PopulateCollection()
            ' Get all of the votes
            Dim VoteData As New GalleryXML(mParentPath)
            Dim Votes As ArrayList = VoteData.GetVotings(mFileName)
            Me.AddRange(Votes)

            Dim totalScore As Integer = 0
            For Each voteItem As Vote In votes
                totalScore += voteItem.Score
                mUserList.Add(voteItem.UserID)
            Next
            mScore = totalScore / votes.Count
        End Sub

        Public Shared Function GetVoting(ByVal ParentPath As String, ByVal FileName As String) As VoteCollection
            Dim app As HttpApplicationState = HttpContext.Current.Application

            If app(GalleryVotingCacheKeyPrefix & "-" & ParentPath & "-" & FileName) Is Nothing Then
                Dim colVoting As New VoteCollection(ParentPath, FileName)
                app.Add(GalleryVotingCacheKeyPrefix & "-" & ParentPath & "-" & FileName, colVoting)
            End If

            Return CType(app.Item(GalleryVotingCacheKeyPrefix & "-" & ParentPath & "-" & FileName), VoteCollection)

        End Function 'GetVoting

        Public Shared Sub ResetCollection(ByVal ParentPath As String, ByVal FileName As String)
            Dim app As HttpApplicationState = HttpContext.Current.Application
            app.Remove(GalleryVotingCacheKeyPrefix & "-" & ParentPath & "-" & FileName)

        End Sub

        Public ReadOnly Property Score() As Double
            Get
                Return mScore
            End Get
        End Property

        Public ReadOnly Property UserList() As ArrayList
            Get
                Return mUserList
            End Get
        End Property

        Public Function UserCanVote(ByVal UserID As Integer) As Boolean
            If UserID > 0 Then
                Return (Not UserList.Contains(UserID))
            End If
            Return False
        End Function

        Public ReadOnly Property ScoreImage() As String
            Get
                Dim intScore As Integer = (CInt(Math.Floor(Score)) + CInt(Math.Ceiling(Score))) * 5
                Return Config.DefaultTheme & "/stars_" & intScore.ToString & ".gif"
            End Get
        End Property

    End Class

    Public Class GalleryVoteCollection
        Private mGalleryVoteList As New ArrayList

        Sub New(ByVal ModuleID As Integer)
            Dim mGalleryConfig As DotNetNuke.Modules.Gallery.Config = Config.GetGalleryConfig(ModuleID)
            Dim mRootFolder As GalleryFolder = mGalleryConfig.RootFolder
            mGalleryVoteList.Clear()

            PopuplateFolderVote(mRootFolder)

        End Sub

        Private Sub PopuplateFolderVote(ByVal Folder As GalleryFolder)
            Dim item As IGalleryObjectInfo

            For Each item In Folder.List
                Dim fileItem As GalleryFile
                Dim folderItem As GalleryFolder
                If Not item.IsFolder Then
                    fileItem = CType(item, GalleryFile)
                    mGalleryVoteList.AddRange(PopulateFileVote(fileItem))
                Else
                    folderItem = CType(item, GalleryFolder)
                    PopuplateFolderVote(folderItem)
                End If
            Next
        End Sub

        Private Function PopulateFileVote(ByVal File As GalleryFile) As ArrayList
            Dim colFileVote As VoteCollection = VoteCollection.GetVoting(File.Parent.Path, File.Name)
            Return colFileVote
        End Function

        Public ReadOnly Property GalleryVoteList() As ArrayList
            Get
                Return mGalleryVoteList
            End Get
        End Property
    End Class

End Namespace