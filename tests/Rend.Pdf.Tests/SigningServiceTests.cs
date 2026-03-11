using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Rend.Pdf.Tests
{
    /// <summary>
    /// Tests for <see cref="PdfSigningService"/> and <see cref="IPdfSigningService"/> argument validation.
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

        // ── SignAsync(Stream, Stream, PdfSignatureOptions) validation ──

        [Fact]
        public async Task SignAsync_Stream_NullInput_Throws()
        {
            var options = new PdfSignatureOptions();
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.SignAsync((Stream)null!, new MemoryStream(), options));
        }

        [Fact]
        public async Task SignAsync_Stream_NullOutput_Throws()
        {
            using var input = new MemoryStream(new byte[] { 1 });
            var options = new PdfSignatureOptions();
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.SignAsync(input, null!, options));
        }

        [Fact]
        public async Task SignAsync_Stream_NullOptions_Throws()
        {
            using var input = new MemoryStream(new byte[] { 1 });
            using var output = new MemoryStream();
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.SignAsync(input, output, (PdfSignatureOptions)null!));
        }

        [Fact]
        public async Task SignAsync_Stream_NullSigner_Throws()
        {
            using var input = new MemoryStream(new byte[] { 1 });
            using var output = new MemoryStream();
            var options = new PdfSignatureOptions { Signer = null! };
            await Assert.ThrowsAnyAsync<ArgumentException>(() =>
                _service.SignAsync(input, output, options));
        }

        // ── SignAsync(byte[], PdfSignatureOptions) validation ──

        [Fact]
        public async Task SignAsync_ByteArray_NullPdf_Throws()
        {
            var options = new PdfSignatureOptions();
            await Assert.ThrowsAnyAsync<ArgumentException>(() =>
                _service.SignAsync((byte[])null!, options));
        }

        [Fact]
        public async Task SignAsync_ByteArray_EmptyPdf_Throws()
        {
            var options = new PdfSignatureOptions();
            await Assert.ThrowsAnyAsync<ArgumentException>(() =>
                _service.SignAsync(Array.Empty<byte>(), options));
        }

        [Fact]
        public async Task SignAsync_ByteArray_NullOptions_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.SignAsync(new byte[] { 1 }, (PdfSignatureOptions)null!));
        }

        [Fact]
        public async Task SignAsync_ByteArray_NullSigner_Throws()
        {
            var options = new PdfSignatureOptions { Signer = null! };
            await Assert.ThrowsAnyAsync<ArgumentException>(() =>
                _service.SignAsync(new byte[] { 1 }, options));
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
