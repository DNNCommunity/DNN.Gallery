<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Control language="vb" Inherits="DotNetNuke.Modules.Gallery.FileEdit" AutoEventWireup="false" Codebehind="FileEdit.ascx.vb" %>
<%@ Reference Control="~/DesktopModules/Gallery/Controls/ControlBreadCrumbs.ascx" %>
<!-- Colling meta added to remove xhtml validation errors - needs to be removed in DDN 5.0 GAL8522 - HWZassenhaus -->
	<meta http-equiv="Content-Style-Type" content="text/css"/>
<script type="text/javascript" language="javascript" src='<%= Page.ResolveUrl("DesktopModules/Gallery/Popup/gallerypopup.js") %>'></script>
	<table class="Gallery_AdminContainer" id="tblMain">
        <tr id="trNavigation" runat="server">
			<td class="Gallery_HeaderCapLeft" id="celHeaderLeft" runat="server"><img src='<%=Page.ResolveUrl(GalleryConfig.GetImageURL("spacer_left.gif"))%>' alt="" /></td>
			<td class="Gallery_HeaderImage" id="celBreadcrumbs" runat="server" colspan="2" ></td>
			<td class="Gallery_HeaderCapRight" id="celHeaderRight"><img src='<%=Page.ResolveUrl(GalleryConfig.GetImageURL("spacer_left.gif"))%>' alt="" /></td>
		</tr>
		<tr>
            <td class="Gallery_AltHeaderCapLeft">&nbsp;</td>
			<td class="Gallery_AltHeaderImage" colspan="2">
                <asp:Label ID="lblInfo" runat="server" CssClass="Gallery_AltHeaderText"></asp:Label>
            </td>
            <td class="Gallery_AltHeaderCapRight">&nbsp;</td>
		</tr>
		<tr id="rowInfo" runat="server">
            <td class="Gallery_RowCapLeft"></td>
			<td class="Gallery_Row">
				<table id="tblDetails" class="Gallery_AdminSection">
					<tr>
						<td class="Gallery_AdminRowPanel">
							<dnn:label id="plPath" text="Path:" runat="server" controlname="txtPath"></dnn:label>
						</td>
						<td class="Gallery_AdminRow">
							<asp:TextBox id="txtPath" runat="server" cssclass="NormalTextBox" enabled="False"></asp:TextBox>
						</td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel">
							<dnn:label id="plName" text="Name:" runat="server" controlname="txtName"></dnn:label>
						</td>
						<td class="Gallery_AdminRow">
							<asp:textbox id="txtName" runat="server" enabled="False" cssclass="NormalTextBox"></asp:textbox>
						</td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel">
							<dnn:label id="plTitle" text="Title:" runat="server" controlname="txtTitle"></dnn:label>
						</td>
						<td class="Gallery_AdminRow">
							<asp:textbox id="txtTitle" runat="server" cssclass="NormalTextBox"></asp:textbox>
						</td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel">
							<dnn:label id="plAuthor" text="Author:" runat="server" controlname="txtAuthor"></dnn:label>
						</td>
						<td class="Gallery_AdminRow" align="left">
							<asp:textbox id="txtAuthor" runat="server" cssclass="NormalTextBox"></asp:textbox>
						</td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel">
							<dnn:label id="plClient" text="Client:" runat="server" controlname="txtClient"></dnn:label>
						</td>
						<td class="Gallery_AdminRow">
							<asp:textbox id="txtClient" runat="server" cssclass="NormalTextBox"></asp:textbox>
						</td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel">
							<dnn:label id="plLocation" text="Location:" runat="server" controlname="txtLocation"></dnn:label>
						</td>
						<td class="Gallery_AdminRow">
							<asp:textbox id="txtLocation" runat="server" cssclass="NormalTextBox"></asp:textbox>
						</td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel TopAlign">&nbsp;
							<dnn:label id="plDescription" text="Description:" runat="server" controlname="txtDescription"></dnn:label>
						</td>
						<td class="Gallery_AdminRow">
							<asp:textbox id="txtDescription" runat="server" cssclass="NormalTextBox" textmode="MultiLine" Rows="3"></asp:textbox>
						</td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel">&nbsp;
							<dnn:label id="plCreatedDate" text="Created Date:" runat="server" controlname="txtCreatedDate"></dnn:label>
						</td>
						<td class="Gallery_AdminRow">
							<asp:TextBox id="txtCreatedDate" runat="server" cssclass="NormalTextBox" width="150px"></asp:TextBox>
							<asp:hyperlink id="cmdCreatedDate" runat="server" imageurl="~/DesktopModules/Gallery/Images/m_calendar.gif"
							    resourcekey="cmdCreatedDate"></asp:hyperlink>
							<asp:rangevalidator ID="valCreatedDate" runat="server" CssClass="NormalRed" ControlToValidate="txtCreatedDate" Type="Date"
							      ErrorMessage="Invalid Date" ResourceKey="valInvalidDate.ErrorMessage" Display="Dynamic"></asp:rangevalidator>
						</td>
					</tr>
					<tr id="rowApprovedDate" runat="server">
						<td class="Gallery_AdminRowPanel">
							<dnn:label id="plApprovedDate" text="ApprovedDate:" runat="server" controlname="txtApprovedDate"></dnn:label>
						</td>
						<td class="Gallery_AdminRow">
							<asp:textbox id="txtApprovedDate" runat="server" width="150px" cssclass="NormalTextBox"></asp:textbox>
							<asp:hyperlink id="cmdApprovedDate" runat="server" imageurl="~/DesktopModules/Gallery/Images/m_calendar.gif"
								resourcekey="cmdApprovedDate"></asp:hyperlink>
						    <asp:rangevalidator ID="valApprovedDate" runat="server" CssClass="NormalRed" ControlToValidate="txtApprovedDate" Type="Date"
							      ErrorMessage="Invalid Date" ResourceKey="valInvalidDate.ErrorMessage" Display="Dynamic"></asp:rangevalidator>						
						</td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel TopAlign">
							<dnn:label id="plCategories" text="Categories:" runat="server" controlname="lstCategories"></dnn:label>
						</td>
						<td class="Gallery_AdminRow">
							<asp:checkboxlist id="lstCategories" cssclass="Normal" runat="server" RepeatDirection="Vertical" repeatcolumns="2"></asp:checkboxlist>
						</td>
					</tr>
				</table>
			</td>
			<td id="tdFileEditImage" class="Gallery_FileEditImage" runat="server">
                   <span><asp:hyperlink id="cmdMovePrevious" runat="server"></asp:hyperlink>&nbsp;&nbsp;
				         <asp:label id="lblCurrentFile" runat="server" cssClass="Gallery_AltHeaderText"></asp:label>&nbsp;&nbsp;
                         <asp:hyperlink id="cmdMoveNext" runat="server"></asp:hyperlink>
                   </span>
                   <asp:hyperlink ID="imgFile" runat="server" resourcekey="EditImage" class="Gallery_DropShadow"></asp:hyperlink>
                   <asp:hyperlink id="lnkEditImage" cssclass="CommandButton" runat="server" resourcekey="EditImage">Edit Image</asp:hyperlink>
  			</td>
            <td class="Gallery_RowCapRight"></td>
		</tr>
        <tr>
            <td class="Gallery_RowCapLeft"></td>
			<td class="Gallery_Row Centered" colspan="2">
				<span class="Gallery_Commands">
                   <asp:LinkButton id="cmdSave" runat="server" CssClass="CommandButton" text="Update" resourcekey="cmdUpdate"></asp:LinkButton>&nbsp;
				   <asp:LinkButton id="cmdReturn" runat="server" CssClass="CommandButton" resourcekey="cmdCancel" text="Cancel" CausesValidation="false"></asp:LinkButton>
                </span>
		    </td>
            <td class="Gallery_RowCapRight"></td>
		</tr>
        <tr>
			<td class="Gallery_FooterCapLeft"></td>
			<td class="Gallery_Footer" colspan="2"></td>
			<td class="Gallery_FooterCapRight"></td>
		</tr>
		<tr>
			<td class="Gallery_BottomCapLeft"></td>
			<td class="Gallery_Bottom" colspan="2"></td>
			<td class="Gallery_BottomCapRight"></td>
		</tr>
	</table>
