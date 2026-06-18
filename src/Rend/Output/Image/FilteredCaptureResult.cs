namespace Rend.Output.Image
{
    /// <summary>
    /// The rasterized result of an offscreen filter/mask capture, cropped to its non-transparent
    /// content bounds. The crop offset and the full raster size let the consumer place the cropped
    /// image back at the correct position and scale on the page (see <c>PdfRenderTarget</c> filter
    /// embedding — PDF-B7).
    /// </summary>
    internal sealed class FilteredCaptureResult
    {
        /// <summary>Unpremultiplied RGBA bytes of the cropped region, row-major, <see cref="Width"/>×<see cref="Height"/>.</summary>
        public byte[] Rgba { get; }

        /// <summary>Width of the cropped region, in raster pixels.</summary>
        public int Width { get; }

        /// <summary>Height of the cropped region, in raster pixels.</summary>
        public int Height { get; }

        /// <summary>Left edge of the crop within the full raster, in pixels.</summary>
        public int OffsetX { get; }

        /// <summary>Top edge of the crop within the full raster, in pixels.</summary>
        public int OffsetY { get; }

        /// <summary>Width of the full (uncropped) raster, in pixels — the page-sized capture.</summary>
        public int FullWidth { get; }

        /// <summary>Height of the full (uncropped) raster, in pixels.</summary>
        public int FullHeight { get; }

        /// <summary>True when nothing was drawn (no non-transparent pixels), so there is nothing to embed.</summary>
        public bool IsEmpty => Width <= 0 || Height <= 0;

        public FilteredCaptureResult(byte[] rgba, int width, int height, int offsetX, int offsetY,
            int fullWidth, int fullHeight)
        {
            Rgba = rgba;
            Width = width;
            Height = height;
            OffsetX = offsetX;
            OffsetY = offsetY;
            FullWidth = fullWidth;
            FullHeight = fullHeight;
        }

        public static FilteredCaptureResult Empty(int fullWidth, int fullHeight)
        {
            return new FilteredCaptureResult(System.Array.Empty<byte>(), 0, 0, 0, 0, fullWidth, fullHeight);
        }
    }
}
