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

Imports System.IO
Imports System.Text
Imports System.Drawing
Imports System.Drawing.Imaging

Namespace DotNetNuke.Modules.Gallery
    Public MustInherit Class ImageEdit
        Inherits System.Web.UI.Page

#Region " Web Form Designer Generated Code "

        'This call is required by the Web Form Designer.
        <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

        End Sub

        Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
            'CODEGEN: This method call is required by the Web Form Designer
            'Do not modify it using the code editor.
            InitializeComponent()
        End Sub

#End Region

        Private mZoomIndex As Integer
        Private mColorIndex As Integer
        Private mGalleryImage As GalleryFile
        Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config
        Private mGalleryAuthorization As DotNetNuke.Modules.Gallery.Authorization
        Private mImage As System.Drawing.Image
        Private mModuleId As Integer

        Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
            Dim iFormat As ImageFormat

            ' Set color index, if any.
            If Not Request.QueryString("c") Is Nothing Then
                mColorIndex = Int32.Parse(Request.QueryString("c"))
                If mColorIndex < 0 Then
                    mColorIndex = 0
                End If
            End If

            ' Set zoom index, if any. WES - added min/max constraints
            If Not Request.QueryString("zoomindex") Is Nothing Then
                mZoomIndex = Math.Min(3, Math.Max(-3, Int32.Parse(Request.QueryString("zoomindex"))))
            End If

            If Not Request.QueryString("mid") Is Nothing Then
                mModuleId = Int32.Parse(Request.QueryString("mid"))
            End If

            ' Check for View permission added by WES - 2-10-2009
            If GalleryAuthorization.HasViewPermission Then
                Dim _request As GalleryViewerRequest = New GalleryViewerRequest(mModuleId)
                mGalleryImage = _request.CurrentItem
                mGalleryConfig = mGalleryImage.GalleryConfig

                ' Check for valid image file extension added by wES - 2-10-2009
                If mGalleryConfig.IsValidImageType(Path.GetExtension(mGalleryImage.Path)) Then

                    ' Get smaller dimension from gallery config
                    Dim maxDimension As Integer
                    If mGalleryConfig.FixedWidth < mGalleryConfig.FixedHeight Then
                        maxDimension = mGalleryConfig.FixedWidth
                    Else
                        maxDimension = mGalleryConfig.FixedHeight
                    End If

                    Dim sourceImage As Bitmap = New Bitmap(mGalleryImage.Path)

                    iFormat = sourceImage.RawFormat

                    maxDimension += mZoomIndex * 100

                    ' Aspect ratio logic provided by Bryce Jasmer.
                    Dim aspect As Single = sourceImage.PhysicalDimension.Width / sourceImage.PhysicalDimension.Height
                    Dim size As Size
                    If aspect <= 1.0 Then ' Portrait
                        size = New Size(CInt(maxDimension * aspect), maxDimension)
                        ' Landscape
                    Else
                        size = New Size(maxDimension, CInt(maxDimension / aspect))
                    End If

                    ' Resize while retaining image quality.
                    ' Provided by Jan Emil Christiansen.
                    Dim newImage As Bitmap = New Bitmap(sourceImage, size)
                    sourceImage.Dispose()

                    ' Complete refactoring of manner in which rotate/flip operations are handled added by
                    ' WES on 2-10-2009 to simplify code, reduce length of query string, and correct issues
                    ' with wrong orientation produced after sequence of mixed rotates and flips.

                    If Not Request.QueryString("rotateflip") Is Nothing Then
                        Dim rotateFlip As Integer = Math.Max(0, Math.Min(7, Int32.Parse(Request.QueryString("rotateflip"))))
                        newImage = GalleryGraphics.FlipImage(newImage, CType(rotateFlip, RotateFlipType))
                    End If

                    If Not Request.QueryString("color") Is Nothing Then
                        Dim colorIndex As Integer = Int16.Parse(Request.QueryString("color"))
                        If colorIndex > 0 Then
                            ToggleColorIndex(newImage, colorIndex)
                        End If
                    End If

                    ' Update image - ItemEditPermission check added by WES 2-10-09
                    If Not Request.QueryString("mode") Is Nothing Then
                        If Request.QueryString("mode") = "save" AndAlso GalleryAuthorization.HasItemEditPermission(mGalleryImage) Then
                            UpdateImage(newImage, iFormat)
                        End If
                    End If

                    If mGalleryConfig.UseWatermark AndAlso (Not mGalleryImage.Parent.WatermarkImage Is Nothing) Then
                        Dim imgWatermark As Image = Image.FromFile(mGalleryImage.Parent.WatermarkImage.Path)
                        mImage = GalleryGraphics.DrawWatermark(newImage, imgWatermark)
                        imgWatermark.Dispose()
                        newImage.Dispose()
                    Else
                        mImage = newImage
                    End If

                    ' Send image to the browser.
                    Response.ContentType = GetContentType(iFormat)

                    'Added by William Severance to use intermediate memory stream when
                    'image type is PNG to avoid generic GDI+ error

                    Dim ms As MemoryStream = Nothing
                    Try
                        If Response.ContentType = "image/png" Then
                            ms = New MemoryStream
                            mImage.Save(ms, ImageFormat.Png)
                            ms.WriteTo(Response.OutputStream)
                        Else
                            mImage.Save(Response.OutputStream, iFormat)
                        End If
                    Finally
                        If Not ms Is Nothing Then ms.Dispose()
                        newImage.Dispose()
                        mImage.Dispose()
                    End Try
                End If
            End If
        End Sub

        Private Sub UpdateImage(ByVal NewImage As Bitmap, ByVal iFormat As ImageFormat)
            Try
                mGalleryImage.UpdateImage(NewImage, iFormat)
                If Not mGalleryImage.Parent Is Nothing Then
                    Config.ResetGalleryFolder(mGalleryImage.Parent)
                Else
                    Config.ResetGalleryConfig(mModuleID)
                End If

            Catch exc As Exception
                Throw
            End Try
        End Sub

        Public ReadOnly Property GalleryAuthorization() As DotNetNuke.Modules.Gallery.Authorization
            Get
                If mGalleryAuthorization Is Nothing Then
                    mGalleryAuthorization = New DotNetNuke.Modules.Gallery.Authorization(mModuleId)
                End If
                Return mGalleryAuthorization
            End Get
        End Property

    End Class
End Namespace



