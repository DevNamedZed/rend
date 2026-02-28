using System.Collections.Generic;

namespace Rend.Pdf.Internal
{
    /// <summary>
    /// Manages PDF indirect object allocation, numbering, and cross-reference table generation.
    /// Object numbers start at 1 (object 0 is the free-list head per PDF spec).
    /// </summary>
    internal sealed class PdfObjectTable
    {
        private readonly List<IndirectObject> _objects = new List<IndirectObject>();

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

                writer.WriteLong(entry.ObjectNumber);
                writer.WriteRaw(PdfWriter.Bytes_ObjStart);
                entry.Object.WriteTo(writer);
                writer.WriteRaw(PdfWriter.Bytes_ObjEnd);
            }
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
        public void WriteTrailer(PdfWriter writer, PdfReference catalogRef, PdfReference? infoRef, long xrefOffset)
        {
            writer.WriteAscii("trailer\n");

            var trailer = new PdfDictionary(4);
            trailer[PdfName.Size] = new PdfInteger(_objects.Count + 1);
            trailer[PdfName.Root] = catalogRef;
            if (infoRef != null)
                trailer[PdfName.Info] = infoRef;

            trailer.WriteTo(writer);
            writer.WriteNewLine();

            writer.WriteAscii("startxref\n");
            writer.WriteLong(xrefOffset);
            writer.WriteNewLine();
            writer.WriteAscii("%%EOF\n");
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
            public long ByteOffset;

            public IndirectObject(int objectNumber, PdfObject obj)
            {
                ObjectNumber = objectNumber;
                Object = obj;
                ByteOffset = 0;
            }
        }
    }
}
