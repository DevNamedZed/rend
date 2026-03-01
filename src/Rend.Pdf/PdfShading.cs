using System;
using Rend.Core.Values;

namespace Rend.Pdf
{
    /// <summary>PDF shading type.</summary>
    public enum PdfShadingType
    {
        /// <summary>Axial (linear) shading — Type 2.</summary>
        Linear = 2,
        /// <summary>Radial shading — Type 3.</summary>
        Radial = 3
    }

    /// <summary>A color stop in a gradient.</summary>
    public readonly struct PdfGradientColorStop
    {
        /// <summary>Position along the gradient (0.0 to 1.0).</summary>
        public float Position { get; }
        /// <summary>Red component (0.0 to 1.0).</summary>
        public float R { get; }
        /// <summary>Green component (0.0 to 1.0).</summary>
        public float G { get; }
        /// <summary>Blue component (0.0 to 1.0).</summary>
        public float B { get; }

        /// <summary>Create a color stop from RGB float values.</summary>
        public PdfGradientColorStop(float position, float r, float g, float b)
        {
            Position = position;
            R = r;
            G = g;
            B = b;
        }

        /// <summary>Create a color stop from a CssColor.</summary>
        public PdfGradientColorStop(float position, CssColor color)
        {
            Position = position;
            color.ToFloatRgb(out float r, out float g, out float b);
            R = r;
            G = g;
            B = b;
        }
    }

    /// <summary>A linear (axial) gradient definition.</summary>
    public sealed class PdfLinearGradient
    {
        /// <summary>Start X coordinate.</summary>
        public float X0 { get; set; }
        /// <summary>Start Y coordinate.</summary>
        public float Y0 { get; set; }
        /// <summary>End X coordinate.</summary>
        public float X1 { get; set; }
        /// <summary>End Y coordinate.</summary>
        public float Y1 { get; set; }
        /// <summary>Whether to extend the gradient before the start point.</summary>
        public bool ExtendStart { get; set; } = true;
        /// <summary>Whether to extend the gradient after the end point.</summary>
        public bool ExtendEnd { get; set; } = true;
        /// <summary>Color stops (must have at least 2).</summary>
        public PdfGradientColorStop[] Stops { get; set; } = Array.Empty<PdfGradientColorStop>();
    }

    /// <summary>A radial gradient definition.</summary>
    public sealed class PdfRadialGradient
    {
        /// <summary>Start circle center X.</summary>
        public float X0 { get; set; }
        /// <summary>Start circle center Y.</summary>
        public float Y0 { get; set; }
        /// <summary>Start circle radius.</summary>
        public float R0 { get; set; }
        /// <summary>End circle center X.</summary>
        public float X1 { get; set; }
        /// <summary>End circle center Y.</summary>
        public float Y1 { get; set; }
        /// <summary>End circle radius.</summary>
        public float R1 { get; set; }
        /// <summary>Whether to extend the gradient before the start circle.</summary>
        public bool ExtendStart { get; set; } = true;
        /// <summary>Whether to extend the gradient after the end circle.</summary>
        public bool ExtendEnd { get; set; } = true;
        /// <summary>Color stops (must have at least 2).</summary>
        public PdfGradientColorStop[] Stops { get; set; } = Array.Empty<PdfGradientColorStop>();
    }

    /// <summary>A conic (sweep) gradient definition.</summary>
    public sealed class PdfConicGradient
    {
        /// <summary>Center X coordinate.</summary>
        public float CenterX { get; set; }
        /// <summary>Center Y coordinate.</summary>
        public float CenterY { get; set; }
        /// <summary>Start angle in degrees (CSS convention: 0 = top, clockwise).</summary>
        public float StartAngle { get; set; }
        /// <summary>Number of wedge segments used to approximate the conic gradient (default 72).</summary>
        public int Segments { get; set; } = 72;
        /// <summary>Bounding box width (used for wedge radius calculation).</summary>
        public float Width { get; set; }
        /// <summary>Bounding box height (used for wedge radius calculation).</summary>
        public float Height { get; set; }
        /// <summary>Color stops (must have at least 2).</summary>
        public PdfGradientColorStop[] Stops { get; set; } = Array.Empty<PdfGradientColorStop>();
    }
}
