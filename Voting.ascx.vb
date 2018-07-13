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

Imports System.IO
Imports DotNetNuke
Imports DotNetNuke.Security
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Entities.Modules.Actions
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.Entities.Users
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Modules.Gallery.Utils
Imports DotNetNuke.Modules.Gallery.Config

Namespace DotNetNuke.Modules.Gallery

  Public MustInherit Class Voting
    Inherits DotNetNuke.Entities.Modules.PortalModuleBase
    Implements DotNetNuke.Entities.Modules.IActionable

    Private Const maxStars As Integer = 5       'WES - Later make a configuration setting?

    ' Obtain PortalSettings from Current Context   
    Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings

    Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config
    Private mRequest As GalleryRequest
    Private mFolder As GalleryFolder
    Private mFile As GalleryFile
    Private mMode As String = ""
    Private mVoteCollection As New VoteCollection
    Private mUserID As Integer = 0
    Private mVoteText As String = "{0} out of {1} stars"

#Region " Web Form Designer Generated Code "

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

    End Sub

    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
      'CODEGEN: This method call is required by the Web Form Designer
      'Do not modify it using the code editor.
      InitializeComponent()

      If mGalleryConfig Is Nothing Then
        mGalleryConfig = GetGalleryConfig(ModuleId)
      End If
    End Sub

#End Region

#Region "Optional Interfaces"

    Public ReadOnly Property ModuleActions() As ModuleActionCollection Implements Entities.Modules.IActionable.ModuleActions
      Get
        Dim Actions As New ModuleActionCollection
        Actions.Add(GetNextActionID, Localization.GetString("Configuration.Action", LocalResourceFile), ModuleActionType.ModuleSettings, "", "edit.gif", EditUrl("configuration"), "", False, SecurityAccessLevel.Admin, True, False)
        Actions.Add(GetNextActionID, Localization.GetString("GalleryHome.Action", LocalResourceFile), "", "", "icon_moduledefinitions_16px.gif", NavigateURL(), False, SecurityAccessLevel.Anonymous, True, False)
        Return Actions
      End Get
    End Property

#End Region

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
      'Put user code to initialize the page here

      ' Load the styles
      DirectCast(Page, CDefault).AddStyleSheet(CreateValidID(GalleryConfig.Css()), GalleryConfig.Css())

      If Not Request.QueryString("mode") Is Nothing Then
        mMode = Request.QueryString("mode")
      End If

      mRequest = New GalleryRequest(ModuleId)
      mFolder = mRequest.Folder

      If Not Request.QueryString("currentitem") Is Nothing Then
        Dim mCurrentItem As Integer = Int32.Parse(Request.QueryString("currentitem"))
        mFile = CType(mRequest.Folder.List.Item(mCurrentItem), GalleryFile)
        mVoteCollection = mFile.Votes
      End If

      mUserID = UserController.GetCurrentUserInfo.UserID

      'WES - Added localization

      If Not IsPostBack Then
        If mMode = "add" Then
          mVoteText = Localization.GetString("Vote", LocalResourceFile)
          tdTitle.InnerText = Localization.GetString("Voting", LocalResourceFile)
          tdYourRating.InnerText = Localization.GetString("Your_Rating", LocalResourceFile)
        Else
          tdTitle.InnerText = Localization.GetString("Rating", LocalResourceFile)
          tdResult.InnerText = Localization.GetString("Rating_Summary", LocalResourceFile)
          tdName.InnerText = Localization.GetString("Name", LocalResourceFile)
          tdCreatedDate.InnerText = Localization.GetString("Created_Date", LocalResourceFile)
          tdAuthor.InnerText = Localization.GetString("Author", LocalResourceFile)
          tdDescription.InnerText = Localization.GetString("Description", LocalResourceFile)
        End If

        If mGalleryConfig.IsValidPath Then
          BindData()
          If Not ViewState("UrlReferrer") Is Nothing Then
            ViewState("UrlReferrer") = Request.UrlReferrer.ToString()
          End If
        End If
      End If
    End Sub

    Private Sub BindData()

      tdFilePreview.Style.Add("width", (GalleryConfig.MaximumThumbWidth + 50).ToString & "px")
      imgTitle.ImageUrl = mFile.IconURL
      imgVoteSummary.ImageUrl = mFile.ScoreImageURL

      lnkTitle.NavigateUrl = mFile.BrowserURL
      lnkTitle.Text = mFile.Title

      lnkThumb.NavigateUrl = mFile.BrowserURL
      lnkThumb.ImageUrl = mFile.ThumbnailURL
      lnkThumb.ToolTip = mFile.GetItemInfo(False)

      btnAddVote.Visible = mVoteCollection.UserCanVote(mUserID)
      If mMode = "add" Then
        divAddVote.Visible = True
        divDetails.Visible = False
        BindVoteList()
      Else
        BindInfo()
      End If

      If mVoteCollection.Count > 0 Then
        trVotes.Visible = True
        lblVotes.Text = Localization.GetString("RatingsAndComments", LocalResourceFile)
        dlVotes.DataSource = mVoteCollection
        dlVotes.DataBind()
      Else
        trVotes.Visible = False
      End If

    End Sub

    Private Sub BindInfo()
      Me.lblName.Text = mFile.Name
      Dim currentRating As String = String.Empty
      If mVoteCollection.Count = 0 Then
        currentRating = Localization.GetString("Not_Rated.Text", LocalResourceFile)
      Else
        currentRating = String.Format(Localization.GetString("Result.Text", LocalResourceFile), mFile.Score, maxStars, mVoteCollection.Count)
      End If
      lblResult.Text = currentRating
      imgVoteSummary.ToolTip = currentRating
      imgVoteSummary.AlternateText = currentRating

      Me.lblAuthor.Text = mFile.Author
      Me.lblDate.Text = mFile.CreatedDate.ToShortDateString
      Me.lblDescription.Text = mFile.Description
    End Sub

    Private Sub BindVoteList()
      lstVote.Items.Clear()

      Dim intCount As Integer

      For intCount = maxStars To 1 Step -1
        Dim altTitle As String = String.Format(mVoteText, intCount, maxStars)
        Dim imgURL As String = "<img src='DesktopModules/Gallery/Themes/" & DefaultTheme & "/Images/stars_" _
                                   & (intCount * 10).ToString & ".gif' alt='" & altTitle & "' title='" & altTitle & "' />"
        Dim lstItem As New ListItem(imgURL, intCount.ToString)
        lstVote.Items.Add(lstItem)
      Next
      lstVote.SelectedIndex = 0
    End Sub

    Private Sub Goback()
      Dim url As String = GetURL(Page.Request.ServerVariables("URL"), Page, "", "ctl=&mode=&mid=&media=&currentitem=")
      Response.Redirect(url)
    End Sub

    Protected Function GetUserName(ByVal DataItem As Object) As String
      Dim voteItem As Vote = CType(DataItem, Vote)
      Return New UserController().GetUser(_portalSettings.PortalId, voteItem.UserID).DisplayName
    End Function

    Protected Function ScoreImage(ByVal DataItem As Object) As String
      Dim objVote As Vote = CType(DataItem, Vote)
      Dim intScore As Integer = objVote.Score * 10
      Dim imageName As String = "stars_" & intScore.ToString & ".gif"
      Return mGalleryConfig.GetImageURL(imageName)
    End Function

    Public ReadOnly Property GalleryConfig() As DotNetNuke.Modules.Gallery.Config
      Get
        Return mGalleryConfig
      End Get
    End Property

    Private Overloads Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
      Dim newVote As New Vote
      Dim Security As New PortalSecurity

      With newVote
        .UserID = mUserID
        .FileName = mFile.Name
        .CreatedDate = DateTime.Today
        .Score = Int16.Parse(lstVote.SelectedItem.Value)
        'WES - Added InputFilter to strip markup and SQL
        .Comment = Security.InputFilter(txtComment.Text, PortalSecurity.FilterFlag.NoMarkup Or PortalSecurity.FilterFlag.NoScripting)
      End With
      mFile.UpdateVotes(newVote)
      Goback()
    End Sub

    Private Overloads Sub cmdCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCancel.Click
      'Modified by WES to permit cancelation of voting without returning back to gallery
      Dim url As String = GetURL(Page.Request.ServerVariables("URL"), Page, "", "mode=")
      Response.Redirect(url)
    End Sub

    Private Overloads Sub btnAddVote_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddVote.Click
      Dim url As String = GetURL(Page.Request.ServerVariables("URL"), Page, "mode=add", "")
      Response.Redirect(url)
    End Sub

    Private Overloads Sub btnBack_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBack.Click
      Goback()
    End Sub

  End Class

End Namespace

