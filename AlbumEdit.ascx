<%@ Control language="vb" Inherits="DotNetNuke.Modules.Gallery.AlbumEdit" AutoEventWireup="false" Codebehind="AlbumEdit.ascx.vb" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="GalleryControlUpload" Src="./Controls/ControlUpload.ascx" %>
<%@ Register TagPrefix="dnn" TagName="GalleryControlAlbum" Src="./Controls/ControlAlbum.ascx" %>
<%@ Register TagPrefix="TTP" TagName="LookupControl" Src="./Controls/ControlLookup.ascx" %>
<%@ Reference Control="~/DesktopModules/Gallery/Controls/ControlGalleryMenu.ascx" %>

<!-- Colling meta added to remove xhtml validation errors - needs to be removed in DDN 5.0 GAL8522 - HWZassenhaus -->
<meta http-equiv="Content-Style-Type" content="text/css"/>
<script type="text/javascript"  language="javascript" src='<%= Page.ResolveUrl("DesktopModules/Gallery/Popup/gallerypopup.js") %>'></script>
	<table class="Gallery_Container" id="Table1">
        <tr id="trNavigation" runat="server">
			<td class="Gallery_HeaderCapLeft" id="celHeaderLeft" runat="server"><img src='<%=Page.ResolveUrl(GalleryConfig.GetImageURL("spacer_left.gif"))%>' alt="" /></td>
			<td class="Gallery_HeaderImage" id="celGalleryMenu" runat="server" ></td>
            <td class="Gallery_Header" id="celBreadcrumbs" runat="server"></td>
			<td class="Gallery_HeaderCapRight" id="celHeaderRight"><img src='<%=Page.ResolveUrl(GalleryConfig.GetImageURL("spacer_left.gif"))%>' alt="" /></td>
		</tr>
		<tr>
            <td class="Gallery_AltHeaderCapLeft">&nbsp;</td>
			<td class="Gallery_AltHeader" colspan="2">
				<asp:label id="lblInfo" runat="server" cssClass="Gallery_AltHeaderText"></asp:label>
            </td>
            <td class="Gallery_AltHeaderCapRight">&nbsp;</td>
		</tr>
		<tr id="rowDetails" runat="server">
            <td class="Gallery_RowCapLeft"></td>
			<td class="Gallery_Row" colspan="2">
				<table id="tblDetails" class="Gallery_AdminSection">
					<tr>
						<td class="Gallery_AdminRowPanel" ><dnn:label id="plName" Text="Name:" Runat="server" resourcekey="plName" controlname="txtName"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:textbox id="txtName" runat="server" Enabled="False" CssClass="NormalTextBox"></asp:textbox>
                            <asp:Label ID="lblAlbumName" runat="server" CssClass="Normal" ForeColor="Red"></asp:Label>
                            <asp:RequiredFieldValidator ID="rqdFieldValidatorTxtName" runat="server" ControlToValidate="txtName" CssClass="NormalRed" ResourceKey="rqdFieldValidatorTxtName.ErrorMessage" Display="Dynamic"></asp:RequiredFieldValidator>
                            <asp:RegularExpressionValidator ID="validateCharacters4txtName" runat="server" ControlToValidate="txtName" CssClass="NormalRed" ResourceKey="validateCharacters4txtName.ErrorMessage" Display="Dynamic" ValidationExpression='[^\000-\037\\/:*?#!"><|&%_][^\000-\037\\/:*?#!"><|&%]*'></asp:RegularExpressionValidator>
                        </td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plTitle" text="Title:" Runat="server" resourcekey="plTitle" controlname="txtTitle"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:textbox id="txtTitle" runat="server" CssClass="NormalTextBox"></asp:textbox>
                            <asp:RequiredFieldValidator ID="rqdFieldValidatorTxtTitle" runat="server" ControlToValidate="txtTitle" CssClass="NormalRed" ResourceKey="rqdFieldValidatorTxtTitle.ErrorMessage" Display="Dynamic"></asp:RequiredFieldValidator>
                        </td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plAuthor" Text="Author:" Runat="server" resourcekey="plAuthor" controlname="txtAuthor"></dnn:label></td>
						<td class="Gallery_AdminRow">
                            <asp:textbox id="txtAuthor" runat="server" cssclass="NormalTextBox"></asp:textbox>
                        </td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plClient" Text="Client:" Runat="server" resourcekey="plClient" controlname="txtClient"></dnn:label></td>
						<td class="Gallery_AdminRow">
                            <asp:textbox id="txtClient" runat="server" cssclass="NormalTextBox"></asp:textbox>
                        </td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plLocation" Text="Location:" Runat="server" resourcekey="plLocation" controlname="txtLocation"></dnn:label></td>
						<td class="Gallery_AdminRow">
                            <asp:textbox id="txtLocation" runat="server" cssclass="NormalTextBox"></asp:textbox>
                        </td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plDescription" Text="Description:" Runat="server" resourcekey="plDescription" controlname="txtDescription"></dnn:label></td>
						<td class="Gallery_AdminRow">
                            <asp:textbox id="txtDescription" runat="server" CssClass="NormalTextBox" TextMode="MultiLine"></asp:textbox>
                        </td>
					</tr>
					<tr id="rowApprovedDate" runat="server">
						<td class="Gallery_AdminRowPanel"><dnn:label id="plApprovedDate" Text="ApprovedDate:" Runat="server" resourcekey="plApprovedDate" controlname="txtApprovedDate"></dnn:label></td>
						<td class="Gallery_AdminRow">
						     <asp:textbox id="txtApprovedDate" runat="server" Width="150px" cssclass="NormalTextBox"></asp:textbox><asp:hyperlink id="cmdApprovedDate" runat="server" imageurl="~/DesktopModules/Gallery/Images/m_calendar.gif"
								      resourcekey="cmdApprovedDate"></asp:hyperlink>
						      <asp:rangevalidator ID="valApprovedDate" runat="server" CssClass="NormalRed" ControlToValidate="txtApprovedDate" Type="Date"
							      ErrorMessage="Invalid Date" ResourceKey="valApprovedDate.ErrorMessage" Display="Dynamic"></asp:rangevalidator>							
						</td>
					</tr>
					<tr id="rowOwner" runat="server" visible="false">
						<td class="Gallery_AdminRowPanel"><dnn:label id="plOwner" Text="Owner:" Runat="server" resourcekey="plOwner" controlname="ctlOwnerLookup"></dnn:label></td>
						<td class="Gallery_AdminRow"><TTP:LOOKUPCONTROL id="ctlOwnerLookup" runat="server"></TTP:LOOKUPCONTROL></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel TopAlign"><dnn:label id="plCategories" Text="Categories:" Runat="server" resourcekey="plCategories" controlname="lstCategories"></dnn:label></td>
						<td class="Gallery_AdminRow">
                             <asp:checkboxlist id="lstCategories" runat="server" RepeatDirection="Vertical" RepeatColumns="1"></asp:checkboxlist>
                        </td>
					</tr>
				</table>
			</td>
            <td class="Gallery_RowCapRight"></td>
		</tr>
		<tr id="rowUpload" runat="server">
            <td class="Gallery_RowCapLeft"></td>
			<td class="Gallery_Row" colspan="2"><dnn:GALLERYCONTROLUPLOAD id="galControlUpload" runat="server"></dnn:GALLERYCONTROLUPLOAD></td>
            <td class="Gallery_RowCapRight"></td>
		</tr>
		<tr>
            <td class="Gallery_RowCapLeft"></td>
			<td class="Gallery_Row" colspan="2">
				<span class="Gallery_Commands Right">
                   <asp:LinkButton id="cmdUpdate" runat="server" CssClass="CommandButton" text="Update" resourcekey="cmdUpdate"></asp:LinkButton>&nbsp;
				   <asp:LinkButton id="cmdCancel" runat="server" CssClass="CommandButton" resourcekey="cmdCancel" text="Cancel" CausesValidation="false"></asp:LinkButton>
                </span>
		    </td>
            <td class="Gallery_RowCapRight"></td>
		</tr>
		<tr id="rowAlbumGrid" runat="server">
            <td class="Gallery_RowCapLeft"></td>
			<td class="Gallery_Row" colspan="2"><dnn:GALLERYCONTROLALBUM id="ControlAlbum1" runat="server"></dnn:GALLERYCONTROLALBUM></td>
            <td class="Gallery_RowCapRight"></td>
		</tr>
		<tr id="rowRefuse" runat="server" visible="False">
            <td class="Gallery_RowCapLeft"></td>
			<td class="Gallery_Row Centered" colspan="2">
				<DNN:LABEL id="plRefuseTitle" Text="Private Gallery" Runat="server" resourcekey="plRefuseTitle"></DNN:LABEL>&nbsp;
			    <asp:label id="lblRefuse" CssClass="Normal" Runat="server" resourcekey="lblRefuse"></asp:label>
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
