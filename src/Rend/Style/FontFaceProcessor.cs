using System;
using System.Collections.Generic;
using Rend.Css;
using Rend.Fonts;

namespace Rend.Style
{
    /// <summary>
    /// Processes @font-face rules from stylesheets and registers fonts with the font provider.
    /// </summary>
    internal static class FontFaceProcessor
    {
        public static void Process(IReadOnlyList<CssRule> rules, IFontProvider fontProvider, Func<string, byte[]?>? resourceLoader = null)
        {
            for (int i = 0; i < rules.Count; i++)
            {
                if (rules[i] is FontFaceRule fontFace)
                    ProcessFontFace(fontFace, fontProvider, resourceLoader);
            }
        }

        private static void ProcessFontFace(FontFaceRule rule, IFontProvider fontProvider, Func<string, byte[]?>? resourceLoader)
        {
            string? familyName = null;
            string? srcUrl = null;

            for (int i = 0; i < rule.Declarations.Count; i++)
            {
                var decl = rule.Declarations[i];
                if (decl.Property == "font-family")
                {
                    familyName = ExtractStringValue(decl.Value);
                }
                else if (decl.Property == "src")
                {
                    srcUrl = ExtractUrlValue(decl.Value);
                }
            }

            if (familyName == null || srcUrl == null) return;

            // Try to load the font data
            byte[]? fontData = null;

            if (srcUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                fontData = DecodeDataUri(srcUrl);
            }
            else if (resourceLoader != null)
            {
                fontData = resourceLoader(srcUrl);
            }

            if (fontData != null && fontData.Length > 0)
            {
                fontProvider.RegisterFont(fontData, familyName);
            }
        }

        private static string? ExtractStringValue(CssValue value)
        {
            if (value is CssStringValue sv) return sv.Value;
            if (value is CssKeywordValue kv) return kv.Keyword;
            if (value is CssListValue lv && lv.Values.Count > 0)
                return ExtractStringValue(lv.Values[0]);
            return value.ToString().Trim('"', '\'');
        }

        private static string? ExtractUrlValue(CssValue value)
        {
            if (value is CssUrlValue uv) return uv.Url;
            if (value is CssFunctionValue fv && fv.Name == "url" && fv.Arguments.Count > 0)
            {
                if (fv.Arguments[0] is CssStringValue sv) return sv.Value;
                return fv.Arguments[0].ToString().Trim('"', '\'');
            }
            if (value is CssListValue lv)
            {
                for (int i = 0; i < lv.Values.Count; i++)
                {
                    var url = ExtractUrlValue(lv.Values[i]);
                    if (url != null) return url;
                }
            }
            return null;
        }

        private static byte[]? DecodeDataUri(string dataUri)
        {
            // data:[mediatype][;base64],data
            int commaIndex = dataUri.IndexOf(',');
            if (commaIndex < 0) return null;

            string header = dataUri.Substring(0, commaIndex);
            string data = dataUri.Substring(commaIndex + 1);

            if (header.EndsWith(";base64", StringComparison.OrdinalIgnoreCase))
            {
                try { return Convert.FromBase64String(data); }
                catch { return null; }
            }

            return null;
        }
    }
}
