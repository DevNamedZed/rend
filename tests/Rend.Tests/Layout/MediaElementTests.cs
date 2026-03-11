using Xunit;

namespace Rend.Tests.Layout
{
    public class MediaElementTests
    {
        #region Canvas

        [Fact]
        public void Canvas_Empty_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"<canvas width='300' height='150'></canvas>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Canvas_WithFallbackContent_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <p>Chart:</p>
                <canvas width='400' height='200'>
                    Your browser does not support canvas.
                </canvas>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        #endregion

        #region Video

        [Fact]
        public void Video_Placeholder_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <video width='320' height='240' controls>
                    <source src='movie.mp4' type='video/mp4'>
                    Your browser does not support video.
                </video>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Video_DefaultDimensions_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"<video src='test.mp4'></video>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Video_WithPoster_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <video poster='thumbnail.jpg' width='640' height='480'>
                    <source src='video.mp4'>
                </video>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        #endregion

        #region Audio

        [Fact]
        public void Audio_Placeholder_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <p>Listen to the song:</p>
                <audio controls>
                    <source src='song.mp3' type='audio/mpeg'>
                    Your browser does not support audio.
                </audio>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Audio_InParagraph_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div>
                    <h2>Podcast Episode 1</h2>
                    <audio src='episode1.mp3' controls></audio>
                    <p>Episode description here.</p>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        #endregion

        #region Multiple Media Elements

        [Fact]
        public void MixedMedia_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div>
                    <canvas width='200' height='100'></canvas>
                    <br>
                    <video width='200' height='150' controls></video>
                    <br>
                    <audio controls></audio>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        #endregion

    }
}
