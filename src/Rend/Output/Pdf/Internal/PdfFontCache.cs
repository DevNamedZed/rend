using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rend.Fonts;
using Rend.Pdf;

namespace Rend.Output.Pdf.Internal
{
    internal sealed class PdfFontCache
    {
        private static readonly ConditionalWeakTable<byte[], PdfFontData> ParsedFontDataCache =
            new ConditionalWeakTable<byte[], PdfFontData>();

        private readonly Dictionary<FontDescriptor, PdfFont> _cache = new Dictionary<FontDescriptor, PdfFont>();
        private PdfFont? _fallbackFont;

        internal PdfFont GetOrAdd(FontDescriptor descriptor, byte[]? fontData,
                                  PdfDocument doc, FontEmbedMode embedMode)
        {
            if (embedMode == FontEmbedMode.None)
            {
                return GetFallbackFont(doc);
            }

            if (_cache.TryGetValue(descriptor, out var existing))
            {
                if (fontData != null && fontData.Length > 0 && existing.IsStandard14)
                {
                    var embedded = AddFontWithCache(fontData, doc, embedMode);
                    _cache[descriptor] = embedded;
                    return embedded;
                }
                return existing;
            }

            PdfFont pdfFont;
            if (fontData != null && fontData.Length > 0)
            {
                pdfFont = AddFontWithCache(fontData, doc, embedMode);
            }
            else
            {
                pdfFont = GetFallbackFont(doc);
            }

            _cache[descriptor] = pdfFont;
            return pdfFont;
        }

        private static PdfFont AddFontWithCache(byte[] fontData, PdfDocument doc,
                                                 FontEmbedMode embedMode)
        {
            var parsedData = ParsedFontDataCache.GetValue(fontData,
                bytes => PdfFontData.FromBytes(bytes, FontEmbedMode.Subset));
            return doc.AddFont(parsedData, embedMode);
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
