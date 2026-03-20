#nullable enable

namespace Rend.Pdf.Parsing
{
    public sealed class PdfName : PdfObj
    {
        public string Value { get; }
        public PdfName(string value) => Value = value;
        public override string AsName() => Value;
        public override string ToString() => "/" + Value;
    }
}
