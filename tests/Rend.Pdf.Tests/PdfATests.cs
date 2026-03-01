using System;
using System.IO;
using System.Text;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class PdfATests
    {
        // Minimal valid ICC profile (sRGB header) for testing purposes.
        // A real ICC profile would be ~3KB+, but for unit testing we just need non-empty bytes
        // that the PDF writer will embed as the DestOutputProfile stream.
        private static byte[] CreateTestIccProfile()
        {
            // Minimal ICC profile header (128 bytes) + tag table.
            // This is enough for the PDF writer to embed; actual ICC validation
            // is not performed by the writer.
            var profile = new byte[128 + 4]; // header + tag count (0)
            // Profile size (big-endian)
            int size = profile.Length;
            profile[0] = (byte)(size >> 24);
            profile[1] = (byte)(size >> 16);
            profile[2] = (byte)(size >> 8);
            profile[3] = (byte)size;
            // Preferred CMM type
            profile[4] = (byte)'a'; profile[5] = (byte)'c'; profile[6] = (byte)'s'; profile[7] = (byte)'p';
            // Profile version 2.1.0
            profile[8] = 2; profile[9] = 0x10;
            // Device class: 'mntr' (monitor)
            profile[12] = (byte)'m'; profile[13] = (byte)'n'; profile[14] = (byte)'t'; profile[15] = (byte)'r';
            // Color space: 'RGB '
            profile[16] = (byte)'R'; profile[17] = (byte)'G'; profile[18] = (byte)'B'; profile[19] = (byte)' ';
            // PCS: 'XYZ '
            profile[20] = (byte)'X'; profile[21] = (byte)'Y'; profile[22] = (byte)'Z'; profile[23] = (byte)' ';
            // Profile signature 'acsp'
            profile[36] = (byte)'a'; profile[37] = (byte)'c'; profile[38] = (byte)'s'; profile[39] = (byte)'p';
            // Tag count = 0 (big-endian at offset 128)
            // Already zero from array initialization
            return profile;
        }

        [Fact]
        public void PdfA1b_CreatesOutputIntentsInCatalog()
        {
            var options = new PdfDocumentOptions
            {
                PdfAConformance = PdfALevel.A1b,
                OutputIntentProfile = CreateTestIccProfile()
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/OutputIntents", pdfText);
            Assert.Contains("/Type /OutputIntent", pdfText);
            Assert.Contains("/S /GTS_PDFA1", pdfText);
            Assert.Contains("/DestOutputProfile", pdfText);
            Assert.Contains("/OutputConditionIdentifier", pdfText);
        }

        [Fact]
        public void PdfA1b_IncludesPdfaidNamespaceInXmp()
        {
            var options = new PdfDocumentOptions
            {
                PdfAConformance = PdfALevel.A1b,
                OutputIntentProfile = CreateTestIccProfile()
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("xmlns:pdfaid", pdfText);
            Assert.Contains("<pdfaid:part>1</pdfaid:part>", pdfText);
            Assert.Contains("<pdfaid:conformance>B</pdfaid:conformance>", pdfText);
        }

        [Fact]
        public void PdfA2b_CreatesOutputIntentsWithGTS_PDFA2()
        {
            var options = new PdfDocumentOptions
            {
                PdfAConformance = PdfALevel.A2b,
                OutputIntentProfile = CreateTestIccProfile()
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/OutputIntents", pdfText);
            Assert.Contains("/S /GTS_PDFA2", pdfText);
            Assert.Contains("<pdfaid:part>2</pdfaid:part>", pdfText);
            Assert.Contains("<pdfaid:conformance>B</pdfaid:conformance>", pdfText);
        }

        [Fact]
        public void PdfA3b_CreatesOutputIntentsWithGTS_PDFA2()
        {
            var options = new PdfDocumentOptions
            {
                PdfAConformance = PdfALevel.A3b,
                OutputIntentProfile = CreateTestIccProfile()
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/OutputIntents", pdfText);
            Assert.Contains("/S /GTS_PDFA2", pdfText);
            Assert.Contains("<pdfaid:part>3</pdfaid:part>", pdfText);
            Assert.Contains("<pdfaid:conformance>B</pdfaid:conformance>", pdfText);
        }

        [Fact]
        public void PdfA_MissingIccProfile_ThrowsInvalidOperationException()
        {
            var options = new PdfDocumentOptions
            {
                PdfAConformance = PdfALevel.A1b,
                OutputIntentProfile = null
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            Assert.Throws<InvalidOperationException>(() =>
            {
                using var ms = new MemoryStream();
                doc.Save(ms);
            });
        }

        [Fact]
        public void PdfA_EmptyIccProfile_ThrowsInvalidOperationException()
        {
            var options = new PdfDocumentOptions
            {
                PdfAConformance = PdfALevel.A1b,
                OutputIntentProfile = Array.Empty<byte>()
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            Assert.Throws<InvalidOperationException>(() =>
            {
                using var ms = new MemoryStream();
                doc.Save(ms);
            });
        }

        [Fact]
        public void PdfA_ForcesXmpMetadataInclusion()
        {
            var options = new PdfDocumentOptions
            {
                PdfAConformance = PdfALevel.A1b,
                OutputIntentProfile = CreateTestIccProfile(),
                IncludeXmpMetadata = false // Explicitly disabled, but PDF/A should force it on
            };
            using var doc = new PdfDocument(options);
            doc.Info.Title = "PDF/A Test";
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/Metadata", pdfText);
            Assert.Contains("xmpmeta", pdfText);
        }

        [Fact]
        public void PdfA_ForcesTaggedPdf()
        {
            var options = new PdfDocumentOptions
            {
                PdfAConformance = PdfALevel.A1b,
                OutputIntentProfile = CreateTestIccProfile(),
                EnableTaggedPdf = false // Explicitly disabled, but PDF/A should force it on
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/StructTreeRoot", pdfText);
            Assert.Contains("/MarkInfo", pdfText);
        }

        [Fact]
        public void PdfA_CustomOutputCondition_AppearsInOutput()
        {
            var options = new PdfDocumentOptions
            {
                PdfAConformance = PdfALevel.A2b,
                OutputIntentProfile = CreateTestIccProfile(),
                OutputCondition = "Custom CMYK Profile"
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("Custom CMYK Profile", pdfText);
        }

        [Fact]
        public void PdfA_DefaultOutputCondition_IsSrgb()
        {
            var options = new PdfDocumentOptions
            {
                PdfAConformance = PdfALevel.A1b,
                OutputIntentProfile = CreateTestIccProfile()
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("sRGB IEC61966-2.1", pdfText);
        }

        [Fact]
        public void NoPdfA_NoOutputIntents()
        {
            var options = new PdfDocumentOptions();
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.DoesNotContain("/OutputIntents", pdfText);
            Assert.DoesNotContain("/OutputIntent", pdfText);
            Assert.DoesNotContain("pdfaid", pdfText);
        }

        [Fact]
        public void NoPdfA_XmpDoesNotContainPdfaidNamespace()
        {
            var options = new PdfDocumentOptions { IncludeXmpMetadata = true };
            using var doc = new PdfDocument(options);
            doc.Info.Title = "Regular PDF";
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/Metadata", pdfText);
            Assert.DoesNotContain("pdfaid", pdfText);
        }

        [Fact]
        public void PdfA_IccProfileStreamHasNValue()
        {
            var options = new PdfDocumentOptions
            {
                PdfAConformance = PdfALevel.A1b,
                OutputIntentProfile = CreateTestIccProfile()
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            // The ICC stream dict should have /N 3 (for RGB)
            Assert.Contains("/N 3", pdfText);
        }

        private static string SaveToString(PdfDocument doc)
        {
            using var ms = new MemoryStream();
            doc.Save(ms);
            return Encoding.Latin1.GetString(ms.ToArray());
        }
    }
}
