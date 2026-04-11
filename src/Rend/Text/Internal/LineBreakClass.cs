namespace Rend.Text.Internal
{
    /// <summary>
    /// Unicode line break classes as defined by UAX #14.
    /// </summary>
    internal enum LineBreakClass : byte
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
    internal static class LineBreakClassifier
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

            // [UAX #14] Open punctuation (OP)
            if (codePoint == 0x0028 || codePoint == 0x005B || codePoint == 0x007B)
                return LineBreakClass.OP;
            if (codePoint == 0x0F3A || codePoint == 0x0F3C) return LineBreakClass.OP; // Tibetan brackets
            if (codePoint == 0x169B) return LineBreakClass.OP; // Ogham feather mark
            if (codePoint == 0x201A || codePoint == 0x201E) return LineBreakClass.OP; // low-9 quotation marks
            if (codePoint == 0x2045) return LineBreakClass.OP; // left square bracket with quill
            if (codePoint == 0x207D) return LineBreakClass.OP; // superscript left parenthesis
            if (codePoint == 0x208D) return LineBreakClass.OP; // subscript left parenthesis
            if (codePoint == 0x2329) return LineBreakClass.OP; // left-pointing angle bracket
            if (codePoint == 0x2768 || codePoint == 0x276A || codePoint == 0x276C ||
                codePoint == 0x276E || codePoint == 0x2770 || codePoint == 0x2772 ||
                codePoint == 0x2774)
                return LineBreakClass.OP; // ornament left brackets
            if (codePoint == 0x27C5) return LineBreakClass.OP; // left s-shaped bag delimiter
            if (codePoint == 0x27E6 || codePoint == 0x27E8 || codePoint == 0x27EA ||
                codePoint == 0x27EC || codePoint == 0x27EE)
                return LineBreakClass.OP; // mathematical left brackets
            if (codePoint == 0x2983 || codePoint == 0x2985 || codePoint == 0x2987 ||
                codePoint == 0x2989 || codePoint == 0x298B || codePoint == 0x298D ||
                codePoint == 0x298F || codePoint == 0x2991 || codePoint == 0x2993 ||
                codePoint == 0x2995 || codePoint == 0x2997)
                return LineBreakClass.OP; // misc mathematical left brackets
            if (codePoint == 0x29D8 || codePoint == 0x29DA) return LineBreakClass.OP; // wiggly fences
            if (codePoint == 0x29FC) return LineBreakClass.OP; // left-pointing curved angle bracket
            if (codePoint == 0xFE35 || codePoint == 0xFE37 || codePoint == 0xFE39 ||
                codePoint == 0xFE3B || codePoint == 0xFE3D || codePoint == 0xFE3F ||
                codePoint == 0xFE41 || codePoint == 0xFE43 || codePoint == 0xFE47)
                return LineBreakClass.OP; // presentation forms vertical left brackets
            if (codePoint == 0xFE59 || codePoint == 0xFE5B || codePoint == 0xFE5D)
                return LineBreakClass.OP; // small left brackets
            if (codePoint == 0xFF5F || codePoint == 0xFF62) return LineBreakClass.OP; // fullwidth/halfwidth left brackets
            if (codePoint == 0x00AB) return LineBreakClass.QU; // left guillemet

            // [UAX #14] Close punctuation (CL)
            if (codePoint == 0x0F3B || codePoint == 0x0F3D) return LineBreakClass.CL; // Tibetan brackets
            if (codePoint == 0x169C) return LineBreakClass.CL; // Ogham reversed feather mark
            if (codePoint == 0x2046) return LineBreakClass.CL; // right square bracket with quill
            if (codePoint == 0x207E) return LineBreakClass.CL; // superscript right parenthesis
            if (codePoint == 0x208E) return LineBreakClass.CL; // subscript right parenthesis
            if (codePoint == 0x232A) return LineBreakClass.CL; // right-pointing angle bracket
            if (codePoint == 0x2769 || codePoint == 0x276B || codePoint == 0x276D ||
                codePoint == 0x276F || codePoint == 0x2771 || codePoint == 0x2773 ||
                codePoint == 0x2775)
                return LineBreakClass.CL; // ornament right brackets
            if (codePoint == 0x27C6) return LineBreakClass.CL; // right s-shaped bag delimiter
            if (codePoint == 0x27E7 || codePoint == 0x27E9 || codePoint == 0x27EB ||
                codePoint == 0x27ED || codePoint == 0x27EF)
                return LineBreakClass.CL; // mathematical right brackets
            if (codePoint == 0x2984 || codePoint == 0x2986 || codePoint == 0x2988 ||
                codePoint == 0x298A || codePoint == 0x298C || codePoint == 0x298E ||
                codePoint == 0x2990 || codePoint == 0x2992 || codePoint == 0x2994 ||
                codePoint == 0x2996 || codePoint == 0x2998)
                return LineBreakClass.CL; // misc mathematical right brackets
            if (codePoint == 0x29D9 || codePoint == 0x29DB) return LineBreakClass.CL; // wiggly fences
            if (codePoint == 0x29FD) return LineBreakClass.CL; // right-pointing curved angle bracket
            if (codePoint == 0xFE36 || codePoint == 0xFE38 || codePoint == 0xFE3A ||
                codePoint == 0xFE3C || codePoint == 0xFE3E || codePoint == 0xFE40 ||
                codePoint == 0xFE42 || codePoint == 0xFE44 || codePoint == 0xFE48)
                return LineBreakClass.CL; // presentation forms vertical right brackets
            if (codePoint == 0xFE50 || codePoint == 0xFE52) return LineBreakClass.CL; // small comma, small full stop
            if (codePoint == 0xFE5A || codePoint == 0xFE5C || codePoint == 0xFE5E)
                return LineBreakClass.CL; // small right brackets
            if (codePoint == 0xFF60 || codePoint == 0xFF63) return LineBreakClass.CL; // fullwidth/halfwidth right brackets
            if (codePoint == 0xFF61 || codePoint == 0xFF64) return LineBreakClass.CL; // halfwidth ideographic period/comma

            // [UAX #14] Close parenthesis (CP) — same break behaviour as CL for our rules
            if (codePoint == 0x0029 || codePoint == 0x005D || codePoint == 0x007D)
                return LineBreakClass.CP;
            if (codePoint == 0x00BB) return LineBreakClass.QU; // right guillemet

            // [UAX #14] Exclamation / interrogation (EX)
            if (codePoint == 0x0021 || codePoint == 0x003F || codePoint == 0x203C || codePoint == 0x2047 || codePoint == 0x2048 || codePoint == 0x2049)
                return LineBreakClass.EX;

            // [UAX #14] Infix separators (IS) — small punctuation
            if (codePoint == 0xFE54 || codePoint == 0xFE55) return LineBreakClass.IS_; // small semicolon, small colon

            // [UAX #14] Non-starters (NS) — characters that cannot start a line
            if (codePoint == 0x0E5A || codePoint == 0x0E5B) return LineBreakClass.NS; // Thai signs
            if (codePoint == 0x17D4 || codePoint == 0x17D6 || codePoint == 0x17DA) return LineBreakClass.NS; // Khmer signs
            if (codePoint == 0x303C) return LineBreakClass.NS; // masu mark (was incorrectly ID below)
            if (codePoint == 0x30A0) return LineBreakClass.NS; // katakana-hiragana double hyphen
            if (codePoint == 0xFF65) return LineBreakClass.NS; // halfwidth katakana middle dot
            if (codePoint == 0xFF9E || codePoint == 0xFF9F) return LineBreakClass.NS; // halfwidth katakana voiced/semi-voiced

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

            // CJK punctuation (U+3000-303F)
            if (codePoint >= 0x3001 && codePoint <= 0x3002) return LineBreakClass.CL; // ideographic comma/full stop
            if (codePoint >= 0x3003 && codePoint <= 0x3004) return LineBreakClass.ID; // ditto mark, JIS symbol
            if (codePoint == 0x3005) return LineBreakClass.NS; // ideographic iteration mark
            if (codePoint >= 0x3006 && codePoint <= 0x3007) return LineBreakClass.ID; // ideographic closing mark, number zero
            if (codePoint == 0x3008 || codePoint == 0x300A || codePoint == 0x300C || codePoint == 0x300E ||
                codePoint == 0x3010 || codePoint == 0x3014 || codePoint == 0x3016 || codePoint == 0x3018 ||
                codePoint == 0x301A || codePoint == 0xFF08 || codePoint == 0xFF3B || codePoint == 0xFF5B)
                return LineBreakClass.OP;
            if (codePoint == 0x3009 || codePoint == 0x300B || codePoint == 0x300D || codePoint == 0x300F ||
                codePoint == 0x3011 || codePoint == 0x3015 || codePoint == 0x3017 || codePoint == 0x3019 ||
                codePoint == 0x301B || codePoint == 0xFF09 || codePoint == 0xFF3D || codePoint == 0xFF5D)
                return LineBreakClass.CL;
            if (codePoint == 0x301C) return LineBreakClass.NS; // wave dash
            if (codePoint == 0x301D) return LineBreakClass.OP; // reversed double prime quotation
            if (codePoint >= 0x301E && codePoint <= 0x301F) return LineBreakClass.CL; // double prime quotation marks
            if (codePoint == 0x3030) return LineBreakClass.ID; // wavy dash
            if (codePoint == 0x303B) return LineBreakClass.NS; // vertical ideographic iteration mark
            // U+303C masu mark is NS — handled in the NS block above

            // Hiragana / Katakana → ID (ideographic, allows breaks around them)
            if (codePoint >= 0x3041 && codePoint <= 0x3096) return LineBreakClass.ID; // Hiragana
            if (codePoint >= 0x3099 && codePoint <= 0x309A) return LineBreakClass.CM; // combining marks
            if (codePoint >= 0x309B && codePoint <= 0x309E) return LineBreakClass.NS; // voiced/semi-voiced marks, iteration
            if (codePoint == 0x309F) return LineBreakClass.ID; // Hiragana digraph yori
            if (codePoint >= 0x30A1 && codePoint <= 0x30FA) return LineBreakClass.ID; // Katakana
            if (codePoint == 0x30FB) return LineBreakClass.NS; // katakana middle dot
            if (codePoint == 0x30FC) return LineBreakClass.NS; // prolonged sound mark (CJ→NS default)
            if (codePoint >= 0x30FD && codePoint <= 0x30FE) return LineBreakClass.NS; // katakana iteration marks
            if (codePoint == 0x30FF) return LineBreakClass.ID; // Katakana digraph koto

            // Fullwidth punctuation (U+FF00-FFEF)
            if (codePoint == 0xFF01) return LineBreakClass.EX; // fullwidth exclamation
            if (codePoint == 0xFF0C) return LineBreakClass.CL; // fullwidth comma
            if (codePoint == 0xFF0E) return LineBreakClass.CL; // fullwidth full stop
            if (codePoint == 0xFF1A) return LineBreakClass.NS; // fullwidth colon
            if (codePoint == 0xFF1B) return LineBreakClass.NS; // fullwidth semicolon
            if (codePoint == 0xFF1F) return LineBreakClass.EX; // fullwidth question mark
            // Fullwidth Latin letters/digits → ID in CJK context
            if (codePoint >= 0xFF10 && codePoint <= 0xFF19) return LineBreakClass.ID; // fullwidth digits
            if (codePoint >= 0xFF21 && codePoint <= 0xFF3A) return LineBreakClass.ID; // fullwidth uppercase
            if (codePoint >= 0xFF41 && codePoint <= 0xFF5A) return LineBreakClass.ID; // fullwidth lowercase

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
