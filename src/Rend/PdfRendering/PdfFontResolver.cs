#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Rend.Pdf.Parsing;
using SkiaSharp;

namespace Rend.PdfRendering
{
    internal static class PdfFontResolver
    {
        public static void ResolveFontTypeface(PdfDocumentReader reader, GraphicsState state,
            PdfObj pageDict, Dictionary<int, SKTypeface> typefaceCache)
        {
            var resources = reader.Resolve(pageDict["Resources"]);
            var fonts = reader.Resolve(resources["Font"]);
            if (fonts.IsNull || string.IsNullOrEmpty(state.FontName))
            {
                return;
            }

            var fontDict = reader.Resolve(fonts[state.FontName]);
            if (fontDict.IsNull)
            {
                return;
            }

            state.FontDict = fontDict;

            var fontType = reader.Resolve(fontDict["Subtype"]).AsName();
            state.IsCIDFont = fontType == "Type0" || fontDict.ContainsKey("DescendantFonts");

            PdfObj cidFontDict = fontDict;
            if (state.IsCIDFont)
            {
                var descendants = reader.Resolve(fontDict["DescendantFonts"]);
                if (descendants.IsArray && descendants.Count > 0)
                {
                    cidFontDict = reader.Resolve(descendants[0]);
                }
            }

            var toUnicode = reader.Resolve(fontDict["ToUnicode"]);
            if (toUnicode.IsStream)
            {
                var cmapData = reader.GetStreamBytes(toUnicode);
                if (cmapData != null)
                {
                    state.ToUnicodeMap = ParseToUnicodeCMap(cmapData);
                }
            }

            state.Encoding = ResolveEncoding(reader, fontDict);

            state.FontWidths = ResolveFontWidths(reader, fontDict, cidFontDict);
            var firstCharObj = reader.Resolve(fontDict["FirstChar"]);
            state.FontFirstChar = firstCharObj.IsNull ? 0 : (int)firstCharObj.AsInt();
            var defaultWidthObj = reader.Resolve(cidFontDict["DW"]);
            state.FontDefaultWidth = defaultWidthObj.IsNull ? 1000f : defaultWidthObj.AsFloat();

            int cacheKey = GetFontCacheKey(fontDict);
            bool isSystemFallback = false;

            if (typefaceCache.TryGetValue(cacheKey, out var cached))
            {
                state.Typeface = cached;
                // Check if this was a system font fallback by trying embedded data
                isSystemFallback = !IsEmbeddedFontLoadable(reader, fontDict, cidFontDict);
            }
            else
            {
                byte[]? fontData = reader.GetFontProgramData(cidFontDict);
                if (fontData == null || fontData.Length == 0)
                {
                    fontData = reader.GetFontProgramData(fontDict);
                }
                if (fontData != null && fontData.Length > 0)
                {
                    using var skData = SKData.CreateCopy(fontData);
                    var typeface = SKTypeface.FromData(skData);
                    if (typeface != null)
                    {
                        typefaceCache[cacheKey] = typeface;
                        state.Typeface = typeface;
                        return;
                    }

                    // Skia cannot load Type1 (PFA/PFB) directly. Convert to OpenType/CFF,
                    // interpreting the Type1 charstrings and re-emitting them as Type2.
                    byte[]? convertedOtf = Rend.Pdf.Parsing.Type1ToCffConverter.Convert(fontData, GlyphNameToUnicode);
                    if (convertedOtf != null && convertedOtf.Length > 0)
                    {
                        using var convertedData = SKData.CreateCopy(convertedOtf);
                        var convertedTypeface = SKTypeface.FromData(convertedData);
                        if (convertedTypeface != null)
                        {
                            typefaceCache[cacheKey] = convertedTypeface;
                            state.Typeface = convertedTypeface;
                            return;
                        }
                    }
                }

                isSystemFallback = true;
                var baseFontNameForLookup = GetStrippedBaseFontName(reader, fontDict, cidFontDict);
                var systemName = MapPdfFontToSystem(baseFontNameForLookup);
                var systemTypeface = SKTypeface.FromFamilyName(systemName, GetFontStyle(baseFontNameForLookup));
                if (systemTypeface != null)
                {
                    typefaceCache[cacheKey] = systemTypeface;
                    state.Typeface = systemTypeface;
                }
            }

            if (isSystemFallback)
            {
                // Keep PDF widths for positioning — they match the PDF's layout intent.
                // System font glyphs may be slightly wider/narrower per character, but
                // keeping PDF widths prevents cumulative line-width overflow.

                if (state.Encoding == null && state.ToUnicodeMap == null)
                {
                    var baseFontNameForEncoding = GetStrippedBaseFontName(reader, fontDict, cidFontDict);
                    var syntheticEncoding = GetSyntheticEncoding(baseFontNameForEncoding);
                    if (syntheticEncoding != null)
                    {
                        state.Encoding = syntheticEncoding;
                    }
                }
            }
        }

        private static string GetStrippedBaseFontName(PdfDocumentReader reader, PdfObj fontDict, PdfObj cidFontDict)
        {
            var baseFontName = reader.Resolve(cidFontDict["BaseFont"]).AsName();
            if (string.IsNullOrEmpty(baseFontName))
            {
                baseFontName = reader.Resolve(fontDict["BaseFont"]).AsName();
            }
            if (baseFontName.StartsWith("/"))
            {
                baseFontName = baseFontName.Substring(1);
            }
            return baseFontName;
        }

        private static bool IsEmbeddedFontLoadable(PdfDocumentReader reader, PdfObj fontDict, PdfObj cidFontDict)
        {
            byte[]? fontData = reader.GetFontProgramData(cidFontDict);
            if (fontData == null || fontData.Length == 0)
            {
                fontData = reader.GetFontProgramData(fontDict);
            }
            if (fontData == null || fontData.Length == 0)
            {
                return false;
            }
            // Quick check: TrueType starts with 0x00010000 or 'true', OpenType with 'OTTO'
            if (fontData.Length >= 4)
            {
                if ((fontData[0] == 0 && fontData[1] == 1 && fontData[2] == 0 && fontData[3] == 0) ||
                    (fontData[0] == 'O' && fontData[1] == 'T' && fontData[2] == 'T' && fontData[3] == 'O') ||
                    (fontData[0] == 't' && fontData[1] == 'r' && fontData[2] == 'u' && fontData[3] == 'e'))
                {
                    return true;
                }
            }
            return false;
        }

        public static string DecodeTextBytes(GraphicsState state, byte[] bytes)
        {
            bool isTwoByte = state.IsCIDFont ||
                             (state.ToUnicodeMap != null && IsTwoByteEncoding(state.ToUnicodeMap));

            if (state.ToUnicodeMap != null && state.ToUnicodeMap.Count > 0)
            {
                var builder = new StringBuilder();

                if (isTwoByte && bytes.Length >= 2)
                {
                    for (int i = 0; i + 1 < bytes.Length; i += 2)
                    {
                        int code = (bytes[i] << 8) | bytes[i + 1];
                        if (state.ToUnicodeMap.TryGetValue(code, out string? mapped))
                        {
                            builder.Append(mapped);
                        }
                        else
                        {
                            builder.Append((char)code);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        int code = bytes[i];
                        if (state.ToUnicodeMap.TryGetValue(code, out string? mapped))
                        {
                            builder.Append(mapped);
                        }
                        else
                        {
                            builder.Append((char)code);
                        }
                    }
                }
                return builder.ToString();
            }

            if (isTwoByte)
            {
                var builder = new StringBuilder();
                for (int i = 0; i + 1 < bytes.Length; i += 2)
                {
                    int code = (bytes[i] << 8) | bytes[i + 1];
                    builder.Append((char)code);
                }
                return builder.ToString();
            }

            if (state.Encoding != null)
            {
                var builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    if (state.Encoding.TryGetValue(bytes[i], out string? mapped))
                    {
                        builder.Append(mapped);
                    }
                    else
                    {
                        builder.Append((char)bytes[i]);
                    }
                }
                return builder.ToString();
            }

            {
                var builder = new StringBuilder(bytes.Length);
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append((char)bytes[i]);
                }
                return builder.ToString();
            }
        }

        public static string[] DecodeTextBytesPerCode(GraphicsState state, byte[] bytes)
        {
            bool isTwoByte = state.IsCIDFont ||
                             (state.ToUnicodeMap != null && IsTwoByteEncoding(state.ToUnicodeMap));

            if (state.ToUnicodeMap != null && state.ToUnicodeMap.Count > 0)
            {
                var result = new List<string>();
                if (isTwoByte && bytes.Length >= 2)
                {
                    for (int i = 0; i + 1 < bytes.Length; i += 2)
                    {
                        int code = (bytes[i] << 8) | bytes[i + 1];
                        if (state.ToUnicodeMap.TryGetValue(code, out string? mapped))
                        {
                            result.Add(mapped);
                        }
                        else
                        {
                            result.Add(((char)code).ToString());
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        int code = bytes[i];
                        if (state.ToUnicodeMap.TryGetValue(code, out string? mapped))
                        {
                            result.Add(mapped);
                        }
                        else
                        {
                            result.Add(((char)code).ToString());
                        }
                    }
                }
                return result.ToArray();
            }

            if (isTwoByte)
            {
                var result = new List<string>();
                for (int i = 0; i + 1 < bytes.Length; i += 2)
                {
                    int code = (bytes[i] << 8) | bytes[i + 1];
                    result.Add(((char)code).ToString());
                }
                return result.ToArray();
            }

            if (state.Encoding != null)
            {
                var result = new List<string>();
                for (int i = 0; i < bytes.Length; i++)
                {
                    if (state.Encoding.TryGetValue(bytes[i], out string? mapped))
                    {
                        result.Add(mapped);
                    }
                    else
                    {
                        result.Add(((char)bytes[i]).ToString());
                    }
                }
                return result.ToArray();
            }

            {
                var result = new string[bytes.Length];
                for (int i = 0; i < bytes.Length; i++)
                {
                    result[i] = ((char)bytes[i]).ToString();
                }
                return result;
            }
        }

        public static int[] GetCharCodes(GraphicsState state, byte[] textBytes)
        {
            bool isTwoByte = state.IsCIDFont ||
                             (state.ToUnicodeMap != null && IsTwoByteEncoding(state.ToUnicodeMap));

            if (isTwoByte)
            {
                int count = textBytes.Length / 2;
                var codes = new int[count];
                for (int i = 0; i < count; i++)
                {
                    codes[i] = (textBytes[i * 2] << 8) | textBytes[i * 2 + 1];
                }
                return codes;
            }
            else
            {
                var codes = new int[textBytes.Length];
                for (int i = 0; i < textBytes.Length; i++)
                {
                    codes[i] = textBytes[i];
                }
                return codes;
            }
        }

        private static int GetFontCacheKey(PdfObj fontDict)
        {
            if (fontDict is PdfRef pdfRef)
            {
                return (pdfRef.ObjNum * 397) ^ pdfRef.GenNum;
            }
            return fontDict.GetHashCode();
        }

        private static string MapPdfFontToSystem(string baseFontName)
        {
            if (string.IsNullOrEmpty(baseFontName))
            {
                return "Arial";
            }

            string name = baseFontName.Replace(",", "-");

            // Strip subset prefix (e.g., "ABCDEF+FontName")
            int plus = name.IndexOf('+');
            if (plus >= 0 && plus < 7)
            {
                name = name.Substring(plus + 1);
            }

            // Standard 14 PDF fonts
            if (name.StartsWith("Helvetica", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("Arial", StringComparison.OrdinalIgnoreCase))
            {
                return "Arial";
            }
            if (name.StartsWith("Times", StringComparison.OrdinalIgnoreCase))
            {
                return "Times New Roman";
            }
            if (name.StartsWith("Courier", StringComparison.OrdinalIgnoreCase))
            {
                return "Courier New";
            }
            if (name.StartsWith("Symbol", StringComparison.OrdinalIgnoreCase))
            {
                return "Symbol";
            }
            if (name.StartsWith("ZapfDingbats", StringComparison.OrdinalIgnoreCase))
            {
                return "Wingdings";
            }

            // TeX/LaTeX Computer Modern fonts
            if (name.StartsWith("CM", StringComparison.Ordinal))
            {
                if (name.StartsWith("CMTT", StringComparison.Ordinal) ||
                    name.StartsWith("CMTypewriter", StringComparison.OrdinalIgnoreCase))
                {
                    return "Courier New";
                }
                if (name.StartsWith("CMSS", StringComparison.Ordinal) ||
                    name.StartsWith("CMSans", StringComparison.OrdinalIgnoreCase))
                {
                    return "Arial";
                }
                if (name.StartsWith("CMSY", StringComparison.Ordinal) ||
                    name.StartsWith("CMMI", StringComparison.Ordinal) ||
                    name.StartsWith("CMEX", StringComparison.Ordinal))
                {
                    return "Symbol";
                }
                return "Times New Roman";
            }

            // Nimbus fonts (URW replacements for standard fonts)
            if (name.Contains("Nimbus", StringComparison.OrdinalIgnoreCase))
            {
                if (name.Contains("Sans", StringComparison.OrdinalIgnoreCase))
                {
                    return "Arial";
                }
                if (name.Contains("Mon", StringComparison.OrdinalIgnoreCase))
                {
                    return "Courier New";
                }
                return "Times New Roman";
            }

            // Liberation fonts
            if (name.Contains("Liberation", StringComparison.OrdinalIgnoreCase))
            {
                if (name.Contains("Sans", StringComparison.OrdinalIgnoreCase))
                {
                    return "Arial";
                }
                if (name.Contains("Mono", StringComparison.OrdinalIgnoreCase))
                {
                    return "Courier New";
                }
                return "Times New Roman";
            }

            // DejaVu fonts
            if (name.Contains("DejaVu", StringComparison.OrdinalIgnoreCase))
            {
                if (name.Contains("Sans", StringComparison.OrdinalIgnoreCase) &&
                    name.Contains("Mono", StringComparison.OrdinalIgnoreCase))
                {
                    return "Courier New";
                }
                if (name.Contains("Sans", StringComparison.OrdinalIgnoreCase))
                {
                    return "Arial";
                }
                return "Times New Roman";
            }

            // Strip style suffixes for family name
            string family = name.Replace("-BoldItalic", "").Replace("-BoldOblique", "")
                .Replace("-Bold", "").Replace("-Italic", "")
                .Replace("-Oblique", "").Replace("-Roman", "")
                .Replace("-Regular", "").Replace("-Medi", "")
                .Replace("-Regu", "").Replace("-Light", "")
                .Replace("Ital", "");

            // If the cleaned name still has L- suffix patterns (like NimbusRomNo9L-), clean up
            if (family.EndsWith("-") || family.EndsWith("L"))
            {
                family = family.TrimEnd('-', 'L');
            }

            // Try the system font directly; if it looks like it won't resolve, use a safe default
            if (family.Length < 3)
            {
                return "Arial";
            }

            return family;
        }

        private static SKFontStyle GetFontStyle(string baseFontName)
        {
            if (string.IsNullOrEmpty(baseFontName))
            {
                return SKFontStyle.Normal;
            }

            // Strip subset prefix
            string name = baseFontName;
            int plusIndex = name.IndexOf('+');
            if (plusIndex >= 0 && plusIndex < 7)
            {
                name = name.Substring(plusIndex + 1);
            }

            bool bold = name.IndexOf("Bold", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        name.IndexOf("-Medi", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        name.IndexOf("Demi", StringComparison.OrdinalIgnoreCase) >= 0;
            bool italic = name.IndexOf("Italic", StringComparison.OrdinalIgnoreCase) >= 0 ||
                          name.IndexOf("Oblique", StringComparison.OrdinalIgnoreCase) >= 0 ||
                          name.IndexOf("Ital", StringComparison.OrdinalIgnoreCase) >= 0 ||
                          name.IndexOf("Slant", StringComparison.OrdinalIgnoreCase) >= 0;

            if (bold && italic) { return SKFontStyle.BoldItalic; }
            if (bold) { return SKFontStyle.Bold; }
            if (italic) { return SKFontStyle.Italic; }
            return SKFontStyle.Normal;
        }

        private static Dictionary<int, string>? ResolveEncoding(PdfDocumentReader reader, PdfObj fontDict)
        {
            var encoding = reader.Resolve(fontDict["Encoding"]);
            if (encoding.IsNull)
            {
                return null;
            }

            string encodingName = encoding.AsName();
            if (string.IsNullOrEmpty(encodingName) && encoding.IsDict)
            {
                encodingName = reader.Resolve(encoding["BaseEncoding"]).AsName();
            }

            if (encodingName.Contains("WinAnsi") || encodingName.Contains("WinAnsiEncoding"))
            {
                return null;
            }

            if (encodingName.Contains("MacRoman") || encodingName.Contains("MacRomanEncoding"))
            {
                return null;
            }

            if (encoding.IsDict && encoding.ContainsKey("Differences"))
            {
                var diffs = reader.Resolve(encoding["Differences"]);
                if (diffs.IsArray)
                {
                    return ParseEncodingDifferences(reader, diffs);
                }
            }

            return null;
        }

        private static Dictionary<int, float>? ResolveFontWidths(PdfDocumentReader reader, PdfObj fontDict, PdfObj cidFontDict)
        {
            var widths = reader.Resolve(fontDict["Widths"]);
            if (!widths.IsNull && widths.IsArray && widths.Count > 0)
            {
                var firstChar = reader.Resolve(fontDict["FirstChar"]);
                int firstCharValue = firstChar.IsNull ? 0 : (int)firstChar.AsInt();
                var map = new Dictionary<int, float>();
                for (int i = 0; i < widths.Count; i++)
                {
                    float width = widths[i].AsFloat();
                    if (width > 0)
                    {
                        map[firstCharValue + i] = width;
                    }
                }
                return map.Count > 0 ? map : null;
            }

            var wideArray = reader.Resolve(cidFontDict["W"]);
            if (!wideArray.IsNull && wideArray.IsArray && wideArray.Count > 0)
            {
                var map = new Dictionary<int, float>();
                int index = 0;
                while (index < wideArray.Count)
                {
                    int startCid = (int)reader.Resolve(wideArray[index]).AsInt();
                    index++;
                    if (index >= wideArray.Count)
                    {
                        break;
                    }

                    var next = reader.Resolve(wideArray[index]);
                    if (next.IsArray)
                    {
                        for (int j = 0; j < next.Count; j++)
                        {
                            map[startCid + j] = next[j].AsFloat();
                        }
                        index++;
                    }
                    else
                    {
                        int endCid = (int)next.AsInt();
                        index++;
                        if (index >= wideArray.Count)
                        {
                            break;
                        }
                        float width = reader.Resolve(wideArray[index]).AsFloat();
                        index++;
                        for (int cidValue = startCid; cidValue <= endCid; cidValue++)
                        {
                            map[cidValue] = width;
                        }
                    }
                }
                return map.Count > 0 ? map : null;
            }

            return null;
        }

        private static Dictionary<int, string> ParseEncodingDifferences(PdfDocumentReader reader, PdfObj diffsArray)
        {
            var map = new Dictionary<int, string>();
            int code = 0;
            for (int i = 0; i < diffsArray.Count; i++)
            {
                var item = reader.Resolve(diffsArray[i]);
                if (item.IsInt || item.IsReal)
                {
                    code = (int)item.AsInt();
                }
                else if (item.IsName)
                {
                    string glyphName = item.AsName();
                    if (glyphName.StartsWith("/"))
                    {
                        glyphName = glyphName.Substring(1);
                    }
                    string? unicode = GlyphNameToUnicode(glyphName);
                    if (unicode != null)
                    {
                        map[code] = unicode;
                    }
                    code++;
                }
            }
            return map;
        }

        private static Dictionary<int, string>? GetSyntheticEncoding(string baseFontName)
        {
            // Strip subset prefix
            string name = baseFontName;
            int plusIndex = name.IndexOf('+');
            if (plusIndex >= 0 && plusIndex < 7)
            {
                name = name.Substring(plusIndex + 1);
            }

            // Computer Modern Symbol (CMSY) — TeX math symbols encoding
            if (name.StartsWith("CMSY", StringComparison.Ordinal))
            {
                return new Dictionary<int, string>
                {
                    [0] = "\u2212", // minus
                    [1] = "\u00B7", // periodcentered
                    [2] = "\u00D7", // multiply
                    [3] = "\u2217", // asteriskmath (∗)
                    [4] = "\u00F7", // divide
                    [5] = "\u25C7", // diamondmath
                    [6] = "\u00B1", // plusminus
                    [7] = "\u2213", // minusplus
                    [8] = "\u2295", // circleplus
                    [9] = "\u2296", // circleminus
                    [10] = "\u2297", // circlemultiply
                    [11] = "\u2298", // circledivide
                    [12] = "\u2299", // circledot
                    [13] = "\u25CB", // circlecopyrt (○)
                    [14] = "\u2218", // openbullet
                    [15] = "\u2219", // bullet
                    [16] = "\u224D", // asymptoticallyequal
                    [17] = "\u2261", // equivalence
                    [18] = "\u2286", // reflexsubset
                    [19] = "\u2287", // reflexsuperset
                    [20] = "\u2264", // lessequal
                    [21] = "\u2265", // greaterequal
                    [22] = "\u227C", // precedesequal
                    [23] = "\u227D", // followsequal
                    [24] = "\u223C", // similar
                    [25] = "\u2248", // approxequal
                    [26] = "\u2282", // propersubset
                    [27] = "\u2283", // propersuperset
                    [28] = "\u226A", // lessmuch
                    [29] = "\u226B", // greatermuch
                    [30] = "\u227A", // precedes
                    [31] = "\u227B", // follows
                    [48] = "\u2190", // arrowleft
                    [49] = "\u2192", // arrowright
                    [50] = "\u2191", // arrowup
                    [51] = "\u2193", // arrowdown
                    [52] = "\u2194", // arrowboth
                    [102] = "\u007B", // braceleft
                    [103] = "\u007D", // braceright
                    [104] = "\u27E8", // angleleft
                    [105] = "\u27E9", // angleright
                    [106] = "\u007C", // bar
                    [110] = "\u005C", // backslash
                };
            }

            // Computer Modern Roman (CMR) — TeX text encoding
            if (name.StartsWith("CMR", StringComparison.Ordinal))
            {
                return new Dictionary<int, string>
                {
                    [0] = "\u0393", // Gamma
                    [1] = "\u0394", // Delta
                    [2] = "\u0398", // Theta
                    [3] = "\u039B", // Lambda
                    [4] = "\u039E", // Xi
                    [5] = "\u03A0", // Pi
                    [6] = "\u03A3", // Sigma
                    [7] = "\u03A5", // Upsilon
                    [8] = "\u03A6", // Phi
                    [9] = "\u03A8", // Psi
                    [10] = "\u03A9", // Omega
                    [11] = "\uFB00", // ff
                    [12] = "\uFB01", // fi
                    [13] = "\uFB02", // fl
                    [14] = "\uFB03", // ffi
                    [15] = "\uFB04", // ffl
                    [16] = "\u0131", // dotlessi
                    [17] = "\u0237", // dotlessj
                    [18] = "\u0060", // grave
                    [19] = "\u00B4", // acute
                    [20] = "\u02C7", // caron
                    [21] = "\u02D8", // breve
                    [22] = "\u00AF", // macron
                    [23] = "\u02DA", // ring
                    [24] = "\u00B8", // cedilla
                    [25] = "\u00DF", // germandbls
                    [26] = "\u00E6", // ae
                    [27] = "\u0153", // oe
                    [28] = "\u00F8", // oslash
                    [29] = "\u00C6", // AE
                    [30] = "\u0152", // OE
                    [31] = "\u00D8", // Oslash
                    [34] = "\u201D", // quotedblleft (in CM encoding, 34 = ")
                    [35] = "#",
                    [36] = "$",
                    [37] = "%",
                    [38] = "&",
                    [39] = "\u2019", // quoteright
                    [92] = "\u201C", // quotedblleft
                    [123] = "\u2013", // endash
                    [124] = "\u2014", // emdash
                    [125] = "\u02DD", // hungarumlaut
                    [126] = "\u007E", // tilde
                    [127] = "\u00A8", // dieresis
                };
            }

            // Computer Modern Typewriter (CMTT)
            if (name.StartsWith("CMTT", StringComparison.Ordinal))
            {
                // CMTT uses a mostly ASCII-compatible encoding
                return null;
            }

            // Computer Modern Math Italic (CMMI)
            if (name.StartsWith("CMMI", StringComparison.Ordinal))
            {
                return new Dictionary<int, string>
                {
                    [0] = "\u0393", // Gamma
                    [1] = "\u0394", // Delta
                    [2] = "\u0398", // Theta
                    [3] = "\u039B", // Lambda
                    [4] = "\u039E", // Xi
                    [5] = "\u03A0", // Pi
                    [6] = "\u03A3", // Sigma
                    [7] = "\u03A5", // Upsilon
                    [8] = "\u03A6", // Phi
                    [9] = "\u03A8", // Psi
                    [10] = "\u03A9", // Omega
                    [11] = "\u03B1", // alpha
                    [12] = "\u03B2", // beta
                    [13] = "\u03B3", // gamma
                    [14] = "\u03B4", // delta
                    [15] = "\u03B5", // epsilon
                    [16] = "\u03B6", // zeta
                    [17] = "\u03B7", // eta
                    [18] = "\u03B8", // theta
                    [19] = "\u03B9", // iota
                    [20] = "\u03BA", // kappa
                    [21] = "\u03BB", // lambda
                    [22] = "\u03BC", // mu
                    [23] = "\u03BD", // nu
                    [24] = "\u03BE", // xi
                    [25] = "\u03C0", // pi
                    [26] = "\u03C1", // rho
                    [27] = "\u03C3", // sigma
                    [28] = "\u03C4", // tau
                    [29] = "\u03C5", // upsilon
                    [30] = "\u03C6", // phi
                    [31] = "\u03C7", // chi
                    [58] = ".", // period
                    [59] = ",", // comma
                };
            }

            return null;
        }

        internal static string? GlyphNameToUnicode(string name)
        {
            switch (name)
            {
                case "space": return " ";
                case "exclam": return "!";
                case "quotedbl": return "\"";
                case "numbersign": return "#";
                case "dollar": return "$";
                case "percent": return "%";
                case "ampersand": return "&";
                case "quotesingle": return "'";
                case "parenleft": return "(";
                case "parenright": return ")";
                case "asterisk": return "*";
                case "plus": return "+";
                case "comma": return ",";
                case "hyphen": return "-";
                case "period": return ".";
                case "slash": return "/";
                case "zero": return "0";
                case "one": return "1";
                case "two": return "2";
                case "three": return "3";
                case "four": return "4";
                case "five": return "5";
                case "six": return "6";
                case "seven": return "7";
                case "eight": return "8";
                case "nine": return "9";
                case "colon": return ":";
                case "semicolon": return ";";
                case "less": return "<";
                case "equal": return "=";
                case "greater": return ">";
                case "question": return "?";
                case "at": return "@";
                case "bracketleft": return "[";
                case "backslash": return "\\";
                case "bracketright": return "]";
                case "asciicircum": return "^";
                case "underscore": return "_";
                case "grave": return "`";
                case "braceleft": return "{";
                case "bar": return "|";
                case "braceright": return "}";
                case "asciitilde": return "~";
                case "asteriskmath": return "*";
                case "minus": return "\u2212";
                case "periodcentered": return "\u00B7";
                case "multiply": return "\u00D7";
                case "divide": return "\u00F7";
                case "plusminus": return "\u00B1";
                case "lessequal": return "\u2264";
                case "greaterequal": return "\u2265";
                case "infinity": return "\u221E";
                case "integral": return "\u222B";
                case "radical": return "\u221A";
                case "summation": return "\u2211";
                case "product": return "\u220F";
                case "partialdiff": return "\u2202";
                case "notequal": return "\u2260";
                case "equivalence": return "\u2261";
                case "approxequal": return "\u2248";
                case "arrowleft": return "\u2190";
                case "arrowright": return "\u2192";
                case "arrowup": return "\u2191";
                case "arrowdown": return "\u2193";
                case "arrowboth": return "\u2194";
                case "lozenge": return "\u25CA";
                case "diamondmath": return "\u25C7";
                case "circleplus": return "\u2295";
                case "circleminus": return "\u2296";
                case "circlemultiply": return "\u2297";
                case "circledivide": return "\u2298";
                case "circledot": return "\u2299";
                case "circlecopyrt": return "\u25CB";
                case "openbullet": return "\u2218";
                case "propersubset": return "\u2282";
                case "propersuperset": return "\u2283";
                case "reflexsubset": return "\u2286";
                case "reflexsuperset": return "\u2287";
                case "precedesequal": return "\u227C";
                case "followsequal": return "\u227D";
                case "similar": return "\u223C";
                case "precedes": return "\u227A";
                case "follows": return "\u227B";
                case "lessmuch": return "\u226A";
                case "greatermuch": return "\u226B";
                case "element": return "\u2208";
                case "owner": return "\u220B";
                case "logicaland": return "\u2227";
                case "logicalor": return "\u2228";
                case "logicalnot": return "\u00AC";
                case "universal": return "\u2200";
                case "existential": return "\u2203";
                case "emptyset": return "\u2205";
                case "nabla": return "\u2207";
                case "therefore": return "\u2234";
                case "perpendicular": return "\u22A5";
                case "angle": return "\u2220";
                case "angleleft": return "\u27E8";
                case "angleright": return "\u27E9";
                case "dotmath": return "\u22C5";
                case "exclamdown": return "\u00A1";
                case "questiondown": return "\u00BF";
                case "guillemotleft": return "\u00AB";
                case "guillemotright": return "\u00BB";
                case "guilsinglleft": return "\u2039";
                case "guilsinglright": return "\u203A";
                case "quotesinglbase": return "\u201A";
                case "quotedblbase": return "\u201E";
                case "dieresis": return "\u00A8";
                case "cedilla": return "\u00B8";
                case "caron": return "\u02C7";
                case "breve": return "\u02D8";
                case "dotaccent": return "\u02D9";
                case "ring": return "\u02DA";
                case "ogonek": return "\u02DB";
                case "tilde": return "\u02DC";
                case "hungarumlaut": return "\u02DD";
                case "macron": return "\u00AF";
                case "acute": return "\u00B4";
                case "germandbls": return "\u00DF";
                case "ae": return "\u00E6";
                case "oe": return "\u0153";
                case "oslash": return "\u00F8";
                case "AE": return "\u00C6";
                case "OE": return "\u0152";
                case "Oslash": return "\u00D8";
                case "dotlessi": return "\u0131";
                case "dotlessj": return "\u0237";
                case "fraction": return "\u2044";
                case "perthousand": return "\u2030";
                case "Euro": return "\u20AC";
                case "bullet": return "\u2022";
                case "endash": return "\u2013";
                case "emdash": return "\u2014";
                case "quotedblleft": return "\u201C";
                case "quotedblright": return "\u201D";
                case "quoteleft": return "\u2018";
                case "quoteright": return "\u2019";
                case "fi": return "\uFB01";
                case "fl": return "\uFB02";
                case "ellipsis": return "\u2026";
                case "trademark": return "\u2122";
                case "copyright": return "\u00A9";
                case "registered": return "\u00AE";
                case "degree": return "\u00B0";
                case "mu": return "\u00B5";
                case "paragraph": return "\u00B6";
                case "section": return "\u00A7";
                case "dagger": return "\u2020";
                case "daggerdbl": return "\u2021";
                default:
                    if (name.StartsWith("uni") && name.Length == 7)
                    {
                        if (int.TryParse(name.Substring(3), NumberStyles.HexNumber,
                            CultureInfo.InvariantCulture, out int codePoint))
                        {
                            return char.ConvertFromUtf32(codePoint);
                        }
                    }
                    if (name.Length == 1)
                    {
                        return name;
                    }
                    if (name.Length == 1 || (name.Length > 0 && char.IsLetter(name[0]) && name.Length > 1 && !char.IsLetter(name[1])))
                    {
                        return name[0].ToString();
                    }
                    // Unknown glyph name — return empty rather than rendering
                    // the entire name as visible text
                    return "";
            }
        }

        internal static Dictionary<int, string> ParseToUnicodeCMap(byte[] data)
        {
            var map = new Dictionary<int, string>();
            string text = Encoding.ASCII.GetString(data);
            int pos = 0;

            while (pos < text.Length)
            {
                int bfcharStart = text.IndexOf("beginbfchar", pos, StringComparison.Ordinal);
                int bfrangeStart = text.IndexOf("beginbfrange", pos, StringComparison.Ordinal);

                int nextSection;
                bool isRange;

                if (bfcharStart < 0 && bfrangeStart < 0)
                {
                    break;
                }

                if (bfcharStart >= 0 && (bfrangeStart < 0 || bfcharStart < bfrangeStart))
                {
                    nextSection = bfcharStart + "beginbfchar".Length;
                    isRange = false;
                }
                else
                {
                    nextSection = bfrangeStart + "beginbfrange".Length;
                    isRange = true;
                }

                string endMarker = isRange ? "endbfrange" : "endbfchar";
                int endPos = text.IndexOf(endMarker, nextSection, StringComparison.Ordinal);
                if (endPos < 0) { pos = nextSection; continue; }

                string section = text.Substring(nextSection, endPos - nextSection);
                var hexValues = ExtractHexValues(section);

                if (isRange)
                {
                    for (int i = 0; i + 2 < hexValues.Count; i += 3)
                    {
                        int start = hexValues[i].Code;
                        int end = hexValues[i + 1].Code;
                        int dst = hexValues[i + 2].Code;
                        string? dstStr = hexValues[i + 2].RawHex;

                        for (int charCode = start; charCode <= end; charCode++)
                        {
                            if (dstStr != null && dstStr.Length > 2)
                            {
                                int offset = charCode - start;
                                map[charCode] = IncrementUnicodeString(dstStr, offset);
                            }
                            else
                            {
                                map[charCode] = char.ConvertFromUtf32(dst + (charCode - start));
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i + 1 < hexValues.Count; i += 2)
                    {
                        int src = hexValues[i].Code;
                        string? dstStr = hexValues[i + 1].RawHex;
                        int dst = hexValues[i + 1].Code;

                        if (dstStr != null && dstStr.Length > 2)
                        {
                            map[src] = DecodeUtf16Hex(dstStr);
                        }
                        else
                        {
                            map[src] = char.ConvertFromUtf32(dst);
                        }
                    }
                }

                pos = endPos + endMarker.Length;
            }

            return map;
        }

        private struct HexEntry
        {
            public int Code;
            public string? RawHex;
        }

        private static List<HexEntry> ExtractHexValues(string section)
        {
            var result = new List<HexEntry>();
            int index = 0;
            while (index < section.Length)
            {
                int open = section.IndexOf('<', index);
                if (open < 0)
                {
                    break;
                }
                int close = section.IndexOf('>', open);
                if (close < 0)
                {
                    break;
                }

                string hex = section.Substring(open + 1, close - open - 1).Trim();
                int code = 0;
                if (hex.Length > 0 && int.TryParse(hex, NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture, out int parsed))
                {
                    code = parsed;
                }

                result.Add(new HexEntry { Code = code, RawHex = hex.Length > 4 ? hex : null });
                index = close + 1;
            }
            return result;
        }

        private static string DecodeUtf16Hex(string hex)
        {
            var builder = new StringBuilder();
            for (int i = 0; i + 3 < hex.Length; i += 4)
            {
                if (int.TryParse(hex.Substring(i, 4), NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture, out int value))
                {
                    builder.Append((char)value);
                }
            }
            return builder.Length > 0 ? builder.ToString() : "?";
        }

        private static string IncrementUnicodeString(string hex, int offset)
        {
            int baseVal = 0;
            if (hex.Length >= 4 && int.TryParse(hex.Substring(hex.Length - 4, 4),
                NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int parsed))
            {
                baseVal = parsed;
            }
            return char.ConvertFromUtf32(baseVal + offset);
        }

        private static bool IsTwoByteEncoding(Dictionary<int, string> map)
        {
            foreach (var key in map.Keys)
            {
                if (key > 255)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
