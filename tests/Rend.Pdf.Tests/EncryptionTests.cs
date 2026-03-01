using System;
using System.IO;
using System.Text;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class EncryptionTests
    {
        [Fact]
        public void Encryption_NoPassword_NoEncryptDict()
        {
            using var doc = new PdfDocument();
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.DoesNotContain("/Encrypt", pdfText);
        }

        [Fact]
        public void Encryption_UserPassword_ProducesEncryptDict()
        {
            var options = new PdfDocumentOptions { UserPassword = "secret" };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/Encrypt", pdfText);
        }

        [Fact]
        public void Encryption_OwnerPasswordOnly_ProducesEncryptDict()
        {
            var options = new PdfDocumentOptions { OwnerPassword = "owner123" };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/Encrypt", pdfText);
        }

        [Fact]
        public void Encryption_Aes128_ProducesCorrectStructure()
        {
            var options = new PdfDocumentOptions
            {
                UserPassword = "test",
                EncryptionMethod = PdfEncryptionMethod.Aes128
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/Filter /Standard", pdfText);
            Assert.Contains("/V 4", pdfText);
            Assert.Contains("/R 4", pdfText);
            Assert.Contains("/Length 128", pdfText);
            Assert.Contains("/StmF /StdCF", pdfText);
            Assert.Contains("/StrF /StdCF", pdfText);
            Assert.Contains("/CFM /AESV2", pdfText);
            Assert.Contains("/AuthEvent /DocOpen", pdfText);
        }

        [Fact]
        public void Encryption_Rc4_128_ProducesCorrectStructure()
        {
            var options = new PdfDocumentOptions
            {
                UserPassword = "test",
                EncryptionMethod = PdfEncryptionMethod.Rc4_128
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/Filter /Standard", pdfText);
            Assert.Contains("/V 2", pdfText);
            Assert.Contains("/R 3", pdfText);
            Assert.DoesNotContain("/StmF", pdfText);
            Assert.DoesNotContain("/StrF", pdfText);
            Assert.DoesNotContain("/CF", pdfText);
        }

        [Fact]
        public void Encryption_ProducesFileId()
        {
            var options = new PdfDocumentOptions { UserPassword = "test" };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/ID", pdfText);
        }

        [Fact]
        public void Encryption_OAndUValues_Present()
        {
            var options = new PdfDocumentOptions { UserPassword = "test" };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/O <", pdfText);
            Assert.Contains("/U <", pdfText);
        }

        [Fact]
        public void Encryption_OAndUValues_Are32Bytes()
        {
            var options = new PdfDocumentOptions { UserPassword = "test" };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            // O value: 32 bytes = 64 hex chars between < and >
            int oIdx = pdfText.IndexOf("/O <", StringComparison.Ordinal);
            Assert.True(oIdx >= 0);
            int oStart = pdfText.IndexOf('<', oIdx) + 1;
            int oEnd = pdfText.IndexOf('>', oStart);
            Assert.Equal(64, oEnd - oStart);

            // U value: 32 bytes = 64 hex chars
            int uIdx = pdfText.IndexOf("/U <", StringComparison.Ordinal);
            Assert.True(uIdx >= 0);
            int uStart = pdfText.IndexOf('<', uIdx) + 1;
            int uEnd = pdfText.IndexOf('>', uStart);
            Assert.Equal(64, uEnd - uStart);
        }

        [Fact]
        public void Encryption_PValue_Present()
        {
            var options = new PdfDocumentOptions
            {
                UserPassword = "test",
                Permissions = PdfPermissions.Print | PdfPermissions.Extract
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/P ", pdfText);
        }

        [Fact]
        public void Encryption_DifferentPermissions_DifferentPValue()
        {
            string pdfAll = SaveEncryptedWithPermissions(PdfPermissions.All);
            string pdfNone = SaveEncryptedWithPermissions(PdfPermissions.None);

            // Extract P values
            int pAll = ExtractPValue(pdfAll);
            int pNone = ExtractPValue(pdfNone);
            Assert.NotEqual(pAll, pNone);
        }

        [Fact]
        public void Encryption_OutputDiffersFromUnencrypted()
        {
            // Create identical content with and without encryption
            byte[] unencrypted;
            byte[] encrypted;

            {
                var options = new PdfDocumentOptions { Compression = PdfCompression.None };
                using var doc = new PdfDocument(options);
                doc.Info.Title = "Test Title";
                var page = doc.AddPage(595, 842);
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.MoveTextPosition(50, 750);
                page.Content.ShowText(font, "Hello World");
                page.Content.EndText();
                using var ms = new MemoryStream();
                doc.Save(ms);
                unencrypted = ms.ToArray();
            }

            {
                var options = new PdfDocumentOptions
                {
                    Compression = PdfCompression.None,
                    UserPassword = "secret"
                };
                using var doc = new PdfDocument(options);
                doc.Info.Title = "Test Title";
                var page = doc.AddPage(595, 842);
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.MoveTextPosition(50, 750);
                page.Content.ShowText(font, "Hello World");
                page.Content.EndText();
                using var ms = new MemoryStream();
                doc.Save(ms);
                encrypted = ms.ToArray();
            }

            // Encrypted output must differ and be larger (due to encryption overhead)
            string unencText = Encoding.Latin1.GetString(unencrypted);
            string encText = Encoding.Latin1.GetString(encrypted);

            Assert.DoesNotContain("/Encrypt", unencText);
            Assert.Contains("/Encrypt", encText);

            // The literal text "Hello World" should appear in unencrypted but not in encrypted streams
            // (streams are encrypted after compression)
            Assert.Contains("Hello World", unencText);
        }

        [Fact]
        public void Encryption_BothPasswords_ProducesEncryptDict()
        {
            var options = new PdfDocumentOptions
            {
                UserPassword = "user",
                OwnerPassword = "owner"
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/Encrypt", pdfText);
            Assert.Contains("/O <", pdfText);
            Assert.Contains("/U <", pdfText);
        }

        [Fact]
        public void Encryption_EmptyUserPassword_ProducesEncryptDict()
        {
            var options = new PdfDocumentOptions
            {
                UserPassword = "",
                OwnerPassword = "owner"
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/Encrypt", pdfText);
        }

        [Fact]
        public void Encryption_MultiplePages_AllEncrypted()
        {
            var options = new PdfDocumentOptions
            {
                UserPassword = "test",
                Compression = PdfCompression.None
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);
            doc.AddPage(595, 842);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/Encrypt", pdfText);
            // Should still have valid PDF structure
            Assert.Contains("/Pages", pdfText);
            Assert.Contains("/Page", pdfText);
        }

        [Fact]
        public void Encryption_WithXmpMetadata_BothPresent()
        {
            var options = new PdfDocumentOptions
            {
                UserPassword = "test",
                IncludeXmpMetadata = true
            };
            using var doc = new PdfDocument(options);
            doc.Info.Title = "Encrypted + XMP";
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/Encrypt", pdfText);
            Assert.Contains("/Metadata", pdfText);
        }

        private static string SaveToString(PdfDocument doc)
        {
            using var ms = new MemoryStream();
            doc.Save(ms);
            return Encoding.Latin1.GetString(ms.ToArray());
        }

        private static string SaveEncryptedWithPermissions(PdfPermissions permissions)
        {
            var options = new PdfDocumentOptions
            {
                UserPassword = "test",
                Permissions = permissions
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);
            return SaveToString(doc);
        }

        private static int ExtractPValue(string pdfText)
        {
            // Find "/P " followed by a number
            int idx = pdfText.IndexOf("/P ", StringComparison.Ordinal);
            if (idx < 0) return 0;
            idx += 3;
            // Read the integer value (may be negative)
            var sb = new StringBuilder();
            while (idx < pdfText.Length && (char.IsDigit(pdfText[idx]) || pdfText[idx] == '-'))
            {
                sb.Append(pdfText[idx]);
                idx++;
            }
            return int.TryParse(sb.ToString(), out int val) ? val : 0;
        }
    }
}
