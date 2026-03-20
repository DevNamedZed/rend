#nullable enable

namespace Rend.Pdf.Parsing
{
    public sealed class PdfRef : PdfObj
    {
        public int ObjNum { get; }
        public int GenNum { get; }

        public PdfRef(int objNum, int genNum)
        {
            ObjNum = objNum;
            GenNum = genNum;
        }

        public override string ToString() => ObjNum + " " + GenNum + " R";
    }
}
