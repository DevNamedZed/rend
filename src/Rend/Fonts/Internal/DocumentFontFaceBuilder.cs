using System;

namespace Rend.Fonts.Internal
{
    /// <summary>
    /// Builds <see cref="FontEntry"/> instances from raw <c>@font-face</c> data,
    /// handling sfnt extraction, OpenType parsing, and descriptor overrides.
    /// </summary>
    internal static class DocumentFontFaceBuilder
    {
        public static FontEntry Build(byte[] fontData, FontFaceDescriptor descriptor)
        {
            if (fontData == null)
            {
                throw new ArgumentNullException(nameof(fontData));
            }
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            if (FontFileDetector.Detect(fontData) == FontFileFormat.TrueTypeCollection)
            {
                throw new InvalidOperationException("TrueType Collection files are not valid @font-face sources.");
            }

            byte[] sfntData = EnsureSfnt(fontData);
            OpenTypeFontData parsed;
            try
            {
                parsed = new OpenTypeFontData(sfntData);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to parse font data: " + ex.Message, ex);
            }

            // [CSS-FONTS-4 §5.1.2] Descriptor values override the intrinsic OpenType
            // metadata for font matching purposes.
            var fontDescriptor = new FontDescriptor(
                descriptor.FamilyName,
                descriptor.Weight,
                descriptor.Style,
                descriptor.Stretch);
            FontMetricsInfo metrics = parsed.BuildMetrics();
            return new FontEntry(fontDescriptor, sfntData, metrics, descriptor.FamilyName, null, parsed);
        }

        private static byte[] EnsureSfnt(byte[] data)
        {
            FontFileFormat format = FontFileDetector.Detect(data);
            switch (format)
            {
                case FontFileFormat.TrueType:
                case FontFileFormat.OpenType:
                    return data;
                case FontFileFormat.Woff:
                    return WoffDecompressor.Decompress(data);
                case FontFileFormat.Woff2:
                    return Woff2Decompressor.Decompress(data);
                default:
                    return data;
            }
        }
    }
}
