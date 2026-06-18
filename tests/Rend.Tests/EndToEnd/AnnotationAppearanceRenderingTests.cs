using System.Collections.Generic;
using System.Text;
using SkiaSharp;
using Xunit;

namespace Rend.Tests.EndToEnd
{
    /// <summary>
    /// Verifies the PDF reader paints annotation appearance streams (/AP /N) per ISO 32000-1
    /// §12.5.5: the appearance Form XObject is placed into the annotation's /Rect and composited
    /// over the page content, with the Hidden flag suppressing it.
    /// </summary>
    public class AnnotationAppearanceRenderingTests
    {
        [Fact]
        public void SquareAnnotationAppearance_RendersAtRect_OverPageContent()
        {
            byte[] pdf = BuildAnnotatedPdf(annotationFlags: 0);
            using SKBitmap bitmap = PdfToImage.RenderPageToBitmap(pdf, 0, 72f);

            Assert.Equal(200, bitmap.Width);
            Assert.Equal(200, bitmap.Height);

            // Page content: red rect at user (10,10)-(60,60) → device (10,140)-(60,190).
            AssertColor(bitmap, 35, 165, expectRed: true, where: "page content red rect");

            // Annotation appearance: blue rect at user (100,100)-(180,160) → device (100,40)-(180,100).
            AssertColor(bitmap, 140, 70, expectBlue: true, where: "annotation appearance");

            // Top-left corner is neither → page background white.
            AssertWhite(bitmap, 8, 8, "background");
        }

        [Fact]
        public void HiddenAnnotation_IsNotRendered()
        {
            const int hidden = 1 << 1;
            byte[] pdf = BuildAnnotatedPdf(annotationFlags: hidden);
            using SKBitmap bitmap = PdfToImage.RenderPageToBitmap(pdf, 0, 72f);

            // The blue appearance must be absent; the spot stays white.
            AssertWhite(bitmap, 140, 70, "hidden annotation spot");

            // Page content is unaffected.
            AssertColor(bitmap, 35, 165, expectRed: true, where: "page content red rect");
        }

        [Theory]
        [InlineData("q 0 0 10 10 re W n ")]   // clip leaked inside an unclosed q
        [InlineData("0 0 10 10 re W n ")]      // clip applied at the TOP level (no q at all)
        public void LeakedClipInPageContent_DoesNotClipAnnotations(string contentPrefix)
        {
            // The content stream clips to a tiny region and never restores it. That leaked clip must
            // not carry into the annotation pass — the annotation appearance lies outside the clip
            // and must still render, whether the clip was inside a q or at the top level.
            byte[] pdf = BuildAnnotatedPdf(annotationFlags: 0, contentPrefix: contentPrefix);
            using SKBitmap bitmap = PdfToImage.RenderPageToBitmap(pdf, 0, 72f);

            AssertColor(bitmap, 140, 70, expectBlue: true, where: "annotation despite leaked content clip");
        }

        // Builds a minimal, valid PDF with a correct xref table: a 200×200 page that draws a red
        // square in its content and carries one /Square annotation whose /AP /N Form XObject draws
        // a blue square. BBox (80×60) matches the Rect size, so the placement matrix is a pure
        // translation — the blue square lands exactly on the annotation rectangle.
        private static byte[] BuildAnnotatedPdf(int annotationFlags, string contentPrefix = "")
        {
            string pageContent = contentPrefix + "1 0 0 rg 10 10 50 50 re f";
            string appearanceContent = "0 0 1 rg 0 0 80 60 re f";
            string flagsEntry = annotationFlags != 0 ? $" /F {annotationFlags}" : "";

            var bodies = new List<string>
            {
                "", // object 0 is the free-list head; bodies are 1-based
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 200 200] "
                    + "/Contents 4 0 R /Resources << >> /Annots [5 0 R] >>",
                $"<< /Length {pageContent.Length} >>\nstream\n{pageContent}\nendstream",
                $"<< /Type /Annot /Subtype /Square /Rect [100 100 180 160]{flagsEntry} /AP << /N 6 0 R >> >>",
                $"<< /Type /XObject /Subtype /Form /BBox [0 0 80 60] /Length {appearanceContent.Length} >>"
                    + $"\nstream\n{appearanceContent}\nendstream",
            };

            var builder = new StringBuilder();
            builder.Append("%PDF-1.7\n");

            var offsets = new int[bodies.Count];
            for (int objectNumber = 1; objectNumber < bodies.Count; objectNumber++)
            {
                offsets[objectNumber] = builder.Length; // ASCII → char count == byte offset
                builder.Append($"{objectNumber} 0 obj\n{bodies[objectNumber]}\nendobj\n");
            }

            int xrefOffset = builder.Length;
            builder.Append("xref\n");
            builder.Append($"0 {bodies.Count}\n");
            builder.Append("0000000000 65535 f \n");
            for (int objectNumber = 1; objectNumber < bodies.Count; objectNumber++)
            {
                builder.Append(offsets[objectNumber].ToString("D10") + " 00000 n \n");
            }

            builder.Append("trailer\n");
            builder.Append($"<< /Size {bodies.Count} /Root 1 0 R >>\n");
            builder.Append("startxref\n");
            builder.Append(xrefOffset + "\n");
            builder.Append("%%EOF");

            return Encoding.ASCII.GetBytes(builder.ToString());
        }

        private static void AssertColor(SKBitmap bitmap, int x, int y, bool expectRed = false,
            bool expectBlue = false, string where = "")
        {
            SKColor pixel = bitmap.GetPixel(x, y);
            if (expectRed)
            {
                Assert.True(pixel.Red > 200 && pixel.Green < 80 && pixel.Blue < 80,
                    $"expected red at {where} ({x},{y}) but got R={pixel.Red} G={pixel.Green} B={pixel.Blue}");
            }
            if (expectBlue)
            {
                Assert.True(pixel.Blue > 200 && pixel.Red < 80 && pixel.Green < 80,
                    $"expected blue at {where} ({x},{y}) but got R={pixel.Red} G={pixel.Green} B={pixel.Blue}");
            }
        }

        private static void AssertWhite(SKBitmap bitmap, int x, int y, string where)
        {
            SKColor pixel = bitmap.GetPixel(x, y);
            Assert.True(pixel.Red > 220 && pixel.Green > 220 && pixel.Blue > 220,
                $"expected white at {where} ({x},{y}) but got R={pixel.Red} G={pixel.Green} B={pixel.Blue}");
        }
    }
}
