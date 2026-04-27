namespace Rend.Css.Values
{
    /// <summary>
    /// [CSS-OVERFLOW-3 §3.4] Resolved value of the overflow-clip-margin property.
    /// Specifies the reference box and outward expansion distance for overflow:clip clipping.
    /// </summary>
    public sealed class OverflowClipMarginInfo
    {
        /// <summary>The reference box from which the clip margin extends.</summary>
        public CssVisualBox ReferenceBox { get; }

        /// <summary>The outward expansion distance in pixels (always >= 0).</summary>
        public float Margin { get; }

        public OverflowClipMarginInfo(CssVisualBox referenceBox, float margin)
        {
            ReferenceBox = referenceBox;
            Margin = margin;
        }

        /// <summary>Default value: padding-box 0px.</summary>
        public static readonly OverflowClipMarginInfo Default =
            new OverflowClipMarginInfo(CssVisualBox.PaddingBox, 0f);
    }
}
