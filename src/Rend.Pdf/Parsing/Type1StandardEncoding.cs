#nullable enable
using System.Collections.Generic;
using System.Globalization;

namespace Rend.Pdf.Parsing
{
    /// <summary>
    /// Adobe StandardEncoding (code → glyph name) plus a default glyph-name → Unicode
    /// code point mapping. StandardEncoding is required by the <c>seac</c> operator,
    /// which references its base and accent components by StandardEncoding code. The
    /// name → code-point map is the fallback used when the caller does not inject the
    /// renderer's own glyph-name table.
    /// [SPEC] Adobe Type 1 Font Format (1990), Appendix C (StandardEncoding).
    /// </summary>
    internal static class Type1StandardEncoding
    {
        private static readonly string?[] Names = BuildStandardEncoding();
        private static readonly Dictionary<string, int> NameToCodePoint = BuildNameToCodePoint();

        public static string? GetName(int code)
        {
            return code >= 0 && code < 256 ? Names[code] : null;
        }

        /// <summary>
        /// Default glyph-name → Unicode code point, or -1 when unknown. Handles the
        /// StandardEncoding Latin/punctuation names and the AGL <c>uniXXXX</c> / <c>uXXXXXX</c> rules.
        /// </summary>
        public static int GlyphNameToCodePoint(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return -1;
            }
            if (NameToCodePoint.TryGetValue(name, out int codePoint))
            {
                return codePoint;
            }
            if (name.Length == 7 && name[0] == 'u' && name[1] == 'n' && name[2] == 'i' &&
                int.TryParse(name.Substring(3), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int uni))
            {
                return uni;
            }
            if (name.Length >= 5 && name.Length <= 7 && name[0] == 'u' &&
                int.TryParse(name.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int u))
            {
                return u;
            }
            return -1;
        }

        private static string?[] BuildStandardEncoding()
        {
            var names = new string?[256];
            void Set(int code, string name) { names[code] = name; }

            Set(32, "space"); Set(33, "exclam"); Set(34, "quotedbl"); Set(35, "numbersign");
            Set(36, "dollar"); Set(37, "percent"); Set(38, "ampersand"); Set(39, "quoteright");
            Set(40, "parenleft"); Set(41, "parenright"); Set(42, "asterisk"); Set(43, "plus");
            Set(44, "comma"); Set(45, "hyphen"); Set(46, "period"); Set(47, "slash");
            Set(48, "zero"); Set(49, "one"); Set(50, "two"); Set(51, "three"); Set(52, "four");
            Set(53, "five"); Set(54, "six"); Set(55, "seven"); Set(56, "eight"); Set(57, "nine");
            Set(58, "colon"); Set(59, "semicolon"); Set(60, "less"); Set(61, "equal");
            Set(62, "greater"); Set(63, "question"); Set(64, "at");
            for (int letter = 0; letter < 26; letter++)
            {
                Set(65 + letter, ((char)('A' + letter)).ToString());
                Set(97 + letter, ((char)('a' + letter)).ToString());
            }
            Set(91, "bracketleft"); Set(92, "backslash"); Set(93, "bracketright");
            Set(94, "asciicircum"); Set(95, "underscore"); Set(96, "quoteleft");
            Set(123, "braceleft"); Set(124, "bar"); Set(125, "braceright"); Set(126, "asciitilde");
            Set(161, "exclamdown"); Set(162, "cent"); Set(163, "sterling"); Set(164, "fraction");
            Set(165, "yen"); Set(166, "florin"); Set(167, "section"); Set(168, "currency");
            Set(169, "quotesingle"); Set(170, "quotedblleft"); Set(171, "guillemotleft");
            Set(172, "guilsinglleft"); Set(173, "guilsinglright"); Set(174, "fi"); Set(175, "fl");
            Set(177, "endash"); Set(178, "dagger"); Set(179, "daggerdbl"); Set(180, "periodcentered");
            Set(182, "paragraph"); Set(183, "bullet"); Set(184, "quotesinglbase");
            Set(185, "quotedblbase"); Set(186, "quotedblright"); Set(187, "guillemotright");
            Set(188, "ellipsis"); Set(189, "perthousand"); Set(191, "questiondown");
            Set(193, "grave"); Set(194, "acute"); Set(195, "circumflex"); Set(196, "tilde");
            Set(197, "macron"); Set(198, "breve"); Set(199, "dotaccent"); Set(200, "dieresis");
            Set(202, "ring"); Set(203, "cedilla"); Set(205, "hungarumlaut"); Set(206, "ogonek");
            Set(207, "caron"); Set(208, "emdash"); Set(225, "AE"); Set(227, "ordfeminine");
            Set(232, "Lslash"); Set(233, "Oslash"); Set(234, "OE"); Set(235, "ordmasculine");
            Set(241, "ae"); Set(245, "dotlessi"); Set(248, "lslash"); Set(249, "oslash");
            Set(250, "oe"); Set(251, "germandbls");
            return names;
        }

        private static Dictionary<string, int> BuildNameToCodePoint()
        {
            var map = new Dictionary<string, int>();
            for (int code = 32; code < 127; code++)
            {
                string? name = Names[code];
                if (name != null && !map.ContainsKey(name))
                {
                    map[name] = code;
                }
            }
            map["quoteright"] = '\'';
            map["quoteleft"] = '`';
            map["fi"] = 0xFB01;
            map["fl"] = 0xFB02;
            map["endash"] = 0x2013;
            map["emdash"] = 0x2014;
            map["bullet"] = 0x2022;
            map["quotedblleft"] = 0x201C;
            map["quotedblright"] = 0x201D;
            map["quotesinglbase"] = 0x201A;
            map["quotedblbase"] = 0x201E;
            map["ellipsis"] = 0x2026;
            map["dagger"] = 0x2020;
            map["daggerdbl"] = 0x2021;
            map["perthousand"] = 0x2030;
            map["germandbls"] = 0x00DF;
            map["dotlessi"] = 0x0131;
            return map;
        }
    }
}
