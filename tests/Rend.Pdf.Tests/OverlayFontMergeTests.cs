using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Rend.Pdf.Parsing;
using Xunit;

namespace Rend.Pdf.Tests
{
    /// <summary>
    /// End-to-end checks for overlaying onto a page that already has fonts: the overlay must
    /// supersede the page in the page tree (PDF-W2) and merge its fonts into the page's existing
    /// /Font sub-dictionary rather than emit a duplicate /Font key (PDF-W1).
    /// </summary>
    public class OverlayFontMergeTests
    {
        [Fact]
        public async Task TextOverlay_OnPageWithExistingFont_MergesFontsAndSupersedesPage()
        {
            byte[] basePdf = BuildPdfWithText("BaseText");

            var overlay = new PdfOverlay();
            var elements = new List<PdfOverlayElement>
            {
                new TextOverlay { Page = 1, Text = "Overlay", X = 20, Y = 20, FontSize = 14, FontFamily = "Helvetica" }
            };
            byte[] result = await overlay.ApplyAsync(basePdf, elements);

            using var reader = PdfDocumentReader.Open(result);
            PdfObj page = reader.Resolve(reader.GetPage(0));

            // PDF-W2: the reader resolves the superseded (merged) page — not the original, which had
            // only F1 — and its /Contents now references both the original and the overlay streams.
            PdfObj contents = reader.Resolve(page["Contents"]);
            Assert.True(contents.IsArray && contents.Count == 2,
                "overlaid page should reference the original + overlay content streams");

            // PDF-W1: both the page's own font (F1) and the overlay font (F_Helvetica) live in one
            // valid /Font dict. A duplicate /Font key would leave the reader seeing only one of them.
            PdfObj fonts = reader.Resolve(reader.Resolve(page["Resources"])["Font"]);
            Assert.Contains("F1", fonts.Keys);
            Assert.Contains("F_Helvetica", fonts.Keys);
        }

        [Fact]
        public async Task TextOverlay_OnSingleLinePageDict_PreservesExistingResources()
        {
            // A page dictionary written on ONE line (common from non-Rend producers). The old
            // line-based /Contents removal would delete everything after /Contents — including the
            // page's own /Resources — dropping its fonts. The merge must keep both.
            byte[] basePdf = BuildSingleLinePagePdf();

            var overlay = new PdfOverlay();
            var elements = new List<PdfOverlayElement>
            {
                new TextOverlay { Page = 1, Text = "Overlay", X = 20, Y = 20, FontSize = 14, FontFamily = "Helvetica" }
            };
            byte[] result = await overlay.ApplyAsync(basePdf, elements);

            using var reader = PdfDocumentReader.Open(result);
            PdfObj page = reader.Resolve(reader.GetPage(0));
            PdfObj fonts = reader.Resolve(reader.Resolve(page["Resources"])["Font"]);
            Assert.Contains("F1", fonts.Keys);          // page's own font preserved
            Assert.Contains("F_Helvetica", fonts.Keys);  // overlay font merged
        }

        [Fact]
        public async Task TextOverlay_MultiplePages_DoesNotLeakFontsAcrossPages()
        {
            byte[] basePdf = BuildTwoPagePdf();

            var overlay = new PdfOverlay();
            var elements = new List<PdfOverlayElement>
            {
                new TextOverlay { Page = 1, Text = "One", X = 20, Y = 20, FontSize = 14, FontFamily = "Times" },
                new TextOverlay { Page = 2, Text = "Two", X = 20, Y = 20, FontSize = 14, FontFamily = "Courier" },
            };
            byte[] result = await overlay.ApplyAsync(basePdf, elements);

            using var reader = PdfDocumentReader.Open(result);
            PdfObj page2 = reader.Resolve(reader.GetPage(1));
            PdfObj fonts2 = reader.Resolve(reader.Resolve(page2["Resources"])["Font"]);
            // Page 2 carries only its own overlay font; page 1's Times must not bleed in.
            Assert.Contains("F_Courier", fonts2.Keys);
            Assert.DoesNotContain("F_Times_Roman", fonts2.Keys);
        }

        private static byte[] BuildTwoPagePdf()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AddPage(612, 792);
            doc.AddPage(612, 792);
            using var ms = new MemoryStream();
            doc.Save(ms);
            return ms.ToArray();
        }

        [Fact]
        public async Task TextOverlay_OnIndirectResources_PreservesOriginalResources()
        {
            // Page /Resources is an INDIRECT reference (/Resources 5 0 R) — the overlay must resolve
            // and inline it so the page keeps its own fonts, not discard them for a fresh dict.
            string content = "BT /F1 12 Tf 10 100 Td (Base) Tj ET";
            byte[] basePdf = AssemblePdf(new List<string>
            {
                "",
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 200 200] /Contents 4 0 R /Resources 5 0 R >>",
                $"<< /Length {content.Length} >>\nstream\n{content}\nendstream",
                "<< /Font << /F1 6 0 R >> /ProcSet [/PDF /Text] >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            });

            byte[] result = await new PdfOverlay().ApplyAsync(basePdf, OneHelveticaStamp());

            using var reader = PdfDocumentReader.Open(result);
            PdfObj page = reader.Resolve(reader.GetPage(0));
            PdfObj fonts = reader.Resolve(reader.Resolve(page["Resources"])["Font"]);
            Assert.Contains("F1", fonts.Keys);
            Assert.Contains("F_Helvetica", fonts.Keys);
        }

        [Fact]
        public async Task TextOverlay_ResourcesValueWithStringLiteral_DoesNotCorruptDict()
        {
            // A string literal containing ">>" inside the page's inline /Resources must not be
            // mistaken for the dictionary's closing delimiter by the brace scanner.
            string content = "BT /F1 12 Tf 10 100 Td (Base) Tj ET";
            byte[] basePdf = AssemblePdf(new List<string>
            {
                "",
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 200 200] /Contents 4 0 R "
                    + "/Resources << /Font << /F1 5 0 R >> /Note (close >> here) >> >>",
                $"<< /Length {content.Length} >>\nstream\n{content}\nendstream",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            });

            byte[] result = await new PdfOverlay().ApplyAsync(basePdf, OneHelveticaStamp());

            using var reader = PdfDocumentReader.Open(result);
            PdfObj page = reader.Resolve(reader.GetPage(0));
            PdfObj resources = reader.Resolve(page["Resources"]);
            Assert.True(resources.IsDict, "page /Resources must remain a parseable dictionary");
            PdfObj fonts = reader.Resolve(resources["Font"]);
            Assert.Contains("F1", fonts.Keys);
            Assert.Contains("F_Helvetica", fonts.Keys);
        }

        private static List<PdfOverlayElement> OneHelveticaStamp()
        {
            return new List<PdfOverlayElement>
            {
                new TextOverlay { Page = 1, Text = "Overlay", X = 20, Y = 20, FontSize = 14, FontFamily = "Helvetica" }
            };
        }

        // Hand-built PDF whose page dictionary (incl. an inline /Font in /Resources) is on a single
        // line — PdfDocument always writes multi-line, so this case needs a hand-crafted fixture.
        private static byte[] BuildSingleLinePagePdf()
        {
            string content = "BT /F1 12 Tf 10 100 Td (Base) Tj ET";
            return AssemblePdf(new List<string>
            {
                "",
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 200 200] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> /ProcSet [/PDF /Text] >> >>",
                $"<< /Length {content.Length} >>\nstream\n{content}\nendstream",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            });
        }

        // Assembles a single-section PDF (objects 1..N) with a correct cross-reference table.
        private static byte[] AssemblePdf(List<string> bodies)
        {
            var builder = new StringBuilder();
            builder.Append("%PDF-1.7\n");
            var offsets = new int[bodies.Count];
            for (int objectNumber = 1; objectNumber < bodies.Count; objectNumber++)
            {
                offsets[objectNumber] = builder.Length;
                builder.Append($"{objectNumber} 0 obj\n{bodies[objectNumber]}\nendobj\n");
            }
            int xrefOffset = builder.Length;
            builder.Append("xref\n").Append($"0 {bodies.Count}\n").Append("0000000000 65535 f \n");
            for (int objectNumber = 1; objectNumber < bodies.Count; objectNumber++)
            {
                builder.Append(offsets[objectNumber].ToString("D10") + " 00000 n \n");
            }
            builder.Append("trailer\n").Append($"<< /Size {bodies.Count} /Root 1 0 R >>\n")
                .Append("startxref\n").Append(xrefOffset + "\n").Append("%%EOF");
            return Encoding.ASCII.GetBytes(builder.ToString());
        }

        private static byte[] BuildPdfWithText(string text)
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            PdfPage page = doc.AddPage(612, 792);
            PdfFont font = doc.GetStandardFont(StandardFont.Helvetica);
            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.ShowText(font, text);
            page.Content.EndText();
            using var ms = new MemoryStream();
            doc.Save(ms);
            return ms.ToArray();
        }
    }
}
