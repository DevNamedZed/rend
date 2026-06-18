#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Rend.Pdf.Parsing
{
    public sealed class PdfDocumentReader : IDisposable
    {
        private byte[] _data;
        private int _pos;
        private readonly Dictionary<int, (long offset, int gen)> _xrefTable = new Dictionary<int, (long, int)>();
        private readonly Dictionary<int, PdfObj> _objectCache = new Dictionary<int, PdfObj>();
        private readonly List<PdfObj> _pages = new List<PdfObj>();
        private PdfObj _trailer = PdfObj.Null;
        private PdfDecryptor? _decryptor;
        private int _encryptObjNum = -1;
        private PdfObj _catalog = PdfObj.Null;
        #pragma warning disable CS0414
        private bool _disposed;
        #pragma warning restore CS0414

        public PdfObj Trailer => _trailer;
        public PdfObj Catalog => _catalog;
        public int PageCount => _pages.Count;
        public string HeaderVersion { get; private set; } = "";
        private readonly List<string> _parseWarnings = new List<string>();
        public IReadOnlyList<string> ParseWarnings => _parseWarnings;

        private PdfDocumentReader(byte[] data)
        {
            _data = data;
            ParseHeaderVersion();
            Parse();
        }

        private void ParseHeaderVersion()
        {
            if (_data.Length >= 8 && _data[0] == '%' && _data[1] == 'P' && _data[2] == 'D' && _data[3] == 'F' && _data[4] == '-')
            {
                int end = 5;
                while (end < _data.Length && end < 12 && _data[end] != '\r' && _data[end] != '\n')
                {
                    end++;
                }
                HeaderVersion = System.Text.Encoding.ASCII.GetString(_data, 5, end - 5);
            }
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
                        if (!parsed.IsNull)
                        {
                            ApplyDecryption(parsed, r.ObjNum, entry.gen);
                            _objectCache[r.ObjNum] = parsed;
                            return parsed;
                        }
                        // For compressed objects, ParseCompressedObject caches all objects
                        // in the stream. Check cache again.
                        if (_objectCache.TryGetValue(r.ObjNum, out var cachedAfterParse))
                        {
                            return cachedAfterParse;
                        }
                        return PdfObj.Null;
                    }
                    catch (Exception resolveException)
                    {
                        _parseWarnings.Add($"Failed to resolve object {r.ObjNum}: {resolveException.Message}");
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

            if (_decryptor != null && stream.OwnerObjNum >= 0 && stream.OwnerObjNum != _encryptObjNum)
            {
                raw = _decryptor.Decrypt(raw, stream.OwnerObjNum, stream.OwnerGen);
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
            if (!_disposed)
            {
                _disposed = true;
                _data = Array.Empty<byte>();
                _objectCache.Clear();
                _xrefTable.Clear();
                _pages.Clear();
                _trailer = PdfObj.Null;
                _catalog = PdfObj.Null;
            }
        }

        // ─── Parsing ─────────────────────────────────────────────────────

        private void Parse()
        {
            long startXRef = FindStartXRef();
            ParseXRef(startXRef);
            SetupEncryption();
            LoadCatalogAndPages();
        }

        // [SPEC §7.6] Standard Security Handler. Builds a decryptor from the /Encrypt dict
        // (empty user password) before any content objects are resolved. The /Encrypt dict,
        // the /ID and xref streams are themselves never encrypted.
        private void SetupEncryption()
        {
            var encryptRef = _trailer["Encrypt"];
            if (encryptRef.IsNull)
            {
                return;
            }
            if (encryptRef is PdfRef encryptReference)
            {
                _encryptObjNum = encryptReference.ObjNum;
            }

            var encrypt = Resolve(encryptRef);
            if (encrypt.IsNull)
            {
                return;
            }
            if (StripName(Resolve(encrypt["Filter"]).AsName()) != "Standard")
            {
                _parseWarnings.Add("Unsupported PDF security handler (only Standard is supported)");
                return;
            }

            int version = (int)Resolve(encrypt["V"]).AsInt();
            int revision = (int)Resolve(encrypt["R"]).AsInt();
            if (version >= 5)
            {
                _parseWarnings.Add("AES-256 (V5/R6) PDF encryption is not yet supported");
                return;
            }

            int keyLength = encrypt["Length"].IsNull ? 40 : (int)Resolve(encrypt["Length"]).AsInt();
            byte[] oValue = Resolve(encrypt["O"]).AsBytes();
            byte[] uValue = Resolve(encrypt["U"]).AsBytes();
            int permissions = (int)Resolve(encrypt["P"]).AsInt();
            byte[] fileId = GetFirstFileId();
            bool useAes = UsesAesV2(encrypt, version);

            try
            {
                var decryptor = new PdfDecryptor(revision, keyLength, oValue, permissions, fileId, useAes);
                if (decryptor.IsUserPasswordValid(uValue))
                {
                    _decryptor = decryptor;
                }
                else
                {
                    _parseWarnings.Add("PDF requires a user password; cannot decrypt with the empty password");
                }
            }
            catch (Exception encryptionException)
            {
                _parseWarnings.Add($"Failed to initialize PDF decryption: {encryptionException.Message}");
                _decryptor = null;
            }
        }

        private bool UsesAesV2(PdfObj encrypt, int version)
        {
            if (version < 4)
            {
                return false;
            }
            string streamFilter = StripName(Resolve(encrypt["StmF"]).AsName());
            if (streamFilter.Length == 0 || streamFilter == "Identity")
            {
                return false;
            }
            var cryptFilters = Resolve(encrypt["CF"]);
            var filterDict = Resolve(cryptFilters[streamFilter]);
            return StripName(Resolve(filterDict["CFM"]).AsName()).Contains("AESV2");
        }

        private byte[] GetFirstFileId()
        {
            var id = _trailer["ID"];
            if (id is PdfArray idArray && idArray.Count > 0)
            {
                return Resolve(idArray[0]).AsBytes();
            }
            return Array.Empty<byte>();
        }

        private static string StripName(string name)
        {
            return name.StartsWith("/") ? name.Substring(1) : name;
        }

        // Decrypts the strings of a freshly-parsed indirect object in place and tags any
        // stream with its owner so GetStreamBytes can decrypt it. Runs once per object,
        // before it is cached. Not called for objects inside an object stream (those are
        // already decrypted as part of the containing stream).
        private void ApplyDecryption(PdfObj obj, int objectNumber, int generation)
        {
            if (_decryptor == null || objectNumber == _encryptObjNum)
            {
                return;
            }
            if (obj is PdfStream stream)
            {
                stream.OwnerObjNum = objectNumber;
                stream.OwnerGen = generation;
            }
            DecryptStringsIn(obj, objectNumber, generation);
        }

        private void DecryptStringsIn(PdfObj obj, int objectNumber, int generation)
        {
            switch (obj)
            {
                case PdfString str:
                    str.Bytes = _decryptor!.Decrypt(str.Bytes, objectNumber, generation);
                    break;
                case PdfDict dict:
                    foreach (var value in dict.Entries.Values)
                    {
                        DecryptStringsIn(value, objectNumber, generation);
                    }
                    break;
                case PdfArray array:
                    for (int i = 0; i < array.Count; i++)
                    {
                        DecryptStringsIn(array[i], objectNumber, generation);
                    }
                    break;
            }
        }

        private void LoadCatalogAndPages()
        {
            var rootRef = _trailer["Root"];
            var root = Resolve(rootRef);
            if (root.IsNull)
            {
                _parseWarnings.Add($"Catalog not found: trailer Root={rootRef}");
                return;
            }
            _catalog = root;

            var pagesRef = root["Pages"];
            var pagesObj = Resolve(pagesRef);
            if (pagesObj.IsNull)
            {
                if (pagesRef.IsRef)
                {
                    _parseWarnings.Add($"Cannot resolve page tree object {pagesRef} (may be encrypted)");
                }
                else
                {
                    _parseWarnings.Add("Catalog does not contain a Pages entry");
                }
                return;
            }

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
            int searchStart = Math.Max(0, _data.Length - 4096);
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
                long streamObjNum = -(offset + 1);
                return ParseCompressedObject((int)streamObjNum);
            }

            _pos = (int)offset;
            return ParseIndirectObject();
        }

        private PdfObj ParseCompressedObject(int streamObjNum)
        {
            var streamObj = Resolve(new PdfRef(streamObjNum, 0));
            if (!(streamObj is PdfStream objStream))
            {
                return PdfObj.Null;
            }

            byte[] decoded = GetStreamBytes(objStream);
            if (decoded.Length == 0)
            {
                return PdfObj.Null;
            }

            int objectCount = (int)Resolve(objStream["N"]).AsInt();
            int firstOffset = (int)Resolve(objStream["First"]).AsInt();

            var savedPos = _pos;
            var savedData = _data;
            _data = decoded;
            _pos = 0;

            try
            {
                var objectNumbers = new int[objectCount];
                var objectOffsets = new int[objectCount];
                for (int i = 0; i < objectCount; i++)
                {
                    SkipWhitespaceAndComments();
                    objectNumbers[i] = ReadIntDirect();
                    SkipWhitespaceAndComments();
                    objectOffsets[i] = ReadIntDirect();
                    SkipWhitespaceAndComments();
                }

                // Parse ALL objects in this stream and cache them
                for (int i = 0; i < objectCount; i++)
                {
                    int objectNumber = objectNumbers[i];
                    if (_objectCache.ContainsKey(objectNumber))
                    {
                        continue;
                    }

                    _pos = firstOffset + objectOffsets[i];
                    SkipWhitespaceAndComments();
                    var parsed = ParseObject();
                    _objectCache[objectNumber] = parsed;
                }

                return PdfObj.Null;
            }
            finally
            {
                _data = savedData;
                _pos = savedPos;
            }
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
            var resolvedParms = Resolve(parms);
            return StreamDecoder.ApplyFilter(filter, data, resolvedParms);
        }

        // Stream decompression moved to StreamDecoder.cs
        // DecodeStreamRaw below is kept for xref stream parsing (needs inline filter resolution)
    }
}
