<%@ Page Language="vb" Inherits="DotNetNuke.Modules.Gallery.PopupControls.PopupSelectMulti" AutoEventWireup="false" Codebehind="PopupSelectMulti.aspx.vb" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Modules.Gallery.PopupControls" Assembly="Dotnetnuke.Modules.Gallery" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title><%= PageTitle %></title>
	<meta http-equiv="Content-Type" content="text/html; CHARSET=UTF-8"/>
	<meta http-equiv="Content-Style-Type" content="text/css" />
	<meta http-equiv="Content-Script-Type" content="text/javascript" />

	<link href="popup.css" type="text/css" rel="stylesheet"/>

	<script type="text/javascript" language="javascript" src="gallerypopup.js"></script>
	<script type="text/javascript" language="javascript"> 
       /* <![CDATA[ */
       
        var _mode = LookupMulti;
        
        function applychanges()
		{   
			buildReturnValue( getById('tblSelected').rows, location.href.substr(location.href.indexOf('mid=')+4))
			window.close();
		}

		function cancel()
		{
			//var cancelItem = new popupCancel();
			//window.returnValue = cancelItem;
			window.close();
		}

		function windowonload()
		{
		    appendExistingItems();
			if ( !checkMode( _mode, LookupBrowse ) )
			{
				findValue.focus();
			}
		}

		function appendExistingItems()
		{
		    //alert(location.href);
		    var modul = location.href.substr(location.href.indexOf('mid=')+4);
    	    if (location.href.substr(location.href.indexOf('objectclasses=')+14,3) == "102"){
    	        oc = "DownloadRoles";
    	        oc1 = "102";}
	        else if (location.href.substr(location.href.indexOf('objectclasses=')+14,3) == "101"){
	            oc = "OwnerLookup";
    	        oc1 = "101";}
    	     
            //var exist = opener.getById("dnn_ctr" + modul + "_Settings_ctl" + oc + "_txtItemsID").value.substr(1);
            var exist = opener.getById(opener.namingContainer +"_ctl" + oc + "_txtItemsID").value.substr(1);
            var seletecitems = exist.split(";");
			var len = seletecitems.length;
            //if (opener.getById("dnn_ctr" + modul + "_Settings_ctl" + oc + "_celLookupItems").getElementsByTagName("span")[0]){
            if (opener.getById(opener.namingContainer + "_ctl" + oc + "_celLookupItems").getElementsByTagName("span")[0]){
			    for ( var i = 0; i < len; i++ )
    			{
	    			var item = seletecitems[i];
		    		subitem = item.split(":");
			    	//html = opener.getById("dnn_ctr" + modul + "_Settings_ctl" + oc + "_celLookupItems").getElementsByTagName("span")[i].innerHTML;
			    	html = opener.getById(opener.namingContainer + "_ctl" + oc + "_celLookupItems").getElementsByTagName("span")[i].innerHTML;
    				html += "<input type='hidden' name='oname' value='" + subitem[1] +"' /><input type='hidden' name='otype' value='" + oc1 + "' /><input type='hidden' name='oid' value='" +subitem[0] + "' /><input type='hidden' name='olocation' value='' />";
                    if (subitem.length == 2)
    	    		    appendItem( html, 0);
    			 }
    	    }
			//setNavigationState();
			getById('tblNoRecords').style.display = ( getById('tblSelected').rows.length == 0 ? "" : "none" );
		}

		function resultsReady()
		{
            //if ( frmResults.document.readyState == "complete" )
			//{
			   	document.body.style.cursor = "auto";

				if ( !checkMode( _mode, LookupBrowse ) )
				{
				    var btnGo = getById('btnGo');
					btnGo.disabled = false;
					btnGo.style.cursor = "pointer";
				}

				setNavigationState();
			//}
		}

		function removeSelected()
		{
		    var tblSelected = getById('tblSelected');
			var items = tblSelected.selectedItems;
			if (items) {
			    for ( var i = 0; i < items.length; i++ )
			    {
			        items[i].parentNode.removeChild(items[i]);
			    }

			    items.splice( 0, items.length );

			    if ( tblSelected.rows.length > 0 )
			    {
				    selectItem( tblSelected, tblSelected.rows[0], true );
			    }
			    setNavigationState();
			}
		}

		function duplicateSelection( oid )
		{
			var len = getById('tblSelected').rows.length;
			for ( var i = 0; i < len; i++ )
			{
				if ( document.getElementsByName("oid")[i].value == oid )
				{
					return true;
				}
			}
			return false;
		}

		function appendItem(html, i)
		{
		    var tblSelected = getById('tblSelected');
			var tr2	= tblSelected.insertRow(i);
			var td = tr2.insertCell(0);
			td.innerHTML = html;

            anz = tblSelected.rows.length;
            ins = new Array();
            for ( var i2 = 0; i2 < anz; i2++ ){
                ins[i2] = tblSelected.rows[i2].cells[0].innerHTML;
            }
            ins.sort();
            for ( var i2 = 0; i2 < anz; i2++ ){
                tblSelected.deleteRow(i2);
			    var tr = tblSelected.insertRow(i2);
        		var td= tr.insertCell(0);
			    td.noWrap = true;
			    td.innerHTML = ins[i2];
            }  
		}
		
		function appendSelected()
		{
		    var frmResults = getById('frmResults');
		    var tblResults = getById('tblResults', frmResults.contentWindow.document);
		    if (tblResults == undefined)
				return;
			var items = tblResults.rows;
			if ( items )
			{
				var len	= items.length;
				for ( var i = 0; i < len; i++ )
				{
				    if (items[i].selected) {
    					var o = items[i];
	    				if ( !duplicateSelection(frmResults.contentWindow.document.getElementsByName("oid")[i].value) )
		    			{
				    		appendItem(findFirstChild(o).innerHTML, 0);
					    }
					}
				}
				setNavigationState();
			}
		}

		function setNavigationState()
		{
		    var frmResults = getById('frmResults');
		    var tblResults = getById('tblResults', frmResults.contentWindow.document);
		    var tblSelected = getById('tblSelected');
		    
		    if (tblResults != undefined)
			{	
			    var btnAppend = getById('btnAppend');
				btnAppend.disabled = ( tblResults.rows.length == 0 );		
				btnAppend.style.cursor = ( tblResults.rows.length == 0 ? "auto" : "pointer");		
				getById('tblNoRecords').style.display = ( tblSelected.rows.length == 0 ? "" : "none" );
			}
			if (tblSelected != undefined)
			{
			    var btnRemove = getById('btnRemove');
			    var hasSelected = ((tblSelected.selectedItems) && (tblSelected.selectedItems.length > 0));    //WES-note IE8 Beta 1 throws exception on rows property-known issue corrected in Beta 2
				btnRemove.disabled = !hasSelected;
				btnRemove.style.cursor = ( hasSelected ? "pointer" : "auto");
			}
		}
		
		window.onload = windowonload;
		
	  /* ]]> */	
	</script>

</head>
<body>
	<table style="width: 100%;" cellspacing="0" cellpadding="0">
		<tbody>
			<tr>
				<td colspan="2">
					<div class="Main">
						<div id="divSearch">
							<table cellspacing="0" cellpadding="0" style="width: 100%">
								<tbody>
									<tr>
										<td class="Search">
											<dnn:PopupSearch ID="ctlSearch" runat="server"></dnn:PopupSearch>
										</td>
									</tr>
									<tr>
										<td>
											<table id="tblFind" cellspacing="0" cellpadding="0" style="width: 100%">
												<tbody>
													<tr>
														<td style="width:100%;">
															<iframe id="frmResults" name="frmResults" onload="resultsReady()" scrolling="no"
																style="width: 100%"></iframe>
														</td>
													</tr>
													<tr>
														<td style="text-align: right; height: 24px; width:100%;">
                                                            <button id="btnAppend" title="<%= Appendinfo %>" style="width: 100px" disabled="disabled"
																onclick="javascript:appendSelected();" tabindex="5" ><%=Appendtxt%></button>&nbsp;
															<button id="btnRemove" title="<%= Removeinfo %>" style="width: 100px" disabled="disabled"
																onclick="javascript:removeSelected();" tabindex="6" type="button"><%=Removetxt%></button>&nbsp;
														</td>
													</tr>
													<tr>
														<td>
															<div class="Objects" id="rtnObjList" style="height: 100px">
																<table id="tblSelected" onkeydown="javascript:listKeyDown(this);" ondblclick="javascript:removeSelected();"
																	onclick="javascript:clickItem(this, event);" cellspacing="0" cellpadding="2" style="width: 100%">
																	<tbody>
																	</tbody>
																</table>
																<table class="Message" id="tblNoRecords">
																	<tbody>
																		<tr>
																			<td class="Message" style="text-align: center"><%=SelectRecords%></td>
																		</tr>
																	</tbody>
																</table>
															</div>
														</td>
													</tr>
												</tbody>
											</table>
										</td>
									</tr>
								</tbody>
							</table>
						</div>
					</div>

					<div id="divFill" style="display: none;">
					</div>
				</td>
    		</tr>
 			<tr>
				<td colspan="2" style="text-align: right; height: 30px">
                </td>
              </tr>
			<tr>
				<td class="Footer" colspan="2" style="text-align: right">
					&nbsp;
					<button id="btnOK" onclick="javascript:applychanges();" style="cursor: pointer;" type="button"><%= btnOK %></button>&nbsp;
					<button id="btnCancel" onclick="javascript:cancel();" style="cursor: pointer;" type="button"><%=btnCancel%></button></td>
			</tr>
		</tbody>
	</table>
</body>
</html>
