namespace Rend.Text.Internal
{
    /// <summary>
    /// Unicode line break classes as defined by UAX #14.
    /// </summary>
    public enum LineBreakClass : byte
    {
        /// <summary>Mandatory Break (BK).</summary>
        BK,
        /// <summary>Carriage Return (CR).</summary>
        CR,
        /// <summary>Line Feed (LF).</summary>
        LF,
        /// <summary>Combining Mark (CM).</summary>
        CM,
        /// <summary>Surrogates (SG).</summary>
        SG,
        /// <summary>Non-breaking ("Glue") (GL).</summary>
        GL,
        /// <summary>Contingent Break Opportunity (CB).</summary>
        CB,
        /// <summary>Space (SP).</summary>
        SP,
        /// <summary>Zero Width Space (ZW).</summary>
        ZW,
        /// <summary>Next Line (NL).</summary>
        NL,
        /// <summary>Word Joiner (WJ).</summary>
        WJ,
        /// <summary>Hangul L Jamo (JL).</summary>
        JL,
        /// <summary>Hangul V Jamo (JV).</summary>
        JV,
        /// <summary>Hangul T Jamo (JT).</summary>
        JT,
        /// <summary>Hangul LV Syllable (H2).</summary>
        H2,
        /// <summary>Hangul LVT Syllable (H3).</summary>
        H3,
        /// <summary>Unknown (XX).</summary>
        XX,
        /// <summary>Open Punctuation (OP).</summary>
        OP,
        /// <summary>Close Punctuation (CL).</summary>
        CL,
        /// <summary>Close Parenthesis (CP).</summary>
        CP,
        /// <summary>Quotation (QU).</summary>
        QU,
        /// <summary>Nonstarter (NS).</summary>
        NS,
        /// <summary>Exclamation/Interrogation (EX).</summary>
        EX,
        /// <summary>Symbols Allowing Break After (SY).</summary>
        SY,
        /// <summary>Infix Numeric Separator (IS).</summary>
        IS_,
        /// <summary>Prefix Numeric (PR).</summary>
        PR,
        /// <summary>Postfix Numeric (PO).</summary>
        PO,
        /// <summary>Numeric (NU).</summary>
        NU,
        /// <summary>Alphabetic (AL).</summary>
        AL,
        /// <summary>Ideographic (ID).</summary>
        ID,
        /// <summary>Inseparable Characters (IN).</summary>
        IN_,
        /// <summary>Hyphen (HY).</summary>
        HY,
        /// <summary>Break After (BA).</summary>
        BA,
        /// <summary>Break Before (BB).</summary>
        BB,
        /// <summary>Break Opportunity Before and After (B2).</summary>
        B2,
        /// <summary>Zero Width Joiner (ZWJ).</summary>
        ZWJ,
        /// <summary>Emoji Base (EB).</summary>
        EB,
        /// <summary>Emoji Modifier (EM).</summary>
        EM,
        /// <summary>Ambiguous (Alphabetic or Ideographic) (AI).</summary>
        AI,
        /// <summary>Conditional Japanese Starter (CJ) / BK2.</summary>
        BK2,
        /// <summary>Regional Indicator (RI).</summary>
        RI
    }

    /// <summary>
    /// Provides simplified UAX #14 line break class lookups for common code point ranges.
    /// </summary>
    public static class LineBreakClassifier
    {
        /// <summary>
        /// Returns the line break class for the given Unicode code point.
        /// This is a simplified lookup covering common ranges.
        /// </summary>
        /// <param name="codePoint">The Unicode code point.</param>
        /// <returns>The line break class.</returns>
        public static LineBreakClass GetClass(int codePoint)
        {
            // Mandatory breaks
            if (codePoint == 0x000A) return LineBreakClass.LF;
            if (codePoint == 0x000D) return LineBreakClass.CR;
            if (codePoint == 0x000B || codePoint == 0x000C) return LineBreakClass.BK;
            if (codePoint == 0x0085) return LineBreakClass.NL;
            if (codePoint == 0x2028 || codePoint == 0x2029) return LineBreakClass.BK;

            // Spaces
            if (codePoint == 0x0020) return LineBreakClass.SP;
            if (codePoint == 0x00A0) return LineBreakClass.GL; // non-breaking space
            if (codePoint == 0x1680 || (codePoint >= 0x2000 && codePoint <= 0x200A) || codePoint == 0x205F || codePoint == 0x3000)
                return LineBreakClass.SP;

            // Zero-width
            if (codePoint == 0x200B) return LineBreakClass.ZW;
            if (codePoint == 0x200D) return LineBreakClass.ZWJ;
            if (codePoint == 0x2060) return LineBreakClass.WJ;
            if (codePoint == 0xFEFF) return LineBreakClass.WJ; // BOM / zero-width no-break space

            // Tab
            if (codePoint == 0x0009) return LineBreakClass.BA;

            // Hyphens
            if (codePoint == 0x002D) return LineBreakClass.HY; // hyphen-minus
            if (codePoint == 0x2010 || codePoint == 0x2013) return LineBreakClass.BA; // hyphen, en dash
            if (codePoint == 0x00AD) return LineBreakClass.BA; // soft hyphen

            // Open punctuation
            if (codePoint == 0x0028 || codePoint == 0x005B || codePoint == 0x007B)
                return LineBreakClass.OP;
            if (codePoint == 0x00AB) return LineBreakClass.QU; // left guillemet

            // Close punctuation
            if (codePoint == 0x0029 || codePoint == 0x005D || codePoint == 0x007D)
                return LineBreakClass.CP;
            if (codePoint == 0x00BB) return LineBreakClass.QU; // right guillemet

            // Exclamation / question
            if (codePoint == 0x0021 || codePoint == 0x003F || codePoint == 0x203C || codePoint == 0x2047 || codePoint == 0x2048 || codePoint == 0x2049)
                return LineBreakClass.EX;

            // Quotation marks
            if (codePoint == 0x0022 || codePoint == 0x0027 || codePoint == 0x2018 || codePoint == 0x2019 ||
                codePoint == 0x201C || codePoint == 0x201D)
                return LineBreakClass.QU;

            // Numeric
            if (codePoint >= 0x0030 && codePoint <= 0x0039) return LineBreakClass.NU;

            // Symbols
            if (codePoint == 0x002F) return LineBreakClass.SY; // solidus
            if (codePoint == 0x002C || codePoint == 0x002E || codePoint == 0x003A || codePoint == 0x003B)
                return LineBreakClass.IS_;

            // CJK Ideographic ranges
            if ((codePoint >= 0x3400 && codePoint <= 0x4DBF) ||   // CJK Unified Ext A
                (codePoint >= 0x4E00 && codePoint <= 0x9FFF) ||   // CJK Unified
                (codePoint >= 0xF900 && codePoint <= 0xFAFF) ||   // CJK Compatibility
                (codePoint >= 0x20000 && codePoint <= 0x2FA1F))   // CJK Unified Ext B-F, Compatibility Supplement
                return LineBreakClass.ID;

            // CJK punctuation
            if (codePoint >= 0x3001 && codePoint <= 0x3002) return LineBreakClass.CL; // ideographic comma/full stop
            if (codePoint == 0x3008 || codePoint == 0x300A || codePoint == 0x300C || codePoint == 0x300E ||
                codePoint == 0x3010 || codePoint == 0x3014 || codePoint == 0x3016 || codePoint == 0x3018 ||
                codePoint == 0x301A || codePoint == 0xFF08 || codePoint == 0xFF3B || codePoint == 0xFF5B)
                return LineBreakClass.OP;
            if (codePoint == 0x3009 || codePoint == 0x300B || codePoint == 0x300D || codePoint == 0x300F ||
                codePoint == 0x3011 || codePoint == 0x3015 || codePoint == 0x3017 || codePoint == 0x3019 ||
                codePoint == 0x301B || codePoint == 0xFF09 || codePoint == 0xFF3D || codePoint == 0xFF5D)
                return LineBreakClass.CL;

            // Hangul Jamo
            if (codePoint >= 0x1100 && codePoint <= 0x115F) return LineBreakClass.JL;
            if (codePoint >= 0x1160 && codePoint <= 0x11A7) return LineBreakClass.JV;
            if (codePoint >= 0x11A8 && codePoint <= 0x11FF) return LineBreakClass.JT;
            if (codePoint >= 0xAC00 && codePoint <= 0xD7A3)
            {
                // Hangul syllables: LV or LVT
                int syllableIndex = codePoint - 0xAC00;
                return (syllableIndex % 28 == 0) ? LineBreakClass.H2 : LineBreakClass.H3;
            }

            // Surrogates
            if (codePoint >= 0xD800 && codePoint <= 0xDFFF) return LineBreakClass.SG;

            // Combining marks (general category Mn, Mc ranges - simplified)
            if ((codePoint >= 0x0300 && codePoint <= 0x036F) || // Combining Diacritical Marks
                (codePoint >= 0x0483 && codePoint <= 0x0489) ||
                (codePoint >= 0x0591 && codePoint <= 0x05BD) ||
                (codePoint >= 0x0610 && codePoint <= 0x061A) ||
                (codePoint >= 0x064B && codePoint <= 0x065F) ||
                (codePoint >= 0x0816 && codePoint <= 0x0819) ||
                (codePoint >= 0x0900 && codePoint <= 0x0903) ||
                (codePoint >= 0xFE00 && codePoint <= 0xFE0F) ||  // Variation selectors
                (codePoint >= 0x20D0 && codePoint <= 0x20FF))    // Combining Diacritical Marks for Symbols
                return LineBreakClass.CM;

            // Regional indicator symbols
            if (codePoint >= 0x1F1E6 && codePoint <= 0x1F1FF) return LineBreakClass.RI;

            // Latin, Cyrillic, Greek, and other alphabetic ranges
            if ((codePoint >= 0x0041 && codePoint <= 0x005A) || // A-Z
                (codePoint >= 0x0061 && codePoint <= 0x007A) || // a-z
                (codePoint >= 0x00C0 && codePoint <= 0x02AF) || // Latin Extended
                (codePoint >= 0x0370 && codePoint <= 0x03FF) || // Greek
                (codePoint >= 0x0400 && codePoint <= 0x04FF) || // Cyrillic
                (codePoint >= 0x0500 && codePoint <= 0x052F) || // Cyrillic Supplement
                (codePoint >= 0x1E00 && codePoint <= 0x1EFF) || // Latin Extended Additional
                (codePoint >= 0xFB00 && codePoint <= 0xFB06))   // Alphabetic Presentation Forms
                return LineBreakClass.AL;

            // Arabic, Hebrew - alphabetic
            if ((codePoint >= 0x0590 && codePoint <= 0x05FF) || // Hebrew
                (codePoint >= 0x0600 && codePoint <= 0x06FF) || // Arabic
                (codePoint >= 0x0750 && codePoint <= 0x077F) || // Arabic Supplement
                (codePoint >= 0x0800 && codePoint <= 0x083F))   // Samaritan, Mandaic
                return LineBreakClass.AL;

            // Default: treat as alphabetic
            return LineBreakClass.AL;
        }
    }
}
