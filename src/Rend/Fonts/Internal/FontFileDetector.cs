using System;

namespace Rend.Fonts.Internal
{
    /// <summary>
    /// Identifies font file formats from raw data.
    /// </summary>
    public enum FontFileFormat
    {
        /// <summary>Unknown or unsupported format.</summary>
        Unknown = 0,

        /// <summary>TrueType font (.ttf).</summary>
        TrueType,

        /// <summary>OpenType font with CFF outlines (.otf).</summary>
        OpenType,

        /// <summary>TrueType Collection (.ttc).</summary>
        TrueTypeCollection,

        /// <summary>Web Open Font Format 1.0 (.woff).</summary>
        Woff,

        /// <summary>Web Open Font Format 2.0 (.woff2).</summary>
        Woff2
    }

    /// <summary>
    /// Detects font file format from magic bytes.
    /// </summary>
    public static class FontFileDetector
    {
        /// <summary>
        /// Detects the format of the given font data by examining magic bytes.
        /// </summary>
        public static FontFileFormat Detect(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Length < 4) return FontFileFormat.Unknown;

            // TrueType: 0x00010000
            if (data[0] == 0x00 && data[1] == 0x01 && data[2] == 0x00 && data[3] == 0x00)
                return FontFileFormat.TrueType;

            // TrueType: "true" (0x74727565)
            if (data[0] == 0x74 && data[1] == 0x72 && data[2] == 0x75 && data[3] == 0x65)
                return FontFileFormat.TrueType;

            // OpenType: "OTTO" (0x4F54544F)
            if (data[0] == 0x4F && data[1] == 0x54 && data[2] == 0x54 && data[3] == 0x4F)
                return FontFileFormat.OpenType;

            // TrueType Collection: "ttcf" (0x74746366)
            if (data[0] == 0x74 && data[1] == 0x74 && data[2] == 0x63 && data[3] == 0x66)
                return FontFileFormat.TrueTypeCollection;

            // WOFF: "wOFF" (0x774F4646)
            if (data[0] == 0x77 && data[1] == 0x4F && data[2] == 0x46 && data[3] == 0x46)
                return FontFileFormat.Woff;

            // WOFF2: "wOF2" (0x774F4632)
            if (data[0] == 0x77 && data[1] == 0x4F && data[2] == 0x46 && data[3] == 0x32)
                return FontFileFormat.Woff2;

            return FontFileFormat.Unknown;
        }
    }
}
