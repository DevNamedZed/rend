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
    public sealed class SkiaFontMapper : IDisposable
    {
        private readonly Dictionary<FontDescriptor, SKTypeface> _cache = new Dictionary<FontDescriptor, SKTypeface>();
        // Global typeface cache shared across ALL mapper instances.
        // SKTypeface.FromData() produces subtly different SubpixelAntialias rendering
        // for each native instance, even from identical font bytes. Sharing a single
        // SKTypeface per font ensures deterministic rendering across threads.
        private static readonly Dictionary<int, SKTypeface> s_globalTypefaceCache = new();
        private static readonly object s_typefaceLock = new();
        private bool _disposed;

        /// <summary>
        /// Gets or creates an <see cref="SKTypeface"/> for the given font descriptor.
        /// </summary>
        /// <param name="descriptor">The font descriptor to resolve.</param>
        /// <param name="fontData">Raw font file bytes, or null to use the system default.</param>
        /// <returns>An SKTypeface for the font, or the default typeface if font data is unavailable.</returns>
        public SKTypeface GetOrCreate(FontDescriptor descriptor, byte[]? fontData)
        {
            if (_cache.TryGetValue(descriptor, out var existing))
            {
                return existing;
            }

            SKTypeface typeface;
            if (fontData != null && fontData.Length > 0)
            {
                // Use global cache so all threads share the same SKTypeface instance
                // for each font. SKTypeface.FromData() creates different native objects
                // that produce subtly different SubpixelAntialias rendering.
                int dataKey = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(fontData);
                lock (s_typefaceLock)
                {
                    if (!s_globalTypefaceCache.TryGetValue(dataKey, out typeface!))
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
                        s_globalTypefaceCache[dataKey] = typeface;
                    }
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
            ["monospace"] = new[] { "Consolas", "Courier New", "Courier", "Menlo", "DejaVu Sans Mono" },
            ["cursive"] = new[] { "Comic Sans MS", "Apple Chancery" },
            ["fantasy"] = new[] { "Impact", "Papyrus" },
            ["system-ui"] = new[] { ".AppleSystemUIFont", "Segoe UI", "Roboto", "Helvetica Neue", "Helvetica", "Arial" },
            ["ui-sans-serif"] = new[] { ".AppleSystemUIFont", "Segoe UI", "Roboto", "Helvetica Neue" },
            ["ui-serif"] = new[] { "New York", "Georgia", "Times New Roman" },
            ["ui-monospace"] = new[] { "SF Mono", "Menlo", "Consolas", "Courier New" },
        };

        /// <summary>
        /// Gets or creates a shared SKTypeface from font data bytes.
        /// Thread-safe. Returns the same instance for the same byte[] identity.
        /// </summary>
        internal static SKTypeface GetOrCreateSharedTypeface(byte[] fontData)
        {
            int dataKey = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(fontData);
            lock (s_typefaceLock)
            {
                if (s_globalTypefaceCache.TryGetValue(dataKey, out var existing))
                    return existing;

                SKTypeface typeface;
                using (var skData = SKData.CreateCopy(fontData))
                {
                    typeface = SKTypeface.FromData(skData) ?? SKTypeface.Default;
                }
                s_globalTypefaceCache[dataKey] = typeface;
                return typeface;
            }
        }

        // Shared cache for family-name resolved typefaces.
        // SKTypeface.FromFamilyName() can return different native objects per call,
        // causing non-deterministic SubpixelAntialias rendering across threads.
        private static readonly Dictionary<string, SKTypeface> s_familyTypefaceCache = new();

        private static SKTypeface ResolveByFamilyName(FontDescriptor descriptor)
        {
            // Check shared cache first.
            string cacheKey = descriptor.Family + "|" + (int)descriptor.Weight + "|" + (int)descriptor.Style;
            lock (s_typefaceLock)
            {
                if (s_familyTypefaceCache.TryGetValue(cacheKey, out var cached))
                    return cached;
            }

            // Map CSS font-weight to SKFontStyleWeight.
            SKFontStyleWeight weight = (SKFontStyleWeight)(int)descriptor.Weight;
            SKFontStyleSlant slant = descriptor.Style == Css.CssFontStyle.Italic ? SKFontStyleSlant.Italic
                : descriptor.Style == Css.CssFontStyle.Oblique ? SKFontStyleSlant.Oblique
                : SKFontStyleSlant.Upright;
            var skStyle = new SKFontStyle(weight, SKFontStyleWidth.Normal, slant);

            // CSS font-family may be a comma-separated list.
            // Parse and try each family in order.
            var families = Fonts.FontMatchingAlgorithm.ParseFontFamilyList(descriptor.Family);

            foreach (var family in families)
            {
                // Try the exact family name first.
                var tf = SKTypeface.FromFamilyName(family, skStyle);
                if (tf != null && !IsDefault(tf, family))
                {
                    lock (s_typefaceLock)
                    {
                        if (s_familyTypefaceCache.TryGetValue(cacheKey, out var existing))
                        {
                            tf.Dispose();
                            return existing;
                        }
                        s_familyTypefaceCache[cacheKey] = tf;
                    }
                    return tf;
                }
                tf?.Dispose();

                // Try generic CSS family name fallbacks.
                if (GenericFamilyMap.TryGetValue(family, out var fallbacks))
                {
                    for (int i = 0; i < fallbacks.Length; i++)
                    {
                        tf = SKTypeface.FromFamilyName(fallbacks[i], skStyle);
                        if (tf != null && !IsDefault(tf, fallbacks[i]))
                        {
                            lock (s_typefaceLock)
                            {
                                if (s_familyTypefaceCache.TryGetValue(cacheKey, out var existing))
                                {
                                    tf.Dispose();
                                    return existing;
                                }
                                s_familyTypefaceCache[cacheKey] = tf;
                            }
                            return tf;
                        }
                        tf?.Dispose();
                    }
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
                // Don't dispose any shared typefaces (from global data cache or family cache).
                lock (s_typefaceLock)
                {
                    foreach (var kvp in _cache)
                    {
                        if (kvp.Value != SKTypeface.Default
                            && !s_globalTypefaceCache.ContainsValue(kvp.Value)
                            && !s_familyTypefaceCache.ContainsValue(kvp.Value))
                        {
                            kvp.Value.Dispose();
                        }
                    }
                }
                _cache.Clear();
            }
        }
    }
}
