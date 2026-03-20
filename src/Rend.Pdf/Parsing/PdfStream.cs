#nullable enable
using System;
using System.Collections.Generic;

namespace Rend.Pdf.Parsing
{
    public sealed class PdfStream : PdfDict
    {
        public byte[] RawData { get; set; }
        public long StreamOffset { get; set; }
        public int StreamLength { get; set; }

        public PdfStream() => RawData = Array.Empty<byte>();

        public PdfStream(Dictionary<string, PdfObj> entries) : base(entries)
        {
            RawData = Array.Empty<byte>();
        }

        public override string ToString() => "<<stream:" + Entries.Count + ",len:" + StreamLength + ">>";
    }
}
