namespace Rend.Text.Internal
{
    /// <summary>
    /// Unicode Bidirectional character classes as defined by UAX #9.
    /// </summary>
    public enum BidiClass : byte
    {
        /// <summary>Left-to-Right.</summary>
        L,
        /// <summary>Right-to-Left.</summary>
        R,
        /// <summary>Right-to-Left Arabic.</summary>
        AL,
        /// <summary>European Number.</summary>
        EN,
        /// <summary>European Number Separator.</summary>
        ES,
        /// <summary>European Number Terminator.</summary>
        ET,
        /// <summary>Arabic Number.</summary>
        AN,
        /// <summary>Common Number Separator.</summary>
        CS,
        /// <summary>Nonspacing Mark.</summary>
        NSM,
        /// <summary>Boundary Neutral.</summary>
        BN,
        /// <summary>Paragraph Separator.</summary>
        B,
        /// <summary>Segment Separator.</summary>
        S,
        /// <summary>Whitespace.</summary>
        WS,
        /// <summary>Other Neutrals.</summary>
        ON,
        /// <summary>Left-to-Right Embedding.</summary>
        LRE,
        /// <summary>Left-to-Right Override.</summary>
        LRO,
        /// <summary>Right-to-Left Embedding.</summary>
        RLE,
        /// <summary>Right-to-Left Override.</summary>
        RLO,
        /// <summary>Pop Directional Formatting.</summary>
        PDF,
        /// <summary>Left-to-Right Isolate.</summary>
        LRI,
        /// <summary>Right-to-Left Isolate.</summary>
        RLI,
        /// <summary>First Strong Isolate.</summary>
        FSI,
        /// <summary>Pop Directional Isolate.</summary>
        PDI
    }

    /// <summary>
    /// Provides simplified UAX #9 Bidi class lookups for common code point ranges.
    /// </summary>
    public static class BidiClassifier
    {
        /// <summary>
        /// Returns the bidirectional class for the given Unicode code point.
        /// This is a simplified lookup covering common ranges.
        /// </summary>
        /// <param name="codePoint">The Unicode code point.</param>
        /// <returns>The bidirectional class.</returns>
        public static BidiClass GetClass(int codePoint)
        {
            // Explicit formatting characters
            if (codePoint == 0x202A) return BidiClass.LRE;
            if (codePoint == 0x202B) return BidiClass.RLE;
            if (codePoint == 0x202C) return BidiClass.PDF;
            if (codePoint == 0x202D) return BidiClass.LRO;
            if (codePoint == 0x202E) return BidiClass.RLO;
            if (codePoint == 0x2066) return BidiClass.LRI;
            if (codePoint == 0x2067) return BidiClass.RLI;
            if (codePoint == 0x2068) return BidiClass.FSI;
            if (codePoint == 0x2069) return BidiClass.PDI;

            // Paragraph separators
            if (codePoint == 0x000A || codePoint == 0x000D || codePoint == 0x001C ||
                codePoint == 0x001D || codePoint == 0x001E || codePoint == 0x0085 ||
                codePoint == 0x2029)
                return BidiClass.B;

            // Segment separators
            if (codePoint == 0x0009 || codePoint == 0x001F)
                return BidiClass.S;

            // Whitespace
            if (codePoint == 0x000C || codePoint == 0x0020 || codePoint == 0x00A0 ||
                codePoint == 0x1680 || (codePoint >= 0x2000 && codePoint <= 0x200A) ||
                codePoint == 0x2028 || codePoint == 0x202F || codePoint == 0x205F ||
                codePoint == 0x3000)
                return BidiClass.WS;

            // Boundary neutral: zero-width characters and formatting
            if (codePoint == 0x200B || codePoint == 0x200C || codePoint == 0x200D ||
                codePoint == 0x2060 || codePoint == 0xFEFF ||
                (codePoint >= 0x0000 && codePoint <= 0x0008) ||
                codePoint == 0x000E || codePoint == 0x000F ||
                (codePoint >= 0x0010 && codePoint <= 0x001B))
                return BidiClass.BN;

            // European numbers (digits)
            if (codePoint >= 0x0030 && codePoint <= 0x0039)
                return BidiClass.EN;
            if (codePoint >= 0x06F0 && codePoint <= 0x06F9)
                return BidiClass.EN; // Extended Arabic-Indic digits

            // Arabic-Indic digits
            if (codePoint >= 0x0660 && codePoint <= 0x0669)
                return BidiClass.AN;

            // European number separators
            if (codePoint == 0x002B || codePoint == 0x002D)
                return BidiClass.ES;

            // European number terminators
            if (codePoint == 0x0023 || codePoint == 0x0024 || codePoint == 0x0025 ||
                codePoint == 0x00A2 || codePoint == 0x00A3 || codePoint == 0x00A4 ||
                codePoint == 0x00A5 || codePoint == 0x00B0 || codePoint == 0x00B1 ||
                codePoint == 0x20AC || codePoint == 0x2030 || codePoint == 0x2031 ||
                codePoint == 0x2032 || codePoint == 0x2033)
                return BidiClass.ET;

            // Common separators
            if (codePoint == 0x002C || codePoint == 0x002E || codePoint == 0x002F ||
                codePoint == 0x003A || codePoint == 0x00A0)
                return BidiClass.CS;

            // Nonspacing marks (combining diacriticals - simplified)
            if ((codePoint >= 0x0300 && codePoint <= 0x036F) ||
                (codePoint >= 0x0483 && codePoint <= 0x0489) ||
                (codePoint >= 0x0591 && codePoint <= 0x05BD) ||
                codePoint == 0x05BF ||
                (codePoint >= 0x05C1 && codePoint <= 0x05C2) ||
                (codePoint >= 0x05C4 && codePoint <= 0x05C5) ||
                codePoint == 0x05C7 ||
                (codePoint >= 0x0610 && codePoint <= 0x061A) ||
                (codePoint >= 0x064B && codePoint <= 0x065F) ||
                codePoint == 0x0670 ||
                (codePoint >= 0x06D6 && codePoint <= 0x06DC) ||
                (codePoint >= 0x06DF && codePoint <= 0x06E4) ||
                (codePoint >= 0x06E7 && codePoint <= 0x06E8) ||
                (codePoint >= 0x06EA && codePoint <= 0x06ED) ||
                (codePoint >= 0x0900 && codePoint <= 0x0903) ||
                (codePoint >= 0xFE00 && codePoint <= 0xFE0F) ||
                (codePoint >= 0x20D0 && codePoint <= 0x20FF))
                return BidiClass.NSM;

            // Hebrew (Right-to-Left)
            if ((codePoint >= 0x0590 && codePoint <= 0x05FF) ||
                (codePoint >= 0xFB1D && codePoint <= 0xFB4F))
                return BidiClass.R;

            // Arabic (Right-to-Left Arabic)
            if ((codePoint >= 0x0600 && codePoint <= 0x06FF) ||
                (codePoint >= 0x0750 && codePoint <= 0x077F) ||
                (codePoint >= 0x0800 && codePoint <= 0x08FF) ||
                (codePoint >= 0xFB50 && codePoint <= 0xFDFF) ||
                (codePoint >= 0xFE70 && codePoint <= 0xFEFF))
                return BidiClass.AL;

            // Syriac, Thaana, N'Ko, etc. (Right-to-Left)
            if ((codePoint >= 0x0700 && codePoint <= 0x074F) ||
                (codePoint >= 0x0780 && codePoint <= 0x07BF) ||
                (codePoint >= 0x07C0 && codePoint <= 0x07FF))
                return BidiClass.R;

            // Latin, Greek, Cyrillic, CJK, etc. (Left-to-Right)
            if ((codePoint >= 0x0041 && codePoint <= 0x005A) ||
                (codePoint >= 0x0061 && codePoint <= 0x007A) ||
                (codePoint >= 0x00C0 && codePoint <= 0x024F) ||
                (codePoint >= 0x0250 && codePoint <= 0x02AF) ||
                (codePoint >= 0x0370 && codePoint <= 0x03FF) ||
                (codePoint >= 0x0400 && codePoint <= 0x052F) ||
                (codePoint >= 0x1E00 && codePoint <= 0x1EFF) ||
                (codePoint >= 0x2C00 && codePoint <= 0x2C5F) ||
                (codePoint >= 0x3000 && codePoint <= 0x9FFF) ||
                (codePoint >= 0xAC00 && codePoint <= 0xD7AF) ||
                (codePoint >= 0xF900 && codePoint <= 0xFAFF))
                return BidiClass.L;

            // Devanagari, Bengali, Thai, Georgian, etc. (Left-to-Right)
            if ((codePoint >= 0x0900 && codePoint <= 0x0DFF) ||
                (codePoint >= 0x0E00 && codePoint <= 0x0E7F) ||
                (codePoint >= 0x0E80 && codePoint <= 0x0EFF) ||
                (codePoint >= 0x10A0 && codePoint <= 0x10FF) ||
                (codePoint >= 0x1100 && codePoint <= 0x11FF) ||
                (codePoint >= 0x1780 && codePoint <= 0x17FF))
                return BidiClass.L;

            // Common punctuation - Other Neutrals
            if ((codePoint >= 0x0021 && codePoint <= 0x0040) ||
                (codePoint >= 0x005B && codePoint <= 0x0060) ||
                (codePoint >= 0x007B && codePoint <= 0x007E) ||
                (codePoint >= 0x00A1 && codePoint <= 0x00BF) ||
                (codePoint >= 0x2010 && codePoint <= 0x2027) ||
                (codePoint >= 0x2030 && codePoint <= 0x205E))
                return BidiClass.ON;

            // Default to Left-to-Right for unknown code points.
            return BidiClass.L;
        }
    }
}
