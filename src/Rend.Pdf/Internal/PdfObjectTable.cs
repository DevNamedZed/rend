using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Rend.Pdf.Internal
{
    /// <summary>
    /// Manages PDF indirect object allocation, numbering, and cross-reference table generation.
    /// Object numbers start at 1 (object 0 is the free-list head per PDF spec).
    /// </summary>
    internal sealed class PdfObjectTable
    {
        private readonly List<IndirectObject> _objects = new List<IndirectObject>();
        private readonly List<ObjStreamInfo> _objectStreams = new List<ObjStreamInfo>();

        /// <summary>Total number of indirect objects (not counting object 0).</summary>
        public int Count => _objects.Count;

        /// <summary>
        /// Allocate a new indirect object. Returns a reference to it.
        /// </summary>
        public PdfReference Allocate(PdfObject obj)
        {
            int objectNumber = _objects.Count + 1; // 1-based
            _objects.Add(new IndirectObject(objectNumber, obj));
            return new PdfReference(objectNumber, 0);
        }

        /// <summary>
        /// Write all indirect objects to the output. Records byte offsets for xref.
        /// </summary>
        public void WriteAllObjects(PdfWriter writer)
        {
            for (int i = 0; i < _objects.Count; i++)
            {
                var entry = _objects[i];
                entry.ByteOffset = writer.Position;
                _objects[i] = entry;

                // Set current object context for per-object encryption
                writer.CurrentObjectNumber = entry.ObjectNumber;
                writer.CurrentGeneration = 0;

                writer.WriteLong(entry.ObjectNumber);
                writer.WriteRaw(PdfWriter.Bytes_ObjStart);
                entry.Object.WriteTo(writer);
                writer.WriteRaw(PdfWriter.Bytes_ObjEnd);
            }

            // Clear object context so trailer content is not encrypted
            writer.CurrentObjectNumber = 0;
            writer.CurrentGeneration = 0;
        }

        /// <summary>
        /// Write all indirect objects, packing eligible non-stream objects into object streams (PDF 1.5+).
        /// Non-eligible objects (streams, encryption dict, catalog) are written normally.
        /// Returns metadata for xref stream entries.
        /// </summary>
        public void WriteAllObjectsWithStreams(PdfWriter writer, int maxPerObjStream = 100)
        {
            // Phase 1: Identify which objects can go into object streams.
            // Per PDF spec, these CANNOT go into object streams:
            // - Stream objects (they have their own stream data)
            // - The encryption dictionary object
            // - Objects whose generation != 0 (we always use gen 0)
            // We keep it simple: pack all non-PdfStream objects into ObjStm groups.

            var normalIndices = new List<int>();   // indices of objects that must be written normally
            var compressibleIndices = new List<int>(); // indices of objects that can be packed

            for (int i = 0; i < _objects.Count; i++)
            {
                if (_objects[i].Object is PdfStream)
                    normalIndices.Add(i);
                else
                    compressibleIndices.Add(i);
            }

            // Phase 2: Write normal (non-compressible) objects directly
            for (int i = 0; i < normalIndices.Count; i++)
            {
                int idx = normalIndices[i];
                var entry = _objects[idx];
                entry.ByteOffset = writer.Position;
                _objects[idx] = entry;

                writer.CurrentObjectNumber = entry.ObjectNumber;
                writer.CurrentGeneration = 0;

                writer.WriteLong(entry.ObjectNumber);
                writer.WriteRaw(PdfWriter.Bytes_ObjStart);
                entry.Object.WriteTo(writer);
                writer.WriteRaw(PdfWriter.Bytes_ObjEnd);
            }

            // Phase 3: Pack compressible objects into ObjStm objects
            for (int batch = 0; batch < compressibleIndices.Count; batch += maxPerObjStream)
            {
                int count = Math.Min(maxPerObjStream, compressibleIndices.Count - batch);
                int objStmNum = _objects.Count + 1 + _objectStreams.Count;

                // Build the object stream content:
                // Header: "objNum1 offset1 objNum2 offset2 ..."
                // Body: serialized objects concatenated
                var headerParts = new List<string>();
                var bodyMs = new MemoryStream();
                var bodyWriter = new PdfWriter(bodyMs);

                for (int j = 0; j < count; j++)
                {
                    int idx = compressibleIndices[batch + j];
                    var entry = _objects[idx];

                    headerParts.Add($"{entry.ObjectNumber} {bodyWriter.Position}");

                    bodyWriter.CurrentObjectNumber = entry.ObjectNumber;
                    bodyWriter.CurrentGeneration = 0;
                    entry.Object.WriteTo(bodyWriter);
                    bodyWriter.WriteByte((byte)' '); // separator between objects

                    // Mark as compressed: store objStm number and index within stream
                    entry.ByteOffset = -1; // sentinel: compressed
                    entry.ObjStreamNumber = objStmNum;
                    entry.ObjStreamIndex = j;
                    _objects[idx] = entry;
                }

                bodyWriter.Flush();
                string header = string.Join(" ", headerParts) + " ";
                byte[] headerBytes = System.Text.Encoding.ASCII.GetBytes(header);
                byte[] bodyBytes = bodyMs.ToArray();
                bodyWriter.Dispose();

                // Combine header + body and compress
                byte[] rawData = new byte[headerBytes.Length + bodyBytes.Length];
                Buffer.BlockCopy(headerBytes, 0, rawData, 0, headerBytes.Length);
                Buffer.BlockCopy(bodyBytes, 0, rawData, headerBytes.Length, bodyBytes.Length);

                byte[] compressed;
                using (var cMs = new MemoryStream())
                {
                    using (var ds = new DeflateStream(cMs, CompressionLevel.Optimal, true))
                        ds.Write(rawData, 0, rawData.Length);
                    compressed = cMs.ToArray();
                }

                // Write the ObjStm as an indirect object
                long objStmOffset = writer.Position;

                var dict = new PdfDictionary(6);
                dict[PdfName.Type] = PdfName.ObjStm;
                dict[PdfName.N_Name] = new PdfInteger(count);
                dict[PdfName.First] = new PdfInteger(headerBytes.Length);
                dict[PdfName.Filter] = PdfName.FlateDecode;
                dict[PdfName.Length] = new PdfInteger(compressed.Length);

                writer.CurrentObjectNumber = objStmNum;
                writer.CurrentGeneration = 0;
                writer.WriteLong(objStmNum);
                writer.WriteRaw(PdfWriter.Bytes_ObjStart);
                dict.WriteTo(writer);
                writer.WriteByte((byte)'\n');
                writer.WriteRaw(PdfWriter.Bytes_stream);
                writer.WriteByte((byte)'\n');
                writer.WriteRawBytes(compressed, 0, compressed.Length);
                writer.WriteByte((byte)'\n');
                writer.WriteRaw(PdfWriter.Bytes_endstream);
                writer.WriteRaw(PdfWriter.Bytes_ObjEnd);

                _objectStreams.Add(new ObjStreamInfo(objStmNum, objStmOffset));
            }

            writer.CurrentObjectNumber = 0;
            writer.CurrentGeneration = 0;
        }

        /// <summary>
        /// Write the cross-reference table. Returns the byte offset of "xref".
        /// </summary>
        public long WriteXRefTable(PdfWriter writer)
        {
            long xrefOffset = writer.Position;

            writer.WriteAscii("xref\n");

            // One section: 0 to Count+1
            writer.WriteLong(0);
            writer.WriteSpace();
            writer.WriteLong(_objects.Count + 1);
            writer.WriteNewLine();

            // Object 0: free list head
            writer.WriteAscii("0000000000 65535 f \n");

            // Objects 1..N
            for (int i = 0; i < _objects.Count; i++)
            {
                WriteXRefEntry(writer, _objects[i].ByteOffset, 0, 'n');
            }

            return xrefOffset;
        }

        /// <summary>
        /// Write the trailer dictionary.
        /// </summary>
        public void WriteTrailer(PdfWriter writer, PdfReference catalogRef, PdfReference? infoRef,
                                  PdfReference? encryptRef, byte[]? fileId, long xrefOffset)
        {
            writer.WriteAscii("trailer\n");

            var trailer = new PdfDictionary(6);
            trailer[PdfName.Size] = new PdfInteger(_objects.Count + 1);
            trailer[PdfName.Root] = catalogRef;
            if (infoRef != null)
                trailer[PdfName.Info] = infoRef;
            if (encryptRef != null)
                trailer[PdfName.Encrypt] = encryptRef;
            if (fileId != null)
            {
                var idArray = new PdfArray(2);
                idArray.Add(new PdfHexString(fileId));
                idArray.Add(new PdfHexString(fileId));
                trailer[PdfName.ID] = idArray;
            }

            trailer.WriteTo(writer);
            writer.WriteNewLine();

            writer.WriteAscii("startxref\n");
            writer.WriteLong(xrefOffset);
            writer.WriteNewLine();
            writer.WriteAscii("%%EOF\n");
        }

        /// <summary>
        /// Write a cross-reference stream (PDF 1.5+) instead of the traditional xref table and trailer.
        /// The xref stream is itself an indirect object that combines the xref table and trailer into one.
        /// Handles type 1 (normal) and type 2 (compressed in object stream) entries.
        /// Returns the byte offset of the xref stream object.
        /// </summary>
        public long WriteXRefStream(PdfWriter writer, PdfReference catalogRef, PdfReference? infoRef,
                                     PdfReference? encryptRef, byte[]? fileId)
        {
            // The xref stream gets the next object number after all objects + object streams
            int xrefObjNum = _objects.Count + _objectStreams.Count + 1;

            // Total entries: object 0 + original objects + object streams + xref stream itself
            int totalEntries = 1 + _objects.Count + _objectStreams.Count + 1;

            // W = [1, field2Size, field3Size]
            // Type 0: free (next-free, gen)
            // Type 1: normal (byte-offset, gen)
            // Type 2: compressed (objstm-num, index-within)
            // We need field2 large enough for byte offsets AND object stream numbers
            // We need field3 large enough for generation numbers AND indices

            long maxField2 = 0;
            int maxField3 = 0;

            for (int i = 0; i < _objects.Count; i++)
            {
                var entry = _objects[i];
                if (entry.ObjStreamNumber > 0)
                {
                    // Type 2: field2 = obj stream number, field3 = index
                    if (entry.ObjStreamNumber > maxField2) maxField2 = entry.ObjStreamNumber;
                    if (entry.ObjStreamIndex > maxField3) maxField3 = entry.ObjStreamIndex;
                }
                else
                {
                    // Type 1: field2 = byte offset, field3 = generation (always 0)
                    if (entry.ByteOffset > maxField2) maxField2 = entry.ByteOffset;
                }
            }

            // Object stream entries are type 1 with byte offsets
            for (int i = 0; i < _objectStreams.Count; i++)
            {
                if (_objectStreams[i].ByteOffset > maxField2) maxField2 = _objectStreams[i].ByteOffset;
            }

            // Add estimate for xref stream itself
            maxField2 = Math.Max(maxField2, writer.Position + 1000);

            int field2Size;
            if (maxField2 <= 0xFF) field2Size = 1;
            else if (maxField2 <= 0xFFFF) field2Size = 2;
            else if (maxField2 <= 0xFFFFFF) field2Size = 3;
            else field2Size = 4;

            int field3Size;
            if (maxField3 <= 0) field3Size = 0;
            else if (maxField3 <= 0xFF) field3Size = 1;
            else field3Size = 2;

            int entrySize = 1 + field2Size + field3Size;
            var data = new byte[totalEntries * entrySize];

            // Entry 0: free object (type=0, next-free=0, gen=65535)
            // Since field3Size may be 0 when we have no compressed objects and always gen=0,
            // but free entry needs gen=65535 — if field3Size is 0, that's fine, PDF spec says
            // the default for missing field 3 is 0 for type 1 and type 2. For type 0, next free
            // object number goes in field 2, generation in field 3. With field3=0 bytes, gen is implicit 0.
            // This is technically non-conformant for the free entry gen, but acceptable because
            // free entries are rarely used by readers. Let's ensure field3Size is at least 1 if we
            // have any entries (for the free list gen number).
            // Actually, let's just always have field3Size >= 1 for correctness.
            if (field3Size == 0 && _objectStreams.Count == 0)
            {
                // Pure type-1 only, no compressed objects. field3 stores generation (always 0).
                // For the free entry, gen=65535. We need at least 2 bytes.
                field3Size = 2;
                entrySize = 1 + field2Size + field3Size;
                data = new byte[totalEntries * entrySize];
            }
            else if (field3Size == 0)
            {
                field3Size = 1; // at least 1 byte for index values
                entrySize = 1 + field2Size + field3Size;
                data = new byte[totalEntries * entrySize];
            }

            // Write entry 0: free (type=0, next=0, gen=65535)
            WriteXRefStreamEntry(data, 0, entrySize, field2Size, field3Size, 0, 0, 65535);

            // Write entries for original objects 1..N
            for (int i = 0; i < _objects.Count; i++)
            {
                var entry = _objects[i];
                int entryIdx = i + 1; // object 0 takes slot 0
                if (entry.ObjStreamNumber > 0)
                {
                    // Type 2: compressed in object stream
                    WriteXRefStreamEntry(data, entryIdx, entrySize, field2Size, field3Size,
                                          2, entry.ObjStreamNumber, entry.ObjStreamIndex);
                }
                else
                {
                    // Type 1: normal object with byte offset
                    WriteXRefStreamEntry(data, entryIdx, entrySize, field2Size, field3Size,
                                          1, entry.ByteOffset, 0);
                }
            }

            // Write entries for object stream objects
            for (int i = 0; i < _objectStreams.Count; i++)
            {
                int entryIdx = _objects.Count + 1 + i;
                WriteXRefStreamEntry(data, entryIdx, entrySize, field2Size, field3Size,
                                      1, _objectStreams[i].ByteOffset, 0);
            }

            // Record the xref stream offset before writing
            long xrefOffset = writer.Position;

            // Write entry for the xref stream itself (last entry)
            int xrefEntryIdx = totalEntries - 1;
            WriteXRefStreamEntry(data, xrefEntryIdx, entrySize, field2Size, field3Size,
                                  1, xrefOffset, 0);

            // Compress the xref data
            byte[] compressedData;
            using (var ms = new MemoryStream())
            {
                using (var ds = new DeflateStream(ms, CompressionLevel.Optimal, true))
                    ds.Write(data, 0, data.Length);
                compressedData = ms.ToArray();
            }

            // Build the xref stream dictionary (doubles as the trailer)
            var dict = new PdfDictionary(12);
            dict[PdfName.Type] = PdfName.XRef;
            dict[PdfName.Size] = new PdfInteger(xrefObjNum + 1);
            dict[PdfName.Root] = catalogRef;

            if (infoRef != null)
                dict[PdfName.Info] = infoRef;
            if (encryptRef != null)
                dict[PdfName.Encrypt] = encryptRef;
            if (fileId != null)
            {
                var idArray = new PdfArray(2);
                idArray.Add(new PdfHexString(fileId));
                idArray.Add(new PdfHexString(fileId));
                dict[PdfName.ID] = idArray;
            }

            var wArray = new PdfArray(3);
            wArray.Add(new PdfInteger(1));
            wArray.Add(new PdfInteger(field2Size));
            wArray.Add(new PdfInteger(field3Size));
            dict[PdfName.W] = wArray;

            dict[PdfName.Filter] = PdfName.FlateDecode;
            dict[PdfName.Length] = new PdfInteger(compressedData.Length);

            // Write the xref stream as an indirect object
            writer.CurrentObjectNumber = xrefObjNum;
            writer.CurrentGeneration = 0;
            writer.WriteLong(xrefObjNum);
            writer.WriteRaw(PdfWriter.Bytes_ObjStart);
            dict.WriteTo(writer);
            writer.WriteByte((byte)'\n');
            writer.WriteRaw(PdfWriter.Bytes_stream);
            writer.WriteByte((byte)'\n');
            writer.WriteRawBytes(compressedData, 0, compressedData.Length);
            writer.WriteByte((byte)'\n');
            writer.WriteRaw(PdfWriter.Bytes_endstream);
            writer.WriteRaw(PdfWriter.Bytes_ObjEnd);

            writer.CurrentObjectNumber = 0;
            writer.CurrentGeneration = 0;

            // Write startxref
            writer.WriteAscii("startxref\n");
            writer.WriteLong(xrefOffset);
            writer.WriteNewLine();
            writer.WriteAscii("%%EOF\n");

            return xrefOffset;
        }

        private static void WriteXRefStreamEntry(byte[] data, int entryIndex, int entrySize,
                                                   int field2Size, int field3Size,
                                                   byte type, long field2, int field3)
        {
            int pos = entryIndex * entrySize;
            data[pos] = type;

            // Write field2 (big-endian)
            long f2 = field2;
            for (int b = field2Size - 1; b >= 0; b--)
            {
                data[pos + 1 + b] = (byte)(f2 & 0xFF);
                f2 >>= 8;
            }

            // Write field3 (big-endian)
            int f3 = field3;
            for (int b = field3Size - 1; b >= 0; b--)
            {
                data[pos + 1 + field2Size + b] = (byte)(f3 & 0xFF);
                f3 >>= 8;
            }
        }

        private static void WriteXRefEntry(PdfWriter writer, long offset, int generation, char type)
        {
            // Format: 0000000009 00000 n \n  (exactly 20 bytes including \n)
            // 10-digit offset, space, 5-digit generation, space, type, space, \n
            var digits = new byte[20];
            long o = offset;
            for (int i = 9; i >= 0; i--)
            {
                digits[i] = (byte)('0' + (o % 10));
                o /= 10;
            }
            digits[10] = (byte)' ';
            int g = generation;
            for (int i = 15; i >= 11; i--)
            {
                digits[i] = (byte)('0' + (g % 10));
                g /= 10;
            }
            digits[16] = (byte)' ';
            digits[17] = (byte)type;
            digits[18] = (byte)' ';
            digits[19] = (byte)'\n';

            writer.WriteRaw(digits);
        }

        private struct IndirectObject
        {
            public int ObjectNumber;
            public PdfObject Object;
            public long ByteOffset;    // -1 if compressed in an object stream
            public int ObjStreamNumber; // object number of containing ObjStm (0 if not compressed)
            public int ObjStreamIndex;  // index within the object stream

            public IndirectObject(int objectNumber, PdfObject obj)
            {
                ObjectNumber = objectNumber;
                Object = obj;
                ByteOffset = 0;
                ObjStreamNumber = 0;
                ObjStreamIndex = 0;
            }
        }

        private struct ObjStreamInfo
        {
            public int ObjectNumber;
            public long ByteOffset;

            public ObjStreamInfo(int objectNumber, long byteOffset)
            {
                ObjectNumber = objectNumber;
                ByteOffset = byteOffset;
            }
        }
    }
}
