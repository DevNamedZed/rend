using System;

namespace Rend.Fonts
{
    /// <summary>
    /// Represents a registered font with its descriptor, raw data, and metrics.
    /// </summary>
    public sealed class FontEntry
    {
        /// <summary>Gets the font descriptor identifying this font.</summary>
        public FontDescriptor Descriptor { get; }

        /// <summary>Gets the raw font data bytes.</summary>
        public byte[] FontData { get; }

        /// <summary>Gets the parsed font metrics.</summary>
        public FontMetricsInfo Metrics { get; }

        /// <summary>Gets the font family name.</summary>
        public string FamilyName { get; }

        /// <summary>
        /// Gets the per-glyph advance widths in font design units, or null if not available.
        /// Indexed by glyph ID.
        /// </summary>
        public float[]? GlyphWidths { get; }

        private readonly Internal.OpenTypeFontData? _fontData;

        /// <summary>
        /// Creates a new <see cref="FontEntry"/>.
        /// </summary>
        public FontEntry(FontDescriptor descriptor, byte[] fontData, FontMetricsInfo metrics, string familyName, float[]? glyphWidths)
        {
            Descriptor = descriptor;
            FontData = fontData ?? throw new ArgumentNullException(nameof(fontData));
            Metrics = metrics;
            FamilyName = familyName ?? throw new ArgumentNullException(nameof(familyName));
            GlyphWidths = glyphWidths;
        }

        /// <summary>
        /// Creates a new <see cref="FontEntry"/> backed by parsed OpenType data for glyph lookups.
        /// </summary>
        internal FontEntry(FontDescriptor descriptor, byte[] fontData, FontMetricsInfo metrics, string familyName, float[]? glyphWidths, Internal.OpenTypeFontData parsedData)
            : this(descriptor, fontData, metrics, familyName, glyphWidths)
        {
            _fontData = parsedData;
        }

        /// <summary>
        /// Returns the advance width of the given code point at the specified font size, in pixels.
        /// Falls back to an average width estimate if the glyph is not found.
        /// </summary>
        public float GetCharWidth(int codePoint, float fontSize)
        {
            if (Metrics.UnitsPerEm == 0)
                return fontSize * 0.5f;

            // Try parsed OpenType data first for accurate glyph mapping.
            if (_fontData != null)
            {
                int glyphId = _fontData.GetGlyphId(codePoint);
                if (glyphId > 0)
                {
                    int advanceWidth = _fontData.GetAdvanceWidth(glyphId);
                    return fontSize * advanceWidth / Metrics.UnitsPerEm;
                }
            }

            // Fall back to GlyphWidths array if available.
            if (GlyphWidths != null && codePoint >= 0 && codePoint < GlyphWidths.Length)
            {
                float width = GlyphWidths[codePoint];
                if (width > 0)
                    return fontSize * width / Metrics.UnitsPerEm;
            }

            // Default: estimate as half an em.
            return fontSize * 0.5f;
        }
    }
}
