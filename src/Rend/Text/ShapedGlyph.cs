namespace Rend.Text
{
    /// <summary>
    /// Represents a single shaped glyph with its positioning information.
    /// </summary>
    public readonly struct ShapedGlyph
    {
        /// <summary>Gets the glyph identifier within the font.</summary>
        public uint GlyphId { get; }

        /// <summary>Gets the cluster index mapping this glyph back to the original text.</summary>
        public uint Cluster { get; }

        /// <summary>Gets the horizontal advance width in pixels.</summary>
        public float XAdvance { get; }

        /// <summary>Gets the vertical advance in pixels.</summary>
        public float YAdvance { get; }

        /// <summary>Gets the horizontal offset from the current pen position in pixels.</summary>
        public float XOffset { get; }

        /// <summary>Gets the vertical offset from the current pen position in pixels.</summary>
        public float YOffset { get; }

        /// <summary>
        /// Creates a new <see cref="ShapedGlyph"/>.
        /// </summary>
        public ShapedGlyph(uint glyphId, uint cluster, float xAdvance, float yAdvance, float xOffset, float yOffset)
        {
            GlyphId = glyphId;
            Cluster = cluster;
            XAdvance = xAdvance;
            YAdvance = yAdvance;
            XOffset = xOffset;
            YOffset = yOffset;
        }
    }
}
