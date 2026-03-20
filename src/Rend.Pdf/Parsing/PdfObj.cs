#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rend.Pdf.Parsing
{
    public abstract class PdfObj
    {
        public static readonly PdfObj Null = new PdfNullObj();

        public virtual long AsInt() => 0;
        public virtual double AsReal() => (double)AsInt();
        public virtual bool AsBool() => false;
        public virtual string AsName() => "";
        public virtual byte[] AsBytes() => Array.Empty<byte>();
        public virtual string AsText() => "";

        public virtual PdfObj this[string key] => Null;
        public virtual PdfObj this[int index] => Null;
        public virtual int Count => 0;
        public virtual bool ContainsKey(string key) => false;
        public virtual IEnumerable<string> Keys => Enumerable.Empty<string>();

        public virtual bool IsNull => false;
        public bool IsRef => this is PdfRef;
        public bool IsArray => this is PdfArray;
        public bool IsDict => this is PdfDict;
        public bool IsStream => this is PdfStream;
        public bool IsName => this is PdfName;
        public bool IsString => this is PdfString;
        public bool IsInt => this is PdfInt;
        public bool IsReal => this is PdfReal;
        public bool IsBool => this is PdfBool;

        public float AsFloat() => (float)AsReal();
    }

    public sealed class PdfNullObj : PdfObj
    {
        public override bool IsNull => true;
        public override string ToString() => "null";
    }
}
