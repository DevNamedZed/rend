#nullable enable

namespace Rend.Pdf.Parsing
{
    public sealed class PdfInt : PdfObj
    {
        public long Value { get; }
        public PdfInt(long value) => Value = value;
        public override long AsInt() => Value;
        public override double AsReal() => Value;
        public override string ToString() => Value.ToString();
    }
}
