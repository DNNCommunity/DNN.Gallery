<%@ Control Inherits="DotNetNuke.Modules.Gallery.WebControls.LookupControl"
	Language="vb" AutoEventWireup="false" Explicit="True" Codebehind="ControlLookup.ascx.vb" %>
<%@ Import Namespace="DotNetNuke.Modules.Gallery" %>

<table id="tblLookup" cellspacing="0" cellpadding="1" width="100%" runat="server">
	<tr id="rowLookup" runat="server">
		<td id="celLookupItems" class="NormalTextBox" style="width: 316px" runat="server">
		</td>
		<td id="celLookupData" valign="top" runat="server">
		    <input type="hidden" value ="123" id="hiddenPP" name ="hiddenPP" />
			<input type="hidden" id="txtItemsID" runat="server" />
			<img id="imgLookup" onclick="<%=ItemLookup()%>" alt="<%= AddText %>" src='<%= Page.ResolveUrl("DesktopModules/Gallery/Images/sq_lookup.gif") %>'/>
			<img id="imgClear" onclick="javascript:CleartargetControl(this)" alt="<%= RemoveText %>"
				src='<%= Page.ResolveUrl("DesktopModules/Gallery/Images/sq_remove.gif") %>'/>
			<input type="hidden" id="txtInputItemsID" runat="server" />
		</td>
	</tr>
</table>
