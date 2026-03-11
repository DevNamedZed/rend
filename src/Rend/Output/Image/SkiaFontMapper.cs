using System;
using System.Collections.Generic;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif
using System.Linq;
using Rend.Css;
using Rend.Fonts;
using SkiaSharp;

namespace Rend.Output.Image
{
    /// <summary>
    /// Content-based key for font data bytes. Uses length plus sampled byte hash
    /// so that identical font content always maps to the same cache entry,
    /// regardless of array identity.
    /// </summary>
    internal readonly struct FontDataKey : IEquatable<FontDataKey>
    {
        private readonly int _hash;
        private readonly int _length;

        public FontDataKey(byte[] data)
        {
            _length = data.Length;
            unchecked
            {
                int hash = data.Length;
                int step = Math.Max(1, data.Length / 16);
                for (int i = 0; i < data.Length; i += step)
                    hash = hash * 31 + data[i];
                _hash = hash;
            }
        }

        public override int GetHashCode() => _hash;
        public override bool Equals(object? obj) => obj is FontDataKey other && Equals(other);
        public bool Equals(FontDataKey other) => _hash == other._hash && _length == other._length;
    }

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
        private static readonly Dictionary<FontDataKey, SKTypeface> s_globalTypefaceCache = new();
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
                var dataKey = new FontDataKey(fontData);
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

        // Generic CSS family → concrete family names (shared with FontMatchingAlgorithm).
#if NET8_0_OR_GREATER
        private static readonly FrozenDictionary<string, string[]> GenericFamilyMap =
            GenericFontFamilies.FallbackMap.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
#else
        private static readonly Dictionary<string, string[]> GenericFamilyMap = GenericFontFamilies.FallbackMap;
#endif

        /// <summary>
        /// Gets or creates a shared SKTypeface from font data bytes.
        /// Thread-safe. Returns the same instance for the same byte[] identity.
        /// </summary>
        internal static SKTypeface GetOrCreateSharedTypeface(byte[] fontData)
        {
            var dataKey = new FontDataKey(fontData);
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

        /// <summary>
        /// Disposes all cached typefaces in the static global and family caches,
        /// then clears them. Call this in long-running services to reclaim native memory.
        /// </summary>
        public static void ClearCache()
        {
            lock (s_typefaceLock)
            {
                foreach (var kvp in s_globalTypefaceCache)
                {
                    if (kvp.Value != null && kvp.Value != SKTypeface.Default)
                        kvp.Value.Dispose();
                }
                s_globalTypefaceCache.Clear();

                foreach (var kvp in s_familyTypefaceCache)
                {
                    if (kvp.Value != null && kvp.Value != SKTypeface.Default)
                        kvp.Value.Dispose();
                }
                s_familyTypefaceCache.Clear();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                // Build a HashSet of shared typefaces for O(1) lookups
                // instead of O(n) ContainsValue per cache entry.
                HashSet<SKTypeface> sharedTypefaces;
                lock (s_typefaceLock)
                {
                    sharedTypefaces = new HashSet<SKTypeface>(
                        s_globalTypefaceCache.Values.Concat(s_familyTypefaceCache.Values));
                }

                foreach (var kvp in _cache)
                {
                    if (kvp.Value != SKTypeface.Default
                        && !sharedTypefaces.Contains(kvp.Value))
                    {
                        kvp.Value.Dispose();
                    }
                }
                _cache.Clear();
            }
        }
    }
}
