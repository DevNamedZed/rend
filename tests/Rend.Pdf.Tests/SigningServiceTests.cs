using System;
using System.IO;
using Xunit;

namespace Rend.Pdf.Tests
{
    /// <summary>
    /// Tests for <see cref="PdfSigningService"/> and <see cref="IPdfSigningService"/> argument validation.
    /// Functional signing tests are in SignatureTests.cs.
    /// </summary>
    public class SigningServiceTests
    {
        private readonly IPdfSigningService _service = new PdfSigningService();

        // ── Interface contract ──

        [Fact]
        public void ImplementsIPdfSigningService()
        {
            Assert.IsAssignableFrom<IPdfSigningService>(_service);
        }

        // ── Sign(Stream, Stream, PdfSignatureOptions) validation ──

        [Fact]
        public void Sign_Stream_NullInput_Throws()
        {
            var options = new PdfSignatureOptions();
            Assert.Throws<ArgumentNullException>(() =>
                _service.Sign((Stream)null!, new MemoryStream(), options));
        }

        [Fact]
        public void Sign_Stream_NullOutput_Throws()
        {
            using var input = new MemoryStream(new byte[] { 1 });
            var options = new PdfSignatureOptions();
            Assert.Throws<ArgumentNullException>(() =>
                _service.Sign(input, null!, options));
        }

        [Fact]
        public void Sign_Stream_NullOptions_Throws()
        {
            using var input = new MemoryStream(new byte[] { 1 });
            using var output = new MemoryStream();
            Assert.Throws<ArgumentNullException>(() =>
                _service.Sign(input, output, (PdfSignatureOptions)null!));
        }

        [Fact]
        public void Sign_Stream_NullSigner_Throws()
        {
            using var input = new MemoryStream(new byte[] { 1 });
            using var output = new MemoryStream();
            var options = new PdfSignatureOptions { Signer = null! };
            Assert.ThrowsAny<ArgumentException>(() =>
                _service.Sign(input, output, options));
        }

        // ── Sign(byte[], PdfSignatureOptions) validation ──

        [Fact]
        public void Sign_ByteArray_NullPdf_Throws()
        {
            var options = new PdfSignatureOptions();
            Assert.ThrowsAny<ArgumentException>(() =>
                _service.Sign((byte[])null!, options));
        }

        [Fact]
        public void Sign_ByteArray_EmptyPdf_Throws()
        {
            var options = new PdfSignatureOptions();
            Assert.ThrowsAny<ArgumentException>(() =>
                _service.Sign(Array.Empty<byte>(), options));
        }

        [Fact]
        public void Sign_ByteArray_NullOptions_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _service.Sign(new byte[] { 1 }, (PdfSignatureOptions)null!));
        }

        [Fact]
        public void Sign_ByteArray_NullSigner_Throws()
        {
            var options = new PdfSignatureOptions { Signer = null! };
            Assert.ThrowsAny<ArgumentException>(() =>
                _service.Sign(new byte[] { 1 }, options));
        }

        // ── PdfSignatureOptions model ──

        [Fact]
        public void PdfSignatureOptions_DefaultValues()
        {
            var options = new PdfSignatureOptions();
            Assert.Null(options.SignerName);
            Assert.Null(options.Reason);
            Assert.Null(options.Location);
            Assert.Null(options.ContactInfo);
        }

        [Fact]
        public void PdfSignatureOptions_SetProperties()
        {
            var options = new PdfSignatureOptions
            {
                SignerName = "John Doe",
                Reason = "Approval",
                Location = "New York",
                ContactInfo = "john@example.com"
            };

            Assert.Equal("John Doe", options.SignerName);
            Assert.Equal("Approval", options.Reason);
            Assert.Equal("New York", options.Location);
            Assert.Equal("john@example.com", options.ContactInfo);
        }
    }
}
