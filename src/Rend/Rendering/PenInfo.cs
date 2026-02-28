using Rend.Core.Values;

namespace Rend.Rendering
{
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

        /// <summary>
        /// Creates a new <see cref="PenInfo"/>.
        /// </summary>
        /// <param name="color">The stroke color.</param>
        /// <param name="width">The stroke width in pixels.</param>
        /// <param name="dashPattern">Optional dash pattern array, or null for solid.</param>
        /// <param name="dashOffset">The offset into the dash pattern.</param>
        public PenInfo(CssColor color, float width, float[]? dashPattern = null, float dashOffset = 0f)
        {
            Color = color;
            Width = width;
            DashPattern = dashPattern;
            DashOffset = dashOffset;
        }
    }
}
