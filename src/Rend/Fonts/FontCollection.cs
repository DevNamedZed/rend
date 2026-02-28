using System;
using System.Collections.Generic;
using System.IO;
using Rend.Css;
using Rend.Fonts.Internal;

namespace Rend.Fonts
{
    /// <summary>
    /// A concrete <see cref="IFontProvider"/> that manages a collection of registered fonts
    /// and resolves requests using the CSS font matching algorithm.
    /// </summary>
    public sealed class FontCollection : IFontProvider
    {
        private readonly List<FontEntry> _entries = new List<FontEntry>();
        private readonly Dictionary<FontDescriptor, FontEntry?> _resolveCache = new Dictionary<FontDescriptor, FontEntry?>();
        private readonly object _lock = new object();

        /// <summary>
        /// Gets all registered font entries.
        /// </summary>
        public IReadOnlyList<FontEntry> Entries
        {
            get
            {
                lock (_lock)
                {
                    return _entries.ToArray();
                }
            }
        }

        /// <inheritdoc />
        public FontEntry? ResolveFont(FontDescriptor descriptor)
        {
            lock (_lock)
            {
                if (_resolveCache.TryGetValue(descriptor, out FontEntry? cached))
                    return cached;

                FontEntry? result = FontMatchingAlgorithm.FindBestMatch(descriptor, _entries);
                _resolveCache[descriptor] = result;
                return result;
            }
        }

        /// <inheritdoc />
        public FontMetricsInfo GetMetrics(FontDescriptor descriptor)
        {
            FontEntry? entry = ResolveFont(descriptor);
            if (entry != null)
                return entry.Metrics;

            // Return default metrics when no font is resolved.
            return new FontMetricsInfo(800, -200, 0, 1000, 700, 500);
        }

        /// <inheritdoc />
        public float MeasureCharWidth(FontDescriptor descriptor, int codePoint, float fontSize)
        {
            FontEntry? entry = ResolveFont(descriptor);
            if (entry != null)
                return entry.GetCharWidth(codePoint, fontSize);

            // Fallback: assume half-em width.
            return fontSize * 0.5f;
        }

        /// <inheritdoc />
        public void RegisterFont(byte[] fontData, string? familyNameOverride = null)
        {
            if (fontData == null) throw new ArgumentNullException(nameof(fontData));

            // Detect format and decompress if needed.
            byte[] sfntData = EnsureSfnt(fontData);

            OpenTypeFontData parsed;
            try
            {
                parsed = new OpenTypeFontData(sfntData);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to parse font data: " + ex.Message, ex);
            }

            string familyName = familyNameOverride ?? parsed.FamilyName;
            if (string.IsNullOrEmpty(familyName))
                familyName = "Unknown";

            CssFontStyle style = parsed.IsItalic ? CssFontStyle.Italic : CssFontStyle.Normal;
            float weight = parsed.Weight > 0 ? parsed.Weight : 400f;

            var descriptor = new FontDescriptor(familyName, weight, style);
            FontMetricsInfo metrics = parsed.BuildMetrics();

            var entry = new FontEntry(descriptor, sfntData, metrics, familyName, null, parsed);

            lock (_lock)
            {
                _entries.Add(entry);
                _resolveCache.Clear();
            }
        }

        /// <inheritdoc />
        public void RegisterFontDirectory(string directoryPath)
        {
            if (directoryPath == null) throw new ArgumentNullException(nameof(directoryPath));
            if (!Directory.Exists(directoryPath)) return;

            var resolver = new DirectoryFontResolver(directoryPath);
            foreach (string path in resolver.GetFontPaths())
            {
                byte[] data;
                try
                {
                    data = File.ReadAllBytes(path);
                }
                catch (IOException)
                {
                    continue;
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }

                try
                {
                    RegisterFont(data);
                }
                catch (InvalidOperationException)
                {
                    // Skip fonts that fail to parse.
                }
            }
        }

        /// <summary>
        /// Registers all fonts discovered by a system font resolver.
        /// </summary>
        public void RegisterFromResolver(SystemFontResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));

            foreach (string path in resolver.GetFontPaths())
            {
                byte[] data;
                try
                {
                    data = File.ReadAllBytes(path);
                }
                catch (IOException)
                {
                    continue;
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }

                try
                {
                    RegisterFont(data);
                }
                catch (InvalidOperationException)
                {
                    // Skip fonts that fail to parse.
                }
            }
        }

        /// <summary>
        /// Registers all fonts discovered by the given composite resolver.
        /// </summary>
        public void RegisterFromResolver(CompositeFontResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));

            foreach (string path in resolver.GetFontPaths())
            {
                byte[] data;
                try
                {
                    data = File.ReadAllBytes(path);
                }
                catch (IOException)
                {
                    continue;
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }

                try
                {
                    RegisterFont(data);
                }
                catch (InvalidOperationException)
                {
                    // Skip fonts that fail to parse.
                }
            }
        }

        private static byte[] EnsureSfnt(byte[] data)
        {
            FontFileFormat format = FontFileDetector.Detect(data);
            switch (format)
            {
                case FontFileFormat.TrueType:
                case FontFileFormat.OpenType:
                    return data;

                case FontFileFormat.Woff:
                    return WoffDecompressor.Decompress(data);

                case FontFileFormat.Woff2:
                    return Woff2Decompressor.Decompress(data);

                default:
                    // Try parsing as-is; may be a valid sfnt without a recognized magic.
                    return data;
            }
        }
    }
}
