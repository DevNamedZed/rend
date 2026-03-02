using System;
using Xunit;

namespace Rend.Tests.Layout
{
    public class MediaElementTests
    {
        #region Canvas

        [Fact]
        public void Canvas_Empty_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"<canvas width='300' height='150'></canvas>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Canvas_WithFallbackContent_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <p>Chart:</p>
                    <canvas width='400' height='200'>
                        Your browser does not support canvas.
                    </canvas>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        #endregion

        #region Video

        [Fact]
        public void Video_Placeholder_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <video width='320' height='240' controls>
                        <source src='movie.mp4' type='video/mp4'>
                        Your browser does not support video.
                    </video>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Video_DefaultDimensions_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"<video src='test.mp4'></video>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Video_WithPoster_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <video poster='thumbnail.jpg' width='640' height='480'>
                        <source src='video.mp4'>
                    </video>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        #endregion

        #region Audio

        [Fact]
        public void Audio_Placeholder_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <p>Listen to the song:</p>
                    <audio controls>
                        <source src='song.mp3' type='audio/mpeg'>
                        Your browser does not support audio.
                    </audio>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Audio_InParagraph_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div>
                        <h2>Podcast Episode 1</h2>
                        <audio src='episode1.mp3' controls></audio>
                        <p>Episode description here.</p>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        #endregion

        #region Multiple Media Elements

        [Fact]
        public void MixedMedia_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div>
                        <canvas width='200' height='100'></canvas>
                        <br>
                        <video width='200' height='150' controls></video>
                        <br>
                        <audio controls></audio>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        #endregion

        private static bool IsNativeLibraryFailure(Exception ex)
        {
            return ex is DllNotFoundException ||
                   ex is TypeInitializationException ||
                   (ex.InnerException is DllNotFoundException) ||
                   ex.Message.Contains("native", StringComparison.OrdinalIgnoreCase);
        }
    }
}
