using System;
using System.Collections.Generic;
using System.Text;

namespace Rend.Fonts.Internal
{
    /// <summary>
    /// Parses minimal OpenType/TrueType tables required for font metrics, glyph mapping, and advance widths.
    /// </summary>
    internal sealed class OpenTypeFontData
    {
        // Table tag constants (big-endian uint32).
        private const uint TagHead = 0x68656164; // "head"
        private const uint TagHhea = 0x68686561; // "hhea"
        private const uint TagHmtx = 0x686D7478; // "hmtx"
        private const uint TagOs2  = 0x4F532F32; // "OS/2"
        private const uint TagName = 0x6E616D65; // "name"
        private const uint TagCmap = 0x636D6170; // "cmap"
        private const uint TagMaxp = 0x6D617870; // "maxp"

        private readonly byte[] _data;
        private readonly Dictionary<uint, TableRecord> _tables;

        // Parsed metrics.
        private string _familyName = string.Empty;
        private int _unitsPerEm;
        private int _ascent;
        private int _descent;
        private int _lineGap;
        private float _weight;
        private bool _isItalic;
        private int _capHeight;
        private int _xHeight;
        private int _numberOfHMetrics;

        // hmtx advance widths (in font design units).
        private ushort[]? _advanceWidths;
        private ushort _lastAdvanceWidth;

        // cmap: code point -> glyph ID mapping (populated on demand for fallback lookups).
        private Dictionary<int, int>? _cmapCache = null;

        // cmap subtable data positions for lazy parsing.
        private int _cmapFormat4Offset = -1;
        private int _cmapFormat12Offset = -1;
        private int _numGlyphs;

        /// <summary>Gets the font family name from the name table.</summary>
        public string FamilyName => _familyName;

        /// <summary>Gets the units per em.</summary>
        public int UnitsPerEm => _unitsPerEm;

        /// <summary>Gets the typographic ascent.</summary>
        public int Ascent => _ascent;

        /// <summary>Gets the typographic descent (typically negative).</summary>
        public int Descent => _descent;

        /// <summary>Gets the typographic line gap.</summary>
        public int LineGap => _lineGap;

        /// <summary>Gets the font weight (e.g. 400 for regular, 700 for bold).</summary>
        public float Weight => _weight;

        /// <summary>Gets whether the font is italic.</summary>
        public bool IsItalic => _isItalic;

        /// <summary>Gets the cap height in font design units.</summary>
        public int CapHeight => _capHeight;

        /// <summary>Gets the x-height in font design units.</summary>
        public int XHeight => _xHeight;

        /// <summary>
        /// Parses the given font data. The data must be a valid TrueType or OpenType sfnt.
        /// </summary>
        public OpenTypeFontData(byte[] fontData)
        {
            _data = fontData ?? throw new ArgumentNullException(nameof(fontData));
            _tables = new Dictionary<uint, TableRecord>();

            if (_data.Length < 12)
                throw new InvalidOperationException("Font data is too small.");

            ParseOffsetTable();
            ParseHeadTable();
            ParseMaxpTable();
            ParseHheaTable();
            ParseHmtxTable();
            ParseOs2Table();
            ParseNameTable();
            FindCmapSubtables();
        }

        /// <summary>
        /// Builds a <see cref="FontMetricsInfo"/> from the parsed data.
        /// </summary>
        public FontMetricsInfo BuildMetrics()
        {
            return new FontMetricsInfo(_ascent, _descent, _lineGap, _unitsPerEm, _capHeight, _xHeight);
        }

        /// <summary>
        /// Returns the glyph ID for the given Unicode code point, or 0 (the .notdef glyph) if not found.
        /// </summary>
        public int GetGlyphId(int codePoint)
        {
            // Try format 12 first (full Unicode range).
            if (_cmapFormat12Offset >= 0)
            {
                int glyphId = LookupFormat12(codePoint);
                if (glyphId > 0) return glyphId;
            }

            // Fall back to format 4 (BMP only).
            if (_cmapFormat4Offset >= 0 && codePoint <= 0xFFFF)
            {
                int glyphId = LookupFormat4(codePoint);
                if (glyphId > 0) return glyphId;
            }

            // Check cache from any previous full parse.
            if (_cmapCache != null && _cmapCache.TryGetValue(codePoint, out int cached))
                return cached;

            return 0;
        }

        /// <summary>
        /// Returns the advance width in font design units for the given glyph ID.
        /// </summary>
        public int GetAdvanceWidth(int glyphId)
        {
            if (_advanceWidths == null) return 0;

            if (glyphId < _advanceWidths.Length)
                return _advanceWidths[glyphId];

            // Glyphs beyond numberOfHMetrics all share the last advance width.
            return _lastAdvanceWidth;
        }

        #region Offset Table

        private void ParseOffsetTable()
        {
            // uint sfntVersion = ReadUInt32(0);
            ushort numTables = ReadUInt16(4);

            int offset = 12;
            for (int i = 0; i < numTables; i++)
            {
                if (offset + 16 > _data.Length) break;

                uint tag = ReadUInt32(offset);
                // uint checksum = ReadUInt32(offset + 4);
                uint tableOffset = ReadUInt32(offset + 8);
                uint length = ReadUInt32(offset + 12);

                _tables[tag] = new TableRecord(tableOffset, length);
                offset += 16;
            }
        }

        #endregion

        #region head

        private void ParseHeadTable()
        {
            if (!_tables.TryGetValue(TagHead, out TableRecord rec)) return;
            int o = (int)rec.Offset;
            if (o + 54 > _data.Length) return;

            // unitsPerEm is at offset 18 within the head table.
            _unitsPerEm = ReadUInt16(o + 18);
        }

        #endregion

        #region maxp

        private void ParseMaxpTable()
        {
            if (!_tables.TryGetValue(TagMaxp, out TableRecord rec)) return;
            int o = (int)rec.Offset;
            if (o + 6 > _data.Length) return;

            _numGlyphs = ReadUInt16(o + 4);
        }

        #endregion

        #region hhea

        private void ParseHheaTable()
        {
            if (!_tables.TryGetValue(TagHhea, out TableRecord rec)) return;
            int o = (int)rec.Offset;
            if (o + 36 > _data.Length) return;

            // hhea layout:
            // offset 4: ascent (int16)
            // offset 6: descent (int16)
            // offset 8: lineGap (int16)
            // offset 34: numberOfHMetrics (uint16)
            int hheaAscent = ReadInt16(o + 4);
            int hheaDescent = ReadInt16(o + 6);
            int hheaLineGap = ReadInt16(o + 8);
            _numberOfHMetrics = ReadUInt16(o + 34);

            // Use hhea values as defaults; OS/2 values will override if present.
            _ascent = hheaAscent;
            _descent = hheaDescent;
            _lineGap = hheaLineGap;
        }

        #endregion

        #region hmtx

        private void ParseHmtxTable()
        {
            if (!_tables.TryGetValue(TagHmtx, out TableRecord rec)) return;
            int o = (int)rec.Offset;

            // Each longHorMetric record is 4 bytes: advanceWidth (uint16) + lsb (int16).
            // After numberOfHMetrics records, remaining glyphs share the last advance width.
            int totalGlyphs = _numGlyphs > 0 ? _numGlyphs : _numberOfHMetrics;
            _advanceWidths = new ushort[totalGlyphs];

            for (int i = 0; i < _numberOfHMetrics && o + 4 <= (int)rec.Offset + (int)rec.Length; i++)
            {
                ushort aw = ReadUInt16(o);
                if (i < _advanceWidths.Length)
                    _advanceWidths[i] = aw;
                _lastAdvanceWidth = aw;
                o += 4; // skip lsb
            }

            // Fill remaining glyphs with the last advance width.
            for (int i = _numberOfHMetrics; i < totalGlyphs; i++)
            {
                _advanceWidths[i] = _lastAdvanceWidth;
            }
        }

        #endregion

        #region OS/2

        private void ParseOs2Table()
        {
            if (!_tables.TryGetValue(TagOs2, out TableRecord rec)) return;
            int o = (int)rec.Offset;
            if (o + 78 > _data.Length) return;

            // OS/2 table:
            // offset 4: usWeightClass (uint16)
            _weight = ReadUInt16(o + 4);

            // offset 62: fsSelection (uint16) - bit 0 = italic
            ushort fsSelection = ReadUInt16(o + 62);
            _isItalic = (fsSelection & 0x0001) != 0;

            // offset 68: sTypoAscender (int16)
            // offset 70: sTypoDescender (int16)
            // offset 72: sTypoLineGap (int16)
            int typoAscender = ReadInt16(o + 68);
            int typoDescender = ReadInt16(o + 70);
            int typoLineGap = ReadInt16(o + 72);

            // Prefer OS/2 typo metrics.
            _ascent = typoAscender;
            _descent = typoDescender;
            _lineGap = typoLineGap;

            // sCapHeight at offset 88, sxHeight at offset 86 (OS/2 version >= 2).
            ushort version = ReadUInt16(o);
            if (version >= 2 && o + 90 <= _data.Length)
            {
                _xHeight = ReadInt16(o + 86);
                _capHeight = ReadInt16(o + 88);
            }
        }

        #endregion

        #region name

        private void ParseNameTable()
        {
            if (!_tables.TryGetValue(TagName, out TableRecord rec)) return;
            int o = (int)rec.Offset;
            if (o + 6 > _data.Length) return;

            // ushort format = ReadUInt16(o);
            ushort count = ReadUInt16(o + 2);
            ushort stringOffset = ReadUInt16(o + 4);
            int storageOffset = o + stringOffset;

            // Look for nameID 16 (Typographic Family) first, then nameID 1 (Family).
            string? nameId16 = null;
            string? nameId1 = null;

            int nameRecordOffset = o + 6;
            for (int i = 0; i < count; i++)
            {
                if (nameRecordOffset + 12 > _data.Length) break;

                ushort platformId = ReadUInt16(nameRecordOffset);
                ushort encodingId = ReadUInt16(nameRecordOffset + 2);
                // ushort languageId = ReadUInt16(nameRecordOffset + 4);
                ushort nameId = ReadUInt16(nameRecordOffset + 6);
                ushort nameLength = ReadUInt16(nameRecordOffset + 8);
                ushort nameOff = ReadUInt16(nameRecordOffset + 10);

                if (nameId == 16 || nameId == 1)
                {
                    int strStart = storageOffset + nameOff;
                    if (strStart + nameLength <= _data.Length)
                    {
                        string? value = DecodeName(platformId, encodingId, strStart, nameLength);
                        if (!string.IsNullOrEmpty(value))
                        {
                            if (nameId == 16) nameId16 = value;
                            else if (nameId == 1 && nameId1 == null) nameId1 = value;
                        }
                    }
                }

                nameRecordOffset += 12;
            }

            _familyName = nameId16 ?? nameId1 ?? string.Empty;
        }

        private string? DecodeName(ushort platformId, ushort encodingId, int offset, int length)
        {
            if (length == 0) return null;

            // Platform 3 (Windows), encoding 1 (Unicode BMP) or Platform 0 (Unicode): UTF-16BE.
            if (platformId == 3 || platformId == 0)
            {
                return DecodeUtf16BE(offset, length);
            }

            // Platform 1 (Macintosh), encoding 0 (Roman): single-byte ASCII-like.
            if (platformId == 1 && encodingId == 0)
            {
                return Encoding.ASCII.GetString(_data, offset, length);
            }

            return null;
        }

        private string DecodeUtf16BE(int offset, int length)
        {
            var sb = new StringBuilder(length / 2);
            for (int i = 0; i + 1 < length; i += 2)
            {
                char c = (char)((_data[offset + i] << 8) | _data[offset + i + 1]);
                sb.Append(c);
            }
            return sb.ToString();
        }

        #endregion

        #region cmap

        private void FindCmapSubtables()
        {
            if (!_tables.TryGetValue(TagCmap, out TableRecord rec)) return;
            int o = (int)rec.Offset;
            if (o + 4 > _data.Length) return;

            // ushort version = ReadUInt16(o);
            ushort numSubtables = ReadUInt16(o + 2);

            int subtableOffset = o + 4;
            for (int i = 0; i < numSubtables; i++)
            {
                if (subtableOffset + 8 > _data.Length) break;

                ushort platformId = ReadUInt16(subtableOffset);
                ushort encodingId = ReadUInt16(subtableOffset + 2);
                uint relOffset = ReadUInt32(subtableOffset + 4);
                int absOffset = o + (int)relOffset;

                if (absOffset + 2 <= _data.Length)
                {
                    ushort format = ReadUInt16(absOffset);

                    // Prefer Windows Unicode (platform 3, encoding 1 or 10) or Unicode (platform 0).
                    bool isUnicode = platformId == 0 || (platformId == 3 && (encodingId == 1 || encodingId == 10));

                    if (isUnicode)
                    {
                        if (format == 4 && _cmapFormat4Offset < 0)
                            _cmapFormat4Offset = absOffset;
                        else if (format == 12 && _cmapFormat12Offset < 0)
                            _cmapFormat12Offset = absOffset;
                    }
                }

                subtableOffset += 8;
            }
        }

        private int LookupFormat4(int codePoint)
        {
            int o = _cmapFormat4Offset;
            if (o < 0 || o + 14 > _data.Length) return 0;

            // Format 4 header:
            // offset 0: format (uint16) = 4
            // offset 2: length (uint16)
            // offset 6: segCountX2 (uint16)
            ushort segCountX2 = ReadUInt16(o + 6);
            int segCount = segCountX2 / 2;

            // Arrays start at offset 14.
            int endCodesOffset = o + 14;
            int startCodesOffset = endCodesOffset + segCountX2 + 2; // +2 for reservedPad
            int idDeltaOffset = startCodesOffset + segCountX2;
            int idRangeOffsetBase = idDeltaOffset + segCountX2;

            // Binary search through segments.
            int low = 0;
            int high = segCount - 1;
            while (low <= high)
            {
                int mid = (low + high) / 2;
                int endCode = ReadUInt16(endCodesOffset + mid * 2);
                int startCode = ReadUInt16(startCodesOffset + mid * 2);

                if (codePoint > endCode)
                {
                    low = mid + 1;
                }
                else if (codePoint < startCode)
                {
                    high = mid - 1;
                }
                else
                {
                    // Found segment.
                    short idDelta = (short)ReadUInt16(idDeltaOffset + mid * 2);
                    ushort idRangeOffset = ReadUInt16(idRangeOffsetBase + mid * 2);

                    int glyphId;
                    if (idRangeOffset == 0)
                    {
                        glyphId = (codePoint + idDelta) & 0xFFFF;
                    }
                    else
                    {
                        int rangeOffsetLocation = idRangeOffsetBase + mid * 2;
                        int glyphIdArrayOffset = rangeOffsetLocation + idRangeOffset + (codePoint - startCode) * 2;
                        if (glyphIdArrayOffset + 2 > _data.Length) return 0;
                        glyphId = ReadUInt16(glyphIdArrayOffset);
                        if (glyphId != 0)
                            glyphId = (glyphId + idDelta) & 0xFFFF;
                    }

                    return glyphId;
                }
            }

            return 0;
        }

        private int LookupFormat12(int codePoint)
        {
            int o = _cmapFormat12Offset;
            if (o < 0 || o + 16 > _data.Length) return 0;

            // Format 12 header (after the format field):
            // offset 0: format (uint16) = 12
            // offset 2: reserved (uint16)
            // offset 4: length (uint32)
            // offset 8: language (uint32)
            // offset 12: numGroups (uint32)
            uint numGroups = ReadUInt32(o + 12);
            int groupsOffset = o + 16;

            // Each group: startCharCode(4), endCharCode(4), startGlyphID(4) = 12 bytes.
            // Binary search.
            int low = 0;
            int high = (int)numGroups - 1;
            while (low <= high)
            {
                int mid = (low + high) / 2;
                int groupOffset = groupsOffset + mid * 12;
                if (groupOffset + 12 > _data.Length) return 0;

                uint startCharCode = ReadUInt32(groupOffset);
                uint endCharCode = ReadUInt32(groupOffset + 4);

                if ((uint)codePoint > endCharCode)
                {
                    low = mid + 1;
                }
                else if ((uint)codePoint < startCharCode)
                {
                    high = mid - 1;
                }
                else
                {
                    uint startGlyphId = ReadUInt32(groupOffset + 8);
                    return (int)(startGlyphId + (uint)codePoint - startCharCode);
                }
            }

            return 0;
        }

        #endregion

        #region Primitive Readers

        private uint ReadUInt32(int offset)
        {
            return (uint)((_data[offset] << 24) | (_data[offset + 1] << 16) | (_data[offset + 2] << 8) | _data[offset + 3]);
        }

        private ushort ReadUInt16(int offset)
        {
            return (ushort)((_data[offset] << 8) | _data[offset + 1]);
        }

        private short ReadInt16(int offset)
        {
            return (short)((_data[offset] << 8) | _data[offset + 1]);
        }

        #endregion

        private readonly struct TableRecord
        {
            public readonly uint Offset;
            public readonly uint Length;

            public TableRecord(uint offset, uint length)
            {
                Offset = offset;
                Length = length;
            }
        }
    }
}
