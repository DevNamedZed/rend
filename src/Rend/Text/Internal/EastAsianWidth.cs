namespace Rend.Text.Internal
{
    /// <summary>
    /// [CSS-TEXT-3 §4.1.1] East Asian Width classification for segment break transformation.
    /// A segment break between two East Asian Fullwidth (F), Wide (W), or Halfwidth (H)
    /// characters is removed. Otherwise it collapses to a space.
    /// </summary>
    internal static class EastAsianWidth
    {
        /// <summary>
        /// Returns true if the character has East Asian Width property of
        /// Fullwidth (F), Wide (W), or Halfwidth (H) — the categories that
        /// trigger segment break removal per CSS Text Level 3.
        /// Ambiguous (A) and Neutral (N) do NOT trigger removal.
        /// </summary>
        public static bool IsEastAsianFWH(int codePoint)
        {
            // East Asian Fullwidth (F)
            if (codePoint >= 0xFF01 && codePoint <= 0xFF60) { return true; }
            if (codePoint >= 0xFFE0 && codePoint <= 0xFFE6) { return true; }

            // East Asian Halfwidth (H)
            if (codePoint >= 0xFF61 && codePoint <= 0xFFDC) { return true; }
            if (codePoint >= 0xFFE8 && codePoint <= 0xFFEE) { return true; }

            // East Asian Wide (W) — CJK ideographs, kana, hangul, etc.
            if (IsCjkWide(codePoint)) { return true; }

            return false;
        }

        /// <summary>
        /// Returns true for characters with East Asian Width = Wide (W).
        /// Covers CJK Unified Ideographs, Hiragana, Katakana, Hangul,
        /// CJK Compatibility, and related blocks.
        /// </summary>
        private static bool IsCjkWide(int codePoint)
        {
            // CJK Radicals Supplement + Kangxi Radicals
            if (codePoint >= 0x2E80 && codePoint <= 0x2FDF) { return true; }
            // Ideographic Description Characters
            if (codePoint >= 0x2FF0 && codePoint <= 0x2FFF) { return true; }
            // CJK Symbols and Punctuation (includes U+3000 ideographic space)
            if (codePoint >= 0x3000 && codePoint <= 0x303F) { return true; }
            // Hiragana
            if (codePoint >= 0x3040 && codePoint <= 0x309F) { return true; }
            // Katakana
            if (codePoint >= 0x30A0 && codePoint <= 0x30FF) { return true; }
            // Bopomofo
            if (codePoint >= 0x3100 && codePoint <= 0x312F) { return true; }
            // Hangul Compatibility Jamo
            if (codePoint >= 0x3130 && codePoint <= 0x318F) { return true; }
            // Kanbun + Bopomofo Extended
            if (codePoint >= 0x3190 && codePoint <= 0x31FF) { return true; }
            // Katakana Phonetic Extensions
            if (codePoint >= 0x31F0 && codePoint <= 0x31FF) { return true; }
            // Enclosed CJK Letters and Months
            if (codePoint >= 0x3200 && codePoint <= 0x32FF) { return true; }
            // CJK Compatibility
            if (codePoint >= 0x3300 && codePoint <= 0x33FF) { return true; }
            // CJK Unified Ideographs Extension A
            if (codePoint >= 0x3400 && codePoint <= 0x4DBF) { return true; }
            // CJK Unified Ideographs
            if (codePoint >= 0x4E00 && codePoint <= 0x9FFF) { return true; }
            // Yi Syllables + Yi Radicals
            if (codePoint >= 0xA000 && codePoint <= 0xA4CF) { return true; }
            // Hangul Syllables
            if (codePoint >= 0xAC00 && codePoint <= 0xD7AF) { return true; }
            // CJK Compatibility Ideographs
            if (codePoint >= 0xF900 && codePoint <= 0xFAFF) { return true; }
            // CJK Compatibility Forms
            if (codePoint >= 0xFE30 && codePoint <= 0xFE4F) { return true; }
            // Small Form Variants
            if (codePoint >= 0xFE50 && codePoint <= 0xFE6F) { return true; }
            // CJK Unified Ideographs Extension B and beyond (supplementary planes)
            if (codePoint >= 0x20000 && codePoint <= 0x2FA1F) { return true; }
            // Emoji/symbols that are wide per UAX #11
            if (codePoint >= 0x1F300 && codePoint <= 0x1F9FF) { return true; }

            return false;
        }
    }
}
