#nullable enable

namespace Rend.Pdf.Parsing
{
    public sealed class PdfReal : PdfObj
    {
        public double Value { get; }
        public PdfReal(double value) => Value = value;
        public override double AsReal() => Value;
        public override long AsInt() => (long)Value;
        public override string ToString() => Value.ToString("G");
    }
}
