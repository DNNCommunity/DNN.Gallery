<%@ Control language="vb" Inherits="DotNetNuke.Modules.Gallery.Maintenance" AutoEventWireup="false" Codebehind="Maintenance.ascx.vb" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Reference Control="~/DesktopModules/Gallery/Controls/ControlGalleryMenu.ascx" %>
<!-- Following meta added to remove xhtml validation errors - needs to be removed in DDN 5.0 GAL8522 - HWZassenhaus -->
<meta http-equiv="Content-Style-Type" content="text/css"/>
	<script type="text/javascript" language="javascript" src='<%= Page.ResolveUrl("DesktopModules/Gallery/Popup/gallerypopup.js") %>'></script>
    <script type="text/javascript" language="javascript">
        function toggleChecks(b) {
            jQuery("input[id$=chkSelect]").each(function () {
                if (typeof this.checked != "undefined") {
                    if (b == null) this.checked = (!this.checked);
                    else this.checked = b;
                }
            });
        }
    </script>
	<table class="Gallery_Container">
        <tr id="trNavigation" runat="server">
			<td class="Gallery_HeaderCapLeft" id="celHeaderLeft" runat="server"><img src='<%=Page.ResolveUrl(GalleryConfig.GetImageURL("spacer_left.gif"))%>' alt="" /></td>
			<td class="Gallery_HeaderImage" id="celGalleryMenu" runat="server" ></td>
            <td class="Gallery_Header" id="celBreadcrumbs" runat="server"></td>
			<td class="Gallery_HeaderCapRight" id="celHeaderRight"><img src='<%=Page.ResolveUrl(GalleryConfig.GetImageURL("spacer_left.gif"))%>' alt="" /></td>
		</tr>
		<tr>
            <td class="Gallery_AltHeaderCapLeft">&nbsp;</td>
			<td class="Gallery_AltHeaderImage" colspan="2">
				<asp:imagebutton id="ClearCache1" resourcekey="ClearCache1.AlternateText" runat="server" imageurl="~/DesktopModules/Gallery/Images/s_refresh.gif"
						AlternateText='""'></asp:imagebutton>
            </td>
            <td class="Gallery_AltHeaderCapRight">&nbsp;</td>
		</tr>
		<tr id="rowDetails" runat="server">
            <td class="Gallery_RowCapLeft"></td>
			<td class="Gallery_Row" colspan="2">
				<table id="tblDetails" class="Gallery_AdminSection">
					<tr>
						<td class="Gallery_AdminRowPanel">
							<dnn:label id="plPath" text="Path:" runat="server" resourcekey="plPath" controlname="txtPath"></dnn:label>
						</td>
						<td class="Gallery_AdminRow">
							<asp:TextBox id="txtPath" runat="server" cssclass="NormalTextBox" enabled="False"></asp:TextBox>
                        </td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel">
                            <dnn:label id="plName" text="Name:" runat="server" resourcekey="plName" controlname="txtName"></dnn:label>
                        </td>
						<td class="Gallery_AdminRow">
                            <asp:textbox id="txtName" runat="server" enabled="False" cssclass="NormalTextBox"></asp:textbox>
                        </td>
					</tr>
					<tr>
						<td class="Gallery_AdminRowPanel">
							<dnn:label id="plAlbumInfo" text="Album Info:" runat="server" resourcekey="plAlbumInfo" controlname="lblAlbumInfo"></dnn:label>
						</td>
						<td class="Gallery_AdminRow">
							<asp:label id="lblAlbumInfo" runat="server" CssClass="Gallery_Info"></asp:label>
                        </td>
					</tr>
					<tr>
						<td class="Gallery_AdminRow" colspan="2">
							<span class="Gallery_Commands Left">
                                <asp:LinkButton id="btnSyncAll" runat="server" resourcekey="btnSyncAll" CssClass="CommandButton"></asp:LinkButton>&nbsp;&nbsp;
							    <asp:LinkButton id="btnDeleteAll" runat="server" resourcekey="btnDeleteAll" CssClass="CommandButton"></asp:LinkButton>
                            </span>
                            <span class="Gallery_Commands Right">
                                <asp:LinkButton id="btnSelectAll" runat="server" resourcekey="btnSelectAll" CssClass="CommandButton" 
								       OnClientClick="javascript:toggleChecks(true);return false;"></asp:LinkButton>
                                <asp:LinkButton id="btnDeselectAll" runat="server" resourcekey="btnDeselectAll" CssClass="CommandButton" 
                                       OnClientClick="javascript:toggleChecks(false);return false;"></asp:LinkButton>
                            </span>
						</td>
					</tr>
				</table>
			</td>
            <td class="Gallery_RowCapRight"></td>
		</tr>
		<tr>
            <td class="Gallery_RowCapLeft"></td>
			<td colspan="2">
                <asp:datagrid id="grdContent" resourcekey="grdContent" runat="server" DataKeyField="Name" AutoGenerateColumns="False" CssClass="Gallery_Grid">
                    <HeaderStyle CssClass="Gallery_GridHeader Centered" />
                    <ItemStyle CssClass="Gallery_GridItem Centered" />
					<Columns>
						<asp:TemplateColumn HeaderText="">
							<ItemTemplate>
								<asp:HyperLink runat="server" ImageUrl='<%# Eval("IconURL")%>' NavigateUrl='<%# BrowserURL(Container.DataItem)%>' ID="lnkView"  ToolTip ="<%# lnkViewText %>">
								</asp:HyperLink>
							</ItemTemplate>
						</asp:TemplateColumn>
						<asp:BoundColumn DataField="Name" HeaderText="Name" />
						<asp:TemplateColumn HeaderText="Info">
							<ItemTemplate>
								<asp:Label id="lblFileInfo" Text='<%# FileInfo(Container.DataItem)%>' Runat="server"></asp:Label>
							</ItemTemplate>
						</asp:TemplateColumn>
						<asp:TemplateColumn HeaderText="Thumb">
							<ItemTemplate>
								<asp:ImageButton id="chkThumb" CommandName="RebuildThumb"
                                   ImageUrl='<%# StatusImage(Container.DataItem, "Thumb")%>'
                                   Enabled='<%# Not CBool(Eval("ThumbExists"))%>'
                                   ToolTip='<%# Tooltip(Container.DataItem, "Thumb")%>' Runat="server" />
							</ItemTemplate>
						</asp:TemplateColumn>
						<asp:TemplateColumn HeaderText="Album">
							<ItemTemplate>
								<asp:ImageButton id="chkFile" CommandName="CopySourceToFile"
                                    ImageUrl='<%# StatusImage(Container.DataItem, "File")%>'
                                    Enabled='<%# Not CBool(Eval("FileExists"))%>'
                                    ToolTip='<%# Tooltip(Container.DataItem, "File")%>' Runat="server" />
							</ItemTemplate>
						</asp:TemplateColumn>
						<asp:TemplateColumn HeaderText="Source">
							<ItemTemplate>
								<asp:ImageButton id="chkSource" CommandName="CopyFileToSource" 
                                   ImageUrl='<%# StatusImage(Container.DataItem, "Source")%>' 
                                   Enabled='<%# Not CBool(Eval("SourceExists")) %>'
                                    ToolTip='<%# Tooltip(Container.DataItem, "Source") %>' Runat="server" />
							</ItemTemplate>
						</asp:TemplateColumn>
						<asp:TemplateColumn HeaderText="Selected">
							<ItemTemplate>
								<asp:CheckBox id="chkSelect" Runat="server"></asp:CheckBox>
							</ItemTemplate>
						</asp:TemplateColumn>
					</Columns>
				</asp:datagrid>
			</td>
            <td class="Gallery_RowCapRight"></td>
		</tr>
        <tr>
			<td class="Gallery_RowCapLeft"></td>
			<td class="Gallery_Row Centered" colspan="2">
                <asp:LinkButton id="cmdReturn" cssclass="CommandButton" resourcekey="cmdReturn" text="Cancel" runat="server" />
            </td>
			<td class="Gallery_RowCapRight"></td>
		</tr>
        <tr>
			<td class="Gallery_FooterCapLeft"></td>
			<td class="Gallery_Footer Centered" colspan="2"></td>
			<td class="Gallery_FooterCapRight"></td>
		</tr>
		<tr>
			<td class="Gallery_BottomCapLeft"></td>
			<td class="Gallery_Bottom" colspan="2"></td>
			<td class="Gallery_BottomCapRight"></td>
		</tr>
	</table>
