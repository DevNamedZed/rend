namespace Rend
{
    /// <summary>
    /// Result container from a rendering operation.
    /// </summary>
    public sealed class RenderResult
    {
        /// <summary>The rendered output as a byte array.</summary>
        public byte[] Data { get; }

        /// <summary>Number of pages rendered.</summary>
        public int PageCount { get; }

        /// <summary>The output format ("pdf", "png", "jpeg", "webp").</summary>
        public string Format { get; }

        /// <summary>
        /// Snapshot of the layout tree for diagnostic comparison with browser layout.
        /// Only populated when requested via RenderOptions.
        /// </summary>
        public LayoutSnapshot? LayoutTree { get; }

        internal RenderResult(byte[] data, int pageCount, string format, LayoutSnapshot? layoutTree = null)
        {
            Data = data;
            PageCount = pageCount;
            Format = format;
            LayoutTree = layoutTree;
        }
    }
}
