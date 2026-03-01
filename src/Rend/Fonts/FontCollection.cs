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

            // Handle TTC (TrueType Collection) files.
            if (FontFileDetector.Detect(fontData) == FontFileFormat.TrueTypeCollection)
            {
                RegisterTtcFonts(fontData);
                return;
            }

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

        /// <summary>
        /// Registers all fonts from a TTC (TrueType Collection) file.
        /// Each font in the collection is extracted as a standalone sfnt and registered individually.
        /// </summary>
        private void RegisterTtcFonts(byte[] data)
        {
            // TTC header: 'ttcf' (4), majorVersion (2), minorVersion (2), numFonts (4)
            if (data.Length < 12) return;
            int numFonts = (data[8] << 24) | (data[9] << 16) | (data[10] << 8) | data[11];
            if (numFonts <= 0 || numFonts > 256) return;

            // Offset table: 4 bytes per font, starting at offset 12
            int offsetTableEnd = 12 + numFonts * 4;
            if (data.Length < offsetTableEnd) return;

            for (int i = 0; i < numFonts; i++)
            {
                int offsetPos = 12 + i * 4;
                int fontOffset = (data[offsetPos] << 24) | (data[offsetPos + 1] << 16)
                               | (data[offsetPos + 2] << 8) | data[offsetPos + 3];
                if (fontOffset < 0 || fontOffset >= data.Length) continue;

                try
                {
                    byte[] sfntData = ExtractSfntFromTtc(data, fontOffset);
                    RegisterFont(sfntData);
                }
                catch
                {
                    // Skip individual fonts that fail to parse/extract.
                }
            }
        }

        /// <summary>
        /// Extracts a standalone sfnt (TrueType/OpenType) from a TTC at the given offset.
        /// Copies the offset table header and all referenced table data into a new byte array.
        /// </summary>
        private static byte[] ExtractSfntFromTtc(byte[] ttcData, int sfntOffset)
        {
            if (sfntOffset + 12 > ttcData.Length)
                throw new InvalidOperationException("Invalid TTC font offset.");

            // Read the offset table header at sfntOffset
            ushort numTables = (ushort)((ttcData[sfntOffset + 4] << 8) | ttcData[sfntOffset + 5]);
            int dirEnd = sfntOffset + 12 + numTables * 16;
            if (dirEnd > ttcData.Length)
                throw new InvalidOperationException("TTC font directory extends beyond file.");

            // Calculate total size needed: 12 (header) + numTables * 16 (directory) + all table data
            int headerSize = 12 + numTables * 16;
            int totalTableSize = 0;
            for (int t = 0; t < numTables; t++)
            {
                int entryPos = sfntOffset + 12 + t * 16;
                int tableLen = (ttcData[entryPos + 12] << 24) | (ttcData[entryPos + 13] << 16)
                             | (ttcData[entryPos + 14] << 8) | ttcData[entryPos + 15];
                // Pad to 4-byte boundary
                totalTableSize += (tableLen + 3) & ~3;
            }

            byte[] result = new byte[headerSize + totalTableSize];

            // Copy the sfnt version (4 bytes)
            Array.Copy(ttcData, sfntOffset, result, 0, 4);
            // Copy numTables, searchRange, entrySelector, rangeShift (8 bytes)
            Array.Copy(ttcData, sfntOffset + 4, result, 4, 8);

            // Copy table directory entries and table data
            int dataOffset = headerSize;
            for (int t = 0; t < numTables; t++)
            {
                int srcEntry = sfntOffset + 12 + t * 16;
                int dstEntry = 12 + t * 16;

                // Copy tag and checksum (8 bytes)
                Array.Copy(ttcData, srcEntry, result, dstEntry, 8);

                // Read original offset and length
                int origOffset = (ttcData[srcEntry + 8] << 24) | (ttcData[srcEntry + 9] << 16)
                               | (ttcData[srcEntry + 10] << 8) | ttcData[srcEntry + 11];
                int tableLen = (ttcData[srcEntry + 12] << 24) | (ttcData[srcEntry + 13] << 16)
                             | (ttcData[srcEntry + 14] << 8) | ttcData[srcEntry + 15];

                // Write new offset
                result[dstEntry + 8] = (byte)(dataOffset >> 24);
                result[dstEntry + 9] = (byte)(dataOffset >> 16);
                result[dstEntry + 10] = (byte)(dataOffset >> 8);
                result[dstEntry + 11] = (byte)dataOffset;

                // Copy length unchanged
                Array.Copy(ttcData, srcEntry + 12, result, dstEntry + 12, 4);

                // Copy table data
                if (origOffset >= 0 && origOffset + tableLen <= ttcData.Length && dataOffset + tableLen <= result.Length)
                {
                    Array.Copy(ttcData, origOffset, result, dataOffset, tableLen);
                }

                dataOffset += (tableLen + 3) & ~3;
            }

            return result;
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
