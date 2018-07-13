<%@ Control language="vb" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Gallery.MediaPlayer" Codebehind="MediaPlayer.ascx.vb" %>
<%@ Register TagPrefix="dnn" TagName="ControlMediaPlayer" Src="./Controls/ControlMediaPlayer.ascx" %>
	<table id="tblViewControl" runat="server" class="Gallery_ViewControl">
        <tr>
            <td class="Gallery_HeaderCapLeft"></td>
            <td class="Gallery_Header Centered">
                <asp:label id="lblTitle" runat="server" cssclass="Gallery_HeaderText"></asp:label>
            </td>
		    <td class="Gallery_HeaderCapRight"></td>
        </tr>
        <tr>
		    <td class="Gallery_RowCapLeft"></td>
		    <td class="Gallery_Row Centered">
				<dnn:ControlMediaPlayer id="ctlMediaPlayer" runat="server"></dnn:ControlMediaPlayer>
            </td>
            <td class="Gallery_RowCapRight"></td>
	    </tr>
        <tr id="rowControls" runat="server">
		    <td class="Gallery_FooterCapLeft"></td>
		    <td class="Gallery_Footer Centered">
                <asp:button id="btnBack" runat="server" Text="Back" ResourceKey="btnBack" CssClass="Gallery_BackButton" ></asp:button>
            </td>
			<td class="Gallery_FooterCapRight"></td>
	    </tr>
	    <tr>
		    <td class="Gallery_BottomCapLeft"></td>
		    <td class="Gallery_Bottom"></td>
		    <td class="Gallery_BottomCapRight"></td>
	    </tr>
	</table>	<asp:Label id="ErrorMessage" cssclass="NormalRed" runat="server" visible="False"></asp:Label>
