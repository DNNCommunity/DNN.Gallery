<%@ Control language="vb" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Gallery.WebControls.Viewer" Codebehind="ControlViewer.ascx.vb" %>
<table class="Gallery_Viewer">
	<tr>
		<td class="Gallery_Toolbar">
          <span>
			<asp:hyperlink id="MovePrevious" runat="server"></asp:hyperlink>
   			<asp:hyperlink id="MoveNext" runat="server"></asp:hyperlink>
			<asp:hyperlink id="ZoomOut" runat="server"></asp:hyperlink>
			<asp:hyperlink id="ZoomIn" runat="server"></asp:hyperlink>
			<asp:hyperlink id="RotateLeft" runat="server"></asp:hyperlink>
			<asp:hyperlink id="RotateRight" runat="server"></asp:hyperlink>
			<asp:hyperlink id="FlipX" runat="server"></asp:hyperlink>
			<asp:hyperlink id="FlipY" runat="server"></asp:hyperlink>
			<asp:hyperlink id="BWMinus" runat="server"></asp:hyperlink>
			<asp:hyperlink id="Color" runat="server"></asp:hyperlink>
			<asp:hyperlink id="BWPlus" runat="server"></asp:hyperlink>
			<asp:hyperlink id="UpdateButton" runat="server" visible="False"></asp:hyperlink>
          </span>
		</td>
	</tr>
	<tr>
		<td>
			<table class="Gallery_Picture">
				<tr>
					<td class="Gallery_PictureTL"></td>
					<td class="Gallery_PictureTC"></td>
					<td class="Gallery_PictureTR"></td>
				</tr>
				<tr>
					<td class="Gallery_PictureML"></td>
                    <td class="Gallery_PictureMC">
                          <div><img src="<%=ImageUrl() %>" alt="<%=Name() %>" /></div></td>
                    <td class="Gallery_PictureMR"></td>
                </tr>
				<tr>
					<td class="Gallery_PictureBL"></td>
					<td class="Gallery_PictureBC"></td>
					<td class="Gallery_PictureBR"></td>
				</tr>
            </table>
		</td>
	</tr>
	<tr>
	   <td class="Gallery_Info">
			<asp:Literal ID="litInfo" runat="server"></asp:Literal>
       </td>
   </tr>
   <tr>
       <td>
            <asp:Literal ID="litDescription" runat="server"></asp:Literal>
	   </td>
	</tr>
</table>
