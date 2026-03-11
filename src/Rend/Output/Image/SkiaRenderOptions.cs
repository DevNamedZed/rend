namespace Rend.Output.Image
{
    /// <summary>
    /// Configuration options for SkiaSharp-based image rendering output.
    /// </summary>
    internal sealed class SkiaRenderOptions
    {
        /// <summary>Gets or sets the rendering DPI. Default is 96.</summary>
        public float Dpi { get; set; } = 96f;

        /// <summary>Gets or sets the output image format. Default is <see cref="ImageOutputFormat.Png"/>.</summary>
        public ImageOutputFormat Format { get; set; } = ImageOutputFormat.Png;

        /// <summary>Gets or sets the encoding quality for lossy formats (0-100). Default is 90.</summary>
        public int Quality { get; set; } = 90;
    }
}
