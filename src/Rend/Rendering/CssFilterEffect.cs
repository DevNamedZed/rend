using Rend.Core.Values;

namespace Rend.Rendering
{
    /// <summary>
    /// Represents a single CSS filter function (e.g., blur(5px), brightness(1.2)).
    /// </summary>
    public struct CssFilterEffect
    {
        public CssFilterType Type;
        public float Amount;
        /// <summary>For drop-shadow: X offset.</summary>
        public float OffsetX;
        /// <summary>For drop-shadow: Y offset.</summary>
        public float OffsetY;
        /// <summary>For drop-shadow: color.</summary>
        public CssColor Color;
    }

    /// <summary>
    /// CSS filter function types.
    /// </summary>
    public enum CssFilterType
    {
        Blur,
        Brightness,
        Contrast,
        Grayscale,
        Sepia,
        Saturate,
        HueRotate,
        Invert,
        Opacity,
        DropShadow
    }
}
