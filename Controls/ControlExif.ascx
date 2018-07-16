<%@ Control language="vb" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Gallery.WebControls.Exif" Codebehind="ControlExif.ascx.cs" %>
<table>
	<tr>
		<td class="Gallery_FileEditImage"><asp:image id="imgExif" Runat="server" AlternateText='""'></asp:image></td>
	</tr>
	<tr id="rowGrid" runat="server">
		<td>
			<table class="Gallery_Exif">
				<tr>
					<td>
                      <asp:datagrid id="grdExif" runat="server" AutoGenerateColumns="False" DataKeyField="ID" CssClass="Gallery_ExifGrid" OnItemDataBound="grdExif_ItemDataBound">
                      		<HeaderStyle CssClass="Gallery_GridHeader Left" />
                            <ItemStyle CssClass="Gallery_GridItem Left" />
							<Columns>
								<asp:TemplateColumn HeaderText="ID">
									<ItemTemplate>
										<asp:Image id="Image1" ImageUrl="~/images/help.gif" Runat="server" Title='<%# ExifHelp(Databinder.eval(Container,"DataItem.Name")) %>' />&nbsp;
                                        <asp:Label id="plID" runat="server" Text='<%# Databinder.Eval(Container,"DataItem.Id") %>'></asp:Label>
									</ItemTemplate>
								</asp:TemplateColumn>
								<asp:BoundColumn DataField="Category" HeaderText="Category"></asp:BoundColumn>
								<asp:TemplateColumn HeaderText="Name">
									<ItemTemplate>
										<asp:label id="plPropertyName" runat="server" text='<%# ExifText(Databinder.Eval(Container,"DataItem.Name")) %>'></asp:label>
									</ItemTemplate>
								</asp:TemplateColumn>
								<asp:BoundColumn DataField="Value" HeaderText="Value"></asp:BoundColumn>
							</Columns>
						</asp:datagrid>
                     </td>
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
