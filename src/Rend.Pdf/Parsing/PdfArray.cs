#nullable enable
using System.Collections.Generic;

namespace Rend.Pdf.Parsing
{
    public sealed class PdfArray : PdfObj
    {
        public List<PdfObj> Items { get; }

        public PdfArray() => Items = new List<PdfObj>();
        public PdfArray(List<PdfObj> items) => Items = items;

        public override PdfObj this[int index] =>
            index >= 0 && index < Items.Count ? Items[index] : Null;

        public override int Count => Items.Count;
        public override string ToString() => "[array:" + Items.Count + "]";
    }
}
