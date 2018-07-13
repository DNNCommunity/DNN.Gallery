var SelectedColorFocused = "#adc3e7";
var SelectedColorUnfocused = "#eeeeee";
var _mode = 0;
var LookupBrowse = 0x1;
var LookupMulti = 0x4;
var LookupSingle = 0x2;

var DNN = 1;
var ADSI = 2;
var CRM = 3;
//var FORUM = 4;
var GALLERY = 5;
var LIBRARY = 6;

var popUp = null; 

function getById (sID, oCtl){
	if (oCtl == null)
		oCtl = document;
	if (oCtl.getElementById)
		return oCtl.getElementById(sID);
	else if (oCtl.all)
		return oCtl.all(sID);
	else
		return null;
}

function getModId(){
var modulstring = "" + getById("imgLookup").onclick;
var modulstring = modulstring.replace(/\"/g,"'");
var strstart = modulstring.indexOf("mid=")+4;
var strstop = modulstring.indexOf("', 'dialogHeight");
var strcount = strstop - strstart;
var modulid = modulstring.substr(strstart,strcount);	
return modulid;
}

function RefreshParent() 
{ 			
	popUp.close();	
	window.location.reload();		
} 

function OpenGalleryPopup(targetControl, url, features)
{		
    var args = new PopupArguments();

	var dwidth = features.substr(features.indexOf('dialogWidth:')+12,3);
	var dheight = features.substr(features.indexOf('dialogHeight:')+13,3);

	if (targetControl != null)
	{		
		var tdDisplay = findFirstChild(targetControl.parentNode.parentNode);
		var tdValue = targetControl.parentNode;
		args.items = tdDisplay.getElementsByTagName("span");		
	}
	
	var returnValue = window.open(url,"select",'height='+dheight+',width='+dwidth+',toolbar=no,directories=no,status=no,menubar=no,scrollbars=no,resizable=yes,center=yes,modal=yes');
} 
               
function findFirstChild(elm) {
	if ( !elm.childNodes.length ) {
		return;
	}
	var children = elm.childNodes.length;
	for ( var i = 0; i <= children; ++i ) {
		if ( elm.childNodes[i].nodeType == 1 ) {
			return elm.childNodes[i];
		}
	}
	return;
}

function PopupArguments()
{	
	this.items = null;
}

function CleartargetControl(targetControl)
{
    //var modulid = getModId();
    var from = "" + targetControl.parentNode.id;
    if (from.indexOf("OwnerLookup") > 0)
        oc = "OwnerLookup";
    else 
        oc = "DownloadRoles";
    //getById("dnn_ctr" + modulid + "_Settings_ctl" + oc + "_txtInputItemsID").value = "";
    //getById("dnn_ctr" + modulid + "_Settings_ctl" + oc + "_txtItemsID").value = "";
    //getById("dnn_ctr" + modulid + "_Settings_ctl" + oc + "_celLookupItems").innerHTML = "<span class='normaltextbox'>" + notSpecified + "</span>";
    getById(namingContainer + "_ctl" + oc + "_txtInputItemsID").value = "";
    getById(namingContainer + "_ctl" + oc + "_txtItemsID").value = "";
    getById(namingContainer + "_ctl" + oc + "_celLookupItems").innerHTML = "<span class='normaltextbox'>" + notSpecified + "</span>"    
}

function PopuplatetargetControl(targetControl, resultValues)
{
	var tdDisplay = findFirstChild(targetControl.parentNode.parentNode);	
	var idField = findFirstChild(targetControl.parentNode);
	var iHTML = "";
	var idList = "";
	for (var i = 0; i < resultValues.items.length; ++i)
	{	
		var item = resultValues.items[i];
		if (item.name == 'popupcancel')
		{
			return;
		}
		var elem = parent.document.createElement("SPAN");		
		elem.className	= 	"TTTNormalTextBox";
		elem.oid	= 	item.id;
		elem.otype	= 	item.type;
		elem.olocation = item.location;
		elem.innerHTML	= 	item.html;		
		iHTML += elem.outerHTML;
		iHTML += "<br />";
		idList += ";" + item.id + ":" + item.name;
		//idList += item.id + ";";	
		//nameList += item.name + "<br/>";	
	}	
	if (iHTML.length == 0)
	{
		iHTML = "<span class='normaltextbox'>" + notSpecified + "</span>";
		
	}	
	tdDisplay.innerHTML = iHTML;	
	idField.value = idList;
	//nameField.value = nameList;
		
}

function checkMode(mode, option)
{
	return ((mode & option) == option);
}

function openPopup()
{
	return window.createPopup();
}

function search()
{
    document.body.style.cursor = "wait";
	var findValue = getById('findValue');
	var url = "popupdata.aspx";
	url += window.location.search;	
	url += "&objectclass=" + getById("selObject").value;		
	url += "&location=" + getById('selLocation').value;
	url += "&searchvalue=" + URLEncode(findValue.value);
	getById('btnGo').disabled = true;
	findValue.focus();
	getById("frmResults").src = url;
}
function selectChange(o)
{	
	if (checkMode(_mode, LookupBrowse))
	{
		search();
	}
}
function buildReturnValue(rows, modul, objectclass)
{	
	var len = rows.length;
	if (len > 0){
	    var retvalue = "";
	    var retvalue2 = "";
	    var retvalue3 = "";

	    if (document.getElementsByName("otype")[0].value == "102")
	        oc = "DownloadRoles";
	    else if (document.getElementsByName("otype")[0].value == "101")
	        oc = "OwnerLookup";
	    
	    for (var i = 0; i < len; i++)
	    {
		    var tr = rows[i];
		    retvalue += ";"+document.getElementsByName("oid")[i].value;
		    retvalue2 += ";"+document.getElementsByName("oid")[i].value + ":"+ document.getElementsByName("oname")[i].value;
		    lowinner = tr.innerHTML.toLowerCase();
		    retvalue3 += "<span class='normaltextbox'>"+lowinner.substr(lowinner.indexOf('<img'),lowinner.indexOf('&nbsp;')-lowinner.indexOf('<img')) + "&nbsp;" + document.getElementsByName("oname")[i].value + "</span><br />";
	    }
	    
	    
	    //opener.getById("dnn_ctr" + modul + "_Settings_ctl" + oc + "_txtInputItemsID").value = retvalue;
        //opener.getById("dnn_ctr" + modul + "_Settings_ctl" + oc + "_txtItemsID").value = retvalue2;
        //opener.getById("dnn_ctr" + modul + "_Settings_ctl" + oc + "_celLookupItems").innerHTML = retvalue3;
        opener.getById(opener.namingContainer + "_ctl" + oc + "_txtInputItemsID").value = retvalue;
        opener.getById(opener.namingContainer + "_ctl" + oc + "_txtItemsID").value = retvalue2;
        opener.getById(opener.namingContainer + "_ctl" + oc + "_celLookupItems").innerHTML = retvalue3;
    }
    else{
	    //opener.getById("dnn_ctr" + modul + "_Settings_ctl" + oc + "_txtInputItemsID").value = "";
        //opener.getById("dnn_ctr" + modul + "_Settings_ctl" + oc + "_txtItemsID").value = "";
        //opener.getById("dnn_ctr" + modul + "_Settings_ctl" + oc + "_celLookupItems").innerHTML = "<span class='normaltextbox'>" + opener.notSpecified + "</span>";
        opener.getById(opener.namingContainer + "_ctl" + oc + "_txtInputItemsID").value = "";
        opener.getById(opener.namingContainer + "_ctl" + oc + "_txtItemsID").value = "";
        opener.getById(opener.namingContainer + "_ctl" + oc + "_celLookupItems").innerHTML = "<span class='normaltextbox'>" + opener.notSpecified + "</span>";
    }
}

function initSelectedItems(table)
{
	//alert("initSelectedItems");
	if (table.selectedItems == undefined)
	{
		table.selectedItems = new Array();
		table.onactivate=activateItems;
		table.ondeactivate=deactivateItems;
	}
}
function getActiveItem(elem)
{
	//alert("getActiveItem");
	while (elem.tagName != "TR")
	{
		elem = elem.parentNode;
	}
	return elem;
}
function unselectItems(table)
{
	//alert("unselectItems1");
//	var multiSelect = checkMode(_mode, LookupMulti);
    if (table.selectedItems == undefined) return;
//	if (multiSelect && event.shiftKey) return;
//	if (multiSelect && event.ctrlKey && event.keyCode == 0) return;
	
	while (table.selectedItems.length > 0)
	{
		unselectItem(table, table.selectedItems[0]);	
	}
}
function unselectItem(table, item)
{
	//alert("unselectItems2");
	if (table.selectedItems == undefined) return;
	var items = table.selectedItems;
	for (var i = 0; i < items.length; i++)
	{
		if (items[i] == item)
		{
			items[i].selected = 0;
			items[i].style.backgroundColor = "";
			table.selectedItems.splice(i, 1);
			break;
		}
	}
}

function selectItem(table, item, focused)
{
	initSelectedItems(table);
	item.selected = 1;
	
	if (focused)
		item.style.backgroundColor = SelectedColorFocused;
	else
		item.style.backgroundColor = SelectedColorUnfocused;
	
    table.lastSelected = item;
	table.selectedItems.push (item);
	
//	if ( table.id == "tblResults" && "undefined" != typeof(parent.btnProperties))
//	{
//		parent.btnProperties.disabled = (item.oid == undefined);
//	}

    // William Severance - modified to disable btnAdd or btnRemove if no items selected in table
    var btn = undefined;
    switch (table.id) {
       case 'tblResults':
           btn = getById ('btnAppend', parent.document);
           break;
       case 'tblSelected':
           btn = getById ('btnRemove', parent.document);
           break;
    }
    if (btn != undefined) {
        btn.disabled = (table.selectedItems.length == 0);
    }
}
function shiftSelectItems(table, o)
{
	if (table.lastSelected != undefined)
	{
		var lastSelected = table.lastSelected;
		
		if (lastSelected.rowIndex >= o.rowIndex)
		{
			var rows = table.rows;
//			for (var i = o.rowIndex; i < lastSelected.rowIndex; i++)
			{
				selectItem(table, rows[i], true);
			}
		} 
		else
		{
			var rows = table.rows;
			
			for (var i = lastSelected.rowIndex + 1; i <= o.rowIndex; i++)
			{
				selectItem(table, rows[i], true);
			}
		}
	}
}
function clickItem(table, event)
{	
    if (!event)
	    //event = window.event;
	    event = window.event.keyCode; 
    if (event.srcElement)
        eventsrc = event.srcElement;
    else
        eventsrc = event.target;
        
	if (eventsrc.tagName == "TBODY") return;
	var item = getActiveItem(eventsrc);
	
	var multiSelect = checkMode(parent._mode, LookupMulti);
	
	if (multiSelect && event.shiftKey && !item.selected)
	{
			shiftSelectItems(table, item);	
	}
	else if (multiSelect && event.ctrlKey && item.selected)
	{
		unselectItem(table, item, true);
	}
	else
	{
		unselectItems(table);
		selectItem(table, item, true);
	}
	return item;
}
function findValueKeyDown(event)
{
	if (!event)
	    event = window.event;

	if (event.keyCode == 13)
	{
		search();	
	}
}
function listKeyDown(table,event)
{
	if (!event)
	    event = window.event;

	if (event.keyCode == 32)
	{
		table.ondblclick();
	}
	else if (event.keyCode == 38)
	{
		var item = table.lastSelected;
		
		if (item && item.rowIndex > 0)
		{
			item = item.previousSibling;
			
			if (item.unselectable != undefined)
			{
				item = item.previousSibling;
				
				if (item == null)
					return;
			}
			unselectItems(table);
			
			selectItem(table, item, true);
			
			
			
			item.scrollIntoView(true);
		}
	} 
	else if (event.keyCode == 40)
	{
		var item = table.lastSelected;
		if (item && item.rowIndex < table.rows.length- 1)
		{
			item = item.nextSibling;
			
			if (item.unselectable != undefined)
			{
				item = item.nextSibling;
				
				if (item == null)
					return;
			}
			unselectItems(table);
			
			selectItem(table, item, true);
			
			
			
			item.scrollIntoView(false);
		}
	}
	else if (!(event.shiftKey || event.ctrlKey) && ((event.keyCode > 47 && event.keyCode < 58) || (event.keyCode > 64 && event.keyCode < 91)))
	{
		var code;
		var len = table.rows.length;
		
		for (i = (checkMode(_mode, LookupMulti)) ? 0 : 2; i < len; i++)
		{
			code = table.rows[i].cells[0].innerText.charCodeAt(0);
			
			if ((code == event.keyCode) || ((event.keyCode > 64 && event.keyCode < 91) && (code == event.keyCode + 32)))
			{
				item = table.rows[i];
				
				unselectItems(table);
			
				selectItem(table, item, true);
				item.scrollIntoView(true);
				break;
			}
		}
	}
}
function focusSelectedItems(table, focused)
{
	if (table.selectedItems == undefined) return;
	var items = table.selectedItems;
	
	if (items.length == 0 && table.rows.length > 0)
	{
		selectItem(table, table.rows[0], true);
		//alert("table.rows.length > 0");
	}
		
	for (var i = 0; i < items.length; i++)
	{
		if (focused)
		{
			items[i].style.backgroundColor = SelectedColorFocused;
		}
		else
		{
			items[i].style.backgroundColor = SelectedColorUnfocused;
		}
	}
}
function activateItems()
{	if (!this.contains (event.toElement))
	{
		focusSelectedItems(this, true);
	}
}
function deactivateItems()
{  	if (!this.contains (event.toElement))
	{
		focusSelectedItems(this, false);
	}
}
//function showProperties()
//{
//	var items = frmResults.tblResults.selectedItems;	
//	if (items.length == 0)
//	{
//		alert("You must first select an object.");
//	}
//	else if (items.length > 1)
//	{
//		alert("You must only select one object.");
//	}
//	else
//	{		
//	}
//}

function LookupItem()
{
	//alert("LookupItem");
	LookupItems
	this.id = "";
	this.name = "";
	this.location = "";
	this.html = "";
	this.type = "";
	this.objectclass = "" ;
	this.values = null;
	this.keyValues = null;
}
function LookupItemData(name, value)
{
		
	this.name = name;
	this.value = value;
}
function popupCancel()
{
	this.items = new Array();
	var ci = new LookupItem();
	ci.id = '0';
	ci.name = 'popupcancel';
	this.items.push (ci);
}
		
function LookupItems()
{
		
	this.add = _add;
	this.items = new Array();
	
	
	function _add(tr, columns)
	{	
		var li = new LookupItem();
		var td = tr.cells[0];
		li.id		= tr.oid;
		li.name		= td.innerText;
		li.location = tr.olocation;
		li.html		= td.innerHTML;
		li.type		= tr.otype;		
		li.values	= new Array();
		li.keyValues	= new Array();
		
		var len = columns.length;
		if (len > 1)
		{
			for (var i = 1; i < len; ++i)
			{
				li.keyValues[new String(columns[i].name)] = new LookupItemData(columns[i].name, tr.cells[i].innerText)
				li.values.push(new LookupItemData(columns[i].name, tr.cells[i].innerText));
			}
		}		
		this.items.push (li);
	}
}

function URLEncode(s)
{
    s = s.replace(" ", "%20");
	s = s.replace("\"", "%22");
	s = s.replace("#", "%23");
	s = s.replace("&", "%26");
	return s.replace("+", "%2B");
}

/*/
/ /  Author: Jeremy Falcon
/ /    Date: November 08, 2001
/ / Version: 1.4
/*/

/*/ THIS FILE CONTAINS FUNCTIONS THAT WILL WRAP THE POP-UP PROCESS /*/

// this variable will hold the window obect
// we only allow one pop-up at a time
//var popup = null;
/*/
/ / PURPOSE:
/ /		To create and center a pop-up window.
/ /
/ / COMMENTS:
/ /		It will replace to old pop-up if called
/ / 	without calling DestroyWnd() first..
/*/

function CreateWnd (file, width, height, resize)
{
	var doCenter = false;
    if((popUp == null) || popUp.closed)
	{
		attribs = "";

		/*/ there's no popup displayed /*/

		// assemble some params
		if(resize) size = "yes"; else size = "no";

		/*/
		/ / We want to center the pop-up; however, to do this we need to know the
		/ / screen size.  The screen object is only available in JavaScript 1.2 and
		/ / later (w/o Java and/or CGI helping), so we must check for the existance
		/ / of it in the window object to determine if we can get the screen size.
		/ /
		/ / It is safe to assume the window object exists because it was implemented
		/ / in the very first version of JavaScript (that's 1.0).
		/*/
		for(var item in window)
			{ if(item == "screen") { doCenter = true; break; } }

		if(doCenter)
		{	/*/ center the window /*/

			// if the screen is smaller than the window, override the resize setting
			if(screen.width <= width || screen.height <= height) size = "yes";

			WndTop  = (screen.height - height) / 2;
			WndLeft = (screen.width  - width)  / 2;

			// collect the attributes
			attribs = "width=" + width + ",height=" + height + ",resizable=" + size + ",scrollbars=" + size + "," + 
			"status=no,toolbar=no,directories=no,menubar=no,location=no,help=no;,top=" + WndTop + ",left=" + WndLeft;
		}
		else
		{
			/*/
			/ / There is still one last thing we can do for JavaScrpt 1.1
			/ / users in Netscape.  Using the AWT in Java we can pull the
			/ / information we need, provided it is enabled.
			/*/
			if(navigator.appName=="Netscape" && navigator.javaEnabled())
			{	/*/ center the window /*/

				var toolkit = java.awt.Toolkit.getDefaultToolkit();
				var screen_size = toolkit.getScreenSize();

				// if the screen is smaller than the window, override the resize setting
				if(screen_size.width <= width || screen_size.height <= height) size = "yes";

				WndTop  = (screen_size.height - height) / 2;
				WndLeft = (screen_size.width  - width)  / 2;

				// collect the attributes
				attribs = "width=" + width + ",height=" + height + ",resizable=" + size + ",scrollbars=" + size + "," + 
				"status=no,toolbar=no,directories=no,menubar=no,location=no,help=no,top=" + WndTop + ",left=" + WndLeft;
			}
			else
			{	/*/ use the default window position /*/

				// override the resize setting
				size = "yes";

				// collect the attributes
				attribs = "width=" + width + ",height=" + height + ",resizable=" + size + ",scrollbars=" + size + "," + 
				"status=no,toolbar=no,directories=no,menubar=no,location=no,help=no";
			}
		}

		// create the window
		popUp = open(file, "", attribs);
	}
	else
	{
		// destory the current window
		DestroyWnd();
		// recurse, just once, to display the new window
		CreateWnd(file, width, height, resize);
	}
}

/*/
/ / PURPOSE:
/ /		To destroy the pop-up window.
/ /
/ / COMMENTS:
/ /		This is available if wish to destroy
/ / 	the pop-up window manually.
/*/

function DestroyWnd ()
{
	// close the current window
	if ((popUp != null) && (!popUp.closed))
	{
		popUp.close();
		popUp=null;
	}
}



