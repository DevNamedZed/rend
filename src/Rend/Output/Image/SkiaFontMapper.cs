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
                {
                    hash = hash * 31 + data[i];
                }
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
    /// All caches are instance-level: disposing the mapper frees all native typeface memory.
    /// </summary>
    public sealed class SkiaFontMapper : IDisposable
    {
        private const int MaxCacheEntries = 30;

        private readonly Dictionary<FontDescriptor, SKTypeface> _cache = new Dictionary<FontDescriptor, SKTypeface>();
        private readonly Dictionary<FontDataKey, SKTypeface> _typefaceCache = new();
        private readonly Dictionary<string, SKTypeface> _familyCache = new();
        private readonly Dictionary<int, SKTypeface?> _charFallbackCache = new();
        private readonly object _lock = new();
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
                var dataKey = new FontDataKey(fontData);
                lock (_lock)
                {
                    if (!_typefaceCache.TryGetValue(dataKey, out typeface!))
                    {
                        using (var skData = SKData.CreateCopy(fontData))
                        {
                            typeface = SKTypeface.FromData(skData);
                        }

                        if (typeface == null)
                        {
                            typeface = SKTypeface.Default;
                        }

                        EvictIfNeeded(_typefaceCache, MaxCacheEntries);
                        _typefaceCache[dataKey] = typeface;
                    }
                }
            }
            else
            {
                typeface = ResolveByFamilyName(descriptor);
            }

            _cache[descriptor] = typeface;
            return typeface;
        }

        // Generic CSS family -> concrete family names (shared with FontMatchingAlgorithm).
#if NET8_0_OR_GREATER
        private static readonly FrozenDictionary<string, string[]> GenericFamilyMap =
            GenericFontFamilies.FallbackMap.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
#else
        private static readonly Dictionary<string, string[]> GenericFamilyMap = GenericFontFamilies.FallbackMap;
#endif

        /// <summary>
        /// Gets or creates a shared SKTypeface from font data bytes.
        /// Returns the same instance for identical byte content (by FontDataKey).
        /// </summary>
        internal SKTypeface GetOrCreateTypeface(byte[] fontData)
        {
            var dataKey = new FontDataKey(fontData);
            lock (_lock)
            {
                if (_typefaceCache.TryGetValue(dataKey, out var existing))
                {
                    return existing;
                }

                SKTypeface typeface;
                using (var skData = SKData.CreateCopy(fontData))
                {
                    typeface = SKTypeface.FromData(skData) ?? SKTypeface.Default;
                }

                EvictIfNeeded(_typefaceCache, MaxCacheEntries);
                _typefaceCache[dataKey] = typeface;
                return typeface;
            }
        }

        /// <summary>
        /// Gets or creates a cached fallback typeface for a Unicode codepoint.
        /// Uses SKFontManager.Default.MatchCharacter for resolution.
        /// </summary>
        /// <param name="codepoint">The Unicode codepoint to find a typeface for.</param>
        /// <param name="resolver">A function that resolves the codepoint to a typeface (typically SKFontManager.Default.MatchCharacter).</param>
        /// <returns>A cached typeface for the codepoint, or null if no fallback was found.</returns>
        internal SKTypeface? GetCharacterFallback(int codepoint, Func<int, SKTypeface?> resolver)
        {
            lock (_lock)
            {
                if (_charFallbackCache.TryGetValue(codepoint, out var cached))
                {
                    return cached;
                }
            }

            var typeface = resolver(codepoint);

            lock (_lock)
            {
                if (_charFallbackCache.TryGetValue(codepoint, out var existing))
                {
                    typeface?.Dispose();
                    return existing;
                }

                EvictIfNeeded(_charFallbackCache, MaxCacheEntries);
                _charFallbackCache[codepoint] = typeface;
            }
            return typeface;
        }

        private SKTypeface ResolveByFamilyName(FontDescriptor descriptor)
        {
            string cacheKey = string.Join(",", descriptor.Families) + "|" + (int)descriptor.Weight + "|" + (int)descriptor.Style;
            lock (_lock)
            {
                if (_familyCache.TryGetValue(cacheKey, out var cached))
                {
                    return cached;
                }
            }

            SKFontStyleWeight weight = (SKFontStyleWeight)(int)descriptor.Weight;
            SKFontStyleSlant slant = descriptor.Style == Css.CssFontStyle.Italic ? SKFontStyleSlant.Italic
                : descriptor.Style == Css.CssFontStyle.Oblique ? SKFontStyleSlant.Oblique
                : SKFontStyleSlant.Upright;
            var skStyle = new SKFontStyle(weight, SKFontStyleWidth.Normal, slant);

            var families = descriptor.Families;

            foreach (var family in families)
            {
                var typeface = SKTypeface.FromFamilyName(family, skStyle);
                if (typeface != null && !IsDefault(typeface, family))
                {
                    lock (_lock)
                    {
                        if (_familyCache.TryGetValue(cacheKey, out var existing))
                        {
                            typeface.Dispose();
                            return existing;
                        }

                        EvictIfNeeded(_familyCache, MaxCacheEntries);
                        _familyCache[cacheKey] = typeface;
                    }
                    return typeface;
                }
                typeface?.Dispose();

                if (GenericFamilyMap.TryGetValue(family, out var fallbacks))
                {
                    for (int i = 0; i < fallbacks.Length; i++)
                    {
                        typeface = SKTypeface.FromFamilyName(fallbacks[i], skStyle);
                        if (typeface != null && !IsDefault(typeface, fallbacks[i]))
                        {
                            lock (_lock)
                            {
                                if (_familyCache.TryGetValue(cacheKey, out var existing))
                                {
                                    typeface.Dispose();
                                    return existing;
                                }

                                EvictIfNeeded(_familyCache, MaxCacheEntries);
                                _familyCache[cacheKey] = typeface;
                            }
                            return typeface;
                        }
                        typeface?.Dispose();
                    }
                }
            }

            return SKTypeface.Default;
        }

        /// <summary>
        /// SkiaSharp's FromFamilyName never returns null -- it returns the default typeface
        /// when the requested family isn't found. Detect this by comparing family names.
        /// </summary>
        private static bool IsDefault(SKTypeface typeface, string requestedFamily)
        {
            if (string.Equals(typeface.FamilyName, requestedFamily, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (typeface.FamilyName != SKTypeface.Default.FamilyName)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Disposes an SKTypeface if it is not the Default and has not already been disposed.
        /// Uses IntPtr (native handle) for identity-based dedup that works across all .NET targets.
        /// </summary>
        private static void DisposeTypeface(SKTypeface? typeface, HashSet<IntPtr> disposedTypefaces)
        {
            if (typeface != null && typeface != SKTypeface.Default && typeface.Handle != IntPtr.Zero)
            {
                if (disposedTypefaces.Add(typeface.Handle))
                {
                    typeface.Dispose();
                }
            }
        }

        /// <summary>
        /// Evicts the oldest entry from a dictionary if it exceeds the maximum size.
        /// Uses the first key from the enumerator as the eviction target.
        /// </summary>
        private static void EvictIfNeeded<TKey, TValue>(Dictionary<TKey, TValue> dictionary, int maxEntries)
            where TKey : notnull
        {
            if (dictionary.Count < maxEntries)
            {
                return;
            }

            // Remove the first entry (oldest insertion order in .NET Dictionary).
            using var enumerator = dictionary.GetEnumerator();
            if (enumerator.MoveNext())
            {
                var keyToRemove = enumerator.Current.Key;
                var valueToRemove = enumerator.Current.Value;
                dictionary.Remove(keyToRemove);

                if (valueToRemove is SKTypeface typeface && typeface != SKTypeface.Default)
                {
                    typeface.Dispose();
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            lock (_lock)
            {
                // Track disposed typefaces by identity to avoid double-dispose.
                // A typeface may appear in multiple caches (e.g., _cache references
                // typefaces also stored in _typefaceCache or _familyCache).
                var disposedTypefaces = new HashSet<IntPtr>();

                foreach (var kvp in _typefaceCache)
                {
                    DisposeTypeface(kvp.Value, disposedTypefaces);
                }
                _typefaceCache.Clear();

                foreach (var kvp in _familyCache)
                {
                    DisposeTypeface(kvp.Value, disposedTypefaces);
                }
                _familyCache.Clear();

                foreach (var kvp in _charFallbackCache)
                {
                    DisposeTypeface(kvp.Value, disposedTypefaces);
                }
                _charFallbackCache.Clear();

                // The _cache entries reference typefaces that are also in the other caches,
                // so they've already been disposed above. Just clear.
                _cache.Clear();
            }
        }
    }
}
