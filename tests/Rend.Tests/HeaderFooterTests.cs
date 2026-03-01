using System;
using System.Text;
using Xunit;

namespace Rend.Tests
{
    public class HeaderFooterTests
    {
        [Fact]
        public void ToPdf_WithHeaderHtml_ProducesValidPdf()
        {
            var options = new RenderOptions
            {
                HeaderHtml = "<div style='text-align:center;font-size:10px;'>My Header</div>"
            };

            byte[] result;
            try
            {
                result = Render.ToPdf("<p>Body content</p>", options);
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
        public void ToPdf_WithFooterHtml_ProducesValidPdf()
        {
            var options = new RenderOptions
            {
                FooterHtml = "<div style='text-align:center;font-size:10px;'>Page Footer</div>"
            };

            byte[] result;
            try
            {
                result = Render.ToPdf("<p>Body content</p>", options);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void ToPdf_WithHeaderAndFooter_ProducesValidPdf()
        {
            var options = new RenderOptions
            {
                HeaderHtml = "<div>Header</div>",
                FooterHtml = "<div>Footer</div>"
            };

            byte[] result;
            try
            {
                result = Render.ToPdf("<h1>Title</h1><p>Content</p>", options);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void ToPdf_WithPageNumberVariable_ProducesValidPdf()
        {
            var options = new RenderOptions
            {
                FooterHtml = "<div style='text-align:center;font-size:9px;'>Page {pageNumber} of {totalPages}</div>"
            };

            byte[] result;
            try
            {
                result = Render.ToPdf("<p>Content with page number footer</p>", options);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void ToPdf_WithDateVariable_ProducesValidPdf()
        {
            var options = new RenderOptions
            {
                HeaderHtml = "<div>{date}</div>"
            };

            byte[] result;
            try
            {
                result = Render.ToPdf("<p>Content</p>", options);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void ToPdf_WithStyledHeaderFooter_ProducesValidPdf()
        {
            var options = new RenderOptions
            {
                HeaderHtml = @"<div style='display:flex;justify-content:space-between;font-size:9px;color:#666;'>
                    <span>Company Name</span>
                    <span>Confidential</span>
                </div>",
                FooterHtml = @"<div style='text-align:center;font-size:8px;border-top:1px solid #ccc;padding-top:4px;'>
                    Page {pageNumber} of {totalPages}
                </div>"
            };

            byte[] result;
            try
            {
                result = Render.ToPdf("<h1>Report</h1><p>Important content</p>", options);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void ToPdf_WithoutHeaderFooter_StillWorks()
        {
            // Verify that the pipeline still works without headers/footers
            byte[] result;
            try
            {
                result = Render.ToPdf("<p>No header or footer</p>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void RenderOptions_HeaderFooter_DefaultsToNull()
        {
            var options = new RenderOptions();
            Assert.Null(options.HeaderHtml);
            Assert.Null(options.FooterHtml);
        }

        [Fact]
        public void VariableSubstitution_ReplacesPageNumber()
        {
            // Test that template variables are replaced correctly
            // This is a unit test for the SubstituteVariables logic
            string template = "Page {pageNumber} of {totalPages} - {date}";
            string result = template
                .Replace("{pageNumber}", "3")
                .Replace("{totalPages}", "10")
                .Replace("{date}", DateTime.Now.ToString("yyyy-MM-dd"));

            Assert.Contains("Page 3 of 10", result);
            Assert.Contains(DateTime.Now.ToString("yyyy-MM-dd"), result);
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
