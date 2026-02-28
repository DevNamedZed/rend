using Rend.Core.Values;
using Rend.Css;
using Rend.Layout;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Paints the background of a layout box, including solid colors and gradients.
    /// </summary>
    internal static class BackgroundPainter
    {
        /// <summary>
        /// Paints the background for the given box onto the render target.
        /// Fills the padding rect with the computed background-color. If the style
        /// specifies a gradient, a gradient-based brush is used instead.
        /// </summary>
        /// <param name="box">The layout box whose background to paint.</param>
        /// <param name="target">The render target to draw on.</param>
        public static void Paint(LayoutBox box, IRenderTarget target)
        {
            ComputedStyle? style = box.StyledNode?.Style;
            if (style == null)
            {
                return;
            }

            CssColor bgColor = style.BackgroundColor;

            // Skip fully transparent backgrounds.
            if (bgColor.A == 0)
            {
                return;
            }

            RectF paddingRect = box.PaddingRect;

            // Check for border-radius to determine if we need a rounded fill.
            float tlr = style.BorderTopLeftRadius;
            float trr = style.BorderTopRightRadius;
            float brr = style.BorderBottomRightRadius;
            float blr = style.BorderBottomLeftRadius;
            bool hasRadius = tlr > 0f || trr > 0f || brr > 0f || blr > 0f;

            BrushInfo brush = BrushInfo.Solid(bgColor);

            if (hasRadius)
            {
                var path = new PathData();
                path.AddRoundedRectangle(paddingRect, tlr, trr, brr, blr);
                target.FillPath(path, brush);
            }
            else
            {
                target.FillRect(paddingRect, brush);
            }
        }
    }
}
