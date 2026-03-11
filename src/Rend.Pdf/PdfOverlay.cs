using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rend.Pdf
{
    /// <summary>
    /// Default implementation of <see cref="IPdfOverlay"/>.
    /// Uses PDF incremental updates with standard PDF fonts (no embedding required).
    /// </summary>
    public sealed class PdfOverlay : IPdfOverlay
    {
        /// <inheritdoc />
        public async Task ApplyAsync(Stream input, Stream output, IEnumerable<PdfOverlayElement> elements, CancellationToken cancellationToken = default)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (elements == null) throw new ArgumentNullException(nameof(elements));

            byte[] pdfBytes;
            using (var ms = new MemoryStream())
            {
                await input.CopyToAsync(ms, 81920, cancellationToken).ConfigureAwait(false);
                pdfBytes = ms.ToArray();
            }

            cancellationToken.ThrowIfCancellationRequested();
            var result = Internal.PdfOverlayWriter.Apply(pdfBytes, elements);
            await output.WriteAsync(result, 0, result.Length, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<byte[]> ApplyAsync(byte[] pdfBytes, IEnumerable<PdfOverlayElement> elements, CancellationToken cancellationToken = default)
        {
            if (pdfBytes == null) throw new ArgumentNullException(nameof(pdfBytes));
            if (pdfBytes.Length == 0) throw new ArgumentException("PDF data must not be empty.", nameof(pdfBytes));
            if (elements == null) throw new ArgumentNullException(nameof(elements));

            cancellationToken.ThrowIfCancellationRequested();
            var result = Internal.PdfOverlayWriter.Apply(pdfBytes, elements);
            return Task.FromResult(result);
        }
    }
}
