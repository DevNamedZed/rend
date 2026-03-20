#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rend.Pdf.Parsing
{
    public class PdfDict : PdfObj
    {
        public Dictionary<string, PdfObj> Entries { get; }

        public PdfDict() => Entries = new Dictionary<string, PdfObj>(StringComparer.Ordinal);
        public PdfDict(Dictionary<string, PdfObj> entries) => Entries = entries;

        public override PdfObj this[string key]
        {
            get
            {
                if (Entries.TryGetValue(key, out var val))
                {
                    return val;
                }
                return Null;
            }
        }

        public override int Count => Entries.Count;
        public override bool ContainsKey(string key) => Entries.ContainsKey(key);
        public override IEnumerable<string> Keys => Entries.Keys;
        public override string ToString() => "<<dict:" + Entries.Count + ">>";
    }
}
