using System;

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
        /// Uses OS/2 Win metrics with per-component rounding to match Chrome/Windows:
        /// lineHeight = round(winAscent_px) + round(winDescent_px) + round(lineGap_px)
        /// </summary>
        public float GetLineHeight(float fontSize)
        {
            if (UnitsPerEm == 0) return fontSize;

            // Chrome on Windows (DirectWrite) uses WinAscent + WinDescent + hhea lineGap,
            // with each component rounded individually before summing.
            if (WinAscent > 0 && WinDescent > 0)
            {
                float a = (float)Math.Round(fontSize * WinAscent / UnitsPerEm);
                float d = (float)Math.Round(fontSize * WinDescent / UnitsPerEm);
                float lg = LineGap > 0 ? (float)Math.Round(fontSize * LineGap / UnitsPerEm) : 0;
                float lh = a + d + lg;
                return lh > 0 ? lh : fontSize;
            }

            float fallback = fontSize * (Ascent - Descent + LineGap) / UnitsPerEm;
            return fallback > 0 ? fallback : fontSize;
        }

        /// <summary>
        /// Computes the ascent in pixels for the given font size.
        /// Uses OS/2 WinAscent when available to match Chrome/Windows.
        /// </summary>
        public float GetAscent(float fontSize)
        {
            if (UnitsPerEm == 0) return fontSize;

            // Chrome rounds ascent to integer (lroundf) before using in half-leading.
            if (WinAscent > 0)
                return (float)Math.Round(fontSize * WinAscent / UnitsPerEm);

            return (float)Math.Round(fontSize * Ascent / UnitsPerEm);
        }

        /// <summary>
        /// Computes the descent in pixels for the given font size (returns a positive value).
        /// Uses OS/2 WinDescent when available to match Chrome/Windows.
        /// </summary>
        public float GetDescent(float fontSize)
        {
            if (UnitsPerEm == 0) return 0f;

            // Chrome rounds descent to integer (lroundf) before using in half-leading.
            if (WinDescent > 0)
                return (float)Math.Round(fontSize * WinDescent / UnitsPerEm);

            // hhea Descent is typically negative, so negate to return a positive pixel value.
            return (float)Math.Round(fontSize * -Descent / UnitsPerEm);
        }
    }
}
