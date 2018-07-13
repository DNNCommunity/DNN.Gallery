<%@ Control language="vb" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Gallery.WebControls.MediaPlayer" Codebehind="ControlMediaPlayer.ascx.vb" %>
<table class="Gallery_MediaPlayer">
  <tr>
       <td>
           <asp:Literal ID="ctrlMediaPlayer" runat="server"></asp:Literal>
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