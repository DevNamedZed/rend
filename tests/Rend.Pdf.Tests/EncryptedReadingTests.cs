using System.IO;
using Rend.Pdf;
using Rend.Pdf.Parsing;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class EncryptedReadingTests
    {
        [Theory]
        [InlineData(PdfEncryptionMethod.Rc4_128)]
        [InlineData(PdfEncryptionMethod.Aes128)]
        public void Reader_DecryptsStringsWrittenWithEmptyUserPassword(PdfEncryptionMethod method)
        {
            const string title = "Hello Encrypted World";

            var options = new PdfDocumentOptions
            {
                // Owner password only → empty user password → readable without a password.
                OwnerPassword = "owner123",
                EncryptionMethod = method,
            };

            byte[] bytes;
            using (var doc = new PdfDocument(options))
            {
                doc.Info.Title = title;
                doc.AddPage(595, 842);
                using var ms = new MemoryStream();
                doc.Save(ms);
                bytes = ms.ToArray();
            }

            using var reader = PdfDocumentReader.Open(bytes);
            var info = reader.Resolve(reader.Trailer["Info"]);
            Assert.Equal(title, info["Title"].AsText());
        }
    }
}
