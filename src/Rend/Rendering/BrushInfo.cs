using Rend.Core.Values;

namespace Rend.Rendering
{
    /// <summary>
    /// Describes a fill operation. May be a solid color, gradient, or image fill.
    /// </summary>
    public sealed class BrushInfo
    {
        /// <summary>Gets or sets the solid fill color.</summary>
        public CssColor Color { get; set; }

        /// <summary>Gets or sets the gradient fill descriptor, or null for non-gradient fills.</summary>
        public GradientInfo? Gradient { get; set; }

        /// <summary>Gets or sets the image fill data, or null for non-image fills.</summary>
        public ImageData? Image { get; set; }

        /// <summary>
        /// Creates a <see cref="BrushInfo"/> for a solid color fill.
        /// </summary>
        /// <param name="color">The fill color.</param>
        /// <returns>A new brush configured for a solid color fill.</returns>
        public static BrushInfo Solid(CssColor color)
        {
            return new BrushInfo { Color = color };
        }

        /// <summary>
        /// Creates a <see cref="BrushInfo"/> for a gradient fill.
        /// </summary>
        /// <param name="gradient">The gradient descriptor.</param>
        /// <returns>A new brush configured for a gradient fill.</returns>
        public static BrushInfo FromGradient(GradientInfo gradient)
        {
            return new BrushInfo { Gradient = gradient };
        }
    }
}
