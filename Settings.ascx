<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="SectionHead" Src="~/controls/SectionHeadControl.ascx" %>
<%@ Register TagPrefix="TTP" TagName="LookupControl" Src="./Controls/ControlLookup.ascx" %>
<%@ Register TagPrefix="dnn" NameSpace="DotNetNuke.Modules.Gallery.Views" Assembly="DotNetNuke.Modules.Gallery" %>
<%@ Control language="vb" CodeBehind="Settings.ascx.vb" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Gallery.Settings" Explicit="true" %>

<script type="text/javascript" language="javascript" src='<%= Page.ResolveUrl("DesktopModules/Gallery/Popup/gallerypopup.js") %>'>
</script>

	<table class="Gallery_AdminContainer" id="tblMain">
		<tr>
			<td class="Gallery_HeaderCapLeft" id="celHeaderLeft" runat="server"><img src='<%=Page.ResolveUrl(GalleryConfig.GetImageURL("spacer_left.gif"))%>' alt="" /></td>
			<td class="Gallery_Header Centered" id="celHeader" ><asp:label id="lblAdminTitle" CssClass="Gallery_HeaderText" text="Gallery Configuration" resourcekey="GalleryConfiguration" runat="server">Gallery Configuration</asp:label></td>
			<td class="Gallery_HeaderCapRight" id="celHeaderRight"><img src='<%=Page.ResolveUrl(GalleryConfig.GetImageURL("spacer_left.gif"))%>' alt="" /></td>
		</tr>
		<tr id="rowAdmin" runat="server">
			<td class="Gallery_AltHeaderCapLeft" id="celBodyLeft0">&nbsp;</td>
			<td class="Gallery_AltHeader"><dnn:sectionhead id="dshAdminSettings" runat="server" text="Admin Settings" resourcekey="AdminSettings"
					includerule="True" section="tblAdminSettings" cssclass="Gallery_AltHeaderText"></dnn:sectionhead>
				<table class="Gallery_AdminSection" id="tblAdminSettings" runat="server">
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plRootURL" runat="server" text="Root URL:" controlname="txtRootURL"></dnn:label></td>
						<td class="Gallery_AdminRow">
						   <asp:Label ID="lblHomeDirectory" runat="server" CssClass="NormalTextBox" /><asp:textbox id="RootURL" runat="server" cssclass="NormalTextBox Width50PC"></asp:textbox>
						   <asp:RegularExpressionValidator ID="valRootURL" runat="server" ValidationExpression='^(([a-zA-Z0-9][^\000-\037\\/:*?"><|&]*)[\\/]?)+$' ControlToValidate="RootURL" ErrorMessage="Invalid RootURL"
						   CssClass="NormalRed" Display="Dynamic" ResourceKey= "RootURL.ErrorMessage"></asp:RegularExpressionValidator>
						</td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plCreatedDate" runat="server" text="Created On:" controlname="txtCreatedDate"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:textbox id="txtCreatedDate" runat="server" cssclass="NormalTextBox" style="width: 150px" Enabled="false"></asp:textbox>
						</td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plQuota" runat="server" text="Quota:" controlname="txtQuota"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:textbox id="txtQuota" runat="server" cssclass="NormalTextBox" width="75px"></asp:textbox>&nbsp;<asp:regularexpressionvalidator id="Regularexpressionvalidator9" runat="server" CssClass="NormalRed" resourcekey="NumericValue.ErrorMessage"
								ValidationExpression="[0-9]{1,}" ControlToValidate="txtQuota" ErrorMessage="Needs to be integer!"></asp:regularexpressionvalidator></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plMaxFileSize" runat="server" text="Max File Size:" controlname="txtMaxFileSize"></dnn:label></td>
						<td class="Gallery_AdminRow" align="left"><asp:textbox id="txtMaxFileSize" runat="server" cssclass="NormalTextBox" width="75px"></asp:textbox>&nbsp;<asp:regularexpressionvalidator id="Regularexpressionvalidator8" runat="server" CssClass="NormalRed" resourcekey="NumericValue.ErrorMessage"
								ValidationExpression="[0-9]{1,}" ControlToValidate="txtMaxFileSize" ErrorMessage="Needs to be integer!"></asp:regularexpressionvalidator>&nbsp;
								<asp:Label ID="lblMaxFileSizeWarning" runat= "server" CssClass="NormalRed" ResourceKey="lblMaxFileSizeWarning"></asp:Label></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plMaxPendingUploadsSize" runat="server" text="Max Pending Uploads Size:" controlname="txtMaxPendingUploadsSize"></dnn:label></td>
						<td class="Gallery_AdminRow">
						      <asp:textbox id="txtMaxPendingUploadsSize" runat="server" cssclass="NormalTextBox" width="75px"></asp:textbox>&nbsp;
						      <asp:regularexpressionvalidator id="Regularexpressionvalidator1" runat="server" CssClass="NormalRed" resourcekey="NumericValue.ErrorMessage"
								ValidationExpression="[0-9]{1,}" ControlToValidate="txtMaxPendingUploadsSize" ErrorMessage="Needs to be integer!"></asp:regularexpressionvalidator>
						      <asp:RangeValidator ID="RangeValidator1" runat="server" cssclass="NormalRed" ControlToValidate="txtMaxPendingUploadsSize" Display="Dynamic"
								ErrorMessage="Must be integer 0 to 20000" Type="Integer" MinimumValue="0" MaximumValue="20000" resourcekey="MaxPendingUploadsSizeRange.ErrorMessage"></asp:RangeValidator></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plAutoApproval" runat="server" text="Auto Approval:" controlname="chkApproval"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:checkbox id="chkAutoApproval" runat="server" cssclass="NormalTextBox"></asp:checkbox></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plBuildCacheOnStart" runat="server" text="Build Cache On Start:" controlname="chkBuildCacheOnStart"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:checkbox id="chkBuildCacheOnStart" runat="server" cssclass="NormalTextBox" checked="True"></asp:checkbox></td>
					</tr>
				</table>
			</td>
			<td class="Gallery_AltHeaderCapRight" id="celBodyRight0">&nbsp;</td>
		</tr>
		<tr id="rowDisplaySettings" runat="server">
			<td class="Gallery_AltHeaderCapLeft" id="celBodyLeft2"></td>
			<td class="Gallery_AltHeader"><dnn:sectionhead id="dshDisplaySettings" runat="server" text="Display Settings" resourcekey="DisplaySettings"
					includerule="True" section="tblDisplaySettings" cssclass="Gallery_AltHeaderText" isExpanded="False"></dnn:sectionhead>
				<table id="tblDisplaySettings" class="Gallery_AdminSection" runat="server">
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plSkin" runat="server" text="Themes:" controlname="ddlSkins"></dnn:label></td>
						<td class="Gallery_AdminRow">
						<dnn:templatelist id="ddlSkins" runat="server" cssclass="NormalTextBox" targetname="Themes" targettype="Folder"
								autopostback="False"></dnn:templatelist>
								</td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plGalleryTitle" runat="server" text="Gallery Title:" controlname="txtGalleryTitle"></dnn:label></td>
						<td class="Gallery_AdminRow" align="left"><asp:textbox id="txtGalleryTitle" runat="server" cssclass="NormalTextBox"></asp:textbox></td>
					</tr>
                    <tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plDefaultAlbum" runat="server" text="Default Album:" controlname="ddlDefaultAlbum"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:dropdownlist id="ddlDefaultAlbum" runat="server" cssclass="NormalTextBox"></asp:dropdownlist></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plImageExtensions" runat="server" text="Image Extensions:" controlname="txtImageExtensions"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:label id="lblImageExtensions" runat="server" cssclass="NormalTextBox"></asp:label></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plMediaExtensions" runat="server" text="Media Extensions:" controlname="txtMediaExtensions"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:label id="lblMediaExtensions" runat="server" cssclass="NormalTextBox"></asp:label></td>
					</tr>
					<tr id="trDescription" runat="server" visible="false">
						<td class="Gallery_AdminRowPanel"><dnn:label id="plDescription" runat="server" text="Description:" controlname="txtDescription"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:textbox id="txtDescription" runat="server" cssclass="NormalTextBox" Rows="3" TextMode="MultiLine"></asp:textbox></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plFixedWidth" runat="server" text="Max Fixed Width:" controlname="txtFixedWidth"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:textbox id="txtFixedWidth" runat="server" cssclass="NormalTextBox" width="75px"></asp:textbox>&nbsp;<asp:RequiredFieldValidator id="valFixedWidth1" runat="server" cssclass="NormalRed" resourcekey="RequiredField.ErrorMessage"
								controltovalidate="txtFixedWidth" Display="Dynamic" errormessage="Value cannot be blank"></asp:RequiredFieldValidator><asp:RangeValidator ID="valFixedWidth2" runat="server" cssclass="NormalRed" ControlToValidate="txtFixedWidth" Display="Dynamic"
								ErrorMessage="Must be integer > 0 and <= 1950" Type="Integer" MinimumValue="1" MaximumValue="1950" resourcekey="NumericValueGT0LT1950.ErrorMessage"></asp:RangeValidator></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plFixedHeight" runat="server" text="Max Fixed Height:" controlname="txtFixedHeight"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:textbox id="txtFixedHeight" runat="server" cssclass="NormalTextBox" width="75px"></asp:textbox>&nbsp;<asp:RequiredFieldValidator id="valFixedHeight1" runat="server" cssclass="NormalRed" resourcekey="RequiredField.ErrorMessage"
								controltovalidate="txtFixedHeight" Display="Dynamic" errormessage="Value cannot be blank"></asp:RequiredFieldValidator><asp:RangeValidator ID="valFixedHeight2" runat="server" cssclass="NormalRed" ControlToValidate="txtFixedHeight" Display="Dynamic"
								ErrorMessage="Must be integer > 0 and <= 1950" Type="Integer" MinimumValue="1" MaximumValue="1950" resourcekey="NumericValueGT0LT1950.ErrorMessage"></asp:RangeValidator></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plStripWidth" runat="server" text="Strip Width:" controlname="txtStripWidth"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:textbox id="txtStripWidth" runat="server" cssclass="NormalTextBox" width="75px" columns="26"></asp:textbox>&nbsp;<asp:RequiredFieldValidator id="valStripWidth1" runat="server" cssclass="NormalRed" resourcekey="RequiredField.ErrorMessage"
								controltovalidate="txtStripWidth" Display="Dynamic" errormessage="Value cannot be blank"></asp:RequiredFieldValidator><asp:RangeValidator ID="valStripWidth2" runat="server" cssclass="NormalRed" ControlToValidate="txtStripWidth" Display="Dynamic"
								ErrorMessage="Must be integer > 0 and <= 20" Type="Integer" MinimumValue="1" MaximumValue="20" resourcekey="NumericValueGT0LE20.ErrorMessage"></asp:RangeValidator></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plStripHeight" runat="server" text="Strip Height:" controlname="txtStripHeight"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:textbox id="txtStripHeight" runat="server" cssclass="NormalTextBox" width="75px" columns="26"></asp:textbox>&nbsp;<asp:RequiredFieldValidator id="valStripHeight1" runat="server" cssclass="NormalRed" resourcekey="RequiredField.ErrorMessage"
								controltovalidate="txtStripHeight" Display="Dynamic" errormessage="Value cannot be blank"></asp:RequiredFieldValidator><asp:RangeValidator ID="valStripHeight2" runat="server" cssclass="NormalRed" ControlToValidate="txtStripHeight" Display="Dynamic"
								ErrorMessage="Must be integer > 0 and <= 99" Type="Integer" MinimumValue="1" MaximumValue="99" resourcekey="NumericValueGT0LE99.ErrorMessage"></asp:RangeValidator></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plMaxThumbWidth" runat="server" text="Max Thumb Width:" controlname="txtMaxThumbWidth"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:textbox id="txtMaxThumbWidth" runat="server" cssclass="NormalTextBox" width="75px" columns="26"></asp:textbox>&nbsp;<asp:RequiredFieldValidator id="valMaxThumbWidth1" runat="server" cssclass="NormalRed" resourcekey="RequiredField.ErrorMessage"
								controltovalidate="txtMaxThumbWidth" Display="Dynamic" errormessage="Value cannot be blank"></asp:RequiredFieldValidator><asp:RangeValidator ID="valMaxThumbWidth2" runat="server" cssclass="NormalRed" ControlToValidate="txtMaxThumbWidth" Display="Dynamic"
								ErrorMessage="Must be integer >= 25 and <= 800" Type="Integer" MinimumValue="25" MaximumValue="800" resourcekey="NumericValueGE25LE800.ErrorMessage"></asp:RangeValidator></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plMaxThumbHeight" runat="server" text="Max Thumb Height:" controlname="txtMaxThumbHeight"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:textbox id="txtMaxThumbHeight" runat="server" cssclass="NormalTextBox" width="75px" columns="26"></asp:textbox>&nbsp;<asp:RequiredFieldValidator id="valMaxThumbHeight1" runat="server" cssclass="NormalRed" resourcekey="RequiredField.ErrorMessage"
								controltovalidate="txtMaxThumbHeight" Display="Dynamic" errormessage="Value cannot be blank"></asp:RequiredFieldValidator><asp:RangeValidator ID="valMaxThumbHeight2" runat="server" cssclass="NormalRed" ControlToValidate="txtMaxThumbHeight" Display="Dynamic"
								ErrorMessage="Must be integer >= 25 and <= 800" Type="Integer" MinimumValue="25" MaximumValue="800" resourcekey="NumericValueGE25LE800.ErrorMessage"></asp:RangeValidator></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plEncoderQuality" runat="server" text="JPEG Encoder Quality:" controlname="txtEncoderQuality"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:textbox id="txtEncoderQuality" runat="server" cssclass="NormalTextBox" width="75px" columns="26"></asp:textbox>&nbsp;<asp:RequiredFieldValidator id="valEncoderQuality1" runat="server" cssclass="NormalRed" resourcekey="RequiredField.ErrorMessage"
								controltovalidate="txtEncoderQuality" Display="Dynamic" errormessage="Value cannot be blank"></asp:RequiredFieldValidator><asp:RangeValidator ID="valEncoderQuality2" runat="server" cssclass="NormalRed" ControlToValidate="txtEncoderQuality" Display="Dynamic"
								ErrorMessage="Must be integer >= 10 and <= 100" Type="Integer" MinimumValue="10" MaximumValue="100" resourcekey="NumericValueGE10LE100.ErrorMessage"></asp:RangeValidator></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel TopAlign"><dnn:label id="plDisplay" runat="server" text="Display Info:" controlname="lstDisplay"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:checkboxlist id="lstDisplay" runat="server" CssClass="NormalTextBox" RepeatDirection="Vertical" RepeatColumns="2"></asp:checkboxlist></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel TopAlign"><dnn:label id="plCategoryValues" runat="server" text="Category Values:" controlname="txtCategoryValues"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:textbox id="txtCategoryValues" runat="server" cssclass="NormalTextBox"></asp:textbox></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel TopAlign"><dnn:label id="plSortProperties" runat="server" text="Sort Properties:" controlname="lstSortProperties"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:checkboxlist id="lstSortProperties" runat="server" cssclass="NormalTextBox" RepeatDirection="Vertical" RepeatColumns="2"></asp:checkboxlist></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plGallerySort" runat="server" text="Default Sort:" controlname="lstSortProperties"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:dropdownlist id="ddlGallerySort" runat="server" cssclass="NormalTextBox" ></asp:dropdownlist>&nbsp;<asp:checkbox id="chkDESC" runat="server" cssclass="Normal" resourcekey="Descending" text="Descending"></asp:checkbox></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plGalleryView" runat="server" text="Default View:" controlname="ddlGalleryView"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:dropdownlist id="ddlGalleryView" runat="server" CssClass="NormalTextBox" ></asp:dropdownlist>&nbsp;<asp:checkbox id="chkChangeView" runat="server" cssclass="Normal" resourcekey="VisitorChangeView"
								text="Visitors can change view."></asp:checkbox></td>
					</tr>
				</table>
			</td>
			<td class="Gallery_AltHeaderCapRight" id="celBodyRight2" valign="top"></td>
		</tr>
		<tr>
			<td class="Gallery_AltHeaderCapLeft" id="celBodyLeft3"></td>
			<td class="Gallery_AltHeader"><dnn:sectionhead id="dshFeatureSettings" runat="server" text="Feature Settings" resourcekey="FeatureSettings"
					includerule="True" section="tblFeatureSettings" cssclass="Gallery_AltHeaderText" isExpanded="False"></dnn:sectionhead>
				<table id="tblFeatureSettings" class="Gallery_AdminSection" runat="server">
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plSlideshowSpeed" runat="server" text="Slideshow Speed:" controlname="txtSlideshowSpeed"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:textbox id="txtSlideshowSpeed" runat="server" cssclass="NormalTextBox" width="75px" columns="26"></asp:textbox>&nbsp;<asp:RequiredFieldValidator id="valSlideShowSpeed1" runat="server" cssclass="NormalRed" resourcekey="RequiredField.ErrorMessage"
								controltovalidate="txtSlideshowSpeed" Display="Dynamic" errormessage="Value cannot be blank"></asp:RequiredFieldValidator><asp:RangeValidator ID="valSlideShowSpeed2" runat="server" cssclass="NormalRed" ControlToValidate="txtSlideShowSpeed" Display="Dynamic"
								ErrorMessage="Must be integer >= 25 and <= 300000" Type="Integer" MinimumValue="25" MaximumValue="300000" resourcekey="NumericValueGE25LE300000.ErrorMessage"></asp:RangeValidator></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plSlideshow" runat="server" text="Enable Slideshow?" controlname="chkSlideshow"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:checkbox id="chkSlideshow" runat="server" cssclass="Normal"></asp:checkbox></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plPopup" runat="server" text="Enable Popup?" controlname="txtPopup"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:checkbox id="chkPopup" runat="server" CssClass="Normal"></asp:checkbox></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plMultiLevelMenu" runat="server" text="Multi Level Menu?" controlname="chkMultiLevelMenu"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:checkbox id="chkMultiLevelMenu" runat="server" cssclass="Normal"></asp:checkbox></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plWatermark" runat="server" text="Enable Watermark?" controlname="chkWatermark"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:checkbox id="chkWatermark" runat="server" cssclass="Normal"></asp:checkbox></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plExif" runat="server" text="Enable Exif?" controlname="chkExif"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:checkbox id="chkExif" runat="server" cssclass="Normal"></asp:checkbox></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plVoting" runat="server" text="Enable Voting?" controlname="chkVoting"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:checkbox id="chkVoting" runat="server" cssclass="Normal"></asp:checkbox></td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel">&nbsp;
							<dnn:label id="plDownload" runat="server" text="Enable Download?" controlname="chkDownload"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:checkbox id="chkDownload" runat="server" cssclass="Normal"></asp:checkbox></td>
					</tr>
					<tr id="rowDownloadRoles" runat="server">
						<td class="Gallery_AdminRowPanel"><dnn:label id="plDownloadRoles" runat="server" text="Download Roles:" controlname="ctlDownloadRoles"></dnn:label></td>
						<td class="Gallery_AdminRow"><TTP:LOOKUPCONTROL id="ctlDownloadRoles" runat="server"></TTP:LOOKUPCONTROL></td>
					</tr>
				</table>
			</td>
			<td class="Gallery_AltHeaderCapRight" id="celBodyRight3"></td>
		</tr>
		<tr>
			<td class="Gallery_AltHeaderCapLeft" id="celBodyLeft4"></td>
			<td class="Gallery_AltHeader"><dnn:sectionhead id="dshPrivateGallery" runat="server" text="Private Gallery" resourcekey="PrivateGallery"
					includerule="True" section="tblPrivateGallery" cssclass="Gallery_AltHeaderText" isExpanded="False"></dnn:sectionhead>
				<table id="tblPrivateGallery" runat="server" class="Gallery_AdminSection">
					<tr>
						<td class="Gallery_AdminRowPanel"><dnn:label id="plPrivate" runat="server" text="Is Private" controlname="chkPrivate"></dnn:label></td>
						<td class="Gallery_AdminRow"><asp:checkbox id="chkPrivate" runat="server" cssclass="NormalTextBox"></asp:checkbox></td>
					</tr>
					<tr id="rowOwner" runat="server" style="display:block">
						<td class="Gallery_AdminRowPanel"><dnn:label id="plOwnerLookup" runat="server" text="Owner:" controlname="ctlOwnerLookup"></dnn:label></td>
						<td class="Gallery_AdminRow"><TTP:LOOKUPCONTROL id="ctlOwnerLookup" runat="server"></TTP:LOOKUPCONTROL></td>
					</tr>
				</table>
			</td>
			<td class="Gallery_AltHeaderCapRight" id="celBodyRight4"></td>
		</tr>
		<tr>
			<td class="Gallery_FooterCapLeft" id="celFooterLeft"></td>
			<td class="Gallery_Footer Centered">
                <span class="Gallery_Commands">
                    <asp:linkbutton id="cmdSave" runat="server" text="Update" resourcekey="cmdUpdate" cssclass="CommandButton"></asp:linkbutton>&nbsp;
				    <asp:linkbutton id="cmdReturn" runat="server" text="Cancel" resourcekey="cmdCancel" cssclass="CommandButton"></asp:linkbutton>
                </span>
            </td>
			<td class="Gallery_FooterCapRight" id="celFooterRight"></td>
		</tr>
		<tr>
			<td class="Gallery_BottomCapLeft" id="celleftBottomLeft"></td>
			<td class="Gallery_Bottom" id="celBottom"></td>
			<td class="Gallery_BottomCapRight" id="celBottomRight"></td>
		</tr>
	</table>
