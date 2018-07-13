<%@ Control language="vb" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Gallery.WebControls.Slideshow" Codebehind="ControlSlideshow.ascx.vb" %>
          
<script type="text/javascript" language="javascript">

    function runSlideShow(startIndex, slideInterval, fadeInterval) {
        if (!startIndex || startIndex < 0) startIndex = 0;
        if (!slideInterval || slideInterval < 0) slideInterval = 4000;
        if (!fadeInterval || fadeInterval < 0) fadeInterval = 500;

        var Pics = new Array;
        Pics = jQuery('td[id$=celPicture]').data("pics");
        var j = startIndex;
        var p = Pics.length
        var paused = false;
        var slideTimerId = null;
        var img = jQuery('img[id$=imgSlide]');
        var titleSpan = jQuery('td > span.Gallery_HeaderText');
        var captionBox = jQuery('#CaptionBox');
        var direction = 'forward';
        var preLoad = new Array;

        for (i = 0;  i < p; i++) {
            preLoad[i] = new Image;
            var src = Pics[i].src;
            //alert('i: ' + i + ' -- src: ' + src);
            preLoad[i].src = src;
        }

        function updateMetadata() {
            var picData = Pics[j];     
            var title = picData.title.replace("^", "'");
            document.title = baseTitle + " > " + title;
            titleSpan.text(title);
            img.attr('alt', title);
            captionBox.text(picData.desc.replace("^", "'"));
        }

        function nextSlide() {
            if (!direction || direction == 'forward' || direction != 'reverse') {
                j += 1;
                if (j > (p - 1)) j = 0;
            } else {
                j -= 1;
                if (j < 0) j = p - 1;
            }

            img.fadeOut(fadeInterval,
                      function () {
                          img.attr('src', preLoad[j].src);
                          updateMetadata();
                          img.fadeIn(fadeInterval, finishAnimation);
                      });
        }

        function finishAnimation() {
            
        }

        function pauseSlideShow () {
            paused = true;
            clearInterval(slideTimerId);
        }

        function resumeSlideShow () {
            paused = false;
            nextSlide();
            slideTimerId = setInterval(nextSlide, slideInterval)
        }
        
        jQuery('.Gallery_Picture').hover(pauseSlideShow, resumeSlideShow);
        img.attr('src', preLoad[j].src);
        updateMetadata();
        img.fadeIn(fadeInterval).delay(slideInterval);
        resumeSlideShow();
    }    			
</script>

<table class="Gallery_SlideShow">
  <tr>
     <td class="Gallery_Description" id="CaptionBox"></td>
  </tr>
  <tr>
	 <td>
		<table class="Gallery_Picture">
			<tr>
				<td class="Gallery_PictureTL"></td>
				<td class="Gallery_PictureTC"></td>
				<td class="Gallery_PictureTR"></td>
			</tr>
			<tr>
				<td class="Gallery_PictureML"></td>
				<td class="Gallery_PictureMC" id="celPicture" runat="server">
					<div><img id="imgSlide" src="" alt="" /></div>
				</td>
                <td class="Gallery_PictureMR"></td>
			</tr>
			<tr>
				<td class="Gallery_PictureBL"></td>
				<td class="Gallery_PictureBC"></td>
				<td class="Gallery_PictureBR"></td>
			</tr>
		</table>
	</td>
  </tr>
</table>
<asp:Label id="ErrorMessage" runat="server" CssClass="NormalRed" Visible="False"></asp:Label>
