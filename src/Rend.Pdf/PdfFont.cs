using System;
using System.Collections.Generic;

namespace Rend.Pdf
{
    /// <summary>
    /// Represents a font resource in a PDF document.
    /// Handles character-to-glyph mapping, width measurement, and encoding.
    /// </summary>
    public sealed class PdfFont
    {
        private readonly string _baseFont;
        private readonly bool _isStandard14;
        private readonly bool _isCff;
        private readonly byte[]? _cffTableData;
        private readonly bool _isType1;
        private readonly byte[]? _type1Header;
        private readonly byte[]? _type1Encrypted;
        private readonly byte[]? _type1Trailer;
        private readonly FontMetrics _metrics;
        private readonly FontEmbedMode _embedMode;

        // Glyph cache: char -> glyph ID (BMP direct lookup, supplementary via dictionary)
        private readonly ushort[] _charToGlyph;
        private readonly Dictionary<int, ushort>? _supplementaryMap;

        // Glyph advance widths indexed by glyph ID (in font units, 1/1000 of text space)
        private readonly float[] _advanceWidths;

        // Kerning pairs: key = (leftGlyphId << 16) | rightGlyphId, value = adjustment in font units
        private readonly Dictionary<uint, short>? _kerningPairs;

        // Track which glyphs have been used (for subsetting)
        private readonly HashSet<ushort> _usedGlyphs = new HashSet<ushort>();

        // Track char -> glyph mapping for ToUnicode CMap generation
        private readonly Dictionary<ushort, int> _glyphToUnicode = new Dictionary<ushort, int>();

        internal PdfFont(string baseFont, FontMetrics metrics, ushort[] charToGlyph,
                         float[] advanceWidths, Dictionary<int, ushort>? supplementaryMap,
                         bool isStandard14, Dictionary<uint, short>? kerningPairs = null,
                         FontEmbedMode embedMode = FontEmbedMode.Subset,
                         bool isCff = false, byte[]? cffTableData = null,
                         bool isType1 = false, byte[]? type1Header = null,
                         byte[]? type1Encrypted = null, byte[]? type1Trailer = null)
        {
            _baseFont = baseFont;
            _metrics = metrics;
            _charToGlyph = charToGlyph;
            _advanceWidths = advanceWidths;
            _supplementaryMap = supplementaryMap;
            _isStandard14 = isStandard14;
            _kerningPairs = kerningPairs;
            _embedMode = embedMode;
            _isCff = isCff;
            _cffTableData = cffTableData;
            _isType1 = isType1;
            _type1Header = type1Header;
            _type1Encrypted = type1Encrypted;
            _type1Trailer = type1Trailer;
        }

        /// <summary>PostScript base font name.</summary>
        public string BaseFont => _baseFont;

        /// <summary>Font metrics (ascent, descent, etc.).</summary>
        public FontMetrics Metrics => _metrics;

        /// <summary>Whether this is a Standard 14 font (not embedded).</summary>
        public bool IsStandard14 => _isStandard14;

        /// <summary>Whether this font uses CFF outlines (OpenType CFF / .otf).</summary>
        internal bool IsCff => _isCff;

        /// <summary>Raw CFF table data for embedding as FontFile3.</summary>
        internal byte[]? CffTableData => _cffTableData;

        /// <summary>Whether this is a Type 1 PostScript font (PFB/PFA).</summary>
        internal bool IsType1 => _isType1;

        /// <summary>ASCII header segment of a Type 1 font (before eexec).</summary>
        internal byte[]? Type1Header => _type1Header;

        /// <summary>Binary encrypted segment of a Type 1 font (eexec data).</summary>
        internal byte[]? Type1Encrypted => _type1Encrypted;

        /// <summary>ASCII trailer segment of a Type 1 font (after cleartomark).</summary>
        internal byte[]? Type1Trailer => _type1Trailer;

        /// <summary>Font embedding mode.</summary>
        internal FontEmbedMode EmbedMode => _embedMode;

        /// <summary>Set of glyph IDs used so far (for subsetting).</summary>
        internal IReadOnlyCollection<ushort> UsedGlyphs => _usedGlyphs;

        /// <summary>Glyph ID to Unicode code point mapping (for ToUnicode CMap).</summary>
        internal IReadOnlyDictionary<ushort, int> GlyphToUnicode => _glyphToUnicode;

        /// <summary>
        /// Get the glyph ID for a Unicode code point. Records it as used.
        /// </summary>
        public ushort GetGlyphId(int codePoint)
        {
            ushort glyphId;
            if (codePoint >= 0 && codePoint < _charToGlyph.Length)
            {
                glyphId = _charToGlyph[codePoint];
            }
            else if (_supplementaryMap != null && _supplementaryMap.TryGetValue(codePoint, out glyphId))
            {
                // found via dictionary
            }
            else
            {
                glyphId = 0; // .notdef
            }

            if (glyphId != 0)
            {
                _usedGlyphs.Add(glyphId);
                if (!_glyphToUnicode.ContainsKey(glyphId))
                    _glyphToUnicode[glyphId] = codePoint;
            }

            return glyphId;
        }

        /// <summary>
        /// Encode a Unicode string to bytes for a PDF text operator.
        /// For CIDFont: 2 bytes per glyph (big-endian glyph ID).
        /// </summary>
        public byte[] Encode(string text)
        {
            var bytes = new byte[text.Length * 2]; // worst case: all BMP
            int bytePos = 0;

            for (int i = 0; i < text.Length; i++)
            {
                int codePoint;
                if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                {
                    codePoint = char.ConvertToUtf32(text[i], text[i + 1]);
                    i++;
                }
                else
                {
                    codePoint = text[i];
                }

                ushort glyphId = GetGlyphId(codePoint);

                // Ensure we have space (supplementary chars could cause initial estimate to be short)
                if (bytePos + 2 > bytes.Length)
                {
                    var newBytes = new byte[bytes.Length * 2];
                    Buffer.BlockCopy(bytes, 0, newBytes, 0, bytePos);
                    bytes = newBytes;
                }

                bytes[bytePos++] = (byte)(glyphId >> 8);
                bytes[bytePos++] = (byte)(glyphId & 0xFF);
            }

            if (bytePos != bytes.Length)
            {
                var result = new byte[bytePos];
                Buffer.BlockCopy(bytes, 0, result, 0, bytePos);
                return result;
            }
            return bytes;
        }

        /// <summary>
        /// Measure the width of a text string in points.
        /// </summary>
        public float MeasureWidth(string text, float fontSize)
        {
            float totalWidth = 0;
            for (int i = 0; i < text.Length; i++)
            {
                int codePoint;
                if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                {
                    codePoint = char.ConvertToUtf32(text[i], text[i + 1]);
                    i++;
                }
                else
                {
                    codePoint = text[i];
                }

                ushort glyphId = GetGlyphId(codePoint);
                if (glyphId < _advanceWidths.Length)
                    totalWidth += _advanceWidths[glyphId];
            }

            return totalWidth * fontSize / _metrics.UnitsPerEm;
        }

        /// <summary>
        /// Get the advance width for a glyph ID in font units.
        /// </summary>
        public float GetAdvanceWidth(ushort glyphId)
        {
            return glyphId < _advanceWidths.Length ? _advanceWidths[glyphId] : 0;
        }

        /// <summary>
        /// Get the kerning adjustment between two glyphs in font units.
        /// Returns 0 if no kerning pair exists or the font has no kerning data.
        /// </summary>
        public float GetKerning(ushort leftGlyphId, ushort rightGlyphId)
        {
            if (_kerningPairs == null) return 0;

            uint key = ((uint)leftGlyphId << 16) | rightGlyphId;
            return _kerningPairs.TryGetValue(key, out short value) ? value : 0;
        }

        /// <summary>Whether this font has kerning data.</summary>
        public bool HasKerning => _kerningPairs != null && _kerningPairs.Count > 0;

        /// <summary>
        /// Bulk glyph ID lookup. Converts a character span to glyph IDs and records usage.
        /// Returns the number of glyph IDs written (may differ from text length due to surrogate pairs).
        /// </summary>
        public int GetGlyphIds(ReadOnlySpan<char> text, Span<ushort> glyphIds)
        {
            int glyphIdx = 0;
            for (int i = 0; i < text.Length; i++)
            {
                int codePoint;
                if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                {
                    codePoint = char.ConvertToUtf32(text[i], text[i + 1]);
                    i++;
                }
                else
                {
                    codePoint = text[i];
                }

                ushort glyphId = GetGlyphId(codePoint);
                if (glyphIdx < glyphIds.Length)
                    glyphIds[glyphIdx] = glyphId;
                glyphIdx++;
            }
            return glyphIdx;
        }

        /// <summary>
        /// Bulk advance width query. Gets advance widths for an array of glyph IDs.
        /// </summary>
        public void GetAdvanceWidths(ReadOnlySpan<ushort> glyphIds, Span<float> advances)
        {
            int count = Math.Min(glyphIds.Length, advances.Length);
            for (int i = 0; i < count; i++)
            {
                advances[i] = glyphIds[i] < _advanceWidths.Length ? _advanceWidths[glyphIds[i]] : 0;
            }
        }

        /// <summary>
        /// Record a glyph ID as used (for subsetting). Called internally when using
        /// glyph-level APIs that bypass the normal Encode/GetGlyphId path.
        /// </summary>
        internal void RecordGlyphUsage(ushort glyphId)
        {
            if (glyphId != 0)
                _usedGlyphs.Add(glyphId);
        }

        /// <summary>
        /// Encode a single glyph ID to 2 bytes big-endian (for CIDFont).
        /// </summary>
        internal static void EncodeGlyphId(ushort glyphId, byte[] buffer, int offset)
        {
            buffer[offset] = (byte)(glyphId >> 8);
            buffer[offset + 1] = (byte)(glyphId & 0xFF);
        }

        /// <summary>Total number of glyphs in the font.</summary>
        internal int GlyphCount => _advanceWidths.Length;
    }

    /// <summary>
    /// Font metric data extracted from the font file.
    /// All values in font units unless noted.
    /// </summary>
    public readonly struct FontMetrics
    {
        /// <summary>Typographic ascent (above baseline).</summary>
        public float Ascent { get; }

        /// <summary>Typographic descent (below baseline, typically negative).</summary>
        public float Descent { get; }

        /// <summary>Cap height (height of uppercase letters).</summary>
        public float CapHeight { get; }

        /// <summary>x-height (height of lowercase 'x').</summary>
        public float XHeight { get; }

        /// <summary>Dominant vertical stem width.</summary>
        public float StemV { get; }

        /// <summary>Italic angle in degrees (0 for upright).</summary>
        public float ItalicAngle { get; }

        /// <summary>Font bounding box.</summary>
        public Core.Values.RectF BBox { get; }

        /// <summary>Units per em (typically 1000 for PostScript or 2048 for TrueType).</summary>
        public float UnitsPerEm { get; }

        /// <summary>Font flags for PDF FontDescriptor.</summary>
        public int Flags { get; }

        public FontMetrics(float ascent, float descent, float capHeight, float xHeight,
                           float stemV, float italicAngle, Core.Values.RectF bBox,
                           float unitsPerEm, int flags)
        {
            Ascent = ascent;
            Descent = descent;
            CapHeight = capHeight;
            XHeight = xHeight;
            StemV = stemV;
            ItalicAngle = italicAngle;
            BBox = bBox;
            UnitsPerEm = unitsPerEm;
            Flags = flags;
        }
    }
}
