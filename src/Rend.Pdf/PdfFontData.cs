using System;
using System.Collections.Generic;
using System.IO;
using Rend.Pdf.Fonts;

namespace Rend.Pdf
{
    /// <summary>
    /// Pre-parsed font data that can be reused across multiple PdfDocument instances.
    /// Caches the expensive TrueType/OpenType parsing so that AddFont() is nearly free.
    /// </summary>
    /// <example>
    /// <code>
    /// // Parse once, reuse many times
    /// var fontData = PdfFontData.FromFile("arial.ttf");
    ///
    /// for (int i = 0; i &lt; 1000; i++)
    /// {
    ///     using var doc = new PdfDocument();
    ///     var font = doc.AddFont(fontData);  // ~0 cost — no re-parsing
    ///     // ...
    /// }
    /// </code>
    /// </example>
    public sealed class PdfFontData
    {
        internal string BaseFontName { get; }
        internal FontMetrics Metrics { get; }
        internal ushort[] CharToGlyph { get; }
        internal float[] AdvanceWidths { get; }
        internal Dictionary<int, ushort>? SupplementaryMap { get; }
        internal Dictionary<uint, short>? KerningPairs { get; }
        internal bool IsCff { get; }
        internal byte[]? CffTableData { get; }
        internal FontEmbedMode EmbedMode { get; }
        internal byte[] RawFontBytes { get; }
        internal bool IsType1 { get; }
        internal byte[]? Type1Header { get; }
        internal byte[]? Type1Encrypted { get; }
        internal byte[]? Type1Trailer { get; }

        internal PdfFontData(string baseFontName, FontMetrics metrics,
                             ushort[] charToGlyph, float[] advanceWidths,
                             Dictionary<int, ushort>? supplementaryMap,
                             Dictionary<uint, short>? kerningPairs,
                             bool isCff, byte[]? cffTableData,
                             FontEmbedMode embedMode, byte[] rawFontBytes,
                             bool isType1 = false, byte[]? type1Header = null,
                             byte[]? type1Encrypted = null, byte[]? type1Trailer = null)
        {
            BaseFontName = baseFontName;
            Metrics = metrics;
            CharToGlyph = charToGlyph;
            AdvanceWidths = advanceWidths;
            SupplementaryMap = supplementaryMap;
            KerningPairs = kerningPairs;
            IsCff = isCff;
            CffTableData = cffTableData;
            EmbedMode = embedMode;
            RawFontBytes = rawFontBytes;
            IsType1 = isType1;
            Type1Header = type1Header;
            Type1Encrypted = type1Encrypted;
            Type1Trailer = type1Trailer;
        }

        /// <summary>
        /// Parse a TrueType/OpenType/Type1 font from a byte array.
        /// The result can be reused across multiple PdfDocument instances.
        /// </summary>
        public static PdfFontData FromBytes(byte[] fontBytes, FontEmbedMode mode = FontEmbedMode.Subset)
        {
            if (fontBytes == null) throw new ArgumentNullException(nameof(fontBytes));
            return TrueTypeParser.ParseToFontData(fontBytes, mode);
        }

        /// <summary>
        /// Parse a TrueType/OpenType/Type1 font from a file path.
        /// The result can be reused across multiple PdfDocument instances.
        /// </summary>
        public static PdfFontData FromFile(string fontFilePath, FontEmbedMode mode = FontEmbedMode.Subset)
        {
            var fontBytes = File.ReadAllBytes(fontFilePath);
            return FromBytes(fontBytes, mode);
        }

        /// <summary>
        /// Parse a TrueType/OpenType/Type1 font from a stream.
        /// The result can be reused across multiple PdfDocument instances.
        /// </summary>
        public static PdfFontData FromStream(Stream fontStream, FontEmbedMode mode = FontEmbedMode.Subset)
        {
            if (fontStream == null) throw new ArgumentNullException(nameof(fontStream));

            byte[] fontBytes;
            using (var ms = new MemoryStream())
            {
                fontStream.CopyTo(ms);
                fontBytes = ms.ToArray();
            }

            return FromBytes(fontBytes, mode);
        }
    }
}
