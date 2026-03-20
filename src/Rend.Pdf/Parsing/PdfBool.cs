#nullable enable

namespace Rend.Pdf.Parsing
{
    public sealed class PdfBool : PdfObj
    {
        public bool Value { get; }
        public PdfBool(bool value) => Value = value;
        public override bool AsBool() => Value;
        public override string ToString() => Value ? "true" : "false";
    }
}
