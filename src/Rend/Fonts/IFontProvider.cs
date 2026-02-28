namespace Rend.Fonts
{
    /// <summary>
    /// Provides font resolution, metrics, and character measurement capabilities.
    /// </summary>
    public interface IFontProvider
    {
        /// <summary>
        /// Resolves the best matching font entry for the given descriptor.
        /// </summary>
        FontEntry? ResolveFont(FontDescriptor descriptor);

        /// <summary>
        /// Returns font metrics for the given descriptor.
        /// </summary>
        FontMetricsInfo GetMetrics(FontDescriptor descriptor);

        /// <summary>
        /// Measures the advance width of a single character at the given font size.
        /// </summary>
        float MeasureCharWidth(FontDescriptor descriptor, int codePoint, float fontSize);

        /// <summary>
        /// Registers raw font data (TrueType, OpenType, WOFF, or WOFF2).
        /// </summary>
        void RegisterFont(byte[] fontData, string? familyNameOverride = null);

        /// <summary>
        /// Scans a directory for font files and registers all found fonts.
        /// </summary>
        void RegisterFontDirectory(string directoryPath);
    }
}
