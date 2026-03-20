#nullable enable

namespace Rend.Pdf.Parsing
{
    public struct PdfRect
    {
        public float Left;
        public float Bottom;
        public float Right;
        public float Top;

        public PdfRect(float left, float bottom, float right, float top)
        {
            Left = left;
            Bottom = bottom;
            Right = right;
            Top = top;
        }

        public float Width => Right - Left;
        public float Height => Top - Bottom;

        public static PdfRect FromArray(PdfObj array)
        {
            return new PdfRect(
                array[0].AsFloat(),
                array[1].AsFloat(),
                array[2].AsFloat(),
                array[3].AsFloat()
            );
        }

        public override string ToString() => $"[{Left}, {Bottom}, {Right}, {Top}]";
    }
}
