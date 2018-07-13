<%@ Import namespace="DotNetNuke.Modules.Gallery" %>
<%@ Control language="vb" Inherits="DotNetNuke.Modules.Gallery.WebControls.Pager" AutoEventWireup="false" Codebehind="ControlPager.ascx.vb" %>
<span id="spnPager" runat="server" class="Gallery_Pager">
			<asp:label id="lblPageInfo" runat="server"></asp:label>
			<asp:datalist id="dlPager" runat="server" repeatdirection="Horizontal" repeatlayout="Flow">
				<selecteditemtemplate>
					<asp:label id="lblText" runat="server">
						<%# Ctype(Container.DataItem, PagerDetail).Text %>
					</asp:label>
				</selecteditemtemplate>
				<itemtemplate>					
					<asp:hyperlink id="hlStrip" CssClass="CommandButton" runat="server" navigateurl='<%# Ctype(Container.DataItem, PagerDetail).URL %>'>
						<%# Ctype(Container.DataItem, PagerDetail).Text %>
					</asp:hyperlink>
				</itemtemplate>
			</asp:datalist>
</span>
