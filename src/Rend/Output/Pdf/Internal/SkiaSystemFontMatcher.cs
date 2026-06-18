#nullable enable
using Rend.Css;
using Rend.Fonts;
using SkiaSharp;

namespace Rend.Output.Pdf.Internal
{
    /// <summary>
    /// Last-resort font resolution for the PDF bridge: when the configured font provider cannot
    /// resolve a descriptor, ask the platform font manager (SkiaSharp's <see cref="SKFontManager"/>)
    /// for the closest match and return its raw font bytes for embedding. This catches fonts the
    /// provider's directory scan missed (platform-aliased families, fontconfig-only fonts) instead
    /// of silently dropping to Helvetica.
    /// </summary>
    internal static class SkiaSystemFontMatcher
    {
        public static byte[]? TryResolveFontData(FontDescriptor descriptor, out string matchedFamily)
        {
            matchedFamily = "";
            SKFontStyle style = ToFontStyle(descriptor);

            foreach (string family in descriptor.Families)
            {
                if (string.IsNullOrWhiteSpace(family))
                {
                    continue;
                }

                using SKTypeface? typeface = SKFontManager.Default.MatchFamily(family, style);
                if (typeface == null)
                {
                    continue;
                }

                byte[]? bytes = ExtractFontData(typeface);
                if (bytes != null)
                {
                    matchedFamily = typeface.FamilyName ?? family;
                    return bytes;
                }
            }

            return null;
        }

        private static SKFontStyle ToFontStyle(FontDescriptor descriptor)
        {
            int weight = (int)descriptor.Weight;
            int width = StretchToSkiaWidth(descriptor.Stretch);
            SKFontStyleSlant slant = descriptor.Style == CssFontStyle.Normal
                ? SKFontStyleSlant.Upright
                : (descriptor.Style == CssFontStyle.Oblique ? SKFontStyleSlant.Oblique : SKFontStyleSlant.Italic);
            return new SKFontStyle(weight, width, slant);
        }

        // CSS font-stretch percentage → OpenType usWidthClass (1–9), which is SKFontStyle's width.
        private static int StretchToSkiaWidth(float stretchPercent)
        {
            if (stretchPercent <= 50f) { return (int)SKFontStyleWidth.UltraCondensed; }
            if (stretchPercent <= 62.5f) { return (int)SKFontStyleWidth.ExtraCondensed; }
            if (stretchPercent <= 75f) { return (int)SKFontStyleWidth.Condensed; }
            if (stretchPercent <= 87.5f) { return (int)SKFontStyleWidth.SemiCondensed; }
            if (stretchPercent < 112.5f) { return (int)SKFontStyleWidth.Normal; }
            if (stretchPercent < 125f) { return (int)SKFontStyleWidth.SemiExpanded; }
            if (stretchPercent < 150f) { return (int)SKFontStyleWidth.Expanded; }
            if (stretchPercent < 200f) { return (int)SKFontStyleWidth.ExtraExpanded; }
            return (int)SKFontStyleWidth.UltraExpanded;
        }

        private static byte[]? ExtractFontData(SKTypeface typeface)
        {
            using SKStreamAsset? stream = typeface.OpenStream(out _);
            if (stream == null)
            {
                return null;
            }

            long length = stream.Length;
            if (length <= 0 || length > int.MaxValue)
            {
                return null;
            }

            var buffer = new byte[length];
            int read = stream.Read(buffer, (int)length);
            if (read != length)
            {
                return null;
            }

            // Decline TrueType/OpenType Collections (`ttcf`): the embedder reads a single sfnt and
            // OpenStream discards the face index, so embedding a collection would be wrong. Returning
            // null lets the caller surface a fallback warning instead of silently mis-embedding.
            if (buffer.Length >= 4 && buffer[0] == (byte)'t' && buffer[1] == (byte)'t'
                && buffer[2] == (byte)'c' && buffer[3] == (byte)'f')
            {
                return null;
            }

            return buffer;
        }
    }
}
