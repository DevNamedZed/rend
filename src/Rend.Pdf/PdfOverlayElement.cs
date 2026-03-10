using System;
using Rend.Core.Values;

namespace Rend.Pdf
{
    /// <summary>
    /// Base class for content drawn onto an existing PDF page.
    /// Coordinates use top-left origin (converted to PDF bottom-left internally).
    /// </summary>
    public abstract class PdfOverlayElement
    {
        /// <summary>Page number (1-based).</summary>
        public int Page { get; set; } = 1;

        /// <summary>X position from the left edge of the page in points.</summary>
        public float X { get; set; }

        /// <summary>Y position from the top edge of the page in points.</summary>
        public float Y { get; set; }
    }

    /// <summary>
    /// Draws text onto a PDF page using a standard PDF font.
    /// </summary>
    public sealed class TextOverlay : PdfOverlayElement
    {
        /// <summary>The text to draw.</summary>
        public string Text { get; set; } = "";

        /// <summary>Font size in points. Default: 12.</summary>
        public float FontSize { get; set; } = 12f;

        /// <summary>Text color. Default: black.</summary>
        public CssColor Color { get; set; } = CssColor.FromRgba(0, 0, 0);

        /// <summary>
        /// Font family name. Resolved to a standard PDF font.
        /// Supported: "Helvetica", "Times", "Courier". Default: "Helvetica".
        /// </summary>
        public string FontFamily { get; set; } = "Helvetica";

        /// <summary>Whether to use the bold variant.</summary>
        public bool Bold { get; set; }

        /// <summary>Whether to use the italic variant.</summary>
        public bool Italic { get; set; }
    }

    /// <summary>
    /// Draws an image onto a PDF page.
    /// Supports JPEG and PNG formats.
    /// </summary>
    public sealed class ImageOverlay : PdfOverlayElement
    {
        /// <summary>Image bytes (JPEG or PNG).</summary>
        public byte[] Data { get; set; } = Array.Empty<byte>();

        /// <summary>Display width in points.</summary>
        public float Width { get; set; }

        /// <summary>Display height in points.</summary>
        public float Height { get; set; }
    }
}
