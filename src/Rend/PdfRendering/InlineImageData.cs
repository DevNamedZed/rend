#nullable enable
using System;

namespace Rend.PdfRendering
{
    internal sealed class InlineImageData
    {
        public int Width;
        public int Height;
        public int BitsPerComponent = 8;
        public string ColorSpace = "DeviceRGB";
        public string Filter = "";
        public byte[] Data = Array.Empty<byte>();
    }
}
