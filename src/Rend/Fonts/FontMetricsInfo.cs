using System;

namespace Rend.Fonts
{
    /// <summary>
    /// Contains typographic metrics parsed from an OpenType/TrueType font.
    /// </summary>
    public readonly struct FontMetricsInfo
    {
        /// <summary>Typographic ascent in font design units (hhea ascender).</summary>
        public int Ascent { get; }

        /// <summary>Typographic descent in font design units (hhea descender, typically negative).</summary>
        public int Descent { get; }

        /// <summary>Typographic line gap in font design units (hhea lineGap).</summary>
        public int LineGap { get; }

        /// <summary>Units per em for the font (typically 1000 or 2048).</summary>
        public int UnitsPerEm { get; }

        /// <summary>Cap height in font design units.</summary>
        public int CapHeight { get; }

        /// <summary>x-height in font design units.</summary>
        public int XHeight { get; }

        /// <summary>OS/2 usWinAscent in font design units (positive).</summary>
        public int WinAscent { get; }

        /// <summary>OS/2 usWinDescent in font design units (positive).</summary>
        public int WinDescent { get; }

        /// <summary>
        /// Creates a new <see cref="FontMetricsInfo"/>.
        /// </summary>
        public FontMetricsInfo(int ascent, int descent, int lineGap, int unitsPerEm, int capHeight, int xHeight,
            int winAscent = 0, int winDescent = 0)
        {
            Ascent = ascent;
            Descent = descent;
            LineGap = lineGap;
            UnitsPerEm = unitsPerEm;
            CapHeight = capHeight;
            XHeight = xHeight;
            WinAscent = winAscent;
            WinDescent = winDescent;
        }

        /// <summary>
        /// Computes the line height in pixels for the given font size.
        /// Chrome/DirectWrite uses hhea ascent + |hhea descent| + hhea lineGap,
        /// with each component rounded individually (lroundf) before summing.
        /// </summary>
        public float GetLineHeight(float fontSize)
        {
            if (UnitsPerEm == 0) return fontSize;

            // DirectWrite's DWRITE_FONT_METRICS uses hhea metrics.
            // Chrome computes: round(ascent) + round(|descent|) + round(lineGap)
            float a = (float)Math.Round((double)fontSize * Ascent / UnitsPerEm, MidpointRounding.AwayFromZero);
            float d = (float)Math.Round((double)fontSize * -Descent / UnitsPerEm, MidpointRounding.AwayFromZero);
            float lg = LineGap > 0 ? (float)Math.Round((double)fontSize * LineGap / UnitsPerEm, MidpointRounding.AwayFromZero) : 0;
            float lh = a + d + lg;
            return lh > 0 ? lh : fontSize;
        }

        /// <summary>
        /// Computes the ascent in pixels for the given font size.
        /// Uses hhea ascent to match Chrome/DirectWrite.
        /// </summary>
        public float GetAscent(float fontSize)
        {
            if (UnitsPerEm == 0) return fontSize;
            return (float)Math.Round((double)fontSize * Ascent / UnitsPerEm, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Computes the descent in pixels for the given font size (returns a positive value).
        /// Uses hhea descent to match Chrome/DirectWrite.
        /// </summary>
        public float GetDescent(float fontSize)
        {
            if (UnitsPerEm == 0) return 0f;
            // hhea Descent is typically negative, so negate to return a positive pixel value.
            return (float)Math.Round((double)fontSize * -Descent / UnitsPerEm, MidpointRounding.AwayFromZero);
        }
    }
}
