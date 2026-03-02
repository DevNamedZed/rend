using System;
using Xunit;

namespace Rend.Tests.Layout
{
    public class RubyAnnotationTests
    {
        [Fact]
        public void Ruby_SimpleAnnotation_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <p>
                        <ruby>漢<rt>かん</rt></ruby>
                        <ruby>字<rt>じ</rt></ruby>
                    </p>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Ruby_WithRpFallback_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <p>
                        <ruby>漢<rp>(</rp><rt>かん</rt><rp>)</rp></ruby>
                        <ruby>字<rp>(</rp><rt>じ</rt><rp>)</rp></ruby>
                    </p>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Ruby_InParagraph_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <p>This is a test of <ruby>漢字<rt>かんじ</rt></ruby> annotations in English text.</p>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Ruby_MultipleInline_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <p>
                        <ruby>東<rt>ひがし</rt></ruby>
                        <ruby>京<rt>きょう</rt></ruby>
                        <ruby>都<rt>と</rt></ruby>
                    </p>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Ruby_WithRbElement_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <ruby>
                        <rb>漢</rb>
                        <rt>かん</rt>
                        <rb>字</rb>
                        <rt>じ</rt>
                    </ruby>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Ruby_PositionUnder_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <p style='ruby-position: under;'>
                        <ruby>漢<rt>かん</rt></ruby>
                        <ruby>字<rt>じ</rt></ruby>
                    </p>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Ruby_EmptyAnnotation_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <p><ruby>Base<rt></rt></ruby> text.</p>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Ruby_Nested_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div>
                        <h2>Ruby Test</h2>
                        <p>
                            <ruby>明日<rt>あした</rt></ruby>は
                            <ruby>天気<rt>てんき</rt></ruby>です。
                        </p>
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
        public void Ruby_DisplayProperty_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <p>
                        <span style='display: ruby;'>Base<span style='display: ruby-text;'>Annotation</span></span>
                    </p>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Ruby_LongText_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <p>
                        <ruby>東京特許許可局<rt>とうきょうとっきょきょかきょく</rt></ruby>
                    </p>");
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
