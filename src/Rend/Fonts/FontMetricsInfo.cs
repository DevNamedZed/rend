namespace Rend.Fonts
{
    /// <summary>
    /// Contains typographic metrics parsed from an OpenType/TrueType font.
    /// </summary>
    public readonly struct FontMetricsInfo
    {
        /// <summary>Typographic ascent in font design units.</summary>
        public int Ascent { get; }

        /// <summary>Typographic descent in font design units (typically negative).</summary>
        public int Descent { get; }

        /// <summary>Typographic line gap in font design units.</summary>
        public int LineGap { get; }

        /// <summary>Units per em for the font (typically 1000 or 2048).</summary>
        public int UnitsPerEm { get; }

        /// <summary>Cap height in font design units.</summary>
        public int CapHeight { get; }

        /// <summary>x-height in font design units.</summary>
        public int XHeight { get; }

        /// <summary>
        /// Creates a new <see cref="FontMetricsInfo"/>.
        /// </summary>
        public FontMetricsInfo(int ascent, int descent, int lineGap, int unitsPerEm, int capHeight, int xHeight)
        {
            Ascent = ascent;
            Descent = descent;
            LineGap = lineGap;
            UnitsPerEm = unitsPerEm;
            CapHeight = capHeight;
            XHeight = xHeight;
        }

        /// <summary>
        /// Computes the line height in pixels for the given font size.
        /// </summary>
        public float GetLineHeight(float fontSize)
        {
            if (UnitsPerEm == 0) return fontSize;
            return fontSize * (Ascent - Descent + LineGap) / UnitsPerEm;
        }

        /// <summary>
        /// Computes the ascent in pixels for the given font size.
        /// </summary>
        public float GetAscent(float fontSize)
        {
            if (UnitsPerEm == 0) return fontSize;
            return fontSize * Ascent / UnitsPerEm;
        }

        /// <summary>
        /// Computes the descent in pixels for the given font size (returns a positive value).
        /// </summary>
        public float GetDescent(float fontSize)
        {
            if (UnitsPerEm == 0) return 0f;
            // Descent is typically negative, so negate to return a positive pixel value.
            return fontSize * -Descent / UnitsPerEm;
        }
    }
}
