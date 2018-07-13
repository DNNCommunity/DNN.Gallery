<%@ Page language="vb" Inherits="DotNetNuke.Modules.Gallery.PopupControls.PopupSelectSingle" Codebehind="PopupSelectSingle.aspx.vb" AutoEventWireup="false" %>
<%@ Register TagPrefix="dnn" Assembly="Dotnetnuke.Modules.Gallery" Namespace="DotNetNuke.Modules.Gallery.PopupControls" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" style="width:100%;height:100%">
	<head>
	  <title><%= PageTitle %></title>
	  <meta http-equiv="Content-Type" content="text/html; CHARSET=UTF-8" />
	  <meta http-equiv="Content-Style-Type" content="text/css" />
	  <meta http-equiv="Content-Script-Type" content="text/javascript" />
	  
	  <link href="popup.css" type="text/css" rel="stylesheet" />
	  
	  <script type="text/javascript" language="javascript" src="gallerypopup.js"></script>
	  <script type="text/javascript" language="javascript">     
       /* <![CDATA[ */
       
		function applychanges()
		{
            selectItem();
		}

		function cancel()
		{
			window.close();
		}

		function windowonload()
		{
			//findValue.focus();
		}

		function resultsReady()
		{
			document.body.style.cursor = "auto";
			var btnGo=getById('btnGo');
			if (btnGo != null) {
				 btnGo.disabled = false;
				 btnGo.style.cursor = "pointer";
			}
		}

		function selectItem()
		{	
		    var oc = "OwnerLookup";
		    var frmResults = getById('frmResults')
		    var tblResults = getById('tblResults', frmResults.contentWindow.document);
		    
			if (tblResults == undefined){
			    opener.getById(opener.namingContainer + "_ctl" + oc + "_celLookupItems").innerHTML = "<span class='normaltextbox'>" + opener.notSpecified + "</span>";
			} else {
    			var resdoc = frmResults.contentWindow.document;
	    		var modul = location.href.substr(location.href.indexOf('mid=')+4);
		    	var len = tblResults.rows.length;
			    for (var i = 0; i < len; i++){
			        var tr = tblResults.rows[i];
			        if (tr.selected){
			            retvalue = resdoc.getElementsByName("oid")[i].value;
		                retvalue2 = resdoc.getElementsByName("oid")[i].value + ":"+ resdoc.getElementsByName("oname")[i].value;
		                lowinner = tr.innerHTML.toLowerCase();
		                retvalue3 = "<span class='normaltextbox'>"+lowinner.substr(lowinner.indexOf('<img'),lowinner.indexOf('&nbsp;')-lowinner.indexOf('<img')) + "&nbsp;" + resdoc.getElementsByName("oname")[i].value + "</span><br />";
	                    //opener.getById("dnn_ctr" + modul + "_Settings_ctl" + oc + "_txtInputItemsID").value = retvalue;
                        //opener.getById("dnn_ctr" + modul + "_Settings_ctl" + oc + "_txtItemsID").value = retvalue2;
                        //opener.getById("dnn_ctr" + modul + "_Settings_ctl" + oc + "_celLookupItems").innerHTML = retvalue3;
                        opener.getById(opener.namingContainer + "_ctl" + oc + "_txtInputItemsID").value = retvalue;
                        opener.getById(opener.namingContainer + "_ctl" + oc + "_txtItemsID").value = retvalue2;
                        opener.getById(opener.namingContainer + "_ctl" + oc + "_celLookupItems").innerHTML = retvalue3;
                        break;
			        }
			    }
		    }
			window.close();
		}

		function appendSelected()
		{
			selectItem();
		}

		function setNavigationState()
		{
		}
		
        window.onload = windowonload;
        
       /* ]]> */
	  </script>
	  
	</head>
	<body style="width:100%;height:100%">
	  <form id="Form1" runat="server" method="post">
		<table style="width:100%;height:100%" cellspacing="0" cellpadding="0">
			<tbody>
				<tr>
					<td colspan="2">
						<div class="Main">
							<div id="divSearch">
								<table style="width:100%;height:100%" cellspacing="0" cellpadding="0" >
									<tbody>
										<tr>
											<td class="search">
												<dnn:PopupSearch id="ctlSearch" runat="server"></dnn:PopupSearch>
											</td>
										</tr>
										<tr>
											<td>
												<table id="tblFind" cellspacing="0" cellpadding="0" style="width:100%;height:100%" >
													<tbody>
														<tr>
															<td style="width: 100%">
																<iframe class="Results" id="frmResults" name="frmResults" onload="resultsReady();" scrolling="no" ></iframe>
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
						<div id="divFill" style="display: none;"></div>
					</td>
				</tr>
 			<tr>
				<td colspan="2" align="right" style="height: 30px">
                </td>
              </tr>
				<tr>
					<td class="Footer" colspan="2" style="text-align: right; height: 20px">&nbsp;
						<button id="btnOK" style="cursor: pointer" onclick="javascript:applychanges();" type="button">
                            <%= btnOK %>
                        </button>&nbsp;
						<button id="btnCancel" style="cursor: pointer" onclick="javascript:cancel();" type="button">
                            <%= btnCancel %>
                        </button>
					</td>
				</tr>
			</tbody>
		</table>
	  </form>
	</body>
</html>
