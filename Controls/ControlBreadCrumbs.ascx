<%@ Control language="vb" Inherits="DotNetNuke.Modules.Gallery.WebControls.BreadCrumbs" AutoEventWireup="false" Codebehind="ControlBreadCrumbs.ascx.vb" %>
<%@ Import namespace="DotNetNuke.Modules.Gallery" %>
<asp:Repeater ID="rptFolders" runat="server" >
    <HeaderTemplate>
        <span class="Gallery_Breadcrumbs">
    </HeaderTemplate>
    <itemtemplate>
		<asp:hyperlink id="hlFolder" CssClass="Gallery_HeaderText" runat="server" Enabled='<%# IsEnabled %>' navigateurl='<%# Ctype(Container.DataItem, FolderDetail).URL %>'>
			<%# Ctype(Container.DataItem, FolderDetail).Name %>
		</asp:hyperlink>
	</itemtemplate>
	<FooterTemplate>
	    </span>
	</FooterTemplate>
</asp:Repeater>
