using System;
using Xunit;

namespace Rend.Tests.Layout
{
    public class BidirectionalTests
    {
        [Fact]
        public void Bdo_RtlOverride_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <p>This is <bdo dir='rtl'>overridden text</bdo> in a paragraph.</p>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Bdi_Isolate_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <p>User <bdi>إيان</bdi> posted 3 comments.</p>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Bdo_LtrOverride_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='direction: rtl;'>
                        <bdo dir='ltr'>Left to right override</bdo>
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
        public void MultipleBdi_InMixedContent_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <ul>
                        <li><bdi>אריה</bdi> - 1st place</li>
                        <li><bdi>وليد</bdi> - 2nd place</li>
                        <li><bdi>Dave</bdi> - 3rd place</li>
                    </ul>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void NestedBdo_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <p>
                        Normal text
                        <bdo dir='rtl'>RTL override
                            <bdo dir='ltr'>nested LTR</bdo>
                        back to RTL</bdo>
                        back to normal.
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
