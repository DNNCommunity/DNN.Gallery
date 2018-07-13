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

    Partial Public Class PopupSelectSingle
        Inherits PopupPageBase
        'Inherits DotNetNuke.Framework.PageBase
        '' Change Inherits from System.Web.UI.Page to DNN.Framework.PageBase by M. Schlomann

        'Protected WithEvents ctlSearch As DotNetNuke.Modules.Gallery.PopupControls.PopupSearch

        Public PageTitle As String = ""
        Public btnOK As String = ""
        Public btnCancel As String = ""


#Region " Web Form Designer Generated Code "

        'This call is required by the Web Form Designer.
        <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

        End Sub

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

            ' Added Localization by M. Schlomann
            PageTitle = Localization.GetString(Request.QueryString("resourcekey") & "Single_Title", LocalResourceFile)
            btnOK = Localization.GetString(Request.QueryString("resourcekey") & "btnOK_text", LocalResourceFile)
            btnCancel = Localization.GetString(Request.QueryString("resourcekey") & "btnCancel_text", LocalResourceFile)
        End Sub

    End Class

End Namespace