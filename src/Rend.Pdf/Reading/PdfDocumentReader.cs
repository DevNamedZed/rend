#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Rend.Pdf.Reading
{
    public sealed class PdfDocumentReader : IDisposable
    {
        private readonly byte[] _data;
        private int _pos;
        private readonly Dictionary<int, (long offset, int gen)> _xrefTable = new Dictionary<int, (long, int)>();
        private readonly Dictionary<int, PdfObj> _objectCache = new Dictionary<int, PdfObj>();
        private readonly List<PdfObj> _pages = new List<PdfObj>();
        private PdfObj _trailer = PdfObj.Null;
        private PdfObj _catalog = PdfObj.Null;
        #pragma warning disable CS0414
        private bool _disposed;
        #pragma warning restore CS0414

        public PdfObj Trailer => _trailer;
        public PdfObj Catalog => _catalog;
        public int PageCount => _pages.Count;

        private PdfDocumentReader(byte[] data)
        {
            _data = data;
            Parse();
        }

        public static PdfDocumentReader Open(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            return new PdfDocumentReader(data);
        }

        public static PdfDocumentReader Open(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return new PdfDocumentReader(ms.ToArray());
            }
        }

        public PdfObj GetPage(int pageIndex)
        {
            if (pageIndex < 0 || pageIndex >= _pages.Count)
                return PdfObj.Null;
            return _pages[pageIndex];
        }

        public PdfObj Resolve(PdfObj obj)
        {
            if (obj is PdfRef r)
            {
                if (_objectCache.TryGetValue(r.ObjNum, out var cached))
                    return cached;

                if (_xrefTable.TryGetValue(r.ObjNum, out var entry))
                {
                    try
                    {
                        var parsed = ParseObjectAt(entry.offset);
                        _objectCache[r.ObjNum] = parsed;
                        return parsed;
                    }
                    catch
                    {
                        return PdfObj.Null;
                    }
                }

                return PdfObj.Null;
            }

            return obj;
        }

        public byte[] GetStreamBytes(PdfObj streamObj)
        {
            var stream = Resolve(streamObj) as PdfStream;
            if (stream == null) return Array.Empty<byte>();

            byte[] raw;
            if (stream.RawData.Length > 0)
            {
                raw = stream.RawData;
            }
            else if (stream.StreamOffset > 0 && stream.StreamLength > 0)
            {
                long offset = stream.StreamOffset;
                int length = stream.StreamLength;
                if (offset + length > _data.Length)
                    length = (int)(_data.Length - offset);
                if (length <= 0) return Array.Empty<byte>();
                raw = new byte[length];
                Array.Copy(_data, (int)offset, raw, 0, length);
            }
            else
            {
                return Array.Empty<byte>();
            }

            var filterObj = Resolve(stream["Filter"]);
            var parmsObj = Resolve(stream["DecodeParms"]);

            if (filterObj.IsNull)
                return raw;

            var filters = new List<string>();
            var parmsList = new List<PdfObj>();

            if (filterObj is PdfName fname)
            {
                filters.Add(fname.Value);
                parmsList.Add(parmsObj.IsNull ? PdfObj.Null : parmsObj);
            }
            else if (filterObj is PdfArray farr)
            {
                for (int i = 0; i < farr.Count; i++)
                {
                    var f = Resolve(farr[i]);
                    filters.Add(f.AsName());
                    parmsList.Add(parmsObj is PdfArray pa ? (i < pa.Count ? Resolve(pa[i]) : PdfObj.Null) : PdfObj.Null);
                }
            }

            byte[] result = raw;
            for (int i = 0; i < filters.Count; i++)
            {
                result = ApplyFilter(filters[i], result, parmsList[i]);
            }

            return result;
        }

        public byte[]? GetFontProgramData(PdfObj fontDict)
        {
            var resolved = Resolve(fontDict);
            var descriptor = Resolve(resolved["FontDescriptor"]);
            if (descriptor.IsNull) return null;

            var fontFile2 = Resolve(descriptor["FontFile2"]);
            if (!fontFile2.IsNull) return GetStreamBytes(fontFile2);

            var fontFile = Resolve(descriptor["FontFile"]);
            if (!fontFile.IsNull) return GetStreamBytes(fontFile);

            var fontFile3 = Resolve(descriptor["FontFile3"]);
            if (!fontFile3.IsNull) return GetStreamBytes(fontFile3);

            return null;
        }

        public void Dispose()
        {
            _disposed = true;
        }

        // ─── Parsing ─────────────────────────────────────────────────────

        private void Parse()
        {
            long startXRef = FindStartXRef();
            ParseXRef(startXRef);
            LoadCatalogAndPages();
        }

        private void LoadCatalogAndPages()
        {
            var root = Resolve(_trailer["Root"]);
            if (root.IsNull) return;
            _catalog = root;

            var pagesRef = root["Pages"];
            var pagesObj = Resolve(pagesRef);
            if (pagesObj.IsNull) return;

            FlattenPageTree(pagesObj, new Dictionary<string, PdfObj>());
        }

        private void FlattenPageTree(PdfObj node, Dictionary<string, PdfObj> inherited)
        {
            var resolved = Resolve(node);
            var type = Resolve(resolved["Type"]).AsName();

            var current = new Dictionary<string, PdfObj>(inherited, StringComparer.Ordinal);
            InheritIfPresent(resolved, current, "MediaBox");
            InheritIfPresent(resolved, current, "CropBox");
            InheritIfPresent(resolved, current, "Rotate");
            InheritIfPresent(resolved, current, "Resources");

            if (type == "Pages")
            {
                var kids = Resolve(resolved["Kids"]);
                for (int i = 0; i < kids.Count; i++)
                {
                    FlattenPageTree(kids[i], current);
                }
            }
            else if (type == "Page")
            {
                if (resolved is PdfDict pageDict)
                {
                    foreach (var kvp in current)
                    {
                        if (!pageDict.Entries.ContainsKey(kvp.Key))
                        {
                            pageDict.Entries[kvp.Key] = kvp.Value;
                        }
                    }
                }
                _pages.Add(resolved);
            }
        }

        private static void InheritIfPresent(PdfObj node, Dictionary<string, PdfObj> inherited, string key)
        {
            var val = node[key];
            if (!val.IsNull)
                inherited[key] = val;
        }

        // ─── XRef ────────────────────────────────────────────────────────

        private long FindStartXRef()
        {
            int searchStart = Math.Max(0, _data.Length - 1024);
            string tail = Encoding.ASCII.GetString(_data, searchStart, _data.Length - searchStart);
            int idx = tail.LastIndexOf("startxref", StringComparison.Ordinal);
            if (idx < 0)
                throw new InvalidOperationException("Cannot find startxref marker.");

            int numStart = idx + 9;
            while (numStart < tail.Length && IsWhitespace((byte)tail[numStart]))
                numStart++;

            int numEnd = numStart;
            while (numEnd < tail.Length && tail[numEnd] >= '0' && tail[numEnd] <= '9')
                numEnd++;

            if (numEnd == numStart)
                throw new InvalidOperationException("Cannot parse startxref offset.");

            return long.Parse(tail.Substring(numStart, numEnd - numStart));
        }

        private void ParseXRef(long offset)
        {
            if (offset < 0 || offset >= _data.Length)
                return;

            _pos = (int)offset;
            SkipWhitespaceAndComments();

            if (_pos + 4 <= _data.Length && _data[_pos] == 'x' && _data[_pos + 1] == 'r'
                && _data[_pos + 2] == 'e' && _data[_pos + 3] == 'f')
            {
                ParseTraditionalXRef();
            }
            else
            {
                ParseXRefStream();
            }
        }

        private void ParseTraditionalXRef()
        {
            _pos += 4; // skip "xref"
            SkipWhitespaceAndComments();

            while (_pos < _data.Length)
            {
                if (_data[_pos] == 't') break; // "trailer"

                int startObj = ReadIntDirect();
                SkipWhitespaceAndComments();
                int count = ReadIntDirect();
                SkipWhitespaceAndComments();

                for (int i = 0; i < count; i++)
                {
                    long entryOffset = ReadLongDirect();
                    SkipWhitespaceAndComments();
                    int gen = ReadIntDirect();
                    SkipWhitespaceAndComments();

                    byte flag = _data[_pos];
                    _pos++;
                    SkipWhitespaceAndComments();

                    if (flag == (byte)'n')
                    {
                        int objNum = startObj + i;
                        if (!_xrefTable.ContainsKey(objNum))
                        {
                            _xrefTable[objNum] = (entryOffset, gen);
                        }
                    }
                }
            }

            // Parse trailer
            if (_pos + 7 <= _data.Length && _data[_pos] == 't')
            {
                // Skip "trailer"
                _pos += 7;
                SkipWhitespaceAndComments();
                var trailerDict = ParseObject();
                if (_trailer.IsNull)
                    _trailer = trailerDict;

                // Follow /Prev
                var prev = trailerDict["Prev"];
                if (!prev.IsNull)
                {
                    ParseXRef((long)prev.AsReal());
                }
            }
        }

        private void ParseXRefStream()
        {
            var streamObj = ParseIndirectObject();
            if (streamObj is PdfStream xrefStream)
            {
                if (_trailer.IsNull)
                {
                    _trailer = xrefStream;
                }

                byte[] decoded = DecodeStreamRaw(xrefStream);
                var wArr = Resolve(xrefStream["W"]);
                if (wArr.Count < 3) return;

                int w0 = (int)wArr[0].AsInt();
                int w1 = (int)wArr[1].AsInt();
                int w2 = (int)wArr[2].AsInt();
                int entrySize = w0 + w1 + w2;

                var indexArr = Resolve(xrefStream["Index"]);
                var subsections = new List<(int start, int count)>();

                if (!indexArr.IsNull && indexArr.Count >= 2)
                {
                    for (int i = 0; i + 1 < indexArr.Count; i += 2)
                    {
                        subsections.Add(((int)indexArr[i].AsInt(), (int)indexArr[i + 1].AsInt()));
                    }
                }
                else
                {
                    var size = xrefStream["Size"];
                    subsections.Add((0, (int)size.AsInt()));
                }

                int dataPos = 0;
                foreach (var (start, count) in subsections)
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (dataPos + entrySize > decoded.Length) break;

                        long type = w0 > 0 ? ReadBytesAsLong(decoded, dataPos, w0) : 1;
                        dataPos += w0;
                        long field2 = ReadBytesAsLong(decoded, dataPos, w1);
                        dataPos += w1;
                        long field3 = ReadBytesAsLong(decoded, dataPos, w2);
                        dataPos += w2;

                        int objNum = start + i;

                        if (type == 1 && !_xrefTable.ContainsKey(objNum))
                        {
                            _xrefTable[objNum] = (field2, (int)field3);
                        }
                        else if (type == 2 && !_xrefTable.ContainsKey(objNum))
                        {
                            // Compressed object in object stream field2, index field3
                            // Store with negative offset to mark as compressed
                            _xrefTable[objNum] = (-(field2 + 1), (int)field3);
                        }
                    }
                }

                var prev = xrefStream["Prev"];
                if (!prev.IsNull)
                {
                    ParseXRef((long)prev.AsReal());
                }
            }
        }

        private static long ReadBytesAsLong(byte[] data, int offset, int count)
        {
            long val = 0;
            for (int i = 0; i < count; i++)
            {
                val = (val << 8) | data[offset + i];
            }
            return val;
        }

        // ─── Object Parsing at Offset ────────────────────────────────────

        private PdfObj ParseObjectAt(long offset)
        {
            if (offset < 0)
            {
                // Compressed object in an object stream
                long streamObjNum = -(offset + 1);
                return ParseCompressedObject((int)streamObjNum, offset);
            }

            _pos = (int)offset;
            return ParseIndirectObject();
        }

        private PdfObj ParseCompressedObject(int streamObjNum, long originalOffset)
        {
            // Find which object we're looking for by scanning xref for this offset
            int targetIndex = -1;
            int targetObjNum = -1;
            foreach (var kvp in _xrefTable)
            {
                if (kvp.Value.offset == originalOffset)
                {
                    targetObjNum = kvp.Key;
                    targetIndex = kvp.Value.gen; // gen field holds the index for type 2
                    break;
                }
            }
            if (targetObjNum < 0) return PdfObj.Null;

            var streamObj = Resolve(new PdfRef(streamObjNum, 0));
            if (!(streamObj is PdfStream objStream)) return PdfObj.Null;

            byte[] decoded = GetStreamBytes(objStream);
            int n = (int)Resolve(objStream["N"]).AsInt();
            int first = (int)Resolve(objStream["First"]).AsInt();

            // Parse the N pairs of (objNum offset) from the beginning of decoded data
            var savedPos = _pos;
            var savedData = _data;

            // We need to parse from the decoded byte array
            // Temporarily work with decoded data
            var reader = new PdfDocumentReader(decoded);
            reader._pos = 0;

            var objNums = new int[n];
            var offsets = new int[n];
            for (int i = 0; i < n; i++)
            {
                reader.SkipWhitespaceAndComments();
                objNums[i] = reader.ReadIntDirect();
                reader.SkipWhitespaceAndComments();
                offsets[i] = reader.ReadIntDirect();
                reader.SkipWhitespaceAndComments();
            }

            // Find our target object
            for (int i = 0; i < n; i++)
            {
                if (objNums[i] == targetObjNum || i == targetIndex)
                {
                    reader._pos = first + offsets[i];
                    reader.SkipWhitespaceAndComments();
                    var result = reader.ParseObject();
                    return result;
                }
            }

            return PdfObj.Null;
        }

        private PdfObj ParseIndirectObject()
        {
            SkipWhitespaceAndComments();

            // Read object number
            ReadIntDirect();
            SkipWhitespaceAndComments();

            // Read generation number
            ReadIntDirect();
            SkipWhitespaceAndComments();

            // Skip "obj"
            SkipKeyword();
            SkipWhitespaceAndComments();

            var obj = ParseObject();
            SkipWhitespaceAndComments();

            // Check for "stream" keyword after dict
            if (obj is PdfDict dict && !(obj is PdfStream))
            {
                if (MatchKeyword("stream"))
                {
                    SkipKeyword();
                    // Skip single \r\n, \n, or \r after "stream"
                    if (_pos < _data.Length && _data[_pos] == '\r')
                        _pos++;
                    if (_pos < _data.Length && _data[_pos] == '\n')
                        _pos++;

                    long streamOffset = _pos;
                    var lengthObj = dict["Length"];
                    int streamLength = 0;

                    if (lengthObj is PdfRef lengthRef)
                    {
                        var resolved = Resolve(lengthRef);
                        streamLength = (int)resolved.AsInt();
                    }
                    else
                    {
                        streamLength = (int)lengthObj.AsInt();
                    }

                    if (streamLength < 0) streamLength = 0;
                    if (streamOffset + streamLength > _data.Length)
                        streamLength = (int)(_data.Length - streamOffset);

                    var pdfStream = new PdfStream(dict.Entries)
                    {
                        StreamOffset = streamOffset,
                        StreamLength = streamLength
                    };

                    _pos = (int)streamOffset + streamLength;
                    SkipWhitespaceAndComments();
                    if (MatchKeyword("endstream"))
                        SkipKeyword();

                    obj = pdfStream;
                }
            }

            SkipWhitespaceAndComments();
            if (MatchKeyword("endobj"))
                SkipKeyword();

            return obj;
        }

        // ─── Lexer ───────────────────────────────────────────────────────

        private void SkipWhitespaceAndComments()
        {
            while (_pos < _data.Length)
            {
                byte b = _data[_pos];
                if (IsWhitespace(b))
                {
                    _pos++;
                }
                else if (b == '%')
                {
                    _pos++;
                    while (_pos < _data.Length && _data[_pos] != '\n' && _data[_pos] != '\r')
                        _pos++;
                }
                else
                {
                    break;
                }
            }
        }

        private static bool IsWhitespace(byte b)
        {
            return b == 0 || b == 9 || b == 10 || b == 12 || b == 13 || b == 32;
        }

        private static bool IsDelimiter(byte b)
        {
            return b == '(' || b == ')' || b == '<' || b == '>' || b == '['
                || b == ']' || b == '{' || b == '}' || b == '/' || b == '%';
        }

        private byte PeekByte()
        {
            return _pos < _data.Length ? _data[_pos] : (byte)0;
        }

        private byte ReadByte()
        {
            return _pos < _data.Length ? _data[_pos++] : (byte)0;
        }

        private bool MatchKeyword(string keyword)
        {
            if (_pos + keyword.Length > _data.Length) return false;
            for (int i = 0; i < keyword.Length; i++)
            {
                if (_data[_pos + i] != (byte)keyword[i]) return false;
            }
            // After keyword must be whitespace, delimiter, or EOF
            int after = _pos + keyword.Length;
            if (after < _data.Length)
            {
                byte b = _data[after];
                if (!IsWhitespace(b) && !IsDelimiter(b))
                    return false;
            }
            return true;
        }

        private void SkipKeyword()
        {
            while (_pos < _data.Length && !IsWhitespace(_data[_pos]) && !IsDelimiter(_data[_pos]))
                _pos++;
        }

        private int ReadIntDirect()
        {
            int sign = 1;
            if (_pos < _data.Length && _data[_pos] == '-')
            {
                sign = -1;
                _pos++;
            }
            else if (_pos < _data.Length && _data[_pos] == '+')
            {
                _pos++;
            }

            int val = 0;
            while (_pos < _data.Length && _data[_pos] >= '0' && _data[_pos] <= '9')
            {
                val = val * 10 + (_data[_pos] - '0');
                _pos++;
            }
            return val * sign;
        }

        private long ReadLongDirect()
        {
            long sign = 1;
            if (_pos < _data.Length && _data[_pos] == '-')
            {
                sign = -1;
                _pos++;
            }
            else if (_pos < _data.Length && _data[_pos] == '+')
            {
                _pos++;
            }

            long val = 0;
            while (_pos < _data.Length && _data[_pos] >= '0' && _data[_pos] <= '9')
            {
                val = val * 10 + (_data[_pos] - '0');
                _pos++;
            }
            return val * sign;
        }

        // ─── Parser ──────────────────────────────────────────────────────

        private PdfObj ParseObject()
        {
            SkipWhitespaceAndComments();
            if (_pos >= _data.Length) return PdfObj.Null;

            byte b = _data[_pos];

            // Boolean
            if (MatchKeyword("true"))
            {
                SkipKeyword();
                return new PdfBool(true);
            }
            if (MatchKeyword("false"))
            {
                SkipKeyword();
                return new PdfBool(false);
            }
            if (MatchKeyword("null"))
            {
                SkipKeyword();
                return PdfObj.Null;
            }

            // Array
            if (b == '[')
            {
                return ParseArray();
            }

            // Dict or hex string
            if (b == '<')
            {
                if (_pos + 1 < _data.Length && _data[_pos + 1] == '<')
                {
                    return ParseDict();
                }
                return ParseHexString();
            }

            // Literal string
            if (b == '(')
            {
                return ParseLiteralString();
            }

            // Name
            if (b == '/')
            {
                return ParseName();
            }

            // Number (possibly followed by another number and "R" for indirect ref)
            if (b == '-' || b == '+' || b == '.' || (b >= '0' && b <= '9'))
            {
                return ParseNumber();
            }

            // End markers — return null and let caller handle
            if (b == ']' || (b == '>' && _pos + 1 < _data.Length && _data[_pos + 1] == '>'))
            {
                return PdfObj.Null;
            }

            // Unknown token — skip
            _pos++;
            return PdfObj.Null;
        }

        private PdfArray ParseArray()
        {
            _pos++; // skip '['
            var items = new List<PdfObj>();
            while (_pos < _data.Length)
            {
                SkipWhitespaceAndComments();
                if (_pos >= _data.Length) break;
                if (_data[_pos] == ']')
                {
                    _pos++;
                    break;
                }
                var obj = ParseObject();
                items.Add(obj);
            }
            return new PdfArray(items);
        }

        private PdfDict ParseDict()
        {
            _pos += 2; // skip '<<'
            var entries = new Dictionary<string, PdfObj>(StringComparer.Ordinal);

            while (_pos < _data.Length)
            {
                SkipWhitespaceAndComments();
                if (_pos >= _data.Length) break;
                if (_data[_pos] == '>' && _pos + 1 < _data.Length && _data[_pos + 1] == '>')
                {
                    _pos += 2;
                    break;
                }

                // Key must be a name
                if (_data[_pos] != '/')
                {
                    // Malformed — try to skip
                    _pos++;
                    continue;
                }

                var nameObj = ParseName();
                SkipWhitespaceAndComments();
                var value = ParseObject();
                entries[nameObj.Value] = value;
            }

            return new PdfDict(entries);
        }

        private PdfName ParseName()
        {
            _pos++; // skip '/'
            var sb = new StringBuilder();
            while (_pos < _data.Length)
            {
                byte b = _data[_pos];
                if (IsWhitespace(b) || IsDelimiter(b))
                    break;

                if (b == '#' && _pos + 2 < _data.Length)
                {
                    int hi = HexVal(_data[_pos + 1]);
                    int lo = HexVal(_data[_pos + 2]);
                    if (hi >= 0 && lo >= 0)
                    {
                        sb.Append((char)((hi << 4) | lo));
                        _pos += 3;
                        continue;
                    }
                }

                sb.Append((char)b);
                _pos++;
            }
            return new PdfName(sb.ToString());
        }

        private PdfString ParseLiteralString()
        {
            _pos++; // skip '('
            var bytes = new List<byte>();
            int parenDepth = 1;

            while (_pos < _data.Length && parenDepth > 0)
            {
                byte b = _data[_pos];
                if (b == '(')
                {
                    parenDepth++;
                    bytes.Add(b);
                    _pos++;
                }
                else if (b == ')')
                {
                    parenDepth--;
                    if (parenDepth > 0)
                    {
                        bytes.Add(b);
                    }
                    _pos++;
                }
                else if (b == '\\')
                {
                    _pos++;
                    if (_pos >= _data.Length) break;
                    byte esc = _data[_pos];
                    switch (esc)
                    {
                        case (byte)'n': bytes.Add((byte)'\n'); _pos++; break;
                        case (byte)'r': bytes.Add((byte)'\r'); _pos++; break;
                        case (byte)'t': bytes.Add((byte)'\t'); _pos++; break;
                        case (byte)'b': bytes.Add((byte)'\b'); _pos++; break;
                        case (byte)'f': bytes.Add((byte)'\f'); _pos++; break;
                        case (byte)'(': bytes.Add((byte)'('); _pos++; break;
                        case (byte)')': bytes.Add((byte)')'); _pos++; break;
                        case (byte)'\\': bytes.Add((byte)'\\'); _pos++; break;
                        case (byte)'\r':
                            _pos++;
                            if (_pos < _data.Length && _data[_pos] == '\n') _pos++;
                            break;
                        case (byte)'\n':
                            _pos++;
                            break;
                        default:
                            if (esc >= '0' && esc <= '7')
                            {
                                int octal = esc - '0';
                                _pos++;
                                if (_pos < _data.Length && _data[_pos] >= '0' && _data[_pos] <= '7')
                                {
                                    octal = octal * 8 + (_data[_pos] - '0');
                                    _pos++;
                                    if (_pos < _data.Length && _data[_pos] >= '0' && _data[_pos] <= '7')
                                    {
                                        octal = octal * 8 + (_data[_pos] - '0');
                                        _pos++;
                                    }
                                }
                                bytes.Add((byte)(octal & 0xFF));
                            }
                            else
                            {
                                bytes.Add(esc);
                                _pos++;
                            }
                            break;
                    }
                }
                else
                {
                    bytes.Add(b);
                    _pos++;
                }
            }

            return new PdfString(bytes.ToArray(), false);
        }

        private PdfString ParseHexString()
        {
            _pos++; // skip '<'
            var bytes = new List<byte>();
            int nibble = -1;

            while (_pos < _data.Length)
            {
                byte b = _data[_pos];
                if (b == '>')
                {
                    _pos++;
                    break;
                }

                if (IsWhitespace(b))
                {
                    _pos++;
                    continue;
                }

                int val = HexVal(b);
                if (val < 0)
                {
                    _pos++;
                    continue;
                }

                if (nibble < 0)
                {
                    nibble = val;
                }
                else
                {
                    bytes.Add((byte)((nibble << 4) | val));
                    nibble = -1;
                }
                _pos++;
            }

            // Odd number of hex digits — last nibble padded with 0
            if (nibble >= 0)
            {
                bytes.Add((byte)(nibble << 4));
            }

            return new PdfString(bytes.ToArray(), true);
        }

        private PdfObj ParseNumber()
        {
            int startPos = _pos;
            int sign = 1;
            if (_data[_pos] == '-') { sign = -1; _pos++; }
            else if (_data[_pos] == '+') { _pos++; }

            long intPart = 0;
            bool hasDigits = false;
            while (_pos < _data.Length && _data[_pos] >= '0' && _data[_pos] <= '9')
            {
                intPart = intPart * 10 + (_data[_pos] - '0');
                hasDigits = true;
                _pos++;
            }

            if (_pos < _data.Length && _data[_pos] == '.')
            {
                _pos++;
                double fracPart = 0;
                double fracDiv = 10;
                while (_pos < _data.Length && _data[_pos] >= '0' && _data[_pos] <= '9')
                {
                    fracPart += (_data[_pos] - '0') / fracDiv;
                    fracDiv *= 10;
                    hasDigits = true;
                    _pos++;
                }
                double realVal = sign * (intPart + fracPart);

                if (!hasDigits) return PdfObj.Null;

                // Check for "N G R" pattern — but reals can't be obj numbers
                return new PdfReal(realVal);
            }

            if (!hasDigits) return PdfObj.Null;

            long firstNum = sign * intPart;

            // Check if this might be "objNum genNum R"
            int savedPos = _pos;
            SkipWhitespaceAndComments();

            if (_pos < _data.Length && (_data[_pos] >= '0' && _data[_pos] <= '9'))
            {
                int secondStart = _pos;
                long secondNum = 0;
                while (_pos < _data.Length && _data[_pos] >= '0' && _data[_pos] <= '9')
                {
                    secondNum = secondNum * 10 + (_data[_pos] - '0');
                    _pos++;
                }

                SkipWhitespaceAndComments();
                if (_pos < _data.Length && _data[_pos] == 'R')
                {
                    // Check that R is followed by delimiter/whitespace/EOF
                    if (_pos + 1 >= _data.Length || IsWhitespace(_data[_pos + 1]) || IsDelimiter(_data[_pos + 1]))
                    {
                        _pos++; // skip 'R'
                        return new PdfRef((int)firstNum, (int)secondNum);
                    }
                }

                // Not a ref — backtrack
                _pos = savedPos;
            }
            else
            {
                _pos = savedPos;
            }

            return new PdfInt(firstNum);
        }

        private static int HexVal(byte b)
        {
            if (b >= '0' && b <= '9') return b - '0';
            if (b >= 'a' && b <= 'f') return b - 'a' + 10;
            if (b >= 'A' && b <= 'F') return b - 'A' + 10;
            return -1;
        }

        // ─── Stream Decoding ─────────────────────────────────────────────

        private byte[] DecodeStreamRaw(PdfStream stream)
        {
            byte[] raw;
            if (stream.RawData.Length > 0)
            {
                raw = stream.RawData;
            }
            else if (stream.StreamOffset > 0 && stream.StreamLength > 0)
            {
                int length = stream.StreamLength;
                int offset = (int)stream.StreamOffset;
                if (offset + length > _data.Length)
                    length = _data.Length - offset;
                if (length <= 0) return Array.Empty<byte>();
                raw = new byte[length];
                Array.Copy(_data, offset, raw, 0, length);
            }
            else
            {
                return Array.Empty<byte>();
            }

            var filterObj = stream["Filter"];
            // For xref stream decoding, resolve filter inline if it's not a ref
            if (filterObj is PdfRef filterRef)
            {
                if (_objectCache.TryGetValue(filterRef.ObjNum, out var cached))
                    filterObj = cached;
                else
                    return raw; // Can't resolve during xref parsing
            }

            if (filterObj.IsNull)
                return raw;

            var filters = new List<string>();
            var parmsObj = stream["DecodeParms"];
            var parmsList = new List<PdfObj>();

            if (filterObj is PdfName fname)
            {
                filters.Add(fname.Value);
                parmsList.Add(parmsObj.IsNull ? PdfObj.Null : parmsObj);
            }
            else if (filterObj is PdfArray farr)
            {
                for (int i = 0; i < farr.Count; i++)
                {
                    var f = farr[i];
                    if (f is PdfName n)
                        filters.Add(n.Value);
                    else
                        filters.Add(f.AsName());
                    parmsList.Add(parmsObj is PdfArray pa ? (i < pa.Count ? pa[i] : PdfObj.Null) : PdfObj.Null);
                }
            }

            byte[] result = raw;
            for (int i = 0; i < filters.Count; i++)
            {
                result = ApplyFilter(filters[i], result, parmsList[i]);
            }

            return result;
        }

        private byte[] ApplyFilter(string filter, byte[] data, PdfObj parms)
        {
            switch (filter)
            {
                case "FlateDecode":
                    return FlateDecode(data, parms);
                case "ASCIIHexDecode":
                    return ASCIIHexDecode(data);
                case "ASCII85Decode":
                    return ASCII85Decode(data);
                default:
                    return data;
            }
        }

        private byte[] FlateDecode(byte[] data, PdfObj parms)
        {
            byte[] decompressed;
            try
            {
                // Skip 2-byte zlib header
                int offset = 0;
                if (data.Length >= 2)
                {
                    // Check for zlib header (CMF + FLG)
                    if ((data[0] & 0x0F) == 8) // CM = deflate
                        offset = 2;
                }

                using (var input = new MemoryStream(data, offset, data.Length - offset))
                using (var deflate = new DeflateStream(input, CompressionMode.Decompress))
                using (var output = new MemoryStream())
                {
                    deflate.CopyTo(output);
                    decompressed = output.ToArray();
                }
            }
            catch
            {
                // Retry without skipping header
                try
                {
                    using (var input = new MemoryStream(data))
                    using (var deflate = new DeflateStream(input, CompressionMode.Decompress))
                    using (var output = new MemoryStream())
                    {
                        deflate.CopyTo(output);
                        decompressed = output.ToArray();
                    }
                }
                catch
                {
                    return data;
                }
            }

            // Apply predictor if specified
            var resolvedParms = Resolve(parms);
            if (!resolvedParms.IsNull)
            {
                int predictor = (int)resolvedParms["Predictor"].AsInt();
                if (predictor >= 10 && predictor <= 15)
                {
                    int columns = (int)resolvedParms["Columns"].AsInt();
                    if (columns <= 0) columns = 1;
                    int colors = (int)resolvedParms["Colors"].AsInt();
                    if (colors <= 0) colors = 1;
                    int bpc = (int)resolvedParms["BitsPerComponent"].AsInt();
                    if (bpc <= 0) bpc = 8;

                    decompressed = ApplyPngPredictor(decompressed, columns, colors, bpc);
                }
            }

            return decompressed;
        }

        private static byte[] ApplyPngPredictor(byte[] data, int columns, int colors, int bpc)
        {
            int bytesPerPixel = (colors * bpc + 7) / 8;
            int rowBytes = (columns * colors * bpc + 7) / 8;
            int srcRowSize = 1 + rowBytes; // 1 byte for filter type

            if (data.Length < srcRowSize)
                return data;

            int numRows = data.Length / srcRowSize;
            byte[] output = new byte[numRows * rowBytes];
            byte[] prevRow = new byte[rowBytes];

            for (int row = 0; row < numRows; row++)
            {
                int srcOffset = row * srcRowSize;
                if (srcOffset >= data.Length) break;

                byte filterType = data[srcOffset];
                int dstOffset = row * rowBytes;

                for (int i = 0; i < rowBytes; i++)
                {
                    int srcIdx = srcOffset + 1 + i;
                    if (srcIdx >= data.Length) break;

                    byte raw = data[srcIdx];
                    byte a = i >= bytesPerPixel ? output[dstOffset + i - bytesPerPixel] : (byte)0;
                    byte b = prevRow[i];
                    byte c = i >= bytesPerPixel ? prevRow[i - bytesPerPixel] : (byte)0;

                    byte val;
                    switch (filterType)
                    {
                        case 0: val = raw; break;
                        case 1: val = (byte)(raw + a); break;
                        case 2: val = (byte)(raw + b); break;
                        case 3: val = (byte)(raw + ((a + b) >> 1)); break;
                        case 4: val = (byte)(raw + PaethPredictor(a, b, c)); break;
                        default: val = raw; break;
                    }

                    output[dstOffset + i] = val;
                }

                Array.Copy(output, dstOffset, prevRow, 0, rowBytes);
            }

            return output;
        }

        private static byte PaethPredictor(byte a, byte b, byte c)
        {
            int p = a + b - c;
            int pa = Math.Abs(p - a);
            int pb = Math.Abs(p - b);
            int pc = Math.Abs(p - c);
            if (pa <= pb && pa <= pc) return a;
            if (pb <= pc) return b;
            return c;
        }

        private static byte[] ASCIIHexDecode(byte[] data)
        {
            var result = new List<byte>();
            int nibble = -1;
            for (int i = 0; i < data.Length; i++)
            {
                byte b = data[i];
                if (b == '>') break;
                if (IsWhitespace(b)) continue;

                int val = HexVal(b);
                if (val < 0) continue;

                if (nibble < 0)
                {
                    nibble = val;
                }
                else
                {
                    result.Add((byte)((nibble << 4) | val));
                    nibble = -1;
                }
            }
            if (nibble >= 0)
                result.Add((byte)(nibble << 4));

            return result.ToArray();
        }

        private static byte[] ASCII85Decode(byte[] data)
        {
            var result = new List<byte>();
            int count = 0;
            long tuple = 0;

            for (int i = 0; i < data.Length; i++)
            {
                byte b = data[i];
                if (b == '~')
                {
                    // End of data (~>)
                    break;
                }
                if (IsWhitespace(b)) continue;

                if (b == 'z')
                {
                    // Special case: z = 4 zero bytes
                    result.Add(0);
                    result.Add(0);
                    result.Add(0);
                    result.Add(0);
                    continue;
                }

                if (b < '!' || b > 'u') continue;

                tuple = tuple * 85 + (b - '!');
                count++;

                if (count == 5)
                {
                    result.Add((byte)(tuple >> 24));
                    result.Add((byte)(tuple >> 16));
                    result.Add((byte)(tuple >> 8));
                    result.Add((byte)tuple);
                    tuple = 0;
                    count = 0;
                }
            }

            // Handle remaining bytes
            if (count > 0)
            {
                for (int i = count; i < 5; i++)
                    tuple = tuple * 85 + 84;

                for (int i = 0; i < count - 1; i++)
                {
                    result.Add((byte)(tuple >> (24 - i * 8)));
                }
            }

            return result.ToArray();
        }
    }
}
