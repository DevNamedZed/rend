using System.Collections.Generic;
using System.Text;
using SkiaSharp;
using Xunit;

namespace Rend.Tests.EndToEnd
{
    /// <summary>
    /// Verifies a transparency-group Form XObject composites as a unit at the group alpha
    /// (ISO 32000-1 §11.4.7, PDF-R12): two overlapping opaque rects in a group painted at ca=0.5
    /// must show a uniform 50% tint — the overlap equals the single-covered area, not a darker
    /// double-composited region (which is what per-element alpha would produce).
    /// </summary>
    public class TransparencyGroupPdfTests
    {
        [Fact]
        public void TransparencyGroup_CompositesAtGroupAlpha_NotPerElement()
        {
            byte[] pdf = BuildGroupOpacityPdf();
            using SKBitmap bitmap = PdfToImage.RenderPageToBitmap(pdf, 0, 72f);

            // rect1 user (40,40)-(120,120) → device (40,80)-(120,160); rect2 user (80,80)-(160,160)
            // → device (80,40)-(160,120); overlap device (80,80)-(120,120).
            SKColor singleCovered = bitmap.GetPixel(55, 150); // rect1 only
            SKColor overlap = bitmap.GetPixel(100, 100);      // both rects

            // Both must be ~50% red over white (uniform group composite). Per-element alpha would
            // make the overlap markedly darker (green/blue ~64 vs ~128).
            Assert.True(singleCovered.Red > 230 && singleCovered.Green > 100 && singleCovered.Green < 170,
                $"single-covered should be ~50% red, got R={singleCovered.Red} G={singleCovered.Green} B={singleCovered.Blue}");
            Assert.True(overlap.Red > 230 && overlap.Green > 100 && overlap.Green < 170,
                $"overlap should match single (uniform group alpha), got R={overlap.Red} G={overlap.Green} B={overlap.Blue}");
            Assert.True(System.Math.Abs(overlap.Green - singleCovered.Green) < 30,
                $"overlap must equal single-covered (group composited once): single G={singleCovered.Green} overlap G={overlap.Green}");
        }

        private static byte[] BuildGroupOpacityPdf()
        {
            string pageContent = "/GS gs /Fm Do";
            string formContent = "1 0 0 rg 40 40 80 80 re f 80 80 80 80 re f";

            var bodies = new List<string>
            {
                "", // object 0
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 200 200] /Contents 4 0 R "
                    + "/Resources << /ExtGState << /GS 5 0 R >> /XObject << /Fm 6 0 R >> >> >>",
                $"<< /Length {pageContent.Length} >>\nstream\n{pageContent}\nendstream",
                "<< /Type /ExtGState /ca 0.5 >>",
                $"<< /Type /XObject /Subtype /Form /FormType 1 /BBox [0 0 200 200] "
                    + $"/Group << /Type /Group /S /Transparency >> /Length {formContent.Length} >>"
                    + $"\nstream\n{formContent}\nendstream",
            };

            var builder = new StringBuilder();
            builder.Append("%PDF-1.7\n");
            var offsets = new int[bodies.Count];
            for (int objectNumber = 1; objectNumber < bodies.Count; objectNumber++)
            {
                offsets[objectNumber] = builder.Length;
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
    }
}
