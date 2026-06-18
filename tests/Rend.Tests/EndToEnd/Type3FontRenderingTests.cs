using System.Collections.Generic;
using System.Text;
using SkiaSharp;
using Xunit;

namespace Rend.Tests.EndToEnd
{
    /// <summary>
    /// Verifies the PDF reader renders Type3 fonts (ISO 32000-1 §9.6.5): glyphs defined as
    /// /CharProcs content streams, drawn through the /FontMatrix and the text rendering matrix,
    /// taking their colour from the text graphics state (d1 glyphs) and advancing by /Widths.
    /// </summary>
    public class Type3FontRenderingTests
    {
        [Fact]
        public void SelfReferentialType3Font_DoesNotCrash_RecursionGuarded()
        {
            // A Type3 glyph whose CharProc shows itself in the same font would recurse forever
            // without a depth guard — an uncatchable StackOverflow that kills the process. The
            // renderer must bound the recursion and still return a bitmap.
            byte[] pdf = BuildSelfReferentialType3Pdf();
            using SKBitmap bitmap = PdfToImage.RenderPageToBitmap(pdf, 0, 72f);
            Assert.Equal(100, bitmap.Width);
            Assert.Equal(100, bitmap.Height);
        }

        [Fact]
        public void Type3Glyphs_RenderAsContentStreams_AtTextPositionWithAdvance()
        {
            byte[] pdf = BuildType3Pdf();
            using SKBitmap bitmap = PdfToImage.RenderPageToBitmap(pdf, 0, 72f);

            Assert.Equal(200, bitmap.Width);
            Assert.Equal(200, bitmap.Height);

            // Each 'A' glyph fills a 100×100 glyph-space square. FontMatrix 0.01 → 1×1 text unit;
            // font size 50 → 50×50 user units. First glyph origin (30,30) → user [30,80]², the
            // advance (100 × 0.01 × 50 = 50) puts the second at user [80,130]². With the page's
            // y-flip both map to device rows [120,170].
            AssertBlue(bitmap, 55, 145, "first Type3 glyph");
            AssertBlue(bitmap, 105, 145, "second Type3 glyph (after advance)");

            // Past the second glyph (user x>130 → device x>130) and the corner are background.
            AssertWhite(bitmap, 150, 145, "past last glyph");
            AssertWhite(bitmap, 10, 10, "background");
        }

        // Minimal PDF (correct xref) with a Type3 font: one glyph /square (code 65) whose CharProc
        // is a d1 shape — a filled 100×100 glyph-space rectangle — shown twice as "AA" in blue.
        private static byte[] BuildType3Pdf()
        {
            string pageContent = "0 0 1 rg BT /F1 50 Tf 1 0 0 1 30 30 Tm (AA) Tj ET";
            string glyphProc = "100 0 0 0 100 100 d1\n0 0 100 100 re f";

            var bodies = new List<string>
            {
                "", // object 0: free-list head
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 200 200] "
                    + "/Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>",
                $"<< /Length {pageContent.Length} >>\nstream\n{pageContent}\nendstream",
                "<< /Type /Font /Subtype /Type3 /FontBBox [0 0 100 100] "
                    + "/FontMatrix [0.01 0 0 0.01 0 0] /CharProcs 6 0 R /Encoding 7 0 R "
                    + "/FirstChar 65 /LastChar 65 /Widths [100] >>",
                "<< /square 8 0 R >>",
                "<< /Type /Encoding /Differences [65 /square] >>",
                $"<< /Length {glyphProc.Length} >>\nstream\n{glyphProc}\nendstream",
            };

            return AssemblePdf(bodies);
        }

        // A Type3 font whose single glyph /a (code 97) shows itself in the same font (its
        // /Resources references the font), so naive rendering recurses without bound.
        private static byte[] BuildSelfReferentialType3Pdf()
        {
            string pageContent = "BT /F1 20 Tf 1 0 0 1 10 50 Tm (a) Tj ET";
            string glyphProc = "100 0 0 0 100 100 d1\nBT /F1 20 Tf (a) Tj ET";

            var bodies = new List<string>
            {
                "",
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 100 100] "
                    + "/Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>",
                $"<< /Length {pageContent.Length} >>\nstream\n{pageContent}\nendstream",
                "<< /Type /Font /Subtype /Type3 /FontBBox [0 0 100 100] "
                    + "/FontMatrix [0.01 0 0 0.01 0 0] /CharProcs 6 0 R /Encoding 7 0 R "
                    + "/FirstChar 97 /LastChar 97 /Widths [100] "
                    + "/Resources << /Font << /F1 5 0 R >> >> >>",
                "<< /a 8 0 R >>",
                "<< /Type /Encoding /Differences [97 /a] >>",
                $"<< /Length {glyphProc.Length} >>\nstream\n{glyphProc}\nendstream",
            };

            return AssemblePdf(bodies);
        }

        // Assembles a single-section PDF (objects 1..N) with a correct cross-reference table.
        private static byte[] AssemblePdf(List<string> bodies)
        {
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

        private static void AssertBlue(SKBitmap bitmap, int x, int y, string where)
        {
            SKColor pixel = bitmap.GetPixel(x, y);
            Assert.True(pixel.Blue > 200 && pixel.Red < 80 && pixel.Green < 80,
                $"expected blue at {where} ({x},{y}) but got R={pixel.Red} G={pixel.Green} B={pixel.Blue}");
        }

        private static void AssertWhite(SKBitmap bitmap, int x, int y, string where)
        {
            SKColor pixel = bitmap.GetPixel(x, y);
            Assert.True(pixel.Red > 220 && pixel.Green > 220 && pixel.Blue > 220,
                $"expected white at {where} ({x},{y}) but got R={pixel.Red} G={pixel.Green} B={pixel.Blue}");
        }
    }
}
