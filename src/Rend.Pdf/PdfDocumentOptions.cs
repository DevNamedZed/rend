namespace Rend.Pdf
{
    /// <summary>
    /// Configuration options for PDF document generation.
    /// </summary>
    public sealed class PdfDocumentOptions
    {
        /// <summary>PDF version to write. Default: PDF 1.7.</summary>
        public PdfVersion Version { get; set; } = PdfVersion.Pdf17;

        /// <summary>Compression mode for streams. Default: Flate.</summary>
        public PdfCompression Compression { get; set; } = PdfCompression.Flate;

        /// <summary>Fan-out for balanced page tree. Default: 32. Higher values mean flatter trees.</summary>
        public int PageTreeFanOut { get; set; } = 32;

        /// <summary>Initial buffer size for content stream builders. Default: 64KB.</summary>
        public int ContentStreamBufferSize { get; set; } = 65536;

        /// <summary>Whether to include XMP metadata in the PDF catalog. Default: false.</summary>
        public bool IncludeXmpMetadata { get; set; }
    }

    /// <summary>PDF version identifiers.</summary>
    public enum PdfVersion
    {
        /// <summary>PDF 1.4 — basic feature set, Acrobat 5 compatible.</summary>
        Pdf14,
        /// <summary>PDF 1.5 — object streams, cross-reference streams.</summary>
        Pdf15,
        /// <summary>PDF 1.6 — OpenType font embedding, AES encryption.</summary>
        Pdf16,
        /// <summary>PDF 1.7 — ISO 32000-1:2008. Default and recommended.</summary>
        Pdf17
    }

    /// <summary>Compression mode for PDF streams.</summary>
    public enum PdfCompression
    {
        /// <summary>No compression. Larger output, useful for debugging.</summary>
        None,
        /// <summary>Flate (Deflate) compression. Maps to CompressionLevel.Optimal for backward compatibility.</summary>
        Flate,
        /// <summary>Flate with fastest compression. Larger output but faster encoding.</summary>
        FlateFast,
        /// <summary>Flate with optimal compression. Explicit alias for best compression ratio.</summary>
        FlateOptimal
    }

    /// <summary>Font embedding mode.</summary>
    public enum FontEmbedMode
    {
        /// <summary>Embed only used glyphs. Smallest file size. Default.</summary>
        Subset,
        /// <summary>Embed the entire font file.</summary>
        Full,
        /// <summary>Do not embed (Standard 14 fonts only).</summary>
        None
    }

    /// <summary>Image format for AddImage.</summary>
    public enum ImageFormat
    {
        /// <summary>JPEG — passthrough (no re-encoding).</summary>
        Jpeg,
        /// <summary>PNG — decoded, alpha separated to SMask, re-compressed.</summary>
        Png
    }

    /// <summary>The 14 standard PDF fonts that don't require embedding.</summary>
    public enum StandardFont
    {
#pragma warning disable CS1591
        Helvetica,
        HelveticaBold,
        HelveticaOblique,
        HelveticaBoldOblique,
        TimesRoman,
        TimesBold,
        TimesItalic,
        TimesBoldItalic,
        Courier,
        CourierBold,
        CourierOblique,
        CourierBoldOblique,
        Symbol,
        ZapfDingbats
#pragma warning restore CS1591
    }

    /// <summary>Text rendering mode for content streams.</summary>
    public enum TextRenderingMode
    {
#pragma warning disable CS1591
        Fill = 0,
        Stroke = 1,
        FillAndStroke = 2,
        Invisible = 3,
        FillAndClip = 4,
        StrokeAndClip = 5,
        FillStrokeAndClip = 6,
        Clip = 7
#pragma warning restore CS1591
    }

    /// <summary>Line cap style for path stroking.</summary>
    public enum LineCapStyle
    {
        /// <summary>Butt cap — stroke squared off at endpoint.</summary>
        Butt = 0,
        /// <summary>Round cap — semicircle at endpoint.</summary>
        Round = 1,
        /// <summary>Projecting square cap — extends half line width beyond endpoint.</summary>
        Square = 2
    }

    /// <summary>Line join style for path stroking.</summary>
    public enum LineJoinStyle
    {
        /// <summary>Miter join — sharp corner.</summary>
        Miter = 0,
        /// <summary>Round join — circular arc.</summary>
        Round = 1,
        /// <summary>Bevel join — triangle at corner.</summary>
        Bevel = 2
    }
}
