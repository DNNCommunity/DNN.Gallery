<%@ Control language="vb" Inherits="DotNetNuke.Modules.Gallery.Voting" AutoEventWireup="false" Codebehind="Voting.ascx.vb" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
	<script type="text/javascript" language="javascript" src='<%= Page.ResolveUrl("DesktopModules/Gallery/Popup/gallerypopup.js") %>'></script>
	<table class="Gallery_Voting">
        <tr>
            <td class="Gallery_HeaderCapLeft"></td>
            <td id="tdTitle" runat="server" class="Gallery_HeaderImage" colspan="2">&nbsp;</td>
		    <td class="Gallery_HeaderCapRight"></td>
        </tr>
       	<tr>
            <td class="Gallery_RowCapLeft"></td>
			<td id="tdFilePreview" runat="server" class="Gallery_FileEditImage">
               <span><asp:image id="imgTitle" runat="server" />&nbsp;<asp:hyperlink id="lnkTitle" runat="server" cssclass="CommandButton"></asp:hyperlink></span>
               <asp:hyperlink id="lnkThumb" runat="server"></asp:hyperlink>
               <asp:image id="imgVoteSummary" runat="server" />
            </td>
			<td>
				<div id="divDetails" runat="server">
					<table>
						<tr>
							<td id="tdResult" runat="server" class="Gallery_AdminRowPanel">Rating Summary:</td>
							<td class="Gallery_AdminRow"><asp:label id="lblResult" runat="server" cssclass="Normal"></asp:label></td>
						</tr>
						<tr>
							<td id="tdName" runat="server" class="Gallery_AdminRowPanel">Name:</td>
							<td class="Gallery_AdminRow"><asp:label id="lblName" runat="server" cssclass="Normal"></asp:label></td>
						</tr>
						<tr>
							<td id="tdCreatedDate" runat="server" class="Gallery_AdminRowPanel">Created Date:</td>
							<td class="Gallery_AdminRow"><asp:label id="lblDate" runat="server" cssclass="Normal"></asp:label></td>
						</tr>
						<tr>
							<td id="tdAuthor" runat="server" class="Gallery_AdminRowPanel">Author:</td>
							<td class="Gallery_AdminRow"><asp:label id="lblAuthor" runat="server" cssclass="Normal"></asp:label></td>
						</tr>
						<tr>
							<td id="tdDescription" runat="server" class="Gallery_AdminRowPanel">Description:</td>
							<td class="Gallery_AdminRow"><asp:label id="lblDescription" runat="server" cssclass="Normal"></asp:label></td>
						</tr>
						<tr>
							<td class="Gallery_AdminRowPanel">&nbsp;</td>
							<td class="Gallery_AdminRow">
                                <asp:Linkbutton id="btnAddVote" runat="server" cssclass="CommandButton" resourcekey="btnAddVote" text="Add Vote"></asp:Linkbutton>
							</td>
						</tr>
					</table>
				</div>
				<div id="divAddVote" runat="server" visible="false">
					<table>
						<tr>
							<td id="tdYourRating" runat="server" class="Gallery_AdminRowPanel">Your Rating:</td>
							<td class="Gallery_AdminRow">
                                <asp:radiobuttonlist id="lstVote" runat="server" RepeatDirection="Vertical" RepeatLayout="Flow"></asp:radiobuttonlist>
                            </td>
						</tr>
						<tr>
							<td class="Gallery_AdminRowPanel">
                                <dnn:Label ID="plComments" runat="server" ControlName="txtComment" Text="Your Comments:" />
                            </td>
							<td class="Gallery_AdminRow">
                                    <asp:textbox id="txtComment" runat="server" cssclass="NormalTextBox" TextMode="MultiLine"></asp:textbox>
                            </td>
						</tr>
						<tr>
							<td class="Gallery_AdminRowPanel">&nbsp;</td>
							<td class="Gallery_RowPanel Centered">
                                <span class="Gallery_Commands">
                                    <asp:Linkbutton id="cmdCancel" runat="server" text="Cancel" cssclass="CommandButton" resourcekey="cmdCancel"></asp:Linkbutton>&nbsp;&nbsp;
									<asp:Linkbutton id="btnSave" runat="server" text="Update" cssclass="CommandButton" resourcekey="cmdUpdate"></asp:Linkbutton>
                                </span>
							</td>
						</tr>
					</table>
	            </div>
			</td>
            <td class="Gallery_RowCapRight"></td>
		</tr>
		<tr id="trVotes" runat="server" visible="false">
            <td class="Gallery_RowCapLeft"></td>
			<td class="Gallery_RecordedVotes" colspan="2">
                <asp:label ID="lblVotes" runat="server" CssClass="NormalBold"></asp:label><br /><br />
			    <asp:datalist id="dlVotes" runat="server" DataKeyField="FileName" EnableViewState="False" RepeatDirection="Vertical">
					<ItemTemplate>
						<table class="Gallery_RecordedVote">
							<tr>
								<td>
									<asp:label id="lblItemName" cssclass="NormalBold" runat="server" text='<%# GetUserName(Container.DataItem) %>'></asp:label>&nbsp;-&nbsp;
									<asp:label id="lblCreatedDate" cssclass="Normal" runat="server" text= '<%# DataBinder.Eval(Container.DataItem, "CreatedDate").ToShortDateString %>'></asp:label>
								</td>
								<td>
									<asp:Image id="imgScore" ImageUrl='<%# ScoreImage(Container.DataItem) %>' runat="server"></asp:Image>
								</td>
							</tr>
                            <tr>
                                <td class="Gallery_Comment" colspan="2">
                                    <asp:label id="lblComment" runat="server" text='<%# DataBinder.Eval(Container.DataItem, "Comment") %>'></asp:label>
                                </td>
                            </tr>
						</table>
					</ItemTemplate>
				</asp:datalist>
             </td>
             <td class="Gallery_RowCapRight"></td>
		</tr>
		<tr>
            <td class="Gallery_FooterCapLeft"></td>
			<td class="Gallery_FooterImage Centered" colspan="2">
				<span class="Gallery_Commands">
                    <asp:Linkbutton id="btnBack" runat="server" CssClass="CommandButton" ResourceKey="btnBack" Text="Back" CommandName="back"></asp:Linkbutton>
                </span>
            </td>
			<td class="Gallery_FooterCapRight"></td>
		</tr>
        <tr>
			<td class="Gallery_BottomCapLeft"></td>
			<td class="Gallery_Bottom" colspan="2"></td>
			<td class="Gallery_BottomCapRight"></td>
		</tr>
	</table>
