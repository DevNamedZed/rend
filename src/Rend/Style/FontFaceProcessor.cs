using System;
using System.Collections.Generic;
using Rend.Css;
using Rend.Fonts;

namespace Rend.Style
{
    /// <summary>
    /// Processes <c>@font-face</c> rules from stylesheets and registers fonts with the font provider.
    /// </summary>
    /// <spec>CSS-FONTS-4 §4 https://drafts.csswg.org/css-fonts-4/#font-face-rule</spec>
    internal static class FontFaceProcessor
    {
        public static void Process(IReadOnlyList<CssRule> rules, IFontProvider fontProvider, Func<string, byte[]?>? resourceLoader = null)
        {
            for (int i = 0; i < rules.Count; i++)
            {
                if (rules[i] is FontFaceRule fontFace)
                {
                    ProcessFontFace(fontFace, fontProvider, resourceLoader);
                }
            }
        }

        private static void ProcessFontFace(FontFaceRule rule, IFontProvider fontProvider, Func<string, byte[]?>? resourceLoader)
        {
            string? familyName = null;
            string? srcUrl = null;
            float weight = 400f;
            CssFontStyle style = CssFontStyle.Normal;
            float stretch = 100f;

            for (int i = 0; i < rule.Declarations.Count; i++)
            {
                var decl = rule.Declarations[i];
                switch (decl.Property)
                {
                    case "font-family":
                        familyName = ExtractStringValue(decl.Value);
                        break;
                    case "src":
                        srcUrl = ExtractUrlValue(decl.Value);
                        break;
                    case "font-stretch":
                        stretch = ParseStretchDescriptor(decl.Value);
                        break;
                    case "font-weight":
                        weight = ParseWeightDescriptor(decl.Value);
                        break;
                    case "font-style":
                        style = ParseStyleDescriptor(decl.Value);
                        break;
                }
            }

            if (familyName == null || srcUrl == null)
            {
                return;
            }

            byte[]? fontData = LoadFontData(srcUrl, resourceLoader);
            if (fontData == null || fontData.Length == 0)
            {
                return;
            }

            var descriptor = new FontFaceDescriptor(familyName, weight, style, stretch);
            fontProvider.RegisterFontFace(fontData, descriptor);
        }

        private static byte[]? LoadFontData(string srcUrl, Func<string, byte[]?>? resourceLoader)
        {
            if (srcUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                return DecodeDataUri(srcUrl);
            }
            if (resourceLoader != null)
            {
                return resourceLoader(srcUrl);
            }
            return null;
        }

        private static string? ExtractStringValue(CssValue value)
        {
            if (value is CssStringValue sv)
            {
                return sv.Value;
            }
            if (value is CssKeywordValue kv)
            {
                return kv.Keyword;
            }
            if (value is CssListValue lv && lv.Values.Count > 0)
            {
                return ExtractStringValue(lv.Values[0]);
            }
            return value.ToString().Trim('"', '\'');
        }

        private static string? ExtractUrlValue(CssValue value)
        {
            if (value is CssUrlValue uv)
            {
                return uv.Url;
            }
            if (value is CssFunctionValue fv && fv.Name == "url" && fv.Arguments.Count > 0)
            {
                if (fv.Arguments[0] is CssStringValue sv)
                {
                    return sv.Value;
                }
                return fv.Arguments[0].ToString().Trim('"', '\'');
            }
            if (value is CssListValue lv)
            {
                for (int i = 0; i < lv.Values.Count; i++)
                {
                    var url = ExtractUrlValue(lv.Values[i]);
                    if (url != null)
                    {
                        return url;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Parses the <c>font-stretch</c> descriptor value. Accepts the nine named keywords
        /// (<c>ultra-condensed</c>..<c>ultra-expanded</c>) and percentage values. Returns the
        /// default 100% (<c>normal</c>) on unrecognized input.
        /// </summary>
        /// <spec>CSS-FONTS-4 §4.4 https://drafts.csswg.org/css-fonts-4/#font-prop-desc</spec>
        private static float ParseStretchDescriptor(CssValue value)
        {
            if (value is CssListValue lv && lv.Values.Count > 0)
            {
                return ParseStretchDescriptor(lv.Values[0]);
            }
            if (value is CssPercentageValue pv)
            {
                return pv.Value;
            }
            if (value is CssKeywordValue kv)
            {
                return KeywordToStretchPercentage(kv.Keyword);
            }
            return 100f;
        }

        private static float KeywordToStretchPercentage(string keyword)
        {
            switch (keyword)
            {
                case "ultra-condensed": return 50f;
                case "extra-condensed": return 62.5f;
                case "condensed": return 75f;
                case "semi-condensed": return 87.5f;
                case "normal": return 100f;
                case "semi-expanded": return 112.5f;
                case "expanded": return 125f;
                case "extra-expanded": return 150f;
                case "ultra-expanded": return 200f;
                default: return 100f;
            }
        }

        /// <summary>
        /// Parses the <c>font-weight</c> descriptor value. Accepts numeric weights (100-900),
        /// the <c>normal</c>/<c>bold</c> keywords, and the first value of a two-value range
        /// (e.g. <c>100 900</c> for variable fonts). Returns 400 on unrecognized input.
        /// </summary>
        /// <spec>CSS-FONTS-4 §4.3 https://drafts.csswg.org/css-fonts-4/#font-weight-desc</spec>
        private static float ParseWeightDescriptor(CssValue value)
        {
            if (value is CssListValue lv && lv.Values.Count > 0)
            {
                return ParseWeightDescriptor(lv.Values[0]);
            }
            if (value is CssNumberValue nv)
            {
                return nv.Value;
            }
            if (value is CssKeywordValue kv)
            {
                switch (kv.Keyword)
                {
                    case "normal": return 400f;
                    case "bold": return 700f;
                    default: return 400f;
                }
            }
            return 400f;
        }

        /// <summary>
        /// Parses the <c>font-style</c> descriptor value. Accepts <c>normal</c>, <c>italic</c>,
        /// and <c>oblique</c> (with optional angle, which we ignore — presence alone is enough
        /// to match per CSS Fonts 4 §4.2). Returns <see cref="CssFontStyle.Normal"/> otherwise.
        /// </summary>
        /// <spec>CSS-FONTS-4 §4.2 https://drafts.csswg.org/css-fonts-4/#font-style-desc</spec>
        private static CssFontStyle ParseStyleDescriptor(CssValue value)
        {
            if (value is CssListValue lv && lv.Values.Count > 0)
            {
                return ParseStyleDescriptor(lv.Values[0]);
            }
            if (value is CssKeywordValue kv)
            {
                switch (kv.Keyword)
                {
                    case "italic": return CssFontStyle.Italic;
                    case "oblique": return CssFontStyle.Oblique;
                    default: return CssFontStyle.Normal;
                }
            }
            return CssFontStyle.Normal;
        }

        private static byte[]? DecodeDataUri(string dataUri)
        {
            int commaIndex = dataUri.IndexOf(',');
            if (commaIndex < 0)
            {
                return null;
            }

            string header = dataUri.Substring(0, commaIndex);
            string data = dataUri.Substring(commaIndex + 1);

            if (header.EndsWith(";base64", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    return Convert.FromBase64String(data);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }
    }
}
