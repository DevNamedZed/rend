namespace Rend.Pdf
{
    /// <summary>
    /// A glyph ID paired with an x-advance adjustment for glyph-level text positioning.
    /// Used with <see cref="PdfContentStream.ShowGlyphsWithPositioning"/>.
    /// </summary>
    public readonly struct GlyphPosition
    {
        /// <summary>The glyph ID.</summary>
        public ushort GlyphId { get; }

        /// <summary>
        /// X-advance adjustment in thousandths of a unit of text space.
        /// Negative values move the next glyph to the right (tighten).
        /// Zero means use the default advance width.
        /// </summary>
        public float XAdvanceAdjustment { get; }

        public GlyphPosition(ushort glyphId, float xAdvanceAdjustment = 0)
        {
            GlyphId = glyphId;
            XAdvanceAdjustment = xAdvanceAdjustment;
        }
    }
}
