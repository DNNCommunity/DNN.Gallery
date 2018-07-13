'
' DotNetNuke® - http://www.dotnetnuke.com
' Copyright (c) 2002-2011 by DotNetNuke Corp. 

'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
' DEALINGS IN THE SOFTWARE.
'

Option Strict On

Imports System.Web
Imports DotNetNuke


Namespace DotNetNuke.Modules.Gallery.PopupControls

    Partial Class PopupSelectMulti
        Inherits PopupPageBase

        'Inherits DotNetNuke.Framework.PageBase
        ''Changed Inherits System.Web.UI.Page to DNN Framework PageBase by M. Schlomann

        Public PageTitle As String = ""
        Public Appendtxt As String = ""
        Public Appendinfo As String = ""
        Public Removeinfo As String = ""
        Public Removetxt As String = ""
        Public btnOK As String = ""
        Public btnCancel As String = ""
        Public SelectRecords As String = ""


#Region " Web Form Designer Generated Code "

        'This call is required by the Web Form Designer.
        <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

        End Sub

        'Protected WithEvents btnTest As System.Web.UI.WebControls.Button
        'Protected WithEvents Button1 As System.Web.UI.HtmlControls.HtmlButton
        'Protected WithEvents Button2 As System.Web.UI.HtmlControls.HtmlButton
        'Protected WithEvents ctlSearch As DotNetNuke.Modules.Gallery.PopupControls.PopupSearch
        'Protected WithEvents ctlSearch As DotNetNuke.Modules.Gallery.PopupControls.PopupSearch

        'NOTE: The following placeholder declaration is required by the Web Form Designer.
        'Do not delete or move it.
        Private designerPlaceholderDeclaration As System.Object

        Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
            'CODEGEN: This method call is required by the Web Form Designer
            'Do not modify it using the code editor.
            InitializeComponent()
        End Sub

#End Region

        Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

            'Put user code to initialize the page here

            ctlSearch.PortalID = PortalSettings.PortalId

            'Localization  added by M. Schlomann/William Severance
            PageTitle = Localization.GetString("_Title", LocalResourceFile)
            Appendtxt = Localization.GetString("btnAppend_text", LocalResourceFile)
            Appendinfo = Localization.GetString("btnAppend_info", LocalResourceFile)
            Removetxt = Localization.GetString("btnRemove_text", LocalResourceFile)
            Removeinfo = Localization.GetString("btnRemove_info", LocalResourceFile)
            btnOK = Localization.GetString("btnOK_text", LocalResourceFile)
            btnCancel = Localization.GetString("btnCancel_text", LocalResourceFile)
            SelectRecords = Localization.GetString("SelectRecords_text", LocalResourceFile)
        End Sub

    End Class

End Namespace