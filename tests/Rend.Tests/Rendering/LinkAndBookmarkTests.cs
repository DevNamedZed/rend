using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Rend.Core.Values;
using Rend.Css;
using Rend.Fonts;
using Rend.Layout;
using Rend.Output.Pdf;
using Rend.Pdf;
using Rend.Rendering;
using Rend.Style;
using Rend.Text;
using Xunit;

namespace Rend.Tests.Rendering
{
    public class LinkAndBookmarkTests
    {
        // ═══════════════════════════════════════════
        // PdfRenderTarget — Link annotations
        // ═══════════════════════════════════════════

        [Fact]
        public void AddLink_CreatesUriAnnotation()
        {
            var options = new PdfRenderOptions
            {
                DocumentOptions = new PdfDocumentOptions { Compression = PdfCompression.None }
            };
            var target = new PdfRenderTarget(options);

            target.BeginPage(595, 842);
            target.AddLink(new RectF(50, 100, 200, 20), "https://example.com");
            target.EndPage();

            string pdf = FinishAsString(target);
            Assert.Contains("/Annots", pdf);
            Assert.Contains("/URI", pdf);
            Assert.Contains("example.com", pdf);
        }

        [Fact]
        public void AddLink_InvalidUri_NoAnnotation()
        {
            var options = new PdfRenderOptions
            {
                DocumentOptions = new PdfDocumentOptions { Compression = PdfCompression.None }
            };
            var target = new PdfRenderTarget(options);

            target.BeginPage(595, 842);
            target.AddLink(new RectF(50, 100, 200, 20), "not a valid uri");
            target.EndPage();

            string pdf = FinishAsString(target);
            Assert.DoesNotContain("/URI", pdf);
        }

        [Fact]
        public void AddLink_EmptyUri_NoAnnotation()
        {
            var options = new PdfRenderOptions
            {
                DocumentOptions = new PdfDocumentOptions { Compression = PdfCompression.None }
            };
            var target = new PdfRenderTarget(options);

            target.BeginPage(595, 842);
            target.AddLink(new RectF(50, 100, 200, 20), "");
            target.EndPage();

            string pdf = FinishAsString(target);
            Assert.DoesNotContain("/URI", pdf);
        }

        [Fact]
        public void AddLink_ConvertsCoordinatesToPdfSpace()
        {
            var options = new PdfRenderOptions
            {
                DocumentOptions = new PdfDocumentOptions { Compression = PdfCompression.None }
            };
            var target = new PdfRenderTarget(options);

            // Page height 842, CSS rect at (50, 100) height 20
            // PDF bottom = 842 - 100 - 20 = 722
            // PDF top = 842 - 100 = 742
            target.BeginPage(595, 842);
            target.AddLink(new RectF(50, 100, 200, 20), "https://example.com");
            target.EndPage();

            string pdf = FinishAsString(target);
            // PDF rect should contain the converted Y coordinates
            Assert.Contains("722", pdf);
            Assert.Contains("742", pdf);
        }

        [Fact]
        public void AddLink_MultipleLinksOnSamePage()
        {
            var options = new PdfRenderOptions
            {
                DocumentOptions = new PdfDocumentOptions { Compression = PdfCompression.None }
            };
            var target = new PdfRenderTarget(options);

            target.BeginPage(595, 842);
            target.AddLink(new RectF(50, 100, 200, 20), "https://example.com");
            target.AddLink(new RectF(50, 200, 200, 20), "https://other.com");
            target.EndPage();

            string pdf = FinishAsString(target);
            Assert.Contains("example.com", pdf);
            Assert.Contains("other.com", pdf);
        }

        // ═══════════════════════════════════════════
        // PdfRenderTarget — Bookmark generation
        // ═══════════════════════════════════════════

        [Fact]
        public void AddBookmark_CreatesOutlineEntry()
        {
            var options = new PdfRenderOptions
            {
                DocumentOptions = new PdfDocumentOptions { Compression = PdfCompression.None }
            };
            var target = new PdfRenderTarget(options);

            target.BeginPage(595, 842);
            target.AddBookmark("Chapter 1", 1, 50);
            target.EndPage();

            string pdf = FinishAsString(target);
            Assert.Contains("/Outlines", pdf);
            Assert.Contains("Chapter 1", pdf);
        }

        [Fact]
        public void AddBookmark_MultipleHeadingLevels()
        {
            var options = new PdfRenderOptions
            {
                DocumentOptions = new PdfDocumentOptions { Compression = PdfCompression.None }
            };
            var target = new PdfRenderTarget(options);

            target.BeginPage(595, 842);
            target.AddBookmark("Main Title", 1, 50);
            target.AddBookmark("Section A", 2, 150);
            target.AddBookmark("Section B", 2, 350);
            target.EndPage();

            string pdf = FinishAsString(target);
            Assert.Contains("Main Title", pdf);
            Assert.Contains("Section A", pdf);
            Assert.Contains("Section B", pdf);
        }

        [Fact]
        public void AddBookmark_ConvertsYToPdfSpace()
        {
            var options = new PdfRenderOptions
            {
                DocumentOptions = new PdfDocumentOptions { Compression = PdfCompression.None }
            };
            var target = new PdfRenderTarget(options);

            // Page height 842, CSS Y = 100 → PDF Y = 742
            target.BeginPage(595, 842);
            target.AddBookmark("Test", 1, 100);
            target.EndPage();

            string pdf = FinishAsString(target);
            Assert.Contains("742", pdf);
        }

        [Fact]
        public void AddBookmark_EmptyTitle_Skipped()
        {
            var options = new PdfRenderOptions
            {
                DocumentOptions = new PdfDocumentOptions { Compression = PdfCompression.None }
            };
            var target = new PdfRenderTarget(options);

            target.BeginPage(595, 842);
            target.AddBookmark("", 1, 50);
            target.EndPage();

            string pdf = FinishAsString(target);
            Assert.DoesNotContain("/Outlines", pdf);
        }

        [Fact]
        public void AddBookmark_InvalidLevel_Skipped()
        {
            var options = new PdfRenderOptions
            {
                DocumentOptions = new PdfDocumentOptions { Compression = PdfCompression.None }
            };
            var target = new PdfRenderTarget(options);

            target.BeginPage(595, 842);
            target.AddBookmark("Test", 0, 50);
            target.AddBookmark("Test2", 7, 100);
            target.EndPage();

            string pdf = FinishAsString(target);
            Assert.DoesNotContain("/Outlines", pdf);
        }

        [Fact]
        public void AddBookmark_AcrossPages()
        {
            var options = new PdfRenderOptions
            {
                DocumentOptions = new PdfDocumentOptions { Compression = PdfCompression.None }
            };
            var target = new PdfRenderTarget(options);

            target.BeginPage(595, 842);
            target.AddBookmark("Page 1 Heading", 1, 50);
            target.EndPage();

            target.BeginPage(595, 842);
            target.AddBookmark("Page 2 Heading", 1, 50);
            target.EndPage();

            string pdf = FinishAsString(target);
            Assert.Contains("Page 1 Heading", pdf);
            Assert.Contains("Page 2 Heading", pdf);
        }

        // ═══════════════════════════════════════════
        // Recording render target for Painter tests
        // ═══════════════════════════════════════════

        private sealed class RecordingTarget : IRenderTarget
        {
            public List<(RectF Rect, string Uri)> Links { get; } = new List<(RectF, string)>();
            public List<(string Title, int Level, float Y)> Bookmarks { get; } = new List<(string, int, float)>();

            public void BeginPage(float width, float height) { }
            public void EndPage() { }
            public void Save() { }
            public void Restore() { }
            public void SetTransform(Matrix3x2 transform) { }
            public void SetOpacity(float opacity) { }
            public void SetBlendMode(Rend.Css.CssMixBlendMode blendMode) { }
            public void SetImageRendering(Rend.Css.CssImageRendering rendering) { }
            public void SetMaskBlur(float sigma) { }
            public void PushClipRect(RectF rect) { }
            public void PushClipPath(PathData path) { }
            public void PopClip() { }
            public void FillRect(RectF rect, BrushInfo brush) { }
            public void StrokeRect(RectF rect, PenInfo pen) { }
            public void FillPath(PathData path, BrushInfo brush) { }
            public void StrokePath(PathData path, PenInfo pen) { }
            public void DrawImage(ImageData image, RectF destRect) { }
            public void DrawText(string text, float x, float y, TextStyle style) { }
            public void DrawGlyphs(ShapedTextRun run, float x, float y, CssColor color, FontDescriptor font) { }
            public void Finish(Stream output) { }

            public void AddLink(RectF rect, string uri)
            {
                Links.Add((rect, uri));
            }

            public void AddBookmark(string title, int level, float yPosition)
            {
                Bookmarks.Add((title, level, yPosition));
            }
        }

        // ═══════════════════════════════════════════
        // End-to-end pipeline tests
        // ═══════════════════════════════════════════

        [Fact]
        public void ToPdf_WithLink_ContainsAnnotation()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf("<a href=\"https://example.com\">Click here</a>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            string pdf = Encoding.ASCII.GetString(result);
            Assert.Contains("/URI", pdf);
            Assert.Contains("example.com", pdf);
        }

        [Fact]
        public void ToPdf_WithHeadings_ContainsOutlines()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf("<h1>Title</h1><h2>Section</h2><p>Content</p>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            string pdf = Encoding.ASCII.GetString(result);
            Assert.Contains("/Outlines", pdf);
        }

        [Fact]
        public void ToPdf_DisabledLinks_NoAnnotation()
        {
            var options = new RenderOptions { GenerateLinks = false };

            byte[] result;
            try
            {
                result = Render.ToPdf("<a href=\"https://example.com\">Click</a>", options);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            string pdf = Encoding.ASCII.GetString(result);
            Assert.DoesNotContain("/URI", pdf);
        }

        [Fact]
        public void ToPdf_DisabledBookmarks_NoOutlines()
        {
            var options = new RenderOptions { GenerateBookmarks = false };

            byte[] result;
            try
            {
                result = Render.ToPdf("<h1>Title</h1>", options);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            string pdf = Encoding.ASCII.GetString(result);
            Assert.DoesNotContain("/Outlines", pdf);
        }

        [Fact]
        public void ToPdf_PointerEventsNone_NoAnnotation()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf("<a href=\"https://example.com\" style=\"pointer-events: none;\">No Link</a>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            string pdf = Encoding.ASCII.GetString(result);
            Assert.DoesNotContain("/URI", pdf);
        }

        [Fact]
        public void ToPdf_PointerEventsAuto_HasAnnotation()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf("<a href=\"https://example.com\" style=\"pointer-events: auto;\">Has Link</a>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            string pdf = Encoding.ASCII.GetString(result);
            Assert.Contains("/URI", pdf);
        }

        // ═══════════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════════

        private static string FinishAsString(PdfRenderTarget target)
        {
            using var ms = new MemoryStream();
            target.Finish(ms);
            return Encoding.ASCII.GetString(ms.ToArray());
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

            string msg = ex.Message ?? "";
            if (msg.Contains("libHarfBuzz", StringComparison.OrdinalIgnoreCase)) return true;
            if (msg.Contains("libSkiaSharp", StringComparison.OrdinalIgnoreCase)) return true;
            if (msg.Contains("HarfBuzzSharp", StringComparison.OrdinalIgnoreCase)) return true;
            if (msg.Contains("SkiaSharp", StringComparison.OrdinalIgnoreCase)) return true;

            return false;
        }
    }
}
