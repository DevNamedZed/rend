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
                // If we have real font data but the cached entry is a Standard14 fallback,
                // replace it with the embedded font. This handles the case where DrawText
                // (no font data) cached a fallback before DrawGlyphs (with font data) runs.
                if (fontData != null && fontData.Length > 0 && existing.IsStandard14)
                {
                    var embedded = doc.AddFont(fontData, FontEmbedMode.Subset);
                    _cache[descriptor] = embedded;
                    return embedded;
                }
                return existing;
            }

            PdfFont pdfFont;
            if (fontData != null && fontData.Length > 0)
            {
                pdfFont = doc.AddFont(fontData, FontEmbedMode.Subset);
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
