using System.Text;
using Rend.Core.Values;
using Rend.Pdf;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class MarkupAnnotationTests
    {
        [Fact]
        public void Highlight_ContainsHighlightSubtype()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            page.AddHighlight(new RectF(50, 700, 200, 15));

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/Subtype /Highlight", text);
        }

        [Fact]
        public void Highlight_ContainsQuadPoints()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            page.AddHighlight(new RectF(50, 700, 200, 15));

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/QuadPoints", text);
        }

        [Fact]
        public void Highlight_DefaultYellowColor()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            page.AddHighlight(new RectF(50, 700, 200, 15));

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/C", text);
            // Yellow = 1 1 0 in float RGB
            Assert.Contains("1 1 0", text);
        }

        [Fact]
        public void Highlight_CustomColor()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            page.AddHighlight(new RectF(50, 700, 200, 15), CssColor.FromRgba(0, 255, 0));

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/Subtype /Highlight", text);
            // Green = 0 1 0 in float RGB
            Assert.Contains("0 1 0", text);
        }

        [Fact]
        public void Underline_ContainsUnderlineSubtype()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            page.AddUnderline(new RectF(50, 700, 200, 15));

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/Subtype /Underline", text);
        }

        [Fact]
        public void StrikeOut_ContainsStrikeOutSubtype()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            page.AddStrikeOut(new RectF(50, 700, 200, 15));

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/Subtype /StrikeOut", text);
        }

        [Fact]
        public void StickyNote_ContainsTextSubtype()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            page.AddStickyNote(new RectF(50, 700, 24, 24), "Review this section");

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/Subtype /Text", text);
        }

        [Fact]
        public void StickyNote_ContainsContents()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            page.AddStickyNote(new RectF(50, 700, 24, 24), "Important note");

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/Contents (Important note)", text);
        }

        [Fact]
        public void StickyNote_DefaultClosed()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            page.AddStickyNote(new RectF(50, 700, 24, 24), "Closed note");

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/Open false", text);
        }

        [Fact]
        public void StickyNote_Open()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            var note = page.AddStickyNote(new RectF(50, 700, 24, 24), "Open note");
            note.IsOpen = true;

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/Open true", text);
        }

        [Fact]
        public void MultipleAnnotationTypes()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            page.AddHighlight(new RectF(50, 700, 200, 15));
            page.AddUnderline(new RectF(50, 680, 200, 15));
            page.AddStrikeOut(new RectF(50, 660, 200, 15));
            page.AddStickyNote(new RectF(270, 700, 24, 24), "Note");

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/Subtype /Highlight", text);
            Assert.Contains("/Subtype /Underline", text);
            Assert.Contains("/Subtype /StrikeOut", text);
            Assert.Contains("/Subtype /Text", text);
        }

        [Fact]
        public void Annotations_ValidPdf()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            page.AddHighlight(new RectF(50, 700, 200, 15));
            page.AddStickyNote(new RectF(300, 700, 24, 24), "Hello");

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.StartsWith("%PDF-1.", text);
            Assert.Contains("/Root", text);
        }

        [Fact]
        public void Annotations_ContainsRect()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            page.AddHighlight(new RectF(50, 700, 200, 15));

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/Rect", text);
        }

        [Fact]
        public void Annotations_ContainsAnnotType()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            page.AddHighlight(new RectF(50, 700, 200, 15));

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/Type /Annot", text);
        }
    }
}
