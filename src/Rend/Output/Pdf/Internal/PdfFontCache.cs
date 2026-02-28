using System.Collections.Generic;
using System.IO;
using Rend.Fonts;
using Rend.Pdf;

namespace Rend.Output.Pdf.Internal
{
    /// <summary>
    /// Caches PDF font instances by font descriptor to avoid re-adding the same font
    /// to a PDF document multiple times.
    /// </summary>
    internal sealed class PdfFontCache
    {
        private readonly Dictionary<FontDescriptor, PdfFont> _cache = new Dictionary<FontDescriptor, PdfFont>();
        private PdfFont? _fallbackFont;

        /// <summary>
        /// Gets an existing cached font or adds a new one to the PDF document.
        /// Falls back to Helvetica if no font data is available.
        /// </summary>
        /// <param name="descriptor">The font descriptor to look up.</param>
        /// <param name="fontData">Raw font data bytes, or null to use fallback.</param>
        /// <param name="doc">The PDF document to add the font to.</param>
        /// <returns>A PDF font suitable for use in content streams.</returns>
        internal PdfFont GetOrAdd(FontDescriptor descriptor, byte[]? fontData, PdfDocument doc)
        {
            if (_cache.TryGetValue(descriptor, out var existing))
            {
                return existing;
            }

            PdfFont pdfFont;
            if (fontData != null && fontData.Length > 0)
            {
                using (var stream = new MemoryStream(fontData))
                {
                    pdfFont = doc.AddFont(stream, FontEmbedMode.Subset);
                }
            }
            else
            {
                pdfFont = GetFallbackFont(doc);
            }

            _cache[descriptor] = pdfFont;
            return pdfFont;
        }

        private PdfFont GetFallbackFont(PdfDocument doc)
        {
            if (_fallbackFont == null)
            {
                _fallbackFont = doc.GetStandardFont(StandardFont.Helvetica);
            }
            return _fallbackFont;
        }
    }
}
