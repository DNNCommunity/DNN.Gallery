<%@ Register TagPrefix="dnn" Assembly="Dotnetnuke.Modules.Gallery" Namespace="DotNetNuke.Modules.Gallery.PopupControls"  %>
<%@ Page Language="vb" AutoEventWireup="false" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
	<head>
	    <title></title>
		<meta http-equiv="Content-Type" content="text/html; CHARSET=UTF-8" />
		<meta http-equiv="Content-Style-Type" content="text/css" />
				
		<link rel="stylesheet" type="text/css" href="popup.css" />
		
		<script type="text/javascript"  language="javascript" src="gallerypopup.js"></script>
		<script type="text/javascript" language="javascript" >	
		 /* <![CDATA[ */
		
		function itemDoubleClick(event)
		{
	        if (!event)
	            //event = window.event;
	            event = window.event.keyCode; 
            if (event.srcElement)
                eventsrc = event.srcElement;
            else
                eventsrc = event.target;
			    if (typeof(eventsrc.parentNode.unselectable) == "undefined")
			    {
				    parent.appendSelected();
			    }
		}
		
		//	must cancel the selection
		function documentonselectstart()
		{
			event.returnValue = false;
		}
		
		function windowonfocus()
		{
		    var tblResults = getById('tblResults');
			tblResults.focus();
			
			if ( tblResults.rows.length > 0 ) 
			{
				if ( tblResults.selectedItems == undefined )
				{
					selectItem( tblResults, tblResults.rows[0] )
				}
				
				focusSelectedItems( tblResults, true );
			}
		}
		
		function windowonload()
		{
		    var tblResults=getById('tblResults');
			if ( tblResults.rows.length > 0 )
			{
				selectItem( tblResults, tblResults.rows[0], false );
			}
		}

		window.onfocus = windowonfocus;
		window.onload = windowonload;
        document.onselectstart = documentonselectstart;
        
         /* ]]> */	
	   </script>
	   
	</head>
	
	<body style="border-top: 0; border-right: 0; border-bottom: 0; border-left: 0">		
		<dnn:PopupData id="PopupData" Runat="server"></dnn:PopupData>					
	</body>
</html>
