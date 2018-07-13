<%@ Register TagPrefix="dnn" TagName="SectionHead" Src="~/controls/SectionHeadControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Control Language="vb" Inherits="DotNetNuke.Modules.Gallery.WebControls.Upload" AutoEventWireup="false" Codebehind="ControlUpload.ascx.vb" %>
<table id="tblUpload" class="Gallery_AdminSection">
	   <tr>
	        <td class="Gallery_SectionHeader" colspan="2">
	        <dnn:SectionHead id="scnInstructions" runat="server" text="Instructions:" section="trInstructions"
					includerule="False" isExpanded="false" resourcekey="scnInstructions" CssClass="Gallery_AltHeaderText">
				</dnn:SectionHead>
	        </td>
	    </tr>
	    <tr id="trInstructions" runat="server">
	        <td id="tdInstructions" runat="server" colspan="2" class="Gallery_Section">&nbsp;</td>
	    </tr>
		<tr>
			<td class="Gallery_SectionHeader" colspan="2">
				<dnn:SectionHead id="scnInfo" runat="server" text="File Upload Extensions and Available Space" section="trInfo"
					includerule="False" isexpanded="False" resourcekey="scnInfo" CssClass="Gallery_AltHeaderText">
				</dnn:SectionHead>
			</td>
		</tr>
		<tr id="trInfo" runat="server">
			<td id="tdInfo" runat="server" colspan="2" class="Gallery_Section">&nbsp;</td>
		</tr>
		<tr>
			<td class="Gallery_AdminRowPanel">
				<dnn:Label id="plAddFile" text="Add File:" runat="server" controlname="htmlUploadFile"></dnn:Label>
			</td>
			<td class="Gallery_AdminRow">
				<input class="NormalTextBox" id="htmlUploadFile" type="file" name="htmlUploadFile" runat="server" />
                <asp:LinkButton id="cmdAdd" runat="server" text="Add File" resourcekey="cmdAdd" cssclass="CommandButton"></asp:LinkButton>
				<div id="divFileError" runat="server" class="NormalRed"></div>
			</td>
		</tr>
		<tr>
			<td class="Gallery_AdminRowPanel">
				<dnn:Label id="plTitle" text="Title:" runat="server" controlname="txtTitle"></dnn:Label>
			</td>
			<td class="Gallery_AdminRow">
				<asp:TextBox id="txtTitle" runat="server" cssclass="NormalTextBox"></asp:TextBox></td>
		</tr>
		<tr>
			<td class="Gallery_AdminRowPanel">
				<dnn:Label id="plAuthor" text="Author:" runat="server" controlname="txtAuthor"></dnn:Label>
			</td>
			<td class="Gallery_AdminRow">
				<asp:TextBox id="txtAuthor" runat="server" cssclass="NormalTextBox"></asp:TextBox></td>
		</tr>
		<tr>
			<td class="Gallery_AdminRowPanel">
				<dnn:Label id="plClient" text="Notes:" runat="server" controlname="txtClient"></dnn:Label>
			</td>
			<td class="Gallery_AdminRow">
				<asp:TextBox id="txtClient" runat="server" cssclass="NormalTextBox"></asp:TextBox></td>
		</tr>
		<tr>
			<td class="Gallery_AdminRowPanel">
				<dnn:Label id="plLocation" text="Location:" runat="server" controlname="txtLocation"></dnn:Label>
			</td>
			<td class="Gallery_AdminRow">
				<asp:TextBox id="txtLocation" runat="server" cssclass="NormalTextBox"></asp:TextBox></td>
		</tr>
		<tr>
			<td class="Gallery_AdminRowPanel">
				<dnn:Label id="plDescription" text="Description:" runat="server" controlname="txtDescription"></dnn:Label>
			</td>
			<td class="Gallery_AdminRow">
				<asp:TextBox id="txtDescription" runat="server" cssclass="NormalTextBox" Rows="3" TextMode="MultiLine"></asp:TextBox>
            </td>
		</tr>
		<tr>
			<td class="Gallery_AdminRowPanel TopAlign">
				<dnn:Label id="plCategories" text="Categories:" runat="server" controlname="lstCategories"></dnn:Label>
			</td>
			<td class="Gallery_AdminRow">
				<asp:CheckBoxList id="lstCategories" runat="server" cssclass="Normal" RepeatDirection="Vertical" RepeatColumns="2"></asp:CheckBoxList>
            </td>
		</tr>
		<tr id="trPendingUploads" runat="server">
			<td class="Gallery_AdminRowPanel TopAlign">
				<dnn:Label id="plAddedFiles" text="Pending Uploads:" runat="server"></dnn:Label>
			</td>
			<td class="Gallery_AdminRow">
			    <asp:Label ID="lblUpload" runat="server" CssClass="Normal"></asp:Label>
				<asp:DataGrid id="grdUpload" resourcekey="grdUpload" runat="server" DataKeyField="FileName" AutoGenerateColumns="False"
					ShowFooter="true" CssClass="Gallery_Grid">
					<FooterStyle CssClass="Gallery_GridItem" />
					<HeaderStyle CssClass="Gallery_GridHeader" />
					<ItemStyle CssClass="Gallery_GridItem" />
					<Columns>
						<asp:TemplateColumn>
							<ItemTemplate>
								<asp:Image ImageUrl='<%# Ctype(Container.DataItem.Icon, String) %>' runat="server"
									id="Image1" AlternateText="Media Type Image"></asp:Image>
							</ItemTemplate>
						</asp:TemplateColumn>
						<asp:TemplateColumn Headertext="Name" >
							<ItemTemplate>
							    <asp:Image ID="btnExpandZipDetails" runat="server" />
							    <asp:Label id="lblName" runat="server" text="<%# Ctype(Container.DataItem.FileName, String) %>" />				
								<asp:DataGrid id="grdZipDetails" resourcekey="grdZipDetails" runat="server" AutoGenerateColumns="False"
					               CssClass="Gallery_DetailsGrid">
					                <HeaderStyle CssClass="Gallery_GridHeader" />
					                <ItemStyle CssClass="Gallery_GridItem" />
					                <Columns>
						               <asp:TemplateColumn>
							              <ItemTemplate>
								             <asp:Image ImageUrl='<%# Ctype(Container.DataItem.Icon(GalleryConfig), String) %>' runat="server"
									                id="Image2" AlternateText="File Type"></asp:Image>
							                 </ItemTemplate>
						               </asp:TemplateColumn>
						               <asp:BoundColumn HeaderText="File Name" DataField="FileName"></asp:BoundColumn>
						               <asp:TemplateColumn HeaderText="Uncompressed Size">
							               <ItemTemplate>
								               <asp:Label id="lblUncompressedSize" runat="server" text='<%# String.Format ("{0:F}", Container.DataItem.UncompressedSize/1024) %>' />
							               </ItemTemplate>
						               </asp:TemplateColumn>
								  </Columns>
				                </asp:DataGrid>
				             </ItemTemplate>		
						</asp:TemplateColumn>
						<asp:BoundColumn Headertext="Title" DataField="Title"></asp:BoundColumn>
						<asp:BoundColumn Headertext="Description" DataField="Description"></asp:BoundColumn>
						<asp:TemplateColumn Headertext="Size">
							<ItemTemplate>
								<asp:Label id="lblSize" runat="server" text='<%# String.Format ("{0:F}", Container.DataItem.ContentLength/1024) %>'/>
							</ItemTemplate>
						</asp:TemplateColumn>
						<asp:TemplateColumn>
							<ItemTemplate>
								<asp:ImageButton id="btnFileRemove" ImageUrl="~/images/Delete.gif" 
									runat="server" CommandName="delete" CommandArgument='<%# CType(DataBinder.Eval(Container.DataItem, "FileName"), String) %>'
									cssclass="CommandButton" resourcekey="cmdDelete" AlternateText="" />
							</ItemTemplate>
						</asp:TemplateColumn>
					</Columns>
				</asp:DataGrid>
			</td>
		</tr>
		<tr>
		    <td class="Gallery_Row" colspan="2">
				<span class="Gallery_Commands Right">
                    <asp:LinkButton id="btnFileUpload" runat="server" Text="Upload" resourcekey="cmdUpload"
					        cssclass="CommandButton" Visible="False"></asp:LinkButton>&nbsp;
				    <asp:LinkButton ID="cmdReturnCancel" runat="server" Text="Cancel"
					        cssclass="CommandButton"></asp:LinkButton>
                </span>
			</td>
		</tr>
</table>
