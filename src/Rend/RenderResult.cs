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

        public RenderResult(byte[] data, int pageCount, string format)
        {
            Data = data;
            PageCount = pageCount;
            Format = format;
        }
    }
}
