<%@ Control language="vb" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Gallery.WebControls.FlashPlayer" Codebehind="ControlFlashPlayer.ascx.vb" %>

<table class="Gallery_FlashPlayer">
  <tr>
    <td>
          <object id="objFlashPlayer" type="application/x-shockwave-flash"
               data="<%=FlashUrl %>" width="<%=FixedWidth %>px" height="<%=FixedHeight %>px">
              <param name="allowScriptAccess" value="sameDomain" />
              <param name="movie" value="<%=FlashUrl %>" />
              <param name="play" value="true" />
              <param name="loop" value="true" />
              <param name="quality" value="high" />
              <param name="bgcolor" value="#fffff" />
              <param name="scale" value="showall" />
              <param name="salign" value="lt" />
          <!--[if !IE]>-->
		    <object type="application/x-shockwave-flash" data="<%=FlashUrl %>" width="<%=FixedWidth %>" height="<%=FixedHeight %>px">
	      <!--<![endif]-->
	          <a href="http://www.adobe.com/go/getflashplayer">
		  	     <img src="http://www.adobe.com/images/shared/download_buttons/get_flash_player.gif" alt="Get Adobe Flash player" />
		      </a>
          <!--[if !IE]>-->
		    </object>
		  <!--<![endif]-->
          </object>
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