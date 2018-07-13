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
Option Explicit On

Imports System.Text
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Runtime.InteropServices
Imports DotNetNuke

Namespace DotNetNuke.Modules.Gallery

    ' Utility routines for various repeated functions.
    Public Module GalleryGraphics

        'added by Tam - to avoid images have low-resolution after resize using CreateThumb
        'WES - interfaced with DNN file system and changed sub to integer to return DNN FileID
        'WES - added parameter for JPEG compression level (EncoderQuality)

        Public Function ResizeImage(ByVal Source As String, ByVal Destination As String, ByVal MaxWidth As Integer, ByVal MaxHeight As Integer, ByVal EncoderQuality As Long) As Integer
            Dim lWidth, lHeight As Integer
            Dim sRatio As Single
            Dim mImage As System.Drawing.Image = Nothing
            Dim newImage As System.Drawing.Image = Nothing
            Dim iFormat As Imaging.ImageFormat
            Dim fileID As Integer = -1

            Try
                mImage = System.Drawing.Image.FromFile(Source)
                iFormat = mImage.RawFormat
                lWidth = mImage.Width
                lHeight = mImage.Height

                If Not (lWidth <= MaxWidth AndAlso lHeight <= MaxHeight) Then
                    sRatio = CSng(lHeight / lWidth)
                    If sRatio > CSng(MaxHeight / MaxWidth) Then ' Bounded by height
                        lWidth = CInt(MaxHeight / sRatio)
                        lHeight = MaxHeight
                    Else 'Bounded by width
                        lWidth = MaxWidth
                        lHeight = CInt(MaxWidth * sRatio)
                    End If
                    newImage = New Bitmap(FixedSize(mImage, lWidth, lHeight))
                    mImage.Dispose()
                    mImage = Nothing
                    SaveImage(newImage, Destination, iFormat, EncoderQuality)
                Else
                    'Added intermediate image to allow save to Destination = Source
                    newImage = New Bitmap(mImage)
                    mImage.Dispose()
                    mImage = Nothing
                    SaveImage(newImage, Destination, iFormat, EncoderQuality)
                End If

                'William Severance - added to interface with DNN file system
                fileID = Utils.SaveDNNFile(Destination, lWidth, lHeight, False, True)

            Catch ex As Exception
                ' Eat the exception
            Finally
                If mImage IsNot Nothing Then mImage.Dispose()
                If newImage IsNot Nothing Then newImage.Dispose()
            End Try

            Return fileID
        End Function

        Public Sub SaveImage(ByVal src As System.Drawing.Image, ByVal Destination As String, ByVal Format As Imaging.ImageFormat, ByVal EncoderQuality As Long)
            Select Case Path.GetExtension(Destination).ToLower
                Case ".jpg", "jpeg"
                    SaveJPEG(src, EncoderQuality, Destination)
                Case Else
                    src.Save(Destination, Format)
            End Select
        End Sub

        'Return an ImageCodecInfo object for the image/jpeg mime type
        Private Function GetJpegCodec() As ImageCodecInfo
            'Setup a JPEG codec
            For Each Codec As ImageCodecInfo In ImageCodecInfo.GetImageEncoders
                If Codec.MimeType = "image/jpeg" Then
                    Return Codec
                End If
            Next 'Supposedly this cannot fail as image/jpg is built-in to GDI+ encoders
            Return Nothing
        End Function

        'Return the EncoderParameters object for JPEG encoding having a specified EncoderQuality
        Private Function GetJPegEncoderQualityParameters(ByVal EncoderQuality As Long) As EncoderParameters
            Dim EncParams As EncoderParameters = New EncoderParameters(1)
            EncParams.Param(0) = New EncoderParameter(System.Drawing.Imaging.Encoder.Quality, EncoderQuality)
            Return EncParams
        End Function

        'Saves a JPEG image src to the supplied stream with specified encoder quality
        Public Sub SaveJPegImage(ByVal src As Image, ByVal EncoderQuality As Long, ByVal stream As Stream)
            src.Save(stream, GetJpegCodec, GetJPegEncoderQualityParameters(EncoderQuality))
        End Sub

        'Saves a JPEG image src to the specified destination filepath with specified encoder quality
        Public Sub SaveJPEG(ByVal src As Image, ByVal EncoderQuality As Long, ByVal Destination As String)
            src.Save(Destination, GetJpegCodec, GetJPegEncoderQualityParameters(EncoderQuality))
        End Sub

        ' Code by Kenneth Courtney: using Bicubic Rescaling
        Private Function FixedSize(ByVal imgPhoto As Image, ByVal Width As Integer, ByVal Height As Integer) As Image
            Dim sourceWidth As Integer = imgPhoto.Width
            Dim sourceHeight As Integer = imgPhoto.Height
            Dim sourceX As Integer = 0
            Dim sourceY As Integer = 0
            Dim destX As Integer = 0
            Dim destY As Integer = 0

            Dim nPercent As Single = 0
            Dim nPercentW As Single = 0
            Dim nPercentH As Single = 0

            nPercentW = (Convert.ToSingle(Width) / Convert.ToSingle(sourceWidth))
            nPercentH = (Convert.ToSingle(Height) / Convert.ToSingle(sourceHeight))

            If nPercentH < nPercentW Then
                nPercent = nPercentH
                destX = System.Convert.ToInt16((Width - (sourceWidth * nPercent)) / 2)
            Else
                nPercent = nPercentW
                destY = System.Convert.ToInt16((Height - (sourceHeight * nPercent)) / 2)
            End If

            Dim destWidth As Integer = CType((sourceWidth * nPercent), Integer)
            Dim destHeight As Integer = CType((sourceHeight * nPercent), Integer)

            Dim bmPhoto As Bitmap = New Bitmap(Width, Height, PixelFormat.Format24bppRgb)
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution)

            Dim grPhoto As Graphics = Graphics.FromImage(bmPhoto)
            grPhoto.Clear(Color.White)
            grPhoto.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic

            grPhoto.DrawImage(imgPhoto, _
                                New Rectangle(destX, destY, destWidth, destHeight), _
                                New Rectangle(sourceX, sourceY, sourceWidth, sourceHeight), _
                                GraphicsUnit.Pixel)
            grPhoto.Dispose()
            Return bmPhoto

        End Function

        ' Provided by Jan Emil Christiansen.
        Public Function DrawWatermark(ByRef sourceImage As System.Drawing.Bitmap, ByVal Watermark As System.Drawing.Image) As Bitmap
            Dim wmWidth As Integer = Watermark.Width
            Dim wmHeight As Integer = Watermark.Height

            Dim xPosOfWm As Integer = sourceImage.Width - wmWidth - 2
            Dim yPosOfWm As Integer = 2

            Dim bmWatermark As New Bitmap(sourceImage)
            bmWatermark.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution)

            Dim grWatermark As Graphics = Graphics.FromImage(bmWatermark)

            Dim imageAttributes As New ImageAttributes
            Dim colorMap As New ColorMap

            colorMap.OldColor = Color.FromArgb(255, 0, 255, 0)
            colorMap.NewColor = Color.FromArgb(0, 0, 0, 0)
            Dim remapTable As ColorMap() = {colorMap}

            imageAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap)

            Dim colorMatrixElements As Single()() = {New Single() {1.0F, 0.0F, 0.0F, 0.0F, 0.0F}, New Single() {0.0F, 1.0F, 0.0F, 0.0F, 0.0F}, New Single() {0.0F, 0.0F, 1.0F, 0.0F, 0.0F}, New Single() {0.0F, 0.0F, 0.0F, 0.3F, 0.0F}, New Single() {0.0F, 0.0F, 0.0F, 0.0F, 1.0F}}

            Dim wmColorMatrix As New ColorMatrix(colorMatrixElements)

            imageAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap)

            grWatermark.DrawImage(Watermark, New Rectangle(xPosOfWm, yPosOfWm, wmWidth, wmHeight), 0, 0, wmWidth, wmHeight, GraphicsUnit.Pixel, imageAttributes)

            Return bmWatermark

        End Function 'DrawWatermark

        Public Function GetContentType(ByVal iFormat As ImageFormat) As String
            Dim contentType As String

            If iFormat.Guid.Equals(ImageFormat.Jpeg.Guid) Then
                contentType = "image/jpeg"
            ElseIf iFormat.Guid.Equals(ImageFormat.Gif.Guid) Then
                contentType = "image/gif"
            ElseIf iFormat.Guid.Equals(ImageFormat.Png.Guid) Then
                contentType = "image/png"
            ElseIf iFormat.Guid.Equals(ImageFormat.Tiff.Guid) Then
                contentType = "image/tiff"
            ElseIf iFormat.Guid.Equals(ImageFormat.Bmp.Guid) Then
                contentType = "image/x-ms-bmp"
            ElseIf iFormat.Guid.Equals(ImageFormat.Wmf.Guid) Then
                contentType = "image/x-ms-wmf"
            ElseIf iFormat.Guid.Equals(ImageFormat.Emf.Guid) Then
                contentType = "image/x-emf"
            Else
                contentType = "image/jpeg"
            End If

            Return contentType

        End Function 'GetContentType

        Public Function FlipImage(ByVal image As System.Drawing.Bitmap, ByVal FlipType As RotateFlipType) As Bitmap
            image.RotateFlip(FlipType)
            Return image
        End Function

        Public Sub ToggleColorIndex(ByVal image As Bitmap, ByVal ColorIndex As Integer) 'As Bitmap
            ' Toggle color index, i.e., switch color images to black and white.
            ' Provided by Jan Emil Christiansen, modified by Tam for more features
            Dim x As Integer
            Try
                For x = 0 To image.Width - 1
                    Dim y As Integer
                    For y = 0 To image.Height - 1
                        Dim c As Color = image.GetPixel(x, y)
                        ' For better performance save some actions

                        Dim value As Integer = CInt((CInt(c.R) + CInt(c.G) + CInt(c.B)) / 3)

                        ' more white scale
                        If value > ColorIndex Then
                            value = 255
                        End If

                        'If (Not value = c.R) AndAlso (Not value = c.G) AndAlso (Not value = c.B) Then
                        image.SetPixel(x, y, Color.FromArgb(value, value, value))
                        'End If
                    Next y
                Next x
            Catch ex As Exception
            End Try

        End Sub      'ToggleColorIndex

    End Module
End Namespace
