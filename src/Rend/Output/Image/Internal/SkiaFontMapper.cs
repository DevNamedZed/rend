using System;
using System.Collections.Generic;
using Rend.Css;
using Rend.Fonts;
using SkiaSharp;

namespace Rend.Output.Image.Internal
{
    /// <summary>
    /// Maps <see cref="FontDescriptor"/> to <see cref="SKTypeface"/> instances,
    /// creating typefaces from raw font byte data when available.
    /// </summary>
    internal sealed class SkiaFontMapper : IDisposable
    {
        private readonly Dictionary<FontDescriptor, SKTypeface> _cache = new Dictionary<FontDescriptor, SKTypeface>();
        private bool _disposed;

        /// <summary>
        /// Gets or creates an <see cref="SKTypeface"/> for the given font descriptor.
        /// </summary>
        /// <param name="descriptor">The font descriptor to resolve.</param>
        /// <param name="fontData">Raw font file bytes, or null to use the system default.</param>
        /// <returns>An SKTypeface for the font, or the default typeface if font data is unavailable.</returns>
        internal SKTypeface GetOrCreate(FontDescriptor descriptor, byte[]? fontData)
        {
            if (_cache.TryGetValue(descriptor, out var existing))
            {
                return existing;
            }

            SKTypeface typeface;
            if (fontData != null && fontData.Length > 0)
            {
                using (var skData = SKData.CreateCopy(fontData))
                {
                    typeface = SKTypeface.FromData(skData);
                }

                // FromData may return null if the font data is invalid.
                if (typeface == null)
                {
                    typeface = SKTypeface.Default;
                }
            }
            else
            {
                // Try to resolve by family name so rendering uses the same font as layout.
                typeface = ResolveByFamilyName(descriptor);
            }

            _cache[descriptor] = typeface;
            return typeface;
        }

        // Generic CSS family → concrete family names (mirrors FontMatchingAlgorithm).
        private static readonly Dictionary<string, string[]> GenericFamilyMap = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["sans-serif"] = new[] { "Helvetica", "Helvetica Neue", "Arial", "Segoe UI", "DejaVu Sans" },
            ["serif"] = new[] { "Times New Roman", "Times", "Georgia", "DejaVu Serif" },
            ["monospace"] = new[] { "Courier New", "Courier", "Menlo", "Consolas", "DejaVu Sans Mono" },
            ["cursive"] = new[] { "Comic Sans MS", "Apple Chancery" },
            ["fantasy"] = new[] { "Impact", "Papyrus" },
            ["system-ui"] = new[] { ".AppleSystemUIFont", "Segoe UI", "Roboto", "Helvetica Neue", "Helvetica", "Arial" },
            ["ui-sans-serif"] = new[] { ".AppleSystemUIFont", "Segoe UI", "Roboto", "Helvetica Neue" },
            ["ui-serif"] = new[] { "New York", "Georgia", "Times New Roman" },
            ["ui-monospace"] = new[] { "SF Mono", "Menlo", "Consolas", "Courier New" },
        };

        private static SKTypeface ResolveByFamilyName(FontDescriptor descriptor)
        {
            // Map CSS font-weight to SKFontStyleWeight.
            SKFontStyleWeight weight = (SKFontStyleWeight)(int)descriptor.Weight;
            SKFontStyleSlant slant = descriptor.Style == Css.CssFontStyle.Italic ? SKFontStyleSlant.Italic
                : descriptor.Style == Css.CssFontStyle.Oblique ? SKFontStyleSlant.Oblique
                : SKFontStyleSlant.Upright;
            var skStyle = new SKFontStyle(weight, SKFontStyleWidth.Normal, slant);

            // Try the exact family name first.
            var tf = SKTypeface.FromFamilyName(descriptor.Family, skStyle);
            if (tf != null && !IsDefault(tf, descriptor.Family))
                return tf;
            tf?.Dispose();

            // Try generic CSS family name fallbacks.
            if (GenericFamilyMap.TryGetValue(descriptor.Family, out var fallbacks))
            {
                for (int i = 0; i < fallbacks.Length; i++)
                {
                    tf = SKTypeface.FromFamilyName(fallbacks[i], skStyle);
                    if (tf != null && !IsDefault(tf, fallbacks[i]))
                        return tf;
                    tf?.Dispose();
                }
            }

            return SKTypeface.Default;
        }

        /// <summary>
        /// SkiaSharp's FromFamilyName never returns null — it returns the default typeface
        /// when the requested family isn't found. Detect this by comparing family names.
        /// </summary>
        private static bool IsDefault(SKTypeface tf, string requestedFamily)
        {
            // If the returned typeface's family matches the request, it was found.
            if (string.Equals(tf.FamilyName, requestedFamily, StringComparison.OrdinalIgnoreCase))
                return false;
            // Also accept if it's clearly not the default (different family was resolved, e.g. alias).
            if (tf.FamilyName != SKTypeface.Default.FamilyName)
                return false;
            return true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                foreach (var kvp in _cache)
                {
                    // Do not dispose SKTypeface.Default.
                    if (kvp.Value != SKTypeface.Default)
                    {
                        kvp.Value.Dispose();
                    }
                }
                _cache.Clear();
            }
        }
    }
}
