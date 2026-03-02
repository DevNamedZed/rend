using System;
using Rend.Pdf.Internal;

namespace Rend.Pdf
{
    /// <summary>PDF tiling pattern paint type (ISO 32000-1 §8.7.4.2).</summary>
    public enum PdfPatternPaintType
    {
        /// <summary>Colored tiling pattern — the pattern content stream defines its own colors.</summary>
        Colored = 1,
        /// <summary>Uncolored tiling pattern — colors are specified when the pattern is applied.</summary>
        Uncolored = 2
    }

    /// <summary>PDF tiling type (ISO 32000-1 §8.7.4.2).</summary>
    public enum PdfTilingType
    {
        /// <summary>Constant spacing — pattern cells are spaced consistently.</summary>
        ConstantSpacing = 1,
        /// <summary>No distortion — the pattern cell is not distorted, spacing may vary slightly.</summary>
        NoDistortion = 2,
        /// <summary>Constant spacing and faster tiling.</summary>
        ConstantSpacingFaster = 3
    }

    /// <summary>
    /// A PDF tiling pattern that repeats a small drawing (the pattern cell) across a filled area.
    /// Create using <see cref="PdfDocument.CreateTilingPattern"/>.
    /// </summary>
    public sealed class PdfTilingPattern
    {
        /// <summary>Width of the pattern cell in user space units.</summary>
        public float Width { get; }

        /// <summary>Height of the pattern cell in user space units.</summary>
        public float Height { get; }

        /// <summary>Horizontal spacing between pattern cell origins.</summary>
        public float XStep { get; }

        /// <summary>Vertical spacing between pattern cell origins.</summary>
        public float YStep { get; }

        /// <summary>Paint type (colored or uncolored).</summary>
        public PdfPatternPaintType PaintType { get; }

        /// <summary>Tiling type (spacing/distortion behavior).</summary>
        public PdfTilingType TilingType { get; }

        /// <summary>The content stream for drawing the pattern cell.</summary>
        public PdfContentStream Content { get; }

        /// <summary>Resource name used in content stream operators (e.g. "P1").</summary>
        internal string ResourceName { get; set; } = "";

        /// <summary>Reference to the pattern object in the PDF object table.</summary>
        internal PdfReference? ObjectReference { get; set; }

        internal PdfTilingPattern(float width, float height, float xStep, float yStep,
                                   PdfPatternPaintType paintType, PdfTilingType tilingType,
                                   PdfContentStream content)
        {
            Width = width;
            Height = height;
            XStep = xStep;
            YStep = yStep;
            PaintType = paintType;
            TilingType = tilingType;
            Content = content;
        }
    }
}
