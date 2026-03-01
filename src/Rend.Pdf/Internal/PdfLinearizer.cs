using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rend.Pdf.Internal
{
    /// <summary>
    /// Post-processes a standard PDF into a linearized (fast web view) PDF.
    /// Linearization reorders objects so the first page can be displayed before
    /// the entire file is downloaded, per ISO 32000-1 Annex F.
    /// </summary>
    internal static class PdfLinearizer
    {
        /// <summary>
        /// Linearize a PDF byte array. Parses the existing xref table, classifies
        /// objects by page, and rewrites them in linearized order.
        /// </summary>
        public static byte[] Linearize(byte[] pdfBytes)
        {
            // 1. Parse the existing PDF structure
            var parser = new PdfByteParser(pdfBytes);
            var header = parser.ReadHeader();
            var xrefOffset = parser.FindStartXRef();
            var xrefEntries = parser.ReadXRefTable(xrefOffset);
            var trailerDict = parser.ReadTrailerDict();

            int catalogObjNum = trailerDict.GetRefObjectNumber("Root");
            int? infoObjNum = trailerDict.TryGetRefObjectNumber("Info");

            // 2. Parse the catalog to find the page tree
            var catalogDict = parser.ReadObjectDict(xrefEntries[catalogObjNum]);
            int pagesObjNum = catalogDict.GetRefObjectNumber("Kids") != -1
                ? catalogDict.GetRefObjectNumber("Kids")
                : catalogDict.GetRefObjectNumber("Pages");

            // Actually the catalog has /Pages pointing to the page tree root
            pagesObjNum = catalogDict.GetRefObjectNumber("Pages");

            // 3. Parse the page tree to get all page object numbers
            var pageObjectNumbers = new List<int>();
            var pageContentObjects = new Dictionary<int, List<int>>(); // page obj -> content stream objs
            var pageResourceObjects = new Dictionary<int, List<int>>(); // page obj -> resource-referenced objs

            CollectPages(parser, xrefEntries, pagesObjNum, pageObjectNumbers,
                         pageContentObjects, pageResourceObjects);

            int pageCount = pageObjectNumbers.Count;
            if (pageCount == 0)
                return pdfBytes; // degenerate case

            // 4. Classify objects
            // First page objects: page dict, its content streams, and resources referenced only by page 1
            // Shared objects: catalog, info, page tree, fonts/images shared across pages
            // Remaining: page 2+ objects

            var firstPageObjNum = pageObjectNumbers[0];
            var firstPageObjects = new HashSet<int> { firstPageObjNum };

            // Add first page content streams
            if (pageContentObjects.TryGetValue(firstPageObjNum, out var fp1Contents))
                foreach (var c in fp1Contents) firstPageObjects.Add(c);

            // Add first page resource objects
            if (pageResourceObjects.TryGetValue(firstPageObjNum, out var fp1Resources))
                foreach (var r in fp1Resources) firstPageObjects.Add(r);

            // Shared objects: catalog, pages tree root, info
            var sharedObjects = new HashSet<int> { catalogObjNum, pagesObjNum };
            if (infoObjNum.HasValue)
                sharedObjects.Add(infoObjNum.Value);

            // Collect page tree intermediate nodes
            CollectPageTreeNodes(parser, xrefEntries, pagesObjNum, sharedObjects);

            // Find all encrypt dict ref if present
            int? encryptObjNum = trailerDict.TryGetRefObjectNumber("Encrypt");
            if (encryptObjNum.HasValue)
                sharedObjects.Add(encryptObjNum.Value);

            // Remaining page objects (pages 2+)
            var remainingPageObjects = new List<int>();
            for (int i = 1; i < pageObjectNumbers.Count; i++)
            {
                var pn = pageObjectNumbers[i];
                remainingPageObjects.Add(pn);
                if (pageContentObjects.TryGetValue(pn, out var contents))
                    foreach (var c in contents) remainingPageObjects.Add(c);
                if (pageResourceObjects.TryGetValue(pn, out var resources))
                    foreach (var r in resources) remainingPageObjects.Add(r);
            }

            // Any objects not yet classified go into shared
            var allClassified = new HashSet<int>(firstPageObjects);
            allClassified.UnionWith(sharedObjects);
            allClassified.UnionWith(remainingPageObjects);

            var unclassified = new List<int>();
            foreach (var kvp in xrefEntries)
            {
                if (kvp.Key > 0 && !allClassified.Contains(kvp.Key))
                    unclassified.Add(kvp.Key);
            }
            // Put unclassified objects into shared
            foreach (var u in unclassified)
                sharedObjects.Add(u);

            // 5. Build the write order:
            //    - Linearization dict (new object, will get the next available number)
            //    - Shared objects (catalog, page tree, info, fonts, etc.)
            //    - First page objects
            //    - Remaining page objects

            int maxObjNum = 0;
            foreach (var kvp in xrefEntries)
                if (kvp.Key > maxObjNum) maxObjNum = kvp.Key;

            int linearizationObjNum = maxObjNum + 1;
            int totalObjects = maxObjNum + 2; // +1 for obj 0, +1 for linearization dict

            // Build ordered list of object numbers to write
            var writeOrder = new List<int>();

            // Linearization dict first
            writeOrder.Add(linearizationObjNum);

            // Shared objects (catalog first so it's near the top)
            writeOrder.Add(catalogObjNum);
            writeOrder.Add(pagesObjNum);
            foreach (var s in sharedObjects)
            {
                if (s != catalogObjNum && s != pagesObjNum)
                    writeOrder.Add(s);
            }

            // First page objects
            writeOrder.Add(firstPageObjNum);
            foreach (var fp in firstPageObjects)
            {
                if (fp != firstPageObjNum)
                    writeOrder.Add(fp);
            }

            // Remaining page objects
            foreach (var rp in remainingPageObjects)
                writeOrder.Add(rp);

            // Deduplicate while preserving order
            var seen = new HashSet<int>();
            var deduped = new List<int>();
            foreach (var obj in writeOrder)
            {
                if (seen.Add(obj))
                    deduped.Add(obj);
            }
            writeOrder = deduped;

            // 6. Rewrite the PDF
            using var output = new MemoryStream();

            // Write header (same as original)
            output.Write(pdfBytes, 0, header.HeaderEndOffset);

            // We need two passes: first to calculate offsets, then to write.
            // Use a simpler approach: write objects, then fix up the linearization dict.

            // Write a placeholder linearization dictionary (we'll fix L, H, E, T later)
            // The linearization dict has a fixed-size format so we can patch it.
            long linDictOffset = output.Position;
            byte[] linDictPlaceholder = BuildLinearizationDictPlaceholder(
                linearizationObjNum, firstPageObjNum, pageCount);
            output.Write(linDictPlaceholder, 0, linDictPlaceholder.Length);

            // Write first-page xref section
            // (Linearized PDFs can have a partial xref at the start)
            // For simplicity, we write a single xref at the end.

            // Track new offsets for all objects
            var newOffsets = new Dictionary<int, long>();
            newOffsets[linearizationObjNum] = linDictOffset;

            // Write shared + first page objects
            int endOfFirstPageIndex = -1;
            for (int i = 1; i < writeOrder.Count; i++) // skip linearization dict (already written)
            {
                int objNum = writeOrder[i];
                long objOffset = output.Position;
                newOffsets[objNum] = objOffset;

                // Copy original object bytes
                WriteOriginalObject(output, pdfBytes, xrefEntries, objNum);

                // Mark end of first page section
                if (objNum == firstPageObjNum)
                    endOfFirstPageIndex = i;
            }

            // Determine end of first page (E value)
            // E = offset of end of first page section
            long endOfFirstPage;
            if (endOfFirstPageIndex >= 0)
            {
                // E is the offset just past the last first-page object
                // Find the last first-page object in write order
                int lastFpIdx = -1;
                for (int i = 0; i < writeOrder.Count; i++)
                {
                    if (firstPageObjects.Contains(writeOrder[i]))
                        lastFpIdx = i;
                }
                if (lastFpIdx >= 0 && lastFpIdx + 1 < writeOrder.Count)
                    endOfFirstPage = newOffsets[writeOrder[lastFpIdx + 1]];
                else
                    endOfFirstPage = output.Position;
            }
            else
            {
                endOfFirstPage = output.Position;
            }

            // Write hint stream as an object
            // Minimal page offset hint table (Annex F.3)
            int hintObjNum = linearizationObjNum + 1;
            totalObjects++;
            long hintOffset = output.Position;

            byte[] hintStream = BuildMinimalHintStream(pageCount, pageObjectNumbers,
                                                        newOffsets, xrefEntries, pdfBytes);
            WriteHintObject(output, hintObjNum, hintStream);
            long hintLength = output.Position - hintOffset;

            // Write xref table
            long xrefPos = output.Position;
            WriteXRefTable(output, totalObjects, newOffsets, hintObjNum, hintOffset);

            // Write trailer
            byte[]? fileId = ExtractFileId(pdfBytes, xrefOffset);
            WriteTrailer(output, totalObjects, catalogObjNum, infoObjNum,
                        encryptObjNum, fileId, xrefPos);

            // Get total file length
            long fileLength = output.Position;

            // 7. Patch the linearization dictionary with actual values
            byte[] result = output.ToArray();
            PatchLinearizationDict(result, linDictOffset, linDictPlaceholder.Length,
                                    fileLength, hintOffset, hintLength,
                                    firstPageObjNum, endOfFirstPage, pageCount, xrefPos);

            return result;
        }

        private static byte[] BuildLinearizationDictPlaceholder(
            int linObjNum, int firstPageObjNum, int pageCount)
        {
            // Build a fixed-width linearization dictionary.
            // Use padded numbers so we can patch in-place later.
            // Format: N obj\n<</Linearized 1/L XXXXXXXXXX/H [XXXXXXXXXX XXXXXXXXXX]/O N/E XXXXXXXXXX/N N/T XXXXXXXXXX>>\nendobj\n
            var sb = new StringBuilder();
            sb.Append(linObjNum);
            sb.Append(" 0 obj\n");
            sb.Append("<<");
            sb.Append("/Linearized 1");
            sb.Append("/L ");
            sb.Append("0000000000"); // 10-digit placeholder for file length
            sb.Append("/H [");
            sb.Append("0000000000"); // hint table offset
            sb.Append(' ');
            sb.Append("0000000000"); // hint table length
            sb.Append(']');
            sb.Append("/O ");
            sb.AppendFormat("{0,-10}", firstPageObjNum); // first page object number
            sb.Append("/E ");
            sb.Append("0000000000"); // end of first page
            sb.Append("/N ");
            sb.AppendFormat("{0,-10}", pageCount); // page count
            sb.Append("/T ");
            sb.Append("0000000000"); // main xref offset
            sb.Append(">>");
            sb.Append("\nendobj\n");

            return Encoding.ASCII.GetBytes(sb.ToString());
        }

        private static void PatchLinearizationDict(byte[] data, long dictOffset,
            int dictLength, long fileLength, long hintOffset, long hintLength,
            int firstPageObjNum, long endOfFirstPage, int pageCount, long xrefOffset)
        {
            // Rebuild the linearization dict content with actual values
            // Find the start of the dictionary content (after "N 0 obj\n")
            string dictStr = Encoding.ASCII.GetString(data, (int)dictOffset, dictLength);
            int dictContentStart = dictStr.IndexOf("<<", StringComparison.Ordinal);
            if (dictContentStart < 0) return;

            // Patch /L value
            PatchValue(data, (int)dictOffset, dictLength, "/L ", fileLength);

            // Patch /H values [offset length]
            PatchHintValues(data, (int)dictOffset, dictLength, "/H [", hintOffset, hintLength);

            // Patch /O value
            PatchPaddedValue(data, (int)dictOffset, dictLength, "/O ", firstPageObjNum);

            // Patch /E value
            PatchValue(data, (int)dictOffset, dictLength, "/E ", endOfFirstPage);

            // Patch /N value
            PatchPaddedValue(data, (int)dictOffset, dictLength, "/N ", pageCount);

            // Patch /T value
            PatchValue(data, (int)dictOffset, dictLength, "/T ", xrefOffset);
        }

        private static void PatchValue(byte[] data, int regionStart, int regionLength,
            string marker, long value)
        {
            string region = Encoding.ASCII.GetString(data, regionStart, regionLength);
            int idx = region.IndexOf(marker, StringComparison.Ordinal);
            if (idx < 0) return;

            int valueStart = regionStart + idx + marker.Length;
            string formatted = value.ToString().PadLeft(10, '0');
            for (int i = 0; i < 10 && i < formatted.Length; i++)
                data[valueStart + i] = (byte)formatted[i];
        }

        private static void PatchPaddedValue(byte[] data, int regionStart, int regionLength,
            string marker, int value)
        {
            string region = Encoding.ASCII.GetString(data, regionStart, regionLength);
            int idx = region.IndexOf(marker, StringComparison.Ordinal);
            if (idx < 0) return;

            int valueStart = regionStart + idx + marker.Length;
            string formatted = string.Format("{0,-10}", value);
            for (int i = 0; i < 10 && i < formatted.Length; i++)
                data[valueStart + i] = (byte)formatted[i];
        }

        private static void PatchHintValues(byte[] data, int regionStart, int regionLength,
            string marker, long offset, long length)
        {
            string region = Encoding.ASCII.GetString(data, regionStart, regionLength);
            int idx = region.IndexOf(marker, StringComparison.Ordinal);
            if (idx < 0) return;

            int valueStart = regionStart + idx + marker.Length;
            string offsetStr = offset.ToString().PadLeft(10, '0');
            string lengthStr = length.ToString().PadLeft(10, '0');

            for (int i = 0; i < 10; i++)
                data[valueStart + i] = (byte)offsetStr[i];
            // skip space
            for (int i = 0; i < 10; i++)
                data[valueStart + 11 + i] = (byte)lengthStr[i];
        }

        private static void WriteOriginalObject(MemoryStream output, byte[] pdfBytes,
            Dictionary<int, long> xrefEntries, int objNum)
        {
            if (!xrefEntries.TryGetValue(objNum, out long offset))
                return;

            // Find the extent of this object: from "N 0 obj" to "endobj\n"
            int start = (int)offset;
            int end = FindEndObj(pdfBytes, start);
            if (end <= start)
                return;

            output.Write(pdfBytes, start, end - start);
        }

        private static int FindEndObj(byte[] data, int start)
        {
            // Search for "endobj" followed by whitespace
            byte[] endObjMarker = Encoding.ASCII.GetBytes("endobj");
            for (int i = start; i < data.Length - endObjMarker.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < endObjMarker.Length; j++)
                {
                    if (data[i + j] != endObjMarker[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    int end = i + endObjMarker.Length;
                    // Consume trailing whitespace (newline)
                    while (end < data.Length && (data[end] == '\n' || data[end] == '\r'))
                        end++;
                    return end;
                }
            }
            return data.Length;
        }

        private static byte[] BuildMinimalHintStream(int pageCount,
            List<int> pageObjectNumbers, Dictionary<int, long> newOffsets,
            Dictionary<int, long> xrefEntries, byte[] pdfBytes)
        {
            // Build a minimal page offset hint table per Annex F.3
            // This is the minimum required for linearized PDF validity.
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            // Page offset hint table header (Table F.3)
            // Item 1: Least number of objects in a page
            int minObjects = int.MaxValue;
            int maxObjects = 0;
            var pageSizes = new List<int>();

            for (int i = 0; i < pageCount; i++)
            {
                int objCount = 1; // the page dict itself
                pageSizes.Add(EstimateObjectSize(pdfBytes, xrefEntries, pageObjectNumbers[i]));
                if (objCount < minObjects) minObjects = objCount;
                if (objCount > maxObjects) maxObjects = objCount;
            }
            if (minObjects == int.MaxValue) minObjects = 1;

            // We write a simplified header with correct structure but minimal data
            // All values are big-endian per the spec

            // Item 1: Least number of objects in a page (32-bit)
            WriteBigEndian32(bw, 1);
            // Item 2: Location of first page's page object (32-bit)
            long firstPageOffset = newOffsets.ContainsKey(pageObjectNumbers[0])
                ? newOffsets[pageObjectNumbers[0]] : 0;
            WriteBigEndian32(bw, (int)firstPageOffset);
            // Item 3: Number of bits needed to represent the difference from Item 1 (16-bit)
            WriteBigEndian16(bw, 0);
            // Item 4: Least page length (32-bit)
            int minPageLen = int.MaxValue;
            foreach (var s in pageSizes)
                if (s < minPageLen) minPageLen = s;
            if (minPageLen == int.MaxValue) minPageLen = 0;
            WriteBigEndian32(bw, minPageLen);
            // Item 5: Number of bits needed for page length difference (16-bit)
            WriteBigEndian16(bw, 0);
            // Item 6: Least offset to content stream (32-bit)
            WriteBigEndian32(bw, 0);
            // Item 7: Number of bits for content stream offset difference (16-bit)
            WriteBigEndian16(bw, 0);
            // Item 8: Least content stream length (32-bit)
            WriteBigEndian32(bw, 0);
            // Item 9: Number of bits for content stream length difference (16-bit)
            WriteBigEndian16(bw, 0);
            // Item 10: Number of bits for item 11 (16-bit)
            WriteBigEndian16(bw, 0);
            // Item 11: Number of bits for item 12 (16-bit)
            WriteBigEndian16(bw, 0);
            // Item 12: Number of bits for item 13 (16-bit)
            WriteBigEndian16(bw, 0);
            // Item 13: Numerator of shared object fraction (16-bit)
            WriteBigEndian16(bw, 0);

            bw.Flush();
            return ms.ToArray();
        }

        private static void WriteBigEndian32(BinaryWriter bw, int value)
        {
            bw.Write((byte)((value >> 24) & 0xFF));
            bw.Write((byte)((value >> 16) & 0xFF));
            bw.Write((byte)((value >> 8) & 0xFF));
            bw.Write((byte)(value & 0xFF));
        }

        private static void WriteBigEndian16(BinaryWriter bw, int value)
        {
            bw.Write((byte)((value >> 8) & 0xFF));
            bw.Write((byte)(value & 0xFF));
        }

        private static int EstimateObjectSize(byte[] pdfBytes, Dictionary<int, long> xrefEntries, int objNum)
        {
            if (!xrefEntries.TryGetValue(objNum, out long offset))
                return 100;

            int start = (int)offset;
            int end = FindEndObj(pdfBytes, start);
            return end - start;
        }

        private static void WriteHintObject(MemoryStream output, int objNum, byte[] hintData)
        {
            var sb = new StringBuilder();
            sb.Append(objNum);
            sb.Append(" 0 obj\n");
            sb.Append("<<");
            sb.Append("/Length ");
            sb.Append(hintData.Length);
            sb.Append(">>\n");
            sb.Append("stream\n");

            byte[] header = Encoding.ASCII.GetBytes(sb.ToString());
            output.Write(header, 0, header.Length);
            output.Write(hintData, 0, hintData.Length);

            byte[] footer = Encoding.ASCII.GetBytes("\nendstream\nendobj\n");
            output.Write(footer, 0, footer.Length);
        }

        private static void WriteXRefTable(MemoryStream output, int totalObjects,
            Dictionary<int, long> newOffsets, int hintObjNum, long hintOffset)
        {
            // Include hint object in offsets
            newOffsets[hintObjNum] = hintOffset;

            var sb = new StringBuilder();
            sb.Append("xref\n");
            sb.Append("0 ");
            sb.Append(totalObjects);
            sb.Append('\n');

            // Object 0: free list head
            sb.Append("0000000000 65535 f \n");

            // Objects 1..totalObjects-1
            for (int i = 1; i < totalObjects; i++)
            {
                if (newOffsets.TryGetValue(i, out long off))
                {
                    sb.AppendFormat("{0:D10} 00000 n \n", off);
                }
                else
                {
                    // Free entry
                    sb.Append("0000000000 00000 f \n");
                }
            }

            byte[] xrefBytes = Encoding.ASCII.GetBytes(sb.ToString());
            output.Write(xrefBytes, 0, xrefBytes.Length);
        }

        private static void WriteTrailer(MemoryStream output, int totalObjects,
            int catalogObjNum, int? infoObjNum, int? encryptObjNum,
            byte[]? fileId, long xrefOffset)
        {
            var sb = new StringBuilder();
            sb.Append("trailer\n");
            sb.Append("<<\n");
            sb.Append("/Size ");
            sb.Append(totalObjects);
            sb.Append('\n');
            sb.Append("/Root ");
            sb.Append(catalogObjNum);
            sb.Append(" 0 R\n");
            if (infoObjNum.HasValue)
            {
                sb.Append("/Info ");
                sb.Append(infoObjNum.Value);
                sb.Append(" 0 R\n");
            }
            if (encryptObjNum.HasValue)
            {
                sb.Append("/Encrypt ");
                sb.Append(encryptObjNum.Value);
                sb.Append(" 0 R\n");
            }
            if (fileId != null && fileId.Length > 0)
            {
                string hex = BitConverter.ToString(fileId).Replace("-", "");
                sb.Append("/ID [<");
                sb.Append(hex);
                sb.Append("><");
                sb.Append(hex);
                sb.Append(">]\n");
            }
            sb.Append(">>\n");
            sb.Append("startxref\n");
            sb.Append(xrefOffset);
            sb.Append('\n');
            sb.Append("%%EOF\n");

            byte[] trailerBytes = Encoding.ASCII.GetBytes(sb.ToString());
            output.Write(trailerBytes, 0, trailerBytes.Length);
        }

        private static byte[]? ExtractFileId(byte[] pdfBytes, long xrefOffset)
        {
            // Look for /ID in trailer
            string region = Encoding.ASCII.GetString(pdfBytes,
                (int)Math.Max(0, xrefOffset),
                (int)Math.Min(pdfBytes.Length - xrefOffset, 4096));

            int idIdx = region.IndexOf("/ID", StringComparison.Ordinal);
            if (idIdx < 0) return null;

            // Find the hex string: /ID [<hex><hex>]
            int openAngle = region.IndexOf('<', idIdx);
            if (openAngle < 0) return null;
            int closeAngle = region.IndexOf('>', openAngle);
            if (closeAngle < 0) return null;

            string hex = region.Substring(openAngle + 1, closeAngle - openAngle - 1);
            return HexToBytes(hex);
        }

        private static byte[] HexToBytes(string hex)
        {
            hex = hex.Trim();
            if (hex.Length % 2 != 0) hex = hex + "0";
            byte[] result = new byte[hex.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return result;
        }

        private static void CollectPages(PdfByteParser parser,
            Dictionary<int, long> xrefEntries, int pagesObjNum,
            List<int> pageObjectNumbers,
            Dictionary<int, List<int>> pageContentObjects,
            Dictionary<int, List<int>> pageResourceObjects)
        {
            // Read the page tree node
            var dict = parser.ReadObjectDict(xrefEntries[pagesObjNum]);
            string? type = dict.GetNameValue("Type");

            if (type == "Page")
            {
                // Leaf page
                pageObjectNumbers.Add(pagesObjNum);

                // Collect content stream references
                var contentRefs = dict.GetRefList("Contents");
                if (contentRefs.Count > 0)
                    pageContentObjects[pagesObjNum] = contentRefs;

                // Collect resource references (fonts, xobjects, etc.)
                var resourceRefs = CollectResourceRefs(parser, dict, xrefEntries);
                if (resourceRefs.Count > 0)
                    pageResourceObjects[pagesObjNum] = resourceRefs;
            }
            else if (type == "Pages")
            {
                // Intermediate node — recurse into kids
                var kids = dict.GetRefList("Kids");
                foreach (var kidObjNum in kids)
                {
                    if (xrefEntries.ContainsKey(kidObjNum))
                    {
                        CollectPages(parser, xrefEntries, kidObjNum,
                                     pageObjectNumbers, pageContentObjects, pageResourceObjects);
                    }
                }
            }
        }

        private static List<int> CollectResourceRefs(PdfByteParser parser,
            SimplePdfDict dict, Dictionary<int, long> xrefEntries)
        {
            var refs = new List<int>();

            // If Resources is a reference, add it
            int resourceRef = dict.TryGetRefObjectNumber("Resources") ?? -1;
            if (resourceRef > 0)
            {
                refs.Add(resourceRef);
                // Try to parse the resource dict and collect sub-references
                if (xrefEntries.ContainsKey(resourceRef))
                {
                    var resDict = parser.ReadObjectDict(xrefEntries[resourceRef]);
                    CollectDictValueRefs(resDict, refs);
                }
            }

            return refs;
        }

        private static void CollectDictValueRefs(SimplePdfDict dict, List<int> refs)
        {
            foreach (var kvp in dict.Entries)
            {
                if (kvp.Value is SimplePdfRef sref)
                {
                    refs.Add(sref.ObjNum);
                }
            }
        }

        private static void CollectPageTreeNodes(PdfByteParser parser,
            Dictionary<int, long> xrefEntries, int nodeObjNum, HashSet<int> nodes)
        {
            nodes.Add(nodeObjNum);
            if (!xrefEntries.ContainsKey(nodeObjNum)) return;

            var dict = parser.ReadObjectDict(xrefEntries[nodeObjNum]);
            string? type = dict.GetNameValue("Type");

            if (type == "Pages")
            {
                var kids = dict.GetRefList("Kids");
                foreach (var kid in kids)
                {
                    if (xrefEntries.ContainsKey(kid))
                    {
                        var kidDict = parser.ReadObjectDict(xrefEntries[kid]);
                        string? kidType = kidDict.GetNameValue("Type");
                        if (kidType == "Pages")
                            CollectPageTreeNodes(parser, xrefEntries, kid, nodes);
                    }
                }
            }
        }

        // ═══════════════════════════════════════════
        // Minimal PDF byte-level parser
        // ═══════════════════════════════════════════

        /// <summary>
        /// Simple parsed value types for the linearizer's internal parser.
        /// </summary>
        private abstract class SimplePdfValue { }

        private sealed class SimplePdfRef : SimplePdfValue
        {
            public int ObjNum { get; }
            public SimplePdfRef(int objNum) => ObjNum = objNum;
        }

        private sealed class SimplePdfNameValue : SimplePdfValue
        {
            public string Name { get; }
            public SimplePdfNameValue(string name) => Name = name;
        }

        private sealed class SimplePdfIntValue : SimplePdfValue
        {
            public long Value { get; }
            public SimplePdfIntValue(long value) => Value = value;
        }

        private sealed class SimplePdfArrayValue : SimplePdfValue
        {
            public List<SimplePdfValue> Items { get; }
            public SimplePdfArrayValue(List<SimplePdfValue> items) => Items = items;
        }

        private sealed class SimplePdfStringValue : SimplePdfValue
        {
            public string Value { get; }
            public SimplePdfStringValue(string value) => Value = value;
        }

        private sealed class SimplePdfDict
        {
            public List<KeyValuePair<string, SimplePdfValue>> Entries { get; } =
                new List<KeyValuePair<string, SimplePdfValue>>();

            public int GetRefObjectNumber(string key)
            {
                foreach (var e in Entries)
                {
                    if (e.Key == key && e.Value is SimplePdfRef r)
                        return r.ObjNum;
                }
                return -1;
            }

            public int? TryGetRefObjectNumber(string key)
            {
                foreach (var e in Entries)
                {
                    if (e.Key == key && e.Value is SimplePdfRef r)
                        return r.ObjNum;
                }
                return null;
            }

            public string? GetNameValue(string key)
            {
                foreach (var e in Entries)
                {
                    if (e.Key == key && e.Value is SimplePdfNameValue n)
                        return n.Name;
                }
                return null;
            }

            public List<int> GetRefList(string key)
            {
                var result = new List<int>();
                foreach (var e in Entries)
                {
                    if (e.Key != key) continue;

                    if (e.Value is SimplePdfRef r)
                    {
                        result.Add(r.ObjNum);
                    }
                    else if (e.Value is SimplePdfArrayValue arr)
                    {
                        foreach (var item in arr.Items)
                        {
                            if (item is SimplePdfRef ar)
                                result.Add(ar.ObjNum);
                        }
                    }
                }
                return result;
            }
        }

        private sealed class HeaderInfo
        {
            public int HeaderEndOffset { get; set; }
        }

        private sealed class PdfByteParser
        {
            private readonly byte[] _data;

            public PdfByteParser(byte[] data) => _data = data;

            public HeaderInfo ReadHeader()
            {
                // Find end of header (after binary comment line)
                int pos = 0;

                // Skip first line (%PDF-x.x\n)
                while (pos < _data.Length && _data[pos] != '\n') pos++;
                pos++; // skip \n

                // Skip binary comment line (%\xE2\xE3\xCF\xD3\n)
                if (pos < _data.Length && _data[pos] == '%')
                {
                    while (pos < _data.Length && _data[pos] != '\n') pos++;
                    pos++; // skip \n
                }

                return new HeaderInfo { HeaderEndOffset = pos };
            }

            public long FindStartXRef()
            {
                // Search backwards for "startxref"
                string tail = Encoding.ASCII.GetString(_data,
                    Math.Max(0, _data.Length - 1024), Math.Min(1024, _data.Length));
                int idx = tail.LastIndexOf("startxref", StringComparison.Ordinal);
                if (idx < 0)
                    throw new InvalidOperationException("Cannot find startxref in PDF.");

                // Read the offset after "startxref\n"
                int numStart = idx + "startxref".Length;
                while (numStart < tail.Length && !char.IsDigit(tail[numStart])) numStart++;
                int numEnd = numStart;
                while (numEnd < tail.Length && char.IsDigit(tail[numEnd])) numEnd++;

                return long.Parse(tail.Substring(numStart, numEnd - numStart));
            }

            public Dictionary<int, long> ReadXRefTable(long xrefOffset)
            {
                var entries = new Dictionary<int, long>();
                int pos = (int)xrefOffset;

                // Skip "xref\n"
                pos = SkipToken(pos, "xref");
                pos = SkipWhitespace(pos);

                // Read sections
                while (pos < _data.Length)
                {
                    // Check for "trailer"
                    if (pos + 7 <= _data.Length && MatchAscii(pos, "trailer"))
                        break;

                    // Read start_obj count
                    int startObj = ReadInt(ref pos);
                    pos = SkipWhitespace(pos);
                    int count = ReadInt(ref pos);
                    pos = SkipWhitespace(pos);

                    for (int i = 0; i < count; i++)
                    {
                        // Each entry: "OOOOOOOOOO GGGGG n \n" (20 bytes)
                        long offset = ReadLong10(pos);
                        char type = (char)_data[pos + 17];

                        if (type == 'n')
                        {
                            int objNum = startObj + i;
                            entries[objNum] = offset;
                        }

                        pos += 20;
                    }
                }

                return entries;
            }

            public SimplePdfDict ReadTrailerDict()
            {
                // Find "trailer" and then the dictionary
                string text = Encoding.ASCII.GetString(_data);
                int trailerIdx = text.LastIndexOf("trailer", StringComparison.Ordinal);
                if (trailerIdx < 0)
                    throw new InvalidOperationException("Cannot find trailer in PDF.");

                int pos = trailerIdx + "trailer".Length;
                pos = SkipWhitespace(pos);

                return ParseDict(ref pos);
            }

            public SimplePdfDict ReadObjectDict(long offset)
            {
                int pos = (int)offset;

                // Skip "N G obj\n"
                while (pos < _data.Length && _data[pos] != 'o') pos++;
                pos += 3; // skip "obj"
                pos = SkipWhitespace(pos);

                if (pos < _data.Length && _data[pos] == '<' && pos + 1 < _data.Length && _data[pos + 1] == '<')
                    return ParseDict(ref pos);

                return new SimplePdfDict();
            }

            private SimplePdfDict ParseDict(ref int pos)
            {
                var dict = new SimplePdfDict();

                if (pos + 1 >= _data.Length || _data[pos] != '<' || _data[pos + 1] != '<')
                    return dict;

                pos += 2; // skip <<
                pos = SkipWhitespace(pos);

                while (pos < _data.Length)
                {
                    if (_data[pos] == '>' && pos + 1 < _data.Length && _data[pos + 1] == '>')
                    {
                        pos += 2;
                        break;
                    }

                    if (_data[pos] != '/')
                    {
                        pos++;
                        continue;
                    }

                    // Read name
                    string name = ReadName(ref pos);
                    pos = SkipWhitespace(pos);

                    // Read value
                    var value = ParseValue(ref pos);
                    pos = SkipWhitespace(pos);

                    if (value != null)
                        dict.Entries.Add(new KeyValuePair<string, SimplePdfValue>(name, value));
                }

                return dict;
            }

            private SimplePdfValue? ParseValue(ref int pos)
            {
                if (pos >= _data.Length) return null;

                byte b = _data[pos];

                // Name
                if (b == '/')
                {
                    string name = ReadName(ref pos);
                    return new SimplePdfNameValue(name);
                }

                // Dict
                if (b == '<' && pos + 1 < _data.Length && _data[pos + 1] == '<')
                {
                    // Nested dict — skip it for now
                    var nested = ParseDict(ref pos);
                    // Return as a name placeholder
                    return new SimplePdfStringValue("<<dict>>");
                }

                // Hex string
                if (b == '<')
                {
                    pos++; // skip <
                    int start = pos;
                    while (pos < _data.Length && _data[pos] != '>') pos++;
                    string hex = Encoding.ASCII.GetString(_data, start, pos - start);
                    pos++; // skip >
                    return new SimplePdfStringValue(hex);
                }

                // String
                if (b == '(')
                {
                    pos++; // skip (
                    int depth = 1;
                    int start = pos;
                    while (pos < _data.Length && depth > 0)
                    {
                        if (_data[pos] == '\\') { pos += 2; continue; }
                        if (_data[pos] == '(') depth++;
                        if (_data[pos] == ')') depth--;
                        if (depth > 0) pos++;
                    }
                    string str = Encoding.ASCII.GetString(_data, start, pos - start);
                    pos++; // skip )
                    return new SimplePdfStringValue(str);
                }

                // Array
                if (b == '[')
                {
                    pos++; // skip [
                    pos = SkipWhitespace(pos);
                    var items = new List<SimplePdfValue>();
                    while (pos < _data.Length && _data[pos] != ']')
                    {
                        var item = ParseValue(ref pos);
                        if (item != null) items.Add(item);
                        pos = SkipWhitespace(pos);
                    }
                    if (pos < _data.Length) pos++; // skip ]

                    // Check if array contains references (N G R patterns)
                    // Convert consecutive int-int pairs followed by 'R' into refs
                    var resolved = new List<SimplePdfValue>();
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (i + 2 < items.Count &&
                            items[i] is SimplePdfIntValue objNum &&
                            items[i + 1] is SimplePdfIntValue gen &&
                            items[i + 2] is SimplePdfNameValue rToken && rToken.Name == "R")
                        {
                            resolved.Add(new SimplePdfRef((int)objNum.Value));
                            i += 2;
                        }
                        else
                        {
                            resolved.Add(items[i]);
                        }
                    }

                    return new SimplePdfArrayValue(resolved);
                }

                // Number or reference
                if (b == '-' || b == '+' || b == '.' || (b >= '0' && b <= '9'))
                {
                    long num = ReadLongValue(ref pos);
                    pos = SkipWhitespace(pos);

                    // Check for "G R" pattern (indirect reference)
                    if (pos < _data.Length && _data[pos] >= '0' && _data[pos] <= '9')
                    {
                        int savedPos = pos;
                        long gen = ReadLongValue(ref pos);
                        pos = SkipWhitespace(pos);
                        if (pos < _data.Length && _data[pos] == 'R')
                        {
                            pos++; // skip R
                            return new SimplePdfRef((int)num);
                        }
                        else
                        {
                            pos = savedPos;
                            return new SimplePdfIntValue(num);
                        }
                    }

                    return new SimplePdfIntValue(num);
                }

                // Boolean or other tokens
                if (b == 't' || b == 'f' || b == 'n')
                {
                    // Skip the token
                    int start = pos;
                    while (pos < _data.Length && _data[pos] > ' ' && _data[pos] != '/' &&
                           _data[pos] != '<' && _data[pos] != '>' && _data[pos] != '[' &&
                           _data[pos] != ']')
                        pos++;
                    string token = Encoding.ASCII.GetString(_data, start, pos - start);

                    // Check if this is actually "R" (part of a reference)
                    if (token == "R")
                        return new SimplePdfNameValue("R");

                    return new SimplePdfStringValue(token);
                }

                pos++;
                return null;
            }

            private string ReadName(ref int pos)
            {
                pos++; // skip /
                int start = pos;
                while (pos < _data.Length)
                {
                    byte c = _data[pos];
                    if (c <= ' ' || c == '/' || c == '<' || c == '>' ||
                        c == '[' || c == ']' || c == '(' || c == ')' || c == '{' || c == '}')
                        break;
                    pos++;
                }
                return Encoding.ASCII.GetString(_data, start, pos - start);
            }

            private long ReadLongValue(ref int pos)
            {
                bool neg = false;
                if (pos < _data.Length && _data[pos] == '-') { neg = true; pos++; }
                else if (pos < _data.Length && _data[pos] == '+') { pos++; }

                long value = 0;
                bool hasDot = false;
                while (pos < _data.Length)
                {
                    byte c = _data[pos];
                    if (c >= '0' && c <= '9')
                    {
                        if (!hasDot) value = value * 10 + (c - '0');
                        pos++;
                    }
                    else if (c == '.')
                    {
                        hasDot = true;
                        pos++;
                    }
                    else
                    {
                        break;
                    }
                }

                return neg ? -value : value;
            }

            private int ReadInt(ref int pos)
            {
                return (int)ReadLongValue(ref pos);
            }

            private long ReadLong10(int pos)
            {
                long value = 0;
                for (int i = 0; i < 10; i++)
                {
                    value = value * 10 + (_data[pos + i] - '0');
                }
                return value;
            }

            private int SkipWhitespace(int pos)
            {
                while (pos < _data.Length)
                {
                    byte c = _data[pos];
                    if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
                        pos++;
                    else if (c == '%')
                    {
                        // Skip comment line
                        while (pos < _data.Length && _data[pos] != '\n') pos++;
                        if (pos < _data.Length) pos++;
                    }
                    else
                        break;
                }
                return pos;
            }

            private int SkipToken(int pos, string token)
            {
                for (int i = 0; i < token.Length && pos < _data.Length; i++, pos++)
                {
                    // Just advance past the token
                }
                return pos;
            }

            private bool MatchAscii(int pos, string token)
            {
                if (pos + token.Length > _data.Length) return false;
                for (int i = 0; i < token.Length; i++)
                {
                    if (_data[pos + i] != (byte)token[i]) return false;
                }
                return true;
            }
        }
    }
}
