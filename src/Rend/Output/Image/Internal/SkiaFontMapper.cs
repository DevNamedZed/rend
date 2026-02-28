using System;
using System.Collections.Generic;
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
                typeface = SKTypeface.Default;
            }

            _cache[descriptor] = typeface;
            return typeface;
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
