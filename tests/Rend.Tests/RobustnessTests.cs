using System;
using Xunit;

namespace Rend.Tests
{
    /// <summary>
    /// Tests that the renderer gracefully handles CSS properties and HTML patterns
    /// it doesn't fully support, without crashing.
    /// </summary>
    public class RobustnessTests
    {
        [Fact]
        public void Css_AnimationProperties_DoNotCrash()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <style>
                        @keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
                        .animated { animation: spin 2s linear infinite; transition: opacity 0.3s ease; }
                    </style>
                    <div class='animated'>Animated text</div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Css_InteractivePseudoClasses_DoNotCrash()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <style>
                        a:hover { color: red; }
                        button:active { background: blue; }
                        input:focus { border-color: green; }
                        .link:visited { color: purple; }
                    </style>
                    <a href='#'>Link</a>
                    <button>Button</button>
                    <input type='text' value='Input'>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Css_UnknownProperties_DoNotCrash()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <style>
                        div {
                            scroll-snap-type: y mandatory;
                            overscroll-behavior: contain;
                            caret-color: red;
                            touch-action: none;
                            user-select: none;
                        }
                    </style>
                    <div>Content with unknown CSS properties</div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Html_DeeplyNestedElements_DoNotCrash()
        {
            string html = "<div>";
            for (int i = 0; i < 100; i++)
                html += "<div>";
            html += "Deep content";
            for (int i = 0; i < 100; i++)
                html += "</div>";
            html += "</div>";

            byte[] result;
            try
            {
                result = Render.ToPdf(html);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Html_EmptyDocument_DoNotCrash()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf("");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Html_OnlyWhitespace_DoNotCrash()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf("   \n\t\n   ");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Html_MalformedTags_DoNotCrash()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <p>Unclosed paragraph
                    <div>
                        <span>Unclosed span
                        <b>Nested <i>formatting</b></i>
                    </div>
                    <table><tr><td>Cell</table>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Css_VeryLargeFontSize_DoNotCrash()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='font-size: 999px;'>Giant text</div>
                    <div style='font-size: 0.001px;'>Tiny text</div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Css_NegativeValues_DoNotCrash()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='margin: -50px; padding: -10px; width: -100px; height: -50px;'>
                        Negative values
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Css_ComplexMediaDocument_DoNotCrash()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div>
                        <math><mfrac><mn>1</mn><mn>2</mn></mfrac></math>
                        <svg width='50' height='50'><circle cx='25' cy='25' r='20' fill='red'/></svg>
                        <img src='nonexistent.png' alt='Missing'>
                        <iframe srcdoc='<p>Hello</p>'></iframe>
                        <video width='100' height='75'></video>
                        <audio controls></audio>
                        <canvas width='100' height='50'></canvas>
                        <meter value='0.5'></meter>
                        <progress value='50' max='100'></progress>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        private static bool IsNativeLibraryFailure(Exception ex)
        {
            return ex is DllNotFoundException ||
                   ex is TypeInitializationException ||
                   (ex.InnerException is DllNotFoundException) ||
                   ex.Message.Contains("native", StringComparison.OrdinalIgnoreCase);
        }
    }
}
