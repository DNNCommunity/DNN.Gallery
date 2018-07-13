<%@ Control Language="VB" Inherits="DotNetNuke.Modules.Gallery.Container" AutoEventWireup="false" Codebehind="Container.ascx.vb" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%@ Register TagPrefix="dnn" TagName="ControlPager" Src="./Controls/ControlPager.ascx" %>
<%@ Reference Control="~/DesktopModules/Gallery/Controls/ControlGalleryMenu.ascx" %>
<%@ Reference Control="~/DesktopModules/Gallery/Controls/ControlBreadCrumbs.ascx" %>
<%@ Register TagPrefix="gal" Namespace="DotNetNuke.Modules.Gallery.Views" Assembly="Dotnetnuke.Modules.Gallery" %>
<!-- Following meta added to remove xhtml validation errors - needs to be removed in DDN 5.0 GAL8522 - HWZassenhaus -->
<!-- <meta http-equiv="Content-Style-Type" content="text/css"/> -->

<script type="text/javascript" language="javascript" src='<%=Page.ResolveUrl("DesktopModules/Gallery/Popup/gallerypopup.js")%>'></script>

<table id="tbContent" class="Gallery_Container" runat="server">
	<!-- Main header row -->
	<tr id="rowFolders1" runat="server">
		<td class="Gallery_HeaderCapLeft" id="celHeaderLeft" runat="server">
			<img src='<%=Page.ResolveUrl(GalleryConfig.GetImageURL("spacer_left.gif"))%>' alt="" />
        </td>
		<td class="Gallery_HeaderImage" id="celGalleryMenu" runat="server">
		    <!-- Menu gets put here -->
		</td>
		<td class="Gallery_Header" id="celBreadcrumbs" runat="server" colspan="2">
            <!-- BreadCrumbs gets put here -->
		</td>
		<td class="Gallery_HeaderCapRight" id="celHeaderRight">
            <img src='<%=Page.ResolveUrl(GalleryConfig.GetImageURL("spacer_right.gif"))%>' alt="" /></td>
	</tr>
	<!-- Top row above gallery -->
	<tr id="rowPager1" runat="server">
		<td class="Gallery_AltHeaderCapLeft" id="celAltHeaderLeft"></td>
		<td class="Gallery_AltHeaderImage" colspan="2">
			 <dnn:ControlPager ID="ctlControlPager1" runat="server"></dnn:ControlPager>
		</td>
		<td class="Gallery_AltHeader">
          <span class="Gallery_ViewSortOptions">
            <asp:Label ID="lblView" Text="View" resourcekey="View.Text" runat="server"></asp:Label>&nbsp;
			<asp:DropDownList ID="ddlGalleryView" runat="server" AutoPostBack="True" CssClass="NormalTextBox"></asp:DropDownList>&nbsp;&nbsp;
			<asp:Label ID="lblSortBy" Text="Sort By" resourcekey="SortBy.Text" runat="server"></asp:Label>&nbsp;
			<asp:DropDownList ID="ddlGallerySort" runat="server" AutoPostBack="True" CssClass="NormalTextBox"></asp:DropDownList>&nbsp;
			<asp:CheckBox ID="chkDESC" runat="server" AutoPostBack="True" Text="Desc" resourcekey="chkDESC" />
          </span>
		</td>
		<td class="Gallery_AltHeaderCapRight" id="celAltHeaderRight"></td>
	</tr>
	<!-- Middle row with main gallery -->
	<tr id="rowContent" runat="server">
		<td class="Gallery_RowCapLeft" id="celBodyLeft"></td>
		<td class="Gallery_Row" id="celContent" colspan="3">
		    <!-- Main control that displays the album's and images -->
			<gal:GalleryControl ID="ctlGallery" runat="server" />
		</td>
		<td class="Gallery_RowCapRight" id="celBodyRight">
		</td>
	</tr>
	<!-- Bottom Pager -->
    <tr id="rowPager2" runat="server">
		<td class="Gallery_FooterCapLeft" id="celFooterLeft"></td>
		<td class="Gallery_FooterImage" colspan="2">
			<dnn:ControlPager ID="ctlControlPager2" runat="server"></dnn:ControlPager>
		</td>
		<td class="Gallery_Footer">
            <span class="Gallery_Stats">
                <asp:ImageButton ID="ClearCache" runat="server" ImageUrl="~/DesktopModules/Gallery/Images/s_Refresh.gif"
			            resourcekey="ClearCache.Text" AlternateText=""></asp:ImageButton>&nbsp;
                <asp:Label ID="lblStats" runat="server"></asp:Label>
            </span>
		</td>
		<td class="Gallery_FooterCapRight" id="celFooterRight"></td>
	</tr>
	<!-- Bottom Row -->
	<tr>
		<td class="Gallery_BottomCapLeft" id="celleftBottomLeft"></td>
		<td class="Gallery_Bottom" id="celBottum" colspan="3"></td>
		<td class="Gallery_BottomCapRight" id="celBottomRight"></td>
	</tr>
</table>
