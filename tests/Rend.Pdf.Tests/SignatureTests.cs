using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class SignatureTests
    {
        /// <summary>Create a self-signed test certificate with private key, exported as PFX bytes.</summary>
        private static byte[] CreateTestCertificate(string password = "test")
        {
            using var rsa = RSA.Create(2048);
            var req = new CertificateRequest("CN=Test Signer", rsa, HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
            using var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1));
            return cert.Export(X509ContentType.Pfx, password);
        }

        private static string SaveToString(PdfDocument doc)
        {
            using var ms = new MemoryStream();
            doc.Save(ms);
            return Encoding.Latin1.GetString(ms.ToArray());
        }

        private static byte[] SaveToBytes(PdfDocument doc)
        {
            using var ms = new MemoryStream();
            doc.Save(ms);
            return ms.ToArray();
        }

        // ═══════════════════════════════════════════
        // Validation tests
        // ═══════════════════════════════════════════

        [Fact]
        public void Signature_NoCertificateData_Throws()
        {
            var options = new PdfDocumentOptions
            {
                Signature = new PdfSignatureOptions
                {
                    // CertificateData intentionally null
                }
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            Assert.Throws<InvalidOperationException>(() => doc.ToArray());
        }

        [Fact]
        public void Signature_EmptyCertificateData_Throws()
        {
            var options = new PdfDocumentOptions
            {
                Signature = new PdfSignatureOptions
                {
                    CertificateData = Array.Empty<byte>(),
                    CertificatePassword = "test"
                }
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            Assert.Throws<InvalidOperationException>(() => doc.ToArray());
        }

        // ═══════════════════════════════════════════
        // Structure tests
        // ═══════════════════════════════════════════

        [Fact]
        public void Signature_ProducesSignatureDictionary()
        {
            byte[] pfx = CreateTestCertificate();
            var options = new PdfDocumentOptions
            {
                Signature = new PdfSignatureOptions
                {
                    CertificateData = pfx,
                    CertificatePassword = "test",
                    SignerName = "Test Signer"
                }
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);

            Assert.Contains("/Type /Sig", pdfText);
            Assert.Contains("/Filter /Adobe.PPKLite", pdfText);
            Assert.Contains("/SubFilter /adbe.pkcs7.detached", pdfText);
        }

        [Fact]
        public void Signature_ContainsSignerName()
        {
            byte[] pfx = CreateTestCertificate();
            var options = new PdfDocumentOptions
            {
                Signature = new PdfSignatureOptions
                {
                    CertificateData = pfx,
                    CertificatePassword = "test",
                    SignerName = "Alice"
                }
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/Name (Alice)", pdfText);
        }

        [Fact]
        public void Signature_ContainsReasonAndLocation()
        {
            byte[] pfx = CreateTestCertificate();
            var options = new PdfDocumentOptions
            {
                Signature = new PdfSignatureOptions
                {
                    CertificateData = pfx,
                    CertificatePassword = "test",
                    Reason = "Approval",
                    Location = "New York"
                }
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/Reason (Approval)", pdfText);
            Assert.Contains("/Location (New York)", pdfText);
        }

        [Fact]
        public void Signature_ContainsContactInfo()
        {
            byte[] pfx = CreateTestCertificate();
            var options = new PdfDocumentOptions
            {
                Signature = new PdfSignatureOptions
                {
                    CertificateData = pfx,
                    CertificatePassword = "test",
                    ContactInfo = "alice@example.com"
                }
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/ContactInfo (alice@example.com)", pdfText);
        }

        [Fact]
        public void Signature_ContainsTimestamp()
        {
            byte[] pfx = CreateTestCertificate();
            var options = new PdfDocumentOptions
            {
                Signature = new PdfSignatureOptions
                {
                    CertificateData = pfx,
                    CertificatePassword = "test"
                }
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/M (D:", pdfText);
        }

        [Fact]
        public void Signature_HasAcroFormWithSigFlags()
        {
            byte[] pfx = CreateTestCertificate();
            var options = new PdfDocumentOptions
            {
                Signature = new PdfSignatureOptions
                {
                    CertificateData = pfx,
                    CertificatePassword = "test"
                }
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/SigFlags 3", pdfText);
            Assert.Contains("/Fields [", pdfText);
        }

        [Fact]
        public void Signature_HasWidgetAnnotation()
        {
            byte[] pfx = CreateTestCertificate();
            var options = new PdfDocumentOptions
            {
                Signature = new PdfSignatureOptions
                {
                    CertificateData = pfx,
                    CertificatePassword = "test"
                }
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/Subtype /Widget", pdfText);
            Assert.Contains("/FT /Sig", pdfText);
            Assert.Contains("/Rect [0 0 0 0]", pdfText);
        }

        [Fact]
        public void Signature_CatalogReferencesAcroForm()
        {
            byte[] pfx = CreateTestCertificate();
            var options = new PdfDocumentOptions
            {
                Signature = new PdfSignatureOptions
                {
                    CertificateData = pfx,
                    CertificatePassword = "test"
                }
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            // The catalog should reference the AcroForm
            Assert.Contains("/AcroForm", pdfText);
        }

        // ═══════════════════════════════════════════
        // ByteRange tests
        // ═══════════════════════════════════════════

        [Fact]
        public void Signature_ByteRangeIsPatchedWithRealValues()
        {
            byte[] pfx = CreateTestCertificate();
            var options = new PdfDocumentOptions
            {
                Signature = new PdfSignatureOptions
                {
                    CertificateData = pfx,
                    CertificatePassword = "test"
                }
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);

            // ByteRange should not contain the placeholder zeros
            Assert.DoesNotContain("[0 0000000000 0000000000 0000000000]", pdfText);

            // ByteRange should start with [0 and contain real offset values
            int brIdx = pdfText.IndexOf("/ByteRange [", StringComparison.Ordinal);
            Assert.True(brIdx >= 0, "Should contain /ByteRange");

            // Extract ByteRange value
            int brStart = pdfText.IndexOf('[', brIdx);
            int brEnd = pdfText.IndexOf(']', brStart);
            string brValue = pdfText.Substring(brStart, brEnd - brStart + 1);

            // Parse the four integers
            string inner = brValue.Trim('[', ']').Trim();
            var parts = inner.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(4, parts.Length);

            // First value should be 0 (start of file)
            Assert.Equal("0", parts[0]);

            // All values should be parseable as non-negative integers
            foreach (var part in parts)
            {
                Assert.True(long.TryParse(part, out long val), $"'{part}' should be a valid integer");
                Assert.True(val >= 0, $"ByteRange value {val} should be non-negative");
            }

            // offset2 should be > offset1 (the gap is the /Contents hex string)
            long offset1 = long.Parse(parts[1]);
            long offset2 = long.Parse(parts[2]);
            Assert.True(offset2 > offset1, "offset2 should be greater than offset1");
        }

        [Fact]
        public void Signature_ByteRangeCoversEntireFile()
        {
            byte[] pfx = CreateTestCertificate();
            var options = new PdfDocumentOptions
            {
                Signature = new PdfSignatureOptions
                {
                    CertificateData = pfx,
                    CertificatePassword = "test"
                }
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            byte[] pdfBytes = SaveToBytes(doc);
            string pdfText = Encoding.Latin1.GetString(pdfBytes);

            // Extract ByteRange
            int brIdx = pdfText.IndexOf("/ByteRange [", StringComparison.Ordinal);
            int brStart = pdfText.IndexOf('[', brIdx);
            int brEnd = pdfText.IndexOf(']', brStart);
            string brValue = pdfText.Substring(brStart + 1, brEnd - brStart - 1).Trim();
            var parts = brValue.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            long start1 = long.Parse(parts[0]);
            long len1 = long.Parse(parts[1]);
            long start2 = long.Parse(parts[2]);
            long len2 = long.Parse(parts[3]);

            // The two ranges should cover the entire file except the /Contents hex value
            // start1 + len1 + gap + len2 should equal total file length
            long totalCovered = len1 + len2;
            long gap = start2 - (start1 + len1);
            Assert.True(gap > 0, "Gap between byte ranges should be positive (the signature hex)");
            Assert.Equal(pdfBytes.Length, totalCovered + gap);
        }

        // ═══════════════════════════════════════════
        // Contents / Actual signature tests
        // ═══════════════════════════════════════════

        [Fact]
        public void Signature_ContentsIsNotAllZeros()
        {
            byte[] pfx = CreateTestCertificate();
            var options = new PdfDocumentOptions
            {
                Signature = new PdfSignatureOptions
                {
                    CertificateData = pfx,
                    CertificatePassword = "test"
                }
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            byte[] pdfBytes = SaveToBytes(doc);
            string pdfText = Encoding.Latin1.GetString(pdfBytes);

            // Find /Contents in signature dict context (near /Type /Sig)
            int sigTypeIdx = pdfText.IndexOf("/Type /Sig", StringComparison.Ordinal);
            Assert.True(sigTypeIdx >= 0);

            // Find the hex string contents near the signature dict
            int contentsIdx = pdfText.IndexOf("/Contents <", sigTypeIdx, StringComparison.Ordinal);
            Assert.True(contentsIdx >= 0);

            int hexStart = pdfText.IndexOf('<', contentsIdx) + 1;
            int hexEnd = pdfText.IndexOf('>', hexStart);
            string hexValue = pdfText.Substring(hexStart, hexEnd - hexStart);

            // Should not be all zeros (would mean signing failed to produce output)
            Assert.False(hexValue.TrimEnd('0').Length == 0,
                "Signature /Contents should not be all zeros after signing");
        }

        // ═══════════════════════════════════════════
        // Output validity tests
        // ═══════════════════════════════════════════

        [Fact]
        public void Signature_OutputStartsWithPdfHeader()
        {
            byte[] pfx = CreateTestCertificate();
            var options = new PdfDocumentOptions
            {
                Signature = new PdfSignatureOptions
                {
                    CertificateData = pfx,
                    CertificatePassword = "test"
                }
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            byte[] pdfBytes = SaveToBytes(doc);
            string header = Encoding.ASCII.GetString(pdfBytes, 0, 5);
            Assert.Equal("%PDF-", header);
        }

        [Fact]
        public void Signature_OutputEndsWithEOF()
        {
            byte[] pfx = CreateTestCertificate();
            var options = new PdfDocumentOptions
            {
                Signature = new PdfSignatureOptions
                {
                    CertificateData = pfx,
                    CertificatePassword = "test"
                }
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.EndsWith("%%EOF\n", pdfText);
        }

        [Fact]
        public void Signature_IncrementalUpdateHasPrev()
        {
            byte[] pfx = CreateTestCertificate();
            var options = new PdfDocumentOptions
            {
                Signature = new PdfSignatureOptions
                {
                    CertificateData = pfx,
                    CertificatePassword = "test"
                }
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);

            // Incremental update trailer should have /Prev pointing to original xref
            Assert.Contains("/Prev ", pdfText);
        }

        [Fact]
        public void Signature_UnsignedDocumentHasNoSignatureDict()
        {
            using var doc = new PdfDocument();
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.DoesNotContain("/Type /Sig", pdfText);
            Assert.DoesNotContain("/SubFilter", pdfText);
            Assert.DoesNotContain("/ByteRange", pdfText);
        }

        [Fact]
        public void Signature_MultiplePages_StillWorks()
        {
            byte[] pfx = CreateTestCertificate();
            var options = new PdfDocumentOptions
            {
                Signature = new PdfSignatureOptions
                {
                    CertificateData = pfx,
                    CertificatePassword = "test",
                    SignerName = "Multi-Page Signer"
                }
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);
            doc.AddPage(595, 842);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/Type /Sig", pdfText);
            Assert.Contains("/Filter /Adobe.PPKLite", pdfText);
            Assert.Contains("/Name (Multi-Page Signer)", pdfText);
        }

        [Fact]
        public void Signature_WithContent_ProducesValidOutput()
        {
            byte[] pfx = CreateTestCertificate();
            var options = new PdfDocumentOptions
            {
                Compression = PdfCompression.None,
                Signature = new PdfSignatureOptions
                {
                    CertificateData = pfx,
                    CertificatePassword = "test",
                    SignerName = "Content Signer",
                    Reason = "Testing"
                }
            };
            using var doc = new PdfDocument(options);
            var page = doc.AddPage(595, 842);
            var font = doc.GetStandardFont(StandardFont.Helvetica);
            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.MoveTextPosition(50, 750);
            page.Content.ShowText(font, "Signed document content");
            page.Content.EndText();

            byte[] pdfBytes = SaveToBytes(doc);
            string pdfText = Encoding.Latin1.GetString(pdfBytes);

            // Should have both the content and the signature
            Assert.Contains("Signed document content", pdfText);
            Assert.Contains("/Type /Sig", pdfText);
            Assert.Contains("/SubFilter /adbe.pkcs7.detached", pdfText);
        }

        [Fact]
        public void Signature_SaveToStream_Works()
        {
            byte[] pfx = CreateTestCertificate();
            var options = new PdfDocumentOptions
            {
                Signature = new PdfSignatureOptions
                {
                    CertificateData = pfx,
                    CertificatePassword = "test"
                }
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            using var ms = new MemoryStream();
            doc.Save(ms);

            Assert.True(ms.Length > 0, "Output stream should not be empty");
            ms.Position = 0;
            byte[] header = new byte[5];
            ms.Read(header, 0, 5);
            Assert.Equal("%PDF-", Encoding.ASCII.GetString(header));
        }
    }
}
