using System;

namespace Rend.Rendering
{
    /// <summary>
    /// Holds raw image data along with its dimensions and format.
    /// </summary>
    public sealed class ImageData
    {
        /// <summary>Gets the raw image bytes.</summary>
        public byte[] Data { get; }

        /// <summary>Gets the image width in pixels.</summary>
        public int Width { get; }

        /// <summary>Gets the image height in pixels.</summary>
        public int Height { get; }

        /// <summary>Gets the image format identifier (e.g. "png", "jpeg").</summary>
        public string Format { get; }

        /// <summary>
        /// Creates a new <see cref="ImageData"/>.
        /// </summary>
        /// <param name="data">The raw image bytes.</param>
        /// <param name="width">The image width in pixels.</param>
        /// <param name="height">The image height in pixels.</param>
        /// <param name="format">The image format (e.g. "png", "jpeg").</param>
        public ImageData(byte[] data, int width, int height, string format)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Width = width;
            Height = height;
            Format = format ?? throw new ArgumentNullException(nameof(format));
        }
    }
}
