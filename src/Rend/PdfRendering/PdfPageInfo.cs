#nullable enable
using Rend.Pdf.Parsing;

namespace Rend.PdfRendering
{
    public sealed class PdfPageInfo
    {
        public int Index { get; }
        public float Width { get; }
        public float Height { get; }
        public int Rotation { get; }

        internal PdfPageInfo(PdfDocumentReader reader, PdfObj pageDict, int index)
        {
            Index = index;

            var box = FindInherited(reader, pageDict, "CropBox");
            if (box.IsNull)
            {
                box = FindInherited(reader, pageDict, "MediaBox");
            }

            float width;
            float height;
            if (!box.IsNull && box.IsArray && box.Count >= 4)
            {
                float left = box[0].AsFloat();
                float bottom = box[1].AsFloat();
                float right = box[2].AsFloat();
                float top = box[3].AsFloat();
                width = right - left;
                height = top - bottom;
            }
            else
            {
                width = 612f;
                height = 792f;
            }

            var rotate = FindInherited(reader, pageDict, "Rotate");
            Rotation = rotate.IsNull ? 0 : (int)rotate.AsInt();

            if (Rotation == 90 || Rotation == 270)
            {
                Width = height;
                Height = width;
            }
            else
            {
                Width = width;
                Height = height;
            }
        }

        private static PdfObj FindInherited(PdfDocumentReader reader, PdfObj pageDict, string key)
        {
            var val = reader.Resolve(pageDict[key]);
            if (!val.IsNull)
            {
                return val;
            }

            var parent = reader.Resolve(pageDict["Parent"]);
            while (!parent.IsNull)
            {
                val = reader.Resolve(parent[key]);
                if (!val.IsNull)
                {
                    return val;
                }
                parent = reader.Resolve(parent["Parent"]);
            }

            return PdfObj.Null;
        }
    }
}
