using Rend.Core.Values;

namespace Rend.Rendering
{
    public enum StrokeCap
    {
        Butt,
        Round,
        Square
    }

    /// <summary>
    /// Describes a stroke operation with color, width, and optional dash pattern.
    /// </summary>
    public readonly struct PenInfo
    {
        /// <summary>Gets the stroke color.</summary>
        public CssColor Color { get; }

        /// <summary>Gets the stroke width in pixels.</summary>
        public float Width { get; }

        /// <summary>Gets the dash pattern, or null for a solid stroke.</summary>
        public float[]? DashPattern { get; }

        /// <summary>Gets the offset into the dash pattern at which the stroke begins.</summary>
        public float DashOffset { get; }

        /// <summary>Gets the stroke cap style.</summary>
        public StrokeCap Cap { get; }

        public PenInfo(CssColor color, float width, float[]? dashPattern = null, float dashOffset = 0f, StrokeCap cap = StrokeCap.Butt)
        {
            Color = color;
            Width = width;
            DashPattern = dashPattern;
            DashOffset = dashOffset;
            Cap = cap;
        }
    }
}
