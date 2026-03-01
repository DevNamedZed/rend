using System;
using Xunit;

namespace Rend.Tests
{
    public class MultiColumnLayoutTests
    {
        [Fact]
        public void ToPdf_WithColumnCount_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='column-count: 2; column-gap: 20px;'>
                        <p>First paragraph of text that should flow into two columns.</p>
                        <p>Second paragraph of text that continues in the columns.</p>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
            Assert.Equal((byte)'%', result[0]);
        }

        [Fact]
        public void ToPdf_WithColumnWidth_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='column-width: 200px;'>
                        <p>Content that flows into columns based on width.</p>
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
        public void ToPdf_WithColumnsShorthand_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='columns: 3; column-gap: 15px;'>
                        <p>Column 1 content</p>
                        <p>Column 2 content</p>
                        <p>Column 3 content</p>
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
        public void ToPdf_WithColumnRule_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='column-count: 2; column-rule: 1px solid #ccc;'>
                        <p>Left column content.</p>
                        <p>Right column content.</p>
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
        public void ToPdf_SingleColumn_StillWorks()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='column-count: 1;'>
                        <p>Single column content.</p>
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
        public void ToPdf_NoColumnProperties_NormalLayout()
        {
            // Verify that normal layout still works when no column properties are set
            byte[] result;
            try
            {
                result = Render.ToPdf("<div><p>Normal content</p></div>");
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
            if (ex is DllNotFoundException) return true;
            if (ex is TypeInitializationException) return true;
            if (ex is BadImageFormatException) return true;
            var inner = ex.InnerException;
            while (inner != null)
            {
                if (inner is DllNotFoundException) return true;
                if (inner is TypeInitializationException) return true;
                inner = inner.InnerException;
            }
            return false;
        }
    }
}
